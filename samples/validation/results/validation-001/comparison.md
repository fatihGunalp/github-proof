# Automated Comparison — khellang/Scrutor

```text
Validation ID: validation-001
Frozen commit: 7f315dab5b0f7134a6be58941db1da8c904507e2
ManualReviewStatus: Completed
Judgement source: user-provided human review
AI/Codex judgement recorded as human: NO
```

## Counts

```text
Agreement: 14
MinorDifference: 0
MajorDifference: 1
CannotDetermine: 0
Agreement + MinorDifference: 14 / 15 (93.33%)
```

## Major Difference

```text
Signal key: validation.continuous-integration
Automated result: Partial / 2
Automated evidence: one complete workflow corpus; no bounded Version 1 build or test command recognized.
Human concern: GitHub Actions workflow includes an explicit `dotnet test` invocation through the dotnet-coverage tool, so Partial / 2 understates the observable CI evidence.
Suspected issue: Version 1 CI command-family recognition may not detect the dotnet-coverage invocation shape.
Disposition: visible follow-up record only; no evaluator, score, or rule version changed in Session 038.
```

## Signal Outcomes

| # | Signal | Human result |
|---:|---|---|
| 1 | repository.readme.exists | Agreement |
| 2 | repository.description.present | Agreement |
| 3 | documentation.installation-guide | Agreement |
| 4 | repository.license.present | Agreement |
| 5 | documentation.usage-or-demo | Agreement |
| 6 | process.commit-history | Agreement |
| 7 | process.commit-distribution | Agreement |
| 8 | process.issue-usage | Agreement |
| 9 | process.pull-request-usage | Agreement |
| 10 | process.release-usage | Agreement |
| 11 | validation.test-structure | Agreement |
| 12 | validation.continuous-integration | MajorDifference |
| 13 | portfolio.topics | Agreement |
| 14 | portfolio.homepage-or-live-demo | Agreement |
| 15 | portfolio.presentation-completeness | Agreement |
