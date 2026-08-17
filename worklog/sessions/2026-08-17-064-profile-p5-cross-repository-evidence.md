# Session 064 — Profile P5 Cross-Repository Evidence Engine

**Date:** 2026-08-17
**Result:** PASS
**Scope:** Stage 7 / Profile P5 implementation
**Execution:** ChatGPT analysis → Claude implementation/review → user-owned Git finalization

## Git

Branch:
session/064-profile-p5-cross-repository-evidence

Session Base:
f41f7483eedf201ef88417c56750b9fed7f2cf7c

Remote main at start:
f41f7483eedf201ef88417c56750b9fed7f2cf7c

## Development Position

Completed:
P3 — GitHub Profile Data Collection
P4 — RepositoryAnalysis Orchestration

Current:
P5 — Cross-Repository Evidence Engine

Next:
P6 — Technical, Practice and Problem Patterns

## Goal

Convert repository-level structured evidence into deterministic,
explainable and traceable profile-level aggregate evidence.

Canonical trust chain:

Profile-level observation
→ CrossRepositoryEvidence
→ quantified observations
→ repository references
→ repository-level evidence

P5 produces aggregate evidence truth.

P5 does NOT yet produce semantic developer patterns.

## Canonical Inputs

- docs/product/public-profile-evidence-product-contract.md
- docs/architecture/public-profile-evidence-domain-model.md
- docs/architecture/public-profile-evidence-repository-eligibility.md
- docs/roadmap/mvp-phases.md
- existing RepositoryAnalysis contracts
- P3 GitHubProfileSnapshot contracts
- P4 ProfileRepositoryAnalysisResult / orchestration contracts
- Session 063 remaining-risk notes

## Critical P2 Dependency

P4 implemented only P2 Stage A:

AnalysisAdmissionDecision

Full P2 Stage B runtime eligibility remains absent, including:

- RepositoryEligibilityResult
- purpose-specific ContributionDecision
- RepositoryEligibilityStatus
- activity classification
- independence classification
- structured eligibility reasons

P5 MUST NOT bypass this boundary.

If P5 requires Stage B runtime evaluation, implement only the deterministic
minimum required by the LOCKED P2 contract.

Do not invent:

- new eligibility reasons
- hidden activity windows
- optimistic independence
- authorship inference
- content-similarity heuristics
- undocumented contribution purposes

The V1 reason identities and contribution purposes are closed by P2.

## Required P5 Capability

1. Consume successful/available repository analysis outcomes from P4.
2. Respect P2 Stage B contribution decisions before aggregate evidence use.
3. Preserve RepositoryAnalysis as atomic repository evidence truth.
4. Produce CrossRepositoryEvidence only from traceable repository evidence.
5. Every aggregate observation must identify its contributing repositories.
6. Every repository contribution must remain traceable to underlying repository evidence.
7. Quantification must be explicit and deterministic.
8. No hidden weighting is permitted.
9. Independent repetition must not be inflated by forks/template-derived repositories.
10. Undetermined contribution must not silently become Allowed.
11. Missing repository signal must not become developer-level absence.
12. InsufficientEvidence must remain distinct from Missing and technical failure.
13. Partial repository analysis may contribute only through evidence actually available.
14. Technical repository failure must not become negative evidence.
15. Input/result ordering must not alter semantic output.
16. Aggregation rules must be versioned.
17. Same normalized inputs + same rule versions must produce the same semantic aggregate evidence.
18. No hidden system clock may participate.
19. Locale must not affect structured truth.
20. No LLM may participate before deterministic structured truth exists.

## P4 Carry-Forward

- P4 result outcomes include:
  NotAdmitted
  AdmissionUndetermined
  CollectionUnavailable
  CollectionFailed
  AnalysisFailed
  Analyzed

- One repository failure must not erase successful repository evidence.
- Repository result ordering is deterministic.
- Profile repository references remain the traceability anchor.
- RepositoryAnalysisRunner semantics must remain unchanged.
- IProfileRepositorySnapshotSource still has no Infrastructure adapter.
  Do not solve P10 wiring inside P5.
- RepositoryAnalysisComposer does not currently emit AnalysisStatus.Failed.
  P2 Stage B technical-failure mapping must use the actual P4 outcome boundary
  when required by the contract.

## Aggregation Boundary

CrossRepositoryEvidence is NOT:

- a developer score,
- seniority,
- employability,
- candidate ranking,
- hire/no-hire,
- authorship proof,
- originality proof,
- a narrative summary,
- a technical-focus label,
- an engineering-practice pattern,
- a problem-area pattern.

It is structured aggregate evidence only.

## No Hidden Weighting

P5 must not introduce arbitrary weighted formulas.

If an aggregate observation says that evidence occurs across N repositories,
that count and denominator must be explainable from explicit contributing and
eligible repository sets.

No silent score conversion.

No magic percentages.

No ranking formula.

## Repetition Boundary

Independent repetition must use P2 independence semantics.

Fork/template-derived lineage must not inflate repetition.

Independent does NOT mean:

- author-written,
- original work,
- professional experience,
- independent problem solving.

It means only the P2 V1 observable lineage definition.

## Availability Boundary

Preserve distinctions between:

- repository not admitted
- admission undetermined
- collection unavailable
- collection failed
- analysis failed
- insufficient evidence
- repository signal Missing
- contribution NotAllowed
- contribution NotApplicable
- contribution Undetermined
- aggregate evidence unavailable

None of these states may silently become negative developer evidence.

## Determinism

Same:

- profile snapshot/reference set
- P4 repository analysis outcomes
- repository analyses
- eligibility policy version
- P5 aggregation rule version

must produce the same semantic CrossRepositoryEvidence.

Input enumeration order or execution order must not affect output.

## Traceability

Every CrossRepositoryEvidence item must allow traversal:

CrossRepositoryEvidence
→ quantified observation
→ contributing ProfileRepositoryReference(s)
→ RepositoryAnalysis reference/result
→ underlying repository evidence/signal

No aggregate claim may exist without this chain.

## Non-Goals

Do NOT implement:

- P6 TechnicalFocusArea
- P6 EngineeringPracticePattern
- P6 ProblemAreaPattern
- P7 RepresentativeRepository selection
- P8 DeveloperPublicEvidenceProfile assembly
- profile score
- seniority
- employability
- hire/no-hire
- candidate ranking
- job evidence
- role alignment
- P10 Web Profile Flow
- Infrastructure/Web profile wiring
- LLM summaries

## Architecture Boundary

- Domain owns deterministic profile evidence truth.
- Application may orchestrate deterministic evaluation.
- Web owns no evidence logic.
- Infrastructure transport concerns must not leak into Domain.
- Existing repository analysis semantics must not change.
- Existing 15 signal evaluators must not be copied or reinterpreted.

## Acceptance Criteria

1. P5 uses RepositoryAnalysis evidence rather than duplicating signal evaluation.
2. Required P2 Stage B runtime semantics are implemented only from locked contract.
3. Contribution purpose decisions are explicit.
4. Undetermined is never promoted optimistically.
5. Fork/template lineage cannot inflate independent repetition.
6. CrossRepositoryEvidence is immutable structured truth.
7. Aggregate observations are explicitly quantified.
8. Every aggregate observation is repository-traceable.
9. Every repository contribution is traceable to underlying repository evidence.
10. Missing != InsufficientEvidence != technical failure.
11. Partial analysis contributes only available evidence.
12. No hidden weighting or developer score exists.
13. Output ordering is deterministic.
14. Rule/policy versions participate in deterministic result identity.
15. No hidden clock exists.
16. No P6+ pattern semantics appear.
17. No Web/Infrastructure wiring scope drift appears.
18. Existing P3/P4 and RepositoryAnalysis behavior remains green.
19. New tests cover aggregation, traceability, eligibility, repetition inflation,
    unavailable/partial/failure semantics and input-order independence.
20. Full solution tests PASS.
21. Independent forked review returns no BLOCKER/HIGH findings.

## Claude Workflow

1. Read CLAUDE.md.
2. Verify branch and session base.
3. Read CURRENT and this Session 064.
4. Load the minimum P1/P2/P5 canonical docs.
5. Inspect existing P4 contracts before designing P5.
6. Inspect exact RepositoryAnalysis evidence/reference model.
7. Determine precisely which P2 Stage B runtime pieces P5 requires.
8. Do not implement more P2 than necessary for P5.
9. Recommend Claude-native model + effort and STOP.
10. Wait for user to select model/effort and write devam et.
11. Implement bounded P5.
12. Run focused tests.
13. Run full regression suite.
14. Run independent forked review.
15. Address accepted findings without scope drift.
16. Finalize CURRENT + Session 064.
17. Do NOT commit.
18. Do NOT push.

## Claude Routing

Model: opus
Effort: xhigh

## Implementation Result

Two bounded layers were added:

1. Domain — the minimum P2 Stage B runtime required by P5.
2. Domain evidence model plus one Application composer for aggregation.

Canonical decisions taken during implementation:

- Stage B was implemented only for `ProfileObservation` and
  `IndependentRepetition`. `CurrentFocus`/`HistoricalEvidence` were not
  evaluated, so no overall `RepositoryEligibilityStatus` is derived and the
  result type is `RepositoryContributionAssessment`, not
  `RepositoryEligibilityResult`.
- Because CurrentFocus is not evaluated, no `CurrentActivityWindow` duration
  was invented. `EligibilityPolicyVersion` stays an identity value object.
- The P4 `AnalysisAdmissionReason` enum was replaced by the canonical
  `EligibilityReason` (the exact nine locked V1 identities). Stage A results
  are constrained to `EmptyRepository` and `RequiredInputUnavailable`, so the
  wider taxonomy cannot produce invalid admission states.
- Evidence-surface mapping follows the contract matrix directly: Empty →
  NotAllowed/EmptyRepository; Completed and Partial → observation Allowed;
  InsufficientData → NotAllowed/InsufficientEvidence; Failed, missing analysis
  and undetermined admission → observation Undetermined with a manifest-only
  lineage restriction resolved first.
- `RepositoryAnalysisComposer` never emits `AnalysisStatus.Failed`, so a null
  analysis (P4 `CollectionUnavailable`/`CollectionFailed`/`AnalysisFailed`)
  maps onto the same §22.5 technical-failure boundary.
- Independence is lineage-only: fork or template-derived → NonIndependent with
  `ForkDetected`/`TemplateDerived` (both when both apply); unknown provenance →
  Undetermined with `IndependenceUndetermined`. No optimistic fallback.
- Observations are repository-signal evidence: one observation per canonical
  signal key that has actual supporting evidence. `Strong`/`Present`/`Partial`
  support; `UnavailableEvidence`/`RequiresHumanReview` become an explicit
  undetermined count; `Missing`/`NotApplicable` count as neither, so absence is
  never asserted. Observations with zero support are not emitted.
- Quantification is explicit only: supporting, independent supporting, eligible
  and undetermined repository counts. No weighting, percentage, ranking or
  score exists.
- `CrossRepositoryEvidence` enforces traceability structurally: every evidence
  reference must belong to a contributing repository whose observation
  contribution is Allowed and whose analysis reference matches, and the
  independent count must equal the repositories whose independent repetition is
  Allowed. One eligibility policy version, one observation rule version and one
  analysis version per evidence surface.
- Ordering is normalized inside the Domain results, so input and execution
  order cannot change the semantic output.
- No clock, locale input or LLM participates. Application still does not
  reference Infrastructure, and Web was not touched.

## Validation

- `dotnet test GitHubProof.slnx`: 792/792 PASS
  (Domain 488, Application 87, Infrastructure 149, Web 68).
- Build: 0 warning / 0 error.
- `git diff --check`: clean.
- Acceptance criteria 1–21: PASS.

## Independent Review

`github-proof-review` — two rounds, forked context.

Round 1 verdict: PASS WITH FINDINGS, no BLOCKER/HIGH. Accepted MEDIUM:
`CrossRepositoryEvidence` enforced policy and rule version coherence but not
`AnalysisVersion`, so analyses from different analyzer versions could be mixed
into one aggregate. Accepted LOW: the Stage B Skip branch hard-coded
`EmptyRepository` instead of propagating the admission reason, and
`AnalysisAdmissionResult` accepted all nine reason identities after the
taxonomy merge.

All three were fixed with regression tests. The worklog INDEX table row
separation was repaired in the same pass.

Round 2 verdict: PASS WITH FINDINGS, no BLOCKER/HIGH. All four corrections
verified against the locked contract. Remaining findings were worklog
finalization plus one missing propagation test, which was then added.

## Remaining Risk

- P5 observations are repository-signal evidence only. A technical-area
  taxonomy (for example grouping C# and .NET) needs a versioned taxonomy and
  belongs to P6; primary language was deliberately not turned into an
  observation because it has no signal/evidence traceability chain.
- `IProfileRepositorySnapshotSource` still has no Infrastructure adapter and no
  Web wiring, so the profile pipeline remains library-only.
- `EligibilityReason.Archived`, `Historical` and `NonIndependentContribution`
  are defined but unused; they belong to the CurrentFocus and full Stage B
  rules that P5 does not evaluate.
- Domain ordering uses `ProfileRepositoryReference.CompareTo` (owner, name)
  while the P4 Application comparer adds a URL tie-break. Unreachable today
  because collector URLs are derived from owner/name, but the two orderings
  should be unified when the reference contract is next revisited.
- `worklog/CURRENT.md` carries a UTF-8 BOM introduced before Session 063; it
  was left untouched to avoid unrelated churn.

## Next Action

User manually commits and pushes the Session 064 branch.

After remote verification, continue with P6 — Technical, Practice and Problem
Patterns.