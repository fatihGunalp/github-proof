# Automated Comparison — githubtraining/github-games

```text
Validation ID: validation-006
Frozen commit: ac5daf3081db10b40434cecad0fa53d91b0b3c7f
ManualReviewStatus: Completed
Judgement source: user-provided human review
AI/Codex judgement recorded as human: NO
```

## Counts

```text
Agreement: 12
MinorDifference: 0
MajorDifference: 3
CannotDetermine: 0
Agreement + MinorDifference: 12 / 15 (80.00%)
```

## Major Differences

```text
Signal key: documentation.usage-or-demo
Automated result: Missing / 0
Human concern: README demo amacıyla açık bağlantı içeriyor. Bağlantı insan kontrolünde 404 dönüyor; ancak MVP v1 erişilebilirlik doğrulamasını zorunlu kılmadığından Missing / 0 görünür demo kanıtını kaçırıyor.
Suspected issue: Version 1 Usage/Demo recognition may not detect this explicit demo-link shape. Broken-link/reachability semantiği gelecek rule version'da ayrıca değerlendirilmelidir.

Signal key: portfolio.homepage-or-live-demo
Automated result: Missing / 0
Human concern: README’de açık demo bağlantısı var. Bağlantı 404 olsa da MVP v1 reachability kontrolü yapmadığından Missing / 0 mevcut sözleşmeyle uyumsuz.
Suspected issue: Version 1 README canonical demo-link recognition may not detect this label/intent form. Broken-link/reachability semantiği gelecek rule version'da ayrıca değerlendirilmelidir.

Signal key: portfolio.presentation-completeness
Automated result: Partial / 3
Human concern: Composite, yukarıdaki Usage/Demo sonucundan türetildiği için eksik hesaplanmış durumda.
Suspected issue: dependent result; broken-link/reachability konusu ayrı follow-up olarak tutulmalı ve upstream usage/demo sınıflandırması çözüldükten sonra yeniden değerlendirilmelidir.

Repository-level note: README demo olarak tanımlanmış `https://githubschool.github.io/github-games/` bağlantısı içeriyor, ancak bağlantı insan kontrolünde 404 dönüyor. MVP v1 link erişilebilirliğini zorunlu olarak doğrulamadığından bu review'da detection farkı korunur. Gelecek rule version'da broken-link/reachability semantiği ayrıca değerlendirilmelidir.

Disposition: all records are visible follow-up concerns only; no evaluator, score, or rule version changed in Session 038.
```
