# GitHub Proof — Current Development State

**Last Updated:** 2026-08-23 (Session 111)
**Source of Truth:** `docs/`

## Current Session State

- Active Session: **111 — P10-I6-A Profile shell + first-read hierarchy**.
- Session 111 — P10-I6-A Profile shell + first-read hierarchy: the approved
  dark mockup is implemented as the first-read visual target; STEP 2 adds a
  bounded Starting Point presentation (max 3, deterministic order, bounded
  desktop scroll, natural mobile flow) and a canonical GitHub avatar
  (collected, typed, propagated, rendered with decorative fallback); STEP 3
  adds the canonical GitHub repository Description to each presented Starting
  Point item (collected, typed, propagated, rendered with a neutral fallback).
  A follow-up layout fix stretches the Starting Point card to match the
  bottom edge of the left column's paired work-type/Working Style cards.
  Live review of the first CSS-only attempt (`align-self: stretch` +
  `flex: 1 1 0`) found the card's outer edge aligned correctly but a short
  (1-item) list left a large empty gap beneath the visible content, because
  CSS grid's `auto` row sizing is symmetric — it cannot express "the rail
  always follows the left column, never the reverse" on its own (several
  full-length descriptions can make the rail itself the naturally taller
  side). The corrected approach: the repository list keeps its own natural
  height (`flex: 0 1 auto`) with auto top/bottom margins (`margin-block:
  auto`) to center short content within the stretched card instead of
  stranding it at the top, and a small page-scoped script
  (`wwwroot/js/starting-point-rail.js`, referenced from `_Layout.cshtml`)
  measures the left column's rendered height and applies it explicitly to
  the rail above the existing `60rem` breakpoint — making the left column
  authoritative in both directions, with no new network calls, timers or
  product behavior. A themed scrollbar (existing `--border-strong`/
  `--accent-primary` tokens) replaces the raw browser-default control.
  Mobile clears the inline height and returns to natural flow. Verified live
  for `fatihGunalp` (desktop bottom-edge diff = 0px) and, since GitHub's
  anonymous rate limit blocked further live runs in this environment (no
  `GITHUB_TOKEN` configured), via a throwaway static-HTML harness using the
  real production CSS/JS for the 1/2/3-item and long-description/overflow
  cases (all diff = 0px; content centers when short; internal scroll
  triggers, starting from the top, only when content overflows; mobile
  shows all content with no scroll).
  **STEP 3 — READY FOR STARTING POINT DESCRIPTION LIVE REVIEW** before
  commit/integration.
- Pre-existing `?? artifacts/` is user-owned, read-only and out of scope.
- Session 111 scope remains the visual Profile shell and canonical first-read
  sequence. The approved uploaded dark mockup is the implemented primary visual
  blueprint; unsupported mockup slots remain omitted. Why/Repository Evidence
  trace and inspection remain P10-I6-B, and responsive/theme/localization
  closure remains P10-I6-C.
- The first-read grid now ends at Starting Point. Why, Limitations and
  Repository Evidence follow in normal document flow, so opening either native
  disclosure cannot change first-read grid placement or rail width.
- The centered desktop Profile result surface now uses the existing global
  `80rem` content measure; the responsive collapse remains. Starting Point stays
  secondary to Meaning but now renders each canonical repository identity with
  the existing localized Repository Proof POST action (`/Index`, literal
  `Repository.Url` as `GitHubTarget`), never an outbound GitHub link.
- Session 111 STEP 3 closes **STARTING POINT DESCRIPTION CONTRACT GAP** for
  GitHub's own repository Description: `RepresentativeRepository` /
  `ProfileReport` still carry no Description (unchanged — it is presentation
  metadata, not evidence), but a new non-evidence `RepositoryPresentationMetadata`
  lookup (keyed by canonical `ProfileRepositoryReference`) carries the same
  existing repository-metadata response's `description` field from
  `GitHubProfileSnapshot` through `ProfilePipelineResult` to
  `ProfileModel.RepositoryPresentationMetadata` — no new GitHub request. Each
  presented Starting Point item renders it verbatim (trimmed only, never
  translated/rewritten/summarized/inferred), falling back to a fixed neutral
  localized statement when GitHub returns none.
- Meaning now deterministically renders a username-led two-paragraph human
  synthesis from only the accepted Profile identity plus ordered
  `TechnicalFocusAreas` and `EngineeringPracticePatterns` items, followed by
  one quiet provenance sentence. Every canonical item remains represented in
  report order with no cap; practice definitions/glossary prose and the
  duplicate practice list are removed. It has
  no rank, count, score, role/seniority/capability/quality inference,
  representative, Why, repository, warning or limitation input;
  `ProfileReportProjector` remains locale-neutral. It does not infer recency:
  no canonical profile-level last-observable-public-activity fact exists.
- `EngineeringPracticePatterns` now has the compact `Working style` /
  `Çalışma biçimi` card: every canonical practice item renders in report order
  as a short deterministic evidence-bounded label, with no cap, explanatory
  paragraph, score or person-level methodology/capability/quality inference.
  Meaning connects those labels only through the short synthesis. Its empty
  state is a bounded observation, not a developer deficit.
- The successful Profile header now leads with canonical
  `ProfileIdentity.Login`, visible `github.com/{login}` projected from the exact
  typed `ProfileIdentity.Url`, a short public-work context line and one safe
  localized GitHub external action inside one compact grouped surface. Its
  large `aria-hidden` GitHub mark is decorative only, never an avatar. The
  former visible Profile-view eyebrow is removed; no content metric enters the
  header.
- Why is now visitor-centered: `What is this based on?` / `Bunu neye dayanarak
  söylüyoruz?`. Each canonical conclusion renders a deterministic question,
  family-bounded reason and unchanged supporting repository identities; no raw
  evidence, internal term, count or relationship is invented.
- Canonical representative selection has no global first/recommended/top-K
  priority; its deterministic order is an Owner-then-Name ordinal tie-break,
  not an importance measure. Session 111 STEP 2 authorizes exactly one
  Web-only presentation bound over it: `StartingPointPresentation.Select`
  presents at most the first 3 items of that existing order, with a bounded
  desktop scroll and natural mobile flow. The canonical `RepresentativeRepositories`
  collection itself remains complete and uncapped; this narrows but does not
  close **STARTING POINT PRIORITIZATION CONTRACT GAP** (a true importance
  ordering remains unauthorized).
- Project/application type remains unavailable and uninferred:
  **PROJECT-TYPE PRESENTATION PENDING CANONICAL CONTRACT**.
- `Limitations` is a canonical Supported section and cannot currently be
  omitted solely when empty: **EMPTY LIMITATIONS PRESENTATION CONTRACT GAP**.
- Session 111 STEP 2 closes **PROFILE AVATAR CONTRACT GAP** for collection and
  presentation: `GitHubProfileMetadataCollector` now reads GitHub's own
  `avatar_url` from the existing profile-metadata response (no new GitHub
  request), `GitHubProfileMetadata.AvatarUrl` requires an absolute HTTPS URI,
  and the value travels via `ProfilePipelineResult.AvatarUrl` to
  `ProfileModel.AvatarUrl` without touching `DeveloperPublicEvidenceProfile`/
  `ProfileReport`. The header renders it when available and falls back to the
  existing decorative `aria-hidden` GitHub mark otherwise; no username-derived
  or generated avatar is used. Current typed Profile truth still has no
  authoritative profile-level recency fact: **RECENCY CONTRACT GAP** remains.
- Profile results remain transient POST responses; GET `/profile` redirects to
  entry and no persistence/cache or dynamic social metadata exists. Share,
  LinkedIn and Copy-link controls remain absent: **SHAREABLE PROFILE URL
  CONTRACT GAP** and **SOCIAL PREVIEW CONTRACT GAP**.

## Current Product State

- Repository Proof V1 and Profile Proof deterministic evidence semantics are
  unchanged. No Domain/Application production file changed.
- The canonical live boundary is implemented as `ProfileAvailability? +
  ExecutionAvailability + ProfilePipelineResult?`. Exact DataKey/status/reason/
  RetrievedAt/nullability combinations are enforced at construction.
- Valid Profile POST admission is fail-fast and queue-free in exact order:
  visitor `429`, same normalized profile `409`, global capacity `503`. The
  process-local concurrency defaults are the provisional `1/1`; leases cover
  the live execution and are released on every terminal path. Admission
  rejection creates no live analysis result.
- Visitor permit/window remain unset, so IP limiting is disabled. It reads only
  `RemoteIpAddress`; no forwarded-header parser/trust was added. Production PASS
  still requires deployment-owned trusted proxy/network/hop topology and
  measured visitor values.
- Profile-only sanitized metadata response-budget state can stop execution
  before repository-manifest collection. The exact result is Available Profile
  metadata plus execution `Unavailable / github-budget-protection-rejected`, no
  RetrievedAt and no pipeline/Partial; Web maps it to `503`. Budget state is
  separate from telemetry/evidence and Repository Proof is unchanged.
- `GitHubApiBudget:ProfileAdmissionRemainingFloor` is unset/disabled. No reserve
  value was selected; `500` remains a measurement candidate.
- A dedicated internal deadline begins inside `LiveProfileReportService` before
  the first live metadata operation and remains active through terminal outer
  result creation. Caller cancellation wins overlaps and propagates; owned
  deadline expiration returns no pipeline/Partial; transport/unrelated
  cancellation remains a propagated operational fault.
- Resolved metadata availability is preserved by a minimal Infrastructure
  progress carrier published before manifest collection. Its execution-token
  binding atomically arbitrates `unresolved → resolved(value)` against
  `unresolved → cancellation-won`; late typed metadata cannot overwrite the
  winning deadline, while earlier availability survives later cancellation.
  It is operational only and never evidence, warning, limitation, provenance
  or report truth.
- `ResponseHeadersRead` metadata and manifest content reads are proven to
  observe the execution token. Existing downstream Profile repository request/
  content APIs already receive the same token. Repository Proof is unchanged.
- Execution success uses `DataKey=profile-execution`, `Status=Available`, no
  reason, and UTC terminal-result `RetrievedAt`; non-success has no timestamp.
- Existing transport timeout remains 2 minutes. External
  `ProfileExecutionDeadline:Timeout` is deliberately unset/disabled with no
  fallback; if configured, startup validation requires it to exceed transport.
  Exact production deadline remains unselected and readiness is not claimed.
- Operational HTTP mapping is locked: visitor `429`, duplicate `409`, global
  busy `503`, execution deadline `504`, proactive budget rejection `503`.
- Initial provider-budget/header protection is Profile-only; shared circuit
  behavior remains unauthorized without a separate future decision.
- Production IP limiting remains stopped until the deployment owner supplies
  the actual trusted proxy/network topology. Forwarded headers are not trusted
  generically.
- A machine-user fine-grained PAT remains a possible interim beta credential,
  subject to exact endpoint compatibility and secure secret/expiry handling.
  GitHub App remains a valid longer-term production candidate and is not
  rejected.
- Visitor `2/10 min`, reserve `500` and exact deadline remain unselected
  measurement candidates. Global/same-profile concurrency `1/1` and queue `0`
  are implemented as provisional, externally configurable defaults.
- Real canonical Partial now keeps the complete typed report visible with an
  early localized observation notice. Canonical Warnings are a status notice;
  Limitations render in position 7. Admission/provider/deadline/pipeline
  failures remain reportless localized operational states.
- P10-I5 is integrated; Session 111 P10-I6-A is **STEP 3 — READY FOR STARTING
  POINT DESCRIPTION LIVE REVIEW**. P10 and Milestone B are not complete.

## Exact Next Action

Architecture-owner live review of Session 111 STEP 3: each presented Starting
Point item's real GitHub repository Description (or its neutral fallback),
alongside the STEP 2 bounded rail (max 3 items, desktop internal scroll,
natural mobile flow) and canonical GitHub avatar, against a live
evidence-rich Profile. Functional Share/shareable URL, recency, Starting Point
true prioritization, Project-Type and empty Limitations gaps remain separate
canonical decisions. Production enablement requirements remain separate.
