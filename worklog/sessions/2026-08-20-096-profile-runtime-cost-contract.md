# Session 096 — Profile Runtime Cost Contract

**Date:** 2026-08-20
**Result:** CONTRACT LOCKED (docs-only; no production code, no tests changed)
**Scope:** docs (architecture + product cross-reference) / worklog
**Model Route:** Claude / Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## 1. Goal

Define the canonical Profile V1 runtime collection contract that removes
unnecessary GitHub request amplification without changing Repository Proof
behavior or current Profile V1 semantic truth.

Contract only. No implementation.

## 2. Starting State

~~~text
Branch:
session/096-profile-runtime-cost-contract

Base HEAD:
d64c858 (feat(profile): add unified profile entry routing) — matches the
expected SHA given in the handoff

Pre-existing Working Tree:
CLEAN

Relevant Previous Session:
worklog/sessions/2026-08-20-095-p10-i2-profile-entry-routing.md
~~~

CURRENT.md still listed Session 095 as Active Session on
`session/095-p10-i2-profile-entry-routing`; the Session 095 commits are
contained in this branch's history, so `d64c858` is the correct Session 096
base. CURRENT was updated by this session.

## 3. Hypothesis Verification (before any doc change)

The handoff supplied six hypotheses. All six were verified against code and
canonical contracts before writing. **No contradiction was found.**

**A — Profile reuses the full single-repository snapshot collector: TRUE.**
`GitHubProfileRepositorySnapshotSource` delegates to
`IGitHubRepositorySnapshotCollector`, and
`ProfilePipelineServiceCollectionExtensions` registers that seam with the
container's `IGitHubRepositorySnapshotCollector`, whose production instance is
`GitHubRepositorySnapshotOrchestrator.CreateDefault` — the full Repository Proof
collector set including both enrichment collectors.

**B — Source collector: pinned tree lookup + bounded per-file retrieval, 32
slots: TRUE.** `GitHubRepositorySourceFileCollector.MaximumSourceFiles = 32`; it
resolves its own tree because the regular-blob rule needs the Git mode, which
`RepositorySnapshot.FileTree` does not carry, plus one content request per
consumed slot.

Corrected in the Session 096 correction pass (ChatGPT review Finding 2): the
tree lookup is **two** requests, not one — `CollectTreeAsync` first performs
`GET repos/{owner}/{repo}/git/commits/{headCommitSha}` to obtain the root tree
SHA, then `GET repos/{owner}/{repo}/git/trees/{rootTreeSha}?recursive=1`.
Theoretical ceiling: **34 requests per repository** (2 + 32).

**C — Dependency collector performs additional manifest content requests:
TRUE.** `MaximumManifestFiles = 32`, one content request per candidate not
already carried with content by the contents collector. Ceiling: 32 requests per
repository. Combined theoretical enrichment ceiling: **66 requests per admitted
repository** (34 + 32) — a theoretical maximum, not an observed average.

**D — Isolation tests prove both surfaces are non-scoring: TRUE.**
`RepositorySourceEvidenceIsolationTests` and
`RepositoryDependencyEvidenceIsolationTests` lock status, score breakdown, the 15
signal results, limitation keys, warning codes (and category results for source)
identical between the absent-surface baseline and the present-surface arm, and
lock that an `Unavailable` surface neither degrades `AnalysisStatus` nor adds
`limitation.snapshot.collection-partial`. Their baseline arm is exactly the
omission state, which is why the omission is provable rather than plausible.

Structural confirmation beyond the tests: `RepositoryAnalysisComposer`
`DetermineStatus` reads only `snapshot.CollectionWarnings` and signal statuses,
and `GitHubRepositorySnapshotOrchestrator.CollectWarnings` deliberately excludes
both enrichment surfaces from `CollectionWarnings`.

**E — CrossRepositoryEvidenceComposer does not consume the two surfaces: TRUE.**
It reads `RepositoryAnalysis.SignalResults`, the `languages` availability entry
with `RepositorySnapshot.Languages`, the admission/contribution assessment, and
`ProfileAnalysisOrchestrationResult.IsRepositoryManifestComplete` (which is
`Snapshot.Repositories.IsComplete` — repository-manifest completeness, unrelated
to per-repository enrichment). No read of `SourceFiles` or
`DependencyDeclarations`.

**F — ProblemAreaPatterns Unsupported, RoleEvidenceAlignment Deferred: TRUE.**
`ProfileReport` guards ProblemArea to empty at runtime; `ProfileReportProjector`
emits RoleEvidenceAlignment structurally empty.

Additional verification, not in the hypothesis list but decisive:

- `RepositorySemanticEvidencePolicy` and `RepositorySourceReferenceEvidencePolicy`
  are the only Domain consumers of the two surfaces and have **no `src/` caller
  at all** — the Session 076 finding still holds.
- `public-profile-evidence-domain-model.md` §13 defines the expected evidence
  surface as the obligations imposed by the eligibility, pattern and
  representative-repository contracts, and states that an Unsupported/Deferred
  capability is not an obligation and its absence is never a completeness
  deficit. Neither omitted surface is such an obligation, so the omission is
  provably not a completeness deficit.

## 4. Context Read

~~~text
worklog/CURRENT.md
docs/product/p10-web-profile-flow-implementation-plan.md
docs/architecture/public-profile-evidence-domain-model.md (§13)
docs/architecture/repository-source-evidence-collection.md (§19-§27)
docs/architecture/repository-dependency-evidence.md (headings + §4)
src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySnapshotOrchestrator.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositorySourceFileCollector.cs
src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryDependencyManifestCollector.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositorySnapshotSource.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositoryCollector.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileSnapshotOrchestrator.cs
src/GitHubProof.Application/Profile/ProfileRepositoryAnalysisOrchestrator.cs
src/GitHubProof.Application/Profile/CrossRepositoryEvidenceComposer.cs
src/GitHubProof.Application/Analysis/RepositoryAnalysisRunner.cs
src/GitHubProof.Application/Analysis/RepositorySignalAnalyzer.cs
src/GitHubProof.Application/Analysis/RepositoryAnalysisComposer.cs (status region)
src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs
tests/GitHubProof.Application.Tests/Analysis/RepositorySourceEvidenceIsolationTests.cs
tests/GitHubProof.Application.Tests/Analysis/RepositoryDependencyEvidenceIsolationTests.cs
~~~

## 5. Changes

~~~text
Created:
- docs/architecture/public-profile-runtime-collection.md (CONTRACT LOCKED, 14 sections)
- worklog/sessions/2026-08-20-096-profile-runtime-cost-contract.md

Modified:
- docs/product/p10-web-profile-flow-implementation-plan.md (header Amended line + new §13.17 only)
- worklog/CURRENT.md
- worklog/INDEX.md
~~~

No `.cs`, no `.csproj`, no `.cshtml`, no test file changed.

## 6. Locked Decisions

1. **Repository Proof unchanged.** The single-repository path stays the full
   repository evidence path; no collector, signal, score, evidence, availability
   or enrichment semantics changed (§3.1).
2. **Profile-specific composition allowed.** Profile Proof may use its own
   collection composition at the `IProfileRepositorySnapshotSource` seam. The
   composition may differ; the analysis semantics may not (§3.2, §3.3, §6).
3. **Safe V1 omissions.** Profile V1 must not collect bounded raw `SourceFiles`
   or `DependencyDeclarations` on the normal aggregation path. The list is
   closed; the omission never becomes negative evidence, a judgment, a warning, a
   limitation or a status change, and never touches Repository Proof (§5).
4. **No silent repository cap.** An invisible "top N repositories" optimization
   is prohibited; existing declared manifest bounds and completeness semantics
   remain authoritative (§7).
5. **No concurrency-as-cost-fix.** Request amplification and wall-clock latency
   are distinguished; broad `Task.WhenAll` fan-out is not authorized (§8).
6. **Runtime telemetry contract.** Minimum observable surface locked (duration,
   total request count, per-collector counts, repositories discovered/admitted/
   completed/incomplete, final status, rate-limit/forbidden/failed/cancellation),
   with hard prohibitions on tokens and any content payload, and a rule that
   telemetry never becomes evidence (§9).
7. **Execution guard contract.** Bounded end-to-end lifetime required before
   production beta; guard expiration is never `Complete` and never presents the
   completed-so-far subset as a whole profile; guard expiration and caller
   cancellation are distinct and must never be conflated in either direction; no
   new domain evidence status may be invented, and if the existing runtime result
   types cannot represent guard expiration truthfully the implementation must
   STOP and report a typed-boundary gap; guard is operational safety, not
   evidence; external configuration; no hidden subset fallback; numeric values
   chosen after §5 removal and §9 measurement. Bounded
   concurrent-profile-execution policy recorded as a production-beta requirement
   without authorizing queueing or distributed infrastructure (§10, §10.1,
   §11.2).
8. **Future re-activation rule.** Re-enabling either surface requires an explicit
   named consumer, bounded cost, documented scope, versioned/revalidated semantic
   effect and measured amplification. ProblemArea becoming Supported or
   RoleEvidenceAlignment becoming active requires a new contract review (§12).

## 6b. Correction Pass (ChatGPT Independent Review)

A bounded documentation-only correction pass followed the first contract lock.
Two findings, both verified against the code before correcting, plus one
encoding verification. No production code and no locked optimization decision
changed; the safe omission list remains exactly `SourceFiles` +
`DependencyDeclarations`.

**Finding 1 (HIGH) — execution-guard / cancellation contradiction: VALID,
corrected.** The first draft's §10 principle 2 required cancellation, deadline
and guard exhaustion to all resolve through typed availability/result semantics,
while §11 simultaneously stated that cancellation propagates as
`OperationCanceledException` and is not converted into a result. Current
production behavior is authoritative and confirms the §11 side:
`ProfileRepositoryAnalysisOrchestrator` rethrows `OperationCanceledException`
when its supplied token is cancelled (both around the collection call and around
the analysis call, plus a per-repository `ThrowIfCancellationRequested`), and
`LiveProfileReportService` passes the token through to the collector and to
`ProfilePipeline.RunAsync` with no cancellation-to-result conversion.

Corrected by separating the two concepts exhaustively:

- §11.1 **external / request cancellation** — current behavior, unchanged:
  `OperationCanceledException` propagates; no `RepositoryAnalysis`,
  `ProfilePipelineResult`, partial profile or failure profile is fabricated to
  convert a caller cancellation; not developer evidence.
- §11.2 **internal profile execution guard / deadline** — does not exist yet:
  distinct from caller cancellation in both directions; never `Complete`; never
  the completed-so-far subset presented as a whole profile; typed non-success at
  the outer runtime/Web boundary using the existing vocabulary only where that is
  truthful; no invented domain evidence status; and an explicit STOP-and-report
  rule for the implementation session if the existing runtime result types cannot
  represent it without semantic abuse.

§10 principles were renumbered (1-7) and §14 acceptance criteria 10-12 rewritten
so all three sections state the same rule. No production cancellation code was
touched.

**Finding 2 (MEDIUM) — source request ceiling off by one: VALID, corrected.**
`CollectTreeAsync` performs **two** requests, not one:
`GET repos/{owner}/{repo}/git/commits/{headCommitSha}` to resolve the root tree
SHA, then `GET repos/{owner}/{repo}/git/trees/{rootTreeSha}?recursive=1`.
Corrected everywhere in the Session 096 file set:

~~~text
SourceFiles ceiling            33  ->  34   (2 tree-resolution + 32 content)
Combined enrichment ceiling    65  ->  66   (34 + 32)
"1 pinned recursive tree request"  ->  "2 tree-resolution requests"
~~~

Both remain theoretical maxima, not observed averages. Corrected in
`docs/architecture/public-profile-runtime-collection.md` §2.2, this session file,
`worklog/CURRENT.md` and `worklog/INDEX.md`.

**Encoding verification: files were NOT corrupted.** Both new Session 096 files
are valid UTF-8, carry no BOM and no CR bytes, and contain zero mojibake byte
sequences (`â€`, `Â§`, `Ä±`, `ÅŸ`, `Ã…`). A codepoint census returns only
intended characters: the contract holds `U+00A7 §` ×20 and `U+2014 —` ×7; the
session file holds `§`, `—` and Turkish `ç ö ü ğ ı ş` only. The mojibake in the
review paste was a read/console decoding artifact (Windows PowerShell
`Get-Content` defaults to the ANSI codepage, not UTF-8), not file content. No
byte was rewritten to "repair" anything.

## 7. Contradictions Found

~~~text
Initial pass: none against the six handoff hypotheses.
Correction pass: 2 internal findings from the ChatGPT independent review
                 (1 HIGH self-contradiction in the new contract's own
                  §10/§11, 1 MEDIUM arithmetic error) — both valid, both
                  corrected; see §6b.
~~~

All six handoff hypotheses hold. No canonical contract required amendment beyond
the P10 cross-reference; product semantics, eligibility thresholds, pattern
promotion, representative selection and report section semantics were reviewed
and deliberately left unchanged.

## 8. Validation

~~~text
git diff --check
PASS — no whitespace error

Changed-doc contradiction scan (grep over the two changed canonical docs)
PASS — no clause asserts full-collector reuse, a hidden top-N cap,
       omission-as-evidence-absence, concurrency-as-request-reduction,
       or guard-exhaustion-as-Complete

Reconciliation against runtime behavior
PASS — ProfileRepositoryAnalysisOrchestrator, GitHubProfileRepositorySnapshotSource,
       both isolation test classes, CrossRepositoryEvidenceComposer and the P10
       plan all consistent with the new contract

dotnet build / dotnet test
NOT RUN — documentation-only session; no code file touched

Independent review (github-proof-review)
NOT RUN — not part of this session task's acceptance criteria; a ChatGPT
          independent review ran instead and its two findings are closed (§6b)
~~~

Correction pass re-validation:

~~~text
git diff --check (tracked)                    PASS — no whitespace error
git diff --check (untracked, via git add -N)  PASS — no whitespace error
                                              (index reset afterwards)
Trailing whitespace, tabs, CR bytes           0 / 0 / 0 in both new files
UTF-8 validity                                YES for both new files, no BOM
Mojibake byte sequences                       0 real; the single grep hit is
                                              this file's own deliberate
                                              quotation of the example
                                              sequences in §6b
Stale-number scan (33 / 65 / "1 pinned tree") PASS — only the deliberate
                                              before->after record in §6b and
                                              an unrelated pre-existing
                                              "satır 49-65" line reference in
                                              the P10 plan
§10 / §11 cancellation contradiction scan     PASS — §10 no longer routes
                                              cancellation through typed
                                              result semantics; §11.1 owns
                                              request cancellation, §11.2 owns
                                              guard expiration; §14 criteria
                                              10-12 match both
Reconciliation                                PASS — ProfileRepositoryAnalysisOrchestrator
                                              rethrow, LiveProfileReportService
                                              pass-through, CollectTreeAsync's
                                              two requests and
                                              GitHubRepositoryDependencyManifestCollector's
                                              32-manifest bound all re-read
No .cs/.csproj/.cshtml/test file changed      PASS
~~~

## 9. Problems

~~~text
None.
~~~

## 10. Not Done

~~~text
- No production code written; the Profile-specific composition is not implemented.
- No telemetry type, counter or event implemented.
- No execution guard implemented; no numeric guard value selected.
- No concurrency, queueing or distributed execution designed.
- No measurement of the actual per-profile request count (requires the telemetry
  of §9, which is Session 097 work).
~~~

## 11. Known Risks

~~~text
- Until the contract is implemented, a live Profile Proof run still pays up to
  66 extra GitHub requests per admitted repository (theoretical ceiling) and
  remains exposed to rate-limit and secondary-limit pressure on large accounts.
- Profile analysis still has no bounded end-to-end execution lifetime; this
  remains a production-beta blocker recorded by §10.
~~~

## 12. Working Set / Final Git State

~~~text
Modified:
- docs/product/p10-web-profile-flow-implementation-plan.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked:
- docs/architecture/public-profile-runtime-collection.md
- worklog/sessions/2026-08-20-096-profile-runtime-cost-contract.md

Pre-existing unrelated:
- NONE
~~~

## 13. Next

Session 097 — Profile runtime collection implementation, in this order:

1. Introduce a Profile-specific repository snapshot composition behind the
   existing `IProfileRepositorySnapshotSource` registration, omitting only the
   two §5 surfaces (no HTTP request issued for them), leaving
   `GitHubRepositorySnapshotOrchestrator` and the Repository Proof registration
   untouched.
2. Add the §6 semantic-equivalence test (same profile input through the omitting
   and the full composition yields the same `ProfileReport`, status, limitations
   and warnings) plus the §5.2 no-warning/no-limitation/no-status-change test.
3. Add the §9 telemetry counters and their prohibition test.
4. Measure the resulting per-profile request count, then propose the §10 guard
   numeric values from that measurement in a follow-up decision.

## 14. Git

~~~text
Suggested commit:
docs(profile): lock profile runtime collection contract

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

## 15. Completion Checklist

~~~text
[x] Goal tamamlandı (contract locked, implementation açıkça kapsam dışı)
[x] Hypotheses A-F doğrulandı, çelişki yok
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[ ] Independent review (github-proof-review) — NOT RUN; session task'ının
    acceptance criteria listesi review içermiyor ve bu oturumda subagent
    spawn'ı kısıtlı. Contract review'ı ayrı bir authorized adım olarak açık.
[x] Kullanıcıya kısa final rapor verildi
~~~
