using System.Net.Http.Headers;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Localization;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Progress;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddWebExperienceLocalization();

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

// Process-local, ephemeral live analysis progress registry
// (docs/architecture/live-analysis-progress-runtime-contract.md §12). Singleton
// by contract: one registry per process, not persisted, not shared between
// instances. Registry pressure degrades only the progress enhancement.
builder.Services.AddSingleton<AnalysisProgressRegistry>();

builder.Services.AddSingleton<AnalysisVersion>(
    // Session 099 / Session 098 contract: AnalyzerVersion owns snapshot
    // normalization (CollectionObservationState) and evidence values;
    // RuleSetVersion owns scoring/evaluator semantics, and the two commit
    // signals' terminal-decision gate changed. The report schema is unchanged.
    new AnalysisVersion(
        analyzerVersion: "1.2.0",
        ruleSetVersion: "2026.08.21",
        reportSchemaVersion: 1));

builder.Services.AddScoped<RepositoryAnalysisRunner>();

builder.Services.AddSingleton<RepositoryReportProjector>(
    _ => new RepositoryReportProjector());

builder.Services.AddHttpClient(
    "GitHubProof.GitHub",
    httpClient =>
    {
        httpClient.BaseAddress =
            new Uri("https://api.github.com/");
        httpClient.Timeout =
            TimeSpan.FromMinutes(2);

        string? token =
            Environment.GetEnvironmentVariable(
                "GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token.Trim());
        }
    });

builder.Services.AddScoped<IGitHubRepositorySnapshotCollector>(
    services =>
    {
        IHttpClientFactory httpClientFactory =
            services.GetRequiredService<IHttpClientFactory>();

        HttpClient httpClient =
            httpClientFactory.CreateClient(
                "GitHubProof.GitHub");

        TimeProvider timeProvider =
            services.GetRequiredService<TimeProvider>();

        return GitHubRepositorySnapshotOrchestrator.CreateDefault(
            httpClient,
            timeProvider);
    });

builder.Services.AddScoped<LiveRepositoryReportService>();

builder.Services.AddProfilePipeline(builder.Configuration);

// Profile Proof runs on its own named GitHub client, registered by
// AddProfilePipeline. It carries the same transport contract as the Repository
// Proof client above (same base address, timeout and optional bearer token) plus
// the Profile-only request/budget observation handler, so Profile operational
// state can never mix in or protect Repository Proof traffic
// (docs/architecture/public-profile-runtime-collection.md §§9-10).
builder.Services.AddScoped<IGitHubProfileSnapshotCollector>(
    services =>
    {
        HttpClient httpClient =
            ProfilePipelineServiceCollectionExtensions.CreateGitHubClient(
                services);

        TimeProvider timeProvider =
            services.GetRequiredService<TimeProvider>();

        return GitHubProfileSnapshotOrchestrator.CreateDefault(
            httpClient,
            timeProvider,
            services.GetRequiredService<IGitHubProfileBudgetProtection>());
    });

builder.Services.AddScoped<LiveProfileReportService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWebExperienceLocalization();

app.UseRouting();

app.MapRazorPages();

app.Run();
