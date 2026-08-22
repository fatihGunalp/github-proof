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
| EngineeringPracticePatterns | Recurring engineering practices | Tekrar eden üretim pratikleri |
| WhyThisConclusion | Why this view | Bu görünümü neden görüyoruz? |
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

These lines describe the general analysis, not individually tracked runtime
states. They never receive completed/active/pending labels, progress position,
percentage, ETA or checkmarks without a future telemetry contract that proves
those states. No SSE, SignalR, polling, streaming response or new backend
progress telemetry is introduced. Existing `ProfileExecutionTelemetry` is
completion/observation telemetry, not a live UI progress source.

The form/container exposes `aria-busy`; visible copy uses a polite atomic
`role=status` live region; duplicate submit is blocked without disabling
`GitHubTarget` out of form serialization; BFCache/back restoration clears
stale state; and motion respects `prefers-reduced-motion`.

## 8.4 P10-I3 Meaning Layer Deterministic Synthesis Contract

This section is the normative owner of the P10-I3 Meaning layer's exact
presentation shape and its exact user-facing copy. It adds no analysis
semantics: it fixes how already-projected typed truth is rendered.

### 8.4.1 Projection Source

The Meaning layer reads `ProfileReport.PublicProfileSummary` only, and from it
exactly two `ProfileSummaryFact` entries:

```text
ReferencedSection == TechnicalFocusAreas
    && ReferencedSectionAvailability == Supported
    → tfa := ItemCount > 0

ReferencedSection == EngineeringPracticePatterns
    && ReferencedSectionAvailability == Supported
    → epp := ItemCount > 0
```

Binding rules:

- `ItemCount` is never rendered. It is reduced to a `> 0` presence bit and
  nothing else. No count, number, percentage, ratio, badge or counter derived
  from it may appear in any user-facing surface of this layer.
- `RepresentativeRepositories` does not participate in Meaning state
  derivation and its summary fact is not read here (§8.4.6).
- The `WhyThisConclusion`, `Limitations` and `RepositoryDrillDown` facts are
  not read by this layer; those surfaces remain owned by P10-I4 and P10-I5.
- The `ProblemAreaPatterns` (`Unsupported`) and `RoleEvidenceAlignment`
  (`Deferred`) facts are never read (see
  `docs/product/p10-web-profile-flow-implementation-plan.md` §13.10/§13.11).
- If either expected fact is absent from `PublicProfileSummary.Items`, or
  carries an availability other than `Supported`, that is a contract
  violation --- not an empty-state fallback. Presentation must fail loudly
  rather than invent a state.
- The Meaning presentation surface accepts `IReadOnlyList<ProfileSummaryFact>`
  only. Repository references, pattern items, snapshots, signals, raw
  evidence, eligibility results and score types never reach it.

### 8.4.2 Closed Shape

The Meaning body is exactly one deterministic synthesis sentence.

It carries no additional evidence statements, no separate evidence-scope or
provenance sentence, no list, and no numeric value. Evidence scope is already
carried inside every synthesis variant's own wording, and evidence-boundary /
limitation communication remains P10-I5's responsibility.

This satisfies `docs/product/product-experience-principles.md` §6 --- one
short plain-language synthesis plus at most three evidence statements --- at
its smallest safe size. The permitted maximum is a ceiling, not a requirement:
a generic evidence statement that merely restates the synthesis communicates
no additional typed truth and is therefore prohibited here. The concrete
pattern identities are rendered by the two sections that follow, from their
own `Items`.

### 8.4.3 Closed State Matrix

Exactly two bits, exactly four states. The matrix is a lookup table. No
variant may be derived, combined, extended or generated at runtime, and no
fifth state exists.

| State | `tfa` | `epp` | Rendered |
|---|---|---|---|
| M1 | present | present | Synthesis variant M1 |
| M2 | present | empty | Synthesis variant M2 |
| M3 | empty | present | Synthesis variant M3 |
| M4 | empty | empty | Synthesis variant M4 |

### 8.4.4 Locked EN/TR Synthesis Copy

| State | English (`en`) | Turkish (`tr`) |
|---|---|---|
| M1 | The visible public evidence shows recurring technical direction and recurring engineering practices. | Görünen herkese açık kanıtlar, tekrar eden teknik yön ve tekrar eden üretim pratikleri gösteriyor. |
| M2 | The visible public evidence shows recurring technical direction. | Görünen herkese açık kanıtlar, tekrar eden teknik yön gösteriyor. |
| M3 | The visible public evidence shows recurring engineering practices. | Görünen herkese açık kanıtlar, tekrar eden üretim pratikleri gösteriyor. |
| M4 | The current analysis did not establish a recurring technical direction or engineering-practice pattern from the visible public evidence. | Mevcut analiz, görünen herkese açık kanıtlardan tekrar eden bir teknik yön veya üretim pratiği ortaya koymadı. |

Why this copy stays inside the existing epistemic boundary:

- `shows` / `gösteriyor` is the sanctioned observation verb of §7 (Visible
  Direction), which already permits "This profile shows recurring traces
  related to backend systems." while prohibiting "This person is a backend
  developer."
- `recurring` / `tekrar eden` is the canonical `RepeatedProfilePattern`
  semantic --- evidence that repeats across two or more independent public
  repositories --- which carries no strength, confidence, ranking or
  capability meaning.
- M4's `did not establish` / `ortaya koymadı` is the plain-language form of
  the `Supported + empty` definition owned by
  `docs/architecture/public-profile-evidence-report-contract.md` §6.1: the
  capability ran and produced no result under its versioned rules. It is
  never a negative developer conclusion (§6.4).
- M1 and M2 carry no indefinite article, so the sentence stays independent of
  how many items the section holds, consistent with the rule that `ItemCount`
  is never rendered. M4 keeps its article because negative polarity carries
  no count implication.

### 8.4.5 Traceability

Every user-visible string of this layer is authorized by typed report truth
only:

| Rendered text | Authorizing typed field(s) | Exact condition |
|---|---|---|
| Synthesis M1 | `ProfileSummaryFact{TechnicalFocusAreas, Supported}` and `ProfileSummaryFact{EngineeringPracticePatterns, Supported}` | both `ItemCount > 0` |
| Synthesis M2 | the same two facts | `tfa.ItemCount > 0` and `epp.ItemCount == 0` |
| Synthesis M3 | the same two facts | `tfa.ItemCount == 0` and `epp.ItemCount > 0` |
| Synthesis M4 | the same two facts | both `ItemCount == 0` |
| Section labels | `ProfileReportSectionKey` | existing §8 dictionary mapping, unchanged |

Keying rule: this copy is resolved inside the Web presentation mapping keyed
only by canonical, locale-independent identities --- here the two presence
bits plus the selected request culture --- consistent with §8.2 and with
`docs/product/p10-web-profile-flow-implementation-plan.md` §13.8. It is not a
page-chrome string and must never be keyed by presentation text.

The projection is strictly lossy: `(section, availability, int)` collapses to
one bit per section. No new bit is produced, so compressing existing evidence
can never upgrade its epistemic strength
(`docs/architecture/public-profile-evidence-report-contract.md` §8).

### 8.4.6 Prohibitions

The Meaning layer must never:

- let `RepresentativeRepositories` feed Meaning state derivation, or carry
  repository identity, `Owner`/`Name`/`Url`, pattern supports or selection
  detail into the Meaning surface;
- render any count, number, percentage, ratio, badge or counter;
- express comparison, ranking, magnitude, or a strong/weak axis;
- use `strong`, `weak`, `senior`, `junior`, `experienced`, `skilled`,
  `expert`, `employable`, `suitable`, `best`, `top`, `impressive`,
  `consistent`, or their Turkish equivalents;
- infer role, seniority, employability, capability, quality, confidence, AI
  authorship or personality;
- introduce a taxonomy mapping such as `C# → backend developer`
  (`docs/product/p10-web-profile-flow-implementation-plan.md` §13.15);
- collapse `Unsupported` and `Deferred` into one "not found" state;
- render an empty card, label or empty copy for `ProblemAreaPatterns` or
  `RoleEvidenceAlignment`;
- present `Supported + empty` as a negative developer finding;
- translate programming/GitHub language identities;
- generate, combine, synthesize or modify user-facing prose outside the four
  locked M1–M4 variants at runtime, or use an LLM for any part of this
  surface. Deterministically selecting exactly one locked M1–M4 variant from
  the closed state matrix at runtime IS permitted and is the intended
  mechanism; producing any other string is not;
- read raw GitHub data, snapshots, signals, evidence, eligibility results or
  scores.

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
| `EngineeringPracticePatterns` | The visible public evidence did not establish recurring engineering practices under the current analysis rules. | Görünen herkese açık kanıtlar, mevcut analiz kuralları altında tekrar eden üretim pratikleri ortaya koymadı. |
| `RepresentativeRepositories` | No starting-point repository was selected from the current visible public evidence. | Mevcut görünen herkese açık kanıtlardan bir başlangıç noktası repository'si seçilmedi. |
| `WhyThisConclusion` | The visible public evidence did not establish a repeated pattern to explain under the current analysis rules. | Görünen herkese açık kanıtlar, mevcut analiz kuralları altında açıklanacak tekrar eden bir örüntü ortaya koymadı. |
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
  In state M4 the Meaning synthesis and the two pattern sections therefore
  each state a negative result; this repetition is deliberate, so that a
  canonical section never fails to report its own outcome.
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
    → Recurring engineering practices
    → Starting point          (first read — quiet, text only)
    → Why this view           (evidence layer — collapsed by default)
    → Repository evidence     (evidence layer — collapsed by default)
```

`RepresentativeRepositories` / Starting point stays **text only** in P10-I4.
No Repository Proof action, link or button is added to it. Repository Proof
actions exist only inside the progressively disclosed `RepositoryDrillDown`
surface (§8.6.5), so the product never presents two competing repository
drill-down action surfaces. The first-read layer stays quiet and evidence
inspection begins only when the reader explicitly enters the evidence layer.
A later slice (P10-I6) may revisit this only if user testing proves it
necessary.

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

One Why entry shows exactly two things:

1. the localized pattern label, resolved by the existing pattern-label
   mapping (no new copy, no new identity);
2. the pattern's supporting public repositories, rendered as literal
   repository full names in the canonical order the pattern already provides,
   under the locked lead-in line of §8.6.7.

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
Why this view        : pattern → supporting repository identities
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
  prevention. That script is not modified, and no per-row visible processing
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

Existing section labels are unchanged and are not restated as new copy:
`WhyThisConclusion` stays "Why this view" / "Bu görünümü neden görüyoruz?" and
`RepositoryDrillDown` stays "Repository evidence" / "Repository kanıtı".

| Identity | English (`en`) | Turkish (`tr`) |
|---|---|---|
| Why --- supporting repositories lead-in | Observed across these public repositories | Şu herkese açık repository'lerde gözlemlendi |
| Repository evidence --- fresh-analysis lead-in | Selecting a repository runs a fresh Repository Proof analysis. | Bir repository seçmek yeni bir Repository Proof analizi başlatır. |
| Repository action --- visible label | Analyze this repository | Bu repository'yi analiz et |
| Repository action --- accessible name (`{0}` = repository full name) | Analyze repository {0} | {0} repository'sini analiz et |

Row status copy is locked in §8.6.6. `Supported`-but-empty copy for both
sections is locked in §8.5.

The Why lead-in says "observed across", never "repeated in": a repeated
profile pattern is evidence that repeats **across** independent repositories,
which does not assert that the evidence repeats inside any single repository.

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
- **Fixed P10-I4 presentation-slot copy** --- the Why supporting-repositories
  lead-in, the Repository-evidence fresh-analysis lead-in, the visible action
  label and the accessible-name format --- has no canonical identity of its
  own, because these are fixed slots of one section rather than one string per
  canonical value. Each therefore lives in its own narrow, explicitly named
  `ProfileWebPresentationLocalizer` method and varies only by the selected
  request culture.

Two rules bind all four cases: presentation text is never used as a lookup
key, and no new analysis, Domain or Application semantic identity, enum or
value is created merely to key a fixed presentation string. This keeps §8.2
and §8.4.5 satisfied without inventing taxonomy that canonical truth does not
own.

### 8.6.8 Accessibility and Information Architecture

- The page keeps exactly one `h1`. Each report section contributes one `h2`,
  including the two disclosure sections (their `h2` lives inside `summary`).
- A Why entry's pattern label is an `h3` above its supporting-repository list.
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

> Recurring engineering practices / Tekrar eden üretim pratikleri

Then — EN / TR:

> Starting point / Başlangıç noktası

Finally — EN / TR:

> Why this view / Bu görünümü neden görüyoruz?

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
