using System.Net;
using System.Net.Http.Headers;
using GitHubProof.Web.Profile.Runtime;
using Xunit;

namespace GitHubProof.Web.Tests;

// Session 120 — B-RI: rate-limit telemetry interpretation.
//
// Three live benchmarks produced a first-minus-last remaining delta that ranged
// from 0.98x to 13.48x the measured request count. The old telemetry could not
// say why, because it flattened every observation into one sequence with no
// resource and no reset window. GitHub's rate-limit model carries both.
//
// These guards prove the observation now preserves enough anonymous context to
// tell buckets apart — and nothing more. Nothing here decides anything: the
// proactive budget guard is deliberately untouched, and whether its premise is
// suitable remains the open D-2 question.
public sealed class ProfileRateLimitObservationTests
{
    // ------------------------------------------------- 1-3. context is observed

    [Fact]
    public async Task Observation_CapturesResourceLimitRemainingUsedAndResetWindow()
    {
        DateTimeOffset resetAt = DateTimeOffset.Parse("2026-08-25T11:00:00Z");
        ProfileExecutionTelemetry telemetry = new();

        await SendAsync(
            telemetry,
            "users/octocat",
            resource: "core",
            remaining: "4900",
            limit: "5000",
            used: "100",
            reset: resetAt.ToUnixTimeSeconds().ToString());

        ProfileRateLimitWindow window = Assert.Single(telemetry.RateLimitWindows);

        Assert.Equal("core", window.Resource);
        Assert.Equal(resetAt, window.ResetAt);
        Assert.Equal(5000, window.Limit);
        Assert.Equal(4900, window.FirstRemaining);
        Assert.Equal(4900, window.LastRemaining);
        Assert.Equal(4900, window.MinimumRemaining);
        Assert.Equal(100, window.FirstUsed);
        Assert.Equal(100, window.LastUsed);
        Assert.Equal(1, window.SampleCount);
    }

    [Fact]
    public async Task Observation_TracksUsedAcrossASingleWindow()
    {
        DateTimeOffset resetAt = DateTimeOffset.Parse("2026-08-25T11:00:00Z");
        ProfileExecutionTelemetry telemetry = new();
        string reset = resetAt.ToUnixTimeSeconds().ToString();

        await SendAsync(telemetry, "users/octocat", "core", "4900", "5000", "100", reset);
        await SendAsync(telemetry, "repos/o/r", "core", "4700", "5000", "300", reset);
        await SendAsync(telemetry, "repos/o/r/commits", "core", "4800", "5000", "200", reset);

        ProfileRateLimitWindow window = Assert.Single(telemetry.RateLimitWindows);

        Assert.Equal(4900, window.FirstRemaining);
        Assert.Equal(4800, window.LastRemaining);
        Assert.Equal(4700, window.MinimumRemaining);
        Assert.Equal(100, window.FirstUsed);
        Assert.Equal(200, window.LastUsed);
        Assert.Equal(3, window.SampleCount);
    }

    // ----------------------------------------- 5. resources are never merged

    [Fact]
    public async Task Observation_KeepsDifferentResourcesApart()
    {
        string reset = DateTimeOffset.Parse("2026-08-25T11:00:00Z")
            .ToUnixTimeSeconds().ToString();
        ProfileExecutionTelemetry telemetry = new();

        await SendAsync(telemetry, "users/octocat", "core", "4900", "5000", "100", reset);
        await SendAsync(telemetry, "repos/o/r", "search", "20", "30", "10", reset);
        await SendAsync(telemetry, "repos/o/r/commits", "core", "4800", "5000", "200", reset);

        Assert.Equal(2, telemetry.RateLimitWindows.Count);

        ProfileRateLimitWindow core = Window(telemetry, "core");
        ProfileRateLimitWindow search = Window(telemetry, "search");

        // The search sample must not have dragged core's minimum down to 20 —
        // that flattening is exactly what B-RI removes.
        Assert.Equal(4800, core.MinimumRemaining);
        Assert.Equal(2, core.SampleCount);
        Assert.Equal(20, search.MinimumRemaining);
        Assert.Equal(1, search.SampleCount);
        Assert.Equal(30, search.Limit);
    }

    // ------------------------------------------- 6. windows are never merged

    [Fact]
    public async Task Observation_KeepsDifferentResetWindowsApart()
    {
        string firstWindow = DateTimeOffset.Parse("2026-08-25T11:00:00Z")
            .ToUnixTimeSeconds().ToString();
        string secondWindow = DateTimeOffset.Parse("2026-08-25T12:00:00Z")
            .ToUnixTimeSeconds().ToString();
        ProfileExecutionTelemetry telemetry = new();

        await SendAsync(telemetry, "repos/o/r", "core", "60", "5000", "4940", firstWindow);
        await SendAsync(telemetry, "repos/o/r/commits", "core", "4990", "5000", "10", secondWindow);

        Assert.Equal(2, telemetry.RateLimitWindows.Count);

        // Without window separation this pair would report first=60, last=4990
        // as if remaining had risen inside one budget. It did not: the window
        // reset between them.
        ProfileRateLimitWindow[] ordered = [.. telemetry.RateLimitWindows];

        Assert.All(ordered, window => Assert.Equal("core", window.Resource));
        Assert.Equal(60, ordered[0].LastRemaining);
        Assert.Equal(4990, ordered[1].LastRemaining);
        Assert.All(ordered, window => Assert.Equal(1, window.SampleCount));
    }

    // A response with no reset header is its own bucket, never folded into a
    // dated one.
    [Fact]
    public async Task Observation_KeepsAnUndatedWindowApartFromADatedOne()
    {
        string reset = DateTimeOffset.Parse("2026-08-25T11:00:00Z")
            .ToUnixTimeSeconds().ToString();
        ProfileExecutionTelemetry telemetry = new();

        await SendAsync(telemetry, "repos/o/r", "core", "4900", null, null, reset);
        await SendAsync(telemetry, "repos/o/r/commits", "core", "4800", null, null, null);

        Assert.Equal(2, telemetry.RateLimitWindows.Count);
        Assert.Single(telemetry.RateLimitWindows, window => window.ResetAt is null);
    }

    // ------------------------------- 4. malformed / missing fields are inert

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("not-a-number", "not-a-number", "not-a-number")]
    [InlineData("-1", "-1", "-1")]
    [InlineData("9999999999999999999999", "9999999999999999999999", "9999999999999999999999")]
    [InlineData("", "", "")]
    public async Task Observation_SurvivesMalformedContextWithoutAffectingExecution(
        string? limit,
        string? used,
        string? reset)
    {
        ProfileExecutionTelemetry telemetry = new();

        HttpStatusCode status = await SendAsync(
            telemetry,
            "users/octocat",
            resource: "core",
            remaining: "4900",
            limit: limit,
            used: used,
            reset: reset);

        ProfileRateLimitWindow window = Assert.Single(telemetry.RateLimitWindows);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(1, telemetry.GitHubRequests);
        Assert.Equal(4900, window.FirstRemaining);
        Assert.Equal("core", window.Resource);
    }

    // A malformed remaining still discards the whole observation, exactly as
    // before Session 120 Step 7.
    [Theory]
    [InlineData(null)]
    [InlineData("not-a-number")]
    [InlineData("-1")]
    public async Task Observation_WithoutAWellFormedRemainingIsNotRecorded(string? remaining)
    {
        ProfileExecutionTelemetry telemetry = new();

        await SendAsync(telemetry, "users/octocat", "core", remaining, "5000", "1", null);

        Assert.Empty(telemetry.RateLimitWindows);
        Assert.Equal(1, telemetry.GitHubRequests);
    }

    // ------------------------------------------------ resource normalization

    [Theory]
    [InlineData(null, ProfileRateLimitResource.Unspecified)]
    [InlineData("", ProfileRateLimitResource.Unspecified)]
    [InlineData("core", "core")]
    [InlineData("search", "search")]
    [InlineData("integration_manifest", "integration_manifest")]
    [InlineData("CORE", ProfileRateLimitResource.Unrecognized)]
    [InlineData("core-2", ProfileRateLimitResource.Unrecognized)]
    [InlineData("octocat/hello-world", ProfileRateLimitResource.Unrecognized)]
    [InlineData("a/b", ProfileRateLimitResource.Unrecognized)]
    [InlineData("core 2", ProfileRateLimitResource.Unrecognized)]
    // This is a SHAPE filter, not a secret detector: any short lowercase
    // identifier is echoed verbatim, which is what keeps telemetry cardinality
    // bounded without asserting a list of GitHub resource names. The value is a
    // provider-chosen header and is never derived from our request.
    [InlineData("some_other_bucket", "some_other_bucket")]
    public void ResourceNormalization_KeepsTheValueBounded(
        string? resource,
        string expected) =>
        Assert.Equal(expected, ProfileRateLimitResource.Normalize(resource));

    [Fact]
    public void ResourceNormalization_RejectsAnOverlongValue() =>
        Assert.Equal(
            ProfileRateLimitResource.Unrecognized,
            ProfileRateLimitResource.Normalize(new string('a', 33)));

    // A surprising provider could otherwise grow the bucket dictionary without
    // bound. Overflow is COUNTED, never folded into a recorded bucket, so no
    // recorded sequence silently absorbs a foreign window.
    [Fact]
    public void Observation_CapsDistinctBucketsAndCountsTheOverflow()
    {
        ProfileExecutionTelemetry telemetry = new();
        DateTimeOffset baseReset = DateTimeOffset.Parse("2026-08-25T11:00:00Z");

        for (int index = 0; index < 40; index++)
        {
            telemetry.RecordRateLimitObservation(
                "core",
                remaining: 100 + index,
                limit: 5000,
                used: index,
                resetAt: baseReset.AddHours(index));
        }

        Assert.Equal(16, telemetry.RateLimitWindows.Count);
        Assert.Equal(24, telemetry.UnrecordedRateLimitObservations);
        Assert.All(
            telemetry.RateLimitWindows,
            window => Assert.Equal(1, window.SampleCount));
    }

    // The overflow counter counts OBSERVATIONS, not distinct buckets. One
    // over-cap bucket seen many times contributes many, and — the part that
    // matters — none of them lands on a recorded bucket.
    [Fact]
    public void Observation_OverflowCountsEverySampleAndContaminatesNoRecordedBucket()
    {
        ProfileExecutionTelemetry telemetry = new();
        DateTimeOffset baseReset = DateTimeOffset.Parse("2026-08-25T11:00:00Z");

        for (int index = 0; index < 16; index++)
        {
            telemetry.RecordRateLimitObservation(
                "core", 5000, 5000, 0, baseReset.AddHours(index));
        }

        // One bucket beyond the cap, observed fifty times with a very low
        // remaining that would be visible if it leaked anywhere.
        for (int sample = 0; sample < 50; sample++)
        {
            telemetry.RecordRateLimitObservation(
                "core", 1, 5000, 4999, baseReset.AddHours(99));
        }

        Assert.Equal(16, telemetry.RateLimitWindows.Count);
        Assert.Equal(50, telemetry.UnrecordedRateLimitObservations);
        Assert.All(
            telemetry.RateLimitWindows,
            window =>
            {
                Assert.Equal(1, window.SampleCount);
                Assert.Equal(5000, window.MinimumRemaining);
            });
    }

    // ------------------------------------ 8. the budget decision is untouched

    [Fact]
    public async Task BudgetDecision_StaysMetadataOnlyAndUnchanged()
    {
        DateTimeOffset resetAt = DateTimeOffset.Parse("2026-08-25T11:00:00Z");
        string reset = resetAt.ToUnixTimeSeconds().ToString();
        ProfileExecutionTelemetry telemetry = new();
        ProfileGitHubBudgetState budgetState = new();

        // A repository response with rich rate-limit context must still not
        // reach the budget carrier.
        await SendAsync(
            telemetry, "repos/o/r/commits", "core", "11", "5000", "4989", reset, budgetState);

        Assert.Null(budgetState.MetadataObservation);

        await SendAsync(
            telemetry, "users/octocat", "core", "12", "5000", "4988", reset, budgetState);

        // Still exactly the Session 109 pair: remaining + reset. The enriched
        // observation did not enrich the decision.
        Assert.Equal(
            new ProfileGitHubBudgetObservation(12, resetAt),
            budgetState.MetadataObservation);
    }

    // ------------------------------------------------------- 7. privacy

    [Fact]
    public async Task Telemetry_CarriesNoIdentityCredentialOrProviderTimestamp()
    {
        const string token = "ghp_thisIsASecretTokenValue";
        DateTimeOffset resetAt = DateTimeOffset.Parse("2026-08-25T11:00:00Z");
        string reset = resetAt.ToUnixTimeSeconds().ToString();
        ProfileExecutionTelemetry telemetry = new();

        RecordingHandler inner = new(
            resource: "core",
            remaining: "4900",
            limit: "5000",
            used: "100",
            reset: reset);
        ProfileGitHubRequestTelemetryHandler handler = new() { InnerHandler = inner };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using (ProfileExecutionTelemetryScope.Begin(telemetry))
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                "repos/some-owner/some-private-name/contents/src/Secret.cs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        telemetry.Complete(
            TimeSpan.FromMilliseconds(5),
            "Complete",
            ProfileExecutionOutcome.Composed);

        string message = telemetry.ToLogMessage();

        foreach (string forbidden in new[]
        {
            token,
            "Bearer",
            "Authorization",
            "some-owner",
            "some-private-name",
            "Secret.cs",
            "https://",
            "api.github.com",
            reset,               // the provider's reset instant stays out of the log
                                 // even though it is kept on the typed window
            "2026-08-25",
            "Reset"
        })
        {
            Assert.DoesNotContain(forbidden, message, StringComparison.OrdinalIgnoreCase);
        }

        // It is nonetheless captured on the typed observation, so a later D-2
        // design can reason about the window without the log carrying it.
        Assert.Equal(resetAt, Assert.Single(telemetry.RateLimitWindows).ResetAt);
    }

    // Two buckets of the same resource are distinguishable in the log by an
    // ordinal rather than by their reset instant.
    [Fact]
    public void LogMessage_DistinguishesBucketsWithoutRenderingTheResetInstant()
    {
        ProfileExecutionTelemetry telemetry = new();
        DateTimeOffset first = DateTimeOffset.Parse("2026-08-25T11:00:00Z");
        DateTimeOffset second = DateTimeOffset.Parse("2026-08-25T12:00:00Z");

        telemetry.RecordRateLimitObservation("core", 60, 5000, 4940, first);
        telemetry.RecordRateLimitObservation("core", 4990, 5000, 10, second);
        telemetry.Complete(
            TimeSpan.FromMilliseconds(1),
            "Complete",
            ProfileExecutionOutcome.Composed);

        string message = telemetry.ToLogMessage();

        Assert.Contains("core#1[", message, StringComparison.Ordinal);
        Assert.Contains("core#2[", message, StringComparison.Ordinal);
        Assert.DoesNotContain(
            first.ToUnixTimeSeconds().ToString(),
            message,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            second.ToUnixTimeSeconds().ToString(),
            message,
            StringComparison.Ordinal);
    }

    // ------------------------------------------------------- 9. provider layer

    // GitHubApiAvailability owns provider classification and must not have
    // acquired any dependency on rate-limit telemetry.
    [Fact]
    public void ProviderClassification_HasNoTelemetryDependency()
    {
        string source = ReadInfrastructureFile(
            "GitHub",
            "GitHubApiAvailability.cs");

        foreach (string marker in new[]
        {
            "ProfileExecutionTelemetry",
            "ProfileRateLimit",
            "RecordRateLimitObservation",
            "X-RateLimit-Resource",
            "X-RateLimit-Used"
        })
        {
            Assert.DoesNotContain(marker, source, StringComparison.Ordinal);
        }

        // Its one rate-limit rule is unchanged: remaining == 0 on a 403.
        Assert.Contains(
            "X-RateLimit-Remaining",
            source,
            StringComparison.Ordinal);
    }

    // ------------------------------------------------------- test support

    private static ProfileRateLimitWindow Window(
        ProfileExecutionTelemetry telemetry,
        string resource) =>
        Assert.Single(telemetry.RateLimitWindows, window => window.Resource == resource);

    private static async Task<HttpStatusCode> SendAsync(
        ProfileExecutionTelemetry telemetry,
        string path,
        string? resource,
        string? remaining,
        string? limit,
        string? used,
        string? reset,
        ProfileGitHubBudgetState? budgetState = null)
    {
        ProfileGitHubRequestTelemetryHandler handler = new()
        {
            InnerHandler = new RecordingHandler(resource, remaining, limit, used, reset)
        };

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        using IDisposable telemetryScope = ProfileExecutionTelemetryScope.Begin(telemetry);
        using IDisposable? budgetScope = budgetState is null
            ? null
            : ProfileGitHubBudgetScope.Begin(budgetState);

        using HttpResponseMessage response = await httpClient.GetAsync(path);

        return response.StatusCode;
    }

    private static string ReadInfrastructureFile(params string[] relativeSegments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string candidate = Path.Combine(
                [directory.FullName, "src", "GitHubProof.Infrastructure", .. relativeSegments]);

            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not locate {string.Join('/', relativeSegments)}.");
    }

    private sealed class RecordingHandler(
        string? resource,
        string? remaining,
        string? limit,
        string? used,
        string? reset) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };

            Add("X-RateLimit-Resource", resource);
            Add("X-RateLimit-Remaining", remaining);
            Add("X-RateLimit-Limit", limit);
            Add("X-RateLimit-Used", used);
            Add("X-RateLimit-Reset", reset);

            return Task.FromResult(response);

            void Add(string name, string? value)
            {
                if (value is not null)
                {
                    response.Headers.TryAddWithoutValidation(name, value);
                }
            }
        }
    }
}
