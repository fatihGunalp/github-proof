using GitHubProof.Application.Profile.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Application.Profile;

// One profile repository orchestration result. Unavailable or failed evidence
// stays technical: it is never converted into repository or developer evidence.
public sealed class ProfileRepositoryAnalysisResult
{
    private ProfileRepositoryAnalysisResult(
        AnalysisAdmissionResult admission,
        ProfileRepositoryAnalysisOutcome outcome,
        RepositoryAnalysis? analysis,
        DataAvailability? collectionAvailability,
        string? failureReason)
    {
        Admission = admission
            ?? throw new ArgumentNullException(nameof(admission));
        Outcome = outcome;
        Analysis = analysis;
        CollectionAvailability = collectionAvailability;
        FailureReason = failureReason;
    }

    public AnalysisAdmissionResult Admission { get; }

    public ProfileRepositoryReference Reference => Admission.Reference;

    public ProfileRepositoryAnalysisOutcome Outcome { get; }

    public RepositoryAnalysis? Analysis { get; }

    public DataAvailability? CollectionAvailability { get; }

    public string? FailureReason { get; }

    public bool IsAnalyzed => Analysis is not null;

    public static ProfileRepositoryAnalysisResult NotAdmitted(
        AnalysisAdmissionResult admission)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Skip);

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.NotAdmitted,
            analysis: null,
            collectionAvailability: null,
            failureReason: null);
    }

    public static ProfileRepositoryAnalysisResult AdmissionUndetermined(
        AnalysisAdmissionResult admission)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Undetermined);

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.AdmissionUndetermined,
            analysis: null,
            collectionAvailability: null,
            failureReason: null);
    }

    public static ProfileRepositoryAnalysisResult CollectionUnavailable(
        AnalysisAdmissionResult admission,
        DataAvailability collectionAvailability)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Analyze);
        ArgumentNullException.ThrowIfNull(collectionAvailability);

        if (collectionAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "An unavailable collection result cannot report available data.",
                nameof(collectionAvailability));
        }

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.CollectionUnavailable,
            analysis: null,
            collectionAvailability,
            failureReason: null);
    }

    public static ProfileRepositoryAnalysisResult CollectionFailed(
        AnalysisAdmissionResult admission,
        string failureReason)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Analyze);

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.CollectionFailed,
            analysis: null,
            collectionAvailability: null,
            RequireFailureReason(failureReason));
    }

    public static ProfileRepositoryAnalysisResult AnalysisFailed(
        AnalysisAdmissionResult admission,
        DataAvailability collectionAvailability,
        string failureReason)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Analyze);
        ArgumentNullException.ThrowIfNull(collectionAvailability);

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.AnalysisFailed,
            analysis: null,
            collectionAvailability,
            RequireFailureReason(failureReason));
    }

    public static ProfileRepositoryAnalysisResult Analyzed(
        AnalysisAdmissionResult admission,
        RepositoryAnalysis analysis,
        DataAvailability collectionAvailability)
    {
        RequireDecision(admission, AnalysisAdmissionDecision.Analyze);
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(collectionAvailability);

        if (!collectionAvailability.IsAvailable)
        {
            throw new ArgumentException(
                "An analyzed repository requires available collection data.",
                nameof(collectionAvailability));
        }

        return new ProfileRepositoryAnalysisResult(
            admission,
            ProfileRepositoryAnalysisOutcome.Analyzed,
            analysis,
            collectionAvailability,
            failureReason: null);
    }

    private static void RequireDecision(
        AnalysisAdmissionResult admission,
        AnalysisAdmissionDecision expected)
    {
        ArgumentNullException.ThrowIfNull(admission);

        if (admission.Decision != expected)
        {
            throw new ArgumentException(
                $"This result requires the {expected} admission decision.",
                nameof(admission));
        }
    }

    private static string RequireFailureReason(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "A failed repository result requires a failure reason.",
                nameof(failureReason));
        }

        return failureReason.Trim();
    }
}
