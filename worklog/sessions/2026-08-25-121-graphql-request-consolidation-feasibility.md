# GraphQL Request Consolidation — Feasibility & Architecture Decision

**Date:** 2026-08-25
**Result:** PASS — read-only feasibility; verdict **GQL-F2**
**Scope:** docs/analysis (no production code, no canonical contract mutation)
**Model Route:** Opus / xhigh
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Determine whether Profile V1 can obtain **the same canonical observable truth**
with materially fewer GitHub HTTP round trips, using a bounded GraphQL + REST
composition, and record the exact conditions under which that would be
permitted.

This session owns **feasibility evidence and an architecture verdict only**. It
implements nothing, selects no production value, issues no GraphQL request, adds
no dependency and changes no canonical contract.

This record is historical evidence. It is not a canonical contract. The
canonical owner of Profile runtime collection remains
`docs/architecture/public-profile-runtime-collection.md`.

---

## Starting State

~~~text
Branch:
session/121-graphql-request-consolidation-feasibility

Base HEAD:
59a7c30

Pre-existing Working Tree:
?? artifacts/   (user-owned, out of scope, untouched)

Relevant Previous Session:
worklog/sessions/2026-08-24-120-profile-runtime-performance-budget.md
(historical measurement evidence only)
~~~

### CURRENT reconciliation

At session start `CURRENT.md` still reported Session 120 as *awaiting
architecture-owner verification* and `5599291` as the last integrated commit,
while Git showed `59a7c30` (the Session 120 work) on `main`. The architecture
owner confirmed Session 120 is verified and merged. `CURRENT.md` and the Session
120 `INDEX` row were reconciled to that verified state; no history was rewritten
and no Session 120 content was altered.

---

## Model Route

~~~text
Selected:
Opus / xhigh

Why:
Semantic-parity proof across 15+ evidence surfaces, provider partial-failure
mapping and separate rate-limit accounting. A wrong parity claim would
propagate into canonical Profile truth and the Milestone B production gate.

User Approval:
devam et
~~~

---

## Context Read

~~~text
AGENTS.md, CLAUDE.md, agents/02-context-loading.md, agents/07-worklog-integration.md
docs/architecture/public-profile-runtime-collection.md   (canonical, full)
docs/roadmap/deployment-milestones.md                    (§2, §3, Milestone B)
worklog/CURRENT.md
worklog/sessions/2026-08-24-120-...-performance-budget.md (historical evidence)

Source re-verified at 59a7c30:
  Infrastructure/GitHub/*                (11 collectors + orchestrator)
  Infrastructure/GitHub/Profile/*        (profile metadata / manifest / source)
  Application/Profile/ProfileRepositoryAnalysisOrchestrator.cs
  Application/Profile/CrossRepositoryEvidenceComposer.cs
  Domain/Analysis/Signals/*              (requiredDataKeys + evidence values)
  Domain/Analysis/Collection/RepositoryCollectionDecisionBoundary.cs
  Domain/Analysis/Models/RepositorySnapshotData.cs (RepositoryCollection)
  Domain/Profile/AnalysisAdmissionPolicy.cs, Models/ProfileRepository*.cs
  Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
  Web/Profile/Runtime/ProfileGitHubRequestCategory.cs, ...TelemetryHandler.cs

Provider documentation (fetched 2026-08-25) — see §7 for the source list.
~~~

---

## 1. Current REST request graph — re-verified at `59a7c30`

Re-derived from source, not copied from Session 120. The formula and the
measured category table agree exactly.

### 1.1 Profile level

| # | Request | Source |
|---|---|---|
| 1 | `GET users/{login}` | `GitHubProfileMetadataCollector` |
| 2..P | `GET users/{login}/repos?type=owner&sort=full_name&direction=asc&per_page=100` | `GitHubProfileRepositoryCollector`, bounded at `DefaultMaxPageCount = 10` |

Admission (`AnalysisAdmissionPolicy`) consumes **only** manifest fields —
`Empty` (derived from `size` + `pushed_at`) and `Fork`. **Admission is already
strictly before any repository collection**, so the execution-order invariant is
satisfied by construction today.

### 1.2 Per admitted repository — Profile V1 composition (`CreateProfileComposition`)

Sequential, one `HttpClient`, no concurrency, no cache, no retry.

| # | Request | Collector | Feeds |
|---|---|---|---|
| 1 | `GET repos/{o}/{r}` | Metadata | metadata, topics, license key/name/spdx, identity, default branch, fork/archived/template |
| 2 | `GET repos/{o}/{r}/git/ref/heads/{branch}` | Contents | head commit SHA |
| 3 | `GET repos/{o}/{r}/git/commits/{sha}` | Contents | root tree SHA |
| 4 | `GET repos/{o}/{r}/git/trees/{treeSha}?recursive=1` | Contents | full file tree + `truncated` |
| 5..N | `GET repos/{o}/{r}/contents/{path}?ref={sha}` | Contents | project files, workflows, docs Markdown (≤16) — **unbounded for the first two** |
| — | `GET repos/{o}/{r}/readme` | Descriptive | preferred README (path + content) |
| — | `GET repos/{o}/{r}/license` | Descriptive | license metadata **+ file path + size** |
| — | `GET repos/{o}/{r}/languages` | Descriptive | language → bytes |
| — | `GET .../commits?sha={branch}&per_page=100` (×Pc) | Commits | sha + `commit.committer.date` |
| — | `GET .../issues?state=all&per_page=100` (×Pi) | Issues | PR entries filtered out client-side; skipped entirely when `has_issues=false` |
| — | `GET .../pulls?state=all&per_page=100` (×Pp) | Pull requests | number/title/state/`merged_at`/created |
| — | `GET .../releases?per_page=100` (×Pr) | Releases | tag_name/draft/name/body/published_at |
| — | `GET .../tags?per_page=100` (×Pt) | Tags | name |

~~~text
Total = 1 + P + Σ_admitted [ 7 + Npf + Nwf + min(Ndoc,16) + Pc + Pi + Pp + Pr + Pt ]
Floor  = 12 per admitted repository (11 when Issues is disabled)
~~~

Measured (Session 120, historical): 12.22–15.25 requests per admitted repository;
the fixed component was exactly 7 × admitted in every run.

### 1.3 Decision-sufficient stop rules in force (§15)

Only the Profile composition supplies boundaries; Repository Proof supplies none
and therefore still enumerates fully.

| Surface | Stop predicate (source) | State |
|---|---|---|
| commits | `CommitHistory.IsStrongThresholdReached(count)` **and** `CommitDistribution.IsStrongThresholdReached(activeDays, span)` | `DecisionBounded` |
| issues | `IssueUsage.IsTerminalThresholdReached(observedCount)` | `DecisionBounded` |
| pull requests | `PullRequestUsage.IsTerminalThresholdReached(mergedCount)` | `DecisionBounded` |
| releases | `ReleaseUsage.IsTerminalThresholdReached(publishedWithNotesCount)` | `DecisionBounded` |
| tags | `ReleaseUsage.IsTagContributionPinned(tagCount)` | `DecisionBounded` |

Every stop is gated on `page.NextPage is not null` — i.e. it fires only when a
further page demonstrably exists. `RepositoryCollection` enforces
`Enumerated ⇔ IsComplete` and forbids a non-null `TotalCount` on a
`DecisionBounded` collection.

---

## 2. Milestone B impact

GraphQL is **not** a Milestone B exit criterion
(`deployment-milestones.md` Milestone B — Production Gate). It must not become a
prerequisite. If it is adopted **before** the beta release, these gate items
require re-validation, because a GraphQL transport touches each of them:

| Milestone B gate item | Why it must be re-validated |
|---|---|
| GitHub rate-limit behaviour defined | GraphQL uses a separate `graphql` resource with point accounting; the REST core bucket is no longer the whole picture |
| Partial GitHub API failure behaviour defined | HTTP 200 + partial `data` + `errors` is a failure mode the current availability model has never seen (§8) |
| Profile timeout/failure UX ready | GitHub terminates >10 s GraphQL queries with 502/504 — a new terminal shape inside the deadline window |
| Production telemetry can observe Profile flow | `ProfileGitHubRequestCategory` classifies by REST route shape; `POST /graphql` falls into `other` and the closed 17-member vocabulary would need an authorized extension |
| Deterministic replay PASS | The profile `SnapshotIdentity` hashes manifest content; the manifest source would change |
| Varied profile validation PASS | Full re-run against the validation profile set |
| Multiple real-user production smoke PASS | Re-run |
| Remote production verification PASS | Re-run |

Additionally: **the beta credential decision (D-5) becomes more constrained.**
Canonical §10.4 already rules out an unauthenticated production Profile beta for
the measured request surface, so authentication is not a new production
requirement introduced here. The delta is narrower and twofold. First, the
*code path* currently makes `GITHUB_TOKEN` optional and tolerates unauthenticated
REST at 60/hour, whereas GitHub documents GraphQL access only through an
authenticated identity (personal access token, GitHub App or OAuth app) and
publishes no unauthenticated point budget — that tolerance would disappear.
Second, §10.4's existing requirement that the selected credential be verified
compatible with **every used endpoint** would newly have to cover
`POST /graphql`. No credential type is selected here.

**Recommendation:** do not gate the Milestone B beta on this work. Treat it as a
post-beta or parallel-track performance architecture with its own production
gate.

---

## 3. Verified provider facts (source + date)

All fetched from `docs.github.com` on **2026-08-25**. Numeric provider limits are
provider facts that can change; they are recorded with their source, not
canonicalized.

| Fact | Value | Source |
|---|---|---|
| Connections require `first`/`last` | required; **values must be within 1–100** | Rate limits and query limits for the GraphQL API |
| Node limit | "Individual calls cannot request more than 500,000 total nodes" | same |
| Primary rate limit | 5,000 points/hour per user or App installation (10,000 for Enterprise Cloud installations / EC-owned OAuth apps; 1,000 per repository for `GITHUB_TOKEN`) | same |
| Point formula | sum of requests needed per unique connection ÷ 100, rounded; **minimum 1 point per call** | same |
| Secondary limits | ≤100 concurrent requests; **≤2,000 points/minute** for the GraphQL endpoint | same |
| `rateLimit` object | `limit`, `remaining`, `used`, `resetAt`, `cost` | same |
| Timeout | ">10 seconds to process an API request" → request terminated, 502/504, "We couldn't respond to your request in time" | same |
| Timeout penalty | "additional points will be deducted from your primary rate limit for the next hour" | same |
| Partial results | "If your GraphQL query consumes too many resources, GitHub will terminate the request and **return partial results along with an error**" | same |
| Authentication | documented only via PAT / GitHub App / OAuth app; no unauthenticated tier is published | Forming calls with GraphQL; Using the Explorer |
| REST README | "Gets the **preferred** README for a repository" — provider-side file resolution, no path supplied by the caller | REST repos/contents |
| REST contents size | ≤1 MB fully supported; 1–100 MB returns `content: ""`, `encoding: "none"`; >100 MB unsupported | same |
| REST issues defaults | `sort=created`, `direction=desc`, and "GitHub's REST API considers every pull request an issue" | REST issues |
| REST pulls defaults | `sort=created`, `direction=desc` | REST pulls |
| REST tags order | **not documented** | REST repos |

Schema facts verified on the same date (GraphQL reference: repos, git, commits,
issues, pulls, releases):

~~~text
Repository:  object(expression|oid), defaultBranchRef: Ref, licenseInfo: License,
             languages(LanguageConnection, orderBy LanguageOrder{SIZE}),
             releases(ReleaseConnection!, orderBy ReleaseOrder{CREATED_AT,NAME}),
             refs(refPrefix: String!, orderBy RefOrder), issues(IssueConnection!,
             states, orderBy IssueOrder{COMMENTS,CREATED_AT,UPDATED_AT}),
             pullRequests(PullRequestConnection!, states, orderBy IssueOrder),
             repositoryTopics(pagination only), diskUsage, pushedAt, isFork,
             isArchived, isTemplate, isEmpty, isPrivate, hasIssuesEnabled,
             primaryLanguage, description, homepageUrl, createdAt, nameWithOwner
             — NO field returns the repository README
Commit:      oid, committedDate, authoredDate, message, author, committer,
             tree: Tree! ("Commit's root Tree"),
             history(first/after/…, author, path, since, until)
               -> CommitHistoryConnection! { nodes, edges, pageInfo, totalCount }
Blob:        byteSize, text (null if binary), isBinary, isTruncated, oid
TreeEntry:   path, name, type, mode, size, object, oid, extension, lineCount
Language:    LanguageEdge.size = "number of bytes of code written in the language"
Issue:       number, title, body: String!, state: IssueState{OPEN,CLOSED},
             closed, createdAt
PullRequest: number, title, state: PullRequestState{OPEN,CLOSED,MERGED},
             closed, merged, mergedAt, createdAt
Release:     tagName, name, description, isDraft, isPrerelease, publishedAt
~~~

Not confirmed from an official page in this session, and therefore **not relied
on** below: the exact `RefOrderField` enum values, and `Ref.target`'s exact
return type. Both must be confirmed against the live schema before any tag/head
query is written.

---

## 4. REST → GraphQL surface matrix

`PARITY` = an exactly sufficient GraphQL equivalent exists.
`REST REQUIRED` = no equivalent with proven parity.
`DECISION` = technically expressible, but adopting it changes a canonical
semantic and needs an owner decision.

### 4.1 Profile level

| Surface | REST today | GraphQL candidate | Stop / pagination | Failure semantics | REST fallback | Verdict |
|---|---|---|---|---|---|---|
| profile identity | `GET users/{login}`; `type=User` gate, canonical login after rename redirect, `public_repos`, `created_at`, `avatar_url` | `user(login:)` + `__typename` | none | 404 → `Unavailable`; GraphQL returns `data.user = null` **plus** a `NOT_FOUND` error at HTTP 200 | yes | **DECISION** — parity exists, but this single request is also the *only* input to the D-2 budget guard (§7.3). Moving it changes the guard premise for a 1-request saving |
| public repository manifest | `users/{login}/repos?type=owner&sort=full_name&direction=asc` ×P, bound 10 pages | `user.repositories(ownerAffiliations: OWNER, privacy: PUBLIC, first: 100, after:)` | `pageInfo.hasNextPage` replaces `Link rel=next`; page bound reproducible | page failure → `Incomplete` + warning; GraphQL partial page has no mapping today | yes | **PARITY (conditional)** — `ProfileRepositoryManifest` re-sorts canonically, so provider order does not matter; but `size`→`diskUsage` and the `public_repos` comparison warning both need §4.3 handling |

### 4.2 Repository level

| Surface | REST today | GraphQL candidate | Stop / pagination | Failure semantics | REST fallback | Verdict |
|---|---|---|---|---|---|---|
| metadata / identity | `repos/{o}/{r}` | `repository(owner:,name:)` scalars | none | 404/403/451 → typed availability; GraphQL surfaces the same as field errors | yes | **PARITY** |
| topics | inside metadata (no extra request) | `repositoryTopics(first:100)` | connection, `first` required | — | yes | **PARITY** — no saving; evaluator sorts topics Ordinal, so order is irrelevant |
| default branch ref (head SHA) | `git/ref/heads/{branch}` | `defaultBranchRef.target.oid` | none | empty repo → `defaultBranchRef` null; REST answers 409 "Git Repository is empty." | yes | **PARITY** — empty-repository detection must be re-proven, it currently keys on the 409 body text |
| head commit → root tree SHA | `git/commits/{sha}` | `defaultBranchRef.target … on Commit { tree { oid } }` | none | — | yes | **PARITY** |
| recursive file tree | `git/trees/{sha}?recursive=1` + `truncated` | **none** — GraphQL `Tree.entries` is one level deep, has no `recursive` argument and no `truncated` flag; depth must be hand-nested | n/a | n/a | n/a | **REST REQUIRED** |
| project files (content) | `contents/{path}?ref={sha}` per file | `object(expression:"{sha}:{path}")… on Blob { text isTruncated byteSize }` | n/a | Blob `text` is null for binary, `isTruncated` threshold is undocumented | yes | **DECISION** — byte-identical content parity is unproven; the 1 MB REST rule and the GraphQL truncation rule are different, undocumented mechanisms |
| workflows (content) | same as above | same as above | n/a | same | yes | **DECISION** — same reason |
| documentation Markdown | same, ≤16 files, per-file 128 KB, corpus 512 KB, Ordinal order | same | n/a | same | yes | **DECISION** — same reason; the byte budgets are computed from decoded content and must stay identical |
| README | `repos/{o}/{r}/readme` — provider resolves the **preferred** README | **none** — no README field; a client would have to guess candidate paths | n/a | n/a | n/a | **REST REQUIRED** |
| license | `repos/{o}/{r}/license` — license key/name/spdx **plus file path and size** | `licenseInfo` gives key/name/spdxId only, **no path, no size** | n/a | n/a | n/a | **REST REQUIRED** — the path is load-bearing: `LicensePresent` writes it into `EvidenceItem.Location`, and `MergeFileTree` overlays the license file into the file tree |
| languages | `repos/{o}/{r}/languages` | `languages(first:100, orderBy:{field:SIZE,direction:DESC}) { edges { size node { name } } }` | connection; >100 languages is not realistic but must still be handled | — | yes | **PARITY** — snapshot stores an unordered case-insensitive map, so edge order is irrelevant |
| commits | `commits?sha={branch}&per_page=100`, `sha` + `commit.committer.date` | `defaultBranchRef.target … on Commit { history(first:100, after:) { nodes { oid committedDate } pageInfo { hasNextPage } } }` | §15 commit boundary reproducible on `hasNextPage` | empty repo, 409 | yes | **PARITY (test-gated)** — `committedDate` must be proven equal to `commit.committer.date`, and the history walk order equal to the REST walk order |
| issues | `issues?state=all&per_page=100`, PR entries filtered client-side | `issues(first:100, states:[OPEN,CLOSED], orderBy:{field:CREATED_AT,direction:DESC})` — PRs are not in this connection | boundary reproducible | `has_issues=false` → today **no request at all** and a `NotApplicable` availability; GraphQL would need `hasIssuesEnabled` first | yes | **DECISION** — see §4.3(a): the observed prefix differs from REST whenever the collection stops early |
| pull requests | `pulls?state=all&per_page=100`, `IsMerged = merged_at != null` | `pullRequests(first:100, states:[OPEN,CLOSED,MERGED], orderBy:{field:CREATED_AT,direction:DESC})`; `closed`/`mergedAt` map directly | boundary reproducible | — | yes | **PARITY (test-gated)** — `MERGED` is a distinct state; `IsClosed` must come from `closed`, not from `state != OPEN` alone |
| releases | `releases?per_page=100` | `releases(first:100, orderBy:{field:CREATED_AT,direction:DESC})`; `description`→`Notes`, `isDraft`→`IsDraft` | boundary reproducible | draft visibility depends on token permissions in both APIs | yes | **PARITY (test-gated)** — `publishedAt`'s documented description is ambiguous ("date and time when the release was created"); its null-for-draft behaviour must be proven |
| tags | `tags?per_page=100`, order **undocumented** | `refs(refPrefix:"refs/tags/", first:100, orderBy: RefOrder)` | boundary is count-only (`tagCount >= 1`) | — | yes | **PARITY (test-gated)** — only the count reaches evidence, but the collected `RepositoryTag` set is snapshot content, and REST's order is undocumented so "identical order" cannot be asserted from documentation alone |

### 4.3 The three parity findings that are *not* obvious

**(a) DecisionBounded prefix composition — issues.**
REST `/issues` interleaves pull requests into the same pages; the collector
filters them out *after* the page is fetched, then applies the stop rule to the
filtered count. GraphQL's `issues` connection never contains pull requests. For
the **same** repository and the **same** stop rule, one REST page of 100 mixed
items may yield 60 issues while one GraphQL page yields 100 issues. Both stop at
the same point (≥2 qualifying issues, next page exists) and produce the **same
status and the same score** — but `snapshot.Issues.Items` differs, so the
`IssueUsage` evidence *value* differs
(`qualifyingAtLeast=60` vs `qualifyingAtLeast=100`).

Scope of the consequence, verified in source:
- repository level — `SignalResult.Status`, `Score` and evidence **Key** are
  unchanged; only `EvidenceItem.Value` differs;
- profile level — `CrossRepositoryEvidenceComposer` propagates
  `signal.Evidence.Select(evidence => evidence.Key)` only, so **no**
  `ObservedProfileEvidence`, `ProfileEvidenceReference`, pattern, representative
  selection or `ProfileReport` content changes.

Canonical §6 enumerates "identical evidence keys per signal" and does not
enumerate evidence values; §15.5 is the owner of `DecisionBounded` evidence
values. This is therefore **not** a §6 breach, but it *is* a deliberate
repository-level evidence-value change and must be acknowledged by the
architecture owner rather than absorbed silently. The same class of divergence
can occur on any `DecisionBounded` surface whose page composition differs.

**(b) `Empty` and the manifest.**
`DetermineEmptyState(size, pushedAt)` derives `Empty` from REST `size == 0 &&
pushed_at == null`. GraphQL exposes `isEmpty: Boolean!` directly and `diskUsage`
(nullable) as the `size` analogue. Using `isEmpty` would be *more* accurate and
would change admission outcomes for at least the `Undetermined` case — which is
an **eligibility semantics change**, i.e. exactly the product/domain route §6
requires, not a collection-composition change. Using `diskUsage` preserves the
current derivation but inherits a nullable field whose null semantics differ from
REST's `size`. Either way: **DECISION**.

**(c) Manifest completeness warning.**
`GitHubProfileSnapshotOrchestrator` compares `metadata.PublicRepositoryCount`
(REST `public_repos`) against the collected count and emits a warning on
mismatch. If both numbers came from the same GraphQL connection
(`repositories.totalCount`), the comparison would become trivially true and the
warning could never fire — silently deleting an existing operational signal.
Keep the two sources independent, or amend the contract deliberately.

---

## 5. Consolidation candidate set / must-remain-REST set

### 5.1 Consolidatable — 8 test-gated, 1 owner-acknowledgement required

Per admitted repository, these **9** REST round trips collapse into one shared
GraphQL repository envelope. **Eight of the nine are test-gated parity work;
surface 6 (`issues`) is not** — see the note below and §4.3(a).

~~~text
1  repos/{o}/{r}                      -> repository scalars + topics
2  git/ref/heads/{branch}             -> defaultBranchRef.target.oid
3  git/commits/{sha}                  -> ...on Commit { tree { oid } }
4  languages                          -> languages(first:100, orderBy SIZE DESC)
5  commits      FIRST page            -> history(first:100)
6  issues       FIRST page            -> issues(first:100, …)   DECISION §4.3(a)
7  pulls        FIRST page            -> pullRequests(first:100, …)
8  releases     FIRST page            -> releases(first:100, …)
9  tags         FIRST page            -> refs(refPrefix:"refs/tags/", first:100)
~~~

Continuation pages for surfaces 5–9 remain necessary and move into GraphQL,
where they can be batched across repositories that are still paginating.

**Surface 6 is not proven pure parity — it is an architecture-owner
acknowledgement / contract decision.** Two distinct divergences, both real:

1. **Evidence-value divergence (§4.3(a)).** REST `/issues` interleaves pull
   requests and the collector filters them client-side *after* the page is
   fetched; the GraphQL `issues` connection never contains pull requests. For
   the same repository under the same `DecisionBounded` stop rule, one REST page
   can yield `qualifyingAtLeast=60` where one GraphQL page yields
   `qualifyingAtLeast=100`. Status, score and evidence **Key** are unchanged and
   no profile-level output changes — but the repository-level `IssueUsage`
   evidence **value** does.
2. **`has_issues=false` is not silently equivalent to an unconditional
   connection.** Today such a repository produces **no issues request at all**
   and a `NotApplicable` result. A GraphQL envelope asks for the `issues`
   connection unconditionally, so the not-applicable path must be reconstructed
   deliberately rather than assumed to fall out of the query.

Neither divergence is proven equivalent by this session. Both require explicit
architecture-owner acknowledgement before S3, not silent absorption.

### 5.2 Must remain REST

| Surface | Reason |
|---|---|
| recursive file tree | GraphQL has no recursive tree and no `truncated` equivalent; the `truncated` flag drives `FileTree.IsComplete`, which gates `TestStructure` and `ReadmeExists` |
| README | GraphQL cannot reproduce provider-side "preferred README" resolution; guessing paths would be a new inference, not a collection change |
| license | GraphQL `licenseInfo` carries no file path or size; the path is written into `EvidenceItem.Location` and into the merged file tree |
| file contents (project files, workflows, docs Markdown) | Blob truncation semantics are undocumented and differ from REST's documented 1 MB rule; byte-identical content parity cannot be asserted from documentation |
| profile metadata (recommended) | it is the sole input of the D-2 budget observation; moving it for a 1-request saving is a bad trade |

---

## 6. Query architecture options

| Option | Shape | HTTP per profile | Verdict |
|---|---|---|---|
| **A — REST baseline** | today | 12–15 per admitted repo | the control arm; keep as the fallback path |
| **B — GraphQL envelope per repository + REST deep content** | 1 GraphQL call per repository + 3 REST + contents | ~4 + contents + continuations | works, preserves the per-repository seam exactly (`IProfileRepositorySnapshotSource.CollectAsync(reference)`), no orchestrator change; but leaves ~⅓ of the possible saving on the table |
| **C — bounded GraphQL batches (B repositories per query)** | one envelope query covering N admitted repositories via aliases | best round-trip profile | **recommended target**, but needs a prefetch seam (§6.1) |
| **D — full GraphQL** | everything in GraphQL | — | **rejected on parity, not on cost**: tree recursion, README resolution and license path have no equivalent; would also concentrate every surface behind one 10-second termination window |

### 6.1 The batching seam problem (structural finding)

`ProfileRepositoryAnalysisOrchestrator` evaluates admission **inside** its
`foreach` and calls the snapshot source one repository at a time, and
`public-profile-runtime-collection.md` §14.2 forbids modifying that orchestrator.
So option C needs a prefetch that knows the admitted set *before* the loop.
Three candidates, in preference order:

1. **Source-side priming.** The Profile snapshot source is constructed per scope
   with the manifest and the same `AnalysisAdmissionPolicy` instance, and on the
   first `CollectAsync` it evaluates that policy over the manifest to compute the
   admitted set and prime the first batch. The policy is pure, deterministic and
   I/O-free, so a second evaluation cannot disagree with the orchestrator's. No
   orchestrator change; §14.2 intact. **Recommended.**
2. **An explicit optional prefetch interface** invoked by the composition root
   before `AnalyzeAsync`. Cleaner to read, but the composition root would then
   own an admission-shaped decision — worse ownership.
3. **Orchestrator batch awareness.** Requires a §14.2 amendment. Not recommended
   for a performance change.

### 6.2 Batching key

| Key | Assessment |
|---|---|
| `repositoryOwner.repositories` connection | rejected for evidence collection — it enumerates the owner's repositories again and cannot express "only these admitted N", so it risks collecting deep evidence for rejected repositories, violating the execution-order invariant |
| `repository(owner:,name:)` **aliases** | **recommended** — `r0: repository(owner:"x", name:"y"){…}`; expresses exactly the admitted set, maps 1:1 back to `ProfileRepositoryReference`, degrades per-alias on error, trivially testable |
| `nodes(ids:)` | rejected for V1 — no stable node ID exists in the pipeline today (the manifest carries no ID); adopting it would add an identity concept the domain does not have |

Alias caveat: a repository renamed between manifest collection and the envelope
query resolves through GitHub's redirect in REST today; under aliases the result
would come back under the requested alias with the canonical `nameWithOwner`
inside. Identity mapping must therefore key on the alias, and any
`nameWithOwner` mismatch must be surfaced, never silently rewritten.

---

## 7. Rate-limit / cost model

Four dimensions, deliberately **not** merged into one number. No conversion of
the form "1 GraphQL query = N REST requests" is asserted.

### 7.1 Dimensions

~~~text
HTTP round trips        wall-clock relevant; the thing this work reduces
REST core requests      5,000/hour authenticated; today's binding budget
GraphQL primary points  5,000/hour on the separate `graphql` resource
GraphQL secondary       2,000 points/minute; ≤100 concurrent requests
node count              ≤500,000 per call
~~~

### 7.2 Cost of the proposed envelope

Per repository the envelope opens ~7 connections at `first: 100` →
≈ 1 + 7×100 = **701 nodes**. A batch of 10 repositories ≈ **7,010 nodes**, i.e.
~1.4 % of the 500,000 node cap. The node limit is **not** the binding constraint.

Point cost by GitHub's published formula — (1 + 7×B) requests ÷ 100, rounded,
minimum 1 — is **≈ 1 point** for any batch size up to ~16 repositories. The
5,000 points/hour primary budget is **not** the binding constraint either.

**The binding constraint is the 10-second server-side processing limit**, with a
punitive property: a timeout costs *extra* primary points for the next hour. That
argues for small batches (start at B = 5), `first` values no larger than needed,
and treating 502/504 as a first-class expected outcome rather than an anomaly.

### 7.3 Telemetry consequences (§9 / B-RI / D-2)

- `ProfileGitHubRequestCategory.Classify` matches REST route shapes; `POST
  /graphql` yields path segments `["graphql"]` → `other`. The closed 17-member
  vocabulary needs an authorized extension (e.g. `graphql-repository-envelope`,
  `graphql-continuation`) — and, unlike REST, one GraphQL request covers several
  evidence surfaces, so per-surface request attribution **stops being derivable
  from the route**. §9 requires request counts "grouped by collector / evidence
  surface, **or an equivalent bounded category set**"; a per-operation category
  plus a per-operation surface list satisfies that, but it is a real change to
  what the telemetry can say.
- `ProfileGitHubRequestTelemetryHandler` reads `X-RateLimit-*` from every
  response. GraphQL responses carry the same headers on the `graphql` resource,
  and the Session 120 bucketing already keys on resource + reset window, so
  observation survives — but the two buckets must never be summed.
- The D-2 budget **decision** currently reads `remaining` from the
  `profile-metadata` response only. If profile metadata stayed REST (§5.2), the
  decision keeps its exact current premise and GraphQL adds a second, separately
  observed bucket. If profile metadata moved to GraphQL, the guard would silently
  start deciding REST-path continuation from a GraphQL budget. **That would be a
  semantic defect, not a refactor.**

---

## 8. Partial-failure model — the contract gap

GitHub GraphQL can answer **HTTP 200** with a populated `data` object *and* an
`errors` array, and explicitly does so when resource limits are exceeded
("return partial results along with an error"). Field-level errors also null
individual fields while the rest of `data` succeeds.

The current model cannot express this:

~~~text
GitHubApiAvailability.FromFailureResponse(response, dataKey)
    switches on HttpStatusCode ONLY
    403+X-RateLimit-Remaining:0 -> RateLimited
    429                         -> RateLimited
    403                         -> Forbidden
    404                         -> Unavailable
    other non-2xx               -> Failed
~~~

An HTTP 200 with partial data never reaches that method. Every collector treats
`IsSuccessStatusCode` as success, so a partial GraphQL response would be
**silently normalized into positive evidence** — a missing `releases` connection
would become "no releases", i.e. exactly the prohibited conversion of provider
failure into developer evidence (§5.2, §11).

Required mapping, which **no canonical contract currently defines**:

| GraphQL outcome | Required Profile meaning | Exists today? |
|---|---|---|
| HTTP 200, complete `data`, no `errors` | per-surface `Available` | yes |
| HTTP 200, `data` present, `errors` naming surface X | X → typed non-`Available` availability; the other surfaces in the same envelope stay `Available` | **no** |
| HTTP 200, `errors` of type `RATE_LIMITED` | `RateLimited` on every surface the operation was carrying | **no** |
| HTTP 200, resource-limit termination with partial data | must not become `Complete`, must not become negative evidence | **no** |
| 502/504 timeout | whole-operation failure; must not be conflated with the internal execution deadline (§11.2) or with caller cancellation (§11.1) | **no** |
| `data.repository = null` + `NOT_FOUND` error | `Unavailable`, as REST 404 is today | **no** |

**This is the GQL-F2 driver.** Implementation must not begin before
`public-profile-runtime-collection.md` gains a section that owns:
1. the GraphQL response → `DataAvailability` mapping, per surface, including the
   HTTP-200-with-errors case;
2. the rule that one operation carrying N surfaces must produce N independent
   availability outcomes, never one collapsed outcome;
3. the rule that a GraphQL error type is never developer evidence and never
   downgrades `AnalysisStatus` except through the existing typed availability
   path;
4. the relationship between a GraphQL 502/504 and §10.3's deadline attribution.

---

## 9. Projected request reduction

Projection only. **No wall-clock claim is made and no projection is translated
into seconds.** The measured Session 120 shapes supply the repository counts,
the contents counts and the continuation-page counts.

Assumptions of the projected hybrid (Option C, profile level left on REST):
per admitted repository 3 REST calls remain (tree, README, license) plus its
measured contents calls; the 9 calls of §5.1 move into a batched envelope;
continuation pages remain but are batched per round. `B` = repositories per
envelope query. Round counts are given as ranges because Session 120 measured
continuation *totals*, not their per-repository distribution.

| | torvalds | karpathy | fatihGunalp |
|---|---|---|---|
| admitted | 9 | 54 | 4 |
| **REST today (measured)** | **131** | **662** | **63** |
| projected REST: profile | 2 | 2 | 2 |
| projected REST: tree+README+license | 27 | 162 | 12 |
| projected REST: contents (measured) | 1 | 8 | 13 |
| **projected REST subtotal** | **30** | **172** | **27** |
| projected GraphQL: envelopes (B=5) | 2 | 11 | 1 |
| projected GraphQL: continuation rounds | 3–21 | 1–5 | 0 |
| **projected GraphQL subtotal** | **5–23** | **12–16** | **1** |
| **projected total HTTP round trips** | **35–53** | **184–188** | **28** |
| reduction factor | 2.5×–3.7× | 3.5×–3.6× | 2.3× |
| projected REST core budget consumed | 30 | 172 | 27 |
| projected GraphQL points consumed | ≈ 5–23 | ≈ 12–16 | ≈ 1 |

Continuation-round derivation for torvalds: 30 measured commit requests over 9
repositories = 21 continuation pages. If they belong to one repository they are
strictly sequential (21 rounds); if evenly spread, ~3 rounds. Nothing in the
measured data distinguishes the two, so both bounds are reported.

Two honest observations:

- **The remaining REST cost is dominated by the three per-repository calls that
  cannot move** (tree, README, license): 162 of karpathy's projected 172. Any
  future work beyond this hybrid must attack those three, and §5.2 says they
  cannot be attacked with GraphQL.
- **The commit/PR pagination floor is unchanged.** GraphQL connections are also
  capped at 100 per page, so a deep history still costs one round trip per 100
  commits. GraphQL reduces the *fixed* per-repository cost, not the *depth* cost.

---

## 10. Semantic-equivalence proof plan

No production switch without all of the following, all offline (recorded
fixtures, no live calls in CI):

1. **Dual-composition equivalence test.** The existing Session 097/099 pattern:
   the same profile input through the REST composition and through the hybrid
   composition must produce identical `ProfileReport` section content and
   ordering, identical profile `AnalysisStatus`, `ProfileLimitation` and
   `ProfileWarning` sets, identical `ProfilePatternSet`, promotion outcomes and
   `RepresentativeRepositorySelection`.
2. **Per-repository equality test:** identical `RepositoryAnalysis.Status`, score
   breakdown, all 15 `SignalResults` (key/status/score), evidence **keys**,
   `LimitationKeys`, warning codes and category results.
3. **Evidence-value diff test.** An explicit, asserted list of every evidence
   value that is *allowed* to differ (§4.3(a)) — the test must fail if any
   *other* value differs. This converts the known divergence into a locked,
   reviewable set instead of an unnoticed drift.
4. **Field-mapping unit tests** proving `committedDate == commit.committer.date`,
   `closed`/`mergedAt` → `IsClosed`/`IsMerged`, `description` → `Notes`,
   `isDraft` → `IsDraft`, `LanguageEdge.size` → language bytes, topic
   normalization, and `defaultBranchRef.target.tree.oid` → root tree SHA.
5. **Stop-rule tests** proving each §15 boundary fires on `pageInfo.hasNextPage`
   exactly as it fires on `Link rel="next"`, that a never-reached boundary still
   enumerates fully, that `DecisionBounded` still carries `TotalCount = null`,
   and that GraphQL `totalCount` is **never** written into `TotalCount`.
6. **Partial-failure tests** covering every row of the §8 table, each asserting
   that no partial result becomes `Complete`, negative evidence, a warning, a
   limitation or a status downgrade beyond the typed availability path.
7. **Empty-repository test** proving `defaultBranchRef == null` reproduces the
   current 409 "Git Repository is empty." behaviour exactly.
8. **Isolation tests unchanged and green** —
   `RepositorySourceEvidenceIsolationTests`,
   `RepositoryDependencyEvidenceIsolationTests`.
9. **Repository Proof untouched** — its composition, tests and request graph
   unchanged (§3.1).
10. **Telemetry tests** proving the extended category vocabulary stays closed and
    bounded, that no token/content/path enters telemetry, and that REST and
    GraphQL rate-limit buckets are never summed.
11. **A live A/B parity run** on the three Session 120 profile shapes, comparing
    the two compositions' reports, executed once under architecture-owner
    approval before any production switch.

---

## 11. Canonical contract impact

| Contract | Required change |
|---|---|
| `public-profile-runtime-collection.md` §9 | telemetry category vocabulary must accommodate multi-surface operations; per-surface attribution rule restated |
| `public-profile-runtime-collection.md` §11 | **new**: GraphQL partial-data/error → availability mapping (§8) — the blocking amendment |
| `public-profile-runtime-collection.md` §10.4 | no new authentication principle and no weakening: §10.4 already rejects an unauthenticated production Profile beta for the measured request surface, and already conditions the interim credential on "only after every used endpoint is verified compatible". The narrow delta is that GraphQL adds one more used endpoint — `POST /graphql` — whose credential compatibility must be explicitly verified under that existing, unchanged requirement. Credential type (D-5) stays open |
| `public-profile-runtime-collection.md` §14.2 | unchanged if the §6.1(1) source-side priming seam is used; an amendment is needed only for options 2–3 |
| `public-profile-runtime-collection.md` §15.5 | acknowledgement that `DecisionBounded` evidence values are prefix-dependent and may differ between transports (§4.3(a)) |
| `public-profile-evidence-repository-eligibility.md` | only if `isEmpty` replaces the `size`/`pushed_at` derivation (§4.3(b)) — an eligibility semantics change, not a collection change |
| `deployment-milestones.md` | no change: GraphQL is not a Milestone B criterion |

Nothing above has been changed in this session.

---

## 12. Verdict

~~~text
GQL-F2 — FEASIBLE, CONTRACT AMENDMENT REQUIRED
~~~

A bounded hybrid (GraphQL repository envelope over admitted repositories + REST
for tree, README, license and file contents) can consolidate **9 of the 12**
per-repository REST round trips and project a **2.3×–3.7×** reduction in total
HTTP round trips on the three measured profile shapes — *provided* the GraphQL
partial-data/error semantics are first given a canonical mapping (§8). Without
that amendment, a partial GraphQL response would silently become positive
developer evidence, which the current contract forbids and the current code
cannot detect.

Semantic preservation is **qualified, not blanket**. Eight of the nine envelope
surfaces are test-gated parity work; two qualifications stand and are *not*
proven equivalent by this session:

- **`issues` (§4.3(a), §5.1)** carries a known `DecisionBounded` evidence-value
  divergence plus a separate `has_issues=false` / `NotApplicable` reconstruction
  question. Profile-level outputs are unaffected, but the repository-level
  evidence value changes — this needs architecture-owner acknowledgement.
- **`isEmpty` (§4.3(b))** would be *more* accurate than the current
  `size`/`pushed_at` derivation, which makes adopting it an **eligibility
  semantics change** owned by the product/domain route, not a collection change.

Secondary conditions: the credential decision (D-5) must be closed first, and
the §9 telemetry vocabulary must be extended for multi-surface operations.

---

## 13. Recommended implementation slices

Small, reversible, each independently abandonable. None of them starts before
the §8 amendment is written and approved.

~~~text
S0  CONTRACT   §8 partial-failure mapping + §9 telemetry vocabulary amendment.
               No code. This is the gate for everything below.

S1  TRANSPORT  A read-only GraphQL client behind its own interface on the
               existing profile named client: one POST, no collector wired to
               it, no DI replacement of any collector. Proves auth, headers,
               `rateLimit` observation and the §8 mapping in tests only.

S2  SHADOW     One surface (languages — smallest, order-independent, single
               scalar map) collected through GraphQL behind a default-off
               switch, with the §10 field-mapping and equivalence tests. The
               REST collector stays the default path.

S3  ENVELOPE   The per-repository envelope of §5.1 (Option B), still one query
               per repository, default off. Full §10 (1)-(9) proof.

S4  BATCH      Option C batching via `repository(owner:,name:)` aliases with the
               §6.1(1) source-side priming seam, B configurable, B=5 default.

S5  LIVE A/B   One approved live parity run on the three Session 120 shapes,
               then the production switch decision — separately, with the
               REST composition retained as a configuration fallback.
~~~

---

## Changes

~~~text
Created:
- worklog/sessions/2026-08-25-121-graphql-request-consolidation-feasibility.md

Modified:
- worklog/CURRENT.md   (Session 120 verified/integrated reconciliation;
                        Session 121 recorded CLOSED and Active Session set to
                        NONE at closure; F3 findings carried; next-session
                        sequencing 122/123)
- worklog/INDEX.md     (Session 120 Commit backfilled to 59a7c30;
                        Session 121 row added, then annotated with the
                        independent review outcome)

Deleted:
- none

Production code, tests, docs/: UNCHANGED.
~~~

---

## Decisions

~~~text
- Verdict GQL-F2, not GQL-F1: the partial-data/error mapping gap is real and
  blocking, and no existing canonical section owns it.
- Full GraphQL (option D) rejected on parity grounds, not on cost.
- Profile metadata deliberately recommended to stay REST, to preserve the D-2
  budget observation premise.
- `repository(owner:,name:)` aliases chosen over the repositories connection,
  because the connection cannot express "only these admitted N" and would risk
  collecting evidence for rejected repositories.
- No live GraphQL probe was issued. Three schema facts were left explicitly
  unverified rather than assumed (RefOrderField values, Ref.target type, Blob
  truncation threshold).
~~~

---

## Validation

~~~text
Source re-verification of the Session 120 request graph   PASS (formula and
                                                          category table agree)
Canonical §4/§5/§6/§15/§16 cross-check                    PASS
Provider documentation fetch, 2026-08-25                  PASS (sources in §3)
dotnet build / dotnet test                                NOT RUN — no code
                                                          changed in this session
git diff --check                                          PASS
git status --short                                        worklog-only + the
                                                          pre-existing artifacts/
~~~

**Independent review: RUN — PASS WITH FINDINGS.** See the Independent Review
section below for the reviewer's source re-derivation, the four LOW findings and
the correction applied to each. The only mutation remains this worklog record
plus the CURRENT/INDEX reconciliation; no production code, test or canonical
contract changed, so no build or test run applies.

---

## Independent Review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Scope:    this record + worklog/CURRENT.md + worklog/INDEX.md deltas
Outcome:  GQL-F2 unchanged; 4 LOW findings, 0 blocking, 0 high
~~~

The reviewer re-derived the load-bearing claims from source instead of accepting
this record: `GitHubApiAvailability.FromFailureResponse` switching on
`HttpStatusCode` only, the 17-member closed telemetry vocabulary, the D-2
`profile-metadata`-only budget read, `CrossRepositoryEvidenceComposer`
propagating `evidence.Key` only, the license-path consumers, the 409-body empty
detection, `Enumerated ⇔ IsComplete`, the contents bounds and
`DefaultMaxPageCount = 10`. The §9 projection arithmetic was recomputed
independently and reproduced 2.47×–3.74× / 3.52×–3.60× / 2.25×. Scope was
confirmed worklog-only, `artifacts/` untouched, and no credential string
present.

Four LOW findings were raised, all at worklog-fidelity / wording-precision
level. The architecture owner accepted **GQL-F2** and accepted the review result,
and required all four to be corrected before closure. The findings are recorded
here rather than erased:

| # | Finding | Correction applied in this session |
|---|---|---|
| F1 | Lifecycle contradiction — `INDEX` recorded `PASS` (finalized) while `CURRENT` still declared `Active Session: 121`, leaving the session readable as resumable. | `INDEX` Result stays `PASS`; `CURRENT` `Active Session` set to `NONE`, Session 121 moved to Last Session and marked CLOSED / awaiting final integration, explicitly not resumable. |
| F2 | §5.1 was titled "Consolidatable without semantic drift" although its 6th item (`issues`) is marked DECISION in §4.2 and carries the §4.3(a) evidence-value divergence; §12 repeated the unqualified framing. | §5.1 retitled "8 test-gated, 1 owner-acknowledgement required", item 6 marked `DECISION §4.3(a)`, and a note added covering both the evidence-value divergence and the `has_issues=false` / `NotApplicable` reconstruction question. §12 semantic-preservation claim qualified for `issues` and `isEmpty`. |
| F3 | `CURRENT` carried only the §8 blocking gap and dropped the three decision-relevant §4.3 findings — a fidelity loss in the file the next session reads first. | `CURRENT` now names all three: the `issues` evidence-value difference, `isEmpty` as an eligibility semantics change, and the manifest completeness warning that could stop firing. |
| F4 | The §11 `§10.4` row (and the matching §2 paragraph) framed an authenticated credential as a newly created hard runtime dependency, although §10.4 already rejects an unauthenticated production Profile beta and already requires every used endpoint to be verified compatible. | §11 row, §2 paragraph and the Known Risks line rewritten to the narrow true delta: GraphQL adds `POST /graphql` as one more used endpoint whose credential compatibility must be verified under the existing, unchanged §10.4 requirement. No PAT-vs-App choice made. |

No finding required a change to the verdict, the projections, the parity matrix
or the §8 blocking analysis.

---

## Problems

~~~text
Problem:
CURRENT.md reported Session 120 as unverified with last integrated commit
5599291, while main is at 59a7c30.

Cause:
Session 120 closed before architecture-owner verification; CURRENT was not
updated after the merge.

Resolution:
Reconciled to the verified state on architecture-owner instruction. No history
rewritten, no Session 120 content altered.
~~~

---

## Not Done

~~~text
- no GraphQL transport, client, package, DI registration or query
- no collector replacement, no concurrency, timeout, reserve-floor, credential,
  cache, ETag or retry change
- no canonical docs/ mutation
- no live GraphQL call and no live benchmark
- no production value selected
- Session 121's originally planned subject (D-1/D-2/D-3/D-5 runtime decision
  design) was NOT started; this session answered the separate GraphQL question
  the architecture owner raised instead
~~~

---

## Known Risks

~~~text
- Three schema facts remain unverified (RefOrderField values, Ref.target type,
  Blob truncation threshold) and one documented description is ambiguous
  (Release.publishedAt). None may be assumed at implementation time.
- Provider numeric limits (points, nodes, 10 s termination) are provider facts
  recorded with a 2026-08-25 source date; they can change.
- The continuation-round projections are ranges because Session 120 measured
  continuation totals, not their per-repository distribution.
- The projected reduction is a round-trip projection only. No wall-clock
  improvement is claimed or implied.
- `POST /graphql` always requires authentication, so adopting it removes the
  unauthenticated fallback the REST collectors technically tolerate (60/hour).
  That is a runtime-path consequence and a D-5 input; it does not create a new
  production authentication requirement, because canonical §10.4 already rejects
  an unauthenticated production Profile beta.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked (new):
- worklog/sessions/2026-08-25-121-graphql-request-consolidation-feasibility.md

Pre-existing unrelated:
- artifacts/ (user-owned, untouched)
~~~

---

## Next

The architecture owner **accepted GQL-F2** and accepted the independent review
result (PASS WITH FINDINGS); all four findings are corrected above.

Sequencing decision (architecture owner):

~~~text
Session 122   Profile REST Request Redundancy Audit          <- next
Session 123   Profile Production Runtime Decision Design
              (D-1 / D-2 / D-3 / D-5)
S0            §8 partial-failure mapping amendment — authorized,
              NOT started in this session, still the gate for S1-S5
~~~

This supersedes the earlier ordering in which Session 122 would have opened the
D-1/D-2/D-3/D-5 runtime decision design. No S0 work, no GraphQL code and no
Session 122 work was started here; each opens through the normal model/effort
gate.

---

## Git

~~~text
Suggested commit:
docs(worklog): record GraphQL request consolidation feasibility verdict

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Read-only feasibility only; no production mutation
[x] Session 120 request graph re-verified against current main
[x] Provider facts recorded with source and date
[x] Per-surface parity matrix produced, with REST REQUIRED / DECISION marks
[x] Partial-failure contract gap identified precisely
[x] Rate-limit dimensions kept separate; no fake conversion metric
[x] Projections stated as round trips only, never as seconds
[x] Verdict recorded (GQL-F2)
[x] INDEX row added; Session 120 row reconciled
[x] CURRENT reconciled; Active Session set to NONE at closure
[x] Independent review of this packet — PASS WITH FINDINGS (4 LOW, 0 blocking)
[x] All four review findings (F1-F4) corrected in this session
[x] Architecture-owner decision — GQL-F2 ACCEPTED; S0 authorized
[x] Next-session boundary recorded (122 REST audit, 123 runtime decisions)
[ ] S0 execution — deliberately NOT started here
~~~
