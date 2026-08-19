# P8-I1 — Final Repository Eligibility Runtime

**Date:** 2026-08-19
**Result:** PASS — P8-I1 slice complete. Additive only; P5 provably unchanged.
**Scope:** Domain implementation of the final repository eligibility runtime —
no Application, no Web, no canonical docs change
**Model Route:** opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Verified Baseline

```text
branch   session/084-p8-i1-final-repository-eligibility-runtime
base     89361e6  docs(profile): close P8-1 composition and report contracts
P8-1     COMPLETE
P8       READY
```

Session 083's worklog changes (`CURRENT.md`, `INDEX.md`, the 083 session file)
were still uncommitted at session start and carry over into this branch, since
commit is user-owned. They are Session 083 planning content and make no claim
about this slice.

---

## Additive Composition — the Decision Session 083 Left Open

Session 083 recorded as an open technical choice how the final
`RepositoryEligibilityResult` should relate to the existing narrower
`RepositoryContributionAssessment`, which is load-bearing inside P5
(`CrossRepositoryEvidenceComposer` constructs `RepositoryContributionPolicy`
and `CrossRepositoryEvidence` stores the assessments).

**Chosen: composition.** `RepositoryEligibilityResult` holds a
`RepositoryContributionAssessment` and adds the remaining purposes plus the
derived status; `RepositoryEligibilityPolicy` delegates to the existing
`RepositoryContributionPolicy` for `ProfileObservation` and
`IndependentRepetition` rather than reimplementing them.

Extending the existing type was never required, so the STOP condition in the
task did not trigger. The result is stronger than the plan predicted: Session
083 anticipated changes to four existing files
(`RepositoryContributionAssessment`, `RepositoryContributionPolicy`,
`CrossRepositoryEvidence`, `CrossRepositoryEvidenceComposer`); **none was
touched.** The P8-I1 exit criterion "P5 behaviour provably unchanged" is
therefore satisfied structurally, not just by test outcome.

---

## Added Types

All new; no existing `src/` or `tests/` file was modified.

| File | Responsibility |
|---|---|
| `Domain/Profile/Enums/ContributionPurpose.cs` | The five canonical purposes. `ProfileContext` is context-only; the other four are evidence-bearing. |
| `Domain/Profile/Enums/RepositoryEligibilityStatus.cs` | The four overall status values. `Undetermined = 0` is the conservative default. |
| `Domain/Profile/Enums/ActivityClassification.cs` | `Active` / `Historical` / `Undetermined`, a recency classification only. |
| `Domain/Profile/ValueObjects/EligibilityPolicy.cs` | Binds an `EligibilityPolicyVersion` to its immutable `CurrentActivityWindow`. `V1` = `"1.0.0"` + 365 days. Private constructor; `Create` validates against a canonical registry. `ResolveActivityCutoff` anchors the cutoff to the snapshot capture time. |
| `Domain/Profile/Models/RepositoryEligibilityResult.cs` | The final result. Composes the contribution assessment, adds `CurrentFocus` / `HistoricalEvidence`, carries `Archived` and `Activity` for traceability, and derives `Status` in the constructor. |
| `Domain/Profile/RepositoryEligibilityPolicy.cs` | Completes Stage B: delegates the two P5 purposes, classifies activity, resolves the remaining purposes. |
| `tests/.../EligibilityPolicyTests.cs` | Policy binding, window validation, cutoff anchoring, canonical uniqueness. |
| `tests/.../RepositoryEligibilityPolicyTests.cs` | Lifecycle branches, cutoff boundaries, purpose surface, status derivation, determinism, guards. |

### Contract Mapping

- **§19.2 / §19.3 / §19.3.1** — the window is reachable only through a version
  that owns it; the cutoff is `SnapshotCapturedAt − window`; no system clock;
  a missing `LastPushedAt` yields `Undetermined`, never an optimistic
  `Active`; `CreatedAt` is never read, so repository age is irrelevant.
- **§22.1–§22.6** — baselines derived from the observation contribution:
  `NotAllowed` (empty / insufficient) propagates its own reason to the other
  evidence-bearing purposes; `Undetermined` (failed / unadmitted) leaves them
  unresolved except where a manifest-only restriction resolves first.
- **§24** — all four branches: `Archived` → `NotAllowed(Archived)` even when
  activity is `Undetermined`; `Historical` → `NotAllowed(Historical)`;
  `Absent + Active` → `Allowed`; otherwise `Undetermined`.
- **§26** — `HistoricalEvidence` reads only the evidence surface. Lifecycle is
  not even passed to its resolver, so archived or historical state cannot
  invalidate it, and it is not mutually exclusive with `CurrentFocus`.
- **§27** — manifest-only restrictions resolve before the unresolved-surface
  fallback (principle 3); `Allowed` requires observation, archive state and
  activity all known (principle 5).
- **§14.1–§14.5** — `Status` is a get-only property computed in the
  constructor: `Undetermined` short-circuits, then `NonContributing` when
  nothing is allowed, then `Restricted` when something is also not allowed,
  else `Contributing`. `NotApplicable` sets no flag. `ProfileContext` is
  excluded from derivation.
- **§31.2 / §31.3 / §31.4** — `Undetermined` survives every path; the status
  cannot be set independently; reference, policy version, admission, analysis
  reference, archive condition, activity and per-purpose reasons are all
  preserved, with no quality, capability, seniority, employability or ranking
  semantics anywhere.

---

## Technical Choices Made

1. **`EligibilityPolicy.V1` = `"1.0.0"`** — no production
   `EligibilityPolicyVersion` value existed anywhere in `src/` before this
   slice (only test fixtures), so this establishes the first canonical runtime
   value, following the `RepresentativeRepositoryRuleVersion.V1 = "1.0.0"`
   precedent from Session 075. Versioning choice, not a semantic one.
2. **Binding shape: private constructor + `Create` factory + canonical
   registry** — a canonical version cannot be rebound to a different window.
   This closes the §19.3 uniqueness requirement at the type level rather than
   by convention.
3. **Composition over extension** — recorded above.
4. **`ProfileContext` is not a constructor parameter** — all six canonical
   baselines require `Allowed`, so accepting it as input would have permitted a
   state the contract never defines.

### Deliberate Trade-Off

`Create` enforces uniqueness only for canonical versions. Two calls with the
same non-canonical version and different windows both succeed. Enforcing this
globally would require a process-wide mutable registry, which is worse for
determinism than the gap it closes; the production path uses only `V1`. Noted
rather than fixed.

---

## Validation

```text
build     GitHubProof.slnx — 0 warning / 0 error
tests     Domain        726 / 726   (676 baseline + 50 new)
          Infrastructure 255 / 255  (unchanged)
          Application    101 / 101  (unchanged — P5 regression clean)
          Web             68 /  68  (unchanged)
          total         1150 / 1150
git       diff --check clean; no existing src/ or tests/ file modified
```

Test coverage spans the three cutoff boundary cases (just above, exactly at,
just below — the cutoff itself is `Active`), missing-timestamp `Undetermined`,
repository-age irrelevance, window ownership by policy version, all four §24
lifecycle branches, `Active + Archived`, the §22 baselines including
`Archived + Failed` leaving `HistoricalEvidence` `Undetermined`, all five
purposes, every status value including the `Allowed + NotAllowed +
Undetermined` precedence case, determinism across repeated evaluation and
across capture-time offset representation, and the constructor guards.

---

## Independent Review

**Round 1: PASS WITH FINDINGS** — 2 MEDIUM, 3 LOW, no BLOCKER/HIGH, no
(B)-type finding. All accepted and corrected:

- **MEDIUM-1** — the policy version → window binding was not unique, and a
  test bound canonical `"1.0.0"` to 180 days. Fixed by the private
  constructor + `Create` + canonical registry described above; the offending
  test was replaced by four that assert the uniqueness rule instead.
- **MEDIUM-2** — the result could be constructed with `CurrentFocus` or
  `HistoricalEvidence` `Allowed` while `ProfileObservation` was not. Fixed by
  constructor guards mirroring the sibling guard in
  `RepositoryContributionAssessment`.
- **LOW-1** — the §22.5/§22.6 "otherwise Undetermined" arm and the §22.3
  `Partial` row were exercised but not asserted. Three tests added.
- **LOW-2** — §17 traceability: the archive condition was absent from the
  result, so two different causes of an `Undetermined` `CurrentFocus` were
  indistinguishable. `Archived` is now on the result.
- **LOW-3** — `CURRENT.md` still named the Session 083 branch.

**Round 2: PASS WITH FINDINGS** — 0 BLOCKER, 0 HIGH, 0 MEDIUM, 3 LOW, no
(B)-type finding. It confirmed all five round-1 findings genuinely closed,
re-verified §22.1–§22.6 and §24 against the new constructor guard (no
canonical baseline requires a state the guard rejects), confirmed the registry
blocks no policy the contract allows, confirmed P5 byte-identical, and
independently reproduced the build and 1150 test results.

Round-2 findings, all type (A):

- non-canonical version uniqueness not enforced — recorded above as a
  deliberate trade-off;
- `ProfileContext` was injectable without a guard — fixed by removing the
  parameter entirely;
- the worklog did not yet record Session 084 — closed by this file and the
  `CURRENT.md` / `INDEX.md` updates.

---

## Out of Scope — Confirmed Absent

No profile root, no `ProfileLimitation` / `ProfileWarning`, no composer, no
report or projector, no orchestration wiring, no canonical `docs/` change. The
new result type is referenced only from within Domain.

## Next Action

```text
P8-I2 — Profile Root Foundation
```

`RepositoryEligibilityResult` is now available as the root's
`RepositoryEligibilityResults[]` member. Per the Session 083 plan, P8-I2's
`SnapshotReference` / limitation / warning work has no hard dependency on this
slice. P8-I2 is a separate meaningful session and must be routed through its
own model/effort gate.

No commit, no push — user-owned per CLAUDE.md.
