# GitHub Proof — Current Development State

**Last Updated:** 2026-08-18
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
REMOTE VERIFIED / INTEGRATED INTO MAIN at
72932bb5de4ec37850850ebaea2c2e75f61e406a

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
P7 — Representative Repository and Role Evidence Alignment — STARTED

Session 074 locked the Representative Repository selection contract
(documentation only). RepresentativeRepository/RepresentativePatternSupport
production code has NOT started. RoleEvidenceAlignment semantics have NOT
started and remain a separate later P7 decision.

## Production Delivery

Milestone A:
NOT_STARTED

Milestone B:
BLOCKED

## Active Session

Session:
074 — Representative Repository Decision + Contract

Session File:
worklog/sessions/2026-08-18-074-representative-repository-decision-contract.md

Status:
DOCS_CONTRACT PASS — docs/architecture/public-profile-representative-repositories.md
locked (Contract Version 1); independent review complete; nothing committed or
pushed

## Git

Active Branch:
session/074-representative-repository-decision-contract

Session Base:
72932bb5de4ec37850850ebaea2c2e75f61e406a

Last Remote Verified Main:
72932bb5de4ec37850850ebaea2c2e75f61e406a — includes Session 073

Session 073 integration:
Session 073 (Dependency Source Reference Detector Implementation) is
committed, pushed, integrated into main and remotely verified at
72932bb5de4ec37850850ebaea2c2e75f61e406a. Detail lives in
worklog/sessions/2026-08-18-073-dependency-source-reference-detector.md.

Note:
closed session branches are retained. Deleting any of them is a user-owned Git
side effect and is not pending work. session/074 must not be recommitted,
repushed or reintegrated by a later session — it is Session 074's own active
branch.

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

Representative Repository Selection Contract V1 (Session 074):
docs/architecture/public-profile-representative-repositories.md, CONTRACT
LOCKED version 1. Selection unit is exactly one representative repository per
RepeatedProfilePattern (global bounded set and minimum/maximum-coverage set
are out of V1 scope). Candidates = pattern.IndependentSupportingRepositories
as already produced by P5/P6 (fork/template-derived cannot become candidates;
Undetermined independence cannot be optimistically promoted; archived is not
automatically excluded). Comparison rule is PatternSupportObservationCount
(count of the pattern's SupportingObservations with a positive
ProfileEvidenceReference for that repository), scoped to one pattern only and
never combined across patterns. Tie-break reuses ProfileRepositoryReference
ordering (Owner, then Name). Cross-pattern breadth is explicitly forbidden as
a selection factor; the same repository may still independently win multiple
patterns. Dedicated RepresentativeRepositoryRuleVersion, distinct from
PatternRuleVersion/ObservationRuleVersion/EligibilityPolicyVersion. No rank,
score, quality, strength, confidence, seniority or employability semantics.
RoleEvidenceAlignment explicitly out of scope. No production code implements
this contract yet.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 073 (last code change):
1046/1046 PASS (Domain 622, Infrastructure 255, Application 101, Web 68)
Build: 0 warning / 0 error

Session 074 is documentation-only; no build/test run required or performed.

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
1 — established by Session 074 (contract only; no runtime value yet, no
production code)

## Next Action

Session 074 locked the Representative Repository selection contract
(documentation only, no production code). Independent review is complete; see
the session file for findings and disposition.

Exact next action:
decide the Session 074 Git transition (commit/push are user-owned). After
that, Session 075 may begin production implementation of
RepresentativeRepository/RepresentativePatternSupport per the locked contract,
or continue toward the separate Role Evidence Alignment decision — neither is
started.

Known remaining work, none of it started and none of it in this session's
scope: no production RepresentativeRepository/RepresentativePatternSupport
code; RoleEvidenceAlignment taxonomy and rules; ProducedWorkEvidence remains
unresolved; RepositoryProblemAreaEvidence remains blocked by the Session 067
contract.

Nothing committed or pushed.
