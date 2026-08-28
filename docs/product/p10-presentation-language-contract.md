# GitHub Proof --- P10 Presentation & Language Contract

**Status:** CONTRACT LOCKED / Product Direction\
**Milestone:** B --- Profile P10 --- Web Profile Flow --- Presentation Language Contract\
**Locked:** 2026-08-20 / Session 093 --- reconciled against
`docs/product/p10-web-profile-flow-implementation-plan.md` §13 and the
actual `GitHubProof.Web` rendered UI language (see the rendered-language
note at the end of §8). No narrative, principle, ordering, or upstream
structured semantic truth changed. §8's User Language Dictionary WAS
edited: the single combined `TechnicalFocusAreas / EngineeringPracticePatterns`
row was split into two separate rows with two distinct labels, to match
the distinct-language decision locked in the implementation-plan document
§13.9 (`docs/product/p10-web-profile-flow-implementation-plan.md` §13.9)
and correct an internal inconsistency this same session's own edits had
introduced (see that document's session-093 review-correction record).
This is a presentation-dictionary clarification/split only --- it does not
change, add, or remove any canonical concept, section, availability state,
or ordering rule; `TechnicalFocusAreas` and `EngineeringPracticePatterns`
remain, as before, two distinct upstream pattern families
(`docs/architecture/public-profile-evidence-patterns.md`) that were always
projected as two distinct report sections
(`docs/architecture/public-profile-evidence-report-contract.md` §5/§7) ---
only their shared row in this document's presentation dictionary was
corrected to reflect that. The fixed-English rendered-language boundary
described by this Session 093 record is historical and was later superseded
by Session 101 below.

**Amended:** 2026-08-21 / Session 101 --- the fixed-English P10 Web boundary
is superseded. `GitHubProof.Web` supports exactly English (`en`) and Turkish
(`tr`), resolved by persistent explicit user choice, then browser
`Accept-Language` preference/q ordering, then English fallback. The selected
request culture owns the whole user-facing Web surface. Geolocation and every
IP/device/browser-location signal are prohibited from language selection.
Profile structured truth and `ProfileReportProjector` remain locale-neutral;
Profile localization is deterministic and Web-owned. A truthful immediate
indeterminate processing state is also added without fake progress telemetry
or any change to analysis semantics.

**Amended:** 2026-08-21 / Session 103 --- added §8.4 and §8.5 only, plus a
one-line pointer at the end of §11. This locks the P10-I3 Meaning layer's
exact deterministic presentation shape and its exact EN/TR copy, and the
Supported-but-empty EN/TR copy of `TechnicalFocusAreas`,
`EngineeringPracticePatterns` and `RepresentativeRepositories`. It closes the
gap `docs/product/product-experience-principles.md` §6 ("Bilinçli olarak
kilitlenmeyen") explicitly reserved for an approved bounded presentation
contract or an extension of an existing one; this is that extension. No
upstream structured semantic truth changed: no concept, section, availability
state, canonical ordering, evidence rule, pattern rule or inference
prohibition was added, removed or altered. §8's User Language Dictionary was
NOT edited --- the `TechnicalFocusAreas`, `EngineeringPracticePatterns` and
`Representative Repository` rows, and every other row, stand exactly as
Session 093 left them. §11 WAS reconciled, narrowly: the already canonical
`RepresentativeRepositories` position (Starting point / Başlangıç noktası) was
inserted into its reading sequence between the engineering-practices line and
the why-this-view line, so that §11 stops contradicting §6's Experience
Principle and the canonical report order (report contract §7, positions
1 → 2 → 3 → 5). No label text, concept, section, availability state or
upstream semantic changed with that reconciliation. The
`PublicProfileSummary` section label stays "What does this GitHub
communicate?" / "Bu GitHub ne anlatıyor?". `ProfileReportProjector` remains
locale-neutral and no Domain or Application semantics changed.

**Amended:** 2026-08-22 / Session 104 --- added §8.6 only, plus two rows and
one clause in §8.5. This locks the P10-I4 Why + Repository-evidence
presentation contract: the progressive-disclosure shape of
`WhyThisConclusion` and `RepositoryDrillDown`, exactly what one Why entry
shows, the prohibition on rendering `ProfileEvidenceReference.EvidenceKey` /
`EvidenceKeys` anywhere in the Profile Web surface, the full-coverage rule
for `RepositoryDrillDown`, the deterministic POST interaction that reaches
the existing Repository Proof entry, the exhaustive
`RepositoryEligibilityStatus` row-status mapping (presentation never reads
`AnalysisReference`), and the exact EN/TR copy for all of it. §8.5 gains the
`Supported`-but-empty rows for those two sections plus the rule that an empty
section renders as an ordinary H2 section rather than an empty disclosure
control. No upstream structured semantic truth changed: no concept, section,
availability state, canonical ordering, evidence rule, pattern rule or
inference prohibition was added, removed or altered, and no Domain,
Application or `ProfileReportProjector` behavior changed.
`ProfileReportProjector` remains locale-neutral. §8's User Language
Dictionary was NOT edited, §8.4's Meaning contract was NOT edited, and §11's
reading sequence was NOT edited. The existing section labels stay exactly as
they are, including `WhyThisConclusion` ("Why this view" / "Bu görünümü neden
görüyoruz?") and `RepositoryDrillDown` ("Repository evidence" / "Repository
kanıtı").

**Amended:** 2026-08-23 / Session 111 — §8.4 is fully superseded. The former
lossy two-presence-bit / four-variant Meaning copy did not answer the product
question with the actual canonical Profile truth. Meaning now deterministically
summarizes only the existing ordered items of `TechnicalFocusAreas` and
`EngineeringPracticePatterns` in a bounded Web-owned presentation object.
This adds no analysis, evidence, selection, score, ranking or inference
semantics. `RepresentativeRepositories`, Why, Repository evidence, warnings
and limitations remain excluded; `ProfileReportProjector` remains
locale-neutral. §8.5's detail-section empty copy and §11's reading sequence
are unchanged.

**Amended:** 2026-08-23 / Session 111 narrative refinement — §8.4's
item-aware copy is refined for HR/recruiter first-read readability. Meaning is
now a username-led, fluent deterministic relationship narrative rather than
two adjacent category sentences. It still consumes only the accepted Profile
identity plus the same two ordered pattern sections; no evidence or developer
semantics are added. The four allowed technical/practice relationship states,
their evidence-visibility language and their exact EN/TR templates are now
locked in §8.4.

**Amended:** 2026-08-23 / Session 111 human-narrative refinement — §8.4 is
refined again after live review: Meaning is explicitly a human explanation of
visible GitHub traces for both non-technical and technical readers. The active
shape is observation → plain-language practice explanation → cross-repository
repetition → evidence-bounded working-method synthesis. Every current
EngineeringPractice identity has a deterministic typed presentation mapping;
no person-level role, seniority, capability or quality semantics are added.

**Amended:** 2026-08-23 / Session 111 width + Starting Point usability
refinement — the prior P10-I4 text-only Starting Point rule is superseded.
Starting Point remains secondary to Meaning and preserves the canonical first-
read position, but every canonical representative repository now renders its
identity and a compact existing Repository Proof POST action. No authoritative
Description exists in the current typed `RepresentativeRepository` / Profile
report path, so no description is rendered, fetched, derived or invented.

**Amended:** 2026-08-23 / Session 111 Working Style card simplification — the
former analytical `EngineeringPracticePatterns` card heading and its generic
pattern labels are superseded for that card only. Its active heading is
`Working style` / `Çalışma biçimi`; each canonical EngineeringPractice item
renders a concise, deterministic, evidence-bounded noun phrase in canonical
order. Meaning's distinct job is the shorter cross-item human synthesis locked
by §8.4.9; the card does not make person-level methodology, capability or
quality claims.

**Amended:** 2026-08-23 / Session 111 Profile identity header + shareability
refinement — §8.7 replaces the visible internal-sounding `Profile view` /
`Profil görünümü` eyebrow with a public-profile identity header led by the
canonical successful Profile identity. The canonical GitHub profile URL enables
one safe secondary external action. Avatar and profile-level recency are absent
from current typed truth. The transient POST-only result has no stable
third-party-openable URL or dynamic social metadata, so Share, LinkedIn and
copy-link controls are not rendered; the three contract gaps are explicit.

**Amended:** 2026-08-23 / Session 111 first-read clarity correction — §8.4.9
supersedes the detailed-practice/glossary requirements in §8.4.2–§8.4.7.
Meaning remains deterministic and item-complete, but its active form is two
short synthesis paragraphs plus one quiet evidence-boundary sentence. §8.6 now
uses the visitor question `What is this based on?` / `Bunu neye dayanarak
söylüyoruz?` and translates each existing canonical conclusion trace into a
question, a bounded human reason and its unchanged supporting repositories.
The identity header is visually grouped without adding data. Starting Point is
not silently capped: the canonical selection has no global bounded priority.
Empty Limitations remains rendered because the canonical report contract does
not permit omitting a `Supported` section solely because it is empty.

**Amended:** 2026-08-23 / Session 111 approved mockup implementation — the
approved dark first-read mockup is the visual target for §8.7 and the Profile
first-read cards. This is a visual hierarchy amendment only: it does not add a
Profile field, alter canonical report order, change a selection rule or
authorize Web inference. Its explicit truth-preserving deviations are locked
in §8.7.6: no avatar/recency/share row, no project-type inference, no bounded
Starting Point subset or description, and no omission of canonical Supported
Limitations solely because it is empty.

**Amended:** 2026-08-23 / Session 111 STEP 2 — bounded Starting Point rail +
canonical avatar. Live review of real executions (up to 57 admitted
repositories) proved that an unbounded Starting Point rail destabilizes the
two-column first-read layout. §8.6.1 now authorizes a Web-only **presentation
bound**: at most 3 representative repositories are shown, taken as the first 3
items of the existing canonical `RepresentativeRepositories` order. This is
explicitly not a ranking, score, quality or "top" claim — see §8.6.1's order
caveat — and the canonical report/selection itself is unchanged and uncapped.
The rendered rail keeps a stable desktop height with an internal vertical
scroll on the repository list only; the single-column breakpoint removes the
bound and returns to natural flow. The **Starting Point Prioritization
Contract Gap** is narrowed to presentation-bound-only: a true importance/
priority ordering remains unauthorized and undesigned. The **Starting Point
Description Contract Gap** is unchanged. Separately, §8.7.2's **Profile Avatar
Contract Gap is now closed for collection and presentation**: the collector
now reads GitHub's own `avatar_url` from the same profile-metadata response,
validated as an absolute HTTPS URI, and the header renders it when available;
the decorative `aria-hidden` GitHub mark remains the fallback only when a
typed avatar is legitimately unavailable. No avatar is derived, generated or
guessed from the username. Recency and Share remain open per §8.7.2/§8.7.4.

**Amended:** 2026-08-23 / Session 111 STEP 3 — canonical Starting Point
repository Description. §8.6.1a closes the **Starting Point Description
Contract Gap** for the specific case of GitHub's own repository Description:
each presented Starting Point item now shows one short description line
sourced only from the provider-returned `description` field of the same
existing repository-metadata response already used for `fork`/`language`/etc
(zero additional GitHub requests). It travels via a new non-evidence
`RepositoryPresentationMetadata` lookup — `GitHubProfileSnapshot.
RepositoryPresentationMetadata` → `ProfilePipelineResult.
RepositoryPresentationMetadata` → `ProfileModel.RepositoryPresentationMetadata`
— mirroring STEP 2's avatar path and never touching `RepresentativeRepository`,
representative selection, `DeveloperPublicEvidenceProfile` or `ProfileReport`.
The text is preserved verbatim (trimmed only), never translated, rewritten,
summarized or inferred; a null/blank Description renders a fixed neutral
localized fallback describing metadata absence only. Razor's default encoding
renders it as plain text — never `Html.Raw`, never Markdown, never a link —
and it is visually bounded to ~2 lines via CSS line-clamp without semantic
truncation. The max-3 presentation bound, deterministic order and desktop-
scroll/mobile-natural-flow behavior from STEP 2 are unchanged.

**Amended:** 2026-08-27 / Session 136 — Profile first-read information depth.
Two bounded additions, no upstream semantic change. (1) §8.6.1 / new §8.6.1b:
the Starting Point item's locked "renders exactly" shape grows from three
elements to four, so that the canonical pattern → starting-point relationship
becomes user-visible. The relationship is read only from the already-projected
`RepresentativeRepository.PatternSupports` identities (family + pattern key)
that Web already holds and never rendered; no new evidence, taxonomy,
selection semantics, count, ordering or importance claim is introduced, and
the **Starting Point Prioritization Contract Gap** stays open and unchanged —
showing which repeated result a repository was selected for is not a ranking.
(2) New §8.8 locks the Profile first-read information-depth contract: which
information is FIRST READ, which is PROGRESSIVE DISCLOSURE and which is DEEP
EVIDENCE, the semantic-level ladder behind the already-locked section order,
first-read behavior for the empty/partial/warning boundary cases, and the
implementation-input boundary for the future Razor/CSS work. §8.8 consumes —
and does not restate or replace — the first-read *question* set owned by
`docs/product/public-profile-evidence-product-contract.md` §25 and the
outcome-level first-read *priority* owned by
`docs/product/profile-proof-distinctive-outcomes-contract.md` §8. No section,
availability state, canonical report ordering, evidence rule, pattern rule or
inference prohibition was added, removed or weakened.

------------------------------------------------------------------------

# 1. Purpose

This document defines the user-facing presentation direction of GitHub
Proof.

The purpose is to establish:

-   product narrative,
-   user language,
-   presentation principles,
-   semantic boundaries,
-   copy guidelines.

This document does not modify:

-   domain behavior,
-   signal definitions,
-   scoring rules,
-   eligibility rules,
-   analysis contracts.

It defines how existing product capabilities are communicated to users.

------------------------------------------------------------------------

# 2. Core Product Message

GitHub Proof's primary user-facing message:

> GitHub'ınızda ne olduğunu değil, ne anlattığını görün.

The product is not positioned as a metric dashboard or evaluation
system.

The purpose is to help users understand the meaning behind observable
GitHub activity.

------------------------------------------------------------------------

# 3. User Problem

GitHub profiles contain many observable elements:

-   repositories,
-   commits,
-   issues,
-   pull requests,
-   workflows,
-   documentation,
-   technical artifacts.

However, raw visibility does not automatically create understanding.

Users need help answering:

-   Where should I start?
-   What does this profile communicate?
-   Which visible patterns are meaningful?
-   What evidence supports this understanding?

------------------------------------------------------------------------

# 4. Product Position

GitHub Proof does not:

-   score developers,
-   rank developers,
-   determine seniority,
-   decide hiring suitability,
-   detect AI authorship,
-   make quality judgments.

GitHub Proof does:

-   organize observable GitHub traces,
-   reveal recurring production patterns,
-   provide a meaningful starting point,
-   connect interpretation back to evidence.

------------------------------------------------------------------------

# 5. User Transformation

## Before

My GitHub contains many things.

I do not know what matters.

------------------------------------------------------------------------

## After

I understand what my GitHub profile communicates.

I know where to start.

I can inspect the evidence behind the view.

------------------------------------------------------------------------

# 6. Experience Principle

The user experience follows this order:

    Meaning
    ↓
    Direction
    ↓
    Starting Point
    ↓
    Evidence

The user first understands the overall picture.

Detailed evidence is available progressively.

------------------------------------------------------------------------

# 7. Core Concepts

## Trace

A trace is an observable indication left in a public GitHub profile.

Examples:

-   repository structure,
-   commit history,
-   documentation,
-   tests,
-   workflows.

A trace is not a judgment.

------------------------------------------------------------------------

## Visible Direction

A visible direction is a recurring technical production tendency
observed from multiple traces.

**Source (Milestone B / Profile V1):** Visible Direction is derived from
`TechnicalFocusAreas` and `EngineeringPracticePatterns` --- Supported,
active pattern concepts that already produce output in this product
version. It is not derived from `RoleEvidenceAlignment`. RoleEvidenceAlignment
is a separate, Deferred capability for Milestone B / Profile V1
(`docs/product/public-profile-evidence-product-contract.md` §14): it
produces no active user-facing output today, and this document does not
present it as one. See the User Language Dictionary note below.

The system must not state:

> This person is a backend developer.

The system may state:

> This profile shows recurring traces related to backend systems.

------------------------------------------------------------------------

## Starting Point

A starting point is the recommended place to begin understanding a
profile.

It is not:

-   the best project,
-   the highest quality project,
-   the most successful project.

It is:

> A useful entry point for understanding visible production traces.

------------------------------------------------------------------------

## Evidence

Evidence is the observable source supporting a presented view.

The experience should allow users to move from:

    Meaning
    ↓
    Evidence

when they want deeper inspection.

------------------------------------------------------------------------

# 8. User Language Dictionary

| Internal concept | English (`en`) | Turkish (`tr`) |
|---|---|---|
| Evidence | Visible trace / Evidence | Görünen iz / Kanıt |
| Signal | Trace | İz |
| Pattern | Recurring trace | Tekrar eden iz |
| Representative Repository | Starting point | Başlangıç noktası |
| TechnicalFocusAreas | Visible technical direction | Görünen teknik yön |
| EngineeringPracticePatterns | Working style | Çalışma biçimi |
| WhyThisConclusion | What is this based on? | Bunu neye dayanarak söylüyoruz? |
| Role Evidence Alignment (Deferred --- not active in V1) | No active V1 user language; future direction only | Aktif V1 kullanıcı dili yok; yalnız gelecek yönü |
| Profile Report | Profile view | Profil görünümü |

**Terminology note (vocabulary simplification only):** This dictionary is a
presentation-layer simplification. `Evidence`, `Signal` and `Pattern` remain
distinct canonical concepts at the domain level
(`Evidence != Signal != Pattern`); unifying them under related user-facing
words (the "iz" root) is a presentation choice and does not merge or replace
their underlying domain semantics, which are owned by
`docs/architecture/public-profile-evidence-domain-model.md` and
`docs/architecture/public-profile-evidence-patterns.md`.

`Role Evidence Alignment` is listed here for completeness of the internal
concept inventory, not because it has an active V1 user language today: it
is Deferred for Milestone B / Profile V1
(`docs/product/public-profile-evidence-product-contract.md` §14) and
produces no user-facing output in this version. The "Visible Direction"
narrative concept (above) is jointly evidenced by `TechnicalFocusAreas` and
`EngineeringPracticePatterns`, but the two remain two distinct rendered
sections with two distinct labels, never one merged label or one merged
section (locked in
`docs/product/p10-web-profile-flow-implementation-plan.md` §13.9 — see the
split dictionary rows above).

**Historical rendered-language note (Session 093, superseded by Session
101):** Session 093 recorded the then-current fixed-English rendered Web
surface and treated Turkish narrative copy as product-direction prose rather
than literal Razor/UI markup. Session 101 replaces that rendering decision
with the explicit EN/TR contract above. The historical observation remains
useful context only; it is not the current locale rule.

To be explicit about what changed and what did not, consistent with the
header note above: the §8 `TechnicalFocusAreas` / `EngineeringPracticePatterns`
dictionary split into two distinct rows **did happen** in this session (see
the header note and the split rows above). Upstream structured semantic
truth --- the concepts, sections, availability states, and canonical
ordering owned by
`docs/architecture/public-profile-evidence-report-contract.md` and
`docs/architecture/public-profile-evidence-patterns.md` --- **did not
change**; only this document's own presentation-dictionary row changed, to
stop contradicting itself. This rendered-language note itself introduces no
further semantic or dictionary change beyond that already-documented
split. Session 101 separately supersedes the former fixed-English boundary.

## 8.1 Web Culture Resolution and Persistence

`GitHubProof.Web` supports exactly `en` and `tr`.

Resolution precedence:

```text
explicit user choice persisted with the ASP.NET Core culture cookie
→ browser Accept-Language preference / q ordering
→ English fallback
```

Browser negotiation chooses the first supported language family:

- `tr`, `tr-TR`, `tr-*` → `tr`,
- `en`, `en-US`, `en-GB`, `en-*` → `en`,
- unsupported families are ignored while searching later browser preferences,
- no supported preference → `en`.

The implementation contract prefers
`CookieRequestCultureProvider` →
`AcceptLanguageHeaderRequestCultureProvider` → default `en`. Ordinary
language resolution has no `QueryStringRequestCultureProvider`. No external
localization package is required.

Geolocation, IP-derived location, device location and browser Geolocation API
must never participate in language selection. Explicit user choice always
wins over browser preference.

The header exposes a quiet persistent `EN | TR` utility alongside theme
control. It is not a primary CTA. Current selection is perceivable and the
control is keyboard/screen-reader accessible with the existing target/focus
contract.

Transient Repository/Profile results are POST-produced and not persisted.
Changing language from such a result persists the new culture, never silently
reruns GitHub analysis, and navigates to the localized landing. The last
`GitHubTarget` may be prefilled only when the existing entry model can carry it
safely; a new analysis begins only after an explicit Analyze submission. No
result cache or persistence is introduced by P10-I2A.

While the landing analysis form is actively submitting a synchronous analysis
POST (its existing submitting / `aria-busy` state, §8.3), the `EN | TR` control
is inert: it initiates no navigation and no culture change, so a language click
cannot abandon the in-flight analysis. It reuses that form's own submitting
state as the single busy signal, is perceivably and accessibly disabled for that
window, and returns to normal on the existing reset / BFCache-restore path. This
does not translate, restart or persist the running analysis. Once the page is no
longer analyzing, the behavior above is unchanged.

## 8.2 Whole-Web Presentation Ownership

Selected request culture owns all user-facing Web presentation, including:

- `<html lang>`, header utilities, language selector and theme accessibility
  copy,
- landing hero/supporting copy, input label/placeholder/Analyze button and
  validation copy,
- analysis-running copy,
- Repository Proof and Profile Proof Web presentation,
- user-facing failures, rendered limitations/warnings and accessibility
  labels/titles/status messages.

Repository Proof keeps its existing Application EN/TR catalogs and projector;
Web passes the resolved request UI culture to that projector. Profile keeps a
locale-neutral `ProfileReport` and a locale-neutral `ProfileReportProjector`;
Web maps existing structured identities deterministically into the selected
culture. `ProblemAreaPatterns` remains Unsupported/unrendered and
`RoleEvidenceAlignment` remains Deferred/unrendered.

Literal evidence is never translated: repository names, GitHub usernames,
URLs, paths, branches, SHAs, tags, raw evidence values, or programming/GitHub
language identities such as C#, JavaScript and Python.

## 8.3 Truthful Analysis-Running Language

The unified landing input may immediately announce an indeterminate state
after native form constraint validation succeeds:

```text
EN: GitHub Proof is processing your request…
TR: GitHub Proof isteğinizi işliyor…
```

A neutral explanation headed `What this analysis includes` / `Bu analiz
neleri içerir` may list:

| English | Turkish |
|---|---|
| Validating the GitHub target | GitHub hedefi doğrulanıyor |
| Reading public GitHub data | Herkese açık GitHub verileri okunuyor |
| Collecting visible evidence | Görünür kanıtlar toplanıyor |
| Analyzing traces | İzler analiz ediliyor |
| Preparing the view | Görünüm hazırlanıyor |

This indeterminate state stays **canonical**. It is what the visitor sees
whenever live stage state is unavailable for any reason, and it needs no error
surface of its own when that happens.

The form/container exposes `aria-busy`; duplicate submit is blocked without
disabling `GitHubTarget` out of form serialization; BFCache/back restoration
clears stale state; and motion respects `prefers-reduced-motion`.

### 8.3.1 Session 138 amendment — live stage state

Before Session 138 this section additionally read that the analysis-description
lines "describe the general analysis, not individually tracked runtime states",
that they "never receive completed/active/pending labels ... without a future
telemetry contract that proves those states", and that no SSE, SignalR,
polling, streaming response or new backend progress telemetry was introduced.

Session 138 is that contract. The runtime semantics — run identity, server
state, transport, request lifecycle, terminal delivery, refresh/reconnect,
security and update policy — are owned by
`docs/architecture/live-analysis-progress-runtime-contract.md`. This section
keeps ownership of the language and the visible shape only, and adds no runtime
semantics of its own.

Two conditions from the original prohibition survive unchanged and are
strengthened here:

- **A stage may carry a completed/active/pending state only if that state comes
  from the server.** The client never advances, times, interpolates or animates
  a stage it has not been told is in that state.
- **`ProfileExecutionTelemetry` remains completion/observation telemetry, not a
  live UI progress source.** Its counters may not be rendered, transported or
  reworded into user-facing progress.

The neutral `What this analysis includes` explanation above remains available
unchanged as a *description*. It is a separate thing from the stage list below
and it never acquires stage state.

The two are **mutually exclusive in one view**: three of its **English** lines
are also stage labels (§8.3.2), so rendering both would show the same sentence
twice, once with state and once without. The Turkish columns no longer coincide
— the stage labels are verbal nouns, the description lines present-continuous
(§8.3.2) — but the rule is the same in both languages, because two lists
describing the same phases is duplication regardless of wording. When the stage
list renders, the description list does not.

### 8.3.2 Locked stage copy

The stage identities and their order are the runtime contract's §4 and §5. The
copy for each is locked here. The label describes the work; the **state** is
carried by the state token (§8.3.3), never by the label, so one label serves all
three states.

Repository Proof — four stages, in order:

| Stage identity | English | Turkish |
|---|---|---|
| `repository-target-validated` | Validating the GitHub target | GitHub hedefinin doğrulanması |
| `repository-public-data-read` | Reading public GitHub data | Herkese açık GitHub verilerinin okunması |
| `repository-traces-analyzed` | Analyzing traces | İzlerin analiz edilmesi |
| `repository-proof-prepared` | Preparing the Repository Proof | Repository Proof'un hazırlanması |

Profile Proof — three stages, in order:

| Stage identity | English | Turkish |
|---|---|---|
| `profile-target-validated` | Validating the GitHub profile | GitHub profilinin doğrulanması |
| `profile-public-surface-read` | Reading the public repositories | Herkese açık repository'lerin okunması |
| `profile-evidence-examined` | Examining repository evidence and recurring traces | Repository kanıtlarının ve tekrar eden izlerin incelenmesi |

The Turkish labels are deliberately **verbal nouns** (`doğrulanması`), not the
present-continuous `doğrulanıyor` used by the description list above. A stage
label must serve all three states, and Turkish `-iyor` is a finite verb: pairing
it with a state token would produce `GitHub hedefi doğrulanıyor · Tamamlandı` —
"is being validated · Completed" — which contradicts itself. English gerund
phrases carry no such tense, so the EN column needs no equivalent change. This
is why the TR stage labels differ from the TR description lines even where the
EN ones coincide; the two lists never render together anyway (§8.3.1).

`Trace` and `Evidence` are used in their §7 Core Concept sense. No stage label
may name an analyzer, an evaluator, a rule version, a policy, a signal key, an
evidence key, a collector or any other internal.

`profile-evidence-examined` describes one bounded phase in which both repository
evidence and recurring traces are worked on. It is deliberately **not** worded
as two sequential claims, and it never says traces were *found*: a valid run may
promote no pattern at all, and the wording stays true for a `Supported + empty`
outcome.

Every stage label carries only EN/TR copy from `WebPresentationLocalizer`
(§8.2). No progress string may originate in JavaScript or in a transport
payload.

### 8.3.3 Stage state presentation

Exactly three states, matching the runtime contract's §7 state machine:

| State | English | Turkish |
|---|---|---|
| `PENDING` | Pending | Bekliyor |
| `ACTIVE` | In progress | Sürüyor |
| `COMPLETED` | Completed | Tamamlandı |

- State must never be conveyed by colour alone. Each stage renders a state
  token, and the state name above is exposed to assistive technology as text;
  decorative glyphs are `aria-hidden`.
- **A checkmark glyph is authorized for `COMPLETED`**, and this section is its
  only owner. The pre-Session-138 prohibition on checkmarks meant a checkmark
  not backed by server state; a `COMPLETED` state that came from the server is
  exactly what it was protecting against being faked. `ACTIVE` and `PENDING`
  glyphs carry no completion meaning. No glyph may appear on a stage whose state
  the server has not reported.
- There is no visible failure, cancelled or skipped state. A run that ends
  before reaching a stage leaves it `PENDING`, and the terminal outcome arrives
  as the result or error page that replaces the whole view.
- Stage completion describes a **phase finishing**, never the completeness of
  the result. The stage list must never carry the words partial, incomplete,
  complete, failed or any other completeness vocabulary — a `Partial` profile is
  a terminal success and its own notice already owns that truth.

### 8.3.4 Percentage, ETA and the activity indicator

Percentage and ETA remain **PROHIBITED**, now on measured evidence rather than
absence of a contract: no percentage, no progress-bar fill fraction, no
remaining-time estimate, no elapsed-time figure, no count and no denominator.
Repository count is not a valid denominator, and stage progress is not time
progress. The evidence is in the runtime contract §11.

The existing indeterminate indicator is retained beside the stage list and means
exactly one thing:

~~~text
"analysis is still active"    NOT    "this fraction is done"
~~~

It has no determinate fill state and continues to respect
`prefers-reduced-motion`.

### 8.3.5 Shape

The stage list renders **inline**, in the existing analysis-status region below
the unified input. A modal or a full-screen overlay is not authorized: the whole
progress view is replaced by the result or error page when the analysis request
returns, so a modal buys no exit behavior; duplicate submit is already prevented
by form state; and a modal invites a cancel affordance that
`live-analysis-progress-runtime-contract.md` §14 does not authorize.

Both stage lists are present in the landing markup, fully localized and in
`PENDING` state, and stay hidden until the server reports which one applies.
This keeps every string server-owned and keeps the transport free of copy.

The status region's shape changes, and this is the authorized change. Today the
whole region carries `role="status" aria-live="polite" aria-atomic="true"`,
which would re-announce every stage on every update. The target shape is:

~~~text
analysis status region        container; no live-region role
  stage list                  ordinary content, all stages, visible
  live region                 role=status aria-live=polite aria-atomic=true
                              exactly one sentence: the active stage label
~~~

The live region keeps the same three attributes it has today; it is narrowed to
a single-sentence child rather than removed. The form keeps `aria-busy`.
`p10-web-profile-flow-implementation-plan.md` records the pre-Session-138
delivered shape and is accurate as such; it does not constrain this one.

### 8.3.6 Announcement policy

- The visible stage list is ordinary content, not a live region.
- One polite, atomic live region carries exactly one sentence. Until the first
  server snapshot arrives it carries the canonical indeterminate sentence
  (`GitHub Proof is processing your request…` / `GitHub Proof isteğinizi
  işliyor…`), exactly as it does today; from the first snapshot on it carries
  the label of the currently active stage. If no snapshot ever arrives it keeps
  the indeterminate sentence and nothing is ever announced for progress.
- It changes at most once per stage transition — at most four times per run,
  plus the one switch away from the indeterminate sentence.
- When the run reports its terminal snapshot — every stage completed and no
  active stage — the live region **does not change**. It keeps the last stage
  label it announced. Nothing is announced for "finished": the result page is
  about to replace the whole view and is itself the announcement, and a final
  sentence would be a fifth announcement for a state the visitor sees for at
  most one poll.
- Poll ticks, unchanged snapshots, the activity indicator, counts and transport
  state are **never** announced.
- `aria-busy` on the form remains the coarse busy signal, and BFCache/back
  restoration resets every stage to `PENDING` and re-hides the region.

## 8.4 P10-I3 Meaning Layer Deterministic Synthesis Contract

This section is the normative owner of the P10-I3 Meaning layer's exact
presentation shape and user-facing copy. It adds no analysis semantics: it
fixes how already-projected typed truth is rendered. It fully supersedes the
Session 103 M1–M4 presence-bit contract and the earlier Session 111
category-sentence copy. The active Meaning behavior is a deterministic human
explanation of visible GitHub traces, not an analysis report.

### 8.4.1 Projection Source

Meaning reads only these existing canonical values:

```text
accepted Profile identity / username
TechnicalFocusAreas.Items
EngineeringPracticePatterns.Items
```

Binding rules:

- The username is the accepted canonical Profile identity already held by the
  successful Profile Web route. It is literal data, never localized, inferred,
  normalized again or replaced with an example identity.
- Each section must carry its canonical section key and `Supported`
  availability. A wrong key, unsupported availability or an item from the
  wrong pattern family is a contract violation; presentation fails loudly and
  never invents an empty state.
- The input preserves the existing report item order. Meaning may localize the
  canonical `PatternKey` through the existing Web pattern-label mapping, but
  must not sort, rank, group, cap, filter or otherwise reinterpret items.
- `PublicProfileSummary` and `ProfileSummaryFact` do not participate. Their
  count facts are neither read nor rendered.
- `RepresentativeRepositories`, `WhyThisConclusion`, `RepositoryDrillDown`,
  warnings, limitations, `ProblemAreaPatterns` and
  `RoleEvidenceAlignment` never participate. Raw GitHub data, snapshots,
  signals, evidence and eligibility results are never re-analyzed in Web.
- The rendering partial receives only a Web-owned localized
  `ProfileMeaningPresentation`, not a report, section, item, repository,
  evidence or analysis object. Narrative construction stays in the Web
  localizer, never Razor.

### 8.4.2 HR/Recruiter Readability and Locked Shape

Meaning serves both a non-technical recruiter/HR reader and a technical visitor
who wants a fast profile overview. It must sound like a careful person briefly
explaining what is visibly there, not like a list of detected signals, a
taxonomy explanation or a developer assessment.

Meaning is exactly three concise prose blocks, in this order:

1. username-led technical-direction observation: what technologies are seen,
   that they recur across different repositories, and what this makes visible
   about the profile's technical work;
2. a human practice explanation: every repeated canonical practice is explained
   in ordinary language, its repetition across repositories is stated, and a
   short synthesis says which working methods those public traces make visible;
3. one short scope/provenance sentence.

The first sentence of the first block must begin with the literal username;
generic openings such as “This profile” / “Bu profil” are prohibited. The
dedicated Direction and Practices sections remain the detailed canonical
surfaces below Meaning.

The communication sequence is mandatory:

```text
visible observation
→ ordinary-language explanation when a practice term appears
→ repetition across different repositories
→ visible working-method synthesis
```

“Visible working method” means only a method made visible by repeated public
repository traces. It never claims that this is how the person always works in
reality.

### 8.4.3 Typed Practice Explanation Mapping

Practice explanations are deterministic Web presentation mappings keyed only
by the canonical EngineeringPractice `PatternKey`. They must not inspect or
infer semantics from arbitrary display text. Every participating item is
explained in existing canonical report order; no item may be silently omitted.

| Canonical key | Plain-language meaning to communicate |
|---|---|
| `automated-testing` | Tests that check whether code behaves as expected; their visible traces recur across repositories. |
| `continuous-integration` | Visible workflows aimed at automating builds, tests or similar checks as code changes; do not claim a particular run succeeded. |
| `documentation` | Content that explains a project or how to set it up or use it; do not claim documentation quality or completeness. |
| `release-usage` | Visible GitHub traces that mark particular version or delivery points; do not infer semantic versioning, stability or active maintenance. |
| `issue-usage` | Visible records used to track bugs, tasks or development needs; do not infer project-management quality. |
| `pull-request-usage` | Visible traces of some changes handled through a separate change flow; do not claim that a human review occurred. |

The exact per-item prose clauses are locked below. Selection is keyed only by
the already-validated canonical identity; this table is not a display-text
heuristic.

| Canonical key | English (`en`) clause | Turkish (`tr`) clause |
|---|---|---|
| `automated-testing` | `Automated testing appears across different repositories; these are tests that check whether code behaves as expected, so their traces are not limited to one project.` | `Otomatik test kullanımı farklı repository'lerde tekrar görülüyor; yani kodun beklenen şekilde davranıp davranmadığını kontrol eden testlere ait izler tek bir projeyle sınırlı değil.` |
| `continuous-integration` | `Continuous integration appears across different repositories; in other words, there are visible workflow traces aimed at automating builds, tests, or similar checks as code changes.` | `Sürekli entegrasyon da farklı repository'lerde tekrar görülüyor; yani kod değişiklikleriyle birlikte build, test veya benzeri doğrulamaları otomatikleştirmeye yönelik workflow'lara ait izler var.` |
| `documentation` | `Documentation appears across different repositories; this means traces of content that explains a project or how to set it up or use it are not limited to one project.` | `Dokümantasyon farklı repository'lerde yeniden görülüyor; yani projeyi açıklayan, nasıl kurulacağını veya kullanılacağını anlatan içerik bırakma pratiği tek bir projeyle sınırlı değil.` |
| `release-usage` | `Release practice appears across different repositories; this means visible GitHub traces that mark particular version or delivery points are not limited to one project.` | `Sürüm yayınlama pratiği farklı repository'lerde tekrar görülüyor; yani belirli sürüm veya teslim noktalarını görünür olarak işaretleyen GitHub izleri tek bir projeyle sınırlı değil.` |
| `issue-usage` | `Issue usage appears across different repositories; in other words, there are visible traces of records used to track bugs, tasks, or development needs beyond one project.` | `Issue kullanımı farklı repository'lerde tekrar görülüyor; yani hata, görev veya geliştirme ihtiyaçlarını görünür biçimde takip etmeye yarayan kayıtların tek bir projeyle sınırlı olmadığına dair izler var.` |
| `pull-request-usage` | `Pull request usage appears across different repositories; this means there are GitHub traces of some changes being handled through a separate change flow.` | `Pull request kullanımı da farklı repository'lerde tekrar görülüyor; yani bazı değişikliklerin ayrı bir değişiklik akışı üzerinden ele alındığına dair GitHub izleri var.` |

### 8.4.4 Four-State Human Narrative

Let `{technical-labels}` mean every TechnicalFocus label in existing report
order. For present practices, `{practice-explanations}` means the ordered,
typed explanation sentence for every current practice item, and
`{working-methods}` means each corresponding ordinary-language method phrase
in the same order, joined as §8.4.5 prescribes.

The exhaustive state rules are:

| State | Required human explanation |
|---|---|
| Technical present; practices present | The username-led first block explains the recurring technologies across different repositories. The second block introduces recurring ways of working, renders every `{practice-explanations}`, then closes: the combined public traces make `{working-methods}` visible across different repositories. |
| Technical present; practices empty | The username-led first block explains the recurring technologies. The second block says no equally recurring practice is visible, while explicitly saying this is not a claim that any practice is absent. |
| Technical empty; practices present | The username-led first block says no one recurring technical direction is visible. The second block renders every `{practice-explanations}`, then says the public traces make `{working-methods}` visible rather than one recurring technical direction. |
| Technical empty; practices empty | The username-led first block says no recurring technical direction or working practice is visible. The second block says this is not an assessment of the person; it only describes what the currently visible public work does not make repeat across repositories. |

The fixed first-block templates are:

| Condition | English (`en`) | Turkish (`tr`) |
|---|---|---|
| one technical label | `{username}'s GitHub work shows a recurring technical direction around {technical-labels} across different repositories. {technical-labels} appears again across different repositories, making it visible as a recurring area of technical work in the profile.` | `{username} GitHub çalışmalarında ağırlıklı olarak {technical-labels} etrafında tekrar eden bir teknik yön gösteriyor. {technical-labels} farklı repository'lerde yeniden görülüyor; bu da bu teknolojinin profil boyunca tekrar eden bir teknik çalışma alanı olduğunu görünür kılıyor.` |
| multiple technical labels | `{username}'s GitHub work shows recurring technical traces around {technical-labels} across different repositories. Seeing these technologies recur across different repositories makes them visible as recurring areas of technical work in the profile.` | `{username} GitHub çalışmalarında {technical-labels} etrafında tekrar eden teknik izler gösteriyor. Bu teknolojilerin farklı repository'lerde yeniden görülmesi, bunların profil boyunca tekrar eden teknik çalışma alanları olduğunu görünür kılıyor.` |
| no technical labels, practices present | `{username}'s GitHub work does not currently make one recurring technical direction visible across public repositories.` | `{username} GitHub çalışmalarında repository'ler arasında tekrar eden tek bir teknik yön şu anda görünür hâle gelmiyor.` |
| no technical labels, practices empty | `{username}'s GitHub work does not currently make a recurring technical direction or working practice visible across public repositories.` | `{username} için herkese açık GitHub çalışmalarında şu anda repository'ler arasında tekrar eden belirgin bir teknik yön veya üretim pratiği görünmüyor.` |

For present practices, the fixed second-block opener is `Looking at ways of
working, there are recurring public traces as well.` / `Çalışma biçimine
baktığımızda da bazı tekrarlar var.` when technical labels are present, and
`Looking at ways of working, recurring public traces are visible.` / `Çalışma
biçimine baktığımızda ise bazı tekrarlar görünür hâle geliyor.` otherwise. It
renders every ordered per-item clause, then the fixed synthesis says that the
combined traces make `{working-methods}` visible across different repositories.

The Turkish and English templates must be natural prose, remain byte-identical
for the same input and culture, and preserve the exact semantic constraints
above. The final, always-present provenance block is exactly:

| English (`en`) | Turkish (`tr`) |
|---|---|
| `This summary is based only on traces visible in public GitHub work under the current analysis rules.` | `Bu anlatım yalnızca herkese açık GitHub çalışmalarında görülebilen izlere dayanıyor.` |

The empty states are observation outcomes under the current rules, never a
developer deficit. The provenance sentence communicates scope only: it does
not expose algorithms, scoring or implementation detail.

### 8.4.5 Locale-Aware Label Joining

Joining is deterministic and preserves the canonical report order:

- one label: unchanged;
- two labels: `C# ve .NET` in Turkish; `C# and .NET` in English;
- three or more: `C#, .NET ve ASP.NET Core` in Turkish;
  `C#, .NET, and ASP.NET Core` in English.

There is no hidden item cap, no alphabetical reorder, no ranking and no
substitution of a number or count for labels. Programming/GitHub language
identities remain literal and are not translated.

### 8.4.6 Traceability and Ownership

| Rendered text | Authorizing typed field(s) | Exact condition |
|---|---|---|
| Username-led technical block | accepted Profile identity + `TechnicalFocusAreas.Items` | all item labels in current order, or its truthful empty relationship state |
| Practice/explanation/synthesis block | `TechnicalFocusAreas.Items` + `EngineeringPracticePatterns.Items` | the four locked relationship states; every present practice's typed explanation and working-method phrase in current order |
| Provenance paragraph | selected request culture | always, as the locked presentation scope sentence |
| Section labels | `ProfileReportSectionKey` | existing §8 dictionary mapping, unchanged |

The deterministic localizer is Web-owned. `ProfileReportProjector` remains
locale-neutral and is not changed for Meaning. Same two ordered sections plus
the same culture always produce byte-identical presentation text.

### 8.4.7 Prohibitions

The Meaning layer must never:

- read `RepresentativeRepositories`, Why, Repository evidence, warnings,
  limitations, raw evidence, snapshots, signals, eligibility results or
  scores; or carry repository identity, selection detail or evidence trace
  into the Meaning surface;
- render any count, number, percentage, ratio, badge or counter;
- express comparison, ranking, magnitude, or a strong/weak axis;
- use `strong`, `weak`, `senior`, `junior`, `experienced`, `skilled`,
  `expert`, `employable`, `suitable`, `best`, `top`, `impressive`,
  `consistent`, or their Turkish equivalents;
- infer role, seniority, employability, capability, quality, confidence, AI
  authorship, personality, years of experience, developer activity state,
  backend/frontend/full-stack identity or a strength/weakness judgment;
- introduce a taxonomy mapping such as `C# → backend developer`
  (`docs/product/p10-web-profile-flow-implementation-plan.md` §13.15);
- collapse `Unsupported` and `Deferred` into one "not found" state;
- render an empty card, label or empty copy for `ProblemAreaPatterns` or
  `RoleEvidenceAlignment`;
- present `Supported + empty` as a negative developer finding;
- state that a person is a developer of, expert in, skilled at, experienced
  with or capable in a technology or production practice; presentation may
  say only that visible repeated use/traces appear across public repositories;
- expand practice labels into glossary definitions; the active §8.4.9 copy
  keeps all canonical items in one bounded working-method synthesis;
- use an LLM, external service, runtime heuristic or new analysis; or produce
  prose outside the exact deterministic templates and label joining locked
  above.

### 8.4.8 Working Style Detail Card

`EngineeringPracticePatterns` has a second, separate presentation job from
Meaning: it is the compact first-read card that answers which working methods
are visibly repeated across the public work. Its heading is exactly `Working
style` / `Çalışma biçimi`.

When `Supported` and non-empty, the card renders every item from
`EngineeringPracticePatterns.Items` in existing canonical report order as a
short scannable bullet row. Each row is exactly one deterministic localized
noun phrase keyed only by the canonical EngineeringPractice `PatternKey`:

| Canonical key | English (`en`) | Turkish (`tr`) |
|---|---|---|
| `automated-testing` | Automated testing | Otomatik test kullanımı |
| `continuous-integration` | Automated integration checks | Otomatik kontrol süreçleri |
| `documentation` | Documentation | Dokümantasyon |
| `release-usage` | Published releases | Sürüm yayınları |
| `issue-usage` | Issue tracking | Issue ile takip |
| `pull-request-usage` | Pull request workflow | Pull request ile değişiklik akışı |

The list is complete: no item may be alphabetically reordered, ranked, capped,
filtered or omitted. A label is never derived from arbitrary display text, and
an unknown canonical identity is a presentation contract violation rather than
a fallback label.

The card contains no explanatory paragraph per item, signal/category term,
metric, score, progress bar, percentage or person-level conclusion. In
particular it must not say that the person tests while working, continuously
checks code, manages changes professionally, values documentation or works
with discipline. The labels describe only visible repeated methods; they do
not claim quality, motivation, behavior outside the observed public work,
capability, role or seniority. Meaning §8.4.9 connects these same canonical
items into one short human synthesis and does not redefine them.

### 8.4.9 Active First-Read Clarity Supersession

This subsection is the active Meaning presentation rule. It explicitly
supersedes every conflicting requirement in §8.4.2–§8.4.7 that requires
per-practice definitions, explanatory clauses or a glossary-like expansion.
The earlier detailed templates remain historical Session 111 review context;
they are not active copy.

Meaning is exactly:

1. one short paragraph naming every ordered canonical
   `TechnicalFocusAreas.Items` label and synthesizing what visibly repeats;
2. one short paragraph naming every ordered canonical
   `EngineeringPracticePatterns.Items` through its concise §8.4.8 Working
   Style label and synthesizing what working methods visibly repeat;
3. the existing quiet evidence-boundary sentence.

The first paragraph begins with the literal accepted username. One technical
item states that the item recurs across repositories. Multiple items state
that the Profile shows technical work spanning more than one technology. This
does not introduce project/application types. Repository names and languages
do not authorize labels such as API, backend, authentication, realtime, Web,
mobile or AI: **Project-Type Presentation Pending Canonical Contract**.

The second paragraph is not a glossary. For non-empty practice items it uses
one locale-aware joined list, preserving every canonical item in report order
with no cap, followed by one short relationship sentence that does not repeat
the item list. It does not explain how
tests, CI, pull requests, documentation, releases or issues work. The compact
Working Style card remains the structured scan surface for those same items.

For `TechnicalFocusAreas` present plus `EngineeringPracticePatterns` empty,
the second paragraph says only that no equally recurring working method is
visible across repositories and that the current GitHub view offers more
limited traces of how the work was carried out. This is an evidence-surface
observation, never a developer deficit, absence-of-practice claim or quality
judgment.

All other §8.4 boundaries remain active: only the accepted username and the two
Supported ordered pattern sections participate; there is no raw-data analysis,
sorting, ranking, hidden top-N, count, score, role/seniority/capability/quality
inference, LLM or Razor-side narrative generation. Same canonical input and
culture produce byte-identical output.

## 8.5 Supported + Empty Presentation Copy (P10-I3 / P10-I4 Sections)

Locked neutral EN/TR copy for the `Supported`-but-empty case of the five
report sections whose `Supported`-but-empty Profile Web presentation is locked
across P10-I3 and P10-I4 --- the three first-read sections locked by P10-I3
(`TechnicalFocusAreas`, `EngineeringPracticePatterns`,
`RepresentativeRepositories`) and the two evidence-layer sections locked by
P10-I4 (`WhyThisConclusion`, `RepositoryDrillDown`, §8.6):

| Section | English (`en`) | Turkish (`tr`) |
|---|---|---|
| `TechnicalFocusAreas` | The visible public evidence did not establish a repeated technical direction under the current analysis rules. | Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden bir teknik yön ortaya koymadı. |
| `EngineeringPracticePatterns` | No recurring working method is visible across the current public work. | Farklı çalışmalarda tekrar eden belirgin bir yöntem görünmüyor. |
| `RepresentativeRepositories` | No starting-point repository was selected from the current visible public evidence. | Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi. |
| `WhyThisConclusion` | The public GitHub work did not provide a recurring conclusion to explain. | Herkese açık GitHub çalışmaları, açıklanacak tekrar eden bir sonuç ortaya koymadı. |
| `RepositoryDrillDown` | The visible public evidence contains no public repository to inspect. | Görünen herkese açık kanıtlar, incelenebilecek herkese açık repository içermiyor. |

Rules:

- Each line describes the ANALYSIS RESULT, never a developer deficit. The
  form "the developer has no ..." is prohibited. This is the presentation
  form of `docs/architecture/public-profile-evidence-report-contract.md`
  §6.1/§6.4: `Supported + empty != Negative Developer Evidence`.
- The `RepresentativeRepositories` line preserves P7 selection semantics
  (report contract §10): it says a starting point was not selected. It must
  never say best, top, strongest, highest-quality, recommended or ranked.
- Each line is rendered by its own section, keyed by `ProfileReportSectionKey`
  and the selected request culture, when that section's `Availability` is
  `Supported` and its `Items` collection is empty.
- Sections render their own empty result independently of the Meaning state.
  When both pattern sections are Supported + empty, Meaning renders its two
  independently locked absence paragraphs and each detail section still states
  its own outcome. This deliberate repetition ensures that a canonical section
  never fails to report its own result.
- No `Supported + empty` copy is defined or rendered for `ProblemAreaPatterns`
  (`Unsupported`) or `RoleEvidenceAlignment` (`Deferred`). Those sections are
  omitted entirely, per
  `docs/architecture/public-profile-evidence-report-contract.md` §6.5.
- For the two sections whose non-empty presentation is a progressive
  disclosure (`WhyThisConclusion` and `RepositoryDrillDown`, §8.6), the
  `Supported`-but-empty case renders as an ordinary semantic section with its
  locked `H2` label and the neutral paragraph above --- never as an empty
  `details` / `summary` control. There is nothing to disclose, so no
  disclosure affordance is offered.

`Limitations` is separately the canonical `Supported` section at report
position 7. Report contract §6.5 prohibits omitting a Supported section solely
because its item collection is empty. The current neutral empty Limitations
card therefore remains rendered. Omitting it for visitor value requires a
separately approved presentation exception or a change to that canonical
boundary: **Empty Limitations Presentation Contract Gap**.

## 8.6 P10-I4 Why + Repository Evidence Presentation Contract

This section is the normative owner of the P10-I4 presentation shape,
interaction mechanism and user-facing copy. It adds no analysis semantics: it
fixes how already-projected typed truth is rendered and how the existing
Repository Proof entry is reached.

### 8.6.1 Experience Position

`WhyThisConclusion` and `RepositoryDrillDown` are the evidence layer. They sit
after the first-read layer locked in §8.4/§8.5 and must never dilute it:

```text
Meaning
    → Visible technical direction
    → Working style
    → Starting point          (first read — quiet, compact action)
    → What is this based on?  (evidence layer — collapsed by default)
    → Repository evidence     (evidence layer — collapsed by default)
```

`RepresentativeRepositories` / Starting point is a compact, secondary first-
read surface. For every canonical representative item, in existing report
order, it renders exactly:

1. the literal canonical repository identity;
2. the pattern relationship line naming the repeated result(s) this repository
   was selected for (§8.6.1b);
3. one short description line, sourced only from GitHub's own provider-returned
   repository Description (§8.6.1a);
4. the existing localized Repository Proof action (`Analyze this repository` /
   `Bu repository'yi analiz et`).

The action reuses the existing repository-analysis form contract: a POST to
the existing `/Index` entry with the canonical `Repository.Url` as
`GitHubTarget`, normal tag-helper anti-forgery behavior and the established
validation flow. It is never an outbound GitHub link, GET/query-string flow,
new endpoint or duplicate analysis implementation. The action stays visually
secondary to Meaning, and repository identities retain safe wrapping. No
score, stars, forks, language percentage, activity/status, confidence, role or
quality field is added.

#### 8.6.1a Starting Point Description (Session 111 STEP 3)

The **Starting Point Description Contract Gap** is closed for the specific
case of GitHub's own repository Description. `RepresentativeRepository` /
`ProfileReport` still carry no Description of their own — that boundary is
unchanged and remains correct: Description is repository identity/context
metadata supplied by GitHub, not evidence, not a pattern, not a
representative-repository score and not a `WhyThisConclusion` fact. It reaches
Web through a separate, parallel, non-evidence path: `RepositoryPresentation
Metadata`, looked up by the same canonical `ProfileRepositoryReference` used
everywhere else, carried from `GitHubProfileSnapshot.RepositoryPresentation
Metadata` through `ProfilePipelineResult.RepositoryPresentationMetadata` to
`ProfileModel.RepositoryPresentationMetadata` — mirroring the Session 111
STEP 2 avatar path and never touching `DeveloperPublicEvidenceProfile` or
`ProfileReport`.

The Description is the exact provider-returned text (trimmed of surrounding
whitespace only): never rewritten, translated, summarized, inferred, or
derived from repository name, language, README, topics or detected patterns.
When GitHub returns no Description (null or blank), Web renders a fixed
neutral fallback — `No short description is provided on GitHub.` / `GitHub'da
kısa bir açıklama bulunmuyor.` — describing metadata absence only, never a
claim about the repository's purpose, quality or documentation. Razor's
default encoding renders the Description as plain text; it is never passed
through `Html.Raw`, never interpreted as Markdown, and never turned into a
link. It is bounded to approximately two visual lines on desktop via CSS
line-clamp — a visual bound only, never a semantic truncation of the
underlying string.

The canonical representative-selection contract selects one repository per
repeated pattern and then groups equal repository references. It explicitly has
no global top-N/top-K and its final repository ordering is deterministic
identity order (Owner-then-Name ordinal), not a first/recommended priority. The
complete grouped set may therefore contain as many repository items as there
are independently selected pattern representatives.

Session 111 STEP 2 authorizes exactly one Web-only presentation bound over
this: at most 3 items are **presented**, taken as the first 3 items of the
existing canonical order via `StartingPointPresentation.Select`. This is a
stable presentation sample, not a ranking, score, "top", "best" or
"recommended" claim — the underlying order is an identity tie-break, not an
importance measure, so the 3 presented items are not asserted to be the
visitor's most significant repositories. The canonical
`RepresentativeRepositories` collection itself is never capped, reordered or
mutated by this bound; every item remains in the full typed report. A true
importance/priority ordering over the canonical selection remains a separate,
unauthorized design decision: **Starting Point Prioritization Contract Gap**
(narrowed to presentation-bound-only by this amendment).

#### 8.6.1b Starting Point Pattern Relationship (Session 136)

The canonical representative-repository rule already selects **one repository
per repeated pattern** and only then groups equal repository references
(`docs/architecture/public-profile-representative-repositories.md` §3, §8).
Until this amendment, Web rendered the grouped result but never the grouping
key, so the selection reached the reader as a bare repository suggestion. This
subsection makes that existing relationship visible. It is a presentation
amendment only: no selection semantics, candidate rule, tie-break, ordering or
evidence rule changes, and none is restated here.

**CONSUMED, NOT OWNED.** The selection rule stays owned by
`public-profile-representative-repositories.md`; this document renders its
result.

**Source.** The relationship is read only from the already-projected
`RepresentativeRepository.PatternSupports` collection that the Starting Point
partial already receives — specifically each support's `PatternFamily` and
`PatternKey`, in that collection's existing canonical order. No new field,
lookup, pipeline path or GitHub request is introduced.
`RepresentativePatternSupport.SupportingEvidenceReferences` is **not** read
here: evidence references belong to the evidence layer, never to first read.

**Labels.** Each represented pattern renders through its existing localized
label with no new mapping: `TechnicalFocus` uses its literal language identity
(§8.4.5's literal-identity rule) and `EngineeringPractice` uses the concise
§8.4.8 Working Style label — the same labels the two first-read pattern cards
and the §8.6.3 Why questions already show. An unknown canonical identity is a
presentation contract violation, never a fallback label.

Those two families are the whole authorized scope. `ProfilePatternFamily`
carries a third value, `ProblemArea`, which has no active Web presentation
mapping because `ProblemAreaPatterns` is `Unsupported` in V1
(`docs/architecture/public-profile-evidence-patterns.md` §9, product contract
§24). A `ProblemArea` support reaching this surface is a contract violation
that must fail loudly; no label, placeholder or silent omission may be invented
for it here, and this subsection does not authorize rendering that family.

**Completeness.** Every entry in that item's `PatternSupports` is rendered, in
its existing order, joined by the §8.4.5 locale-aware join. There is no cap,
reorder, filter or "first pattern only" shortcut inside an item. The §8.6.1a
max-3 bound applies to presented **repositories** and is untouched by this
subsection.

**Locked EN/TR copy.** Selection is keyed only by whether the item represents
one or more than one pattern, plus the selected request culture:

| Condition | English (`en`) | Turkish (`tr`) |
|---|---|---|
| one represented pattern (`{0}` = its localized label) | Starting point for this recurring result: {0} | Bu tekrar eden sonuç için başlangıç noktası: {0} |
| more than one represented pattern (`{0}` = joined localized labels) | Starting point for these recurring results: {0} | Bu tekrar eden sonuçlar için başlangıç noktası: {0} |

This copy lives in its own narrow, explicitly named
`ProfileWebPresentationLocalizer` method under the §8.6.7 keying rule. No new
analysis, Domain or Application semantic identity, enum or value is created to
key it.

The singular/plural pair is **grammatical agreement, not a count**. It renders
no number, and the pattern labels it names are identities that the first-read
pattern cards already render. The broad first-read no-count constraint
(`profile-proof-distinctive-outcomes-contract.md` §8.2/§8.3) is therefore
satisfied, and this line remains subject to it: neither the supporting-
repository count nor any other counter may be added to it.

**Prohibitions.** This line must never be rendered or reworded as best, top,
strongest, highest-quality, most important, most professional, recommended,
featured, primary or curated; never imply an order of importance among the
presented repositories or among the patterns inside one item; never carry a
count, score, percentage, star, fork, language share, activity or confidence
field; never expose promotion or selection mechanics, thresholds, rule/policy
versions, evidence keys or observation keys; and never be turned into a claim
about the person. The **Starting Point Prioritization Contract Gap** (§8.6.1a)
remains open: naming which repeated result a repository was selected for is a
relationship, not a priority ordering, and does not authorize closing that gap.

### 8.6.2 Progressive Disclosure Shape

When `Supported` and non-empty, both sections render as a **collapsed native
`details` / `summary` disclosure**, closed by default, with the section's
existing locked label as an `h2` inside the `summary` (the convention already
used by the Repository Proof signal section). No custom disclosure widget, no
ARIA expansion state of its own, and no JavaScript.

When `Supported` and empty, neither section renders a disclosure control:
§8.5 governs, and the section renders as an ordinary `h2` section with its
locked neutral paragraph.

### 8.6.3 What One Why Entry Shows

`WhyThisConclusion` items are, by projection, exactly the same promoted
patterns already rendered by `TechnicalFocusAreas` and
`EngineeringPracticePatterns`, in the same canonical order. The Why layer adds
the evidence trace beneath them, never a new claim.

One Why entry shows exactly three things:

1. a visitor-facing question formed from the canonical-keyed visitor label:
   TechnicalFocus uses its literal language identity and EngineeringPractice
   uses the concise §8.4.8 Working Style label; the format is
   `Why is {label} here?` / `{label} neden burada?`;
2. one deterministic family-bounded explanation: a TechnicalFocus item appears
   because it is visible again across different public repositories and is
   therefore shown as a recurring technical direction; an EngineeringPractice
   item appears for the same canonical repetition reason and is therefore shown
   as a recurring way of working;
3. the pattern's supporting public repositories, rendered as literal
   repository full names in the canonical order the pattern already provides,
   under `Supporting repositories` / `Destekleyen repository'ler`.

The question and reason translate the existing `WhyThisConclusion` trace into
normal visitor language. They add no evidence relationship, count, promotion
decision or inference. The canonical family and pattern key select deterministic
copy; arbitrary display text is never interpreted.

The supporting set rendered is the pattern's **supporting repositories** ---
the set named by
`docs/architecture/public-profile-evidence-report-contract.md` §11's own
trace (`Repeated pattern → Supporting repositories → ...`). Rendering only
the independent subset would silently hide repositories in which the evidence
was genuinely observed.

A Why entry must never render: any count (including the independent
supporting repository count), rule or observation versions, observation keys,
analysis identifiers, selection or promotion mechanics, score, ranking, or
any capability/seniority/role/employability language.

### 8.6.4 Evidence Key Boundary

`ProfileEvidenceReference.EvidenceKey` and
`ProfileEvidenceReference.EvidenceKeys` are **never rendered anywhere in the
Profile Web surface**.

The division of responsibility is:

```text
What is this based on? : conclusion → human reason → supporting repository identities
Repository evidence  : repository → a fresh existing Repository Proof run
Repository Proof     : signal / concrete evidence inspection
```

Report contract §11 requires the evidence **chain** to be visible, not the
implementation; report contract §12 closes the chain with references
sufficient to *navigate* to repository-level truth, which §8.6.5 provides. No
raw analyzer vocabulary --- canonical signal keys, normalized snapshot data
keys or any other evidence identity --- is introduced into the Profile
surface, and no EN/TR mapping for such keys exists or may be invented here.

### 8.6.5 Repository Evidence Coverage and Interaction

**Coverage.** Every `RepositoryDrillDownReportItem` is rendered, in canonical
projector order. The list is never filtered to `WhyThisConclusion`
participants: `RepositoryDrillDown` is a `Supported` section (report contract
§6.5 permits omitting only `Unsupported` / `Deferred` sections), and the
locked fork decision requires a known fork to remain visible in
`RepositoryDrillDown` even though it contributes no evidence. P10-I4
introduces no sorting, ranking, grouping, search, count or pagination over
this list.

**Interaction.** Each row carries exactly one explicit user action, which
submits the repository to the existing unified entry:

```text
<form method="post" asp-page="/Index" data-analysis-form>
    hidden input name="GitHubTarget" value = Repository.Url
    submit button
</form>
```

Binding rules:

- `Repository.Url` is the **posted `GitHubTarget` value**. It is NEVER used as
  the drill-down `href`. An anchor to `Repository.Url` would navigate to
  github.com instead of Repository Proof and is prohibited.
- Exactly one explicit user action starts the analysis. There is no
  GET-triggered analysis, no query-string analysis trigger, no prefill
  navigation and no JavaScript auto-analysis.
- Normal Razor Pages anti-forgery applies: the form is authored with the page
  tag helper so the request-verification token is emitted and validated by the
  framework's own POST protection.
- The existing `IndexModel` repository POST semantics are reused unchanged ---
  classification, canonical parsing, live analysis, failure vocabulary and the
  Repository Proof micro-view are not modified.
- Because the product holds no result cache or persistence, the action always
  performs a **fresh** Repository Proof analysis. The locked lead-in line of
  §8.6.7 states this truthfully to the reader.
- `RepositoryAnalysisReference` never controls the action's target, label or
  existence (§8.6.6).
- The existing analysis-progress script may be reused through the existing
  `data-analysis-form` / `data-analysis-submit` hooks for duplicate-submit
  prevention. That script is not modified by this section — Session 138 does
  authorize Session 139 to modify it, but only for the landing form: the
  drill-down forms emit no correlation token, register no run and keep exactly
  the behavior described here. No per-row visible processing
  region is introduced by P10-I4.

### 8.6.6 Row Status Mapping

Presentation reads `RepositoryEligibilityStatus` **only**. It never reads
`RepositoryAnalysisReference`.

This is not a stylistic preference. `Contributing` and `Restricted` both
require at least one allowed evidence-bearing contribution, and every allowed
evidence-bearing contribution requires an analysis reference, so for those two
statuses the reference is always present and can distinguish nothing. For the
remaining two statuses the reference would expose only whether a repository
analysis happened to run --- internal execution mechanics, which report
contract §11 does not require presentation to surface.

The mapping is exhaustive over the enum; there is no fifth state:

| `RepositoryEligibilityStatus` | English (`en`) | Turkish (`tr`) |
|---|---|---|
| `Contributing` | Contributed evidence to this view | Bu görünüme kanıt sağladı |
| `Restricted` | Contributed evidence to this view with restrictions | Bu görünüme kısıtlamalarla kanıt sağladı |
| `NonContributing` | Did not contribute evidence to this view | Bu görünüme kanıt sağlamadı |
| `Undetermined` | Its contribution to this view could not be determined | Bu görünüme katkısı belirlenemedi |

`Restricted` is **not** an evidence-quantity statement. It means at least one
evidence-bearing purpose is allowed and at least one applicable
evidence-bearing purpose is not allowed, with no undetermined purpose. Copy
implying "less evidence" is therefore prohibited for this status.

Every line above describes only that repository's contribution to **this
analysis**. None of them is a statement about repository quality, developer
capability, seniority, employability or ranking, and none may be rewritten
into one. The `Warnings` / `Limitations` / partial / failure presentation
remains owned by P10-I5 and is not surfaced through these labels.

### 8.6.7 Locked EN/TR Copy

The active `WhyThisConclusion` section label is "What is this based on?" /
"Bunu neye dayanarak söylüyoruz?"; it supersedes the previous system-sounding
label. The
`RepositoryDrillDown` stays "Repository evidence" / "Repository kanıtı".

| Identity | English (`en`) | Turkish (`tr`) |
|---|---|---|
| Why — entry question (`{0}` = localized canonical label) | Why is {0} here? | {0} neden burada? |
| Why — TechnicalFocus reason (`{0}` = localized canonical label) | {0} appears again across different public repositories, so it is shown as one of the profile's recurring technical directions. | {0} farklı herkese açık repository'lerde yeniden görüldüğü için profilin tekrar eden teknik yönlerinden biri olarak gösteriliyor. |
| Why — EngineeringPractice reason (`{0}` = localized canonical label) | {0} appears again across different public repositories, so it is shown as one of the recurring ways of working. | {0} farklı herkese açık repository'lerde yeniden görüldüğü için çalışma biçiminde tekrar eden yöntemlerden biri olarak gösteriliyor. |
| Why — supporting repositories lead-in | Supporting repositories | Destekleyen repository'ler |
| Repository evidence --- fresh-analysis lead-in | Selecting a repository runs a fresh Repository Proof analysis. | Bir repository seçmek yeni bir Repository Proof analizi başlatır. |
| Repository action --- visible label | Analyze this repository | Bu repository'yi analiz et |
| Repository action --- accessible name (`{0}` = repository full name) | Analyze repository {0} | {0} repository'sini analiz et |

Row status copy is locked in §8.6.6. `Supported`-but-empty copy for both
sections is locked in §8.5.

The Why reason says that the item appears again **across different public
repositories**. It never asserts that evidence repeats inside any single
repository and never exposes the promotion mechanism.

Literal identities are never translated: repository full names, GitHub
usernames, URLs, and programming/GitHub language identities.

All of this copy is resolved by the Web-owned Profile presentation mapping in
`ProfileWebPresentationLocalizer`, which remains its sole owner. The keying
rule is precise per kind of string, because one section can own more than one
fixed presentation slot:

- **Section-dependent copy** --- section labels and the `Supported`-but-empty
  paragraphs of §8.5 --- is keyed by `ProfileReportSectionKey` plus the
  selected request culture.
- **Row-status copy** (§8.6.6) is keyed by `RepositoryEligibilityStatus` plus
  the selected request culture.
- **Pattern labels** keep their existing canonical mapping by pattern family
  and pattern key; nothing about that mapping changes here.
- **Fixed P10-I4 presentation-slot copy** --- the Why question, family-bounded
  reason and supporting-repositories lead-in, the Repository-evidence fresh-
  analysis lead-in, the visible action label and the accessible-name format ---
  has no new canonical identity of its own. Each lives in its own narrow,
  explicitly named `ProfileWebPresentationLocalizer` method. Question/reason
  selection uses the existing family/key; the remaining fixed slots vary only
  by selected request culture.

Two rules bind all four cases: presentation text is never used as a lookup
key, and no new analysis, Domain or Application semantic identity, enum or
value is created merely to key a fixed presentation string. This keeps §8.2
and §8.4.5 satisfied without inventing taxonomy that canonical truth does not
own.

### 8.6.8 Accessibility and Information Architecture

- The page keeps exactly one `h1`. Each report section contributes one `h2`,
  including the two disclosure sections (their `h2` lives inside `summary`).
- A Why entry's visitor question is an `h3`, followed by its bounded reason and
  supporting-repository list.
- Item collections render as `ul` / `li`. Empty results render as a paragraph.
  No empty list container is rendered.
- Disclosure is native `details` / `summary`; no ARIA is added where native
  semantics already suffice.
- Each repository row exposes its identity and status as text plus exactly one
  native submit button. Because many rows share the same visible action label,
  the button carries the locked accessible-name format of §8.6.7 with the
  repository full name substituted.
- Keyboard behavior is entirely native: `summary` toggles, buttons submit, the
  existing focus-visible treatment applies. No focus management, roving
  tabindex or scroll hijacking is introduced.
- Rows stack vertically and must not introduce horizontal overflow at mobile
  widths.
- P10-I6 owns visual refinement; P10-I4 introduces no new design token and no
  new component system.

### 8.6.9 Prohibitions

The P10-I4 surface must never:

- render `EvidenceKey`, `EvidenceKeys` or any other raw analyzer/evidence
  identity (§8.6.4);
- use `Repository.Url` as an `href`, or reach analysis through GET, a query
  string or JavaScript (§8.6.5);
- read `RepositoryAnalysisReference` for any presentation decision (§8.6.6);
- render any count, percentage, ratio, badge, counter or progress figure;
- sort, rank, group, filter, search or paginate the repository list;
- embed, duplicate or summarize `RepositoryAnalysis` content on the Profile
  page (report contract §12);
- render `ProblemAreaPatterns` or `RoleEvidenceAlignment`;
- surface `Warnings`, `Limitations`, partial or failure presentation ---
  P10-I5 owns those;
- express repository quality, developer capability, confidence, seniority,
  role, employability or ranking;
- present `Supported + empty` as a negative developer finding;
- introduce free-form or runtime-composed prose, or use an LLM for any part of
  this surface.

## 8.7 P10-I6-A Profile Identity Header and Shareability Boundary

### 8.7.1 Purpose and Active Header Shape

The successful Profile page begins with the public GitHub identity, not an
analysis-dashboard taxonomy label. The former visible `Profile view` / `Profil
görünümü` eyebrow is not rendered and is not replaced with another eyebrow.

The active compact header contains, in priority order:

1. the literal canonical `DeveloperPublicEvidenceProfile.ProfileIdentity.Login`
   as the page's single `h1`;
2. the visible canonical GitHub identity `github.com/{login}`, projected from
   `DeveloperPublicEvidenceProfile.ProfileIdentity.Url` rather than reconstructed
   from the posted string;
3. `View of public GitHub work` / `Herkese açık GitHub çalışmalarının görünümü`;
4. the secondary GitHub external action of §8.7.3.

The header contains no technology/practice item, repository count, score,
percentage, metric tile, activity classification or report summary. Meaning
remains the first report-content surface immediately below the header and any
truthful P10-I5 status notice. The header aligns to the existing centered
`64rem` Profile surface; actions may wrap below identity on narrow viewports and
must not create horizontal overflow.

### 8.7.2 Optional Identity Data and Current Gaps

An avatar renders only from an authoritative typed Profile field. Session 111
STEP 2 closes the **Profile Avatar Contract Gap** for collection and
presentation: `GitHubProfileMetadataCollector` now reads GitHub's own
`avatar_url` from the same successful profile-metadata response used for
`login`/`name`/`public_repos`, and `GitHubProfileMetadata.AvatarUrl` requires
it to be an absolute HTTPS URI (rejecting `http`, `javascript:`, `data:` or any
other scheme as a Failed collection, exactly like the existing required-field
philosophy for `created_at`/`public_repos`). The value travels unchanged from
that same run's `GitHubProfileSnapshot.Metadata` through
`ProfilePipelineResult.AvatarUrl` to `ProfileModel.AvatarUrl` — never through
`DeveloperPublicEvidenceProfile` or `ProfileReport`, which remain evidence-
report truth and carry no avatar field of their own. The header renders this
image when present, with accessible alt text naming the profile, and falls
back to the existing decorative `aria-hidden` GitHub mark only when a typed
avatar is legitimately unavailable (e.g. metadata was not carried into this
particular snapshot). No avatar is ever derived from the username, generated as
an initial/identicon, or fetched separately by Web.

A recency row may render only from a future authoritative profile-level fact
whose meaning is the most recent observable public change/activity. Current
typed Profile truth has no such fact. `GitHubProfileSnapshot.CapturedAt` and
`DataAvailability.RetrievedAt` are observation/runtime times;
`GitHubProfileMetadata.CreatedAt` is account creation; repository-level
`LastPushedAt` and `ActivityClassification` do not authorize profile-level
recency. Web must not scan commits/repositories, aggregate arbitrary timestamps,
infer cadence, or say active/inactive/still coding. The row is therefore absent:
**RECENCY CONTRACT GAP**.

### 8.7.3 Canonical GitHub External Action

The successful header exposes exactly one lightweight secondary action:
`Open on GitHub` / `GitHub'da aç`. Its `href` is the exact canonical
`DeveloperPublicEvidenceProfile.ProfileIdentity.Url`; it is never assembled
from arbitrary display text. It opens in a new tab with `noopener`, `noreferrer`
and external-link semantics, a visible external-link icon and localized
accessible copy announcing the new tab. No GitHub fetch or analysis is started.

### 8.7.4 Shareable URL and Social Preview Gaps

The current Profile result is produced only by `POST /profile`, exists only in
that response and is neither persisted nor cached. `GET /profile` redirects to
the unified entry and performs no analysis. Consequently, the current browser
URL cannot reproduce the result for a third party and must never be copied or
sent to LinkedIn as though it were shareable: **SHAREABLE PROFILE URL CONTRACT
GAP**.

No stable public Profile URL means the application also cannot emit truthful
profile-specific Open Graph title, description and canonical URL for a crawler.
The shared layout contains no dynamic Profile social metadata. Generic static
claims would misrepresent the person and are prohibited: **SOCIAL PREVIEW
CONTRACT GAP**.

Until both boundaries are separately designed and authorized, no Share button,
menu/popover, LinkedIn action or Copy link action is rendered. There is no
disabled/dead Share control and no current-URL clipboard fallback. A future
Share action, once supportable, remains secondary to identity and must be
keyboard-operable, localized and dismissible without a framework or external
package.

### 8.7.5 Future Share Content Truth Boundary

Share content must introduce that specific Profile, never advertise GitHub
Proof or say that the user analyzed/checked a profile. Current canonical truth
could authorize only the literal username plus ordered
`TechnicalFocusAreas.Items` and `EngineeringPracticePatterns.Items`; it cannot
authorize project/application types or recency. Unknown facts are omitted, not
replaced by attractive generic prose. No exact share copy is emitted or locked
while the shareable-route/social-preview gaps remain open.

### 8.7.6 Approved Mockup First-Read Visual Boundary

The approved dark Profile mockup is the primary visual target for the successful
Profile surface. Its binding visual sequence is: back link, public identity
header with its safe GitHub action, a two-column first-read composition (Meaning
plus the two paired pattern cards on the left; Starting Point on the right),
then the full-width collapsed Why and Repository Evidence disclosures. The
surface uses calm, dark raised cards, restrained blue emphasis, rounded borders,
icon-led headings and responsive collapse without changing canonical DOM order.

The header renders the mockup's avatar position as the real provider-returned
GitHub avatar when `ProfileModel.AvatarUrl` is available (§8.7.2), and as the
existing `aria-hidden` decorative GitHub mark only when it is legitimately
unavailable. Neither branch is a generated initial, guessed image or Web-side
fetch. The header surface is widened to the existing global content measure;
its visual grouping must not be interpreted as a new report field.

The mockup's visible work-type card title is `What kinds of work are visible?`
/ `Ne tür çalışmalar görünüyor?`. This is a visitor-facing visual heading, not
a project/application-type projection. Until the Project-Type Presentation
Pending Canonical Contract is closed, the card may render only the existing
ordered `TechnicalFocusAreas` language labels, with the explicit quiet lead-in
`Recurring technologies` / `Tekrar eden teknolojiler`. It must not render,
derive or name API, authentication, realtime, education, backend, frontend,
mobile, AI or any other project type.

Starting Point visually uses compact repository cards and the existing
Repository Proof POST action. Session 111 STEP 2 authorizes presenting at
most 3 of them (§8.6.1's presentation bound) with a bounded desktop rail height
and an internal vertical scroll on the repository list, returning to natural
flow at the single-column breakpoint. This is a Web-only presentation bound
over the existing deterministic order, never a ranking or "curated" selection.
The canonical `RepresentativeRepositories` collection itself still contains
every item, uncapped and unreordered. The narrowed Starting Point
Prioritization Contract Gap remains active. Each presented item's short
description is now the real GitHub-provided repository Description (§8.6.1a),
never a name-derived or fetched substitute, visually bounded to ~2 lines
without semantic truncation. Each presented item also carries the §8.6.1b
pattern relationship line naming the repeated result(s) it was selected for;
it is quiet supporting text on the card, never a badge, tag cloud, count or
emphasis treatment, and it never reads as a priority or quality marker.

The non-empty Why and Repository Evidence native disclosures remain closed by
default. Their summary may contain a fixed localized visitor-facing one-line
description and a visual chevron, but no raw evidence term, count, status,
selection mechanism, custom ARIA state or JavaScript. Opening behavior and
their canonical body content are unchanged.

The mockup does not show Limitations, but its omission is not authorized here.
Limitations remains a canonical Supported section in its existing report order,
including when empty, under the Empty Limitations Presentation Contract Gap.
The mockup's Share action likewise remains absent under the Shareable Profile
URL Contract Gap; no disabled/dead control is a substitute for a real share
flow.

## 8.8 Profile First-Read Information Depth Contract (Session 136)

### 8.8.1 Purpose and Ownership

This section is the normative owner of one question only:

> When a visitor sees a successful Profile Proof result for the first time,
> which information must be perceptible before any scroll, disclosure or
> repository action — and which information must deliberately wait?

It adds no analysis semantics and creates no second owner:

```text
public-profile-evidence-product-contract.md §25
    owns WHICH QUESTIONS the first layer must let a reader answer

profile-proof-distinctive-outcomes-contract.md §8
    owns WHICH DISTINCTIVE VALUE must be perceptible inside that frame,
    and the broad first-read no-count constraint

this section (§8.8)
    owns AT WHAT DEPTH each kind of information is presented,
    and what the boundary cases do in first read

§8.4 / §8.5 / §8.6 / §8.7
    own the concrete surfaces, section behavior and exact copy
```

Neither §25's question set nor §8's outcome priority is restated, narrowed or
overridden here. Where this section and an existing §8.4–§8.7 surface rule
disagree, the surface rule wins and the disagreement is a defect in this
section.

### 8.8.2 First-Read Goal

The first read must make an invisible relationship visible. A visitor who has
already seen this GitHub account on github.com must learn something the
account's own surface does not show: that specific observable traces **recur
across separate repositories**, that this is what the result is built from,
that the reasoning can be checked, and that the statement is about public
evidence rather than about the person. Density is not the goal; a visitor who
understands less but understands it correctly has had a better first read than
one who is shown everything.

**Required understanding — at most five, in this order of necessity:**

1. This GitHub shows things that repeat across different repositories, and
   here is what repeats.
2. This is not a reading of one repository.
3. This describes public evidence under stated rules; it is not a judgment of
   the person.
4. I can find out why the product says this.
5. If I want to look further myself, there is a neutral place to start.

The order is `profile-proof-distinctive-outcomes-contract.md` §8.1's locked
prominence order (its items 1–5), carried over unchanged rather than
re-derived.

This list is **derived, not original**: it is the presentation-depth reading of
the question set owned by `public-profile-evidence-product-contract.md` §25 and
the outcome-level first-read priority owned by
`profile-proof-distinctive-outcomes-contract.md` §8.1. It creates no new
first-read requirement. If either owner changes, that owner wins and this list
is restated to follow it — a divergence here is a defect in this section, never
a competing requirement.

Nothing else is required in first read. Anything else that appears there must
earn its place against these five.

### 8.8.3 Semantic Levels

The first read has five semantic levels. A level is a **depth of meaning**, not
a DOM position and not a visual coordinate:

```text
L1  MEANING            what this GitHub collectively shows
L2  RECURRING RESULTS  which technical direction and which working methods repeat
L3  WHY AFFORDANCE     that the reasoning is inspectable
L4  STARTING POINT     a neutral, pattern-scoped place to begin
L5  EVIDENCE BOUNDARY  what was looked at and what limits the reading
```

Two rules bind this ladder:

- **Level is not position.** The canonical section order stays exactly as
  §8.6.1 and the canonical report order
  (`docs/architecture/public-profile-evidence-report-contract.md` §7) lock it,
  which places Starting Point before the Why disclosure. That is not a
  conflict: L3 is the *affordance* — the visible fact that a why path exists —
  and the affordance may sit after L4 in reading order while still outranking
  it in semantic weight. The why *content* is not first read at all (§8.8.4).
- **L1 and L2 may not be outranked.** No other level may take visual or
  semantic priority over Meaning and the two recurring-result cards, and no new
  surface may be introduced that does. This restates, at presentation depth,
  the priority already owned by
  `profile-proof-distinctive-outcomes-contract.md` §8.1.

The ladder ranks **report content**, so exactly two existing non-report
surfaces sit outside it and are not L1/L2 competitors: the identity header
(§8.7.1) and the P10-I5 status area. Their position is a truthfulness
requirement, not a priority claim — a result the reader must know is partial
has to say so before the reader starts reading it, and the page's `h1` is the
identity the result is about. Within that, the Partial notice is the single
status surface that may precede Meaning; warnings remain subordinate under
§8.8.4 and never take that position. Nothing else may be added to this
exception.

### 8.8.4 Information Depth Classification

Exactly one depth per information type. `FIRST READ` = perceptible with no
interaction; `PROGRESSIVE DISCLOSURE` = one deliberate expansion on this page;
`DEEP EVIDENCE` = reached by leaving this page for a Repository Proof run.

| Information | Depth | Binding surface rule |
|---|---|---|
| Accepted profile identity, canonical GitHub identity, scope line | FIRST READ | §8.7.1 |
| Profile meaning narrative (L1) | FIRST READ | §8.4.9 |
| Recurring technical direction — labels (L2) | FIRST READ | §8.4.5 literal-identity rule, §8.7.6 |
| Recurring working methods — labels (L2) | FIRST READ | §8.4.8 |
| The recurrence relationship itself ("appears again across different repositories") | FIRST READ | §8.4.9, §8.7.6 lead-in |
| Supporting repository identities | PROGRESSIVE DISCLOSURE | §8.6.3, §8.4.7 |
| Supporting repository count, or any count | NEVER RENDERED | §8.6.9, §8.4.7, distinctive-outcomes §8.2 |
| "Why" affordance — that an explanation exists (L3) | FIRST READ | §8.6.2 summary and its one-line description |
| "Why" explanation content | PROGRESSIVE DISCLOSURE | §8.6.2, §8.6.3 |
| Starting-point repository identity (L4) | FIRST READ | §8.6.1 |
| Starting-point pattern relationship | FIRST READ | §8.6.1b |
| Starting-point GitHub description | FIRST READ, subordinate | §8.6.1a |
| Repository eligibility status rows | PROGRESSIVE DISCLOSURE | §8.6.5, §8.6.6 |
| That a declared repository surface exists and is inspectable (L5) | FIRST READ | §8.6.2 summary and its one-line description |
| Limitations (L5) | FIRST READ, subordinate | §8.5, report contract §7 position 7 |
| Warnings — operational notes | FIRST READ, subordinate | P10-I5; plan §13.13 |
| Partial-result notice | FIRST READ, leading the status area | P10-I5 |
| Signal / concrete file / workflow / release evidence | DEEP EVIDENCE | §8.6.4, §8.6.5 |
| Evidence keys, observation keys, rule/policy versions, analysis identifiers | NEVER RENDERED | §8.6.4, §8.6.9 |
| Score, ranking, percentage, badge, activity feed, language share, repository totals | NEVER RENDERED | §8.4.7, §8.6.9, product contract §15/§18 |

"Subordinate" means present in first read but never competing with L1/L2 for
attention: quieter heading weight, no emphasis treatment, and never placed
between Meaning and its own two recurring-result cards.

### 8.8.5 Recurrence Without Counts

Recurrence is the product's central first-read claim and it must be perceived
**as a relationship expressed in language**, never as a quantity. The only
authorized carriers are:

- the Meaning paragraphs, which state that the named items recur across
  different repositories (§8.4.9);
- the quiet `Recurring technologies` / `Tekrar eden teknolojiler` lead-in on
  the technical card (§8.7.6);
- the §8.6.1b starting-point relationship line, which names repeated results;
- the §8.6.7 Why reasons, once disclosed.

A number is never a substitute for any of these, and none of them may be
strengthened into a magnitude, frequency, ratio, "strongly/weakly recurring"
axis or a comparison between items.

**Recorded gap — Working Style recurrence asymmetry.** The technical card
carries a recurrence lead-in; the Working Style card (§8.4.8) renders bare
noun-phrase rows, so on that card the recurrence relationship reaches the
reader only through Meaning. Meaning is always rendered, so this is not a
correctness defect and Outcome 1 remains supported. Giving the practice card a
symmetric quiet lead-in would change §8.4.8's locked card shape and is
therefore **not authorized here**: **Working Style Recurrence Lead-In Contract
Gap**.

### 8.8.6 Boundary Behavior in First Read

Four cases, kept structurally distinct. Collapsing any two of them is a
contract violation.

**CASE A — repeated patterns exist.** Meaning leads with what recurs; the two
recurring-result cards follow; Starting Point carries its §8.6.1b relationship;
the Why and Repository-evidence affordances are visible and closed.

**CASE B — no promoted pattern, evidence surface otherwise valid.** This is a
valid analytical outcome, not a negative result and not a limitation. First
read leads with the locked Meaning absence paragraphs (§8.4.4/§8.4.9), each
section states its own §8.5 `Supported + empty` outcome, and `Limitations` may
legitimately be empty. Nothing on the page may read as "nothing found", "no
skills", a deficit, an error state or an invitation to try a better profile. No
empty-state illustration, retry prompt or encouragement copy is introduced.

**Recorded gap — empty-result first-read repetition.** In this case the reader
receives the same absence statement several times in a row (Meaning's two
paragraphs, then each section's own §8.5 line, then an empty Limitations card).
§8.5 calls this repetition deliberate so that a canonical section never fails
to report its own result, and the empty Limitations card is held open as the
**Empty Limitations Presentation Contract Gap**. Reducing the repetition would
require amending §8.5 and is **not authorized here**: **Empty-Result First-Read
Repetition Contract Gap**.

**CASE C — evidence surface incomplete or narrowed.** Distinct from CASE B and
never merged into it. The truthful Partial notice leads the status area above
the report body, and the canonical `Limitations` items carry the narrowing.
This says the *view* is bounded; it never says the person's work is bounded.

**CASE D — provider / system / operational warning.** Warnings stay operational
notes about this run and are never converted into evidence, negative evidence,
a missing-capability claim or profile truth. They remain subordinate in first
read (§8.8.4) and are never presented as a numbered report section (plan
§13.13). A warning never suppresses, qualifies or annotates an individual
pattern, conclusion or repository row.

### 8.8.7 Generic-AI Differentiation Requirement

Every element admitted to first read must survive this test, and an element
that cannot answer the fourth column does not belong there:

| First-read element | A generic AI can | GitHub Proof adds | The visitor perceives it because |
|---|---|---|---|
| Meaning narrative | produce a fluent summary of the profile | only items promoted by a versioned repetition rule ever appear | every profile-level sentence is phrased as recurrence across different repositories, never as a one-off observation |
| Recurring-result cards | list technologies and practices | a bounded set whose membership condition is repetition, not salience | the cards carry only promoted items, under a recurrence lead-in and recurrence-worded Meaning |
| Why affordance | invent a plausible-sounding justification | the justification is the same production input that produced the result | a visible, permanent path exists before it is opened, and opening it names literal supporting repositories |
| Starting point | recommend a repository by popularity or name recognition | a pattern-scoped selection computed only from that pattern's own evidence | §8.6.1b names the repeated result the repository was selected for |
| Evidence boundary | add a cautious hedge sentence | an auditable declared surface in which non-contributing repositories still appear | the always-present scope sentence in first read, and an inspectable unfiltered repository list behind one disclosure |

### 8.8.8 Anti-Goals

The Profile first read must never be organized as, or drift toward, any of:
dashboard-first, metric-first, score-first, repository-list-first,
AI-summary-first, activity-first, language-percentage-first, or
person-judgment-first.

"Show everything above the fold" is explicitly rejected as a goal. Adding a
surface to first read is a cost paid by L1 and L2, and must be justified
against §8.8.2's five required understandings.

### 8.8.9 Implementation Input Boundary

A future Razor/CSS/Web session working from this section **MAY**:

- render the §8.6.1b starting-point pattern relationship from
  `RepresentativeRepository.PatternSupports` using the locked copy;
- adjust semantic and visual prominence inside the depths of §8.8.4 — in
  particular, make the Why and Repository-evidence affordances more clearly
  perceptible in first read, and keep subordinate surfaces subordinate;
- remove presentation redundancy that renders the same canonical information
  twice on one surface;
- refine heading weight, grouping and spacing within the §8.7.6 visual
  sequence, which is unchanged by this section.

It **MAY NOT**:

- invent evidence, counts, ranking, priority ordering, scores, confidence or
  magnitude;
- infer developer quality, role, seniority, capability, employability or
  authorship;
- add a fifth distinctive outcome, evidence convergence, role alignment, CV
  alignment, candidate scoring, HR ranking or skill verification;
- expose analyzer internals (evidence/observation keys, rule or policy
  versions, thresholds, promotion or selection mechanics, analysis identifiers);
- collapse `Supported + empty`, `Unsupported`, `Deferred`, a limitation and an
  operational warning into one another;
- reorder canonical report sections, or cap, filter, sort or group a canonical
  collection beyond the bounds already locked in §8.6.1a;
- close, narrow or work around any gap this document holds open — including
  the Starting Point Prioritization, Working Style Recurrence Lead-In,
  Empty-Result First-Read Repetition, Empty Limitations, Project-Type
  Presentation, Recency, Shareable Profile URL and Social Preview gaps.

Anything not authorized above requires an amendment to the owning contract
before implementation, not an implementation decision.

------------------------------------------------------------------------

# 9. Presentation Rules

## Meaning Before Mechanics

Do not expose internal processing concepts first.

Avoid:

-   signal counts,
-   internal policies,
-   scoring details.

Prefer:

-   visible direction,
-   recurring traces,
-   supporting evidence.

------------------------------------------------------------------------

## Observation Before Judgment

The product describes observable patterns.

It does not make personal conclusions.

------------------------------------------------------------------------

## Evidence Is Available, Not Forced

The first experience should be simple.

The user should see:

> Bu GitHub ne anlatıyor?

before:

> Nasıl belirlendi?

------------------------------------------------------------------------

# 10. Landing Copy Principles

## Hero

EN:

> See what your GitHub communicates.

TR:

> GitHub'ınızda ne olduğunu değil, ne anlattığını görün.

## Supporting Message

EN:

> Discover the bigger picture formed by the visible traces in your GitHub profile.

TR:

> GitHub profilinizde bıraktığınız izlerden oluşan büyük resmi keşfedin.

Avoid leading with:

-   analysis,
-   score,
-   ranking,
-   evaluation,
-   developer level.

------------------------------------------------------------------------

# 11. Result Presentation Principles

The result experience should answer the same sequence in the selected culture:

First — EN / TR:

> What does this GitHub communicate? / Bu GitHub ne anlatıyor?

Then — EN / TR:

> Visible technical direction / Görünen teknik yön

Then — EN / TR:

> Working style / Çalışma biçimi

Then — EN / TR:

> Starting point / Başlangıç noktası

Finally — EN / TR:

> What is this based on? / Bunu neye dayanarak söylüyoruz?

The exact deterministic synthesis and copy of that first reading layer are
locked in §8.4; the `Supported`-but-empty copy of its sections is locked in
§8.5. The sequence above matches §6's Experience Principle (Meaning →
Direction → Starting Point → Evidence) and the canonical report order
(`docs/architecture/public-profile-evidence-report-contract.md` §7, positions
1 → 2 → 3 → 5, then 6). No label text was changed.

------------------------------------------------------------------------

# 12. Evidence Presentation Model

Evidence presentation follows:

    Source
    ↓
    Observation
    ↓
    Meaning

The user should be able to move from interpretation to observable
source.

------------------------------------------------------------------------

# 13. Agent / Development Guidance

When creating user-facing experiences:

-   preserve evidence boundaries,
-   avoid judgmental language,
-   translate internal concepts into user meaning,
-   show meaning before technical detail.

Never create UI language implying:

-   developer ranking,
-   skill level,
-   hiring decisions,
-   AI authorship conclusions.

------------------------------------------------------------------------

# 14. Validation Questions

Before accepting a presentation change:

1.  Does the user understand what the GitHub profile communicates?
2.  Can the user reach supporting evidence?
3.  Does the language describe observation instead of judgment?
4.  Are internal concepts translated into user language?
5.  Is complexity hidden behind a simple experience?

------------------------------------------------------------------------

# 15. Core Experience Statement

GitHub Proof:

    Shows traces.
    Reveals direction.
    Opens evidence.

The product does not tell users who someone is.

It helps them understand what visible GitHub traces communicate.
