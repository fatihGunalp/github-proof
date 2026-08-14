# GitHub Proof — Operational Project Contract

Bu dosya canonical docs'un kısa operational özetidir.

**Source of truth değildir.** Herhangi bir uyuşmazlıkta `docs/` kazanır.

## Product Boundary

GitHub Proof:

- tek bir public GitHub repository analiz eder,
- dışarıdan gözlemlenebilen mühendislik kanıtlarını değerlendirir,
- açıklanabilir ve deterministik rapor üretir,
- repository üzerinde uygulanabilir geliştirme önerileri üretir.

GitHub Proof şunları yapmaz:

- junior/mid/senior sınıflandırması,
- işe alınabilirlik kararı,
- AI kullanım tespiti,
- kod sahipliği/orijinallik çıkarımı,
- geliştirici kişiliği veya genel yetkinlik hükmü,
- production-ready veya güvenli repository hükmü,
- MVP'de private repository analizi,
- MVP'de repository karşılaştırması.

## Canonical Pipeline

~~~text
Repository URL
→ RepositorySnapshot
→ EvidenceCollection
→ Signal Evaluation
→ SignalResult[]
→ CategoryResult[]
→ ScoreBreakdown
→ ImprovementPlan
→ RepositoryAnalysis
~~~

Kanıt ile yorum ayrıdır.

`EvidenceItem` puan veya kişisel yorum taşımaz.

## MVP Categories

~~~text
RepositoryUnderstandability
DevelopmentProcessVisibility
ValidationAndUsability
PortfolioPresentation
~~~

## 15 MVP Signals

### RepositoryUnderstandability

~~~text
repository.readme.exists                 max 5
repository.description.present           max 4
documentation.installation-guide         max 8
repository.license.present               max 3
documentation.usage-or-demo               max 7
~~~

### DevelopmentProcessVisibility

~~~text
process.commit-history                    max 7
process.commit-distribution               max 8
process.issue-usage                       max 5
process.pull-request-usage                max 6
process.release-usage                     max 6
~~~

### ValidationAndUsability

~~~text
validation.test-structure                max 10
validation.continuous-integration         max 8
~~~

### PortfolioPresentation

~~~text
portfolio.topics                          max 4
portfolio.homepage-or-live-demo           max 5
portfolio.presentation-completeness       max 5
~~~

`portfolio.presentation-completeness` bileşik sinyaldir.

~~~text
IncludeInOverallScore: false
~~~

Kategori skorunda görünür; overall skorda tekrar sayılmaz.

## Score Maxima

~~~text
Kategori içi maksimum toplam: 91
Overall'a dahil maksimum:      86
~~~

## Signal Status Contract

~~~text
Strong
Present
Partial
Missing
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

`Score` nullable integer'dır.

Puanlanabilir:

~~~text
Strong
Present
Partial
Missing
~~~

`Missing`:

~~~text
Score = 0
denominator'a dahil
yalnız gerekli veri başarıyla incelendiyse kullanılabilir
~~~

Puanlanamayan:

~~~text
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

Bunlarda:

~~~text
Score = null
numerator dışı
denominator dışı
~~~

`UnavailableEvidence` hiçbir zaman otomatik `Missing` değildir.

## Scoring Method

MVP v1 kesin yöntem:

~~~text
RawApplicableScoreNormalization
~~~

Overall:

~~~text
puanlanabilir + IncludeInOverallScore=true kazanılan puan
/
aynı sinyallerin applicable maximum puanı
× 100
~~~

Normalize skor en yakın tam sayıya yuvarlanır; tam `.5` yukarı yuvarlanır.

Applicable maximum `0` ise sahte skor üretme.

## Determinism

Aynı snapshot + aynı signal version + aynı RuleSetVersion aynı sonucu üretmelidir.

LLM veya serbest metin yorumu doğrudan score üretmez.

Signal scoring tabloları `docs/architecture/mvp-signal-definitions.md` içindeki kesin kurallardan gelir.

## Important Contract-Lock Decisions

- Signal skorları integer'dır.
- Serbest skor aralığı seçimi yoktur.
- Repository yaşı 14 günden azsa commit distribution `NotApplicable` olur.
- Küçük/eğitim/deneysel olduğu tahmin edilerek issue/PR/release otomatik `NotApplicable` yapılmaz.
- Issues özelliği kapalıysa issue usage `NotApplicable` olabilir.
- MVP v1 README/documentation heading matching Türkçe + İngilizce adayları destekler.
- Demo/homepage gerçek erişilebilirlik kontrolü zorunlu değildir.
- Workflow run durumu Actions API üzerinden zorunlu doğrulanmaz; görünür workflow tanımı analiz edilir.

## Reporting

Rapor en fazla:

~~~text
3 strong signal
3 critical gap
3 improvement action
~~~

gösterir.

Web ve Markdown aynı `RepositoryAnalysis` anlamını temsil etmelidir.

## Change Rule

Bu invariant'lardan biri mevcut task sırasında değişmek zorunda görünüyorsa sessizce değişiklik yapma.

~~~text
DUR
→ hangi canonical docs etkilenecek belirt
→ neden mevcut contract yetmiyor açıkla
→ kullanıcı kararı bekle
~~~
