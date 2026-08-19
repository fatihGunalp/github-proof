# GitHub Proof — Current Development State

**Last Updated:** 2026-08-19
**Source of Truth:** docs/

## Development

Completed:
P6 — Technical, Practice and Problem Patterns — COMPLETE / REMOTE VERIFIED

Completed Foundation:
Session 066 — Repository Evidence Surface Expansion — COMPLETE / REMOTE VERIFIED
Session 067 — Dependency Semantic Category Contract + Registry — COMPLETE /
REMOTE VERIFIED
Session 068 — DependencySourceReferenceEvidence Contract — COMPLETE /
REMOTE VERIFIED
Session 071 — Bounded Source Evidence Collection — COMPLETE / REMOTE VERIFIED /
INTEGRATED INTO MAIN
Session 072 — Dependency Source Reference Detection Contract — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN
Session 073 — Dependency Source Reference Detector Implementation — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN
Session 074 — Representative Repository Decision + Contract — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN at
f32c5de7a6e21e7eedc786deb973b6aec159dcec (contract-only; five post-push
independent-review rounds, all closed; detail in the Session 074 file)

Current:
Stage 7 — Public Profile Evidence Foundation

Completed Product Work:
Session 069 — Product Positioning Alignment — COMPLETE / REMOTE VERIFIED
Canonical owner: docs/product/product-positioning.md

Session 070 — Jobs-Style Product Packaging — COMPLETE / REMOTE VERIFIED
Canonical owner: docs/product/product-experience-principles.md

Incomplete:
ProblemArea — taxonomy, promotion and repository evidence all remain
unimplemented and deferred

Next Profile Phase:
P7 — Representative Repository and Role Evidence Alignment — IN PROGRESS

Session 075 implemented the locked Representative Repository contract in
Domain (RepresentativeRepositorySelection, RepresentativeRepository,
RepresentativePatternSupport, RepresentativeRepositorySelectionPolicy,
RepresentativeRepositoryRuleVersion, with RepresentativeRepositoryRuleVersion.V1
= "1.0.0" as the first canonical runtime rule version) with 54 new Domain
tests, closed across four correction/hardening turns and a final closure
review. No Application orchestration, report/UI or P8 composer consumes it
yet. RoleEvidenceAlignment semantics have NOT started and remain a separate
later P7 decision. Git transition for Session 075 is user-owned and must be
read from actual Git state.

## Production Delivery

Milestone A:
NOT_STARTED

Milestone B:
BLOCKED

## Active Session

Session:
NONE

Last Completed Session:
075 — Representative Repository Implementation (P7 Domain implementation)

Session File:
worklog/sessions/2026-08-19-075-representative-repository-implementation.md

Status:
IMPLEMENTATION PASS. Final closure review of the complete working set
returned PASS WITH FINDINGS (1 MEDIUM — the worklog session file itself had
gone stale after three correction/hardening turns; no production defect;
resolved by finalizing that file). 676/676 Domain tests pass (622 baseline +
54 new); full solution build (Domain, Infrastructure, Application, Web) 0
warning / 0 error; Infrastructure 255/255, Application 101/101, Web 68/68
unchanged from baseline (this session touched no code outside
GitHubProof.Domain/GitHubProof.Domain.Tests). Git transition is user-owned
and must be read from actual Git state.

## Git

Active Branch:
session/075-representative-repository-implementation

Session Base:
f32c5de7a6e21e7eedc786deb973b6aec159dcec (main; Session 074 already
integrated)

Note:
closed session branches are retained; deleting any of them is a user-owned
Git side effect and is not pending work. Git transition (commit/push) for
Session 075 is user-owned and must be read from actual Git state
(`git status`, `git log`), not assumed from this snapshot.

## Canonical Evidence Ladder

DependencyDeclaration
!= DependencySemanticCategoryEvidence
!= RepositorySourceFile
!= DependencySourceReferenceEvidence
!= ProducedWorkEvidence
!= RepositoryProblemAreaEvidence
!= RepeatedProfilePattern(ProblemArea)
!= DeveloperExperienceOrCapability
!= RepresentativeRepository

A declaration is never a source reference. A category observation is never
produced-work evidence. A source reference is never produced-work evidence. A
representative-repository selection is never developer capability, quality or
ranking evidence.

## Locked Foundation

Repository MVP V1 still owns exactly 15 scoring signals.

Repository Dependency Evidence Contract V1: non-scoring declaration surface,
introduced at AnalyzerVersion 1.0.0.

Repository Dependency Semantic Category Contract V1: five locked categories
(database-access, web-application-framework, http-client, authentication,
machine-learning), http-server deferred,
DependencyCategoryMappingVersion 1.0.0, immutable 46-entry registry,
RepositorySemanticEvidence as a non-scoring Domain result.

Repository Source Evidence Collection Contract V1: bounded raw source collection,
regular blobs only, 24-extension allow-list, 23 structural path exclusions, exact
build.gradle.kts and settings.gradle.kts filename exclusions, 32 retrieval
attempts / 64 KiB per file / 384 KiB corpus, deterministic extension round-robin,
RepositorySourceFile(Path, Content), dedicated source-files availability,
implemented at AnalyzerVersion 1.1.0.

Repository Dependency Source Reference Detection Contract V1:
docs/architecture/repository-dependency-source-reference-detection.md,
CONTRACT LOCKED version 1. Import-declaration detection only; V1 surface is
Go + JavaScript/TypeScript + Rust + Python + DotNet, with Java, Php and Ruby
explicitly outside; mandatory syntax-aware comment and string exclusion with a
desynchronization rule; strict single-literal require()/import(); identity
resolution by rule with the Go longest-module-path and DotNet exact-equality
tie-breakers; no identity registry in V1; ambiguity yields no observation;
positive-only evidence; Domain-owned policy; version owner
SourceReferenceRuleVersion; no AnalyzerVersion change required. Implemented in
Domain as of Session 073: RepositorySourceReferenceEvidencePolicy composes
RepositorySourceReferenceEvidence from RepositorySnapshot.DependencyDeclarations
+ RepositorySnapshot.SourceFiles. SourceReferenceRuleVersion first runtime
value 1.0.0. Derived and non-scoring; not written into RepositorySnapshot; no
orchestrator, report or UI consumes it yet.

Representative Repository Selection Contract V1 (Session 074, contract
locked and closed across five independent-review rounds — see
docs/architecture/public-profile-representative-repositories.md and
worklog/sessions/2026-08-18-074-...md for the full record): one
representative-repository selection per RepeatedProfilePattern, materializing
as exactly one RepresentativePatternSupport; grouped, after selection, into
zero-or-more RepresentativeRepository objects when the same repository wins
several patterns (§3.4) — grouping never influences a winner. Candidates =
IndependentSupportingRepositories only; comparison by
PatternSupportObservationCount (higher wins, scoped to one pattern, never
combined across patterns — no cross-pattern breadth advantage); tie-break
Owner→Name ordinal with a canonical-identity precondition that rejects a
non-canonical Url collision deterministically (§6.1). Two distinct traces:
the Canonical Pattern Trace (`Pattern`, an in-memory reference to the exact
`RepeatedProfilePattern` instance — identity/context only, never
representative-specific evidence, not even for the selected repository) and
the Representative Support Evidence Trace (scoped `ObservationKey`/
`ProfileEvidenceReference` values, ordered by `EvidenceKey` ordinal — the
sole source explaining a selection). `PatternRuleVersion`/
`ObservationRuleVersion`/`EligibilityPolicyVersion`/`IsEvidenceSurfaceComplete`
are copied from the source `ProfilePatternSet`; `RepresentativeRepositoryRuleVersion`
is dedicated. No persisted `ProfilePatternSet` identity; no
rank/score/quality/strength/confidence/capability/seniority/employability;
`RoleEvidenceAlignment` out of scope.

Implemented in Domain as of Session 075 (worklog/sessions/2026-08-19-075-...md):
`RepresentativeRepositorySelectionPolicy.Select(ProfilePatternSet)` returns a
`RepresentativeRepositorySelection`. Candidate evidence (repository, scoped
`ProfileEvidenceReference` list, `PatternSupportObservationCount`) is computed
once per candidate during selection; the winner's already-computed evidence
is carried directly into `RepresentativePatternSupport`, which never reads
`Pattern.SupportingObservations` to derive or re-derive evidence.
`RepresentativePatternSupport` also rejects at construction any repository
outside `IndependentSupportingRepositories`, and
`RepresentativeRepositorySelection` rejects at construction: a pattern
represented under two different repositories, a referenced pattern whose
`PatternRuleVersion`/`ObservationRuleVersion` does not match the selection's
own, and a post-grouping Owner+Name/Url canonical-identity collision.
`SupportingObservationKeys` is a get-only computed property re-derived from
`SupportingEvidenceReferences` on every access — never a stored field, so it
cannot be mutated into divergence. `RepresentativeRepositoryRuleVersion.V1 =
"1.0.0"` is the first canonical Domain runtime rule version. 54 new Domain
tests cover the full locked invariant set (empty-set validity, grouping
cardinality, candidate restriction, comparison/tie-break, the
canonical-identity rejection boundary at both the pre-selection and
post-grouping level, the two-trace separation, all six §13 ordering rules,
version/completeness preservation, cross-pattern independence, the
SupportingObservationKeys projection, V1, a reflection guard against
prohibited fields). Final closure review: PASS WITH FINDINGS (1 MEDIUM,
worklog-only, resolved). No `RepresentativeRepositoryRuleVersion` runtime
value has been wired into any running composition yet — using `V1` in an
actual Application flow is a later Application/composition-root decision,
not made by this Domain-only session.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 075 (last code change):
1100/1100 PASS across the full solution (Domain 676 [622 baseline + 54 new],
Infrastructure 255, Application 101, Web 68 — the latter three unchanged
from baseline)
Full solution build (GitHubProof.slnx): 0 warning / 0 error

AnalyzerVersion:
1.1.0

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

SourceReferenceRuleVersion:
1.0.0 — established by Session 073

RepresentativeRepositoryRuleVersion:
1.0.0 (V1) — established by Session 075 as the first canonical Domain
runtime rule version. Not yet wired into any running Application
composition.

## Next Action

Session 075 implemented the locked Representative Repository contract in
Domain with a full test suite, closed across four correction/hardening turns
and a final closure review (PASS WITH FINDINGS, 1 MEDIUM worklog-only
finding, resolved). See the session file for the complete record.

Session implementation work is complete. Git branch state, commit history
and any main integration are user-owned and must be read from actual Git
state (`git log`, `git status`).

Exact next action:
a later session may wire `RepresentativeRepositorySelectionPolicy` into
Application orchestration once that layer is ready to consume a
`ProfilePatternSet`, using `RepresentativeRepositoryRuleVersion.V1` as the
production rule version, or continue toward the separate Role Evidence
Alignment decision — neither is started.

Known remaining work, none of it started and none of it in this session's
scope: Application orchestration integration for the new policy; report/UI
consumption; P8 deterministic profile composer; RoleEvidenceAlignment
taxonomy and rules; ProducedWorkEvidence remains unresolved;
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.
