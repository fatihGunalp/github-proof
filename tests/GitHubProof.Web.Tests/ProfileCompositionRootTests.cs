using GitHubProof.Application.Analysis;
using GitHubProof.Infrastructure.GitHub;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GitHubProof.Web.Tests;

// P10-I1: proves the composition-root registration Program.cs adds for
// IGitHubProfileSnapshotCollector resolves the real production Infrastructure
// type and neither replaces nor duplicates the pre-existing repository
// collector registration.
//
// Session 097 amends one part of that P10-I1 shape: the profile collector no
// longer shares the "GitHubProof.GitHub" client. It runs on the profile-specific
// instrumented client registered by AddProfilePipeline, which carries the same
// transport contract plus Profile-only request/budget observation. Repository Proof's
// registration, client and construction path are unchanged, and these tests
// assert that separation explicitly.
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

    // Session 097 supersedes the P10-I1 shared-client decision for the profile
    // path only: Profile Proof now runs on its own instrumented named client
    // (docs/architecture/public-profile-runtime-collection.md §9 and the
    // Session 097 request-counting boundary), so profile telemetry can never mix
    // in Repository Proof traffic. Repository Proof's own registration, named
    // client and construction path are unchanged.
    [Fact]
    public void ProgramSource_KeepsRepositoryProofOnItsOwnUnchangedNamedClient()
    {
        string content = ReadWebFile("Program.cs");

        Assert.Contains(
            "AddScoped<IGitHubRepositorySnapshotCollector>",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "GitHubRepositorySnapshotOrchestrator.CreateDefault",
            content,
            StringComparison.Ordinal);

        // The repository client is still registered once and still looked up
        // once, by the Repository Proof collector.
        Assert.Equal(2, CountOccurrences(content, "\"GitHubProof.GitHub\""));
    }

    [Fact]
    public void ProgramSource_RegistersTheProfileCollectorOnTheProfileGitHubClient()
    {
        string content = ReadWebFile("Program.cs");

        Assert.Contains(
            "AddScoped<IGitHubProfileSnapshotCollector>",
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

        // The profile collector resolves its client through the single profile
        // client factory helper, never through the repository client name.
        Assert.Contains(
            "ProfilePipelineServiceCollectionExtensions.CreateGitHubClient",
            content,
            StringComparison.Ordinal);
    }

    // The Repository Proof client must not carry the profile telemetry handler,
    // and the profile client must.
    [Fact]
    public void ProfileTelemetryHandler_IsRegisteredOnlyOnTheProfileClient()
    {
        string programContent = ReadWebFile("Program.cs");
        string profileWiring = ReadWebFile(
            "Profile",
            "ProfilePipelineServiceCollectionExtensions.cs");

        Assert.DoesNotContain(
            nameof(ProfileGitHubRequestTelemetryHandler),
            programContent,
            StringComparison.Ordinal);
        Assert.Contains(
            $"AddHttpMessageHandler<{nameof(ProfileGitHubRequestTelemetryHandler)}>",
            profileWiring,
            StringComparison.Ordinal);
    }

    // The wiring must build the profile path from the omitting composition, and
    // Repository Proof must keep the full one
    // (docs/architecture/public-profile-runtime-collection.md §3.1, §3.2, §14.1,
    // §14.2). The zero-request behavior of that composition is proven in
    // GitHubProof.Infrastructure.Tests against a real GitHub fixture.
    [Fact]
    public void ProfileWiring_BuildsTheOmittingCompositionAndLeavesRepositoryProofOnTheFullOne()
    {
        string programContent = ReadWebFile("Program.cs");
        string profileWiring = ReadWebFile(
            "Profile",
            "ProfilePipelineServiceCollectionExtensions.cs");

        Assert.Contains(
            "GitHubRepositorySnapshotOrchestrator.CreateProfileComposition",
            profileWiring,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "GitHubRepositorySnapshotOrchestrator.CreateDefault",
            profileWiring,
            StringComparison.Ordinal);

        Assert.Contains(
            "GitHubRepositorySnapshotOrchestrator.CreateDefault",
            programContent,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "GitHubRepositorySnapshotOrchestrator.CreateProfileComposition",
            programContent,
            StringComparison.Ordinal);
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
                    httpClientFactory.CreateClient(
                        ProfilePipelineServiceCollectionExtensions.GitHubHttpClientName),
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
