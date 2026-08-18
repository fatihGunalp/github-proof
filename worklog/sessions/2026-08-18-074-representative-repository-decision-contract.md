# Representative Repository Decision + Contract

**Date:** 2026-08-18
**Result:** PASS
**Scope:** docs
**Model Route:** sonnet / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Lock the deterministic V1 rule for representative-repository selection: for a
specific canonical `RepeatedProfilePattern`, which repository a reviewer
should inspect first, and why.

---

## Starting State

~~~text
Branch:
session/074-representative-repository-decision-contract

Base HEAD:
72932bb

Pre-existing Working Tree:
CLEAN

Relevant Previous Session:
worklog/sessions/2026-08-18-073-dependency-source-reference-detector.md
~~~

---

## Model Route

Gate sonucu:

~~~text
Selected:
sonnet / high

Why:
Documentation-only session; the decision space was already resolved by the
user handoff (canonical A, candidate set, comparison, tie-break, version
ownership). Remaining risk is invariant density — preventing hidden ranking,
fork/template inflation, cross-pattern breadth leakage, hidden global K and
RoleEvidenceAlignment scope drift from entering the contract text.

User Approval:
devam et
~~~

---

## Model Escalation

None.

---

## Context Read

~~~text
CLAUDE.md
AGENTS.md
docs/architecture/public-profile-evidence-domain-model.md (§7-11, §15, §18)
docs/architecture/public-profile-evidence-patterns.md
docs/architecture/public-profile-evidence-repository-eligibility.md (Fork /
  TemplateDerived / Archived / IndependentRepetition / Undetermined sections)
docs/roadmap/mvp-phases.md (P7 section)
worklog/CURRENT.md
worklog/sessions/TEMPLATE.md
worklog/README.md
src/GitHubProof.Domain/Profile/Models/RepeatedProfilePattern.cs
src/GitHubProof.Domain/Profile/Models/ObservedProfileEvidence.cs
src/GitHubProof.Domain/Profile/ValueObjects/ProfileEvidenceReference.cs
src/GitHubProof.Domain/Profile/ValueObjects/ProfileRepositoryReference.cs
src/GitHubProof.Domain/Profile/ValueObjects/PatternRuleVersion.cs
src/GitHubProof.Domain/Profile/ValueObjects/ObservationRuleVersion.cs
src/GitHubProof.Domain/Profile/ValueObjects/EligibilityPolicyVersion.cs
~~~

---

## Contract / Invariants Checked

~~~text
- RepeatedProfilePattern.IndependentSupportingRepositories already excludes
  Fork/TemplateDerived lineage and Undetermined independence (P2/P5 locked).
- Archived repository state blocks CurrentFocus only, not
  IndependentRepetition (repository-eligibility.md §19.4) — confirms archived
  repositories are not automatically excluded as candidates.
- ObservedProfileEvidence.EvidenceReferences already enforce
  EvidenceKey == ObservationKey and one reference per repository — the basis
  for PatternSupportObservationCount.
- ProfileRepositoryReference.CompareTo already implements Owner-then-Name
  ordinal ordering — reused verbatim as the tie-break, no new heuristic.
- RepresentativeRepository must not exceed the epistemic strength of the
  pattern that selected it (domain-model.md §11, §15 Representative
  Repository Integrity).
- RoleEvidenceAlignment remains out of scope (domain-model.md §12, §18 P7).
~~~

---

## Changes

~~~text
Created:
- docs/architecture/public-profile-representative-repositories.md
  (CONTRACT LOCKED, Version 1)
- worklog/sessions/2026-08-18-074-representative-repository-decision-contract.md

Modified:
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-18-073-dependency-source-reference-detector.md
  (minimal Git-transition correction only; see Decisions)

Deleted:
- None
~~~

---

## Decisions

~~~text
- Canonical V1 selection unit: one representative repository per
  RepeatedProfilePattern (option A from the handoff). Global bounded set (B)
  and minimum-set/maximum-coverage (C) are explicitly excluded from V1; B may
  exist later only as a presentation/deduplication projection computed over
  already-completed pattern-specific selections, never as an independent
  mechanism.
- CandidateRepositories = pattern.IndependentSupportingRepositories, consumed
  as-is from P5/P6; no independence re-evaluation performed here.
- PatternSupportObservationCount defined as the count of the pattern's
  SupportingObservations for which the candidate holds a positive
  ProfileEvidenceReference; higher sorts first; explicitly scoped to one
  pattern and never combined across patterns.
- Tie-break reuses ProfileRepositoryReference ordering (Owner ordinal, then
  Name ordinal) — no new heuristic introduced.
- Cross-pattern breadth explicitly forbidden as a selection factor (§8 of the
  new contract); the same repository may still independently win multiple
  patterns, which is a different and permitted outcome.
- Dedicated RepresentativeRepositoryRuleVersion introduced; does not reuse
  PatternRuleVersion / ObservationRuleVersion / EligibilityPolicyVersion.
- RoleEvidenceAlignment semantics explicitly out of scope (§14 of the new
  contract); no role/language inference introduced.
- Corrected worklog/CURRENT.md and the Session 073 file to record the
  already-verified fact that Session 073 is committed, pushed, integrated
  into main and remotely verified at 72932bb. No Session 073
  implementation/review history was rewritten.

This session introduces a new canonical architecture contract. It is recorded
in docs/architecture/public-profile-representative-repositories.md, which is
the permanent source of truth; this worklog entry is operational memory only.
~~~

---

## Validation

~~~text
git diff --check
PASS — no whitespace errors

git status --short
Reviewed — only the files listed under Changes are touched

Manual invariant review against docs/architecture/public-profile-evidence-
domain-model.md and docs/architecture/public-profile-evidence-patterns.md:
PASS — no rank/score/quality/strength/confidence language; fork/template
cannot inflate candidacy; archived is not auto-excluded; cross-pattern breadth
explicitly forbidden as a selection factor; no hidden global K; incomplete/
undetermined evidence remains bounded per §9; traceability chain matches
domain-model.md §11 direction; RepresentativeRepositoryRuleVersion is
dedicated; RoleEvidenceAlignment did not enter scope.

dotnet build / dotnet test
NOT RUN — documentation-only session, no production code changed

Independent review (github-proof-review, forked context):
PASS WITH FINDINGS. All 12 falsification targets verified with no violation
(hidden ranking, fork/template inflation, cross-pattern breadth, tie-break
determinism, hidden global K, incomplete/undetermined-evidence handling,
traceability isolation, B/C leakage, RoleEvidenceAlignment drift, archived
exclusion, dedicated RuleVersion, worklog-fact-only correction — all
confirmed against both the contract text and the actual P5/P6 Domain models).

One MEDIUM finding: §9 referenced "the pattern's own
UndeterminedEvidenceRepositoryCount and EligibleRepositoryCount" as if
RepeatedProfilePattern carried those fields directly; it does not — they live
per-observation on ObservedProfileEvidence, and UndeterminedEvidenceRepositoryCount
can differ across a multi-signal pattern's SupportingObservations (for
example, the documentation pattern's two mapped signals). Not a BLOCKER/HIGH
and did not affect the locked §3-§8/§11-§13 decisions, but was corrected in
this session for precision: §9 and §11 now state explicitly that completeness
is read per-observation from SupportingObservations via the ObservationKey
references already carried in RepresentativePatternSupport, rather than a
new invented pattern-level field.
~~~

---

## Problems

None.

---

## Not Done

~~~text
- No production RepresentativeRepository/RepresentativePatternSupport code
  implemented (explicitly out of scope for Session 074).
- No RoleEvidenceAlignment semantics defined.
- No global bounded representative set / presentation projection defined.
- Session 075 not started.
~~~

---

## Known Risks

~~~text
- None identified beyond the standard risk that any future
  RepresentativeRepository implementation must be re-validated against this
  contract before it is treated as authoritative.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md
- worklog/sessions/2026-08-18-073-dependency-source-reference-detector.md

Untracked (new):
- docs/architecture/public-profile-representative-repositories.md
- worklog/sessions/2026-08-18-074-representative-repository-decision-contract.md

Pre-existing unrelated:
- None
~~~

---

## Next

Session 075 may begin production implementation of
`RepresentativeRepository`/`RepresentativePatternSupport` per the locked
contract, or continue toward Role Evidence Alignment decision work — neither
is started by this session.

---

## Git

~~~text
Suggested commit:
docs: lock representative repository selection contract

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı veya Result doğru final state'e getirildi
[x] Targeted validation çalıştı
[x] Gerekliyse broader validation çalıştı (N/A — docs only)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[ ] Kullanıcıya kısa final rapor verildi (bu session mesajında yapılacak)
~~~
