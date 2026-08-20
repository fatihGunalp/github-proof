using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;

namespace GitHubProof.Web.Profile;

// Wiring only: this registers the existing P3-P8 production components behind
// ProfilePipeline. It makes no eligibility, composition, pattern-promotion,
// representative-selection or report decision of its own — every one of those
// stays owned by the Application/Domain types it resolves.
//
// Assumes the caller already registered TimeProvider, IGitHubRepositorySnapshotCollector
// and RepositoryAnalysisRunner (Program.cs does, for the existing single-repository
// analysis path this pipeline reuses).
public static class ProfilePipelineServiceCollectionExtensions
{
    public static IServiceCollection AddProfilePipeline(
        this IServiceCollection services)
    {
        services.AddSingleton(EligibilityPolicy.V1);
        services.AddSingleton(ObservationRuleVersion.V1);
        services.AddSingleton(PatternRuleVersion.V1);
        services.AddSingleton(RepresentativeRepositoryRuleVersion.V1);
        services.AddSingleton(ProfileAnalyzerVersion.V1);

        services.AddScoped<IRepositoryAnalysisRunner>(
            provider => provider.GetRequiredService<RepositoryAnalysisRunner>());

        services.AddScoped<IProfileRepositorySnapshotSource>(
            provider => new GitHubProfileRepositorySnapshotSource(
                provider.GetRequiredService<IGitHubRepositorySnapshotCollector>()));

        services.AddScoped(
            provider => new AnalysisAdmissionPolicy(
                provider.GetRequiredService<EligibilityPolicy>().Version));

        services.AddScoped(
            provider => new ProfileRepositoryAnalysisOrchestrator(
                provider.GetRequiredService<IProfileRepositorySnapshotSource>(),
                provider.GetRequiredService<IRepositoryAnalysisRunner>(),
                provider.GetRequiredService<AnalysisAdmissionPolicy>()));

        services.AddScoped(
            provider => new RepositoryEligibilityPolicy(
                provider.GetRequiredService<EligibilityPolicy>()));

        services.AddScoped(
            provider => new RepositoryContributionPolicy(
                provider.GetRequiredService<EligibilityPolicy>().Version));

        services.AddScoped(
            provider => new CrossRepositoryEvidenceComposer(
                provider.GetRequiredService<RepositoryContributionPolicy>()));

        services.AddScoped(
            provider => new ProfilePatternPromotionPolicy(
                provider.GetRequiredService<PatternRuleVersion>()));

        services.AddScoped(
            provider => new RepresentativeRepositorySelectionPolicy(
                provider.GetRequiredService<RepresentativeRepositoryRuleVersion>()));

        services.AddScoped<ProfileComposer>();

        services.AddScoped<ProfileReportProjector>();

        services.AddScoped(
            provider => new ProfilePipeline(
                provider.GetRequiredService<ProfileRepositoryAnalysisOrchestrator>(),
                provider.GetRequiredService<RepositoryEligibilityPolicy>(),
                provider.GetRequiredService<CrossRepositoryEvidenceComposer>(),
                provider.GetRequiredService<ProfilePatternPromotionPolicy>(),
                provider.GetRequiredService<RepresentativeRepositorySelectionPolicy>(),
                provider.GetRequiredService<ProfileComposer>(),
                provider.GetRequiredService<ProfileReportProjector>(),
                provider.GetRequiredService<ObservationRuleVersion>(),
                provider.GetRequiredService<ProfileAnalyzerVersion>(),
                provider.GetRequiredService<TimeProvider>()));

        return services;
    }
}
