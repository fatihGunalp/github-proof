# Usage/Demo Contract Fix and Evaluator Retry

**Date:** 2026-08-15
**Result:** PASS
**Scope:** minimum Usage/Demo canonical fix, production evaluator, and focused regression tests
**Model Route:** Sol / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Make the Version 1 Usage/Demo status matrix exhaustive for high-confidence visual/demo-only evidence, then implement the deterministic `documentation.usage-or-demo` evaluator without changing any other signal semantics.

## Starting State

~~~text
Branch: main
Base HEAD: 5060f1c
Pre-existing Working Tree: CLEAN
Relevant Previous Sessions: 027 (BLOCKED), 028 (PASS), 029 (BLOCKED)
Production evaluators: 4 / 15
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
worklog/sessions/2026-08-15-027-usage-demo-contract-audit.md
worklog/sessions/2026-08-15-028-usage-demo-semantic-contract.md
worklog/sessions/2026-08-15-029-usage-demo-signal-evaluator.md
docs/architecture/mvp-signal-definitions.md — Usage/Demo Version 1
docs/product/analysis-categories-and-signal-boundaries.md — Usage/Demo evidence boundary
docs/architecture/analysis-domain-model.md — normalized documentation and metadata evidence
docs/product/scoring-and-report-contract.md
Session 030 task contract
~~~

## Plan

1. Confirm the Session 029 contradiction and apply only the authorized Partial rule fix.
2. Implement the bounded Domain evaluator using existing `ISignalEvaluator` patterns.
3. Add focused tests for detection, the exhaustive status matrix, availability, source locality, and determinism.
4. Run focused and full regression validation, review scope, and finalize the worklog.

## Constraints

- Preserve Strong 7, Present 5, Partial 4, Missing 0, nullable statuses, Version 1, and overall inclusion.
- Do not change corpus, headings, detection, source-local, homepage, brittle-link, ambiguity, or precedence rules.
- No snapshot, collector, Infrastructure, provider, HTTP, GitHub DTO, other evaluator, orchestration, scoring, report, Web/API, or live validation changes.
- No third-party package, external Markdown parser, generic rule framework, MediatR, or CQRS.
- Commit and push remain user-owned.

## Checkpoint

~~~text
Status: COMPLETE — contract fix, evaluator, tests, full regression validation, and worklog closeout passed.
Next: Implement `process.commit-history` and `process.commit-distribution` in the next bounded session.
~~~

## Contract Fix

- Reconfirmed the Session 029 exhaustive-status contradiction against canonical docs and the Usage/Demo product boundary.
- Expanded only the existing `Partial / 4` rule so complete-corpus high-confidence screenshot/video/live-demo evidence without clear usage is Partial.
- Preserved bounded weak evidence as the other Partial path.
- Updated the Partial evidence and recommendation wording for consistency.
- Preserved Strong 7, Present 5, Missing 0, nullable statuses, status precedence, corpus, detection rules, source locality, Version 1, and overall inclusion.
- `analysis-categories-and-signal-boundaries.md` required no change because it already recognizes screenshots, videos, and live demos as positive evidence.

## Implementation

- Added `UsageDemoSignalEvaluator` as an `ISignalEvaluator` using only normalized Domain snapshot data.
- Parsed exact case-insensitive ATX and Setext English/Turkish usage and demo headings while excluding fenced-code headings and respecting same-or-higher section boundaries.
- Detected clear usage only from meaningful closed fences, non-installation-only ordered steps, locked command families, sample request/response, and sample output.
- Excluded arbitrary code, install/config/build/migration-only content, badges, logos, icons, generic images, generic links, generic video-host links, and homepage alone.
- Detected high-confidence inline/reference images, video links/HTML anchors, and homepage-corroborated live-demo links according to the canonical rules.
- Preserved conflicting positive/exclusion images and uncorroborated explicit demo links as bounded weak evidence without claiming remote link failure.
- Kept Strong evidence source-local and sorted documentation inputs with `StringComparer.Ordinal`.
- Preserved terminal Strong under incomplete corpus while requiring complete README/docs evidence for Present, Partial, or Missing.
- Required metadata availability only when homepage corroboration could change the exact result.
- Limited human review to bounded malformed fence/image/link ambiguity that could change classification.
- Read `TimeProvider` once per evaluation and produced concise deterministic evidence.

## Contract Exhaustiveness Review

~~~text
1. usage + visual            -> Strong / 7
2. usage only               -> Present / 5
3. visual/demo only         -> Partial / 4
4. bounded weak only        -> Partial / 4
5. no evidence              -> Missing / 0
6. incomplete evidence      -> UnavailableEvidence / null
7. bounded malformed syntax -> RequiresHumanReview / null

Result: PASS — 7 / 7 valid states classified
~~~

## Test Coverage

- All exact English/Turkish usage headings, ATX, Setext, trailing decoration, case-insensitive matching, section boundaries, and fenced-heading exclusion.
- Meaningful fences, canonical commands, ordered steps, sample request/response, sample output, missing arguments, prefix false positives, and installation-only exclusions.
- Inline/reference screenshots, demo-section images, exclusion markers, ambiguous markers, video hosts/files/HTML anchors, live-demo URL normalization, and brittle links.
- Strong, Present, weak Partial, high-confidence visual-only Partial, Missing, UnavailableEvidence, and RequiresHumanReview.
- Complete/incomplete corpus behavior, conditional metadata availability, source-local Strong, Ordinal docs ordering, and deterministic time/evidence output.

## Validation

~~~text
focused Usage/Demo evaluator tests: PASS — 73 passed
existing README/Description/License/Installation evaluator regression: PASS — 92 passed
documentation collector + contents + orchestrator regression: PASS — 28 passed
dotnet test tests/GitHubProof.Domain.Tests/GitHubProof.Domain.Tests.csproj --no-restore: PASS — 216 passed
dotnet test tests/GitHubProof.Infrastructure.Tests/GitHubProof.Infrastructure.Tests.csproj --no-restore: PASS — 88 passed
dotnet test GitHubProof.slnx --no-restore: PASS — 304 passed
dotnet build GitHubProof.slnx --no-restore: PASS — 0 warnings, 0 errors
dotnet format GitHubProof.slnx --verify-no-changes --no-restore: PASS
git diff --check: PASS
~~~

## Architecture and Scope Review

~~~text
ISignalEvaluator reused: YES
Provider/HTTP/GitHub DTO dependency in Domain: NO
Remote link validation: NO
Generic Markdown or rule framework: NO
New third-party package: NONE
MediatR: NOT USED
CQRS/mediator framework: NOT INTRODUCED
Canonical change beyond authorized Partial fix: NO
Snapshot/collector/other evaluator/orchestration change: NO
Production evaluators after PASS: 5 / 15
~~~

## Problems

None.

## Working Set / Final Git State

~~~text
Branch: main
HEAD: 5060f1c
Pre-existing unrelated changes: NONE

Modified:
- docs/architecture/mvp-signal-definitions.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- src/GitHubProof.Domain/Analysis/Signals/UsageDemoSignalEvaluator.cs
- tests/GitHubProof.Domain.Tests/Analysis/Signals/UsageDemoSignalEvaluatorTests.cs
- worklog/sessions/2026-08-15-030-usage-demo-contract-fix-and-evaluator.md
~~~

## Next

Implement `process.commit-history` and `process.commit-distribution` in the next bounded Phase 3 session.

## Git

~~~text
Suggested commit: feat(domain): add usage demo signal evaluator
Commit: NOT CREATED — user-owned unless explicitly requested
Push: NOT PERFORMED — user-owned unless explicitly requested
~~~
