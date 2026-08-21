using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
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

    private static ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();

        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddScoped<IGitHubRepositorySnapshotCollector, NeverCalledSnapshotCollector>();

        services.AddProfilePipeline();

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
}
