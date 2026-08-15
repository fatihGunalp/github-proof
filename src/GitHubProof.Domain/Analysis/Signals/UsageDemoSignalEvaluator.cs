using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class UsageDemoSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "documentation.usage-or-demo";

    private const string ReadmeDataKey = "readme";
    private const string DocumentationDataKey = "documentation-markdown";
    private const string MetadataDataKey = "metadata";
    private static readonly string[] UsageHeadings =
    [
        "usage",
        "how to use",
        "using",
        "example",
        "examples",
        "quick start",
        "getting started",
        "kullanım",
        "nasıl kullanılır",
        "örnek",
        "örnekler",
        "başlangıç"
    ];

    private static readonly string[] DemoHeadings =
    [
        "demo",
        "live demo",
        "preview",
        "screenshots",
        "screenshot",
        "canlı demo",
        "önizleme",
        "ekran görüntüsü",
        "ekran görüntüleri"
    ];

    private static readonly string[] RequestResponseLabels =
    [
        "request",
        "response",
        "example request",
        "example response",
        "istek",
        "yanıt",
        "örnek istek",
        "örnek yanıt"
    ];

    private static readonly string[] OutputLabels =
    [
        "output",
        "sample output",
        "result",
        "example result",
        "çıktı",
        "örnek çıktı",
        "sonuç"
    ];

    private static readonly string[] VideoLabels =
    [
        "video",
        "demo video",
        "walkthrough",
        "demo",
        "tanıtım videosu",
        "demo videosu"
    ];

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

    private static readonly string[] PositiveImageMarkers =
    [
        "screenshot",
        "screen",
        "preview",
        "demo",
        "ui",
        "interface",
        "ekran",
        "arayüz",
        "önizleme"
    ];

    private static readonly string[] ExcludedImageMarkers =
    [
        "badge",
        "shield",
        "logo",
        "icon",
        "avatar"
    ];

    private static readonly string[] InstallationOnlyPrefixes =
    [
        "dotnet restore",
        "dotnet build",
        "npm install",
        "npm ci",
        "npm run build",
        "yarn install",
        "yarn build",
        "pnpm install",
        "pnpm build",
        "pip install",
        "pip3 install",
        "poetry install",
        "bundle install",
        "composer install",
        "cargo build",
        "go build",
        "mvn package",
        "mvn compile",
        "gradle build",
        "./gradlew build",
        "install ",
        "configure ",
        "configuration ",
        "build ",
        "copy .env",
        "create .env",
        "set environment",
        "export ",
        "migrate ",
        "migration ",
        "run database migration",
        "run the database migration",
        "seed database",
        "create database",
        "prepare database",
        "initialize database",
        "bağımlılıkları kur",
        "paketleri kur",
        "yapılandır",
        "derle",
        "migration çalıştır",
        "veritabanını hazırla"
    ];

    private static readonly Regex AtxHeadingRegex = new(
        @"^\s{0,3}(?<marks>#{1,6})(?:[ \t]+|$)(?<text>.*)$",
        RegexOptions.CultureInvariant);
    private static readonly Regex SetextUnderlineRegex = new(
        @"^\s{0,3}(?<marks>=+|-+)\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex FenceOpeningRegex = new(
        @"^\s{0,3}(?<marks>`{3,}|~{3,})(?<info>.*)$",
        RegexOptions.CultureInvariant);
    private static readonly Regex OrderedItemRegex = new(
        @"^\s*\d+[.)]\s+(?<content>.+?)\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex InlineImageRegex = new(
        @"!\[(?<alt>[^\]\r\n]*)\]\((?<target>[^)\r\n]*)\)",
        RegexOptions.CultureInvariant);
    private static readonly Regex ReferenceImageRegex = new(
        @"!\[(?<alt>[^\]\r\n]*)\]\[(?<id>[^\]\r\n]+)\]",
        RegexOptions.CultureInvariant);
    private static readonly Regex ReferenceDefinitionRegex = new(
        @"^\s{0,3}\[(?<id>[^\]\r\n]+)\]:\s*(?:<(?<angled>[^>]+)>|(?<plain>\S+))",
        RegexOptions.CultureInvariant);
    private static readonly Regex MarkdownLinkRegex = new(
        @"(?<!!)\[(?<label>[^\]\r\n]+)\]\((?<target>[^)\r\n]+)\)",
        RegexOptions.CultureInvariant);
    private static readonly Regex HtmlAnchorRegex = new(
        @"<a\s+[^>]*href\s*=\s*[\""'](?<target>[^\""']+)[\""'][^>]*>(?<label>.*?)</a>",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex TokenRegex = new(
        @"[\p{L}\p{Nd}]+",
        RegexOptions.CultureInvariant);

    private readonly TimeProvider _timeProvider;

    public UsageDemoSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.RepositoryUnderstandability,
        maximumScore: 7,
        requiredDataKeys: [ReadmeDataKey, DocumentationDataKey, MetadataDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? readmeAvailability = snapshot.FindAvailability(ReadmeDataKey);
        DataAvailability? documentationAvailability = snapshot.FindAvailability(DocumentationDataKey);
        DataAvailability? metadataAvailability = snapshot.FindAvailability(MetadataDataKey);
        bool readmeComplete = readmeAvailability?.IsAvailable == true;
        bool documentationComplete = documentationAvailability?.IsAvailable == true
            && snapshot.DocumentationMarkdown.IsComplete
            && snapshot.DocumentationMarkdown.Items.All(file => file.Content is not null);
        bool corpusComplete = readmeComplete && documentationComplete;
        bool metadataAvailable = metadataAvailability?.IsAvailable == true;
        string? homepage = metadataAvailable ? snapshot.Metadata?.Homepage : null;

        List<SourceAnalysis> sources = [];

        if (readmeComplete && snapshot.Readme is not null)
        {
            sources.Add(AnalyzeSource(
                snapshot.Readme.Path,
                "README",
                snapshot.Readme.Content,
                readmeAvailability?.RetrievedAt ?? snapshot.CapturedAt,
                homepage));
        }

        foreach (RepositoryFile file in snapshot.DocumentationMarkdown.Items
                     .OrderBy(item => item.Path, StringComparer.Ordinal))
        {
            if (file.Content is not null)
            {
                sources.Add(AnalyzeSource(
                    file.Path,
                    "DocumentationMarkdown",
                    file.Content,
                    documentationAvailability?.RetrievedAt ?? snapshot.CapturedAt,
                    homepage));
            }
        }

        SourceAnalysis? strong = sources.FirstOrDefault(source =>
            source.Usage.Count > 0 && source.HighConfidenceDemo.Count > 0);

        if (strong is not null)
        {
            return CreateStrongResult(strong, evaluatedAt);
        }

        Ambiguity? blockingAmbiguity = SelectBlockingAmbiguity(sources);

        if (blockingAmbiguity is not null)
        {
            return CreateHumanReviewResult(blockingAmbiguity, evaluatedAt);
        }

        bool metadataCouldChangeResult = !metadataAvailable
            && sources.Any(source => source.Usage.Count > 0
                && source.DemoLinkCandidates.Count > 0
                && source.HighConfidenceDemo.Count == 0);

        if (!corpusComplete || metadataCouldChangeResult)
        {
            return CreateUnavailableResult(
                snapshot,
                readmeAvailability,
                documentationAvailability,
                metadataAvailability,
                metadataCouldChangeResult,
                evaluatedAt);
        }

        SourceAnalysis? present = sources.FirstOrDefault(source => source.Usage.Count > 0);

        if (present is not null)
        {
            return CreatePresentResult(present, evaluatedAt);
        }

        SourceAnalysis? partialHigh = sources.FirstOrDefault(source => source.HighConfidenceDemo.Count > 0);

        if (partialHigh is not null)
        {
            return CreatePartialResult(
                partialHigh,
                partialHigh.HighConfidenceDemo[0],
                isHighConfidence: true,
                evaluatedAt);
        }

        SourceAnalysis? partialWeak = sources.FirstOrDefault(source => source.WeakDemo.Count > 0);

        if (partialWeak is not null)
        {
            return CreatePartialResult(
                partialWeak,
                partialWeak.WeakDemo[0],
                isHighConfidence: false,
                evaluatedAt);
        }

        return CreateMissingResult(
            snapshot,
            readmeAvailability!,
            documentationAvailability!,
            evaluatedAt);
    }

    private SignalResult CreateStrongResult(
        SourceAnalysis source,
        DateTimeOffset evaluatedAt)
    {
        UsageEvidence usage = source.Usage[0];
        DemoEvidence demo = source.HighConfidenceDemo[0];
        EvidenceItem[] evidence =
        [
            CreateUsageEvidence(source, usage),
            CreateDemoEvidence(source, demo, EvidenceConfidence.High)
        ];

        return CreateResult(
            SignalStatus.Strong,
            score: 7,
            evidence,
            evaluatedAt);
    }

    private SignalResult CreatePresentResult(
        SourceAnalysis source,
        DateTimeOffset evaluatedAt)
    {
        UsageEvidence usage = source.Usage[0];

        return CreateResult(
            SignalStatus.Present,
            score: 5,
            [CreateUsageEvidence(source, usage)],
            evaluatedAt);
    }

    private SignalResult CreatePartialResult(
        SourceAnalysis source,
        DemoEvidence demo,
        bool isHighConfidence,
        DateTimeOffset evaluatedAt)
    {
        return CreateResult(
            SignalStatus.Partial,
            score: 4,
            [CreateDemoEvidence(
                        source,
                        demo,
                        isHighConfidence ? EvidenceConfidence.High : EvidenceConfidence.Medium)],
            evaluatedAt);
    }

    private SignalResult CreateHumanReviewResult(
        Ambiguity ambiguity,
        DateTimeOffset evaluatedAt)
    {
        EvidenceItem evidence = new(
            SignalKey,
            ambiguity.SourceKind,
            "ambiguous",
            ambiguity.SourcePath,
            ambiguity.ObservedAt,
            EvidenceConfidence.Medium,
            ambiguity.Reason);

        return CreateResult(
            SignalStatus.RequiresHumanReview,
            score: null,
            [evidence],
            evaluatedAt);
    }

    private SignalResult CreateUnavailableResult(
        RepositorySnapshot snapshot,
        DataAvailability? readmeAvailability,
        DataAvailability? documentationAvailability,
        DataAvailability? metadataAvailability,
        bool includeMetadataAvailability,
        DateTimeOffset evaluatedAt)
    {
        List<EvidenceItem> evidence =
        [
            CreateAvailabilityEvidence(
                "READMEAvailability",
                ReadmeDataKey,
                readmeAvailability,
                readmeAvailability?.IsAvailable == true
                    ? snapshot.Readme is null ? "available-absent" : "available"
                    : "unavailable",
                snapshot.CapturedAt),
            CreateAvailabilityEvidence(
                "DocumentationMarkdownAvailability",
                DocumentationDataKey,
                documentationAvailability,
                documentationAvailability?.IsAvailable == true
                    ? snapshot.DocumentationMarkdown.IsComplete
                        ? "available-complete"
                        : "available-incomplete"
                    : "unavailable",
                snapshot.CapturedAt)
        ];

        if (includeMetadataAvailability)
        {
            evidence.Add(CreateAvailabilityEvidence(
                "MetadataAvailability",
                MetadataDataKey,
                metadataAvailability,
                metadataAvailability?.IsAvailable == true ? "available" : "unavailable",
                snapshot.CapturedAt));
        }

        return CreateResult(
            SignalStatus.UnavailableEvidence,
            score: null,
            evidence,
            evaluatedAt);
    }

    private SignalResult CreateMissingResult(
        RepositorySnapshot snapshot,
        DataAvailability readmeAvailability,
        DataAvailability documentationAvailability,
        DateTimeOffset evaluatedAt)
    {
        EvidenceItem[] evidence =
        [
            new EvidenceItem(
                SignalKey,
                "READMEInspection",
                snapshot.Readme is null ? "available-absent" : "available-inspected",
                snapshot.Readme?.Path,
                readmeAvailability.RetrievedAt ?? snapshot.CapturedAt,
                EvidenceConfidence.High,
                "README başarıyla incelendi; usage count = 0; visual/demo count = 0."),
            new EvidenceItem(
                SignalKey,
                "DocumentationMarkdownInspection",
                "available-complete",
                "docs/**/*.md",
                documentationAvailability.RetrievedAt ?? snapshot.CapturedAt,
                EvidenceConfidence.High,
                $"Complete docs corpus içindeki {snapshot.DocumentationMarkdown.Items.Count.ToString(CultureInfo.InvariantCulture)} dosya incelendi; usage count = 0; visual/demo count = 0.")
        ];

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            evaluatedAt);
    }

    private static EvidenceItem CreateUsageEvidence(
        SourceAnalysis source,
        UsageEvidence usage)
    {
        return new EvidenceItem(
            SignalKey,
            source.SourceKind,
            $"usage:{usage.Type}",
            usage.Location,
            source.ObservedAt,
            EvidenceConfidence.High,
            $"Clear usage evidence: {usage.Value}.");
    }

    private static EvidenceItem CreateDemoEvidence(
        SourceAnalysis source,
        DemoEvidence demo,
        EvidenceConfidence confidence)
    {
        return new EvidenceItem(
            SignalKey,
            source.SourceKind,
            $"demo:{demo.Type}",
            demo.Location,
            source.ObservedAt,
            confidence,
            demo.Reason);
    }

    private static EvidenceItem CreateAvailabilityEvidence(
        string source,
        string dataKey,
        DataAvailability? availability,
        string value,
        DateTimeOffset capturedAt)
    {
        string notes = availability is null
            ? $"'{dataKey}' erişilebilirliği snapshot içinde kaydedilmemiş."
            : $"'{dataKey}' durumu: {availability.Status}. {availability.Reason}".Trim();

        return new EvidenceItem(
            SignalKey,
            source,
            value,
            location: null,
            availability?.RetrievedAt ?? capturedAt,
            EvidenceConfidence.High,
            notes);
    }

    private static SourceAnalysis AnalyzeSource(
        string sourcePath,
        string sourceKind,
        string content,
        DateTimeOffset observedAt,
        string? homepage)
    {
        MarkdownDocument document = ParseMarkdown(content);
        IReadOnlyList<Section> usageSections = CreateSections(
            document,
            heading => IsExact(heading.Text, UsageHeadings));
        IReadOnlyList<Section> demoSections = CreateSections(
            document,
            heading => IsExact(heading.Text, DemoHeadings));
        IReadOnlyList<UsageEvidence> usage = FindUsageEvidence(
            sourcePath,
            document,
            usageSections);
        List<DemoEvidence> highDemo = [];
        List<DemoEvidence> weakDemo = [];
        IReadOnlyList<MarkdownImage> images = ParseImages(document);
        IReadOnlyList<MarkdownLink> links = ParseLinks(document);

        foreach (MarkdownImage image in images)
        {
            string markerText = $"{image.Alt} {GetTargetFileName(image.Target)}";
            bool positive = HasAnyMarker(markerText, PositiveImageMarkers);
            bool excluded = HasAnyMarker(markerText, ExcludedImageMarkers);
            bool inDemoSection = FindContainingSection(demoSections, image.Line) is not null;

            if (positive && excluded)
            {
                weakDemo.Add(new DemoEvidence(
                    image.Line,
                    "ambiguous screenshot",
                    NormalizeEvidenceValue($"{image.Alt} -> {image.Target}"),
                    CreateNodeLocation(sourcePath, demoSections, image.Line),
                    $"Image positive ve exclusion marker'larını birlikte taşıdığı için bounded weak evidence: {NormalizeEvidenceValue(markerText)}."));
            }
            else if (!excluded && (positive || inDemoSection))
            {
                highDemo.Add(new DemoEvidence(
                    image.Line,
                    "screenshot/image",
                    NormalizeEvidenceValue($"{image.Alt} -> {image.Target}"),
                    CreateNodeLocation(sourcePath, demoSections, image.Line),
                    $"High-confidence screenshot/image evidence: {NormalizeEvidenceValue($"{image.Alt} -> {image.Target}")}."));
            }
        }

        foreach (MarkdownLink link in links)
        {
            bool inUsageOrDemoSection = FindContainingSection(usageSections, link.Line) is not null
                || FindContainingSection(demoSections, link.Line) is not null;

            if (IsVideoTarget(link.Target)
                && (inUsageOrDemoSection || IsExact(link.Label, VideoLabels)))
            {
                highDemo.Add(new DemoEvidence(
                    link.Line,
                    "video",
                    NormalizeEvidenceValue($"{link.Label} -> {link.Target}"),
                    CreateNodeLocation(sourcePath, demoSections, link.Line),
                    $"Canonical video evidence: {NormalizeEvidenceValue($"{link.Label} -> {link.Target}")}."));
            }
        }

        List<MarkdownLink> demoLinkCandidates = links
            .Where(link => IsExact(link.Label, DemoIntentLabels)
                && TryNormalizeHttpUrl(link.Target, out _))
            .OrderBy(link => link.Line)
            .ThenBy(link => link.Target, StringComparer.Ordinal)
            .ToList();

        foreach (MarkdownLink link in demoLinkCandidates)
        {
            if (TryNormalizeHttpUrl(link.Target, out string? normalizedTarget)
                && TryNormalizeHttpUrl(homepage, out string? normalizedHomepage)
                && string.Equals(normalizedTarget, normalizedHomepage, StringComparison.Ordinal))
            {
                highDemo.Add(new DemoEvidence(
                    link.Line,
                    "live demo",
                    NormalizeEvidenceValue($"{link.Label} -> {link.Target}"),
                    CreateNodeLocation(sourcePath, demoSections, link.Line),
                    $"Explicit demo-intent link metadata homepage ile corroborate edildi: {normalizedTarget}."));
            }
        }

        highDemo = highDemo
            .DistinctBy(item => item.Identity, StringComparer.Ordinal)
            .OrderBy(item => item.Line)
            .ThenBy(item => item.Type, StringComparer.Ordinal)
            .ThenBy(item => item.Value, StringComparer.Ordinal)
            .ToList();

        if (highDemo.Count == 0)
        {
            foreach (MarkdownLink link in demoLinkCandidates)
            {
                weakDemo.Add(new DemoEvidence(
                    link.Line,
                    "brittle demo link",
                    NormalizeEvidenceValue($"{link.Label} -> {link.Target}"),
                    CreateNodeLocation(sourcePath, demoSections, link.Line),
                    "Explicit demo-intent link homepage ile corroborate edilmedi; remote link health değerlendirilmedi."));
            }
        }

        weakDemo = weakDemo
            .DistinctBy(item => item.Identity, StringComparer.Ordinal)
            .OrderBy(item => item.Line)
            .ThenBy(item => item.Type, StringComparer.Ordinal)
            .ThenBy(item => item.Value, StringComparer.Ordinal)
            .ToList();

        IReadOnlyList<Ambiguity> ambiguities = FindAmbiguities(
            sourcePath,
            sourceKind,
            observedAt,
            document,
            usageSections,
            demoSections,
            images,
            links);

        return new SourceAnalysis(
            sourcePath,
            sourceKind,
            observedAt,
            usage,
            highDemo,
            weakDemo,
            demoLinkCandidates,
            ambiguities);
    }

    private static IReadOnlyList<UsageEvidence> FindUsageEvidence(
        string sourcePath,
        MarkdownDocument document,
        IReadOnlyList<Section> usageSections)
    {
        List<UsageEvidence> evidence = [];

        foreach (Section section in usageSections)
        {
            string location = CreateLocation(sourcePath, section.Heading.Text);
            IReadOnlyList<SampleContext> sampleContexts = CreateSampleContexts(
                document,
                section);

            foreach (FenceBlock fence in document.Fences.Where(fence =>
                         fence.StartLine >= section.Heading.BodyStartLine
                         && fence.EndLine < section.EndLine))
            {
                string[] body = document.Lines
                    .Skip(fence.StartLine + 1)
                    .Take(fence.EndLine - fence.StartLine - 1)
                    .ToArray();
                bool sampleContext = sampleContexts.Any(context =>
                    fence.StartLine >= context.BodyStartLine
                    && fence.StartLine < context.EndLine);

                if ((sampleContext && body.Any(line => !string.IsNullOrWhiteSpace(line)))
                    || body.Any(IsMeaningfulUsageLine))
                {
                    string representative = body
                        .Select(line => line.Trim())
                        .First(line => !string.IsNullOrWhiteSpace(line));
                    evidence.Add(new UsageEvidence(
                        fence.StartLine,
                        "fenced code example",
                        NormalizeEvidenceValue(representative),
                        location));
                }
            }

            List<(int Line, string Content)> orderedItems = [];

            for (int line = section.Heading.BodyStartLine; line < section.EndLine; line++)
            {
                if (document.FencedLines.Contains(line))
                {
                    continue;
                }

                Match ordered = OrderedItemRegex.Match(document.Lines[line]);

                if (ordered.Success)
                {
                    string content = ordered.Groups["content"].Value.Trim();

                    if (!IsStandaloneArtifact(content))
                    {
                        orderedItems.Add((line, content));
                    }
                }

                if (TryGetUsageCommand(document.Lines[line], out string? command))
                {
                    evidence.Add(new UsageEvidence(
                        line,
                        "usage command",
                        NormalizeEvidenceValue(command!),
                        location));
                }
            }

            (int Line, string Content)? usageStep = null;

            foreach ((int line, string content) in orderedItems)
            {
                if (!IsInstallationOnly(content))
                {
                    usageStep = (line, content);
                    break;
                }
            }

            if (usageStep is not null)
            {
                evidence.Add(new UsageEvidence(
                    usageStep.Value.Line,
                    "ordered usage step",
                    NormalizeEvidenceValue(usageStep.Value.Content),
                    location));
            }

            foreach (SampleContext context in sampleContexts)
            {
                if (HasSampleContent(document, context))
                {
                    evidence.Add(new UsageEvidence(
                        context.LabelLine,
                        context.Type,
                        context.Label,
                        location));
                }
            }
        }

        return evidence
            .DistinctBy(
                item => $"{item.Line.ToString(CultureInfo.InvariantCulture)}|{item.Type}|{item.Value}",
                StringComparer.Ordinal)
            .OrderBy(item => item.Line)
            .ThenBy(item => item.Type, StringComparer.Ordinal)
            .ThenBy(item => item.Value, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<SampleContext> CreateSampleContexts(
        MarkdownDocument document,
        Section usageSection)
    {
        List<SampleContext> contexts = [];

        foreach (Heading heading in document.Headings.Where(heading =>
                     heading.Line >= usageSection.Heading.BodyStartLine
                     && heading.Line < usageSection.EndLine
                     && IsSampleLabel(heading.Text)))
        {
            contexts.Add(new SampleContext(
                heading.Line,
                heading.BodyStartLine,
                FindSectionEnd(document.Headings, heading, usageSection.EndLine),
                GetSampleType(heading.Text),
                NormalizeLabel(heading.Text)));
        }

        for (int line = usageSection.Heading.BodyStartLine; line < usageSection.EndLine; line++)
        {
            if (document.FencedLines.Contains(line)
                || document.Headings.Any(heading => heading.Line == line))
            {
                continue;
            }

            string label = NormalizeStandaloneLabel(document.Lines[line]);

            if (!IsSampleLabel(label))
            {
                continue;
            }

            int endLine = usageSection.EndLine;

            for (int candidate = line + 1; candidate < usageSection.EndLine; candidate++)
            {
                if (document.Headings.Any(heading => heading.Line == candidate)
                    || IsSampleLabel(NormalizeStandaloneLabel(document.Lines[candidate])))
                {
                    endLine = candidate;
                    break;
                }
            }

            contexts.Add(new SampleContext(
                line,
                line + 1,
                endLine,
                GetSampleType(label),
                label));
        }

        return contexts
            .OrderBy(context => context.LabelLine)
            .ThenBy(context => context.Type, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool HasSampleContent(
        MarkdownDocument document,
        SampleContext context)
    {
        if (document.Fences.Any(fence =>
                fence.StartLine >= context.BodyStartLine
                && fence.EndLine < context.EndLine
                && document.Lines
                    .Skip(fence.StartLine + 1)
                    .Take(fence.EndLine - fence.StartLine - 1)
                    .Any(line => !string.IsNullOrWhiteSpace(line))))
        {
            return true;
        }

        for (int line = context.BodyStartLine; line < context.EndLine; line++)
        {
            if (document.FencedLines.Contains(line)
                || document.Headings.Any(heading => heading.Line == line))
            {
                continue;
            }

            string value = document.Lines[line].Trim();

            if (!string.IsNullOrWhiteSpace(value)
                && ContainsAlphanumeric(value)
                && !IsStandaloneArtifact(value))
            {
                return true;
            }
        }

        return false;
    }

    private static MarkdownDocument ParseMarkdown(string content)
    {
        string normalized = content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        List<Heading> headings = [];
        List<FenceBlock> fences = [];
        HashSet<int> fencedLines = [];
        FenceState? fence = null;
        int? unclosedFenceStart = null;

        for (int index = 0; index < lines.Length; index++)
        {
            if (fence is not null)
            {
                fencedLines.Add(index);

                if (IsFenceClosing(lines[index], fence))
                {
                    fences.Add(new FenceBlock(fence.StartLine, index));
                    fence = null;
                }

                continue;
            }

            if (TryGetFenceOpening(lines[index], index, out FenceState? opening))
            {
                fence = opening;
                fencedLines.Add(index);
                continue;
            }

            if (TryParseAtxHeading(lines[index], index, out Heading? atx))
            {
                headings.Add(atx!);
                continue;
            }

            if (index > 0
                && !fencedLines.Contains(index - 1)
                && TryParseSetextHeading(lines[index - 1], lines[index], index - 1, out Heading? setext)
                && !headings.Any(heading => heading.Line == index - 1))
            {
                headings.Add(setext!);
            }
        }

        if (fence is not null)
        {
            unclosedFenceStart = fence.StartLine;
        }

        Dictionary<string, string> references = new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < lines.Length; index++)
        {
            if (fencedLines.Contains(index))
            {
                continue;
            }

            Match definition = ReferenceDefinitionRegex.Match(lines[index]);

            if (definition.Success)
            {
                string id = definition.Groups["id"].Value.Trim();
                string target = definition.Groups["angled"].Success
                    ? definition.Groups["angled"].Value
                    : definition.Groups["plain"].Value;
                references.TryAdd(id, NormalizeTarget(target));
            }
        }

        return new MarkdownDocument(
            lines,
            headings.OrderBy(heading => heading.Line).ToArray(),
            fences,
            fencedLines,
            references,
            unclosedFenceStart);
    }

    private static IReadOnlyList<MarkdownImage> ParseImages(MarkdownDocument document)
    {
        List<MarkdownImage> images = [];

        for (int line = 0; line < document.Lines.Count; line++)
        {
            if (document.FencedLines.Contains(line))
            {
                continue;
            }

            foreach (Match match in InlineImageRegex.Matches(document.Lines[line]))
            {
                string target = NormalizeTarget(match.Groups["target"].Value);

                if (!string.IsNullOrWhiteSpace(target))
                {
                    images.Add(new MarkdownImage(
                        line,
                        match.Groups["alt"].Value.Trim(),
                        target));
                }
            }

            foreach (Match match in ReferenceImageRegex.Matches(document.Lines[line]))
            {
                string id = match.Groups["id"].Value.Trim();

                if (document.References.TryGetValue(id, out string? target)
                    && !string.IsNullOrWhiteSpace(target))
                {
                    images.Add(new MarkdownImage(
                        line,
                        match.Groups["alt"].Value.Trim(),
                        target));
                }
            }
        }

        return images
            .DistinctBy(
                image => $"{image.Line.ToString(CultureInfo.InvariantCulture)}|{image.Alt}|{image.Target}",
                StringComparer.Ordinal)
            .OrderBy(image => image.Line)
            .ThenBy(image => image.Target, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<MarkdownLink> ParseLinks(MarkdownDocument document)
    {
        List<MarkdownLink> links = [];

        for (int line = 0; line < document.Lines.Count; line++)
        {
            if (document.FencedLines.Contains(line))
            {
                continue;
            }

            foreach (Match match in MarkdownLinkRegex.Matches(document.Lines[line]))
            {
                string target = NormalizeTarget(match.Groups["target"].Value);

                if (!string.IsNullOrWhiteSpace(target))
                {
                    links.Add(new MarkdownLink(
                        line,
                        match.Groups["label"].Value.Trim(),
                        target));
                }
            }

            foreach (Match match in HtmlAnchorRegex.Matches(document.Lines[line]))
            {
                string target = NormalizeTarget(match.Groups["target"].Value);

                if (!string.IsNullOrWhiteSpace(target))
                {
                    links.Add(new MarkdownLink(
                        line,
                        NormalizeWhitespace(match.Groups["label"].Value),
                        target));
                }
            }
        }

        return links
            .DistinctBy(
                link => $"{link.Line.ToString(CultureInfo.InvariantCulture)}|{link.Label}|{link.Target}",
                StringComparer.Ordinal)
            .OrderBy(link => link.Line)
            .ThenBy(link => link.Target, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<Ambiguity> FindAmbiguities(
        string sourcePath,
        string sourceKind,
        DateTimeOffset observedAt,
        MarkdownDocument document,
        IReadOnlyList<Section> usageSections,
        IReadOnlyList<Section> demoSections,
        IReadOnlyList<MarkdownImage> images,
        IReadOnlyList<MarkdownLink> links)
    {
        List<Ambiguity> ambiguities = [];

        if (document.UnclosedFenceStart is int fenceLine)
        {
            bool inUsage = FindContainingSection(usageSections, fenceLine) is not null;
            bool inDemo = FindContainingSection(demoSections, fenceLine) is not null;
            bool hidesRelevantHeading = document.Lines
                .Skip(fenceLine + 1)
                .Any(IsRawUsageOrDemoHeading);

            if (inUsage || inDemo || hidesRelevantHeading)
            {
                ambiguities.Add(new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    fenceLine,
                    hidesRelevantHeading || (inUsage && inDemo)
                        ? AmbiguityKind.Both
                        : inUsage ? AmbiguityKind.Usage : AmbiguityKind.Demo,
                    "Canonical usage/demo bağlamındaki fenced block kapanmamış; section ve evidence sınırı güvenilir biçimde belirlenemiyor."));
            }
        }

        for (int line = 0; line < document.Lines.Count; line++)
        {
            if (document.FencedLines.Contains(line))
            {
                continue;
            }

            string value = document.Lines[line];
            bool hasParsedImage = images.Any(image => image.Line == line);

            if (value.Contains("![", StringComparison.Ordinal)
                && !hasParsedImage
                && (FindContainingSection(demoSections, line) is not null
                    || HasAnyMarker(value, PositiveImageMarkers)
                    || HasAnyMarker(value, ExcludedImageMarkers)))
            {
                ambiguities.Add(new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    line,
                    AmbiguityKind.Demo,
                    "Screenshot/demo adayı image syntax'ı malformed olduğu için visual evidence güvenilir biçimde sınıflandırılamıyor."));
            }

            Match incompleteLink = Regex.Match(
                value,
                @"(?<!!)\[(?<label>[^\]]+)\]\(",
                RegexOptions.CultureInvariant);

            if (incompleteLink.Success
                && !links.Any(link => link.Line == line)
                && (IsExact(incompleteLink.Groups["label"].Value, DemoIntentLabels)
                    || IsExact(incompleteLink.Groups["label"].Value, VideoLabels)))
            {
                ambiguities.Add(new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    line,
                    AmbiguityKind.Demo,
                    "Canonical demo/video label taşıyan link syntax'ı malformed olduğu için demo evidence güvenilir biçimde sınıflandırılamıyor."));
            }
        }

        return ambiguities
            .OrderBy(item => item.Line)
            .ThenBy(item => item.Kind)
            .ToArray();
    }

    private static Ambiguity? SelectBlockingAmbiguity(
        IReadOnlyList<SourceAnalysis> sources)
    {
        bool hasUsage = sources.Any(source => source.Usage.Count > 0);
        bool hasPresentation = sources.Any(source =>
            source.HighConfidenceDemo.Count > 0 || source.WeakDemo.Count > 0);

        foreach (SourceAnalysis source in sources)
        {
            foreach (Ambiguity ambiguity in source.Ambiguities)
            {
                bool couldChange = ambiguity.Kind switch
                {
                    AmbiguityKind.Usage => !hasUsage || source.HighConfidenceDemo.Count > 0,
                    AmbiguityKind.Demo => source.Usage.Count > 0
                        || (!hasUsage && !hasPresentation),
                    AmbiguityKind.Both => true,
                    _ => false
                };

                if (couldChange)
                {
                    return ambiguity;
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<Section> CreateSections(
        MarkdownDocument document,
        Func<Heading, bool> predicate)
    {
        return document.Headings
            .Where(predicate)
            .Select(heading => new Section(
                heading,
                FindSectionEnd(document.Headings, heading, document.Lines.Count)))
            .ToArray();
    }

    private static Section? FindContainingSection(
        IReadOnlyList<Section> sections,
        int line)
    {
        return sections
            .Where(section => line >= section.Heading.BodyStartLine && line < section.EndLine)
            .OrderByDescending(section => section.Heading.Level)
            .ThenByDescending(section => section.Heading.Line)
            .FirstOrDefault();
    }

    private static int FindSectionEnd(
        IReadOnlyList<Heading> headings,
        Heading heading,
        int documentEnd)
    {
        Heading? next = headings.FirstOrDefault(candidate =>
            candidate.Line > heading.Line && candidate.Level <= heading.Level);
        return next?.Line ?? documentEnd;
    }

    private static bool TryParseAtxHeading(
        string line,
        int lineNumber,
        out Heading? heading)
    {
        Match match = AtxHeadingRegex.Match(line);

        if (!match.Success)
        {
            heading = null;
            return false;
        }

        string text = RemoveTrailingAtxDecoration(match.Groups["text"].Value);

        if (string.IsNullOrWhiteSpace(text))
        {
            heading = null;
            return false;
        }

        heading = new Heading(
            lineNumber,
            lineNumber + 1,
            match.Groups["marks"].Value.Length,
            text);
        return true;
    }

    private static bool TryParseSetextHeading(
        string textLine,
        string underlineLine,
        int lineNumber,
        out Heading? heading)
    {
        Match underline = SetextUnderlineRegex.Match(underlineLine);
        string text = textLine.Trim();

        if (!underline.Success || string.IsNullOrWhiteSpace(text))
        {
            heading = null;
            return false;
        }

        heading = new Heading(
            lineNumber,
            lineNumber + 2,
            underline.Groups["marks"].Value[0] == '=' ? 1 : 2,
            text);
        return true;
    }

    private static bool TryGetFenceOpening(
        string line,
        int lineNumber,
        out FenceState? fence)
    {
        Match match = FenceOpeningRegex.Match(line);

        if (!match.Success)
        {
            fence = null;
            return false;
        }

        string marks = match.Groups["marks"].Value;
        fence = new FenceState(marks[0], marks.Length, lineNumber);
        return true;
    }

    private static bool IsFenceClosing(string line, FenceState fence)
    {
        string trimmed = line.Trim();

        if (trimmed.Length < fence.Length
            || trimmed.Any(character => character != fence.Marker))
        {
            return false;
        }

        return trimmed.Length >= fence.Length;
    }

    private static bool IsRawUsageOrDemoHeading(string line)
    {
        if (TryParseAtxHeading(line, lineNumber: 0, out Heading? heading))
        {
            return IsExact(heading!.Text, UsageHeadings)
                || IsExact(heading.Text, DemoHeadings);
        }

        return false;
    }

    private static bool IsMeaningfulUsageLine(string line)
    {
        string normalized = NormalizeInstruction(line);

        if (!ContainsAlphanumeric(normalized)
            || normalized.StartsWith("#", StringComparison.Ordinal)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.StartsWith(";", StringComparison.Ordinal)
            || normalized.StartsWith("<!--", StringComparison.Ordinal))
        {
            return false;
        }

        if (TryGetUsageCommand(normalized, out _))
        {
            return true;
        }

        return !IsDependencyOrVersionOnly(normalized)
            && !IsConfigurationFragment(normalized)
            && !IsInstallationOnly(normalized);
    }

    private static bool TryGetUsageCommand(
        string line,
        out string? command)
    {
        string normalized = NormalizeInstruction(line);
        string lower = normalized.ToLowerInvariant();

        if (IsCommand(lower, "dotnet run")
            || IsCommand(lower, "npm start")
            || IsCommand(lower, "npm run dev")
            || IsCommand(lower, "docker compose up"))
        {
            command = normalized;
            return true;
        }

        if (HasRequiredArgument(lower, "python")
            || HasRequiredArgument(lower, "curl")
            || HasRequiredArgument(lower, "git clone"))
        {
            command = normalized;
            return true;
        }

        command = null;
        return false;
    }

    private static bool IsCommand(string line, string command)
    {
        return string.Equals(line, command, StringComparison.OrdinalIgnoreCase)
            || line.StartsWith(command + " ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRequiredArgument(string line, string command)
    {
        return line.StartsWith(command + " ", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(line[(command.Length + 1)..]);
    }

    private static bool IsInstallationOnly(string line)
    {
        string normalized = NormalizeInstruction(line).ToLowerInvariant();

        if (TryGetUsageCommand(normalized, out _))
        {
            return false;
        }

        return InstallationOnlyPrefixes.Any(prefix =>
            normalized.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsDependencyOrVersionOnly(string line)
    {
        string normalized = NormalizeInstruction(line);

        if (Regex.IsMatch(
                normalized,
                @"^(?:[\p{L}\p{Nd}_.+-]+\s+)?v?\d+(?:\.\d+){0,3}(?:\s*[-+][\p{L}\p{Nd}.-]+)?$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
        {
            return true;
        }

        return Regex.IsMatch(
            normalized,
            @"^[\p{L}\p{Nd}_.+-]+\s*(?:>=|<=|==|=|>|<|\^|~)\s*v?\d+(?:\.\d+){0,3}$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    private static bool IsConfigurationFragment(string line)
    {
        string normalized = NormalizeInstruction(line);
        return Regex.IsMatch(
                normalized,
                @"^[\p{L}_][\p{L}\p{Nd}_]*\s*=\s*\S+\s*$",
                RegexOptions.CultureInvariant)
            || Regex.IsMatch(
                normalized,
                @"^[\""']?[\p{L}_][\p{L}\p{Nd}_.-]*[\""']?\s*:\s*.+$",
                RegexOptions.CultureInvariant)
            || (normalized.StartsWith("{", StringComparison.Ordinal)
                && normalized.EndsWith("}", StringComparison.Ordinal));
    }

    private static bool IsStandaloneArtifact(string content)
    {
        string value = content.Trim();

        if (Regex.IsMatch(value, @"^!?\[[^\]]*\](?:\([^)]*\)|\[[^\]]*\])$", RegexOptions.CultureInvariant)
            || Regex.IsMatch(value, @"^https?://\S+$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
            || Regex.IsMatch(value, @"^`[^`]+`$", RegexOptions.CultureInvariant))
        {
            return true;
        }

        return IsDependencyOrVersionOnly(value);
    }

    private static bool IsVideoTarget(string target)
    {
        string cleanTarget = StripQueryAndFragment(target);

        if (cleanTarget.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
            || cleanTarget.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(target, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        return IsHost(uri.Host, "youtube.com")
            || IsHost(uri.Host, "youtu.be")
            || IsHost(uri.Host, "vimeo.com");
    }

    private static bool IsHost(string actual, string expected)
    {
        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
            || actual.EndsWith("." + expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryNormalizeHttpUrl(
        string? value,
        out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        string scheme = uri.Scheme.ToLowerInvariant();
        string host = uri.Host.ToLowerInvariant();
        bool defaultPort = (scheme == Uri.UriSchemeHttp && uri.Port == 80)
            || (scheme == Uri.UriSchemeHttps && uri.Port == 443);
        string port = defaultPort ? string.Empty : $":{uri.Port.ToString(CultureInfo.InvariantCulture)}";
        string path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;

        if (path.Length > 1 && path[^1] == '/')
        {
            path = path[..^1];
        }

        normalized = $"{scheme}://{host}{port}{path}{uri.Query}";
        return true;
    }

    private static bool HasAnyMarker(
        string value,
        IReadOnlyList<string> markers)
    {
        foreach (Match match in TokenRegex.Matches(value))
        {
            string token = match.Value;

            foreach (string marker in markers)
            {
                if (string.Equals(token, marker, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (token.StartsWith(marker, StringComparison.OrdinalIgnoreCase)
                    && token.Length > marker.Length
                    && token[marker.Length..].All(char.IsDigit))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string GetTargetFileName(string target)
    {
        string clean = StripQueryAndFragment(target).Replace('\\', '/');
        int slash = clean.LastIndexOf('/');
        string segment = slash >= 0 ? clean[(slash + 1)..] : clean;
        int extension = segment.LastIndexOf('.');
        return extension > 0 ? segment[..extension] : segment;
    }

    private static string StripQueryAndFragment(string target)
    {
        int query = target.IndexOfAny(['?', '#']);
        return query >= 0 ? target[..query] : target;
    }

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

    private static string NormalizeInstruction(string line)
    {
        string normalized = line.Trim();
        Match ordered = OrderedItemRegex.Match(normalized);

        if (ordered.Success)
        {
            normalized = ordered.Groups["content"].Value.Trim();
        }
        else
        {
            normalized = Regex.Replace(
                normalized,
                @"^[-+*]\s+",
                string.Empty,
                RegexOptions.CultureInvariant);
        }

        if (normalized.StartsWith("$ ", StringComparison.Ordinal)
            || normalized.StartsWith("> ", StringComparison.Ordinal))
        {
            normalized = normalized[2..].TrimStart();
        }

        return NormalizeWhitespace(normalized);
    }

    private static string NormalizeEvidenceValue(string value)
    {
        string normalized = NormalizeWhitespace(value);
        return normalized.Length <= 160 ? normalized : normalized[..157] + "...";
    }

    private static string NormalizeWhitespace(string value)
    {
        StringBuilder builder = new();
        bool pendingSpace = false;

        foreach (char character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static bool ContainsAlphanumeric(string value)
    {
        return value.Any(char.IsLetterOrDigit);
    }

    private static bool IsExact(string value, IReadOnlyList<string> candidates)
    {
        string normalized = NormalizeLabel(value);
        return candidates.Any(candidate => string.Equals(
            normalized,
            candidate,
            StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSampleLabel(string value)
    {
        return IsExact(value, RequestResponseLabels)
            || IsExact(value, OutputLabels);
    }

    private static string GetSampleType(string value)
    {
        return IsExact(value, RequestResponseLabels)
            ? "sample request/response"
            : "sample output";
    }

    private static string NormalizeStandaloneLabel(string value)
    {
        string normalized = value.Trim();

        if (normalized.EndsWith(':'))
        {
            normalized = normalized[..^1].TrimEnd();
        }

        return NormalizeLabel(normalized);
    }

    private static string NormalizeLabel(string value)
    {
        return NormalizeWhitespace(value.Trim());
    }

    private static string RemoveTrailingAtxDecoration(string value)
    {
        string text = value.Trim();
        int index = text.Length - 1;

        while (index >= 0 && text[index] == '#')
        {
            index--;
        }

        if (index < text.Length - 1
            && index >= 0
            && char.IsWhiteSpace(text[index]))
        {
            text = text[..index].TrimEnd();
        }

        return text;
    }

    private static string CreateNodeLocation(
        string sourcePath,
        IReadOnlyList<Section> sections,
        int line)
    {
        Section? section = FindContainingSection(sections, line);
        return section is null
            ? sourcePath
            : CreateLocation(sourcePath, section.Heading.Text);
    }

    private static string CreateLocation(string sourcePath, string heading)
    {
        string normalized = NormalizeLabel(heading).ToLowerInvariant();
        StringBuilder slug = new();
        bool pendingHyphen = false;

        foreach (char character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                if (pendingHyphen && slug.Length > 0)
                {
                    slug.Append('-');
                }

                slug.Append(character);
                pendingHyphen = false;
            }
            else
            {
                pendingHyphen = slug.Length > 0;
            }
        }

        return slug.Length == 0 ? sourcePath : $"{sourcePath}#{slug}";
    }

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        IEnumerable<EvidenceItem> evidence,
        DateTimeOffset evaluatedAt)
    {
        return new SignalResult(
            Definition.Key,
            Definition.Version,
            Definition.Category,
            status,
            score,
            Definition.MaximumScore,
            Definition.IncludeInOverallScore,
            evidence,
            evaluatedAt);
    }

    private enum AmbiguityKind
    {
        Usage,
        Demo,
        Both
    }

    private sealed record MarkdownDocument(
        IReadOnlyList<string> Lines,
        IReadOnlyList<Heading> Headings,
        IReadOnlyList<FenceBlock> Fences,
        IReadOnlySet<int> FencedLines,
        IReadOnlyDictionary<string, string> References,
        int? UnclosedFenceStart);

    private sealed record Heading(
        int Line,
        int BodyStartLine,
        int Level,
        string Text);

    private sealed record Section(Heading Heading, int EndLine);

    private sealed record FenceState(char Marker, int Length, int StartLine);

    private sealed record FenceBlock(int StartLine, int EndLine);

    private sealed record MarkdownImage(int Line, string Alt, string Target);

    private sealed record MarkdownLink(int Line, string Label, string Target);

    private sealed record UsageEvidence(
        int Line,
        string Type,
        string Value,
        string Location);

    private sealed record DemoEvidence(
        int Line,
        string Type,
        string Value,
        string Location,
        string Reason)
    {
        public string Identity => $"{Line.ToString(CultureInfo.InvariantCulture)}|{Type}|{Value}";
    }

    private sealed record SampleContext(
        int LabelLine,
        int BodyStartLine,
        int EndLine,
        string Type,
        string Label);

    private sealed record Ambiguity(
        string SourcePath,
        string SourceKind,
        DateTimeOffset ObservedAt,
        int Line,
        AmbiguityKind Kind,
        string Reason);

    private sealed record SourceAnalysis(
        string SourcePath,
        string SourceKind,
        DateTimeOffset ObservedAt,
        IReadOnlyList<UsageEvidence> Usage,
        IReadOnlyList<DemoEvidence> HighConfidenceDemo,
        IReadOnlyList<DemoEvidence> WeakDemo,
        IReadOnlyList<MarkdownLink> DemoLinkCandidates,
        IReadOnlyList<Ambiguity> Ambiguities);
}
