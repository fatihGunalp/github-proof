# Session 062 — Profile P3 GitHub Profile Data Collection

**Date:** 2026-08-16
**Result:** PASS
**Scope:** Stage 7 / Profile P3 implementation
**Execution:** ChatGPT analysis → Claude implementation/review → user-owned Git finalization

## Git

Branch:
session/062-profile-p3-data-collection

Session Base:
4ca1a2169989ba730149d9a2dc3233fba38e63a4

Remote main at start:
4ca1a2169989ba730149d9a2dc3233fba38e63a4

## Development Position

Completed:
Stage 6 — Profile P2 — COMPLETE / REMOTE VERIFIED

Current:
Stage 7 — Public Profile Evidence Foundation

Active:
P3 — GitHub Profile Data Collection

Next:
P4 — RepositoryAnalysis Orchestration

## Goal

Implement deterministic normalized public GitHub profile collection.

## Required P3 Capability

- GitHub profile identity validation
- public profile metadata
- public repository pagination
- normalized repository manifest
- fork/template/archive metadata
- empty-repository metadata required downstream
- collection bounds
- partial API failure semantics
- rate-limit behavior
- unavailable-data semantics
- deterministic repository ordering
- explicit capture-time input
- deterministic tests

## Canonical Boundaries

GitHubProfileSnapshot is observable public profile state.

It is not:
- Developer
- DeveloperCapabilityProfile
- RepositoryAnalysis
- RepositoryEligibilityResult
- repeated-pattern interpretation
- JobEvidenceAlignment

ProfileRepository is a normalized manifest entry.

It is not:
- RepositoryAnalysis
- RepositorySnapshot duplication
- eligibility decision
- capability claim

## Non-Goals

Do not implement:
- P4 RepositoryAnalysis orchestration
- P5+ cross-repository aggregation
- profile patterns
- representative repositories
- role alignment
- profile report UI
- P10 Web Profile Flow
- job analysis
- developer score
- seniority
- employability
- hire/no-hire semantics

## Acceptance Criteria

1. P3 remains strictly before P4.
2. P1/P2 semantics remain unchanged.
3. Domain has no GitHub REST/HTTP/JSON dependency.
4. Profile snapshot is immutable normalized observable state.
5. Repository manifest ordering is deterministic.
6. Pagination is bounded and deterministic.
7. Partial failure and rate-limit states are explicit.
8. Hidden system clock is not introduced into Domain.
9. Same semantic input produces same semantic snapshot.
10. Existing repository analysis behavior remains intact.
11. Relevant tests PASS.
12. Independent review has no BLOCKER/HIGH findings.

## Claude Routing

Model: opus
Effort: high

## Implementation Result

New Domain area `GitHubProof.Domain.Profile` and new Infrastructure area
`GitHubProof.Infrastructure.GitHub.Profile`.

No existing tracked file was modified; the complete change is additive.

Canonical decisions taken during implementation:

- Repository conditions are tri-state (`RepositoryConditionState`:
  `Undetermined` / `Present` / `Absent`); absent GitHub metadata normalizes to
  `Undetermined`, never to an optimistic `false`.
- Activity truth uses `pushed_at` only. `updated_at` is deliberately not
  collected so P2 §19.2 cannot silently substitute a metadata timestamp for
  source-control activity.
- Empty repository is derived from `size == 0 && pushed_at == null`; any other
  known size resolves to `Absent`, unknown size resolves to `Undetermined`.
- P3 produces repository **conditions** only. No `AnalysisAdmissionDecision`,
  no `EligibilityReason`, no eligibility result.
- Manifest ordering is enforced in Domain (`ProfileRepositoryManifest`) by
  ordinal `(Owner, Name)`, independent of API response order and locale.
- Pagination is stabilized with `type=owner&sort=full_name&direction=asc` and
  bounded by `DefaultMaxPageCount = 10`.
- Snapshot capture time is an explicit Domain constructor input; `TimeProvider`
  stays in Infrastructure.
- The collected profile identity is authoritative over the requested handle,
  so a renamed account resolved by GitHub redirect stays collectable.

## Validation

- `dotnet test GitHubProof.slnx`: 711/711 PASS
  (Domain 440, Infrastructure 149, Application 54, Web 68).
- Build: 0 warning / 0 error.
- `git diff --check`: clean.
- Acceptance criteria 1–12: PASS.

## Independent Review

`github-proof-review` — two rounds.

Round 1 verdict: PASS WITH FINDINGS (LOW only, no BLOCKER/HIGH).

Round 2 reviewed an accepted bounded correction and raised MEDIUM-1: the
correction rejected a metadata/handle login mismatch by throwing, which turns
the legitimate GitHub rename-redirect case into an exception and diverges from
the collector's "every runtime condition becomes DataAvailability" design.

MEDIUM-1 was accepted and the correction was reverted. The reviewed baseline
behaviour is restored and one regression test now locks the canonical-identity
rule instead.

Final state: no BLOCKER/HIGH/MEDIUM finding open.

## Remaining Risk

- Page-bound truncation is reported as `Available` plus `IsComplete = false`
  and a collection warning. P4 consumers must read manifest completeness, not
  only `Availability.IsAvailable`.
- `ProfileRepositoryReference` equality/ordering is ordinal. GitHub
  owner/repository names are case-insensitive-unique; identity is always
  derived from the canonical API response, so this is currently safe but should
  be revisited when `RepositoryAnalysisReference` matching is defined.
- `GitHubProfileSnapshot` has no semantic value equality (its manifest and
  list members use reference equality), matching existing `RepositorySnapshot`.
  P1 §15 `SnapshotReference` must not be built on record equality.
- `IGitHubProfileRepositoryCollector` keeps the handle parameter for symmetry
  with the repository collectors while `metadata.Identity` stays authoritative.
  This is intentional; do not "fix" it into a mismatch guard.

## Next Action

User manually commits and pushes the Session 062 branch.

After remote verification, continue with Profile P4 — RepositoryAnalysis
Orchestration.
