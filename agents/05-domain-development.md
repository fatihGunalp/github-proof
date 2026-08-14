# Faz 1 — Domain Development Playbook

Bu playbook GitHub Proof Domain foundation ve scoring/domain primitive işleri için kullanılır.

Canonical kaynak:

~~~text
docs/architecture/analysis-domain-model.md
~~~

Sinyal/scoring davranışı gerekiyorsa ayrıca:

~~~text
docs/architecture/mvp-signal-definitions.md
docs/product/scoring-and-report-contract.md
~~~

## Domain Boundary

Domain katmanı şunları bilmez:

~~~text
GitHub REST API client
HTTP
JSON serialization ayrıntıları
Entity Framework Core
PostgreSQL
Blazor
ASP.NET Core endpoint'leri
Markdown renderer
LLM provider
~~~

Domain yalnız analiz kavramlarını, invariant'ları ve sonuçları temsil eder.

## Model İlkeleri

- Immutable nesneleri tercih et.
- Zorunlu alanları construction sırasında koru.
- Status/category gibi kapalı kümeleri enum veya güçlü type ile temsil et.
- Tarihleri UTC olarak taşı.
- Score kurallarını dağınık helper'lara yayma.
- `Score` nullable integer contract'ını koru.
- `IncludeInOverallScore` davranışını açık temsil et.
- Evidence ile interpretation'ı ayır.
- Framework annotation'larını Domain'a taşımamaya çalış.
- Geliştirici level/employability alanı ekleme.

## İlk Uygulama Sırası

Canonical domain model sırası:

~~~text
1. Enum ve value object tanımları
2. RepositoryIdentity
3. DataAvailability
4. RepositorySnapshot
5. EvidenceItem
6. EvidenceCollection
7. SignalDefinition
8. SignalResult
9. CategoryResult
10. ScoreBreakdown
11. AnalysisHighlight
12. ImprovementAction
13. ImprovementPlan
14. AnalysisWarning
15. RepositoryAnalysis
~~~

Task scope daha küçükse yalnız gerekli parçayı yap; bütün listeyi tek task'ta zorla implement etme.

## SignalResult Invariants

Unit test ile güvenceye alınması beklenen temel kurallar:

~~~text
Score < 0 olamaz
Score > MaximumScore olamaz
Strong/Present/Partial/Missing => numeric Score
Missing => Score == 0
NotApplicable => Score == null
UnavailableEvidence => Score == null
RequiresHumanReview => Score == null
EvaluatedAt UTC
Explanation boş olamaz
~~~

Recommendation zorunluluğunda canonical docs dilini izle; gereksiz stricter invariant icat etme.

## CategoryResult Invariants

- Kendi SignalResult listesi üzerinden türetilir.
- Ayrı black-box score üretmez.
- Puanlanamayan status'lar numerator/denominator dışıdır.
- `IncludeInOverallScore=false` kategori skorunu etkilemez.
- presentation completeness PortfolioPresentation kategori skoruna dahildir.

## ScoreBreakdown Invariants

~~~text
CalculationMethod = RawApplicableScoreNormalization
~~~

Overall'a yalnız:

~~~text
IncludeInOverallScore = true
+
Strong/Present/Partial/Missing
~~~

sonuçları girer.

`NotApplicable`, `UnavailableEvidence`, `RequiresHumanReview` dışarıdadır.

Applicable maximum 0 ise normalize score üretme.

Yuvarlama `.5` yukarı olacak şekilde açık test edilmelidir; runtime/language default rounding davranışına kör güvenme.

## Determinism

Clock gibi non-deterministic dependency evaluator logic içine gizlenmemelidir.

`EvaluatedAt` audit metadata olabilir; score kararını değiştirmemelidir.

Aynı input aynı score/status üretmelidir.

## Test Stratejisi

Öncelik:

~~~text
constructor/value invariant tests
status-score compatibility tests
normalization denominator tests
excluded composite signal tests
rounding tests
zero applicable maximum tests
~~~

Daha sonra evaluator-specific tests.

## Overengineering Guard

Faz 1'de gerekmedikçe:

- generic rules framework,
- dynamic expression engine,
- plugin architecture,
- reflection tabanlı evaluator discovery,
- database persistence abstraction,
- message bus,
- LLM abstraction

kurma.

Önce contract'ı en sade type'larla doğru temsil et.
