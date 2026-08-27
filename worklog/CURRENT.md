# GitHub Proof — Current Development State

**Last Updated:** 2026-08-27 (Session 138)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** 138 — Unified Live Analysis Progress Contract — branch
  `session/138-unified-live-analysis-progress-contract`, session base `03df70f`
  (`main`/`origin/main`/`HEAD` verified equal), route Opus / high, **DOCS**.
  Contract/architecture decision session; **no Razor, CSS, JavaScript, C#,
  domain, application, infrastructure, test or runtime-configuration change**.
  Answers how both analysis flows may show truthful live progress. **Source
  audit decided the shape, not a preselected transport:** both flows are one
  synchronous POST whose response *is* the result page, so live progress can
  only come from a second read-only channel. **Selected: the POST keeps running
  the analysis and delivering every terminal outcome; a read-only
  `GET /analysis-progress?run=<token>` observes an in-memory ephemeral run
  registry, polled at a constant 2 s interval.** No background job, queue, persistence,
  SSE, SignalR or streaming; nothing is detached from its request.
  **Repository gets 4 stages, Profile 3** — every publication point inside
  `GitHubProof.Web`, because `ProfilePipeline.RunAsync` is one opaque interval
  with no interface seam and the metadata boundary is internal to the snapshot
  orchestrator. The owner's five-stage Profile mock is therefore **not
  achievable** without a progress seam inside `ProfilePipeline` (Application)
  **plus interface seams on two Domain policies**, recorded as a deferred owner
  decision. States are `PENDING → ACTIVE → COMPLETED` only — no `FAILED`,
  because the POST response already delivers every ending, and **`Partial` is a
  terminal success that reaches `COMPLETED`**. **Percentage and ETA PROHIBITED
  on measured evidence** (R3 112.209 s / R4 70.888 s / R5 55.557 s at a constant
  662 requests — wall-clock tracks scheduling, not work units); per-repository
  events, `N of M` and a bare growing count all rejected with reasons. **Cancel
  NOT AUTHORIZED**; UI stays **inline**, not a modal. Ownership split two ways:
  runtime semantics to a new `docs/architecture/live-analysis-progress-runtime-contract.md`,
  language and shape to p10 §8.3.1–§8.3.6. Independent review in a forked cold
  context returned **FAIL** (BLOCKER 1, HIGH 1, MEDIUM 4, LOW 4) — the BLOCKER
  was three further canonical owners of the same prohibition this session had
  missed (`github-proof-ui-system-light-dark-v1.md` §27, `github-proof-design-principles-v1.md`
  §7.3, the P10-I2A acceptance list); **all ten findings closed in a
  same-session correction pass**. Later rounds found the position in seven places
  across **five** documents — two of them top-of-file banners, and
  `github-proof-milestone-b-p7-ux-working-direction.md` §22 carrying a stale
  *permission* for SSE rather than a stale prohibition — and all seven are now
  amended in place. A **second closure round returned PASS WITH FINDINGS** (BLOCKER 0,
  HIGH 0, MEDIUM 0, LOW 2) confirming all ten CLOSED; both new LOWs — a drifted
  checkmark prohibition across the four lists, and a stale registry-security
  rationale after registration moved before the admission lease — were also
  closed. The stage sets, state machine, transport and ownership split never
  changed across either pass. An **architecture-owner precision correction** then
  closed four more findings without reopening any decision: the transport table's
  "survives an off-process move" row was a category error (that is a
  **state/store** property, and this polling implementation is itself
  process-local) and the `SignalR: package required` cell was **not
  source-proven and wrong** — the Web csproj declares no `PackageReference` at
  all — so the cost is narrowed to a vendored JS client plus a hub route;
  `Cache-Control: no-store` is now required on every progress response with
  `ETag`/conditional semantics prohibited (nothing in the codebase used
  `Cache-Control` before, so this is a new rule, not an existing convention);
  **all six bounded runtime values are locked exactly** — poll 2000 ms, unknown
  retry 5, transport-failure stop 3, TTL 10 min, cap 64, **none configurable**;
  and **observable atomicity is LOCKED** — one transition publishes one coherent
  snapshot, `completed` is always exactly the stages preceding `active`, and a
  `null` active is authorized only at terminal. A **final forked cold-context
  review returned PASS WITH FINDINGS** (BLOCKER 0, HIGH 0, MEDIUM 5, LOW 2, seven) with
  every focus area verified against source; **all seven closed**, two of which
  would genuinely have stopped an implementer — the terminal snapshot was
  optional, and a malformed `run` token had no defined response (now HTTP 400,
  empty body). Also corrected: an unsound TTL justification, and two Domain
  policies misattributed to Application in §5.1. A **final closure review** over
  the corrected tree then returned **PASS WITH FINDINGS** (BLOCKER 0, HIGH 0,
  MEDIUM 3, LOW 4 — seven) with **no false factual claim** found in the
  contract; all seven closed. The three that mattered: a `known:false` arriving
  after a known response had two incompatible renderings (now locked — keep the
  last snapshot, never hide the stage list); both amended product documents
  still carried a top-of-file Session 101 banner asserting the superseded
  position; and this worklog had wrongly told Session 139 it could change the
  `aria-atomic="true"` assertion, which p10 §8.3.5 in fact still requires.
  A **ratification round** (BLOCKER 0, HIGH 0, MEDIUM 4, LOW 4) then found four
  pieces of reconciliation debt the previous fixes had themselves created, one
  genuine gap — no owner said what the live region carries before the first
  snapshot, now locked in p10 §8.3.6 as the canonical indeterminate sentence —
  and **one real copy defect this session introduced**: every locked Turkish
  stage label was present-continuous, so `GitHub hedefi doğrulanıyor ·
  Tamamlandı` read "is being validated · Completed". All seven TR labels were
  relocked as verbal nouns (`GitHub hedefinin doğrulanması`); EN unchanged. A
  **closure confirmation round** (BLOCKER 0, HIGH 0, MEDIUM 1, LOW 5) verified
  all eight closed and the Turkish independently sound, and found one last
  ownerless decision: three elements carry `data-analysis-form` — the landing
  input and the two Profile drill-down forms, which post a hidden `GitHubTarget`
  to `/Index` and do start real Repository Proof runs — and
  `analysis-progress.js` binds to all three. Now locked: **only the landing form
  carries a token**; the drill-down forms stay progress-free, per p10 §8.6.5's
  existing no-per-row-processing rule. **Session 139 is READY**, bounded to Web
  only, and selects no runtime value, copy or rendering of its own.
  **No Milestone B completion is claimed.** `artifacts/` untouched.
  Detail: [`sessions/2026-08-27-138-unified-live-analysis-progress-contract.md`](./sessions/2026-08-27-138-unified-live-analysis-progress-contract.md)
- **Previous Session:** 137 — Profile First-Read UI Implementation — branch
  `session/137-profile-first-read-ui-implementation`, session base `7038159`
  (`main`/`origin/main`/`HEAD` verified equal), route Sonnet / high,
  **PARTIAL — closure accepted by the architecture owner**.
  Bounded Web presentation implementation of the Session 136 contract; **no
  Domain, Application, Infrastructure, architecture-contract or runtime
  configuration change, and no live provider call**. Four slices, all PASS:
  **(A)** the Starting Point item now renders the recurring pattern →
  repository relationship (p10 §8.6.1b) from `RepresentativeRepository.
  PatternSupports` **identities only**, through one narrow localizer method —
  `SupportingEvidenceReferences`/`SupportingObservationKeys` never reach first
  read, every support renders in canonical order with no cap or reordering, and
  the singular/plural copy is grammatical agreement carrying no number;
  **(B)** G-2 closed — the quiet `Recurring technologies` lead-in moved above
  the one canonical list and the duplicate trailing `PatternKey` paragraph is
  gone, so a technical identity appears exactly once; **(C)** G-3 closed by
  **spacing only** — a stronger border was implemented then reverted in the
  review pass because it made the L3 Why card outrank the L2 recurring-result
  cards that §8.8.3 protects, and an **L2 ≥ L3 invariant test** now guards it;
  **(D)** G-4 closed — the operational observation notes moved below both
  recurring-result cards, still FIRST READ but subordinate, with the Partial
  notice untouched. **Outcome 4 moved `PARTIALLY SUPPORTED` → `SUPPORTED NOW`**
  in `profile-proof-distinctive-outcomes-contract.md`, only after independent
  verification of the full §8.6.1b delivery; the outcome set stays exactly
  four, priority stays `SUPPORTING`, moat/acceptance criteria/first-read order
  unchanged, evidence convergence still NOT SUPPORTED, and the **Starting Point
  Prioritization Contract Gap stays open** — a relationship is not a ranking.
  Independent review **PASS WITH FINDINGS** (BLOCKER 0, HIGH 1, MEDIUM 2,
  LOW 3); **all six closed in a same-session correction pass**, and a second
  round returned PASS WITH FINDINGS (BLOCKER 0, HIGH 0, MEDIUM 0, LOW 1) whose
  single LOW was also closed. Build 0 warnings, full suite **2339/2339**.
  **Architecture-owner human visual review: 880px PASS; 390px PASS for G-1 and
  G-2 with no horizontal overflow and correct relationship wrapping; G-3
  PARTIAL on mobile** — the three Starting Point cards push the Why summary
  roughly two viewport heights below the fold. **No source change was made for
  it:** the only §8.8.9-authorized lever (per-item spacing) is worth ~60px
  against a ~1600px offset, and every lever with enough magnitude breaks a
  locked rule — canonical DOM order, §8.7.6's mobile natural flow, the §8.6.1a
  bound, §8.6.1b completeness or §8.6.2's collapsed-by-default. **The architecture owner
  ACCEPTED G-3 mobile PARTIAL as a known presentation gap**: p10 §8.7.6 is not
  amended in this session, and no mobile nested scroll, carousel, reordered Why,
  duplicate affordance or other workaround was introduced. **G-1, G-2 and G-4
  are CLOSED; G-3 stays PARTIAL** (PASS desktop). The remaining gap is
  **presentation hierarchy only — not correctness, traceability, evidence or
  Outcome 4 capability**: the Why disclosure is present, collapsed, in canonical
  order with locked copy on mobile too, and all three Starting Point
  repositories render complete. **Outcome 4 remains SUPPORTED NOW**; responsive
  result is **PASS WITH KNOWN MOBILE HIERARCHY GAP**; session lifecycle is
  **PARTIAL, closure accepted**. **No Milestone B completion is claimed.** The
  gap is recorded in worklog as the *Mobile First-Read Affordance Hierarchy
  Gap* and is deliberately **not** registered as a canonical p10 gap — that
  would be a separate owner decision. `artifacts/` untouched.
  Detail: [`sessions/2026-08-27-137-profile-first-read-ui-implementation.md`](./sessions/2026-08-27-137-profile-first-read-ui-implementation.md)
- **Earlier Session:** 136 — Profile First-Read UI/UX Contract — branch
  `session/136-profile-first-read-ux-contract`, session base `82569fa`
  (`main`/`origin/main`/`HEAD` verified equal), route Opus / high, **DOCS**.
  Product UX / presentation-contract session; **no Razor, CSS, JavaScript, C#,
  domain, application, infrastructure, test or config change**. Answers what a
  visitor must understand in the first 5–10 seconds of a successful Profile
  result. **Ownership decision: extend `p10-presentation-language-contract.md`
  minimally; no new document**, because §25 already owns the first-read
  question set, Session 135 §8 owns outcome-level first-read priority, and
  presentation depth is p10's own concern — a new owner would have duplicated
  normative truth. Two additions: **§8.6.1 / new §8.6.1b** raises the Starting
  Point item's locked "renders exactly" shape from three elements to four so
  the canonical **pattern → starting-point relationship** finally becomes
  user-visible, rendered only from the already-projected
  `RepresentativeRepository.PatternSupports` identities, with locked EN/TR copy
  and an explicit ranking prohibition (the **Starting Point Prioritization
  Contract Gap stays open**); and **new §8.8** locks the first-read information
  depth contract — the five required understandings, an L1–L5 semantic ladder
  that is explicitly **not** a DOM order, a per-information-type FIRST READ /
  PROGRESSIVE DISCLOSURE / DEEP EVIDENCE / NEVER RENDERED classification,
  boundary behavior for the four cases, the generic-AI differentiation test and
  the implementation-input boundary. The apparent Session 135 §8.1 vs p10
  §8.6.1 ordering conflict resolves without a STOP: Session 135 constrains the
  why-path and the starting point only **relative to Meaning** and leaves
  concrete section order to p10, and L3 splits into a first-read *affordance*
  and a disclosed *content*. **No new outcome, evidence rule, threshold,
  taxonomy, limitation identity, availability state, count, ranking or score**
  was introduced; edits to the Session 135 contract are cross-reference only
  and **Outcome 4 remains PARTIALLY SUPPORTED** — authorization is not
  delivery. Audit of the delivered Web produced a current first-read map and
  six classified gaps (G-1 contract half closed here, implementation half open;
  G-2 duplicated technology list; G-3 why-affordance prominence; G-4 warning
  placement; G-5 and G-6 recorded as new named contract gaps, deliberately not
  closed). **Independent review ran twice in a forked cold context and returned
  PASS WITH FINDINGS both times (BLOCKER 0, HIGH 0)**; the first round's
  1 MEDIUM / 3 LOW and the second round's 1 MEDIUM / 2 LOW were all closed in
  bounded same-session correction passes — derived-list precedence and owner
  order, a wrong binding-rule attribution, the status-area/L1-L2 tension, the
  `ProblemArea` family scope, and worklog record consistency. The outcome set,
  the two additions and the gap map never changed across those rounds.
  `artifacts/` untouched.
  Detail: [`sessions/2026-08-27-136-profile-first-read-ux-contract.md`](./sessions/2026-08-27-136-profile-first-read-ux-contract.md)
- **Earlier Session:** 135 — Profile Proof Distinctive Outcomes Product
  Contract — branch
  `session/135-profile-proof-distinctive-outcomes-contract`, session base
  `9c4c90d` (`main`/`origin/main`/`HEAD` verified equal), route Opus / high,
  **DOCS**. Product/architecture-level decision session; **no production,
  domain, application, web, test or config change**. Locks the canonical
  answer to “why GitHub Proof instead of GitHub + a generic AI” at outcome
  level in a new canonical owner,
  `docs/product/profile-proof-distinctive-outcomes-contract.md`: four accepted
  outcomes (**CORE** cross-repository recurring evidence, **CORE** traceable
  conclusion-to-repository evidence chain, **CORE** declared evidence surface
  and its boundary, **SUPPORTING** pattern-scoped starting point), the canonical
  moat statement, and a first-read information-priority contract that is
  deliberately **not** a layout. Verification ran in two layers — canonical/
  domain/application semantics **and** the Web/user-visible projection — giving:
  Outcome 1 **SUPPORTED NOW**; Outcome 2 **SUPPORTED NOW** with a fresh-run
  qualification (the last hop is a fresh Repository Proof run, not a frozen
  replay of the analysis instance that produced the profile result); Outcome 3
  **SUPPORTED NOW** with its already-recorded evidence-surface taxonomy
  limitation; Outcome 4 **PARTIALLY SUPPORTED** — `RepresentativePatternSupport`
  is never rendered in Web, so the pattern relationship that makes the starting
  point distinctive does not reach the user today. A **same-session correction
  pass** closed **3 HIGH / 3 MEDIUM / 4 LOW** review findings (outcome set
  unchanged). **Evidence convergence is REJECTED — NOT SUPPORTED**: it would
  require new domain semantics and a new `PatternRuleVersion`, so it is left
  as an explicit owner decision instead of being invented. Score, technology
  lists, activity ranking, AI prose, `ProblemArea` and `RoleEvidenceAlignment`
  are also rejected as distinctive outcomes. **No new threshold, taxonomy,
  limitation identity, availability state or user-facing copy was created** —
  recurrence stays owned by patterns §5, and coverage is worded **declared**,
  never “complete”, because the repository manifest carries a declared
  collection bound. Edits to `product-positioning.md` §9 and
  `public-profile-evidence-product-contract.md` §4 are cross-reference only.
  A **final closure review then ran in a forked cold context** and returned
  **PASS WITH FINDINGS** (BLOCKER 0, HIGH 0, MEDIUM 1, LOW 3), confirming all
  ten earlier findings CLOSED and the four-outcome model ACCEPTED; a second
  correction pass closed **F-N1** (Outcome 4's requirement was wrongly attributed
  to the **Starting Point Prioritization Contract Gap**, which is about ranking
  — the pattern relationship is **not** a recorded canonical gap and needs a
  p10 §8.6.1 amendment), plus **F-N2/F-N3/F-N4**. The earlier author-context
  review was never recorded as independent (`AGENTS.md` §6 / `agents/06` §7).
  **Session 135 closure: architecture-owner decision PASS.** The contract is
  ratified and integrated into `main` as
  `82569faaf51dda2a69f24888e2eddde3c661df55`. `artifacts/` remains untouched.
  Detail: [`sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md`](./sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md)
- **Previous Session:** 134 — Milestone B Production-Candidate Runtime
  Configuration + Exit Validation — branch
  `session/134-milestone-b-production-candidate-runtime`, session base
  `53991ac`, route Sonnet / high, **PARTIAL** — independent closure review
  **PASS WITH FINDINGS** (critical findings **NONE**), and the configuration
  implementation itself is **technically accepted**. Wires the Session
  133-accepted R5 candidate into
  `src/GitHubProof.Web/appsettings.Production.json`
  (`ProfileGuardrails:Collection:MaxRepositoryConcurrency = 5`) — the first
  `appsettings*.json` this repository has ever had. U and both GraphQL flags
  stay unset because their existing default already **is** the
  production-candidate value; the C# default (`1`) is untouched. Proven
  through the real ASP.NET Core file-loading path (5 under `Production`, 1
  under `Development`) and the real production orchestrator composition
  end to end, with the real `dotnet publish` artifact verified to carry the
  file. Regression **2327/2327**, build 0 warnings. Scored **PARTIAL**, not
  PASS, solely because authenticated live validation — an explicit part of the
  Session 134 objective — is **BLOCKED** (`GITHUB_TOKEN` unavailable), so the
  dense-latency and burst gates were not re-measured. **Milestone B closure is
  NOT CLAIMED**: VPS deployment, remote production verification, production
  telemetry/monitoring and a multi-user smoke under R5 all remain open.
  Detail: [`sessions/2026-08-27-134-milestone-b-production-candidate-runtime.md`](./sessions/2026-08-27-134-milestone-b-production-candidate-runtime.md)
- **Previous Session:** 133 — Milestone B Runtime Gates — branch
  `session/133-milestone-b-runtime-gates`, session base `37b8ec2`, route
  Sol / high, **PASS** for the bounded runtime-gate objective. Independent
  closure review **PASS WITH FINDINGS** (critical findings **NONE**; the
  non-blocking findings were record-accuracy items, since corrected in a
  worklog-only pass). **R5 ACCEPTED** as the Milestone B production-candidate
  repository concurrency; **Milestone B closure is NOT CLAIMED.**
  Authenticated production-composition sequence
  at U1 measured R3 `112.209 s`, R4 `70.888 s`, and first-pass R5 `55.557 s`.
  Every candidate issued exactly 662 requests, composed with incomplete=0,
  semantic parity PASS, GraphQL=0, no `Retry-After`, no primary event, and max
  in-flight HTTP equal to R. Production configuration remains **unchanged**
  (R/U defaults still 1) — R5 is accepted as a candidate, not applied.
  Deterministic 10/50/100 arrival gates PASS at U1
  (accepted/capacity 1/9, 1/49, 1/99; crash/wait 0), and existing processing,
  capacity/provider/terminal Web mappings PASS. One test-only multi-core-window
  telemetry formatter defect was found on the first R3 attempt, fixed and
  regression-tested before the full sequence reran successfully. Review-driven
  test/harness-only proof-strengthening followed: true all-ready burst
  synchronization, direct R4/R5 repository parity coverage, and immediate
  candidate invalidation on a provider-protection response. A separate
  architecture-owner manual R5 Web smoke (57/57 repositories, incomplete 0,
  `composed`, provider budget above the 1500 floor) is recorded as **supporting**
  evidence only — a different profile shape, not a replacement for the
  `55.557 s` benchmark. Final current-tree validation: build 0 warnings/0 errors,
  **2322/2322**, `git diff --check` PASS. `artifacts/` remains untouched.
  Detail: [`sessions/2026-08-26-133-milestone-b-runtime-gates.md`](./sessions/2026-08-26-133-milestone-b-runtime-gates.md)
- **Previous Session:** 131 — Profile Repository Concurrency and Capacity —
  branch `session/131-profile-repository-concurrency-capacity`, session base
  `cc76c75`, route Opus / high. Verdict **R2-PERFORMANCE-INSUFFICIENT** — technical R2/U2 gate PASS, but the dense-profile product latency gate FAILS because **127.667 s > 60 s**. Bounded
  repository concurrency (**R**) added as an externally configurable,
  **DEFAULT-OFF (R=1)** seam in `ProfileRepositoryAnalysisOrchestrator`, plus
  the measurement proving it: dense profile **254.634 s → 127.667 s (1.99×)**
  at R=2 with the **same 662 requests** and an identical §6 profile-level
  fingerprint, and **U2/R2** running two different profiles at once with
  isolated telemetry and no provider distress (no `Retry-After`, no primary or
  secondary limit event, zero GraphQL requests). Two measurement rounds in one
  window did drive the lowest observed core `remaining` to **773/5000**, and a
  pre-flight `/rate_limit` probe disagreed with the in-run headers — both
  recorded as §10.4 reserve-design inputs.
  **U needed no code at all** — `ProfileAdmissionOptions` already exposes it.
  §8 authorized this once §9 measurement existed (Session 122 REST-R3 proved
  the removable request set is 0), and §10.5 already calls the concurrency
  numbers provisional, so **no canonical change was required**. Closure review
  **PASS WITH FINDINGS**; the MEDIUM §8 finding (the first implementation used
  the fan-out shape §8 names, defended by a code comment) was closed by
  **rewriting the concurrent path as a fixed pool of R workers over a shared
  queue** rather than by asking for a contract reading in its own favour. Build
  0 warnings, **2312/2312**. **Nothing is enabled**: R and U both still default
  to 1 and no configuration file sets either. **CLOSED / integrated to main**.
  Detail: [`sessions/2026-08-26-131-profile-repository-concurrency-capacity.md`](./sessions/2026-08-26-131-profile-repository-concurrency-capacity.md)
- **Previous Session:** 130 — Milestone B Pre-Production Real Profile Validation —
  **B-PREPROD-READY-WITH-FINDINGS**. Branch
  `session/130-milestone-b-real-profile-validation`, base `4075e9a`, route
  Opus / high. First opened the same day and initially returned
  REAL-VALIDATION-BLOCKED (`GITHUB_TOKEN` unset in every environment scope
  this session's tools could see); the owner then set it as a persistent User
  environment variable, which this session bridged into the Process scope of
  the exact commands that needed it (never printed/logged) and continued.
  Ran the real Profile Proof pipeline (production `AddProfilePipeline` +
  `LiveProfileReportService`, Session 120's own opt-in
  `GITHUBPROOF_RUN_LIVE_TESTS`/`GITHUBPROOF_BENCHMARK_PROFILES` harness
  pattern, reused not duplicated) against **six** real public GitHub
  profiles: Stage A `fatihGunalp`/`torvalds`/`karpathy` (Session 120's own
  comparison shapes) and Stage B `imArafatHussein` (sparse),
  `newluckybaby` (fork-heavy), `asynkayo` (archive-heavy) — the latter three
  selected by observable public repository composition via bounded
  unauthenticated reconnaissance, never through the app's credential. All
  six completed with **zero** incomplete repositories and **zero**
  authoritative GraphQL requests (envelope confirmed DEFAULT OFF throughout).
  Admission/eligibility behaved coherently for every shape (forks excluded
  from admission; archived repositories admitted but eligibility-restricted,
  not excluded). Session 120 comparison: two of the three known profiles
  reproduced their exact discovered/admitted/completed *and* request counts
  two days later; the third differed by exactly one request while matching
  every discrete count — no regression, no structural request-graph change.
  Traceability: a mechanical defect in this session's own new *test-only*
  verification code (checked only the signal-evidence `EvidenceKey` shape,
  missed the documented snapshot-data shape `ProfileEvidenceKeys` names for
  languages) was found on the first live run, fixed in-session, and
  re-verified — **0 broken traceability** post-fix; **not** a production
  defect. Web/Profile flow smoke-tested end to end for a small
  (`fatihGunalp`) and a denser (`torvalds`) profile via a locally-run
  instance, including a working repository drill-down into Repository Proof.
  No naturally occurring provider failure was observed (none provoked).
  **Closure round (architecture-owner review):** the original
  prohibited-inference check only scanned closed vocabulary (limitation/
  warning codes, pattern keys) and never the actual rendered report text a
  reader sees, so "PASS for all six" was not yet fully evidenced. Fixed by
  adding `PROHIBITED_INFERENCE_TEXT` to the same test-only harness — it
  renders every `ProfileWebPresentationLocalizer` surface each profile's
  `ProfileReport` actually reaches, in both shipped cultures, with a
  placeholder substituted only for the two GitHub-owned identity/repository
  parameters (never for a language/pattern key, which is technical metadata,
  not excluded free-form text) — then reran all six profiles once (rate-limit
  confirmed safe: 5000/5000 before the rerun). **Result: PASS for all six**,
  0 matches across 432 rendered surfaces (60/82/114/58/60/58 per profile).
  Build 0 warnings, **2292/2292** throughout both closure rounds. Independent
  closure review: first pass **PASS WITH FINDINGS** (a worklog-sequencing
  note, since resolved); second pass (post-text-scan-fix) **PASS**, confirmed
  independently against actual repo state. **Recommendation: GO TO
  PRODUCTION RUNTIME DECISIONS.** No production code changed in either round
  (two test-file accessibility keywords + one new/extended test-only file
  only). No commit, no push, no deploy.
  Detail: [`sessions/2026-08-26-130-milestone-b-real-profile-validation.md`](./sessions/2026-08-26-130-milestone-b-real-profile-validation.md)
- **Previous Session:** 129 — GraphQL Default-Branch Head / Tree Identity Parity —
  **PASS**; **integrated as `4075e9a`**. Branch
  `session/129-graphql-head-tree-identity-parity`, base
  `9608804`, route Opus / high. Verdict **HEADTREE-PASS**: the §18.13 proof
  succeeded and the bounded implementation closed in the same session. The
  default-branch **head commit SHA** and that commit's **root tree SHA** are now
  adopted over GraphQL inside the **existing** Session 127 envelope — no second
  request, no second envelope, no second circuit — and the envelope stays
  **DEFAULT OFF**. Build 0 warnings, **2291/2291**. Closure review **PASS WITH
  FINDINGS** (0 blocker, 0 high, 0 medium, 1 low), the low finding (a stale
  code comment overstating parity with `GitHubCommitReference`'s case
  normalization) corrected in-session and re-verified with a clean rebuild.
  Detail: [`sessions/2026-08-26-129-graphql-head-tree-identity-parity.md`](./sessions/2026-08-26-129-graphql-head-tree-identity-parity.md)
- **Previous Session:** 128 — GraphQL DecisionBounded Ordering Parity — **PASS**;
  **integrated as `9608804`**.
  Branch `session/128-graphql-decision-bounded-ordering-parity`, base `8f42a6f`,
  route Opus / high. A proof/decision session, not an implementation one. Verdict
  **PARITY-BLOCKED**: no `DecisionBounded` surface — commits, pull requests,
  releases, tags — can meet §19.7 from current provider guarantees, and all four
  are ORDER-BLOCKED. Test-only change; build 0 warnings, **2247/2247**. Closure
  review **PASS WITH FINDINGS** (0 blocker, 0 high, 3 MEDIUM, 3 LOW), all six
  corrected — the review found a real hole in the tags proof, which is why tags
  is ORDER-BLOCKED rather than ORDER-PASS. The owner accepted PARITY-BLOCKED and
  **withdrew one further claim the record had made about REST pagination**; no
  verdict changed.
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
- **The Profile runtime measurement phase is COMPLETE.** Session 120 owns the
  baseline shapes and Session 133 owns the Milestone B R3/R4/R5 latency gate.
- **R5 is CONFIGURED as the production-candidate value (Session 134), not yet
  deployed or remote-verified.** `src/GitHubProof.Web/appsettings.Production.json`
  selects `ProfileGuardrails:Collection:MaxRepositoryConcurrency = 5`. U stays
  `1` and GraphQL stays OFF through their unchanged defaults — neither was
  duplicated into the new file. No execution deadline, request budget, GitHub
  reserve floor or contents bound has been selected. Session 120 owns
  measurement and diagnostic evidence only; Sessions 121 and 122 own
  feasibility and audit analysis only; Session 123 owns a contract decision
  only; Session 124 owns an unused transport only; Session 125 owns a
  default-off observation only; Session 126 owns a transport classification
  only; Session 127 owns a default-off authoritative transport consolidation
  only — no evidence, score, status or report value changes with it; Session 128
  owns a transport-feasibility verdict only and adopts nothing; Session 129 owns
  a default-off identity-transport consolidation only — it changes which requests
  are issued, never what any value means. Session 133 measured R3/R4/R5 and
  proposed R5 from that evidence, but Session 133 itself left production
  configuration unchanged; it was Session 134 that actually selected R5 in
  configuration, still without deployment or remote verification.
- **Last remote-verified `main`:** `03df70f` — `main` == `origin/main` ==
  `HEAD` verified equal at Session 138 start. Session 137's first-read UI
  implementation is integrated at that commit; Session 138's own work is not
  yet integrated.
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
- **Profile Proof distinctive-outcome semantics: LOCKED** (Session 135).
  `docs/product/profile-proof-distinctive-outcomes-contract.md` is now the
  canonical owner of which Profile Proof results must be structurally more
  valuable than GitHub's own UI or a generic AI reading of the same profile,
  of the product moat statement, and of first-read information priority — the
  last constraining, not replacing, `public-profile-evidence-product-contract.md`
  §25, which remains owner of the general Profile first-read questions. Three
  **CORE** outcomes are **SUPPORTED NOW** (Outcome 2 with a fresh-run
  qualification, Outcome 3 with its recorded evidence-surface taxonomy
  limitation); the one **SUPPORTING** outcome, pattern-scoped starting point, is
  **PARTIALLY SUPPORTED**. **Evidence convergence is NOT SUPPORTED** and left as
  an owner decision. This is a docs contract only — it changed no capability.
  **Main handoff to the later Profile UI/UX session:** make the
  `recurring pattern → selected starting-point repository` relationship visible,
  without turning it into a best/recommended/quality ranking. That work is
  **not** a recorded canonical gap today and requires an explicit
  `p10-presentation-language-contract.md` §8.6.1 amendment, whose "renders
  exactly" list currently locks the starting-point item to three fields; it must
  not be confused with the §8.6.1a **Starting Point Prioritization Contract
  Gap**, which concerns ranking. That session has not started.
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
  unreliable. **Selected:** repository concurrency `R = 5` (Session 134
  configuration, not yet deployed). **Preserved at default:** `U = 1`, GraphQL
  OFF. **Still unselected/open:** execution deadline, provider reserve/admission
  floor, run-time provider-limit behaviour, credential strategy, visitor
  rate-limit and trusted-proxy topology, and the other Open Runtime Decisions
  below.
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
- **GraphQL default-branch head/tree identity: IMPLEMENTED, DEFAULT-OFF**
  (Session 129, **HEADTREE-PASS**). Two Git object identities — the default
  branch's head commit SHA and that commit's root tree SHA — are now adopted over
  GraphQL inside the **existing** S127 envelope. §18.13 was satisfied through the
  exit its own text names: *"those surfaces stay on REST, **or fall back to REST
  under §18.5**"*. So this session never classified null as empty. A clean
  non-null `defaultBranchRef` → `Commit` → `oid` → `tree` → `oid` chain may be
  adopted; **every** other shape — null ref, null target, a target that is not a
  Commit, a missing or null tree, or an OID that is not 40 lowercase hex — is
  "candidate unusable" and discards the **whole** envelope, leaving the exact
  REST 409 `"Git Repository is empty."` rule as the sole owner of
  empty-versus-failed. Proven in both directions: null + the exact empty 409
  reproduces the REST successful-empty result identically, and null + any other
  409 stays a typed failure. **GitHub's docs do NOT establish
  `defaultBranchRef == null ⟺ successfully empty`, and that equivalence is
  asserted nowhere.** Mapping parity is Git-defined rather than
  provider-computed, and the two OIDs arrive in **one** envelope, so they cannot
  straddle a push the way the two REST requests can. **The recursive file tree
  stays REST** — GraphQL exposes no recursive tree and no `truncated`,
  `truncated` drives `FileTree.IsComplete`, and `Tree.entries` is never
  requested; the adopted tree OID **anchors** the REST request instead of
  replacing it. An accepted repository now skips the REST git-ref and git-commit
  GETs on top of S127's metadata and languages GETs: four GETs become one POST,
  **three fewer physical provider requests** — request counts only, no latency
  claim. Explicit `contentReference` is never consulted against the supplier at
  all. The seam is one optional delegate on the contents collector mirroring the
  S127 supplied-languages seam: **no shared interface changed**, `CreateDefault`
  untouched and still issuing both identity requests. No new availability status,
  reason, key or telemetry; `file-tree` keeps the REST collector's own
  `RetrievedAt`; S126 run-disable and budget isolation unchanged; **no canonical
  change**. Build 0 warnings, **2291/2291**.
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

Session 136 added two more, both recorded in p10 and deliberately left open:
**Working Style Recurrence Lead-In** (§8.8.5 — closing it would change §8.4.8's
locked card shape) and **Empty-Result First-Read Repetition** (§8.8.6 — closing
it would require amending §8.5's deliberate-repetition rule). Session 136 also
**closed the contract half** of the Starting Point pattern-relationship gap via
§8.6.1b; its implementation half is open and is *not* the Starting Point
Prioritization gap, which remains untouched.

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

**Session 135 is closed: architecture-owner decision PASS, integrated into
`main` as `82569fa`.** It changed no production, domain, application, web, test
or configuration code; the runtime queue below is unaffected by it.

**Session 136 produced the Profile first-read presentation decision as a
bounded p10 extension, and Session 137 implemented it.** Session 137 changed
Web presentation, Web tests and two docs cross-references only; it touched no
Domain, Application, Infrastructure, architecture contract or runtime
configuration, so the runtime queue below is unaffected by all three sessions.

**Session 138 locked the live analysis progress contract.** It changed no
production, domain, application, web, test or configuration code, so the runtime
queue below is unaffected by it too.

Three independent next steps are now available, and the architecture owner
chooses the order:

0. **Session 139 — Live Analysis Progress implementation. READY.** Bounded to
   `GitHubProof.Web` only. It implements exactly what
   `docs/architecture/live-analysis-progress-runtime-contract.md` and p10
   §8.3.1–§8.3.6 authorize: the 4 Repository / 3 Profile stage identities in
   their locked order, `PENDING → ACTIVE → COMPLETED`, the read-only
   `GET /analysis-progress?run=<token>` Razor Page handler, the ephemeral run
   registry, the client-generated UUID correlation token, the locked EN/TR copy
   through `WebPresentationLocalizer`, the narrowed single-sentence live region
   and the BFCache reset. It **makes no architecture decision** — if something
   is ambiguous, it stops rather than choosing. It must also update
   `tests/GitHubProof.Web.Tests/AnalysisProgressContractTests.cs`, where exactly
   two assertions are superseded: the `setInterval` and `fetch(` exclusions.
   The `EventSource` and `WebSocket` exclusions must **stay**, and so must
   `Assert.Contains("aria-atomic=\"true\"", ...)` — it is a file-level
   substring check that still passes under the new DOM, because p10 §8.3.5
   keeps all three attributes on the narrowed live region, and it is the only
   automated guard for that.

1. **A separately authorized owner decision on the Mobile First-Read Affordance
   Hierarchy Gap**, if and when it is wanted. The narrowest candidate remains
   p10 §8.7.6's `returning to natural flow at the single-column breakpoint`
   sentence: retaining the bounded rail height and its existing internal scroll
   at mobile would keep all three presented repositories present, complete and
   reachable while letting the Why affordance rise into the first read. This is
   **not** queued work — Session 137 is closed as PARTIAL with the gap
   accepted. Any follow-up Web session stays bound by p10 §8.8.9 and may **not**
   introduce a new evidence rule, threshold, taxonomy, limitation identity,
   availability state, count or ranking, nor close any gap p10 holds open.
2. **The Milestone B runtime path below**, which none of these sessions
   touched.

Deferred by Session 138, recorded as owner decisions and **not** queued: a
`profile-identity-read` stage (needs an Infrastructure publication point on
`GitHubProfileSnapshotProgress`); splitting `profile-evidence-examined` into
repository-evidence and recurring-trace phases (needs an Application progress
seam inside `ProfilePipeline` plus interface seams for the composer and the two
policies — this is what the five-stage Profile mock would cost); explicit user
cancellation; and anything that must survive a restart or a second instance.
The capacity-state half of `public-profile-runtime-collection.md` §10.1.1's
user-visible presentation requirement also stays open.

---

**Session 133 is independently closed** (review **PASS WITH FINDINGS**, critical
findings **NONE**), and **R5 is ACCEPTED as the Milestone B production-candidate
repository concurrency**. **Session 134 applied that value to a real
configuration surface** — `src/GitHubProof.Web/appsettings.Production.json`
selects `MaxRepositoryConcurrency = 5`; U and GraphQL stay at their unset
defaults (already the candidate value). **Session 134 has now itself been
independently closure-reviewed: PASS WITH FINDINGS, critical findings NONE.**
The configuration implementation is **technically accepted** (real
ASP.NET Core file-loading path proven, real `dotnet publish` artifact
verified, orchestrator wiring proven, 2327/2327, `git diff --check` PASS). The
review's non-blocking findings were worklog/record-accuracy issues only (an
invented Result status, stale current-state wording) and were corrected in
this same-session pass. **Session 134's lifecycle result is `PARTIAL`, not
PASS** — solely because authenticated live validation, an explicit part of the
Session 134 objective, could not run (`GITHUB_TOKEN` unavailable).

**R/U/GraphQL production-candidate values, now wired into configuration:**

~~~text
R = 5   (src/GitHubProof.Web/appsettings.Production.json — CONFIGURED,
         not yet deployed or remote-verified)
U = 1   (default, unset)
GraphQL = default OFF (default, unset)
~~~

**The next meaningful session is one bounded authenticated live smoke run
against this exact production-candidate configuration** (`R=5`, `U=1`,
GraphQL OFF) — the one piece of the Session 134 objective a missing
`GITHUB_TOKEN` left undone. This is deliberately **not** bundled with VPS
deployment: deployment, the real host's `ASPNETCORE_ENVIRONMENT` and a
production telemetry/health surface are a separate, later, owner-controlled
session that only makes sense once the authenticated smoke evidence exists.

Two constraints carry into it from Session 131's measurement:

- R does **not** change request count (662 at R1 through R5 alike), so R affects
  only request *rate* and never the reserve arithmetic; **U** and profile shape
  are what govern the reserve (one dense profile = 662 requests, a dense+medium
  U2 pair = 792 measured, a dense+dense pair ≈ 1324 projected, never observed).
  This is why the candidate keeps **U=1**.
- The remaining open runtime decisions (D-1 execution deadline, D-2
  request/reserve protection, D-3 run-time provider-limit behaviour, D-5
  credential strategy — see the Open Runtime Decisions table above) are still
  open. Accepting R5 selects a concurrency candidate only; it decides none of
  them, and the provider floor is not changed without a separate owner decision.

**No production, beta, default-on or live-A/B approval exists for any GraphQL
switch.** The remaining queue (unblocked by GraphQL, not gated on Sessions
130/131/133/134):

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
- **Default-branch head / tree is DONE** (Session 129, default-off). What it
  unblocks is narrow: `Commit.history` is now reachable through an adopted
  default-branch ref target, but commits stay **ORDER-BLOCKED** on Session 128's
  own ordering evidence, which Session 129 did not touch.
- **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) — still
  pending.
- **Unscheduled:** a bounded session for the Session 122 §6.1 content-selector
  duplicate-fetch defect. Correctness only; it saves 0 requests.

Each opens through the normal model/effort gate. The Milestone B production gate
remains available as the architecture owner's alternative order.
