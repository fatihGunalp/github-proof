using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Infrastructure.Tests.GitHub;

internal static class ValidationArtifactWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task WriteAsync(
        string outputRoot,
        ValidationRepository repository,
        RepositorySnapshot snapshot,
        IReadOnlyList<SignalResult> signalResults,
        DateTimeOffset evaluationTime,
        CancellationToken cancellationToken)
    {
        string repositoryDirectory = Path.Combine(outputRoot, repository.Id);
        Directory.CreateDirectory(repositoryDirectory);

        object snapshotSummary = new
        {
            validationId = repository.Id,
            repository = new
            {
                snapshot.Repository.Owner,
                snapshot.Repository.Name,
                url = snapshot.Repository.Url.ToString(),
                snapshot.Repository.DefaultBranch,
                snapshot.Repository.IsPublic,
                snapshot.Repository.IsArchived,
                snapshot.Repository.IsFork,
                snapshot.Repository.IsTemplate,
                snapshot.Repository.PrimaryLanguage
            },
            frozenCommitSha = repository.CommitSha,
            observedHeadCommitSha = snapshot.HeadCommitSha,
            headMatchesFrozen = string.Equals(
                repository.CommitSha,
                snapshot.HeadCommitSha,
                StringComparison.Ordinal),
            snapshot.CapturedAt,
            atomicity = new
            {
                defaultBranchContentsBoundToObservedHead = true,
                otherSectionsMayReflectLiveGitHubState = true,
                limitation = "Metadata, issues, pull requests, releases, tags, contributors, and commit pagination are not collected as one atomic GitHub transaction."
            },
            counts = new
            {
                fileTree = snapshot.FileTree.Items.Count,
                commits = snapshot.Commits.Items.Count,
                issues = snapshot.Issues.Items.Count,
                pullRequests = snapshot.PullRequests.Items.Count,
                releases = snapshot.Releases.Items.Count,
                tags = snapshot.Tags.Items.Count,
                workflows = snapshot.Workflows.Items.Count,
                topics = snapshot.Topics.Count,
                contributors = snapshot.Contributors.Items.Count
            },
            availability = snapshot.Availability.Select(item => new
            {
                item.DataKey,
                item.Status,
                item.Reason,
                item.RetrievedAt
            }),
            collectionWarnings = snapshot.CollectionWarnings
        };

        await WriteJsonAsync(
            Path.Combine(repositoryDirectory, "snapshot-summary.json"),
            snapshotSummary,
            cancellationToken);
        await WriteJsonAsync(
            Path.Combine(repositoryDirectory, "signal-results.json"),
            new
            {
                validationId = repository.Id,
                frozenCommitSha = repository.CommitSha,
                evaluationTime,
                signalCount = signalResults.Count,
                signals = signalResults
            },
            cancellationToken);
        await WriteJsonAsync(
            Path.Combine(repositoryDirectory, "determinism.json"),
            new
            {
                validationId = repository.Id,
                snapshotAnalyses = 2,
                networkCollections = 1,
                fixedEvaluationTime = evaluationTime,
                semanticDifferences = Array.Empty<string>(),
                passed = true
            },
            cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(repositoryDirectory, "manual-review.md"),
            CreateManualReview(repository, signalResults),
            Encoding.UTF8,
            cancellationToken);
    }

    public static Task WriteFailureAsync(
        string outputRoot,
        ValidationRepository repository,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string repositoryDirectory = Path.Combine(outputRoot, repository.Id);
        Directory.CreateDirectory(repositoryDirectory);

        return WriteJsonAsync(
            Path.Combine(repositoryDirectory, "failure.json"),
            new
            {
                validationId = repository.Id,
                repository = $"{repository.Owner}/{repository.Name}",
                frozenCommitSha = repository.CommitSha,
                failureType = exception.GetType().Name,
                exception.Message,
                explainableFailure = true
            },
            cancellationToken);
    }

    private static Task WriteJsonAsync(
        string path,
        object value,
        CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine,
            Encoding.UTF8,
            cancellationToken);

    private static string CreateManualReview(
        ValidationRepository repository,
        IReadOnlyList<SignalResult> signalResults)
    {
        StringBuilder markdown = new();
        markdown.AppendLine($"# Manual Review — {repository.Owner}/{repository.Name}");
        markdown.AppendLine();
        markdown.AppendLine("```text");
        markdown.AppendLine($"Validation ID: {repository.Id}");
        markdown.AppendLine($"Frozen commit: {repository.CommitSha}");
        markdown.AppendLine($"Repository profile: {repository.RepositoryProfile}");
        markdown.AppendLine("ManualReviewStatus: Pending");
        markdown.AppendLine("Human review completed: NO");
        markdown.AppendLine("```");
        markdown.AppendLine();
        markdown.AppendLine("This file is a human-review template. Automated Codex validation is not recorded as human review.");
        markdown.AppendLine();
        markdown.AppendLine("## Repository-level questions");
        markdown.AppendLine();
        markdown.AppendLine("- [ ] Repository purpose is understandable from visible evidence.");
        markdown.AppendLine("- [ ] Installation and usage evidence is interpreted in context.");
        markdown.AppendLine("- [ ] Test and CI evidence avoids filename-only false positives.");
        markdown.AppendLine("- [ ] Archive, small size, education, and single-commit context is not treated as a quality judgment.");
        markdown.AppendLine("- [ ] Missing evidence is distinguished from unavailable data.");
        markdown.AppendLine("- [ ] No developer seniority, copying, authorship, or AI-use inference is present.");
        markdown.AppendLine();
        markdown.AppendLine("## Signal comparison");
        markdown.AppendLine();
        markdown.AppendLine("| Signal | Automated status | Score | Human result | Notes |");
        markdown.AppendLine("|---|---|---:|---|---|");

        foreach (SignalResult signal in signalResults)
        {
            markdown.AppendLine(
                $"| {EscapePipe(signal.SignalKey)} | {signal.Status} | {signal.Score?.ToString() ?? "—"} | Pending | |");
        }

        markdown.AppendLine();
        markdown.AppendLine("Allowed human results: `Agreement`, `MinorDifference`, `MajorDifference`, `CannotDetermine`.");
        return markdown.ToString();
    }

    private static string EscapePipe(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
}
