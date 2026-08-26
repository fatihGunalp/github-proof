using System.Globalization;
using System.Text;
using GitHubProof.Application.Analysis;
using GitHubProof.Domain.Analysis.ValueObjects;
using GitHubProof.Infrastructure.GitHub.Profile;
using GitHubProof.Web.Profile;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace GitHubProof.Web.Tests;

// The opt-in plan for a controlled Profile runtime benchmark
// (Session 120 Step 3). It is a plain value read from environment variables so
// the opt-in decision itself is unit-testable without touching the network.
//
// No benchmark profile handle is hard-coded anywhere: the architecture owner
// supplies the target list externally, and an unset list means there is nothing
// to run.
internal sealed record ProfileRuntimeBenchmarkPlan(IReadOnlyList<string> Profiles)
{
    public const string EnabledVariable = "GITHUBPROOF_RUN_LIVE_TESTS";
    public const string ProfilesVariable = "GITHUBPROOF_BENCHMARK_PROFILES";
    public const string OutputRootVariable = "GITHUBPROOF_VALIDATION_OUTPUT_ROOT";

    public static bool TryCreate(
        Func<string, string?> readVariable,
        out ProfileRuntimeBenchmarkPlan? plan,
        out string reason)
    {
        ArgumentNullException.ThrowIfNull(readVariable);

        plan = null;

        if (!string.Equals(readVariable(EnabledVariable), "1", StringComparison.Ordinal))
        {
            reason =
                $"Live benchmark disabled. Set {EnabledVariable}=1 to opt in.";
            return false;
        }

        string[] profiles = (readVariable(ProfilesVariable) ?? string.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);

        if (profiles.Length == 0)
        {
            reason =
                $"No benchmark profile supplied. Set {ProfilesVariable} to a "
                + "comma-separated list of GitHub profile handles.";
            return false;
        }

        plan = new ProfileRuntimeBenchmarkPlan(profiles);
        reason = $"Enabled for {profiles.Length} profile(s).";
        return true;
    }
}

// The Step 3 measurement entry point.
//
// It runs the EXACT production composition — AddProfilePipeline, the production
// profile collector, the production LiveProfileReportService — and then reads
// the §9 telemetry the run already produced. It adds no measurement logic of its
// own, changes no semantics, and asserts nothing about the profile's content:
// a benchmark must never become a judgment about a developer.
//
// It does not execute unless GITHUBPROOF_RUN_LIVE_TESTS=1 AND the owner has
// supplied GITHUBPROOF_BENCHMARK_PROFILES.
public sealed class ProfileRuntimeBenchmarkHarness(ITestOutputHelper output)
{
    // The version Program.cs registers. Nothing branches on it, but a benchmark
    // that claims to measure production must not run a different one. Internal
    // so Session 130's real-profile validation harness runs the identical
    // version rather than declaring its own.
    internal static readonly AnalysisVersion BenchmarkVersion =
        new(
            analyzerVersion: "1.2.0",
            ruleSetVersion: "2026.08.21",
            reportSchemaVersion: 1);

    // Opt-in isolation, proven without any network access: the default
    // environment produces no plan at all.
    [Fact]
    public void Benchmark_IsDisabledUnlessExplicitlyEnabled()
    {
        Assert.False(
            ProfileRuntimeBenchmarkPlan.TryCreate(
                _ => null,
                out ProfileRuntimeBenchmarkPlan? unset,
                out _));
        Assert.Null(unset);

        // Enabled, but with no owner-supplied target list: still nothing runs.
        Assert.False(
            ProfileRuntimeBenchmarkPlan.TryCreate(
                variable => variable == ProfileRuntimeBenchmarkPlan.EnabledVariable
                    ? "1"
                    : null,
                out ProfileRuntimeBenchmarkPlan? withoutProfiles,
                out _));
        Assert.Null(withoutProfiles);

        // Target list supplied, but the opt-in flag is absent: still nothing runs.
        Assert.False(
            ProfileRuntimeBenchmarkPlan.TryCreate(
                variable => variable == ProfileRuntimeBenchmarkPlan.ProfilesVariable
                    ? "some-handle"
                    : null,
                out ProfileRuntimeBenchmarkPlan? withoutFlag,
                out _));
        Assert.Null(withoutFlag);
    }

    [Fact]
    public void Benchmark_ReadsItsTargetsOnlyFromTheSuppliedEnvironment()
    {
        Assert.True(
            ProfileRuntimeBenchmarkPlan.TryCreate(
                variable => variable switch
                {
                    ProfileRuntimeBenchmarkPlan.EnabledVariable => "1",
                    ProfileRuntimeBenchmarkPlan.ProfilesVariable => " first , second ;third ",
                    _ => null
                },
                out ProfileRuntimeBenchmarkPlan? plan,
                out _));

        Assert.Equal(["first", "second", "third"], plan!.Profiles);
    }

    [Fact]
    [Trait("Category", "LiveIntegration")]
    public async Task MeasureAsync_RecordsProfileRuntimeCostForSuppliedProfiles()
    {
        if (!ProfileRuntimeBenchmarkPlan.TryCreate(
                Environment.GetEnvironmentVariable,
                out ProfileRuntimeBenchmarkPlan? plan,
                out string reason))
        {
            output.WriteLine(reason);
            return;
        }

        StringBuilder report = new();
        report.AppendLine("# Profile runtime benchmark");
        report.AppendLine();
        AppendPreamble(report);

        using ServiceProvider provider = BuildBenchmarkProvider();

        foreach (string profile in plan!.Profiles)
        {
            using IServiceScope scope = provider.CreateScope();

            LiveProfileReportService service = scope.ServiceProvider
                .GetRequiredService<LiveProfileReportService>();

            string outcome;

            try
            {
                LiveProfileAnalysisResult result =
                    await service.AnalyzeAsync(profile, BenchmarkVersion);

                outcome = result.IsSuccessful ? "completed" : "non-success";
            }
            catch (Exception exception)
            {
                // A benchmark must report an operational failure, never hide it
                // and never turn it into a claim about the profile.
                outcome = $"faulted: {exception.GetType().Name}";
            }

            ProfileExecutionTelemetry? telemetry = service.LastExecutionTelemetry;

            // The supplied handle labels the measurement artifact so the owner
            // can tell one benchmark row from another. It is deliberately
            // written HERE and never into ProfileExecutionTelemetry, which stays
            // free of any identity (§9.1).
            report.AppendLine(
                CultureInfo.InvariantCulture,
                $"## {profile} — {outcome}");
            report.AppendLine();
            report.AppendLine("```text");
            report.AppendLine(telemetry?.ToLogMessage() ?? "no telemetry recorded");
            report.AppendLine("```");
            report.AppendLine();

            output.WriteLine($"{profile}: {outcome}");
            output.WriteLine(telemetry?.ToLogMessage() ?? "no telemetry recorded");
        }

        WriteReport(report.ToString());
    }

    // The production Profile composition — AddProfilePipeline, the production
    // collector, the production LiveProfileReportService — with two deliberate
    // and REPORTED differences, both stated in the artifact preamble:
    //
    //   * AddProfilePipeline is called without an IConfiguration, so the
    //     deadline, admission and budget options keep their unconfigured
    //     defaults. That is what a raw-cost measurement wants — production has
    //     no appsettings for them either — but it must not be mistaken for
    //     "guards were active".
    //   * The Web admission guard and visitor limiter live in the Razor page and
    //     are not part of the measured path at all.
    //
    // Internal (not private) so Session 130's real-profile validation harness can
    // reuse the EXACT same production composition rather than building a second
    // one — one composition, two observers.
    internal static ServiceProvider BuildBenchmarkProvider()
    {
        ServiceCollection services = new();

        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<RepositoryAnalysisRunner>();
        services.AddProfilePipeline();

        services.AddScoped<IGitHubProfileSnapshotCollector>(
            provider => GitHubProfileSnapshotOrchestrator.CreateDefault(
                ProfilePipelineServiceCollectionExtensions.CreateGitHubClient(provider),
                provider.GetRequiredService<TimeProvider>(),
                provider.GetRequiredService<IGitHubProfileBudgetProtection>()));

        services.AddScoped<LiveProfileReportService>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    // The artifact must describe its own measurement conditions. Whoever reads
    // it later will be choosing the §10.4 deadline and reserve values from it,
    // and must not have to reconstruct what was and was not active.
    private static void AppendPreamble(StringBuilder report)
    {
        report.AppendLine("## Measurement conditions");
        report.AppendLine();
        report.AppendLine("```text");
        report.AppendLine("composition      AddProfilePipeline + production collector");
        report.AppendLine("                 + production LiveProfileReportService");
        report.AppendLine("analysis version 1.2.0 / 2026.08.21 / schema 1  (as Program.cs)");
        report.AppendLine("execution deadline   NOT CONFIGURED — no outer time bound applied");
        report.AppendLine("budget reserve floor NOT CONFIGURED — no proactive rejection applied");
        report.AppendLine("web admission guard  NOT IN PATH — lives in the Razor page");
        report.AppendLine("visitor limiter      NOT IN PATH — lives in the Razor page");
        report.AppendLine("concurrency          1 profile, 1 repository, 1 request (unchanged)");
        report.AppendLine("```");
        report.AppendLine();
        report.AppendLine("## Metric legend");
        report.AppendLine();
        report.AppendLine("```text");
        report.AppendLine("DurationMs                  whole live execution");
        report.AppendLine("DiscoveryMs                 profile metadata + repository manifest");
        report.AppendLine("PipelineMs                  ProfilePipeline.RunAsync");
        report.AppendLine("RepositoryCollection        per-repository GitHub collection,");
        report.AppendLine("                            anonymous count/total/min/max");
        report.AppendLine("RepositoryAnalysis          per-repository analysis, same shape");
        report.AppendLine("PipelineOutsideRepositoriesMs");
        report.AppendLine("                            PipelineMs minus the two totals above.");
        report.AppendLine("                            Includes per-repository admission");
        report.AppendLine("                            evaluation and orchestrator bookkeeping");
        report.AppendLine("                            as well as eligibility, cross-repository");
        report.AppendLine("                            evidence, promotion, selection,");
        report.AppendLine("                            composition and projection.");
        report.AppendLine("RateLimitRemaining          observed X-RateLimit-Remaining only;");
        report.AppendLine("                            never a runtime decision input");
        report.AppendLine("```");
        report.AppendLine();
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
            Path.Combine(outputRoot, "profile-runtime-benchmark.md"),
            report,
            Encoding.UTF8);
    }
}
