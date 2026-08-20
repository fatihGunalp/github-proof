using GitHubProof.Application.Analysis;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I1: proves the composition-root registration Program.cs adds for
// IGitHubProfileSnapshotCollector follows the exact pattern already used for
// IGitHubRepositorySnapshotCollector (same "GitHubProof.GitHub" HttpClient,
// same reused TimeProvider, GitHubProfileSnapshotOrchestrator.CreateDefault),
// resolves the real production Infrastructure type, and neither replaces nor
// duplicates the pre-existing repository collector registration.
public sealed class ProfileCompositionRootTests
{
    [Fact]
    public void ProfileCollectorRegistration_ResolvesTheProductionOrchestratorType()
    {
        using ServiceProvider provider = BuildProviderWithBothCollectorRegistrations();
        using IServiceScope scope = provider.CreateScope();

        IGitHubProfileSnapshotCollector collector =
            scope.ServiceProvider.GetRequiredService<IGitHubProfileSnapshotCollector>();

        Assert.Equal(
            typeof(GitHubProfileSnapshotOrchestrator),
            collector.GetType());
    }

    [Fact]
    public void ProfileCollectorRegistration_DoesNotReplaceTheRepositoryCollectorRegistration()
    {
        using ServiceProvider provider = BuildProviderWithBothCollectorRegistrations();
        using IServiceScope scope = provider.CreateScope();

        IGitHubRepositorySnapshotCollector repositoryCollector =
            scope.ServiceProvider.GetRequiredService<IGitHubRepositorySnapshotCollector>();
        IGitHubProfileSnapshotCollector profileCollector =
            scope.ServiceProvider.GetRequiredService<IGitHubProfileSnapshotCollector>();

        Assert.Equal(
            typeof(GitHubRepositorySnapshotOrchestrator),
            repositoryCollector.GetType());
        Assert.Equal(
            typeof(GitHubProfileSnapshotOrchestrator),
            profileCollector.GetType());
    }

    [Fact]
    public void LiveProfileReportServiceRegistration_ResolvesFromTheRealComposition()
    {
        using ServiceProvider provider = BuildProviderWithBothCollectorRegistrations();
        using IServiceScope scope = provider.CreateScope();

        LiveProfileReportService service =
            scope.ServiceProvider.GetRequiredService<LiveProfileReportService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void ProgramSource_RegistersTheProfileCollectorFollowingTheRepositoryCollectorPattern()
    {
        string content = ReadWebFile("Program.cs");

        Assert.Contains(
            "AddScoped<IGitHubRepositorySnapshotCollector>",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "AddScoped<IGitHubProfileSnapshotCollector>",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "GitHubRepositorySnapshotOrchestrator.CreateDefault",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "GitHubProfileSnapshotOrchestrator.CreateDefault",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "AddScoped<LiveProfileReportService>",
            content,
            StringComparison.Ordinal);

        int namedClientReferences = CountOccurrences(
            content,
            "\"GitHubProof.GitHub\"");

        // One AddHttpClient registration plus one CreateClient lookup per
        // collector (repository, profile) = at least three references to the
        // same named client.
        Assert.True(
            namedClientReferences >= 3,
            "Both collector registrations must reuse the same named "
            + "\"GitHubProof.GitHub\" HttpClient.");
    }

    private static int CountOccurrences(string content, string value)
    {
        int count = 0;
        int index = 0;

        while ((index = content.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static ServiceProvider BuildProviderWithBothCollectorRegistrations()
    {
        ServiceCollection services = new();

        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();

        services.AddHttpClient(
            "GitHubProof.GitHub",
            httpClient => httpClient.BaseAddress = new Uri("https://api.github.com/"));

        services.AddScoped<IGitHubRepositorySnapshotCollector>(
            provider =>
            {
                IHttpClientFactory httpClientFactory =
                    provider.GetRequiredService<IHttpClientFactory>();

                return GitHubRepositorySnapshotOrchestrator.CreateDefault(
                    httpClientFactory.CreateClient("GitHubProof.GitHub"),
                    provider.GetRequiredService<TimeProvider>());
            });

        services.AddProfilePipeline();

        services.AddScoped<IGitHubProfileSnapshotCollector>(
            provider =>
            {
                IHttpClientFactory httpClientFactory =
                    provider.GetRequiredService<IHttpClientFactory>();

                return GitHubProfileSnapshotOrchestrator.CreateDefault(
                    httpClientFactory.CreateClient("GitHubProof.GitHub"),
                    provider.GetRequiredService<TimeProvider>());
            });

        services.AddScoped<LiveProfileReportService>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static string ReadWebFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "GitHubProof.slnx");

            if (File.Exists(solutionPath))
            {
                string path = Path.Combine(
                    new[] { directory.FullName, "src", "GitHubProof.Web" }
                        .Concat(relativeSegments)
                        .ToArray());

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"Expected Web file was not found: {path}");
                }

                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root containing GitHubProof.slnx.");
    }
}
