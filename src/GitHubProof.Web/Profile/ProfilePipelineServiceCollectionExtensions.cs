using System.Net.Http.Headers;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile;
using GitHubProof.Domain.Profile.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Options;

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
    // behavior, same timeout) plus the Profile-only request/budget observation
    // handler. Keeping it separate guarantees Repository Proof traffic never
    // enters Profile telemetry or provider protection (§9-§10).
    public const string GitHubHttpClientName = "GitHubProof.GitHub.Profile";

    // Existing transport timeout, not a newly selected execution deadline.
    // Any configured end-to-end deadline must be deliberately greater so the
    // two cancellation sources remain attributable.
    public static readonly TimeSpan GitHubTransportTimeout = TimeSpan.FromMinutes(2);

    public static IServiceCollection AddProfilePipeline(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        OptionsBuilder<ProfileExecutionDeadlineOptions> deadlineOptions =
            services.AddOptions<ProfileExecutionDeadlineOptions>();

        if (configuration is not null)
        {
            deadlineOptions.Bind(
                configuration.GetSection(
                    ProfileExecutionDeadlineOptions.ConfigurationSectionName));
        }

        deadlineOptions
            .Validate(
                options => options.Timeout is null
                    || options.Timeout > GitHubTransportTimeout,
                "A configured Profile execution deadline must be greater than "
                + "the Profile GitHub transport timeout.")
            .ValidateOnStart();

        OptionsBuilder<ProfileAdmissionOptions> admissionOptions =
            services.AddOptions<ProfileAdmissionOptions>();

        if (configuration is not null)
        {
            admissionOptions.Bind(
                configuration.GetSection(
                    ProfileAdmissionOptions.ConfigurationSectionName));
        }

        admissionOptions
            .Validate(
                options => options.GlobalConcurrency > 0,
                "Profile global concurrency must be greater than zero.")
            .Validate(
                options => options.SameProfileConcurrency > 0,
                "Profile same-profile concurrency must be greater than zero.")
            .Validate(
                options => (options.VisitorPermitLimit is null
                        && options.VisitorWindow is null)
                    || (options.VisitorPermitLimit > 0
                        && options.VisitorWindow > TimeSpan.Zero),
                "Profile visitor permit limit and window must either both be "
                + "unset or both be greater than zero.")
            .ValidateOnStart();

        OptionsBuilder<ProfileGitHubBudgetProtectionOptions> budgetOptions =
            services.AddOptions<ProfileGitHubBudgetProtectionOptions>();

        if (configuration is not null)
        {
            budgetOptions.Bind(
                configuration.GetSection(
                    ProfileGitHubBudgetProtectionOptions.ConfigurationSectionName));
        }

        budgetOptions
            .Validate(
                options => options.ProfileAdmissionRemainingFloor is null
                    || options.ProfileAdmissionRemainingFloor >= 0,
                "The Profile GitHub remaining floor must be non-negative.")
            .ValidateOnStart();

        services.AddSingleton<
            IProfileExecutionDeadlineFactory,
            ProfileExecutionDeadlineFactory>();

        services.AddSingleton<
            IProfileExecutionAdmissionGuard,
            ProfileExecutionAdmissionGuard>();

        services.AddSingleton<
            IGitHubProfileBudgetProtection,
            ProfileGitHubBudgetProtection>();

        services.AddTransient<ProfileGitHubRequestTelemetryHandler>();

        services
            .AddHttpClient(GitHubHttpClientName, ConfigureGitHubClient)
            .AddHttpMessageHandler<ProfileGitHubRequestTelemetryHandler>();

        // Session 099: the production profile path runs on the eligibility
        // policy version that carries the Fork Policy B admission rule. V1 stays
        // immutable and is still the historical definition.
        services.AddSingleton(EligibilityPolicy.V2);
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
        // assembly with three surfaces omitted — SourceFiles,
        // DependencyDeclarations and (Session 099) Contributors — so no
        // source-tree, source-content, dependency-manifest or contributor
        // request is issued on this path (§5). The five paginated surfaces also
        // carry their canonical decision boundaries (§15).
        services.AddScoped<IProfileRepositorySnapshotSource>(
            provider => new TelemetryProfileRepositorySnapshotSource(
                new GitHubProfileRepositorySnapshotSource(
                    GitHubRepositorySnapshotOrchestrator.CreateProfileComposition(
                        CreateGitHubClient(provider),
                        provider.GetRequiredService<TimeProvider>()))));

        // Session 124 (S1). The Profile-only GraphQL transport is registered and
        // resolvable, and NOTHING in the production collection path resolves it:
        // the snapshot source above still composes the exact REST collector set,
        // and no collector takes this dependency. It shares the existing profile
        // named client, so a GraphQL request would pass through the same §9
        // observation handler exactly once, and it introduces no second client,
        // no credential source and no package
        // (docs/architecture/public-profile-runtime-collection.md §§18-19).
        services.AddScoped<IGitHubGraphQlTransport>(
            provider => new GitHubGraphQlTransport(CreateGitHubClient(provider)));

        // Constructed from the policy — the only construction input — so Stage A
        // reads the fork admission rule the version owns and cannot be paired
        // with a rule that version does not carry.
        services.AddScoped(
            provider => new AnalysisAdmissionPolicy(
                provider.GetRequiredService<EligibilityPolicy>()));

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
    // collection alike — passes the Profile-only observation handler exactly
    // once.
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
        httpClient.Timeout = GitHubTransportTimeout;

        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Trim());
        }
    }
}
