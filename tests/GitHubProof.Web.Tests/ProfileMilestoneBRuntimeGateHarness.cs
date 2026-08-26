using System.Diagnostics;
using System.Globalization;
using System.Net;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace GitHubProof.Web.Tests;

// Session 133 — opt-in, real-provider Milestone B dense-profile exploration.
//
// It tries R3 -> R4 -> R5 in order and stops at the first <= 60 s result. A
// caller-supplied remaining floor is enforced by the production Profile budget
// guard immediately after the real metadata response, before manifest or
// repository fan-out. This makes the live response header — not /rate_limit —
// authoritative for whether an expensive candidate may proceed.
public sealed class ProfileMilestoneBRuntimeGateHarness(ITestOutputHelper output)
{
    private const string DenseProfileVariable =
        "GITHUBPROOF_RUNTIME_GATE_DENSE_PROFILE";

    private const string RemainingFloorVariable =
        "GITHUBPROOF_RUNTIME_GATE_REMAINING_FLOOR";

    private static readonly int[] Candidates = [3, 4, 5];

    [Fact]
    public void RuntimeGateComposition_UsesExternalConcurrencyFloorAndObserver()
    {
        RequestPressureObserver observer = new();

        using ServiceProvider provider =
            ProfileRuntimeBenchmarkHarness.BuildBenchmarkProvider(
                maxRepositoryConcurrency: 5,
                profileAdmissionRemainingFloor: 1500,
                additionalProfileHandler: observer);

        Assert.Equal(
            5,
            provider.GetRequiredService<IOptions<ProfileCollectionOptions>>()
                .Value
                .MaxRepositoryConcurrency);
        Assert.Equal(
            1500,
            provider
                .GetRequiredService<IOptions<ProfileGitHubBudgetProtectionOptions>>()
                .Value
                .ProfileAdmissionRemainingFloor);

        // Building the named pipeline twice exercises HttpClientFactory's
        // handler reuse boundary without issuing a provider request.
        using HttpClient first =
            ProfilePipelineServiceCollectionExtensions.CreateGitHubClient(provider);
        using HttpClient second =
            ProfilePipelineServiceCollectionExtensions.CreateGitHubClient(provider);

        Assert.NotSame(first, second);
        Assert.Equal(0, observer.MaxObservedInFlight);
    }

    [Fact]
    public void RuntimeGateFormatting_PreservesMultipleCoreWindows()
    {
        ProfileRateLimitWindow[] windows =
        [
            new(
                "core",
                DateTimeOffset.Parse("2026-08-26T19:57:57Z"),
                5000,
                4277,
                3616,
                3616,
                723,
                1384,
                661),
            new(
                "core",
                DateTimeOffset.Parse("2026-08-26T19:58:15Z"),
                5000,
                4965,
                4965,
                4965,
                35,
                35,
                1)
        ];

        Assert.Equal(
            "4277->3616(min 3616);4965->4965(min 4965)",
            FormatRemaining(windows));
        Assert.Equal(
            "2026-08-26T19:57:57.0000000+00:00;"
            + "2026-08-26T19:58:15.0000000+00:00",
            FormatReset(windows));
    }

    [Fact]
    public async Task RuntimeGateObserver_StopsOnRetryAfter()
    {
        using CancellationTokenSource stop = new();
        HttpResponseMessage response = new(HttpStatusCode.Forbidden);
        response.Headers.TryAddWithoutValidation("Retry-After", "60");

        using HttpMessageInvoker client = new(
            new RequestPressureObserver(stop.Cancel)
            {
                InnerHandler = new StaticResponseHandler(response)
            });

        using HttpResponseMessage observed = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/test"),
            CancellationToken.None);

        Assert.True(stop.IsCancellationRequested);
    }

    [Fact]
    public async Task RuntimeGateObserver_StopsOnPrimaryExhaustion()
    {
        using CancellationTokenSource stop = new();
        HttpResponseMessage response = new(HttpStatusCode.Forbidden);
        response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", "0");

        using HttpMessageInvoker client = new(
            new RequestPressureObserver(stop.Cancel)
            {
                InnerHandler = new StaticResponseHandler(response)
            });

        using HttpResponseMessage observed = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/test"),
            CancellationToken.None);

        Assert.True(stop.IsCancellationRequested);
    }

    [Fact]
    [Trait("Category", "LiveIntegration")]
    public async Task MeasureAsync_StopsAtFirstDenseCandidateWithinSixtySeconds()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    ProfileRuntimeBenchmarkPlan.EnabledVariable),
                "1",
                StringComparison.Ordinal))
        {
            output.WriteLine("Live Milestone B runtime gate disabled.");
            return;
        }

        string? dense = Environment.GetEnvironmentVariable(DenseProfileVariable);

        if (string.IsNullOrWhiteSpace(dense))
        {
            output.WriteLine($"Set {DenseProfileVariable} to run.");
            return;
        }

        if (!int.TryParse(
                Environment.GetEnvironmentVariable(RemainingFloorVariable),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int remainingFloor)
            || remainingFloor < 0)
        {
            output.WriteLine(
                $"Set {RemainingFloorVariable} to a non-negative benchmark safety floor.");
            return;
        }

        string? referenceFingerprint = null;
        int? referenceRequestCount = null;
        bool latencyGatePassed = false;

        foreach (int repositoryConcurrency in Candidates)
        {
            using CancellationTokenSource providerProtectionStop = new();
            RequestPressureObserver observer = new(providerProtectionStop.Cancel);

            using ServiceProvider provider =
                ProfileRuntimeBenchmarkHarness.BuildBenchmarkProvider(
                    repositoryConcurrency,
                    remainingFloor,
                    observer);

            using IServiceScope scope = provider.CreateScope();

            LiveProfileReportService service = scope.ServiceProvider
                .GetRequiredService<LiveProfileReportService>();

            Stopwatch elapsed = Stopwatch.StartNew();
            LiveProfileAnalysisResult result;

            try
            {
                result = await service.AnalyzeAsync(
                    dense,
                    ProfileRuntimeBenchmarkHarness.BenchmarkVersion,
                    providerProtectionStop.Token);
            }
            catch (OperationCanceledException exception)
                when (providerProtectionStop.IsCancellationRequested)
            {
                elapsed.Stop();
                throw new InvalidOperationException(
                    $"R={repositoryConcurrency} stopped immediately after a "
                    + "provider-protection response.",
                    exception);
            }

            elapsed.Stop();

            ProfileExecutionTelemetry telemetry = service.LastExecutionTelemetry
                ?? throw new InvalidOperationException(
                    "A live Profile run must expose its execution telemetry.");

            string fingerprint = ProfileConcurrencyMeasurementHarness
                .Fingerprint(result);

            referenceFingerprint ??= fingerprint;
            referenceRequestCount ??= telemetry.GitHubRequests;

            bool semanticParity = string.Equals(
                referenceFingerprint,
                fingerprint,
                StringComparison.Ordinal);

            ProfileRateLimitWindow[] core = telemetry.RateLimitWindows
                .Where(window => window.Resource == "core")
                .ToArray();

            output.WriteLine(
                $"R={repositoryConcurrency} elapsed={elapsed.Elapsed.TotalSeconds:F3}s "
                + $"outcome={(result.IsSuccessful ? telemetry.Outcome : "non-success")} "
                + $"requests={telemetry.GitHubRequests} "
                + $"max-http={observer.MaxObservedInFlight} "
                + $"remaining={FormatRemaining(core)} "
                + $"reset={FormatReset(core)} "
                + $"retry-after={observer.RetryAfterResponses} "
                + $"primary-limit={observer.PrimaryLimitResponses} "
                + $"secondary-limit={observer.SecondaryLimitResponses} "
                + $"incomplete={telemetry.RepositoriesIncomplete} "
                + $"semantic-parity={semanticParity} "
                + $"graphql={GraphQlRequests(telemetry)}");

            Assert.Equal(0, GraphQlRequests(telemetry));
            Assert.True(semanticParity);

            if (!result.IsSuccessful)
            {
                Assert.Fail(
                    $"R={repositoryConcurrency} was stopped by an operational "
                    + "provider/budget outcome before the latency gate could close.");
            }

            Assert.Equal("composed", telemetry.Outcome);
            Assert.Equal(referenceRequestCount.Value, telemetry.GitHubRequests);
            Assert.Equal(0, telemetry.RepositoriesIncomplete);
            Assert.Equal(0, telemetry.UnrecordedRateLimitObservations);
            Assert.Equal(0, observer.RetryAfterResponses);
            Assert.Equal(0, observer.PrimaryLimitResponses);
            Assert.Equal(0, observer.SecondaryLimitResponses);
            Assert.InRange(observer.MaxObservedInFlight, 1, repositoryConcurrency);

            if (elapsed.Elapsed <= TimeSpan.FromSeconds(60))
            {
                latencyGatePassed = true;
                output.WriteLine(
                    $"FIRST_PASS R={repositoryConcurrency} "
                    + $"elapsed={elapsed.Elapsed.TotalSeconds:F3}s");
                break;
            }
        }

        Assert.True(
            latencyGatePassed,
            "No R3/R4/R5 production-candidate measurement met the <= 60 s gate.");
    }

    private static int GraphQlRequests(ProfileExecutionTelemetry telemetry) =>
        telemetry.RequestCategories.TryGetValue("graphql", out int count)
            ? count
            : 0;

    private static string FormatRemaining(
        IReadOnlyList<ProfileRateLimitWindow> windows) =>
        windows.Count == 0
            ? "unobserved"
            : string.Join(
                ";",
                windows.Select(window =>
                    $"{window.FirstRemaining}->{window.LastRemaining}"
                    + $"(min {window.MinimumRemaining})"));

    private static string FormatReset(
        IReadOnlyList<ProfileRateLimitWindow> windows) =>
        windows.Count == 0
            ? "unobserved"
            : string.Join(
                ";",
                windows.Select(window =>
                    window.ResetAt?.ToString("O", CultureInfo.InvariantCulture)
                        ?? "unobserved"));

    private sealed class RequestPressureObserver(Action? stopOnProviderProtection = null)
        : DelegatingHandler
    {
        private int inFlight;
        private int maxObservedInFlight;
        private int retryAfterResponses;
        private int primaryLimitResponses;
        private int secondaryLimitResponses;

        public int MaxObservedInFlight => Volatile.Read(ref maxObservedInFlight);

        public int RetryAfterResponses => Volatile.Read(ref retryAfterResponses);

        public int PrimaryLimitResponses => Volatile.Read(ref primaryLimitResponses);

        public int SecondaryLimitResponses => Volatile.Read(ref secondaryLimitResponses);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            int current = Interlocked.Increment(ref inFlight);
            InterlockedMax(ref maxObservedInFlight, current);

            try
            {
                HttpResponseMessage response =
                    await base.SendAsync(request, cancellationToken);

                bool hasRetryAfter = response.Headers.RetryAfter is not null;
                bool isProtectionStatus = response.StatusCode is HttpStatusCode.Forbidden
                    or HttpStatusCode.TooManyRequests;
                bool isPrimaryLimit = isProtectionStatus && HasZeroRemaining(response);
                bool isSecondaryLimit = hasRetryAfter
                    || (isProtectionStatus && !isPrimaryLimit);

                if (hasRetryAfter)
                {
                    Interlocked.Increment(ref retryAfterResponses);
                }

                if (isPrimaryLimit)
                {
                    Interlocked.Increment(ref primaryLimitResponses);
                }

                if (isSecondaryLimit)
                {
                    Interlocked.Increment(ref secondaryLimitResponses);
                }

                if (isPrimaryLimit || isSecondaryLimit)
                {
                    stopOnProviderProtection?.Invoke();
                }

                return response;
            }
            finally
            {
                Interlocked.Decrement(ref inFlight);
            }
        }

        private static bool HasZeroRemaining(HttpResponseMessage response) =>
            response.Headers.TryGetValues(
                "X-RateLimit-Remaining",
                out IEnumerable<string>? values)
            && values.Count() == 1
            && string.Equals(values.Single(), "0", StringComparison.Ordinal);

        private static void InterlockedMax(ref int target, int value)
        {
            int observed = Volatile.Read(ref target);

            while (value > observed)
            {
                int previous = Interlocked.CompareExchange(
                    ref target,
                    value,
                    observed);

                if (previous == observed)
                {
                    return;
                }

                observed = previous;
            }
        }
    }

    private sealed class StaticResponseHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(response);
    }
}
