# Simple Repository Evidence Signals

**Date:** 2026-08-15
**Result:** BLOCKED — CONTRACT GAP
**Scope:** domain
**Model Route:** Terra / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement only the deterministic `repository.description.present` and `repository.license.present` evaluators from normalized RepositorySnapshot evidence to SignalResult.

## Starting State

~~~text
Branch: main
Base HEAD: fe47257
Pre-existing Working Tree: CLEAN
Relevant Previous Session: worklog/sessions/2026-08-15-019-signal-engine-readme-foundation.md
~~~

## Context Read

~~~text
AGENTS.md
agents/02-context-loading.md
agents/04-development-workflow.md
agents/05-domain-development.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
worklog/INDEX.md
docs/architecture/mvp-signal-definitions.md — Description and License sections
docs/architecture/analysis-domain-model.md
docs/product/analysis-categories-and-signal-boundaries.md
docs/product/scoring-and-report-contract.md
ISignalEvaluator and ReadmeExistsSignalEvaluator
RepositorySnapshot, RepositorySnapshotData, DataAvailability, SignalResult
Phase 2 descriptive-data collector and snapshot orchestrator
Session 020 task contract
~~~

## Contract Audit

- Description can be evaluated deterministically from `metadata` availability and `RepositoryMetadata.Description`.
- The canonical License signal requires three distinct available-data outcomes: recognized license, unrecognized license file, and no license evidence.
- Phase 2 collects an unrecognized `/license` response as `License: null` plus `LicenseFile`, but the snapshot orchestrator merges that file into the generic `FileTree` and drops the dedicated `LicenseFile` semantic.
- `RepositorySnapshot` therefore carries `License` and generic `FileTree` entries, but no normalized marker or canonical file-name rule that identifies which entry is the unrecognized license evidence.
- A new evaluator-side file-name heuristic would be a product/domain decision not specified by canonical docs and could classify an unrelated root file as a license.
- Implementation is blocked because `Partial` cannot be produced reliably without inventing that missing contract.

## Changes

- No production or test code was added.
- Session worklog records the contract gap and required correction.

## Decisions

- No new third-party dependency will be introduced.
- Only the Description Present and License Present evaluators are in scope.
- The existing evaluator boundary remains sufficient; no new framework is needed.
- Do not implement the Description evaluator independently, because this bounded task requires both evaluators and the License contract gate failed first.
- Do not infer a license from generic FileTree paths until canonical docs define the normalized evidence contract.

## Validation

No test, build, or format command was run because the required contract audit blocked implementation before production code changed.

## Problems

~~~text
BLOCKED — CONTRACT GAP

Affected signal:
repository.license.present

Canonical requirement:
Available data must deterministically distinguish a GitHub-recognized license,
an existing but unrecognized license file, and no license evidence.

RepositorySnapshot representation:
License is nullable recognized-license metadata. The Phase 2 LicenseFile is merged
into generic FileTree and its license-evidence provenance is not preserved.

Why deterministic evaluation is impossible:
When License is null, a generic root RepositoryFile entry does not canonically state
that it is the unrecognized license file returned by the license collection. Selecting
one by name would introduce an undocumented heuristic and can misclassify unrelated files.

Recommended minimum contract correction:
Preserve the normalized LicenseFile separately on RepositorySnapshot (or an equivalent
LicenseEvidence value with optional recognized metadata and optional detected file path),
and document its semantics. Then License != null maps to Present, License == null with
LicenseFile != null maps to Partial, and both absent with available license data maps to Missing.
~~~

## Not Done

- `repository.description.present` and `repository.license.present` were not implemented.
- The other 12 evaluator implementations.
- Category and overall scoring, analysis orchestration, reporting, and calibration.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: fe47257
Pre-existing unrelated changes: NONE
Final working tree: Session 020 worklog changes only
~~~

## Next

Resolve the canonical License evidence contract, then run a new mandatory model gate before resuming this task.

## Git

~~~text
Suggested commit: feat(domain): add description and license signal evaluators
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~
