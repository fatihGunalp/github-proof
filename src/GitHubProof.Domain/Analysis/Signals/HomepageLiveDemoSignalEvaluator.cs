using System.Globalization;
using System.Text.RegularExpressions;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class HomepageLiveDemoSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "portfolio.homepage-or-live-demo";

    private const string MetadataDataKey = "metadata";
    private const string ReadmeDataKey = "readme";
    private static readonly string[] DemoIntentLabels =
    [
        "demo",
        "live demo",
        "try it",
        "open app",
        "launch app",
        "canlı demo",
        "uygulamayı aç",
        "deneyin"
    ];

    private static readonly string[] AmbiguousIntentTokens =
    [
        "demo",
        "live",
        "try",
        "preview",
        "app",
        "launch",
        "canlı",
        "deneyin",
        "önizleme",
        "uygulama"
    ];

    private static readonly Regex FenceRegex = new(
        @"^\s{0,3}(?<marks>`{3,}|~{3,})",
        RegexOptions.CultureInvariant);
    private static readonly Regex MarkdownLinkRegex = new(
        @"(?<!!)\[(?<label>[^\]\r\n]+)\]\((?<target>[^)\r\n]+)\)",
        RegexOptions.CultureInvariant);
    private static readonly Regex IncompleteMarkdownLinkRegex = new(
        @"(?<!!)\[(?<label>[^\]\r\n]+)\]\((?<target>[^)\r\n]*)$",
        RegexOptions.CultureInvariant);
    private static readonly Regex HtmlAnchorRegex = new(
        @"<a\s+[^>]*href\s*=\s*[\""'](?<target>[^\""']+)[\""'][^>]*>(?<label>.*?)</a>",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]+>",
        RegexOptions.CultureInvariant);
    private static readonly Regex WhitespaceRegex = new(
        @"\s+",
        RegexOptions.CultureInvariant);
    private static readonly Regex TokenRegex = new(
        @"[\p{L}\p{Nd}]+",
        RegexOptions.CultureInvariant);

    private readonly TimeProvider _timeProvider;

    public HomepageLiveDemoSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.PortfolioPresentation,
        maximumScore: 5,
        requiredDataKeys: [MetadataDataKey, ReadmeDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? metadataAvailability = snapshot.FindAvailability(MetadataDataKey);

        if (metadataAvailability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                [CreateAvailabilityEvidence(snapshot, metadataAvailability, MetadataDataKey)],
                evaluatedAt);
        }

        HomepageAnalysis homepage = AnalyzeHomepage(snapshot);
        EvidenceItem homepageEvidence = CreateHomepageEvidence(
            snapshot,
            metadataAvailability,
            homepage);

        if (homepage.Kind == HomepageKind.Meaningful)
        {
            return CreateResult(
                SignalStatus.Present,
                score: 5,
                [homepageEvidence],
                evaluatedAt);
        }

        if (homepage.Kind == HomepageKind.Malformed)
        {
            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                [homepageEvidence],
                evaluatedAt);
        }

        DataAvailability? readmeAvailability = snapshot.FindAvailability(ReadmeDataKey);

        if (readmeAvailability?.IsAvailable != true)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                [
                                homepageEvidence,
                                CreateAvailabilityEvidence(snapshot, readmeAvailability, ReadmeDataKey)
                            ],
                evaluatedAt);
        }

        ReadmeAnalysis readme = AnalyzeReadme(snapshot.Readme);

        if (readme.Explicit.Count > 0)
        {
            LinkCandidate candidate = readme.Explicit[0];

            return CreateResult(
                SignalStatus.Present,
                score: 4,
                [homepageEvidence, CreateReadmeLinkEvidence(snapshot, readmeAvailability, candidate)],
                evaluatedAt);
        }

        if (readme.Malformed.Count > 0)
        {
            LinkCandidate candidate = readme.Malformed[0];

            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                [homepageEvidence, CreateReadmeLinkEvidence(snapshot, readmeAvailability, candidate)],
                evaluatedAt);
        }

        if (readme.Ambiguous.Count > 0)
        {
            LinkCandidate candidate = readme.Ambiguous[0];

            return CreateResult(
                SignalStatus.Partial,
                score: 2,
                [homepageEvidence, CreateReadmeLinkEvidence(snapshot, readmeAvailability, candidate)],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            [homepageEvidence, CreateReadmeInspectionEvidence(snapshot, readmeAvailability)],
            evaluatedAt);
    }

    private static HomepageAnalysis AnalyzeHomepage(RepositorySnapshot snapshot)
    {
        string? value = snapshot.Metadata?.Homepage;

        if (value is null)
        {
            return new HomepageAnalysis(HomepageKind.Absent, null, "Metadata homepage alanı boş.");
        }

        if (!TryNormalizeHttpUrl(value, out Uri? uri, out string? normalized))
        {
            return new HomepageAnalysis(
                HomepageKind.Malformed,
                value,
                "Non-empty metadata homepage absolute HTTP/HTTPS URL olarak parse edilemedi.");
        }

        if (IsRepositoryManagementUrl(uri!, snapshot.Repository))
        {
            return new HomepageAnalysis(
                HomepageKind.Excluded,
                normalized,
                "Homepage repository GitHub veya repository-management URL'sine yöneliyor.");
        }

        if (IsDocumentationOnlyDestination(uri!))
        {
            return new HomepageAnalysis(
                HomepageKind.Excluded,
                normalized,
                "Homepage açık documentation-only destination'a yöneliyor.");
        }

        return new HomepageAnalysis(
            HomepageKind.Meaningful,
            normalized,
            "Absolute HTTP/HTTPS homepage repository-management ve documentation-only sınırlarının dışında.");
    }

    private static ReadmeAnalysis AnalyzeReadme(RepositoryReadme? readme)
    {
        if (readme is null)
        {
            return ReadmeAnalysis.Empty;
        }

        string[] lines = NormalizeLines(readme.Content);
        IReadOnlySet<int> fencedLines = FindFencedLines(lines);
        List<LinkCandidate> explicitCandidates = [];
        List<LinkCandidate> ambiguousCandidates = [];
        List<LinkCandidate> malformedCandidates = [];

        for (int line = 0; line < lines.Length; line++)
        {
            if (fencedLines.Contains(line))
            {
                continue;
            }

            string content = lines[line];

            foreach (Match match in MarkdownLinkRegex.Matches(content))
            {
                ClassifyCandidate(
                    line,
                    NormalizeLabel(match.Groups["label"].Value),
                    match.Groups["target"].Value.Trim(),
                    "Markdown link",
                    explicitCandidates,
                    ambiguousCandidates,
                    malformedCandidates);
            }

            foreach (Match match in HtmlAnchorRegex.Matches(content))
            {
                ClassifyCandidate(
                    line,
                    NormalizeLabel(match.Groups["label"].Value),
                    match.Groups["target"].Value.Trim(),
                    "HTML anchor",
                    explicitCandidates,
                    ambiguousCandidates,
                    malformedCandidates);
            }

            Match incomplete = IncompleteMarkdownLinkRegex.Match(content);

            if (incomplete.Success)
            {
                string label = NormalizeLabel(incomplete.Groups["label"].Value);

                if (IsExactDemoIntent(label) || HasAmbiguousIntent(label))
                {
                    malformedCandidates.Add(new LinkCandidate(
                        line,
                        label,
                        incomplete.Groups["target"].Value.Trim(),
                        "malformed Markdown link",
                        IsMalformed: true));
                }
            }
        }

        return new ReadmeAnalysis(
            NormalizeCandidates(explicitCandidates),
            NormalizeCandidates(ambiguousCandidates),
            NormalizeCandidates(malformedCandidates));
    }

    private static void ClassifyCandidate(
        int line,
        string label,
        string target,
        string syntax,
        ICollection<LinkCandidate> explicitCandidates,
        ICollection<LinkCandidate> ambiguousCandidates,
        ICollection<LinkCandidate> malformedCandidates)
    {
        bool isExplicit = IsExactDemoIntent(label);
        bool isAmbiguous = !isExplicit && HasAmbiguousIntent(label);

        if (!isExplicit && !isAmbiguous)
        {
            return;
        }

        string normalizedTarget = NormalizeTarget(target);

        if (!TryNormalizeHttpUrl(normalizedTarget, out _, out string? normalized))
        {
            malformedCandidates.Add(new LinkCandidate(
                line,
                label,
                target,
                $"malformed {syntax}",
                IsMalformed: true));
            return;
        }

        LinkCandidate candidate = new(
            line,
            label,
            normalized!,
            isExplicit ? "explicit demo intent" : "ambiguous presentation intent",
            IsMalformed: false);

        if (isExplicit)
        {
            explicitCandidates.Add(candidate);
        }
        else
        {
            ambiguousCandidates.Add(candidate);
        }
    }

    private static IReadOnlyList<LinkCandidate> NormalizeCandidates(
        IEnumerable<LinkCandidate> candidates) =>
        candidates
            .DistinctBy(
                item => $"{item.Line.ToString(CultureInfo.InvariantCulture)}|{item.Label}|{item.Target}|{item.Type}",
                StringComparer.Ordinal)
            .OrderBy(item => item.Line)
            .ThenBy(item => item.Label, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToArray();

    private static string[] NormalizeLines(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');

    private static IReadOnlySet<int> FindFencedLines(IReadOnlyList<string> lines)
    {
        HashSet<int> fenced = [];
        char? marker = null;
        int markerLength = 0;

        for (int line = 0; line < lines.Count; line++)
        {
            Match match = FenceRegex.Match(lines[line]);

            if (marker is null)
            {
                if (!match.Success)
                {
                    continue;
                }

                string marks = match.Groups["marks"].Value;
                marker = marks[0];
                markerLength = marks.Length;
                fenced.Add(line);
                continue;
            }

            fenced.Add(line);

            if (match.Success)
            {
                string marks = match.Groups["marks"].Value;

                if (marks[0] == marker && marks.Length >= markerLength)
                {
                    marker = null;
                    markerLength = 0;
                }
            }
        }

        return fenced;
    }

    private static bool IsExactDemoIntent(string label) =>
        DemoIntentLabels.Any(candidate => string.Equals(
            label,
            candidate,
            StringComparison.OrdinalIgnoreCase));

    private static bool HasAmbiguousIntent(string label)
    {
        foreach (Match token in TokenRegex.Matches(label))
        {
            if (AmbiguousIntentTokens.Any(candidate => string.Equals(
                token.Value,
                candidate,
                StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeLabel(string value) =>
        WhitespaceRegex.Replace(
            HtmlTagRegex.Replace(value, string.Empty),
            " ")
        .Trim();

    private static string NormalizeTarget(string target)
    {
        string normalized = target.Trim();

        if (normalized.StartsWith('<') && normalized.EndsWith('>'))
        {
            normalized = normalized[1..^1].Trim();
        }

        int whitespace = normalized.IndexOfAny([' ', '\t']);
        return whitespace >= 0 ? normalized[..whitespace] : normalized;
    }

    private static bool TryNormalizeHttpUrl(
        string? value,
        out Uri? uri,
        out string? normalized)
    {
        uri = null;
        normalized = null;

        if (string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? candidate)
            || (candidate.Scheme != Uri.UriSchemeHttp && candidate.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(candidate.Host))
        {
            return false;
        }

        string scheme = candidate.Scheme.ToLowerInvariant();
        string host = candidate.Host.ToLowerInvariant();
        bool defaultPort = (scheme == Uri.UriSchemeHttp && candidate.Port == 80)
            || (scheme == Uri.UriSchemeHttps && candidate.Port == 443);
        string port = defaultPort
            ? string.Empty
            : $":{candidate.Port.ToString(CultureInfo.InvariantCulture)}";
        string path = string.IsNullOrEmpty(candidate.AbsolutePath) ? "/" : candidate.AbsolutePath;

        if (path.Length > 1 && path[^1] == '/')
        {
            path = path[..^1];
        }

        uri = candidate;
        normalized = $"{scheme}://{host}{port}{path}{candidate.Query}";
        return true;
    }

    private static bool IsRepositoryManagementUrl(
        Uri uri,
        RepositoryIdentity repository)
    {
        if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Host, "www.github.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string[] segments = GetPathSegments(uri);

        if (segments.Length < 2
            || !string.Equals(segments[0], repository.Owner, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(segments[1], repository.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static bool IsDocumentationOnlyDestination(Uri uri)
    {
        string firstHostLabel = uri.Host.Split('.')[0];

        if (string.Equals(firstHostLabel, "docs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(firstHostLabel, "documentation", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return GetPathSegments(uri).Any(segment =>
            string.Equals(segment, "docs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "documentation", StringComparison.OrdinalIgnoreCase));
    }

    private static string[] GetPathSegments(Uri uri) =>
        uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString)
            .ToArray();

    private static EvidenceItem CreateHomepageEvidence(
        RepositorySnapshot snapshot,
        DataAvailability availability,
        HomepageAnalysis homepage) =>
        new(
            SignalKey,
            "RepositoryMetadata",
            homepage.Value ?? "none",
            "homepage",
            availability.RetrievedAt ?? snapshot.CapturedAt,
            homepage.Kind == HomepageKind.Malformed
                ? EvidenceConfidence.Low
                : EvidenceConfidence.High,
            homepage.Reason);

    private static EvidenceItem CreateAvailabilityEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability,
        string dataKey) =>
        new(
            SignalKey,
            dataKey == MetadataDataKey ? "MetadataAvailability" : "ReadmeAvailability",
            "unavailable",
            location: null,
            availability?.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            availability is null
                ? $"{dataKey} veri erişilebilirliği snapshot içinde kaydedilmemiş."
                : $"{dataKey} veri erişilebilirlik durumu: {availability.Status}. {availability.Reason}");

    private static EvidenceItem CreateReadmeLinkEvidence(
        RepositorySnapshot snapshot,
        DataAvailability availability,
        LinkCandidate candidate) =>
        new(
            SignalKey,
            "README",
            candidate.IsMalformed ? candidate.Target : $"{candidate.Label} -> {candidate.Target}",
            snapshot.Readme is null
                ? null
                : $"{snapshot.Readme.Path}:L{(candidate.Line + 1).ToString(CultureInfo.InvariantCulture)}",
            availability.RetrievedAt ?? snapshot.CapturedAt,
            candidate.IsMalformed ? EvidenceConfidence.Low : EvidenceConfidence.High,
            $"{candidate.Type}; label: {candidate.Label}.");

    private static EvidenceItem CreateReadmeInspectionEvidence(
        RepositorySnapshot snapshot,
        DataAvailability availability) =>
        new(
            SignalKey,
            "README",
            "0",
            snapshot.Readme?.Path,
            availability.RetrievedAt ?? snapshot.CapturedAt,
            EvidenceConfidence.High,
            "README başarıyla incelendi; eligible demo/presentation link count: 0.");

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        IEnumerable<EvidenceItem> evidence,
        DateTimeOffset evaluatedAt) =>
        new(
            Definition.Key,
            Definition.Version,
            Definition.Category,
            status,
            score,
            Definition.MaximumScore,
            Definition.IncludeInOverallScore,
            evidence,
            evaluatedAt);

    private enum HomepageKind
    {
        Absent,
        Meaningful,
        Excluded,
        Malformed
    }

    private sealed record HomepageAnalysis(
        HomepageKind Kind,
        string? Value,
        string Reason);

    private sealed record LinkCandidate(
        int Line,
        string Label,
        string Target,
        string Type,
        bool IsMalformed);

    private sealed record ReadmeAnalysis(
        IReadOnlyList<LinkCandidate> Explicit,
        IReadOnlyList<LinkCandidate> Ambiguous,
        IReadOnlyList<LinkCandidate> Malformed)
    {
        public static ReadmeAnalysis Empty { get; } = new([], [], []);
    }
}
