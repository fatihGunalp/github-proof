# Session 062 — Profile P3 GitHub Profile Data Collection

**Date:** 2026-08-16
**Result:** IN_PROGRESS
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

## Claude Workflow

1. Read CLAUDE.md.
2. Verify branch and session base.
3. Read CURRENT and this session.
4. Load minimum canonical context.
5. Inspect existing GitHub infrastructure.
6. Recommend Claude model + effort and STOP.
7. Wait for user to select model/effort and write devam et.
8. Implement bounded P3.
9. Validate.
10. Run independent review.
11. Update worklog.
12. Do not commit or push.

## Next Action

Open Claude Code and continue from CURRENT.
