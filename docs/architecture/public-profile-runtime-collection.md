# Public Profile V1 — Runtime Collection Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Profile Proof repository-evidence collection composition, runtime
telemetry and execution guard semantics
**Session:** 096
**Date:** 2026-08-20
**Amended:** 2026-08-21 / Session 098 — after the Session 097 controlled live
smoke failed on unbounded required-surface pagination: added §15
(decision-sufficient collection) and §16 (fork policy), moved `Contributors`
from the §4 required surface list into the §5 omission list with its
zero-consumer proof, and added §17 acceptance criteria. §14 remains Session
096's criteria, unchanged
**Amended:** 2026-08-22 / Session 107 — locked the minimum production-guardrail
runtime contract in §10.2-§10.5 and reconciled §11.2-§11.3: the nullable-profile
three-part live result boundary, execution status/reason vocabulary, deadline
ownership and transport/content-read relationship, Profile-only initial
provider protection, authentication boundary and trusted-proxy production
dependency. Architecture-owner re-review corrected the deadline start to occur
before the first live metadata operation and recorded `ResponseHeadersRead`
semantics. No evidence, scoring, eligibility, composition, report or Repository
Proof behavior changed
**Amended:** 2026-08-25 / Session 123 — closed the blocking GQL-F2 gap Session
121 recorded: added §18 (GraphQL transport optimization — Hybrid V1 surface
split, the HTTP 200 + `errors` rule, error isolation and per-surface fallback,
run-scoped envelope disable, availability ownership, guard separation and
telemetry) and §19 acceptance criteria, and added Session 123 scope notes to
§9.1, §11 and §13. Independent review returned CHANGES REQUIRED and the
architecture owner accepted all six findings, closed in the same session: §18.12
(exact-total provider fields not adopted), §18.13 (empty-repository shape), the
§18.2 mapping obligation, the §18.7 `RetrievedAt` rule, the §18.9 counting rule,
and §19.2 / §19.11 / §19.14 / §19.15-§19.17. GraphQL is transport only: no
evidence surface, signal, score, status, evidence key, availability status,
limitation, warning, pattern, representative or report semantics changed, and no
production code, runtime numeric or performance claim was selected
**Production Code:** NOT IN SCOPE (contract only)

## 1. Purpose

This contract defines how much GitHub collection a Profile Proof run is allowed
to perform, and what that bound may never mean.

It owns:

- the Repository Proof / Profile Proof collection boundary,
- the required Profile V1 repository evidence surface,
- the enrichment surfaces Profile V1 must not collect,
- the semantic-equivalence invariant that constrains any composition change,
- the no-hidden-repository-cap rule,
- the request-amplification vs latency distinction,
- the minimal profile runtime telemetry surface,
- the end-to-end execution guard semantics,
- the future re-activation rule for the omitted enrichment surfaces,
- the Profile V1 decision-sufficient collection stop rules (§15),
- the Profile V1 consequences of the fork admission rule (§16).

It owns no signal, no scoring rule, no eligibility rule, no pattern promotion
rule, no representative-selection rule and no report presentation rule. Every
one of those stays with its existing canonical owner. In particular §15 consumes
`CollectionObservationState` from `analysis-domain-model.md` and the terminal
score permissions from `mvp-signal-definitions.md`, and §16 consumes the fork
admission rule from `public-profile-evidence-repository-eligibility.md`; it
defines none of them.

Input contracts:

- `docs/architecture/analysis-domain-model.md`
- `docs/architecture/mvp-signal-definitions.md`
- `docs/architecture/public-profile-evidence-domain-model.md`
- `docs/architecture/public-profile-evidence-repository-eligibility.md`
- `docs/architecture/repository-dependency-evidence.md`
- `docs/architecture/repository-source-evidence-collection.md`
- `docs/product/public-profile-evidence-product-contract.md`

Downstream consumer:

- `docs/product/p10-web-profile-flow-implementation-plan.md`

## 2. Existing Runtime Problem

The problem is request amplification, verified against the code, not a
performance opinion.

### 2.1 Verified current behavior

`GitHubProfileRepositorySnapshotSource` performs no collection of its own. It
delegates every admitted profile repository to the single-repository
`IGitHubRepositorySnapshotCollector`, whose production implementation is
`GitHubRepositorySnapshotOrchestrator.CreateDefault`. That composition contains
the full Repository Proof collector set, including the two enrichment
collectors:

- `GitHubRepositoryDependencyManifestCollector`,
- `GitHubRepositorySourceFileCollector`.

A Profile Proof run therefore currently pays the complete Repository Proof
collection cost once per admitted repository.

### 2.2 Verified enrichment cost bounds

These are the collectors' own theoretical maxima, not measured averages:

    GitHubRepositorySourceFileCollector
      2 tree-resolution requests
        1. GET repos/{owner}/{repo}/git/commits/{headCommitSha}
        2. GET repos/{owner}/{repo}/git/trees/{rootTreeSha}?recursive=1
      + up to MaximumSourceFiles (32) source-content requests
      = up to 34 requests per repository

    GitHubRepositoryDependencyManifestCollector
      up to MaximumManifestFiles (32) manifest-content requests,
      reduced by manifests whose content the contents collector already
      carries
      = up to 32 requests per repository

    Combined theoretical enrichment ceiling
      = up to 66 GitHub requests per admitted repository

The source collector needs both requests because it resolves the pinned commit
to its root tree SHA first, and only then reads the recursive tree; it obtains
its own tree because the regular-blob rule needs the Git mode, which
`RepositorySnapshot.FileTree` does not carry.

The profile repository manifest itself is separately bounded at
`GitHubProfileRepositoryCollector.DefaultMaxPageCount` (10) pages of 100, so the
admitted repository set can be large. The enrichment ceiling multiplies against
that set.

### 2.3 Why this is unnecessary work for Profile V1

Both enrichment surfaces are non-scoring for the existing `RepositoryAnalysis`,
and neither reaches any supported Profile V1 output:

- `RepositoryAnalysis.Status` is derived from
  `RepositorySnapshot.CollectionWarnings` and signal statuses only. The
  orchestrator deliberately keeps the dependency and source availability entries
  out of `CollectionWarnings` (`repository-dependency-evidence.md` §4.2,
  `repository-source-evidence-collection.md` §20), so neither surface can change
  it.
- `CrossRepositoryEvidenceComposer` consumes `RepositoryAnalysis.SignalResults`,
  the `languages` availability entry with `RepositorySnapshot.Languages`, the
  admission/contribution assessment, and
  `ProfileAnalysisOrchestrationResult.IsRepositoryManifestComplete`. It reads
  neither `SourceFiles` nor `DependencyDeclarations`.
- `RepositorySemanticEvidencePolicy` and
  `RepositorySourceReferenceEvidencePolicy` are the only Domain consumers of
  those surfaces, and neither is wired into the profile pipeline (Session 076
  finding, re-verified in Session 096: they have no `src/` caller at all).
- Their single documented downstream chain ends in ProblemArea
  (`repository-source-evidence-collection.md` §26), and ProblemArea is
  `Unsupported` in Profile V1.

So the requests are paid, and the result cannot reach an authorized Profile V1
output.

## 3. Repository Proof vs Profile Proof Collection Boundary

### 3.1 Repository Proof is unchanged

The existing single-repository Repository Proof collection path remains the full
repository evidence path. This contract does not weaken, remove, reorder or
otherwise change:

- any Repository Proof collector,
- the 15 signal semantics,
- scoring or category results,
- repository evidence or evidence keys,
- repository availability, limitation or warning semantics,
- source or dependency enrichment for Repository Proof.

A Repository Proof run keeps collecting `SourceFiles`,
`DependencyDeclarations` and `Contributors` exactly as it does today, on exactly
the bounds their own contracts define, and it never produces a
`DecisionBounded` collection (§15): every Repository Proof collection is
`Enumerated` or `Incomplete`.

### 3.2 Profile Proof may use its own composition

Profile Proof may use a dedicated repository snapshot collection composition
instead of reusing the complete Repository Proof collector set.

The seam is the existing `IProfileRepositorySnapshotSource` boundary. Nothing in
the profile pipeline requires that this boundary resolve the same collector
composition that the Repository Proof entry resolves.

### 3.3 What may differ and what may not

    The collection composition is allowed to differ.
    The analysis semantics are not.

## 4. Required Profile V1 Repository Evidence Surface

The following inputs feed existing Profile V1 semantics and **must** continue to
be collected for every admitted profile repository:

- repository metadata and identity,
- default-branch file tree,
- project files,
- workflows,
- README,
- license and license file,
- documentation Markdown,
- languages,
- commits,
- issues,
- pull requests,
- releases,
- tags,
- topics.

`Contributors` was a required surface in the Session 096 text. Session 098
moved it to §5 after re-verifying it has no production consumer (§5.1.1).

Each of those feeds at least one of: the canonical 15 signals
(`mvp-signal-definitions.md`), the eligibility/contribution assessment
(`public-profile-evidence-repository-eligibility.md`), or the language evidence
`CrossRepositoryEvidenceComposer` reads directly.

Omission of any surface in that list is **not** authorized by this contract, and
must never be inferred from §5. An additional omission requires its own proof and
its own contract amendment.

## 5. Explicitly Omitted V1 Enrichment Surfaces

Profile V1 repository analysis **must not** collect, during the normal Profile
Proof aggregation path:

- bounded raw `SourceFiles` (`repository-source-evidence-collection.md`),
- `DependencyDeclarations` / dependency-manifest enrichment
  (`repository-dependency-evidence.md`),
- `Contributors` (added by Session 098; see §5.1.1).

The omission list is exactly those three surfaces. It is closed.

### 5.1 Basis for the omission

Locked by the existing isolation proof, re-verified in Session 096:

- `tests/GitHubProof.Application.Tests/Analysis/RepositorySourceEvidenceIsolationTests.cs`
- `tests/GitHubProof.Application.Tests/Analysis/RepositoryDependencyEvidenceIsolationTests.cs`

Both test classes compare an analysis over a snapshot **without** the surface
(and without its availability entry) against an analysis **with** it, and lock
that `Status`, `EarnedScore`, `ApplicableMaximumScore`, `NormalizedScore`, the 15
`SignalResults` (key, status, score), `LimitationKeys`, warning codes and — for
the source surface — category results are identical. Both also lock that an
`Unavailable` enrichment surface does not degrade `AnalysisStatus` and does not
introduce `limitation.snapshot.collection-partial`.

The baseline arm of those tests is the absent case. That is precisely the state
an omitting Profile composition produces, which is why the omission is provable
rather than merely plausible.

### 5.1.1 Basis for the Contributors omission (Session 098)

Re-verified against the current tree, not inherited from an earlier session.

The surface has a collector, a collection type, an interface declaration and a
snapshot property, exactly like every other surface. What matters is that
**nothing reads it**. Outside its own collection and transport types, every
`Contributor` occurrence in `src/` is one of:

~~~text
RepositorySnapshot                      ctor parameter, normalization, property
                                        (carrier only, never read by a consumer)
GitHubRepositorySnapshotOrchestrator    the single line writing it into the
                                        snapshot
ProfileGitHubRequestCategory            a telemetry category name (not a reader)
~~~

The excluded types are `GitHubRepositoryContributorCollector`,
`GitHubRepositoryContributorCollection` and the `IGitHubRepositoryCollectors`
interface declaration — the surface's own production path, which Repository Proof
keeps using unchanged.

Supporting facts, each checked directly:

- **no** signal evaluator declares `contributors` in its `requiredDataKeys`
  (all fourteen independent evaluators plus the presentation-completeness
  evaluator were enumerated);
- **no** `FindAvailability("contributors")` call exists anywhere;
- **nothing** enumerates `RepositorySnapshot.Availability` wholesale, so the
  entry cannot be consumed indirectly;
- `CrossRepositoryEvidenceComposer` reads signal results, the `languages`
  availability entry, the contribution assessment and
  `IsRepositoryManifestComplete` only.

Therefore omitting it is provably neutral for the canonical 15 signals,
`RepositoryAnalysis` (status, score, limitations, warnings), eligibility,
`CrossRepositoryEvidenceComposer`, pattern promotion, representative selection
and `ProfileReport`.

Required consequences on the Profile path: no contributor GitHub request, no
`contributors` availability entry, and no warning, limitation or status effect.
Repository Proof continues to collect contributors exactly as before (§3.1).

### 5.2 What the omission may never become

The omission must never:

- become negative evidence about the repository or the developer,
- produce or support a developer judgment of any kind,
- create a repository collection warning, a `ProfileWarning` or a
  `ProfileLimitation`,
- by itself downgrade `RepositoryAnalysis.Status`, profile-level `AnalysisStatus`
  or any `DataAvailability` interpretation of another surface,
- alter any of the canonical 15 signal outputs,
- alter `TechnicalFocusAreas`, `EngineeringPracticePatterns`,
  `RepresentativeRepositories`, `WhyThisConclusion`, `Limitations` or any other
  `ProfileReport` section,
- affect Repository Proof.

### 5.3 Consistency with the expected evidence surface

`public-profile-evidence-domain-model.md` §13 defines the expected evidence
surface as the supported and applicable obligations that the eligibility,
pattern and representative-repository contracts already impose, and states that
an Unsupported or Deferred capability is not an obligation and its absence is
never a completeness deficit.

**None of the three omitted surfaces** is an obligation under any of those three
contracts:

- `SourceFiles` and `DependencyDeclarations` have exactly one documented
  downstream chain, and it ends in the Unsupported ProblemArea capability
  (`repository-source-evidence-collection.md` §26);
- `Contributors` has **no production consumer at all** (§5.1.1).

The omission of any of the three is therefore not a completeness deficit and must
never be accounted as one.

### 5.4 Non-extension rule

Do not extend the omission list without explicit proof. In particular the
following are **not** covered and are **not** safe to omit: commits, issues,
pull requests, releases, tags, README, languages, file tree, project files,
workflows, repository metadata, topics, license, documentation Markdown. Each
feeds existing signal or profile semantics unless separately proven otherwise,
by the same standard of proof used in §5.1 and §5.1.1.

`Contributors` was removed from this list by Session 098 because that proof was
produced. Removing any other name requires the same evidence.

Note the distinction this section does **not** cover: §15 bounds *how much* of a
required surface is enumerated. It never omits a required surface.

## 6. Semantic-Equivalence Invariant

Any Profile-specific collection composition must satisfy:

> For every profile input, the optimized Profile V1 composition must produce a
> semantically identical result to the current composition.

Semantically identical means, per admitted repository:

- identical `RepositoryAnalysis.Status`,
- identical score breakdown (earned, applicable maximum, normalized),
- identical 15 `SignalResults` by key, status and score,
- identical evidence keys per signal,
- identical `LimitationKeys`,
- identical warning codes,
- identical category results;

and at profile level:

- identical repository universe and admission decisions,
- identical `RepositoryContributionAssessment` set,
- identical `ObservedProfileEvidence` set,
- identical `ProfilePatternSet` and promotion outcomes,
- identical `RepresentativeRepositorySelection`,
- identical `AnalysisStatus`, `ProfileLimitation` and `ProfileWarning` sets,
- identical `ProfileReport` section content and ordering.

If a composition change cannot meet this invariant, it is out of scope for this
contract and requires a product/domain decision instead.

**Session 098 scope note.** This invariant governs *collection composition*
changes. §16 is not one: it is an eligibility semantics change made through the
product/domain route this paragraph requires, so it deliberately changes the
admission decision for a known fork and therefore the observation set that
follows from it. §15 remains fully inside the invariant — a `DecisionBounded`
collection must still produce the identical repository-level and profile-level
result, with the single documented exception of the §15.5 evidence-value
language.

## 7. Repository Completeness — No Hidden Cap

Runtime cost must not be reduced by silently analyzing only the first N
repositories.

Current profile truth depends on repetition across eligible repositories
(`public-profile-evidence-patterns.md`, independent-repetition semantics). A
repository-count truncation would change the evidence surface itself and could
change which patterns are promoted, which repositories are representative, and
whether the profile is Complete or Partial.

Therefore:

- an invisible "top N repositories" optimization is **prohibited**;
- no ranking, sampling, scoring or heuristic subset selection may be introduced
  as a cost measure;
- a repository-count bound, if it is ever wanted, is a separate product/domain
  decision with its own contract, its own visibility rule and its own
  completeness semantics.

Where the repository manifest itself is already incomplete because of an existing
explicit collection bound or failure — the `GitHubProfileRepositoryCollector`
page bound producing `ProfileRepositoryManifest(isComplete: false)` plus its
collection warning, or a failed page — the existing completeness and limitation
semantics remain authoritative and unchanged. That is a declared bound, not a
hidden cap.

## 8. Request Amplification vs Latency

These are different problems and must not be traded against each other:

    request amplification   =  how many GitHub requests a profile run issues
    wall-clock latency      =  how long the run takes to finish

Parallelism is not a request-cost optimization. Bounded concurrency may reduce
elapsed time later, but:

- it must never be used as a substitute for removing unnecessary requests,
- it must respect GitHub primary rate limits and secondary-limit pressure,
- broad `Task.WhenAll` fan-out over repositories or collectors is **not**
  authorized by this contract.

Unnecessary requests are removed first, and measured (§9), before any concurrency
work is considered.

## 9. Runtime Telemetry Contract

A profile analysis execution must make the following observable. This is the
minimum surface; a richer implementation is allowed as long as §9.1 holds.

- total elapsed duration of the profile analysis,
- total GitHub HTTP request count for the run,
- request count grouped by collector / evidence surface, or an equivalent
  bounded category set,
- repositories discovered,
- repositories admitted for repository analysis,
- repositories whose analysis completed,
- repositories whose analysis could not complete,
- final profile result status,
- rate-limit, forbidden, failed or cancellation outcome when applicable.

Prefer structural counters and events over logging raw payloads. The category set
must be bounded and enumerable, never derived from repository or file identities.

### 9.1 Telemetry prohibitions

Telemetry must never:

- contain GitHub tokens or any credential material,
- contain source file contents or any source substring,
- contain README contents,
- contain dependency manifest contents,
- change analysis semantics in any way,
- become evidence, or be read by any evaluator, policy, composer, projector or
  report section.

This restates, at profile level, the logging boundary already locked by
`repository-source-evidence-collection.md` §21.

**Session 123 note.** This list is not exhaustive once a second transport
exists. §18.9 adds the GraphQL-specific prohibitions — query text, query
variables beyond existing bounded policy, `data` payloads and raw error
messages — and they bind exactly as strictly as the list above.

### 9.2 Purpose

Telemetry exists so the implementation session can measure the actual request
surface after §5 is applied, and so production guard values (§10) can be chosen
from measurement rather than guessed.

## 10. Execution Guard Contract

Profile analysis must have a bounded end-to-end execution lifetime before
production beta.

The guard is an **internal operational deadline**. It is a different thing from
externally supplied request cancellation, and §11 separates the two exhaustively.
Nothing in this section changes the current production cancellation behavior.

Locked principles:

1. **Guard expiration is never Complete.** A run stopped by the internal guard
   must never present its partial result as a complete profile, and must never
   take the repositories completed so far and present that subset as a whole
   profile.
2. **Guard expiration is not request cancellation.** Ordinary caller/request
   cancellation must never be silently reinterpreted as guard expiration, and
   guard expiration must never be reported as if the caller had cancelled. The
   two are distinct outcomes with distinct rules (§11.1, §11.2).
3. **No invented domain evidence status.** Guard expiration must not introduce a
   new domain evidence status, a new `AnalysisStatus` value, a new
   `DataAvailabilityStatus` value or a new state vocabulary. The outer
   runtime/Web boundary must expose a typed non-success outcome using the
   existing status/availability vocabulary **where that vocabulary can represent
   the outcome truthfully**. If the existing runtime result types cannot
   truthfully represent internal guard expiration without semantic abuse, the
   implementation session **MUST STOP** and report a typed-boundary gap before
   implementing the guard, rather than stretching an existing value to fit.
4. **The guard is operational safety, not evidence.** Guard expiration says
   something about this execution, never about the developer, the repositories or
   the quality of the work.
5. **Configuration is external.** Guard values must be configurable
   (configuration/options), never embedded into domain evidence rules, policy
   versions or evidence semantics. Changing a guard value must not change a rule
   version.
6. **No hidden subset fallback.** Analyzing a subset and presenting it as a
   complete profile is prohibited — the same rule as §7, seen from the failure
   side.
7. **Numbers come after measurement.** Exact production numeric values may be
   selected during the implementation/measurement session, after the unnecessary
   request surfaces of §5 have been removed and the §9 telemetry has measured the
   remaining surface.

### 10.1 Concurrent execution policy

Production beta must additionally have a bounded concurrent-profile-execution
policy before deployment: a limit on how many profile analyses may run at once.

This contract records the requirement only. It does not implement, design or
authorize queueing, distributed coordination, background job infrastructure or
any other such mechanism.

### 10.2 Live runtime result boundary (Session 107)

The future live Web/runtime result has exactly this semantic shape:

```text
ProfileAvailability?
+ ExecutionAvailability
+ ProfilePipelineResult?
```

Both availability values reuse the existing `DataAvailability` /
`DataAvailabilityStatus` vocabulary. `ExecutionAvailability` is operational
runtime truth only: its `DataKey` is exactly `profile-execution`, it is not
repository availability or Domain evidence, and it may never be read by an
evaluator, policy, composer, projector or report section.

The allowed combinations and stable reason identities are exhaustive for the
initial Milestone B implementation:

| Profile availability | Execution status | Exact execution reason | Execution `RetrievedAt` | Pipeline result |
|---|---|---|---|---|
| unresolved (`null`) | `Failed` | `execution-deadline-expired` | `null` | `null` |
| non-`Available` | `NotRequested` | `profile-unavailable` | `null` | `null` |
| `Available` | `Available` | `null` | UTC terminal-result instant | non-null |
| `Available` | `Failed` | `execution-deadline-expired` | `null` | `null` |
| `Available` | `Unavailable` | `github-budget-protection-rejected` | `null` | `null` |

Every `ExecutionAvailability` has `DataKey = "profile-execution"`. For successful
execution, `Status = Available`, `Reason = null`, and `RetrievedAt` is the UTC
instant at which the live Profile execution successfully reaches its terminal
non-null `ProfilePipelineResult`. Every non-success execution outcome has
`RetrievedAt = null`, preserving the existing `DataAvailability` invariant.

This execution `RetrievedAt` is operational runtime metadata only. It is not
evidence, profile observation time, repository retrieval time, analysis
provenance or report truth. Domain/Application evaluators, policies, composers
and projectors must not consume it, and presentation must not expose or interpret
it as developer evidence.

For `ExecutionAvailability`, the only allowed statuses are therefore
`Available`, `NotRequested`, `Failed` and `Unavailable`. `RateLimited`,
`Forbidden` and `NotApplicable` are prohibited: an internal execution guard
cannot claim that GitHub rate-limited or forbade evidence, and execution is not
an applicability decision. No additional execution reason identity is
authorized by this contract.

`ExecutionAvailability = Available` means a `ProfilePipelineResult` exists; it
does not mean that composition succeeded. A truthful
`ProfilePipelineResult.Failed` therefore remains non-null beside available
execution. Ordinary thrown faults and per-request transport timeouts retain
their existing propagation behavior and do not authorize another typed
execution reason. Caller cancellation remains governed by §11.1.

A null `ProfileAvailability` has exactly one meaning: profile availability never
resolved before an execution-level terminal event. In the initial exhaustive
vocabulary, internal deadline expiration is the only event authorized to
produce that state. It is outer runtime truth only, never a synthetic
`Unavailable`/`Failed` provider result. It may not enter Domain/Application
evidence, profile warnings, limitations, scoring, composition or projection.
Once profile availability has resolved, a later execution terminal event
preserves that truthful value; it never resets it to null or rewrites its
status/reason.

Internal deadline expiration and proactive budget rejection terminate at this
outer boundary. Neither may fabricate a `ProfilePipelineResult`, reuse
completed-so-far repositories, compose a `Partial` profile, alter profile or
repository availability, or become a warning, limitation, score or developer
claim.

### 10.3 Deadline ownership, start and transport relationship (Session 107)

The internal deadline begins after a valid Profile POST has passed the
visitor/same-profile/global Web admission guards, but **before the first live
GitHub profile-metadata operation**. It remains active through profile metadata
collection and content consumption, the proactive GitHub-budget decision,
public-repository manifest collection, every admitted-repository
collection/analysis step, profile composition, report projection and
construction of the outer live terminal result. Razor rendering and
response-body transmission are outside it.

If the deadline expires before profile availability resolves, the outer result
uses the null-profile row in §10.2. If profile availability resolves
non-successfully first, its truthful provider result is preserved and execution
is `NotRequested`. If availability has resolved as `Available`, a later deadline
preserves it beside the `Failed` execution outcome. At no point may deadline
expiration be rewritten as profile/provider availability.

The deadline is owned by a dedicated internal cancellation source. It must not
mutate `HttpContext.RequestAborted` and must not be implemented by treating
ASP.NET request-timeout middleware as the internal guard.

The configuration invariant is:

```text
0 < per-request Profile GitHub transport timeout
  < end-to-end Profile execution deadline
```

The current Profile `HttpClient` timeout is two minutes; this is an observed
implementation value, not a newly canonicalized production number. An outer
deadline equal to that timeout is invalid. Exact transport and execution values
remain external configuration/measurement decisions, but startup validation
must enforce the strict relationship.

The current GitHub collectors use `HttpCompletionOption.ResponseHeadersRead`.
Under that transport mode, `HttpClient.Timeout` bounds the operation only until
the response headers are obtained; it does **not** by itself bound subsequent
response-content consumption. Therefore `HttpClient.Timeout` alone cannot prove
the bounded end-to-end lifetime or even bound the whole metadata phase. Every
live GitHub response-content read must observe the internal execution deadline
token, or another explicitly owned bounded token whose identity remains
distinguishable from caller cancellation, transport timeout and the execution
deadline. No content read may become an unbounded gap between those guards.

Attribution must be based on independently owned cancellation sources and a
recorded terminal cause, never exception text. Caller cancellation remains the
caller path and propagates; if caller cancellation overlaps another source it
wins. With no caller cancellation, an expiration caused by the internal source
maps to `execution-deadline-expired`. A per-request timeout for which the
internal deadline was not the terminal cause remains a transport fault and
retains the existing propagation behavior. The implementation must prove all
three paths and their overlap boundary deterministically.

### 10.4 Initial provider-protection and authentication scope (Session 107)

Initial Milestone B provider-budget/header protection is **Profile-only**. It
may observe sanitized GitHub response headers and reject further Profile work at
the §10.3 boundary, but it must not wrap, block or otherwise change Repository
Proof requests. A shared Repository/Profile operational circuit requires a
separate canonical decision and proof that Repository Proof collection,
availability and evidence semantics remain unchanged.

A proactive internal budget rejection is GitHub Proof's own operational
protection decision, not proof of a GitHub `RateLimited`, `Forbidden` or
repository failure response. Its only typed result is the §10.2
`Unavailable`/`github-budget-protection-rejected` combination. It never enters
collector availability or completed-so-far profile composition. The reserve
value remains externally configured and measurement-selected; `500` is not
canonicalized.

Unauthenticated production Profile beta is not acceptable for the measured
request surface. A dedicated machine-user fine-grained PAT may be supplied
through external secret configuration as an interim beta credential only after
every used endpoint is verified compatible and secret storage, least privilege,
expiry and rotation are handled. This authorizes no private-repository access
and no write/admin permission. GitHub App remains a valid longer-term
production candidate and is not rejected. Credentials and credential values
never enter telemetry, availability reasons, evidence or reports.

### 10.5 Deployment dependency and numeric boundary (Session 107)

IP-based visitor limiting cannot receive production PASS until the deployment
owner supplies the actual trusted proxy/network and hop topology. The
implementation must configure only those supplied trusted proxies/networks;
forwarded headers are not trusted generically and application code must not
parse raw `X-Forwarded-For`.

Visitor `2 / 10 minutes`, GitHub reserve `500` and the exact execution deadline
remain configuration/measurement candidates, not canonical policy. Initial
global Profile concurrency `1`, same-profile concurrency `1` and queue `0`
remain provisional implementation recommendations. If the same-profile guard
rejects a duplicate, its Web mapping is canonically `409` (§10.2's downstream
P10 contract); global-capacity rejection is `503` and visitor-limiter rejection
is `429`. These HTTP semantics do not canonicalize the numeric permit/window
configuration. Production concurrency remains bounded, admission outcomes are
operational rather than evidence, and no rejected or expired execution
fabricates an analysis result.

## 11. Failure and Cancellation Behavior

**Session 123 note.** This section stays authoritative for every failure the
Profile path can produce, on any transport. §18.4-§18.8 add how a GraphQL
response maps onto it; they introduce no new failure kind and no new status, and
§18.8 keeps §11.1-§11.3 strictly separate from GraphQL error handling.

Collection and analysis failure semantics are unchanged, and restated here so
the guard work cannot drift:

- A collector failure remains a typed `DataAvailability` outcome (`RateLimited`,
  `Forbidden`, `Failed`, `NotRequested`, ...) on its own surface.
- A repository whose collection or analysis could not complete remains the
  existing `ProfileRepositoryAnalysisResult` failure/unavailable case and flows
  into the existing contribution and completeness accounting.
- An omitted §5 enrichment surface is not a failure and must not be reported
  through any of the above.

Cancellation is **two different things**, and this contract keeps them separate.
§11.1 describes current production behavior and does not change it. §11.2
describes a mechanism that does not exist yet.

### 11.1 External / request cancellation

This is cancellation supplied by the caller: client disconnect,
`HttpContext.RequestAborted`, a host shutdown token, or any externally supplied
`CancellationToken`.

Verified current production behavior, which stays exactly as it is:

- `ProfileRepositoryAnalysisOrchestrator` explicitly rethrows
  `OperationCanceledException` when its supplied token is cancelled — both from
  the per-repository collection call and from the analysis call — and checks the
  token before each repository.
- `LiveProfileReportService` passes the supplied token through to the collector
  and to `ProfilePipeline.RunAsync`, and performs no cancellation-to-result
  conversion of its own.

Locked rules:

- `OperationCanceledException` continues to propagate; it is not swallowed.
- No `RepositoryAnalysis`, no `ProfilePipelineResult`, no partial profile and no
  failure profile may be fabricated merely to convert a caller cancellation into
  a result.
- Caller cancellation is not developer evidence and says nothing about the
  developer, the repositories or the work.
- Session 096 changes no production cancellation code.

### 11.2 Internal profile execution guard / deadline

This is the §10.3 guard: an internal operational deadline owned outside domain
evidence semantics, and it does not exist yet.

Locked rules:

- Guard expiration is distinct from §11.1 caller cancellation, and the two must
  never be conflated in either direction.
- Guard expiration may never yield `Complete`, and may never present the
  repositories completed so far as a whole profile.
- The outer runtime/Web boundary exposes the exact §10.2
  `Failed`/`execution-deadline-expired` outcome with no pipeline result.
  `ProfileAvailability` is null if expiration won before availability resolved;
  otherwise its already-resolved truthful value is preserved.
- No new domain evidence status may be invented for it.
- Guard expiration is operational truth about one execution, never developer
  evidence.
- Deadline ownership, start/end boundary, transport relationship and
  deterministic attribution are exactly §10.3.

### 11.3 Proactive internal GitHub-budget rejection

This is GitHub Proof's Profile-only §10.4 protection circuit, not a provider
response and not collection evidence.

Locked rules:

- Rejection after profile metadata becomes available produces the exact §10.2
  `Unavailable`/`github-budget-protection-rejected` execution outcome and no
  pipeline result.
- It must terminate before repository-manifest and per-repository execution
  begins.
- It must not be caught or normalized by a repository collector, and must not
  produce repository `RateLimited`, `Forbidden`, `Failed` or `Unavailable`
  availability.
- It must not compose repositories completed earlier into a `Partial` profile,
  fabricate a failure profile, or become a warning/limitation/evidence item.
- A real GitHub primary/secondary rate-limit response remains the existing
  provider-failure path on the surface where it occurred; the internal reserve
  decision must never impersonate it.

## 12. Future Feature Re-Activation Rule

Source and dependency collection must not be silently re-enabled for every
Profile Proof repository merely because a future feature wants it.

If a future **supported** feature actually requires one of those enrichments, all
of the following are required before collection is re-enabled on the profile
path:

- the consumer is explicit and named (which policy, composer or section reads
  it),
- the cost is bounded and stated,
- the collection scope is documented (all admitted repositories, or a
  contractually defined narrower scope),
- the profile semantic effect is versioned and re-validated under the existing
  version-ownership rules,
- the resulting request amplification is measured with §9 telemetry.

ProblemArea becoming `Supported`, or `RoleEvidenceAlignment` becoming active,
would require a new contract review under this rule. Neither may silently expand
the current Profile V1 request surface.

## 13. Non-Goals

**Session 098 scope note.** The list below is Session 096's. Two of its items
were deliberately narrowed by Session 098 and are restated after the list; every
other item still holds exactly as written.

This contract does not:

- change any Repository Proof behavior,
- change any of the 15 signals, their scoring or their evidence,
- change eligibility thresholds, contribution rules or the current activity
  window,
- change pattern promotion, representative selection or report section semantics,
- introduce a repository-count cap or any subset selection rule,
- authorize concurrency, fan-out, queueing or distributed execution,
- introduce persistence, caching or replay of any collected surface,
- fix production numeric values for the execution guard,
- implement anything: no production code is in scope for this session.

**Session 123 scope note.** Every item above still holds. §18 authorizes a
transport *design* and its acceptance criteria only — it implements nothing,
introduces no concurrency, fan-out, queueing, persistence, caching or replay,
fixes no production numeric, and adds no repository-count cap or subset
selection rule. The "no production code is in scope for this session" item is
Session 096's statement about Session 096, and remains true of Session 123 as
well.

Narrowed by Session 098:

- **signals and their evidence** — the 15 signal thresholds, scores and evidence
  *keys* are still unchanged, but §15 changes when the two commit signals may
  produce a terminal score, and §15.5 changes evidence *values* for a
  `DecisionBounded` collection. Canonical owner: `mvp-signal-definitions.md`.
- **contribution rules** — eligibility thresholds and the current activity window
  are still unchanged, but §16 changes fork admission. Canonical owner:
  `public-profile-evidence-repository-eligibility.md` §6.3.

Repository Proof behavior remains unchanged without exception.

## 14. Implementation Acceptance Criteria (Session 096)

**Session 098 supersession note.** This section is the historical Session 096
acceptance surface, and Session 097 implemented it. It is preserved unchanged as
that baseline.

Where a §17 requirement deliberately changes an invariant stated here, **§17
supersedes the conflicting §14 requirement for Session 099 only**. The one
current instance is §14.2's "or any Domain policy" restriction: it does **not**
prohibit the explicitly authorized Fork Policy B eligibility amendment (§16,
`public-profile-evidence-repository-eligibility.md` §6.3), which necessarily
changes `AnalysisAdmissionPolicy` and the `EligibilityPolicy` version identity.

Everything else in §14.2 still binds:
`ProfileRepositoryAnalysisOrchestrator`, `CrossRepositoryEvidenceComposer`,
`ProfileComposer` and `ProfileReportProjector` remain unchanged unless another
§17 rule explicitly requires otherwise, and no §14 or §17 rule weakens
Repository Proof preservation.

An implementation of this contract is acceptable only if all of the following
hold:

1. Repository Proof composition is unchanged, and its tests are unchanged and
   green.
2. The Profile path resolves a Profile-specific composition at the
   `IProfileRepositorySnapshotSource` seam without modifying
   `ProfileRepositoryAnalysisOrchestrator`, `CrossRepositoryEvidenceComposer`,
   `ProfileComposer`, `ProfileReportProjector` or any Domain policy.
3. The Profile path issues zero requests for the two §5 surfaces: no pinned
   source tree request, no source content request, no dependency manifest content
   request.
4. A test proves §6 semantic equivalence: the same profile input through the
   omitting composition and through the full composition yields the same
   `ProfileReport` and the same profile-level status, limitations and warnings.
5. A test proves that the omission produces no warning, no limitation and no
   status change at repository or profile level.
6. The existing isolation tests (`RepositorySourceEvidenceIsolationTests`,
   `RepositoryDependencyEvidenceIsolationTests`) remain unchanged and green.
7. §9 telemetry exists with the listed counters, and a test proves the §9.1
   prohibitions for at least tokens and content payloads.
8. No repository-count cap, ranking, sampling or subset selection is introduced.
9. No `Task.WhenAll` fan-out over repositories or collectors is introduced.
10. Existing external/request cancellation behavior is unchanged: no test or
    production path converts an `OperationCanceledException` into a fabricated
    `RepositoryAnalysis`, `ProfilePipelineResult`, partial profile or failure
    profile (§11.1).
11. The execution guard, when implemented, is externally configured, cannot yield
    a `Complete` profile on expiration, is never conflated with caller
    cancellation in either direction, and introduces no new domain evidence
    status. If it cannot be represented truthfully by the existing runtime result
    types, the implementation stops and reports a typed-boundary gap instead
    (§10 principle 3, §11.2).
12. Full solution build and test run green with 0 warnings.

## 15. Decision-Sufficient Collection (Session 098)

The Session 097 controlled live smoke failed: a real profile run exceeded three
minutes without producing a result, dominated by unbounded commit pagination on
one repository. The cause was not the removed enrichment surfaces. It was that
the collection model could not express **"enough has been observed"** — only
**"everything was enumerated"** or **"collection broke"**.

### 15.1 The distinction

    Enumeration completeness
      every item in the provider collection was enumerated

    Decision sufficiency
      every currently authorized consumer of this Profile V1 surface can
      already make its semantic decision irreversibly from what was observed

`IsComplete` keeps its exact existing meaning and is never overloaded. The
distinction is carried by `CollectionObservationState`, whose canonical owner is
`docs/architecture/analysis-domain-model.md`:

    Enumerated        IsComplete = true
    DecisionBounded   IsComplete = false, deliberate, no warning
    Incomplete        IsComplete = false, failure/interruption, today's semantics

`DecisionBounded` requires: `TotalCount = null`, `DataAvailability` unchanged at
`Available`, **no** `CollectionWarning`, and therefore no
`limitation.snapshot.collection-partial` and no `RepositoryAnalysis.Status`
downgrade from the bound alone. It is never negative developer evidence.

### 15.2 Why every early-stopping surface needs this

`IssueUsage`, `PullRequestUsage` and `ReleaseUsage` already produce their
terminal maximum result before consulting `IsComplete`
(`mvp-signal-definitions.md`). That is a **signal-level** permission only.

At the **repository-analysis level** the picture is different: today every
collector represents an unfinished collection by emitting a `CollectionWarning`,
and `GitHubRepositorySnapshotOrchestrator` merges those warnings into
`RepositorySnapshot.CollectionWarnings`, which produces
`limitation.snapshot.collection-partial` and turns `RepositoryAnalysis.Status`
into `Partial`.

Therefore stopping issues, pull requests, releases or tags early **without**
`DecisionBounded` would still change `RepositoryAnalysis.Status`. This contract
does not claim those surfaces need no repository-analysis-level change. They do.

### 15.3 Per-surface Profile V1 stop rules

Each rule is monotone-maximal: the observed quantity can only grow, and the
result is already the signal's maximum, so no further page can change it. The
monotonicity is independent of provider ordering.

| Surface | Stop when | Resulting state |
|---|---|---|
| commits | observed count >= 10 **and** distinct observed UTC commit days >= 8 **and** observed span >= 14 days | `DecisionBounded` |
| issues | >= 2 qualifying issues observed | `DecisionBounded` if further pages exist |
| pull requests | >= 2 merged pull requests observed | `DecisionBounded` if further pages exist |
| releases | first published release with notes observed | `DecisionBounded` if further pages exist |
| tags | >= 1 tag observed, **or** the first page is exhausted with no tag | `DecisionBounded` if >= 1 tag and further pages exist; `Enumerated` if no next page exists |

The tags rule is a decision-sufficiency rule, not a one-page cap. `tagCount >= 1`
already pins the tag contribution, and a first page containing no tag has no
`rel="next"`, so the collection is genuinely `Enumerated`. The two conditions
together cover every case; neither permits stopping while a tag decision is still
open.

A stop condition that is never reached means the surface is enumerated exactly
as it is today. No arbitrary page cap, no sampling, no approximation and no
probabilistic inference is authorized by this section.

### 15.4 Commits — what cannot be proven

For commits the fallback is not a convenience, it is a proof obligation.

`CommitHistory` bands below 10 (1, 2, 3, 5, 7) require an exact count.
`CommitDistribution` bands below Strong (1, 2, 3, 4, 5, 6 active days) require an
exact distinct-day count, and **any unseen commit can add a new distinct day**.

There is no provider field that establishes the exact distinct commit-day count
over a repository's whole history. The GitHub statistics endpoints cover only the
last 52 weeks and answer asynchronously, so they cannot establish it either.

Consequence, stated plainly: **a repository with thousands of commits spread over
fewer than eight distinct UTC days cannot have its exact `CommitDistribution`
classification proven without enumerating its full history.** Such repositories
are enumerated in full. No bound is manufactured for them, and the §10 execution
guard remains the only outer operational limit.

`Link rel="last"` count optimization is deliberately **not** adopted in Profile
V1: it does not establish the distinct-day fact and would broaden the first
implementation without closing the actual gap.

### 15.5 Evidence language

The lower-bound vocabulary applies **strictly and only** to
`CollectionObservationState.DecisionBounded`.

Evidence derived from a `DecisionBounded` collection must report observed lower
bounds and never exact repository totals — `observedCountAtLeast`,
`activeDaysAtLeast`, `spanDaysAtLeast`, `firstObservedCommitAt`. Evidence keys
stay stable.

Explicitly preserved, with no change of any kind:

- `Enumerated` evidence semantics;
- `Incomplete` evidence semantics — `Incomplete` is also non-`Enumerated`, and it
  is **not** covered by this rule;
- Repository Proof evidence semantics, which produce only `Enumerated` or
  `Incomplete` collections and therefore never enter this rule at all.

The canonical owner of this rule is `mvp-signal-definitions.md`, whose wording
carries the same strict scope.

## 16. Fork Policy (Session 098)

**This is a Profile V1 product and eligibility semantics change. It is not a
performance optimization and must never be described as one.**

For a repository whose profile manifest deterministically reports
`Fork = Present`:

    AnalysisAdmissionDecision = Skip
    Reason                    = ForkDetected

Canonical owner of the rule and its evidence-surface baseline:
`public-profile-evidence-repository-eligibility.md` §6.3, §19.3.2, §22.1.1, §25.

Profile V1 consequences:

- the repository remains visible in the profile repository universe and its
  observable fork condition remains visible;
- `RepositoryAnalysis` is not run for it, so this contract's §4 required-surface
  obligation does not apply to it at all;
- it contributes no `ProfileObservation`, no `IndependentRepetition`, no
  `CurrentFocus` and no `HistoricalEvidence`;
- it cannot promote a `TechnicalFocus` or `EngineeringPractice` pattern, and it
  can never become a `RepresentativeRepository` — both were already true, because
  promotion and selection draw only from independent supporters;
- a failure to deeply collect a fork can no longer make the whole profile
  `Partial`, because that deep collection is not a V1 evidence obligation.

### 16.1 The accepted false negative

This deliberately accepts that the profile owner may have meaningful work inside
a fork and that Profile V1 will not see it.

The reason is epistemic, not operational: Profile V1 owns **no attribution
contract** capable of proving which changes inside a fork belong to the profile
owner. Using the fork as developer evidence would assert something the contract
cannot support.

### 16.2 Reactivation

Fork evidence may be reactivated only by a new, explicit attribution surface —
for example a contract able to prove owner-authored delta or contribution. No
such capability may be inferred today, and no partial fork collection may be
introduced as a substitute.

`TemplateDerived` semantics are **not** changed by Session 098.

## 17. Session 098 Acceptance Criteria

These are additional to §14, which remains Session 096's criteria unchanged and
which §17 supersedes only where §14's supersession note says so. Session 099 must
be able to satisfy §14 and §17 together; no criterion here may be read as
requiring something another criterion forbids.

1. The Profile path issues zero contributor requests and contributes no
   `contributors` availability entry; Repository Proof still collects them.
2. A `DecisionBounded` collection produces no `CollectionWarning`, no
   `limitation.snapshot.collection-partial` and no `RepositoryAnalysis.Status`
   downgrade attributable to the bound.
3. `IsComplete` is `true` only for `Enumerated`; no code path fabricates it.
4. `TotalCount` is `null` for every `DecisionBounded` collection.
5. Each §15.3 stop rule is proven monotone-maximal by test, and a repository that
   never reaches its stop condition is still enumerated in full.
6. Evidence values from a `DecisionBounded` collection use lower-bound language
   and carry no exact-total or repository-first-commit claim.
7. A known fork is skipped before any repository collection request, and its
   eligibility result is `NonContributing` with reason `ForkDetected`.
8. For a profile whose forks were already non-independent, and given the same
   independent supporting repositories:
   - no `RepeatedProfilePattern` is created or removed **solely** because a fork
     is now skipped — pattern existence and pattern identity are preserved;
   - `IndependentSupportingRepositories` stays governed by the unchanged
     `MinimumIndependentSupportingRepositories` (>= 2) rule;
   - `RepresentativeRepository` winner and selection semantics are unchanged.

   This is deliberately **not** a claim of exact `ProfilePatternSet` or
   `ProfileReport` equality across the old and new fork policy.
   Fork-derived `SupportingObservations`, `SupportingRepositories`,
   `ProfileEvidenceReference` entries and traceability content **may
   intentionally disappear**, because `CrossRepositoryEvidenceComposer` admits
   evidence from any repository whose `ProfileObservation` is `Allowed` and
   tracks independent support separately. That disappearance is the expected
   eligibility semantic change of §16, never a regression, and no test may assert
   traceability equality across the two policies.
9. Repository Proof composition, the 15 signal thresholds and scores,
   `DataAvailabilityStatus`, `AnalysisStatus`, the promotion threshold and the
   representative-selection rule are all unchanged.
10. No repository-count cap, sampling, approximation, concurrency fan-out or
    arbitrary page cap is introduced, and no execution-guard numeric value is
    selected.

## 18. GraphQL Transport Optimization (Session 123)

**Status:** CONTRACT DECISION. This section closes the blocking gap Session 121
recorded as **GQL-F2**, and it is the gate every GraphQL implementation slice
must satisfy first. It authorizes no production code.

### 18.1 GraphQL is transport only

GraphQL may change **how** an already-required surface is fetched. It may never
change **what** is required, what it means, or what its absence means.

Locked rules:

- GraphQL introduces no evidence surface, removes none and re-scopes none. §4
  remains the required surface list; §5 remains the omission list.
- No signal, threshold, score, status, evidence key, evidence value, limitation,
  warning, category result, pattern, representative selection or report section
  may change because a surface arrived over GraphQL.
- A `DataAvailability` data key belongs to the **surface**, not to the transport.
  `commits` is `commits` whether it arrived over REST or GraphQL.
- A Profile result must be indistinguishable, in every output this contract
  governs, between a run served entirely by REST and a run served by the hybrid.
  Which transport ran is observable only through §18.9 telemetry.
- Repository Proof is untouched (§3.1). GraphQL is a Profile-path composition
  concern resolved at the existing `IProfileRepositorySnapshotSource` seam
  (§3.2), without modifying any §14.2 component.

### 18.2 Hybrid V1 surface split

| Surface | Hybrid V1 transport |
|---|---|
| profile metadata | **REST** |
| repository manifest | **REST** |
| repository metadata / topics | GraphQL candidate |
| default branch head, commit and tree identity | GraphQL candidate |
| languages | GraphQL candidate |
| commits | GraphQL candidate |
| pull requests | GraphQL candidate |
| releases | GraphQL candidate |
| tags | GraphQL candidate |
| issues | **REST** |
| recursive file tree | **REST** |
| README | **REST** |
| license and license file | **REST** |
| project files, workflows, documentation Markdown contents | **REST** |

Each REST retention is a verifiable reason, not a preference:

- **profile metadata** is the sole input of the §10.4 / §11.3 budget observation;
  moving it would silently change that premise;
- **the repository manifest** is the repository universe (§7) and the input of
  the `Empty` derivation (§18.3);
- **issues** stays REST because REST `/issues` interleaves pull requests and
  filters them client-side *after* the page is fetched, while a GraphQL `issues`
  connection never contains them. For the same repository under the same §15.3
  stop rule the two transports observe different prefixes and therefore produce
  different §15.5 `DecisionBounded` evidence values. Keeping issues on REST
  removes the only known evidence-value divergence in the candidate set, so the
  GraphQL column carries none;
- **the recursive tree** stays REST because GraphQL exposes no recursive tree and
  no `truncated` equivalent, and `truncated` drives `FileTree.IsComplete`;
- **README** stays REST because provider-side preferred-README resolution cannot
  be reproduced from a query; guessing paths would be new inference, not
  transport;
- **license** stays REST because GraphQL `licenseInfo` carries no file path or
  size, and the path is written into `EvidenceItem.Location` and into the merged
  file tree;
- **file contents** stay REST because blob truncation semantics differ from
  REST's documented rule, so byte-identical parity cannot be asserted.

**Ordering obligation.** Commits, pull requests, releases and tags are
`DecisionBounded` surfaces (§15): the observed *prefix* decides. A candidate may
be adopted only if its GraphQL ordering is proven to reach the same §15.3 stop
outcome and the same §15.5 evidence values as the REST ordering. Where ordering
parity cannot be proven, the surface stays REST. Appearing in the GraphQL column
is permission to attempt adoption under §19, never a finding that parity already
holds.

**Mapping obligation.** Adoption also requires proven field-mapping parity.
For every adopted GraphQL field the implementation must prove, by test, that it
maps onto exactly the value the REST path produces today. At minimum:
`committedDate` onto REST `commit.committer.date`; `closed` and `mergedAt` onto
`IsClosed` and `IsMerged`; a release `description` onto `Notes`; `isDraft` onto
`IsDraft`; `LanguageEdge.size` onto the language byte count; topic
normalization; and `defaultBranchRef` target and commit tree `oid` onto the
existing head-commit and root-tree identities. A surface whose mapping parity is
not proven stays REST.

Moving any surface into the GraphQL column later is a contract amendment, not an
implementation choice.

### 18.3 `isEmpty` is not adopted

`RepositoryConditionState Empty` continues to be derived exactly as it is today,
from the REST manifest's `size` and `pushed_at`. GitHub's GraphQL `isEmpty` is
**not** adopted in Hybrid V1.

It is not rejected for being inaccurate — it would be *more* accurate, which is
precisely the problem. Replacing the derivation would change admission outcomes
for the `Undetermined` case. That is an eligibility semantics change owned by
`public-profile-evidence-repository-eligibility.md`, reachable only through the
product/domain route §6 requires, and it is not a transport decision.

### 18.4 Partial data with `errors` — the blocking rule

GitHub GraphQL may answer **HTTP 200** with a `data` object **and** a non-empty
`errors` array. The existing availability mapping resolves failure from the HTTP
status alone, so without this section such a response reaches the domain as
success. That is the gap Session 121 refused to implement past.

> A GraphQL response carrying `errors` may never cause affected or missing data
> to be treated as successful absence, as `Missing` evidence, as zero evidence,
> or as any statement about the developer.

Consequences:

- HTTP 200 is not by itself success. A response is successful only when `errors`
  is absent or empty **and** every surface read from it is complete.
- An absent, null or empty field in a response carrying `errors` is
  **unresolved**, not observed-empty, unless it is deterministically proven
  unaffected under §18.5.
- "No commits", "no releases", "no tags", "no languages" are evidence claims.
  They may be made only from a response carrying no error touching that surface.
- Where both readings are available, the conservative one — fallback — is
  required. There is no optimistic read of an errored response.

### 18.5 Error isolation and per-surface fallback

Three cases, evaluated in order:

1. **Deterministically attributable error.** The error is attributable to exactly
   one GraphQL surface. Discard that surface's GraphQL result entirely and
   collect the surface through the existing REST path. The discarded payload is
   never merged, sampled, or used as a lower bound.
2. **Retaining unaffected surfaces.** A surface untouched by any error may be
   retained only if **both** hold: its payload is complete for that surface, and
   its independence from every reported error is deterministically established.
   Both must be provable from the response itself, never inferred from an error
   message's wording.
3. **Ambiguity.** If attribution or completeness is ambiguous for any surface,
   discard the **entire repository GraphQL envelope** and collect that repository
   through the existing REST path.

Ambiguity is the default, not the exception: an implementation that cannot prove
case 1 or case 2 is in case 3. Nothing here authorizes merging a GraphQL payload
with a REST payload for the same surface — **one surface is served by exactly one
transport, per repository, per run**.

### 18.6 Envelope-level and provider-wide failure

An envelope-level failure is one not attributable to any single surface:
transport failure, a non-200 status, an unparsable body, an authentication or
authorization failure, a GraphQL primary or secondary rate-limit response, or a
server-side timeout of the query.

Locked rules:

- On an envelope-level failure, GraphQL optimization is **disabled for the
  remainder of that Profile execution**, and collection continues through the
  proven REST path.
- The failing envelope must not be re-attempted for every repository. This exists
  so a systemic failure cannot be multiplied by the admitted repository count. It
  is not a retry policy, and no retry, backoff or circuit numeric is selected
  (§18.11).
- The disable is scoped to a single execution. It is not persisted, not shared
  between executions and not remembered.
- Because §6 requires both transports to produce identical results, disabling
  GraphQL mid-run cannot change any output this contract governs; it is visible
  only in §18.9 telemetry. An implementation that can produce different results
  before and after the switch violates §6 — the switch is not a licence to
  differ.
- A GraphQL rate-limit response is a provider fact about the **GraphQL resource**.
  It is neither GitHub Proof's internal §11.3 budget rejection nor a REST
  rate-limit outcome, and none of the three may impersonate another.

### 18.7 Availability ownership — no new status

- Falling back from GraphQL to REST is **not** an availability failure. It
  produces no `DataAvailability` entry of its own, no collection warning, no
  limitation and no `RepositoryAnalysis.Status` change.
- The final `DataAvailability` for a surface is the one produced by the
  collector/provider result that **actually supplied** it. A surface served by
  REST after a GraphQL fallback carries exactly the REST outcome it would have
  carried had GraphQL never been attempted.
- **No new `DataAvailabilityStatus` member.** No GraphQL-specific domain evidence
  status, signal status, warning code or limitation code.
- A discarded GraphQL attempt leaves no trace in evidence. It is a structural
  telemetry fact only (§18.9).

**`RetrievedAt` for a GraphQL envelope.** `DataAvailability.RetrievedAt` is
truthful retrieval metadata and is read as an observation instant by signal
evaluators, so its semantics are fixed here rather than left to the
implementation:

- one successful envelope captures **one** UTC retrieval instant, taken only
  after the response body has been fully read, parsed, validated and accepted;
- every surface actually supplied by that envelope carries that same
  `RetrievedAt`;
- a surface that falls back to REST carries the REST collector's own
  `RetrievedAt`, exactly as it does today.

A REST execution and a hybrid execution are separate observations, so their
wall-clock instants legitimately differ. That difference is **not** semantic
drift and must never be asserted equal in a parity test (§19.2). Status, reason
and data-key semantics must still match.

### 18.8 Both transports failing, and the guard relationship

- If GraphQL falls back and the REST collection then fails, the existing
  REST/provider availability semantics are authoritative: the surface reports its
  own typed outcome (`RateLimited`, `Forbidden`, `Failed`, ...) exactly as §11
  already requires. The earlier GraphQL attempt neither strengthens nor weakens
  it, and adds nothing to it.
- **Failure is never converted to `Missing`.** `UnavailableEvidence` is never
  automatically `Missing`, and adding a transport creates no exception.
- §11.1 caller cancellation, §11.2 execution-deadline expiration and §11.3
  proactive budget rejection remain governed by §§10-11, unchanged. None may be
  reinterpreted, caught, normalized or reported as a GraphQL error, and no
  GraphQL error may be reported as any of them.
- An `OperationCanceledException` crossing a GraphQL call continues to propagate
  under §11.1, and must not trigger the §18.6 run-scoped disable — it is not a
  provider failure.

### 18.9 Telemetry

Extends §9 and is bound by §9.1 without exception.

May be recorded — bounded structural facts only:

- GraphQL request count for the run,
- fallback count, at the granularity the implementation actually decides at
  (per surface, and per repository envelope),
- whether GraphQL was disabled for the run (§18.6), as a boolean structural fact,
- per-surface request attribution consistent with the existing bounded category
  set.

The §9 category set must stay **bounded, enumerable and closed**, and must never
be derived from repository, file or identity values. Extending it so that
GraphQL-served surfaces remain attributable is authorized here; inventing an
open-ended category is not.

Must never be recorded, in addition to every §9.1 prohibition:

- raw GraphQL query or mutation text,
- query variables carrying identities beyond what existing bounded policy already
  permits,
- any part of a GraphQL `data` payload,
- source file, README, documentation or dependency-manifest content,
- credentials or token material,
- raw GraphQL error messages, error extensions or error paths as free text.
  Error content may drive control flow (§18.5) without any of it reaching
  telemetry.

**Counting rule (locked).** One physical GraphQL HTTP POST is counted **exactly
once** in the total GitHub request count, and **exactly once** in a closed,
bounded GraphQL request category. A physical request is never counted once per
surface it served: doing so would make §9's "total GitHub HTTP request count"
untrue and would make the existing measured baseline incomparable.

Per-surface GraphQL served/fallback facts may exist as separate bounded
structural counters. They are **non-additive**, they are **not** HTTP request
counts, and they must never be summed with request counts or presented as them.

REST and GraphQL rate-limit observations address **different provider resources**
and must never be summed, averaged or presented as one budget. The §10.4 / §11.3
budget decision continues to read the REST profile-metadata response, which §18.2
keeps on REST precisely so that premise stays unchanged.

### 18.10 Preserved invariants

- **§6 semantic equivalence stands unchanged and is the acceptance bar.** For
  every profile input the hybrid composition must produce a semantically
  identical result to the current REST composition — exactly the per-repository
  and profile-level list §6 enumerates. GraphQL is granted no equivalent of the
  Session 098 scope-note exception; that exception belongs to §15 and §16 alone.
- **§7 no-hidden-cap stands unchanged.** No repository-count truncation, ranking,
  sampling or subset selection may be introduced under a transport argument, and
  a GraphQL page or batch bound may never become a hidden repository cap.
- §15 decision-sufficient collection and §15.5 evidence language continue to
  govern the bounded surfaces, on whichever transport serves them.
- §3.1 Repository Proof preservation and the §14.2 untouched-component list are
  unchanged.

### 18.11 What this section does not select

No concurrency increase or fan-out; no execution-deadline value; no rate-limit
reserve or floor value; no cache, persistence or replay; no queue; no repository
cap; no GraphQL batch size, page size or query shape; no retry or backoff policy;
no credential type, which stays open; and **no performance, latency or wall-clock
claim of any kind**. Session 121's figures are HTTP round-trip projections only,
and this section adopts none of them as an expectation or a target.

This exclusion covers the whole of §18, including §18.12 and §18.13 below.

### 18.12 Exact-total provider fields are not adopted

GraphQL connections expose `totalCount`, and equivalent exact-total fields exist
elsewhere in the schema. None of them is adopted.

Locked rules:

- GraphQL `totalCount`, and any equivalent exact-total field, must never populate
  `RepositoryCollection.TotalCount`.
- It must never enter a `DecisionBounded` evidence value, and must never be
  presented as an exact repository total in any evidence, report, warning or
  limitation.
- Existing enumeration and `DecisionBounded` semantics stay authoritative. §17.4
  (`TotalCount` is `null` for every `DecisionBounded` collection), §15.5
  lower-bound evidence language and §15.4's refusal of count-based optimization
  are unchanged, and none of them is relaxed by a cheaper provider count becoming
  available.

This is the same pattern §18.3 rejects for `isEmpty`: a more precise provider
field silently becoming an evidence-strength upgrade. Being cheaper to obtain is
not a reason to adopt it, and adopting it would be an evidence semantics change
rather than a transport change.

### 18.13 Empty-repository shape

REST distinguishes an empty repository from a failure by response body: HTTP 409
carrying exactly `"Git Repository is empty."` is successful emptiness, and every
other 409 is a failure. GraphQL has no 409 — an empty repository answers
`defaultBranchRef: null`, which collapses that distinction.

Locked rules:

- `defaultBranchRef == null` must never be read blindly as successful empty
  evidence.
- The default-branch head / commit-tree identity and the commits surface may be
  adopted over GraphQL only if the implementation **proves** that the GraphQL
  empty-repository shape reproduces the current REST successful-empty semantics
  exactly, **and** remains distinguishable from failure or error.
- If that equivalence cannot be proven, those surfaces stay on REST, or fall back
  to REST under §18.5.
- No new availability status, reason vocabulary or evidence status is introduced
  for this. The existing successful-empty and failure outcomes remain the only
  two, on either transport.

Note that §18.4 does not cover this case on its own: its trigger is the presence
of `errors`, and an empty-repository response carries none. §18.13 is what closes
the null-becomes-successful-absence path when no error is reported at all.

## 19. Session 123 Acceptance Criteria

Additional to §14 and §17, which remain unchanged. A first GraphQL implementation
slice is acceptable only if all of the following hold.

1. **§6 parity proven by test.** The same profile input through the REST
   composition and through the hybrid composition yields an identical
   `ProfileReport`, profile status, limitations, warnings, admitted repository
   set, contribution assessments, observed evidence, promoted patterns and
   representative selection; and per admitted repository an identical
   `RepositoryAnalysis.Status`, score breakdown, 15 `SignalResults`, evidence
   keys, limitation keys, warning codes and category results.
2. **Availability parity.** For every surface the `DataAvailability` key, status
   and semantics match the REST-only run whenever the observed provider facts
   match, and no new `DataAvailabilityStatus` member exists. `RetrievedAt` is
   deliberately excluded from that equality: a REST execution and a hybrid
   execution are separate observations whose wall-clock instants legitimately
   differ (§18.7). Parity tests assert key, status, reason and data-key
   semantics, never wall-clock equality; they do assert that one envelope's
   surfaces share one instant and that a fallen-back surface carries the REST
   collector's own.
3. **HTTP 200 with `errors` provably does not reach the domain as success.** A
   test feeds a 200 response carrying both `data` and `errors` and asserts that
   no surface becomes observed-empty, `Missing`, zero evidence or any positive
   claim.
4. **All three §18.5 cases are covered by test.** An attributable single-surface
   error falls back only that surface; an unaffected surface is retained only
   when independently complete; ambiguous attribution discards the whole
   repository envelope and collects that repository over REST. The ambiguous case
   is the default whenever the test cannot prove otherwise.
5. **An envelope-level failure disables GraphQL for the run.** A test in which
   the first repository fails systemically results in exactly one GraphQL
   envelope attempt for the whole execution, REST collection for every remaining
   repository, and an identical `ProfileReport`.
6. **Fallback leaves no evidence trace.** No availability entry, collection
   warning, limitation, status change or report difference is attributable to a
   fallback.
7. **Ordering and prefix parity per adopted `DecisionBounded` surface.** For
   commits, pull requests, releases and tags the GraphQL ordering reaches the
   same §15.3 stop outcome and the same §15.5 evidence values as REST, or the
   surface is not adopted in this slice.
8. **The REST-retained surfaces stay on REST**, proven by request-level
   assertion: profile metadata, repository manifest, issues, recursive tree,
   README, license and file contents.
9. **`isEmpty` is not read**, and the `Empty` derivation is unchanged.
10. **Guard separation holds.** Caller cancellation, deadline expiration and
    budget rejection are not reinterpreted as GraphQL errors and do not trigger
    the §18.6 disable; no GraphQL error is reported as any of them.
11. **Telemetry tests** prove the §18.9 prohibitions for at least query text,
    response data, raw error messages and tokens; that the category set stays
    closed and bounded; that one physical GraphQL POST is counted exactly once in
    the total request count and exactly once in a closed bounded GraphQL
    category, never once per surface served; that per-surface served/fallback
    counters are never summed into or presented as HTTP request counts; and that
    REST and GraphQL rate-limit observations are never summed.
12. **Repository Proof is unchanged** — composition, request graph and tests
    untouched and green.
13. **No §18.11 value is selected**, no repository cap, sampling or `Task.WhenAll`
    fan-out is introduced, and the full solution builds and tests green with 0
    warnings.
14. **Rollout is gated on live parity evidence.** The REST composition remains
    the reference and fallback path, and the hybrid is switchable off by
    configuration to exactly that composition, for as long as the hybrid exists.
    Before **any** production switch: the approved live A/B parity validation is
    run on the same three profile shapes Session 120 measured; REST and hybrid
    outputs are compared under the canonical parity contract of criterion 1; and
    the production switch requires architecture-owner approval on that evidence.
    No performance target, threshold or claim is attached to this gate, and none
    may be introduced by it.
15. **Exact-total fields are gated.** A test proves that GraphQL `totalCount`,
    and any equivalent exact-total field, never reaches
    `RepositoryCollection.TotalCount`, never enters a `DecisionBounded` evidence
    value and is never presented as an exact repository total; `TotalCount`
    remains `null` for every `DecisionBounded` collection (§17.4, §18.12).
16. **Empty-repository equivalence is proven in both directions.** A positive
    test proves the GraphQL empty-repository shape produces exactly the current
    REST successful-empty result; a negative test proves a failure or error
    response is **not** read as empty. If either cannot be proven, the affected
    surfaces stay on REST. No availability, reason or status vocabulary is added
    (§18.13).
17. **Field-mapping parity is proven per adopted field**, explicitly rather than
    incidentally: at minimum `committedDate`, `closed` and `mergedAt`, a release
    `description` onto `Notes`, `isDraft`, `LanguageEdge.size`, topic
    normalization, and `defaultBranchRef` target and commit tree `oid`. A surface
    whose mapping parity is not proven stays REST (§18.2).
