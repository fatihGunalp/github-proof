# Representative Repository Decision + Contract

**Date:** 2026-08-18
**Result:** PASS — corrected across three rounds of post-push independent
review (see "Post-Push Independent Review — Round 1", "— Round 2" and
"— Round 3" below). Session contract work is complete; Git branch/main
integration is user-owned and must be read from actual Git state, not this
field.
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

## Post-Push Independent Review — Round 1

Session 074 was committed and pushed by the user as `a034aaa` ("docs: lock
representative repository selection contract"). A first post-push independent
review of that pushed state found two BLOCKER findings against
`docs/architecture/public-profile-representative-repositories.md`. Both were
corrected in this same session, on the same branch, without a new session.
The correction was itself subsequently committed and pushed by the user as
`efed302` ("docs: correct representative repository completeness and version
semantics") — a verified Git fact, not a prediction.

~~~text
BLOCKER 1 — COMPLETENESS

Finding:
§9 relied only on per-observation EligibleRepositoryCount /
UndeterminedEvidenceRepositoryCount for completeness. The existing canonical
runtime already carries a coherent evidence-surface completeness flag,
ProfilePatternSet.IsEvidenceSurfaceComplete (propagated directly from
CrossRepositoryEvidence.IsEvidenceSurfaceComplete by
ProfilePatternPromotionPolicy.Promote), which the contract had not accounted
for. Per-observation UndeterminedEvidenceRepositoryCount is a different,
narrower semantic and cannot substitute for it.

Correction:
§9 was restructured into §9.1 (Coherent Source Set) and §9.2 (Completeness
Preservation). Representative-repository selection now explicitly consumes
one coherent ProfilePatternSet as input and must preserve
ProfilePatternSet.IsEvidenceSurfaceComplete, either directly on the new
RepresentativeRepositorySelection result boundary (§11) or through an
explicit stable reference to the source ProfilePatternSet. No new
completeness metric was invented; per-observation
UndeterminedEvidenceRepositoryCount remains observation-scoped and is not
collapsed into a new field.

(The "explicit stable reference to the source ProfilePatternSet" option
introduced here was itself found undefined in Round 2 below and removed;
§9.2 now requires only the direct-value option.)

BLOCKER 2 — VERSION + TRACEABILITY

Finding:
§12 incorrectly claimed EligibilityPolicyVersion was available "transitively
through the pattern." In the actual runtime, RepeatedProfilePattern owns only
PatternRuleVersion and ObservationRuleVersion; EligibilityPolicyVersion and
IsEvidenceSurfaceComplete exist only on the containing ProfilePatternSet.
Family+PatternKey identity was also not explicitly scoped to one coherent
source set, leaving room to read a pattern identity as portable across sets.

Correction:
§12 gained §12.1 (ProfilePatternSet as the Coherent Anchor), stating exactly
which version fields ProfilePatternSet owns versus RepeatedProfilePattern, and
requiring selection to consume one coherent ProfilePatternSet and reject a
run that mixes patterns from different sets or version combinations. §10
gained §10.1 (Exact Pattern Identity): Family+PatternKey identify a pattern
only within one explicitly identified source ProfilePatternSet, mirroring
ProfilePatternSet's own (Family, PatternKey) uniqueness invariant — no
duplication of pattern truth. A new top-level `RepresentativeRepositorySelection`
shape was introduced in §11 as the single coherent result boundary carrying
SourcePatternSet (a stable reference, not a re-embedded copy),
PatternRuleVersion, ObservationRuleVersion, EligibilityPolicyVersion,
IsEvidenceSurfaceComplete and RepresentativeRepositoryRuleVersion in one
place, so Session 075 does not have to invent where these come from.

(The `SourcePatternSet` "stable reference" field introduced here was itself
found to be an undefined/fictional identity in Round 2 below and was removed;
§10.1/§11/§12.1 now use only direct value-carrying plus in-execution pattern
lookup — see Round 2 for the exact replacement wording.)

Also corrected: worklog/CURRENT.md previously implied a runtime
RepresentativeRepositoryRuleVersion value ("1 — established by Session 074").
Corrected to state explicitly that Contract Version 1 is a document version
only and no runtime rule-version value exists until implementation.

KEPT LOCKED, UNCHANGED:
one representative per RepeatedProfilePattern; candidates =
IndependentSupportingRepositories; PatternSupportObservationCount; Owner→Name
tie-break; no global K; no set-cover; no cross-pattern breadth advantage;
fork/template restrictions; archived not automatically excluded; no
rank/score/quality/strength/confidence/capability/seniority/employability;
RoleEvidenceAlignment out of scope.

Re-review (github-proof-review, forked context, focused specifically on the
five points above plus worklog-fact accuracy):
PASS. IsEvidenceSurfaceComplete preservation, PolicyVersion ownership, exact
RepeatedProfilePattern traceability, no duplicate pattern truth and no
ranking/global-coverage regression all confirmed directly against
ProfilePatternSet.cs/RepeatedProfilePattern.cs/ProfilePatternPromotionPolicy.cs
and against the actual `git diff a034aaa` (only §1, §9-§13, §17 changed;
§2-§8, §14-§16 untouched). worklog/CURRENT.md's runtime-value wording and the
session file's addition-only correction were both confirmed accurate. No
BLOCKER/HIGH/MEDIUM finding.
~~~

---

## Post-Push Independent Review — Round 2

The Round 1 correction was committed and pushed by the user as `efed302`. A
second post-push independent review of that pushed state found one remaining
contract gap in `docs/architecture/public-profile-representative-repositories.md`.
Corrected in this same session, on the same branch, without a new session.
The correction was itself subsequently committed and pushed by the user as
`3896c99` ("docs: remove undefined pattern-set reference and tighten identity
semantics") — a verified Git fact, not a prediction.

~~~text
FINDING — UNDEFINED SourcePatternSet STABLE REFERENCE

Finding:
Round 1 correctly made one coherent ProfilePatternSet the input/version/
completeness anchor, but §11's RepresentativeRepositorySelection shape
introduced a `SourcePatternSet` field described as "a stable reference
identifying the exact ProfilePatternSet." No canonical ProfilePatternSet
identity (persisted or cross-process) exists anywhere in the repository —
unlike RepositoryAnalysisReference, which already carries a real stable Guid.
Leaving that field in the contract would have left Session 075 to invent an
identity contract that isn't canonically defined upstream.

Correction:
The `SourcePatternSet` stable-reference field was removed from §11. In its
place, §9.1 explicitly states that representative-repository selection is one
deterministic, in-execution composition over exactly one supplied
ProfilePatternSet instance, and that this contract defines no persisted or
cross-process identity for that instance — deferring such an identity to a
separate, later contract if it is ever needed, rather than inventing one now.
§11 RepresentativeRepositorySelection now records only plain values copied at
selection time (PatternRuleVersion, ObservationRuleVersion,
EligibilityPolicyVersion, IsEvidenceSurfaceComplete,
RepresentativeRepositoryRuleVersion) with no reference type. §12.1 was
updated to match: version/completeness fields are recorded "directly, as
plain values," not "through any stable or persisted reference."

PATTERN IDENTITY WORDING

Finding:
§10.1 previously stated "(Family, PatternKey) has no independent meaning
outside one ProfilePatternSet," which read as though Family+PatternKey had no
semantic meaning at all outside a set — understating that it IS the pattern's
canonical semantic identity within the locked pattern taxonomy, just not an
execution/persistence identity.

Correction:
§10.1 was rewritten to state the bounded distinction precisely: (Family,
PatternKey) is the canonical semantic identity of the pattern within the
versioned pattern taxonomy (which pattern, under which PatternRuleVersion);
it is sufficient to locate the unique RepeatedProfilePattern within one
coherent ProfilePatternSet; it is NOT, by itself, an identity for a specific
analysis execution or a persisted ProfilePatternSet instance. No new
execution/persistence identity was introduced.

KEPT LOCKED, UNCHANGED:
one representative per RepeatedProfilePattern; candidates =
IndependentSupportingRepositories; PatternSupportObservationCount; Owner→Name
tie-break; no global K; no set-cover/maximum coverage; no cross-pattern
breadth advantage; fork/template restrictions; archived not automatically
excluded; ProfilePatternSet as coherent version/completeness input;
IsEvidenceSurfaceComplete preservation; dedicated
RepresentativeRepositoryRuleVersion; RoleEvidenceAlignment out of scope; no
Rank/Score/Quality/Strength/Confidence/Capability/Seniority/Employability.

Re-review (github-proof-review, forked context, focused specifically on: no
undefined ProfilePatternSet reference/identity required; ProfilePatternSet
remains the coherent input anchor; version/completeness semantics preserved;
exact pattern traceability implementable without duplicating pattern truth;
semantic pattern identity not confused with analysis-execution identity; no
regression to ranking/global-coverage semantics; worklog Git-fact accuracy):
PASS. All six technical points and both worklog-accuracy checks confirmed
directly against the corrected contract text and the actual Domain runtime
(ProfilePatternSet.cs, RepeatedProfilePattern.cs, RepositoryAnalysisReference.cs
as the only existing stable-identity precedent). Diff scope confirmed limited
to §9-§12; §2-§8 and §14-§16 untouched. No BLOCKER/HIGH/MEDIUM finding.
~~~

---

## Post-Push Independent Review — Round 3

The Round 2 correction was committed and pushed by the user as `3896c99`. A
third post-push independent review of that pushed state found two remaining
contract gaps in `docs/architecture/public-profile-representative-repositories.md`.
Corrected in this same session, on the same branch, without a new session and
without committing/pushing the correction — it currently sits as an
uncommitted working-tree change on top of `3896c99` (user-owned per Git
ownership rules).

~~~text
FINDING 1 — EMPTY ProfilePatternSet MUST BE VALID

Finding:
`docs/architecture/public-profile-evidence-patterns.md` §6 locks that an
empty pattern collection is not a claim and not negative developer evidence;
`ProfilePatternSet.Patterns` may legitimately be `Count == 0`. §11 of this
contract nonetheless defined `RepresentativeRepositorySelection.
RepresentativeRepositories` as "one or more" — a cardinality that made a
zero-pattern `ProfilePatternSet` structurally impossible to represent
correctly, contradicting a locked P6 invariant.

Correction:
New §3.3 (Empty ProfilePatternSet) states the required mapping explicitly:
zero RepeatedProfilePattern values → zero RepresentativeRepository values,
not a failure, not negative evidence, never a reason to fabricate a
repository. §7 gained a clarifying sentence: the one-selection-per-pattern
rule applies to every pattern in the source set, and produces nothing when
that set is empty. §11's `RepresentativeRepositorySelection.
RepresentativeRepositories` cardinality was corrected from "one or more" to
"zero or more," while the other five recorded fields (PatternRuleVersion,
ObservationRuleVersion, EligibilityPolicyVersion, IsEvidenceSurfaceComplete,
RuleVersion) are still always recorded, even when the repository list is
empty. `RepresentativeRepository.PatternSupports` itself remains "one or
more" unchanged — a `RepresentativeRepository` object still must not exist
without at least one represented pattern; only the outer collection may be
empty.

FINDING 2 — EXACT RepeatedProfilePattern TRACEABILITY

Finding:
§10.1 correctly established that `(Family, PatternKey)` is semantic pattern
identity, not analysis-execution identity — but `RepresentativePatternSupport`
then carried only `PatternFamily`/`PatternKey`/observation identities/evidence
references, with no structural way to resolve back to the exact
`RepeatedProfilePattern` instance actually consumed. The contract claimed
exact-pattern traceability (§10, matching domain-model.md §11's required
direction) without a structurally implementable mechanism to deliver it,
short of a `(Family, PatternKey)` lookup after the fact — exactly what §10.1
had just ruled out as insufficient for execution-level exactness.

Correction:
`RepresentativePatternSupport` gained a `Pattern` field: an in-memory
canonical reference to the exact `RepeatedProfilePattern` instance it was
produced from — the same instance already present in the one
`ProfilePatternSet` supplied to that selection execution. This is
composition over existing canonical pattern truth (a reference established
once, directly, when the support is constructed), not a copy, not a
re-derivation and not a new persistence identity — it does not reopen the
`ProfilePatternSetReference` question closed in Round 2. §10.1 was extended
to describe this as the actual traceability mechanism; `PatternFamily` and
`PatternKey` remain convenience projections of `Pattern.Family`/
`Pattern.PatternKey`, explicitly not themselves the traceability mechanism
and never to be used for cross-execution matching.

KEPT LOCKED, UNCHANGED:
ProfilePatternSet as the coherent input; one representative selection per
existing RepeatedProfilePattern; candidates = IndependentSupportingRepositories;
PatternSupportObservationCount; Owner→Name tie-break; no global K; no
set-cover; no cross-pattern breadth advantage; fork/template restrictions;
archived not automatically excluded; completeness preservation; version
ownership; no persisted ProfilePatternSet identity in V1; no
rank/score/quality/strength/confidence/capability/seniority/employability;
RoleEvidenceAlignment out of scope.

Re-review (github-proof-review, forked context, focused on the eight
falsification cases from the correction instructions plus worklog-fact
accuracy):
PASS WITH FINDINGS. Falsification cases 1-4 and 6-8 confirmed directly
against the corrected contract text, `ProfilePatternSet.cs`,
`RepeatedProfilePattern.cs` and `patterns.md` §6. Case 5 (structural
traceability without weakening §10.2 Evidence Scope) surfaced one MEDIUM
finding: the new `Pattern` field necessarily points at the full
`RepeatedProfilePattern` instance — including `SupportingObservations` for
every supporting repository, not only the selected one, per
`RepeatedProfilePattern.cs`'s own `NormalizeSupportingRepositories` — and the
contract did not say `Pattern` may only be used for identity/traceability
verification rather than as a second, wider evidence-enumeration path around
§10.2's scoping. Not a BLOCKER/HIGH — no production code exists yet, and the
already-scoped `ObservationKey`/`ProfileEvidenceReference` fields remained the
primary evidence path — but a real contract-consistency gap that could
mislead a Session 075 implementation. Corrected in this session: §10.2 gained
a paragraph distinguishing evidence enumeration (must stay scoped to the
selected repository) from the traceability reference itself (necessarily
points at the full pattern instance), and the `Pattern` field's own
definition in §11 now states explicitly that it is for
identity/traceability verification only, never an evidence-enumeration path.
Worklog Round 1/Round 2 history was not rewritten; no Round 3 commit SHA was
fabricated.
~~~

---

## Problems

None beyond the findings recorded above across Round 1 (two BLOCKERs), Round 2
(one remaining gap) and Round 3 (two remaining gaps), all corrected within
this session.

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
Git history so far (verified via `git log`, not asserted from memory):
a034aaa — docs: lock representative repository selection contract
efed302 — docs: correct representative repository completeness and version
          semantics (Round 1 correction)
3896c99 — docs: remove undefined pattern-set reference and tighten identity
          semantics (Round 2 correction)
All three committed and pushed by the user.

Modified since 3896c99 (this Round 3 correction, currently uncommitted in the
working tree):
- docs/architecture/public-profile-representative-repositories.md
  (new §3.3 empty-set boundary; §7 clarified; §11 RepresentativeRepositories
  cardinality corrected to zero-or-more; RepresentativePatternSupport gained
  a Pattern reference field; §10.1 traceability wording updated to match)
- worklog/CURRENT.md (Locked Foundation entry and Next Action updated to
  match)
- worklog/sessions/2026-08-18-074-representative-repository-decision-contract.md
  (this Round 3 correction recorded)

Pre-existing unrelated:
- None

Actual current Git state (branch, HEAD, staged/unstaged) should always be
read live via `git status`/`git log` rather than from this snapshot, since it
changes the moment the user commits.
~~~

---

## Next

Session 075 may begin production implementation of
`RepresentativeRepositorySelection`/`RepresentativeRepository`/
`RepresentativePatternSupport` per the locked and corrected contract, or
continue toward Role Evidence Alignment decision work — neither is started by
this session. Session contract work itself is complete; the Round 3
correction is a working-tree change the user has not yet committed — that
transition, like all Git transitions in this project, is user-owned.

---

## Git

~~~text
Commits so far (user-owned, already happened):
a034aaa — docs: lock representative repository selection contract
efed302 — docs: correct representative repository completeness and version
          semantics
3896c99 — docs: remove undefined pattern-set reference and tighten identity
          semantics

Suggested Round 3 correction commit:
docs: fix empty-set cardinality and add exact pattern traceability reference

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
