# S2 — GraphQL Languages Shadow Parity

**Session:** 125
**Date:** 2026-08-25
**Route:** Opus / high
**Result:** PASS — S2 implemented and validated (build 0 warnings,
**2033/2033**). Three review rounds; F-1..F-6, G-1..G-5 and M-1/L-1/L-2/L-3 are
all closed, with no unresolved code or review finding. The mechanics are proven,
and **`LanguagesShadowEnabled` stays DEFAULT OFF and is NOT approved for
enablement** — the provider-wide HTTP 200 + `DataWithErrors` classification
blocker is separate and still open; see below

---

## Goal

Prove the first real REST ↔ GraphQL evidence-surface parity on `languages`,
without changing any Profile result — slice **S2** of
`docs/architecture/public-profile-runtime-collection.md` §§18-19.

The core rule of the slice is one sentence: **REST remains authoritative.** Even
with the shadow on, the languages map, the `languages` DataAvailability and its
`RetrievedAt` all come from the REST collector, and no shadow outcome —
match, mismatch, failure or bug — may produce evidence, a warning, a limitation,
a score change, a status change or a report change.

---

## Starting State

~~~text
Branch       session/125-graphql-languages-shadow-parity
Base         bf5fc8e  (== main == origin/main, verified with git rev-parse)
             bf5fc8e = "feat(profile): add isolated GraphQL transport client"
                       (Session 124, S1)
Working set  clean
Pre-existing ?? artifacts/  (user-owned, untouched, out of scope)
~~~

Precondition PASS. The S1 transport was verified present on `main` before any
code was written, so this slice builds on a committed seam.

---

## Model Route

Opus / high. Unlike S1 this slice touches the production collection path, so the
failure mode is silent evidence drift rather than unused code being wrong — a §6
semantic-equivalence concern. Three escalation triggers were declared up front
(seam requiring a canonical ownership change, OFF↔ON parity requiring
orchestrator/composer changes, telemetry requiring a new canonical decision);
none of the first two fired, and the third was resolved by *not* adding the
telemetry (below).

---

## Context Read

~~~text
AGENTS.md · CLAUDE.md
agents/02-context-loading.md · agents/04-development-workflow.md
docs/architecture/public-profile-runtime-collection.md
    §3.1, §3.2 · §7 · §18.1 · §18.3 · §18.4 · §18.7 · §18.9 · §18.12 · §19
src  GitHubRepositoryDescriptiveDataCollector (languages normalization)
     GitHubRepositoryDescriptiveDataCollection · IGitHubRepositoryCollectors
     GitHubRepositorySnapshotOrchestrator.CreateProfileComposition
     RepositorySnapshot.NormalizeLanguages · ProfileEvidenceKeys
     S1 transport: IGitHubGraphQlTransport, GitHubGraphQlResponse
     ProfileAdmissionOptions (configuration pattern)
tests existing composition/wiring/telemetry conventions and fixtures
~~~

Sessions 121 and 124 were read as history only.

---

## The shadow seam

The smallest Profile-only seam that leaves the production language collector in
place: **one optional argument on the Profile composition factory**.

~~~text
CreateProfileComposition(httpClient, timeProvider)                 <- unchanged
CreateProfileComposition(httpClient, timeProvider, languagesShadow) <- S2 overload
~~~

It is a separate **overload**, not a third optional parameter. The first attempt
used an optional parameter and broke an existing method-group conversion in
`ProfileRepositoryCollectionCompositionTests`; the overload keeps the
two-argument signature byte-identical for every caller that predates S2.

Inside, a non-null shadow wraps the composition's own
`GitHubRepositoryDescriptiveDataCollector` in `ShadowedDescriptiveDataCollector`.
That decorator:

1. calls the REST collector exactly as today,
2. runs the shadow **only** if the REST `languages` availability is `Available`,
3. discards the outcome,
4. returns **the same collection instance** the REST collector produced.

Step 4 is what makes "REST remains authoritative" structural rather than a
promise: there is no copy, no rebuild and no merge, so the languages map, the
`languages` DataAvailability, its `RetrievedAt`, the README, the license and
every other availability entry are literally the REST collector's own values.

Two containment rules in the decorator:

- caller cancellation propagates and is never converted into a shadow outcome
  (§11.1, §18.8);
- **any other** shadow exception is contained. A bug in an observation must not
  change a collection result, and that has to hold for a bug just as much as for
  a provider error.

Repository Proof is untouched: `CreateDefault` has no shadow parameter, and a
test asserts that the overload exists only on the Profile factory (§3.1).

---

## Default-off

~~~text
ProfileGraphQlShadowOptions.LanguagesShadowEnabled = false
section: "ProfileGraphQlShadow"
~~~

One boolean, deliberately not a feature-flag framework, and no validation rule —
a boolean has no invalid value and inventing one would be a policy this slice
does not own.

Off is not "a shadow that decides to do nothing": `CreateLanguagesShadow` returns
**null**, so the composition constructs the same collectors it did in Session
099, the transport is never resolved, and no GraphQL request can be issued at
all. That is why the default path needs no behavioural argument — there is
nothing there to behave.

---

## GraphQL query and pagination

~~~graphql
query ProfileRepositoryLanguages($owner: String!, $name: String!, $first: Int!, $after: String) {
  repository(owner: $owner, name: $name) {
    languages(first: $first, after: $after) {
      pageInfo { hasNextPage endCursor }
      edges { size node { name } }
    }
  }
}
~~~

- Repository identity travels in **variables**; the operation text is a constant
  and never carries an owner or name.
- `first: 100`, the connection maximum. Pagination follows `endCursor` until
  `hasNextPage` is false — no arbitrary cap, no sampling, no batching across
  repositories, no concurrency.
- **No `totalCount`**, no `totalSize`, no `isEmpty`, and no field belonging to
  any other surface. §18.12 forbids adopting exact-total provider fields;
  requesting one that is never read would still be a step toward it. A test
  asserts the query text contains none of them.
- Each physical POST is one `graphql` request in the existing closed telemetry
  category — proven for a two-page run, so pages are counted per POST and never
  per language.

Two pagination guards abstain rather than guess: a connection that promises
another page without a usable cursor, and a cursor that does not advance. Both
return `NotCompared`; looping or comparing a knowingly incomplete map would be
worse than saying nothing.

---

## Normalization and parity

Parity is semantic equality of the **complete** normalized `language -> bytes`
map, reproduced from the REST path rather than guessed:

~~~text
REST  Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
      key trimmed and required non-empty
      negative byte count rejected
      duplicate key rejected (the collector throws)
~~~

The shadow uses the same comparer, the same trimming and the same rejections, so
"equal" means equal under exactly the production rules. GraphQL edge order cannot
matter, because both sides are dictionaries.

Where REST *throws* on ambiguity the shadow **abstains** — it must not be more
permissive than the path it is checking, and it must not throw into a production
call. Duplicate language names after the same normalization, negative sizes,
blank names, missing nodes and non-numeric sizes all yield `NotCompared`, never
a last-wins verdict.

`GraphQlLanguagesShadowOutcome` is `NotCompared | Match | Mismatch`. It is not a
domain status: no `DataAvailability`, no signal status, no warning, no limitation
(§18.7).

---

## GraphQL failure and partial data

S1 outcome semantics are reused unchanged:

~~~text
Success          -> normalize and compare
DataWithErrors   -> NotCompared; the partial map is never consumed (§18.4)
EnvelopeFailure  -> NotCompared
Malformed        -> NotCompared
cancellation     -> propagates
~~~

Because REST is authoritative, every one of these leaves the returned collection
exactly unchanged — proven by fingerprinting the whole snapshot across all of
them.

---

## The telemetry gap as first shipped — SUPERSEDED by the F-2 closure

§18.9 authorizes a GraphQL **request** count, a fallback count, the
run-disabled flag and per-surface request attribution. It does **not** authorize
a shadow match/mismatch counter, and adding one would be a canonical telemetry
decision this slice does not own.

So, as first shipped, the outcome was discarded in production and the comparison
was observable only through the shadow component and its tests, and the gap was
reported rather than closed.

**Superseded.** The architecture owner rejected that reading — §9 permits a
richer surface subject to §9.1, and §18.9 extends §9 rather than replacing it —
and accepted F-2. The four bounded values described in the F-2 closure above are
now recorded. This section is kept as the state at first delivery, not as the
current state.

---

## Owner decision and F-1 / F-2 / F-3 closure

The architecture owner **accepted F-1, F-2 and F-3**, ruled **no action** on F-6,
and F-4/F-5 had already been mechanically closed. All three acceptances are now
implemented.

### F-1 — §18.6 execution-scoped disable

**The classification question came first, and the answer was a gap.** The owner
required that a §18.6 provider-wide failure be distinguished from an ordinary
partial-data error using **only bounded structural information**, and that the
gap be reported rather than papered over if the information is not there.

It is not there. `GitHubGraphQlResponse.DataWithErrors` is constructed with
`HttpStatusCode.OK` hard-coded and carries only an `ErrorCount`; S1 deliberately
normalized away the error `type`, `path` and `extensions` so nothing could leak
into telemetry. **There is therefore no bounded structural way to tell an auth or
rate-limit failure from an ordinary partial-data error.**

Consequently:

~~~text
EnvelopeFailure  -> disables the shadow for the rest of the execution
Malformed        -> disables the shadow for the rest of the execution
DataWithErrors   -> per-repository abstention only; does NOT disable
~~~

`DataWithErrors` is left alone deliberately. Classifying it from message text is
forbidden, and classifying **every** such response as provider-wide cannot be
justified against §18.5 — most of them are exactly the partial-data case §18.5
handles per surface. **Reported as an S1 classification gap:** if `DataWithErrors`
should ever participate in §18.6, S1 must first expose a bounded structural
discriminator (an error-category enum or the HTTP-level signal), which is a
contract and transport decision, not something this slice may invent.

**The seam.** `GraphQlLanguagesShadowState` + `GraphQlLanguagesShadowScope`,
mirroring the existing `ProfileGitHubBudgetScope` exactly: an `AsyncLocal` state
begun and disposed inside one `AnalyzeAsync`. The owner warned against a mutable
flag whose lifetime can outlive one call, and that is precisely why an
instance-level field was rejected — `LiveProfileReportService` is scoped and can
be called more than once per DI scope, so a field on the shadow or on the
service could leak between executions. The scope cannot: it is restored on
dispose.

`GitHubGraphQlLanguagesShadow` consults the scope before sending. Once disabled,
later repositories **return without any POST at all**. With no ambient scope — a
direct unit-test call — behaviour is exactly as before.

No retry, no backoff, no numeric circuit policy, nothing persisted, nothing
global, nothing shared. Caller cancellation never disables.

### F-2 — bounded shadow telemetry

Four values, recorded once at the end of the execution from the same scoped
state:

~~~text
LanguagesShadowMatch        int
LanguagesShadowMismatch     int
LanguagesShadowNotCompared  int
GraphQlDisabledForRun       bool
~~~

They are **not** HTTP request counts and are never added to `GitHubRequests`;
they are never evidence and reach no Domain, Application or report type; they
carry no repository identity, no language name, no byte count, no query text, no
variables, no payload and no error content. `RecordLanguagesShadow` accepts three
`int`s and a `bool` and nothing else, which is asserted by test. The log renders
a `LanguagesShadow={…}` section **only** when something was observed, so a
default-off execution logs exactly what it logged before S2.

One consequence surfaced while testing and was closed: a shadow exception
contained by the decorator previously recorded nothing at all. It now records
`NotCompared`, so a contained shadow bug is operationally visible instead of
silent — which is what the owner's "failed/unusable comparison increments
NotCompared" rule requires.

### F-3 — the two untested decorator branches

Both are now covered at decorator level, through the real composition:

- **Caller cancellation** propagates unchanged, is not swallowed by the shadow
  exception isolation, does not become `NotCompared` and does not set the §18.6
  disable.
- **REST languages unavailable**: when the authoritative availability is not
  `Available` the shadow is not invoked, **zero** `/graphql` requests are sent
  for that repository, and the REST failure keeps its own status, reason and
  `RetrievedAt` — compared against a shadow-free run of the same fixture.

### F-6 — no action, by owner ruling

The cursor-cycle guard is left as it is. The existing duplicate normalized-language
protection prevents the identified repeated-page cycle from becoming an infinite
successful enumeration, and S2 is not broadened with a generic cursor-cycle
framework.

---

## Owner decision and G-1 / G-2 closure

The architecture owner **accepted G-1 and required the fix**, and **accepted G-2
as a reasoning correction while rejecting the inference it might invite**.

### G-1 — a client transport timeout is a §18.6 transport failure

`HttpClient` surfaces its own `Timeout` as a `TaskCanceledException` whose token
is not the caller's. The transport previously matched neither catch clause, so
the exception escaped, the decorator contained it, and the run was **not**
disabled — a systemic timeout was paid once per admitted repository, the most
expensive shape of the multiplication §18.6 exists to prevent.

~~~text
OperationCanceledException, supplied token IS cancelled      -> propagates (§11.1)
OperationCanceledException, supplied token NOT cancelled     -> EnvelopeFailure
HttpRequestException                                          -> EnvelopeFailure
~~~

The two are separated by the **supplied token alone**, never by exception type:
the filtered clause runs first, so a genuine caller or execution-deadline
cancellation can never reach the new one. `EnvelopeFailure` then drives the
existing run-scoped disable unchanged. No retry, no backoff, no numeric policy.

### G-2 — the reasoning was wrong; the behaviour stays

The earlier claim that there is "no bounded structural discriminator" was too
absolute, and it is corrected in the code comment and in this record. A
`DataWithErrors` envelope **can** carry a null `data`, and that is a bounded
structural fact.

It is also **not proof of anything the owner ruled it must not be taken for**:
not an authentication failure, not an authorization failure, not a primary or
secondary rate limit, not a server-wide or provider-wide fault. An ordinary
top-level query error has exactly the same shape. So S2 behaviour is unchanged:

~~~text
DataWithErrors -> this comparison is NotCompared
               -> REST remains authoritative
               -> does NOT trigger the run-scoped disable
~~~

No message text is parsed, no error-code semantics are invented, and no raw
`path` or `extensions` is retained to force a closure.

### Named pre-enablement blocker

**S1 has no proven bounded structural classification capable of distinguishing a
provider-wide GraphQL failure carried as HTTP 200 + `DataWithErrors` from an
ordinary GraphQL partial or query error.**

Therefore Session 125 proves the **languages parity shadow mechanics**, and
`LanguagesShadowEnabled` remains **DEFAULT OFF** and is **NOT approved** for:

~~~text
production enablement
beta enablement
live A/B validation (§19.14)
default-on use
~~~

Before any of those, a separate bounded session must establish a proven GraphQL
error-kind classification for the §18.6 provider-wide `DataWithErrors` cases,
using provider-supported structural information.

§18.6 is **not** weakened and is **not** claimed fully closed for every possible
`DataWithErrors` provider response. This is an **operational enablement
blocker**, not evidence or domain drift: with the shadow off the Profile path is
exactly the pre-S2 path, and even with it on REST remains authoritative, so no
evidence, score, status or report can move either way.

---

## Changes

~~~text
Created (production):
- src/GitHubProof.Infrastructure/GitHub/Profile/GraphQlLanguagesShadowScope.cs
    (GraphQlLanguagesShadowState + the AsyncLocal execution scope)
- src/GitHubProof.Infrastructure/GitHub/Profile/IGitHubGraphQlLanguagesShadow.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlLanguagesShadow.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/GraphQlLanguagesShadowOutcome.cs
- src/GitHubProof.Infrastructure/GitHub/Profile/ShadowedDescriptiveDataCollector.cs
- src/GitHubProof.Web/Profile/Runtime/ProfileGraphQlShadowOptions.cs

Modified (production):
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlTransport.cs
    the G-1 clause: an OperationCanceledException whose supplied token is NOT
    cancelled is a client transport timeout and becomes an EnvelopeFailure
- src/GitHubProof.Web/Profile/Runtime/ProfileExecutionTelemetry.cs
    four bounded shadow values + RecordLanguagesShadow + conditional rendering
- src/GitHubProof.Web/Profile/LiveProfileReportService.cs
    begins the shadow scope alongside the existing telemetry and budget scopes,
    and records the counters before Complete
- src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
    the S2 CreateProfileComposition overload; the existing signature delegates
    to it with a null shadow
- src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
    options binding + CreateLanguagesShadow (returns null unless enabled)

Created (tests):
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GraphQlLanguagesShadowRunDisableTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GitHubGraphQlLanguagesShadowTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/ProfileLanguagesShadowSeamTests.cs
- tests/GitHubProof.Web.Tests/ProfileGraphQlLanguagesShadowWiringTests.cs

Modified (tests):
- tests/GitHubProof.Web.Tests/ProfileGraphQlTransportWiringTests.cs
    the S1 constructor scan becomes an allow-list: S2 adds the first legitimate
    consumer, exactly as the S1 test comment anticipated
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GitHubGraphQlTransportTests.cs
    the three G-1 transport-timeout cases, and the L-3 widening of
    Send_DoesNotTreatHttp200WithErrorsAsOrdinarySuccess into a Theory

Canonical docs/: UNCHANGED. artifacts/: UNCHANGED.
No package, no Repository Proof change, no concurrency, cache, retry, batching,
repository cap, credential policy or runtime numeric.
~~~

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror       PASS — 0 warnings, 0 errors
GitHubGraphQlLanguagesShadowTests                32/32
ProfileLanguagesShadowSeamTests                  14/14
ProfileGraphQlLanguagesShadowWiringTests          8/8
focused Infrastructure (shadow+seam+composition+transport)   87/87
GraphQlLanguagesShadowRunDisableTests            18/18
GitHubGraphQlTransportTests                      35/35
focused languages shadow + seam + run-disable    64/64
dotnet test GitHubProof.slnx                     2033/2033, 0 skipped
    Domain 835 · Web 529 · Infrastructure 407 · Application 262
git diff --check                                 PASS
git status --short                               src/tests/worklog only, plus
                                                 the pre-existing artifacts/
~~~

Baseline was 1953 (Session 124); the +80 are this slice's own tests — 54 before
the first review closure, 19 added by it, 5 added by the G-1 closure and 2 by the
L-3 closure (each an added `[InlineData]` on an existing Theory).

### Required test coverage A-R

~~~text
A  default configuration is OFF          LanguagesShadow_IsDisabledByDefault +
                                         ...StaysDisabledWhenConfigurationSaysNothingAboutIt
B  default-off issues zero /graphql      DefaultOff_IssuesZeroGraphQlRequestsOnTheProductionPath +
                                         TheDefaultProfileComposition_IssuesNoGraphQlRequest
C  ON returns identical REST languages   ShadowOn_ReturnsTheIdenticalLanguagesMapAndAvailability +
   and availability                      ShadowOn_LeavesTheRestRetrievedAtAuthoritative +
                                         DefaultOff_LeavesTheCollectedLanguagesUntouched
D  different edge order => Match         Compare_MatchesWhenTheSameMapArrivesInADifferentEdgeOrder +
                                         Compare_UsesTheSameCaseInsensitiveKeyingAsTheRestPath
E  byte difference => Mismatch,          Compare_ReportsMismatchOnAByteDifference +
   REST unchanged                        EveryShadowOutcome_LeavesTheWholeSnapshotIdentical
F  missing/extra language => Mismatch    Compare_ReportsMismatchWhenGraphQlIsMissingALanguage +
                                         Compare_ReportsMismatchWhenGraphQlCarriesAnExtraLanguage
G  200 + data + errors: no partial map   Compare_NeverConsumesAPartialPayloadFromAnErroredEnvelope
H  EnvelopeFailure => REST unchanged     Compare_AbstainsOnAnEnvelopeFailure +
                                         ARealShadowWhoseEnvelopeFails_LeavesTheWholeSnapshotIdentical
I  Malformed => REST unchanged           Compare_AbstainsOnAMalformedOrUnreadableEnvelope (7 bodies)
J  pagination >100 fully followed        Compare_FollowsEveryPageAndComparesTheCompleteMap +
                                         two cursor-guard tests
K  no totalCount requested or consumed   Compare_RequestsOnlyWhatLanguageParityNeeds +
                                         Compare_CarriesRepositoryIdentityInVariablesOnly
L  one POST per page = one graphql count EachGraphQlPage_CountsOnceInTheExistingGraphQlCategory
M  caller cancellation propagates        Compare_PropagatesCallerCancellation
N  no DataAvailability/domain status     ShadowOn_AddsNoAvailabilityEntryOfItsOwn +
                                         ShadowOutcome_IsNotADomainStatus
O  Repository Proof unchanged            RepositoryProofComposition_HasNoShadowSeam
P  profile result identical OFF vs ON    ShadowOffAndOn_ProduceTheIdenticalProfileReport
Q  mismatch/failure cannot drift         AMismatchingShadow_ProducesTheIdenticalProfileReport +
                                         EveryShadowOutcome_ProducesTheIdenticalAnalysis +
                                         AThrowingShadow_LeavesTheWholeSnapshotIdentical
R  existing tests green                  2033/2033
~~~

P and Q run the **real** pipeline end to end — profile metadata, manifest,
admission, per-repository collection, composition and report — with the shadow
off, on and deliberately disagreeing, and compare the resulting `ProfileReport`.
Both assert the GraphQL request counts (0 off, 1 on) so the comparison cannot
pass vacuously.

---

## Independent Review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Scope:    5 new production types, 2 modified production files, 3 new test files,
          1 modified test file, CURRENT, INDEX, this record
Outcome:  1 HIGH, 2 MEDIUM, 3 LOW — none blocking for commit
~~~

The reviewer re-derived the load-bearing behaviour from source and re-ran the
validation itself (0 warnings, 2007/2007 at that point). It confirmed: the decorator really
returns the same collection instance, so no shadow path can touch the languages
map, the availability, `RetrievedAt` or the warnings; the cancellation filter is
correct and a token-independent `OperationCanceledException` is contained rather
than misrouted; default-off constructs no shadow and never resolves the
transport, with the S1 poisoned-registration test still passing as the
authoritative proof; the normalization reproduces the REST rules exactly and
abstains wherever REST throws; §18.4, §18.7, §18.9, §18.12, §18.3 and §7 hold
against the code; the seam stays inside §3.2 and touches no §14.2 component or
Domain policy; and it found no tautological or vacuous test among the 54.

### Findings and disposition

| # | Sev | Finding | Disposition |
|---|---|---|---|
| **F-1** | **HIGH** | **§18.6 was neither implemented nor evaluated.** §18.6 locks: *"The failing envelope must not be re-attempted for every repository… so a systemic failure cannot be multiplied by the admitted repository count."* There is no run-scoped GraphQL disable. With the shadow on, a systemic failure (401/403/secondary limit) sends one failed POST per admitted repository — exactly the outcome §18.6 exists to prevent. §18.6 is absent from this session's Context Read: the rule was never weighed. Zero production impact today because the shadow is default-off, but blocking for any enabled run, and §19.14's live parity gate requires exactly such a run. | **REPORTED, NOT FIXED.** Implementing a run-scoped disable is new behaviour and a scope decision for the owner: it needs a decision on where the per-execution state lives and whether the shadow shares the §18.6 disable with future S3 work or owns its own. Confirmed in source before accepting. |
| **F-2** | MEDIUM | **The §18.9 reading is over-cautious, and the gap left S2 unobservable.** §9 says *"This is the minimum surface; a richer implementation is allowed as long as §9.1 holds"*, and §18.9 extends §9 rather than replacing it; parity outcome is not on its "must never be recorded" list, and it already foresees *"per-surface GraphQL served/fallback facts… as separate bounded structural counters"*. A three-valued closed counter would violate no §9.1 clause. As built, an enabled run produces **no signal at all**. | **REPORTED, NOT FIXED.** The session's own reasoning is recorded above and the reviewer's counter-reading is recorded here; which is right is a canonical telemetry decision the owner owns. Note the two findings compound: F-1 makes enabling unsafe, F-2 makes it valueless. |
| **F-3** | MEDIUM | **Two contract-critical decorator branches are untested.** The caller-cancellation rethrow is proven on the shadow (`Compare_PropagatesCallerCancellation`) but not on the wrapper, and the "skip when REST languages is not Available" guard has no test at all — inverting it would break nothing and would produce a fabricated Mismatch against `EmptyLanguages`. | **REPORTED, NOT FIXED.** Both are real coverage gaps on new code paths; adding them is small and uncontroversial, but it is still acting on a review finding rather than reporting it. |
| **F-4** | LOW | A Session 124 comment in the DI wiring still claimed *"NOTHING in the production collection path resolves it"*, twelve lines below the call that now can. | **CORRECTED.** Purely mechanical and, with the flag on, actively false. The comment now names the single resolver and states that the default configuration resolves nothing. |
| **F-5** | LOW | `CURRENT.md` carried a truncated sentence: the Session 124 line ended on the word "Branch". | **CORRECTED.** Editing artefact from the Session 124 → 125 reshuffle. |
| **F-6** | LOW (informational) | The pagination loop guard compares only the immediately preceding cursor, so an A→B→A cycle would not trip it. A real provider cannot hang the loop, because any repeated page content hits the duplicate-language guard and yields `NotCompared`; only an adversarial server emitting unbounded distinct language names could. | **NOTED, no action.** Recorded so it is not rediscovered as new. |

The reviewer also confirmed `PageSize = 100` does not conflict with §18.11's
"selects no page size": it is the connection's mandatory `first:` argument at its
documented maximum, with no path to becoming a repository cap.

**F-1, F-2 and F-3 are deliberately unapplied.** The session brief requires
substantive findings to be reported for owner decision rather than absorbed, and
each of these changes behaviour, a canonical telemetry position, or the test
surface. F-4 and F-5 were corrected because they were mechanical contradictions
in text this session authored.

---

## Re-review after the F-1 / F-2 / F-3 closure

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Outcome:  3 MEDIUM, 2 LOW — none blocking for commit
~~~

The reviewer re-ran the build and the suite itself (0 warnings, 2026/2026 at that point) and
confirmed each closure from source: the §18.6 disable really stops later POSTs
and cannot leak (no instance field, no static, only the restored `AsyncLocal`);
no message text is parsed anywhere; the four telemetry values are bounded, are
never added to `GitHubRequests` and never reach a Domain, Application or report
type; the contained exception records exactly one `NotCompared` per call and does
not over-count; both F-3 branches are non-vacuous; and no existing test was
weakened.

Five findings. Two are substantive and are **reported, not fixed**:

| # | Sev | Finding | Disposition |
|---|---|---|---|
| **G-1** | MEDIUM | **The §18.6 disable does not cover a client-side transport timeout.** The transport maps only `HttpRequestException` to `EnvelopeFailure`. An `HttpClient.Timeout` produces a `TaskCanceledException` with the caller token **not** cancelled, which matches neither catch clause, escapes `CompareAsync` without touching the state, and is contained by the decorator as `NotCompared` — **without disabling the run**. §18.6 names transport failure and server-side timeout as envelope-level, so a systemic timeout is still paid once per admitted repository: the most expensive form of the multiplication §18.6 exists to prevent. | **REPORTED, NOT FIXED.** Closing it changes how S1 classifies a timeout, which is a transport and contract decision beyond this slice. Confirmed in source. |
| **G-2** | MEDIUM | **The "no bounded structural discriminator" claim is too absolute.** `DataWithErrors` can carry `Data == null` — the shape of a `{"data":null,"errors":[…]}` envelope, which is what a top-level failure such as a primary GraphQL rate limit looks like. `Outcome == DataWithErrors && Data is null` is therefore an existing, bounded, purely structural distinction between a top-level failure and partial data. The *decision* to keep `DataWithErrors` out of §18.6 remains defensible; the *reasoning* recorded in the code comment and in this record overstated the absence of any signal. | **REPORTED, NOT FIXED.** Acting on it changes which envelopes disable the run — exactly the §18.5 / §18.6 classification decision the owner reserved. |
| G-3 | MEDIUM | The record and INDEX still carried the pre-closure "§18.9 gap, not closed" claims alongside the F-2 closure — two opposite statements in one catalogue cell. | **CORRECTED.** The section is marked superseded and kept as the state at first delivery; the Not Done line and the INDEX cell now match the closure. |
| G-4 | LOW | The decorator comment still said §18.9 "does not authorize a shadow match/mismatch counter", eight lines above the code that now writes exactly that counter. | **CORRECTED.** |
| G-5 | LOW | The `ToLogMessage` documentation comment ended up above the newly inserted `FormatLanguagesShadow` helper. | **CORRECTED.** |

G-1 and G-2 compound, and belong to one owner decision: G-1 leaves a real
envelope-level failure mode outside the disable, and G-2 shows the classification
question has more structural signal available than this record first claimed.

---

## Re-review after the G-1 / G-2 closure

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-26
Outcome:  1 MEDIUM, 3 LOW — all worklog or test hygiene; none in production code
~~~

The reviewer re-ran the build and the suite itself (0 warnings, 2031/2031 at that point) and
verified both closures from source.

On **G-1** it traced the token flow rather than trusting the catch order: the
collection call receives `deadline.ExecutionToken`, and `ProfileExecutionDeadline`
builds that from `CreateLinkedTokenSource(callerToken, deadlineSource.Token)`, so
**both** a caller cancellation and a deadline expiry cancel the supplied token
and are caught by the filtered clause. Only `HttpClient`'s own `Timeout` can
reach the new unfiltered clause. All five required tests exist and are
non-vacuous.

On **G-2** it confirmed `DataWithErrors` still does not disable, and that no
message text, error code, path or extension is parsed or retained anywhere — the
only `GetString()` calls under `Profile/` read a language name and an
`endCursor`. It judged the corrected reasoning accurate: neither over- nor
under-claiming.

It also confirmed the **enablement blocker** is recorded in all three places and
that the record does not claim §18.6 is fully closed for every possible
`DataWithErrors` response, and that calling it operational rather than evidence
drift is correct in code — the shadow produces no `DataAvailability` and the
decorator returns the REST collection instance itself, so an unclassifiable
provider-wide fault costs extra POSTs and nothing else.

### Findings and disposition

| # | Sev | Finding | Disposition |
|---|---|---|---|
| **M-1** | MEDIUM | The Completion Checklist marked `[x] Re-review after the G-1/G-2 closure` before that review existed — claiming a verification that had not happened (`AGENTS.md` §6). | **CORRECTED.** The line now records this review and its outcome. Third occurrence of this pattern in the session; it is the reason the checklist is now written only after a review returns. |
| L-1 | LOW | The Changes block listed only one modified test file; git also showed `GitHubGraphQlTransportTests.cs`, which carries the three G-1 timeout cases. | **CORRECTED.** |
| L-2 | LOW | The Problems block still asserted "no run-scoped disable exists. Reported rather than fixed" — contradicting the F-1 closure directly above it. | **CORRECTED** and marked superseded, with the still-open item named precisely (the pre-enablement blocker, not F-1). |
| **L-3** | LOW | The exact envelope the owner ruled on — `{"data": null, "errors": [...]}`, i.e. `DataWithErrors` with `Data == null` — is not pinned by any shadow-level test. The existing cases cover `{"data": null}` **without** errors (Malformed, which does disable) and errors **with** populated data. Behaviour is correct today because the code branches on `Outcome`, but the specific shape the owner said must not disable is unprotected against regression. | **ACCEPTED AND CLOSED.** The owner accepted it; two existing surfaces were widened from `[Fact]` to `[Theory]` rather than adding a fixture or an abstraction. See the L-3 closure below. No production behaviour changed. |

An editorial duplication in `CURRENT.md` ("Build 0 warnings, 2031/2031" twice in
one bullet) was also corrected.

---

## L-3 closure — the owner-decided envelope is now pinned

The owner accepted L-3 and required the smallest possible regression test. Two
existing surfaces were widened from `[Fact]` to `[Theory]`; no new fixture, no
new abstraction, and **no production change**.

**Transport** — `Send_DoesNotTreatHttp200WithErrorsAsOrdinarySuccess` now runs
two envelopes and asserts the outcome is identical for both:

~~~text
{"data": {...}, "errors": [...]}   -> DataWithErrors, Data present
{"data": null,  "errors": [...]}   -> DataWithErrors, Data null
~~~

Both assert `IsSuccess == false`, `ErrorCount == 1`, and — through the existing
`DescribeResponseSurface` walk — that no error `message`, `path`, `type` or
extension survives anywhere on the response.

**Shadow** — `APartialDataEnvelope_AbstainsWithoutDisablingTheRun` became
`ADataWithErrorsEnvelope_AbstainsWithoutDisablingTheRun`, running the same two
envelopes through the real composition inside one execution scope. Per case it
proves:

~~~text
first repository        NotCompared == 1, IsDisabledForRun == false
second repository       still attempted -> GraphQlRequests == 2
totals                  NotCompared == 2, Match == 0, Mismatch == 0
                        IsDisabledForRun == false
REST                    both snapshots carry the REST language map unchanged
~~~

That is the owner's ruling in executable form: a null `data` alongside `errors`
is a bounded structural fact, it is **not** proof of a provider-wide failure, it
never disables the run, its payload is never consumed, and REST remains
authoritative.

---

## Problems

~~~text
- No canonical gap was hit and no STOP was required: the seam fit inside §3.2
  without touching any §14.2 component or any Domain policy.
- The first seam attempt used a third optional parameter and broke an existing
  method-group conversion. Corrected to a separate overload, which is also the
  stronger choice: the pre-S2 signature is now provably unchanged.
- Two snapshot-parity assertions initially compared `RetrievedAt` across two
  separate collections and failed on wall-clock drift. That is exactly what
  §18.7 / §19.2 exclude from parity; the fingerprint now compares key, status
  and reason, and a dedicated test proves the languages `RetrievedAt` is the
  REST collector's own live stamp.
- The S1 constructor-scan test failed once S2 added the first legitimate
  consumer. It was converted from an emptiness check to an allow-list rather
  than deleted, so a fourth type still has to be a deliberate decision.
- Independent review found that **§18.6 was never read or weighed** when this
  slice was first written (F-1): it is absent from the Context Read list above,
  and no run-scoped disable existed. **Superseded** — the owner accepted F-1 and
  it is closed; the disable is implemented, and G-1 later extended it to a client
  transport timeout. What remains open is not F-1 but the named pre-enablement
  blocker: a provider-wide failure carried as HTTP 200 + `errors` still cannot be
  classified.
~~~

---

## Not Done

~~~text
- No S3 envelope, no GraphQL metadata/topics/commits/PR/releases/tags/issues.
- No recursive tree or content change.
- No shadow telemetry beyond the four bounded values the F-2 closure added:
  no per-repository breakdown, no identity, no content.
- No batching, concurrency, cache, retry/backoff, repository cap.
- No package, no credential policy, no deadline/reserve value.
- No performance or latency claim. No live GitHub call.
- Profile Production Runtime Decision Design still pending.
~~~

---

## Known Risks

~~~text
- **F-1 and G-1 are closed**, so a systemic envelope-level failure — including
  a client transport timeout — now costs exactly one POST per execution rather
  than one per admitted repository. The residual risk is narrower and named: a
  provider-wide fault that presents as HTTP 200 + `DataWithErrors` is still paid
  per repository, because no bounded structural fact available today proves it
  is provider-wide rather than an ordinary query error. That is the recorded
  pre-enablement blocker, and it is why the shadow is not approved for
  enablement.
- The shadow is default-off, so nothing exercises it in production until someone
  turns it on. An enabled run now does produce a signal (F-2), so the first real
  parity evidence is obtainable.
- The decorator contains non-cancellation shadow exceptions on purpose. That is
  what makes the core rule hold for a shadow bug, but it also means a shadow bug
  is silent — which is acceptable only while the outcome is discarded anyway.
- `NotCompared` covers both "did not run" and "could not be trusted". S3 may
  need those separated; S2 does not, and splitting them now would be
  speculative.
~~~

---

## Next

Independent review, then architecture-owner acceptance of S2. After that the
open questions are the §18.9 telemetry amendment (if match/mismatch is ever
wanted in production) and S3, neither of which is started.

The **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) remains
pending. The Session 122 §6.1 content-selector duplicate-fetch defect still
awaits its own bounded session.

---

## Git

~~~text
Suggested commit:
feat(profile): add default-off graphql languages shadow parity

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Baseline and branch verified against Git before any mutation
[x] Smallest Profile-only seam; production language collector not replaced
[x] REST authoritative: same collection instance returned, unchanged
[x] Default-off constructs no shadow and issues no GraphQL request
[x] Complete-map parity with cursor pagination; no cap, no sampling
[x] No totalCount, no isEmpty, no unrelated field requested
[x] Ambiguity abstains instead of applying last-wins
[x] No DataAvailability, no domain status, no new telemetry vocabulary
[x] Repository Proof untouched; canonical docs unchanged
[x] Full build 0 warnings; full suite 2033/2033
[x] Independent review of this slice — PASS WITH FINDINGS (1 HIGH, 2 MEDIUM,
    3 LOW; none blocking for commit)
[x] F-4 and F-5 corrected — mechanical contradictions in authored text
[x] F-1 closed — §18.6 execution-scoped disable on EnvelopeFailure/Malformed,
    with the DataWithErrors classification gap reported rather than invented
[x] F-2 closed — four bounded shadow values, not request counts, not evidence
[x] F-3 closed — both decorator branches covered
[x] F-6 — no action, by owner ruling
[x] Re-review after closure
[x] G-1 closed — a client transport timeout is now an EnvelopeFailure and
    drives the run-scoped disable; supplied-token cancellation still propagates
[x] G-2 closed as a reasoning correction — Data == null is a bounded structural
    fact but is NOT proof of a provider-wide failure, so behaviour is unchanged
[x] Pre-enablement blocker recorded: provider-wide HTTP 200 + errors
    classification is unproven, so LanguagesShadowEnabled is not approved for
    enablement anywhere
[x] Re-review after the G-1/G-2 closure — PASS WITH FINDINGS (1 MEDIUM,
    3 LOW), all worklog/test hygiene; M-1, L-1 and L-2 corrected
[x] L-3 accepted and closed — the owner-decided `{"data": null, "errors": [...]}`
    envelope is pinned at both transport and shadow level, no production change
[x] Fourth review round — PASS WITH FINDINGS, no unresolved substantive code,
    contract, invariant, scope or validation finding; its four worklog-fidelity
    findings (missing production file in Changes, stale test description, two
    editorial duplications) were corrected
[ ] Architecture-owner acceptance of S2
[ ] Separate bounded session: proven GraphQL error-kind classification for the
    §18.6 provider-wide DataWithErrors cases
~~~
