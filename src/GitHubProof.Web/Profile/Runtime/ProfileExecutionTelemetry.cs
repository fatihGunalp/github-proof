using System.Collections.Concurrent;
using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Web.Profile.Runtime;

// Operational truth about ONE profile analysis execution
// (docs/architecture/public-profile-runtime-collection.md §9).
//
// It is never evidence: nothing here is read by an evaluator, policy, composer,
// projector or report section, and it never flows back into RepositoryAnalysis,
// eligibility, patterns, representative selection or ProfileReport. It records
// counts only — never a token, an Authorization header, a repository name, a
// file path, or any source, README or manifest content.
//
// One instance belongs to exactly one execution, so two concurrent profile
// requests can never mix counters.
public sealed class ProfileExecutionTelemetry
{
    private readonly ConcurrentDictionary<string, int> requestCategories =
        new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, int> repositoryOutcomes =
        new(StringComparer.Ordinal);

    private int gitHubRequests;
    private int repositoriesDiscovered;
    private int repositoriesAdmitted;
    private int repositoriesCompleted;

    public int GitHubRequests => Volatile.Read(ref gitHubRequests);

    public int RepositoriesDiscovered => Volatile.Read(ref repositoriesDiscovered);

    public int RepositoriesAdmitted => Volatile.Read(ref repositoriesAdmitted);

    public int RepositoriesCompleted => Volatile.Read(ref repositoriesCompleted);

    // Admitted repositories whose collection or analysis did not complete
    // successfully — derived, never counted separately, so the two can never
    // disagree.
    public int RepositoriesIncomplete =>
        Math.Max(0, RepositoriesAdmitted - RepositoriesCompleted);

    public TimeSpan Duration { get; private set; }

    public string Status { get; private set; } = ProfileExecutionOutcome.NotRecorded;

    public string Outcome { get; private set; } = ProfileExecutionOutcome.NotRecorded;

    public IReadOnlyDictionary<string, int> RequestCategories => requestCategories;

    // Why admitted repositories could not complete, so a rate limit is
    // distinguishable from a forbidden repository and from an ordinary failure
    // (§9 "rate-limit / forbidden / failed ... when applicable"). The vocabulary
    // is closed by ProfileRepositoryOutcome.
    public IReadOnlyDictionary<string, int> RepositoryOutcomes => repositoryOutcomes;

    public void RecordGitHubRequest(string category)
    {
        Interlocked.Increment(ref gitHubRequests);

        // A category outside the closed vocabulary is folded into "other" so the
        // dictionary can never grow unboundedly at runtime.
        string key = ProfileGitHubRequestCategory.All.Contains(category)
            ? category
            : ProfileGitHubRequestCategory.Other;

        requestCategories.AddOrUpdate(key, 1, (_, current) => current + 1);
    }

    public void RecordRepositoriesDiscovered(int count) =>
        Interlocked.Exchange(ref repositoriesDiscovered, Math.Max(0, count));

    public void RecordRepositoryAdmitted() =>
        Interlocked.Increment(ref repositoriesAdmitted);

    public void RecordRepositoryCompleted() =>
        Interlocked.Increment(ref repositoriesCompleted);

    // Operational only. It says why THIS execution could not finish a
    // repository; it is never repository evidence and never reaches the
    // analysis, the profile or the report.
    public void RecordRepositoryOutcome(string outcome) =>
        repositoryOutcomes.AddOrUpdate(
            ProfileRepositoryOutcome.Normalize(outcome),
            1,
            (_, current) => current + 1);

    public void Complete(TimeSpan duration, string status, string outcome)
    {
        Duration = duration;
        Status = status;
        Outcome = outcome;
    }

    // A single structured Information-level line, so a controlled live run can
    // be read straight from the console. It deliberately carries no profile
    // identity: the measurement does not need one.
    public string ToLogMessage() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"Profile analysis completed: DurationMs={(long)Duration.TotalMilliseconds} "
            + $"GitHubRequests={GitHubRequests} "
            + $"RepositoriesDiscovered={RepositoriesDiscovered} "
            + $"RepositoriesAdmitted={RepositoriesAdmitted} "
            + $"RepositoriesCompleted={RepositoriesCompleted} "
            + $"RepositoriesIncomplete={RepositoriesIncomplete} "
            + $"Status={Status} "
            + $"Outcome={Outcome} "
            + $"RepositoryOutcomes={{{Format(repositoryOutcomes)}}} "
            + $"RequestCategories={{{Format(requestCategories)}}}");

    private static string Format(ConcurrentDictionary<string, int> counters) =>
        string.Join(
            ", ",
            counters
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}={entry.Value}"));
}

// The closed vocabulary for why an admitted repository did not complete. Every
// value is either a canonical DataAvailabilityStatus name — the same typed
// vocabulary the collectors already report — or one of the two explicit
// non-availability cases, so it can never grow at runtime and never carries a
// repository name, path or message.
public static class ProfileRepositoryOutcome
{
    public const string AnalysisFailed = "AnalysisFailed";
    public const string Other = "Other";

    private static readonly HashSet<string> Known =
        new(
            Enum.GetNames<DataAvailabilityStatus>().Concat([AnalysisFailed, Other]),
            StringComparer.Ordinal);

    public static IReadOnlyCollection<string> All => Known;

    public static string Normalize(string? outcome) =>
        outcome is not null && Known.Contains(outcome) ? outcome : Other;

    public static string FromAvailabilityStatus(DataAvailabilityStatus status) =>
        Normalize(status.ToString());
}

// The closed outcome vocabulary. It describes this execution only, never the
// developer, the repositories or the quality of the work, and it introduces no
// new AnalysisStatus or DataAvailabilityStatus value.
public static class ProfileExecutionOutcome
{
    public const string NotRecorded = "not-recorded";
    public const string Composed = "composed";
    public const string CompositionFailed = "composition-failed";
    public const string CollectionUnavailable = "collection-unavailable";
    public const string Cancelled = "cancelled";
    public const string DeadlineExpired = "deadline-expired";
    public const string BudgetProtectionRejected = "budget-protection-rejected";
    public const string Faulted = "faulted";
}
