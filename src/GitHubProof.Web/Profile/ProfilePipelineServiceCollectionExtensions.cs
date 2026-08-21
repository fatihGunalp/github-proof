using System.Net.Http.Headers;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile.Runtime;

namespace GitHubProof.Web.Profile;

// Wiring only: this registers the existing P3-P8 production components behind
// ProfilePipeline. It makes no eligibility, composition, pattern-promotion,
// representative-selection or report decision of its own — every one of those
// stays owned by the Application/Domain types it resolves.
//
// Assumes the caller already registered TimeProvider and RepositoryAnalysisRunner
// (Program.cs does, for the existing single-repository analysis path).
//
// Session 097: the profile path no longer resolves the caller's
// IGitHubRepositorySnapshotCollector. It owns its own GitHub client and its own
// repository snapshot composition
// (docs/architecture/public-profile-runtime-collection.md §3.2), so Repository
// Proof's registration, client and collection behavior are untouched.
public static class ProfilePipelineServiceCollectionExtensions
{
    // A profile-specific named client with the same GitHub transport contract as
    // the Repository Proof client (same BaseAddress, same authentication
    // behavior, same timeout) plus the request-counting handler. Keeping it
    // separate is what guarantees Repository Proof traffic is never counted into
    // profile telemetry (§9, §E of the Session 097 task).
    public const string GitHubHttpClientName = "GitHubProof.GitHub.Profile";

    public static IServiceCollection AddProfilePipeline(
        this IServiceCollection services)
    {
        services.AddTransient<ProfileGitHubRequestTelemetryHandler>();

        services
            .AddHttpClient(GitHubHttpClientName, ConfigureGitHubClient)
            .AddHttpMessageHandler<ProfileGitHubRequestTelemetryHandler>();

        services.AddSingleton(EligibilityPolicy.V1);
        services.AddSingleton(ObservationRuleVersion.V1);
        services.AddSingleton(PatternRuleVersion.V1);
        services.AddSingleton(RepresentativeRepositoryRuleVersion.V1);
        services.AddSingleton(ProfileAnalyzerVersion.V1);

        // Observation-only wrappers around the exact production components.
        // They record the §9 repository counters and change no decision, so
        // ProfileRepositoryAnalysisOrchestrator stays unchanged and admission is
        // never recomputed in Web.
        services.AddScoped<IRepositoryAnalysisRunner>(
            provider => new TelemetryRepositoryAnalysisRunner(
                provider.GetRequiredService<RepositoryAnalysisRunner>()));

        // The Profile V1 collection composition: the same repository snapshot
        // assembly with the two non-scoring enrichment collectors omitted, so no
        // source-tree, source-content or dependency-manifest request is issued
        // on this path (§5).
        services.AddScoped<IProfileRepositorySnapshotSource>(
            provider => new TelemetryProfileRepositorySnapshotSource(
                new GitHubProfileRepositorySnapshotSource(
                    GitHubRepositorySnapshotOrchestrator.CreateProfileComposition(
                        CreateGitHubClient(provider),
                        provider.GetRequiredService<TimeProvider>()))));

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

    // The one place the profile GitHub client is created, so every profile
    // request — profile metadata, repository manifest and per-repository
    // collection alike — passes the counting handler exactly once.
    public static HttpClient CreateGitHubClient(IServiceProvider provider) =>
        provider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(GitHubHttpClientName);

    // Byte-identical to the Repository Proof client configuration in Program.cs:
    // same base address, same timeout, same optional bearer token. The token is
    // read here and never copied anywhere else — it is not logged, not stored
    // and never reaches telemetry.
    public static void ConfigureGitHubClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        httpClient.BaseAddress = new Uri("https://api.github.com/");
        httpClient.Timeout = TimeSpan.FromMinutes(2);

        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Trim());
        }
    }
}
