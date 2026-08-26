using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Enums;
using GitHubProof.Domain.Analysis.Models;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Domain.Profile.ValueObjects;
using Xunit;
using static GitHubProof.Application.Tests.Profile.ProfileAnalysisTestSupport;

namespace GitHubProof.Application.Tests.Profile;

// Session 131 — bounded repository concurrency (R) inside ONE profile
// execution.
//
// public-profile-runtime-collection.md §8 authorizes "bounded concurrency" to
// reduce elapsed time but explicitly does NOT authorize "broad Task.WhenAll
// fan-out over repositories or collectors", and §6 requires the result to stay
// semantically identical. These tests pin both halves: that R>1 really does
// overlap work and really is capped, and that nothing observable about the
// result changes because of it.
public sealed class ProfileRepositoryConcurrencyTests
{
    [Fact]
    public void DefaultConcurrency_IsOne()
    {
        // The seam ships OFF. Raising R is an owner decision, so the default
        // must stay the sequential path this session did not change.
        Assert.Equal(1, ProfileRepositoryAnalysisOrchestrator.DefaultMaxRepositoryConcurrency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConcurrencyBelowOne_IsRejected(int concurrency)
    {
        ArgumentOutOfRangeException error =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ProfileRepositoryAnalysisOrchestrator(
                    CreateSource(),
                    CreateAnalysisRunner(),
                    new AnalysisAdmissionPolicy(TestEligibilityPolicy),
                    analysisIdFactory: null,
                    maxRepositoryConcurrency: concurrency));

        Assert.Equal("maxRepositoryConcurrency", error.ParamName);
    }

    [Fact]
    public async Task DefaultConcurrency_NeverOverlapsRepositories()
    {
        ConcurrencyObservingSource source = new(repositoryDelay: TimeSpan.FromMilliseconds(20));

        await CreateOrchestrator(source, concurrency: 1).AnalyzeAsync(
            CreateProfileSnapshot(ManyRepositories(6)),
            TestAnalysisVersion);

        // The whole point of the default: still strictly one at a time.
        Assert.Equal(1, source.MaxObservedInFlight);
    }

    [Fact]
    public async Task ConcurrencyOfTwo_ActuallyOverlapsRepositories()
    {
        // Proves the seam is wired, not merely present: without real overlap
        // this would read 1 and the whole session would be measuring nothing.
        //
        // Overlap is forced rather than raced: each repository blocks until two
        // have arrived, so on a single slow core this still observes 2 or the
        // gate times out loudly instead of flaking.
        using ConcurrencyGate gate = new(expected: 2);
        ConcurrencyObservingSource source = new(
            repositoryDelay: TimeSpan.Zero,
            onCollectAsync: gate.SignalAndWaitAsync);

        await CreateOrchestrator(source, concurrency: 2).AnalyzeAsync(
            CreateProfileSnapshot(ManyRepositories(6)),
            TestAnalysisVersion);

        Assert.Equal(2, source.MaxObservedInFlight);
    }

    [Fact]
    public async Task ConcurrencyIsBounded_NotAFanOut()
    {
        // 24 admitted repositories at R=2. A fan-out over repositories could
        // show up to 24 in flight; the fixed worker pool can never exceed 2.
        // This is §8's "not authorized" shape, pinned as an upper bound — the
        // bound is what the contract cares about, and unlike a lower bound it
        // cannot flake on a loaded machine.
        ConcurrencyObservingSource source = new(repositoryDelay: TimeSpan.FromMilliseconds(15));

        await CreateOrchestrator(source, concurrency: 2).AnalyzeAsync(
            CreateProfileSnapshot(ManyRepositories(24)),
            TestAnalysisVersion);

        Assert.InRange(source.MaxObservedInFlight, 1, 2);
        Assert.Equal(24, source.TotalCollected);
    }

    [Fact]
    public async Task ResultOrdering_FollowsTheManifest_NotCompletionOrder()
    {
        // The first repository finishes last and the last finishes first. If
        // results were appended on completion, this ordering would invert.
        ConcurrencyObservingSource source = new(
            repositoryDelay: TimeSpan.Zero,
            perRepositoryDelay: new Dictionary<string, TimeSpan>(StringComparer.Ordinal)
            {
                ["octocat/repo-0"] = TimeSpan.FromMilliseconds(120),
                ["octocat/repo-1"] = TimeSpan.FromMilliseconds(60),
                ["octocat/repo-2"] = TimeSpan.Zero,
            });

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source, concurrency: 3).AnalyzeAsync(
                CreateProfileSnapshot(ManyRepositories(3)),
                TestAnalysisVersion);

        Assert.Equal(
            ["repo-0", "repo-1", "repo-2"],
            result.Results.Select(repository => repository.Reference.Name));

        // And completion really did invert, so the assertion above is load-bearing.
        Assert.Equal(
            ["octocat/repo-2", "octocat/repo-1", "octocat/repo-0"],
            source.CompletionOrder);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task ConcurrentRun_IsSemanticallyIdenticalToTheSequentialRun(int concurrency)
    {
        // §6: same inputs, same versions => same result. Mixed admission
        // outcomes, a collection failure, an unavailable surface and an
        // analysis failure are all present so the comparison covers every
        // per-repository outcome the orchestrator can produce.
        ProfileAnalysisOrchestrationResult sequential = await RunMixedAsync(concurrency: 1);
        ProfileAnalysisOrchestrationResult concurrent = await RunMixedAsync(concurrency);

        Assert.Equal(Describe(sequential), Describe(concurrent));
        Assert.Equal(sequential.PolicyVersion, concurrent.PolicyVersion);
        Assert.Equal(sequential.Results.Count, concurrent.Results.Count);
    }

    [Fact]
    public async Task ConcurrentRun_PreservesPerRepositoryFailureIsolation()
    {
        // One repository failing must stay one repository failing — never a
        // faulted run, and never a lost sibling result.
        ProfileAnalysisOrchestrationResult result = await RunMixedAsync(concurrency: 2);

        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.Analyzed,
            FindByName(result, "analyzed").Outcome);
        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.CollectionFailed,
            FindByName(result, "collection-fails").Outcome);
        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.CollectionUnavailable,
            FindByName(result, "forbidden").Outcome);
        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.AnalysisFailed,
            FindByName(result, "analysis-fails").Outcome);
        Assert.Equal(
            ProfileRepositoryAnalysisOutcome.NotAdmitted,
            FindByName(result, "skipped").Outcome);
    }

    [Fact]
    public async Task ConcurrentRun_PropagatesCallerCancellation()
    {
        // §11.1: OperationCanceledException propagates and is never converted
        // into a result. Concurrency must not swallow it inside a task.
        using CancellationTokenSource cancellation = new();

        ConcurrencyObservingSource source = new(
            repositoryDelay: TimeSpan.FromMilliseconds(30),
            onCollect: _ => cancellation.Cancel());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateOrchestrator(source, concurrency: 2).AnalyzeAsync(
                CreateProfileSnapshot(ManyRepositories(8)),
                TestAnalysisVersion,
                cancellation.Token));
    }

    [Fact]
    public async Task ConcurrentRun_WithNothingAdmitted_StillProducesEveryResultInOrder()
    {
        // No admitted repository means no worker is ever started; the ordered
        // assembly still has to cover every manifest entry.
        //
        // The expected order is the manifest's own canonical order, not the
        // order the repositories were supplied in: ProfileRepositoryManifest
        // sorts by ProfileRepositoryReference (owner, then name, ordinal), so
        // "unknown" follows "empty-b". Determinism therefore has two layers —
        // the manifest's canonical sort, and this orchestrator preserving that
        // index regardless of completion order.
        ConcurrencyObservingSource source = new(repositoryDelay: TimeSpan.Zero);

        ProfileAnalysisOrchestrationResult result =
            await CreateOrchestrator(source, concurrency: 2).AnalyzeAsync(
                CreateProfileSnapshot(
                [
                    CreateProfileRepository("empty-a", RepositoryConditionState.Present),
                    CreateProfileRepository("unknown", RepositoryConditionState.Undetermined),
                    CreateProfileRepository("empty-b", RepositoryConditionState.Present),
                ]),
                TestAnalysisVersion);

        Assert.Equal(
            ["empty-a", "empty-b", "unknown"],
            result.Results.Select(repository => repository.Reference.Name));
        Assert.Equal(0, source.TotalCollected);
    }

    // ---------------------------------------------------------------- helpers

    private static async Task<ProfileAnalysisOrchestrationResult> RunMixedAsync(
        int concurrency)
    {
        ConcurrencyObservingSource source = new(
            repositoryDelay: TimeSpan.FromMilliseconds(5),
            responses: new Dictionary<string, Func<ProfileRepositorySnapshotResult>>(
                StringComparer.Ordinal)
            {
                ["octocat/analyzed"] =
                    () => CreateCollected(CreateRepositorySnapshot("analyzed")),
                ["octocat/analysis-fails"] =
                    () => CreateCollected(CreateRepositorySnapshot("analysis-fails")),
                ["octocat/collection-fails"] =
                    () => throw new HttpRequestException("GitHub is unreachable."),
                ["octocat/forbidden"] = () => CreateUnavailable(),
            });

        return await new ProfileRepositoryAnalysisOrchestrator(
                source,
                new FailingAnalysisRunner(CreateAnalysisRunner(), "analysis-fails"),
                new AnalysisAdmissionPolicy(TestEligibilityPolicy),
                analysisIdFactory: null,
                maxRepositoryConcurrency: concurrency)
            .AnalyzeAsync(
                CreateProfileSnapshot(
                [
                    CreateProfileRepository("analyzed"),
                    CreateProfileRepository("analysis-fails"),
                    CreateProfileRepository("collection-fails"),
                    CreateProfileRepository("forbidden"),
                    CreateProfileRepository("skipped", RepositoryConditionState.Present),
                    CreateProfileRepository("unknown", RepositoryConditionState.Undetermined),
                ]),
                TestAnalysisVersion);
    }

    private static ProfileRepositoryAnalysisOrchestrator CreateOrchestrator(
        IProfileRepositorySnapshotSource source,
        int concurrency) =>
        new(
            source,
            CreateAnalysisRunner(),
            new AnalysisAdmissionPolicy(TestEligibilityPolicy),
            analysisIdFactory: null,
            maxRepositoryConcurrency: concurrency);

    private static IEnumerable<ProfileRepository> ManyRepositories(int count) =>
        Enumerable.Range(0, count).Select(index => CreateProfileRepository($"repo-{index}"));

    private static ProfileRepositoryAnalysisResult FindByName(
        ProfileAnalysisOrchestrationResult result,
        string name) =>
        result.Results.Single(repository => string.Equals(
            repository.Reference.Name,
            name,
            StringComparison.Ordinal));

    // Everything observable about one result EXCEPT the analysis id, which is
    // a fresh Guid per run and is deliberately not part of the §6 invariant.
    private static IEnumerable<string> Describe(
        ProfileAnalysisOrchestrationResult result) =>
        result.Results.Select(repository => string.Join(
            "|",
            repository.Reference.FullName,
            repository.Outcome,
            repository.Admission.Decision,
            repository.Admission.Reason?.ToString() ?? "-",
            repository.Analysis?.Status.ToString() ?? "-",
            repository.Analysis?.ScoreBreakdown.NormalizedScore.ToString() ?? "-",
            repository.Analysis is null
                ? "-"
                : string.Join(
                    ",",
                    repository.Analysis.SignalResults.Select(
                        signal => $"{signal.SignalKey}:{signal.Status}:{signal.Score?.ToString() ?? "-"}")),
            repository.CollectionAvailability?.Status.ToString() ?? "-",
            repository.FailureReason ?? "-"));

    // A snapshot source that can delay per repository, records real overlap,
    // and is safe to call from several repositories at once. The existing
    // FakeSnapshotSource appends to a plain List and is therefore not usable
    // under R>1.
    // Releases every caller only once `expected` of them have arrived, so a
    // test can prove real overlap without depending on thread scheduling. If
    // the orchestrator never runs two at once the wait times out and the test
    // fails with that reason rather than flaking.
    private sealed class ConcurrencyGate(int expected) : IDisposable
    {
        private readonly SemaphoreSlim released = new(0);
        private readonly Lock sync = new();
        private int arrived;

        public async Task SignalAndWaitAsync(string _, CancellationToken cancellationToken)
        {
            bool isLast;

            lock (sync)
            {
                isLast = ++arrived >= expected;
            }

            if (isLast)
            {
                released.Release(expected);
            }

            using CancellationTokenSource timeout =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            await released.WaitAsync(timeout.Token);
        }

        public void Dispose() => released.Dispose();
    }

    private sealed class ConcurrencyObservingSource(
        TimeSpan repositoryDelay,
        IReadOnlyDictionary<string, Func<ProfileRepositorySnapshotResult>>? responses = null,
        IReadOnlyDictionary<string, TimeSpan>? perRepositoryDelay = null,
        Action<string>? onCollect = null,
        Func<string, CancellationToken, Task>? onCollectAsync = null)
        : IProfileRepositorySnapshotSource
    {
        private readonly object gate = new();
        private readonly List<string> completionOrder = [];
        private int inFlight;
        private int maxObservedInFlight;
        private int totalCollected;

        public int MaxObservedInFlight => Volatile.Read(ref maxObservedInFlight);

        public int TotalCollected => Volatile.Read(ref totalCollected);

        public IReadOnlyList<string> CompletionOrder
        {
            get
            {
                lock (gate)
                {
                    return [.. completionOrder];
                }
            }
        }

        public async Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(reference);

            int current = Interlocked.Increment(ref inFlight);

            lock (gate)
            {
                if (current > maxObservedInFlight)
                {
                    maxObservedInFlight = current;
                }
            }

            try
            {
                Interlocked.Increment(ref totalCollected);
                onCollect?.Invoke(reference.FullName);

                if (onCollectAsync is not null)
                {
                    await onCollectAsync(reference.FullName, cancellationToken);
                }

                TimeSpan delay =
                    perRepositoryDelay is not null
                    && perRepositoryDelay.TryGetValue(reference.FullName, out TimeSpan configured)
                        ? configured
                        : repositoryDelay;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }

                lock (gate)
                {
                    completionOrder.Add(reference.FullName);
                }

                if (responses is null)
                {
                    return CreateCollected(CreateRepositorySnapshot(reference.Name));
                }

                if (!responses.TryGetValue(
                        reference.FullName,
                        out Func<ProfileRepositorySnapshotResult>? response))
                {
                    throw new InvalidOperationException(
                        $"No collection response is configured for '{reference.FullName}'.");
                }

                return response();
            }
            finally
            {
                Interlocked.Decrement(ref inFlight);
            }
        }
    }
}
