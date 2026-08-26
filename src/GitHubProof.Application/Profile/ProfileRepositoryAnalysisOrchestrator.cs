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
    // Session 131. R = how many admitted repositories may be collected and
    // analyzed at once inside ONE profile execution. The default is 1, which
    // takes the original sequential path verbatim, so default behavior is
    // unchanged until an owner decision raises it.
    //
    // public-profile-runtime-collection.md §8 authorizes "bounded concurrency"
    // for elapsed time but explicitly does NOT authorize "broad Task.WhenAll
    // fan-out over repositories or collectors", so the concurrent path is a
    // fixed pool of R workers over a shared queue (see AnalyzeConcurrentlyAsync).
    // At most R repositories are ever in flight regardless of how many are
    // admitted, because only R workers exist.
    public const int DefaultMaxRepositoryConcurrency = 1;

    private readonly IProfileRepositorySnapshotSource snapshotSource;
    private readonly IRepositoryAnalysisRunner analysisRunner;
    private readonly AnalysisAdmissionPolicy admissionPolicy;
    private readonly Func<Guid> analysisIdFactory;
    private readonly int maxRepositoryConcurrency;

    public ProfileRepositoryAnalysisOrchestrator(
        IProfileRepositorySnapshotSource snapshotSource,
        IRepositoryAnalysisRunner analysisRunner,
        AnalysisAdmissionPolicy admissionPolicy,
        Func<Guid>? analysisIdFactory = null,
        int maxRepositoryConcurrency = DefaultMaxRepositoryConcurrency)
    {
        this.snapshotSource = snapshotSource
            ?? throw new ArgumentNullException(nameof(snapshotSource));
        this.analysisRunner = analysisRunner
            ?? throw new ArgumentNullException(nameof(analysisRunner));
        this.admissionPolicy = admissionPolicy
            ?? throw new ArgumentNullException(nameof(admissionPolicy));
        this.analysisIdFactory = analysisIdFactory ?? Guid.NewGuid;

        if (maxRepositoryConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxRepositoryConcurrency),
                maxRepositoryConcurrency,
                "Repository concurrency must be at least 1.");
        }

        this.maxRepositoryConcurrency = maxRepositoryConcurrency;
    }

    public async Task<ProfileAnalysisOrchestrationResult> AnalyzeAsync(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(version);

        IReadOnlyList<ProfileRepositoryAnalysisResult> results =
            maxRepositoryConcurrency == 1
                ? await AnalyzeSequentiallyAsync(snapshot, version, cancellationToken)
                : await AnalyzeConcurrentlyAsync(snapshot, version, cancellationToken);

        return new ProfileAnalysisOrchestrationResult(
            snapshot,
            version,
            admissionPolicy.PolicyVersion,
            results);
    }

    // The original path, unchanged. R=1 executes exactly this.
    private async Task<IReadOnlyList<ProfileRepositoryAnalysisResult>> AnalyzeSequentiallyAsync(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion version,
        CancellationToken cancellationToken)
    {
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

        return results;
    }

    // Bounded-concurrency path. Three deliberate properties keep it
    // semantically identical to the sequential path (§6):
    //
    //  1. Admission is resolved FIRST, sequentially, in manifest order.
    //     AnalysisAdmissionPolicy.Evaluate is pure — immutable repository
    //     condition state and immutable policy fields, no I/O — so evaluating
    //     it up front cannot change any admission outcome, and the skip and
    //     undetermined results are produced in exactly the order they are
    //     today.
    //  2. Only the I/O-bound Analyze decisions run concurrently, and each
    //     writes into its OWN pre-assigned index. Result ordering therefore
    //     follows the manifest, never completion order.
    //  3. At most R are in flight, held by a permit rather than a fan-out.
    private async Task<IReadOnlyList<ProfileRepositoryAnalysisResult>> AnalyzeConcurrentlyAsync(
        GitHubProfileSnapshot snapshot,
        AnalysisVersion version,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ProfileRepository> repositories =
            snapshot.Repositories.Repositories;

        ProfileRepositoryAnalysisResult?[] results =
            new ProfileRepositoryAnalysisResult?[repositories.Count];

        List<(int Index, AnalysisAdmissionResult Admission)> admitted = [];

        for (int index = 0; index < repositories.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AnalysisAdmissionResult admission =
                admissionPolicy.Evaluate(repositories[index]);

            switch (admission.Decision)
            {
                case AnalysisAdmissionDecision.Skip:
                    results[index] =
                        ProfileRepositoryAnalysisResult.NotAdmitted(admission);
                    break;

                case AnalysisAdmissionDecision.Analyze:
                    admitted.Add((index, admission));
                    break;

                default:
                    results[index] =
                        ProfileRepositoryAnalysisResult.AdmissionUndetermined(admission);
                    break;
            }
        }

        if (admitted.Count > 0)
        {
            // A fixed pool of at most R workers pulling from one shared queue.
            //
            // Deliberately NOT one task per repository gated by a semaphore.
            // §8 prohibits "broad Task.WhenAll fan-out over repositories or
            // collectors" by name; a permit would have bounded the provider
            // pressure but would still have created one task per repository and
            // awaited Task.WhenAll over them, leaving this code to argue that
            // the contract's named construct is acceptable here. A worker pool
            // needs no such argument: the fan-out is over R workers, which is
            // the bound itself, and it never allocates per-repository task
            // state for a profile with hundreds of repositories.
            int workerCount = Math.Min(maxRepositoryConcurrency, admitted.Count);

            // Workers pull in ascending order, so collection still STARTS in
            // manifest order; only completion order is unconstrained, and
            // nothing reads completion order.
            int nextSlot = -1;

            Task[] workers = new Task[workerCount];

            for (int worker = 0; worker < workerCount; worker++)
            {
                workers[worker] = RunWorkerAsync();
            }

            await Task.WhenAll(workers);

            async Task RunWorkerAsync()
            {
                while (true)
                {
                    int slot = Interlocked.Increment(ref nextSlot);

                    if (slot >= admitted.Count)
                    {
                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    (int index, AnalysisAdmissionResult admission) = admitted[slot];

                    results[index] = await AnalyzeRepositoryAsync(
                        admission,
                        version,
                        cancellationToken);
                }
            }
        }

        ProfileRepositoryAnalysisResult[] ordered =
            new ProfileRepositoryAnalysisResult[results.Length];

        for (int index = 0; index < results.Length; index++)
        {
            ordered[index] = results[index]
                ?? throw new InvalidOperationException(
                    "Every profile repository must produce an analysis result.");
        }

        return ordered;
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
