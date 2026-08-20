# Session 095 — P10-I2 Profile Entry + Routing

**Type:** implementation (Web) + bounded canonical docs amendment
**Branch:** session/095-p10-i2-profile-entry-routing
**Session Base:** f2ebd3c (feat(profile): add live profile request boundary) — `main`, verified clean working tree.

---

## 1. Goal

Implement P10-I2 (Profile Entry + Routing) and, on explicit user decision
taken after visual review of the running application, replace the previously
locked separate Profile Proof / Repository Proof entry surfaces with ONE
unified GitHub entry input.

The session ran in three user-directed stages:

1. Landing visual composition corrections (CSS/markup only, iterated against
   the running app until accepted).
2. Unified entry + `/profile` route + boundary icons.
3. Closing the three MEDIUM findings from independent review round 1.

## 2. Outcome

**P10-I2 COMPLETE.**

The landing page now carries one unified GitHub entry input. The two analysis
semantics were NOT merged: Profile Proof remains the macro view
(`LiveProfileReportService` → `ProfilePipeline`) and Repository Proof remains
the micro view (`LiveRepositoryReportService` → repository analysis path).
Only the entry experience is unified.

### Deterministic entry routing

Classification is two-stage and depends only on the submitted string's
shape — no heuristic, no probabilistic scoring, no AI/intent detection, and
no GitHub call to discover what the user meant. The same input always
resolves the same way.

~~~text
Stage 1 — shape selects the canonical owner
  scheme-less, no '/'       -> Profile candidate    (bare login)
  scheme + 1 path segment   -> Profile candidate    (profile URL)
  scheme + 2 path segments  -> Repository candidate (repository URL)
  anything else             -> not accepted

Stage 2 — the selected canonical parser decides acceptance
  GitHubProfileHandle.Parse  /  GitHubRepositoryUrl.Parse
~~~

Resulting behavior:

~~~text
bare GitHub login          -> Profile Proof
canonical profile URL      -> Profile Proof
canonical repository URL   -> Repository Proof
invalid / ambiguous shape  -> entry-level validation error; never guessed
~~~

Stage 2 **calls** the canonical parsers rather than reimplementing them: the
login pattern, scheme, host, userinfo, query and fragment rules keep exactly
one definition each. The parse result is discarded and the raw trimmed string
travels onward, so the collector's own `GitHubProfileHandle.Parse` remains the
parse of record and the Web layer never derives its own profile identity —
the P10-I1 boundary holds.

Shapes such as `owner/repo` and `github.com/owner/repo` are not part of the
accepted input contract and are rejected at the unified entry. They are not
silently normalized into either flow. Invalid input stays on the landing with
one message listing the three accepted shapes, and the submitted value is
preserved for correction in place.

### `/profile` route

`/profile` exists. Live profile analysis is POST-only:

~~~text
GET  /profile  -> no live analysis of any kind; redirects to the landing entry
POST /profile  -> LiveProfileReportService (the existing P10-I1 service)
~~~

Landing → profile routing preserves POST semantics via
`RedirectToPagePreserveMethod` (307), so the identity travels in the re-posted
form body and is never written into a query string. `/profile` POST
re-classifies defensively, so a direct POST cannot push a repository-shaped or
non-canonical value into the profile flow.

**Replay-safety scope, stated precisely:** GET `/profile` itself performs no
analysis, so a refresh-to-GET, browser back/forward to a GET, prefetch,
crawler or link-preview fetch cannot start a GitHub collection. This is NOT a
claim that every refresh is replay-safe — a browser re-submitting the POST
(the standard "resend form?" path) will re-run the analysis, which is ordinary
POST semantics.

`/profile` renders a minimal result boundary only. No report section
presentation was added: Meaning / Direction / Starting Point is P10-I3,
drill-down is P10-I4, and final failure/partial UX is P10-I5.

### Repository Proof

The existing Repository Proof analysis path remains intact. A canonical
repository URL runs exactly the same flow as before
(`LiveRepositoryReportService.AnalyzeAsync` → `Project`);
`RepositoryReportProjector` and every `_Repository*.cshtml` report partial are
untouched. What changed is the entry surface only: bound property
`RepositoryUrl` → `GitHubTarget`, partial `_RepositoryInput.cshtml` →
`_GitHubInput.cshtml`, model `RepositoryInputModel` → `GitHubInputModel`, and
the field's `type="url"` → `type="text"` (`type="url"` would have made the
browser reject a bare login before submission).

### Landing composition (accepted after live visual review)

~~~text
hero horizontally centered, optically above the mathematical centre
two-line desktop title ("See what your GitHub communicates.")
one unified input, wide and standalone
Analyze button below the input, narrower and centred
"What GitHub Proof is not" at the bottom of the first normal desktop viewport
responsive mobile behavior preserved (mobile scrolls naturally)
~~~

Implemented with `.home` as a CSS grid
(`grid-template-rows: minmax(0,1fr) auto minmax(0,2fr) auto`) over
`min-height: calc(100dvh - 4.5rem)`, so the free vertical space lands mainly
between hero and boundaries without any fixed-height fragility, sticky or
absolute positioning. Verified by direct measurement against the running
application at 1440×900 and 1280×720: no scrolling, title on two lines, hero
midpoint above the viewport midpoint.

### Boundary icons

Four decorative inline SVG icons were added to the four "What GitHub Proof is
not" items, through the existing `UiIcon` / `_UiIcon.cshtml` system — no icon
package, CDN, external asset, emoji, or new dependency. They follow the
theme-toggle icon language (`fill="none"`, `stroke="currentColor"`,
stroke-width 1.75, round linecap/linejoin, `aria-hidden="true"`) and share a
common prohibition stroke so each reads as a declined capability, never as a
judgment GitHub Proof performs. The text remains the contract; the icons are
secondary.

## 3. Canonical Docs Amended

The unified-entry decision superseded previously locked clauses, so three
canonical product documents were amended — bounded to the affected clauses
only:

- `docs/product/p10-web-profile-flow-implementation-plan.md` — new §13.0
  (unified entry: eight locked facts), amended §7/P10-I2 (slice scope),
  §13.1 (route boundary), §13.2 (identity input contract), §13.5 (Repository
  Proof preservation, incl. the bound-property/partial/model renames that
  P10-I4's drill-down must use).
- `docs/product/github-proof-design-principles-v1.md` — Principle 13
  component family (`RepositoryInput` → `GitHubInput`), landing composition,
  product component list, amendment header.
- `docs/product/github-proof-ui-system-light-dark-v1.md` — domain component
  lists, §25 minimal home composition, §40 build order, amendment header.

`docs/product/p10-presentation-language-contract.md` was inspected and
deliberately NOT changed: its only "entry point" mention is about
representative-repository "starting point" semantics, unrelated to the landing
entry surface.

No evidence, scoring, pattern, role-inference or presentation prohibition was
changed by any of these amendments.

## 4. Validation

~~~text
dotnet build (Release)                  -> 0 Warning / 0 Error
dotnet test  GitHubProof.Web.Tests      -> 145/145 PASS
dotnet test  GitHubProof.slnx (full)    -> 1448/1448 PASS
  (Domain 798, Application 246, Infrastructure 259, Web 145)
git diff --check                        -> clean
~~~

Note on how tests were run: the user's own `GitHubProof.Web` dev server held a
lock on the Debug output for the whole session, so Debug builds failed at the
file-copy step. All builds and test runs were therefore executed with
`-c Release`. The independent reviewer confirmed the same numbers
independently.

New/updated Web test coverage (83 → 145): entry classification (bare login,
canonical profile URL, canonical repository URL, scheme-less path shapes,
non-canonical URL forms, determinism, delegation-not-duplication), landing
routing (preserve-method redirect, no repository collection on a profile
shape, validation error keeps the submitted value), `/profile`
(GET performs zero analysis plus a reflection guard against reintroducing an
async GET analysis handler, POST reaches the service, raw identity passed
through unparsed, typed rate-limited/unavailable failures preserved,
non-profile shapes redirected without analyzing), and boundary icons
(inline-SVG only, one per item, text contract unchanged).

Tests that encoded the superseded separate-entry contract were updated
explicitly rather than weakened: `Repository_input_accepts_only_canonical_repository_url_scope`
→ `Github_input_accepts_the_three_unified_entry_shapes`,
`Repository_input_posts_to_real_analysis_flow` →
`Github_input_posts_to_real_analysis_flow`, plus a new
`Landing_exposes_exactly_one_entry_form_and_no_separate_profile_link`.

## 5. Independent Review

Two rounds via `github-proof-review` (forked context).

**Round 1 — PASS WITH FINDINGS** (0 BLOCKER, 0 HIGH, 3 MEDIUM, 3 LOW):
canonical doc coverage incomplete; `/profile` ran live analysis on GET;
non-canonical shapes (`owner/repo`, `github.com/owner/repo`) fell into the
profile flow and dead-ended on a flow-specific error.

All three MEDIUM findings were corrected in-session (see §2 and §3).

**Round 2 — PASS WITH FINDINGS** (0 BLOCKER, 0 HIGH, 1 MEDIUM, 2 LOW):
all three round-1 MEDIUMs confirmed closed, with no contract drift, no rule
duplication, no layer-boundary violation and no Repository Proof regression
introduced by the corrections. The reviewer independently verified the
antiforgery behavior of the 307 preserve-method redirect (Razor Pages
antiforgery tokens are not page-scoped, so the re-posted token stays valid;
and if a client degraded the 307 to a GET, `OnGet()` still starts no
analysis).

Round-2 findings:

- MEDIUM — worklog out of sync with actual Git state. **Resolved by this
  documentation step.**
- LOW — plan §13.2 described an `ArgumentException` catch that does not exist
  in `ProfileModel`. Corrected in the same step: the clause now states that
  the path is unreachable because the classifier pre-checks with the same
  canonical parser, and that no catch exists.
- LOW — CSS naming. Deferred, see below.

## 6. Known Deferred Items

- **`GITHUB_TOKEN` wording (P10-I5).** The rate-limit failure message still
  exposes the technical `GITHUB_TOKEN` environment-variable name to end users.
  The typed `RateLimited` state itself is correctly preserved and never
  collapses into a negative claim; only the user-facing wording is
  provisional. Final failure/partial presentation UX is P10-I5's
  responsibility.
- **CSS naming (LOW, P10-I6).** `_GitHubInput.cshtml` still uses the
  `.repository-input*` BEM block and `wwwroot/css/repository-input.css`,
  which now names a surface that also accepts profile identities. Left
  unchanged deliberately in this session to avoid touching the just-accepted
  visual composition; mechanical rename belongs to P10-I6.

## 7. Production Guardrails (separately pending, NOT part of P10-I2)

Unchanged by this session and still required before the Milestone B
production gate:

- a concurrency guard for profile analysis (profile analysis fans out across
  the account's admitted repositories),
- GitHub request / rate-limit telemetry.

The POST-only `/profile` correction reduces *accidental* analysis triggering
(GET, refresh-to-GET, prefetch, crawler, link preview) but is not a substitute
for either guardrail.

## 8. Next Action

~~~text
P10-I3 — Meaning / Direction / Starting Point
~~~

Commit/push/merge NOT performed (user-owned).
