# Historical-Ref Repository Content Collection

**Date:** 2026-08-15
**Result:** PASS
**Scope:** explicit commit/ref support for ref-pinnable repository content while preserving live/non-atomic GitHub sections
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Allow `RepositorySnapshot` collection to read default-branch content evidence from an explicitly requested valid commit SHA without silently substituting current HEAD, while preserving the existing live semantics for issues, pull requests, releases, tags, and contributors.

## Starting State

```text
Branch: main
Base HEAD: 2552fda
Pre-existing working tree: uncommitted Session 038 and Session 039 artifacts; no unrelated changes identified
Active prior session: Session 039 — BLOCKED
Current default-HEAD collection: supported
Historical-ref content collection: not supported
```

## Locked Boundaries

- Pin only ref-addressable repository content: file tree, selected project/test files, workflow files, documentation Markdown, README, and license-file content where supported.
- Preserve live/non-atomic semantics for metadata, issues, pull requests, releases, tags, contributors, and other non-ref-pinned sections.
- Preserve existing default-HEAD behavior without regression.
- Accept arbitrary valid commit SHA values.
- Invalid or nonexistent refs must fail fast or be represented explicitly as unavailable; never fall back to current HEAD.
- Snapshot warnings/artifacts must distinguish frozen-ref content from live/non-atomic sections.
- No dependency, MediatR, CQRS, evaluator, signal, scoring, or canonical rule change.
- Do not run validation-011 automated analysis in this session.
- Commit and push remain user-owned.

## Context Read

```text
AGENTS.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/04-development-workflow.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
docs/architecture/analysis-domain-model.md — RepositorySnapshot and DataAvailability
docs/roadmap/mvp-phases.md — Phase 2 collection boundary
contents/descriptive collector interfaces, implementations, orchestrator, and focused tests
Session 039 target mismatch checkpoint
```

## Implementation

- Added `GitHubCommitReference` validation and lowercase normalization for full 40-character hexadecimal SHAs.
- Added `GitHubRepositorySnapshotOrchestrator.CollectAtCommitAsync` while preserving existing `CollectAsync` default-HEAD behavior.
- Explicit SHA collection skips default-branch ref resolution and retrieves the commit/tree directly.
- File tree, project/dependency files, workflow files, documentation Markdown, README, and license evidence use the requested SHA.
- Metadata, languages, commit history, issues, pull requests, releases, tags, and contributors retain live collection semantics.
- Snapshot warnings explicitly distinguish requested-commit content from live/non-atomic sections.
- Invalid SHA format fails before collector calls; nonexistent/unavailable SHA fails without current-HEAD fallback.
- Updated canonical Phase 2/RepositorySnapshot documentation; no signal or scoring contract changed.

## Checkpoint

```text
Status: PASS — historical-ref content collection capability implemented and verified.
Next: resume blocked Session 039 against c3eea5bec401935206d4697d0c73af950514c1ac in a separate continuation.
```

## Final Validation

```text
Focused Infrastructure tests: PASS — 101 passed
Solution tests: PASS — 531 passed (420 Domain, 10 Application, 101 Infrastructure)
Solution build: PASS — 0 warnings, 0 errors
dotnet format verify: PASS
git diff --check: PASS
Live validation-011 analysis: NOT RUN — intentionally outside Session 040
```

## Result

```text
PASS — arbitrary valid historical commit SHA content collection is supported without changing default-HEAD behavior or reconstructing live GitHub sections.
Session 039 remains BLOCKED until explicitly resumed; its target SHA is unchanged.
```

## Suggested Commit

```text
feat(infrastructure): support historical content references
```
