# Manual Review — spring-guides/gs-spring-boot

```text
Validation ID: validation-009
Frozen commit: d29be6bd11373e40b8e00885ec3106c74b67fd25
Repository profile: education-example
ManualReviewStatus: Completed
Human review completed: YES — user provided all 15 comparison outcomes.
```

This file is a human-review template. Automated Codex validation is not recorded as human review.

## Repository-level questions

- [ ] Repository purpose is understandable from visible evidence.
- [ ] Installation and usage evidence is interpreted in context.
- [ ] Test and CI evidence avoids filename-only false positives.
- [ ] Archive, small size, education, and single-commit context is not treated as a quality judgment.
- [ ] Missing evidence is distinguished from unavailable data.
- [ ] No developer seniority, copying, authorship, or AI-use inference is present.

## Signal comparison

| Signal | Automated status | Score | Human result | Notes |
|---|---|---:|---|---|
| repository.readme.exists | Present | 5 | Agreement | User-provided judgement |
| repository.description.present | Present | 4 | Agreement | User-provided judgement |
| documentation.installation-guide | Missing | 0 | MajorDifference | User reports README.adoc contains Spring Initializr project preparation steps and Gradle/Maven run commands; Missing / 0 may omit visible installation/getting-started evidence. |
| repository.license.present | Present | 3 | Agreement | User-provided judgement |
| documentation.usage-or-demo | Missing | 0 | MajorDifference | User reports README.adoc contains explicit run commands, curl examples, expected output, and Java/Kotlin code examples; Missing / 0 may omit visible usage evidence. |
| process.commit-history | Strong | 7 | Agreement | User-provided judgement |
| process.commit-distribution | Strong | 8 | Agreement | User-provided judgement |
| process.issue-usage | Present | 5 | Agreement | User-provided judgement |
| process.pull-request-usage | Strong | 6 | Agreement | User-provided judgement |
| process.release-usage | Partial | 3 | Agreement | User-provided judgement |
| validation.test-structure | Present | 8 | Agreement | User-provided judgement |
| validation.continuous-integration | Partial | 2 | Agreement | User-provided judgement |
| portfolio.topics | Missing | 0 | Agreement | User-provided judgement |
| portfolio.homepage-or-live-demo | Present | 5 | Agreement | User-provided judgement |
| portfolio.presentation-completeness | Partial | 3 | MajorDifference | User reports this composite is derived from the disputed Usage/Demo result; at least four presentation pieces are visible rather than three. |

Allowed human results: `Agreement`, `MinorDifference`, `MajorDifference`, `CannotDetermine`.
