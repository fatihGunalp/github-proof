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
071 — Bounded Source Evidence Collection

Session File:
worklog/sessions/2026-08-18-071-bounded-source-evidence-collection.md

Status:
PASS / READY_FOR_COMMIT

## Git

Active Branch:
session/071-bounded-source-evidence-collection

Session Base:
2a1809d128de43b7c07709a8d3377afb37168ea9

Last Remote Verified Main:
2a1809d128de43b7c07709a8d3377afb37168ea9

## Goal

Decide the smallest safe and useful raw source collection contract boundary that
a future canonical contract should lock, so that
DependencySourceReferenceEvidence becomes producible.

Steps 1, 2 and 2B produced the decision report and the bounded measurement
evidence. Step 3 locked the canonical contract, Step 4 reviewed and corrected it,
and Step 5 implemented the bounded collector, the RepositorySourceFile snapshot
surface and the source-files availability key. No reference detector exists.

## Canonical Evidence Ladder

DependencyDeclaration
!= DependencySemanticCategoryEvidence
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

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 071:
967/967 PASS
Build: 0 warning / 0 error
Independent review: PASS WITH FINDINGS, no open BLOCKER/HIGH

AnalyzerVersion:
1.1.0 — additive minor bump for the bounded raw source collection surface

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

## Next Action

Sessions 069 and 070 are closed, integrated and remote verified. Their canonical
owners are docs/product/product-positioning.md and
docs/product/product-experience-principles.md. P10 is not brought forward, the
technical roadmap is unchanged, and development has returned to the pre-P7
evidence pipeline position.

Session 068 remains the semantics-only owner of
docs/architecture/repository-dependency-source-reference-evidence.md. Its
separate future approvals still pending: bounded source-evidence collection
contract, evidence payload and replay decision, reference-detection rule
contract and its version owner, and the ProducedWorkEvidence resolution.

Session 071 Steps 1, 2 and 2B delivered the decision report and the bounded
measurement evidence. Step 2B returned READY_FOR_CONTRACT after a targeted
supplemental measurement discriminated the open decisions.

Step 3 created docs/architecture/repository-source-evidence-collection.md,
CONTRACT LOCKED version 1. It owns bounded raw source collection semantics: the
supported V1 source surface, normalization, availability and completeness,
isolation, version ownership and the resource and security boundary. It owns no
detection rule, no dependency-to-source association, no ProducedWorkEvidence, no
RepositoryProblemAreaEvidence, no ProblemArea taxonomy, no scoring and no report
presentation.

Locked V1: regular blobs only; one global 24-extension allow-list; exact
build.gradle.kts and settings.gradle.kts filename exclusions without excluding
the .kts extension; 23 structural path exclusions declared as collection budget
policy; 32 files, 64 KiB per file, 384 KiB corpus; deterministic extension
round-robin ordering; RepositorySourceFile carrying Path and Content only;
strict UTF-8 with one leading BOM stripped as a versioned rule; a dedicated
source-files availability key; full isolation from FileTree, CollectionWarnings,
Status, the 15 signals and the report; AnalyzerVersion ownership with an
additive minor bump class.

RepositorySourceFile is a raw collection input and is explicitly not
DependencySourceReferenceEvidence. The contract completes no downstream ladder
stage; ProducedWorkEvidence remains unresolved and RepositoryProblemAreaEvidence
remains blocked by the Session 067 contract.

docs/architecture/repository-dependency-source-reference-evidence.md was aligned
only where the prerequisite state changed: collection semantics now exist,
production collection does not. Session 068's epistemic ceiling is unchanged.

Still unresolved: the persisted evidence payload and self-contained replay; the
qualifying-reference definition and per-ecosystem detection design; the detection
version owner name; the exact future AnalyzerVersion value; and the
ProducedWorkEvidence resolution.

Step 3 itself made no src, tests, report, UI or roadmap mutation.

Step 4 independent review returned PASS_WITH_FINDINGS: two HIGH determinism
gaps (round-robin skip semantics undefined; byte bounds not bound to a byte
count), three MEDIUM (availability versus provider failure, a wrong
settings.gradle.kts rationale, stale session worklog) and three LOW findings.

Step 4.1 corrected all eight. The contract now locks a single total candidate
order materialized before acceptance with consumed skips, canonical per-file and
corpus accounting on normalized Content UTF-8 bytes after BOM strip, provider
failure keeping its real availability status while preserving collected
artefacts, a per-filename rationale for the two excluded Gradle filenames,
declaration-independent collection, and a dedicated tree-metadata requirement
that forbids enriching FileTree. No approved decision was reopened.

Step 4.2 and Step 4.3 closed the remaining slot-semantics ambiguity:
MaximumSourceFiles is the maximum number of source-content retrieval attempts per
repository, pre-fetch rejection consumes no slot, a begun retrieval consumes its
slot permanently even when it later fails, the artefact count is 0..32, and the
stop condition counts consumed slots rather than successful artefacts.

Step 4.4 returned PASS and declared the contract implementation-ready.

Step 5 implemented it. Domain gained RepositorySourceFile(Path, Content), the
additive RepositorySnapshot.SourceFiles surface and the source-files data key.
Infrastructure gained GitHubRepositorySourceFileCollector, which obtains its own
pinned tree metadata because the regular-blob rule needs Git mode that FileTree
does not carry, applies the eligibility pipeline, builds the round-robin total
candidate order, and enforces the retrieval-slot and normalized-byte bounds. The
orchestrator contributes availability only. AnalyzerVersion moved 1.0.0 to 1.1.0.

Validation: build 0 warning / 0 error, 967/967 tests pass, git diff --check
clean. No scoring signal, category result, report element or UI changed, and no
source failure reaches CollectionWarnings or FileTree.

The independent implementation review returned PASS WITH FINDINGS with no
BLOCKER and no HIGH. Both MEDIUM findings were handled: the stale session worklog
text was corrected, and the contract §17 unbounded-allocation gap was recorded in
Known Risks and handed to a separate bounded task because it affects every
existing collector equally and §17 leaves the implementation strategy open.

Session 071 is PASS / READY_FOR_COMMIT. The mutation set is staged but not
committed, not integrated into main and not remote verified.

Exact next action:
run the user-owned commit and push, then record remote verification and set the
Active Session accordingly.

Nothing committed or pushed.
