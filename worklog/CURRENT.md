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
P7 — Representative Repository Selection — NOT STARTED

## Production Delivery

Milestone A:
NOT_STARTED

Milestone B:
BLOCKED

## Active Session

Session:
073 — Dependency Source Reference Detector Implementation

Session File:
worklog/sessions/2026-08-18-073-dependency-source-reference-detector.md

Status:
IMPLEMENTED — detector, models and tests complete; independent review PASS after
two HIGH corrections; nothing committed or pushed

## Git

Active Branch:
session/073-dependency-source-reference-detector

Session Base:
275b6c5590f4e490dea552173af0f373295d14f2

Last Remote Verified Main:
275b6c5590f4e490dea552173af0f373295d14f2 — includes Session 072

Session 072 integration:
77d05f8, 942090a and beb7f92 are all in main. The fast-forward is complete and
remote verified.

Note:
closed session branches are retained. On origin, every branch from session/059
through session/072 still exists at its closing commit; locally, session/062
through session/072 exist and 059-061 are origin-only. Deleting any of them is a
user-owned Git side effect and is not pending work.

## Canonical Evidence Ladder

DependencyDeclaration
!= DependencySemanticCategoryEvidence
!= RepositorySourceFile
!= DependencySourceReferenceEvidence
!= ProducedWorkEvidence
!= RepositoryProblemAreaEvidence
!= RepeatedProfilePattern(ProblemArea)
!= DeveloperExperienceOrCapability

A declaration is never a source reference. A category observation is never
produced-work evidence. A source reference is never produced-work evidence.

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
SourceReferenceRuleVersion; no AnalyzerVersion change required.

Detection is contract-locked and, as of Session 073, implemented in Domain:
RepositorySourceReferenceEvidencePolicy composes RepositorySourceReferenceEvidence
from RepositorySnapshot.DependencyDeclarations + RepositorySnapshot.SourceFiles.
SourceReferenceRuleVersion first runtime value 1.0.0. The result is derived and
non-scoring; it is not written into RepositorySnapshot and no orchestrator,
report or UI consumes it yet.

Contract amendment, Session 073: Go `// indirect` was removed from the §10
classes the detector must identify, because the declaration surface preserves no
indirect marker. New §10.1 treats such a requirement as an ordinary Go
declaration. The document stays Contract Version 1.

The Session 072 measurement evidence behind that contract, its READY verdict and
its R1-R16 re-evaluation live in the session file, not here.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 073:
1046/1046 PASS (Domain 622, Infrastructure 255, Application 101, Web 68)
Build: 0 warning / 0 error
Baseline before Session 073 was 967; 79 tests are new.

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

## Next Action

Session 073 implemented the locked detection contract as Domain policy, models
and tests. Verdict after correction: independent review PASS.

Step 0 corrected a real contract defect before any implementation: contract §10
required the detector to identify Go `// indirect` requirements, but
RepositoryDependencyDeclaration carries no such marker and the Go manifest parser
strips the comment before a declaration is built. The class was also not
structurally unreferenceable. §10 and the new §10.1 record the correction inside
Contract Version 1.

The first implementation review returned FAILED with two HIGH findings, both
reproduced and both corrected: a commented-out entry inside a Go `import` block
produced an observation, and an unterminated single-quoted Python literal
silently violated §6.1. The lexical scan now distinguishes code, comment and
string zones, and a Python single-quoted literal ends at the newline exactly as
in the C-like scanner. Four regression tests were added and the narrow re-review
returned PASS.

Exact next action:
decide the Session 073 Git transition. Committing the branch, pushing it and any
main integration are separate explicit user-owned Git side effects.

Known remaining work, none of it started and none of it in this session's scope:
no orchestrator, report or UI consumes RepositorySourceReferenceEvidence;
ProducedWorkEvidence remains unresolved; RepositoryProblemAreaEvidence remains
blocked by the Session 067 contract; P7 is NOT STARTED.

Nothing committed or pushed.
