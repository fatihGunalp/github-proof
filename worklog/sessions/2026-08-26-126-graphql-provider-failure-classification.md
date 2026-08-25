# GraphQL Provider Failure Classification

**Session:** 126
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** PASS — bounded transport/control-flow slice; verdict
**CLASS-PARTIAL**, accepted by the architecture owner. The closure review
returned PASS WITH FINDINGS (0 blocker, 0 high, 2 MEDIUM, 4 LOW), all corrected;
the owner then narrowed the residual blocker to exactly one shape (below)

---

## Goal

Close every HTTP 200 + `DataWithErrors` provider-wide GraphQL failure case that
can be **proven** from GitHub-supported structural information, without parsing
raw error text and without weakening §18.5 / §18.6.

This is not S3. It is the bounded session Session 125 named as its
pre-enablement blocker.

---

## Starting State

~~~text
Branch       session/126-graphql-provider-failure-classification
Base         5a4bae5  (== main == origin/main, verified with git rev-parse)
             5a4bae5 = "feat(profile): add GraphQL languages shadow parity"
                       (Session 125, S2)
Working set  clean
Pre-existing ?? artifacts/  (user-owned, untouched, out of scope)
~~~

---

## Verdict

~~~text
CLASS-PARTIAL
~~~

The documented structural cases are now covered safely. **One** §18.6 shape
remains impossible to distinguish under the no-message rule, and it is named
exactly below rather than papered over.

---

## The canonical question, answered first

§18.6 already names *"a GraphQL primary or secondary rate-limit response"* as
envelope-level. So acting on such a response is **applying** the locked rule,
not extending it, and **no canonical docs change was needed or made**.

What §18.6 does not say — and what this session had to establish — is *how* such
a response can be recognised when it arrives as HTTP 200, given that reading the
error message is prohibited.

---

## Structural classification table

For an HTTP 200 envelope that parsed as `DataWithErrors`:

| Condition (headers only) | Signal | Run-disable | Basis |
|---|---|---|---|
| `x-ratelimit-remaining: 0` | `RateLimitExhausted` | **yes** | GitHub answers primary GraphQL rate-limit exhaustion with 200 + an error and documents that no further request should be made until reset. Whether *this* error is the rate-limit error cannot be proven without its text — but zero remaining points is the provider's own statement, and it is sufficient on its own to stop attempting GraphQL for the rest of the execution. |
| valid `Retry-After` | `RetryAfterRequested` | **yes** | GitHub sends it with some secondary rate-limit responses, including ones carried on 200. The provider explicitly asked us to wait. |
| neither | `None` | **no** | Stays `DataWithErrors`: comparison `NotCompared`, REST authoritative, run untouched. |

Everything outside that table is unchanged:

~~~text
non-success HTTP (403 / 502 / 504 / any)  -> EnvelopeFailure, disables (§18.6)
unreadable body                            -> Malformed, disables
client transport timeout                   -> EnvelopeFailure, disables (S125 G-1)
successful envelope                        -> Success, consumed, run NOT disabled
caller / deadline cancellation             -> propagates, never a provider failure
~~~

**Ordering is deterministic:** exhaustion outranks `Retry-After` when both are
present.

**A malformed, absent or repeated header is not a signal**, and a test pins
each case. `remaining` must parse as a non-negative integer and equal exactly
zero. `Retry-After` must read as delta-seconds or as a timestamp; the timestamp
branch is `DateTimeOffset.TryParse`, which accepts more than the strict HTTP-date
grammar — deliberately, because the looseness is one-way: accepting a value
GitHub would not have sent can only stop the shadow early, never produce
evidence. Guessing from a broken header would be the inference this session
exists to avoid.

**`x-ratelimit-resource: graphql` is never a signal** — it is normal on every
GraphQL response, and a test pins that.

---

## The residual blocker — CLASS-PARTIAL, stated exactly

This response shape remains **unclassifiable** and is deliberately left as a
per-repository abstention:

~~~text
HTTP 200
+ DataWithErrors
+ x-ratelimit-remaining > 0
+ no Retry-After
+ the condition indicated only by the error message
~~~

§18.6 lists six envelope-level conditions. Checking each against how it actually
arrives:

~~~text
transport failure            covered — exception -> EnvelopeFailure
non-200 status               covered — 401/403/502/504 -> EnvelopeFailure
unparsable body              covered — Malformed
server-side query timeout    covered — GitHub answers 502/504 -> non-200 path
auth / authorization failure covered — bad or invalid credentials and
                             insufficient App permissions answer 401, and
                             documented inaccessible-token cases 403; all reach
                             the non-200 path
primary rate limit           covered — 200 + x-ratelimit-remaining: 0
secondary rate limit         PARTLY covered — Retry-After when present;
                             message-only otherwise -> OPEN
~~~

So exactly **one** §18.6 shape can still arrive as HTTP 200 with the condition
stated only in the error message: the secondary rate limit GitHub identifies that
way. It has no documented stable structural discriminator, reading the message is
prohibited, and so it stays open. **No workaround was invented.**

**Authentication and authorization are not part of the residual.** Verified
GitHub behaviour answers bad or invalid GraphQL credentials and insufficient App
GraphQL permissions with 401, and documented inaccessible-token cases with 403 —
all already envelope-level through the existing non-200 path. §18.6 is not
weakened: an authentication or authorization failure remains an envelope-level
failure whenever it is observed.

The residual shape falls through to `None` and therefore to a per-repository
abstention, so the cost is extra requests, never wrong evidence.

Consequence, unchanged from Session 125: `LanguagesShadowEnabled` stays
**DEFAULT OFF** and is **not approved** for production, beta, §19.14 live A/B or
default-on use.

---

## Why a successful response is still successful

Test 7 asked for a proven choice. A `Success` envelope carrying
`x-ratelimit-remaining: 0` stays `Success`, its comparison is consumed, and the
run is **not** disabled.

§18.6 disables on failure. Pre-emptively disabling on a complete, error-free
observation would be a new policy this session does not own, and the cost of not
doing it is bounded: the next request fails and disables normally. The signal is
therefore computed only for `DataWithErrors`, so a `Success` response structurally
always carries `None` — a header can never turn a success into a failure.

---

## Changes

~~~text
Created (production):
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlProviderSignal.cs
    closed three-member transport-only vocabulary

Modified (production):
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlResponse.cs
    ProviderSignal + ReportsProviderWideCondition; set only on DataWithErrors
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlTransport.cs
    ReadProviderSignal / IsRateLimitExhausted / HasRetryAfter, read from
    response headers before the body is read
- src/GitHubProof.Infrastructure/GitHub/Profile/GitHubGraphQlLanguagesShadow.cs
    the §18.6 disable branch also fires on ReportsProviderWideCondition

Created (tests):
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GitHubGraphQlProviderSignalTests.cs

Modified (tests):
- tests/GitHubProof.Infrastructure.Tests/GitHub/Profile/GraphQlLanguagesShadowRunDisableTests.cs
    provider-wide header cases, execution isolation, success-with-zero-remaining
- tests/GitHubProof.Web.Tests/ProfileGraphQlTransportWiringTests.cs
    REST budget isolation and physical-request counting for an exhausted
    GraphQL budget

Worklog:
- worklog/sessions/2026-08-26-126-graphql-provider-failure-classification.md
- worklog/CURRENT.md
- worklog/INDEX.md

Canonical docs/: UNCHANGED. artifacts/: UNCHANGED.
No package, no Repository Proof change, no retry/backoff, no numeric policy,
no Domain/Application/DataAvailability status, no telemetry handler change.
~~~

---

## Proof: no message parsing

The transport contains **zero** `GetString()` calls, and never reads `message`,
`type`, `extensions` or `path`. The only two `GetString()` calls anywhere under
`GitHub/Profile/` are in the shadow, reading a language name and an `endCursor`
— both payload fields, neither error content.

Classification reads exactly two headers, and the errors array is still reduced
to a count before anything else sees it. `Send_RetainsNoErrorContentWhileClassifying`
puts distinct sentinels in the error `message`, `type` and `path`, asserts the
payload **is** present so the walk is not vacuous, and asserts no sentinel
survives anywhere on the response.

---

## Proof: run-disable

`AProviderWideHeaderOnAnErroredEnvelope_DisablesTheRun` drives three repositories
through one execution scope for each qualifying header:

~~~text
GraphQL POSTs issued        1     (the first repository only)
IsDisabledForRun            true
NotCompared                 3
Match / Mismatch            0 / 0
REST languages per repo     unchanged, all three
~~~

`AFollowingExecution_MayAttemptGraphQlAgainAfterAProviderWideHeader` proves the
disable is scoped to one execution: the second scope starts clean, attempts
again and produces a `Match`. `AZeroRemainingHeaderOnASuccessfulEnvelope_DoesNotDisableTheRun`
proves a success is not disabled by a header, and
`ADataWithErrorsEnvelope_AbstainsWithoutDisablingTheRun` still proves an errored
envelope **without** a qualifying header does not disable.

---

## Proof: REST budget isolation

`AnExhaustedGraphQlBudget_NeverReachesTheRestBudgetDecision` sends a GraphQL
response with `x-ratelimit-remaining: 0` and `x-ratelimit-resource: graphql`
through the real `ProfileGitHubRequestTelemetryHandler`:

~~~text
budgetState.MetadataObservation   null          <- §10.4 / §11.3 untouched
telemetry.GitHubRequests          1
RequestCategories                 exactly one key, "graphql"
log message                       carries no error content
~~~

The telemetry handler is unchanged — it still inspects only the route shape and
the five named rate-limit headers, never a body, a query or an error. GraphQL
`remaining` / `reset` / `Retry-After` reach no `ProfileGitHubBudgetState`, no
§10.4 decision and no Domain or Application result.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror       PASS — 0 warnings, 0 errors
GitHubGraphQlProviderSignalTests                 30/30
focused GraphQL transport (both suites)          62/62
focused shadow + seam + run-disable              69/69
focused Web telemetry / budget / wiring          57/57
dotnet test GitHubProof.slnx                     2069/2069, 0 skipped
    Domain 835 · Web 530 · Infrastructure 442 · Application 262
git diff --check                                 PASS
git status --short                               src/tests/worklog only, plus
                                                 the pre-existing artifacts/
~~~

Baseline was 2033 (Session 125); the +36 are this session's own tests — 33 before the closure review and 3 added by the L-1 correction.

### Required coverage 1-16

~~~text
 1 remaining=0 => provider structural result   Send_ClassifiesZeroRemainingAsAProviderWideCondition
 2 same through shadow, later repos zero POST  AProviderWideHeaderOnAnErroredEnvelope_DisablesTheRun
 3 new execution may attempt again             AFollowingExecution_MayAttemptGraphQlAgainAfterAProviderWideHeader
 4 valid Retry-After => same behaviour         Send_ClassifiesAValidRetryAfterAsAProviderWideCondition (3 forms)
                                               + the Retry-After rows of the shadow Theory
 5 remaining>0, no Retry-After => not disabled Send_LeavesAnErroredEnvelopeUnclassifiedWithoutAQualifyingHeader
                                               + ADataWithErrorsEnvelope_AbstainsWithoutDisablingTheRun
 6 data:null + errors, no header => not wide   Send_LeavesANullDataEnvelopeUnclassifiedWithoutAQualifyingHeader
 7 success + remaining=0 stays success         Send_DoesNotTurnASuccessfulEnvelopeIntoAFailureOnZeroRemaining
                                               + AZeroRemainingHeaderOnASuccessfulEnvelope_DoesNotDisableTheRun
 8 403 remains EnvelopeFailure                 Send_KeepsNonSuccessStatusesAsEnvelopeFailure
 9 502 / 504 remain EnvelopeFailure            same Theory
10 malformed unchanged                         Send_KeepsMalformedBehaviourUnchanged (4 bodies)
11 cancellation propagates                     Send_StillPropagatesCallerCancellationRatherThanClassifyingIt
                                               + CallerCancellation_PropagatesAndDoesNotDisableTheRun
12 no GraphQL data in REST budget state        AnExhaustedGraphQlBudget_NeverReachesTheRestBudgetDecision
13 one physical POST == one graphql request    same test + EachGraphQlPage_CountsOnceInTheExistingGraphQlCategory
14 no raw error/query/payload/identity leak    Send_RetainsNoErrorContentWhileClassifying
                                               + TheProviderSignalIsAClosedStructuralVocabulary
15 Repository Proof unchanged                  RepositoryProofComposition_HasNoShadowSeam (unchanged, green)
16 full suite green                            2069/2069
~~~

---

## Decisions

~~~text
- CLASS-PARTIAL, not CLASS-PASS: the message-only secondary rate-limit shape is
  named and left open rather than guessed.
- No canonical docs change: §18.6 already classifies a rate-limit response as
  envelope-level, so this is application of the rule, not amendment.
- The signal is computed for DataWithErrors only, so Success structurally always
  carries None and a header can never demote a successful observation.
- Retry-After's VALUE is not retained. No retry or backoff exists to consume it,
  and a bounded "the provider asked us to wait" is the whole signal.
- Exhaustion outranks Retry-After, so classification is deterministic.
- A malformed or repeated header yields None rather than a guess.
- The telemetry handler was not touched: classification happens in the transport,
  which already has the response in hand.
~~~

---

## Problems

~~~text
- None blocking, and no STOP was required: the classification fit inside §18.6
  without touching canonical text, and none of the three escalation triggers
  declared at the gate fired.
- One test initially asserted that the string "repository" was absent from the
  response surface. That was wrong: "repository" is a legitimate key of the
  retained payload as well as an element of the error path, so the sentinel was
  ambiguous. Replaced with distinct sentinels in the error members only.
~~~

---

## Not Done

~~~text
- The one open shape is not classified: a secondary rate limit arriving as
  HTTP 200 whose identity is available only through the error message
  (CLASS-PARTIAL).
- No retry, backoff, circuit numeric or wait implementation.
- No S3, no other GraphQL surface, no batching, concurrency, cache or cap.
- No canonical docs change, no package, no credential policy, no runtime value.
- No performance or latency claim. No live GitHub call.
- LanguagesShadowEnabled remains default OFF and unapproved for enablement.
~~~

---

## Known Risks

~~~text
- The residual shape above is the reason enablement stays blocked. The blocker
  is much narrower than Session 125's — one message-only shape rather than the
  whole HTTP 200 + errors class — but it is not absent.
- `x-ratelimit-remaining: 0` is treated as sufficient even though the specific
  error may be unrelated to the rate limit. That is deliberate and conservative:
  it can only cause the shadow to stop early, never to produce evidence.
- GitHub could change header behaviour; the classification is header-shaped and
  would need re-verification if it did.
~~~

---

## Next

The architecture owner accepted the implementation and the **CLASS-PARTIAL**
verdict. The remaining pre-enablement work is the one open shape: a secondary
rate limit arriving as HTTP 200 with its identity available only through the
error message. Until it has a provider-supported structural discriminator,
`LanguagesShadowEnabled` stays off.

**S3 has not started.** The **Profile Production Runtime Decision Design**
(D-1, D-2, D-3, D-5) remains pending.

---

## Independent closure review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-26
Outcome:  0 BLOCKER, 0 HIGH, 2 MEDIUM, 4 LOW — none blocking
~~~

The reviewer re-ran build and suite itself and confirmed from source: zero
`GetString()` in the transport and no `message`/`type`/`path`/`extensions` read
anywhere; the ordering is deterministic and every parse path is total, so no
header value can throw; a wrong disable can only stop the shadow early and never
lose evidence, because the signal's only consumer is the shadow and the
decorator returns the REST collection instance itself; **Success cannot carry a
non-None signal by construction**, since the `Success`, `Malformed` and
`EnvelopeFailure` factories do not take the parameter at all; the run-disable
reuses the Session 125 scope correctly; the telemetry handler and budget state
are unchanged as files; no new status and no canonical drift; and CLASS-PARTIAL
is the right verdict.

It also noted the strongest negative evidence is a test that already existed:
`ADataWithErrorsEnvelope_AbstainsWithoutDisablingTheRun` carries
`"type": "RATE_LIMITED"` and an "API rate limit exceeded." message with **no**
qualifying header, and asserts the run is not disabled — direct proof that the
text plays no part in classification.

All six findings were corrected under the accelerated-workflow rule:

| # | Sev | Finding | Correction |
|---|---|---|---|
| **M-1** | MEDIUM | The residual blocker was described as one shape ("the message-only secondary rate-limit shape") in every operational summary, while the reviewer read §18.6 as leaving further 200-carried conditions open too. | **CORRECTED, THEN NARROWED BY THE OWNER.** The residual section was first widened to name two open conditions, adding a 200-carried authorization/scope failure. The reviewer's third candidate — server-side timeout — was checked and rejected at the time, because GitHub answers it with 502/504. The architecture owner then rejected the authorization/scope candidate on the same ground: verified GitHub behaviour answers bad or invalid credentials and insufficient App permissions with 401, and inaccessible-token cases with 403, so all of them already reach the non-200 envelope-level path. The residual is therefore back to exactly **one** shape, and §18.6 is not weakened — authentication and authorization remain envelope-level whenever observed. |
| **M-2** | MEDIUM | The record declared `Result: PASS` while its own checklist showed the review unrun (`AGENTS.md` §6). | **CORRECTED.** IN_PROGRESS until the owner accepts. |
| L-1 | LOW | "A repeated header yields None" was asserted in code, record, CURRENT and INDEX but never tested. | **CORRECTED.** `Send_TreatsARepeatedHeaderAsNoSignal` pins it for three header shapes. |
| L-2 | LOW | The `Retry-After` timestamp branch is `DateTimeOffset.TryParse`, which accepts more than the strict HTTP-date grammar, while the comment claimed strictness. | **CORRECTED** by making the comment accurate and stating why the looseness is one-way safe (extra disable costs an observation, never evidence). The parser was deliberately left as-is: tightening it is a behaviour change, not hygiene. |
| L-3 | LOW | `&& seconds >= 0` was dead, because `NumberStyles.None` already rejects a sign. | **CORRECTED** — condition removed, reason recorded in a one-line comment. |
| L-4 | LOW | A dropped word in a test comment. | **CORRECTED.** |

Only one production line changed after the review — the dead condition in L-3 —
and the full suite was re-run afterwards.

---

## Git

~~~text
Suggested commit:
feat(profile): classify structural graphql provider failures

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Baseline and branch verified against Git before any mutation
[x] Canonical question answered first: §18.6 already covers rate-limit responses
[x] Classification reads headers only; zero message/type/path/extension reads
[x] Malformed, absent or repeated headers yield no signal
[x] Success envelopes are never demoted by a header
[x] Run-disable reuses the Session 125 execution scope; no leak between runs
[x] REST and GraphQL budgets remain separate; telemetry handler untouched
[x] No Domain/Application/DataAvailability status; no canonical docs change
[x] Residual shape named exactly; no workaround invented
[x] Full build 0 warnings; full suite 2069/2069
[x] Independent closure review — PASS WITH FINDINGS (0 blocker, 0 high,
    2 MEDIUM, 4 LOW); all six corrected under the accelerated-workflow rule
[x] Architecture-owner acceptance — implementation PASS, CLASS-PARTIAL accepted,
    residual narrowed to the one message-only secondary rate-limit shape
[x] Owner-directed worklog fidelity correction applied; no production or test
    file changed for it
~~~
