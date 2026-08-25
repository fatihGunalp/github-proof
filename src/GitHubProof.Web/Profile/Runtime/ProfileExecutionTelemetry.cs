using System.Collections.Concurrent;
using System.Globalization;
using GitHubProof.Domain.Analysis.Enums;

namespace GitHubProof.Web.Profile.Runtime;

// Operational truth about ONE profile analysis execution
// (docs/architecture/public-profile-runtime-collection.md §9).
//
// It is never evidence: nothing here is read by an evaluator, policy, composer,
// projector or report section, and it never flows back into RepositoryAnalysis,
// eligibility, patterns, representative selection or ProfileReport. It records
// counts only — never a token, an Authorization header, a repository name, a
// file path, or any source, README or manifest content.
//
// One instance belongs to exactly one execution, so two concurrent profile
// requests can never mix counters.
public sealed class ProfileExecutionTelemetry
{
    // A single execution should see one bucket per resource, occasionally two if
    // it crosses a reset. The cap only exists so a surprising provider response
    // can never grow this dictionary without bound.
    private const int MaximumRateLimitWindows = 16;

    private readonly ConcurrentDictionary<string, int> requestCategories =
        new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, int> repositoryOutcomes =
        new(StringComparer.Ordinal);

    // Session 120. Two narrow gates rather than one: the rate-limit triple is
    // written from the HTTP handler, the duration aggregates from the two
    // observation-only decorators, and neither should ever serialize the other.
    private readonly object rateLimitGate = new();
    private readonly object durationGate = new();

    private int gitHubRequests;
    private int repositoriesDiscovered;
    private int repositoriesAdmitted;
    private int repositoriesCompleted;

    // Session 120 — B-RI. Rate-limit observations are grouped by their own
    // bucket instead of being flattened into one sequence, because GitHub's
    // rate-limit model has a resource dimension and a reset window that the
    // earlier aggregate silently mixed together.
    private readonly Dictionary<RateLimitWindowKey, MutableRateLimitWindow>
        rateLimitWindows = [];

    private int unrecordedRateLimitObservations;

    private int collectionSamples;
    private TimeSpan collectionTotal;
    private TimeSpan collectionMinimum;
    private TimeSpan collectionMaximum;

    private int analysisSamples;
    private TimeSpan analysisTotal;
    private TimeSpan analysisMinimum;
    private TimeSpan analysisMaximum;

    public int GitHubRequests => Volatile.Read(ref gitHubRequests);

    public int RepositoriesDiscovered => Volatile.Read(ref repositoriesDiscovered);

    public int RepositoriesAdmitted => Volatile.Read(ref repositoriesAdmitted);

    public int RepositoriesCompleted => Volatile.Read(ref repositoriesCompleted);

    // Admitted repositories whose collection or analysis did not complete
    // successfully — derived, never counted separately, so the two can never
    // disagree.
    public int RepositoriesIncomplete =>
        Math.Max(0, RepositoriesAdmitted - RepositoriesCompleted);

    public TimeSpan Duration { get; private set; }

    // ------------------------------------------------------- Session 120 timing
    //
    // Four measured intervals and one derived remainder. Every value describes
    // how long THIS execution spent somewhere; none of them is read by any
    // decision, and none is keyed by a repository.

    // The live profile collector call: profile metadata plus the public
    // repository manifest.
    public TimeSpan DiscoveryElapsed { get; private set; }

    // The whole ProfilePipeline.RunAsync call.
    public TimeSpan PipelineElapsed { get; private set; }

    // Per-repository snapshot collection, aggregated anonymously.
    public ProfileDurationDistribution RepositoryCollection
    {
        get
        {
            lock (durationGate)
            {
                return Distribution(
                    collectionSamples,
                    collectionTotal,
                    collectionMinimum,
                    collectionMaximum);
            }
        }
    }

    // Per-repository analysis, aggregated anonymously.
    public ProfileDurationDistribution RepositoryAnalysis
    {
        get
        {
            lock (durationGate)
            {
                return Distribution(
                    analysisSamples,
                    analysisTotal,
                    analysisMinimum,
                    analysisMaximum);
            }
        }
    }

    // Derived, and deliberately NOT called "composition time": it is exactly
    // the pipeline interval minus the two per-repository intervals, so it
    // contains every piece of pipeline work that is not a per-repository
    // collection or analysis call — admission evaluation, the orchestrator's own
    // bookkeeping, final eligibility, cross-repository evidence, pattern
    // promotion, representative selection, composition and report projection.
    //
    // The subtraction is sound because the two aggregated intervals are pairwise
    // disjoint and fully contained in the pipeline interval:
    // ProfileRepositoryAnalysisOrchestrator drives repositories with a plain
    // foreach + await, the decorators wrap the only two call sites it uses, and
    // the Profile path introduces no concurrency at all. The clamp below is a
    // defensive floor that this sequential model cannot reach.
    public TimeSpan PipelineWorkOutsideRepositoriesElapsed
    {
        get
        {
            lock (durationGate)
            {
                TimeSpan remainder = PipelineElapsed - collectionTotal - analysisTotal;

                return remainder < TimeSpan.Zero ? TimeSpan.Zero : remainder;
            }
        }
    }

    // One entry per observed rate-limit bucket: a resource plus its reset
    // window. Two observations are only ever compared against each other when
    // they belong to the SAME bucket, so a first/last/minimum sequence can no
    // longer span a window boundary or two different GitHub resources.
    //
    // Observation only. The proactive budget decision keeps reading its own
    // separate ProfileGitHubBudgetState, which this never touches.
    public IReadOnlyList<ProfileRateLimitWindow> RateLimitWindows
    {
        get
        {
            lock (rateLimitGate)
            {
                return
                [
                    .. rateLimitWindows
                        .OrderBy(entry => entry.Key.Resource, StringComparer.Ordinal)
                        .ThenBy(entry => entry.Key.ResetUnixSeconds)
                        .Select(entry => entry.Value.ToWindow(entry.Key))
                ];
            }
        }
    }

    // Observations discarded because they belonged to a bucket beyond the cap.
    // It counts OBSERVATIONS, not distinct buckets: one unrecorded bucket seen
    // fifty times contributes fifty. That is deliberate — the number says how
    // much was dropped, not how many buckets were missed, and the two must not
    // be confused.
    //
    // Non-zero means this execution saw rate-limit context it did not record.
    // It never means a recorded bucket absorbed it: overflow is dropped, never
    // merged.
    public int UnrecordedRateLimitObservations
    {
        get
        {
            lock (rateLimitGate)
            {
                return unrecordedRateLimitObservations;
            }
        }
    }

    public string Status { get; private set; } = ProfileExecutionOutcome.NotRecorded;

    public string Outcome { get; private set; } = ProfileExecutionOutcome.NotRecorded;

    public IReadOnlyDictionary<string, int> RequestCategories => requestCategories;

    // Why admitted repositories could not complete, so a rate limit is
    // distinguishable from a forbidden repository and from an ordinary failure
    // (§9 "rate-limit / forbidden / failed ... when applicable"). The vocabulary
    // is closed by ProfileRepositoryOutcome.
    public IReadOnlyDictionary<string, int> RepositoryOutcomes => repositoryOutcomes;

    public void RecordGitHubRequest(string category)
    {
        Interlocked.Increment(ref gitHubRequests);

        // A category outside the closed vocabulary is folded into "other" so the
        // dictionary can never grow unboundedly at runtime.
        string key = ProfileGitHubRequestCategory.All.Contains(category)
            ? category
            : ProfileGitHubRequestCategory.Other;

        requestCategories.AddOrUpdate(key, 1, (_, current) => current + 1);
    }

    public void RecordRepositoriesDiscovered(int count) =>
        Interlocked.Exchange(ref repositoriesDiscovered, Math.Max(0, count));

    public void RecordRepositoryAdmitted() =>
        Interlocked.Increment(ref repositoriesAdmitted);

    public void RecordRepositoryCompleted() =>
        Interlocked.Increment(ref repositoriesCompleted);

    // Operational only. It says why THIS execution could not finish a
    // repository; it is never repository evidence and never reaches the
    // analysis, the profile or the report.
    public void RecordRepositoryOutcome(string outcome) =>
        repositoryOutcomes.AddOrUpdate(
            ProfileRepositoryOutcome.Normalize(outcome),
            1,
            (_, current) => current + 1);

    // One well-formed rate-limit observation from one GitHub response.
    //
    // `remaining` is required — it is what made an observation well-formed
    // before Session 120 and still is. Everything else is optional CONTEXT that
    // GitHub may or may not send, and whose absence must never discard the
    // observation or change execution.
    //
    // Nothing here is a decision input, and nothing carries a route, repository,
    // login, path or credential.
    public void RecordRateLimitObservation(
        string? resource,
        int remaining,
        int? limit,
        int? used,
        DateTimeOffset? resetAt)
    {
        if (remaining < 0)
        {
            return;
        }

        RateLimitWindowKey key = new(
            ProfileRateLimitResource.Normalize(resource),
            resetAt?.ToUnixTimeSeconds());

        lock (rateLimitGate)
        {
            if (!rateLimitWindows.TryGetValue(key, out MutableRateLimitWindow? window))
            {
                // Never merge an unrecorded bucket into a recorded one: that is
                // exactly the mixing B-RI exists to stop. Count it instead.
                if (rateLimitWindows.Count == MaximumRateLimitWindows)
                {
                    unrecordedRateLimitObservations++;
                    return;
                }

                window = new MutableRateLimitWindow(remaining, used);
                rateLimitWindows[key] = window;
            }

            window.Observe(remaining, limit, used);
        }
    }

    public void RecordDiscoveryElapsed(TimeSpan elapsed) =>
        DiscoveryElapsed = Floor(elapsed);

    // Written under durationGate because the derived remainder reads it there
    // together with the two totals: a lock that guards only part of what its
    // reader touches would be misleading even where no race is reachable.
    public void RecordPipelineElapsed(TimeSpan elapsed)
    {
        TimeSpan value = Floor(elapsed);

        lock (durationGate)
        {
            PipelineElapsed = value;
        }
    }

    public void RecordRepositoryCollectionElapsed(TimeSpan elapsed)
    {
        TimeSpan sample = Floor(elapsed);

        lock (durationGate)
        {
            Accumulate(
                sample,
                ref collectionSamples,
                ref collectionTotal,
                ref collectionMinimum,
                ref collectionMaximum);
        }
    }

    public void RecordRepositoryAnalysisElapsed(TimeSpan elapsed)
    {
        TimeSpan sample = Floor(elapsed);

        lock (durationGate)
        {
            Accumulate(
                sample,
                ref analysisSamples,
                ref analysisTotal,
                ref analysisMinimum,
                ref analysisMaximum);
        }
    }

    public void Complete(TimeSpan duration, string status, string outcome)
    {
        Duration = duration;
        Status = status;
        Outcome = outcome;
    }

    private static TimeSpan Floor(TimeSpan elapsed) =>
        elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;

    private static void Accumulate(
        TimeSpan sample,
        ref int samples,
        ref TimeSpan total,
        ref TimeSpan minimum,
        ref TimeSpan maximum)
    {
        if (samples == 0 || sample < minimum)
        {
            minimum = sample;
        }

        if (samples == 0 || sample > maximum)
        {
            maximum = sample;
        }

        total += sample;
        samples++;
    }

    // Accumulator for one bucket. Kept private and mutable so the public record
    // above stays an immutable snapshot.
    private sealed class MutableRateLimitWindow(int remaining, int? used)
    {
        private int limit = -1;

        public int First { get; } = remaining;

        public int Last { get; private set; } = remaining;

        public int Minimum { get; private set; } = remaining;

        public int? FirstUsed { get; } = used;

        public int? LastUsed { get; private set; } = used;

        public int SampleCount { get; private set; }

        public void Observe(int observedRemaining, int? observedLimit, int? observedUsed)
        {
            Last = observedRemaining;

            if (observedRemaining < Minimum)
            {
                Minimum = observedRemaining;
            }

            if (observedLimit is not null)
            {
                limit = observedLimit.Value;
            }

            if (observedUsed is not null)
            {
                LastUsed = observedUsed;
            }

            SampleCount++;
        }

        public ProfileRateLimitWindow ToWindow(RateLimitWindowKey key) =>
            new(
                key.Resource,
                key.ResetUnixSeconds is null
                    ? null
                    : DateTimeOffset.FromUnixTimeSeconds(key.ResetUnixSeconds.Value),
                limit < 0 ? null : limit,
                First,
                Last,
                Minimum,
                FirstUsed,
                LastUsed,
                SampleCount);
    }

    private static ProfileDurationDistribution Distribution(
        int samples,
        TimeSpan total,
        TimeSpan minimum,
        TimeSpan maximum) =>
        samples == 0
            ? ProfileDurationDistribution.Empty
            : new ProfileDurationDistribution(samples, total, minimum, maximum);

    // A single structured Information-level line, so a controlled live run can
    // be read straight from the console. It deliberately carries no profile
    // identity: the measurement does not need one.
    public string ToLogMessage()
    {
        ProfileDurationDistribution collection = RepositoryCollection;
        ProfileDurationDistribution analysis = RepositoryAnalysis;
        IReadOnlyList<ProfileRateLimitWindow> rateLimit = RateLimitWindows;
        int unrecorded = UnrecordedRateLimitObservations;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"Profile analysis completed: DurationMs={(long)Duration.TotalMilliseconds} "
            + $"GitHubRequests={GitHubRequests} "
            + $"RepositoriesDiscovered={RepositoriesDiscovered} "
            + $"RepositoriesAdmitted={RepositoriesAdmitted} "
            + $"RepositoriesCompleted={RepositoriesCompleted} "
            + $"RepositoriesIncomplete={RepositoriesIncomplete} "
            + $"DiscoveryMs={(long)DiscoveryElapsed.TotalMilliseconds} "
            + $"PipelineMs={(long)PipelineElapsed.TotalMilliseconds} "
            + $"RepositoryCollection={{{Format(collection)}}} "
            + $"RepositoryAnalysis={{{Format(analysis)}}} "
            + $"PipelineOutsideRepositoriesMs="
            + $"{(long)PipelineWorkOutsideRepositoriesElapsed.TotalMilliseconds} "
            + $"RateLimitBuckets={{{Format(rateLimit)}}} "
            + $"UnrecordedRateLimitObservations={unrecorded} "
            + $"Status={Status} "
            + $"Outcome={Outcome} "
            + $"RepositoryOutcomes={{{Format(repositoryOutcomes)}}} "
            + $"RequestCategories={{{Format(requestCategories)}}}");
    }

    private static string Format(ConcurrentDictionary<string, int> counters) =>
        string.Join(
            ", ",
            counters
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => $"{entry.Key}={entry.Value}"));

    private static string Format(ProfileDurationDistribution distribution) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"count={distribution.Count}, "
            + $"totalMs={(long)distribution.Total.TotalMilliseconds}, "
            + $"minMs={(long)distribution.Minimum.TotalMilliseconds}, "
            + $"maxMs={(long)distribution.Maximum.TotalMilliseconds}");

    // Each bucket is rendered separately, so a first/last/minimum sequence in
    // the log always belongs to ONE resource and ONE window.
    //
    // Windows of the same resource are distinguished by a per-execution ordinal
    // rather than by their reset instant: the ordinal is enough to see that two
    // buckets are different, and it keeps the provider's reset timestamp out of
    // the log exactly as Session 109 required. The instant itself stays
    // available on ProfileRateLimitWindow.ResetAt for a caller that needs it.
    private static string Format(IReadOnlyList<ProfileRateLimitWindow> windows)
    {
        if (windows.Count == 0)
        {
            return "unobserved";
        }

        Dictionary<string, int> ordinals = new(StringComparer.Ordinal);
        List<string> rendered = [];

        foreach (ProfileRateLimitWindow window in windows)
        {
            ordinals.TryGetValue(window.Resource, out int ordinal);
            ordinals[window.Resource] = ++ordinal;

            rendered.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"{window.Resource}#{ordinal}["
                + $"limit={Format(window.Limit)}, "
                + $"first={window.FirstRemaining}, "
                + $"last={window.LastRemaining}, "
                + $"min={window.MinimumRemaining}, "
                + $"used={Format(window.FirstUsed)}->{Format(window.LastUsed)}, "
                + $"samples={window.SampleCount}]"));
        }

        return string.Join(", ", rendered);
    }

    private static string Format(int? value) =>
        value is null
            ? "unobserved"
            : value.Value.ToString(CultureInfo.InvariantCulture);
}

// Anonymous per-repository duration aggregate. It deliberately carries counts
// and durations only: no repository name, owner, URL, path or any other stable
// identity may enter profile telemetry
// (docs/architecture/public-profile-runtime-collection.md §9.1).
public sealed record ProfileDurationDistribution(
    int Count,
    TimeSpan Total,
    TimeSpan Minimum,
    TimeSpan Maximum)
{
    public static ProfileDurationDistribution Empty { get; } =
        new(0, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
}

// Observation-only view of ONE GitHub rate-limit bucket across one execution:
// a resource together with a single reset window.
//
// Session 120 — B-RI. The earlier shape carried a bare first/last/minimum
// remaining sequence with no resource and no window, so three live benchmarks
// produced a first-minus-last delta that ranged from 0.98x to 13.48x the
// measured request count with no way to tell whether a window boundary, a
// different GitHub resource, or unrelated activity on the same token explained
// it. This record preserves the context needed to tell those apart later; it
// does NOT decide which of them was the cause, and it is still never a runtime
// decision input.
//
// `Limit` and `Used` are optional because GitHub may not send them; their
// absence is not a failure and never discards the observation.
//
// One deliberate limit on that context: two resource values that both fail the
// shape filter share the single `unrecognized` bucket, as do two responses that
// both omit the header under `unspecified`. Bounded cardinality is preferred
// over perfect separation for values GitHub is not known to emit.
public sealed record ProfileRateLimitWindow(
    string Resource,
    DateTimeOffset? ResetAt,
    int? Limit,
    int FirstRemaining,
    int LastRemaining,
    int MinimumRemaining,
    int? FirstUsed,
    int? LastUsed,
    int SampleCount);

// The bucket a rate-limit observation belongs to. Both parts come from GitHub
// response headers; neither is a repository, login, path or credential.
internal readonly record struct RateLimitWindowKey(
    string Resource,
    long? ResetUnixSeconds);

// GitHub's rate-limit resource name, kept to a safe bounded shape.
//
// The codebase cannot establish which resource each Profile endpoint family
// reports — before Session 120 it never read the header at all — so no list of
// GitHub resource names is asserted here. Instead a value is accepted only if
// it already looks like a short lowercase identifier, which keeps telemetry
// cardinality bounded without claiming to know GitHub's vocabulary.
public static class ProfileRateLimitResource
{
    // The response carried no resource header.
    public const string Unspecified = "unspecified";

    // The response carried a resource header whose shape is not a safe short
    // identifier. Folded rather than echoed.
    public const string Unrecognized = "unrecognized";

    private const int MaximumLength = 32;

    public static string Normalize(string? resource)
    {
        if (string.IsNullOrEmpty(resource))
        {
            return Unspecified;
        }

        if (resource.Length > MaximumLength)
        {
            return Unrecognized;
        }

        foreach (char character in resource)
        {
            if (!char.IsAsciiLetterLower(character) && character != '_')
            {
                return Unrecognized;
            }
        }

        return resource;
    }
}

// The closed vocabulary for why an admitted repository did not complete. Every
// value is either a canonical DataAvailabilityStatus name — the same typed
// vocabulary the collectors already report — or one of the two explicit
// non-availability cases, so it can never grow at runtime and never carries a
// repository name, path or message.
public static class ProfileRepositoryOutcome
{
    public const string AnalysisFailed = "AnalysisFailed";
    public const string Other = "Other";

    private static readonly HashSet<string> Known =
        new(
            Enum.GetNames<DataAvailabilityStatus>().Concat([AnalysisFailed, Other]),
            StringComparer.Ordinal);

    public static IReadOnlyCollection<string> All => Known;

    public static string Normalize(string? outcome) =>
        outcome is not null && Known.Contains(outcome) ? outcome : Other;

    public static string FromAvailabilityStatus(DataAvailabilityStatus status) =>
        Normalize(status.ToString());
}

// The closed outcome vocabulary. It describes this execution only, never the
// developer, the repositories or the quality of the work, and it introduces no
// new AnalysisStatus or DataAvailabilityStatus value.
public static class ProfileExecutionOutcome
{
    public const string NotRecorded = "not-recorded";
    public const string Composed = "composed";
    public const string CompositionFailed = "composition-failed";
    public const string CollectionUnavailable = "collection-unavailable";
    public const string Cancelled = "cancelled";
    public const string DeadlineExpired = "deadline-expired";
    public const string BudgetProtectionRejected = "budget-protection-rejected";
    public const string Faulted = "faulted";
}
