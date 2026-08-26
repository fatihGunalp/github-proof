# GraphQL DecisionBounded Ordering Parity

**Session:** 128
**Date:** 2026-08-26
**Route:** Opus / high
**Result:** PASS — proof/decision session, verdict **PARITY-BLOCKED**, accepted
by the architecture owner. No authoritative GraphQL adoption for any
`DecisionBounded` surface. Test-only additions; no production change. The
independent closure review returned PASS WITH FINDINGS (0 blocker, 0 high,
3 MEDIUM, 3 LOW), all six corrected — one of them changed a verdict in the table.
The owner then **withdrew one further claim this record had made about REST
pagination**; that withdrawal is recorded below and changes no verdict.

---

## Baseline

~~~text
Branch : session/128-graphql-decision-bounded-ordering-parity
Base   : 8f42a6f  (== main == origin/main, verified before any mutation)
Working tree at start: ?? artifacts/  (user-owned, untouched)
~~~

Session 127 is **integrated as `8f42a6f`**. `CURRENT` and `INDEX` still said
"awaiting final integration" and named `09707f4` as the last integrated commit;
that was stale operational memory and is reconciled here.

---

## Verdict

~~~text
PARITY-BLOCKED
~~~

None of the four candidate surfaces can meet §19.7 from current provider
guarantees. This is not a failure of GitHub Proof: it is the accurate finding
that GitHub publishes no ordering contract for three of the four REST endpoints,
and no tie-break rule for the fourth.

All four are **ORDER-BLOCKED**, each on its own ordering evidence. Tags very
nearly is not, and the reason it still is turns out to be a property of this
repository's own collector rather than of GitHub — recorded in full below,
because it is the one thread a later session can actually pull.

| Surface | REST ordering contract | GraphQL ordering contract | Tie / prefix risk | Field parity | Ordering verdict | Adoptability |
|---|---|---|---|---|---|---|
| commits | **none published** for `GET /repos/{o}/{r}/commits` | `Commit.history` — "in the same order as git log"; **no** `orderBy` argument | prefix decides `activeDaysAtLeast`, `firstObservedCommitAt`, `spanDaysAtLeast`, and the stop page itself | FIELD-PASS | **ORDER-BLOCKED** | **NOT-ADOPTABLE** |
| pull requests | `sort` default `created`; `direction` default `desc` — **published** | `pullRequests(orderBy: {field: CREATED_AT, direction: DESC})` | equal `createdAt` at the 100/101 boundary changes prefix membership; **no tie-break published on either side** | FIELD-PASS | **ORDER-BLOCKED** | **NOT-ADOPTABLE** |
| releases | **none published** for `GET /repos/{o}/{r}/releases`; no `sort`/`direction` parameters exist | `releases(orderBy: {field: CREATED_AT\|NAME, ...})` | prefix decides `publishedAtLeast`, `publishedWithNotesAtLeast`, `draftAtLeast` | FIELD-PASS (see `publishedAt` note) | **ORDER-BLOCKED** | **NOT-ADOPTABLE** |
| tags | **none published** for `GET /repos/{o}/{r}/tags`; no `sort`/`direction` parameters exist | `refs(refPrefix:"refs/tags/", orderBy: {field: ALPHABETICAL\|TAG_COMMIT_DATE, ...})` | tag *identity* is never read, but the collector dedupes **case-insensitively**, so under a different provider prefix a case-variant pair changes the deduped observed count | FIELD-PASS | **ORDER-BLOCKED** (narrowly — see below) | **NOT-ADOPTABLE** |

---

## Exact REST ordering fact, per surface

Checked against current official documentation on 2026-08-26, not inferred:

- **pull requests** — [REST list pull requests]: `sort` — "Default: `created`. Can be
  one of: `created`, `updated`, `popularity`, `long-running`". `direction` — "The
  direction of the sort. Default: desc when sort is created or sort is not
  specified, otherwise asc." The production collector sends neither parameter, so
  it is on **created / desc**. **No tie-break rule is published** for equal
  `created_at`.
- **commits** — [REST list commits]: the documentation contains **no statement of
  ordering at all**. `sha` is documented only as "SHA or branch to start listing
  commits from."
- **releases** — [REST list releases]: the description is "This returns a list of
  releases, which does not include regular Git tags that have not been associated
  with a release." **No ordering statement, and no `sort`/`direction`
  parameters exist.**
- **tags** — [REST list repository tags]: the endpoint carries **no description and
  no ordering statement**, and offers only `per_page` / `page`.

## Exact GraphQL ordering capability, per surface

- **commits** — `Commit.history`: "The linear commit history starting from (and
  including) this commit, in the same order as git log." Arguments are `after`,
  `before`, `author`, `first`, `last`, `path`, `since`, `until` — **there is no
  `orderBy`**, so the ordering cannot even be aligned to a REST default that
  does not exist.
- **pull requests** — `IssueOrder` / `IssueOrderField` includes `CREATED_AT`;
  `OrderDirection` includes `DESC`. Tie behaviour is not documented.
- **releases** — `ReleaseOrderField`: "CREATED_AT: Order releases by creation
  time." and "NAME: Order releases alphabetically by name." Tie behaviour is not
  documented.
- **tags** — `RefOrderField`: "ALPHABETICAL: Order refs by their alphanumeric
  name." and "TAG_COMMIT_DATE: Order refs by underlying commit date if the ref
  prefix is refs/tags/." Tie behaviour is not documented.

**No provider ordering fact above was inferred from observed live behaviour, and
no tie-breaker was invented.** Where GitHub is silent, this record says GitHub is
silent. An earlier draft made one further inference, about REST pagination; the
owner rejected it and it is withdrawn below.

Sources, all checked 2026-08-26:

~~~text
REST list pull requests   https://docs.github.com/en/rest/pulls/pulls?apiVersion=2022-11-28
REST list commits         https://docs.github.com/en/rest/commits/commits?apiVersion=2022-11-28
REST list releases        https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28
REST list repository tags https://docs.github.com/en/rest/repos/repos?apiVersion=2022-11-28
REST pagination           https://docs.github.com/en/rest/using-the-rest-api/using-pagination-in-the-rest-api?apiVersion=2022-11-28
GraphQL commits           https://docs.github.com/en/graphql/reference/commits
GraphQL releases          https://docs.github.com/en/graphql/reference/releases
GraphQL git (refs)        https://docs.github.com/en/graphql/reference/git
~~~

---

## Why ordering is material at all

The collectors read a **whole page**, then ask the boundary. So the observed
prefix is the first `K x 100` items in provider order. Two orderings over the
same item set therefore produce different prefixes whenever the collection
exceeds one page — and every `DecisionBounded` evidence value is computed from
that prefix:

~~~text
commits        observedCountAtLeast, activeDaysAtLeast,
               firstObservedCommitAt, lastObservedCommitAt, spanDaysAtLeast
pull requests  totalAtLeast, mergedAtLeast, openAtLeast, closedUnmergedAtLeast
releases       publishedAtLeast, publishedWithNotesAtLeast, draftAtLeast
tags           tagsAtLeast
~~~

§15.3's "monotonicity is independent of provider ordering" is about the **stop
outcome** — that a reached maximum stays reached. It is **not** a statement that
the observed prefix, and therefore the §15.5 evidence value, is
order-independent. Reading it as the latter is the mistake this session exists to
avoid.

Throughout this record, "the adoptability bar" refers to the ten numbered
conditions in the **Session 128 brief**, not to a numbered list in `docs/`. Where
a canonical section is meant, it is cited by section number.

### §15.3 stop-effect analysis

| Surface | Rule | Can ordering change the page on which it becomes true? |
|---|---|---|
| commits | count >= 10 **and** distinct UTC days >= 8 **and** span >= 14d | **Yes.** Days and span are properties of *which* commits were seen, so a different prefix can satisfy the rule a page earlier or later. |
| pull requests | >= 2 merged | **Yes.** Which PRs land in the first 100 decides whether two merged ones are among them. |
| releases | first published release with notes | **Yes.** Same argument. |
| tags | >= 1 tag | **No.** Any non-empty first page satisfies it under every ordering; an empty one satisfies the `Enumerated` half. (The stop *outcome* is order-independent; the observed *count* is not — see below.) |

### §15.5 evidence-effect analysis

| Surface | Can ordering change an evidence value? |
|---|---|
| commits | **Yes** — every distribution value is a function of the observed set, and the observed *count* also moves when the stop page moves. |
| pull requests | **Yes** — `mergedAtLeast` and friends count within the prefix; swapping the 100th and 101st item changes them. |
| releases | **Yes** — identical argument. |
| tags | **Yes, narrowly** — not through tag identity, which is never read, but through the collector's case-insensitive dedup. See below. |

---

## Tags: proof-standard B attempted, and where it fails

Tags is the one surface where ordering *almost* stops mattering, and the near
miss is worth recording precisely, because it is the only candidate a later
session should reconsider.

**What is proven:**

1. **Tag identity is never consumed.** `ReleaseUsageSignalEvaluator` reads
   `snapshot.Tags.Items.Count`, `IsComplete` and `IsDecisionBounded`. A search
   over Domain and Application finds **no other reader**, and
   `RepositoryTag.Name` has no consumer outside the Infrastructure collector that
   creates it. No evaluator, policy, composer, projector or report section reads
   a tag name. `TagOrderingIndependenceTests` pins this for
   `ReleaseUsageSignalEvaluator`, which is the only current reader; the wider
   claim rests on the search, not on that test, and a newly added evaluator that
   read a tag name would not fail it.
2. **The stop rule is satisfied identically.** `>= 1 tag` holds for any non-empty
   first page under every ordering; a repository with no tags has an empty first
   page and no next link, so it is `Enumerated` either way.

**What is NOT proven — and this is the correction that matters:**

The step "the observed prefix size is order-independent" is **false as stated**.
`GitHubRepositoryTagCollector` accumulates into
`HashSet<string>(StringComparer.OrdinalIgnoreCase)`, while Git tag refs are
case-sensitive, so a repository may legitimately carry both `V1.0` and `v1.0`.
For a repository with more than one page of tags, whether both members of a
case-variant pair fall inside the observed prefix depends on the ordering, and
the **deduped** count — which is exactly `tagsAtLeast` — is therefore
order-dependent:

~~~text
ordering A   page 1 contains V1.0 and v1.0   -> deduped count 99
ordering B   v1.0 falls to page 2            -> deduped count 100
~~~

That is an order-dependent §15.5 evidence value, which is exactly what
proof-standard B has to exclude. Tags is therefore **ORDER-BLOCKED**.

Two things distinguish it from the other three, and both matter for the next
session:

- The blocker is **not** an absent GitHub contract. It is a property of this
  repository's own collector, and it would be closed by deduplicating tags with
  `StringComparer.Ordinal`, which is what Git ref semantics actually are.
- It is therefore also a **latent order-dependence in the existing REST path**:
  the same repository can already report a different `tagsAtLeast` if GitHub's
  undocumented tag order changes between two runs. GraphQL did not introduce it;
  proving §19.7 exposed it.

Changing that comparer alters existing REST collection behaviour and is not a
transport decision, so it is reported here rather than done. The narrowness of
the case — more than one page of tags, plus a case-variant pair split by the
prefix — is why this is recorded as a narrow blocker rather than a defect claim
about current results. The dedup behaviour itself is now pinned by test in
`GitHubRepositoryTagCollectorTests`.

---

## Withdrawn: the REST/GraphQL next-page claim

An earlier draft of this record carried an additional blocker: that
`CollectionObservationState` could diverge because REST answers "does another
page exist?" from `Link rel="next"` while GraphQL answers it from
`pageInfo.hasNextPage`, and that a collection sized at an exact page multiple
could therefore be `DecisionBounded` on one transport and `Enumerated` on the
other.

**The architecture owner rejected it, and it is withdrawn.** The reasoning did
not hold: GitHub documents `rel="next"` as the URL for the next page, and
separately documents that `rel="last"` may be absent when the last page cannot
be calculated. The second statement is about a different link relation and does
**not** establish that `rel="next"` may be absent while a real next page exists.
Inferring the divergent shape from it was the one inference this record made,
and it was not supported.

Nothing here is retained as a blocker or as a provider fact, and the withdrawal
changes no verdict: all four surfaces are ORDER-BLOCKED on their own ordering
evidence, so the final verdict remains **PARITY-BLOCKED**. It is recorded rather
than deleted so a later session does not rediscover the same unsupported
argument and mistake it for a finding.

---

## Field mapping verdict

Checked against the **actual production models**, not against §18.2's example
list.

**commits — FIELD-PASS.** `RepositoryCommit` carries only `Sha` and
`CommittedAt`; `message` and `authorLogin` are set to null by the collector.
`oid` → `sha`, `committedDate` → `commit.committer.date`, both documented.

*Structural note, not a field failure:* reaching `Commit.history` requires
resolving the default branch ref and its target — exactly the head/commit
identity §18.13 leaves unproven and Session 127 deliberately did not adopt. So
commits is blocked twice over, and a second time by the fact that REST reads
`?sha={defaultBranch}` at its own request time while GraphQL would resolve the
tip at another.

**pull requests — FIELD-PASS.** The production model is `Number`, `Title`,
`IsClosed`, `IsMerged`, `CreatedAt`. Mapping: `number`, `title`, `createdAt`,
`mergedAt != null` → `IsMerged`, and `closed` → REST `state == "closed"`.
`state` must **not** be mapped directly: GraphQL's `PullRequestState` is
`OPEN | CLOSED | MERGED`, so a merged pull request reports `MERGED` where REST
reports `closed`. **`isDraft` is not required** — the production model carries no
draft field, and §18.2 names `isDraft` for releases, not for pull requests.

**releases — FIELD-PASS for every consumed field.** The evaluator reads
`HasPublishedRelease` (`!draft`), `Notes` (`body`) **and** `IsDraft` — the last
drives both the Partial/2 branch and the `draftAtLeast` evidence value. Mapping:
`isDraft` → `draft`, `description` → `body`, `tagName` → `tag_name`, `name` →
`name`.

*Recorded ambiguity:* GraphQL documents `Release.publishedAt` as "Identifies the
date and time when the release was **created**", which is not REST
`published_at`'s semantics. `RepositoryRelease.PublishedAt` has **no consumer**
today, so this changes nothing now — but it must not be mapped as equal without
evidence if a consumer ever appears.

*Recorded eligibility note:* REST states "Only users with push access will
receive listings for draft releases." Whether the GraphQL `releases` connection
applies the identical rule is not documented, so the eligible set is equal only
under the credentials the Profile path actually uses.

**tags — FIELD-PASS.** `RepositoryTag` carries only `Name`, and even that is
unconsumed. `Ref.name` → `name`. Two properties would still need a test at
adoption time: that `refs(refPrefix: "refs/tags/")` returns the short tag name,
and that its ref set equals REST `/tags`.

---

## `totalCount`

Not used anywhere in this analysis: not to prove parity, not to bound
pagination, not to populate `RepositoryCollection.TotalCount`, not in evidence.
§18.12 is unchanged.

---

## What is safe for the next implementation session

**Nothing, for authoritative `DecisionBounded` adoption.** REST remains
authoritative for commits, pull requests, releases and tags. All four are
ORDER-BLOCKED.

**Tags remains the only candidate worth revisiting**, because part of its
blocker is closable without a GitHub contract that does not exist: the
case-insensitive tag dedup, which an owner may decide to change to `Ordinal`. It
is a change to existing REST behaviour, hence an owner decision. Even then the
REST ordering contract itself is still absent, so tags does not become adoptable
on that change alone.

Commits, pull requests and releases each need a provider guarantee GitHub does
not currently publish, and none of them should be re-attempted on the basis of
observed behaviour. Commits is additionally blocked by §18.13, since
`Commit.history` is reachable only through the default branch ref target.

---

## Changes

~~~text
docs/                 unchanged  (no canonical mutation)
src/                  unchanged  (no production change)
tests/                +1 file    TagOrderingIndependenceTests.cs (7 tests)
tests/                +1 test    GitHubRepositoryTagCollectorTests: case-variant dedup
worklog/              this record, CURRENT, INDEX
~~~

The two test additions exist because both halves of the tags analysis are claims
about this repository rather than about GitHub, and claims like that belong in
tests that fail when they stop being true: one pins that
`ReleaseUsageSignalEvaluator` reads only the tag count, the other pins the
case-insensitive dedup that makes the observed count order-dependent.

---

## Validation

~~~text
dotnet build GitHubProof.slnx -warnaserror   0 Uyarı, 0 Hata
dotnet test  GitHubProof.slnx                2247/2247, 0 skipped
git diff --check                             PASS
docs/                                        unchanged
src/                                         unchanged
artifacts/                                   untouched
~~~

No live GitHub API call was made. Every provider fact above comes from official
documentation, quoted in place; no live ordering observation was used, because
observed order is not a contract.

---

## Not done

- **No authoritative `DecisionBounded` GraphQL adoption.** None was implemented,
  wired or registered.
- No production transport or composition change.
- No canonical `docs/` amendment. §15, §18.2, §18.12 and §19.7 are untouched.
- No live A/B, no production enablement, no commit, no push.

---

## Known risks and carried facts

- Session 127's authoritative metadata / topics / languages envelope remains
  **DEFAULT OFF** with no production, beta, default-on or live-A/B approval.
- The Session 126 secondary-rate-limit residual remains a **separate** enablement
  blocker and is unaffected by this session.
- The REST `homepage` vs GraphQL `homepageUrl` provider-value question from
  Session 127, together with `primaryLanguage` and `description`, remains part of
  the same rollout validation surface.
- The tag-dedup order-dependence above is a **latent property of the existing
  REST path**, not something GraphQL introduced. It changes no current verdict
  and no current result, and it is recorded so a later session does not treat
  tags as ordering-safe.

---

## Checklist

~~~text
[x] Precondition verified against Git before any mutation
[x] Session 127 reconciled as integrated (8f42a6f)
[x] Official GitHub documentation checked per surface, quoted in place
[x] §15.3 stop-effect and §15.5 evidence-effect analysed per surface
[x] Field mapping checked against the actual production models
[x] Tags analysis pinned by test on both sides — identity unread, dedup
    order-dependent
[x] No canonical mutation, no production change, no adoption
[x] Build 0 warnings, full suite green
[x] Independent closure review — PASS WITH FINDINGS (0 blocker, 0 high,
    3 MEDIUM, 3 LOW); all six corrected under the accelerated-workflow rule
[x] Architecture-owner acceptance — PARITY-BLOCKED ACCEPTED; the unsupported
    REST pagination claim withdrawn on owner direction, no verdict changed
~~~

---

## Independent closure review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-26
Outcome:  0 BLOCKER, 0 HIGH, 3 MEDIUM, 3 LOW — none blocking
~~~

The reviewer re-ran the build and suite, and independently re-checked all four
REST documentation claims against live GitHub documentation, confirming each
quotation and confirming that no tie-breaker was invented and no provider fact
was asserted without a source. It also verified the field-mapping conclusions
against the production models — including that pull requests need no `isDraft`
and that GraphQL `closed`, not `state`, is the correct mapping onto REST
`closed` — confirmed that `docs/` was not amended, and confirmed that
PARITY-BLOCKED rather than PARITY-PARTIAL is the correct verdict.

| # | Sev | Finding | Correction |
|---|---|---|---|
| F-1 | MEDIUM | **The tags ORDER-PASS proof had a hole.** `GitHubRepositoryTagCollector` deduplicates with `StringComparer.OrdinalIgnoreCase` while Git tag refs are case-sensitive, so for a repository with more than one page of tags the deduped prefix size — and therefore `tagsAtLeast` — is order-dependent after all. | **CORRECTED, and it changed a verdict.** Tags is now **ORDER-BLOCKED**, the table row, the two effect tables, the proof section and the next-session section all say so, and the finding is recorded as a latent order-dependence in the existing REST path rather than something GraphQL introduced. Two tests in `GitHubRepositoryTagCollectorTests` pin the dedup behaviour. The final verdict, PARITY-BLOCKED, is unchanged. |
| F-2 | MEDIUM | The record claimed `TagOrderingIndependenceTests` would fail if any future signal read a tag name; it only exercises `ReleaseUsageSignalEvaluator`. | **CORRECTED.** Both the record and the test file now state the scope exactly: the test pins the property for the only current reader, and the wider no-reader claim rests on the recorded source search. |
| F-3 | MEDIUM | "canonical-bar items 4 and 6" pointed at no resolvable numbered list in `docs/`. | **CORRECTED.** Both occurrences now name the Session 128 brief's adoptability conditions explicitly, and a note distinguishes them from canonical section numbers. |
| F-4 | LOW | The next-page argument was supported by a `rel="last"` sentence while making a claim about `rel="next"`, under a blanket "nothing was inferred". | **CORRECTED, then SUPERSEDED.** The record first quoted the more apposite sentence and labelled the paragraph as an inference; the architecture owner subsequently rejected the argument outright, and the whole claim is withdrawn above. The reviewer had identified the right weak point. |
| F-5 | LOW | "Only `HasPublishedRelease` and `Notes` are read" omitted `IsDraft`, which drives the Partial/2 branch and `draftAtLeast`. | **CORRECTED.** |
| F-6 | LOW | Citations were labels without URLs, which is the opposite of what a "checked, not inferred" record needs. | **CORRECTED.** All eight documentation URLs are now listed with the date checked. |

F-1 is the finding that justifies having run the review at all: the session's one
positive result was wrong, and the corrected record says so plainly rather than
retaining a verdict it could not support.

---

## Git

~~~text
No commit. No push. HEAD == 8f42a6f.
Suggested commit (user-owned):

test(profile): pin tag ordering independence for DecisionBounded parity
~~~
