# P9-I1 — Validation Contract + Scenario Matrix

**Date:** 2026-08-20
**Result:** DOCS — new `docs/product/p9-profile-validation-contract.md`
created; canonical P9 validation surface (11-scenario matrix, 9-risk
mapping, prohibited-claim checklist, traceability PASS/FAIL rules,
unsupported/deferred acceptance rules, deterministic comparison surface,
existing-asset reuse boundary, P9-I2 handoff requirements) locked — no
src/, no tests/
**Scope:** docs/-only, validation-contract slice — production semantics
unchanged
**Model Route:** sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Implement `p9-implementation-plan.md` §8 P9-I1: lock the exact bounded
profile-validation scenario set, risks, expected behavior, prohibited
claims, traceability rules and deterministic comparison surface so that
P9-I2 (fixtures/harness) can be built without inventing validation
semantics.

---

## Context Read

```text
Operational:
CLAUDE.md
.claude/skills/github-proof-dev/SKILL.md (implied by CLAUDE.md skill gate)
worklog/CURRENT.md
agents/02-context-loading.md
agents/07-worklog-integration.md
docs/product/p9-implementation-plan.md (full)
docs/roadmap/mvp-phases.md — Profile P9 / Stage 7 clarification only

Canonical (bounded, only what could change the P9-I1 contract):
docs/product/public-profile-evidence-product-contract.md — §7, §8, §9, §10,
  §11, §12, §13, §14, §15, §16, §17, §18
docs/architecture/public-profile-evidence-report-contract.md (full)
docs/product/validation-repository-set.md (full)
tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs (full)
```

Not eager-loaded: domain model full text, eligibility/pattern/
representative-repository contract full text, prior session history beyond
CURRENT.md's own summaries (already sufficient for P8 canonical-owner
citations).

---

## Model Route

```text
Selected:
sonnet / high

Why:
Not mechanical documentation — required deriving a scenario/risk matrix and
a determinism comparison surface consistent with already-locked canonical
contracts (product contract, report contract, domain model via CURRENT.md)
and with the actual ProfilePipelineTests.cs assertions, without inventing
semantics or conflating repository-level and profile-level validation.
```

---

## Work Performed

Created `docs/product/p9-profile-validation-contract.md` (CONTRACT LOCKED,
12 required sections):

1. Canonical validation purpose — reuses `p9-implementation-plan.md` §1
   exit criteria as reference, not re-derived.
2. Exact profile scenario matrix — the 11 canonical scenarios from
   `p9-implementation-plan.md` §4, verbatim, with an explicit
   scenario-label-is-not-a-role-claim boundary.
3. Scenario → risk mapping — 11 scenarios × 9 required risks
   (`p9-implementation-plan.md` §8 P9-I3), each cell stating the concrete
   claim being falsified (not ornamental).
4. Expected bounded behavior per scenario — derived only from already-locked
   product contract §7/§10/§12/§16 and the existing empirically-verified
   `ProfilePipelineTests` behavior (sparse-single-repository →
   `LimitedIndependentEvidenceSurface`); no new threshold/algorithm.
5. Prohibited profile-claim checklist — direct consolidation of product
   contract §18 (17 items) + report contract §16 (3 additional
   presentation-boundary items), no speculative expansion.
6. Evidence-traceability acceptance rules — single reused trace model
   (product contract §6.1 chain / report contract §11 WhyThisConclusion /
   `ProfileComposer`'s existing reference-identity coherence check), with
   concrete PASS (5 conditions) and FAIL (broken/fabricated/orphaned)
   criteria; no second trace model introduced.
7. Unsupported-claim acceptance rules — direct from report contract §6.2/
   §6.3/§6.4 and product contract §14; explicit statement that
   `RoleEvidenceAlignment` false-alignment is not a dedicated P9 risk
   because it is never produced in V1 (matches
   `p9-implementation-plan.md` §8 P9-I3 note).
8. Deterministic comparison surface — the 9 fields actually asserted by
   `ProfilePipelineTests.AssertSemanticallyEquivalent` (Status,
   RepositoryEligibilityResults, Limitations, Warnings,
   RepeatedProfilePatterns, RepresentativeRepositories, Report.Sections,
   WhyThisConclusion.Items, RepositoryDrillDown.Items) — inspected from the
   real test file, not assumed from the task's candidate list; explicitly
   scopes out `AnalysisId` values / object-reference identity / wall-clock
   as non-semantic unless fixed as input.
9. Existing validation asset reuse boundary — explicit
   `repository-level baseline (validation-repository-set.md,
   samples/validation/repositories.json, samples/validation/results/,
   ProfilePipelineTests.cs) != profile-level P9 scenario set`; states a
   scenario is a multi-repository composition, not a 1:1 mapping to one
   validation-repository record.
10. P9-I2 handoff requirements — 6 concrete deliverables (scenario
    fixtures, risk-check hooks, prohibited-claim scan, traceability check,
    unsupported/deferred check, determinism hook) each traced back to a
    section of this contract.
11. Explicit non-goals — restates the task's production/tests/P10/Web/
    threshold/taxonomy/harness prohibitions.
12. P9 exit relationship — states this document does not itself close P9;
    only P9-I2→I3→I4 running against this locked contract can prove the
    `p9-implementation-plan.md` §8/§14 exit criteria.

No production code, no test code, no other docs/ file touched.

---

## Scenario / Risk Coverage

```text
Scenarios: 11 / 11
Required risks: 9 / 9
Unmapped scenarios: 0
Unmapped required risks: 0
```

---

## Validation

```bash
git status --short
git diff --check
git diff --stat
```

Result: only `docs/product/p9-profile-validation-contract.md` created
(untracked, new file — `git diff --stat` shows nothing against HEAD for an
untracked file, confirmed via `git status --short`); no whitespace/diff
errors; no `src/`, `tests/`, `Web`, `Razor`, `CSS` file touched.

Manual cross-checks:
- all 11 canonical scenarios from `p9-implementation-plan.md` §4 present
  exactly once,
- all 9 required risks from `p9-implementation-plan.md` §8 P9-I3 present
  exactly once and each mapped to ≥1 scenario,
- every scenario mapped to ≥1 concrete validation concern (§3 table, no
  empty cells),
- no new unsupported inference introduced — §7 restates only the
  already-locked `Unsupported`/`Deferred` boundary,
- `Evidence != Signal != Pattern` preserved — §6 reuses the existing chain
  and existing `ProfileComposer` coherence check rather than defining a new
  one,
- repository-level vs profile-level validation kept distinct (§9),
- P9-I2 can derive its fixture/harness requirements from §10 without
  inventing semantic expectations (each requirement cites the section it
  implements).

---

## Independent Review

Run via `github-proof-review`. Verdict: **PASS** — 0 BLOCKER, 0 HIGH,
0 MEDIUM, 0 LOW.

Findings summary: scope confirmed bounded (session base = HEAD, only the
new contract file + worklog files changed, no `src/`/`tests/`/
`mvp-phases.md` touched); 11 scenarios and 9 risks cross-checked
line-for-line against `p9-implementation-plan.md`; the §8.1 determinism
surface cross-checked against the real `AssertSemanticallyEquivalent`
method body (not invented); the §5 prohibited-claim checklist's 17-item
consolidation of product contract §18's 20 semantic items noted as an
intentional, non-lossy grouping (e.g. junior/mid/senior → P1,
hire/no-hire → P2) — non-blocking. No invented semantics, no canonical
contract drift, no P9/P10 scope leakage.

---

## Preservation

```text
Production code changed: NO
Tests changed: NO
P10 started: NO
Production semantics changed: NO
Commit: NOT CREATED
Push: NOT PERFORMED
```

---

## Next

If PASS: `P9-I2 — Profile Validation Fixtures / Harness` is the only
authorized next implementation slice.
