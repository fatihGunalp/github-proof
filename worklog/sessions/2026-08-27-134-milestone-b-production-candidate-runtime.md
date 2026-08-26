# Milestone B Production-Candidate Runtime Configuration + Exit Validation

**Session:** 134
**Date:** 2026-08-27
**Result:** PARTIAL — the Session 133-accepted R5 production candidate is now
wired into a real, discoverable production configuration surface
(`appsettings.Production.json`), proven deterministically through the actual
ASP.NET Core file-loading path (not an in-memory test double), with U and
GraphQL left untouched at their already-correct default values. Local
production-candidate smoke (deterministic, no network) confirms the
configuration genuinely reaches the production orchestrator composition and
bounds concurrency at exactly 5 with no repository-count cap. Independently
closure-reviewed: **PASS WITH FINDINGS, critical findings NONE** — the
configuration implementation is **technically accepted**; the review's
findings were worklog/record-accuracy issues only (an invented Result status,
stale current-state wording), corrected in a same-session pass recorded below.
**The session's lifecycle result is `PARTIAL`, not `PASS`**, solely because
authenticated live validation — an explicit part of the Session 134 objective
— is **BLOCKED**: `GITHUB_TOKEN` is not present in this session's environment.
Milestone B closure is **NOT CLAIMED**; several exit gates (VPS deploy, remote
production verification, production telemetry/monitoring infrastructure, live
smoke under this exact configuration) remain open and outside this session's
locked scope.
**Scope:** web / test / worklog (configuration only — no domain, application
or infrastructure source changed)
**Model Route:** Sonnet / high
**Model Gate:** APPROVED — supplied task explicitly records `USER APPROVAL: devam et`

---

## Goal

Apply the Session 133-accepted Milestone B production-candidate runtime value
(R=5, U=1, GraphQL default OFF) to a real production configuration surface,
prove deterministically that it resolves and actually reaches the production
runtime composition, and perform the production-candidate smoke/exit review
this task locks — without deciding any of the runtime questions explicitly
reserved for a separate owner decision (provider floor, execution deadline,
credential architecture, visitor rate-limit, GraphQL enablement, R>5/U>1).

---

## Starting State

~~~text
Branch:
session/134-milestone-b-production-candidate-runtime

Base HEAD:
53991ac36486f47113950e26224c0b3d361acfd4

Pre-existing Working Tree:
?? artifacts/

Relevant Previous Session:
worklog/sessions/2026-08-26-133-milestone-b-runtime-gates.md
~~~

`main`, `origin/main` and `HEAD` were all verified equal to `53991ac` — the
exact commit and message (`test(profile): close Milestone B runtime gates with
R5 evidence`) the task expected — before the branch was created. `artifacts/`
is user-owned and out of scope.

---

## Model Route

~~~text
Selected:
Sonnet / high

User Approval:
devam et (recorded explicitly in the supplied task; model set to
claude-sonnet-5 in the same turn)
~~~

---

## Context Read

- `AGENTS.md`
- `CLAUDE.md`
- `agents/02-context-loading.md`
- `agents/03-project-contract.md`
- `agents/04-development-workflow.md`
- `agents/06-review-and-validation.md`
- `agents/07-worklog-integration.md`
- `worklog/CURRENT.md`
- `worklog/INDEX.md`
- `worklog/sessions/2026-08-26-133-milestone-b-runtime-gates.md`
- `docs/architecture/public-profile-runtime-collection.md` — §10.1.1
  (Milestone B latency/burst contract), §10.5 (deployment dependency and
  numeric boundary), §10.2-10.4 (execution guard/budget boundary)
- `docs/roadmap/deployment-milestones.md` — Milestone B production gate and
  exit condition (full list)
- `docs/roadmap/mvp-phases.md` — Milestone B clarification

---

## Contract / Invariants Checked

- R governs bounded repository-worker concurrency only; it never becomes a
  repository-count cap, a sampling rule or a top-N selection (§7, §10.1.1).
  Every admitted repository is still analyzed regardless of R.
- U (`ProfileAdmissionOptions.GlobalConcurrency`), the same-profile guard and
  the GitHub reserve floor are separate dimensions this session does not own
  and does not change.
- GraphQL stays default OFF; the Session 126 secondary-limit residual is
  unchanged and no GraphQL switch may become production/beta/default-on this
  session.
- Configuration is external (§10 guard principle 5): the selected value must
  live in configuration, never in a domain rule, policy version or evidence
  semantics — and, per this task's explicit preference, not in the C# source
  default either, unless proven necessary.
- No product/domain/architecture decision is made silently: provider floor,
  execution deadline, credential architecture, visitor rate-limit/trusted-proxy
  topology and R>5/U>1 all remain open owner decisions.

---

## Configuration Ownership — What Was Actually Found

The repository has **no `appsettings.json` of any kind** anywhere before this
session (verified: no file matching `appsettings*.json` under `src/`, no
`docker-compose`, systemd unit, CI/CD workflow, or other deploy artifact in the
tree). `Program.cs` calls the standard `WebApplication.CreateBuilder(args)`,
which is the plain ASP.NET Core default configuration pipeline: appsettings
JSON files (optional), then environment variables, then command-line
arguments. Every existing options section
(`ProfileCollectionOptions`, `ProfileAdmissionOptions`,
`ProfileGitHubBudgetProtectionOptions`, the two GraphQL options types) is bound
through `IConfiguration.GetSection(...)`, exactly the mechanism a JSON
configuration file feeds.

There was therefore no existing in-repo "production configuration" artifact to
edit — only the standard, unused `appsettings.{Environment}.json` convention
the SDK already supports. This session adds exactly one file,
`src/GitHubProof.Web/appsettings.Production.json`, selecting only the one
value Session 133 actually decided:

~~~json
{
  "ProfileGuardrails": {
    "Collection": {
      "MaxRepositoryConcurrency": 5
    }
  }
}
~~~

`Microsoft.NET.Sdk.Web` automatically globs and copies `appsettings*.json` to
the build/publish output (verified directly: `dotnet build` produced
`src/GitHubProof.Web/bin/Debug/net10.0/appsettings.Production.json`), so this
file becomes a real artifact of any build of this project, not a test-only
fixture.

**What this session did NOT do, and why:** it did not set `ASPNETCORE_ENVIRONMENT`
on any real host, and it has no access to the Softbreak VPS process
environment. ASP.NET Core's own default (`ASPNETCORE_ENVIRONMENT` unset ⇒
`"Production"`) means this file becomes authoritative automatically once
deployed, unless the deployment process explicitly overrides the environment
name to something else — a fact about the real host this session cannot
verify from inside the repository. This is recorded as a known gap in
`## Known Risks`, not silently assumed.

U and the two GraphQL flags are **not** added to the new file: their existing
default (`GlobalConcurrency = 1`, `SameProfileConcurrency = 1`,
`LanguagesShadowEnabled = false`, `RepositoryEnvelopeEnabled = false`) already
**is** the production-candidate value, and the task's own instruction against
"gereksiz duplicate configuration" applies directly. Tests below prove this
omission does not leave them ambiguous.

No `ProfileCollectionOptions.MaxRepositoryConcurrency` C# default was changed;
it remains `1`. The production-candidate value lives in configuration only, as
`docs/architecture/public-profile-runtime-collection.md` requires.

---

## Changes

Created:

- `src/GitHubProof.Web/appsettings.Production.json`: the one-key production
  configuration selecting `MaxRepositoryConcurrency = 5`.
- `tests/GitHubProof.Web.Tests/ProfileProductionCandidateConfigurationTests.cs`:
  five deterministic tests proving, through the real file-loading
  configuration path (not an in-memory double):
  - a `Production`-environment load resolves repository concurrency to `5`;
  - the same load leaves `GlobalConcurrency`/`SameProfileConcurrency` at `1`;
  - the same load leaves both GraphQL feature flags `false`;
  - a `Development`-environment load (no `appsettings.Development.json`
    exists) is **not** masked by the production value and still resolves to
    the unchanged source default `1`;
  - the real `appsettings.Production.json`, fed through the actual
    `AddProfilePipeline` composition, reaches the constructed
    `ProfileRepositoryAnalysisOrchestrator` and bounds observed concurrency at
    exactly `5` for a 7-repository input, and all 7 repositories still
    produce a result — proving both the wiring and the no-cap invariant
    together.

No production/domain/application/infrastructure source file was modified.

---

## Decisions

- **Production configuration selection over source default mutation.**
  `appsettings.Production.json` is the smallest correct implementation of "R=5
  in the production-candidate configuration": it uses the exact mechanism the
  Session 131 options seam was already built for, requires no code change, and
  keeps the C# default (`1`) as the honest "nothing configured" behavior for
  every other environment (development, test, an unconfigured deployment).
- **No explicit U/GraphQL entries in the new file.** Their default already is
  the production-candidate value; adding them would be configuration that
  exists only to restate a default, which the task explicitly discourages.
- **No `ASPNETCORE_ENVIRONMENT` was set anywhere by this session**, and no
  attempt was made to reach the Softbreak VPS. Deploying this file to a real
  running production process is a separate, owner-controlled action (Milestone
  A's own gate: "production configuration development'tan ayrılmış" is a
  deploy-time/ops concern this repository session does not perform).
- **Provider floor, execution deadline, credential strategy, visitor
  rate-limit/trusted-proxy topology, R>5, U>1 and any GraphQL switch are left
  exactly as they were.** None of the open D-1/D-2/D-3/D-5 decisions in
  `worklog/CURRENT.md`'s Open Runtime Decisions table were touched or closed.
- **Live authenticated validation is BLOCKED, not skipped-and-ignored.**
  `GITHUB_TOKEN` presence was checked (never its value) in both the Bash and
  PowerShell shells available to this session; neither has it set. Per the
  task's own §12, configuration implementation and test validation were
  completed in full and the live portion is reported BLOCKED rather than
  attempted or assumed.
- **Launch-burst was not re-implemented.** The Session 133 admission tests
  (`ProfileLaunchBurstTests`) are untouched — this session's change never
  touches `ProfileAdmissionOptions` or the admission guard — and were rerun as
  part of the full regression suite below to confirm they still pass
  unmodified.

---

## Validation

~~~text
New configuration tests
PASS — 5/5 (ProfileProductionCandidateConfigurationTests)

Targeted Profile-related regression (all four test assemblies,
FullyQualifiedName~Profile)
PASS — 1343/1343
      (Domain 289, Infrastructure 409, Web 430, Application 215)
      includes ProfileLaunchBurstTests, ProfileExecutionAdmissionGuardTests,
      ProfileModelTests, ProfileRepositoryConcurrencyWiringTests,
      ProfileGraphQlHybridEnvelopeWiringTests,
      ProfileGraphQlLanguagesShadowWiringTests, unmodified and green

dotnet build GitHubProof.slnx -warnaserror
PASS — 0 warnings, 0 errors

dotnet test GitHubProof.slnx --no-build
PASS — Domain 842/842
       Application 276/276
       Infrastructure 636/636
       Web 573/573   (568 + this session's 5 new tests)
       TOTAL 2327/2327

git diff --check
PASS — clean (two new untracked files only, no tracked-file modification)

Snapshot taken BEFORE worklog files were written — intermediate, not final
state. It predates this session file, and the CURRENT.md/INDEX.md edits that
follow it. See `## Working Set / Final Git State` below for the authoritative
final working tree.

git status --short
?? artifacts/                                            (pre-existing, untouched)
?? src/GitHubProof.Web/appsettings.Production.json
?? tests/GitHubProof.Web.Tests/ProfileProductionCandidateConfigurationTests.cs

git diff --stat
(empty at this point in the session — every changed file was still new/
untracked; worklog/CURRENT.md and worklog/INDEX.md become tracked-modified
only once this session file itself is written, see below)
~~~

Local production-candidate smoke (deterministic, no network — the best
available without a token):

~~~text
Configuration source: real appsettings.Production.json, loaded through the
                       same ConfigurationBuilder JSON-file mechanism
                       WebApplication.CreateBuilder uses
DI composition:        the real AddProfilePipeline(configuration) graph
Snapshot source:       fake, deterministic (network-free), the same
                       gate/observer pattern Session 131 already established
Repositories:          7 admitted
Max observed in-flight: 5   (bounded exactly at R=5, never 7, never 1)
Results produced:       7   (no repository-count cap or sampling)
GlobalConcurrency:      1   (U untouched)
SameProfileConcurrency: 1   (untouched)
LanguagesShadowEnabled: false
RepositoryEnvelopeEnabled: false
~~~

Authenticated live validation: **BLOCKED**. `GITHUB_TOKEN` is not present in
this session's Bash or PowerShell environment (presence-only check, value
never read or printed). No live Profile request, dense-latency remeasurement
or authenticated burst run was attempted.

---

## Milestone B Exit Review

Cross-checked against `docs/roadmap/deployment-milestones.md`'s full
Milestone B production gate and exit condition list, and against
`worklog/CURRENT.md`'s Open Runtime Decisions table, re-verified against
current repository state rather than assumed from prior worklog text.

**ALREADY PROVEN (prior sessions, re-verified present in this tree):**

- Stage 7 / Profile P3-P9 COMPLETE; P10 Profile Web Flow COMPLETE.
- Unsupported profile claim = 0, broken evidence traceability = 0, prohibited
  developer inference = 0 (Session 130, PROHIBITED_INFERENCE_TEXT PASS across
  432 rendered surfaces).
- Deterministic replay PASS; varied profile validation PASS including
  sparse/fork-heavy/archive-heavy scenarios (Session 130, six real profiles).
- Designated dense-profile scenario measured at `<= 60 s` (Session 133, R5 =
  `55.557 s`, semantic parity PASS, request count unchanged, provider-safe).
- Launch-burst 10/50/100 simultaneous arrivals PASS with explicit
  capacity/retry outcomes, no crash, no indefinite wait (Session 133).
- GitHub rate-limit behaviour, partial API failure behaviour and
  processing/capacity/provider/terminal UX are all defined and covered by
  existing tests (§10-11 of the runtime contract; `ProfileModelTests`,
  provider budget protection tests).

**THIS SESSION (134) PROVEN:**

- The Session 133-accepted R5 candidate is no longer merely "accepted" — it is
  wired into a real, buildable, deployable production configuration file, with
  U and GraphQL left correctly untouched.
- That file resolves deterministically to R=5 through the actual ASP.NET Core
  configuration-loading mechanism (not an in-memory stand-in), and does not
  leak into a non-production environment.
- The configuration seam still reaches the real production
  `ProfileRepositoryAnalysisOrchestrator` composition end to end, bounding
  concurrency at exactly 5 with zero repository-count cap.
- Full regression (2327/2327, 0 warnings) confirms nothing about admission,
  burst handling, provider protection, GraphQL default-off state or Repository
  Proof was disturbed by adding the file.

**STILL OPEN (explicitly out of this session's locked scope; none decided or
closed here):**

- **Actual VPS deployment** of this configuration to the Softbreak host — this
  session has no VPS access and did not attempt one. "Production configuration
  development'tan ayrılmış" (Milestone A) governs how that deploy-time
  separation is achieved and is an ops action, not a repository change.
- **Whether `ASPNETCORE_ENVIRONMENT` on the real host will actually load this
  file.** ASP.NET Core defaults to `"Production"` when unset, which favors
  this file being picked up automatically, but this session cannot observe or
  set the real host's environment variable.
- **Authenticated live validation under this exact configuration** —
  `GITHUB_TOKEN` unavailable; BLOCKED, not proven.
- **Production telemetry/monitoring infrastructure.** The application-level
  §9 telemetry surface (`ProfileExecutionTelemetry`) exists and is already
  exercised by tests, but no health endpoint, log/metrics export or dashboard
  exists anywhere in this repository — "production telemetry profile flow'u
  izleyebiliyor" in the ops-monitoring sense is not built.
- **Remote production verification** and **"birden fazla gerçek GitHub
  kullanıcısı ile smoke PASS" specifically under R=5** — Session 130's
  multi-profile real smoke ran under R=1 (before Session 131 even introduced
  R); it has not been repeated under the accepted R5 candidate.
- **D-1 execution deadline, D-2 request/reserve protection, D-3 run-time
  provider-limit behaviour, D-5 credential strategy** — all still open exactly
  as `worklog/CURRENT.md` records them; R5 selects a concurrency candidate
  only and answers none of them.
- **GraphQL enablement** — still blocked by the Session 126 secondary-limit
  residual; untouched.
- **Visitor rate-limit numeric value and trusted-proxy topology (§10.5)** —
  still deployment-owner dependent; untouched.

Milestone B is **not** claimed complete. The "Exit Condition" bullets in
`docs/roadmap/deployment-milestones.md` (production accessibility, production
profile smoke set, evidence drill-down verified in production, production
telemetry verified, remote production verification) require real deployment
and cannot be satisfied from inside this repository session.

---

## Problems

None. No defect was found or fixed in existing code or tests this session.

---

## Not Done

- No production/domain/application/infrastructure source code was changed —
  only a new configuration file and a new test file.
- The provider floor, execution deadline, credential strategy, visitor
  rate-limit/trusted-proxy topology, R>5, U>1 and any GraphQL switch remain
  fully open and were not decided.
- No VPS deployment, no remote production verification, no production
  telemetry/monitoring infrastructure was built.
- No authenticated live run was performed (`GITHUB_TOKEN` unavailable) — the
  dense-latency and burst gates were **not** re-measured this session; Session
  133's `55.557 s` single-run measurement remains the latest authenticated
  evidence.
- No commit, push, merge or deploy was performed.

---

## Known Risks

- **Real-host environment-name gap.** This session cannot verify what
  `ASPNETCORE_ENVIRONMENT` the actual Softbreak VPS process will run with.
  `appsettings.Production.json` becomes authoritative under ASP.NET Core's own
  default behavior (unset ⇒ `"Production"`), but if the real deployment
  explicitly sets a different environment name, this file will not load and
  R will silently remain the source default `1`. This is a deploy-time fact
  outside repository scope, not a code defect — the next session touching
  deployment should confirm the real host's environment name explicitly.
- **R5's `55.557 s` measurement is still a single authenticated run** from
  Session 133, now one full day old and not re-verified against the exact file
  this session adds — because doing so would require the token this session
  does not have. It remains a measured candidate, not a guarantee.
- **Multi-user real smoke has not been repeated under R5.** Session 130's real
  multi-profile validation predates R entirely (it ran at the unconditional
  R=1 sequential path). A production-candidate-specific multi-profile smoke
  has not been performed.
- The Session 126 message-only secondary-limit residual remains unchanged;
  GraphQL stays out of scope for this and every session until it is resolved.

---

## Working Set / Final Git State

This is the final, authoritative working-tree description — it includes the
worklog edits this correction pass made, unlike the intermediate `git status`
snapshot captured earlier in `## Validation`, which predates this session file
and the `CURRENT.md`/`INDEX.md` edits. `git status --short` cannot show a
worklog file's own path as "modified" before that file has been saved once;
the lists below describe the real, current, on-disk state, independently
re-verified after every edit in this correction pass.

~~~text
Modified:
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked (this session):
- src/GitHubProof.Web/appsettings.Production.json
- tests/GitHubProof.Web.Tests/ProfileProductionCandidateConfigurationTests.cs
- worklog/sessions/2026-08-27-134-milestone-b-production-candidate-runtime.md

Pre-existing unrelated:
- artifacts/
~~~

No production/domain/application/infrastructure source file is modified —
`git diff --stat` against tracked files shows only `worklog/CURRENT.md` and
`worklog/INDEX.md`.

---

## Independent Review

An independent closure review was run over this session's diff, configuration
mechanism and exit-gate classification.

~~~text
REVIEW RESULT:
PASS WITH FINDINGS

CRITICAL FINDINGS:
NONE

Configuration:
PASS — Production resolves R=5, Development stays at source default R=1 (no
masking either direction), U stays 1, both GraphQL flags stay false, real
ASP.NET Core JSON-file-loading path used (not a test-specific fake), the real
production orchestrator composition bounds concurrency at exactly 5 with no
repository-count cap.

Publish artifact:
PASS — `appsettings.Production.json` independently verified present, with the
exact expected content, in a real `dotnet publish` output (not just
`dotnet build` output) to a temporary directory, which was then cleaned up.

Build:
PASS — 0 warnings, 0 errors (independently re-run)

Tests:
PASS — 2327/2327 (independently re-run; the 5 new tests individually
confirmed passing by name)

git diff --check:
PASS

Worklog:
NEEDS UPDATE at review time — findings corrected in this same-session
worklog-only pass:
  1. Invented Result status "PASS WITH OPEN GATES" (not one of the six
     protocol values) in CURRENT.md, INDEX.md and this file's own header —
     corrected to PARTIAL.
  2. Stale present-tense "R5 is proposed, not configured" in CURRENT.md,
     contradicted by this session's own change — corrected to describe R5 as
     configured (Session 134), not yet deployed or remote-verified, while
     preserving Session 133's own historical fact that it left configuration
     unchanged.
  3. Stale "Last remote-verified main: 37b8ec2 ... at Session 133 start" in
     CURRENT.md — corrected to `53991ac`, verified at Session 134 start.
  4. Overstated "Production values remain unselected" in CURRENT.md —
     narrowed to name R as selected (configured, not deployed) and list what
     genuinely remains open.
  5. The `## Validation` git-status snapshot was an unlabeled intermediate
     capture — now explicitly labeled as taken before worklog files were
     written, pointing to this section as authoritative.
  6. `## Working Set / Final Git State` normalized to the repository's
     Modified:/Untracked:/Pre-existing convention.

Protocol-correct Session result:
PARTIAL — the configuration implementation and its validation are complete
and technically accepted; the session is not PASS because authenticated live
validation, an explicit part of the Session 134 objective, could not run
(`GITHUB_TOKEN` unavailable). Not BLOCKED, because substantial, independently
verified progress was made outside that one dependency.

Session closure:
PASS

Milestone B:
NOT CLAIMED
~~~

---

## Next

One bounded authenticated live smoke run against this exact
production-candidate configuration:

~~~text
R = 5   (src/GitHubProof.Web/appsettings.Production.json)
U = 1
GraphQL = OFF
~~~

This is the one piece of the Session 134 objective a missing `GITHUB_TOKEN`
left undone, and it is deliberately **not** bundled with VPS deployment in the
same task: deployment, confirming the real host's `ASPNETCORE_ENVIRONMENT` and
building the minimal production telemetry/health surface the Milestone A/B
gates require are a separate, later, owner-controlled session that only makes
sense once the authenticated smoke evidence exists. Only after both of those
does remote production verification and a multi-user smoke specifically under
R5 become meaningful.

---

## Git

~~~text
Suggested commit:
feat(profile): select R5 as the production-candidate repository concurrency

Commit:
PENDING — user-owned

Push:
PENDING — user-owned
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı veya Result doğru final state'e getirildi
[x] Targeted validation çalıştı
[x] Broader validation çalıştı (full solution suite)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Next Immediate Action güncellendi
[x] Kullanıcıya kısa final rapor verildi
~~~
