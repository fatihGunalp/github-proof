# GitHub Proof --- P10 Presentation & Language Contract

**Status:** Draft / Product Direction\
**Milestone:** B --- Profile P10 --- Web Profile Flow --- Presentation Language Contract

------------------------------------------------------------------------

# 1. Purpose

This document defines the user-facing presentation direction of GitHub
Proof.

The purpose is to establish:

-   product narrative,
-   user language,
-   presentation principles,
-   semantic boundaries,
-   copy guidelines.

This document does not modify:

-   domain behavior,
-   signal definitions,
-   scoring rules,
-   eligibility rules,
-   analysis contracts.

It defines how existing product capabilities are communicated to users.

------------------------------------------------------------------------

# 2. Core Product Message

GitHub Proof's primary user-facing message:

> GitHub'ınızda ne olduğunu değil, ne anlattığını görün.

The product is not positioned as a metric dashboard or evaluation
system.

The purpose is to help users understand the meaning behind observable
GitHub activity.

------------------------------------------------------------------------

# 3. User Problem

GitHub profiles contain many observable elements:

-   repositories,
-   commits,
-   issues,
-   pull requests,
-   workflows,
-   documentation,
-   technical artifacts.

However, raw visibility does not automatically create understanding.

Users need help answering:

-   Where should I start?
-   What does this profile communicate?
-   Which visible patterns are meaningful?
-   What evidence supports this understanding?

------------------------------------------------------------------------

# 4. Product Position

GitHub Proof does not:

-   score developers,
-   rank developers,
-   determine seniority,
-   decide hiring suitability,
-   detect AI authorship,
-   make quality judgments.

GitHub Proof does:

-   organize observable GitHub traces,
-   reveal recurring production patterns,
-   provide a meaningful starting point,
-   connect interpretation back to evidence.

------------------------------------------------------------------------

# 5. User Transformation

## Before

My GitHub contains many things.

I do not know what matters.

------------------------------------------------------------------------

## After

I understand what my GitHub profile communicates.

I know where to start.

I can inspect the evidence behind the view.

------------------------------------------------------------------------

# 6. Experience Principle

The user experience follows this order:

    Meaning
    ↓
    Direction
    ↓
    Starting Point
    ↓
    Evidence

The user first understands the overall picture.

Detailed evidence is available progressively.

------------------------------------------------------------------------

# 7. Core Concepts

## Trace

A trace is an observable indication left in a public GitHub profile.

Examples:

-   repository structure,
-   commit history,
-   documentation,
-   tests,
-   workflows.

A trace is not a judgment.

------------------------------------------------------------------------

## Visible Direction

A visible direction is a recurring technical production tendency
observed from multiple traces.

**Source (Milestone B / Profile V1):** Visible Direction is derived from
`TechnicalFocusAreas` and `EngineeringPracticePatterns` --- Supported,
active pattern concepts that already produce output in this product
version. It is not derived from `RoleEvidenceAlignment`. RoleEvidenceAlignment
is a separate, Deferred capability for Milestone B / Profile V1
(`docs/product/public-profile-evidence-product-contract.md` §14): it
produces no active user-facing output today, and this document does not
present it as one. See the User Language Dictionary note below.

The system must not state:

> This person is a backend developer.

The system may state:

> This profile shows recurring traces related to backend systems.

------------------------------------------------------------------------

## Starting Point

A starting point is the recommended place to begin understanding a
profile.

It is not:

-   the best project,
-   the highest quality project,
-   the most successful project.

It is:

> A useful entry point for understanding visible production traces.

------------------------------------------------------------------------

## Evidence

Evidence is the observable source supporting a presented view.

The experience should allow users to move from:

    Meaning
    ↓
    Evidence

when they want deeper inspection.

------------------------------------------------------------------------

# 8. User Language Dictionary

  Internal Concept                                      User Language
  -----------------------------------------------------  ---------------------------------------------
  Evidence                                                Görünen iz / Kanıt
  Signal                                                  İz
  Pattern                                                 Tekrar eden iz
  Representative Repository                               Başlangıç noktası
  TechnicalFocusAreas / EngineeringPracticePatterns       Görünen teknik yön
  Role Evidence Alignment (Deferred --- not active in V1)  No active V1 user language; future direction only
  Profile Report                                          Profil görünümü

**Terminology note (vocabulary simplification only):** This dictionary is a
presentation-layer simplification. `Evidence`, `Signal` and `Pattern` remain
distinct canonical concepts at the domain level
(`Evidence != Signal != Pattern`); unifying them under related user-facing
words (the "iz" root) is a presentation choice and does not merge or replace
their underlying domain semantics, which are owned by
`docs/architecture/public-profile-evidence-domain-model.md` and
`docs/architecture/public-profile-evidence-patterns.md`.

`Role Evidence Alignment` is listed here for completeness of the internal
concept inventory, not because it has an active V1 user language today: it
is Deferred for Milestone B / Profile V1
(`docs/product/public-profile-evidence-product-contract.md` §14) and
produces no user-facing output in this version. "Görünen teknik yön" is
produced by `TechnicalFocusAreas` / `EngineeringPracticePatterns` instead
(see "Visible Direction" above).

------------------------------------------------------------------------

# 9. Presentation Rules

## Meaning Before Mechanics

Do not expose internal processing concepts first.

Avoid:

-   signal counts,
-   internal policies,
-   scoring details.

Prefer:

-   visible direction,
-   recurring traces,
-   supporting evidence.

------------------------------------------------------------------------

## Observation Before Judgment

The product describes observable patterns.

It does not make personal conclusions.

------------------------------------------------------------------------

## Evidence Is Available, Not Forced

The first experience should be simple.

The user should see:

> Bu GitHub ne anlatıyor?

before:

> Nasıl belirlendi?

------------------------------------------------------------------------

# 10. Landing Copy Principles

## Hero

> GitHub'ınızda ne olduğunu değil, ne anlattığını görün.

## Supporting Message

> GitHub profilinizde bıraktığınız izlerden oluşan büyük resmi keşfedin.

Avoid leading with:

-   analysis,
-   score,
-   ranking,
-   evaluation,
-   developer level.

------------------------------------------------------------------------

# 11. Result Presentation Principles

The result experience should answer:

First:

> Bu GitHub ne anlatıyor?

Then:

> Görünen üretim yönü

Then:

> Tekrar eden izler

Finally:

> Bunu neden görüyoruz?

------------------------------------------------------------------------

# 12. Evidence Presentation Model

Evidence presentation follows:

    Source
    ↓
    Observation
    ↓
    Meaning

The user should be able to move from interpretation to observable
source.

------------------------------------------------------------------------

# 13. Agent / Development Guidance

When creating user-facing experiences:

-   preserve evidence boundaries,
-   avoid judgmental language,
-   translate internal concepts into user meaning,
-   show meaning before technical detail.

Never create UI language implying:

-   developer ranking,
-   skill level,
-   hiring decisions,
-   AI authorship conclusions.

------------------------------------------------------------------------

# 14. Validation Questions

Before accepting a presentation change:

1.  Does the user understand what the GitHub profile communicates?
2.  Can the user reach supporting evidence?
3.  Does the language describe observation instead of judgment?
4.  Are internal concepts translated into user language?
5.  Is complexity hidden behind a simple experience?

------------------------------------------------------------------------

# 15. Core Experience Statement

GitHub Proof:

    Shows traces.
    Reveals direction.
    Opens evidence.

The product does not tell users who someone is.

It helps them understand what visible GitHub traces communicate.
