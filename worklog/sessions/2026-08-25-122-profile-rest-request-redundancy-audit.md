# Profile REST Request Redundancy Audit

**Session:** 122
**Date:** 2026-08-25
**Route:** Opus / high
**Result:** PASS — read-only audit, verdict **REST-R3**

---

## Goal

Determine how many GitHub REST requests in the current Profile Proof collection
path are genuinely necessary to preserve the exact existing GitHub Proof truth,
and identify duplicate acquisition or safely reusable upstream data.

The governing rule for the whole session:

~~~text
FEWER GITHUB REQUESTS ARE ALLOWED.
LESS EVIDENCE IS NOT.
~~~

This is not an evidence-reduction task. No repository universe, admission rule,
required surface, signal, status, score, evidence key, pattern, representative,
warning, limitation or report content was changed or proposed for change.

---

## Starting State

~~~text
Branch       session/122-profile-rest-request-redundancy-audit
Base         3b77600  (== main == origin/main, verified with git rev-parse)
Working set  worklog only
Pre-existing ?? artifacts/  (user-owned, untouched, out of scope)
~~~

`worklog/CURRENT.md` declared `Active Session: NONE` and Session 121 CLOSED, so
there was no session collision. Session 121's sequencing decision named this
session as 122 and moved the runtime decision design to 123.

---

## Model Route

Opus / high, approved by the user before any inspection. Read-only audit with a
worklog-only mutation; the risk is a false-positive redundancy finding rather
than a broken build, and no escalation to `xhigh` proved necessary because the
candidate set collapsed to two on documented provider semantics.

---

## Context Read

~~~text
docs/architecture/public-profile-runtime-collection.md   (§2, §3, §4, §5, §6,
                                                          §7, §8, §14, §15)
worklog/sessions/2026-08-24-120-profile-runtime-performance-budget.md
                                                         (measured category table)
worklog/CURRENT.md
src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryMetadataCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryContentsCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryDescriptiveDataCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryCommitCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryIssueCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryPullRequestCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryReleaseCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryTagCollector.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileMetadataCollector.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositoryCollector.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileSnapshotOrchestrator.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositorySnapshotSource.cs
src/GitHubProof.Application/Profile/ProfileRepositoryAnalysisOrchestrator.cs
src/GitHubProof.Application/Profile/CrossRepositoryEvidenceComposer.cs
src/GitHubProof.Domain/Profile/AnalysisAdmissionPolicy.cs
src/GitHubProof.Domain/Analysis/Collection/RepositoryCollectionDecisionBoundary.cs
src/GitHubProof.Domain/Analysis/Signals/CommitDistributionSignalEvaluator.cs
~~~

The request graph was re-derived from source at `3b77600`. Session 120's numbers
are used only as the measured baseline for projection, never as the description
of the graph.

---

## 1. Session / baseline

Session 120's measured per-category request table is the arithmetic baseline.
It is reproduced here because every projection in §10 divides it.

~~~text
category                  torvalds   karpathy   fatihGunalp
profile-metadata                 1          1             1
profile-repositories             1          1             1
repository-metadata              9         54             4
repository-git-reference         9         54             4
repository-git-commit            9         54             4
repository-git-tree              9         54             4
repository-readme                9         54             4
repository-license               9         54             4
repository-languages             9         54             4
repository-commits              30         54             4
repository-issues                8         53             4
repository-pull-requests         9         59             4
repository-releases              9         54             4
repository-tags                  9         54             4
repository-contents              1          8            13
repository-contributors          0          0             0
other                            0          0             0
TOTAL                          131        662            63

admitted repositories            9         54             4
~~~

`repository-issues` is one short of the admitted count in two shapes because
`has_issues=false` already suppresses the request entirely — an existing
saving, not a new one.

## 2. Current REST request graph

Re-derived from source. The Profile path resolves its own composition through
`GitHubRepositorySnapshotOrchestrator.CreateProfileComposition` (eight
collectors; `SourceFiles`, `DependencyDeclarations` and `Contributors` are not
constructed at all), so no source-tree, source-content, dependency-manifest or
contributor request exists on this path.

### 2.1 Profile level — exactly 2 requests plus manifest continuations

~~~text
1  GET users/{login}
     -> GitHubProfileIdentity.Login (authoritative after redirect),
        GitHubProfileMetadata.PublicRepositoryCount, Name, CreatedAt, AvatarUrl
     -> also the sole D-2 budget observation point

2  GET users/{login}/repos?type=owner&sort=full_name&direction=asc&per_page=100
     (+ Link rel="next", bounded at DefaultMaxPageCount = 10 pages)
     -> ProfileRepositoryManifest + RepositoryPresentationMetadata
~~~

Admission (`AnalysisAdmissionPolicy`) runs on the manifest entry alone and
issues no request. Only `AnalysisAdmissionDecision.Analyze` reaches
per-repository collection, which is why the measured per-repository categories
equal the admitted count and not the manifest count.

### 2.2 Per admitted repository — 12 fixed requests plus fan-out

Order is the orchestrator's actual `await` order.

~~~text
 1  GET repos/{o}/{r}                                    metadata
 2  GET repos/{o}/{r}/git/ref/heads/{defaultBranch}       head SHA
 3  GET repos/{o}/{r}/git/commits/{headSha}               root tree SHA
 4  GET repos/{o}/{r}/git/trees/{rootTreeSha}?recursive=1 recursive tree
 5  GET repos/{o}/{r}/contents/{path}?ref={headSha}       project files   (Npf)
 6  GET repos/{o}/{r}/contents/{path}?ref={headSha}       workflows       (Nwf)
 7  GET repos/{o}/{r}/contents/{path}?ref={headSha}       docs Markdown   (Ndoc)
 8  GET repos/{o}/{r}/readme                              README
 9  GET repos/{o}/{r}/license                             license + path
10  GET repos/{o}/{r}/languages                           languages
11  GET repos/{o}/{r}/commits?sha={defaultBranch}&per_page=100   (+pages)
12  GET repos/{o}/{r}/issues?state=all&per_page=100              (+pages)
13  GET repos/{o}/{r}/pulls?state=all&per_page=100               (+pages)
14  GET repos/{o}/{r}/releases?per_page=100                      (+pages)
15  GET repos/{o}/{r}/tags?per_page=100                          (+pages)
~~~

Fixed floor per repository = 12 requests (rows 1-4, 8-15 with one page each),
minus 1 when `has_issues=false`. Rows 5-7 are the unbounded contents fan-out
(D4-E2). Rows 11-15 carry the §15 decision boundaries.

### 2.3 Where the graph is *already* minimal

Three existing mechanisms already remove requests and were verified, not
assumed:

- omitted surfaces — no contributor, dependency-manifest or source-file request
  is issued, and the collectors are never constructed;
- `has_issues=false` returns `NotApplicable` without issuing a request;
- §15 decision boundaries stop commits / issues / pulls / releases / tags as
  soon as the authorized decision is irreversible.

## 3. Data-lifetime map

What each network-obtained value is, where it is produced, and every consumer.

~~~text
value                 produced by                consumers
--------------------  -------------------------  ---------------------------
profile login         users/{login}              manifest URI, identity
public_repos          users/{login}              manifest completeness warning
manifest entry        users/{login}/repos        admission (Empty, Fork),
                                                 ProfileRepositoryReference,
                                                 presentation Description
default branch        repos/{o}/{r}              git/ref URI, commits ?sha=,
                                                 RepositoryIdentity
homepage              repos/{o}/{r}              HomepageLiveDemo signal
has_issues            repos/{o}/{r}              IssueUsage NotApplicable +
                                                 issue-collector gate
topics                repos/{o}/{r}              Topics signal
metadata availability repos/{o}/{r}              collection gate; snapshot
                                                 availability entry; written
                                                 into CommitDistribution
                                                 evidence VALUE as
                                                 metadataAvailability=...
head SHA              git/ref/heads/{branch}     snapshot.HeadCommitSha;
                                                 ?ref= of every content
                                                 request; consistency warning
root tree SHA         git/commits/{headSha}      the git/trees URI ONLY
recursive tree        git/trees/{sha}?recursive  FileTree; the three content
                                                 selections; truncated ->
                                                 FileTree.IsComplete
file size             recursive tree             docs Markdown bounds
file content          contents/{path}            project files, workflows,
                                                 docs Markdown
license path + size   license                    EvidenceItem.Location,
                                                 MergeFileTree overlay
~~~

Two lifetimes matter for this audit and both are already single-resolution:
the **default branch** is fetched once and reused by two collectors, and the
**head SHA** is fetched once and reused by every content request. Neither is
re-resolved over the network.

## 4. Request family matrix

~~~text
family                caller                       fields consumed          earlier equivalent?
--------------------  ---------------------------  -----------------------  -------------------
profile metadata      GitHubProfileMetadata        login, public_repos,     none (first request)
                      Collector                    name, created_at, avatar
profile manifest      GitHubProfileRepository      name, owner.login,       none
                      Collector                    private, fork,
                                                   is_template, archived,
                                                   size, language,
                                                   default_branch,
                                                   pushed_at, created_at,
                                                   description
repository metadata   GitHubRepositoryMetadata     name, owner.login,       PARTIAL — manifest
                      Collector                    private, archived,       lacks homepage,
                                                   fork, is_template,       has_issues, topics
                                                   language,                in the parsed DTO
                                                   default_branch,
                                                   description, homepage,
                                                   created_at, has_issues,
                                                   topics
git ref               ContentsCollector            object.sha               none
git commit            ContentsCollector            commit.tree.sha          none
recursive tree        ContentsCollector            tree[].path/type/size,   none
                                                   truncated
content (x3)          ContentsCollector            type, path, content,     none
                                                   encoding
readme                DescriptiveDataCollector     path, content, encoding  none
license               DescriptiveDataCollector     path, size, license.*    metadata carries
                                                                            license.* but NOT
                                                                            path or size
languages             DescriptiveDataCollector     {language: bytes}        none
commits               CommitCollector              sha, committer.date      none
issues                IssueCollector               number, title, body,     none
                                                   state, created_at,
                                                   pull_request (filter)
pull requests         PullRequestCollector         number, title, state,    none — /issues PR
                                                   merged_at, created_at    stubs carry no
                                                                            merged_at
releases              ReleaseCollector             tag/name/body/draft/     none
                                                   prerelease/published_at
tags                  TagCollector                 name                     none
~~~

## 5. R0 / R1 / R2 / R3 / R4 classification

~~~text
R0  NOT REDUNDANT
      profile metadata            login is authoritative after redirect;
                                  public_repos owns the completeness warning;
                                  sole D-2 budget observation point
      profile manifest            the repository universe itself
      git ref                     only source of the head SHA *with the
                                  current failure and empty-repository
                                  semantics* — the commits surface carries a
                                  second in-execution copy, but not those
                                  semantics (see R4 below and §7.3)
      git commit                  only source of the root tree SHA (see §8-A)
      recursive tree              only source of paths, sizes and truncated
      contents x3                 bytes; nothing earlier carries content
      readme                      provider-side preferred-README resolution
      license                     only source of the license PATH and size
      languages                   only source of the byte map
      commits                     §15.4: no provider field establishes the
                                  distinct commit-day count
      issues                      qualifying-issue prefix
      pull requests               merged_at exists only here
      releases                    published-with-notes prefix
      tags                        tag names; releases do not imply tags

R1  EXACT DUPLICATE ACQUISITION
      LATENT, CONDITIONAL — one real cross-selector overlap exists in source
      (jest.config.* / vitest.config.* project-file prefixes vs the workflow
      and documentation selectors). Zero incidence in all three measured
      benchmark shapes. See §6.

R2  SAFE UPSTREAM REUSE
      NONE that survives the parity bar. See §7.

R3  APPARENT DUPLICATE, SEMANTICALLY DIFFERENT
      git/trees keyed by branch instead of the pinned tree SHA
      /issues used as a pull-request source
      /releases used as a tag source
      metadata.license used as the license surface
      readme vs docs/*.md byte overlap (unproven, see §8-D)

R4  CONTRACT DECISION REQUIRED
      repository metadata seeded from the manifest entry
      head SHA taken from the commits surface instead of git/ref (§7.3)
      count-based substitution for any paginated surface (§15.4 already
      rejected the Link rel="last" variant explicitly)
~~~

## 6. Proven duplicate acquisition

**One latent exact duplicate exists in source. Its incidence in all three
measured benchmark shapes is zero.**

This section originally claimed the three content selectors were provably
disjoint and that R1 was empty. Independent review falsified that proof and the
counterexamples were re-verified in source. The corrected finding follows.

### 6.1 The latent cross-selector duplicate

`IsRelevantProjectFile` is **not** a closed filename set. Two of its rules are
open-ended prefix matches with no extension constraint
(`GitHubRepositoryContentsCollector.cs`):

~~~csharp
|| fileName.StartsWith("jest.config.", StringComparison.OrdinalIgnoreCase)
|| fileName.StartsWith("vitest.config.", StringComparison.OrdinalIgnoreCase);
~~~

So a path can satisfy two selectors at once:

~~~text
.github/workflows/jest.config.yml     -> project file AND workflow
.github/workflows/vitest.config.yaml  -> project file AND workflow
docs/jest.config.md                   -> project file AND documentation Markdown
docs/vitest.config.md                 -> project file AND documentation Markdown
~~~

And the overlap really costs a request. `CollectSelectedFilesAsync` iterates
`fileTree.Where(selection)` independently for the project-file and workflow
selectors, and `CollectDocumentationMarkdownAsync` walks its own candidate list;
all three call `CollectFileContentAsync` per match. The `observedPaths` set
deduplicates only *within the tree parse*, never *across selectors*. An
overlapping path therefore issues two byte-identical
`GET repos/{o}/{r}/contents/{path}?ref={headSha}` requests, and the same file is
placed into both collections.

That is exactly the R1 shape this audit set out to find, and it is in the source
at `3b77600`.

### 6.2 What that does and does not mean

- It is **latent**: `repository-contents` totals in Session 120 were 1 / 8 / 13
  and no overlapping path occurred, so the measured request counts contain zero
  duplicates of this kind.
- Its ceiling is one extra request per overlapping path, and such paths are
  unusual — a Jest or Vitest config living under `.github/workflows/` or `docs/`.
- Removing it is a **source correctness fix**, not a redundancy optimization:
  its projected saving on every measured shape is 0.
- It is recorded, not fixed. Production mutation is outside this session's scope,
  and it warrants its own bounded session.

### 6.3 Everything else checked for an exact repeat

- the recursive tree deduplicates paths (`observedPaths.Add(file.Path)`), so no
  path is selected twice *inside* one selector;
- `readme`, `license` and `languages` are distinct endpoints issued once each;
- the five paginated surfaces are distinct endpoints, each page issued once;
- the manifest deduplicates repositories by full name, and
  `ProfileRepositoryAnalysisOrchestrator` iterates it once, so no repository is
  collected twice in a run.

## 7. Proven safe upstream reuse

**None.**

Two candidates were examined seriously and neither passed the parity bar.

### 7.1 `git/commits/{headSha}` — the strongest structural candidate, REJECTED

The root tree SHA it returns has exactly one consumer: the `git/trees` URI. It
is never stored, never surfaced and never read again. Structurally it is pure
indirection, and Session 120 §2.2 describes the same two-step.

It cannot be removed, on documented provider semantics:

> "The SHA1 value or ref (branch or tag) name of the tree."
> — *Get a tree*, docs.github.com/en/rest/git/trees, read 2026-08-25

The `tree_sha` parameter is documented for a tree SHA, a branch name or a tag
name — **not** a commit SHA. So `git/trees/{headSha}?recursive=1` is not a
documented substitution and must not be assumed.

Keying the tree on the *branch name* is documented, and would remove both the
ref and the commit request, but it breaks a real invariant: today the chain is
SHA-pinned, so the tree is guaranteed to be the tree of `headCommitSha`, and
every content request is issued with `?ref={headCommitSha}`. A branch-keyed
tree can belong to a different commit if the branch moves between the two
requests, producing a `FileTree` that does not match the `HeadCommitSha` written
into the snapshot and into the atomicity warning. That is **R3**, not a saving.

### 7.2 Repository metadata seeded from the manifest — R4, not R2

The manifest already returns most of what `repos/{o}/{r}` is parsed for. Three
consumed fields are absent from the manifest DTO — `homepage`, `has_issues`,
`topics` — and all three are load-bearing:

- `homepage` -> `HomepageLiveDemoSignalEvaluator`;
- `has_issues` -> `IssueUsage` `NotApplicable` and the issue-collector gate;
- `topics` -> `TopicsSignalEvaluator`.

Provider documentation says the fields exist in the list schema:

> `GET /users/{username}/repos` returns the **Minimal Repository** schema, in
> which `homepage`, `has_issues` and `topics` are present but **optional**.
> — docs.github.com/en/rest/repos/repos, read 2026-08-25

So the *field* objection is soluble. The **availability** objection is not, and
it is what blocks the reuse:

1. `metadata.Availability` is the collection **gate**. `!IsAvailable` returns a
   null snapshot and produces `CollectionUnavailable` with the `metadata` key.
   Seeding removes the only per-repository probe of that state, so a repository
   deleted, renamed, made private, 403'd or rate-limited between the manifest
   and its collection would no longer be detected as a `metadata` outcome — the
   next request would fail instead, under the `file-tree` key. Different
   availability key, different warning, different `RepositoryAnalysis` inputs.
2. A repository that turns private mid-run is `metadata` / `NotApplicable`
   today; seeded, it would become `file-tree` / `NotFound`.
3. The `metadata` availability state is written into evidence **value**:
   `CommitDistributionSignalEvaluator` emits `metadataAvailability={state}`.
4. `retrievedAt` would become the manifest's timestamp rather than a
   per-repository one, changing the source identity of the entry.

Points 1-2 are failure-truth divergences, which the session's own rule forbids
trading. Removing the request is therefore possible only with a canonical
amendment defining manifest-seeded metadata availability — **R4, out of scope
for this session**, and a Session 123 / contract question.

### 7.3 Head SHA from the commits surface — R4, not R2 and not R0-absolute

Independent review correctly challenged the original absolute R0 for `git/ref`.
A second in-execution copy of the head SHA does exist: the commits collector
issues `commits?sha={defaultBranch}&per_page=100` and `NormalizeCommit` parses
`response.Sha`, so the first element of the first page is the default branch's
head commit. The seam to exploit it is already present — `ContentsCollector`
accepts a `contentReference` and skips the `git/ref` request entirely when it is
non-null; the Profile path simply always passes null.

It is still not an R2, for three reasons, and the third is the same objection
this audit used to reject metadata seeding in §7.2:

1. **Ordering.** Commits would have to run before contents, changing the
   availability-entry order and the `capturedAt` relationship.
2. **Failure-truth relocation.** A `git/ref` failure produces the `file-tree`
   availability key today. Sourcing the SHA from commits moves that failure to
   the `commits` key — a different key, a different warning, different
   `RepositoryAnalysis` inputs.
3. **Empty-repository detection.** `git/ref` answering 409 with the exact body
   `"Git Repository is empty."` is what produces `TreeResult.Empty` and the
   all-Available empty result. The commits collector has its own 409 handling,
   but whether the two are equivalent for every empty-repository shape is
   **unverified** (§14.5 records the probe that would settle it).

So the correct classification is **R4 — contract decision required**, at the same
magnitude as the §7.2 metadata route (~1 request per admitted repository). No
implementation is proposed here, and the §10 proven-removable count stays 0.

## 8. False-positive duplicates

Five things that look like redundancy from the request list and are not.

**A. The ref -> commit -> tree chain is not a redundant hop.** It is the only
documented SHA-pinned route to a recursive tree. Removing the middle request
requires an undocumented provider behaviour; removing it by keying on the branch
breaks head/tree atomicity. Recorded because it is the most tempting entry in
the whole graph.

**B. The content selectors are *mostly* disjoint — and the exception is real.**
This entry originally claimed full disjointness; independent review falsified it
and §6.1 now carries the corrected finding. The accurate statement is: the
`.github/workflows/` and `docs/` prefixes are mutually exclusive, and every
*fixed-name* project-file rule is extension-incompatible with both other
selectors — but the two open-ended prefix rules (`jest.config.`,
`vitest.config.`) are not, and they can genuinely produce a duplicate content
request. In the three measured shapes the fan-out (1 / 8 / 13) was entirely
distinct-path work, so no duplicate was actually paid.

**C. `/issues` is not a pull-request source.** It does interleave pull requests,
and the collector filters them out — but the interleaved stubs carry no
`merged_at`, which `PullRequestUsageSignalEvaluator` requires, and the issues
collection is a `DecisionBounded` prefix that stops at two qualifying issues.
Deriving pull-request evidence from it would be strictly weaker evidence.

**D. README vs `docs/*.md` is an unproven overlap, deliberately left unproven.**
If GitHub's preferred-README resolution can select a file under `docs/`, then a
repository whose only README lives there would fetch the same bytes twice —
once via `/readme`, once via `contents/docs/README.md`. GitHub's documentation
does not state which locations the preferred-README search covers (read
2026-08-25), so this is **not** established. It is recorded as unverified rather
than assumed. Even if true, the ceiling is one request per admitted repository
in a rare shape, and realising it would require running the descriptive
collector before the contents collector — reordering the availability list and
the `capturedAt` relationship for a saving that rounds to zero in all three
measured shapes. Not pursued.

**E. Repository identity re-parsing is CPU, not network.**
`GitHubRepositoryUrl.Parse` runs per repository and `RepositoryIdentity` is
rebuilt per collection, but the default branch and head SHA are each resolved
over the network exactly once and then passed through the collector arguments.
There is no shared-context gap that costs a request.

## 9. Minimum architecture seam

**None is justified, and none is proposed.**

A `ProfileRepositoryCollectionContext` was evaluated against the actual
findings. Its purpose would be to carry already-observed provider values —
manifest entry, identity, default branch, head SHA, tree SHA, tree, fetched
content — forward between collectors. Every one of those values is *already*
single-resolution and already passed forward through explicit arguments:

- the default branch travels in `GitHubRepositoryMetadataCollection`;
- the head SHA and the tree travel in `GitHubRepositoryContentsCollection`;
- content is fetched once per distinct path.

The context would therefore remove **zero** requests. Building it would add a
Profile-specific object, widen the collector signatures and create exactly the
generic-cache shape the session brief prohibits, in exchange for nothing.

## 10. Projected request counts

Proven R1 + R2 removals = **0 requests**.

~~~text
shape          current   proven removable   optimized REST projection
torvalds           131                  0                         131
karpathy           662                  0                         662
fatihGunalp         63                  0                          63
~~~

For the architecture owner, the ceiling of the *contract-amendment* route (§7.2,
R4) is stated separately and is **not** a finding of this session:

~~~text
shape          current   metadata seeding (R4)   would-be projection   reduction
torvalds           131                       9                   122       6.9%
karpathy           662                      54                   608       8.2%
fatihGunalp         63                       4                    59       6.3%
~~~

No latency claim is made or implied. These are request counts only; no
"N fewer requests = N% faster" conversion is asserted anywhere in this record.

## 11. Semantic-equivalence proof plan

There is no R1/R2 removal candidate to prove, so no proof plan is owed for this session.
Recorded for whoever takes the §7.2 R4 route later, the bar would be, per
admitted repository: identical `RepositorySnapshot` data for current consumers,
identical availability entries **including the `metadata` key and its state
string**, identical collection warnings, identical `RepositoryAnalysis.Status`,
identical score breakdown, identical 15 `SignalResults`, identical evidence keys,
identical limitations, warnings and categories; and at profile level identical
admitted set, contribution assessments, observed evidence, promoted patterns,
representatives, status, warnings, limitations and `ProfileReport`. Crucially it
would also need negative-path proof: a repository that becomes unavailable
between manifest and collection must still produce the same typed outcome.

## 12. Complexity / risk assessment

~~~text
finding                      implementation cost   risk            worth it?
metadata seeding (R4)        contract amendment    failure-truth   NO, not here
                             + availability model  regression
git/commits removal          undocumented provider high            NO
                             behaviour
branch-keyed tree            small                 breaks head/    NO
                                                   tree atomicity
collection context seam      medium                new abstraction NO — saves 0
readme/docs overlap          reordering + probe    changes         NO — ~0
                                                   ordering
~~~

The dominant risk this session was producing a false positive, not missing an
optimization. Two candidates that read as obvious redundancy from the request
list — the commit->tree hop and the metadata/manifest overlap — both survived
first inspection and failed on documented provider semantics and on availability
truth respectively.

## 13. Verdict

~~~text
REST-R3 — NO MATERIAL REDUNDANCY
~~~

The current Profile REST request graph carries **no removable duplicate
acquisition**: the one exact duplicate that exists (§6.1) is latent, had zero
incidence in all three measured shapes, and removing it saves zero requests —
it is a source correctness defect, not a redundancy saving. There is also no
safe upstream reuse that preserves availability and failure truth. The
12-request per-repository floor is 12 distinct provider facts, and the three
existing reduction mechanisms — omitted surfaces, the `has_issues` gate and the
§15 decision boundaries — have already taken the reductions that were available
without a contract change.

The verdict is stated from absolute cost, not a pre-invented percentage. Even
the most permissive route in the audit, the R4 metadata amendment, removes
6.3-8.2% of requests, requires a canonical availability amendment, and trades
away per-repository failure detection. That cost/benefit is what decides it: a
single-digit request reduction does not justify a canonical availability
amendment or a new abstraction.

No claim is made here about what that reduction would do to wall clock. This
record measured request counts only; translating a request percentage into a
latency percentage would need its own measurement, which this session did not
take and did not attempt to infer.

REST-only redundancy reduction therefore does **not** justify an implementation
slice before GraphQL or concurrency are compared.

## 14. Recommended next slice

1. **Do not build** `ProfileRepositoryCollectionContext` or any REST redundancy
   slice. There is nothing for it to remove.
2. The runtime question returns to the two tracks that were already open, and
   this audit removes the "cheap REST win first" alternative from between them:
   the accepted **GQL-F2** path gated on S0, and bounded concurrency, which §8
   of the canonical contract permits only after unnecessary requests are removed
   — a condition this audit shows is satisfied for every request that carries a
   distinct provider fact. The one open item, the §6.1 latent duplicate, is a
   correctness defect worth 0 requests and does not hold that condition back.
3. **Session 123 — Profile Production Runtime Decision Design** (D-1, D-2, D-3,
   D-5) is unaffected and remains the next scheduled session. The §7.2 R4
   metadata-seeding question may be recorded there as an input, but it is a
   contract question, not a performance slice.
4. **The §6.1 latent cross-selector duplicate deserves its own bounded session.**
   It is a source correctness defect — one file fetched twice and written into
   two collections — not a performance item, and its projected saving on every
   measured shape is 0. It is recorded here and deliberately not fixed, because
   production mutation is outside this session's scope.
5. Three live REST probes are *available* but **not requested**, because none of
   them changes this verdict: the empty-repository 409 body on
   `repos/{o}/{r}/commits/{branch}` (also the §7.3 blocker), the same question
   for the commits collector's own 409 handling, and GitHub's preferred-README
   search locations. All are recorded as unverified rather than assumed.

## 15. Git / worklog status

~~~text
Branch  session/122-profile-rest-request-redundancy-audit
Base    3b77600
Scope   worklog only; src/, tests/ and docs/ unchanged; artifacts/ untouched
Commit  PENDING — user-owned
Push    PENDING — user-owned
~~~

---

## Verified provider facts (source + date)

All read 2026-08-25. Recorded with their source, not canonicalized.

| Fact | Source |
|---|---|
| *Get a tree* `tree_sha`: "The SHA1 value or ref (branch or tag) name of the tree." — no commit SHA documented | docs.github.com/en/rest/git/trees |
| `GET /users/{username}/repos` returns the **Minimal Repository** schema; `homepage`, `has_issues` and `topics` are present but optional | docs.github.com/en/rest/repos/repos |
| *Get a commit* `ref` accepts a commit SHA, branch or tag; the response carries `sha` and `commit.tree.sha` **and** a `files` diff array, paginated beyond 300 files up to 3000 | docs.github.com/en/rest/commits/commits |
| *Get a reference* and *Get a commit* both document 409, but neither documents the response body | docs.github.com/en/rest/git/refs, /commits/commits |
| GitHub does not document which locations the preferred-README search covers | docs.github.com/en/rest/repos/contents |

---

## Changes

~~~text
Created:
- worklog/sessions/2026-08-25-122-profile-rest-request-redundancy-audit.md

Modified:
- worklog/CURRENT.md   (Session 121 reconciled as integrated at 3b77600;
                        Session 122 recorded CLOSED and Active Session set to
                        NONE; REST-R3 carried into Position; next-session
                        sequencing 123)
- worklog/INDEX.md     (Session 121 Commit backfilled to 3b77600;
                        Session 122 row added)

Deleted:
- none

Production code, tests, canonical docs/, artifacts/: UNCHANGED.
~~~

---

## Decisions

~~~text
- Verdict REST-R3, not REST-R2: the proven removable set is empty, not small.
- git/commits retained: the trees endpoint is not documented to accept a commit
  SHA, and branch-keying breaks head/tree atomicity.
- Repository metadata retained: classified R4, not R2, because the blocker is
  canonical availability semantics rather than missing fields — the fields are
  documented as present in the manifest schema.
- No collection-context abstraction proposed, because it would remove zero
  requests.
- Two live probes identified and deliberately NOT requested; the facts are
  recorded as unverified rather than assumed.
- After independent review: the R1 claim was corrected from "zero, provably
  disjoint" to "one latent cross-selector duplicate, zero measured incidence",
  and head-SHA-from-commits was added as an R4 route. Neither changes the
  proven-removable count, which stays 0, or the REST-R3 verdict.
~~~

---

## Validation

~~~text
Request graph re-derived from source at 3b77600     PASS
Session 120 category table reconciled to the graph  PASS (12-request floor,
                                                    has_issues gap explained)
Content-selector overlap analysis                   FAIL then CORRECTED — the
                                                    first disjointness proof was
                                                    falsified by independent
                                                    review; §6.1 now records the
                                                    real latent overlap
Provider documentation fetch, 2026-08-25            PASS (sources listed above)
dotnet build / dotnet test                          NOT RUN — no code changed
git diff --check                                    PASS
git status --short                                  worklog-only + pre-existing
                                                    artifacts/
~~~

---

## Independent Review

~~~text
Result:   PASS WITH FINDINGS
Reviewer: independent github-proof-review pass (forked context), 2026-08-25
Scope:    this record + branch/base verification
Outcome:  REST-R3 unchanged; 1 HIGH, 2 MEDIUM, 1 LOW; proven-removable stays 0
~~~

The reviewer re-derived the per-repository request list from source rather than
accepting this record, and confirmed independently: the §10 projection
arithmetic against Session 120's table, the §7.1 atomicity argument
(`rootTreeSha`'s single consumer, `HeadCommitSha` reaching the rendered report),
all four §7.2 availability claims, the license R0, the eight-collector Profile
composition, the §15.4 quotations, and that no canonical invariant was proposed
for change and no evidence reduction occurred.

It also falsified one claim. The findings and their corrections:

| # | Sev | Finding | Correction applied |
|---|---|---|---|
| F1 | **HIGH** | §6 claimed the three content selectors were provably disjoint and R1 was empty. False: `IsRelevantProjectFile` carries two open-ended prefix rules (`jest.config.`, `vitest.config.`) with no extension constraint, so `.github/workflows/jest.config.yml` and `docs/vitest.config.md` match two selectors each. `CollectSelectedFilesAsync` iterates per selector and `observedPaths` dedupes only within the tree parse, so an overlapping path issues two byte-identical content requests. | §6 rewritten as §6.1–§6.3 recording the latent duplicate, its zero incidence in the measured shapes, and that removing it is a correctness fix worth 0 requests. §5's R1 line and §8-B corrected to match. |
| F2 | MEDIUM | `git ref` was classified R0 absolutely, but the commits collector parses `response.Sha` and the `contentReference` seam already exists, so a second in-execution head-SHA source is available. | §7.3 added classifying it **R4** on ordering, failure-key relocation and unverified empty-repository equivalence. §5's R0 rationale narrowed to "with the current failure and empty-repository semantics"; R4 row added. No implementation proposed. |
| F3 | MEDIUM | The Changes block and checklist declared `CURRENT.md`/`INDEX.md` already modified while git showed both unmodified — the packet claimed work it had deliberately deferred. | Both files updated truthfully at closure, including retiring the stale "Session 121 awaiting integration / last integrated 59a7c30" state now that `3b77600` is on `main`. Changes block and checklist reconciled to what actually happened. |
| F4 | LOW | §13 inferred that "a 7% request reduction cannot move" the 95.3–99.5% collection share — an inference, not a measurement, and arguably backwards. | The inference was removed. §13 now decides on cost/benefit alone and states explicitly that no request-to-latency translation is claimed. |

### Re-review

The corrections were re-reviewed independently in the same session. The re-review
confirmed F2, F3 and F4 complete and correct and the F1 evidence sound, but
returned **FAIL** because the F1 correction had not been propagated: §13 still
asserted "no exact duplicate acquisition at all", contradicting §6.1; the
Validation block still recorded the falsified disjointness proof as PASS; and a
§14 cross-reference had gone stale when a list item was inserted. All three were
corrected, along with an imprecise attribution of the documentation-Markdown
fetch path. Recorded rather than erased: the first pass at the F1 fix was
incomplete, and the load-bearing verdict paragraph was where it was missed.

The first reviewer additionally noted that §7.2 **under**-claimed: the `metadata`
availability key also appears in `HomepageLiveDemoSignalEvaluator` and
`UsageDemoSignalEvaluator` `requiredDataKeys`. That strengthens the R4
classification rather than changing it, and no correction was required.

---

## Problems

~~~text
- None blocking. Provider semantics that could not be settled from
  documentation — the empty-repository 409 body on
  repos/{o}/{r}/commits/{branch}, the equivalent question for the commits
  collector's own 409 handling, and GitHub's preferred-README search locations —
  are recorded as unverified. None changes the verdict, so no live probe was
  requested.
- The first draft of §6 asserted a disjointness proof that was wrong. It was
  caught by independent review, re-verified in source and corrected rather than
  quietly softened; the original claim and its falsification are both on record.
~~~

---

## Not Done

~~~text
- No production, test or docs mutation.
- No collection-context abstraction, no GraphQL, no concurrency, no cache, no
  ETag, no retry, no new package.
- No live GitHub request of any kind.
- Session 123 not started.
~~~

---

## Known Risks

~~~text
- The §7.2 R4 route stays open and will look attractive again from the request
  table alone. Its blocker is availability truth, not field coverage, and that
  must be re-read before it is reconsidered.
- The contents fan-out (rows 5-7) remains unbounded in source; this audit found
  no redundancy in it, which is not the same as finding no risk. D4-E2 stands.
- Provider schemas can change; the facts above carry a 2026-08-25 source date.
~~~

---

## Next

Architecture-owner decision on the REST-R3 verdict. If accepted, no REST
implementation slice is opened, and the next session is **Session 123 — Profile
Production Runtime Decision Design** (D-1, D-2, D-3, D-5) through the normal
model/effort gate. **S0** — the §8 GraphQL partial-data/error availability
mapping amendment — remains separately authorized from Session 121 and is still
the gate for any GraphQL slice.

---

## Git

~~~text
Suggested commit:
docs(worklog): record profile REST request redundancy audit

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Read-only audit; no production mutation
[x] Request graph re-derived from source, not copied from Session 120
[x] Every request family mapped to caller, fields and downstream consumer
[x] Data-lifetime map produced
[x] R0-R4 classification applied to every family
[x] Duplicate-acquisition claim re-derived in source; the latent cross-selector
    overlap is recorded rather than claimed away (§6.1)
[x] Provider facts recorded with source and date; unverified left unverified
[x] Projections stated as request counts only, never as seconds; no
    request-to-latency inference retained
[x] Verdict recorded (REST-R3)
[x] Independent review of this packet — PASS WITH FINDINGS (1 HIGH, 2 MEDIUM,
    1 LOW)
[x] All four review findings (F1-F4) corrected in this session; the re-review's
    three follow-ups on the F1 propagation (§13, Validation line, cross-
    reference) were corrected in the same session
[x] INDEX row added; CURRENT reconciled; Active Session set to NONE at closure
[ ] Architecture-owner decision on the verdict
[ ] Separate bounded session for the §6.1 source defect — NOT opened here
~~~
