# GitHub Proof — Current Development State

**Last Updated:** 2026-08-21 (Session 097 — live smoke failure finalization)
**Source of Truth:** docs/

## Development

Completed:
P6 — Technical, Practice and Problem Patterns — COMPLETE / REMOTE VERIFIED
P7 — Representative Repository and Role Evidence Alignment — COMPLETE WITH
EXPLICIT DEFERRAL (Session 076 decision; Session 077 canonical docs
amendment — see below)
P8 — Deterministic Profile Composer and Report Contract — COMPLETE, bounded
post-review correction applied and validated, external closure audit PASSED
(Sessions 083–088; see "P8 Phase State" and "Next Action" below)
P9 — Public Profile Evidence Validation (Validation Profile Set) — COMPLETE
(Sessions 089–092; validation-only, no `src/` changed across all of P9; see
`docs/roadmap/mvp-phases.md` "P9 Validation Durumu" and "Next Action" below)

Completed Foundation:
Session 066 — Repository Evidence Surface Expansion — COMPLETE / REMOTE VERIFIED
Session 067 — Dependency Semantic Category Contract + Registry — COMPLETE /
REMOTE VERIFIED
Session 068 — DependencySourceReferenceEvidence Contract — COMPLETE /
REMOTE VERIFIED
Session 071 — Bounded Source Evidence Collection — COMPLETE / REMOTE VERIFIED /
INTEGRATED INTO MAIN
Session 072 — Dependency Source Reference Detection Contract — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN
Session 073 — Dependency Source Reference Detector Implementation — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN
Session 074 — Representative Repository Decision + Contract — COMPLETE /
REMOTE VERIFIED / INTEGRATED INTO MAIN at
f32c5de7a6e21e7eedc786deb973b6aec159dcec (contract-only; five post-push
independent-review rounds, all closed; detail in the Session 074 file)

Current:
P9 COMPLETE (Session 092; P9-I1 Session 089 at 4a1bc5a, P9-I2 Session 090 at
c195dc9, P9-I3 Session 091 at 8d07321, P9-I4 Session 092). P10 — Profile Web
Flow contract/planning CLOSED (Session 093, docs-only). P10-I1 Live Profile
Request Boundary COMPLETE (Session 094). P10-I2 Profile Entry + Routing
COMPLETE (Session 095) — landing entry unified by explicit user decision;
`/profile` exists. P10-I3..I6 not started. Session 096 (docs-only) locked the
Profile runtime collection contract
(`docs/architecture/public-profile-runtime-collection.md`). See "P10 Phase
State" below.

Completed Product Work:
Session 069 — Product Positioning Alignment — COMPLETE / REMOTE VERIFIED
Canonical owner: docs/product/product-positioning.md

Session 070 — Jobs-Style Product Packaging — COMPLETE / REMOTE VERIFIED
Canonical owner: docs/product/product-experience-principles.md

Incomplete:
ProblemArea — taxonomy, promotion and repository evidence all remain
unimplemented and deferred

P7 Final V1 State (user decision, Option A, recorded this session):

~~~text
Representative Repository:
COMPLETE

RoleEvidenceAlignment:
DEFERRED — NOT_READY_WITH_CURRENT_EVIDENCE

P7 V1:
COMPLETE WITH EXPLICIT DEFERRAL
~~~

DEFERRED means: not implemented in Profile V1 / Milestone B, not required to
proceed to P8, not negative evidence, not permanently prohibited. Feasibility
may be reopened later if the canonical evidence surface becomes strong
enough. The reason for deferral is narrow and stated exactly as decided: the
current canonical public evidence surface is insufficient for a useful, safe
and deterministic generic technical-role alignment without semantic
inflation — not that RoleEvidenceAlignment is permanently rejected. No
upstream evidence expansion (ProducedWorkEvidence, ProblemArea, semantic-
category wiring, or any other Session 076 §6 candidate gap) is required or
started by this decision. The project continues toward P8 using the
canonical profile evidence that already exists (TechnicalFocus,
EngineeringPractice, RepresentativeRepository).

Session 075 implemented the locked Representative Repository contract in
Domain (RepresentativeRepositorySelection, RepresentativeRepository,
RepresentativePatternSupport, RepresentativeRepositorySelectionPolicy,
RepresentativeRepositoryRuleVersion, with RepresentativeRepositoryRuleVersion.V1
= "1.0.0" as the first canonical runtime rule version) with 54 new Domain
tests, closed across four correction/hardening turns and a final closure
review. No Application orchestration, report/UI or P8 composer consumes it
yet. Git transition for Session 075 is user-owned and must be read from
actual Git state.

Session 076 (feasibility/decision, no code) evaluated whether
RoleEvidenceAlignment is safely definable from the current canonical evidence
surface. Verdict: NOT_READY_WITH_CURRENT_EVIDENCE — the proven statement is
narrow: the existing canonical evidence surface is insufficient for a useful
safe V1 generic technical-role alignment, not that such a role taxonomy is
never possible. All five falsification probes (.NET/Backend/Frontend/
Full-Stack/AI-ML Developer) fail today, each for one of: a locked-forbidden
bridge (C#!=.NET, JavaScript!=Frontend, Python!=AI/ML), a role-shaped label
(backend/frontend/full-stack) excluded specifically from the
`DependencySemanticCategory` taxonomy (§8.6 of that contract) — not from any
future, separately-contracted P7 generic role taxonomy — or evidence that
exists in Domain but is not wired into the profile pipeline
(DependencySemanticCategoryEvidence, DependencySourceReferenceEvidence are
produced by RepositorySemanticEvidencePolicy /
RepositorySourceReferenceEvidencePolicy but never consumed by
CrossRepositoryEvidenceComposer or ProfilePatternPromotionPolicy). The user
then made the P7 closure decision (Option A) recorded above. See
worklog/sessions/2026-08-19-076-role-evidence-alignment-feasibility.md for
the full feasibility report and the finalization record.

Session 077 (docs/worklog-only, no code) amended the minimum canonical
surface so RoleEvidenceAlignment is explicitly non-blocking for Profile V1 /
Milestone B / P8 composition / P9 validation / P10 first-read surface, while
preserving the concept, its safety semantics and its non-permanent status.
Amended: docs/roadmap/mvp-phases.md (P7/P8/P9/P10 + DoD),
docs/product/public-profile-evidence-product-contract.md (§14, §24),
docs/architecture/public-profile-evidence-domain-model.md (§12, §21),
docs/architecture/public-profile-representative-repositories.md (§14
cross-reference only — no selection semantics changed). Reviewed and
intentionally unchanged: docs/roadmap/deployment-milestones.md,
docs/architecture/public-profile-evidence-patterns.md,
docs/product/job-evidence-alignment-product-contract.md. See
worklog/sessions/2026-08-19-077-p7-closure-role-evidence-alignment-deferral.md
for the full record.

Next Profile Phase:
P8 — Deterministic Profile Composer and Report Contract planning is now the
next authorized development stage; it may proceed without treating
RoleEvidenceAlignment as a dependency.

P8 Phase State (Sessions 078–082):

~~~text
P8-1 — Exact Contract Closure:
COMPLETE

P8 implementation:
READY
~~~

Session 078 recorded the 10 locked P8-1 principles. Session 079 recorded the
8 locked exact contract decisions; its review found four semantic decisions
open plus one inherited prerequisite, which Session 080 locked. The P8-1
independent contract review then returned PASS WITH FINDINGS (1 HIGH, 2
MEDIUM, 3 LOW); the HIGH required a new semantic decision. Session 081
recorded that decision — the profile-level evidence-surface accounting rule —
and corrected all six findings. Session 082 ran the final exit gate: PASS WITH
FINDINGS (4 LOW, all type (A), all corrected), then a confirmation re-run that
found no new decision, capability, identity or contradiction.

Gate outcome: 0 BLOCKER, 0 HIGH, 0 MEDIUM, and no remaining semantic
implementation-choice gap. P8 implementation does not have to guess any
product or domain semantics.

P8 CLOSED (Session 088). A bounded post-review correction was applied and
validated, and the external closure audit passed (see the Session 088 status
entry and session file):

~~~text
P8-I1 Final Repository Eligibility Runtime:  COMPLETE
P8-I2 Profile Root Foundation:               COMPLETE
P8-I3 Deterministic Profile Composer:        COMPLETE
P8-I4 Typed Profile Report and Projector:    COMPLETE
P8-I5 Integration and P8 Closure:            COMPLETE

P8 — Deterministic Profile Composer and Report Contract:  COMPLETE
~~~

Canonical owners defining P8 implementation semantics:

- final `RepositoryEligibilityResult` consumption contract —
  docs/architecture/public-profile-evidence-repository-eligibility.md §31,
- `CurrentActivityWindow` = 365 days — same file §19.3.1,
- `SnapshotReference` / `SnapshotIdentity` (`CapturedAt` excluded from
  identity) — docs/architecture/public-profile-evidence-domain-model.md §6,
- `AnalysisStatus` / `Failed` ownership boundary, the expected-evidence-surface
  definition and the Complete/Partial/Failed accounting rule (incl.
  `IsEvidenceSurfaceComplete` as consumed upstream input) — same file §13,
- `ProfileLimitation` / `ProfileWarning` structured contract, V1 taxonomies,
  the `LimitedIndependentEvidenceSurface` emission condition, the
  deterministic warning identity mapping and canonical ordering keys — same
  file §13,
- `DeveloperPublicEvidenceProfile` canonical root and version-lineage
  compatibility — same file §14,
- typed `ProfileReport`, section availability, canonical ordering —
  docs/architecture/public-profile-evidence-report-contract.md
  (CONTRACT LOCKED V1),
- `Unsupported` ProblemArea presentation —
  docs/product/public-profile-evidence-product-contract.md §24.

Deliberately left to implementation (naming/typing/versioning/mechanism only,
never semantics): the concrete type and name of the composition-result
wrapper, report DTO code naming, the mechanism deriving `SnapshotIdentity`
from the canonical normalized snapshot representation, the first runtime value
of the profile-level `AnalyzerVersion`, the binding shape tying an
`EligibilityPolicyVersion` to its immutable `CurrentActivityWindow`, and how
the final `RepositoryEligibilityResult` relates to the existing narrower
`RepositoryContributionAssessment`. The last two were surfaced by Session
083's code inspection and are engineering trade-offs, not semantic decisions.
Precedent for this class of choice:
`RepresentativeRepositoryRuleVersion.V1 = "1.0.0"` (Session 075).

P8 Implementation Plan (Session 083):
Five bounded slices, in order —
P8-I1 Final Repository Eligibility Runtime,
P8-I2 Profile Root Foundation,
P8-I3 Deterministic Profile Composer,
P8-I4 Typed Profile Report + Projector,
P8-I5 Integration / P8 Closure.
Full plan with per-slice responsibility, files, tests, invariants,
out-of-scope and exit criteria:
worklog/sessions/2026-08-19-083-p8-implementation-planning.md

P10 Phase State (Sessions 093-095):

~~~text
P10 contract/planning:
COMPLETE (docs-only)

P10-I1 Live Profile Request Boundary:
COMPLETE (Session 094)

P10-I2 Profile Entry + Routing:
COMPLETE (Session 095)

P10-I3..I6:
NOT STARTED

Profile runtime collection contract (Session 096):
CONTRACT LOCKED

Profile runtime collection implementation (Session 097):
PASS WITH FINDINGS — automated proofs PASS, controlled live smoke FAILED

Profile runtime readiness:
BLOCKED — unbounded required-surface pagination

Profile execution guard:
NOT IMPLEMENTED — no numeric value selected
~~~

Session 096 (docs-only) added a new canonical architecture owner,
`docs/architecture/public-profile-runtime-collection.md` (CONTRACT LOCKED),
and a minimal P10 cross-reference (§13.17 + header amendment line only). It
locks that Profile Proof may use its own repository collection composition at
the existing `IProfileRepositorySnapshotSource` seam — the composition may
differ, the analysis semantics may not — that Profile V1 must not collect
bounded raw `SourceFiles` or dependency-manifest `DependencyDeclarations` on
the aggregation path (proven non-scoring by the two Application isolation test
classes, whose baseline arm is the absent-surface case), that the omission
never becomes negative evidence/warning/limitation/status change, that a
silent top-N repository cap is prohibited, that concurrency is not a
request-cost optimization, plus the minimum runtime telemetry surface and the
bounded execution-guard semantics (guard expiration is never `Complete`, and is
kept strictly distinct from ordinary request cancellation, whose current
propagating behavior is unchanged).
Repository Proof collection is explicitly unchanged. No `src/`, no `tests/`
changed. Full record:
`worklog/sessions/2026-08-20-096-profile-runtime-cost-contract.md`

P10 entry contract amended (Session 095, explicit user decision after visual
review of the running application): the landing carries ONE unified GitHub
entry input, superseding the previously locked separate Profile Proof /
Repository Proof entry surfaces. The two analysis semantics were NOT merged —
Profile Proof remains the macro view, Repository Proof the micro view; only
the entry experience is unified. Canonical owner of the decision:
`docs/product/p10-web-profile-flow-implementation-plan.md` §13.0.

Session 093 reconciled the two P10 drafts
(`docs/product/p10-web-profile-flow-implementation-plan.md`,
`docs/product/p10-presentation-language-contract.md`) against canonical
roadmap/product/report contracts and the actual `GitHubProof.Web`
architecture (routing, `ProfilePipeline` wiring, Repository Proof flow,
design/token CSS architecture), and locked 16 decisions covering the
`/profile` route boundary, profile identity input contract (reuses the
existing `GitHubProfileHandle.Parse` username/canonical-URL behavior
verbatim), the Web composition boundary from profile input to
`GitHubProfileSnapshot` to `ProfilePipeline` to `ProfileReport`, Repository
Proof flow preservation (behavioral: `OnPostAsync`/`RepositoryUrl`/analysis
semantics unchanged; `Pages/Index.cshtml`'s landing markup may additively
gain a Profile Proof entry point — corrected in a same-session ChatGPT
review round, see the session file §10), the Profile = macro / Repository =
micro drill-down mechanic (a live re-analysis through the existing `/`
entry, since no `RepositoryAnalysis` is ever persisted), the locale
boundary (`ProfileReportProjector` keeps zero locale parameters — all
Profile presentation-language mapping stays Web-layer-only, one fixed
English dictionary, no site-wide localization project), deterministic
presentation-language mapping, distinct language for `TechnicalFocusAreas`
("Visible technical direction") vs `EngineeringPracticePatterns`
("Recurring engineering practices"), the `ProblemAreaPatterns`
Unsupported/`RoleEvidenceAlignment` Deferred omission rules (both
unchanged, re-locked), the closed partial/sparse/unavailable/rate-limited/
forbidden/failed state vocabulary (`DataAvailabilityStatus` +
`ProfileLimitationCode`/`ProfileWarningCode`, no new state invented), and
the six implementation slices below (P10-I1's scope explicitly includes the
missing `IGitHubProfileSnapshotCollector` Web composition-root
registration, not only a new service — `Program.cs` registers
`IGitHubRepositorySnapshotCollector` but never `IGitHubProfileSnapshotCollector`,
verified by reading the actual file). No canonical `docs/architecture/` or
`docs/product/public-profile-evidence-product-contract.md` file was
amended — every decision was already derivable from existing locked
contracts and actual code; only the two P10 drafts themselves were amended
to state the reconciled, implementation-ready contract explicitly. `src/`
and `tests/` were not touched. Five review passes ran in this session: (1)
`github-proof-review` round 1 — PASS WITH FINDINGS (1 HIGH label
inconsistency, 1 MEDIUM drill-down nullability gap), both corrected; (2) a
same-session ChatGPT final review — found four further contract-closure
gaps (P10-I1 scope, slice-overlap wording, presentation-language
audit-trail accuracy, landing/routing over-lock), all corrected; (3)
`github-proof-review` round 2 — **PASS, 0 BLOCKER/HIGH/MEDIUM/LOW**; (4) a
same-session ChatGPT pre-commit audit — found three remaining doc
consistency issues (drill-down mechanic still described via
`RepositoryAnalysisReference` → `RepositoryIdentity.Url` in two spots
instead of the locked `Repository.Url` rule; presentation-language
contract's two internal notes contradicted each other on whether the §8
dictionary changed; an invalid `P10-I8` slice reference), all corrected;
(5) `github-proof-review` round 3, re-scoped to those three corrections —
**PASS, 0 BLOCKER/HIGH/MEDIUM/LOW**. All findings across all five passes
are closed. Full decision evidence and validation:
worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md

P10 Implementation Plan (Session 093):
Six bounded slices, in order —
P10-I1 Live Profile Request Boundary,
P10-I2 Profile Entry + Routing,
P10-I3 Meaning / Direction / Starting Point,
P10-I4 Why + Evidence Drill-Down,
P10-I5 Limitations + Partial / Failure UX,
P10-I6 Experience Refinement + Closure.
Full plan with per-slice responsibility and locked decisions:
docs/product/p10-web-profile-flow-implementation-plan.md §7, §13

## Production Delivery

Milestone A:
NOT_STARTED

Milestone B:
BLOCKED

## Active Session

Session:
097 — Profile Runtime Collection Implementation

Active Branch:
session/097-profile-runtime-collection-implementation

Session Base:
715c55a (Session 096 — profile runtime collection contract)

Goal:
Implement the Session 096 contract: a Profile V1 collection composition that
issues zero GitHub requests for `SourceFiles` and `DependencyDeclarations`,
preserves the exact Profile V1 semantic result, leaves Repository Proof
unchanged, and adds bounded per-profile runtime telemetry. The execution guard
is deliberately not implemented.

Session File:
worklog/sessions/2026-08-21-097-profile-runtime-collection-implementation.md

Status:
**PASS WITH FINDINGS — automated validation PASS (1491/1491, 0 warning /
0 error), controlled live smoke FAILED, Profile runtime readiness BLOCKED.**

The user ran the controlled live smoke on `fatihGunalp`. It exceeded 3 minutes
without producing a Profile Proof result; at least 842 outbound GitHub GET send
events were observed before abort, dominated by repeated commit pagination for
repository id 1079771485 (`fatihGunalp/fatih-azure-sdk-for-net`). No
`ProfileExecutionTelemetry` completion line was produced because the process was
terminated before `LiveProfileReportService`'s `finally` block ran. The later
`core: used=47, remaining=4953, reset=1787271349` reading was taken **after the
reset boundary**, so it belongs to the new primary-rate-limit window and must not
be compared with those 842 send events — request amplification and primary-quota
consumption are separate facts, and nothing here indicates a telemetry counting
defect.

**Blocking reason: unbounded required-surface pagination**, discovered during the
real profile run and confirmed by a read-only audit of the six paginated
surfaces. commits paginate without bound and both `CommitHistorySignalEvaluator`
and `CommitDistributionSignalEvaluator` require `IsComplete` **before** their
ordinary decision paths, so no early stop exists under current semantics. issues
become irreversible at >= 2 qualifying issues, pull requests at >= 2 merged PRs,
releases at the first published release with notes, and tags can be decided from
one page; contributors has no current production consumer. commits remain the
dominant blocker because an exact non-Strong `CommitDistribution` can require the
full history under the current contract.

The Session 097 implementation facts below all still hold; the blocker is a
pre-existing surface the optimization did not touch.
`GitHubRepositorySnapshotOrchestrator` gained a Profile-specific factory
(`CreateProfileComposition`) and a nine-collector construction path, and the
shared assembly was refactored so the two enrichment collectors can be absent —
the collector fields became nullable and `CollectAsync`/`CollectAvailability`
changed accordingly, so the source change is **not** purely additive. What is
preserved exactly is the Repository Proof contract: the eleven-collector
constructor keeps its non-null parameter contract, `CreateDefault` still builds
the full composition, and on that path both collectors are non-null so every new
branch takes the route it took before. Repository Proof's observable semantics
are unchanged and proven by test. When a surface
is omitted the collector is never reached, `RepositorySnapshot` normalizes null
to the isolation-test baseline's empty collection, and **no availability entry
is contributed at all** (absent, never `NotRequested`/`Unavailable`/`Failed`).
`AddProfilePipeline` now owns the whole profile runtime boundary: its own
`"GitHubProof.GitHub.Profile"` named client (identical transport contract plus
the counting handler), its own composition via `CreateProfileComposition`, and
it no longer resolves the caller's `IGitHubRepositorySnapshotCollector` at all.
Telemetry (`src/GitHubProof.Web/Profile/Runtime/`) is per-execution and ambient
via `AsyncLocal` — the deliberate answer to `IHttpClientFactory` handler
pooling — with a closed 17-value request-category vocabulary, a closed
repository-outcome vocabulary (canonical `DataAvailabilityStatus` names +
`AnalysisFailed`), and hard prohibitions proven by test (no token, no
Authorization header, no source/README/manifest content, no identifier as a
key). Repository counters come from two observation-only wrappers, so
`ProfileRepositoryAnalysisOrchestrator` is unchanged and admission is never
recomputed in Web. Removed request ceiling: up to 66 per admitted repository;
measured on the deterministic fixture as exactly 4 requests removed with the
profile request set a strict subset of the full one. Cancellation semantics are unchanged
— every exception propagates exactly as before, nothing fabricated; only the
telemetry *label* distinguishes caller cancellation (`cancelled`) from an
`OperationCanceledException` raised while the caller's token was not cancelled,
such as `HttpClient`'s transport timeout (`Faulted`). **Execution guard NOT
implemented; no numeric value chosen.** Final: **1491/1491**, 0 warning /
0 error.

Two independent review passes. Round 1 (`github-proof-review`): **PASS WITH
FINDINGS** (0 BLOCKER, 0 HIGH, 2 MEDIUM, 3 LOW) — both MEDIUMs closed in-session
(F1 worklog sync, F2 per-repository rate-limit/forbidden/failed
distinguishability). Round 2 (ChatGPT): 3 further MEDIUMs, all closed — F1
transport timeout must not be labelled caller cancellation (this also closed
round 1's LOW F5), F2 a *thrown* repository collection now records the closed
`Failed` outcome and still rethrows unchanged so the orchestrator's semantics are
untouched, F3 the worklog's "two additive members only" claim corrected to match
the diff. Two LOWs remain open and deliberately unfixed: the public
nine-collector constructor could in principle build an omitting Repository Proof
composition, and `repository-contents` merges enrichment with project-file
traffic in the category split. Commit/push NOT performed (user-owned).

Previously Active Session:
096 — Profile Runtime Cost Contract

Branch:
session/096-profile-runtime-cost-contract

Session Base:
d64c858 (feat(profile): add unified profile entry routing)

Goal:
Define the canonical Profile V1 runtime collection contract that removes
unnecessary GitHub request amplification without changing Repository Proof
behavior or current Profile V1 semantic truth. Contract only.

Session File:
worklog/sessions/2026-08-20-096-profile-runtime-cost-contract.md

Status:
**CONTRACT LOCKED (docs-only).** New canonical owner:
`docs/architecture/public-profile-runtime-collection.md`. All six handoff
hypotheses were verified against code and contracts before any doc change and
all six hold — **no contradiction found**. Verified facts: the Profile path
delegates every admitted repository to the full Repository Proof collector
composition (`GitHubProfileRepositorySnapshotSource` →
`IGitHubRepositorySnapshotCollector` → `GitHubRepositorySnapshotOrchestrator.CreateDefault`);
the source collector costs up to 34 requests per repository (2 tree-resolution
requests — `git/commits/{sha}` then `git/trees/{rootTreeSha}?recursive=1` — plus
32 content slots) and the dependency collector up to 32, a combined theoretical
ceiling of 66 extra requests per admitted repository; both surfaces are provably non-scoring
(the two Application isolation test classes use the *absent-surface* case as
their baseline arm, which is exactly the omission state), `RepositoryAnalysis`
status derives only from `CollectionWarnings` + signal statuses and the
orchestrator deliberately keeps both surfaces out of `CollectionWarnings`;
`CrossRepositoryEvidenceComposer` reads neither surface; the only Domain
consumers (`RepositorySemanticEvidencePolicy`,
`RepositorySourceReferenceEvidencePolicy`) still have no `src/` caller at all;
and domain-model §13 already establishes that an Unsupported/Deferred
capability is not an evidence obligation, so the omission is not a
completeness deficit. Eight decisions locked: Repository Proof unchanged;
Profile-specific composition allowed at the `IProfileRepositorySnapshotSource`
seam (composition may differ, semantics may not); `SourceFiles` +
`DependencyDeclarations` omitted from the Profile V1 aggregation path with a
closed list and explicit never-negative-evidence rules; no silent top-N
repository cap; no concurrency-as-cost-fix; a minimum runtime telemetry
surface with hard token/content prohibitions; a bounded execution guard whose
expiration can never be `Complete`; and a future re-activation rule.

A bounded ChatGPT-review correction pass followed (docs-only, see §6b of the
session file). Two valid findings, both corrected: (1) HIGH — the contract's own
§10/§11 contradicted each other on cancellation, now separated exhaustively into
§11.1 **external/request cancellation** (current behavior unchanged:
`OperationCanceledException` propagates, no fabricated analysis/pipeline/partial/
failure result, not developer evidence — verified against
`ProfileRepositoryAnalysisOrchestrator`'s explicit rethrow and
`LiveProfileReportService`'s pass-through) and §11.2 **internal guard
expiration** (distinct from caller cancellation in both directions, never
`Complete`, never the completed-so-far subset as a whole profile, no invented
domain evidence status, STOP-and-report if the existing runtime result types
cannot represent it truthfully), with §10 renumbered and §14 criteria 10-12
rewritten to match; (2) MEDIUM — the source ceiling was off by one because
`CollectTreeAsync` performs two requests, corrected to 34 / combined 66
everywhere. Encoding was verified and **was not corrupted**: both new files are
valid UTF-8, no BOM, no CR, zero mojibake byte sequences — the review paste's
`â€`/`Â§`/`Ä±` were a PowerShell `Get-Content` ANSI-decoding artifact. The locked
optimization decision itself did not change. P10 was
NOT rewritten — only a header amendment line and a new §13.17 cross-reference
so §13.4 is not read as requiring full-composition reuse. No `.cs`, `.csproj`,
`.cshtml` or test file changed; no test suite run (documentation-only).
Commit/push NOT performed (user-owned).

Earlier Session:
095 — P10-I2 Profile Entry + Routing

Branch:
session/095-p10-i2-profile-entry-routing

Session Base:
f2ebd3c (feat(profile): add live profile request boundary)

Goal:
Implement P10-I2 (Profile Entry + Routing) and, on explicit user decision
after visual review of the running application, replace the previously
locked separate Profile Proof / Repository Proof entry surfaces with ONE
unified GitHub entry input.

Session File:
worklog/sessions/2026-08-20-095-p10-i2-profile-entry-routing.md

Status:
**P10-I2 COMPLETE.** The landing carries one unified GitHub entry input;
the two analysis semantics were NOT merged (Profile Proof = macro view,
Repository Proof = micro view — only the entry is unified). Entry routing is
deterministic from the submitted string's shape — no heuristic, no
probabilistic scoring, no AI/intent detection, no GitHub call to discover
intent: bare GitHub login → Profile Proof, canonical profile URL → Profile
Proof, canonical repository URL → Repository Proof, invalid/ambiguous shape →
entry-level validation error, never guessed. `GitHubEntryClassifier`
(`src/GitHubProof.Web/Entry/`) is two-stage — shape selects the canonical
owner, then acceptance is delegated to `GitHubProfileHandle.Parse` /
`GitHubRepositoryUrl.Parse` by calling them, never reimplementing them; the
parse result is discarded and the raw string travels on, so the P10-I1
boundary (Web derives no profile identity) holds. `/profile` exists: GET
performs no live analysis and returns to the landing entry, POST invokes the
existing P10-I1 `LiveProfileReportService`, and landing → profile routing
preserves POST semantics via `RedirectToPagePreserveMethod` (307), so the
identity travels in the re-posted body rather than a query string. Note the
scope of that guarantee: GET `/profile` itself never analyzes, but a browser
re-submitting the POST can still re-run the analysis — ordinary POST
semantics. The existing Repository Proof analysis path remains intact
(projector and all `_Repository*.cshtml` partials untouched); only the entry
surface changed (`RepositoryUrl` → `GitHubTarget`, `_RepositoryInput.cshtml`
→ `_GitHubInput.cshtml`, `RepositoryInputModel` → `GitHubInputModel`,
`type="url"` → `type="text"`). Four decorative inline-SVG boundary icons were
added through the existing `UiIcon` system (no package, CDN, asset, emoji or
dependency). Accepted landing composition: hero centred and optically above
the mathematical centre, two-line desktop title, one unified input, Analyze
button below it, "What GitHub Proof is not" at the bottom of the first normal
desktop viewport, responsive mobile behavior preserved. Canonical docs
amended for the unified-entry decision (P10 plan §13.0 + affected clauses;
design-principles and UI-system landing/component clauses). 145/145 Web,
1448/1448 full solution, 0 warning / 0 error. Independent review: round 1
PASS WITH FINDINGS (3 MEDIUM, all corrected in-session), round 2 PASS WITH
FINDINGS (all three closed; remaining MEDIUM was worklog sync, resolved by
this session's documentation step; 2 LOW deferred). Commit/push NOT performed
(user-owned).

Earlier Session:
094 — P10-I1 Live Profile Request Boundary

Session File:
worklog/sessions/2026-08-20-094-p10-i1-live-profile-request-boundary.md

Status:
P10-I1 COMPLETE. `IGitHubProfileSnapshotCollector` Web composition-root
registration + `LiveProfileReportService`/`LiveProfileAnalysisResult`
(`src/GitHubProof.Web/Profile/`), the profile counterpart of the repository
live-report pair. Typed collector failures preserved; no pipeline/domain/
infrastructure contract changed. 1386/1386 full solution, 0 warning.
Independent review PASS WITH FINDINGS (1 MEDIUM, worklog-sync only,
resolved same session).

Previously Completed Session:
093 — P10 Contract + Implementation Closure

Session File:
worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md

Status:
P10 contract/planning CLOSED (docs-only). See "P10 Phase State" below for
the full outcome. `src/`/`tests/` were not touched by Session 093.

Last Completed Session:
091 — P9-I3 Profile Risk and Traceability Validation — COMPLETE / merged to
main at 8d07321. 11/11 scenarios executed, 9/9 required risks falsified
against real `ProfilePipeline` output, 0 traceability/prohibited-inference/
unsupported-claim failures, no production contract violation found.
Independent review PASS (0 BLOCKER/HIGH/MEDIUM/LOW). Session file:
worklog/sessions/2026-08-20-091-p9-i3-risk-traceability-validation.md

Previously Completed Session:
088 — P8-I5 Integration and P8 Closure (Application/Infrastructure/Web
integration wiring)

Session File:
worklog/sessions/2026-08-20-088-p8-i5-integration-and-p8-closure.md

Status:
P8-I5 COMPLETE. P8 COMPLETE — external closure audit passed (see below).
Added `ProfilePipeline`/`ProfilePipelineResult`
(`src/GitHubProof.Application/Profile/`), the single running composition path
`ProfileRepositoryAnalysisOrchestrator` (P3) -> `RepositoryEligibilityPolicy`
(P8-I1, evaluated per repository against `snapshot.CapturedAt`, never a
system clock) -> `CrossRepositoryEvidenceComposer` (P5) ->
`ProfilePatternPromotionPolicy` (P6) ->
`RepresentativeRepositorySelectionPolicy` (P7) -> `ProfileComposer` (P8-I3)
-> `ProfileReportProjector` (P8-I4, only over a composed profile). No stage's
algorithm was reimplemented; each stage's own output object is passed by
reference into the next, which is what lets `ProfileComposer`'s existing
reference-identity coherence checks succeed on a correctly-wired run.
Established the first canonical production values `ObservationRuleVersion.V1`
and `PatternRuleVersion.V1` (both `"1.0.0"`, following the
`RepresentativeRepositoryRuleVersion.V1` precedent); reused
`EligibilityPolicy.V1`, `ProfileAnalyzerVersion.V1`,
`RepresentativeRepositoryRuleVersion.V1` as-is. Added the smallest production
`IProfileRepositorySnapshotSource` adapter,
`GitHubProfileRepositorySnapshotSource` (Infrastructure), wrapping the
existing `IGitHubRepositorySnapshotCollector` single-repository GitHub
collection path with no new collection semantics — this required one
additive `ProjectReference` from `GitHubProof.Infrastructure` to
`GitHubProof.Application` (no cycle: Application depends only on Domain).
Added minimal Web DI wiring, `AddProfilePipelineServiceCollectionExtensions`
(`src/GitHubProof.Web/Profile/`), called once from `Program.cs`; Web makes no
eligibility, composition, pattern-promotion, representative-selection or
report decision of its own, and no profile page/UI was added. Complete,
Partial and the sparse-but-resolved-stays-Complete case are all proven
end-to-end through the real pipeline; a correctly-wired pipeline cannot
naturally retrigger `ProfileComposer`'s own `Failed` path (already
exhaustively covered by P8-I3's tests), so this slice instead directly tests
its own Failed-routing guard. Determinism (fixed snapshot + deterministic
analysis IDs -> semantically identical profile and report),
repository-order invariance, and the full trace
`Report.WhyThisConclusion -> Pattern -> SupportingEvidence -> RepositoryAnalysisReference`
are all proven through the real pipeline, including one guard
(`snapshot.CapturedAt`, never `TimeProvider`, drives activity classification)
empirically verified to fail when temporarily removed. No canonical docs
changed; no conflicting canonical runtime-version literal found. Per the
user's explicit task instructions, no `/github-proof-review` was run for
this slice.

Post-review bounded correction (same session): an external reviewer flagged
1 MEDIUM finding — `ProfilePipelineResult.Create(ProfileCompositionResult,
ProfileReport?)` was `public` and accepted the composition and the report as
two independent parameters, so an external caller could pair one profile's
composition with a different profile's report; `ProfilePipeline.RunAsync`
itself always passed the correct same-run pair, so this was a public-API
exposure risk, not a live defect. Corrected: `Create` removed, replaced with
a public `Failed(composition)` (accepts only a `Failed` composition, always
`Report = null`, carries no report parameter at all) and an `internal`
`Composed(composition, report)` usable only from within
`GitHubProof.Application` (i.e. only `ProfilePipeline`). `RunAsync` updated
mechanically to call the new factories; no other pipeline semantics changed.
New reflection-based test proves no public constructor/factory on
`ProfilePipelineResult` accepts both a `ProfileCompositionResult` and a
`ProfileReport`, empirically verified against a temporarily reintroduced
`Create` method. Full solution build 0 warning / 0 error; 1289/1289 tests
(Application 160 = 158 prior + 3 new − 1 removed; Infrastructure 259, Web 72,
Domain 798 unchanged from the initial slice).

A subsequent independent closure audit (scope completeness, evidence
integrity, determinism, boundary review, API integrity, test coverage
adequacy, documentation consistency) returned PASS, with one non-blocking
required correction: `docs/roadmap/mvp-phases.md`'s P8 status lines still
read `READY`/`READY (not started)`; corrected in this same docs-only pass
(see worklog/sessions/2026-08-20-088-p8-i5-integration-and-p8-closure.md).

Previous completed session:
087 — P8-I4 Typed Profile Report and Projector (Application implementation,
self-review, post-review bounded correction)

Session File:
worklog/sessions/2026-08-20-087-p8-i4-typed-profile-report-projector.md

Status:
P8-I4 COMPLETE. Implemented in Application, additively, under
`src/GitHubProof.Application/Profile/Report/`: `ProfileReportSectionKey` (the
nine canonical section identities, in canonical order),
`ProfileReportSectionAvailability` (Supported | Unsupported | Deferred),
`ProfileReportSection<TItem>` (SectionKey + Availability + Items),
`ProfileReport` (the nine-section aggregate, each section's expected V1
key/availability hard-locked in the constructor, plus `Warnings` carried
separately — never a tenth section — and a type-erased `Sections` descriptor
list for order/availability verification), and `ProfileReportProjector.Project(
DeveloperPublicEvidenceProfile)` — a pure, deterministic, one-way mapping
that reruns nothing (no eligibility/observation/pattern-promotion/
representative-selection/composition) and introduces no sort of its own,
preserving every collection's already-canonical upstream order.
`RepresentativeRepositories`/`Limitations`/`RoleEvidenceAlignment` sections
reuse the exact Domain objects by reference (no reselection, no re-derivation,
and `RoleEvidenceAlignment`'s empty guarantee is compile-enforced via its
private constructor); `TechnicalFocusAreas`/`EngineeringPracticePatterns`
filter `profile.RepeatedProfilePatterns` by family and attach a representative
reference via the same reference-equality "Canonical Pattern Trace" technique
`ProfileComposer` uses; `ProblemAreaPatterns` never reads the `ProblemArea`
family at all (proven by a test that plants one upstream and confirms it still
projects `Unsupported + []`); `WhyThisConclusion` flattens each pattern's own
already-owned supporting-evidence references; `RepositoryDrillDown` carries
only `RepositoryAnalysisReference`, never embedded `RepositoryAnalysis`;
`PublicProfileSummary` is structured facts (section + availability + item
count) only, no prose, cross-checked by test against the sections they
reference. No localization resources added — locale independence is
structural (no presentation-text field exists yet in this slice), verified by
projecting under two different cultures. Per the user's explicit instruction,
no broad `/github-proof-review` was launched for the initial implementation —
self-review only against the handoff's locked contract; no semantic gap
encountered there.

A subsequent bounded review raised 1 MEDIUM finding — `WhyThisConclusion`
projected `profile.RepeatedProfilePatterns` unfiltered, so an upstream
`ProblemArea`-family pattern could leak into it even though `ProblemAreaPatterns`
itself correctly stayed `Unsupported + []` — plus one test-adequacy gap (the
locale test proved only section order, not full semantic invariance).
Corrected in the same slice: `WhyThisConclusion` now filters to
`ProfilePatternFamily.TechnicalFocus`/`EngineeringPractice` only (regression
test `WhyThisConclusion_ExcludesUnsupportedProblemAreaPatterns`, empirically
confirmed to fail pre-fix and pass post-fix); a new
`Project_UnderDifferentCultures_ProducesSemanticallyIdenticalReport` test now
proves culture cannot alter section identity/availability, pattern items,
representative repositories, limitations, warnings, drill-down references or
conclusion evidence. No Domain change, no new semantic decision, no
production behavior beyond the one filter added. Full solution build 0
warning / 0 error; 1272/1272 tests (Application 151 = 123 baseline + 28 new;
Domain 798, Infrastructure 255, Web 68 unchanged). No canonical docs changed.

Previous completed session:
086 — P8-I3 Deterministic Profile Composer (Application implementation,
independent review + finalization)

Session File:
worklog/sessions/2026-08-19-086-p8-i3-deterministic-profile-composer.md

Status:
P8-I3 COMPLETE. Implemented in Application, additively:
`ProfileCompositionInput` (bundles `ProfileAnalysisOrchestrationResult`,
`RepositoryEligibilityResult[]`, `CrossRepositoryEvidence`, `ProfilePatternSet`,
`RepresentativeRepositorySelection`, `AnalyzerVersion`, `StartedAt`/
`CompletedAt`) and `ProfileComposer.Compose(...)`, which: verifies repository-
universe coherence across orchestration/eligibility/evidence (value-based, not
reference-based — `RepositoryContributionAssessment` is independently computed
today by both `CrossRepositoryEvidenceComposer` and `RepositoryEligibilityPolicy`,
so coherent runs are value-equal but reference-distinct); verifies, by
reference (`ReferenceEqualityComparer`), that every promoted pattern's
supporting observations actually belong to the given `CrossRepositoryEvidence`
and every representative selection's referenced pattern actually belongs to
the given `ProfilePatternSet` (the "Canonical Observation/Pattern Trace");
derives `AnalysisStatus` (Complete/Partial) from `IsEvidenceSurfaceComplete`
plus per-repository `RepositoryEligibilityStatus.Undetermined`; emits
`LimitedIndependentEvidenceSurface`/`PartialEvidenceSurface` limitations and
all four `ProfileWarning` identities (the latter from
`ProfileRepositoryAnalysisResult.Outcome`/`CollectionAvailability`/
`Analysis.Status`, since `RepositoryEligibilityResult` alone cannot
distinguish rate-limited from other-unavailable from failed); and returns
`Failed()` with no root — via catching any `ArgumentException`, including the
one `DeveloperPublicEvidenceProfile`'s own constructor already throws for
version incoherence — when coherence cannot be proven. Full solution build 0
warning / 0 error; 1244/1244 tests (Application 123 = 101 baseline + 22 new;
Domain 798, Infrastructure 255, Web 68 unchanged). Independent review via the
repository's `github-proof-review` workflow, three rounds: round 1 PASS WITH
FINDINGS (1 HIGH — a missing pattern↔evidence coherence check, fixed);
round 2 PASS WITH FINDINGS (1 MEDIUM — the round-1 fix's test accidentally
masked pre-existing coverage of the pattern↔selection check, fixed and
empirically re-isolated in both directions); round 3 PASS, 0 BLOCKER/HIGH/
MEDIUM/LOW. No canonical docs changed. `ProfileComposer` is not yet wired into
any running Application composition path (P8-I5's responsibility).

Previous completed session:
085 — P8-I2 Profile Root Foundation (Domain implementation, independent
review + finalization)

Session File:
worklog/sessions/2026-08-19-085-p8-i2-profile-root-foundation.md

Status:
P8-I2 COMPLETE. Implemented in Domain, additively: `SnapshotIdentity`
(SHA-256 over a deterministic, length-prefixed, culture-invariant token
stream written by `SnapshotSemanticWriter` walking the
`ISnapshotSemanticComponent` tree — `CapturedAt`/`RetrievedAt` excluded at
the point each component writes itself, every other canonical field
included, order-irrelevant collections sorted before writing) and
`SnapshotReference`; `ProfileAnalysisStatus` (`Complete`/`Partial` only, no
`Failed` member — makes a `Failed` valid root a compile-time
impossibility); structured `ProfileLimitation`/`ProfileWarning` via a shared
`ProfileCondition<TCode>` base implementing the §13 identity/scope/
deduplication/canonical-ordering rules, with exactly the two locked
limitation identities and four locked warning identities;
`DeveloperPublicEvidenceProfile` matching the §14 canonical root shape
(`RoleEvidenceAlignments` always empty via a private-constructor
placeholder type); `ProfileCompositionResult` owning `Failed` per the §13
composition-result wrapper boundary (`Failed()` carries no profile,
`Composed()` requires one). Six existing files
(`GitHubProfileMetadata`, `GitHubProfileSnapshot`, `ProfileRepository`,
`ProfileRepositoryManifest`, `GitHubProfileIdentity`,
`ProfileRepositoryReference`) each received exactly one additive
`ISnapshotSemanticComponent` implementation — no existing field,
constructor or behavior changed. Full solution build 0 warning / 0 error;
1222/1222 tests (Domain 798 = 726 baseline + 72 new; Infrastructure 255,
Application 101, Web 68 unchanged). Independent review via the repository's
`github-proof-review` workflow: PASS, 0 BLOCKER/HIGH/MEDIUM/LOW — no
correction needed, no new semantic decision required. No canonical docs
changed.

Previous completed session:
084 — P8-I1 Final Repository Eligibility Runtime (Domain implementation)

Session File:
worklog/sessions/2026-08-19-084-p8-i1-final-repository-eligibility-runtime.md

Status:
P8-I1 COMPLETE. The final repository eligibility runtime is implemented in
Domain, additively: `EligibilityPolicy` binds `EligibilityPolicyVersion` to an
immutable `CurrentActivityWindow` (V1 = "1.0.0" + exactly 365 days, private
ctor + canonical registry), `RepositoryEligibilityPolicy` completes Stage B
with `CurrentFocus`/`HistoricalEvidence` and deterministic
`ActivityClassification`, and `RepositoryEligibilityResult` composes the
existing `RepositoryContributionAssessment` while deriving
`RepositoryEligibilityStatus` in its constructor. The Session 083 open choice
was resolved as composition, so `RepositoryContributionAssessment`,
`RepositoryContributionPolicy`, `CrossRepositoryEvidence` and
`CrossRepositoryEvidenceComposer` were NOT modified — P5 is provably
unchanged. Full solution build 0 warning / 0 error; 1150/1150 tests (Domain
726 = 676 baseline + 50 new; Infrastructure 255, Application 101, Web 68
unchanged). Two independent review rounds: PASS WITH FINDINGS then PASS WITH
FINDINGS (0 BLOCKER/HIGH/MEDIUM, no finding requiring a new semantic
decision); all findings corrected in-session. No canonical docs changed.

Previous completed session:
083 — P8 Implementation Planning (planning only, no code)

Session File:
worklog/sessions/2026-08-19-083-p8-implementation-planning.md

Status:
Executable five-slice P8 implementation plan recorded. Code inspection
confirmed `ProfileRepository` already carries `Archived` and `LastPushedAt`,
so `CurrentFocus` needs no collection-layer change; and surfaced two new
technical (not semantic) choices — the `EligibilityPolicyVersion` →
`CurrentActivityWindow` binding shape, and how the final
`RepositoryEligibilityResult` relates to the existing narrower
`RepositoryContributionAssessment`, which is load-bearing inside P5's
`CrossRepositoryEvidenceComposer`. The five-slice grouping holds; P8-I1
carries a P5 regression surface that is an explicit exit criterion, and
P8-I2's identity/limitation/warning work has no hard dependency on P8-I1. No
blockers. No src/, tests/ or docs/ changed. P8 implementation remains
READY / NOT STARTED.

Previous completed session:
082 — P8-1 Exit Gate and Finalization (docs/worklog only, no code)

Session File:
worklog/sessions/2026-08-19-082-p8-1-exit-gate-finalization.md

Status:
P8-1 EXIT GATE PASSED. Round 1: PASS WITH FINDINGS (4 LOW, no BLOCKER/HIGH/
MEDIUM, none requiring a new semantic decision); all four corrected as
editorial precision in domain model §6/§13 — total resolved/unresolved
partition, named canonical repository comparer for limitation/warning
ordering, enumerated V1 scope values (`Profile`, `Repository`, derived from
the already-locked identities), and the "materially affect" floor
clarification. Confirmation re-run verified the corrections introduced no new
decision, capability, identity or contradiction, and specifically that the
total partition matches the eligibility enums exactly (union minus
`Undetermined`) and that the scope enumeration is derivable rather than
invented. P8-1 finalized COMPLETE; P8 implementation READY, not started. No
src/, tests/ changed.

Previous completed session:
081 — P8-1 Review Corrections (docs/worklog only, no code)

Session File:
worklog/sessions/2026-08-19-081-p8-1-review-corrections.md

Status:
All six P8-1 independent-review findings corrected. The HIGH finding (F1)
required a new user+ChatGPT semantic decision, now locked and recorded in
domain model §13: the "expected evidence surface" definition (supported and
applicable obligations already imposed by the existing canonical owners;
Unsupported/Deferred are not obligations), the resolved-vs-unresolved
distinction (`Analyze`/`Skip`/`Allowed`/`NotAllowed`/`NotApplicable` resolved;
`Undetermined` unresolved), Complete/Partial/Failed rewritten on that
accounting, and `ProfilePatternSet.IsEvidenceSurfaceComplete` locked as a
consumed upstream input (false → Partial; true alone insufficient for
Complete). F2 closed by the deterministic `ProfileWarning` identity mapping
(same four identities, none added). F3 corrected the claim that snapshot
capture time is audit-only — it is a semantic input to temporal eligibility,
so `SnapshotIdentity` alone is not a safe cache key (domain model §14, §16).
F4, F5, F6 applied as bounded consistency corrections. No new capability,
warning identity or limitation identity created; no locked decision reopened;
no new semantic gap discovered. No src/, tests/ changed. **Exit gate not run;
P8-1 not closed.**

Previous completed session:
080 — P8-1 Remaining Semantic Decisions (docs/worklog only, no code)

Session File:
worklog/sessions/2026-08-19-080-p8-1-remaining-semantic-decisions.md

Status:
Six locked semantic decisions recorded in their canonical owners:
`SnapshotIdentity` excluding `CapturedAt` and derived from the canonical
normalized snapshot representation (domain model §6);
`CurrentActivityWindow` = 365 days as a recency boundary (eligibility
§19.3.1); the `LimitedIndependentEvidenceSurface` emission condition, bound
to the already-locked P6 repetition minimum, with empty patterns explicitly
excluded (domain model §13); the four V1 `ProfileWarning` identities (same
§13); version-lineage compatibility as lineage coherence rather than equal
version strings, incoherence → Failed (domain model §14); and the default
Milestone B report not rendering an `Unsupported` ProblemArea section while
structured truth retains the state (product contract §24, report contract
§6.5). No conflict found. No new semantics invented. No src/, tests/
changed. **P8-1 not closed by this session and no completion review was
run** — that review is the next action.

Previous completed session:
079 — P8-1 Exact Contract Closure (docs/worklog only, no code)

Session File:
worklog/sessions/2026-08-19-079-p8-1-exact-contract-closure.md

Status:
Recorded the 8 locked exact contract decisions in their canonical owners
(eligibility §31; domain model §6, §13, §14; NEW
docs/architecture/public-profile-evidence-report-contract.md; bounded
cross-references in product contract §24/§27 and mvp-phases P8). Three
independent review rounds (BLOCKED, BLOCKED, then PASS WITH FINDINGS)
established that this did not close the phase; the four semantic decisions
plus one inherited prerequisite it left open were then closed by Session 080.
No src/, tests/ changed.

Previous completed session:
078 — P8-1 Profile Composition and Report Decision Lock (docs/worklog only,
no code)

Session File:
worklog/sessions/2026-08-19-078-p8-1-profile-composition-decision-lock.md

Status:
10 P8-1 decisions from the user-supplied ChatGPT handoff
(`p8-1-profile-composition-report-decisions.md`) recorded in the session
file. No conflict found against canonical docs. Bounded amendment applied to
docs/architecture/public-profile-evidence-domain-model.md (§13:
profile-level AnalysisStatus Complete/Partial/Failed; Limitation/Warning
count-independence rule; §14 Result Completion Boundary: incoherence ->
Failed not Partial) and docs/roadmap/mvp-phases.md (P8 section: P2
eligibility runtime-prerequisite cross-reference). No src/, tests/ changed.
This session recorded the 10 locked decisions and the bounded alignment they
directly supported; it does NOT authorize P8 implementation and does NOT
close P8-1. Independent review: PASS WITH FINDINGS (1 MEDIUM, worklog
finalize-sequencing inconsistency, non-blocking, resolved in the same
turn). P8-1 remains in contract-closure work; several exact contract
decisions are still unresolved (see "P8 Phase State (Session 078 / 079)"
above) and P8 implementation is NOT_READY.

Earlier completed sessions (closed and integrated into `main`; detail lives
in the session files, not here):

- 077 — P7 Closure + RoleEvidenceAlignment Deferral Alignment. DOCS, no code.
  Canonical amendment complete; RoleEvidenceAlignment made explicitly
  optional/non-blocking across 4 canonical docs files.
  `worklog/sessions/2026-08-19-077-p7-closure-role-evidence-alignment-deferral.md`
- 076 — Role Evidence Alignment Feasibility / Decision. No code, no docs
  changed. Verdict NOT_READY_WITH_CURRENT_EVIDENCE; user chose Option A
  (DEFERRED), making P7 V1 COMPLETE WITH EXPLICIT DEFERRAL.
  `worklog/sessions/2026-08-19-076-role-evidence-alignment-feasibility.md`
- 075 — Representative Repository Implementation (P7 Domain). PASS. 676/676
  Domain tests, full solution build 0 warning / 0 error; closure review PASS
  WITH FINDINGS (1 MEDIUM, worklog-only, resolved).
  `worklog/sessions/2026-08-19-075-representative-repository-implementation.md`

## Git

Session Branch:
session/097-profile-runtime-collection-implementation

Session Base:
715c55a (Session 096 — profile runtime collection contract)

Operational Git State:
Not tracked in CURRENT.md.

Commit, push, merge, current checkout, working-tree cleanliness and current
HEAD are transient Git state and must always be read directly from Git.

CURRENT.md records development state, not a second copy of Git state.

## Canonical Evidence Ladder

DependencyDeclaration
!= DependencySemanticCategoryEvidence
!= RepositorySourceFile
!= DependencySourceReferenceEvidence
!= ProducedWorkEvidence
!= RepositoryProblemAreaEvidence
!= RepeatedProfilePattern(ProblemArea)
!= DeveloperExperienceOrCapability
!= RepresentativeRepository

A declaration is never a source reference. A category observation is never
produced-work evidence. A source reference is never produced-work evidence. A
representative-repository selection is never developer capability, quality or
ranking evidence.

## Locked Foundation

Repository MVP V1 still owns exactly 15 scoring signals.

Repository Dependency Evidence Contract V1: non-scoring declaration surface,
introduced at AnalyzerVersion 1.0.0.

Repository Dependency Semantic Category Contract V1: five locked categories
(database-access, web-application-framework, http-client, authentication,
machine-learning), http-server deferred,
DependencyCategoryMappingVersion 1.0.0, immutable 46-entry registry,
RepositorySemanticEvidence as a non-scoring Domain result.

Repository Source Evidence Collection Contract V1: bounded raw source collection,
regular blobs only, 24-extension allow-list, 23 structural path exclusions, exact
build.gradle.kts and settings.gradle.kts filename exclusions, 32 retrieval
attempts / 64 KiB per file / 384 KiB corpus, deterministic extension round-robin,
RepositorySourceFile(Path, Content), dedicated source-files availability,
implemented at AnalyzerVersion 1.1.0.

Repository Dependency Source Reference Detection Contract V1:
docs/architecture/repository-dependency-source-reference-detection.md,
CONTRACT LOCKED version 1. Import-declaration detection only; V1 surface is
Go + JavaScript/TypeScript + Rust + Python + DotNet, with Java, Php and Ruby
explicitly outside; mandatory syntax-aware comment and string exclusion with a
desynchronization rule; strict single-literal require()/import(); identity
resolution by rule with the Go longest-module-path and DotNet exact-equality
tie-breakers; no identity registry in V1; ambiguity yields no observation;
positive-only evidence; Domain-owned policy; version owner
SourceReferenceRuleVersion; no AnalyzerVersion change required. Implemented in
Domain as of Session 073: RepositorySourceReferenceEvidencePolicy composes
RepositorySourceReferenceEvidence from RepositorySnapshot.DependencyDeclarations
+ RepositorySnapshot.SourceFiles. SourceReferenceRuleVersion first runtime
value 1.0.0. Derived and non-scoring; not written into RepositorySnapshot; no
orchestrator, report or UI consumes it yet.

Representative Repository Selection Contract V1 (Session 074, contract
locked and closed across five independent-review rounds — see
docs/architecture/public-profile-representative-repositories.md and
worklog/sessions/2026-08-18-074-...md for the full record): one
representative-repository selection per RepeatedProfilePattern, materializing
as exactly one RepresentativePatternSupport; grouped, after selection, into
zero-or-more RepresentativeRepository objects when the same repository wins
several patterns (§3.4) — grouping never influences a winner. Candidates =
IndependentSupportingRepositories only; comparison by
PatternSupportObservationCount (higher wins, scoped to one pattern, never
combined across patterns — no cross-pattern breadth advantage); tie-break
Owner→Name ordinal with a canonical-identity precondition that rejects a
non-canonical Url collision deterministically (§6.1). Two distinct traces:
the Canonical Pattern Trace (`Pattern`, an in-memory reference to the exact
`RepeatedProfilePattern` instance — identity/context only, never
representative-specific evidence, not even for the selected repository) and
the Representative Support Evidence Trace (scoped `ObservationKey`/
`ProfileEvidenceReference` values, ordered by `EvidenceKey` ordinal — the
sole source explaining a selection). `PatternRuleVersion`/
`ObservationRuleVersion`/`EligibilityPolicyVersion`/`IsEvidenceSurfaceComplete`
are copied from the source `ProfilePatternSet`; `RepresentativeRepositoryRuleVersion`
is dedicated. No persisted `ProfilePatternSet` identity; no
rank/score/quality/strength/confidence/capability/seniority/employability;
`RoleEvidenceAlignment` out of scope.

Implemented in Domain as of Session 075 (worklog/sessions/2026-08-19-075-...md):
`RepresentativeRepositorySelectionPolicy.Select(ProfilePatternSet)` returns a
`RepresentativeRepositorySelection`. Candidate evidence (repository, scoped
`ProfileEvidenceReference` list, `PatternSupportObservationCount`) is computed
once per candidate during selection; the winner's already-computed evidence
is carried directly into `RepresentativePatternSupport`, which never reads
`Pattern.SupportingObservations` to derive or re-derive evidence.
`RepresentativePatternSupport` also rejects at construction any repository
outside `IndependentSupportingRepositories`, and
`RepresentativeRepositorySelection` rejects at construction: a pattern
represented under two different repositories, a referenced pattern whose
`PatternRuleVersion`/`ObservationRuleVersion` does not match the selection's
own, and a post-grouping Owner+Name/Url canonical-identity collision.
`SupportingObservationKeys` is a get-only computed property re-derived from
`SupportingEvidenceReferences` on every access — never a stored field, so it
cannot be mutated into divergence. `RepresentativeRepositoryRuleVersion.V1 =
"1.0.0"` is the first canonical Domain runtime rule version. 54 new Domain
tests cover the full locked invariant set (empty-set validity, grouping
cardinality, candidate restriction, comparison/tie-break, the
canonical-identity rejection boundary at both the pre-selection and
post-grouping level, the two-trace separation, all six §13 ordering rules,
version/completeness preservation, cross-pattern independence, the
SupportingObservationKeys projection, V1, a reflection guard against
prohibited fields). Final closure review: PASS WITH FINDINGS (1 MEDIUM,
worklog-only, resolved). No `RepresentativeRepositoryRuleVersion` runtime
value has been wired into any running composition yet — using `V1` in an
actual Application flow is a later Application/composition-root decision,
not made by this Domain-only session.

## Blocker

NONE for this stage. ProducedWorkEvidence remains unresolved and
RepositoryProblemAreaEvidence remains blocked by the Session 067 contract.

## Validation Baseline

Session 095 (last code change): 1448/1448 PASS across the full solution
(Domain 798, Application 246, Infrastructure 259, Web 145 [83 baseline + 62
new/updated])
Full solution build (GitHubProof.slnx): 0 warning / 0 error

Note: builds/tests in Session 095 were run with `-c Release`, because the
user's running `GitHubProof.Web` dev server held a lock on the Debug output
for the whole session. Same sources, same result; independently confirmed by
the reviewer.

AnalyzerVersion:
1.1.0

RuleSetVersion:
UNCHANGED

ReportSchemaVersion:
UNCHANGED

DependencyCategoryMappingVersion:
1.0.0

SourceReferenceRuleVersion:
1.0.0 — established by Session 073

RepresentativeRepositoryRuleVersion:
1.0.0 (V1) — established by Session 075 as the first canonical Domain
runtime rule version. Wired into the running Application composition path
(`ProfilePipeline`) as of Session 088.

ObservationRuleVersion:
1.0.0 (V1) — established by Session 088 as the first canonical Domain
runtime observation rule version, wired into the running Application
composition path.

PatternRuleVersion:
1.0.0 (V1) — established by Session 088 as the first canonical Domain
runtime pattern rule version, wired into the running Application composition
path.

EligibilityPolicyVersion (profile composition):
1.0.0 (`EligibilityPolicy.V1`) — established by Session 084, wired into the
running Application composition path as of Session 088.

ProfileAnalyzerVersion:
1.0.0 (`ProfileAnalyzerVersion.V1`) — established by Session 085, wired into
the running Application composition path as of Session 088.

## Next Action

P8 is CLOSED (Session 088). A bounded post-review correction (1 MEDIUM
finding) was corrected and validated in-session, and the external closure
audit passed (see the Session 088 status entry above and session file):

~~~text
P8-I1 Final Repository Eligibility Runtime:  COMPLETE
P8-I2 Profile Root Foundation:               COMPLETE
P8-I3 Deterministic Profile Composer:        COMPLETE
P8-I4 Typed Profile Report and Projector:    COMPLETE
P8-I5 Integration and P8 Closure:            COMPLETE

P8 — Deterministic Profile Composer and Report Contract:  COMPLETE
~~~

`ProfilePipeline` (Application, `src/GitHubProof.Application/Profile/`) is
now the one running composition path from `GitHubProfileSnapshot` to
`DeveloperPublicEvidenceProfile` + `ProfileReport`, wired behind
`AddProfilePipeline` (Web, `src/GitHubProof.Web/Profile/`). See the Session
088 status entry above and its session file for the full record.

P10 contract/planning is CLOSED (Session 093, docs-only), P10-I1 is COMPLETE
(Session 094) and P10-I2 is COMPLETE (Session 095 — see "P10 Phase State"
above and
`worklog/sessions/2026-08-20-095-p10-i2-profile-entry-routing.md`):

~~~text
P10 contract/planning:                COMPLETE
P10-I1 Live Profile Request Boundary: COMPLETE
P10-I2 Profile Entry + Routing:       COMPLETE
P10-I3..I6:                           NOT STARTED

Profile runtime collection contract:  CONTRACT LOCKED (Session 096)
Profile runtime implementation:       PASS WITH FINDINGS (Session 097)
Controlled live smoke:                FAILED
Profile runtime readiness:            BLOCKED
Profile execution guard:              NOT IMPLEMENTED
~~~

Exact next action:

~~~text
Session 098 — Profile Decision-Sufficient Pagination Contract
(bounded CONTRACT AMENDMENT / DECISION session — NOT implementation)
~~~

Session 098 must decide how Profile Proof may stop required-surface pagination
**only** when the existing V1 semantic decision has already become
mathematically irreversible, without pretending the underlying collection is
complete.

Locked constraints for that session:

~~~text
- no fake IsComplete = true
- no arbitrary hard page cap
- no sampling and no approximation
- no concurrency-as-cost-fix
- Repository Proof unchanged
- fork semantics are a separate product decision and are NOT changed by 097
- contributors omission requires an explicit amendment of Session 096 §5.4
- commits need a contract change before any implementation
~~~

Still true and unchanged by the live failure: the §10 execution guard has no
numeric value, and P10-I3 (Meaning / Direction / Starting Point) remains the next
P10 slice. Neither can proceed usefully before Session 098, because no §9
measurement exists yet — the live run aborted before any telemetry line was
produced.

Carried into later P10 slices (recorded in the Session 095 file, not
blocking P10-I3):

- P10-I5 — the rate-limit failure message still exposes the technical
  `GITHUB_TOKEN` environment-variable name to end users. The typed
  `RateLimited` state itself is correctly preserved; only the user-facing
  wording is provisional.
- P10-I6 — LOW cleanup: the unified input still uses the `.repository-input*`
  CSS block and `wwwroot/css/repository-input.css`, naming a surface that now
  also accepts profile identities. Deferred deliberately to avoid touching
  the just-accepted visual composition.

Production guardrails remain separately pending before the Milestone B
production gate, unchanged by Session 095: a concurrency guard for profile
analysis (it fans out across the account's admitted repositories), and GitHub
request / rate-limit telemetry. P10-I2's POST-only `/profile` reduces
accidental analysis triggering (GET, refresh-to-GET, prefetch, crawler, link
preview) but replaces neither guardrail. Session 096 locked the canonical
contract for both (`docs/architecture/public-profile-runtime-collection.md` §9
telemetry, §10 bounded end-to-end execution guard, §10.1 bounded
concurrent-profile-execution policy). Session 097 implemented the §9 telemetry
and removed the unnecessary request amplification; the §10 execution guard and
the §10.1 concurrency policy remain **NOT IMPLEMENTED**, and their numeric
values are deliberately deferred until the controlled live measurement.

P9 — Public Profile Evidence Validation (Validation Profile Set) is
**COMPLETE**:

~~~text
P9-I1 — Validation Contract + Scenario Matrix:      COMPLETE (Session 089, 4a1bc5a)
P9-I2 — Profile Validation Fixtures / Harness:      COMPLETE (Session 090, c195dc9)
P9-I3 — Profile Risk and Traceability Validation:   COMPLETE (Session 091, 8d07321)
P9-I4 — Determinism + Exit Validation:              COMPLETE (Session 092)

Profile P9 — Validation Profile Set:  COMPLETE
~~~

Final verified exit criteria (Session 092; see
`docs/roadmap/mvp-phases.md` "P9 Validation Durumu" and
`worklog/sessions/2026-08-20-092-p9-i4-determinism-exit-validation.md`):
`prohibited inference = 0`, `unsupported profile claim = 0`, evidence
traceability satisfied for every meaningful claim across all 11 scenarios,
`determinism = PASS` across all 11 scenarios (replay + repository-order
invariance). No production contract violation was found at any point across
P9; `src/` was never changed across P9-I1 through P9-I4.

P10 implementation is **in progress**: P10-I1 and P10-I2 are COMPLETE, and
P10-I3 through P10-I6 are NOT STARTED (see "P10 Phase State" above — this
sentence restates that state, it does not change any slice). Session 093
(docs-only) made implementation READY by locking the six slices and their
decisions; Sessions 094 and 095 then delivered the first two. Milestone B's
production gate additionally requires `P10 — Profile Web Flow COMPLETE`
(`docs/roadmap/deployment-milestones.md`) — Milestone B therefore remains
BLOCKED pending the remaining P10 slices, unaffected by P9's or P10
contract-closure's completion.

Known remaining work, none of it started: `RoleEvidenceAlignment` taxonomy
and rules (DEFERRED, not blocking — see P7 closure below); `ProducedWorkEvidence`
remains unresolved; `RepositoryProblemAreaEvidence` remains blocked by the
Session 067 contract. Neither is required for P9 or P10.

P7 closure state (Session 076/077, unaffected by P8):

~~~text
Representative Repository:
COMPLETE

RoleEvidenceAlignment:
DEFERRED — NOT_READY_WITH_CURRENT_EVIDENCE

P7 V1:
COMPLETE WITH EXPLICIT DEFERRAL
~~~

DEFERRED means: not implemented in Profile V1 / Milestone B, not required,
not negative evidence, not permanently prohibited — feasibility may be
reopened later if the canonical evidence surface becomes strong enough. No
upstream evidence expansion is required or started by this status.
