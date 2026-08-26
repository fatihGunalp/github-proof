# GraphQL Default-Branch Head / Tree Identity Parity

**Session:** 129
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** HEADTREE-PASS — the §18.13 proof succeeded and the bounded
implementation closed in the same session. The default-branch **head commit SHA**
and **root tree SHA** are now adopted over GraphQL inside the existing Session
127 envelope, **DEFAULT OFF**. The recursive file tree stays REST. GraphQL null
never becomes emptiness.

---

## Baseline

~~~text
Branch : session/129-graphql-head-tree-identity-parity
Base   : 9608804  (== main == origin/main, verified before any mutation)
Working tree at start: ?? artifacts/  (user-owned, untouched)
~~~

**Worklog reconciliation.** Session 128 is **integrated as `9608804`**. `CURRENT`
said "CLOSED / awaiting final integration" and named `8f42a6f` as the last
integrated commit, and `INDEX` row 128 carried `—` in its Commit column. Both
were stale operational memory and are reconciled here mechanically, without
re-opening anything Session 128 decided.

---

## Goal

Prove, and only then implement, the Profile-only GraphQL adoption of two Git
object identities inside the **existing** S127 repository envelope:

- the default branch's head commit SHA,
- that commit's root tree SHA.

The recursive file tree itself had to stay REST, and the session was forbidden
from ever reading `defaultBranchRef == null` as successful emptiness.

No second GraphQL request, no second envelope, no second circuit, no generic
GraphQL object framework.

---

## The §18.13 question, answered

§18.13 is the section that governs this, and it offers **two** exits, not one:

> If that equivalence cannot be proven, those surfaces stay on REST, **or fall
> back to REST under §18.5**.

That second clause is what this session used. It is not a workaround — it is the
conservative branch the canonical text already names. Concretely:

- a **clean, complete, non-null** identity chain may be adopted, because its
  mapping onto the REST values is provable (below);
- **every** other shape — including the null one an empty repository answers
  with — is "candidate unusable", which is §18.5 case 3: discard the whole
  repository envelope and collect that repository over REST.

So the session never had to classify null as empty. It classified null as
*nothing at all*, and let the existing REST 409 rule keep sole ownership of the
empty-versus-failed distinction.

**§19.16 is satisfied in both directions, and literally.** The positive test
proves "the GraphQL empty-repository shape produces exactly the current REST
successful-empty result" — it does, by falling back to the path that produces it.
The negative test proves a failure response is not read as empty. Nothing in
§19.16 requires GraphQL to produce the empty result *directly*, and requiring
that would have meant adopting a semantics GitHub's documentation does not
support.

**What was deliberately NOT claimed.** GitHub documents `defaultBranchRef` only
as the Ref associated with the default branch, and documents `Ref.target` as
returning null when the object does not exist. Neither establishes
`defaultBranchRef == null ⟺ successfully empty`. That equivalence is **not
proven and is not asserted anywhere in this slice**. GitHub separately documents
REST 409 as covering an empty repository **or** an unavailable one (for example
while creation is still in progress) — and the existing GitHub Proof
implementation is deliberately stricter than that, treating only the exact body
`"Git Repository is empty."` as successful emptiness and every other 409 as a
failure. That strictness is unchanged.

---

## Mapping proof (§18.2 mapping obligation, §19.17)

| REST | GraphQL | Why they are the same value |
|---|---|---|
| `GET /git/ref/heads/{defaultBranch}` → `reference.object.sha` | `repository.defaultBranchRef.target` as `Commit` → `oid` | Both name the Git object the **default-branch ref points to**. Same ref, same object, same identifier space. |
| `GET /git/commits/{headSha}` → `tree.sha` | `Commit.tree.oid` | Both name the **single root tree of that commit**. Git guarantees a commit has exactly one root tree. |

These are **Git-defined object identities**, not provider-computed values. For
one ref at one instant there is exactly one correct answer on either transport,
which is what makes this a transport change rather than an evidence change
(§18.1).

Two structural facts make the mapping usable rather than merely plausible:

- `Ref.target` is typed `GitObject`, so a target that is not a `Commit` simply
  does not match the `... on Commit` fragment and arrives as an object **without
  `oid`**. That is refused exactly like a null target — there is no shape in
  which a non-commit silently supplies an identity.
- The two OIDs come from **one** envelope, so they are internally consistent with
  each other. REST resolves them with two separate requests that can straddle a
  push; the hybrid cannot.

**Not claimed:** that a REST run and a hybrid run observe the same instant.
§18.7 already says they are separate observations, and §19.2 excludes wall-clock
equality from parity.

---

## Truth table — every GraphQL shape

| `defaultBranchRef` shape | Envelope | Who decides emptiness | Result |
|---|---|---|---|
| complete: name + Commit target + `oid` + tree + tree `oid`, all valid | **accepted** | not applicable — a commit exists | ref + commit GETs skipped; recursive tree GET from the envelope tree OID |
| `null` | discarded (whole envelope) | **REST 409 body** | exact `"Git Repository is empty."` → successful empty; any other 409 → failure |
| `target: null` | discarded | REST | REST decides, unchanged |
| target present but not a `Commit` (no `oid`) | discarded | REST | REST decides, unchanged |
| `tree` missing or `null` | discarded | REST | REST decides, unchanged |
| either `oid` not 40 lowercase hex (short, uppercase, non-hex, non-string) | discarded | REST | REST decides, unchanged |
| branch `name` absent/blank | discarded | REST | REST decides, unchanged |
| `DataWithErrors` (HTTP 200 + `errors`) | discarded | REST | per-repository abstention; **run stays enabled** |
| `EnvelopeFailure` / `Malformed` / proven provider-wide header signal | discarded | REST | **run disabled** for the rest of the execution (§18.6) |

GraphQL null means exactly one thing here: **candidate unusable**. It never means
"repository empty", and no code path can express that.

**Whole-envelope fallback** was used deliberately, as the session brief
authorized. If any adopted member is unusable, the whole candidate falls back —
metadata, topics and languages included — rather than producing a mixed
half-valid authoritative object. That avoids inventing a partial-envelope policy,
which would have been a new canonical decision.

**No S127 regression.** S127 already required `defaultBranchRef` and already
discarded the candidate on null, so an empty repository already fell back before
this session. This slice tightens the same rule; it does not loosen anything.

---

## Request graph, accepted repository

~~~text
REST-only (relevant subset)          Extended hybrid
-------------------------------      -------------------------------
GET  /repos/{o}/{n}                  POST /graphql
GET  /repos/{o}/{n}/languages
GET  .../git/ref/heads/{branch}
GET  .../git/commits/{headSha}
GET  .../git/trees/{treeSha}?recursive=1   GET .../git/trees/{treeSha}?recursive=1
GET  .../readme                            GET .../readme
GET  .../license                           GET .../license
GET  .../contents/...                      GET .../contents/...
...                                        ...
~~~

Four REST GETs become one POST: **three fewer physical provider requests** for
that repository, of which S127 already accounted for one and S129 adds two.

**Request counts only.** No latency claim, no speed percentage, no production
estimate (§18.11).

The recursive tree request is issued from the **exact GraphQL tree OID**, never
from the branch name, so the snapshot stays anchored to the commit
`HeadCommitSha` names.

This is asserted from **recorded request paths**, and one test points the
envelope at a root tree OID the REST commit response never names — so the test
cannot pass while a REST ref/commit call still happened, and cannot pass if the
OID were re-derived from REST.

---

## Why the recursive tree stays REST

Unchanged from §18.2, and re-proved here:

- GraphQL exposes no recursive tree;
- GraphQL exposes no `truncated` equivalent, and `truncated` is what drives
  `FileTree.IsComplete` and the truncation warning.

`Tree.entries` is **never requested**, pinned by a query-text test. Adopting the
tree OID is what lets the REST request be *anchored*; it is not a step toward
replacing it.

---

## Composition seam

The smallest Profile-only seam, reusing the shape Session 127 already proved:

- `GitHubRepositoryHeadTreeIdentity(HeadCommitSha, RootTreeSha)` — a new
  Infrastructure record, deliberately carrying **no `RetrievedAt`**, because it
  is structural **input to collection**, not an observed surface.
- `HybridRepositoryMetadataCollector` now stores languages **and** identity in
  **one** `ConditionalWeakTable` entry, keyed by the metadata collection instance
  it returned. One entry means the two can never diverge.
- `GitHubRepositoryContentsCollector` gains one optional constructor delegate,
  `Func<GitHubRepositoryMetadataCollection, GitHubRepositoryHeadTreeIdentity?>?`
  — the same delegate shape the supplied-languages seam already uses.

**No shared collector interface changed.** `IGitHubRepositoryContentsCollector`
is untouched, and `CreateDefault` still constructs
`new GitHubRepositoryContentsCollector(httpClient)` — the Repository Proof
composition is byte-for-behavior unchanged, pinned by a test that proves it still
issues both identity requests (§3.1).

**Explicit `contentReference` stays authoritative.** The supplier is not merely
ignored when a commit was named — it is **not consulted at all**. A test makes
the envelope name a *different* head and proves the requested commit wins, which
is the only case where the difference is observable.

---

## RetrievedAt

No new availability timestamp was introduced.

- The S127 envelope keeps its single `RetrievedAt`, shared by the evidence
  surfaces it supplies (metadata, topics, languages).
- The `file-tree` availability keeps the **REST collector's own** instant,
  because the recursive tree request is still the observation that produced it.
  Proven by test: the file-tree instant is *not* the envelope instant, while the
  metadata instant *is*.

---

## Availability, evidence and telemetry

- **No new `DataAvailabilityStatus`**, no new domain or application status, no
  new reason vocabulary, no new evidence key.
- Head/tree identity introduces **no** availability entry of its own. The
  recursive REST request continues to own `file-tree`.
- A GraphQL fallback before REST produces no availability entry, warning or
  limitation.
- If the retained REST recursive-tree request fails, the existing REST file-tree
  failure result stays authoritative and is never converted to `Missing`.
- Telemetry is unchanged: one physical POST is still counted exactly once, in
  the existing closed `graphql` category. No new counter, no new category.
- REST and GraphQL budget observations stay isolated; §10.4 / §11.3 still read
  the REST profile-metadata response.

---

## Failure / run-disable, unchanged

S126/S127 rules were preserved exactly and are still green:

- `EnvelopeFailure`, `Malformed`, or a proven provider-wide **header** signal →
  GraphQL disabled for the remainder of the Profile execution; the current
  repository falls back in full; later repositories issue **zero** GraphQL POSTs.
- `DataWithErrors` without a proven provider-wide structural signal → repository
  fallback, run stays enabled.
- A clean `Success` is never demoted by a rate-limit header alone.
- Caller cancellation propagates and never triggers the disable.
- **No error-message parsing anywhere.**

The Session 126 residual — a secondary rate limit GitHub identifies only through
the error message — remains **unresolved** and remains the enablement blocker.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 warnings, 0 errors
Domain          842/842
Application     262/262
Infrastructure  636/636
Web             551/551
TOTAL          2291/2291     (baseline 2247, +44)
git diff --check                              clean
~~~

New test file: `ProfileHybridHeadTreeIdentityTests` (21 tests). The remaining
new tests extend the existing S127 suites rather than duplicating them.

### Required test surface, 1–29

| # | Requirement | Where |
|---|---|---|
| 1 | Default OFF = exact existing request graph, zero S129 effect | seam + Web wiring, ref/commit/tree pinned |
| 2 | `Commit.oid` == snapshot `HeadCommitSha` | `AnAcceptedEnvelope_ProducesTheSameHeadCommitShaAsRest`, `TheSnapshotHead_IsTheEnvelopeCommitOid` |
| 3 | Recursive tree GET uses the GraphQL tree OID | `TheRecursiveTreeRequest_UsesTheEnvelopeTreeOid` (distinct OID) |
| 4 | REST git-ref GET absent | request-path assertions, seam + Web |
| 5 | REST git-commit GET absent | request-path assertions, seam + Web |
| 6 | REST metadata/languages GET still absent | existing S127 assertions, extended |
| 7 | README / license / file contents stay REST | `AnAcceptedEnvelope_LeavesEveryContentRequestOnRest` |
| 8 | null + exact empty 409 → REST successful-empty | `ANullDefaultBranchRefWithTheExactEmpty409_...` |
| 9 | null + other 409 → REST failure | `ANullDefaultBranchRefWithAnyOther409_StaysAFailure` (+ the two-409 discrimination test) |
| 10 | `target: null` → fallback | truth-table theories, both suites |
| 11 | non-Commit target → fallback | same |
| 12 | tree missing/null/invalid OID → fallback | same, plus short/uppercase/non-hex/non-string OID rows |
| 13 | `DataWithErrors` → fallback, never empty | existing S127 fallback theory, extended |
| 14 | provider-wide structural failure → fallback + run disable | `HybridRepositoryEnvelopeRunDisableTests` (unchanged, green) |
| 15 | malformed → fallback + run disable | same |
| 16 | clean Success + `remaining: 0` header → still served | `ASuccessfulEnvelopeWithZeroRemaining_IsStillServed` |
| 17 | cancellation propagation unchanged | `CallerCancellation_StillPropagates` |
| 18 | explicit `contentReference` never overridden | two dedicated tests, incl. a disagreeing head |
| 19 | `file-tree` availability semantics match REST-only | `TheIdentitySeam_IntroducesNoAvailabilityVocabulary`, instant test |
| 20 | `truncated: true` → same `IsComplete` / warning | `ATruncatedTree_KeepsIdenticalCompletenessAndWarningSemantics` |
| 21 | semantic repository fingerprint REST == hybrid | fingerprint comparisons throughout |
| 22 | end-to-end analysis: status, scores, 15 signals, evidence, warnings, limitations, categories | `AnAcceptedEnvelope_ProducesTheIdenticalAnalysis` |
| 23 | end-to-end Profile parity | `EnvelopeOffAndAccepted_ProduceTheIdenticalProfileReport` |
| 24 | fallback parity | fallback theories, seam + Web |
| 25 | request-count assertion, named not just counted | `AnAcceptedEnvelope_CostsThreeFewerPhysicalRequests` |
| 26 | no `isEmpty` / `totalCount` / `tree.entries` / unrelated field in query | `TheQuery_NeverRequestsAnUnadoptedField` (+ `entries`) |
| 27 | no new Domain/Application status | availability vocabulary assertions |
| 28 | one physical POST == one request | existing telemetry tests, unchanged |
| 29 | REST/GraphQL budget isolation | existing tests, unchanged |

---

## Fixture change worth naming

The Web wiring fixture repository was **empty** (its `git/ref` answered 409). Once
the envelope carries an identity chain, an empty repository plus a GraphQL commit
would have been an internally contradictory fixture. It was made **non-empty**,
with a REST ref/commit/tree chain carrying exactly the two SHAs the GraphQL
envelope carries — which is what turns the off/on comparison into a mapping proof
rather than a coincidence. Every assertion in that file is relative (off vs on)
or request-count based, so nothing was weakened.

The shared `FakeGitHubApiHandler` gained two opt-in knobs: a `git/ref` responder
(so a test can serve the exact empty 409, a different 409, or any failure) and a
`truncated` flag. It also now answers **any** tree OID with the same body, on
purpose, so a test proves which OID was used from the recorded path rather than
inferring it from a 404.

---

## Not done

- **No S4.** Batching and aliases were not opened.
- **No live A/B run**, no production enablement, no beta, no default-on.
- **No `DecisionBounded` adoption.** Commits, pull requests, releases and tags
  stay REST; Session 128's PARITY-BLOCKED verdict is untouched. Commits remain
  blocked on their own ordering evidence, not on this slice — although
  `Commit.history` is now reachable through the adopted default-branch ref.
- **No canonical change.** `public-profile-runtime-collection.md` was not edited;
  §18.13's existing fallback clause already authorized this shape.
- **No `isEmpty`**, no `totalCount`, no `Tree.entries`.
- `artifacts/` untouched.

---

## Known risks

| Risk | State |
|---|---|
| Secondary rate limit carried as HTTP 200 + `DataWithErrors` with the condition only in the error message | **Unresolved, Session 126 residual.** Still the blocker for enabling any GraphQL switch |
| A branch deleted between the GraphQL envelope and the REST tree request | The tree GET fails and the existing REST file-tree failure is authoritative. Two observations legitimately differ (§18.7); §19.2 conditions parity on matching observed provider facts |
| A future GitHub SHA-256 object ID | Refused by the 40-lowercase-hex rule, so the repository silently falls back to REST rather than writing a `HeadCommitSha` the REST path would not produce. Conservative by design |
| `homepageUrl` vs REST `homepage`, `primaryLanguage`, `description` provider-value parity | Unchanged S127 live-A/B risk. Mapping is pinned by test; provider-level equality is still not claimed |

---

## Independent closure review

**PASS WITH FINDINGS** — 0 blocker, 0 high, 0 medium, 1 low.

The reviewer independently re-derived the §18.13 null-never-empty proof from
`TryReadDefaultBranch`'s short-circuit on a missing/null `defaultBranchRef`
(never reaching `reference.GetProperty`), confirmed every broken-chain shape
(`target: null`, non-`Commit` target, missing/null `tree`) discards the whole
envelope the same way, and independently reproduced the build (0 warnings) and
test evidence (842 + 262 + 636 + 551 = **2291/2291**), matching this record
exactly. It also re-verified, from the diff rather than this record's prose:
`Tree.entries` never requested; explicit `contentReference` never consulted
against the supplier at all; no shared interface changed; `CreateDefault`
byte-for-behavior unchanged; no new `DataAvailabilityStatus`; S126 disable and
budget isolation untouched in the diff; the envelope switch untouched and
still `false` by default.

**The one LOW finding:** the `TryReadObjectId` comment claimed its 40-lowercase-hex
rule was "the same shape `GitHubCommitReference` accepts" — but that type
normalizes case, while this method refuses anything not already lowercase
rather than normalizing it. No behavioral effect (real GitHub SHAs are already
lowercase); corrected in-session and the rebuild re-verified 0 warnings.

---

## Verdict

**HEADTREE-PASS.**

The envelope remains **DEFAULT OFF** behind the existing
`ProfileGraphQlHybrid:RepositoryEnvelopeEnabled` switch, and is **not approved**
for production, beta, the §19.14 live A/B validation, or default-on use.

---

## Git

~~~text
git diff --check     clean
git status --short   10 modified, 2 new, plus pre-existing ?? artifacts/
~~~

No commit, no push. Finalization is the owner's.
