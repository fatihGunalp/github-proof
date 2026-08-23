using System.Net;
using System.Text;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class ProfileGitHubBudgetProtectionTests
{
    [Fact]
    public async Task Handler_RecordsOnlySanitizedMetadataBudgetHeaders()
    {
        DateTimeOffset resetAt =
            DateTimeOffset.Parse("2026-08-22T10:00:00Z");
        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Headers.Add("X-RateLimit-Remaining", "37");
        response.Headers.Add(
            "X-RateLimit-Reset",
            resetAt.ToUnixTimeSeconds().ToString());
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new FixedResponseHandler(response)
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            using HttpResponseMessage ignored = await client.GetAsync("users/octocat");
        }

        Assert.Equal(
            new ProfileGitHubBudgetObservation(37, resetAt),
            budgetState.MetadataObservation);
        Assert.DoesNotContain("37", telemetry.ToLogMessage(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    [InlineData("1,2")]
    public async Task Handler_IgnoresMalformedRemainingHeader(string value)
    {
        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", value);
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new FixedResponseHandler(response)
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            using HttpResponseMessage ignored = await client.GetAsync("users/octocat");
        }

        Assert.Null(budgetState.MetadataObservation);
    }

    [Fact]
    public async Task Handler_DoesNotUseRepositoryResponseAsMetadataBudgetState()
    {
        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Headers.Add("X-RateLimit-Remaining", "0");
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new FixedResponseHandler(response)
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            using HttpResponseMessage ignored =
                await client.GetAsync("users/octocat/repos");
        }

        Assert.Null(budgetState.MetadataObservation);
    }

    [Fact]
    public void Guard_RejectsAtOrBelowConfiguredFloor()
    {
        ProfileGitHubBudgetProtection guard = new(
            Options.Create(new ProfileGitHubBudgetProtectionOptions
            {
                ProfileAdmissionRemainingFloor = 40
            }));
        ProfileGitHubBudgetState budgetState = new();
        budgetState.RecordMetadataObservation(40, resetAt: null);

        using IDisposable scope = ProfileGitHubBudgetScope.Begin(budgetState);

        Assert.Throws<GitHubProfileBudgetProtectionRejectedException>(
            guard.EnsureCanContinueAfterAvailableMetadata);
    }

    [Fact]
    public void Guard_AllowsWhenFloorOrTrustworthyObservationIsAbsent()
    {
        ProfileGitHubBudgetProtection disabled = new(
            Options.Create(new ProfileGitHubBudgetProtectionOptions()));
        ProfileGitHubBudgetProtection unobserved = new(
            Options.Create(new ProfileGitHubBudgetProtectionOptions
            {
                ProfileAdmissionRemainingFloor = 40
            }));
        ProfileGitHubBudgetState budgetState = new();
        budgetState.RecordMetadataObservation(0, resetAt: null);

        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            disabled.EnsureCanContinueAfterAvailableMetadata();
        }

        unobserved.EnsureCanContinueAfterAvailableMetadata();
    }

    [Fact]
    public async Task RealOrchestrator_StopsAfterMetadataWithoutManifestRequest()
    {
        ProfileGitHubBudgetProtection guard = new(
            Options.Create(new ProfileGitHubBudgetProtectionOptions
            {
                ProfileAdmissionRemainingFloor = 50
            }));
        MetadataResponseHandler inner = new(remaining: 50);
        ProfileGitHubRequestTelemetryHandler telemetryHandler = new()
        {
            InnerHandler = inner
        };
        using HttpClient client = new(telemetryHandler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        GitHubProfileSnapshotOrchestrator orchestrator =
            GitHubProfileSnapshotOrchestrator.CreateDefault(
                client,
                TimeProvider.System,
                guard);
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();
        using GitHubProfileSnapshotProgress progress =
            new(CancellationToken.None);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        using (ProfileGitHubBudgetScope.Begin(budgetState))
        {
            await Assert.ThrowsAsync<GitHubProfileBudgetProtectionRejectedException>(
                () => orchestrator.CollectAsync(
                    "octocat",
                    CancellationToken.None,
                    progress));
        }

        Assert.Equal(1, inner.CallCount);
        Assert.True(progress.ProfileAvailability!.IsAvailable);
        Assert.Equal(50, budgetState.MetadataObservation!.Remaining);
    }

    private sealed class FixedResponseHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(response);
    }

    private sealed class MetadataResponseHandler(int remaining) : HttpMessageHandler
    {
        private const string ProfileJson = """
            {
              "login": "octocat",
              "type": "User",
              "name": "The Octocat",
              "created_at": "2011-01-25T18:44:36Z",
              "public_repos": 1,
              "avatar_url": "https://avatars.githubusercontent.com/u/1"
            }
            """;

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            if (CallCount > 1)
            {
                throw new InvalidOperationException(
                    "Budget rejection must prevent the repository-manifest request.");
            }

            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    ProfileJson,
                    Encoding.UTF8,
                    "application/json")
            };
            response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());

            return Task.FromResult(response);
        }
    }
}
