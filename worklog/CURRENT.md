# GitHub Proof — Current Development State

**Last Updated:** 2026-08-25 (Session 123)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 123 — GraphQL Partial-Data / Error Contract (**S0**) —
  **PASS**. §18 (13 subsections) and §19 (17 acceptance criteria) are locked in
  `public-profile-runtime-collection.md`, with Session 123 notes in §9.1, §11
  and §13. Independent review returned CHANGES REQUIRED; the architecture owner
  accepted all six findings and all six were closed in-session and re-reviewed.
  Canonical docs + worklog only — no code, no runtime value, no performance
  claim. **CLOSED / awaiting final integration** — the commit is user-owned and
  still pending on branch `session/123-graphql-partial-data-error-contract`
  (base `5044b03`, route Opus / xhigh). **Not resumable**; further work opens a
  new session.
  Detail: [`sessions/2026-08-25-123-graphql-partial-data-error-contract.md`](./sessions/2026-08-25-123-graphql-partial-data-error-contract.md)
- **Previous Session:** 122 — Profile REST Request Redundancy Audit — **PASS**,
  verdict **REST-R3 — no material redundancy**, independent review PASS WITH
  FINDINGS (1 HIGH, 2 MEDIUM, 1 LOW, all corrected); **integrated as `5044b03`**.
  Detail: [`sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md`](./sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md)
- **The Profile runtime measurement phase is COMPLETE.** Three live benchmark
  shapes were measured; the full evidence, including the complete per-category
  request breakdowns, is persisted in the Session 120 record. No further
  benchmark is planned.
- **No production runtime value has been selected.** No execution deadline, no
  request budget, no GitHub reserve floor, no concurrency change, no contents
  bound. Session 120 owns measurement and diagnostic evidence only; Sessions 121
  and 122 own feasibility and audit analysis only; Session 123 owns a contract
  decision only.
- **Last integrated commit on `main`:** `5044b03` (Session 122).
- `?? artifacts/` is user-owned, read-only and out of scope.

## Position

- **P10 — Profile Web Flow: COMPLETE.** Recorded by architecture-owner verdict
  in Session 118 against the sole canonical exit owner,
  `docs/product/p10-web-profile-flow-implementation-plan.md` §11. All four
  success criteria PASS; I1 through I6 covered; product invariants,
  traceability and presentation/accessibility all PASS.
  **No remaining P10 blocker.**
- **Milestone B — Profile Proof Beta: IN PROGRESS, not COMPLETE.**
  `docs/roadmap/deployment-milestones.md` §3 counts P10 COMPLETE as an *input*
  to Milestone B; §4 reserves milestone COMPLETE for production deploy + smoke
  + remote production verification. None of that has happened.
- **Repository Proof first-read visual alignment: COMPLETE for this bounded
  slice.** Repository Proof already shared the design system; the one concrete
  canonical violation was first-read score dominance. The score card left the
  first-read layer, which is now exactly Proves → Weakens → Do First. Score
  semantics are unchanged and the score still renders in its canonical
  Categories + Score surface. **No unresolved Repository Proof accessibility
  defect.** Measure/breakpoint unification, the identity card and the Profile
  radius-token cleanup were deferred by owner decision.
- **Profile runtime measurement: COMPLETE** (Session 120). Repository collection
  dominates Profile wall-clock at 95-99.5% in every measured shape; runtime is
  materially affected by admitted repository count x repository weight. Three
  operational findings are locked: **A-T** (pagination telemetry classification)
  CLOSED; **D4-E2** — contents amplification is real but no bound is justified
  by current measurements, and the source-level mechanism stays a **monitored
  production risk, not "no risk"**; **B-RI** — rate-limit telemetry lacked the
  context to reason about D-2, which is *not* a claim that GitHub's headers are
  unreliable. Production values remain unselected.
- **GraphQL request consolidation: GQL-F2 ACCEPTED, not implemented**
  (Session 121). Verdict **GQL-F2 — feasible, contract amendment required**. A
  hybrid (GraphQL repository envelope over admitted repositories + REST for the
  recursive tree, README, license and file contents) can consolidate 9 of the
  12 per-repository REST round trips; projected 2.3x-3.7x fewer HTTP round
  trips on the three measured shapes, **round trips only, no wall-clock
  claim**. Its blocking gap — an HTTP 200 carrying partial `data` **and**
  `errors` had no canonical `DataAvailability` mapping — is **CLOSED by Session
  123 §18**; it is no longer open. Session 121's three decision-relevant
  findings were all resolved by that amendment: `issues` stays REST, so its
  `DecisionBounded` evidence-value difference cannot arise; `isEmpty` is not
  adopted, so eligibility semantics are unchanged; and the repository manifest
  stays REST, so the manifest completeness warning keeps its independent source.
  What remains open is **ordering parity** for the adopted `DecisionBounded`
  surfaces (§18.2, §19.7). GraphQL is **not** a Milestone B exit criterion and
  must not become one. **Still no GraphQL code and nothing implemented.**
- **REST request redundancy: AUDITED, nothing to implement** (Session 122).
  Verdict **REST-R3 — no material redundancy**. The Profile REST graph is
  2 profile-level requests plus a 12-request per-repository floor, and the
  proven removable set is **0** on all three measured shapes (131 / 662 / 63
  unchanged) — request counts only, no wall-clock claim. Nothing was found that
  can be removed while preserving availability and failure truth; the three
  existing reduction mechanisms (omitted surfaces, the `has_issues` gate, the
  §15 decision boundaries) have already taken what was available. Two routes
  remain **R4 — contract decision required**, each worth ~1 request per admitted
  repository: seeding repository metadata from the manifest, and taking the head
  SHA from the commits surface. Both are blocked on availability/failure-key
  semantics, not on missing fields. **No `ProfileRepositoryCollectionContext`
  and no REST slice is justified.** One source defect was found and deliberately
  not fixed: the `jest.config.*` / `vitest.config.*` project-file prefixes can
  overlap the workflow and documentation selectors, producing a duplicate
  content request for the same path — zero incidence in the measured shapes,
  worth 0 requests to fix, and owed its own bounded session.
- **GraphQL partial-data / error contract: LOCKED** (Session 123, **S0
  complete**). `public-profile-runtime-collection.md` §18 says GraphQL is
  **transport only** and may never change evidence semantics; fixes the Hybrid V1
  split (GraphQL candidates: repository metadata/topics, default-branch head and
  commit/tree identity, languages, commits, pull requests, releases, tags — REST
  retained: profile metadata, repository manifest, **issues**, recursive tree,
  README, license/file, project/workflow/docs contents); does **not** adopt
  `isEmpty`; and closes the blocking gap — an HTTP 200 carrying `errors` may
  never become successful absence, `Missing`, zero evidence or developer truth.
  Error isolation is per-surface REST fallback when attribution is
  deterministic, whole-envelope discard when it is ambiguous, and a run-scoped
  GraphQL disable on envelope-level failure. Fallback is **not** an availability
  failure; **no new `DataAvailabilityStatus`** and no GraphQL-specific evidence
  status. §§10-11 cancellation/deadline/budget semantics are untouched. §19 adds
  17 acceptance criteria for the first implementation slice. Review-driven
  additions, all owner-accepted: **§18.12** — GraphQL `totalCount` and any
  exact-total field may never reach `TotalCount`, a `DecisionBounded` evidence
  value or an exact-total claim; **§18.13** — `defaultBranchRef == null` may not
  be read blindly as empty, and the head/commit-tree and commits surfaces are
  adoptable only if the GraphQL empty shape provably reproduces REST
  successful-empty semantics and stays distinguishable from failure; **§18.9** —
  one physical GraphQL POST counts exactly once, never once per surface;
  **§18.7** — one envelope, one `RetrievedAt`, captured only after the body is
  accepted; **§18.2** — field-mapping parity is an adoption condition;
  **§19.14** — rollout is gated on a live A/B parity run on the three measured
  profile shapes plus owner approval. **Still no GraphQL code, no runtime value
  and no performance claim.**
- **Milestone B remains IN PROGRESS.**

## Locked Decisions

- **Profile trace is deliberately two-stage** (architecture owner, Session 114).
  `WhyThisConclusion` names supporting repository identities;
  `RepositoryDrillDown` owns the single Repository Proof POST per repository.
  A direct action inside a Why entry is **not** introduced. Owned by
  `p10-presentation-language-contract.md` §8.6.3–§8.6.5.
- Shared text-contrast accessibility is closed. Light `--fg-tertiary` and both
  themes' `--fg-muted` were remapped in `tokens.css` only; the canonical UI
  System doc was reconciled to match. `CssArchitectureTests` owns five
  contrast/token guards.

## Separate Follow-up — context routing

Sessions 116-117 initially missed
`docs/product/github-proof-ui-system-light-dark-v1.md`, a canonical owner for
shared theme/token/accessibility semantics. The canonical contradiction is
**already resolved at HEAD**, so this is not a product blocker — it is a
process / knowledge-routing concern awaiting its own bounded session. No
agent-architecture file has been changed for it.

## Open Decisions — Milestone B production gate

None of these blocks development work; all block a production PASS.

| Open item | State |
|---|---|
| Visitor rate limit `2/10 min` | unselected candidate; permit/window unset, so IP limiting is disabled |
| Trusted proxy / forwarded-header topology | deployment-owner input required; no forwarded-header parser exists |
| `ProfileExecutionDeadline:Timeout` | deliberately unset, no fallback; must exceed the 2-minute transport timeout if configured |
| `GitHubApiBudget:ProfileAdmissionRemainingFloor` | unset/disabled; `500` is a measurement candidate, not a decision |
| Profile concurrency `1/1`, queue `0` | implemented as provisional, externally configurable defaults |
| Beta credential | machine-user fine-grained PAT vs GitHub App undecided; neither rejected |
| Shared circuit behavior beyond Profile | unauthorized without a separate decision |
| Production telemetry / deployment / real-user smoke / failure monitoring / remote verification | Milestone B work, not started |

## Residual Risks (recorded, not blockers)

| Item | State |
|---|---|
| Profile pixel review | presentation validation has not been closed by a final human pixel-level sign-off. Session 111 included real human visual review that returned CHANGES REQUIRED; no final human visual PASS is recorded. I6-B/I6-C evidence is measured layout geometry and computed style |
| Real analyzed Profile render | not captured since Session 111 — no `GITHUB_TOKEN`, unauthenticated quota 60/hour. Live smoke belongs to the Milestone B gate |
| `--fg-muted` AA margin headroom | margins over 4.5 are +0.047 (light on `--bg-canvas`) and +0.036 (dark on `--surface-default`). Some surfaces sit below AA, lowest dark `--surface-raised` / `--surface-hover` at 4.190, but no non-exempt semantic consumer renders on any of them. Future semantic `--fg-muted` use on a new background needs its own contrast validation |
| P10-I6-A closure record | `INDEX.md` row 111 reads `PASS` per architecture-owner adjudication, while `sessions/2026-08-23-111-...md` still reads "acceptance remains required" |

## Deferred Canonical Gaps — non-blocking for P10

RECENCY · SHAREABLE PROFILE URL · SOCIAL PREVIEW · Project-Type Presentation ·
Starting Point Prioritization · Empty Limitations. All six were classified in
Session 118 from canonical ownership: four are prohibitions being honoured, one
is required behaviour being sustained, one is an unauthorized future design.
None is unfinished required P10 implementation.

## Open Runtime Decisions — for the Profile Production Runtime Decision session

| Decision | State at Session 120 exit |
|---|---|
| D-1 execution deadline | measurement complete; product/production decision required |
| D-2 reserve / request protection | telemetry context refined; the guard premise needs an architecture decision |
| D-3 run-time provider-limit behaviour | design decision ready; no natural limit event observed |
| D-4 contents fan-out | D4-E2 locked — monitored risk, no bound justified |
| D-5 credential strategy | ready; measured cost 63 / 131 / 662 requests per profile shape |
| D-6 secondary rate-limit response shape | OPEN; intentionally not provoked |

GraphQL stays a separate performance-architecture question and must not be
folded into these guard decisions. Session 121 answered feasibility only
(**GQL-F2**) and Session 123 locked the contract (**§18/§19**); neither selected
a guard value, and §18.11 explicitly selects none. Two couplings still matter to
D-2 and D-5: `POST /graphql` would become one more used endpoint whose
credential compatibility §10.4 already requires to be verified — and the code
path's unauthenticated REST tolerance would disappear. The second former
coupling is now closed: §18.2 keeps profile metadata on REST precisely so the
D-2 budget premise is unchanged.

## Exact Next Action

Session 123 is closed; its commit is user-owned and still pending. All six
review findings were owner-accepted and closed, so **S0 is complete**. The
immediate item is architecture-owner final acceptance of the locked amendment.
The queue is:

- **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) — next
  session. It was queued as 123 and moved by explicit owner prioritization of
  S0.
- **First GraphQL implementation slice** — now possible for the first time,
  gated by §19's 17 criteria. Session 121's S1 (a read-only transport client
  behind its own interface, no collector wired, default off) is the smallest
  slice consistent with them.
- **Unscheduled:** a bounded session for the Session 122 §6.1 content-selector
  duplicate-fetch defect. Correctness only; it saves 0 requests.

Each opens through the normal model/effort gate. The Milestone B production gate
remains available as the architecture owner's alternative order.
