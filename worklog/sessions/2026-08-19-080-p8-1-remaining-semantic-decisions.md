# P8-1 — Remaining Semantic Decisions (Canonical Mutation)

**Date:** 2026-08-19
**Result:** DOCS — six locked semantic decisions recorded in their canonical
owners. P8-1 remains IN PROGRESS; P8 implementation remains NOT_READY.
**Scope:** docs/-only canonical mutation — no src/, no tests/, no
implementation, no P8-1 completion review
**Model Route:** opus / high (continuation of the approved Session 079 route;
risk unchanged, no new gate required)

---

## Goal

Session 079's independent review left four open semantic decisions plus one
inherited prerequisite. The user and ChatGPT have now resolved all of them.

This session records those decisions in their canonical owners and nothing
else. It explicitly does not run the P8-1 completion review, does not mark
P8-1 complete, and does not authorize implementation.

---

## Verified State Before Mutation

```text
branch  session/078-p8-1-profile-composition-decision-lock
HEAD    16c04471fb5a415dd821b0ffca2bc08a332fbcec
status  Sessions 078+079 changes uncommitted (commit is user-owned)
P8-1    IN PROGRESS
P8      NOT_READY
src/    untouched
tests/  untouched
```

Next session number confirmed from `worklog/sessions/` contents: 079 is the
latest, so this is 080. Sessions 078, 079 and 080 share one uncommitted
working tree on the same P8-1-scoped branch, for the reason recorded in the
Session 079 branch note.

---

## Where Each Locked Decision Was Recorded

| # | Decision | Canonical owner | Section |
|---|---|---|---|
| A | SnapshotIdentity — `CapturedAt` excluded; derived from the canonical normalized snapshot semantic representation; no composer-owned field inventory | domain model | §6 "SnapshotReference" → "CapturedAt Does Not Participate", "Canonical Snapshot Contract Owns the Semantic Content" |
| B | CurrentActivityWindow = exactly 365 days, recency boundary | eligibility contract | new §19.3.1 (+ §31.5 prerequisite note updated) |
| C | LimitedIndependentEvidenceSurface emission condition | domain model | §13 "LimitedIndependentEvidenceSurface Emission Condition" |
| D | V1 ProfileWarning identity set | domain model | §13 "Canonical V1 Warning Taxonomy" |
| E | Version-lineage compatibility = lineage coherence, not equal version strings; incoherence → Failed | domain model | §14 "What 'Compatible' Means", "Failure Boundary" |
| F | Unsupported ProblemArea not rendered by default; structured truth retains Unsupported | product contract §24 (owner) + report contract §6.5 (cross-reference) | as noted |

### A — SnapshotIdentity

§6 now states the member split explicitly (`CapturedAt` = when was it
observed; `SnapshotIdentity` = what canonical normalized semantic snapshot was
observed), then locks that `CapturedAt` must not participate in
`SnapshotIdentity`, with the consequence spelled out: the same normalized
semantic snapshot observed at two different times yields the same
`SnapshotIdentity` and a different `CapturedAt`. `CapturedAt` remains a
required `SnapshotReference` member and remains available to consumers,
including the eligibility activity rules that anchor on `SnapshotCapturedAt`.

A second subsection locks that identity derivation consumes the canonical
normalized snapshot semantic representation the snapshot contract already
owns, and prohibits a parallel hand-maintained field inventory in the
composer, which would drift and silently change identity semantics without a
contract change.

The pre-existing prohibited-derivation-input list and the
provenance-dimension separation are unchanged.

### B — CurrentActivityWindow

New eligibility §19.3.1 locks the Profile V1 value at exactly 365 days,
immutably owned by the `EligibilityPolicyVersion` that carries it, changeable
only by a new policy version — exactly as §19.3 already required.

It is stated explicitly as a recency boundary, not a history-length,
minimum-age or maturity requirement, with the worked consequence that
repository age is irrelevant: a two-week-old repository with in-window
source-control activity is `Active`, a ten-year-old repository whose last
source-control activity precedes the cutoff is `Historical`.

The application restates the §19.2 reference unchanged (cutoff =
`SnapshotCapturedAt` − 365 days; `>=` → Active; `<` → Historical; missing
required timestamp → Undetermined), and a "Preserved Boundaries" subsection
re-anchors: general metadata timestamps may not substitute for source-control
activity, no system clock participates, unavailable timestamps never fall
back to Active, and Archived remains an independent condition that may still
force `CurrentFocus = NotAllowed` via §19.4.

§31.5's note that this prerequisite was missing is updated to record that it
is now satisfied.

### C — LimitedIndependentEvidenceSurface

§13 now locks the emission condition: emit when the count of canonical
repositories with `IndependentRepetition = Allowed` is below the canonical
minimum repository count required for repetition promotion.

That minimum is deliberately **not** restated as a new number here. It
references the already-locked repetition threshold in
`public-profile-evidence-patterns.md` §5 (2 under the current V1
`PatternRuleVersion`), and the text explicitly forbids introducing a separate
limitation threshold. So 0 or 1 → emit; 2 or more → do not emit on
repository-count grounds.

A subsection locks that empty patterns are not this limitation: with 2+
independent repositories and no promoted pattern, the limitation must not be
emitted merely because the pattern collections are empty, because that would
convert an ordinary empty result on an adequate surface into an apparent
evidence deficiency — already prohibited by patterns.md §6.

### D — V1 ProfileWarning Identities

§13 now locks exactly four identities: `RepositoryEvidenceUnavailable`,
`RepositoryEvidenceRateLimited`, `RepositoryAnalysisFailed`,
`RepositoryEligibilityUndetermined`. No additional V1 identity without a
versioned contract change.

Each carries the structured scope and affected canonical identities already
required by the structured-identity rules, so a warning always states which
repositories or canonical results it concerns. An explicit sentence records
that none of the four is a statement about the developer.

The pre-existing promotion boundary (promote only when profile completeness,
provenance or safe interpretation is materially affected; repository-level
warnings are not copied upward) and the count-independence rule (warning
count does not determine `AnalysisStatus`) are unchanged and were already
locked in Session 079.

### E — Version-Lineage Compatibility

§14 "Analyzer Version Ownership" now defines compatibility as a property of
the analysis lineage rather than of version literals, opening with the
explicit statement that compatible does not mean equal version strings and
that different semantic layers legitimately advance independently.

Five requirements are locked: same snapshot provenance chain, same canonical
repository universe, explicit upstream/downstream version provenance, no
silent cross-run mixing, and independently visible version dimensions (a
flattened combined token does not satisfy this).

A "Failure Boundary" subsection restates that unproven coherence yields
`Failed`, not `Partial`, cross-referencing §14 Result Completion Boundary and
§13 AnalysisStatus Ownership Boundary rather than restating those rules.

### F — Unsupported ProblemArea Presentation

The product decision is recorded in its product owner, product contract §24:
the default Milestone B user-facing report does not render the
`Problem Area Patterns` section while availability is `Unsupported`.

Report contract §6.5 is updated from "this decision is open and implementation
may not default it" to record that both non-supported V1 sections are now
sanctioned for omission, plus two new bounded rules: structured/typed report
truth still carries `Availability = Unsupported` (and `Deferred` for
RoleEvidenceAlignment) so presentation may decline to render but may never
delete the capability state; and `Unsupported`/`Deferred` may never be
presented as "no problem areas found", "no role match", a misleading empty
card, or any negative developer conclusion.

`Unsupported` and `Deferred` remain distinct in both the typed contract and
presentation.

---

## Conflicts Found

None. No locked decision contradicted a higher-authority canonical contract.

Two decisions were deliberately expressed as references to already-locked
rules rather than as new normative statements, to avoid creating a second
source of truth:

- decision C references the P6 repetition threshold instead of restating a
  number;
- decision E references the existing Failed-not-Partial boundary instead of
  restating it.

---

## What This Session Did Not Do

- No new semantic decision was invented; every rule recorded here traces to
  the locked handoff.
- No previously locked decision was reopened.
- No P8-1 completion review was run.
- P8-1 was not marked complete; implementation was not authorized.
- No `src/`, `tests/`, runtime code, hashing, eligibility, composition or
  report-projection implementation.

The three "Deliberately Not Locked in V1" subsections added in Session 079
(§6, §13, §14) were removed, because the decisions they recorded as open are
now locked. That is the intended lifecycle of those notes, not a weakening.

---

## Validation

- `git diff --check`: clean, no whitespace errors.
- `git status --short`: 5 modified canonical docs + 1 new canonical doc
  (Session 079's report contract, still untracked) + 2 worklog files + 3
  session files. No `src/`, no `tests/`.
- `git status --short -- src/ tests/`: empty.
- `git diff --stat`: additive apart from the intended replacements (the three
  "Deliberately Not Locked" notes, the §31.5 prerequisite note, the
  mvp-phases open-decision list, product contract §24 item 2).
- All six locked decisions verified present in their recorded owners.
- P8-1 status re-grepped across docs/ and worklog/: `IN PROGRESS` /
  `NOT_READY` everywhere, no `COMPLETE` or `READY` claim introduced.

---

## Repository Mutation

```text
src/            UNCHANGED
tests/          UNCHANGED
docs/           architecture/public-profile-evidence-domain-model.md
                architecture/public-profile-evidence-repository-eligibility.md
                architecture/public-profile-evidence-report-contract.md
                product/public-profile-evidence-product-contract.md
                roadmap/mvp-phases.md
worklog/        this session file + CURRENT.md + INDEX.md
Git             no commit, no push, no merge, no branch created — user-owned
```

## Next Action

```text
P8-1 Independent Contract Review
```

P8-1 stays IN PROGRESS and P8 implementation stays NOT_READY until that
review passes without a BLOCKER or HIGH finding.

No commit, no push — user-owned per CLAUDE.md.
