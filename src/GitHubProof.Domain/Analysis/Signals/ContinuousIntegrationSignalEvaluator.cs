using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Domain.Analysis.Signals;

public sealed class ContinuousIntegrationSignalEvaluator : ISignalEvaluator
{
    public const string SignalKey = "validation.continuous-integration";

    private const string WorkflowsDataKey = "workflows";
    private readonly TimeProvider _timeProvider;

    public ContinuousIntegrationSignalEvaluator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public SignalDefinition Definition { get; } = new(
        SignalKey,
        AnalysisCategory.ValidationAndUsability,
        maximumScore: 8,
        requiredDataKeys: [WorkflowsDataKey],
        includeInOverallScore: true,
        version: 1);

    public SignalResult Evaluate(RepositorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        DateTimeOffset evaluatedAt = _timeProvider.GetUtcNow();
        DataAvailability? availability = snapshot.FindAvailability(WorkflowsDataKey);
        IReadOnlyList<WorkflowObservation> observations = snapshot.Workflows.Items
            .OrderBy(workflow => workflow.Path, StringComparer.Ordinal)
            .Select(InspectWorkflow)
            .ToArray();
        bool hasBuild = observations.Any(item => item.HasBuild);
        bool hasTest = observations.Any(item => item.HasTest);
        EvidenceCollection evidence = CollectEvidence(snapshot, availability, observations);

        if (hasBuild && hasTest)
        {
            return CreateResult(
                SignalStatus.Strong,
                score: 8,
                evidence,
                evaluatedAt);
        }

        bool corpusComplete = availability?.IsAvailable == true
            && snapshot.Workflows.IsComplete;

        if (!corpusComplete || observations.Any(item => item.ContentUnavailable))
        {
            return CreateResult(
                SignalStatus.UnavailableEvidence,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (observations.Any(item => item.HasAmbiguity))
        {
            return CreateResult(
                SignalStatus.RequiresHumanReview,
                score: null,
                evidence,
                evaluatedAt);
        }

        if (observations.Count == 0)
        {
            return CreateResult(
                SignalStatus.Missing,
                score: 0,
                evidence,
                evaluatedAt);
        }

        if (hasBuild || hasTest)
        {
            string recognized = hasBuild ? "build" : "test";

            return CreateResult(
                SignalStatus.Present,
                score: 6,
                evidence,
                evaluatedAt);
        }

        bool deployOnly = observations.All(item => item.HasDeploy);

        if (deployOnly)
        {
            return CreateResult(
                SignalStatus.Partial,
                score: 3,
                evidence,
                evaluatedAt);
        }

        return CreateResult(
            SignalStatus.Partial,
            score: 2,
            evidence,
            evaluatedAt);
    }

    private static WorkflowObservation InspectWorkflow(RepositoryFile workflow)
    {
        if (workflow.Content is null)
        {
            return new WorkflowObservation(
                workflow.Path,
                HasBuild: false,
                HasTest: false,
                HasDeploy: false,
                HasAmbiguity: false,
                ContentUnavailable: true);
        }

        RunCommandExtraction extraction = ExtractRunCommands(workflow.Content);
        bool hasBuild = false;
        bool hasTest = false;
        bool hasDeploy = false;

        foreach (string command in extraction.Commands.SelectMany(SplitCommandSegments))
        {
            hasBuild |= IsBuildCommand(command);
            hasTest |= IsTestCommand(command);
            hasDeploy |= IsDeployCommand(command);
        }

        return new WorkflowObservation(
            workflow.Path,
            hasBuild,
            hasTest,
            hasDeploy,
            extraction.HasAmbiguity,
            ContentUnavailable: false);
    }

    private static RunCommandExtraction ExtractRunCommands(string content)
    {
        string normalized = content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        List<string> commands = [];
        bool ambiguity = string.IsNullOrWhiteSpace(content);

        for (int index = 0; index < lines.Length; index++)
        {
            string rawLine = lines[index];
            int indent = CountLeadingSpaces(rawLine, out bool hasIndentTab);
            ambiguity |= hasIndentTab;
            string line = rawLine[indent..].TrimEnd();

            if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                line = line[2..].TrimStart();
            }

            if (!line.StartsWith("run:", StringComparison.Ordinal))
            {
                continue;
            }

            string scalar = line[4..].TrimStart();

            if (IsBlockIndicator(scalar))
            {
                List<string> blockLines = [];

                while (index + 1 < lines.Length)
                {
                    string candidate = lines[index + 1];
                    int candidateIndent = CountLeadingSpaces(candidate, out bool candidateTab);
                    ambiguity |= candidateTab;

                    if (candidate.Trim().Length > 0 && candidateIndent <= indent)
                    {
                        break;
                    }

                    index++;

                    if (candidate.Trim().Length > 0)
                    {
                        blockLines.Add(candidate.Trim());
                    }
                }

                if (blockLines.Count == 0)
                {
                    ambiguity = true;
                }
                else
                {
                    commands.Add(string.Join('\n', blockLines));
                }

                continue;
            }

            if (!TryReadInlineScalar(scalar, out string? command))
            {
                ambiguity = true;
                continue;
            }

            commands.Add(command!);
        }

        return new RunCommandExtraction(commands, ambiguity);
    }

    private static int CountLeadingSpaces(string value, out bool hasTab)
    {
        int count = 0;
        hasTab = false;

        foreach (char character in value)
        {
            if (character == ' ')
            {
                count++;
                continue;
            }

            if (character == '\t')
            {
                hasTab = true;
                count++;
                continue;
            }

            break;
        }

        return count;
    }

    private static bool IsBlockIndicator(string value) =>
        value is "|" or "|-" or "|+" or ">" or ">-" or ">+";

    private static bool TryReadInlineScalar(string scalar, out string? command)
    {
        command = null;

        if (string.IsNullOrWhiteSpace(scalar) || scalar.StartsWith("#", StringComparison.Ordinal))
        {
            return false;
        }

        char? quote = null;
        int commentIndex = -1;

        for (int index = 0; index < scalar.Length; index++)
        {
            char character = scalar[index];

            if (character is '\'' or '"')
            {
                if (quote is null)
                {
                    quote = character;
                }
                else if (quote == character)
                {
                    quote = null;
                }

                continue;
            }

            if (character == '#'
                && quote is null
                && (index == 0 || char.IsWhiteSpace(scalar[index - 1])))
            {
                commentIndex = index;
                break;
            }
        }

        if (quote is not null)
        {
            return false;
        }

        string normalized = (commentIndex >= 0 ? scalar[..commentIndex] : scalar).Trim();

        if (normalized.Length >= 2
            && normalized[0] is '\'' or '"'
            && normalized[^1] == normalized[0])
        {
            normalized = normalized[1..^1].Trim();
        }

        if (normalized.Length == 0)
        {
            return false;
        }

        command = normalized;
        return true;
    }

    private static IEnumerable<string> SplitCommandSegments(string command)
    {
        string normalized = command
            .Replace("&&", "\n", StringComparison.Ordinal)
            .Replace("||", "\n", StringComparison.Ordinal)
            .Replace(';', '\n');

        return normalized
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Trim())
            .Where(segment => segment.Length > 0 && !segment.StartsWith("#", StringComparison.Ordinal));
    }

    private static bool IsBuildCommand(string command)
    {
        string[] tokens = Tokenize(command);

        return StartsWith(tokens, "dotnet", "build")
            || IsPackageScript(tokens, "build")
            || IsMavenGoal(tokens, "package", "verify")
            || IsGradleGoal(tokens, "build")
            || StartsWith(tokens, "cargo", "build")
            || StartsWith(tokens, "go", "build")
            || tokens.FirstOrDefault() == "make";
    }

    private static bool IsTestCommand(string command)
    {
        string[] tokens = Tokenize(command);

        return StartsWith(tokens, "dotnet", "test")
            || IsPackageScript(tokens, "test")
            || tokens.FirstOrDefault() == "pytest"
            || StartsWith(tokens, "python", "-m", "pytest")
            || IsMavenGoal(tokens, "test", "verify")
            || IsGradleGoal(tokens, "test")
            || StartsWith(tokens, "cargo", "test")
            || StartsWith(tokens, "go", "test");
    }

    private static bool IsDeployCommand(string command)
    {
        string[] tokens = Tokenize(command);

        return IsPackageScript(tokens, "deploy")
            || StartsWith(tokens, "vercel", "deploy")
            || StartsWith(tokens, "netlify", "deploy")
            || StartsWith(tokens, "firebase", "deploy")
            || StartsWith(tokens, "kubectl", "apply")
            || StartsWith(tokens, "helm", "upgrade")
            || StartsWith(tokens, "docker", "push");
    }

    private static string[] Tokenize(string command) =>
        command.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static bool IsPackageScript(string[] tokens, string script)
    {
        if (tokens.Length < 2 || tokens[0] is not ("npm" or "yarn" or "pnpm"))
        {
            return false;
        }

        return string.Equals(tokens[1], script, StringComparison.Ordinal)
            || tokens.Length >= 3
                && tokens[1] is "run" or "run-script"
                && string.Equals(tokens[2], script, StringComparison.Ordinal);
    }

    private static bool IsMavenGoal(string[] tokens, params string[] goals)
    {
        if (tokens.Length == 0 || tokens[0] is not ("mvn" or "mvnw" or "./mvnw"))
        {
            return false;
        }

        return tokens.Skip(1).Any(token => goals.Contains(token, StringComparer.Ordinal));
    }

    private static bool IsGradleGoal(string[] tokens, params string[] goals)
    {
        if (tokens.Length == 0 || tokens[0] is not ("gradle" or "gradlew" or "./gradlew"))
        {
            return false;
        }

        return tokens.Skip(1).Any(token => goals.Contains(token, StringComparer.Ordinal));
    }

    private static bool StartsWith(string[] actual, params string[] expected)
    {
        return actual.Length >= expected.Length
            && expected.Select((value, index) => string.Equals(
                actual[index],
                value,
                StringComparison.Ordinal)).All(matches => matches);
    }

    private static EvidenceCollection CollectEvidence(
        RepositorySnapshot snapshot,
        DataAvailability? availability,
        IReadOnlyList<WorkflowObservation> observations)
    {
        EvidenceCollection evidence = new();
        string state = availability is null ? "not-recorded" : availability.Status.ToString();
        string summary = $"workflows={Format(observations.Count)};build={Format(observations.Count(item => item.HasBuild))};test={Format(observations.Count(item => item.HasTest))};deploy={Format(observations.Count(item => item.HasDeploy))};ambiguous={Format(observations.Count(item => item.HasAmbiguity))};contentUnavailable={Format(observations.Count(item => item.ContentUnavailable))};complete={snapshot.Workflows.IsComplete.ToString(CultureInfo.InvariantCulture)};availability={state}";
        DateTimeOffset observedAt = availability?.RetrievedAt ?? snapshot.CapturedAt;

        evidence.Add(new EvidenceItem(
            SignalKey,
            "RepositoryWorkflows",
            summary,
            location: null,
            observedAt,
            EvidenceConfidence.High,
            "Yalnız workflow run scalar komutları bounded Version 1 command aileleriyle değerlendirilmiştir."));

        AddFirstEvidence(evidence, observations, item => item.HasBuild, "WorkflowBuild", observedAt);
        AddFirstEvidence(evidence, observations, item => item.HasTest, "WorkflowTest", observedAt);
        AddFirstEvidence(evidence, observations, item => item.HasDeploy, "WorkflowDeploy", observedAt);
        AddFirstEvidence(evidence, observations, item => item.HasAmbiguity, "WorkflowAmbiguity", observedAt);

        if (observations.Count > 0
            && observations.All(item => !item.HasBuild && !item.HasTest && !item.HasDeploy && !item.HasAmbiguity))
        {
            AddFirstEvidence(evidence, observations, _ => true, "WorkflowUnknownPurpose", observedAt);
        }

        return evidence;
    }

    private static void AddFirstEvidence(
        EvidenceCollection evidence,
        IReadOnlyList<WorkflowObservation> observations,
        Func<WorkflowObservation, bool> predicate,
        string source,
        DateTimeOffset observedAt)
    {
        WorkflowObservation? first = observations
            .Where(predicate)
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .FirstOrDefault();

        if (first is null)
        {
            return;
        }

        evidence.Add(new EvidenceItem(
            SignalKey,
            source,
            "recognized",
            first.Path,
            observedAt,
            EvidenceConfidence.High,
            "Bu evidence türü için Ordinal sıralamadaki ilk workflow path'i gösterilmiştir."));
    }

    private static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private SignalResult CreateResult(
        SignalStatus status,
        int? score,
        EvidenceCollection evidence,
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
            evaluatedAt);
    }

    private sealed record WorkflowObservation(
        string Path,
        bool HasBuild,
        bool HasTest,
        bool HasDeploy,
        bool HasAmbiguity,
        bool ContentUnavailable);

    private sealed record RunCommandExtraction(
        IReadOnlyList<string> Commands,
        bool HasAmbiguity);
}
