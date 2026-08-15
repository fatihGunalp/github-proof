# Session 037 Automated Validation Summary

**Run date:** 2026-08-15

**Manifest:** `samples/validation/repositories.json`

**Manual review:** Pending for all repositories

**Human review claimed:** No

> Historical note: the preceding statements describe Session 037's automated run. The subsequent user-provided human comparison is complete; see [Session 038 Human Review and Automated Comparison Summary](./human-review-summary.md).

## Automated Run

```text
Repositories requested: 10
Repositories collected: 10
Explainable collection failures: 0
Frozen SHA matches: 10 / 10
Signal results: 150 / 150
Canonical 15-key integrity: PASS
Snapshot-level repeated analysis: 10 / 10
Semantic differences: 0
Forbidden developer/seniority/AI/copying inference: 0
```

Observed signal statuses:

```text
Strong: 27
Present: 49
Partial: 20
Missing: 52
NotApplicable: 1
UnavailableEvidence: 0
RequiresHumanReview: 1
```

The explicit not-found probe produced no snapshot and returned unavailable metadata. No fake signal result was produced for it.

## Status-Boundary Evidence

- `Missing` appeared only as a scored zero result after the relevant live section was available and complete.
- `NotApplicable` appeared for `process.issue-usage` on `validation-010`, where GitHub Issues is disabled and normalized as `issues: NotApplicable`.
- No live repository produced `UnavailableEvidence` because all required API sections were collected successfully. Network-free Application coverage separately proves that unavailable README evidence remains `UnavailableEvidence` and is not converted to `Missing`.
- `RequiresHumanReview` appeared for `validation.test-structure` on `validation-004`: the exact `src/tests` directory name was observable, but Version 1 could not confirm an additional supported test marker.

## Focused Edge-Case Observations

- `validation-003` and `validation-010` each expose a one-commit history as `Partial`; no AI use, copying, authorship, or developer-skill inference was emitted.
- Archived repositories remained analyzable and were not labeled failed or valueless.
- The small `yocto-queue` repository produced evidence-based results without a size-based quality judgment.
- The educational Spring guide remained analyzable without a production-readiness penalty or inference.
- `github-games`, the test/CI absence candidate, produced `Missing` for both test structure and CI after complete file/workflow inspection; no filename-only false positive was observed.
- The Spring guide homepage result came from its explicit metadata homepage and therefore matches the locked Version 1 rule; no remote health or content-quality claim was made.
- Scrutor, pytest-subtests, and the Spring guide expose workflows whose commands are partly outside the bounded Version 1 command families. Their `Partial` CI results are observations for later human comparison, not automatic rule-change requests.

## Snapshot Atomicity Limitation

The current collector resolves default-branch contents and `HeadCommitSha` together, but it cannot request the complete snapshot by the manifest SHA. Metadata, commit pagination, issues, pull requests, releases, tags, and contributors are collected from live GitHub state and are not one atomic transaction.

This run verified that every observed head matched its frozen manifest SHA. Future opt-in runs intentionally fail fast on head drift instead of pretending to reproduce the frozen snapshot.

## Artifact Layout

Each `validation-###/` directory contains:

- `snapshot-summary.json` — normalized collection counts, availability, warnings, and atomicity notice;
- `signal-results.json` — 15 ordered signal results at the fixed evaluation time;
- `determinism.json` — one network collection, two snapshot analyses, and zero semantic differences;
- `manual-review.md` — an uncompleted human-review template with status `Pending`.

These are validation-only artifacts. They are not Phase 4 production reports and do not implement category or overall scoring.
