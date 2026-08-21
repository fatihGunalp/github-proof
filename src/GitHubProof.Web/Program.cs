using System.Net.Http.Headers;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Analysis;
using GitHubProof.Web.Profile;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddSingleton<AnalysisVersion>(
    new AnalysisVersion(
        analyzerVersion: "1.1.0",
        ruleSetVersion: "2026.08.15",
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

builder.Services.AddProfilePipeline();

// Profile Proof runs on its own named GitHub client, registered by
// AddProfilePipeline. It carries the same transport contract as the Repository
// Proof client above (same base address, timeout and optional bearer token) plus
// the profile request-counting handler, so profile telemetry can never mix in
// Repository Proof traffic
// (docs/architecture/public-profile-runtime-collection.md §9).
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
            timeProvider);
    });

builder.Services.AddScoped<LiveProfileReportService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
