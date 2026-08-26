# GitHub Proof — Current Development State

**Last Updated:** 2026-08-26 (Session 128)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 128 — GraphQL DecisionBounded Ordering Parity — **PASS**.
  Branch `session/128-graphql-decision-bounded-ordering-parity`, base `8f42a6f`,
  route Opus / high. A proof/decision session, not an implementation one. Verdict
  **PARITY-BLOCKED**: no `DecisionBounded` surface — commits, pull requests,
  releases, tags — can meet §19.7 from current provider guarantees, and all four
  are ORDER-BLOCKED. Test-only change; build 0 warnings, **2247/2247**. Closure
  review **PASS WITH FINDINGS** (0 blocker, 0 high, 3 MEDIUM, 3 LOW), all six
  corrected — the review found a real hole in the tags proof, which is why tags
  is ORDER-BLOCKED rather than ORDER-PASS. The owner accepted PARITY-BLOCKED and
  **withdrew one further claim the record had made about REST pagination**; no
  verdict changed. **CLOSED / awaiting final integration** — the commit is
  user-owned and still pending. **Not resumable**; further work opens a new
  session.
  Detail: [`sessions/2026-08-26-128-graphql-decision-bounded-ordering-parity.md`](./sessions/2026-08-26-128-graphql-decision-bounded-ordering-parity.md)
- **Previous Session:** 127 — S3 Hybrid Repository Envelope — **PASS**;
  **integrated as `8f42a6f`**. Branch
  `session/127-graphql-hybrid-repository-envelope`, base `09707f4`, route
  Opus / high. The first **authoritative** hybrid slice: one GraphQL envelope
  supplies repository metadata, topics and languages, replacing the REST
  repository-metadata GET and the REST languages GET for every repository whose
  candidate is accepted — one fewer physical provider request. **DEFAULT OFF.**
  Build 0 warnings, **2238/2238**. Closure review **PASS WITH FINDINGS**
  (0 blocker, 0 high, 3 LOW), all three corrected; owner accepted with no
  production or test change requested. Remains **DEFAULT OFF** with no
  production, beta, default-on or live-A/B approval.
  Detail: [`sessions/2026-08-26-127-graphql-hybrid-repository-envelope.md`](./sessions/2026-08-26-127-graphql-hybrid-repository-envelope.md)
- **Previous Session:** 126 — GraphQL Provider Failure Classification — **PASS**;
  **integrated as `09707f4`**. Branch
  `session/126-graphql-provider-failure-classification`, base `5a4bae5`,
  route Opus / high. Verdict **CLASS-PARTIAL**, owner-accepted; build 0
  warnings, **2069/2069**. Closure review PASS WITH FINDINGS (0 blocker, 0 high),
  all corrected.
  Detail: [`sessions/2026-08-26-126-graphql-provider-failure-classification.md`](./sessions/2026-08-26-126-graphql-provider-failure-classification.md)
- **Previous Session:** 125 — S2 GraphQL Languages Shadow Parity — **PASS**;
  **integrated as `5a4bae5`**. Branch
  `session/125-graphql-languages-shadow-parity`, base `bf5fc8e`, route
  Opus / high. Build 0 warnings, **2033/2033**. Three review rounds, every
  finding closed across three review rounds (F-1..F-6, G-1..G-5,
  M-1/L-1/L-2/L-3).
  Detail: [`sessions/2026-08-25-125-graphql-languages-shadow-parity.md`](./sessions/2026-08-25-125-graphql-languages-shadow-parity.md)
- **Previous Session:** 124 — S1 GraphQL Transport Client — **PASS**.
  Independent review PASS WITH FINDINGS; the owner rejected F-2 with reason and
  accepted F-3 through F-6, all closed in-session and re-reviewed;
  **integrated as `bf5fc8e`**.
  Detail: [`sessions/2026-08-25-124-graphql-transport-client.md`](./sessions/2026-08-25-124-graphql-transport-client.md)
- **Earlier:** 123 — GraphQL Partial-Data / Error Contract (**S0**) —
  **PASS**; §18 (13 subsections) and §19 (17 acceptance criteria) locked in
  `public-profile-runtime-collection.md`, review CHANGES REQUIRED with all six
  owner-accepted findings closed in-session; **integrated as `2016501`**.
  Detail: [`sessions/2026-08-25-123-graphql-partial-data-error-contract.md`](./sessions/2026-08-25-123-graphql-partial-data-error-contract.md)
- **The Profile runtime measurement phase is COMPLETE.** Three live benchmark
  shapes were measured; the full evidence, including the complete per-category
  request breakdowns, is persisted in the Session 120 record. No further
  benchmark is planned.
- **No production runtime value has been selected.** No execution deadline, no
  request budget, no GitHub reserve floor, no concurrency change, no contents
  bound. Session 120 owns measurement and diagnostic evidence only; Sessions 121
  and 122 own feasibility and audit analysis only; Session 123 owns a contract
  decision only; Session 124 owns an unused transport only; Session 125 owns a
  default-off observation only; Session 126 owns a transport classification
  only; Session 127 owns a default-off authoritative transport consolidation
  only — no evidence, score, status or report value changes with it; Session 128
  owns a transport-feasibility verdict only and adopts nothing.
- **Last integrated commit on `main`:** `8f42a6f` (Session 127).
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
- **GraphQL transport S1: IMPLEMENTED, UNUSED** (Session 124). A Profile-only,
  read-only `IGitHubGraphQlTransport` exists on the **existing** profile named
  client — no second HttpClient, no credential source, no package, no GraphQL
  framework. It makes §18.4's four states structurally distinguishable
  (`Success` / `DataWithErrors` / `EnvelopeFailure` / `Malformed`), so an
  HTTP 200 carrying `errors` and an unreadable body can never become successful
  absence. GraphQL errors are normalized to a **count** — no message, path or
  extension is retained anywhere. It produces **no `DataAvailability` and no
  domain status** (§18.7); availability stays with the future surface adapter.
  Telemetry gains exactly one closed-vocabulary member, `graphql` (17 -> 18),
  and one physical POST counts once in total and once in that category,
  **never once per surface** (§18.9). Caller cancellation propagates and is
  never converted into a failure result. **Nothing uses it:** the snapshot
  source still composes the exact REST set, no Infrastructure type takes the
  dependency, Repository Proof is untouched, and the D-2 profile-metadata budget
  observation is unchanged. Full suite **1953/1953**, 0 warnings. **No S2, no
  production query, no runtime value, no performance claim.**
- **GraphQL languages shadow S2: IMPLEMENTED, DEFAULT-OFF** (Session 125). The
  first real REST ↔ GraphQL parity observation, on `languages`, behind
  `ProfileGraphQlShadow:LanguagesShadowEnabled` (**false** by default). Off
  constructs no shadow at all, so the Profile request graph and result are
  exactly what they were. On, the seam is one optional argument to the Profile
  composition factory that wraps the descriptive collector: it calls REST as
  today, observes on the side, discards the outcome and returns **the same
  collection instance** — so the languages map, the `languages`
  DataAvailability and its `RetrievedAt` remain REST's own. Complete-map parity
  with cursor pagination (no cap, no sampling); **no `totalCount`**, no
  `isEmpty`, no unrelated field requested; ambiguity (duplicate language,
  negative size, partial `errors` envelope, malformed body, non-advancing
  cursor) **abstains** rather than applying last-wins. **No
  `DataAvailability`, no domain status, no new telemetry vocabulary** — one POST
  per page counts once in the existing `graphql` category. Match, mismatch,
  failure and even a throwing shadow all leave the `ProfileReport` identical,
  proven end to end. Full suite **2033/2033**, 0 warnings. Review-driven additions, all
  owner-accepted: **§18.6 execution-scoped disable** — an `EnvelopeFailure` or
  `Malformed` envelope disables the shadow for the rest of that Profile
  execution, so a systemic fault costs one POST per run instead of one per
  admitted repository, and the disable provably cannot leak into another
  execution; and **bounded §18.9 shadow telemetry** — Match / Mismatch /
  NotCompared counts plus a `GraphQlDisabledForRun` boolean, never request
  counts, never evidence, never carrying identity or content. **Open gap:**
  `DataWithErrors` cannot participate in the §18.6 disable, because S1
  normalizes errors to a count and always reports HTTP 200 and so exposes no
  bounded structural discriminator between a provider-wide auth/rate-limit
  failure and ordinary partial data. A `DataWithErrors` envelope *can* carry a
  null `data`, which is a bounded structural fact, but it does not **prove** an
  auth failure, a rate limit or any provider-wide fault — an ordinary top-level
  query error looks identical — so it does not disable the run.
- **S2 is NOT approved for enablement** (Session 125, owner ruling). The
  mechanics are proven and the shadow stays **default-off**; because S1 cannot
  yet classify a provider-wide GraphQL failure carried as HTTP 200 + `errors`,
  `LanguagesShadowEnabled` is **not** approved for production, beta, live A/B
  (§19.14) or default-on use. A separate bounded session must first establish a
  proven GraphQL error-kind classification from provider-supported structural
  information. This is an **operational enablement blocker, not evidence or
  domain drift**: off, the Profile path is exactly the pre-S2 path; on, REST
  stays authoritative either way.
- **GraphQL provider-failure classification: CLASS-PARTIAL** (Session 126).
  An HTTP 200 envelope carrying `errors` is now treated as §18.6 provider-wide
  **only when a response header says so**: `x-ratelimit-remaining: 0`
  (primary rate-limit exhaustion) or a valid `Retry-After`. Both disable the
  GraphQL shadow for the rest of that Profile execution; everything else stays a
  per-repository abstention with REST authoritative. §18.6 already named a
  rate-limit response as envelope-level, so **no canonical change was needed**.
  Classification is header-only — the transport makes zero `GetString()` calls
  and never reads `message`, `type`, `path` or `extensions`; a malformed or
  repeated header yields no signal; `x-ratelimit-resource: graphql` is never a
  signal; and a **successful** envelope is never demoted by a header. GraphQL
  `remaining`/`reset`/`Retry-After` reach no `ProfileGitHubBudgetState`, no
  §10.4 decision and no Domain result, and one physical POST still counts once.
  **Residual blocker, named:** exactly **one** §18.6 shape can still arrive as
  HTTP 200 + `DataWithErrors` with the condition stated only in the error
  message — the secondary rate limit GitHub identifies that way. It has no
  documented structural discriminator, so it stays a per-repository abstention
  (extra requests, never wrong evidence). Authentication and authorization are
  **not** residual: 401 and 403 already reach the non-200 envelope-level path,
  and §18.6 is unchanged. So
  **`LanguagesShadowEnabled` stays DEFAULT OFF and is still not approved** for
  production, beta, §19.14 live A/B or default-on use.
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

Session 128 is closed; its commit is user-owned and still pending. **No
production, beta, default-on or live-A/B approval exists for any GraphQL
switch.** The queue:

- **The DecisionBounded surfaces stay REST.** Commits, pull requests and
  releases each lack a published REST ordering contract (or, for pull requests, a
  published tie-break). Tags is blocked for a different reason worth keeping
  visible: `GitHubRepositoryTagCollector` deduplicates tag names with
  `OrdinalIgnoreCase` while Git refs are case-sensitive, so the observed count —
  and therefore `tagsAtLeast` — is order-dependent for a repository with more
  than one page of tags and a case-variant pair. That is a **latent
  order-dependence in the existing REST path**, not something GraphQL
  introduced; changing the comparer to `Ordinal` would change existing REST
  results and is an owner decision.

- **The residual message-only shape** — a secondary rate limit arriving as
  HTTP 200 + `DataWithErrors` with `remaining > 0`, no `Retry-After`, and its
  identity available only through the error message. Until it has a
  provider-supported structural discriminator, **neither** the languages shadow
  **nor** the S3 repository envelope may be enabled in production, in beta, for
  the §19.14 live A/B validation, or by default.
- **The S3 live-parity items** — `homepageUrl` versus REST `homepage`, plus
  `primaryLanguage` and `description`, which are part of the same rollout
  validation surface. The mapping is pinned by test; **provider-level equality
  must not be claimed before live evidence**, so this belongs to the §19.14
  comparison.
- **S4 — batching / aliases.** Not started, and not opened by S3 or S128.
- **Default-branch head / tree** still needs the §18.13 empty-repository
  re-proof. Commits depend on it twice over, since `Commit.history` is only
  reachable through the default branch ref target.
- **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) — still
  pending.
- **Unscheduled:** a bounded session for the Session 122 §6.1 content-selector
  duplicate-fetch defect. Correctness only; it saves 0 requests.

Each opens through the normal model/effort gate. The Milestone B production gate
remains available as the architecture owner's alternative order.
