using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;

namespace GitHubProof.Application.Profile;

// Drives the existing repository snapshot and analysis pipeline for admitted
// profile repositories. It owns no signal evaluation, no repository score and
// no clock of its own.
public sealed class ProfileRepositoryAnalysisOrchestrator
{
    private readonly IProfileRepositorySnapshotSource snapshotSource;
    private readonly IRepositoryAnalysisRunner analysisRunner;
    private readonly AnalysisAdmissionPolicy admissionPolicy;
    private readonly Func<Guid> analysisIdFactory;

    public ProfileRepositoryAnalysisOrchestrator(
        IProfileRepositorySnapshotSource snapshotSource,
        IRepositoryAnalysisRunner analysisRunner,
        AnalysisAdmissionPolicy admissionPolicy,
        Func<Guid>? analysisIdFactory = null)
    {
        this.snapshotSource = snapshotSource
            ?? throw new ArgumentNullException(nameof(snapshotSource));
        this.analysisRunner = analysisRunner
            ?? throw new ArgumentNullException(nameof(analysisRunner));
        this.admissionPolicy = admissionPolicy
            ?? throw new ArgumentNullException(nameof(admissionPolicy));
        this.analysisIdFactory = analysisIdFactory ?? Guid.NewGuid;
    }

    public async Task<ProfileAnalysisOrchestrationResult> AnalyzeAsync(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(version);

        List<ProfileRepositoryAnalysisResult> results = [];

        foreach (ProfileRepository repository in snapshot.Repositories.Repositories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AnalysisAdmissionResult admission = admissionPolicy.Evaluate(repository);

            results.Add(admission.Decision switch
            {
                AnalysisAdmissionDecision.Skip =>
                    ProfileRepositoryAnalysisResult.NotAdmitted(admission),

                AnalysisAdmissionDecision.Analyze =>
                    await AnalyzeRepositoryAsync(admission, version, cancellationToken),

                _ => ProfileRepositoryAnalysisResult.AdmissionUndetermined(admission)
            });
        }

        return new ProfileAnalysisOrchestrationResult(
            snapshot,
            version,
            admissionPolicy.PolicyVersion,
            results);
    }

    private async Task<ProfileRepositoryAnalysisResult> AnalyzeRepositoryAsync(
        AnalysisAdmissionResult admission,
        AnalysisVersion version,
        CancellationToken cancellationToken)
    {
        ProfileRepositorySnapshotResult collected;

        try
        {
            collected = await snapshotSource.CollectAsync(
                admission.Reference,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return ProfileRepositoryAnalysisResult.CollectionFailed(
                admission,
                $"Repository collection failed: {exception.Message}");
        }

        if (collected is null)
        {
            return ProfileRepositoryAnalysisResult.CollectionFailed(
                admission,
                "Repository collection returned no result.");
        }

        if (!collected.IsSuccessful)
        {
            return ProfileRepositoryAnalysisResult.CollectionUnavailable(
                admission,
                collected.Availability);
        }

        RepositorySnapshot repositorySnapshot = collected.Snapshot!;
        Guid analysisId = analysisIdFactory();

        if (analysisId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The analysis ID factory must produce a non-empty analysis ID.");
        }

        try
        {
            RepositoryAnalysis analysis = analysisRunner.Analyze(
                repositorySnapshot,
                version,
                analysisId);

            return ProfileRepositoryAnalysisResult.Analyzed(
                admission,
                analysis,
                collected.Availability);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return ProfileRepositoryAnalysisResult.AnalysisFailed(
                admission,
                collected.Availability,
                $"Repository analysis failed: {exception.Message}");
        }
    }
}
