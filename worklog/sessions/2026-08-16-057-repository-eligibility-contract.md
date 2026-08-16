# Session 057 — Repository Eligibility Contract

**Date:** 2026-08-16
**Result:** IN_PROGRESS
**Profile Phase:** P2 — Repository Eligibility Contract
**Roadmap Stage:** Stage 6 — Profile P2
**Scope:** domain/product contract / eligibility semantics / docs
**Base HEAD:** 4c93400
**Execution:** ChatGPT-guided / macOS Terminal / manual
**Implementation Agent:** NONE

## Goal

Contract-lock the Public Profile Evidence V1 repository eligibility semantics
before profile-level production implementation begins.

P2 must define which public repositories may contribute to which
profile-level evidence operations and why.

Production code is not in scope.

## Canonical Parent

docs/architecture/public-profile-evidence-domain-model.md

Status:
CONTRACT LOCKED

Profile Phase:
P1 — COMPLETE

## Core Question

Repository eligibility answers:

> Whether and how this repository may contribute to profile-level evidence.

It does not answer:

> Is this a good repository?

or:

> Is this a good developer?

## Required Repository Conditions

P2 must explicitly contract at least:

- original repository,
- fork,
- template,
- archived repository,
- empty repository,
- insufficient-evidence repository,
- active repository,
- historical repository,
- independent repository semantics.

## Non-Negotiable Boundaries

RepositoryEligibilityResult != repository quality

RepositoryEligibilityResult != developer quality

Eligibility must not become:

- developer score
- capability score
- seniority
- employability
- hire/no-hire
- candidate ranking

Forks/templates must not inflate repeated evidence.

Historical/archived repositories may remain evidence without silently becoming
current-practice claims.

## Next

Step 6.1 — extract and classify exact eligibility dimensions, statuses,
reasons and contribution semantics before drafting the canonical P2 contract.

## Pre-P2 Bounded P1 Clarification

Before P2 semantic work begins, the locked P1 contract received one bounded
clarification.

Canonical document:

docs/architecture/public-profile-evidence-domain-model.md

### SnapshotReference

Clarified semantic ownership:

- SnapshotReference provides stable/deterministic traceability to the exact
  normalized GitHubProfileSnapshot consumed by profile analysis.
- profile result cannot silently rebind to another snapshot.
- semantic identity/equality is required.
- exact implementation/storage shape remains unlocked.

### AnalyzerVersion

Clarified semantic ownership:

- AnalyzerVersion versions profile-analysis orchestration / aggregate semantic
  contract.
- it does not replace individual eligibility, observation, pattern,
  representative-repository or role rule versions.
- one DeveloperPublicEvidenceProfile must be verifiably produced from one
  coherent and compatible rule-version set.
- exact implementation/serialization/storage shape remains unlocked.

### Preserved P1 Decisions

UNCHANGED:

- bounded inference remains DEFERRED,
- DeveloperPublicEvidenceProfile remains job-agnostic,
- RoleEvidenceAlignment != JobEvidenceAlignment,
- no developer/candidate score,
- no seniority,
- no employability,
- no hire/no-hire,
- no candidate ranking.

### Validation

Result:

PASS

Production code/tests:

UNCHANGED

P1 status:

CONTRACT LOCKED

P2 semantic work:

NOT STARTED YET
