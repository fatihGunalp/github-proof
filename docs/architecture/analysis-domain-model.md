# GitHub Proof — Analiz Domain Modeli

Bu doküman, GitHub Proof MVP analiz akışında kullanılacak temel domain nesnelerini, sorumluluklarını ve aralarındaki ilişkileri tanımlar.

Amaç framework, veritabanı veya GitHub API ayrıntılarını belirlemek değil; analiz çekirdeğinin kavramsal modelini sabitlemektir.

---

## 1. Temel Akış

~~~text
Repository URL
      ↓
RepositorySnapshot
      ↓
EvidenceCollection
      ↓
Signal Evaluation
      ↓
SignalResult Listesi
      ↓
CategoryResult Listesi
      ↓
ScoreBreakdown
      ↓
ImprovementPlan
      ↓
RepositoryAnalysis
~~~

---

## 2. RepositoryAnalysis

Bir repository için tamamlanmış veya kısmi analiz sonucunun kök nesnesidir.

~~~text
RepositoryAnalysis
├── AnalysisId
├── Repository
├── Snapshot
├── Status
├── AnalyzerVersion
├── RuleSetVersion
├── ReportSchemaVersion
├── StartedAt
├── CompletedAt
├── SignalResults
├── CategoryResults
├── ScoreBreakdown
├── Strengths
├── CriticalGaps
├── ImprovementPlan
├── Limitations
└── Warnings
~~~

### Sorumlulukları

- Analizin hangi repository ve snapshot üzerinde yapıldığını taşımak
- Analiz sürümlerini görünür tutmak
- Sinyal ve kategori sonuçlarını bir arada sunmak
- Toplam puanlama sonucunu taşımak
- Güçlü sinyalleri ve kritik eksikleri göstermek
- Geliştirme planını rapor katmanına sunmak
- Eksik veri ve sınırlamaları açık biçimde taşımak

### Yapmaması gerekenler

- GitHub API çağrısı yapmak
- Sinyalleri doğrudan hesaplamak
- Markdown veya HTML üretmek
- Veritabanına kendisini kaydetmek
- Geliştirici hakkında seviye veya işe alınabilirlik kararı vermek

---

## 3. RepositoryIdentity

Analiz edilen repository'nin değişmeyen veya kimlik niteliğindeki bilgilerini taşır.

~~~text
RepositoryIdentity
├── Owner
├── Name
├── Url
├── DefaultBranch
├── IsPublic
├── IsArchived
├── IsFork
├── IsTemplate
└── PrimaryLanguage
~~~

### Kurallar

- `Owner`, `Name` ve `Url` zorunludur.
- MVP yalnızca public repository kabul eder.
- Fork, template ve archived durumları analiz sonucunda görünür olmalıdır.
- `PrimaryLanguage` bulunamadığında analiz başarısız sayılmamalıdır.

---

## 4. RepositorySnapshot

Analizin belirli bir anda dayandığı normalize edilmiş repository verisidir.

~~~text
RepositorySnapshot
├── Repository
├── CapturedAt
├── HeadCommitSha
├── Metadata
├── Readme
├── FileTree
├── Commits
├── Issues
├── PullRequests
├── Releases
├── Workflows
├── Topics
├── License
├── Languages
├── Contributors
├── Availability
└── CollectionWarnings
~~~

### Sorumlulukları

- GitHub'dan gelen verileri normalize edilmiş biçimde taşımak
- Analizin tekrar üretilebilmesi için snapshot zamanını ve commit SHA bilgisini saklamak
- Hangi veri alanlarının başarıyla toplandığını görünür hâle getirmek
- Veri toplama sırasında oluşan uyarıları taşımak

### Kural

Snapshot eksik olabilir.

Eksik snapshot otomatik olarak başarısız analiz anlamına gelmez. Kullanılabilir verilerle kısmi analiz üretilebilir.

---

## 5. DataAvailability

Her veri bölümünün erişilebilirlik durumunu taşır.

~~~text
DataAvailability
├── DataKey
├── Status
├── Reason
└── RetrievedAt
~~~

### Status değerleri

~~~text
Available
Unavailable
NotRequested
NotApplicable
RateLimited
Forbidden
Failed
~~~

### Amaç

Aşağıdaki iki durumun birbirine karıştırılmasını engellemek:

- Veri alındı fakat kanıt bulunamadı
- Veri teknik veya erişim problemi nedeniyle alınamadı

---

## 6. EvidenceCollection

Snapshot içerisinden sinyal değerlendirmesinde kullanılabilecek yapılandırılmış kanıtların koleksiyonudur.

~~~text
EvidenceCollection
├── RepositoryEvidence
├── DocumentationEvidence
├── ProcessEvidence
├── ValidationEvidence
├── PortfolioEvidence
└── EvidenceWarnings
~~~

### Örnek kanıtlar

- README dosyasının bulunması
- README içerisinde kurulum başlığı bulunması
- `.github/workflows` dizininin bulunması
- Commitlerin farklı günlere yayılması
- Release sayısı
- Test projesi olabilecek dosya ve klasörlerin bulunması
- Repository topics listesi
- Homepage bağlantısı

### Kural

Evidence nesnesi yorum veya puan taşımaz.

Yalnızca gözlemlenen gerçekleri ve bunların kaynaklarını taşır.

---

## 7. EvidenceItem

Tek bir gözlemlenebilir kanıtı temsil eder.

~~~text
EvidenceItem
├── Key
├── Source
├── Value
├── Location
├── ObservedAt
├── Confidence
└── Notes
~~~

### Örnek

~~~text
Key: readme.installation-section
Source: README.md
Value: true
Location: README.md#kurulum
Confidence: High
~~~

### Confidence değerleri

~~~text
High
Medium
Low
~~~

Confidence, kanıtın yorumlanma güvenini ifade eder; geliştirici veya repository kalitesini ifade etmez.

---

## 8. SignalDefinition

Bir sinyalin sabit ürün ve kural tanımını temsil eder.

~~~text
SignalDefinition
├── Key
├── Name
├── Category
├── Description
├── MaximumScore
├── ApplicabilityRules
├── EvaluationRules
├── EvidenceRequirements
├── ExplanationTemplate
├── RecommendationTemplate
├── IncludeInOverallScore
└── Version
~~~

### Sorumlulukları

- Sinyalin hangi kategoriye ait olduğunu tanımlamak
- Hangi repository türlerinde uygulanabilir olduğunu belirtmek
- Gerekli kanıtları tanımlamak
- Maksimum puanı sabitlemek
- Açıklama ve öneri çerçevesini belirlemek
- Sinyalin toplam görünür kanıt skoruna dahil edilip edilmeyeceğini belirtmek
- Kural sürümünü taşımak

### Kural

Bir sinyal tanımı değiştiğinde `Version` değeri de değişmelidir.

---

## 9. SignalResult

Tek bir sinyal değerlendirmesinin sonucudur.

~~~text
SignalResult
├── SignalKey
├── SignalVersion
├── Category
├── Status
├── Score
├── MaximumScore
├── Evidence
├── Explanation
├── Recommendation
├── Limitations
└── EvaluatedAt
~~~

### Status değerleri

~~~text
Strong
Present
Partial
Missing
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

### Kurallar

- `Score`, sıfırdan küçük olamaz.
- `Score`, `MaximumScore` değerini aşamaz.
- `NotApplicable` toplam maksimum puana dahil edilmez.
- `UnavailableEvidence` otomatik olarak sıfır puan değildir.
- `Missing` yalnızca gerekli veri başarıyla incelendiyse kullanılabilir.
- Her sonuç en az bir açıklama taşımalıdır.
- `Partial`, `Missing` ve `RequiresHumanReview` mümkünse uygulanabilir öneri taşımalıdır.

---

## 10. CategoryResult

Aynı kategoriye ait sinyal sonuçlarının özetidir.

~~~text
CategoryResult
├── Category
├── Score
├── MaximumScore
├── NormalizedScore
├── Status
├── SignalResults
├── Summary
└── Limitations
~~~

### Category değerleri

~~~text
RepositoryUnderstandability
DevelopmentProcessVisibility
ValidationAndUsability
PortfolioPresentation
~~~

### Status değerleri

~~~text
Completed
Partial
InsufficientData
NotEvaluated
~~~

### Kural

Kategori sonucu kendi sinyal sonuçlarından türetilmelidir. Ayrı ve bağımsız bir kara kutu puan üretmemelidir.

---

## 11. ScoreBreakdown

Toplam görünür kanıt skorunun nasıl oluştuğunu açıklar.

~~~text
ScoreBreakdown
├── EarnedScore
├── ApplicableMaximumScore
├── NormalizedScore
├── Label
├── CategoryBreakdowns
├── ExcludedSignals
├── ExcludedFromOverallScoreSignals
└── CalculationMethod
~~~

### CalculationMethod

MVP için:

~~~text
RawApplicableScoreNormalization
~~~

### Kural

Toplam skor şu yöntemle hesaplanır:

~~~text
Toplam skor =
Uygulanabilir sinyallerden kazanılan puan
/
Uygulanabilir sinyallerin maksimum puanı
× 100
~~~

---

## 12. AnalysisHighlight

Güçlü sinyalleri ve kritik eksikleri ortak bir modelle temsil eder.

~~~text
AnalysisHighlight
├── Type
├── SignalKey
├── Title
├── Summary
├── ScoreImpact
└── EvidenceReferences
~~~

### Type değerleri

~~~text
Strength
CriticalGap
HumanReviewRequired
~~~

### Kural

MVP raporunda en fazla:

- Üç güçlü sinyal
- Üç kritik eksik

gösterilir.

---

## 13. ImprovementPlan

Repository'nin görünür kanıtlarını güçlendirmek için önceliklendirilmiş eylemleri taşır.

~~~text
ImprovementPlan
├── Actions
├── GeneratedAt
└── RuleSetVersion
~~~

---

## 14. ImprovementAction

Tek bir uygulanabilir geliştirme adımıdır.

~~~text
ImprovementAction
├── Priority
├── Title
├── Reason
├── ExpectedImpact
├── RelatedSignals
├── Effort
└── EvidenceReferences
~~~

### Effort değerleri

~~~text
Low
Medium
High
Unknown
~~~

### Kurallar

- MVP raporunda en fazla üç eylem gösterilir.
- Her eylem en az bir sinyal sonucuna dayanmalıdır.
- Eylem repository üzerinde uygulanabilir olmalıdır.
- Genel ve belirsiz tavsiyeler üretilmemelidir.
- Geliştirici hakkında kişisel yargı içermemelidir.

---

## 15. AnalysisStatus

Repository analizinin genel durumunu belirtir.

~~~text
Completed
Partial
InsufficientData
Failed
~~~

### Completed

Gerekli verilerin yeterli bölümü toplanmış ve analiz tamamlanmıştır.

### Partial

Bazı veriler alınamamış olsa da anlamlı bir kısmi analiz üretilmiştir.

### InsufficientData

Güvenilir rapor üretmek için yeterli kanıt yoktur.

### Failed

Teknik hata nedeniyle analiz sonucu üretilememiştir.

---

## 16. AnalysisWarning

Analiz sırasında kullanıcıya gösterilmesi gereken sınırlama veya uyarıyı taşır.

~~~text
AnalysisWarning
├── Code
├── Severity
├── Message
├── RelatedDataKeys
└── RelatedSignals
~~~

### Severity değerleri

~~~text
Info
Warning
Critical
~~~

### Örnek uyarılar

- GitHub API rate limit nedeniyle commit geçmişinin tamamı alınamadı.
- Repository bir fork olduğu için sahiplik çıkarımı yapılamaz.
- Repository arşivlenmiş durumdadır.
- Test klasörü bulundu ancak test framework'ü doğrulanamadı.
- Repository çok yeni olduğu için zamana yayılım sinyali sınırlıdır.

---

## 17. Domain Sınırları

Domain modeli aşağıdaki bağımlılıkları bilmemelidir:

- GitHub REST API istemcisi
- HTTP
- JSON serialization ayrıntıları
- Entity Framework Core
- PostgreSQL
- Blazor
- ASP.NET Core endpoint'leri
- Markdown renderer
- LLM sağlayıcısı

Domain yalnızca analiz kavramlarını, kuralları ve sonuçları temsil etmelidir.

---

## 18. MVP Model İlkeleri

- Immutable nesneler tercih edilmelidir.
- Zorunlu alanlar boş bırakılamamalıdır.
- String tabanlı durum değerleri yerine enum veya value object kullanılmalıdır.
- Puan hesaplaması tek bir yerde tanımlanmalıdır.
- `IncludeInOverallScore: false` olan bileşik sinyaller toplam skorda yeniden sayılmamalıdır.
- Eksik veri ile eksik kanıt ayrılmalıdır.
- Her sonuç kaynak kanıta bağlanabilmelidir.
- Sürüm bilgileri analiz sonucunda saklanmalıdır.
- Tarihler UTC olarak tutulmalıdır.
- Geliştirici seviyesi veya işe alınabilirlik alanı modele eklenmemelidir.

---

## 19. İlk Uygulama Sırası

Domain modeli kodlanırken aşağıdaki sıra izlenmelidir:

1. Enum ve value object tanımları
2. `RepositoryIdentity`
3. `DataAvailability`
4. `RepositorySnapshot`
5. `EvidenceItem`
6. `EvidenceCollection`
7. `SignalDefinition`
8. `SignalResult`
9. `CategoryResult`
10. `ScoreBreakdown`
11. `AnalysisHighlight`
12. `ImprovementAction`
13. `ImprovementPlan`
14. `AnalysisWarning`
15. `RepositoryAnalysis`

---

## 20. Faz 1 Çıkış Kriterine Katkısı

Bu belge tamamlandığında:

- Analiz kök nesnesi tanımlanmış olur.
- Snapshot ile analiz sonucu ayrılmış olur.
- Kanıt ile yorum ayrılmış olur.
- Sinyal tanımı ile sinyal sonucu ayrılmış olur.
- Eksik kanıt ile erişilemeyen veri ayrılmış olur.
- Puanlama modelinin veri yapıları tanımlanmış olur.
- Geliştirme planı modeli tanımlanmış olur.
- Domain katmanının teknik bağımlılık sınırları sabitlenmiş olur.
