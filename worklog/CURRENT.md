# GitHub Proof — Current Development State

**Last Updated:** 2026-08-24 (Session 119)
**Source of Truth:** `docs/`. This file is operational memory only — it never
carries product, domain or architecture truth.

## Now

- **Active Session:** NONE.
- **Last Session:** 119 — Repository Proof Visual Alignment — **PASS**.
  Branch `session/119-repository-proof-visual-alignment`, base `edb35fd`,
  route Opus / high.
  Detail: [`sessions/2026-08-24-119-repository-proof-visual-alignment.md`](./sessions/2026-08-24-119-repository-proof-visual-alignment.md)
- **Last integrated commit on `main`:** `edb35fd` (Session 118).
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
- Next product work: **Profile Runtime Performance & Budget**, then the
  **Milestone B production gate**.

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

## Exact Next Action

Open a new model gate for Profile Runtime Performance & Budget, or for the
Milestone B production gate — architecture owner's choice of order.
