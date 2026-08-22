using GitHubProof.Application.Profile;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Domain.Analysis.ValueObjects;

namespace GitHubProof.Web.Profile;

// Outer live-runtime truth only. ExecutionAvailability and its RetrievedAt do
// not enter Domain/Application evidence, provenance, profile observations or
// report projection.
public sealed record LiveProfileAnalysisResult
{
    public const string ExecutionDataKey = "profile-execution";
    public const string DeadlineExpiredReason = "execution-deadline-expired";
    public const string ProfileUnavailableReason = "profile-unavailable";
    public const string BudgetProtectionRejectedReason =
        "github-budget-protection-rejected";

    public LiveProfileAnalysisResult(
        DataAvailability? profileAvailability,
        DataAvailability executionAvailability,
        ProfilePipelineResult? pipelineResult)
    {
        ProfileAvailability = profileAvailability;
        ExecutionAvailability = executionAvailability
            ?? throw new ArgumentNullException(nameof(executionAvailability));
        PipelineResult = pipelineResult;

        Validate();
    }

    public DataAvailability? ProfileAvailability { get; }

    public DataAvailability ExecutionAvailability { get; }

    public ProfilePipelineResult? PipelineResult { get; }

    public bool IsSuccessful => PipelineResult is not null;

    public static LiveProfileAnalysisResult DeadlineExpired(
        DataAvailability? profileAvailability) =>
        new(
            profileAvailability,
            DataAvailability.Unavailable(
                ExecutionDataKey,
                DataAvailabilityStatus.Failed,
                DeadlineExpiredReason),
            pipelineResult: null);

    public static LiveProfileAnalysisResult ProfileUnavailable(
        DataAvailability profileAvailability) =>
        new(
            profileAvailability,
            DataAvailability.Unavailable(
                ExecutionDataKey,
                DataAvailabilityStatus.NotRequested,
                ProfileUnavailableReason),
            pipelineResult: null);

    public static LiveProfileAnalysisResult BudgetProtectionRejected(
        DataAvailability profileAvailability) =>
        new(
            profileAvailability,
            DataAvailability.Unavailable(
                ExecutionDataKey,
                DataAvailabilityStatus.Unavailable,
                BudgetProtectionRejectedReason),
            pipelineResult: null);

    public static LiveProfileAnalysisResult Completed(
        DataAvailability profileAvailability,
        ProfilePipelineResult pipelineResult,
        DateTimeOffset completedAt) =>
        new(
            profileAvailability,
            DataAvailability.Available(ExecutionDataKey, completedAt),
            pipelineResult);

    private void Validate()
    {
        if (!string.Equals(
                ExecutionAvailability.DataKey,
                ExecutionDataKey,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Execution availability DataKey must be '{ExecutionDataKey}'.",
                nameof(ExecutionAvailability));
        }

        bool isDeadline =
            ExecutionAvailability.Status == DataAvailabilityStatus.Failed
            && string.Equals(
                ExecutionAvailability.Reason,
                DeadlineExpiredReason,
                StringComparison.Ordinal)
            && ExecutionAvailability.RetrievedAt is null
            && PipelineResult is null
            && (ProfileAvailability is null || ProfileAvailability.IsAvailable);

        bool isProfileUnavailable =
            ProfileAvailability is not null
            && !ProfileAvailability.IsAvailable
            && ExecutionAvailability.Status == DataAvailabilityStatus.NotRequested
            && string.Equals(
                ExecutionAvailability.Reason,
                ProfileUnavailableReason,
                StringComparison.Ordinal)
            && ExecutionAvailability.RetrievedAt is null
            && PipelineResult is null;

        bool isCompleted =
            ProfileAvailability?.IsAvailable == true
            && ExecutionAvailability.Status == DataAvailabilityStatus.Available
            && ExecutionAvailability.Reason is null
            && ExecutionAvailability.RetrievedAt is not null
            && PipelineResult is not null;

        bool isBudgetProtectionRejected =
            ProfileAvailability?.IsAvailable == true
            && ExecutionAvailability.Status == DataAvailabilityStatus.Unavailable
            && string.Equals(
                ExecutionAvailability.Reason,
                BudgetProtectionRejectedReason,
                StringComparison.Ordinal)
            && ExecutionAvailability.RetrievedAt is null
            && PipelineResult is null;

        if (!isDeadline
            && !isProfileUnavailable
            && !isCompleted
            && !isBudgetProtectionRejected)
        {
            throw new ArgumentException(
                "The live Profile result does not match an authorized runtime outcome.");
        }
    }
}
