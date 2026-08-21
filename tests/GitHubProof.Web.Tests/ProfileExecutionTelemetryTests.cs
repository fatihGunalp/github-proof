using System.Net;
using System.Net.Http.Headers;
using GitHubProof.Domain.Analysis.Enums;
using GitHubProof.Web.Profile.Runtime;
using Xunit;

namespace GitHubProof.Web.Tests;

// docs/architecture/public-profile-runtime-collection.md §9: the minimum
// observable surface for one profile execution, and the prohibitions that keep
// it operational truth rather than evidence.
public sealed class ProfileExecutionTelemetryTests
{
    [Fact]
    public void RequestCategories_AreDerivedFromRouteShapeOnly()
    {
        Assert.Equal(
            ProfileGitHubRequestCategory.ProfileMetadata,
            Classify("https://api.github.com/users/octocat"));
        Assert.Equal(
            ProfileGitHubRequestCategory.ProfileRepositories,
            Classify("https://api.github.com/users/octocat/repos?per_page=100"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryMetadata,
            Classify("https://api.github.com/repos/octocat/hello-world"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryGitReference,
            Classify("https://api.github.com/repos/octocat/hello-world/git/ref/heads/main"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryGitCommit,
            Classify("https://api.github.com/repos/octocat/hello-world/git/commits/abc"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryGitTree,
            Classify("https://api.github.com/repos/octocat/hello-world/git/trees/abc?recursive=1"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryContents,
            Classify("https://api.github.com/repos/octocat/hello-world/contents/src/App.csproj"));
        Assert.Equal(
            ProfileGitHubRequestCategory.RepositoryCommits,
            Classify("https://api.github.com/repos/octocat/hello-world/commits?per_page=100"));
        Assert.Equal(
            ProfileGitHubRequestCategory.Other,
            Classify("https://api.github.com/rate_limit"));
    }

    // The vocabulary must be closed: no repository name, owner, login, path or
    // SHA may ever appear as a category key.
    [Fact]
    public void RequestCategories_NeverCarryAnIdentifier()
    {
        ProfileExecutionTelemetry telemetry = new();

        foreach (string uri in new[]
        {
            "https://api.github.com/users/some-secret-login",
            "https://api.github.com/repos/some-owner/some-private-name",
            "https://api.github.com/repos/o/r/contents/src/Secrets.cs",
            "https://api.github.com/repos/o/r/git/trees/deadbeefdeadbeef",
            "https://api.github.com/unknown/endpoint"
        })
        {
            telemetry.RecordGitHubRequest(
                ProfileGitHubRequestCategory.Classify(new Uri(uri)));
        }

        Assert.All(
            telemetry.RequestCategories.Keys,
            key => Assert.Contains(key, ProfileGitHubRequestCategory.All));
        Assert.Equal(5, telemetry.GitHubRequests);
    }

    [Fact]
    public void RecordGitHubRequest_FoldsAnUnknownCategoryIntoOther()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordGitHubRequest("octocat/hello-world");

        Assert.Equal(
            [ProfileGitHubRequestCategory.Other],
            telemetry.RequestCategories.Keys);
    }

    [Fact]
    public void RepositoriesIncomplete_IsAdmittedMinusCompleted()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoriesDiscovered(9);
        telemetry.RecordRepositoryAdmitted();
        telemetry.RecordRepositoryAdmitted();
        telemetry.RecordRepositoryAdmitted();
        telemetry.RecordRepositoryCompleted();

        Assert.Equal(9, telemetry.RepositoriesDiscovered);
        Assert.Equal(3, telemetry.RepositoriesAdmitted);
        Assert.Equal(1, telemetry.RepositoriesCompleted);
        Assert.Equal(2, telemetry.RepositoriesIncomplete);
    }

    // §9 requires rate-limit / forbidden / failed to stay distinguishable, not
    // merely counted as "incomplete".
    [Fact]
    public void RepositoryOutcomes_DistinguishRateLimitForbiddenAndFailed()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoryOutcome(
            ProfileRepositoryOutcome.FromAvailabilityStatus(
                DataAvailabilityStatus.RateLimited));
        telemetry.RecordRepositoryOutcome(
            ProfileRepositoryOutcome.FromAvailabilityStatus(
                DataAvailabilityStatus.RateLimited));
        telemetry.RecordRepositoryOutcome(
            ProfileRepositoryOutcome.FromAvailabilityStatus(
                DataAvailabilityStatus.Forbidden));
        telemetry.RecordRepositoryOutcome(
            ProfileRepositoryOutcome.FromAvailabilityStatus(
                DataAvailabilityStatus.Failed));
        telemetry.RecordRepositoryOutcome(ProfileRepositoryOutcome.AnalysisFailed);

        Assert.Equal(2, telemetry.RepositoryOutcomes["RateLimited"]);
        Assert.Equal(1, telemetry.RepositoryOutcomes["Forbidden"]);
        Assert.Equal(1, telemetry.RepositoryOutcomes["Failed"]);
        Assert.Equal(1, telemetry.RepositoryOutcomes[ProfileRepositoryOutcome.AnalysisFailed]);
    }

    [Fact]
    public void RepositoryOutcomes_UseAClosedVocabulary()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoryOutcome("octocat/hello-world rate limited");
        telemetry.RecordRepositoryOutcome(null!);

        Assert.Equal(
            [ProfileRepositoryOutcome.Other],
            telemetry.RepositoryOutcomes.Keys);
        Assert.All(
            telemetry.RepositoryOutcomes.Keys,
            key => Assert.Contains(key, ProfileRepositoryOutcome.All));
    }

    [Fact]
    public void LogMessage_CarriesEveryRequiredField()
    {
        ProfileExecutionTelemetry telemetry = new();

        telemetry.RecordRepositoriesDiscovered(4);
        telemetry.RecordRepositoryAdmitted();
        telemetry.RecordRepositoryCompleted();
        telemetry.RecordGitHubRequest(ProfileGitHubRequestCategory.RepositoryMetadata);
        telemetry.Complete(
            TimeSpan.FromMilliseconds(1234),
            "Complete",
            ProfileExecutionOutcome.Composed);

        string message = telemetry.ToLogMessage();

        Assert.Contains("DurationMs=1234", message, StringComparison.Ordinal);
        Assert.Contains("GitHubRequests=1", message, StringComparison.Ordinal);
        Assert.Contains("RepositoriesDiscovered=4", message, StringComparison.Ordinal);
        Assert.Contains("RepositoriesAdmitted=1", message, StringComparison.Ordinal);
        Assert.Contains("RepositoriesCompleted=1", message, StringComparison.Ordinal);
        Assert.Contains("RepositoriesIncomplete=0", message, StringComparison.Ordinal);
        Assert.Contains("Status=Complete", message, StringComparison.Ordinal);
        Assert.Contains("Outcome=composed", message, StringComparison.Ordinal);
        Assert.Contains(
            "RequestCategories={repository-metadata=1}",
            message,
            StringComparison.Ordinal);
        Assert.Contains("RepositoryOutcomes={}", message, StringComparison.Ordinal);
    }

    // §9.1: no token, no Authorization header, no source / README / manifest
    // content may ever reach telemetry.
    [Fact]
    public async Task Handler_RecordsTheRequestWithoutTouchingCredentialsOrContent()
    {
        const string token = "ghp_thisIsASecretTokenValue";
        const string body = "internal class Secret { const string Password = \"hunter2\"; }";

        ProfileExecutionTelemetry telemetry = new();
        RecordingInnerHandler inner = new(body);
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            using HttpResponseMessage response =
                await httpClient.GetAsync("repos/octocat/hello-world/contents/src/Secret.cs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        telemetry.Complete(TimeSpan.FromMilliseconds(5), "Complete", ProfileExecutionOutcome.Composed);

        string message = telemetry.ToLogMessage();

        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(
            [ProfileGitHubRequestCategory.RepositoryContents],
            telemetry.RequestCategories.Keys);
        Assert.DoesNotContain(token, message, StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization", message, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer", message, StringComparison.Ordinal);
        Assert.DoesNotContain("hunter2", message, StringComparison.Ordinal);
        Assert.DoesNotContain("Secret.cs", message, StringComparison.Ordinal);

        // The transport contract is untouched: the credential still reached
        // GitHub exactly as configured.
        Assert.Equal($"Bearer {token}", inner.ObservedAuthorization);
    }

    // Outside a profile execution the handler is a no-op, which is what keeps
    // Repository Proof traffic out of profile telemetry even if a client were
    // ever shared.
    [Fact]
    public async Task Handler_RecordsNothingOutsideAProfileExecution()
    {
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new RecordingInnerHandler("{}")
        };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using HttpResponseMessage response =
            await httpClient.GetAsync("repos/octocat/hello-world");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, telemetry.GitHubRequests);
        Assert.Null(ProfileExecutionTelemetryScope.Current);
    }

    // §9 / §D.4: two concurrent executions must never mix counters. The ambient
    // scope is per async flow, not per pooled handler instance.
    [Fact]
    public async Task ConcurrentExecutions_DoNotShareCounters()
    {
        ProfileExecutionTelemetry first = new();
        ProfileExecutionTelemetry second = new();

        await Task.WhenAll(
            RecordAsync(first, 3),
            RecordAsync(second, 7));

        Assert.Equal(3, first.GitHubRequests);
        Assert.Equal(7, second.GitHubRequests);
        Assert.Null(ProfileExecutionTelemetryScope.Current);
    }

    private static async Task RecordAsync(
        ProfileExecutionTelemetry telemetry,
        int requestCount)
    {
        await Task.Yield();

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            for (int index = 0; index < requestCount; index++)
            {
                await Task.Yield();

                ProfileExecutionTelemetryScope.Current!.RecordGitHubRequest(
                    ProfileGitHubRequestCategory.RepositoryMetadata);
            }
        }
    }

    private static string Classify(string uri) =>
        ProfileGitHubRequestCategory.Classify(new Uri(uri));

    private sealed class RecordingInnerHandler(string body) : HttpMessageHandler
    {
        public string? ObservedAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ObservedAuthorization = request.Headers.Authorization?.ToString();

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body)
                });
        }
    }
}
