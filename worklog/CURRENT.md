# GitHub Proof — Current Development State

**Last Updated:** 2026-08-17
**Source of Truth:** docs/

## Development

Completed:
P6 — Technical, Practice and Problem Patterns — COMPLETE / REMOTE VERIFIED

Completed Foundation:
Session 066 — Repository Evidence Surface Expansion / ProblemArea Evidence
Foundation — COMPLETE / REMOTE VERIFIED

Current:
Stage 7 — Public Profile Evidence Foundation

Interposed Contract Work:
ProblemArea Semantic Mapping Contract

Next Profile Phase:
P7 — Representative Repository Selection

## Production Delivery

Milestone A:
NOT_STARTED

Milestone B:
BLOCKED

## Active Session

Session:
067 — ProblemArea Semantic Mapping Contract

Session File:
worklog/sessions/2026-08-17-067-problem-area-semantic-mapping-contract.md

Status:
READY_FOR_MANUAL_COMMIT

## Git

Active Branch:
session/067-problem-area-semantic-mapping-contract

Session Base:
b9dd3bdd82fd4ee435d844a4e01d37166811a85d

Last Remote Verified Main:
b9dd3bdd82fd4ee435d844a4e01d37166811a85d

## Goal

Resolve the semantic boundary between normalized dependency declarations and
future repository/profile ProblemArea truth without increasing epistemic
strength beyond observable evidence.

## Critical Boundary

Session 066 locked dependency declarations as a non-scoring structured
RepositorySnapshot evidence surface.

Repository MVP V1 still owns exactly 15 scoring signals.

ProblemArea remains deferred.

Session 067 must first resolve:

dependency declaration
!=
semantic category
!=
produced-work evidence
!=
repository-level ProblemArea evidence
!=
repeated profile-level ProblemArea pattern

before any taxonomy, mapping or production implementation.

## Canonical Tension

P6 §9.4 condition 1 uses the phrase "repository-level signal".

Session 066 explicitly locked dependency evidence as a non-scoring structured
repository evidence surface.

This wording/ownership tension must be resolved explicitly and may not be
silently reinterpreted.

## Carry-Forward

- dependency declarations are declared-only evidence
- package declaration != runtime use
- package declaration != produced work
- package declaration != developer capability
- ecosystem neutrality must remain bounded and explicit
- unsupported ecosystem/package != absence
- unavailable != absence
- incomplete != negative evidence
- no profile-layer manifest parsing
- no hidden weighting
- preserve deterministic replay
- preserve full evidence traceability
- no scoring signal 16+
- no developer role/seniority/employability/hire inference

## Blocker

NONE

## Validation

Full suite: 911/911 PASS
Build: 0 warning / 0 error
Independent review: PASS WITH FINDINGS, no BLOCKER; accepted findings fixed

AnalyzerVersion:
1.0.0

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

15 scoring signals, score normalization, RepositoryAnalysis status, report
schema and presentation: UNCHANGED

## Result

Repository Dependency Semantic Category Contract V1 is LOCKED with five category
keys: database-access, web-application-framework, http-client, authentication,
machine-learning. http-server stays deferred.

The canonical registry artefact
docs/architecture/registries/dependency-category-mapping/1.0.0.json carries the
approved 46-entry seed and is embedded directly by Infrastructure, so no mirror
exists.

Domain owns the taxonomy, its canonical keys, lookup normalization, the typed
registry and RepositorySemanticEvidence. Infrastructure owns JSON loading only.
Domain references no JSON, stream or resource API.

RepositorySemanticEvidence is non-scoring: one observation per category with one
or more traceable supporting declarations, no counts, weights or scores, and it
is never ProblemArea evidence.

The manifest package identity blocker was closed earlier in this session: npm
aliases and Cargo renames now record the referenced package identity, and an
unresolvable alias is omitted rather than recorded falsely.

## Remaining Risk

Go `// indirect` requires and Maven dependencyManagement entries can support a
category observation for a dependency the author never declared directly. This
is contracted as a known limitation and needs its own approval and
AnalyzerVersion decision to change.

Nothing composes the registry loader or the semantic policy yet: no consumer, no
persistence, no presentation.

Immutable-version retention across future mapping versions is structurally
supported but no retention policy is written.

produced-work evidence, repository ProblemArea evidence and profile ProblemArea
promotion remain unimplemented and deferred.

## Next Action

User manually commits and pushes the Session 067 branch.

After remote verification, continue with P7 — Representative Repository
Selection, or with the deferred produced-work evidence decision.
