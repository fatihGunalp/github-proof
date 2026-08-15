using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class InstallationGuideSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "documentation.installation-guide";

    private const string ReadmeDataKey = "readme";
    private const string DocumentationDataKey = "documentation-markdown";
    private const string MissingRecommendation =
        "README veya docs içerisinde açık bir Kurulum / Installation bölümü ekleyin; ön koşulları, bağımlılık kurulumunu, yapılandırmayı ve çalıştırma adımlarını belirtin.";
    private const string PartialRecommendation =
        "Mevcut kurulum bölümünü uygulanabilir komutlar veya numaralı adımlarla tamamlayın.";
    private const string HumanReviewRecommendation =
        "Belirsiz kurulum bölümünü açık canonical heading ve uygulanabilir adımlarla netleştirin.";
    private const string PresenceLimitation =
        "Kurulum talimatlarının bulunması, talimatların gerçekten çalıştığını, güncel olduğunu veya tüm ortamlarda geçerli olduğunu kanıtlamaz.";
    private const string IncompleteStrongLimitation =
        "Dokümantasyon corpus'u eksik olduğu için daha güçlü bir Strong sınıflandırması bütünüyle doğrulanamadı.";

    private static readonly string[] InstallationHeadings =
    [
        "kurulum",
        "installation",
        "install",
        "setup",
        "getting started",
        "quick start",
        "başlangıç",
        "çalıştırma",
        "running",
        "run locally"
    ];

    private static readonly string[] ImperativePhrases =
    [
        "geri yükleyin",
        "install",
        "run",
        "build",
        "configure",
        "create",
        "copy",
        "set",
        "add",
        "clone",
        "restore",
        "execute",
        "start",
        "kurun",
        "yükleyin",
        "çalıştırın",
        "derleyin",
        "yapılandırın",
        "oluşturun",
        "kopyalayın",
        "ayarlayın",
        "ekleyin",
        "klonlayın",
        "başlatın"
    ];

    private static readonly string[] RequirementTriggers =
    [
        "requires",
        "required",
        "requirement",
        "gerektirir",
        "gereklidir",
        "gerekli"
    ];

    private static readonly string[] RequirementNoiseTokens =
    [
        "this",
        "that",
        "it",
        "project",
        "repository",
        "app",
        "application",
        "is",
        "are",
        "be",
        "the",
        "a",
        "an",
        "and",
        "or",
        "with",
        "create",
        "directory",
        "bu",
        "şu",
        "proje",
        "repository",
        "uygulama",
        "için",
        "bir",
        "olarak",
        "ve",
        "veya",
        "ile",
        "oluşturun",
        "dizin",
        "dizini"
    ];

    private static readonly string[] ConfigurationTargets =
    [
        ".env",
        "environment variable",
        "environment variables",
        "appsettings",
        "config file",
        "configuration",
        "secret",
        "secrets",
        "connection string"
    ];

    private static readonly string[] DependencyCommands =
    [
        "dotnet restore",
        "npm install",
        "npm ci",
        "yarn install",
        "pnpm install",
        "pip install",
        "pip3 install",
        "poetry install",
        "bundle install",
        "composer install",
        "gradle dependencies",
        "./gradlew dependencies"
    ];

    private static readonly string[] BuildCommands =
    [
        "dotnet build",
        "npm run build",
        "yarn build",
        "pnpm build",
        "mvn package",
        "mvn compile",
        "gradle build",
        "./gradlew build",
        "cargo build",
        "go build",
        "make",
        "cmake --build"
    ];

    private static readonly string[] RunCommands =
    [
        "dotnet run",
        "npm start",
        "npm run dev",
        "yarn start",
        "yarn dev",
        "pnpm start",
        "pnpm dev",
        "flask run",
        "cargo run",
        "docker compose up",
        "docker-compose up"
    ];

    private static readonly string[] MigrationCommands =
    [
        "dotnet ef database update",
        "prisma db push",
        "sequelize db:migrate",
        "rails db:migrate",
        "rake db:migrate",
        "seed database",
        "create database",
        "prepare database",
        "initialize database",
        "generate certificates",
        "copy example configuration",
        "create required directory",
        "veritabanını oluşturun",
        "migration çalıştırın",
        "veritabanını hazırlayın",
        "başlangıç verisini yükleyin",
        "gerekli dizini oluşturun"
    ];

    private readonly TimeProvider _timeProvider;

    public InstallationGuideSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Kurulum Talimatı",
        AnalysisCategory.RepositoryUnderstandability,
        "README ve indekslenebilir dokümantasyon içindeki uygulanabilir kurulum talimatlarını değerlendirir.",
        maximumScore: 8,
        requiredDataKeys: [ReadmeDataKey, DocumentationDataKey],
        applicabilityRules: ["Bütün public repository'lerde uygulanır."],
        evaluationRules:
        [
            "Aynı kurulum bölümündeki en az üç distinct feature Strong sonucu üretir.",
            "Canonical kurulum bölümündeki actionable step Present sonucu üretir.",
            "Complete corpus içindeki yalnız başlık düzeyindeki kanıt Partial sonucu üretir.",
            "Complete corpus içinde canonical bölüm bulunmaması Missing sonucu üretir.",
            "Eksik veya erişilemeyen corpus güvenilir positive kanıt yoksa UnavailableEvidence sonucu üretir.",
            "Yalnız bounded Markdown yapısal belirsizliği RequiresHumanReview sonucu üretir."
        ],
        evidenceRequirements:
        [
            "Normalize edilmiş README içeriği ve erişilebilirliği.",
            "Ordinal sıralı docs/**/*.md içerikleri, collection completeness ve erişilebilirlik."
        ],
        explanationTemplate: "Kurulum başlıkları, actionable adımlar, distinct feature'lar ve corpus erişilebilirliği deterministik olarak değerlendirilir.",
        recommendationTemplate: MissingRecommendation,
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? readmeAvailability = snapshot.FindAvailability(ReadmeDataKey);
        DataAvailability? documentationAvailability = snapshot.FindAvailability(DocumentationDataKey);
        bool readmeComplete = readmeAvailability?.IsAvailable == true;
        bool documentationComplete = documentationAvailability?.IsAvailable == true
            && snapshot.DocumentationMarkdown.IsComplete
            && snapshot.DocumentationMarkdown.Items.All(file => file.Content is not null);
        bool corpusComplete = readmeComplete && documentationComplete;

        List<SourceAnalysis> sources = [];

        if (readmeComplete && snapshot.Readme is not null)
        {
            sources.Add(AnalyzeSource(
                snapshot.Readme.Path,
                "README",
                snapshot.Readme.Content,
                readmeAvailability?.RetrievedAt ?? snapshot.CapturedAt));
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
                    documentationAvailability?.RetrievedAt ?? snapshot.CapturedAt));
            }
        }

        SectionAnalysis? strong = SelectSection(sources, SourceStatus.Strong);

        if (strong is not null)
        {
            return CreatePositiveResult(
                SignalStatus.Strong,
                strong,
                corpusComplete,
                evaluatedAt);
        }

        SectionAnalysis? present = SelectSection(sources, SourceStatus.Present);

        if (present is not null)
        {
            return CreatePositiveResult(
                SignalStatus.Present,
                present,
                corpusComplete,
                evaluatedAt);
        }

        if (!corpusComplete)
        {
            return CreateUnavailableResult(
                snapshot,
                readmeAvailability,
                documentationAvailability,
                evaluatedAt);
        }

        Ambiguity? ambiguity = sources
            .Select(source => source.Ambiguity)
            .FirstOrDefault(item => item is not null);

        if (ambiguity is not null)
        {
            EvidenceItem evidence = new(
                SignalKey,
                ambiguity.SourceKind,
                "ambiguous",
                CreateLocation(ambiguity.SourcePath, ambiguity.Heading),
                ambiguity.ObservedAt,
                EvidenceConfidence.Medium,
                ambiguity.Reason);

            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                [evidence],
                $"'{ambiguity.SourcePath}' içindeki kurulum bölümü yapısal belirsizlik nedeniyle güvenilir biçimde sınıflandırılamadı.",
                HumanReviewRecommendation,
                limitations: null,
                evaluatedAt);
        }

        SectionAnalysis? partial = SelectSection(sources, SourceStatus.Partial);

        if (partial is not null)
        {
            EvidenceItem evidence = new(
                SignalKey,
                partial.SourceKind,
                "partial",
                CreateLocation(partial.SourcePath, partial.Heading.Text),
                partial.ObservedAt,
                EvidenceConfidence.High,
                $"'{partial.Heading.Text}' başlığı bulundu ancak bölümde uygulanabilir bir adım bulunamadı.");

            return CreateResult(
                SignalStatus.Partial,
                score: 4,
                [evidence],
                $"'{partial.SourcePath}' içinde kurulum bölümü bulundu ancak uygulanabilir kurulum adımı bulunamadı.",
                PartialRecommendation,
                limitations: null,
                evaluatedAt);
        }

        return CreateMissingResult(
            snapshot,
            readmeAvailability!,
            documentationAvailability!,
            evaluatedAt);
    }

    private SignalResult CreatePositiveResult(
        SignalStatus status,
        SectionAnalysis section,
        bool corpusComplete,
        DateTimeOffset evaluatedAt)
    {
        string featureText = string.Join(
            ", ",
            section.Features.Select(feature => feature.Name));
        string representativeText = string.Join(
            "; ",
            section.Features.Select(feature => feature.Evidence).Take(3));
        string value = status == SignalStatus.Strong
            ? $"Strong: {featureText}"
            : $"Present: {section.ActionableEvidence}";
        string notes = status == SignalStatus.Strong
            ? $"'{section.Heading.Text}' bölümünde distinct feature'lar: {featureText}. Representative evidence: {representativeText}."
            : $"'{section.Heading.Text}' bölümünde actionable evidence: {section.ActionableEvidence}.";

        EvidenceItem evidence = new(
            SignalKey,
            section.SourceKind,
            value,
            CreateLocation(section.SourcePath, section.Heading.Text),
            section.ObservedAt,
            EvidenceConfidence.High,
            notes);

        List<string> limitations = [PresenceLimitation];

        if (status == SignalStatus.Present && !corpusComplete)
        {
            limitations.Add(IncompleteStrongLimitation);
        }

        string explanation = status == SignalStatus.Strong
            ? $"'{section.SourcePath}' içindeki kurulum bölümü en az üç distinct kurulum feature'ı taşıyor."
            : $"'{section.SourcePath}' içindeki kurulum bölümünde uygulanabilir bir adım bulundu.";

        return CreateResult(
            status,
            Definition.MaximumScore,
            [evidence],
            explanation,
            recommendation: null,
            limitations,
            evaluatedAt);
    }

    private SignalResult CreateUnavailableResult(
        RepositorySnapshot snapshot,
        DataAvailability? readmeAvailability,
        DataAvailability? documentationAvailability,
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
                    ? snapshot.DocumentationMarkdown.IsComplete ? "available-complete" : "available-incomplete"
                    : "unavailable",
                snapshot.CapturedAt)
        ];

        return CreateResult(
            SignalStatus.UnavailableEvidence,
            score: null,
            evidence,
            "Güvenilir bir Strong veya Present kanıtı bulunamadı ve gerekli README/docs corpus'u exact sonucu belirleyecek kadar complete değildi.",
            recommendation: null,
            limitations: null,
            evaluatedAt);
    }

    private SignalResult CreateMissingResult(
        RepositorySnapshot snapshot,
        DataAvailability readmeAvailability,
        DataAvailability documentationAvailability,
        DateTimeOffset evaluatedAt)
    {
        List<EvidenceItem> evidence =
        [
            new EvidenceItem(
                SignalKey,
                "READMEInspection",
                snapshot.Readme is null ? "available-absent" : "available-inspected",
                snapshot.Readme?.Path,
                readmeAvailability.RetrievedAt ?? snapshot.CapturedAt,
                EvidenceConfidence.High,
                "README verisi başarıyla incelendi; matched installation section count = 0."),
            new EvidenceItem(
                SignalKey,
                "DocumentationMarkdownInspection",
                "available-complete",
                "docs/**/*.md",
                documentationAvailability.RetrievedAt ?? snapshot.CapturedAt,
                EvidenceConfidence.High,
                $"Complete docs corpus içindeki {snapshot.DocumentationMarkdown.Items.Count.ToString(CultureInfo.InvariantCulture)} dosya incelendi; matched installation section count = 0.")
        ];

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "README ve complete docs Markdown corpus'u başarıyla incelendi ancak canonical kurulum bölümü bulunamadı.",
            MissingRecommendation,
            limitations: null,
            evaluatedAt);
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
        DateTimeOffset observedAt)
    {
        MarkdownDocument document = ParseMarkdown(content);
        List<SectionAnalysis> sections = [];

        foreach (Heading heading in document.Headings.Where(item => item.IsExactInstallation))
        {
            int endLine = FindSectionEnd(document.Headings, heading, document.Lines.Count);
            string? actionable = FindActionableEvidence(document, heading.BodyStartLine, endLine);
            IReadOnlyList<FeatureEvidence> features = DetectFeatures(
                document,
                heading,
                endLine);
            SourceStatus status = features.Count >= 3
                ? SourceStatus.Strong
                : actionable is not null ? SourceStatus.Present : SourceStatus.Partial;

            sections.Add(new SectionAnalysis(
                sourcePath,
                sourceKind,
                observedAt,
                heading,
                endLine,
                status,
                actionable,
                features));
        }

        Ambiguity? ambiguity = DetectAmbiguity(
            document,
            sourcePath,
            sourceKind,
            observedAt,
            sections);

        return new SourceAnalysis(sourcePath, sections, ambiguity);
    }

    private static SectionAnalysis? SelectSection(
        IEnumerable<SourceAnalysis> sources,
        SourceStatus status)
    {
        return sources
            .SelectMany(source => source.Sections)
            .FirstOrDefault(section => section.Status == status);
    }

    private static MarkdownDocument ParseMarkdown(string content)
    {
        string normalized = content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        List<Heading> headings = [];
        HashSet<int> headingLines = [];
        FenceState? fence = null;
        int? unclosedFenceStart = null;

        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];

            if (fence is not null)
            {
                if (IsFenceClosing(line, fence))
                {
                    fence = null;
                }

                continue;
            }

            if (TryGetFenceOpening(line, out FenceState? opening))
            {
                fence = opening! with { StartLine = index };
                unclosedFenceStart = index;
                continue;
            }

            if (TryParseAtxHeading(line, index, out Heading? atx))
            {
                headings.Add(atx!);
                headingLines.Add(index);
                continue;
            }

            if (index + 1 < lines.Length
                && TryParseSetextHeading(line, lines[index + 1], index, out Heading? setext))
            {
                headings.Add(setext!);
                headingLines.Add(index);
                headingLines.Add(index + 1);
                index++;
            }
        }

        if (fence is null)
        {
            unclosedFenceStart = null;
        }

        return new MarkdownDocument(
            lines,
            headings,
            headingLines,
            unclosedFenceStart);
    }

    private static bool TryParseAtxHeading(
        string line,
        int lineIndex,
        out Heading? heading)
    {
        heading = null;
        string trimmed = line.TrimStart();
        int level = CountPrefix(trimmed, '#');

        if (level is < 1 or > 6
            || level >= trimmed.Length
            || !char.IsWhiteSpace(trimmed[level]))
        {
            return false;
        }

        string text = RemoveTrailingAtxDecoration(trimmed[level..].Trim());

        if (text.Length == 0)
        {
            return false;
        }

        heading = CreateHeading(level, text, lineIndex, lineIndex + 1);
        return true;
    }

    private static bool TryParseSetextHeading(
        string textLine,
        string underlineLine,
        int lineIndex,
        out Heading? heading)
    {
        heading = null;
        string text = textLine.Trim();
        string underline = underlineLine.Trim();

        if (text.Length == 0
            || underline.Length == 0
            || !underline.All(character => character == underline[0])
            || underline[0] is not ('=' or '-'))
        {
            return false;
        }

        heading = CreateHeading(
            underline[0] == '=' ? 1 : 2,
            text,
            lineIndex,
            lineIndex + 2);
        return true;
    }

    private static Heading CreateHeading(
        int level,
        string text,
        int lineIndex,
        int bodyStartLine)
    {
        bool isExact = IsInstallationHeading(text);
        bool isNear = TryGetNearInstallationHeading(text, out _);

        return new Heading(
            level,
            text,
            lineIndex,
            bodyStartLine,
            isExact,
            isNear);
    }

    private static int FindSectionEnd(
        IReadOnlyList<Heading> headings,
        Heading heading,
        int lineCount)
    {
        Heading? boundary = headings.FirstOrDefault(candidate =>
            candidate.LineIndex > heading.LineIndex
            && candidate.Level <= heading.Level);

        return boundary?.LineIndex ?? lineCount;
    }

    private static string? FindActionableEvidence(
        MarkdownDocument document,
        int startLine,
        int endLine)
    {
        FenceState? fence = null;
        List<string> fenceContent = [];
        for (int index = startLine; index < endLine; index++)
        {
            string line = document.Lines[index];

            if (fence is not null)
            {
                if (IsFenceClosing(line, fence))
                {
                    string? representative = fenceContent
                        .Select(NormalizeEvidenceValue)
                        .FirstOrDefault(value => value.Length > 0);

                    if (representative is not null)
                    {
                        return $"non-empty fenced code block: {representative}";
                    }

                    fence = null;
                    fenceContent.Clear();
                }
                else
                {
                    fenceContent.Add(line);
                }

                continue;
            }

            if (TryGetFenceOpening(line, out FenceState? opening))
            {
                fence = opening! with { StartLine = index };
                continue;
            }

            if (document.HeadingLines.Contains(index))
            {
                continue;
            }

            if (TryGetOrderedItemContent(line, out string? orderedContent))
            {
                if (!IsStandaloneNonActionableContent(orderedContent!))
                {
                    return $"ordered step: {NormalizeEvidenceValue(orderedContent!)}";
                }
            }

            if (TryGetImperative(line, out _, out _))
            {
                return $"imperative instruction: {NormalizeEvidenceValue(NormalizeInstruction(line))}";
            }
        }

        return null;
    }

    private static IReadOnlyList<FeatureEvidence> DetectFeatures(
        MarkdownDocument document,
        Heading installationHeading,
        int sectionEnd)
    {
        IReadOnlyList<EvidenceLine> evidenceLines = CollectEvidenceLines(
            document,
            installationHeading.BodyStartLine,
            sectionEnd);
        HashSet<string> consumedIdentities = new(StringComparer.Ordinal);
        List<FeatureEvidence> features = [];

        AssignFirstFeature(
            "prerequisites",
            FindPrerequisiteCandidates(document, installationHeading, sectionEnd, evidenceLines),
            consumedIdentities,
            features);
        AssignFirstFeature(
            "dependency installation",
            evidenceLines
                .Where(line => IsDependencyInstallation(line.Normalized))
                .Select(ToCandidate),
            consumedIdentities,
            features);
        AssignFirstFeature(
            "configuration",
            FindConfigurationCandidates(document, installationHeading, sectionEnd, evidenceLines),
            consumedIdentities,
            features);
        AssignFirstFeature(
            "build",
            evidenceLines
                .Where(line => IsBuild(line.Normalized))
                .Select(ToCandidate),
            consumedIdentities,
            features);
        AssignFirstFeature(
            "run",
            evidenceLines
                .Where(line => IsRun(line.Normalized))
                .Select(ToCandidate),
            consumedIdentities,
            features);
        AssignFirstFeature(
            "migration/preparation",
            evidenceLines
                .Where(line => IsMigrationPreparation(line.Normalized))
                .Select(ToCandidate),
            consumedIdentities,
            features);

        return features;
    }

    private static IEnumerable<FeatureCandidate> FindPrerequisiteCandidates(
        MarkdownDocument document,
        Heading installationHeading,
        int sectionEnd,
        IReadOnlyList<EvidenceLine> evidenceLines)
    {
        string[] prerequisiteHeadings =
        [
            "prerequisites",
            "requirements",
            "gereksinimler",
            "ön koşullar",
            "önkoşullar"
        ];

        foreach (Heading heading in document.Headings.Where(candidate =>
                     candidate.LineIndex >= installationHeading.BodyStartLine
                     && candidate.LineIndex < sectionEnd
                     && candidate.Level > installationHeading.Level
                     && prerequisiteHeadings.Contains(candidate.Text, StringComparer.OrdinalIgnoreCase)))
        {
            int subsectionEnd = Math.Min(
                FindSectionEnd(document.Headings, heading, document.Lines.Count),
                sectionEnd);

            if (HasNonEmptyBody(document, heading.BodyStartLine, subsectionEnd))
            {
                yield return new FeatureCandidate(
                    $"heading:{heading.LineIndex.ToString(CultureInfo.InvariantCulture)}",
                    heading.LineIndex,
                    $"{heading.Text} subsection");
            }
        }

        foreach (EvidenceLine line in evidenceLines.Where(line => IsExplicitRequirement(line.Normalized)))
        {
            yield return ToCandidate(line);
        }
    }

    private static IEnumerable<FeatureCandidate> FindConfigurationCandidates(
        MarkdownDocument document,
        Heading installationHeading,
        int sectionEnd,
        IReadOnlyList<EvidenceLine> evidenceLines)
    {
        string[] configurationHeadings =
        [
            "configuration",
            "config",
            "configuration setup",
            "yapılandırma",
            "ayarlar"
        ];

        foreach (Heading heading in document.Headings.Where(candidate =>
                     candidate.LineIndex >= installationHeading.BodyStartLine
                     && candidate.LineIndex < sectionEnd
                     && candidate.Level > installationHeading.Level
                     && configurationHeadings.Contains(candidate.Text, StringComparer.OrdinalIgnoreCase)))
        {
            int subsectionEnd = Math.Min(
                FindSectionEnd(document.Headings, heading, document.Lines.Count),
                sectionEnd);

            if (FindActionableEvidence(document, heading.BodyStartLine, subsectionEnd) is not null)
            {
                yield return new FeatureCandidate(
                    $"heading:{heading.LineIndex.ToString(CultureInfo.InvariantCulture)}",
                    heading.LineIndex,
                    $"{heading.Text} actionable subsection");
            }
        }

        foreach (EvidenceLine line in evidenceLines.Where(line =>
                     TryGetImperative(line.Normalized, out _, out _)
                     && ContainsPhrase(line.Normalized, ConfigurationTargets)))
        {
            yield return ToCandidate(line);
        }
    }

    private static IReadOnlyList<EvidenceLine> CollectEvidenceLines(
        MarkdownDocument document,
        int startLine,
        int endLine)
    {
        List<EvidenceLine> result = [];
        FenceState? fence = null;
        List<EvidenceLine> fenceLines = [];

        for (int index = startLine; index < endLine; index++)
        {
            string raw = document.Lines[index];

            if (fence is not null)
            {
                if (IsFenceClosing(raw, fence))
                {
                    result.AddRange(fenceLines);
                    fenceLines.Clear();
                    fence = null;
                }
                else if (!string.IsNullOrWhiteSpace(raw))
                {
                    fenceLines.Add(CreateEvidenceLine(index, raw));
                }

                continue;
            }

            if (TryGetFenceOpening(raw, out FenceState? opening))
            {
                fence = opening! with { StartLine = index };
                continue;
            }

            if (!document.HeadingLines.Contains(index)
                && !string.IsNullOrWhiteSpace(raw))
            {
                result.Add(CreateEvidenceLine(index, raw));
            }
        }

        return result;
    }

    private static EvidenceLine CreateEvidenceLine(int lineIndex, string raw)
    {
        return new EvidenceLine(
            lineIndex,
            NormalizeOperation(raw),
            NormalizeEvidenceValue(raw));
    }

    private static void AssignFirstFeature(
        string featureName,
        IEnumerable<FeatureCandidate> candidates,
        ISet<string> consumedIdentities,
        ICollection<FeatureEvidence> features)
    {
        FeatureCandidate? candidate = candidates
            .OrderBy(item => item.LineIndex)
            .FirstOrDefault(item => !consumedIdentities.Contains(item.Identity));

        if (candidate is null)
        {
            return;
        }

        consumedIdentities.Add(candidate.Identity);
        features.Add(new FeatureEvidence(featureName, candidate.Evidence));
    }

    private static FeatureCandidate ToCandidate(EvidenceLine line)
    {
        return new FeatureCandidate(
            $"line:{line.LineIndex.ToString(CultureInfo.InvariantCulture)}:{line.Normalized}",
            line.LineIndex,
            line.Display);
    }

    private static bool IsDependencyInstallation(string line)
    {
        if (StartsWithAnyCommand(line, DependencyCommands)
            || StartsWithParameterizedCommand(line, "mvn dependency:"))
        {
            return true;
        }

        return TryGetImperative(line, out _, out string? remainder)
            && StartsWithAnyPhrase(line, ["install", "yükleyin", "kurun"])
            && ContainsAlphanumeric(remainder);
    }

    private static bool IsBuild(string line)
    {
        return StartsWithAnyCommand(line, BuildCommands)
            || (TryGetImperative(line, out string? phrase, out string? remainder)
                && phrase is "build" or "derleyin"
                && ContainsAlphanumeric(remainder));
    }

    private static bool IsRun(string line)
    {
        if (StartsWithAnyCommand(line, RunCommands)
            || StartsWithParameterizedCommand(line, "python ")
            || StartsWithParameterizedCommand(line, "python3 ")
            || StartsWithParameterizedCommand(line, "uvicorn ")
            || StartsWithParameterizedCommand(line, "go run "))
        {
            return true;
        }

        return TryGetImperative(line, out string? phrase, out string? remainder)
            && phrase is "run" or "start" or "çalıştırın" or "başlatın"
            && ContainsAlphanumeric(remainder);
    }

    private static bool IsMigrationPreparation(string line)
    {
        return StartsWithAnyCommand(line, MigrationCommands)
            || StartsWithParameterizedCommand(line, "dotnet ef migrations ")
            || StartsWithParameterizedCommand(line, "alembic upgrade ")
            || StartsWithParameterizedCommand(line, "prisma migrate ");
    }

    private static bool IsExplicitRequirement(string line)
    {
        MatchCollection tokens = Regex.Matches(
            line,
            @"[\p{L}\p{Nd}][\p{L}\p{Nd}._+\-]*",
            RegexOptions.CultureInvariant);
        bool hasTrigger = tokens.Any(match => RequirementTriggers.Contains(
            match.Value,
            StringComparer.OrdinalIgnoreCase));

        if (!hasTrigger)
        {
            return false;
        }

        return tokens.Any(match =>
            !RequirementTriggers.Contains(match.Value, StringComparer.OrdinalIgnoreCase)
            && !RequirementNoiseTokens.Contains(match.Value, StringComparer.OrdinalIgnoreCase)
            && match.Value.Any(char.IsLetterOrDigit));
    }

    private static bool HasNonEmptyBody(
        MarkdownDocument document,
        int startLine,
        int endLine)
    {
        return Enumerable.Range(startLine, Math.Max(0, endLine - startLine))
            .Any(index => !document.HeadingLines.Contains(index)
                && !string.IsNullOrWhiteSpace(document.Lines[index])
                && !TryGetFenceOpening(document.Lines[index], out _));
    }

    private static Ambiguity? DetectAmbiguity(
        MarkdownDocument document,
        string sourcePath,
        string sourceKind,
        DateTimeOffset observedAt,
        IReadOnlyList<SectionAnalysis> exactSections)
    {
        foreach (Heading heading in document.Headings.Where(item => item.IsNearInstallation))
        {
            int endLine = FindSectionEnd(document.Headings, heading, document.Lines.Count);

            if (FindActionableEvidence(document, heading.BodyStartLine, endLine) is not null)
            {
                return new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    heading.Text,
                    $"'{heading.Text}' başlığı canonical adayı yalnız bounded parenthetical suffix veya trailing punctuation ile genişletiyor ve actionable body taşıyor.");
            }
        }

        for (int index = 0; index < document.Lines.Count; index++)
        {
            if (!TryParseMalformedAtxHeading(document.Lines[index], index, out Heading? malformed))
            {
                continue;
            }

            Heading parsedMalformed = malformed!;
            int endLine = FindSectionEnd(document.Headings, parsedMalformed, document.Lines.Count);

            if (FindActionableEvidence(document, parsedMalformed.BodyStartLine, endLine) is not null)
            {
                return new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    parsedMalformed.Text,
                    $"'{parsedMalformed.Text}' metni whitespace eksikliği nedeniyle malformed ATX heading ve actionable body arasında belirsizlik oluşturuyor.");
            }
        }

        if (document.UnclosedFenceStart is int fenceStart)
        {
            SectionAnalysis? affectedSection = exactSections.FirstOrDefault(section =>
                fenceStart >= section.Heading.BodyStartLine
                && fenceStart < section.EndLine
                && document.Lines
                    .Skip(fenceStart + 1)
                    .Take(section.EndLine - fenceStart - 1)
                    .Any(line => !string.IsNullOrWhiteSpace(line)));

            if (affectedSection is not null)
            {
                return new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    affectedSection.Heading.Text,
                    "Matched kurulum bölümü içindeki kapanmamış fenced block actionable kanıt ve section boundary sınıflandırmasını belirsiz bırakıyor.");
            }

            Heading? hiddenHeading = FindRawInstallationHeadingAfter(
                document.Lines,
                fenceStart + 1);

            if (hiddenHeading is not null
                && HasPlainActionableLine(
                    document.Lines,
                    hiddenHeading.BodyStartLine,
                    document.Lines.Count))
            {
                return new Ambiguity(
                    sourcePath,
                    sourceKind,
                    observedAt,
                    hiddenHeading.Text,
                    "Kapanmamış fenced block canonical heading ve actionable satırların section içinde mi dışında mı olduğunu belirsiz bırakıyor.");
            }
        }

        return null;
    }

    private static bool TryParseMalformedAtxHeading(
        string line,
        int lineIndex,
        out Heading? heading)
    {
        heading = null;
        string trimmed = line.TrimStart();
        int level = CountPrefix(trimmed, '#');

        if (level is < 1 or > 6
            || level >= trimmed.Length
            || char.IsWhiteSpace(trimmed[level]))
        {
            return false;
        }

        string text = RemoveTrailingAtxDecoration(trimmed[level..].Trim());

        if (!IsInstallationHeading(text)
            && !TryGetNearInstallationHeading(text, out _))
        {
            return false;
        }

        heading = CreateHeading(level, text, lineIndex, lineIndex + 1);
        return true;
    }

    private static Heading? FindRawInstallationHeadingAfter(
        IReadOnlyList<string> lines,
        int startLine)
    {
        for (int index = startLine; index < lines.Count; index++)
        {
            if (TryParseAtxHeading(lines[index], index, out Heading? atx)
                && (atx!.IsExactInstallation || atx.IsNearInstallation))
            {
                return atx;
            }

            if (index + 1 < lines.Count
                && TryParseSetextHeading(lines[index], lines[index + 1], index, out Heading? setext)
                && (setext!.IsExactInstallation || setext.IsNearInstallation))
            {
                return setext;
            }
        }

        return null;
    }

    private static bool HasPlainActionableLine(
        IReadOnlyList<string> lines,
        int startLine,
        int endLine)
    {
        for (int index = startLine; index < endLine; index++)
        {
            if (TryGetOrderedItemContent(lines[index], out _)
                || TryGetImperative(lines[index], out _, out _))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetFenceOpening(
        string line,
        out FenceState? fence)
    {
        fence = null;
        string trimmed = line.TrimStart();

        if (trimmed.Length < 3 || trimmed[0] is not ('`' or '~'))
        {
            return false;
        }

        char marker = trimmed[0];
        int length = CountPrefix(trimmed, marker);

        if (length < 3)
        {
            return false;
        }

        fence = new FenceState(marker, length, StartLine: -1);
        return true;
    }

    private static bool IsFenceClosing(string line, FenceState fence)
    {
        string trimmed = line.Trim();
        int length = CountPrefix(trimmed, fence.Marker);

        return length >= fence.Length
            && trimmed[length..].Length == 0;
    }

    private static bool TryGetOrderedItemContent(
        string line,
        out string? content)
    {
        content = null;
        string trimmed = line.TrimStart();
        int digitCount = 0;

        while (digitCount < trimmed.Length && char.IsAsciiDigit(trimmed[digitCount]))
        {
            digitCount++;
        }

        if (digitCount == 0
            || digitCount >= trimmed.Length
            || trimmed[digitCount] is not ('.' or ')')
            || digitCount + 1 >= trimmed.Length
            || !char.IsWhiteSpace(trimmed[digitCount + 1]))
        {
            return false;
        }

        string remainder = trimmed[(digitCount + 2)..].Trim();

        if (remainder.Length == 0)
        {
            return false;
        }

        content = remainder;
        return true;
    }

    private static bool TryGetImperative(
        string line,
        out string? phrase,
        out string? remainder)
    {
        phrase = null;
        remainder = null;
        string normalized = NormalizeInstruction(line);

        if (normalized.Length == 0
            || normalized[0] == '`'
            || normalized.StartsWith("![", StringComparison.Ordinal)
            || normalized.StartsWith("[![", StringComparison.Ordinal)
            || normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (string candidate in ImperativePhrases)
        {
            if (!StartsWithPhrase(normalized, candidate))
            {
                continue;
            }

            phrase = candidate;
            remainder = normalized[candidate.Length..]
                .TrimStart(' ', '\t', ':', ';', ',', '.', '!', '?', '-', '–', '—');
            return true;
        }

        return false;
    }

    private static bool IsStandaloneNonActionableContent(string content)
    {
        string value = content.Trim();

        if (TryGetImperative(value, out _, out _))
        {
            return false;
        }

        if (value.StartsWith("![", StringComparison.Ordinal)
            || value.StartsWith("[![", StringComparison.Ordinal)
            || value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Regex.IsMatch(
            value,
            @"^`[^`]+`$|^[vV]?\d+(?:\.\d+){1,3}$",
            RegexOptions.CultureInvariant);
    }

    private static string NormalizeInstruction(string line)
    {
        string value = line.Trim();
        bool changed;

        do
        {
            changed = false;

            if (value.Length >= 2
                && value[0] is '>' or '-' or '*' or '+'
                && char.IsWhiteSpace(value[1]))
            {
                value = value[2..].TrimStart();
                changed = true;
            }

            if (TryGetOrderedItemContent(value, out string? orderedContent))
            {
                value = orderedContent!;
                changed = true;
            }

            if (value.Length >= 3
                && value[0] == '['
                && value[2] == ']'
                && value[1] is ' ' or 'x' or 'X')
            {
                value = value[3..].TrimStart();
                changed = true;
            }
        }
        while (changed);

        return value;
    }

    private static string NormalizeOperation(string line)
    {
        string value = NormalizeInstruction(line);

        if (value.StartsWith("$ ", StringComparison.Ordinal)
            || value.StartsWith("> ", StringComparison.Ordinal))
        {
            value = value[2..].TrimStart();
        }

        return NormalizeWhitespace(value).ToLowerInvariant();
    }

    private static string NormalizeEvidenceValue(string value)
    {
        string normalized = NormalizeWhitespace(value);
        return normalized.Length <= 120
            ? normalized
            : string.Concat(normalized.AsSpan(0, 117), "...");
    }

    private static string NormalizeWhitespace(string value)
    {
        StringBuilder builder = new(value.Length);
        bool previousWasWhitespace = false;

        foreach (char character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
            }
            else
            {
                builder.Append(character);
                previousWasWhitespace = false;
            }
        }

        return builder.ToString();
    }

    private static bool IsInstallationHeading(string text)
    {
        return InstallationHeadings.Contains(
            text.Trim(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryGetNearInstallationHeading(
        string text,
        out string? canonicalHeading)
    {
        canonicalHeading = null;
        string trimmed = text.Trim();

        foreach (string candidate in InstallationHeadings)
        {
            if (!trimmed.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)
                || trimmed.Length == candidate.Length)
            {
                continue;
            }

            string suffix = trimmed[candidate.Length..].Trim();
            bool parenthetical = suffix.Length >= 3
                && suffix[0] == '('
                && suffix[^1] == ')'
                && suffix[1..^1].Length > 0
                && !suffix[1..^1].Contains('(')
                && !suffix[1..^1].Contains(')');
            bool punctuation = suffix.Length > 0 && suffix.All(char.IsPunctuation);

            if (parenthetical || punctuation)
            {
                canonicalHeading = candidate;
                return true;
            }
        }

        return false;
    }

    private static string RemoveTrailingAtxDecoration(string text)
    {
        string trimmed = text.TrimEnd();
        int hashStart = trimmed.Length;

        while (hashStart > 0 && trimmed[hashStart - 1] == '#')
        {
            hashStart--;
        }

        return hashStart < trimmed.Length
            && hashStart > 0
            && char.IsWhiteSpace(trimmed[hashStart - 1])
                ? trimmed[..hashStart].TrimEnd()
                : trimmed;
    }

    private static string CreateLocation(string sourcePath, string heading)
    {
        string anchor = NormalizeWhitespace(heading)
            .ToLowerInvariant()
            .Replace(' ', '-');
        return $"{sourcePath}#{anchor}";
    }

    private static int CountPrefix(string value, char character)
    {
        int count = 0;

        while (count < value.Length && value[count] == character)
        {
            count++;
        }

        return count;
    }

    private static bool StartsWithAnyCommand(
        string line,
        IEnumerable<string> commands)
    {
        return commands.Any(command => StartsWithCommand(line, command));
    }

    private static bool StartsWithCommand(string line, string command)
    {
        return line.Equals(command, StringComparison.OrdinalIgnoreCase)
            || (line.StartsWith(command, StringComparison.OrdinalIgnoreCase)
                && line.Length > command.Length
                && char.IsWhiteSpace(line[command.Length]));
    }

    private static bool StartsWithParameterizedCommand(
        string line,
        string commandPrefix)
    {
        return line.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase)
            && line.Length > commandPrefix.Length
            && line[commandPrefix.Length..].Any(char.IsLetterOrDigit);
    }

    private static bool StartsWithAnyPhrase(
        string line,
        IEnumerable<string> phrases)
    {
        return phrases.Any(phrase => StartsWithPhrase(line, phrase));
    }

    private static bool StartsWithPhrase(string line, string phrase)
    {
        if (!line.StartsWith(phrase, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (line.Length == phrase.Length)
        {
            return true;
        }

        char boundary = line[phrase.Length];
        return char.IsWhiteSpace(boundary)
            || boundary is ':' or ';' or ',' or '.' or '!' or '?' or '-' or '–' or '—';
    }

    private static bool ContainsPhrase(
        string line,
        IEnumerable<string> phrases)
    {
        return phrases.Any(phrase => Regex.IsMatch(
            line,
            $@"(?<![\p{{L}}\p{{Nd}}_]){Regex.Escape(phrase)}(?![\p{{L}}\p{{Nd}}_])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
    }

    private static bool ContainsAlphanumeric(string? value)
    {
        return value?.Any(char.IsLetterOrDigit) == true;
    }

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        IEnumerable<EvidenceItem> evidence,
        string explanation,
        string? recommendation,
        IEnumerable<string>? limitations,
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
            explanation,
            recommendation,
            limitations,
            evaluatedAt);
    }

    private enum SourceStatus
    {
        Partial,
        Present,
        Strong
    }

    private sealed record MarkdownDocument(
        IReadOnlyList<string> Lines,
        IReadOnlyList<Heading> Headings,
        IReadOnlySet<int> HeadingLines,
        int? UnclosedFenceStart);

    private sealed record Heading(
        int Level,
        string Text,
        int LineIndex,
        int BodyStartLine,
        bool IsExactInstallation,
        bool IsNearInstallation);

    private sealed record FenceState(char Marker, int Length, int StartLine);

    private sealed record EvidenceLine(
        int LineIndex,
        string Normalized,
        string Display);

    private sealed record FeatureCandidate(
        string Identity,
        int LineIndex,
        string Evidence);

    private sealed record FeatureEvidence(string Name, string Evidence);

    private sealed record SectionAnalysis(
        string SourcePath,
        string SourceKind,
        DateTimeOffset ObservedAt,
        Heading Heading,
        int EndLine,
        SourceStatus Status,
        string? ActionableEvidence,
        IReadOnlyList<FeatureEvidence> Features);

    private sealed record Ambiguity(
        string SourcePath,
        string SourceKind,
        DateTimeOffset ObservedAt,
        string Heading,
        string Reason);

    private sealed record SourceAnalysis(
        string SourcePath,
        IReadOnlyList<SectionAnalysis> Sections,
        Ambiguity? Ambiguity);
}
