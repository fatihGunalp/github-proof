using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 131 — the R dimension's Web wiring.
//
// R is the repository concurrency INSIDE one profile execution, and it must be
// externally configurable so a deployment can change it without a code change,
// exactly as public-profile-runtime-collection.md §10.5 treats the existing
// concurrency values. These tests pin the default (1, the unchanged sequential
// path), the configuration key, and the validation boundary.
public sealed class ProfileRepositoryConcurrencyWiringTests
{
    [Fact]
    public void RepositoryConcurrency_DefaultsToOne()
    {
        // Ships OFF: absent configuration, the Profile path behaves exactly as
        // it did before this session.
        using ServiceProvider provider = BuildProvider();

        Assert.Equal(
            1,
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>()
                .Value
                .MaxRepositoryConcurrency);
    }

    [Fact]
    public void RepositoryConcurrency_IsReadFromConfiguration()
    {
        using ServiceProvider provider = BuildProvider(
            Configuration(("ProfileGuardrails:Collection:MaxRepositoryConcurrency", "2")));

        Assert.Equal(
            2,
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>()
                .Value
                .MaxRepositoryConcurrency);
    }

    [Fact]
    public void RepositoryConcurrency_IsSeparateFromProfileAdmissionConcurrency()
    {
        // R and U are different knobs and must not be confused: configuring
        // repository concurrency must leave the U-dimension admission values
        // alone, and vice versa.
        using ServiceProvider provider = BuildProvider(
            Configuration(
                ("ProfileGuardrails:Collection:MaxRepositoryConcurrency", "2"),
                ("ProfileGuardrails:Admission:GlobalConcurrency", "2"),
                ("ProfileGuardrails:Admission:SameProfileConcurrency", "1")));

        ProfileCollectionOptions collection =
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>().Value;
        ProfileAdmissionOptions admission =
            provider.GetRequiredService<IOptions<ProfileAdmissionOptions>>().Value;

        Assert.Equal(2, collection.MaxRepositoryConcurrency);
        Assert.Equal(2, admission.GlobalConcurrency);
        Assert.Equal(1, admission.SameProfileConcurrency);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void RepositoryConcurrencyBelowOne_FailsValidation(string configured)
    {
        using ServiceProvider provider = BuildProvider(
            Configuration(
                ("ProfileGuardrails:Collection:MaxRepositoryConcurrency", configured)));

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<ProfileCollectionOptions>>().Value);
    }

    [Fact]
    public async Task ConfiguredConcurrency_ActuallyReachesTheOrchestrator()
    {
        // The options assertions above would all stay green if the argument
        // that passes MaxRepositoryConcurrency into the orchestrator were
        // deleted — the seam would die silently. This proves the composition
        // root really wires it, by observing that the resolved orchestrator
        // overlaps two repositories when configured with R=2.
        using ConcurrencyGate gate = new(expected: 2);

        using ServiceProvider provider = BuildProvider(
            Configuration(("ProfileGuardrails:Collection:MaxRepositoryConcurrency", "2")),
            new OverlapObservingSnapshotSource(gate));

        using IServiceScope scope = provider.CreateScope();

        ProfileRepositoryAnalysisOrchestrator orchestrator = scope.ServiceProvider
            .GetRequiredService<ProfileRepositoryAnalysisOrchestrator>();

        OverlapObservingSnapshotSource source = (OverlapObservingSnapshotSource)
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        await orchestrator.AnalyzeAsync(
            ProfileConcurrencyWiringSupport.CreateSnapshot(repositoryCount: 4),
            ProfileConcurrencyWiringSupport.Version);

        Assert.Equal(2, source.MaxObservedInFlight);
    }

    [Fact]
    public async Task DefaultConcurrency_KeepsTheOrchestratorSequential()
    {
        // The mirror of the test above: with no configuration the resolved
        // orchestrator must never overlap, which is what "ships OFF" means in
        // behavioral terms rather than only in options terms.
        NonOverlapObservingSnapshotSource source = new();

        using ServiceProvider provider = BuildProvider(configuration: null, source);
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositoryAnalysisOrchestrator orchestrator = scope.ServiceProvider
            .GetRequiredService<ProfileRepositoryAnalysisOrchestrator>();

        await orchestrator.AnalyzeAsync(
            ProfileConcurrencyWiringSupport.CreateSnapshot(repositoryCount: 4),
            ProfileConcurrencyWiringSupport.Version);

        Assert.Equal(1, source.MaxObservedInFlight);
    }

    private static IConfiguration Configuration(
        params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                values.Select(
                    entry => new KeyValuePair<string, string?>(entry.Key, entry.Value)))
            .Build();

    private static ServiceProvider BuildProvider(
        IConfiguration? configuration = null,
        IProfileRepositorySnapshotSource? snapshotSource = null)
    {
        ServiceCollection services = new();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();

        services.AddProfilePipeline(configuration);

        // Replaces only the repository snapshot source, so the orchestrator
        // under test is still the one the production composition built.
        if (snapshotSource is not null)
        {
            services.AddScoped(_ => snapshotSource);
        }

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class NeverCalledSnapshotCollector : IGitHubRepositorySnapshotCollector
    {
        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "This collector is a DI-resolution placeholder and must never be invoked.");
    }

    // Blocks each caller until two have arrived, so overlap is forced rather
    // than raced and a sequential orchestrator fails by timeout with a clear
    // reason instead of flaking.
    private sealed class ConcurrencyGate(int expected) : IDisposable
    {
        private readonly SemaphoreSlim released = new(0);
        private readonly Lock sync = new();
        private int arrived;

        public async Task ArriveAsync(CancellationToken cancellationToken)
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

    private sealed class OverlapObservingSnapshotSource(ConcurrencyGate gate)
        : IProfileRepositorySnapshotSource
    {
        private int inFlight;
        private int maxObservedInFlight;

        public int MaxObservedInFlight => Volatile.Read(ref maxObservedInFlight);

        public async Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            int current = Interlocked.Increment(ref inFlight);

            InterlockedMax(ref maxObservedInFlight, current);

            try
            {
                await gate.ArriveAsync(cancellationToken);

                return ProfileConcurrencyWiringSupport.Collected(reference);
            }
            finally
            {
                Interlocked.Decrement(ref inFlight);
            }
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int observed = Volatile.Read(ref target);

            while (value > observed)
            {
                int previous = Interlocked.CompareExchange(ref target, value, observed);

                if (previous == observed)
                {
                    return;
                }

                observed = previous;
            }
        }
    }

    private sealed class NonOverlapObservingSnapshotSource : IProfileRepositorySnapshotSource
    {
        private int inFlight;
        private int maxObservedInFlight;

        public int MaxObservedInFlight => Volatile.Read(ref maxObservedInFlight);

        public async Task<ProfileRepositorySnapshotResult> CollectAsync(
            ProfileRepositoryReference reference,
            CancellationToken cancellationToken = default)
        {
            int current = Interlocked.Increment(ref inFlight);

            if (current > Volatile.Read(ref maxObservedInFlight))
            {
                Volatile.Write(ref maxObservedInFlight, current);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);

                return ProfileConcurrencyWiringSupport.Collected(reference);
            }
            finally
            {
                Interlocked.Decrement(ref inFlight);
            }
        }
    }
}
