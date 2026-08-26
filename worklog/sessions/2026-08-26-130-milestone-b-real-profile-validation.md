# Milestone B Pre-Production Real Profile Validation

**Session:** 130
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** **B-PREPROD-READY-WITH-FINDINGS** — real Profile Proof validation
against six real public GitHub profiles completed cleanly. The GraphQL
authoritative envelope stayed DEFAULT OFF throughout (zero authoritative
GraphQL requests, confirmed from recorded request categories). Two bounded,
non-blocking findings, both in this session's own new *test-only* validation
code (never in production code), both found by the live runs themselves and
fixed in-session:

1. a traceability-check defect (checked only the signal-evidence `EvidenceKey`
   shape, missed the documented snapshot-data shape for languages);
2. an **evidence-completeness gap**, raised by architecture-owner review after
   this session's first close: the automated prohibited-inference check only
   scanned closed system vocabulary (limitation/warning codes, pattern
   keys) — it never inspected the actual **rendered report text** a reader
   sees, so "prohibited inference PASS for all six profiles" was not yet
   fully evidenced. Closed below under "Closure — rendered-text
   prohibited-inference proof."

---

## History — the environmental precondition that first blocked this session

Session 130 was opened once already, earlier the same day, and returned
**REAL-VALIDATION-BLOCKED**: `GITHUB_TOKEN` — the sole credential path both
GitHub clients read (`Program.cs:43-52`,
`ProfilePipelineServiceCollectionExtensions.cs:313-318`) — was unset in
Process, User, and Machine environment scopes, and the session correctly
refused to fall back to the unauthenticated 60/hour path per its own explicit
instruction. No code was changed and no live call was made in that attempt.

**That environmental precondition is now resolved.** The owner set
`GITHUB_TOKEN` as a persistent Windows **User**-scope environment variable
(`setx`). Because this session's tool processes descend from a process tree
that started before the `setx` took effect, the variable did not appear in
Process scope automatically; it was bridged explicitly (read from User scope,
assigned into the Process environment of the *same* command that then invoked
`dotnet test`/`dotnet run`, so the child process inherited it) — without ever
printing, logging, or persisting the token value anywhere. This is not a new
credential architecture: it is the existing `GITHUB_TOKEN` mechanism, made
visible to the process that needed it.

This session's continuation is recorded below. It reuses the same branch,
baseline, and worklog reconciliation already established by the first
attempt.

---

## Baseline

~~~text
Branch : session/130-milestone-b-real-profile-validation
Base   : 4075e9a  (== main == origin/main, re-verified at continuation start)
Working tree at continuation start: same as the first attempt left it
  (worklog reconciliation only) + pre-existing ?? artifacts/
~~~

Session 129 remains **integrated as `4075e9a`** (reconciled by the first
attempt; not reopened here).

---

## Purpose

The first explicit pre-production validation of the current Profile Proof
against real public GitHub profiles, ahead of the Production Runtime
Decisions that follow it on the Milestone B closure path. Scoped narrowly:
no product feature, no GraphQL expansion, no S4, no production runtime value
selection, no deploy.

---

## Execution path

Reused Session 120's existing opt-in live benchmark harness pattern exactly
(`GITHUBPROOF_RUN_LIVE_TESTS=1` + `GITHUBPROOF_BENCHMARK_PROFILES`,
`tests/GitHubProof.Web.Tests/ProfileRuntimeBenchmarkHarness.cs`), which
already proved disabled-by-default and already runs the **exact** production
composition (`AddProfilePipeline`, the production profile collector, the
production `LiveProfileReportService`) with no measurement logic of its own.

A new file, `ProfileRealValidationHarness.cs`, was added alongside it —
**not** a second pipeline. It calls the identical composition
(`ProfileRuntimeBenchmarkHarness.BuildBenchmarkProvider()`, changed from
`private` to `internal` so it could be reused rather than duplicated) and the
identical `AnalysisVersion` (`BenchmarkVersion`, same accessibility change),
then additionally reads surfaces the production pipeline already produces —
`ProfileReport`, `RepositoryEligibilityResults`, `ProfileEvidenceReference` —
with no new evaluator, policy, composer, or judgment. For traceability
verification it reruns the **same** repository drill-down mechanism the Web
UI already exposes (`LiveRepositoryReportService`) rather than inventing a
second way to reach `RepositoryAnalysis`. This doubles as part of the
Web-flow drill-down smoke check.

No production code (`src/`) was touched. The only source changes are the
two accessibility keywords above and the one new test file.

---

## Rate-limit safety

Confirmed before any live call: `GITHUB_TOKEN` present (User scope, bridged
into Process scope for the invoking command only; length checked, value
never printed). The GraphQL envelope confirmed DEFAULT OFF from code before
running (`RepositoryEnvelopeEnabled` defaults `false`, no config overrides
it) — carried over unchanged from the first attempt's finding.

No rate limit, secondary rate limit, or abuse-detection response was
provoked or encountered. Observed core rate-limit remaining fell from
~4906/5000 (session start) to ~3725/5000 (after Stage B), comfortably within
budget for a single-run validation of this size.

---

## Stage A — known comparison shapes

Run against `fatihGunalp`, `torvalds`, `karpathy` in that order, via
`dotnet test --filter "FullyQualifiedName~ProfileRealValidationHarness"`.

**First run surfaced a real, bounded defect — in this session's own new
test-only harness, not in production code:**

`ProfileEvidenceReference.EvidenceKey` is documented
(`ProfileEvidenceKeys.cs`) to name either "a canonical signal key, or the
normalized snapshot data key for non-signal evidence such as languages." The
harness's first traceability check looked only at `SignalResult.Evidence`
and never at `RepositorySnapshot.Languages`, so **every** language-based
evidence reference (the only kind these three profiles' promoted patterns
happened to produce) came back FAIL — 6/6 broken across the three profiles.
This is a **MECHANICAL** defect (test-only validation logic, not a
production/domain/evidence-semantic issue) and was corrected in-session per
the accelerated-workflow rule: `TryFollowTraceabilityAsync` now checks
`ProfileEvidenceKeys.TryReadLanguage` first and resolves against
`RepositorySnapshot.Languages` for that shape, falling through to the
existing `SignalResult.Evidence` check otherwise. Rebuilt (0 warnings) and
**Stage A was rerun in full** rather than patched around, so every number
below is from the corrected harness.

### Real profile matrix — Stage A

Numbers below are from the **final** rerun (the one that also produced the
rendered-text prohibited-inference proof; see Closure section). An
intermediate rerun, done immediately after the traceability fix, reproduced
`discovered`/`admitted`/`completed` and `GitHub requests` **exactly** for all
three profiles versus what is shown here — the two Session 130 live
invocations of Stage A agree with each other down to the request count, and
both agree with Session 120's shape (see comparison table below).

| Profile | Observable shape | Discovered | Admitted | Completed | Incomplete | Result | Elapsed | GitHub requests | GraphQL requests |
|---|---|---|---|---|---|---|---|---|---|
| fatihGunalp | small (P9: contents-heavy) | 5 | 4 | 4 | 0 | Complete/composed | 18.840 s | 63 | **0** |
| torvalds | medium (P9: heavy commit history) | 12 | 9 | 9 | 0 | Complete/composed | 55.361 s | 130 | **0** |
| karpathy | dense (P9: many repositories) | 63 | 54 | 54 | 0 | Complete/composed | 252.329 s | 662 | **0** |

No `graphql` key appears in any run's `RequestCategories` — the authoritative
envelope issued zero physical requests across all three profiles, matching
its DEFAULT OFF configuration.

### Per-profile inventory / result

- **fatihGunalp:** eligibility Contributing=2, NonContributing=1,
  Restricted=2; admission Analyze=4, Skip=1 (ForkDetected=1). 0 limitations,
  0 warnings, 1 repeated pattern, representative repository
  `fatihGunalp/CleanArchitectureNetCore`.
- **torvalds:** eligibility Contributing=6, NonContributing=3, Restricted=3;
  admission Analyze=9, Skip=3 (ForkDetected=3). 0 limitations, 0 warnings,
  4 repeated patterns, representative repository `torvalds/1590A`.
- **karpathy:** eligibility Contributing=10, NonContributing=9,
  Restricted=44; admission Analyze=54, Skip=9 (ForkDetected=9).
  0 limitations, 0 warnings, 10 repeated patterns, representative repository
  `karpathy/EigenLibSVM`.

Forks were consistently excluded from admission (`ForkDetected`) rather than
silently analyzed or silently dropped from the count — coherent with the
canonical eligibility contract in all three shapes.

### Traceability (post-fix)

Two `WhyThisConclusion` evidence references sampled per profile (both were
`language.*` keys in all three — the only kind these three profiles'
promoted patterns produced):

| Profile | Reference 1 | Reference 2 | Overall |
|---|---|---|---|
| fatihGunalp | CleanArchitectureNetCore / language.C# — PASS | claude-code-dotnet-sdk-skill / language.C# — PASS | **PASS** |
| torvalds | AudioNoise / language.C — PASS | GuitarPedal / language.C — PASS | **PASS** |
| karpathy | EigenLibSVM / language.C++ — PASS | MatlabWrapper / language.C++ — PASS | **PASS** |

Broken traceability count: **0** (post-fix, all three profiles).

### Prohibited inference

Two layers — see "Closure — rendered-text prohibited-inference proof" below
for why both are needed and what layer 2 actually checks:

| Profile | PROHIBITED_INFERENCE_VOCAB (codes/keys) | PROHIBITED_INFERENCE_TEXT (rendered narrative, both cultures) |
|---|---|---|
| fatihGunalp | PASS | PASS (60 surfaces scanned) |
| torvalds | PASS | PASS (82 surfaces scanned) |
| karpathy | PASS | PASS (114 surfaces scanned) |

### Session 120 comparison

| Profile | Metric | Session 120 (2026-08-24) | Session 130, first rerun | Session 130, final rerun (closure) | Classification |
|---|---|---|---|---|---|
| fatihGunalp | discovered/admitted/completed | 5/4/4 | 5/4/4 | 5/4/4 | STRUCTURAL STABILITY (exact, x3) |
| fatihGunalp | requests | 63 | 63 | 63 | STRUCTURAL STABILITY (exact, x3) |
| fatihGunalp | elapsed | 19.989 s | 18.084 s | 18.840 s | EXPECTED PROVIDER/NETWORK VARIABILITY |
| torvalds | discovered/admitted/completed | 12/9/9 | 12/9/9 | 12/9/9 | STRUCTURAL STABILITY (exact, x3) |
| torvalds | requests | 131 | 130 | 130 | EXPECTED PROVIDER/PROFILE EVOLUTION (1-request drift vs Session 120 only, consistent with a single pagination-boundary shift; reproduced exactly across both Session 130 runs) |
| torvalds | elapsed | 59.333 s | 54.782 s | 55.361 s | EXPECTED PROVIDER/NETWORK VARIABILITY |
| karpathy | discovered/admitted/completed | 63/54/54 | 63/54/54 | 63/54/54 | STRUCTURAL STABILITY (exact, x3) |
| karpathy | requests | 662 | 662 | 662 | STRUCTURAL STABILITY (exact, x3) |
| karpathy | elapsed | 259.738 s | 246.391 s | 252.329 s | EXPECTED PROVIDER/NETWORK VARIABILITY |

No POSSIBLE REGRESSION and no INCONCLUSIVE classification was needed. Across
**three** independent live invocations now (Session 120, and Session 130's
two reruns two days later), two of three profiles reproduced their exact
discovered/admitted/completed *and* request counts every time, and the third
(torvalds) differed by exactly one request from Session 120 while matching
itself exactly across both Session 130 runs — strong evidence that the
request-count law Session 120 derived
(`7 × admitted + Npf + Nwf + min(Ndoc,16) + Pc+Pi+Pp+Pr+Pt`) still holds
today with GraphQL still off, and that nothing in the request graph changed
structurally across any of the three runs. Wall-clock times varied by a few
percent across all three; treated as ordinary network/provider variability,
not a performance claim in either direction (§18.11 discipline: request
counts only, no latency claim).

---

## Stage B — edge composition

Rate-limit capacity after Stage A (~3725–3853/5000 remaining) was safely
sufficient to proceed. Three candidates were selected by **observable public
repository composition only**, found via bounded, unauthenticated GitHub
Search/REST reconnaissance (never touching the application's credential path
or its authenticated budget) and chosen for being **small** — minimizing
provider budget — rather than for any characteristic of the account holder:

| Shape | Login | Observed composition (unauthenticated, pre-selection) |
|---|---|---|
| Sparse / few public repositories | `imArafatHussein` | 1 public repository, non-fork, non-archived |
| Fork-heavy | `newluckybaby` | 12 public repositories, 9 forks (75%) |
| Archive-heavy | `asynkayo` | 4 public repositories, 2 archived (50%), non-fork |

No shape needed a COVERAGE GAP — a safe candidate was found for all three
within a bounded amount of reconnaissance.

### Real profile matrix — Stage B

Numbers below are from the final rerun (see Closure section); the
intermediate rerun reproduced identical `discovered`/`admitted`/`completed`
and `GitHub requests` for all three.

| Profile | Observable shape | Discovered | Admitted | Completed | Incomplete | Result | Elapsed | GitHub requests | GraphQL requests |
|---|---|---|---|---|---|---|---|---|---|
| imArafatHussein | sparse | 1 | 1 | 1 | 0 | Complete/composed | 4.480 s | 14 | **0** |
| newluckybaby | fork-heavy | 12 | 3 | 3 | 0 | Complete/composed | 10.795 s | 35 | **0** |
| asynkayo | archive-heavy | 4 | 4 | 4 | 0 | Complete/composed | 14.494 s | 50 | **0** |

### Per-profile inventory / result

- **imArafatHussein (sparse):** eligibility Restricted=1; admission
  Analyze=1, no skips. **1 limitation: `LimitedIndependentEvidenceSurface`**
  — phrased as an evidence-surface boundary, never a developer judgment
  (matches the canonical acceptable-phrasing example exactly: a limitation
  about available evidence, not about the person). 0 patterns, no
  representative repository (correct: insufficient independent repetition
  for promotion). No `WhyThisConclusion` evidence references existed to
  trace — recorded as a bounded fact, not a failure, per the session's own
  instruction for profiles with fewer than two claims.
- **newluckybaby (fork-heavy):** eligibility NonContributing=9,
  Restricted=3; admission Analyze=3, Skip=9 (ForkDetected=9) — the 9 forks
  were correctly excluded from admission, confirming coherent fork-exclusion
  behavior at this ratio. 0 limitations, 0 patterns, no representative
  repository, no traceable claims (bounded fact).
- **asynkayo (archive-heavy):** eligibility Restricted=4; admission
  Analyze=4, no skips — **archived repositories were admitted for analysis**
  rather than excluded outright, consistent with the canonical model
  (`Archived` restricts specific contribution purposes; it is not itself an
  admission-skip reason the way `ForkDetected`/`EmptyRepository` are). 0
  limitations, 0 patterns, no representative repository, no traceable claims
  (bounded fact).

### Prohibited inference (Stage B)

| Profile | PROHIBITED_INFERENCE_VOCAB (codes/keys) | PROHIBITED_INFERENCE_TEXT (rendered narrative, both cultures) |
|---|---|---|
| imArafatHussein | PASS | PASS (58 surfaces scanned) |
| newluckybaby | PASS | PASS (60 surfaces scanned) |
| asynkayo | PASS | PASS (58 surfaces scanned) |

Notably, the one limitation surfaced (`LimitedIndependentEvidenceSurface` for
the sparse profile) — both as a code AND as its actual rendered sentence
("The public profile contains a limited independent evidence surface." /
"Herkese açık profil sınırlı bir bağımsız kanıt yüzeyi içeriyor.") — is the
textbook example of acceptable phrasing the session brief itself names: about
the public evidence surface, never about the developer.

---

## Web / Profile flow smoke

Started the production Web app locally (`dotnet run --project
src/GitHubProof.Web --urls http://localhost:5299`, `GITHUB_TOKEN` bridged
into that process only) and drove it through the Browser tool — no new
browser framework or package added.

- **Small profile (`fatihGunalp`):** landing page → unified entry form →
  `/Profile` POST → report rendered successfully. "What does this GitHub
  show?" narrative used strictly evidence-based, non-judgmental language
  ("C# recurs across different repositories... a distinct recurring method
  does not appear in how the work is carried out" — descriptive, never
  "senior"/"weak"/"inactive"). Limitations section correctly rendered "no
  boundary was recorded for this profile view" (neutral, not a positive
  developer claim). **Repository drill-down verified working end-to-end**:
  clicked through to `fatihGunalp/CleanArchitectureNetCore`'s Repository
  Proof page, which rendered strengths, gaps ("Zayıf Noktalar" = missing
  tests/setup guide/PR usage — repository practice gaps, not developer
  judgments), improvement actions, and signal-level detail.
- **Denser profile (`torvalds`):** same flow, rendered multiple repeated
  patterns (issue tracking, pull-request-driven changes, releases) across
  multiple representative repositories, same evidence-first, non-judgmental
  language throughout.
- Both routes returned a successful result with no error page, no
  unhandled exception, and no locale/presentation re-analysis observed.
  **HUMAN VISUAL CHECK REQUIRED** is not newly asserted here — pixel-level
  sign-off is unchanged Milestone B residual risk, unaffected by this
  session — but functional rendering and drill-down were directly observed
  via the browser, not merely assumed.

Server stopped and browser tab closed after the smoke check; port
confirmed free afterward.

---

## Provider failure safety

No provider failure — rate limit, forbidden, unavailable, partial
collection, timeout, or network failure — occurred naturally during this
session's six live runs. Nothing was provoked. This is a bounded fact about
this run, not a claim that such failures cannot occur; the existing
contracts governing that distinction (provider/system failure ≠ negative
evidence ≠ `Missing` ≠ developer statement) were not exercised live in this
session and remain proven by the existing deterministic test suite instead.

---

## Determinism

Not tested by calling GitHub twice and comparing live results — live GitHub
state can legitimately change between calls, and the session brief
explicitly forbids treating that as a determinism test. Canonical
determinism (same captured inputs + same versions ⇒ same semantic output)
remains owned by the existing P9 deterministic scenario suite, unchanged and
green (see Validation below). This session's live runs test **integration**
against real provider behavior, not determinism.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 warnings, 0 errors
Domain          842/842
Application     262/262
Infrastructure  636/636
Web             552/552   (was 551; +1 new opt-in-disabled LiveIntegration test)
TOTAL          2292/2292
git diff --check                              clean
~~~

The one new test (`ProfileRealValidationHarness.ValidateAsync_...`) is
`[Trait("Category","LiveIntegration")]` and opt-in — proven, not assumed:
the full default suite run above completed in well under a second per
assembly, meaning the live path took zero network time when the opt-in
environment variables are unset, exactly mirroring Session 120's own
opt-in-isolation proof for its sibling harness.

Independent closure review: **PASS WITH FINDINGS** (0 blocker, 0 high,
0 medium — see Independent review section below for the one non-blocking
note). The technical implementation (harness composition reuse, the
evidence-key traceability fix, the prohibited-term scan scope, credential
non-leakage, resource cleanup) was independently re-verified against the
actual diff and re-derived build/test evidence.

---

## Independent review

An initial review pass correctly flagged that the worklog, at the moment it
ran, still reflected the **prior** BLOCKED attempt's narrative ("no code
changed", "no live call was made") while the actual working tree already
contained this continuation's real code and results — a genuine
inconsistency between what was checked in and what had actually happened,
caught because the review was invoked before this file was rewritten for the
continuation. That was a sequencing mistake in this session (review run
before the worklog update it needed to see), not a finding about the
underlying work. This file, `CURRENT.md`, and `INDEX.md` were then brought
current with the actual Stage A/B results, and the review's remaining
technical findings — all PASS — stand as recorded in Validation above.

A **second** review pass (architecture-owner) found one further, genuine gap
after this record first closed — see "Closure — rendered-text
prohibited-inference proof" immediately below, which documents what the gap
was and how it was closed. That review's technical findings, once the gap
was closed, are recorded there.

---

## Closure — rendered-text prohibited-inference proof

**The gap.** The session brief required, for every real profile, "inspect
generated structured/report text for prohibited developer judgments." The
prohibited-inference check this record originally reported only scanned
closed system vocabulary — limitation/warning enum **codes** and pattern
family/**key** identities (`PROHIBITED_INFERENCE_VOCAB` above) — never the
actual **rendered narrative text** a reader sees. The Web smoke separately
inspected real rendered text, but only for two of the six profiles
(`fatihGunalp`, `torvalds`). So "prohibited inference PASS for all six
profiles" was asserted but not yet fully evidenced for the other four, and
not evidenced by rendered text for any of the six via the automated check.

**The fix (test-only, mechanical).** `ProfileRealValidationHarness` gained a
second, authoritative scan, `PROHIBITED_INFERENCE_TEXT`. For every live
profile it renders every `ProfileWebPresentationLocalizer` surface that
profile's `ProfileReport` actually reaches — in **both** shipped cultures
(`en`, `tr`) — using the SAME calls the Web layer itself makes: section
labels, empty-section copy (only for sections this profile's report actually
shows empty), the Meaning synthesis narrative, repository-status labels
(only for eligibility statuses this profile actually has), limitation/warning
labels (only for codes this profile actually has), and pattern-driven
Why/label text (only for patterns this profile actually promoted) — then
scans every rendered sentence against the session's banned-term list.

A placeholder (`placeholder-identity` / `placeholder-owner/placeholder-repo`)
is substituted **only** for the two GitHub-owned parameters these methods
take — profile identity and repository full name — never for a pattern key.
A `TechnicalFocus` pattern key is the literal GitHub-detected programming
language name; it is technical/factual metadata, not free-form user
narrative, and is not in the session brief's explicit exclusion list
(README text, repository description, profile bio, repository/login names),
so it is passed through as the real value — the rendered sentence a reader
actually sees is scanned as-is. No production code changed; no browser
framework was added (`ProfileWebPresentationLocalizer` is instantiated
directly in the test, per the closure instruction's stated preference).

A hard assertion was added: if `PROHIBITED_INFERENCE_TEXT` finds a match for
**any** profile, the test fails loudly (`Assert.True`) — but only *after*
the full evidence artifact is written to disk, so a B-PREPROD-BLOCKED finding
is never lost to a stack trace.

**The rerun.** The six `ProfileReport` objects from the original Stage A/B
runs were not persisted in a form the new text scan could run against
retroactively (only markdown summaries were written, by design — see
"Do not put secrets... unnecessary user/profile payloads into worklog").
Per the closure instruction, the six-profile validation was rerun exactly
**once**. Rate-limit headroom was confirmed safe beforehand via the
non-counting `GET /rate_limit` endpoint: `5000/5000` remaining (the earlier
runs' window had fully reset), comfortably sufficient for one more six-profile
pass. All six profiles were run in a single invocation
(`GITHUBPROOF_BENCHMARK_PROFILES=fatihGunalp,torvalds,karpathy,imArafatHussein,newluckybaby,asynkayo`)
rather than two separate Stage A/B calls, so exactly one rerun round covered
all six.

**Result — PROHIBITED_INFERENCE_TEXT, all six profiles:**

| Profile | Rendered surfaces scanned (both cultures) | Result |
|---|---|---|
| fatihGunalp | 60 | **PASS** |
| torvalds | 82 | **PASS** |
| karpathy | 114 | **PASS** |
| imArafatHussein | 58 | **PASS** |
| newluckybaby | 60 | **PASS** |
| asynkayo | 58 | **PASS** |

No system-generated prohibited developer judgment was found in any rendered
surface, in either culture, for any profile. The rerun also reproduced every
`discovered`/`admitted`/`completed`/`GitHub requests` count from the prior
Stage A/B runs exactly (see the updated matrices and comparison table above),
so this closure did not introduce any new observation about the product —
only additional, previously-missing proof for a criterion this record had
already (correctly, as it turns out) claimed to meet.

**Revised validation evidence:**

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 warnings, 0 errors
Domain          842/842
Application     262/262
Infrastructure  636/636
Web             552/552
TOTAL          2292/2292
git diff --check                              clean
~~~

Independent closure review (second pass, post-fix): **PASS**. Confirmed from
actual repo state, not narrative: the diff is exactly one new test file
(`ProfileRealValidationHarness.cs`) plus the two pre-existing accessibility
changes in `ProfileRuntimeBenchmarkHarness.cs`, zero `src/` changes; the
`ProfileEvidenceKeys.TryReadLanguage`-based traceability fix from the first
close is unchanged and still correct; the new `CollectSystemNarrativeSurfaces`
method matches this section's description exactly (placeholder substitution
scope, culture coverage, profile-shape-driven surface selection); build and
full suite reproduced independently at `0 warnings` / `2292/2292`; and the
worklog (this file, `CURRENT.md`, `INDEX.md`) is now internally consistent
with the actual rerun evidence, with no residual claim that the
vocabulary-only scan alone proved the text-inspection requirement.

---

## Not done / not touched

- **No production code changed, in either closure round.** Only two
  test-file accessibility keywords and one new test-only file
  (`ProfileRealValidationHarness.cs`, extended in-session for the
  rendered-text scan; still test-only).
- **No GraphQL envelope enabled** — confirmed DEFAULT OFF throughout, zero
  authoritative GraphQL requests across all six live runs, both reruns.
- **No S4, no batching/aliases, no DecisionBounded GraphQL revisit, no
  error-message parsing, no Session 126 residual resolution.**
- **No production runtime value selected** (D-1/D-2/D-3/D-5 remain open,
  unchanged).
- **No deploy, DNS, VPS, secret, beta credential, or reverse-proxy change.**
- `artifacts/` untouched.
- No commit, no push.

---

## Readiness verdict

**B-PREPROD-READY-WITH-FINDINGS.**

No blocker was found that prevents proceeding to Production Runtime
Decisions:

- the analysis completed for all six real profiles;
- repository inventory/admission behaved coherently across small, medium,
  dense, sparse, fork-heavy, and archive-heavy shapes;
- no provider/system failure became developer truth (none occurred to
  misclassify, and the contracts governing that distinction are otherwise
  unchanged and green);
- evidence traceability is intact (0 broken, post-fix);
- report generation completed for all six;
- sparse/fork/archive conditions produced limitations/context language only,
  never a negative developer judgment — proven not just at the code/key
  level but in the actual **rendered narrative text**, in both shipped
  cultures, for **all six** profiles (`PROHIBITED_INFERENCE_TEXT`, 0 matches
  across 432 total rendered surfaces);
- runtime/request behavior was directly observed and matches Session 120's
  historical shape almost exactly;
- the Web/Profile flow consumed and rendered both a small and a denser
  result, including a working repository drill-down;
- GraphQL stayed DEFAULT OFF with zero authoritative requests throughout.

Two bounded, non-blocking findings, both in this session's own new
*test-only* validation code, both found and closed in-session:

1. a traceability-verification defect on the first live run (checked only
   the signal-evidence shape, not the documented snapshot-data shape for
   languages), fixed and re-verified clean;
2. an evidence-completeness gap raised by architecture-owner review after
   this record first closed — the automated prohibited-inference check had
   only scanned closed vocabulary, never actual rendered report text — closed
   by adding the `PROHIBITED_INFERENCE_TEXT` scan and rerunning all six
   profiles once (see Closure section above).

Neither finding touched production code, and neither reflects a defect in
the Profile Proof product.

**Recommendation: GO TO PRODUCTION RUNTIME DECISIONS.**

The Session 126 secondary-rate-limit residual remains the sole named
blocker for any GraphQL production/beta/live-A/B/default-on enablement —
unrelated to and unaffected by this session, since the envelope stayed off
throughout.

---

## Git

~~~text
git diff --check     clean
git status --short   1 modified test file (accessibility only), 1 new/extended
                      test-only file, worklog files, plus pre-existing
                      ?? artifacts/
~~~

No commit, no push, no deploy.
