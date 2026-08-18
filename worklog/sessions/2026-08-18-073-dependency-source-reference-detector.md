# Session 073 — Dependency Source Reference Detector Implementation

Date: 2026-08-18
Result: PASS — COMMITTED, PUSHED, INTEGRATED INTO MAIN, REMOTELY VERIFIED at
72932bb5de4ec37850850ebaea2c2e75f61e406a
Scope: domain implementation + tests + bounded canonical contract correction
Model Route: Claude Opus / high
Model Gate: APPROVED

## Goal

Implement the locked
`docs/architecture/repository-dependency-source-reference-detection.md`
as a deterministic Domain policy plus Domain models and tests, producing
`DependencySourceReferenceEvidence` from
`RepositorySnapshot.DependencyDeclarations` + `RepositorySnapshot.SourceFiles`.

No `ProducedWorkEvidence`, no `RepositoryProblemAreaEvidence`, no ProblemArea, no
profile pattern, no P7, no scoring, no report, no UI.

## Starting State

Branch:
session/073-dependency-source-reference-detector

Session Base:
275b6c5590f4e490dea552173af0f373295d14f2

Verified at session start: branch `main`, local HEAD == `origin/main` ==
`275b6c5`, working tree clean, `worklog/CURRENT.md` Active Session = NONE.
No unrelated user changes.

## Step 0 — Bounded Contract Alignment

### The contradiction — VERIFIED against code, not assumed

Contract version 1 §10 required the detector to identify Go `// indirect`
requirements as a structurally unreferenceable class, and the Session 072
correction pass added the phrase "a Go `// indirect` requirement by its recorded
indirect marker".

There is no recorded indirect marker. Evidence:

- `src/GitHubProof.Domain/Analysis/Models/RepositorySnapshotData.cs:232` —
  `RepositoryDependencyDeclaration` carries exactly `Ecosystem`, `ManifestPath`,
  `PackageId`, `DeclaredVersion`. No scope, no marker, no flag.
- `docs/architecture/repository-dependency-evidence.md` §11.1 — "Dependency
  scope is not recorded, so runtime, development, build and test declarations
  are indistinguishable."
- `src/GitHubProof.Infrastructure/GitHub/GitHubRepositoryDependencyManifestParser.cs`
  — `ParseGoModule` applies `StripComment(rawLine)` before parsing each line,
  and `StripComment` truncates the line at the first `//`. The `// indirect`
  suffix is therefore removed before a declaration is ever constructed.

The same §11.1 records that indirect requirements *are* recorded as
declarations. What is lost is only the marker that would identify them.

### Second defect found while verifying

The class was also mis-classified. An indirect Go requirement is an ordinary
module path and source can legitimately import it, so it was never
"structurally unreferenceable" in the sense §10 uses for Maven aggregators or
Composer platform requirements.

### Correction applied

`docs/architecture/repository-dependency-source-reference-detection.md`:

- Go `// indirect` removed from the §10 class list and from the sentence
  enumerating identifiable classes;
- new §10.1 records why, and locks the resulting behavior: an indirect Go
  requirement is treated as an ordinary Go declaration; a positively matching
  qualifying import produces the ordinary §3 syntactic observation and nothing
  more; a non-match carries no meaning under §11 and §12; the detector never
  infers direct versus indirect intent;
- an Amendments block was added to the document header.

No heuristic was invented and `RepositoryDependencyDeclaration` was not
expanded.

### Contract version decision

The document stays **Contract Version 1**.

§17 defines a major `SourceReferenceRuleVersion` bump as a change that can
alter, remove or re-attribute a **previously produced observation**. No detector
and no numeric `SourceReferenceRuleVersion` has ever existed, so no observation
has ever been produced under the removed sentence and none can be altered by
removing it. The first runtime `SourceReferenceRuleVersion` is therefore
established after this correction rather than against the stale text.

### Not reopened

Supported V1 ecosystems, the Java/Php/Ruby exclusions, the identity rules,
source collection, `ProducedWorkEvidence` and ProblemArea semantics were all
left untouched.

R1-R16 are untouched with one narrow exception: the Go component of Session
072's R10 is superseded. R10 recorded Go `// indirect` among the confirmed
structurally unreferenceable classes and cited the 283-of-349 measurement. That
measurement read raw `go.mod` text, which the production declaration surface
cannot do, so the Go part of R10 does not survive. Every other class R10
confirmed remains valid. The Session 072 file is a closed record and is not
rewritten; this contract and this note are the authority.

## Step 0 Review

Independent narrow review: **PASS WITH FINDINGS**, no BLOCKER and no HIGH, so
implementation may start.

Verified by the reviewer against source, not against this worklog: the
declaration model's four fields; `repository-dependency-evidence.md` §11.1; and
that `ParseGoModule` strips the comment before the declaration is built. The
reviewer additionally found a second marker-loss point, `AddGoRequirement`,
which reads only the first two whitespace segments of a requirement line.

The single LOW finding — that "R1-R16 generally untouched" overstated the case
— is corrected in the paragraph above.

## Implementation

All production code is Domain. No Infrastructure detection logic was added, and
no Application, Web, report, UI or scoring file was touched.

New Domain types:

    Enums/SourceReferenceForm                the eight qualifying forms of §5
    ValueObjects/SourceReferenceRuleVersion  the detection version owner, §17
    Models/DependencySourceReferenceLocation path + form + 1-based line, §14
    Models/DependencySourceReferenceEvidence one dependency observation, §3
    Models/RepositorySourceReferenceEvidence the derived result, §11 and §12
    SourceArtefactEcosystem                  the §4.1 extension map and §4 surface
    SourceLexicalScanner                     §6 exclusion and §6.1 desynchronization
    SourceImportReader                       §5 and §7 form reading
    SourceReferenceIdentityRules             §8 identity and tie-breakers
    RepositorySourceReferenceEvidencePolicy  composition, §9, §10, §13

`SourceReferenceRuleVersion` first runtime value: **1.0.0**.

Key implementation decisions, each traceable to a contract clause:

- The lexical scanner is hand-written per family and classifies every offset as
  code, comment or string. The import reader accepts a form only when its keyword
  token is code, and a Go import-block entry only when it begins at a code token
  or at the opening delimiter of a real string literal, so a line-level pattern
  can never see inside a comment or a string.
  Modeled forms: Go line and block comments, quoted strings and backtick raw
  strings; JavaScript line and block comments, quoted strings, template literals
  whose interpolation holes are scanned as code, and regular-expression literals;
  Rust line and doc comments, nested block comments, quoted strings and
  `r#"..."#` raw strings with any hash count, plus lifetime-versus-char-literal
  lookahead; Python line comments, quoted strings, triple-quoted strings and raw
  prefixes; DotNet line and doc comments, block comments, quoted strings,
  `@"..."` verbatim strings and `"""` raw strings.
- §6.1 is implemented as a whole-artefact discard: when the scan reaches end of
  file inside an unterminated modeled string or block comment, every observation
  from that artefact is dropped and `HasUnreliableSourceArtefact` becomes true.
  A single-line quoted literal that is never closed on its line terminates at the
  newline and is deliberately not treated as desynchronization, because it cannot
  make the scanner run away.
- §7 is implemented without regular expressions: the argument must be exactly one
  string literal followed by the closing parenthesis. A concatenation, a template
  literal, a variable or a second argument yields nothing.
- The reader is hand-written rather than regular-expression driven throughout,
  because repository source is untrusted input and the contract forbids clock
  dependence, so a regular-expression timeout was not an available mitigation.
- §9 ambiguity is resolved by taking the maximum §8 tie-break score and returning
  nothing when two declarations tie. The only tie-breaker is Go's longest module
  path. Declaration order, manifest path, version and every other heuristic are
  unused, so the same package declared in two manifests of a monorepo yields no
  observation.
- §10 excludes only classes identifiable from the declaration surface: npm
  `@types/*` by name and MSBuild `$(...)` placeholders by form. DotNet
  analyzer-only packages are not excluded, and per §10.1 a Go indirect
  requirement is treated as an ordinary declaration.
- The result is positive-only. There is no unreferenced collection, no count of
  declarations without references, no ratio and no coverage value.

## Validation

Build: 0 warning / 0 error.

Tests:

    Domain          622 PASS   (79 new)
    Infrastructure  255 PASS
    Application     101 PASS
    Web              68 PASS
    total          1046 PASS   (baseline 967)

The 79 new tests cover: one positive case per supported ecosystem; every
materially distinct qualifying form; comments, docstrings, template literals,
Go raw strings, Rust raw strings with nested block comments, Python triple
strings and DotNet verbatim and raw strings producing nothing; a JavaScript
regular-expression literal not desynchronizing a later `require`; end-of-file
desynchronization discarding a whole artefact while leaving other artefacts
intact; the literal-only `require`/`import` rule; relative-specifier exclusion;
scoped and subpath identity; the Go longest-module-path winner; Rust
hyphen-to-underscore; Python PEP 503 normalization and the deliberate
`scikit-learn` false negative; DotNet own-subnamespace acceptance and prohibited
reverse-prefix matching; ambiguity yielding nothing; Java, Php and Ruby yielding
nothing; the `@types/*` and MSBuild-placeholder exclusions; duplicate-location
collapse; deterministic ordering and input-order independence; unavailable and
incomplete semantics; a payload carrying no snippet; the absence of any
unreferenced, count, ratio or coverage member; and `SourceReferenceRuleVersion`
invariants.

Two regression tests guard the Step 0 correction: a Go declaration without an
indirect marker is treated as ordinary and produces the normal observation, and
`RepositoryDependencyDeclaration` is asserted to expose exactly its four
properties, so no future code can assume an indirect or scope marker exists.

Git validation: `git diff --check` clean; changes confined to
`src/GitHubProof.Domain`, `tests/GitHubProof.Domain.Tests`, the detection
contract and worklog. `AnalyzerVersion` remains 1.1.0; `RuleSetVersion`,
`ReportSchemaVersion`, `DependencyCategoryMappingVersion`, `PatternRuleVersion`
and `ObservationRuleVersion` are untouched; no scoring signal, category result,
report element or UI changed.

## Independent Review

First implementation review: **FAILED**, two HIGH findings, no BLOCKER. Both were
real, both were reproduced by the reviewer with a probe harness, and both were
corrected.

**HIGH 1 - a commented-out entry inside a Go `import` block produced an
observation.** The block reader exempted a `"` from the code check so that the
real import literal, which is correctly classified as not-code, could be read.
The exemption also let a quote sitting inside a `/* */` comment through.

Correction: the lexical scan now records three zones rather than two - code,
comment and string - and exposes `IsStringStartAt`, which is true only at the
opening delimiter of a real string literal. A quote inside a comment is comment,
never a string start, so a commented entry is skipped. The same guard was added
to the single-line Go import path.

**HIGH 2 - an unterminated single-quoted Python literal silently violated
§6.1.** The Python scanner returned failure at the newline, which masked the
rest of the artefact as string without setting desynchronization. Observations
already read stayed, later lines were invisible, and
`IsDetectionSurfaceComplete` still reported true.

Correction: a single-quoted Python literal now ends at the newline exactly as in
the C-like scanner, so it can never swallow the artefact and is not a
desynchronization. Only an unterminated triple-quoted string desynchronizes.

**LOW - dead state.** The unused `templateBraceDepths` list was removed.

Four regression tests were added: a Go import block with a multi-line `/* */`
comment and one with a `//` comment both yield only the real entry; an
unterminated single-quoted Python literal does not hide later lines and does not
mark the artefact unreliable; an unterminated triple-quoted literal does.

The two worklog claims the reviewer flagged as unsupported are now true of the
corrected code and are stated accordingly above.

Narrow re-review after the corrections: **PASS WITH FINDINGS**, no BLOCKER and no
HIGH. The reviewer rebuilt an independent probe harness rather than trusting the
new tests, exercised cases the author had not written - a commented alias entry,
a comment between two real entries, a trailing comment on a real entry, EOF
without a newline, backslash line continuation - and confirmed both HIGH findings
closed with no regression in the other four ecosystems. The single LOW finding,
a stale `worklog/CURRENT.md` status line, is corrected.

Two deliberate behaviours the reviewer recorded as safe rather than as findings:
an unterminated single-line literal at end of file is not a desynchronization,
because there is no later position left to misclassify and this matches the
C-like path; and a Go entry written as `/*c*/"os"` on one line is skipped, which
is a false negative in a form gofmt does not produce.

## Remaining Bounded Risk

- No orchestrator, report or UI consumes `RepositorySourceReferenceEvidence`. It
  is produced on demand by the Domain policy and nothing wires it yet.
- The residual false-positive paths the contract enumerates in §20 are unchanged
  and unmitigated by implementation: a module-resolution alias configured outside
  the manifest, a Go `replace` directive, and an identity collision where the
  true supplier is undeclared.
- `.fs`, `.vb`, `.razor` and `.cshtml` are mapped to DotNet by §4.1 and are
  scanned with the C# lexical rules. None of them expresses the §5 `using Ns;`
  directive natively, so in practice they yield no observation and any lexical
  mismatch can only suppress a match, never manufacture one.
- The same package declared in two manifests of one repository is ambiguous under
  §9 and yields no observation. That is contract-mandated, and it costs coverage
  in monorepos.

## Handoff

Changed scope: 10 new files in `src/GitHubProof.Domain/Analysis`, 2 new test
files in `tests/GitHubProof.Domain.Tests/Analysis`, the §10 and §10.1 amendment
in `docs/architecture/repository-dependency-source-reference-detection.md`, and
worklog. No existing production file was modified.

`SourceReferenceRuleVersion`: 1.0.0.

Build 0 warning / 0 error; 1046/1046 tests pass, 79 of them new.

Review: Step 0 PASS WITH FINDINGS, implementation FAILED with two HIGH, both
corrected, re-review PASS WITH FINDINGS with no BLOCKER or HIGH.

`ProducedWorkEvidence` is not resolved, `RepositoryProblemAreaEvidence` is not
unblocked, no ProblemArea exists and P7 is not advanced.

Exact next action at the time this session was implemented was to decide the
Session 073 Git transition. That transition is now complete: committed, pushed
and integrated into main, remotely verified at
72932bb5de4ec37850850ebaea2c2e75f61e406a. This correction, made in Session 074,
records only that already-verified fact; it does not revisit the
implementation or review history above.

## Git

Committed, pushed and integrated into main. Remotely verified at
72932bb5de4ec37850850ebaea2c2e75f61e406a.
