using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Profile;
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

        Assert.Equal(
            EligibilityPolicy.V1,
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

    [Fact]
    public void AddProfilePipeline_ResolvesTheProductionSnapshotSourceAdapter()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();

        IProfileRepositorySnapshotSource source =
            scope.ServiceProvider.GetRequiredService<IProfileRepositorySnapshotSource>();

        Assert.Equal(
            "GitHubProof.Infrastructure.GitHub.Profile.GitHubProfileRepositorySnapshotSource",
            source.GetType().FullName);
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
