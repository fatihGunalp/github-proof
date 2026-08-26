using System.Diagnostics;
using System.Globalization;
using System.Text;
using GitHubProof.Application.Profile;
using GitHubProof.Application.Profile.Report;
using GitHubProof.Domain.Profile.Models;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace GitHubProof.Web.Tests;

// Session 131 — bounded repository concurrency (R) and simultaneous profile
// executions (U) measurement.
//
// It runs the EXACT production composition, supplying R through the same
// external configuration key a deployment would use. It adds no evaluator,
// policy, composer or judgment, and it asserts nothing about any developer.
//
// Opt-in through the Session 120 gate that is already proven disabled by
// default, plus its own profile variables:
//
//   GITHUBPROOF_RUN_LIVE_TESTS=1
//   GITHUBPROOF_CONCURRENCY_DENSE_PROFILE   dense shape, the R subject
//   GITHUBPROOF_CONCURRENCY_SECOND_PROFILE  smaller shape, the U2 partner
public sealed class ProfileConcurrencyMeasurementHarness(ITestOutputHelper output)
{
    private const string DenseProfileVariable = "GITHUBPROOF_CONCURRENCY_DENSE_PROFILE";
    private const string SecondProfileVariable = "GITHUBPROOF_CONCURRENCY_SECOND_PROFILE";

    [Fact]
    [Trait("Category", "LiveIntegration")]
    public async Task MeasureAsync_ComparesR1R2AndU2()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    ProfileRuntimeBenchmarkPlan.EnabledVariable),
                "1",
                StringComparison.Ordinal))
        {
            output.WriteLine("Live concurrency measurement disabled.");
            return;
        }

        string? dense = Environment.GetEnvironmentVariable(DenseProfileVariable);
        string? second = Environment.GetEnvironmentVariable(SecondProfileVariable);

        if (string.IsNullOrWhiteSpace(dense) || string.IsNullOrWhiteSpace(second))
        {
            output.WriteLine(
                $"Set {DenseProfileVariable} and {SecondProfileVariable} to run.");
            return;
        }

        StringBuilder report = new();
        report.AppendLine("# Session 131 — repository concurrency measurement");
        report.AppendLine();
        report.AppendLine(
            "Production composition only. R supplied through "
            + "`ProfileGuardrails:Collection:MaxRepositoryConcurrency`.");
        report.AppendLine();

        // ---- Scenario 1: dense profile, R = 1 (today's shape) --------------
        Measurement r1 = await RunAsync(dense, repositoryConcurrency: 1);
        Append(report, "S1 — dense, U1/R1 (baseline)", dense, 1, 1, r1);

        // ---- Scenario 2: dense profile, R = 2 -----------------------------
        Measurement r2 = await RunAsync(dense, repositoryConcurrency: 2);
        Append(report, "S2 — dense, U1/R2", dense, 2, 1, r2);

        // ---- Semantic parity, live -----------------------------------------
        bool parity = string.Equals(r1.Fingerprint, r2.Fingerprint, StringComparison.Ordinal);

        report.AppendLine("## Live semantic parity — S1 vs S2");
        report.AppendLine();
        report.AppendLine(parity ? "IDENTICAL" : "DIFFERENT");

        if (!parity)
        {
            report.AppendLine();
            report.AppendLine("```text");
            report.AppendLine(FirstDifference(r1.Fingerprint, r2.Fingerprint));
            report.AppendLine("```");
        }

        report.AppendLine();

        // ---- Scenario 3: U2/R2 — two DIFFERENT profiles at once ------------
        // Two distinct ProfilePipeline executions, not two repositories inside
        // one. Each gets its own provider scope, its own telemetry and its own
        // budget state, exactly as two simultaneous visitors would.
        Stopwatch wall = Stopwatch.StartNew();

        Task<Measurement> denseTask = RunAsync(dense, repositoryConcurrency: 2);
        Task<Measurement> secondTask = RunAsync(second, repositoryConcurrency: 2);

        Measurement[] both = await Task.WhenAll(denseTask, secondTask);

        wall.Stop();

        Append(report, "S3 — dense under U2/R2", dense, 2, 2, both[0]);
        Append(report, "S3 — second profile under U2/R2", second, 2, 2, both[1]);

        report.AppendLine("## U2 wall clock (both executions overlapped)");
        report.AppendLine();
        report.AppendLine(
            $"combined elapsed: {wall.Elapsed.TotalSeconds:F3} s");
        report.AppendLine();

        // Telemetry isolation: the two executions must not have shared counters.
        bool isolated =
            both[0].GitHubRequests != both[1].GitHubRequests
            || both[0].RepositoriesDiscovered != both[1].RepositoriesDiscovered;

        report.AppendLine("## U2 telemetry isolation");
        report.AppendLine();
        report.AppendLine(
            $"dense requests={both[0].GitHubRequests}, "
            + $"second requests={both[1].GitHubRequests}, "
            + $"distinct={isolated}");
        report.AppendLine();

        // Dense semantic parity must survive running next to another profile.
        bool u2Parity = string.Equals(
            r1.Fingerprint,
            both[0].Fingerprint,
            StringComparison.Ordinal);

        report.AppendLine("## Live semantic parity — S1 vs dense under U2/R2");
        report.AppendLine();
        report.AppendLine(u2Parity ? "IDENTICAL" : "DIFFERENT");
        report.AppendLine();

        WriteReport(report.ToString());

        output.WriteLine(report.ToString());

        // Hard gates. Anything here failing is a real finding, and the artifact
        // is already on disk before the first assertion runs.
        Assert.Equal("composed", r1.Outcome);
        Assert.Equal("composed", r2.Outcome);
        Assert.Equal("composed", both[0].Outcome);
        Assert.Equal("composed", both[1].Outcome);

        // GraphQL stays DEFAULT OFF in every scenario.
        Assert.All(
            new[] { r1, r2, both[0], both[1] },
            measurement => Assert.Equal(0, measurement.GraphQlRequests));
    }

    private static async Task<Measurement> RunAsync(
        string profile,
        int repositoryConcurrency)
    {
        using ServiceProvider provider =
            ProfileRuntimeBenchmarkHarness.BuildBenchmarkProvider(repositoryConcurrency);

        using IServiceScope scope = provider.CreateScope();

        LiveProfileReportService service = scope.ServiceProvider
            .GetRequiredService<LiveProfileReportService>();

        Stopwatch elapsed = Stopwatch.StartNew();

        LiveProfileAnalysisResult result = await service.AnalyzeAsync(
            profile,
            ProfileRuntimeBenchmarkHarness.BenchmarkVersion);

        elapsed.Stop();

        ProfileExecutionTelemetry? telemetry = service.LastExecutionTelemetry;

        return new Measurement(
            elapsed.Elapsed,
            telemetry,
            result.IsSuccessful ? telemetry?.Outcome ?? "unknown" : "non-success",
            Fingerprint(result));
    }

    // The PROFILE-LEVEL half of §6, flattened to one comparable string:
    // repository universe and admission, eligibility, patterns, representative
    // repositories, limitations, warnings and the report's evidence trace.
    //
    // It deliberately excludes wall clock, RetrievedAt and analysis ids (§19.2
    // already excludes observation time from parity), and it does NOT cover
    // §6's per-repository half — RepositoryAnalysis.Status, the score
    // breakdown, the 15 SignalResults and their evidence keys. That half is
    // proven deterministically by ProfileRepositoryConcurrencyTests against
    // captured inputs, which is where it belongs: comparing it across two LIVE
    // runs would be comparing two separate observations of a moving provider.
    private static string Fingerprint(LiveProfileAnalysisResult result)
    {
        if (result.PipelineResult is not ProfilePipelineResult pipeline
            || pipeline.Profile is not DeveloperPublicEvidenceProfile profile)
        {
            return "no-profile";
        }

        StringBuilder text = new();

        text.AppendLine(CultureInfo.InvariantCulture, $"status={profile.AnalysisStatus}");
        text.AppendLine(CultureInfo.InvariantCulture, $"composition={pipeline.Status}");

        foreach (RepositoryEligibilityResult eligibility in profile.RepositoryEligibilityResults)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"repo={eligibility.Reference.FullName}"
                + $";status={eligibility.Status}"
                + $";admission={eligibility.Contribution.Admission.Decision}"
                + $";reason={eligibility.Contribution.Admission.Reason?.ToString() ?? "-"}"
                + $";archived={eligibility.Archived}"
                + $";activity={eligibility.Activity}"
                + $";independence={eligibility.Independence}");
        }

        foreach (RepeatedProfilePattern pattern in profile.RepeatedProfilePatterns)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"pattern={pattern.Family}:{pattern.PatternKey}"
                + $";supporting={string.Join(",", pattern.SupportingRepositories.Select(r => r.FullName))}"
                + $";independent={string.Join(",", pattern.IndependentSupportingRepositories.Select(r => r.FullName))}");
        }

        foreach (RepresentativeRepository representative in profile.RepresentativeRepositories)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"representative={representative.Repository.FullName}"
                + $";patterns={string.Join(",", representative.PatternSupports.Select(s => $"{s.PatternFamily}:{s.PatternKey}"))}");
        }

        foreach (ProfileLimitation limitation in profile.Limitations)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"limitation={limitation.Code};scope={limitation.Scope}"
                + $";repos={string.Join(",", limitation.AffectedRepositories.Select(r => r.FullName))}");
        }

        foreach (ProfileWarning warning in profile.Warnings)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"warning={warning.Code};scope={warning.Scope}"
                + $";repos={string.Join(",", warning.AffectedRepositories.Select(r => r.FullName))}");
        }

        if (pipeline.Report is ProfileReport report)
        {
            foreach (ConclusionEvidenceReportItem why in report.WhyThisConclusion.Items)
            {
                text.AppendLine(CultureInfo.InvariantCulture,
                    $"why={why.Pattern.Family}:{why.Pattern.PatternKey}"
                    + $";evidence={string.Join(",", why.SupportingEvidence.Select(e => $"{e.Repository.FullName}/{e.EvidenceKey}"))}");
            }

            foreach (RepositoryDrillDownReportItem drill in report.RepositoryDrillDown.Items)
            {
                text.AppendLine(CultureInfo.InvariantCulture,
                    $"drilldown={drill.Repository.FullName};eligibility={drill.EligibilityStatus}");
            }
        }

        return text.ToString();
    }

    private static void Append(
        StringBuilder report,
        string title,
        string profile,
        int r,
        int u,
        Measurement measurement)
    {
        report.AppendLine(CultureInfo.InvariantCulture, $"## {title}");
        report.AppendLine();
        report.AppendLine(CultureInfo.InvariantCulture,
            $"profile={profile} R={r} U={u} outcome={measurement.Outcome}");
        report.AppendLine(CultureInfo.InvariantCulture,
            $"elapsed={measurement.Elapsed.TotalSeconds:F3} s");
        report.AppendLine();
        report.AppendLine("```text");
        report.AppendLine(measurement.Telemetry?.ToLogMessage() ?? "no telemetry recorded");
        report.AppendLine("```");
        report.AppendLine();
    }

    private static string FirstDifference(string left, string right)
    {
        string[] leftLines = left.Split('\n');
        string[] rightLines = right.Split('\n');

        for (int index = 0; index < Math.Max(leftLines.Length, rightLines.Length); index++)
        {
            string a = index < leftLines.Length ? leftLines[index] : "<missing>";
            string b = index < rightLines.Length ? rightLines[index] : "<missing>";

            if (!string.Equals(a, b, StringComparison.Ordinal))
            {
                return $"line {index + 1}\n  R1: {a}\n  R2: {b}";
            }
        }

        return "no line-level difference";
    }

    private static void WriteReport(string report)
    {
        string? outputRoot = Environment.GetEnvironmentVariable(
            ProfileRuntimeBenchmarkPlan.OutputRootVariable);

        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            return;
        }

        Directory.CreateDirectory(outputRoot);

        File.WriteAllText(
            Path.Combine(outputRoot, "profile-concurrency-measurement.md"),
            report,
            Encoding.UTF8);
    }

    private sealed record Measurement(
        TimeSpan Elapsed,
        ProfileExecutionTelemetry? Telemetry,
        string Outcome,
        string Fingerprint)
    {
        public int GitHubRequests => Telemetry?.GitHubRequests ?? 0;

        public int RepositoriesDiscovered => Telemetry?.RepositoriesDiscovered ?? 0;

        public int GraphQlRequests =>
            Telemetry is not null
            && Telemetry.RequestCategories.TryGetValue("graphql", out int count)
                ? count
                : 0;
    }
}
