# Public Profile Evidence V1 — Repeated Pattern Contract

**Status:** CONTRACT LOCKED
**Version:** 1
**Profile Phase:** P6 — Technical, Practice and Problem Patterns
**Roadmap Stage:** Stage 7 — Public Profile Evidence Foundation
**Locked:** 2026-08-17 / Session 065

## 1. Purpose

This contract defines deterministically:

> When repeated public repository evidence may be promoted into one canonical
> profile-level pattern, and what that pattern may mean.

It resolves the taxonomy, threshold and promotion decisions that
`docs/product/public-profile-evidence-product-contract.md` §27 deliberately
left open, and it implements the pattern layer contracted by
`docs/architecture/public-profile-evidence-domain-model.md` §10.

It does not alter P1 result ownership, P2 eligibility semantics or P5
observation semantics.

## 2. Canonical Boundary

RepeatedProfilePattern
!=
developer capability

RepeatedProfilePattern
!=
mastery

RepeatedProfilePattern
!=
seniority

RepeatedProfilePattern
!=
professional-experience duration

RepeatedProfilePattern
!=
employability

RepeatedProfilePattern
!=
hire/no-hire

RepeatedProfilePattern
!=
candidate ranking

RepeatedProfilePattern
!=
repository quality

A pattern describes what is repeatedly visible in a public evidence surface.

Its epistemic strength may never exceed the observations that support it.

## 3. Promotion Boundary

ObservedProfileEvidence
!=
RepeatedProfilePattern

A quantified observation does not promote itself.

Promotion requires this versioned rule and produces a result that identifies the
`PatternRuleVersion` that produced it.

## 4. Canonical Pattern Families

The canonical V1 pattern families are exactly:

- TechnicalFocus
- EngineeringPractice
- ProblemArea

`RepeatedProfilePattern` owns canonical pattern truth.

Family-typed views are projections over that canonical truth. They must not
become parallel stores containing conflicting pattern truth.

## 5. Repetition Threshold

A pattern is promoted only when:

IndependentSupportingRepositoryCount
>=
2

Only repositories whose P2/P5 `IndependentRepetition` contribution decision is
`Allowed` are counted.

Therefore:

- one repository can never establish a repeated pattern,
- fork lineage cannot inflate repetition,
- template-derived lineage cannot inflate repetition.

### 5.1 Undetermined Contribution

`Undetermined` independent repetition:

- is not support,
- is not negative evidence,
- does not block a pattern that already has two or more certain independent
  supports.

### 5.2 Supporting versus Independent

Supporting repositories remain visible as context.

Only independent supporting repositories participate in the threshold.

No other condition, weight, percentage, ranking or score participates in
promotion.

## 6. Absence Boundary

The absence of a pattern is not a claim.

An empty pattern collection means only that the contracted repetition condition
was not met on the observed public evidence surface.

It is not:

- absence of the technology,
- absence of the practice,
- negative developer evidence.

## 7. EngineeringPractice V1

### 7.1 Canonical Keys

The canonical V1 EngineeringPractice pattern keys are exactly:

- automated-testing
- continuous-integration
- documentation
- release-usage
- issue-usage
- pull-request-usage

No key may be added without a new `PatternRuleVersion`.

### 7.2 Evidence Mapping

Each key is supported only by the canonical repository signal observations
listed here:

| Pattern key | Supporting signal keys |
|---|---|
| automated-testing | `validation.test-structure` |
| continuous-integration | `validation.continuous-integration` |
| documentation | `documentation.installation-guide`, `documentation.usage-or-demo` |
| release-usage | `process.release-usage` |
| issue-usage | `process.issue-usage` |
| pull-request-usage | `process.pull-request-usage` |

A repository supports a multi-signal key when at least one mapped observation
lists that repository. The union is counted once per repository.

### 7.3 Excluded Signals

`process.commit-history` and `process.commit-distribution` are excluded from V1.

`repository.readme.exists`, `repository.description.present`,
`repository.license.present`, `portfolio.topics`,
`portfolio.homepage-or-live-demo` and
`portfolio.presentation-completeness` are excluded from V1 because they are
presentation or metadata evidence, not engineering practice.

### 7.4 Semantic Strength

A practice pattern means only:

> This practice evidence is repeatedly visible across independent public
> repositories.

It does not mean the developer is good at the practice.

## 8. TechnicalFocus V1

### 8.1 Canonical Evidence Source

The only admissible V1 source is:

`RepositoryAnalysis.Snapshot.Languages`

Prohibited as technical-focus evidence:

- README prose,
- documentation prose,
- repository description,
- topics,
- keyword matching of any kind,
- `RepositoryIdentity.PrimaryLanguage` as an untraceable shortcut.

### 8.2 Dominant Language Rule

For one repository:

- a language qualifies only with a positive byte count,
- the qualifying language is the repository maximum byte count,
- on an exact maximum tie, every tied language is dominant.

No percentage threshold participates.

### 8.3 Language Availability

When the `languages` data availability of a repository analysis is not
`Available`, that repository's language evidence is `Undetermined` for every
language observation.

It is never absence.

When language data is available and empty, the repository determinately
provides no language evidence. It is neither support nor undetermined.

### 8.4 No Semantic Grouping

The language literal reported by GitHub is preserved as canonical evidence.

V1 performs no grouping, aliasing, framework inference, role inference or
problem-area inference.

Therefore:

C#
!=
.NET

JavaScript
!=
Frontend

Python
!=
AI/ML

A technical-focus pattern key is the preserved language literal.

### 8.5 Semantic Strength

A technical-focus pattern means only:

> This language is the dominant language of two or more independent public
> repositories.

It does not mean proficiency, mastery or professional use.

## 9. ProblemArea V1

ProblemArea remains a canonical pattern family.

P6 V1 defines:

- no ProblemArea taxonomy,
- no ProblemArea promotion rule,
- no production code path that can emit a ProblemArea pattern.

### 9.1 Deferral Reason

The current normalized structured evidence surface cannot produce reliable and
ecosystem-neutral problem-area truth.

Determining facts:

- Dependency manifest content is collected only for `*.csproj`, `pom.xml`,
  `build.gradle`, `build.gradle.kts`, `pytest.ini`, `package.json`,
  `Directory.Packages.props`, `jest.config.*` and `vitest.config.*`, so
  dependency evidence exists only for .NET, Java and JavaScript repositories.
- Interpreting file paths or manifest contents into semantic facts is
  repository-level signal responsibility. Repeating that interpretation inside
  profile aggregation would duplicate repository-level evaluation.
- Declared dependencies and scaffolded files prove declaration, not production,
  and the current evidence surface cannot separate the two.

### 9.2 Explicit Non-Substitutes

Containerization evidence is not moved into ProblemArea, and it is not added to
the EngineeringPractice taxonomy, in this contract version.

### 9.3 Absence Meaning

An empty ProblemArea collection is a contract boundary.

It is not negative evidence and not developer absence.

### 9.4 Future Conditions

ProblemArea may be contracted later only when all of the following exist:

1. a repository-level signal that emits framework/dependency category evidence
   with structured evidence keys,
2. a manifest collection surface covering the major ecosystems,
3. a versioned dependency-to-area mapping owned by a canonical contract,
4. a structured boundary separating declared dependencies from produced work.

## 10. Pattern Traceability

Every promoted pattern must support this traversal:

RepeatedProfilePattern
↓
ObservedProfileEvidence[]
↓
ProfileEvidenceReference[]
↓
RepositoryAnalysisReference[]
↓
repository-level evidence

A pattern must not reference an observation outside the evidence surface it was
promoted from, and must not silently point to a different repository analysis.

Every independent supporting repository must also be a supporting repository of
the pattern.

## 11. Version Ownership

`PatternRuleVersion` identifies the taxonomy plus promotion rules defined here.

A pattern result must record the consumed `ObservationRuleVersion` and
`EligibilityPolicyVersion` and must reject inputs produced under mixed
versions, exactly as `CrossRepositoryEvidence` does.

A new `PatternRuleVersion` is required to change:

- any pattern family,
- any pattern key,
- any evidence mapping,
- the repetition threshold,
- the dominant language rule,
- the ProblemArea deferral.

## 12. Determinism, Ordering and Locale

For the same:

- `CrossRepositoryEvidence`,
- `ObservationRuleVersion`,
- `EligibilityPolicyVersion`,
- `PatternRuleVersion`,

the promoted pattern set must be semantically identical.

Canonical ordering:

1. pattern family,
2. pattern key, ordinal,
3. supporting observations by observation key, ordinal,
4. repositories by `ProfileRepositoryReference` ordering.

Input enumeration order and execution order must not change the result.

No system clock participates.

Locale may change presentation only. It must not change a pattern key, family,
count, threshold outcome or rule version.

## 13. LLM Boundary

No LLM participates in pattern promotion.

An LLM may narrate an existing canonical pattern result. It may not create,
upgrade, reinterpret or override one.

## 14. Prohibited Semantics

Pattern production must never emit:

- developer capability score,
- candidate score,
- seniority,
- employability,
- hire/no-hire,
- candidate ranking,
- years-of-professional-experience inference,
- authorship or originality inference,
- pattern strength or confidence values,
- hidden weighting of any kind.

Pattern strength and confidence calculation remain unlocked and are not
introduced by V1.

## 15. P2 Dependency

P6 requires no additional P2 runtime.

TechnicalFocus
!=
P2 CurrentFocus

No lifecycle classification, activity classification or `CurrentActivityWindow`
is introduced by this contract.
