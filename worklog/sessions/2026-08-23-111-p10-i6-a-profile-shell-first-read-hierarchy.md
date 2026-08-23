# Session 111 — P10-I6-A Profile Shell + First-Read Hierarchy

**Date:** 2026-08-23
**Result:** STEP 3 — READY FOR STARTING POINT DESCRIPTION LIVE REVIEW —
approved dark Profile mockup, bounded Starting Point presentation, canonical
avatar, canonical repository Description and the Starting Point card
bottom-edge alignment follow-up implemented; architecture-owner live
acceptance remains required
**Branch:** `session/111-p10-i6-a-profile-shell-first-read-hierarchy`
**Base HEAD:** `2001e14d79fadd32121f42d449e793209ea5f7bb`
**Model Route:** Terra / high

## Goal

Implement the first bounded P10-I6 visual-refinement slice: the Profile shell
and the Meaning → visible technical direction → recurring engineering
practices → Starting point first-read hierarchy. The approved dark Profile
mockup is now the primary visual target, while canonical report order, typed
truth, locale mapping and existing Profile runtime states remain preserved.

## Mutation Boundary

The intended scope is the existing Profile Razor presentation, its feature CSS,
targeted Web structural tests and this worklog state. Why/Repository Evidence
trace and inspection styling remains P10-I6-B. Responsive, theme and
localization closure remains P10-I6-C. No Domain, Application, Infrastructure,
configuration, Repository Proof, canonical-product-contract or artifact change
is authorized.

## Pre-existing Working Tree

- `?? artifacts/` is user-owned, untracked and read-only for this session.
- `main@2001e14` contains the integrated Session 110 P10-I5 work.

## Context Read

- Repository workflow, product-boundary, development, review and worklog
  playbooks.
- P10 implementation plan; canonical public Profile report contract; P10
  presentation/language contract; Session 105 design reconciliation and
  Session 110 handoff.
- Current Profile Razor partials, Profile CSS, Web structural tests, semantic
  token foundation and the design handoff Profile screen/reference notes.

## Initial Design Boundary

- Reuse only the handoff's neutral shell, restrained semantic blue emphasis,
  bounded first-read grid and mobile DOM-order intent.
- Do not introduce handoff-only icons, metrics, counts, badges, repository
  actions, evidence-row treatment, footer claims or unsupported inference.
- Preserve one `h1`; profile identity may serve that presentational role while
  the existing localized Profile-view label remains visible as an eyebrow.
- Preserve canonical DOM order and existing P10-I5 notices. Native Why and
  Repository Evidence disclosures remain behaviorally and visually deferred to
  P10-I6-B.

## Implemented Presentation Refinement

- The Profile page has a restrained, 74rem shell with the existing back link
  first, localized Profile-view eyebrow and the current literal profile
  identity as the sole `h1`.
- The canonical first-read DOM sequence is enclosed in a visual-only grid:
  Meaning, then any existing warning notice, visible technical direction,
  recurring engineering practices and text-only Starting Point. Desktop uses
  the handoff's proportional three-track intent; at 64rem it drops to one
  column in the same DOM order.
- Meaning receives the only accent surface. Technical direction is visually
  grouped as literal labels, practices as quiet rows and representative
  repositories as monospace text tiles. No count, score, quality, role,
  confidence, ranking, badge or action was added.
- Existing Partial/failure/limitation presentation remains intact. Why and
  Repository Evidence Razor partials and their native `details` behavior were
  not changed; their visual refinement is explicitly deferred to P10-I6-B.

## Contract and Scope Review

- The page retains one `h1`, canonical report DOM order and existing EN/TR
  mapping. Profile identity is literal; the Profile-view label remains
  localized.
- Starting Point remains text-only with no link, form, repository status or
  analysis action.
- No Domain, Application, Infrastructure, configuration, Repository Proof or
  canonical documentation file changed. The design handoff stayed read-only.
- The only pre-existing untracked path remains user-owned `artifacts/`.

## Architecture-Owner Rework

The first I6-A iteration was semantically correct but visually too sparse and
detached. The rework keeps the same typed report and interaction boundaries
while making the production page one bounded report composition:

- The first-read and existing continuation sections now share one centered,
  semantic-token canvas with restrained surface, border, radius and spacing
  relationships.
- Meaning remains the dominant two-column surface. Direction and Practices
  occupy the primary column below it; Starting Point is the text-only top of
  the secondary rail.
- Existing Repository Evidence occupies the related lower secondary rail, and
  the unchanged Why/Limitations sections continue in the primary column. This
  is visual placement only: native `details` disclosure, repository POST
  action, row coverage and canonical DOM order are unchanged.
- Mobile collapses the same canvas to a single column in DOM order. Identity
  heading scale, report-title rhythm and Meaning line length were aligned to
  the read-only selected handoff direction using existing semantic tokens.
- No prototype metric, claim, status, score, role inference, badge, action or
  data field was introduced. `artifacts/` stayed untouched.

## Visual Review Limitation

The selected design handoff was re-inspected read-only. A local running-app
visual check was attempted through the available browser control path, but the
host rejected its `browser-service` dependency as outside a configured trusted
code path before a browser binding could be established. No fallback browser
automation was used. The architecture-owner must therefore perform the final
live visual acceptance review.

## Second Architecture-Owner Visual Rework

The first rework was not accepted: its outer report canvas read as a giant
rounded rectangle, the three-track layout was too wide for the available
truth, and all sections had too much equal visual weight. The second rework
materially changes the composition without changing Profile semantics:

- Removed the outer report background, border, padding and radius. The page is
  the canvas; Profile uses the same compact `70rem`/approximately 1120px
  container as the global header inner content.
- Replaced the three-track grid with a content-sized two-column relationship:
  `minmax(0, 2fr)` main column and `minmax(17rem, 0.85fr)` secondary rail.
- Promoted Meaning to Level 1 with larger padding, strongest section heading,
  bounded body measure and accent surface. Direction and Practices are now a
  coordinated Level 2 pair within the main column, not unrelated page cards.
- Starting Point and the unchanged outer Repository Evidence disclosure are
  content-sized Level 2 rail surfaces. Neither stretches to manufacture
  symmetry or dead space.
- Why and Limitations are Level 3 narrative continuation only: lighter
  top-rule treatment, no full-card surface and no competitive emphasis.
- A narrow DOM-preserving wrapper groups only Direction and Practices. The
  mobile layout naturally reads Meaning → Direction → Practices → Starting
  Point → Why → Limitations → Repository Evidence; no CSS `order` is used.
- No prototype metric, score, percentage, role/seniority claim, active state,
  action or new data was added. I6-B disclosure, drill-down membership and
  Repository Proof POST behavior did not change.

## Architecture-Owner Visual-Fidelity Correction

The second rework was rejected as an interpretation rather than a port. The
exact read-only primary blueprint is now
`artifacts/design-handoff/github-proof-design-system/components/profile/profile.card.html`.
Before correction, its linked stylesheet, token/base imports, local component
bundle/ProfileScreen reference and local Lucide asset license were inspected
read-only. The correction ports the handoff's compact `54rem` canvas, its
`724fr / 310fr` two-column card proportion, 20px rhythm, card radii/surfaces,
hero treatment, paired content cards, icon-heading rhythm and mobile collapse.

The prototype-only metrics, analysis date/completed state, repository count and
active badges, role/capability language, and "next step"/"all evidence"
actions are deliberately omitted: no canonical Profile source exists for them.
The canonical Limitations section occupies the second lower main-card slot;
Starting Point remains text-only; Why and Repository Evidence retain their
native P10-I4 disclosure/action behavior. No DOM/mobile reordering or evidence
semantics change is authorized.

## Validation

- Targeted Profile presentation and CSS-architecture tests: **23/23 PASS**.
- Full Web suite: **316/316 PASS**.
- Full solution: **1704/1704 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 316 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS**.

## Visual-Fidelity Correction Validation

- Targeted `ProfileResultPresentationTests`: **20/20 PASS**.
- Full solution tests: **1704/1704 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 316 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Exact diff, production-file scope and contradiction review: **PASS**.
  The production diff is confined to the Web Razor/CSS presentation boundary;
  no Domain/Application/Infrastructure/configuration/Repository Proof file is
  changed. No CSS `order`, primitive-token consumer reference or prototype
  metric/status/action field was introduced.
- `git diff --check`: **PASS**.
- The local Debug Web process held its standard output DLL/executable during
  validation. Tests/build therefore used per-project temporary output folders
  with the standard intermediate directory; all temporary folders were removed
  afterward. This did not modify `artifacts/`.

## Architecture-Owner Disclosure / Layout-State Correction

The live visual review returned **CHANGES REQUIRED**. The closed state was
sufficiently close, but opening Repository Evidence or Why exposed the prior
desktop-grid defect: Repository Evidence shared the report grid's row sizing
with first-read surfaces. Its expanding height could therefore push Direction,
Practices and Starting Point downward, leave blank space below Meaning, narrow
repository identities in the rail and visually precede Starting Point.

The correction is structural only:

- `profile-first-read` now contains exactly the primary Meaning + warning +
  Direction/Practices composition and the text-only Starting Point surface.
- The first-read grid ends there. Why This View, Limitations and Repository
  Evidence are direct, canonical-order children of the report's normal
  document flow.
- Repository Evidence is no longer a secondary-rail item. Its closed state is
  compact at report width, and its native open state expands downward with a
  readable full-width inspection surface.
- Why This View likewise uses report width in both closed and open states;
  no trace membership, label or native `details` behavior changed.
- Repository identities now use `min-width: 0`, `overflow-wrap: anywhere` and
  normal word breaking. No canonical identity is altered or truncated.
- CSS no longer has Profile `grid-row`, fixed grid-row definitions, CSS
  `order` or dense auto-flow placement. No fixed/min-height or artificial rail
  reservation was added.

The global header regression check found no Session 111 production change to
`_Layout.cshtml` or `site.css`: the existing GitHub Proof / language / theme
header remains in the common layout. The supplied screenshot omission is not a
Profile implementation regression.

No P10-I6-B semantic or behavior expansion occurred: Repository Evidence
membership/truth, native disclosure, Repository Proof POST/GitHubTarget/button
behavior and Why trace membership remain unchanged.

## Disclosure-State Correction Validation

- Targeted `ProfileResultPresentationTests`: **20/20 PASS**.
- Full Web suite: **316/316 PASS**.
- Full solution: **1704/1704 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 316 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS**.
- Structural tests lock the first-read wrapper, canonical placement of
  Repository Evidence after Limitations, and the absence of Profile `grid-row`,
  fixed grid rows, dense auto-flow and CSS `order` rearrangement. Existing
  text-only Starting Point, native Why disclosure and Repository Evidence POST
  behavior tests remain passing.
- Temporary per-project build-output folders were removed after validation;
  `artifacts/` remained untouched.

## Architecture-Owner Meaning Presentation Contract Correction

Live review identified an information-loss gap: the former P10-I3 Meaning
surface reduced the two pattern sections to a 2-bit/4-state generic sentence,
so it could not tell a visitor which canonical technical directions or
engineering practices the searched Profile actually contains.

The old M1–M4 contract is fully superseded in the canonical presentation
contract. Meaning now receives a narrow Web-owned `ProfileMeaningPresentation`
whose three deterministic localized paragraphs are derived only from the
existing ordered `TechnicalFocusAreas.Items` and
`EngineeringPracticePatterns.Items`:

- every current canonical label is included in report order with no cap,
  ranking, sorting or count substitution;
- each section has its own truthful Supported + empty sentence;
- the third paragraph is the locked public-evidence/current-rules provenance
  sentence;
- `RepresentativeRepositories`, Why, Repository evidence, warnings,
  limitations and raw evidence remain excluded.

No Domain/Application analysis, evidence collection, pattern promotion,
representative selection, scoring or `ProfileReportProjector` behavior changed.
`ProfileReportProjector` remains locale-neutral. The existing first-read grid
independence, canonical DOM order, full-width Why/Repository Evidence open
states and repository-name wrapping correction remain intact.

For the current live `fatihGunalp` example (Technical direction `C#`, practices
Supported + empty), the Turkish Meaning narrative is:

> Bu profilde tekrar eden teknik izler C# yönünde yoğunlaşıyor.
>
> Mevcut analiz kuralları altında tekrar eden bir üretim pratiği belirginleşmedi.
>
> Bu özet yalnızca herkese açık ve mevcut analiz kurallarıyla gözlemlenebilen
> izlere dayanır.

## Meaning Correction Validation

- Targeted Meaning/localization/presentation tests: **64/64 PASS**.
- Full Web suite: **317/317 PASS**.
- Full solution: **1705/1705 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 317 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS**.
- The targeted tests lock all six populated/empty combinations in EN/TR,
  locale-aware joining, complete ordered-label inclusion without a cap,
  exclusions through the narrow two-section API, byte-identical output and the
  locale-neutral projector boundary.
- Per-project temporary test/build output folders were removed after
  validation; `artifacts/` remained untouched.

## Architecture-Owner Meaning Narrative Refinement

The architecture-owner rejected the prior item-aware Meaning copy as too
mechanical for the product question. HR/recruiter readability is now an
explicit acceptance criterion: a visitor reading only the Meaning card must
understand recurring technical direction, recurring visible production-practice
use, and the relationship between the two evidence surfaces without needing
analysis-system vocabulary.

The bounded Web presentation remains deterministic and now receives the
accepted Profile identity in addition to the same two ordered pattern sections.
It produces a username-led two-block narrative plus a short provenance line:

- the first block explains why the ordered repeated technical labels make a
  technical direction visible;
- the second explains repeated production-practice use across repositories and
  deterministically expresses whether technical evidence, practice evidence or
  both are more visible in the current Profile;
- the exhaustive technical/practice present-or-empty relationship states and
  EN/TR templates are locked in the amended presentation contract;
- canonical order, all labels and no hidden cap remain preserved.

No raw repository data, representative selection, Why, drill-down, warning or
limitation truth enters Meaning. No role, seniority, experience, ability,
capability, quality, score, confidence or developer judgment was introduced.
Domain, Application, Infrastructure, Repository Proof and
`ProfileReportProjector` remain unchanged; the first-read/disclosure-layout
corrections remain intact.

## HR-Readability Refinement Validation

- Targeted Meaning/localization/presentation tests: **65/65 PASS**.
- Full Web suite: **318/318 PASS**.
- Full solution: **1706/1706 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 318 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS**.
- Tests lock username-first EN/TR narrative, all four relationship states,
  ordered all-label joining without a cap, repeated visible use rather than
  expertise, evidence visibility rather than developer quality, byte-stable
  output and the locale-neutral projector boundary.
- Per-project temporary test/build output folders were removed after
  validation; `artifacts/` remained untouched.

## Architecture-Owner Meaning Human-Narrative Refinement

The architecture-owner rejected the prior Meaning copy because it still read
like a compact analysis report. The same bounded Web presentation now uses a
human explanation sequence for both recruiter/HR and technical first readers:

- it observes the actual ordered technical directions visible in the Profile;
- it explains every promoted canonical engineering practice in ordinary
  language through a deterministic `PatternKey` mapping, then says that its
  public traces recur across different repositories;
- it closes populated-practice cases with one evidence-bounded synthesis of
  the working methods those repeated traces make visible; this is not a claim
  about how the person always works;
- empty practice copy says only that no recurring practice was promoted across
  repositories and explicitly does not claim that any practice, skill or use
  is absent.

The active §8.4 contract now locks the human-explanation boundary, the
observation → explanation → repetition → working-method sequence, every
current canonical practice mapping (`automated-testing`,
`continuous-integration`, `documentation`, `release-usage`, `issue-usage`,
`pull-request-usage`), canonical order/no hidden cap, and the four
technical/practice presence states. The Web localizer owns the fixed EN/TR
templates; Razor still receives only `ProfileMeaningPresentation`.

No Domain/Application analysis, evidence collection, pattern promotion,
ProfileReport projection, Infrastructure, Repository Proof, configuration or
artifact changed. No role, seniority, capability, expertise, quality, score,
confidence or developer judgment was introduced.

### Recency Contract Gap

The canonical `ProfileReport` contains no authoritative deterministic
profile-level “last observable public activity/change” fact. Existing
`ProfileRepository.LastPushedAt` is repository-level semantic content, and
`ActivityClassification` is an eligibility recency boundary, not a reportable
profile-level recency fact. Meaning therefore does not derive dates from raw
repositories, drill-down or commits. This requires a separate architecture
decision if product copy needs a “last visible public change was X days ago”
statement.

### Validation

- Targeted Meaning/localization/presentation tests: **71/71 PASS**.
- Full Web suite: **324/324 PASS**.
- Full solution: **1712/1712 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 324 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Final exact-diff, production-scope and contradiction review: **PASS**.
  `git diff --check`: **PASS**. The production diff remains confined to the
  existing Session 111 Web presentation boundary; Domain, Application,
  Infrastructure and Repository Proof remain unchanged.

## Architecture-Owner Width + Starting Point Usability Refinement

The architecture-owner found the desktop Profile result surface too narrow and
requested a compact, actionable Starting Point without changing first-read
order or the evidence/disclosure surfaces. The bounded correction:

- widens the centered desktop Profile container from `54rem` to `64rem`
  (approximately 18.5%), while retaining the existing `60rem` and `45rem`
  responsive-collapse boundaries;
- keeps Meaning as the dominant first-read surface and keeps Why, Limitations
  and Repository Evidence in their existing full-width normal document flow;
- renders every canonical Starting Point repository in its existing report
  order with its literal full name and the existing localized Repository Proof
  action;
- reuses the established POST contract exactly: form → `/Index` → hidden
  `GitHubTarget=@item.Repository.Url` → existing validation/anti-forgery and
  Repository Proof entry. No outbound GitHub `href`, new endpoint, controller,
  service or analysis logic was added;
- keeps the action visually secondary, preserves safe long-name wrapping and
  adds no score, status, activity, stars, forks, language percentage, role or
  quality field.

### Starting Point Description Contract Gap

The authoritative current `RepresentativeRepository` / `ProfileReport` path
contains `ProfileRepositoryReference` only: owner, name and absolute URL. It
does not carry a repository Description. Web does not fetch GitHub again,
inspect README/raw data, derive a description from the name or render an empty
description placeholder. A short description requires a separate canonical
typed-data decision.

### Validation

- Targeted Profile presentation/localization tests: **73/73 PASS**.
- Full Web suite: **326/326 PASS**.
- Full solution: **1714/1714 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 326 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Final exact-diff, production-scope and contradiction review: **PASS**.
  `git diff --check`: **PASS**. The production diff remains confined to the
  existing Session 111 Web presentation boundary; Domain, Application,
  Infrastructure and Repository Proof remain unchanged.

## Architecture-Owner Working Style Card Simplification

The architecture-owner required the `EngineeringPracticePatterns` first-read
card to become a quick, human-readable working-method summary rather than a
compact analytical explanation. The bounded Web correction:

- changes only this card's heading from `Recurring engineering practices` /
  `Tekrar eden üretim pratikleri` to `Working style` / `Çalışma biçimi`;
- renders all canonical EngineeringPractice items in existing report order as
  short bullet rows with no cap, sort, ranking, metric or per-item paragraph;
- keeps Meaning's existing richer deterministic human explanation untouched;
- maps only the six canonical practice identities to these evidence-bounded
  labels: `Automated testing` / `Otomatik test kullanımı`, `Automated
  integration checks` / `Otomatik kontrol süreçleri`, `Documentation` /
  `Dokümantasyon`, `Published releases` / `Sürüm yayınları`, `Issue tracking`
  / `Issue ile takip`, and `Pull request workflow` / `Pull request ile
  değişiklik akışı`;
- replaces the card's `Supported + empty` copy with `No recurring working
  method is visible across the current public work.` / `Farklı çalışmalarda
  tekrar eden belirgin bir yöntem görünmüyor.`

The card does not derive labels from display text and rejects an unknown
canonical identity. It adds no person-level methodology, behavior, motivation,
capability, quality, role or seniority conclusion. Domain, Application,
Infrastructure, Repository Proof, analysis semantics, first-read DOM order,
width, Starting Point, Why, Limitations, Repository Evidence and `artifacts/`
remain unchanged.

### Validation

- Targeted Profile presentation/localization tests: **81/81 PASS**.
- Full Web suite: **334/334 PASS**.
- Full solution: **1722/1722 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 334 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Final exact-diff, production-scope and contradiction review: **PASS**.
  `git diff --check`: **PASS**. The production diff remains confined to the
  existing Session 111 Web presentation boundary; Domain, Application,
  Infrastructure and Repository Proof remain unchanged.

## Architecture-Owner Profile Identity Header + Shareability Refinement

The architecture-owner required the result page to lead with the public GitHub
identity rather than the internal-sounding Profile-view eyebrow. The bounded
successful-result header now:

- uses the composed profile's exact canonical `ProfileIdentity.Login` as the
  single `h1` and projects the visible `github.com/{login}` identity from its
  exact canonical `ProfileIdentity.Url`;
- removes the visible `Profile view` / `Profil görünümü` eyebrow and adds only
  `View of public GitHub work` / `Herkese açık GitHub çalışmalarının görünümü`;
- exposes `Open on GitHub` / `GitHub'da aç` as a secondary safe external action
  using that typed URL, a localized new-tab accessible name, external-link icon,
  `target=_blank` and `noopener noreferrer external`;
- keeps the `64rem` centered measure, lets actions wrap beneath identity on
  small screens, and adds no technology/practice content, score, percentage,
  count or activity classification to the header.

### Profile Avatar Contract Gap

Current `GitHubProfileMetadata`, `DeveloperPublicEvidenceProfile` and
`ProfileReport` carry no canonical avatar URL. The header therefore renders no
image, generated initials, placeholder or Web-side GitHub fetch.

### Recency Contract Gap

There is still no authoritative profile-level last-observable-public-change
fact. Snapshot/retrieval times, account creation, repository `LastPushedAt` and
eligibility `ActivityClassification` do not authorize the requested copy. The
recency row stays absent; no active/inactive/cadence claim is made.

### Shareable Profile URL Contract Gap

`POST /profile` produces a transient result, while `GET /profile` redirects to
`/Index`; no persistence/cache lookup can reproduce the view for a third party.
No Share button/menu, LinkedIn action, Copy link action, dead control or fake
current-URL clipboard fallback was added.

### Social Preview Contract Gap

There is no stable Profile URL or profile-specific Open Graph/canonical metadata
path, and a crawler cannot access the POST response state. No generic or
fabricated social metadata and no LinkedIn integration were added. Current
canonical truth could support only username plus ordered technical/practice
items, but no exact share copy is emitted while the route/preview gaps remain.

### Validation

- Targeted Profile model/presentation/localization tests: **113/113 PASS**.
- Full Web suite: **338/338 PASS**.
- Full solution: **1726/1726 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 338 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Final exact-diff, production-scope and contradiction review: **PASS**.
  `git diff --check`: **PASS**. The production diff remains confined to the
  existing Session 111 Web presentation/page-model boundary; Domain,
  Application, Infrastructure, Repository Proof, configuration and external
  dependencies remain unchanged.

## Architecture-Owner First-Read Clarity Correction

Live review across `fatihGunalp`, `yusuftalhaklc` and `irmakcosar` established
that low-information Profiles remained readable while evidence-rich Profiles
could again resemble an analysis report. `yusuftalhaklc` was the decisive case:
the authenticated live pipeline completed 57 admitted repositories and
projected six technical directions, three working methods and eight grouped
representative repositories.

The bounded correction:

- replaces Meaning's per-practice glossary/definition expansion with exactly
  two short deterministic synthesis paragraphs plus the existing quiet
  evidence-boundary sentence;
- preserves every canonical technical and practice item in report order with
  no cap, while listing practice items only once;
- keeps Working Style as the compact canonical-order label list and retains its
  one-sentence empty state;
- visually binds the canonical identity copy and GitHub action inside one
  compact header surface; avatar and recency remain omitted under their
  existing gaps;
- renames Why from `Why this view` / `Bu görünümü neden görüyoruz?` to
  `What is this based on?` / `Bunu neye dayanarak söylüyoruz?`;
- translates each existing `WhyThisConclusion` item into a deterministic
  visitor question, one family-bounded human reason and the unchanged canonical
  supporting repository identities; no raw evidence, EvidenceKey, count,
  repository relationship or inference is added;
- leaves Repository Evidence, Repository Proof actions and deep inspection
  unchanged.

### Starting Point Prioritization Contract Gap

The canonical P7 selection unit is one representative repository per repeated
pattern. Equal repositories are grouped, and the final collection is ordered
by `ProfileRepositoryReference`; the contract explicitly has no global top-N,
top-K, first or recommended priority. Eight Starting Point rows can therefore
appear when eight distinct grouped repositories win the independent pattern
selections. Web cannot silently take the first one or first three. A genuine
bounded Starting Point requires a separately approved projection/priority
semantic over the completed canonical selection.

### Starting Point Description Contract Gap

The report path still carries only repository owner, name and URL. No canonical
Description exists, so no name-derived or fetched description is rendered.

### Project-Type Presentation Pending Canonical Contract

Current Profile truth exposes recurring technologies, not project/application
types. Web does not infer API, backend, authentication, realtime, Web, mobile
or AI from names or languages.

### Empty Limitations Presentation Contract Gap

The active report contract permits omission of sanctioned `Unsupported` or
`Deferred` sections only; it expressly forbids omitting a `Supported` section
solely because its items are empty. `Limitations` remains Supported position 7,
so the neutral empty card remains. Omitting it requires a separately approved
presentation exception/canonical change.

### Live Examples

- `fatihGunalp`: `C#`, no recurring Working Style item, one representative.
- `yusuftalhaklc`: `C#`, `Go`, `HTML`, `Java`, `Kotlin`, `Python`; automated
  testing, automated integration checks and documentation; eight grouped
  representatives.
- `irmakcosar`: `C#`, `HTML`, no recurring Working Style item, two grouped
  representatives.

The first authenticated `yusuftalhaklc` attempt without the existing CLI token
truthfully returned provider RateLimited and produced no report. The controlled
retry used the existing `GITHUB_TOKEN` integration without logging or storing
the token and completed successfully. This is live-validation context only.

### First-Read Correction Validation

- Targeted Profile model/presentation/localization tests: **118/118 PASS**.
- Full Web suite: **343/343 PASS**.
- Full solution: **1731/1731 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 343 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- Final exact-diff and contradiction review: **PASS**. The later §8.4.9
  explicitly supersedes the historical detailed-practice templates; active
  Web copy has no glossary expansion or old Why heading.
- Production-scope verification: Domain, Application, Infrastructure,
  configuration and Repository Proof diffs are empty.
- `git diff --check`: **PASS**; only the repository's existing LF→CRLF working-
  copy warnings were emitted.
- Authenticated live Profile runs completed for all three review identities;
  final copy is also locked by deterministic localizer tests.
- Domain, Application, Infrastructure, Repository Proof, configuration,
  dependencies and `artifacts/` remain unchanged.

## Approved Mockup Implementation

The approved mockup is now the implemented first-read visual direction:

- The desktop Profile surface uses a wider, dark premium composition with a
  compact public-identity header, dominant blue-accented Meaning card, paired
  technical/Working Style cards, bordered Starting Point rail cards and
  full-width collapsed Why/Repository Evidence summaries.
- The header reserves the mockup's image position with an `aria-hidden`
  decorative GitHub mark only. It is not an avatar; no profile image, generated
  initials or Web GitHub fetch exists. The typed avatar and recency gaps remain
  active, and the one canonical GitHub external action remains correctly
  grouped with identity.
- The mockup's work-type visual heading is rendered, but its contents remain
  only the canonical ordered technical-language labels plus a quiet recurring-
  technologies line. No project/application type is inferred.
- Every canonical Starting Point row remains visible in canonical order with
  the existing Repository Proof POST action. It is not capped to the mockup's
  example count and has no fabricated description or priority.
- Why and Repository Evidence remain native closed `details` disclosures. Their
  closed summaries add only localized visitor-facing explanatory lines and a
  decorative chevron; body evidence/action semantics are unchanged.
- The mockup's Share action is intentionally absent because Profile results are
  transient POST responses. Canonical Supported Limitations also remains in
  order, even though it is not depicted by the mockup.

### Mockup Implementation Validation

- Targeted Profile presentation/localization tests: **92/92 PASS**.
- Full Web suite: **345/345 PASS**.
- Full solution: **1733/1733 PASS**
  (`834 Domain + 257 Application + 297 Infrastructure + 345 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format GitHubProof.slnx --verify-no-changes --no-restore`: **PASS**.
- `git diff --check`: **PASS** (only existing LF→CRLF working-copy warnings).
- Exact changed-file and production-scope review: no Domain, Application,
  Infrastructure or configuration production file changed; no new external
  package, raw GitHub presentation fetch, Web-side top-N/ranking, recency,
  project-type or repository-description inference was introduced.
- A local Browser attempt reached the real profile route, but live GitHub
  collection returned the existing typed provider RateLimited state before a
  result could render. This is an environment/provider limit, not a Profile
  presentation result or an excuse to fabricate a live report.
- The local browser process was stopped after this check. No token was read,
  logged or stored; `artifacts/` stayed untouched.

## STEP 2 — Bounded Starting Point + Canonical Avatar

Architecture-owner reviewed real `yusuftalhaklc`-shaped executions
(RepositoriesAdmitted up to 57, GitHubRequests up to 793) and confirmed the
Starting Point Prioritization Contract Gap concern from the first-read
correction above: an unbounded right rail destabilizes the two-column
first-read grid. Two explicitly authorized, bounded changes:

- **Starting Point presentation bound (max 3).** New
  `GitHubProof.Web.Presentation.StartingPointPresentation.Select` takes the
  first 3 items of the existing canonical `RepresentativeRepositories` order
  (Owner-then-Name ordinal). This is a Web-only presentation bound, not a
  ranking: no pattern-support count, evidence count or name-based sort is
  introduced, and the canonical collection is never capped, reordered or
  mutated — `_ProfileStartingPoint.cshtml` still receives the complete
  section and calls the helper itself. This narrows (does not close) the
  Starting Point Prioritization Contract Gap; the Starting Point Description
  Contract Gap is unchanged (Step 3).
- **Bounded desktop rail.** The repository list gets a fixed `max-height` +
  `overflow-y: auto`; the existing `60rem` breakpoint removes the bound
  (`max-height: none; overflow-y: visible`) so mobile returns to natural flow
  instead of a nested scroll region.
- **Canonical avatar (collection → presentation).** `GitHubProfileMetadata`
  now requires an `AvatarUrl` (absolute HTTPS only).
  `GitHubProfileMetadataCollector` reads GitHub's own `avatar_url` from the
  same successful profile-metadata response, under the same required-field
  philosophy as `created_at`/`public_repos` — no new GitHub request. The value
  travels `GitHubProfileSnapshot.Metadata.AvatarUrl` → `ProfilePipeline.
  RunAsync` (already holds the full snapshot) → new
  `ProfilePipelineResult.AvatarUrl` → new `ProfileModel.AvatarUrl` —
  deliberately never through `DeveloperPublicEvidenceProfile`/`ProfileReport`,
  which stay evidence-report truth only (the same reason `DisplayName`/
  `CreatedAt` never reached Report either).
- **Header rendering.** Renders the real avatar (`<img>`, fixed dimensions,
  `object-fit: cover`, circular, accessible alt text naming the profile) when
  `Model.AvatarUrl` is available, and the existing decorative `aria-hidden`
  GitHub mark only when it is legitimately unavailable. No username-derived,
  generated or guessed avatar URL.
- Share, recency, Open Graph, repository descriptions and project/application
  type remain untouched and unattempted in this step.

### Contract Doc Updates

`docs/product/p10-presentation-language-contract.md` §8.6.1, §8.7.2 and §8.7.6
and `docs/product/p10-web-profile-flow-implementation-plan.md` §13.23 record
the authorized presentation bound and the now-closed avatar collection/
presentation gap, with a new dated amendment entry.

### STEP 2 Validation

- New `StartingPointPresentationTests` (Web): **9/9 PASS** — 0/1/2/3/4+ items,
  no reordering, no mutation of the canonical source, determinism, null
  rejection.
- Targeted Domain/Infrastructure/Application/Web tests touched by the avatar
  and Starting Point change: **PASS** (Domain 834/834, Application 259/259,
  Infrastructure 301/301).
- Full Web suite: **359/359 PASS**.
- Full solution: **1753/1753 PASS**
  (`834 Domain + 259 Application + 301 Infrastructure + 359 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS** (only existing LF→CRLF working-copy warnings).
- Confirmed by review: `RepresentativeRepositories` canonical result is
  unchanged and uncapped (`ProfileReportProjector`/Domain untouched); the
  Starting Point cap exists only in the new Web presentation helper; no new
  GitHub request was added (`avatar_url` reused from the existing
  `users/{login}` response); `artifacts/` remains untouched.

## STEP 3 — Canonical Starting Point Repository Description

Product goal: a visitor looking at a Starting Point item still could not tell
what the repository was about. This step adds one short description line per
presented item, sourced only from GitHub's own repository Description,
following the exact non-evidence metadata pattern STEP 2 used for the avatar.

- **Verified before editing.** `GitHubProfileRepositoryCollector` already
  calls `GET /users/{login}/repos` for the existing "profile-repositories"
  category, and that response already carries `description` for every
  repository — no `REPOSITORY DESCRIPTION PROVIDER GAP`, no new HTTP request
  needed.
- **Collection.** `GitHubProfileRepositoryResponse` now deserializes
  `description`; a private `NormalizedRepository(ProfileRepository, string?
  Description)` keeps it paired with its repository through pagination/
  de-duplication. `NormalizeDescription` trims surrounding whitespace only;
  null/blank/absent-field are all preserved as `null` and never treated as a
  collection failure.
- **Typed carrier.** New `RepositoryPresentationMetadata` (Domain,
  non-`ISnapshotSemanticComponent`) wraps a
  `IReadOnlyDictionary<ProfileRepositoryReference, string?>` behind
  `FindDescription(reference)`. It is explicitly excluded from
  `GitHubProfileSnapshot.WriteSemanticContent` — a new
  `RepositoryPresentationMetadata_DoesNotParticipateInSnapshotIdentity` test
  proves a Description-only difference never changes `SnapshotIdentity`.
  `ProfileRepositoryReference` equality/`CompareTo`,
  `RepresentativeRepository` and `RepresentativeRepositorySelectionPolicy` are
  byte-for-byte untouched — zero call sites in those types changed.
- **Propagation (mirrors STEP 2's avatar path exactly).**
  `GitHubProfileRepositoryCollection.RepositoryPresentationMetadata` →
  `GitHubProfileSnapshot.RepositoryPresentationMetadata` (new optional
  constructor parameter, default `Empty`, so every pre-existing
  `GitHubProfileSnapshot` call site kept compiling unchanged) →
  `ProfilePipeline.RunAsync` → new
  `ProfilePipelineResult.RepositoryPresentationMetadata` → new
  `ProfileModel.RepositoryPresentationMetadata`. Never through
  `DeveloperPublicEvidenceProfile`/`ProfileReport`, which remain exactly nine
  sections, unchanged.
- **Starting Point rendering.** New small `StartingPointSectionModel(Section,
  Descriptions)` is now `_ProfileStartingPoint.cshtml`'s `@model`, replacing
  the bare `ProfileReportSection<RepresentativeRepository>` — the smallest
  typed way to hand the partial both the existing canonical section and the
  new lookup. STEP 2's `StartingPointPresentation.Select(Model.Section.Items)`
  max-3 bound is unchanged. Each presented item's description comes from
  `Model.Descriptions.FindDescription(item.Repository)`; a null result renders
  the fixed localized fallback (`GetStartingPointDescriptionFallback`:
  `No short description is provided on GitHub.` / `GitHub'da kısa bir açıklama
  bulunmuyor.`) describing metadata absence only. Razor's default `@(...)`
  encoding renders it as plain text — the source has no `Html.Raw`, Markdown
  interpretation or link derivation.
- **Visual bound.** `.profile-result__starting-point-description` uses
  `-webkit-line-clamp: 2` + `overflow: hidden` + `overflow-wrap: anywhere` —
  visual only, never a semantic truncation. STEP 2's bounded desktop rail and
  mobile natural-flow behavior are unchanged.

### STEP 3 Validation

- `GitHubProfileRepositoryCollectorTests` (Infrastructure): **20/20 PASS**
  (7 new Description tests: populated/null/whitespace/absent-field,
  determinism, zero-extra-request, existing-fields-unchanged).
- `SnapshotIdentityTests` (Domain): **19/19 PASS** (1 new: Description does not
  participate in `SnapshotIdentity`).
- `ProfilePipelineTests` (Application): **14/14 PASS** (3 new: propagation
  keyed by canonical reference, empty-not-failed, no effect on representative
  selection/order/report).
- `ProfileModelTests` + `ProfileResultPresentationTests` +
  `ProfileWebPresentationLocalizerTests` (Web): all green, including new
  tests for description propagation, HTML-like-text preservation as data,
  EN/TR fallback copy and the visual-bound/no-truncation CSS rule.
- Full Web suite: **364/364 PASS**.
- Full solution: **1769/1769 PASS**
  (`835 Domain + 262 Application + 308 Infrastructure + 364 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS** (only existing LF→CRLF working-copy warnings).
- Confirmed: zero additional GitHub requests (description reuses the existing
  repository-metadata response); `RepresentativeRepositories`/`ProfileReport`
  unchanged and uncapped; `artifacts/` untouched.

## Follow-up — Starting Point Card Bottom-Edge Alignment

Live review found that the Starting Point card's own bottom edge did not
align with the bottom edge of the left column's paired work-type/Working
Style cards: the shared `.profile-first-read` grid row used `align-items:
start`, so whichever column was taller left the shorter column's card ending
early, with raw page background visible below it up to the row's full height.

**First attempt (CSS-only) and its own live-review finding.** `align-self:
stretch` on the Starting Point grid item plus `flex: 1 1 0` on its repository
list did align the card's outer edge to the row height — but a follow-up
screenshot from a real `fatihGunalp` run showed a large empty area *inside*
the card, beneath the single visible item: forcing the list to grow
(`flex: 1 1 0`) filled the stretched card with mostly dead space whenever
content was short, which does not satisfy "no unnecessary empty area under
the visible content." Analysis also found the first attempt was one-
directional: CSS grid's `auto` row sizing is inherently symmetric (driven by
whichever column is naturally taller), so if the Starting Point list's own
unconstrained content were ever taller than the left column (e.g. three items
with full-length descriptions and a short left column), the row itself would
grow to match the *rail*, breaking alignment in the other direction and
never triggering the required internal scroll.

**Corrected approach** (`profile-result.css`, `wwwroot/js/starting-point-
rail.js`, `_Layout.cshtml`) — still no Domain/Application/Infrastructure or
Razor-DOM-meaning change:

- The repository list keeps its own natural content height instead of being
  forced to grow (`flex: 0 1 auto`), and `margin-block: auto` lets it absorb
  any leftover stretched space evenly above and below itself — centering a
  short (1-2 item) list within the card instead of stranding it at the top
  with a large gap beneath it.
- `min-height: 0` still lets the list shrink below its natural size when the
  available space is smaller than its content, which is what makes internal
  scrolling possible at all.
- A new small page-scoped script, `wwwroot/js/starting-point-rail.js`
  (referenced from `_Layout.cshtml`, alongside the existing `theme.js` /
  `analysis-progress.js`), measures `.profile-first-read__primary`'s actual
  rendered height and applies it as an explicit `height` on the Starting
  Point card above the existing `60rem` breakpoint. This is what CSS alone
  cannot express: it makes the **left column authoritative** in both
  directions — the rail always conforms to it, scrolling internally instead
  of ever inflating the row itself, even when the rail's own unconstrained
  content would otherwise be taller. The script re-syncs on resize, on
  `pageshow`, on the mobile breakpoint's `matchMedia` change, and via a
  `ResizeObserver` on the primary column; on mobile it clears the inline
  height (`removeProperty("height")`) so the CSS-driven natural flow takes
  over unchanged. It introduces no network call, timer or new product
  behavior — it only reads layout geometry and sets one CSS property.
- A themed scrollbar (`scrollbar-width`/`scrollbar-color` plus
  `::-webkit-scrollbar*`, using the existing `--border-strong`/
  `--accent-primary` tokens) replaces the raw browser-default control.
- Starting Point's own structure (title, up to 3 items, description line,
  Analyze button), the existing POST/GitHubTarget flow, the max-3
  presentation bound and representative-repository selection are all
  unchanged.

Verified live for `fatihGunalp` (real GitHub data, no token) for the first,
CSS-only attempt: desktop bottom-edge difference was exactly `0px` (the
alignment itself was already correct; only the internal dead-space/one-
directional issues required the JS correction). A second live `fatihGunalp`
run to confirm the corrected version, and a live `yusuftalhaklc` run, were
both blocked by GitHub's anonymous rate limit in this environment (no
`GITHUB_TOKEN` configured here — it resets on GitHub's own schedule, not
under this session's control). The corrected 1/2/3-item and long-description/
overflow cases were instead verified via a throwaway static-HTML harness
loading the real production `profile-result.css` and `starting-point-
rail.js` (not committed — created and removed within this same turn):

- Case 1 (1 item) and Case 2 (2 items, taller left column): `diff = 0px`,
  content now visibly centered within the card rather than stuck at the top
  with a large trailing gap.
- Case 3 (3 items with full-length descriptions, short left column):
  `diff = 0px` (the script correctly capped the rail to the left column's
  height even though the rail's own unconstrained content is taller),
  `listScrollHeight (481) > listClientHeight (166)` confirming internal
  scroll is active, and the first item remains visible at the top of the
  list (no "hidden above the fold" centering/overflow conflict).
- `375px` mobile: single column, no inline height (script correctly cleared
  it), all three cases show `listScrollHeight == listClientHeight` (no
  scroll, full content visible, natural flow).

### Validation

- `ProfileResultPresentationTests` (updated + 3 tests covering the stretch/
  centering CSS, the mobile reset, and the rail script's contract): all
  green.
- Full Web suite: **367/367 PASS**.
- Full solution: **1772/1772 PASS**
  (`835 Domain + 262 Application + 308 Infrastructure + 367 Web`).
- Release build: **PASS**, 0 warnings / 0 errors.
- `dotnet format --verify-no-changes`: **PASS**.
- `git diff --check`: **PASS** (only existing LF→CRLF working-copy warnings).
- `artifacts/` untouched; the temporary probe HTML and `.claude/launch.json`
  used only for local live verification were removed before finishing.

## Review State

Implementation is **STEP 3 — READY FOR STARTING POINT DESCRIPTION LIVE
REVIEW**, now including the Starting Point card bottom-edge alignment
follow-up. Architecture-owner must review the actual rendered Starting Point
descriptions (populated and fallback), the visual line-clamp bound, the card's
stretched bottom-edge alignment with the paired cards, and that STEP 2's
max-3 behavior still holds, against a live evidence-rich Profile. This does
not claim visual acceptance. No commit, push, merge, rebase, tag or deploy was
performed.

## Exact Next Action

Architecture-owner live review of the STEP 3 Starting Point repository
Description (real text and neutral fallback), the card bottom-edge alignment
follow-up, together with STEP 2's bounded rail and canonical avatar.
Functional Share (Shareable Profile URL / Social
Preview Contract Gaps), recency, Starting Point true prioritization and
Project-Type remain separate, unimplemented canonical decisions.

## Suggested Commit

```text
feat(profile): render canonical GitHub repository description in Starting Point
```
