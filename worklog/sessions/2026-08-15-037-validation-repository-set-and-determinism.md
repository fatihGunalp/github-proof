# Validation Repository Set + Determinism

**Date:** 2026-08-15
**Result:** PASS
**Scope:** first 10-repository validation manifest, opt-in live pipeline validation, and snapshot-level semantic determinism
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam`

---

## Goal

Freeze the first 10 public validation repositories at real commit SHAs, exercise the existing collection-to-analysis pipeline where live access permits, and prove semantic determinism from the same snapshot and fixed evaluation time without implementing Phase 4 reporting.

## Starting State

```text
Branch: main
Base HEAD: c7c2bb6
Pre-existing Working Tree: CLEAN
Active Session: NONE
Production signals: 15 / 15
Application orchestration: COMPLETE
Phase 3: IN PROGRESS
```

## Context Read

```text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
docs/product/validation-repository-set.md
docs/architecture/mvp-signal-definitions.md — common status, determinism, and contract lock
docs/product/scoring-and-report-contract.md
GitHubRepositorySnapshotOrchestrator and existing collectors
RepositorySignalAnalyzer and 15 signal implementations
existing live/opt-in integration harness and Phase 2 orchestration tests
Session 037 task contract
```

## Plan

1. Audit the existing live harness, collection boundary, SHA behavior, and analyzer composition.
2. Select 10 distinct public repositories covering the canonical profiles and at least three ecosystems.
3. Resolve and freeze real default/head SHAs; add the canonical manifest and validation-only artifacts.
4. Add deterministic snapshot-level analyzer validation and explicit live opt-in pipeline coverage.
5. Run live validation where credentials/network permit, then full regression/build/format/Git checks.
6. Review status boundaries and forbidden inference output; finalize Session 037 without claiming human review.

## Constraints

- No signal/scoring/version calibration without a proven contract or implementation bug.
- No Phase 4 report generator, category/overall scoring, Web/API, persistence, MediatR, CQRS, or new package.
- Normal test runs remain network-free; real GitHub validation is explicit opt-in only.
- No token or credential is written to source, manifest, artifacts, or worklog.
- Manual review remains `Pending`; Codex review is not recorded as human review.
- Commit and push remain user-owned.

## Checkpoint

```text
Status: COMPLETE — manifest freeze, live pipeline run, deterministic comparison, artifacts, and regression validation passed.
Next: Session 038 — Phase 3 Final Integration / Exit Validation.
```

## Validation Set

- Added exactly 10 distinct public repositories under `samples/validation/repositories.json`.
- Covered all ten canonical initial profiles, allowing documented secondary edge cases where one repository contributes to more than one profile.
- Included C#/.NET, Rust, Python, JavaScript/HTML, Java, and language-neutral/minimal examples.
- Resolved every default/head commit through GitHub and stored a real lowercase 40-character SHA; no placeholder remains.
- Preserved archive, fork, template, primary-language, profile, expected-edge-case, notes, and `ManualReviewStatus: Pending` fields.
- Avoided an actually empty repository because it has no commit SHA to freeze; used a non-empty but insufficient-data candidate instead.

## Automated Validation

- Extended the existing explicit opt-in live integration harness from two bounded snapshots to the manifest-driven 10-repository collection-to-analysis pipeline.
- Collected each repository once, confirmed public identity and manifest metadata, and verified observed head against the frozen SHA.
- Produced exactly 15 canonical signal keys once and in canonical order for every successful snapshot.
- Confirmed `portfolio.presentation-completeness` remains excluded from overall scoring.
- Preserved snapshot collection warnings through analysis and asserted the known non-atomic snapshot warning.
- Scanned signal explanations, recommendations, limitations, and evidence for bounded forbidden seniority, AI-use, copying, and authorship inference phrases.
- Retained the explicit not-found probe: unavailable metadata produced no snapshot and no fake signal results.

## Determinism

- Reused the same `RepositorySnapshot` instance for two independent analyzers per live repository.
- Supplied the same fixed `TimeProvider` value to all 15 signal evaluations.
- Compared key order, version, category, status, score, maximum, overall inclusion, evidence, explanation, recommendation, limitations, and `EvaluatedAt`.
- All 10 repositories passed with zero semantic differences; the network was not called a second time for determinism.
- Added network-free Application coverage using all production evaluators and the same snapshot/fixed-time contract.

## Status Boundaries and Edge Cases

```text
Observed live signal results: 150
Strong: 27
Present: 49
Partial: 20
Missing: 52
NotApplicable: 1
UnavailableEvidence: 0
RequiresHumanReview: 1
```

- Live `Missing` results had `Score: 0` after relevant sections were available and complete.
- `validation-010` preserved disabled GitHub Issues as `process.issue-usage: NotApplicable` with a null score.
- All live API sections were available, so no repository naturally produced `UnavailableEvidence`; network-free pipeline coverage separately proved unavailable README evidence is not converted to `Missing`.
- `validation-004` produced bounded `RequiresHumanReview` for an exact `src/tests` directory without another supported Version 1 test marker.
- Single-commit repositories produced evidence-limited `Partial` results without AI, copying, authorship, or skill inference.
- Archived, small, and educational repositories were not labeled failed, valueless, or production-inadequate.
- The no-test/no-CI candidate produced complete-data `Missing` for both signals; no filename-only false positive was observed.
- Homepage/demo review found no implementation mismatch: the positive Spring guide result came from its explicit metadata homepage under the locked Version 1 rule.
- CI results outside the bounded Version 1 command families remain automated observations for later human comparison, not silent calibration requests.

## Validation Artifacts

- Added an aggregate automated run summary under `samples/validation/results/README.md`.
- Added per-repository snapshot summaries, ordered signal results, determinism comparisons, and manual-review templates.
- Every manual-review template remains `Pending` and states `Human review completed: NO`.
- Artifacts are validation-only; no production report generator, category score, or overall score was introduced.

## Snapshot Atomicity Limitation

The orchestrator binds default-branch contents to the observed `HeadCommitSha`, but cannot request the complete snapshot by the manifest SHA. Metadata, commit pagination, issues, pull requests, releases, tags, and contributors remain live and non-atomic. The live run confirmed all observed heads matched the frozen SHAs; future opt-in runs fail on head drift instead of claiming frozen reproducibility.

## Validation

```text
Repository baseline: PASS — main at c7c2bb6, clean before Session 037
manifest/schema tests: PASS — 2 passed
focused Application determinism/status-boundary tests: PASS
opt-in live validation: PASS — 10 / 10 repositories, 150 / 150 signals, 0 explainable failures
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 420 passed
dotnet test tests/GitHubProof.Application.Tests/GitHubProof.Application.Tests.csproj --no-restore: PASS — 10 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 90 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 520 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
manifest/result JSON parse: PASS
credential/token scan: PASS
git diff --check: PASS
```

## Architecture and Scope Review

```text
Production code changed: NO
Signal/scoring/version contract changed: NO
Phase 4 report generator: NOT IMPLEMENTED
Category/overall scoring: NOT IMPLEMENTED
Normal test-suite network access: NO
Live validation explicit opt-in: YES
Application production reference to Infrastructure: NO
Cross-layer reference limited to integration test project: YES
New third-party package: NONE
MediatR: NOT USED
CQRS: NOT INTRODUCED
Manual review falsely claimed: NO
Phase 3 after Session 037: IN PROGRESS
```

## Problems

No blocking problem. The existing snapshot atomicity limitation is documented in every snapshot artifact and remains a known constraint for Session 038 exit review.

## Working Set / Final Git State

```text
Branch: main
HEAD: c7c2bb6
Pre-existing unrelated changes: NONE

Modified:
- tests/GitHubProof.Application.Tests/Analysis/RepositorySignalAnalyzerTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/GitHubRepositorySnapshotLiveTests.cs
- tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- samples/validation/repositories.json
- samples/validation/results/
- tests/GitHubProof.Infrastructure.Tests/GitHub/ValidationArtifactWriter.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/ValidationRepositoryManifest.cs
- tests/GitHubProof.Infrastructure.Tests/GitHub/ValidationRepositoryManifestTests.cs
- worklog/sessions/2026-08-15-037-validation-repository-set-and-determinism.md
```

## Next

Session 038 — Phase 3 Final Integration / Exit Validation.

## Git

```text
Suggested commit: test(validation): establish phase 3 repository validation set
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
```
