# Session 098 — Profile Decision-Sufficient Pagination Contract

**Date:** 2026-08-21
**Result:** CONTRACT LOCKED (docs-only; no `src/`, no `tests/`)
**Scope:** docs (architecture + product) / worklog
**Model Route:** Claude / Opus / xhigh
**Model Gate:** APPROVED — user wrote `devam et`

---

## 1. Goal

After the Session 097 controlled live smoke failed on unbounded
required-surface pagination, decide and lock the smallest truthful Profile V1
runtime contract that lets collection stop when the authorized semantic decision
is already mathematically irreversible — **without ever pretending a partially
observed provider collection was fully enumerated.**

The session ran as a decision session first: a Decision Packet was produced and
the user approved it with corrections before any document was edited.

## 2. Starting State

~~~text
Branch:
session/098-profile-decision-sufficient-pagination-contract

Base HEAD:
545210e — matches the expected SHA given in the handoff

Pre-existing Working Tree:
CLEAN

Relevant Previous Session:
worklog/sessions/2026-08-21-097-profile-runtime-collection-implementation.md
~~~

## 3. Locked Decisions

### 3.1 CollectionObservationState

A new closed three-value Domain concept on `RepositoryCollection<T>`:

~~~text
Enumerated        provider collection fully enumerated
DecisionBounded   stopped deliberately; every currently authorized consumer of
                  that Profile V1 surface can already decide irreversibly
Incomplete        stopped by failure/interruption (today's semantics)
~~~

`IsComplete` keeps its exact meaning and is never overloaded:
`IsComplete = true` ⇔ `Enumerated`. No fake completeness.

`DecisionBounded` requires `IsComplete: false`, `TotalCount: null`,
`DataAvailability` unchanged at `Available`, **no** `CollectionWarning`, and
therefore no `limitation.snapshot.collection-partial` and no
`RepositoryAnalysis.Status` downgrade from the bound alone. It is never negative
developer evidence.

Responsibility split: collectors know no thresholds and only report honestly;
the **evaluator** decides whether the observed prefix proves the classification.

### 3.2 Commits — C1 + C3

Stop when **all three** hold: observed count >= 10, distinct observed UTC commit
days >= 8, observed span >= 14 days. At that point `CommitHistory` = Strong/7 and
`CommitDistribution` = Strong/8, both maxima, both irreversible. The monotonicity
is independent of provider ordering.

If those conditions never hold, enumerate in full (C3) and preserve exact current
semantics. `Link rel="last"` was deliberately **not** adopted: it does not
establish the distinct-day fact.

### 3.3 Other surfaces

~~~text
issues         >= 2 qualifying issues observed
pull requests  >= 2 merged pull requests observed
releases       first published release with notes observed
tags           >= 1 tag observed, or first page exhausted with no tag
~~~

**Correction accepted from the user:** although `IssueUsage`,
`PullRequestUsage` and `ReleaseUsage` already return their maximum before
consulting `IsComplete`, that is a *signal-level* permission only. At the
repository-analysis level today's collectors emit a `CollectionWarning` for an
unfinished collection and the orchestrator merges it, which would still turn
`RepositoryAnalysis.Status` into `Partial`. So these surfaces **do** require the
repository-analysis-level `DecisionBounded` change. The contract says so
explicitly.

### 3.4 Contributors omitted

Profile V1 collects no contributors at all. Re-verified at HEAD: no evaluator
declares `contributors` in `requiredDataKeys`, no `FindAvailability("contributors")`
call exists, nothing enumerates `RepositorySnapshot.Availability` wholesale, and
`CrossRepositoryEvidenceComposer` never reads it. Repository Proof keeps
collecting contributors unchanged.

### 3.5 Fork Policy B — an eligibility semantics change

A repository whose manifest deterministically reports `Fork = Present` now
resolves to `AnalysisAdmissionDecision = Skip` with reason `ForkDetected`.

It stays visible in the repository universe and in `RepositoryDrillDown`, its
fork condition stays visible, but it contributes no `ProfileObservation`, no
`IndependentRepetition`, no `CurrentFocus` and no `HistoricalEvidence`, and its
overall status is `NonContributing`. A failure to collect a fork can no longer
make the profile `Partial`.

**This is a product/eligibility decision, never described as a performance
optimization.** The runtime blocker is solved by §3.1–§3.3 on its own; the fork
rule was selected on product grounds: Profile V1 owns no attribution contract
able to prove which fork changes belong to the owner, so a fork must not be used
as developer evidence. The accepted false negative is recorded explicitly.

`TemplateDerived` semantics are unchanged.

### 3.6 Evidence language

`DecisionBounded` evidence reports observed lower bounds
(`observedCountAtLeast`, `activeDaysAtLeast`, `spanDaysAtLeast`,
`firstObservedCommitAt`), never exact totals and never a repository-first-commit
claim. Evidence **keys** stay stable.

## 4. Canonical Files Changed and Why

~~~text
docs/architecture/analysis-domain-model.md
  canonical owner of RepositorySnapshot / RepositoryCollection.
  Added CollectionObservationState, the IsComplete equivalence, the
  DecisionBounded obligations, the collector/evaluator responsibility split and
  the explicit Repository Proof exclusion.

docs/architecture/mvp-signal-definitions.md
  canonical owner of signal/evaluator semantics.
  The commit-history and commit-distribution "Version 1 veri bütünlüğü" clauses
  explicitly FORBADE a terminal score from an incomplete collection — the direct
  contradiction with C1. Amended with the DecisionBounded exception, the
  what-cannot-be-proven rule, the lower-bound evidence rule and version
  ownership.

docs/architecture/public-profile-evidence-repository-eligibility.md
  canonical owner of admission and contribution.
  §6.3 said fork alone does not require Skip — the direct contradiction with
  Fork Policy B. Amended §6.3 (+ new §6.3.1), new §19.3.2 version ownership,
  new §22.1.1 baseline, §25 Fork, the §28 fork example, and guarded the now
  unreachable §22.5/§22.6 fork examples.

docs/architecture/public-profile-runtime-collection.md
  canonical owner of the Profile runtime collection contract.
  Contributors moved from §4 required to §5 omitted with its proof; new §15
  (decision-sufficient collection), §16 (fork policy), §17 (acceptance
  criteria); §6 and §13 given explicit Session 098 scope notes.

docs/product/p9-profile-validation-contract.md
  §4.5 `mostly forks` note only. Its two "must not" rules are unchanged and
  still hold; what changed is how they are satisfied.

docs/product/p10-web-profile-flow-implementation-plan.md
  §13.17 cross-reference refreshed (omission list is now three surfaces; the
  DecisionBounded and fork rules are pointed at). No slice, route, presentation
  or prohibition clause changed.
~~~

Inspected and deliberately **not** changed, because no contradiction exists:

~~~text
docs/architecture/public-profile-evidence-domain-model.md
  §13 already states Skip and NotAllowed are RESOLVED terminal outcomes, so a
  skipped fork is not an unresolved obligation. The new rule is supported, not
  contradicted.

docs/roadmap/mvp-phases.md
  the "Toplanacak veriler" list including Contributors belongs to Faz 2, the
  Repository Proof collection layer, which is unchanged.

docs/product/public-profile-evidence-product-contract.md §7 Fork
  "fork may remain visible profile context" still holds (ProfileContext stays
  Allowed); "must not inflate repetition" holds more strongly than before.

docs/architecture/public-profile-evidence-patterns.md
docs/architecture/public-profile-representative-repositories.md
  both only forbid fork-inflated repetition / fork candidacy; still true.
~~~

## 5. Version Changes

~~~text
EligibilityPolicyVersion   NEW version required (new §19.3.2).
                           Fork admission withdraws contributions a previous
                           version allowed, so it is a breaking eligibility
                           change and a MAJOR increase under the existing
                           convention. Concrete identity fixed in Session 099.

RuleSetVersion             NEW value required. It owns scoring/evaluator
                           semantics, and when the two commit signals may
                           produce a terminal score has changed.

Signal Version             process.commit-history and process.commit-distribution
                           must increase (§1 "İlk sürüm kuralı").

AnalyzerVersion            must change: snapshot normalization gains
                           CollectionObservationState and evidence values change.
~~~

No parallel version identity was invented. Thresholds, scores and
`MaximumScore` values are unchanged.

## 6. Independent Review

`github-proof-review`, docs-only, focused on the eight requested contradiction
dimensions. **Verdict: PASS WITH FINDINGS — 0 BLOCKER, 1 HIGH, 2 MEDIUM,
4 LOW. Six closed in-session; F1's fix was later found incomplete — see §6b.**

~~~text
F1 HIGH   The lower-bound evidence rule was scoped to "not Enumerated", which
          includes Incomplete — and Repository Proof produces Incomplete. Taken
          literally it would change Repository Proof evidence values and
          contradict §3.1, the domain model and §17.9.
          FIX (round 1, INCOMPLETE): scoped strictly to DecisionBounded in
          `mvp-signal-definitions.md` only. The duplicate statement in
          `public-profile-runtime-collection.md` §15.5 kept the "non-Enumerated"
          wording and reintroduced the same defect. Closed in §6b/F2.

F2 MEDIUM §6 semantic-equivalence invariant and §13 Non-Goals (both Session 096
          text) contradicted the new §15/§16.
          FIX: explicit Session 098 scope notes on both — §16 is the
          product/domain route §6 itself requires, and the two narrowed
          Non-Goals are restated precisely.

F3 MEDIUM Signal-level version ownership was unstated.
          FIX: added the version-ownership subsection (§5 above).

F4 LOW    Header cross-references "§22.7" and "§27" were wrong.
          FIX: corrected to §19.3.2, §22.1.1 and §28.

F5 LOW    The §22.6 guard note contradicted its own "Known Fork" premise.
          FIX: reworded to match the §22.5 formulation.

F6 LOW    §5.1.1 claimed to enumerate EVERY Contributor occurrence but listed
          three of seven.
          FIX: reworded to separate the surface's own production types from
          consumers; the load-bearing zero-consumer claim was verified correct.

F7 LOW    The tags row read like a one-page cap.
          FIX: restated as a decision-sufficiency condition with both cases.
~~~

The review also confirmed clean: no `IsComplete` overload or fabrication;
bounded vs failure kept distinct across warnings/limitations/status/availability;
no remaining canonical clause admitting or analyzing a fork; no doc asserting a
fork retains `ProfileObservation`/`HistoricalEvidence`; contributors listed
nowhere else as a required Profile V1 surface; no unmeasured performance claim in
any doc; and no arbitrary page cap, sampling, concurrency or hidden top-N.

## 6b. Second Independent Review (ChatGPT)

A second independent review returned **BLOCKED — 1 BLOCKER, 2 HIGH, 1 MEDIUM,
1 LOW**. All five are closed. No decision changed; every correction made the
already-approved decisions internally consistent.

**F1 — BLOCKER — §14.2 vs §17.7 were mutually impossible. Valid, fixed.**
§14.2 (Session 096) forbids modifying "any Domain policy", while §17.7 requires
the fork Skip rule, which necessarily changes `AnalysisAdmissionPolicy` and the
`EligibilityPolicy` version identity. Session 099 could not have satisfied both
literally. §14 is now explicitly labelled the historical Session 096 acceptance
surface with a supersession note: where a §17 requirement deliberately changes a
§14 invariant, §17 supersedes it **for Session 099 only**, and the one current
instance is §14.2's Domain-policy restriction versus the authorized Fork Policy B
amendment. Everything else in §14.2 still binds —
`ProfileRepositoryAnalysisOrchestrator`, `CrossRepositoryEvidenceComposer`,
`ProfileComposer` and `ProfileReportProjector` stay unchanged — and Repository
Proof preservation is not weakened. Session 096 history was not rewritten.

**F2 — HIGH — §15.5 reintroduced the Incomplete evidence defect. Valid, fixed.**
The round-1 fix had been applied only to `mvp-signal-definitions.md`; §15.5 of
this contract still said "non-`Enumerated`", which includes `Incomplete`, and
Repository Proof produces `Incomplete`. §15.5 now scopes the lower-bound
vocabulary **strictly and only** to `DecisionBounded` and states explicitly that
`Enumerated`, `Incomplete` and Repository Proof evidence semantics are unchanged.
The §6 record above was corrected so it no longer claims round 1 fully closed
this.

**F3 — HIGH — §17.8 overstated fork semantic equivalence. Valid, fixed.**
`CrossRepositoryEvidenceComposer` admits evidence from any repository whose
`ProfileObservation` is `Allowed` and tracks independent support separately, so a
promoted `RepeatedProfilePattern` can carry fork-derived
`SupportingObservations`, `SupportingRepositories` and `ProfileEvidenceReference`
entries. Fork Policy B removes those. §17.8 now locks the narrower truthful
invariant: given the same independent supporting repositories, no pattern is
created or removed **solely** because a fork is skipped, the `>= 2`
`IndependentSupportingRepositories` rule is unchanged and representative
winner/selection semantics are unchanged — but fork-derived supporting evidence
and traceability content **may intentionally disappear**, which is the expected
eligibility semantic change, not a regression. No test may assert traceability
equality across the two policies.

**F4 — MEDIUM — §5.3 stale two-surface wording. Valid, fixed.**
"Neither omitted surface" now reads "none of the three", and the downstream
statement is split: `SourceFiles`/`DependencyDeclarations` end in the Unsupported
ProblemArea chain, `Contributors` has no production consumer at all (§5.1.1).
Omission of any of the three is still not a completeness deficit.

**F5 — LOW — `CURRENT.md` stale future-tense block. Valid, fixed.**
The "Session 098 must decide ..." paragraph and its "Locked constraints for that
session" list were rewritten as a historical outcome table showing how each
Session 097 constraint was honoured. No decision was changed.

## 7. Validation

~~~text
git diff --check
PASS — no whitespace error

Consistency scans over docs/
PASS — 0 hits for unmeasured perf claims (842 / 10-20 / faster),
       0 dangling "§22.7", 0 stray "Fork Policy B", 0 stale "bu iki yüzey"

Second-review targeted scans
PASS — 0 "non-Enumerated" lower-bound wording,
       §14/§17 supersession stated, no exact pattern/profile equality claim
       across fork policies, 0 "Neither omitted surface",
       0 "Session 098 must decide"

Contradiction dimension scans
PASS — contributors nowhere required for Profile V1;
       no "fork does not automatically remove ..." clause remains;
       IsComplete = true stated only as equivalent to Enumerated

dotnet build / dotnet test
NOT RUN — docs-only; no src/ or tests/ file touched
~~~

## 8. Problems

~~~text
None.
~~~

## 9. Not Done

~~~text
- No production code. CollectionObservationState, the stop rules, the fork Skip
  rule and the evidence-language change are all unimplemented.
- No concrete EligibilityPolicyVersion / RuleSetVersion / AnalyzerVersion value
  selected.
- Execution guard still NOT IMPLEMENTED and no numeric value chosen.
- No live GitHub run. The runtime effect of these decisions is unmeasured.
~~~

## 10. Known Risks

~~~text
- Fork Policy B is a deliberate false negative: a profile owner's real work
  inside a fork will not be seen by Profile V1. Recorded and accepted.
- The decisions are unmeasured. Whether a real profile run now completes is a
  hypothesis until Session 099 implements them and a live run measures it.
- A very large NON-fork repository whose commit distribution cannot be pinned
  still requires full enumeration by design. The §10 execution guard remains the
  only outer limit and still has no value.
~~~

## 11. Working Set / Final Git State

~~~text
Modified:
- docs/architecture/analysis-domain-model.md
- docs/architecture/mvp-signal-definitions.md
- docs/architecture/public-profile-evidence-repository-eligibility.md
- docs/architecture/public-profile-runtime-collection.md
- docs/product/p9-profile-validation-contract.md
- docs/product/p10-web-profile-flow-implementation-plan.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- worklog/sessions/2026-08-21-098-profile-decision-sufficient-pagination-contract.md

Pre-existing unrelated:
- NONE
~~~

## 12. Next — Session 099 Implementation Handoff

~~~text
Session 099 — Profile Decision-Sufficient Collection Implementation
~~~

Expected code areas, in dependency order:

~~~text
Domain
  RepositoryCollection<T>                     CollectionObservationState
  CommitHistorySignalEvaluator                gate + lower-bound evidence
  CommitDistributionSignalEvaluator           gate + lower-bound evidence
  IssueUsage / PullRequestUsage / ReleaseUsage lower-bound evidence only
  AnalysisAdmissionPolicy                     fork -> Skip / ForkDetected
  EligibilityPolicy                           new EligibilityPolicyVersion

Infrastructure
  IGitHubRepositoryCollectors                 stop-predicate seam
  Commit / Issue / PullRequest / Release / Tag collectors
                                              decision-bounded stopping,
                                              no CollectionWarning when bounded
  GitHubRepositorySnapshotOrchestrator        contributors omitted on the
                                              profile composition

Web
  AnalysisVersion registration                new RuleSetVersion / AnalyzerVersion

Tests
  monotonicity proofs per stop rule
  DecisionBounded vs Enumerated equivalence (status/score/limitation/warning)
  no-warning proof for DecisionBounded
  fork skipped before any repository request
  Repository Proof unchanged (existing suites must stay green untouched)
~~~

Acceptance criteria are locked in
`docs/architecture/public-profile-runtime-collection.md` §17.

After implementation, a controlled live smoke must be run **by the user** and
measured before any execution-guard numeric value is proposed.

## 13. Git

~~~text
Suggested commit:
docs(profile): lock decision-sufficient pagination and fork policy

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~
