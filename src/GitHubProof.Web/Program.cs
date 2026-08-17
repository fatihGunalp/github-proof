using System.Net.Http.Headers;
using GitHubProof.Application.Analysis;
using GitHubProof.Application.Analysis.Presentation;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Web.Analysis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddSingleton<AnalysisVersion>(
    new AnalysisVersion(
        analyzerVersion: "1.0.0",
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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
