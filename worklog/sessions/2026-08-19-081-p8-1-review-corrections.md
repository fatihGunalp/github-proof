# P8-1 — Review Corrections (F1–F6)

**Date:** 2026-08-19
**Result:** DOCS — all six independent-review findings corrected. P8-1 remains
IN PROGRESS; P8 implementation remains NOT_READY.
**Scope:** docs/-only correction turn — no src/, no tests/, no implementation,
no exit gate
**Model Route:** opus / high (continuation of the approved P8-1 chain; risk
unchanged, no new gate)

---

## Goal

The P8-1 independent contract review returned PASS WITH FINDINGS with one
HIGH (F1) that required a new user + ChatGPT semantic decision, one MEDIUM
that could be closed by the same decision (F2), and four consistency findings
(F3–F6) requiring no new product/domain decision.

The user and ChatGPT have now locked the profile-level evidence-surface
accounting rule. This session records it and applies the remaining bounded
corrections. It does not run the exit gate and does not close P8-1.

---

## Verified State Before Mutation

```text
branch  session/078-p8-1-profile-composition-decision-lock
HEAD    16c04471fb5a415dd821b0ffca2bc08a332fbcec
status  Sessions 078+079+080 changes uncommitted (commit is user-owned)
P8-1    IN PROGRESS
P8      NOT_READY
src/    untouched
tests/  untouched
```

Sessions 078–081 share one uncommitted working tree on the same P8-1-scoped
branch, for the reason recorded in the Session 079 branch note.

---

## F1 — Evidence-Surface Accounting (was HIGH; required a new decision)

**Finding:** "expected evidence surface" was used four times in §13 without
being a defined set. Eligibility §22.6 produces a deterministic `Undetermined`
even when admission input is missing, and §31.2 requires it to reach
composition intact — so whether that counts as "evaluated" (→ Complete) or
"not evaluated" (→ Partial) was undecided. `IsEvidenceSurfaceComplete` existed
upstream but was neither bound nor excluded.

**Corrected in** `docs/architecture/public-profile-evidence-domain-model.md`
§13, by recording the locked decision:

1. **"Expected Evidence Surface" defined** — the set of supported and
   applicable V1 profile-analysis obligations that the canonical contracts
   require to be evaluated for the current canonical repository universe. The
   three load-bearing words (supported, applicable, required by the canonical
   contracts) are unpacked, and obligation ownership is explicitly delegated
   to the existing eligibility, pattern and representative-repository
   contracts. **No new capability list was invented.**
2. **Unsupported and Deferred are not obligations** — so
   `ProblemArea Unsupported != Partial` and
   `RoleEvidenceAlignment Deferred != Partial`. `NotApplicable` is likewise
   not an unmet obligation.
3. **Resolved vs unresolved** — `Analyze`, `Skip`, `Allowed`, `NotAllowed`,
   `NotApplicable` are deterministic terminal outcomes and count as resolved;
   a negative contribution decision is explicitly not incomplete evidence.
   `Undetermined` is a deterministic record of a gap, not an answer, and is
   unresolved. The text anchors this on eligibility §10/§14.1/§31.2, which
   already refuse to collapse `Undetermined` into `NotAllowed`.
4. **Complete / Partial / Failed rewritten on that accounting** — Complete =
   every supported and applicable obligation resolved; Partial = at least one
   unresolved and the remainder still coherent; Failed = coherent truth cannot
   be safely composed (provenance/version/repository-universe incoherence),
   with `Failed != missing evidence`.
5. **Human-language invariant** recorded verbatim.
6. **Upstream completeness** — `ProfilePatternSet.IsEvidenceSurfaceComplete`
   is a consumed canonical input, not recalculated
   (`public-profile-representative-repositories.md` §9.1/§9.2/§12.1). `false`
   → Partial when a valid profile is still producible; `true` alone does
   **not** establish Complete, because it speaks only for the surface the
   `ProfilePatternSet` was built from.

The "Limitation and Warning Do Not Determine Completeness by Count" subsection
now points at this accounting instead of the previously undefined phrase, and
carries `Warnings.Count > 0 != Partial` explicitly.

## F2 — ProfileWarning Mapping (was MEDIUM; closed by the same decision)

**Finding:** the four identities were locked but nothing said which upstream
condition maps to which, and the promotion predicate was purely qualitative.

**Corrected in** the same file, §13, keeping the four V1 identities exactly as
already locked (none added, none removed):

- `RepositoryEvidenceRateLimited` — canonical upstream condition is
  `RateLimited`.
- `RepositoryEvidenceUnavailable` — an availability condition **other than**
  `RateLimited`; explicitly not a generic duplicate of the rate-limited
  identity.
- `RepositoryAnalysisFailed` — a required/attempted repository analysis failed
  technically and that failure affects the expected evidence surface; anchored
  on the existing eligibility §8 "Failed" boundary that a technical failure is
  not evidence absence.
- `RepositoryEligibilityUndetermined` — a material admission or
  purpose-specific final eligibility decision remains `Undetermined`
  (eligibility §6.4, §10, §31.2).

Also added: **one event may produce several identities** when they represent
different semantic facts (worked example: `RateLimited` +
`EligibilityUndetermined`), with the existing deduplication rule explicitly
still binding — the same identity may not repeat for the same canonical scope.
And **warnings describe conditions, not completeness**: `AnalysisStatus` is
derived independently from the accounting, so `Warnings.Count > 0 != Partial`.

## F3 — Snapshot Capture Time Is Not Audit-Only (was MEDIUM)

**Finding:** §14 claimed two results from the same `SnapshotIdentity` and rule
versions are semantically equal, while Decision B made `SnapshotCapturedAt` a
semantic input to eligibility — so the same identity at different capture
times can legitimately yield different `Active`/`Historical` classifications.
§16 still called snapshot capture timestamps audit metadata.

**Corrected in** the same file:

- §14 "Audit Timestamp Boundary" — the equality claim is narrowed to
  `StartedAt`/`CompletedAt`. A new "SnapshotCapturedAt Is Not Audit-Only"
  subsection records that `CapturedAt` is excluded from `SnapshotIdentity`
  yet is a semantic input to temporal eligibility (§19.2, §19.3.1), so
  `same SnapshotIdentity + same rule versions != guaranteed identical
  result`, and `SnapshotIdentity` alone must not be a cache/idempotency key
  for a complete profile result. Determinism is explicitly restated as
  unchanged — same normalized inputs including `CapturedAt`, same rule
  versions, same result.
- §16 "Time Boundary" — snapshot capture time removed from the audit-only
  list, with a paragraph recording that its influence is contracted and
  deterministic rather than a hidden clock read, and that it remains excluded
  from `SnapshotIdentity`.

## F4 — CapturedAt Exclusion Is by Meaning, Not Field Name (was LOW)

**Corrected in** §6: the exclusion now explicitly covers the normalized
snapshot's own audit capture-time field, and any field expressing "when this
snapshot was observed" under any name, on the snapshot, the reference or any
intermediate normalized form.

## F5 — Enumerated Ordering Keys for Limitations and Warnings (was LOW)

**Corrected in** §13 "Structured Identity, Ordering and Deduplication": a new
"Canonical Ordering Keys" subsection enumerates the order — semantic code
ordinal, then scope ordinal, then affected canonical identities compared by
their own canonical ordering, element by element. It records why the three
keys are jointly total (deduplication already forbids two entries sharing all
three), so no further tie-breaker is needed. It mirrors the pattern
contract's ordering discipline (`public-profile-evidence-patterns.md` §12)
without modifying it.

## F6 — Roadmap Wording (was LOW)

**Corrected in** `docs/roadmap/mvp-phases.md`: item 3 no longer claims an
enumerated `SnapshotIdentity` "input set" is locked. It now states what is
actually locked — derivation from the canonical normalized snapshot semantic
representation, exclusion of observation time under any name, the fixed
prohibited-input list, and the ban on a composer-owned second field inventory
— and records that an enumerated field list is deliberately not locked
because semantic field ownership belongs to the canonical snapshot
representation.

---

## New Semantic Gaps Discovered

None. Applying the locked accounting rule closed F1 and F2 without exposing a
further undecided question, and F3–F6 required no product or domain decision.

No new capability, warning identity or limitation identity was created, and no
previously locked decision was reopened.

---

## Validation

- `git diff --check`: clean.
- `git status --short`: 5 modified canonical docs + 1 new canonical doc
  (report contract, from Session 079) + CURRENT.md + INDEX.md + 4 session
  files. No `src/`, no `tests/`.
- `git status --short -- src/ tests/`: empty.
- F1–F6 each verified present in the recorded owner.
- `Complete`/`Partial`/`Failed` implementable without semantic guessing:
  every branch now resolves to a defined accounting outcome.
- `Undetermined` separated from deterministic terminal outcomes.
- `ProfilePatternSet.IsEvidenceSurfaceComplete` consumed, not recalculated,
  and the asymmetry (false → Partial; true alone insufficient) recorded.
- Warning identities remain exactly four.
- `Warnings.Count > 0 != Partial` stated in two places.
- Unsupported ProblemArea and Deferred RoleEvidenceAlignment explicitly do not
  affect completeness.
- P8-1 `IN PROGRESS` and P8 implementation `NOT_READY` unchanged everywhere.

---

## Repository Mutation

```text
src/            UNCHANGED
tests/          UNCHANGED
docs/           architecture/public-profile-evidence-domain-model.md
                roadmap/mvp-phases.md
worklog/        this session file + CURRENT.md + INDEX.md
Git             no commit, no push, no merge — user-owned
```

Only two canonical files needed changing: F1–F5 all belong to the domain
model, and F6 to the roadmap. The eligibility, report and product contracts
were re-read and required no edit — each already carried the rules the
corrections anchor on.

## Next Action

```text
P8-1 Exit Gate and Finalization
```

All six review findings are corrected. P8-1 stays IN PROGRESS and P8
implementation stays NOT_READY until the exit gate is actually run and passes.

No commit, no push — user-owned per CLAUDE.md.
