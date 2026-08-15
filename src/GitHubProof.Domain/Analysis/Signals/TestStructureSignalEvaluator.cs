using System.Globalization;
using System.Xml.Linq;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class TestStructureSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "validation.test-structure";

    private const string FileTreeDataKey = "file-tree";
    private const string ProjectFilesDataKey = "project-files";
    private const string TestDirectoryEvidence = "TestDirectory";
    private const string TestProjectEvidence = "TestProject";
    private const string FrameworkEvidence = "TestFramework";
    private const string SpecFileEvidence = "SpecFile";
    private const string TestFileEvidence = "TestFile";
    private const string Recommendation =
        "Kritik davranışları doğrulayan ve proje yapısından açıkça ayrılan otomatik testler ekleyin.";
    private const string Limitation =
        "Test dosyalarının bulunması testlerin yeterli, doğru, güncel veya başarılı olduğunu kanıtlamaz.";

    private static readonly string[] TestDirectoryNames = ["tests", "test", "__tests__"];
    private readonly TimeProvider _timeProvider;

    public TestStructureSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        "Test Yapısı",
        AnalysisCategory.ValidationAndUsability,
        "Repository file tree ve project/dependency içeriklerindeki görünür otomatik test yapısını değerlendirir.",
        maximumScore: 10,
        requiredDataKeys: [FileTreeDataKey, ProjectFilesDataKey],
        applicabilityRules: ["Bütün public repository'lerde uygulanır."],
        evaluationRules:
        [
            "Birden fazla bağımsız test kanıt türü veya organized test dizini Strong sonucu üretir.",
            "Açık test projesi veya framework kanıtı Present sonucu üretir.",
            "Yalnız güvenilir spec/test dosya deseni Partial sonucu üretir."
        ],
        evidenceRequirements:
        [
            "Recognized test project, config, dependency, file ve directory evidence türleri ile path'leri.",
            "File tree bütünlüğü ve file-tree/project-files erişilebilirliği."
        ],
        explanationTemplate: "Kilitli Version 1 test göstergeleri ve evidence bütünlüğü deterministik olarak değerlendirilir.",
        recommendationTemplate: Recommendation,
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? fileTreeAvailability = snapshot.FindAvailability(FileTreeDataKey);
        DataAvailability? projectFilesAvailability = snapshot.FindAvailability(ProjectFilesDataKey);
        IReadOnlyList<TestEvidence> recognized = FindEvidence(snapshot.FileTree.Items);
        EvidenceCollection evidence = CollectEvidence(
            snapshot,
            fileTreeAvailability,
            projectFilesAvailability,
            recognized);

        bool evidenceComplete = fileTreeAvailability?.IsAvailable == true
            && projectFilesAvailability?.IsAvailable == true
            && snapshot.FileTree.IsComplete;

        if (!evidenceComplete)
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                "Test yapısını değerlendirmek için file tree ve project/dependency evidence erişilebilir ve tam değildir.",
                recommendation: null,
                limitations: null,
                evaluatedAt);
        }

        if (IsStrong(recognized))
        {
            return CreateResult(
                SignalStatus.Strong,
                score: 10,
                evidence,
                "Birden fazla bağımsız test kanıt türü veya organized test dizini doğrulandı.",
                recommendation: null,
                [Limitation],
                evaluatedAt);
        }

        if (recognized.Any(item => item.Type is TestProjectEvidence or FrameworkEvidence))
        {
            return CreateResult(
                SignalStatus.Present,
                score: 8,
                evidence,
                "Açık test projesi veya test framework kanıtı doğrulandı.",
                recommendation: null,
                [Limitation],
                evaluatedAt);
        }

        if (recognized.Any(item => item.Type is SpecFileEvidence or TestFileEvidence))
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 4,
                evidence,
                "Sınırlı ancak güvenilir bir spec/test dosya deseni doğrulandı.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        if (recognized.Any(item => item.Type == TestDirectoryEvidence))
        {
            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                evidence,
                "Exact test dizini adı bulundu ancak otomatik test yapısı ek kanıtla doğrulanamadı.",
                Recommendation,
                [Limitation],
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Missing,
            score: 0,
            evidence,
            "File tree ve project/dependency evidence tam olarak incelendi ancak tanımlı test yapısı kanıtı bulunamadı.",
            Recommendation,
            [Limitation],
            evaluatedAt);
    }

    private static IReadOnlyList<TestEvidence> FindEvidence(
        IReadOnlyList<RepositoryFile> files)
    {
        List<TestEvidence> evidence = [];
        HashSet<string> directoryPaths = new(StringComparer.OrdinalIgnoreCase);

        foreach (RepositoryFile file in files.OrderBy(item => item.Path, StringComparer.Ordinal))
        {
            AddTestDirectories(file, directoryPaths, evidence);

            if (file.IsDirectory)
            {
                continue;
            }

            string fileName = GetFileName(file.Path);

            if (fileName.EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".Test.csproj", StringComparison.OrdinalIgnoreCase))
            {
                evidence.Add(new TestEvidence(TestProjectEvidence, file.Path));
            }

            if (IsFrameworkConfig(fileName) || HasFrameworkDependency(file, fileName))
            {
                evidence.Add(new TestEvidence(FrameworkEvidence, file.Path));
            }

            if (HasExactFileMarker(fileName, ".spec."))
            {
                evidence.Add(new TestEvidence(SpecFileEvidence, file.Path));
            }

            if (HasExactFileMarker(fileName, ".test."))
            {
                evidence.Add(new TestEvidence(TestFileEvidence, file.Path));
            }
        }

        return evidence
            .Distinct()
            .OrderBy(item => item.Type, StringComparer.Ordinal)
            .ThenBy(item => item.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddTestDirectories(
        RepositoryFile file,
        ISet<string> observed,
        ICollection<TestEvidence> evidence)
    {
        string[] segments = file.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int directorySegmentCount = file.IsDirectory ? segments.Length : segments.Length - 1;

        for (int index = 0; index < directorySegmentCount; index++)
        {
            if (!TestDirectoryNames.Contains(segments[index], StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            string path = string.Join('/', segments.Take(index + 1));

            if (observed.Add(path))
            {
                evidence.Add(new TestEvidence(TestDirectoryEvidence, path));
            }
        }
    }

    private static bool IsFrameworkConfig(string fileName) =>
        string.Equals(fileName, "pytest.ini", StringComparison.OrdinalIgnoreCase)
        || fileName.StartsWith("jest.config.", StringComparison.OrdinalIgnoreCase)
        || fileName.StartsWith("vitest.config.", StringComparison.OrdinalIgnoreCase);

    private static bool HasFrameworkDependency(RepositoryFile file, string fileName)
    {
        if (file.Content is null)
        {
            return false;
        }

        if (string.Equals(fileName, "pom.xml", StringComparison.OrdinalIgnoreCase))
        {
            return HasPomTestDependency(file.Content);
        }

        return (string.Equals(fileName, "build.gradle", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fileName, "build.gradle.kts", StringComparison.OrdinalIgnoreCase))
            && HasGradleTestDependency(file.Content);
    }

    private static bool HasPomTestDependency(string content)
    {
        try
        {
            XDocument document = XDocument.Parse(content, LoadOptions.None);

            return document
                .Descendants()
                .Where(element => string.Equals(
                    element.Name.LocalName,
                    "dependency",
                    StringComparison.Ordinal))
                .Any(dependency => dependency.Elements().Any(element =>
                    string.Equals(element.Name.LocalName, "scope", StringComparison.Ordinal)
                    && string.Equals(element.Value.Trim(), "test", StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or InvalidOperationException)
        {
            return false;
        }
    }

    private static bool HasGradleTestDependency(string content)
    {
        string[] declarations =
        [
            "testImplementation",
            "testCompile",
            "testRuntimeOnly",
            "testFixturesImplementation"
        ];

        foreach (string rawLine in content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string line = rawLine.TrimStart();

            if (line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (declarations.Any(declaration =>
                    line.StartsWith(declaration + " ", StringComparison.Ordinal)
                    || line.StartsWith(declaration + "(", StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasExactFileMarker(string fileName, string marker)
    {
        int index = fileName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index > 0 && index + marker.Length < fileName.Length;
    }

    private static bool IsStrong(IReadOnlyList<TestEvidence> evidence)
    {
        TestEvidence[] testProjects = evidence
            .Where(item => item.Type == TestProjectEvidence)
            .ToArray();

        if (testProjects.Select(item => item.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 2)
        {
            return true;
        }

        bool hasSpec = evidence.Any(item => item.Type == SpecFileEvidence);
        bool hasTest = evidence.Any(item => item.Type == TestFileEvidence);

        if (hasSpec && hasTest)
        {
            return true;
        }

        TestEvidence[] highConfidence = evidence
            .Where(item => item.Type != TestDirectoryEvidence)
            .ToArray();
        string[] directories = evidence
            .Where(item => item.Type == TestDirectoryEvidence)
            .Select(item => item.Path)
            .ToArray();

        if (directories.Any(directory => highConfidence.Any(item => IsUnder(item.Path, directory))))
        {
            return true;
        }

        return highConfidence
            .Select(item => item.Type)
            .Distinct(StringComparer.Ordinal)
            .Count() >= 2
            && highConfidence
                .Select(item => item.Path)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() >= 2;
    }

    private static bool IsUnder(string path, string directory) =>
        path.StartsWith(directory + "/", StringComparison.OrdinalIgnoreCase);

    private static string GetFileName(string path) =>
        path[(path.LastIndexOf('/') + 1)..];

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? fileTreeAvailability,
        DataAvailability? projectFilesAvailability,
        IReadOnlyList<TestEvidence> recognized)
    {
        EvidenceCollection evidence = new();
        string fileTreeState = GetAvailabilityState(fileTreeAvailability);
        string projectFilesState = GetAvailabilityState(projectFilesAvailability);
        string summary = $"recognized={Format(recognized.Count)};complete={snapshot.FileTree.IsComplete.ToString(CultureInfo.InvariantCulture)};fileTreeAvailability={fileTreeState};projectFilesAvailability={projectFilesState}";
        DateTimeOffset observedAt = GetObservedAt(
            snapshot.CapturedAt,
            fileTreeAvailability,
            projectFilesAvailability);

        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryTestStructure",
            summary,
            location: null,
            observedAt,
            EvidenceConfidence.High,
            "Recognized evidence yalnız kilitli Version 1 path ve dependency kurallarından üretilmiştir."));

        foreach (IGrouping<string, TestEvidence> group in recognized
                     .GroupBy(item => item.Type, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            TestEvidence first = group.OrderBy(item => item.Path, StringComparer.Ordinal).First();
            evidence.Add(new EvidenceItem(
                SignalKey,
                first.Type,
                $"count={Format(group.Count())}",
                first.Path,
                observedAt,
                EvidenceConfidence.High,
                "Bu evidence türü için Ordinal sıralamadaki ilk recognized path gösterilmiştir."));
        }

        return evidence;
    }

    private static DateTimeOffset GetObservedAt(
        DateTimeOffset fallback,
        DataAvailability? first,
        DataAvailability? second)
    {
        DateTimeOffset? firstAt = first?.RetrievedAt;
        DateTimeOffset? secondAt = second?.RetrievedAt;

        if (firstAt is null)
        {
            return secondAt ?? fallback;
        }

        return secondAt is null || firstAt >= secondAt ? firstAt.Value : secondAt.Value;
    }

    private static string GetAvailabilityState(DataAvailability? availability) =>
        availability is null ? "not-recorded" : availability.Status.ToString();

    private static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        EvidenceCollection evidence,
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
            evidence.Items,
            explanation,
            recommendation,
            limitations,
            evaluatedAt);
    }

    private sealed record TestEvidence(string Type, string Path);
}
