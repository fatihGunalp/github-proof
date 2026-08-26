using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 134 — the Milestone B production-candidate runtime configuration.
//
// Session 133 measured R3/R4/R5 and the independent closure review accepted
// R5 as the Milestone B production-candidate repository concurrency. This
// class proves the SELECTED value through the real ASP.NET Core configuration
// file-loading path (`appsettings.Production.json`, the same file the
// deployed process reads), not through an in-memory test double — the same
// standard applied by `ProfileMilestoneBRuntimeGateHarness` to the runtime
// composition it measures.
//
// Deliberately excluded from the loader below: environment variables and
// command-line arguments. Both are legitimate additional configuration layers
// in the real host, but including them here would make this test's outcome
// depend on the executing machine's ambient environment rather than on the
// repository's own file-based production candidate — the opposite of
// deterministic proof. Their absence here says nothing about whether a real
// deployment may also use them; it only keeps this test isolated to the one
// artifact this session adds.
//
// U (profile concurrency, `ProfileAdmissionOptions.GlobalConcurrency`) and the
// GraphQL feature flags are deliberately left OUT of
// `appsettings.Production.json`: their already-correct default (1 and false
// respectively) is exactly the production-candidate value, so adding
// duplicate configuration for them would carry no semantic weight. These
// tests still pin that the new file leaves them untouched.
public sealed class ProfileProductionCandidateConfigurationTests
{
    [Fact]
    public void ProductionEnvironment_ResolvesRepositoryConcurrencyToFive()
    {
        IConfiguration configuration = LoadWebProjectConfiguration("Production");

        using ServiceProvider provider = BuildProvider(configuration);

        Assert.Equal(
            5,
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>()
                .Value
                .MaxRepositoryConcurrency);
    }

    [Fact]
    public void ProductionEnvironment_LeavesProfileConcurrencyAtOne()
    {
        // U is a separate dimension from R (Session 131) and the Session 134
        // production candidate does not raise it. This proves the new file
        // does not accidentally widen ProfileAdmissionOptions along with
        // ProfileCollectionOptions.
        IConfiguration configuration = LoadWebProjectConfiguration("Production");

        using ServiceProvider provider = BuildProvider(configuration);

        ProfileAdmissionOptions admission = provider
            .GetRequiredService<IOptions<ProfileAdmissionOptions>>()
            .Value;

        Assert.Equal(1, admission.GlobalConcurrency);
        Assert.Equal(1, admission.SameProfileConcurrency);
    }

    [Fact]
    public void ProductionEnvironment_LeavesGraphQlFeatureFlagsOff()
    {
        // The Session 126 secondary-limit residual still blocks every GraphQL
        // switch from production/beta/default-on. The R5 production candidate
        // must not move that position.
        IConfiguration configuration = LoadWebProjectConfiguration("Production");

        using ServiceProvider provider = BuildProvider(configuration);

        Assert.False(
            provider.GetRequiredService<IOptions<ProfileGraphQlShadowOptions>>()
                .Value
                .LanguagesShadowEnabled);
        Assert.False(
            provider.GetRequiredService<IOptions<ProfileGraphQlHybridOptions>>()
                .Value
                .RepositoryEnvelopeEnabled);
    }

    [Fact]
    public void NonProductionEnvironment_IsNotMaskedByTheProductionValue()
    {
        // No appsettings.Development.json exists, so a Development run falls
        // through to the unchanged source default (1) rather than silently
        // inheriting the production candidate. This is the opposite failure
        // mode from the one Session 134 is proposing to fix, and it must stay
        // impossible.
        IConfiguration configuration = LoadWebProjectConfiguration("Development");

        using ServiceProvider provider = BuildProvider(configuration);

        Assert.Equal(
            1,
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>()
                .Value
                .MaxRepositoryConcurrency);
    }

    [Fact]
    public async Task ProductionConfiguration_ActuallyReachesTheOrchestratorAsFive()
    {
        // The options assertions above would all stay green even if the
        // argument that passes MaxRepositoryConcurrency into the orchestrator
        // were deleted (Session 131 already guarded this for the general
        // seam; this proves it again for the actual Session 134 file). Seven
        // repositories, capacity five: the fifth and sixth+seventh arrivals
        // must be admitted only as earlier ones complete, so max observed
        // in-flight must be exactly five, never seven and never one.
        IConfiguration configuration = LoadWebProjectConfiguration("Production");

        using ConcurrencyGate gate = new(expected: 5);
        OverlapObservingSnapshotSource source = new(gate);

        using ServiceProvider provider = BuildProvider(configuration, source);
        using IServiceScope scope = provider.CreateScope();

        ProfileRepositoryAnalysisOrchestrator orchestrator = scope.ServiceProvider
            .GetRequiredService<ProfileRepositoryAnalysisOrchestrator>();

        ProfileAnalysisOrchestrationResult result = await orchestrator.AnalyzeAsync(
            ProfileConcurrencyWiringSupport.CreateSnapshot(repositoryCount: 7),
            ProfileConcurrencyWiringSupport.Version);

        Assert.Equal(5, source.MaxObservedInFlight);

        // No repository-count cap, sampling or top-N: seven admitted
        // repositories in means seven results out, regardless of R.
        Assert.Equal(7, result.Results.Count);
    }

    private static IConfiguration LoadWebProjectConfiguration(string environmentName) =>
        new ConfigurationBuilder()
            .SetBasePath(LocateWebProjectDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile(
                $"appsettings.{environmentName}.json",
                optional: true,
                reloadOnChange: false)
            .Build();

    private static string LocateWebProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                return Path.Combine(directory.FullName, "src", "GitHubProof.Web");
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }

    private static ServiceProvider BuildProvider(
        IConfiguration configuration,
        IProfileRepositorySnapshotSource? snapshotSource = null)
    {
        ServiceCollection services = new();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();

        services.AddProfilePipeline(configuration);

        // Replaces only the repository snapshot source, so the orchestrator
        // under test is still the one the production composition — reading
        // the real appsettings.Production.json — builds.
        if (snapshotSource is not null)
        {
            services.AddScoped(_ => snapshotSource);
        }

        return services.BuildServiceProvider(validateScopes: true);
    }

    // Mirrors ProfileRepositoryConcurrencyWiringTests' gate/observer pair
    // (Session 131): forces genuine overlap up to `expected` arrivals instead
    // of racing for it, so a sequential regression fails by timeout with a
    // clear reason instead of flaking.
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
}
