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
PASS / PUSHED — REMOTE VERIFIED ON SESSION BRANCH, NOT INTEGRATED INTO MAIN

## Git

Active Branch:
session/072-dependency-source-reference-detection-contract

Session Base:
d480a467ec90114beea58f80e5722aa4db745c5d

Session Branch Head:
942090a0142820557f93a1808ec2694747d6eb80 — remote verified

Session Branch Commits:
77d05f8 feasibility audit, 942090a detection contract lock

Last Remote Verified Main:
d480a467ec90114beea58f80e5722aa4db745c5d — includes Session 071, unchanged;
Session 072 is not merged

Note:
session/071-bounded-source-evidence-collection still exists locally and on
origin after integration; deleting it is a user-owned Git side effect.

## Goal

Decide whether reference detection can be defined safely and deterministically,
so that DependencySourceReferenceEvidence becomes producible from
DependencyDeclaration[] + RepositorySourceFile[].

The pre-measurement round returned NEEDS_MORE_MEASUREMENT. The M1-M4 measurement
round resolved every blocked decision, M5 became single-valued, and the canonical
contract was locked. No detector was implemented.

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

Detection is contract-locked but NOT implemented: no detector, no
DependencySourceReferenceEvidence type and no SourceReferenceRuleVersion exists
in src/.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 071:
967/967 PASS
Build: 0 warning / 0 error

Session 072:
no src/ or tests/ mutation, so no build or test run applies

AnalyzerVersion:
1.1.0 — unchanged by the detection contract

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

SourceReferenceRuleVersion:
contract-defined, first numeric value owned by the implementing session

## Next Action

Session 072 completed its measurement round. Verdict: READY.

Measurement, TEMPORARY and read-only, following the Session 071 Step 2 method:
28 pinned repositories — the 10 canonical validation repositories plus 18
temporary supplemental ones — 6 643 supported eligible source files, 1 383
source blobs and 194 manifest blobs, 0 retrieval failures, 0 truncated trees. No
measurement artefact was left in the repository and
samples/validation/repositories.json was not modified.

M1: a naive line-level detector produces a 1.54% false-positive rate over the
4 358 import-shaped matches of the V1 surface, and 1.30% over 6 974 matches
across all eight, reaching 28.24% in Ruby. Syntax-aware comment and string
exclusion is mandatory and sufficient: one desynchronization signature across
1 370 files, in a .groovy artefact, and zero column-0 imports wrongly masked.
The single residual false positive came from a permissive require()/import()
literal rule; the strict single-literal rule removed it and lost nothing.

M2: 17.5% of the V1 surface's declarations acquire a qualifying reference inside
the bounded corpus and 26.4% with one extra deterministic window; across all
eight ecosystems 19.1% and 27.9%. Large repositories
expose as little as 2% of their eligible source surface, so incompleteness is
the normal state, not a defect.

M3: over the canonical 46-entry population a registry would need 8 entries, but
over the observed population Java is 84.2% ambiguous and Php 46.2%, which no
rule and no auditable registry repairs. V1 therefore needs no registry at all,
provided those ecosystems stay out.

M4: Go's plain prefix rule was wrong — nested modules collide — and is repaired
deterministically by longest-module-path-wins. Vendoring is already excluded by
S071 §8. JavaScript and Rust derivations survive under stated normalization.
Cargo rename was observed 13 times and npm alias zero times; both remain
guaranteed false negatives from the S066 §10 surface. TypeScript paths aliases
were not observed and are recorded as an unmeasured residual risk.

M5 derived: Go + JavaScript/TypeScript + Rust + Python + DotNet.

R1-R14 re-evaluated: R3 and R9 revised, R6 and R7 strengthened, the rest valid.
R15 (positive-only evidence) and R16 (desynchronization) added.

Next development step, after the main transition is decided:
implement the locked detection contract as a Domain policy plus tests, in a
separate session. That session owns the first numeric SourceReferenceRuleVersion
value, the per-language lexical scanner including raw strings, template
literals, regex literals and triple-quoted forms, and the §6.1
desynchronization rule.

Session 072 is complete: verdict READY, canonical contract locked, no detector
implemented. The independent contract review returned PASS WITH FINDINGS with no
BLOCKER; every finding was corrected inside the contract and the final narrow
re-review returned PASS. Both commits are pushed and GitHub
confirmed 942090a on the session branch.

origin/main remains d480a46, so Session 072 is not integrated. d480a46 is an
ancestor of 942090a, so the integration is a fast-forward.

Exact next action:
decide the Session 072 main transition. Fast-forwarding main to 942090a, pushing
main and deleting the session branch are separate explicit user-owned Git side
effects.
