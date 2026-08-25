# GitHub Proof — Current Development State

**Last Updated:** 2026-08-25 (Session 122)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 122 — Profile REST Request Redundancy Audit — **PASS**,
  verdict **REST-R3 — no material redundancy**; independent review
  **PASS WITH FINDINGS** (1 HIGH, 2 MEDIUM, 1 LOW, all corrected in-session).
  **CLOSED / awaiting final integration** — read-only audit, worklog was the
  only mutation, and the commit is user-owned and still pending on branch
  `session/122-profile-rest-request-redundancy-audit` (base `3b77600`, route
  Opus / high). **Not resumable**; further work opens a new session.
  Detail: [`sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md`](./sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md)
- **Previous Session:** 121 — GraphQL Request Consolidation Feasibility —
  **PASS**, verdict **GQL-F2 ACCEPTED**, independent review PASS WITH FINDINGS
  (4 LOW, all corrected); **integrated as `3b77600`**.
  Detail: [`sessions/2026-08-25-121-graphql-request-consolidation-feasibility.md`](./sessions/2026-08-25-121-graphql-request-consolidation-feasibility.md)
- **The Profile runtime measurement phase is COMPLETE.** Three live benchmark
  shapes were measured; the full evidence, including the complete per-category
  request breakdowns, is persisted in the Session 120 record. No further
  benchmark is planned.
- **No production runtime value has been selected.** No execution deadline, no
  request budget, no GitHub reserve floor, no concurrency change, no contents
  bound. Session 120 owns measurement and diagnostic evidence only; Sessions 121
  and 122 own feasibility and audit analysis only.
- **Last integrated commit on `main`:** `3b77600` (Session 121).
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
  claim**. Blocking gap: GitHub GraphQL can answer HTTP 200 with partial `data`
  **and** `errors`, and no canonical section maps that onto
  `DataAvailability` — today it would silently become positive evidence.
  Three further findings are non-blocking but decision-relevant: `issues`
  carries a `DecisionBounded` evidence-**value** difference (repository level
  only, no profile output change); adopting `isEmpty` would change current
  **eligibility** semantics; and the manifest completeness / count-consistency
  warning could stop firing if both sides came from the same GraphQL
  connection. Semantic preservation is therefore **qualified, not blanket**.
  GraphQL is **not** a Milestone B exit criterion and must not become one.
  **No S0, no code, nothing implemented.**
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

## Open Runtime Decisions — for Session 123

| Decision | State at Session 120 exit |
|---|---|
| D-1 execution deadline | measurement complete; product/production decision required |
| D-2 reserve / request protection | telemetry context refined; the guard premise needs an architecture decision |
| D-3 run-time provider-limit behaviour | design decision ready; no natural limit event observed |
| D-4 contents fan-out | D4-E2 locked — monitored risk, no bound justified |
| D-5 credential strategy | ready; measured cost 63 / 131 / 662 requests per profile shape |
| D-6 secondary rate-limit response shape | OPEN; intentionally not provoked |

GraphQL request consolidation stays a separate performance-architecture
question and must not be folded into these guard decisions. Session 121 answered
its feasibility only (**GQL-F2**); it selected no guard value. Two couplings are
recorded there and matter to D-2 and D-5: `POST /graphql` would become one more
used endpoint whose credential compatibility §10.4 already requires to be
verified — and the code path's unauthenticated REST tolerance would disappear —
and moving profile metadata off REST would silently change the D-2 budget
premise.

## Exact Next Action

Session 122 is closed; its worklog commit is user-owned and still pending.
Architecture-owner decision on the REST-R3 verdict is the immediate item. If it
is accepted, no REST implementation slice opens and the queue is:

- **Session 123 — Profile Production Runtime Decision Design** (D-1, D-2, D-3,
  D-5) — next.
- **S0** — write the GraphQL partial-data/error availability mapping amendment
  into `docs/architecture/public-profile-runtime-collection.md` (no code) —
  authorized from Session 121, not started, and still the gate for any GraphQL
  slice.
- **Unscheduled:** a bounded session for the Session 122 §6.1 content-selector
  duplicate-fetch defect. Correctness only; it saves 0 requests.

Each opens through the normal model/effort gate. The Milestone B production gate
remains available as the architecture owner's alternative order.
