# Profile Runtime Performance & Budget

**Date:** 2026-08-24 → 2026-08-25
**Result:** PASS
**Scope:** web (Profile runtime telemetry) + live measurement
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Establish an evidence-based runtime and GitHub-request picture for Profile
Proof:

~~~text
PROFILE SIZE -> GITHUB REQUEST COUNT -> PIPELINE STAGE COST -> TOTAL ELAPSED
~~~

Explicitly **not** to optimize, and explicitly **not** to select production
values.

**This session owns MEASUREMENT AND DIAGNOSTIC EVIDENCE. It does not own final
production values** for the execution deadline, request reserve/floor, run-time
rate-limit stop policy, credential strategy, concurrency or GraphQL
consolidation. Those move to Session 121.

This record is historical evidence. It is **not** a canonical contract and
creates no new product or architecture truth owner. The canonical owner of
Profile runtime collection and guard semantics remains
`docs/architecture/public-profile-runtime-collection.md`.

---

## Starting State

~~~text
Branch:
session/120-profile-runtime-performance-budget

Base HEAD:
55992911b0e841afc5a4e093357dec255e88c2c1

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-24-119-repository-proof-visual-alignment.md
~~~

Branch and base matched exactly; `main` was at the same commit.

---

## Step 1 — Read-only runtime architecture audit

Mapped the whole execution path from the Web POST to report projection, against
`public-profile-runtime-collection.md`.

Key source-established facts (no measurement involved):

~~~text
concurrency        1 profile / 1 repository / 1 request. `src/` contains no
                   Task.WhenAll, no Parallel.*, no SemaphoreSlim — §8 forbids
                   fan-out and the code honours it.
cancellation       ExecutionToken propagates unbroken from LiveProfileReport-
                   Service through the pipeline to all 8 collectors, and all
                   22 response-content reads observe it (§10.3 satisfied).
deadline           mechanism complete and validated, but Timeout is null and
                   no appsettings*.json exists, so no outer bound is applied.
composition        Profile V1 uses 8 collectors; Contributors, SourceFiles and
                   DependencyDeclarations are never reached (§5).
retry / cache      none. No backoff, no Retry-After observance, no ETag,
                   no conditional requests, no persistence (§13 non-goals).
~~~

Derived request formula (source only, no measurement):

~~~text
TotalRequests = 1 + P + Σ_admitted [ 7 + Npf + Nwf + min(Ndoc,16)
                                       + Pc + Pi + Pp + Pr + Pt ]

  1        profile metadata
  P        repository manifest pages, 1..10
  7        fixed per repository: metadata, git ref, git commit, git tree,
           readme, license, languages
  Npf/Nwf  project-file and workflow content requests — NO upper bound
  Ndoc     documentation Markdown, bounded at 16
  Pc..Pt   commits / issues / pulls / releases / tags, each >= 1 page
~~~

Floor: **12 requests per admitted repository.** The live runs later measured
12.22–15.25, and the fixed component measured exactly 7 x admitted in every
run.

---

## Step 2 — Minimum runtime instrumentation

Added behaviour-neutral operational telemetry only, at four existing seams:
`LiveProfileReportService` (discovery, pipeline), the two observation-only
decorators (per-repository collection and analysis), and the HTTP telemetry
handler (rate-limit observation).

Timing boundaries are a sound subtraction, not an estimate — the orchestrator
drives repositories with a plain `foreach` + `await`, the decorators wrap the
only two call sites it uses, and the Profile path has no concurrency, so the
sampled intervals are pairwise disjoint and fully contained in the pipeline
interval.

The derived remainder is deliberately **not** named "post-repository
composition": admission evaluation is interleaved with repositories, so
`PipelineWorkOutsideRepositoriesElapsed` names exactly what it contains.

`ProfilePipeline`, `ProfileRepositoryAnalysisOrchestrator`, the collectors,
Domain and Application were not touched.

---

## Live benchmark evidence

Three profile shapes were measured. **Benchmark targets are operational
measurement subjects only. No benchmark supports any person-level comparison,
quality, capability, seniority or activity claim.**

All three ran on the production composition with the execution deadline and
reserve floor unconfigured, the Web admission guard and visitor limiter outside
the measured path, concurrency 1/1/1, and authenticated GitHub access.

### torvalds — heavy commit history

The primary comparable run is the **telemetry validation run**, taken after the
A-T correction below.

~~~text
discovered              12
admitted                 9
completed                9
incomplete               0
GitHub requests        131
elapsed             59.333 s
repository collection 58.255 s
repository analysis     134 ms
repository commits       30
repository contents       1
other                     0
outcome            composed / Complete / RepositoryOutcomes {}
~~~

**Provenance, which matters for how these numbers may be used.** An original
Step 3 run existed and measured 57.447 s / 131 requests / 9 admitted. That run
exposed the A-T telemetry defect: 21 of its 131 requests were classified
`other`. After the correction the same 131 requests attributed those 21
continuation requests to `repository-commits`, taking it from 9 to 30 with every
other category unchanged.

**The original run must not be used for exact pagination attribution.** It
proves commit enumeration did not explode toward full-history enumeration; it
does not establish the commit page count.

### karpathy — many repositories

~~~text
discovered              63
admitted                54
completed               54
incomplete               0
GitHub requests        662
elapsed            259.738 s
repository collection 258.332 s
repository analysis     125 ms
repository pull requests 59
repository commits       54
repository contents       8
other                     0
outcome            composed / Complete / RepositoryOutcomes {}
~~~

Pull requests were the only surface with continuation pages: 59 = 54 first pages
+ 5 continuations. Commits were exactly one page per repository.

### fatihGunalp — contents-heavy

~~~text
discovered               5
admitted                 4
completed                4
incomplete               0
GitHub requests         63
elapsed             19.989 s
repository collection 19.045 s
repository analysis      68 ms
repository contents      13
repository commits        4
other                     0
outcome            composed / Complete / RepositoryOutcomes {}
~~~

### Complete request-category breakdowns

Preserved in full so each total can be reconstructed without the OS-temp
artifacts. The closed vocabulary has 17 members; `repository-contributors` is
absent from every run because Profile V1 omits that surface entirely (§5), and
`other` is 0 in all three because the A-T correction preceded them.

~~~text
category                  torvalds   karpathy   fatihGunalp
profile-metadata                 1          1             1
profile-repositories             1          1             1
repository-metadata              9         54             4
repository-git-reference         9         54             4
repository-git-commit            9         54             4
repository-git-tree              9         54             4
repository-readme                9         54             4
repository-license               9         54             4
repository-languages             9         54             4
repository-commits              30         54             4
repository-issues                8         53             4
repository-pull-requests         9         59             4
repository-releases              9         54             4
repository-tags                  9         54             4
repository-contents              1          8            13
repository-contributors          0          0             0
other                            0          0             0
TOTAL                          131        662            63
~~~

Two details the totals depend on, both worth keeping because they are easy to
mistake for arithmetic errors:

- **`repository-issues` is one short of the admitted count in torvalds (8/9) and
  karpathy (53/54).** One repository in each profile has issues disabled, and
  `GitHubRepositoryIssueCollector` returns without issuing a request in that
  case. `fatihGunalp` has no such repository, so its 4 matches.
- **Only two surfaces ever exceeded one page.** torvalds' commits carried 21
  continuation pages (30 for 9 repositories) and karpathy's pull requests
  carried 5 (59 for 54). Everything else was exactly one page per repository,
  and the profile repository manifest was a single page in all three runs, which
  is why the profile-level cost is 2 requests everywhere.

### Derived comparison

Per-repository request figures divide the **repository-level** requests only:
the two profile-level requests are excluded.

~~~text
                        torvalds   karpathy   fatihGunalp
admitted                       9         54             4
requests / repo            14.33      12.22         15.25
contents / repo            0.111      0.148          3.25
commits / repo              3.33       1.00          1.00
collection ms / repo        6 472      4 783         4 761
collection share of run    98.2%      99.5%         95.3%
~~~

No provider failure occurred in any run: `RepositoryOutcomes` was empty and
`incomplete` was 0 in all three.

---

## Locked findings

### 1. Dominant cost

Across all three measured profile shapes, **repository collection dominated
wall-clock**: 95.3% (fatihGunalp), 98.2% (torvalds) and 99.5% (karpathy).

Supported conclusion: Profile runtime is dominated by sequential GitHub I/O **in
the measured cases**. This is not generalized beyond the measured execution
model.

### 2. Repository count

The many-repository benchmark established a material repository-count
multiplier:

~~~text
torvalds    9 admitted -> 131 requests -> 59.333 s
karpathy   54 admitted -> 662 requests -> 259.738 s
            6.00x            5.05x          4.38x
~~~

Supported model: runtime is materially affected by **admitted repository count x
repository collection weight**. Exact linear scaling is **not** claimed — the
observed multipliers are sub-linear.

### 3. Repository weight

Repository weight varies by evidence-surface behaviour, not by count:

~~~text
torvalds   30 commit requests /  9 admitted repositories
karpathy   54 commit requests / 54 admitted repositories
~~~

Commit pagination is the largest single contributor to the per-repository
request difference between the two profiles (14.33 vs 12.22, a gap of 2.11): it
alone accounts for 2.33, partly offset by karpathy's slightly higher issue,
pull-request and contents rates. It is the dominant term, not the whole of it.

### 4. CPU

**Repository-analysis CPU cost was operationally negligible in all three
measured profiles** — 134 ms, 125 ms and 68 ms.

No claim is made that CPU does not scale with repository count. That was not
proven.

### 5. Contents — D4-E2 (locked by architecture owner)

Measured contents amplification exists. `fatihGunalp` issued **13 contents
requests across 4 admitted repositories**, materially larger per repository than
the other two benchmarks (3.25 vs 0.148 and 0.111), but it produced no measured
wall-clock penalty sufficient to justify a bound — that profile's per-repository
collection time was the lowest of the three.

The source-level mechanism remains **unbounded**: `GitHubRepositoryContents-
Collector` caps documentation Markdown at 16 files but applies no upper bound to
project-file or workflow content requests.

~~~text
D4-E2   monitored production risk
        NOT "no risk"
        no contents bound justified by current measurements
~~~

### 6. Pagination telemetry — A-T CLOSED

A GitHub collector builds only the first page's URI; every continuation page is
the absolute URI GitHub supplies in its `Link: rel="next"` header, which uses a
different addressing prefix. The classifier recognised only the owner/name form,
so continuation pages fell into `other`.

Corrected by classifying on the **resource shape after the addressing prefix**
rather than requiring one prefix. Provider-supplied absolute continuation URIs
are now attributed to their existing request categories, the closed vocabulary
is unchanged at 17 members, and known-endpoint continuation requests no longer
fall into `other` in any of the three post-fix benchmark profiles.

A regression introduced by the first version of that fix — `/user/repos`
classifying as `profile-metadata` — was found by independent review and closed
by requiring the id-addressed forms to carry a numeric id.

### 7. Rate-limit interpretation — B-RI

~~~text
B-RI — rate-limit telemetry interpretation had insufficient context for D-2.
~~~

Across the benchmarks the first-minus-last remaining delta ranged from 0.98x to
13.48x the measured request count. Telemetry stored only a Remaining-oriented
aggregate, so it could not distinguish rate-limit resources, reset windows, a
window boundary, unrelated authenticated activity sharing the same token, or
other provider-side accounting effects.

**This does not establish that `X-RateLimit-Remaining` is unreliable, and it
does not establish that GitHub's rate-limit headers are untrustworthy.** Neither
claim is supported. What was insufficient was our interpretation context. Which
of the possible causes produced the observed numbers is **not** determined here.

Step 7 added operational observations for resource, limit, remaining, used and
reset window, separated by resource + reset window.
`ProfileGitHubBudgetProtection` behaviour remains unchanged, and D-2 still
requires a separate architecture decision.

---

## Rate-limit telemetry implementation record

~~~text
grouping        ProfileRateLimitWindow keyed by normalized resource + reset
                window
separation      different resources are not merged
                different reset windows are not merged
                reset-less observations remain their own bucket
cap             at most 16 recorded buckets
overflow        increments UnrecordedRateLimitObservations and is DROPPED;
                it never contaminates an existing bucket. The counter counts
                OBSERVATIONS, not distinct buckets.
log rendering   execution-local ordinal (`core#1`, `core#2`), never the
                provider's reset timestamp. The instant stays available on
                ProfileRateLimitWindow.ResetAt for a later D-2 design.
privacy         no repository, login, path, token or content is a telemetry
                dimension
decision        the budget decision remains profile-metadata-only, still
                (remaining, reset), unchanged
~~~

**Deliberate normalization trade-off.** Resource values are accepted verbatim
only if they already look like a short lowercase identifier; malformed or
unexpected shapes normalize to `unrecognized`, and a missing header to
`unspecified`. This is a bounded-cardinality telemetry choice, and it means two
different malformed provider resource names sharing a reset window land in the
same bucket. **It is not claimed that arbitrary malformed provider resource
names remain mutually distinct.**

---

## Benchmark limitations

The measurements do **not** establish:

~~~text
- average-user runtime
- the real-user repository-count distribution
- production capacity
- a production execution deadline
- a production request reserve / floor
- safe concurrency
- the secondary-rate-limit response shape
- behaviour under concurrent user load
- behaviour from production network infrastructure
- worst-case contents fan-out
~~~

Three profiles are three shapes, not a distribution. None of these measurements
may be turned into a production guarantee.

---

## Decision readiness at Session 120 exit

~~~text
D-1  execution deadline
     measurement phase complete; production/product decision required next

D-2  reserve / request protection
     telemetry context refined; the guard premise requires an architecture
     decision — it still decides from a single profile-metadata Remaining value

D-3  run-time rate-limit behaviour
     design decision ready; no natural limit event was observed

D-4  contents fan-out
     D4-E2 locked; monitored risk, no bound currently justified

D-5  credential strategy
     decision design ready using measured request costs:
     63 / 131 / 662 requests per measured profile shape

D-6  secondary rate-limit response shape
     remains OPEN; intentionally not provoked
~~~

---

## Validation

~~~text
full solution                          1909/1909  PASS  (835/262/308/504)
Web suite                                504/504  PASS
Release build                          0 warnings, 0 errors
dotnet format --verify-no-changes                PASS
git diff --check                                 PASS
mutation probes                        12 classes across Steps 2, 4 and 7;
                                       every one FAILED on the regression and
                                       restored byte-identically
live benchmarks                        3 profiles + 1 telemetry validation run
~~~

Independent review ran after Steps 2, 4 and 7 and again at closure. Every round
returned PASS WITH FINDINGS and no blocker.

~~~text
Step 2  4 findings. Corrected an inaccurate "byte-for-byte production
        composition" claim in the benchmark harness, made the artifact state
        its own measurement conditions, tightened a lock scope and two
        comments.
Step 4  4 findings. The load-bearing one was a regression the fix itself had
        introduced: /user/repos classified as profile-metadata. Review's
        suggested narrowing would not have closed it; a numeric-id requirement
        did, which also closed the second finding.
Step 7  6 findings. Renamed UnrecordedRateLimitObservations to match what it
        actually counts, corrected a test comment that claimed the reset
        instant never reaches telemetry when it reaches ProfileRateLimitWindow,
        refreshed a stale handler comment, documented the sentinel-folding
        trade-off, and clarified which torvalds run the derived ratios use.
Closure 7 findings. Removed forbidden transient Git state from CURRENT,
        added the complete per-category breakdowns above, and corrected three
        over-strong or imprecise statements.
~~~

The worklog findings raised in Steps 2, 4 and 7 were not closed then because the
architecture owner had explicitly deferred session history and INDEX until this
closure step.

---

## Not Done

~~~text
- no optimization of any kind
- no execution-deadline value, request budget, reserve floor or concurrency
  change
- no contents bound, repository cap, sampling, caching, retry or backoff
- no collector, ProfilePipeline, orchestrator, Domain or Application change
- no provider availability or rate-limit classification change
- no ProfileGitHubBudgetProtection behaviour change
- no docs/ change
- GraphQL request consolidation was not considered — it is a separate
  performance-architecture question and must not be mixed into guard decisions
~~~

---

## Known Risks

~~~text
- The contents fan-out mechanism is unbounded in source and was exercised only
  to 3.25 requests per repository. No benchmark can guarantee to find the worst
  case (D4-E2).
- The proactive budget guard decides from one profile-metadata Remaining value.
  Whether that premise is sound is the open D-2 question. It is currently
  disabled in configuration, so no production behaviour depends on it today.
- No live measurement exists from production network infrastructure or under
  concurrent load.
- The raw benchmark artifacts were written to OS temp outside the repository.
  The numbers preserved in this record are the durable copy.
~~~

---

## Working Set / Final Git State

~~~text
Modified (production):
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileGitHubRequestCategory.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileGitHubRequestTelemetryHandler.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileRuntimeTelemetryDecorators.cs

Modified (tests):
- tests/GitHubProof.Web.Tests/ProfileGitHubBudgetProtectionTests.cs

New (tests):
- tests/GitHubProof.Web.Tests/ProfileRuntimeInstrumentationTests.cs
- tests/GitHubProof.Web.Tests/ProfileRuntimeBenchmarkHarness.cs
- tests/GitHubProof.Web.Tests/ProfileGitHubRequestClassificationTests.cs
- tests/GitHubProof.Web.Tests/ProfileRateLimitObservationTests.cs

Modified (worklog):
- worklog/CURRENT.md
- worklog/INDEX.md — Session 120 row added, and Session 119's empty `Commit`
  field filled with `5599291` per README Session Lifecycle step 9. That
  backfill was owed at the start of this session and is recorded here rather
  than left silent.
- worklog/sessions/2026-08-24-120-profile-runtime-performance-budget.md (new)

Pre-existing unrelated:
- artifacts/ (user-owned, untouched)
~~~

The live benchmark harness is opt-in: it requires both
`GITHUBPROOF_RUN_LIVE_TESTS=1` and an externally supplied
`GITHUBPROOF_BENCHMARK_PROFILES`, hard-codes no profile handle, and does not
run or reach the network by default.

---

## Next

**Session 121 — Profile Production Runtime Decision Design.** Expected subjects:
D-1 execution deadline, D-2 request/reserve protection, D-3 run-time
provider-limit behaviour, D-5 production credential strategy, and the
interaction with Milestone B deployment requirements.

GraphQL request consolidation is a separate performance-architecture question
and must not be silently mixed into those guard decisions.

Session 121 begins through the normal model/effort gate.

---

## Git

~~~text
Suggested commit:
feat(profile): measure profile runtime and refine operational telemetry

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Read-only runtime architecture audit before any mutation
[x] Minimum behaviour-neutral instrumentation, reviewed
[x] A-T pagination classification defect found, corrected and confirmed
[x] Three live benchmark shapes measured
[x] B-RI recorded with accurate wording; no header-reliability claim
[x] D4-E2 locked as monitored risk, not "no risk"
[x] Benchmark evidence persisted here rather than in OS temp only
[x] No production value selected
[ ] Architecture-owner final verification
~~~
