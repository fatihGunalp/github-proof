# GraphQL Partial-Data / Error Contract (S0)

**Session:** 123
**Date:** 2026-08-25
**Route:** Opus / xhigh
**Result:** PASS — canonical contract locked; independent review returned
CHANGES REQUIRED, the architecture owner accepted all six findings, and all six
were closed in the same session and re-reviewed

---

## Goal

Close the blocking **GQL-F2** contract gap Session 121 identified, so that a
Hybrid GraphQL implementation slice becomes possible at all. This is the **S0**
amendment Session 121 authorized and Session 122 left queued; the architecture
owner explicitly prioritized it ahead of the Profile Production Runtime Decision
Design.

The gap, restated: `GitHubApiAvailability.FromFailureResponse` resolves failure
from the HTTP status alone, and GitHub GraphQL can answer **HTTP 200 with both
`data` and `errors`**. No canonical section mapped that onto `DataAvailability`,
so today a partial GraphQL response would reach the domain as success and could
silently become positive developer evidence.

---

## Starting State

~~~text
Branch       session/123-graphql-partial-data-error-contract
Base         5044b03  (== main == origin/main, verified with git rev-parse)
             5044b03 = "docs(worklog): close REST redundancy audit" (Session 122)
Working set  clean
Pre-existing ?? artifacts/  (user-owned, untouched, out of scope)
~~~

Precondition PASS: the declared baseline matched Git exactly and Session 122 was
already integrated, so this session did not stack on an unclosed predecessor.

---

## Model Route

Opus / xhigh. This is a canonical `docs/` mutation that writes GitHub Proof's
failure and availability truth for a second transport — a wrong clause becomes
canonical, and the specific failure mode being closed is silent conversion of a
provider error into positive evidence. Session 121, which only *analysed* the
same subject, ran at xhigh; writing the contract that resolves its blocker is not
lower risk.

---

## Context Read

~~~text
AGENTS.md
CLAUDE.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
docs/architecture/public-profile-runtime-collection.md   (full structure;
                                                          §§1-17 in detail for
                                                          §§6, 7, 9, 9.1, 10.4,
                                                          11, 11.1-11.3, 12, 13,
                                                          14, 15.3-15.5, 17)
worklog/CURRENT.md
worklog/sessions/2026-08-25-121-graphql-request-consolidation-feasibility.md
worklog/sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md
~~~

Sessions 121 and 122 were read as history only. No production source was read or
modified in this session; the source-level facts this contract relies on were
established and independently reviewed in those two sessions.

---

## The exact contract decision

Encoded as **§18** (thirteen subsections) and **§19** (seventeen acceptance
criteria) in `docs/architecture/public-profile-runtime-collection.md`, plus
Session 123 notes in §9.1, §11 and §13 so no older section reads as complete on
its own. §18.12, §18.13 and §19.15-§19.17 were added by the owner-accepted
review closure below; the original twelve decisions are unchanged.

| # | Owner's decision | Encoded in |
|---|---|---|
| 1 | GraphQL is transport optimization only; it may never change Profile/Repository evidence semantics | §18.1 |
| 2 | Hybrid V1 surface split — GraphQL candidates vs REST-retained | §18.2 |
| 3 | Do not adopt GraphQL `isEmpty` | §18.3 |
| 4 | HTTP 200 with `errors` may never become successful absence, `Missing`, zero evidence or developer truth | §18.4 |
| 5 | Error isolation: attributable → per-surface REST fallback; unaffected retained only if complete and independently attributable; ambiguous → discard the whole repository envelope | §18.5 |
| 6 | Envelope-level / provider-wide failure disables GraphQL for the rest of the execution; no per-repository re-attempt of the same failing envelope | §18.6 |
| 7 | REST fallback is not an availability failure; availability belongs to the collector that actually supplied the surface; no new `DataAvailabilityStatus`, no GraphQL-specific evidence status | §18.7 |
| 8 | Both transports failing → existing REST/provider availability semantics authoritative; never convert failure to `Missing` | §18.8 |
| 9 | Cancellation / deadline / budget protection stay governed by §§10-11 and are never reinterpreted as GraphQL errors | §18.8 |
| 10 | Telemetry may record bounded structural facts only; never query text, identity-bearing variables, response data, contents, tokens or raw error messages | §18.9 |
| 11 | §6 semantic-equivalence and §7 no-hidden-cap preserved | §18.10 |
| 12 | No concurrency, deadline, reserve, cache, queue, repository cap, batch size or performance claim selected | §18.11 |
| — | Explicit acceptance criteria sufficient for the first implementation slice | §19 (1-14) |

### Three judgements the encoding makes explicit

These follow from the owner's decisions rather than adding to them, and each is
called out in the contract text so a later reader cannot mistake them for
implementation latitude.

**Keeping `issues` on REST removes the only known evidence-value divergence.**
Session 121 §4.3(a) recorded that REST `/issues` interleaves pull requests and
filters them client-side after the page is fetched, while a GraphQL `issues`
connection never contains them — so under the same §15.3 stop rule the two
transports observe different prefixes and produce different §15.5
`DecisionBounded` evidence values. That divergence was the one item Session 121's
review forced out of "pure parity" and into owner acknowledgement. The owner's
split retains issues on REST, so the GraphQL column now carries **no** known
evidence-value divergence. §18.2 records that with its reason.

**Ordering parity is still owed per adopted surface.** Commits, pull requests,
releases and tags are `DecisionBounded`, so the observed *prefix* decides. Being
listed as a GraphQL candidate is permission to attempt adoption, not a finding
that ordering parity holds; §18.2 states this and §19.7 makes it a gate. Session
121 left GraphQL ordering-enum values explicitly unverified, and nothing in this
session verified them.

**Ambiguity is the default in §18.5.** The three cases are ordered, and an
implementation that cannot prove case 1 or case 2 is in case 3. The conservative
outcome is required wherever both readings are available, which is what makes
§18.4 enforceable rather than aspirational.

---

## Owner decision and F1-F6 closure

The architecture owner **accepted all six findings** and directed that they be
closed in this session. Each was closed by an additive rule; no existing clause
was deleted, weakened or duplicated, and §§1-17 remain untouched.

| # | Owner ruling | Amendment |
|---|---|---|
| **F1** | GraphQL `totalCount` and equivalent exact-total fields must never populate `RepositoryCollection.TotalCount`, enter `DecisionBounded` evidence values, or be presented as an exact repository total. Existing enumeration / `DecisionBounded` semantics stay authoritative. | **§18.12 NEW — "Exact-total provider fields are not adopted."** Binds §17.4, §15.5 and §15.4 explicitly and names the pattern as the same one §18.3 rejects for `isEmpty`: a cheaper, more precise provider field becoming an evidence-strength upgrade. **§19.15 NEW** is the matching test. |
| **F2** | `defaultBranchRef == null` must not be read blindly as successful empty evidence. The head/commit-tree and commits surfaces may be adopted only if the GraphQL empty shape provably reproduces REST successful-empty semantics **and** stays distinguishable from failure; otherwise they stay or fall back to REST. No new vocabulary. | **§18.13 NEW — "Empty-repository shape."** States why §18.4 alone does not close this path: §18.4's trigger is the presence of `errors`, and an empty-repository response carries none. **§19.16 NEW** requires the positive **and** negative test. |
| **F3** | One physical GraphQL POST counts exactly once in the total request count and exactly once in a closed bounded GraphQL category — never once per surface served. Per-surface served/fallback facts are separate, non-additive, and are not HTTP request counts. REST and GraphQL budgets stay separate. | **§18.9 — "Counting rule (locked)"** added, with the reason stated: per-surface counting would make §9's "total GitHub HTTP request count" untrue and the existing measured baseline incomparable. **§19.11 extended** with the counting assertions. The existing separate-budgets paragraph was not duplicated. |
| **F4** | Restore Session 121's field-mapping obligation; explicit mapping tests for every adopted GraphQL field; a surface whose mapping parity is not proven stays REST. | **§18.2 — "Mapping obligation"** paragraph added next to the existing ordering obligation, enumerating the seven minimum mappings. **§19.17 NEW** is the matching criterion. |
| **F5** | One successful envelope captures one UTC instant, taken only after the body is fully read, parsed, validated and accepted; every surface that envelope supplied shares it; a fallen-back surface uses the REST collector's own. Wall-clock differences between separate executions are not semantic drift and must not be asserted equal. | **§18.7 — "`RetrievedAt` for a GraphQL envelope"** added. **§19.2 amended** to exclude wall-clock equality from parity while still asserting the shared-instant and fallback rules, and to keep key/status/reason/data-key equality intact. |
| **F6** | REST composition stays the reference/fallback path; before any production switch, run the approved live A/B parity validation on the same three Session 120 profile shapes, compare under the canonical parity contract, and require owner approval on that evidence. No performance target or claim. | **§19.14 rewritten** from "hybrid is switchable off" into a rollout gate carrying all four conditions, with an explicit statement that no performance target, threshold or claim attaches to it. |

Two mechanical consequences of the closure were also applied: the header
amendment line now lists the added subsections, and §18.11 gained one line
stating that its "selects nothing" exclusion covers §18.12 and §18.13 as well,
so it cannot be misread as closing the section.

---

## Changes

~~~text
Modified:
- docs/architecture/public-profile-runtime-collection.md
    header    Session 123 amendment line
    §9.1      note: the prohibition list is not exhaustive once a second
              transport exists; §18.9 adds the GraphQL prohibitions
    §11       note: §11 stays authoritative for every failure on any transport;
              §18.4-§18.8 add the GraphQL mapping and introduce no new status
    §13       scope note: §18 authorizes a design and acceptance criteria only
    §18       NEW — GraphQL Transport Optimization (18.1-18.13)
    §19       NEW — Session 123 Acceptance Criteria (1-17)

Created:
- worklog/sessions/2026-08-25-123-graphql-partial-data-error-contract.md

Also modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Production code, tests, packages, migrations, artifacts/: UNCHANGED.
~~~

No existing canonical statement was deleted, renumbered or weakened. §§1-17 keep
their numbering and their text; the three notes are additive and each points at
§18 rather than restating it.

---

## Decisions

~~~text
- §18 appended as a new section rather than edited into §11, so §§1-17 keep
  their numbering and Session 098/107 amendments stay legible. §9.1, §11 and §13
  carry pointers so no older section reads as complete on its own.
- §19 follows the §14/§17 precedent: acceptance criteria live in their own
  numbered section, additional to and not superseding the earlier ones.
- Issues kept on REST is recorded with its evidence-value reason, not merely as
  a list entry, because it is what makes the GraphQL column divergence-free.
- Ordering parity written as an obligation on adoption (§18.2) and a gate (§19.7)
  rather than an assumption, because Session 121 left the ordering enums
  unverified.
- The §18.6 run-scoped disable is stated as invisible in output by construction:
  since §6 requires both transports to agree, a result that changes across the
  switch is a §6 violation, not a licence granted by §18.6.
- §19.14 (hybrid switchable off, REST composition as reference path) is included
  because §19.1 parity cannot be demonstrated without both compositions being
  runnable; it selects no runtime value.
- No provider fact was asserted in this session. Every provider-behaviour claim
  is a citation of Session 121, which recorded its sources and left three schema
  facts explicitly unverified.
~~~

---

## Validation

~~~text
Baseline precondition (main @ 5044b03, branch, clean tree)   PASS
Canonical structure intact (§§1-17 numbering unchanged)      PASS
No canonical statement deleted or weakened                   PASS
12 owner decisions mapped 1:1 to encoded sections            PASS
Acceptance criteria cover all 12 decisions                   PASS
No new DataAvailabilityStatus / evidence status introduced   PASS
No runtime numeric, concurrency, cache or performance claim  PASS
dotnet build / dotnet test                                   NOT RUN — no code
                                                             changed in scope
git diff --check                                             PASS
git status --short                                           docs + worklog only,
                                                             plus pre-existing
                                                             artifacts/
~~~

---

## Problems

~~~text
- The two blocking review findings (F1 `totalCount`, F2 empty-repository
  detection) were owner-accepted and are now closed by §18.12, §18.13, §19.15
  and §19.16. Both were Session 121 pre-switch conditions this draft had
  dropped, and both were the §18 error class itself.
- Root cause of F1, F2 and F4: this session encoded the owner's 12 decisions
  faithfully but did not systematically re-check them against Session 121's
  already-accepted pre-switch condition list. Three of those eleven conditions
  were dropped.
- The draft's own §19.7 ordering-parity obligation depends on GraphQL ordering
  semantics Session 121 left unverified and this session did not verify. That
  one is deliberate: it is stated as a gate rather than resolved, which is the
  correct disposition for a contract session with no live provider access.
~~~

---

## Not Done

~~~text
- No production code, tests, packages or migrations.
- No GraphQL client, query, schema file or DI wiring.
- No benchmark and no live GitHub call of any kind.
- No runtime value selected: no concurrency, deadline, reserve, cache, queue,
  repository cap, batch size, retry or credential type.
- Profile Production Runtime Decision Design (D-1, D-2, D-3, D-5) not started —
  moved to the next session by explicit owner prioritization of S0.
- The Session 122 §6.1 content-selector duplicate-fetch defect remains open and
  untouched.
~~~

---

## Known Risks

~~~text
- §18.5 case 2 ("retain unaffected surfaces") is the clause most likely to be
  implemented optimistically. It is written with two conjunctive conditions and a
  default-to-case-3 rule for exactly that reason, and §19.4 gates it, but it
  remains the highest-risk clause in the amendment.
- §18.2's GraphQL column is a candidate list. If an implementer reads it as a
  settled parity finding, the ordering obligation is the thing that gets skipped.
  §18.2 and §19.7 both say so; a reviewer should still check for it.
- Adopting GraphQL still makes an authenticated credential mandatory on the code
  path, which remains a D-5 input recorded in Session 121. §18.11 selects no
  credential type, and §10.4 remains authoritative and unchanged.
- Provider behaviour can change; every provider claim here is inherited from
  Session 121 with its 2026-08-25 source dates.
~~~

---

## Independent Review

~~~text
Result:   CHANGES REQUIRED
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Scope:    the §18/§19 amendment, the three Session 123 notes, CURRENT, INDEX
Outcome:  2 HIGH blocking, 3 MEDIUM, 2 LOW
Status:   SUPERSEDED — the owner accepted all six and they were closed in
          this session; see "Owner decision and F1-F6 closure" above
~~~

The reviewer confirmed the amendment's body: `git diff --numstat` shows **337
insertions, 0 deletions**; §§1-17 keep their numbering and text; all 12 owner
decisions map faithfully to §18.1-§18.11 with no broadening, narrowing or
softening; the three additive notes point at §18 rather than restating it; every
prohibition holds; and CURRENT/INDEX match Git. It also closed four of the gap
candidates it was asked to probe: the §18.6 disable is already execution-scoped
so it cannot collide with §10.1; cross-transport pagination continuation is
already forbidden by "one surface, one transport, per repository, per run";
`SnapshotIdentity` is unaffected because §18.2 keeps both of its inputs on REST;
and §18.9's control-flow/telemetry split closes the raw-error-text path.

Two blocking findings survive, and both have the same shape: **a Session 121
pre-switch condition the architecture owner had already accepted, which this
draft dropped.** Both were re-verified against source before being accepted here.

| # | Sev | Finding |
|---|---|---|
| F1 | **HIGH — blocking** | **GraphQL `totalCount` is not gated.** Session 121 condition 5 required that a `DecisionBounded` collection still carry `TotalCount = null` and that GraphQL `totalCount` is **never** written into `TotalCount`. Session 121 recorded `totalCount` as present on the very connections §18.2 adopts (commits, pull requests, releases, tags). §17.4 and §15.5 already forbid it and §18.1/§18.10 bind them, but §19 carries no criterion — while `isEmpty`, the same "a more accurate provider field becomes an evidence-strength upgrade" pattern, got both §18.3 and §19.9. |
| F2 | **HIGH — blocking** | **Empty-repository detection is not re-proven for the adopted surfaces.** `GitHubRepositoryCommitCollector` maps HTTP 409 carrying the exact body `"Git Repository is empty."` to successful absence and **every other 409 to `Failed`**; the contents collector does the same for the tree. §18.2 adopts commits and default-branch head/commit/tree identity. GraphQL has no 409 — an empty repository answers `defaultBranchRef: null`, collapsing that distinction. §18.4 does not close it, because its trigger is the presence of `errors` and this response carries none. Session 121 condition 7 required exactly this test. This is the §18 error class itself: a null becoming successful absence. |
| F3 | MEDIUM | **§18.9 per-surface attribution is not decidable.** `ProfileGitHubRequestCategory.Classify` derives its category from the request URI inside an `HttpMessageHandler`; one `POST /graphql` envelope can serve up to seven surfaces, so URI-based per-surface attribution is structurally impossible. §18.9 authorizes extending the closed vocabulary but decides neither the new members nor whether one envelope counts as 1 or N against §9's "total GitHub HTTP request count". |
| F4 | MEDIUM | **Field-mapping parity criteria dropped.** Session 121 condition 4 enumerated the exact mappings (`committedDate`, `closed`/`mergedAt`, `description` to `Notes`, `isDraft`, `LanguageEdge.size`, topic normalization, `defaultBranchRef.target.tree.oid`). §19.1/§19.2 would catch a mismatch only if a fixture happens to exercise the field — evidence by luck, not by proof. |
| F5 | MEDIUM | **GraphQL-served `RetrievedAt` semantics undecided.** `DataAvailability.RetrievedAt` is evidence-facing: signal evaluators read it as `observedAt`. REST produces seven independent timestamps; one envelope naturally produces one shared timestamp. §18.7 assigns ownership but not semantics. It does not break §6, which does not compare it — but it is undecided and it touches evidence. |
| F6 | LOW | **§19.14 is the one criterion not traceable to the 12 decisions**, and it introduces a production switch while Session 121 condition 11 — a live A/B parity run before any production switch — was dropped. |
| F7 | LOW | **Lifecycle: the packet declared PASS/CLOSED before its own review ran**, repeating the pattern Session 121's review already flagged. |

**F7 was corrected immediately**, because it was a purely mechanical
contradiction in what this session authored and, once the review returned
CHANGES REQUIRED, an actively false statement. For the duration of the review
`CURRENT.md`, `INDEX.md` and this record reported Session 123 as
**IN_PROGRESS** with `Active Session: 123`. **Now superseded:** after the
owner-accepted closure they report **PASS** with `Active Session: NONE`.

**F1-F6 were not applied at review time.** Closing F1 and F2 meant adding
canonical clauses beyond the twelve decisions the architecture owner had
enumerated, and F3-F5 each required a decision this session was not authorized
to make — a telemetry counting rule, a field-mapping criteria set and a
timestamp semantics rule. Recording them and stopping was the correct
disposition at that point; inventing a thirteenth decision would not have been.

**Superseded.** The architecture owner then accepted all six and directed their
closure with explicit rulings. What each amendment does is recorded in "Owner
decision and F1-F6 closure" above, and the closure was re-reviewed
independently.

### Suggested minimum closure, for the owner's decision — ACCEPTED IN FULL

1. A §18 subsection gating GraphQL `totalCount`, and any equivalent exact-total
   field, out of `TotalCount` and out of every evidence value — on the same
   grounds as §18.3 — with a matching §19 criterion.
2. A §18 subsection requiring empty-repository detection to be reproduced
   exactly, distinguishing `defaultBranchRef == null` from failure as REST's 409
   body distinguishes it, for every adopted GraphQL surface — or the surface
   stays REST; with a matching §19 criterion.
3. Optionally in the same pass: the §18.9 counting rule (F3), the field-mapping
   criteria (F4), the `RetrievedAt` rule (F5), and whether §19.14 keeps the
   switch without Session 121's live A/B condition (F6).

---

## Next

Architecture-owner final acceptance of the locked amendment. All six review
findings are closed, so **S0 is complete** and the first GraphQL implementation
slice becomes possible under §19's seventeen criteria; Session 121's S1
(transport client behind its own interface, no collector wired, default off)
remains the smallest slice consistent with them.

The **Profile Production Runtime Decision Design** (D-1, D-2, D-3, D-5) is the
next session by owner prioritization. The Session 122 §6.1 content-selector
duplicate-fetch defect still awaits its own bounded session.

---

## Git

~~~text
Suggested commit:
docs(profile): lock GraphQL transport partial-data and error contract

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Baseline and branch verified against Git before any mutation
[x] Canonical docs + worklog only; no code, test, package or migration
[x] All 12 owner decisions encoded, each traceable to a section
[x] Acceptance criteria written for the first implementation slice (§19)
[x] §6 and §7 invariants explicitly preserved
[x] No new availability or evidence status; no runtime numeric selected
[x] No performance, latency or wall-clock claim
[x] Older sections given pointers so none reads as complete on its own
[x] Independent review of this packet — CHANGES REQUIRED (2 HIGH, 3 MEDIUM,
    2 LOW)
[x] F7 corrected — the packet no longer declared PASS/CLOSED before its review
[x] Architecture-owner accepted F1-F6
[x] F1-F6 closed additively: §18.2, §18.7, §18.9, §18.12, §18.13, §19.2,
    §19.11, §19.14, §19.15-§19.17; §§1-17 untouched
[x] Re-review after closure — PASS WITH FINDINGS (3, all worklog-only:
    two stale counts/statements and one duplicated paragraph), all corrected
[ ] Architecture-owner final acceptance of the locked amendment
~~~
