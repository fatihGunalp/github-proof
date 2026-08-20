# Public Profile V1 — Runtime Collection Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Scope:** Profile Proof repository-evidence collection composition, runtime
telemetry and execution guard semantics
**Session:** 096
**Date:** 2026-08-20
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
- the future re-activation rule for the omitted enrichment surfaces.

It owns no signal, no scoring rule, no eligibility rule, no pattern promotion
rule, no representative-selection rule and no report presentation rule. Every
one of those stays with its existing canonical owner.

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

A Repository Proof run keeps collecting `SourceFiles` and
`DependencyDeclarations` exactly as it does today, on exactly the bounds their
own contracts define.

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
- contributors,
- topics.

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
  (`repository-dependency-evidence.md`).

The omission list is exactly those two surfaces. It is closed.

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

Neither omitted surface is an obligation under any of those three contracts, and
their only documented downstream consumer is the Unsupported ProblemArea chain.
The omission is therefore not a completeness deficit and must never be accounted
as one.

### 5.4 Non-extension rule

Do not extend the omission list without explicit proof. In particular the
following are **not** covered and are **not** safe to omit: commits, issues,
pull requests, releases, tags, README, languages, file tree, project files,
workflows, repository metadata, topics, license, documentation Markdown,
contributors. Each feeds existing signal or profile semantics unless separately
proven otherwise, by the same standard of proof used in §5.1.

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

## 14. Implementation Acceptance Criteria

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
