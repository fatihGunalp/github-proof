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

## 11. Failure and Cancellation Behavior

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

This is the §10 guard: an internal operational deadline owned outside domain
evidence semantics, and it does not exist yet.

Locked rules:

- Guard expiration is distinct from §11.1 caller cancellation, and the two must
  never be conflated in either direction.
- Guard expiration may never yield `Complete`, and may never present the
  repositories completed so far as a whole profile.
- The outer runtime/Web boundary must expose a typed non-success outcome using
  the existing status/availability vocabulary, where that vocabulary can
  represent the outcome truthfully.
- No new domain evidence status may be invented for it.
- If the existing runtime result types cannot truthfully represent internal guard
  expiration without semantic abuse, the implementation session must stop and
  report a typed-boundary gap before implementing the guard (§10 principle 3).
- Guard expiration is operational truth about one execution, never developer
  evidence.

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
