# Session 063 — Profile P4 RepositoryAnalysis Orchestration

**Date:** 2026-08-17
**Result:** PASS
**Scope:** Stage 7 / Profile P4 implementation
**Execution:** ChatGPT analysis → Claude implementation/review → user-owned Git finalization

## Git

Branch:
session/063-profile-p4-repository-analysis-orchestration

Session Base:
341a450a71eef20794a323b2725b07c11b9943ba

Remote main at start:
341a450a71eef20794a323b2725b07c11b9943ba

## Development Position

Completed:
P3 — GitHub Profile Data Collection — COMPLETE / REMOTE VERIFIED

Current:
P4 — RepositoryAnalysis Orchestration

Next:
P5 — Cross-Repository Evidence Engine

## Goal

Reuse the existing deterministic repository analysis pipeline inside profile
analysis without duplicating repository-analysis semantics.

Canonical flow:

GitHubProfileSnapshot
→ repository admission / eligibility
→ existing repository snapshot + analysis pipeline
→ traceable RepositoryAnalysis results

## Canonical Inputs

- docs/product/public-profile-evidence-product-contract.md
- docs/architecture/public-profile-evidence-domain-model.md
- docs/architecture/public-profile-evidence-repository-eligibility.md
- docs/roadmap/mvp-phases.md
- existing RepositoryAnalysis pipeline and orchestration code
- Session 062 remaining-risk notes

## Critical Constraint

RepositoryEligibilityResult production implementation is not currently present
on main.

P4 MUST NOT bypass P2 by analyzing every repository blindly.

If runtime admission/eligibility is required, implement only the minimum
deterministic bridge required by the locked P2 contract.

Do not invent new eligibility semantics.

## Required P4 Capability

1. Consume GitHubProfileSnapshot from P3.
2. Respect snapshot collection availability and completeness.
3. Apply P2-defined repository admission/eligibility semantics before analysis.
4. Do not analyze repositories whose admission decision is Skip.
5. Do not silently treat Undetermined admission as Analyze.
6. Reuse the existing repository snapshot/analysis pipeline.
7. Do not copy or fork the 15 signal evaluators.
8. Preserve existing RepositoryAnalysis semantics exactly.
9. Keep each profile repository result traceable to its ProfileRepositoryReference.
10. Preserve deterministic result ordering independently of execution order.
11. Represent per-repository collection/analysis failure explicitly.
12. Do not let one repository failure silently erase successful repository analyses.
13. Do not introduce hidden clocks.
14. Keep orchestration outside Web.
15. Add deterministic tests covering admission, ordering, reuse and partial failure.

## P3 Carry-Forward Invariants

- Availability.IsAvailable does not imply the repository manifest is complete.
  P4 must inspect IsComplete where completeness matters.
- ProfileRepositoryReference currently uses ordinal equality/ordering.
  Do not silently redefine its equality semantics in P4.
- GitHubProfileSnapshot record equality is not semantic snapshot identity.
  Do not build SnapshotReference on record equality.
- metadata.Identity remains authoritative over the requested GitHub handle;
  do not reintroduce a login mismatch guard that breaks GitHub rename redirects.

## Non-Goals

Do NOT implement:

- P5 CrossRepositoryEvidence
- technical focus patterns
- engineering practice patterns
- problem-area patterns
- representative repositories
- role alignment
- DeveloperPublicEvidenceProfile
- profile score
- seniority
- employability
- hire/no-hire
- P10 Web Profile Flow
- job evidence alignment

## Architecture Boundary

Profile orchestration must not:

- duplicate repository collectors unnecessarily,
- duplicate signal evaluation,
- recalculate repository score,
- reinterpret RepositoryAnalysis,
- place analysis logic in Web,
- convert repository failure into developer judgment.

RepositoryAnalysis remains the atomic repository evidence truth.

## Failure Boundary

Profile orchestration must preserve distinctions between:

- profile collection unavailable,
- profile collection incomplete,
- repository not admitted,
- repository admission undetermined,
- repository collection failure,
- repository analysis failure,
- successful RepositoryAnalysis.

Unavailable or failed evidence is not negative developer evidence.

## Determinism

Same:

- GitHubProfileSnapshot
- eligibility/admission contract version
- repository observable state
- analyzer version

must produce the same semantic orchestration result and deterministic ordering.

Concurrency, if used, must not change result semantics or order.

## Acceptance Criteria

1. Existing 15-signal analysis engine is reused, not copied.
2. Existing RepositoryAnalysis semantics remain unchanged.
3. P2 admission/eligibility is not bypassed.
4. Skip repositories are not analyzed.
5. Undetermined is not silently promoted to Analyze.
6. Successful repository analyses are traceable to ProfileRepositoryReference.
7. Partial repository failure is explicit and does not erase successful results.
8. Result ordering is deterministic.
9. P3 incomplete collection semantics are respected.
10. No P5+ aggregation or developer inference appears.
11. No Web analysis logic is introduced.
12. No hidden Domain clock is introduced.
13. Existing repository analysis tests remain green.
14. New P4 tests cover reuse, admission, ordering and failure boundaries.
15. Full relevant solution tests PASS.
16. Independent forked review returns no BLOCKER/HIGH findings.

## Claude Workflow

1. Read CLAUDE.md.
2. Verify branch and session base.
3. Read CURRENT and this session.
4. Load minimum canonical P2/P4 context.
5. Inspect existing repository snapshot + RepositoryAnalysis orchestration before designing.
6. Confirm whether P2 runtime eligibility implementation exists.
7. If absent, implement only the minimum locked-contract bridge required for P4.
8. Recommend Claude-native model + effort and STOP.
9. Wait for user to select model/effort and write devam et.
10. Implement bounded P4.
11. Run relevant tests and full regression suite.
12. Run independent forked review.
13. Address accepted findings without scope drift.
14. Finalize CURRENT + Session 063.
15. Do NOT commit.
16. Do NOT push.

## Claude Routing

Model: opus
Effort: high

## Implementation Result

New Domain area `GitHubProof.Domain.Profile` P2 Stage A bridge and new
Application area `GitHubProof.Application.Profile` orchestration.

One tracked file changed: `RepositoryAnalysisRunner` now declares
`IRepositoryAnalysisRunner`. No analysis behaviour, no Web code and no
canonical contract was modified.

Canonical decisions taken during implementation:

- Only P2 Stage A was implemented (`AnalysisAdmissionDecision` +
  `AnalysisAdmissionReason` limited to the canonical `EmptyRepository` and
  `RequiredInputUnavailable` identities). No `RepositoryEligibilityResult`,
  no contribution decision, no independence or activity classification.
- Admission consumes only `ProfileRepository.Empty`: `Present` → `Skip`,
  `Absent` → `Analyze`, `Undetermined` → `Undetermined`. Provenance and
  lifecycle conditions never restrict admission (contract §6.2–§6.4).
- `EligibilityPolicyVersion` is an identity value object only. No
  `CurrentActivityWindow` duration was invented, because Stage A does not
  consume it and the contract does not fix a V1 value.
- The orchestrator drives the existing pipeline through
  `IProfileRepositorySnapshotSource` (Application port, Domain types only)
  and `IRepositoryAnalysisRunner`. Application still does not reference
  Infrastructure.
- `IRepositoryAnalysisRunner` was introduced so the repository-analysis
  failure boundary is representable and testable without forking the
  analyzer. `RepositoryAnalysisRunner` is unchanged otherwise.
- Per-repository outcomes are explicit: `NotAdmitted`,
  `AdmissionUndetermined`, `CollectionUnavailable`, `CollectionFailed`,
  `AnalysisFailed`, `Analyzed`. Technical failure never becomes evidence
  absence, and one failing repository cannot erase successful analyses.
- Ordering is enforced inside `ProfileAnalysisOrchestrationResult`, so it is
  independent of execution order; `ProfileRepositoryReference` ordering stays
  canonical and the URL only breaks ties. Its equality/ordering was not
  redefined.
- The result requires exactly one entry per manifest repository, so a dropped
  repository cannot pass silently.
- `IsRepositoryManifestComplete` surfaces P3 incompleteness; the orchestrator
  makes no aggregate claim over a partial manifest.
- The orchestrator owns no clock. Analysis ids come from an injectable
  `Func<Guid>` defaulting to `Guid.NewGuid`, matching the existing Web pattern.
- A renamed repository resolved by GitHub redirect stays analyzable: the
  profile reference remains the traceability key and the collected identity is
  not guarded against it.

## Validation

- `dotnet test GitHubProof.slnx`: 748/748 PASS
  (Domain 455, Infrastructure 149, Application 76, Web 68).
- Build: 0 warning / 0 error.
- `git diff --check`: clean.
- Acceptance criteria 1–16: PASS.

## Independent Review

`github-proof-review` — two rounds, forked context.

Round 1 verdict: PASS WITH FINDINGS, no BLOCKER/HIGH. Accepted MEDIUM:
`ProfileAnalysisOrchestrationResult` did not require full manifest result
coverage, so a dropped repository result was indistinguishable from a complete
set. Fixed plus regression test; the reviewer confirmed the test is
load-bearing by mutation.

Round 2 verdict: PASS WITH FINDINGS, no BLOCKER/HIGH. Accepted MEDIUM:
`ProfileRepositorySnapshotResult` allowed `(snapshot: null, availability:
Available)`, which would have thrown out of `AnalyzeAsync` and erased already
successful repository analyses. The constructor now rejects that state, so a
malformed source result degrades to `CollectionFailed` instead.

Final state: no BLOCKER/HIGH/MEDIUM finding open.

## Remaining Risk

- `IProfileRepositorySnapshotSource` has no Infrastructure adapter and no Web
  wiring. P4 is library-only; production reuse becomes real when the profile
  flow is composed (P10 or a bounded adapter session). Application must not
  gain an Infrastructure reference to do it.
- `RepositoryAnalysisComposer` never emits `AnalysisStatus.Failed`; technical
  failure lives in `ProfileRepositoryAnalysisOutcome`. P2 Stage B §22.5 must
  map from that outcome rather than expecting a `Failed` analysis status.
- `AnalysisAdmissionReason.RequiredInputUnavailable` carries no structured
  reference to the missing input. Sufficient while `Empty` is the only
  admission input; §22.6 will need a structured reference when Stage B lands.
- `AnalysisAdmissionReason` intentionally exposes only the two Stage A reason
  identities. Full P2 owns the complete canonical `EligibilityReason` taxonomy.
- `worklog/CURRENT.md` carries a UTF-8 BOM introduced before this session;
  it was left untouched to avoid unrelated churn.

## Next Action

User manually commits and pushes the Session 063 branch.

After remote verification, continue with P5 — Cross-Repository Evidence
Engine.
