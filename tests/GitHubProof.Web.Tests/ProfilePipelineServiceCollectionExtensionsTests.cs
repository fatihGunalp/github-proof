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

// P8-I5 "PRODUCTION ADAPTER CHECK" / "WEB" requirements: Web performs only
// DI/wiring, never an analysis decision. These tests prove the ServiceCollection
// extension resolves the real Application ProfilePipeline end to end (17) and
// that Web's own wiring code contributes no eligibility, composition,
// pattern-promotion, representative-selection or report logic of its own (18)
// — every resolved component is the exact production Domain/Application type,
// never a Web-defined decorator or override.
public sealed class ProfilePipelineServiceCollectionExtensionsTests
{
    [Fact]
    public void AddProfilePipeline_ResolvesProfilePipeline()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        ProfilePipeline pipeline = scope.ServiceProvider.GetRequiredService<ProfilePipeline>();

        Assert.NotNull(pipeline);
    }

    [Fact]
    public void AddProfilePipeline_ResolvesTheExactApplicationPipelineType()
    {
        // Web owns no analysis decision: DI must resolve the real Application
        // ProfilePipeline itself, never a Web-namespace subclass or wrapper
        // that could intercept or override its behavior.
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        object pipeline = scope.ServiceProvider.GetRequiredService<ProfilePipeline>();

        Assert.Equal(typeof(ProfilePipeline), pipeline.GetType());
        Assert.Equal(
            "GitHubProof.Application.Profile",
            typeof(ProfilePipeline).Namespace);
    }

    [Fact]
    public void AddProfilePipeline_RegistersTheProductionRuntimeVersionValues()
    {
        using ServiceProvider provider = BuildProvider();

        // Session 099: production runs on the policy version carrying the Fork
        // Policy B admission rule. V1 stays immutable and is still canonical.
        Assert.Equal(
            EligibilityPolicy.V2,
            provider.GetRequiredService<EligibilityPolicy>());
        Assert.Equal(
            ObservationRuleVersion.V1,
            provider.GetRequiredService<ObservationRuleVersion>());
        Assert.Equal(
            PatternRuleVersion.V1,
            provider.GetRequiredService<PatternRuleVersion>());
        Assert.Equal(
            RepresentativeRepositoryRuleVersion.V1,
            provider.GetRequiredService<RepresentativeRepositoryRuleVersion>());
        Assert.Equal(
            ProfileAnalyzerVersion.V1,
            provider.GetRequiredService<ProfileAnalyzerVersion>());
    }

    // Session 097: the profile snapshot source is now wrapped by an
    // observation-only telemetry recorder. The invariant this test protects is
    // unchanged — Web still contributes no analysis decision — so it asserts
    // that the wrapper does nothing but delegate to the exact production
    // Infrastructure adapter.
    [Fact]
    public void AddProfilePipeline_ResolvesTheProductionSnapshotSourceAdapter()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        IProfileRepositorySnapshotSource source =
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        TelemetryProfileRepositorySnapshotSource recorder =
            Assert.IsType<TelemetryProfileRepositorySnapshotSource>(source);

        Assert.Equal(
            "GitHubProof.Infrastructure.GitHub.Profile.GitHubProfileRepositorySnapshotSource",
            recorder.Inner.GetType().FullName);
    }

    [Fact]
    public void AddProfilePipeline_ResolvesTheProductionAnalysisRunner()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        IRepositoryAnalysisRunner runner =
            scope.ServiceProvider.GetRequiredService<IRepositoryAnalysisRunner>();

        TelemetryRepositoryAnalysisRunner recorder =
            Assert.IsType<TelemetryRepositoryAnalysisRunner>(runner);

        Assert.Equal(typeof(RepositoryAnalysisRunner), recorder.Inner.GetType());
    }

    // The profile path owns its own collection composition, so it must not
    // resolve the Repository Proof collector registration at all
    // (docs/architecture/public-profile-runtime-collection.md §3.1, §3.2).
    [Fact]
    public void AddProfilePipeline_DoesNotReuseTheRepositoryProofSnapshotCollector()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        // The placeholder is still the registered Repository Proof collector and
        // still resolvable, yet resolving the whole profile pipeline never
        // invokes it, which the placeholder itself enforces by throwing.
        Assert.IsType<NeverCalledSnapshotCollector>(
            scope.ServiceProvider.GetRequiredService<IGitHubRepositorySnapshotCollector>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ProfilePipeline>());
    }

    [Fact]
    public void AddProfilePipeline_RegistersAProfileSpecificGitHubClient()
    {
        using ServiceProvider provider = BuildProvider();

        HttpClient httpClient = provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(
                ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName);

        Assert.Equal(new Uri("https://api.github.com/"), httpClient.BaseAddress);
        Assert.Equal(TimeSpan.FromMinutes(2), httpClient.Timeout);
    }

    [Fact]
    public void AddProfilePipeline_LeavesExecutionDeadlineExplicitlyUnconfiguredByDefault()
    {
        using ServiceProvider provider = BuildProvider();

        ProfileExecutionDeadlineOptions options = provider
            .GetRequiredService<IOptions<ProfileExecutionDeadlineOptions>>()
            .Value;

        Assert.Null(options.Timeout);
        Assert.NotNull(provider.GetRequiredService<IProfileExecutionDeadlineFactory>());
    }

    [Fact]
    public void AddProfilePipeline_UsesOnlyProvisionalConcurrencyDefaults()
    {
        using ServiceProvider provider = BuildProvider();

        ProfileAdmissionOptions admission = provider
            .GetRequiredService<IOptions<ProfileAdmissionOptions>>()
            .Value;
        ProfileGitHubBudgetProtectionOptions budget = provider
            .GetRequiredService<IOptions<ProfileGitHubBudgetProtectionOptions>>()
            .Value;

        Assert.Equal(1, admission.GlobalConcurrency);
        Assert.Equal(1, admission.SameProfileConcurrency);
        Assert.Null(admission.VisitorPermitLimit);
        Assert.Null(admission.VisitorWindow);
        Assert.Null(budget.ProfileAdmissionRemainingFloor);
        Assert.IsType<ProfileExecutionAdmissionGuard>(
            provider.GetRequiredService<IProfileExecutionAdmissionGuard>());
        Assert.IsType<ProfileGitHubBudgetProtection>(
            provider.GetRequiredService<IGitHubProfileBudgetProtection>());
    }

    [Fact]
    public void AddProfilePipeline_RejectsHalfConfiguredVisitorLimit()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileAdmissionOptions.ConfigurationSectionName}:VisitorPermitLimit"] =
                    "2"
            })
            .Build();
        using ServiceProvider provider = BuildProvider(configuration);

        Assert.Throws<OptionsValidationException>(() => provider
            .GetRequiredService<IOptions<ProfileAdmissionOptions>>()
            .Value);
    }

    [Fact]
    public void AddProfilePipeline_RejectsNegativeGitHubRemainingFloor()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileGitHubBudgetProtectionOptions.ConfigurationSectionName}:"
                    + nameof(ProfileGitHubBudgetProtectionOptions
                        .ProfileAdmissionRemainingFloor)] = "-1"
            })
            .Build();
        using ServiceProvider provider = BuildProvider(configuration);

        Assert.Throws<OptionsValidationException>(() => provider
            .GetRequiredService<IOptions<ProfileGitHubBudgetProtectionOptions>>()
            .Value);
    }

    [Fact]
    public void AddProfilePipeline_RejectsDeadlineThatDoesNotExceedTransportTimeout()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileExecutionDeadlineOptions.ConfigurationSectionName}:Timeout"] =
                    ProfilePipelineServiceCollectionExtensions.GitHubTransportTimeout
                        .ToString()
            })
            .Build();
        using ServiceProvider provider = BuildProvider(configuration);

        Assert.Throws<OptionsValidationException>(() => provider
            .GetRequiredService<IOptions<ProfileExecutionDeadlineOptions>>()
            .Value);
    }

    [Fact]
    public void ConfiguredDeadlineFactory_UsesTheOwnedTimerAndCancelsItsExecutionToken()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ProfileExecutionDeadlineOptions.ConfigurationSectionName}:Timeout"] =
                    "00:03:00"
            })
            .Build();
        ManualTimeProvider timeProvider = new();
        using ServiceProvider provider = BuildProvider(configuration, timeProvider);
        IProfileExecutionDeadlineFactory factory =
            provider.GetRequiredService<IProfileExecutionDeadlineFactory>();
        using IProfileExecutionDeadline deadline = factory.Create(
            CancellationToken.None);

        Assert.False(deadline.IsExpired);
        Assert.False(deadline.ExecutionToken.IsCancellationRequested);

        timeProvider.FireDeadline();

        Assert.True(deadline.IsExpired);
        Assert.True(deadline.ExecutionToken.IsCancellationRequested);
    }

    private static ServiceProvider BuildProvider(
        IConfiguration? configuration = null,
        TimeProvider? timeProvider = null)
    {
        ServiceCollection services = new();

        services.AddSingleton(timeProvider ?? TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();

        services.AddProfilePipeline(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }

    // Never invoked: these tests only prove that DI resolves the pipeline
    // graph, not that it can reach live GitHub.
    private sealed class NeverCalledSnapshotCollector : IGitHubRepositorySnapshotCollector
    {
        public Task<GitHubRepositorySnapshotResult> CollectAsync(
            string repositoryUrl,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "This collector is a DI-resolution placeholder and must never be invoked.");
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private ManualTimer? timer;

        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period)
        {
            timer = new ManualTimer(callback, state, dueTime, period);
            return timer;
        }

        public void FireDeadline() =>
            (timer ?? throw new InvalidOperationException("No timer was created."))
                .Fire();
    }

    private sealed class ManualTimer(
        TimerCallback callback,
        object? state,
        TimeSpan dueTime,
        TimeSpan period) : ITimer
    {
        private bool enabled = dueTime != Timeout.InfiniteTimeSpan;
        private bool disposed;

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            enabled = dueTime != Timeout.InfiniteTimeSpan;
            return true;
        }

        public void Fire()
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (enabled)
            {
                callback(state);
                enabled = period != Timeout.InfiniteTimeSpan;
            }
        }

        public void Dispose()
        {
            disposed = true;
            enabled = false;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
