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
072 — Dependency Source Reference Detection Contract

Session File:
worklog/sessions/2026-08-18-072-dependency-source-reference-detection-contract.md

Status:
DECISION REPORT DELIVERED — verdict NEEDS_MORE_MEASUREMENT, contract NOT locked

## Git

Active Branch:
session/072-dependency-source-reference-detection-contract

Session Base:
d480a467ec90114beea58f80e5722aa4db745c5d

Last Remote Verified Main:
d480a467ec90114beea58f80e5722aa4db745c5d — includes Session 071

Note:
session/071-bounded-source-evidence-collection still exists locally and on
origin after integration; deleting it is a user-owned Git side effect.

## Goal

Decide whether reference detection can be defined safely and deterministically,
so that DependencySourceReferenceEvidence becomes producible from
DependencyDeclaration[] + RepositorySourceFile[].

Feasibility audit and decision report only. No detector, no evidence type, no
registry artefact, no measurement.

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

No reference-detection contract exists. No detector, no
DependencySourceReferenceEvidence type and no detection version owner exists in
src/.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

Reference detection is measurement-blocked, not contract-blocked.

## Validation Baseline

Session 071:
967/967 PASS
Build: 0 warning / 0 error

Session 072:
no src/ or tests/ mutation, so no build or test run applies

AnalyzerVersion:
1.1.0

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

## Next Action

Session 072 delivered the REFERENCE DETECTION DECISION REPORT with verdict
NEEDS_MORE_MEASUREMENT. Fourteen decisions are settled and recorded as R1-R14 in
the session file so the next session does not re-derive them; they are worklog
state, not a canonical lock, and must be re-approved inside the canonical
contract when it is written. Five are measurement-blocked and recorded as M1-M5.

Resolved, in short: detection consumes DependencyDeclaration[] +
RepositorySourceFile[] + the pinned commit only; source-file ecosystem is a
total versioned map over the 24-extension allow-list; package identity is not
source identity, with Go, JavaScript and Rust total, Python, DotNet and Ruby
partial, and Java and Php structurally non-derivable; V1 is import-declaration
detection, not usage detection; require() and import() count only with a literal
argument; comment and string-literal matches never count; unresolved identity
yields no observation; a separate versioned identity registry is required and
must not be folded into dependency-category-mapping; the detector lives in
Domain with Infrastructure loading the registry artefact only; a separate
detection version owner is required, with SourceReferenceRuleVersion
recommended.

Measurement-blocked: the comment and string-literal exclusion false-positive
floor; the reference yield of the bounded 32-file / 384 KiB corpus; identity
registry sizing and viability; whether the Go and JavaScript total derivations
survive vendoring, replace directives, TypeScript path aliases and monorepo
workspaces; and, dependent on all of those, the V1 ecosystem surface.

docs/architecture/repository-dependency-source-reference-evidence.md §5 was
synchronized: source collection is implemented, and the remaining prerequisite
is the reference-detection rule contract of §4, not collection.

Exact next action:
run the M1-M4 measurement round as a temporary, read-only, pinned-commit
measurement using the Session 071 Step 2 method, leaving no measurement artefact
in the repository. Then re-run the decision report and lock the contract only if
M5 becomes single-valued.

Nothing committed or pushed.
