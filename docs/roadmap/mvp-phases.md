# GitHub Proof — MVP Fazları

Bu doküman, GitHub Proof projesinin MVP'ye ulaşması için izlenecek geliştirme fazlarını, kapsam sınırlarını ve çıkış kriterlerini tanımlar.

## MVP Tanımı

GitHub Proof MVP'si:

> Public bir GitHub repository URL'si alan, 15 açıklanabilir MVP sinyalini analiz eden, dört kategoride sonuç ve üç öncelikli geliştirme önerisi üreten çalışan bir web uygulamasıdır.

MVP'nin temel kullanıcı akışı:

```text
Repository URL'si gir
        ↓
GitHub verilerini topla
        ↓
Açıklanabilir sinyalleri değerlendir
        ↓
Kategori sonuçlarını oluştur
        ↓
Öncelikli geliştirme planını göster
```

MVP'nin amacı bir geliştiriciyi puanlamak değil, repository içerisinde dışarıdan gözlemlenebilen mühendislik kanıtlarını görünür hâle getirmektir.

---

## Faz 0 — Ürün Tanımı ve Sınırlar

### Amaç

GitHub Proof'un neyi ölçtüğünü, neyi ölçmediğini ve ilk sürümün hangi iddialarda bulunabileceğini kesinleştirmek.

### Çıktılar

- MVP ürün sözleşmesi
- Analiz kategorileri
- Puanlama kuralları
- Riskli ve yasaklı iddialar listesi
- Örnek rapor formatı
- Test edilecek örnek repository seti

### MVP analiz kategorileri

1. Repository anlaşılabilirliği
2. Geliştirme süreci görünürlüğü
3. Doğrulama ve kullanılabilirlik
4. Portföy sunumu

### MVP kapsamı dışında kalanlar

- Geliştiricinin seviyesini ölçmek
- Kod kalitesini bütünüyle değerlendirmek
- AI kullanımını tespit etmek
- Mimari olgunluk hakkında kesin hüküm vermek
- Private repository analizi
- Recruiter adına işe alım kararı vermek
- Birden fazla repository'yi karşılaştırmak
- Kullanıcı hesabı ve analiz geçmişi
- Ödeme ve takım özellikleri

### Çıkış kriteri

Aynı repository, aynı veri ve aynı kural setiyle analiz edildiğinde aynı sonucu üretmelidir.

---

## Faz 1 — Analiz Modeli

### Amaç

GitHub'dan alınacak ham verileri, açıklanabilir ve tekrar üretilebilir mühendislik sinyallerine dönüştürecek domain modelini kurmak.

### Temel domain modeli

```text
RepositoryAnalysis
├── RepositorySnapshot
├── EvidenceCollection
├── CategoryResult
├── SignalResult
├── ScoreBreakdown
└── ImprovementPlan
```

### Kesin MVP sinyalleri

#### Repository anlaşılabilirliği

- README varlığı
- Proje açıklaması
- Kurulum talimatı
- Lisans
- Kullanım örneği veya demo

#### Geliştirme süreci görünürlüğü

- Commit geçmişi
- Commitlerin zamana yayılımı
- Issue kullanımı
- Pull request kullanımı
- Release kullanımı

#### Doğrulama ve kullanılabilirlik

- Test yapısı
- Continuous Integration

#### Portföy sunumu

- GitHub Topics
- Homepage veya canlı demo bağlantısı
- Repository sunum bütünlüğü

`Repository sunum bütünlüğü` bileşik sinyaldir ve toplam görünür kanıt skoruna yeniden dahil edilmez.

### Puanlama ilkesi

Toplam skor tek başına gösterilmemelidir. Her puan, kanıtı ve açıklamasıyla birlikte sunulmalıdır.

```text
README bulundu: +5
Kurulum adımı bulunamadı: +0
Lisans bulundu: +3
Test yapısı bulundu: +8
Release bulunamadı: +0
```

### Çıkış kriteri

Domain modeli ve 15 sinyal sözleşmesi kodlanabilir kesinlikte kilitlenir. İlk otomatik repository karşılaştırmaları Signal Engine tamamlandıktan sonra doğrulama seti üzerinde yürütülür.

---

## Faz 2 — GitHub Veri Toplama Katmanı

### Amaç

Public bir GitHub repository'sinden MVP için gereken verileri güvenilir ve normalize edilmiş biçimde toplamak.

### Toplanacak veriler

- Repository metadata
- README
- Languages
- Default branch
- Commit geçmişi
- Contributors
- Issues
- Pull requests
- Releases
- Tags
- Workflow dosyaları
- Repository contents
- Topics
- License

### Teknik işler

- GitHub REST API istemcisi
- Repository URL ayrıştırma
- API hata yönetimi
- Rate limit kontrolü
- Bulunamayan repository yönetimi
- Arşivlenmiş repository kontrolü
- Büyük repository sınırları
- API cevaplarının normalize edilmesi
- Default HEAD davranışını koruyan, ref-pinlenebilir repository içerikleri için explicit commit SHA collection desteği
- Historical commit isteğinde ref-pinli content ile live/non-atomic GitHub alanlarının snapshot uyarılarında ayrıştırılması

### Bu fazda yapılmayacaklar

- Veritabanı
- Kullanıcı hesabı
- Background job
- Webhook
- OAuth ile private repository
- Kalıcı analiz geçmişi

### Çıkış kriteri

Sistem geçerli bir public repository URL'sinden gerekli verileri almalı ve normalize edilmiş bir `RepositorySnapshot` üretmelidir.

---

## Faz 3 — Kural Tabanlı Signal Engine

**Durum:** COMPLETE — 2026-08-15, Session 041 final exit validation PASS.

Kapanış doğrulaması: 14 bağımsız evaluator ve derived `portfolio.presentation-completeness` canonical sırada 15 sonuç üretir; determinism PASS; ilk 10 repository için 150/150 human comparison tamamlanmıştır ve 138 `Agreement + MinorDifference` ile kabul oranı %92.00'dır (eşik: >= %80.00).

### Amaç

Toplanan repository verilerinden açıklanabilir analiz sonuçları üretmek.

### İlk sinyal yapısı

~~~text
Signals/
├── ReadmeExistsSignal
├── RepositoryDescriptionSignal
├── InstallationGuideSignal
├── LicenseSignal
├── UsageOrDemoSignal
├── CommitHistorySignal
├── CommitDistributionSignal
├── IssueUsageSignal
├── PullRequestUsageSignal
├── ReleaseUsageSignal
├── TestStructureSignal
├── ContinuousIntegrationSignal
├── TopicsSignal
├── HomepageOrLiveDemoSignal
└── PresentationCompletenessSignal
~~~

### Her sinyalin çıktısı

~~~text
SignalResult
├── SignalKey
├── SignalVersion
├── Category
├── Status
├── Score
├── MaximumScore
├── IncludeInOverallScore
├── Evidence
└── EvaluatedAt
~~~

### Örnek sinyal sonucu

Analysis truth locale-independent tutulur:

~~~text
SignalKey: documentation.installation-guide
SignalVersion: 1
Category: RepositoryUnderstandability
Status: Missing
Score: 0
MaximumScore: 8
IncludeInOverallScore: true

Evidence:
README ve `docs/**/*.md` corpus'u incelendi; desteklenen installation/setup talimatı gözlemlenmedi.
~~~

Kullanıcıya görünen title, explanation, recommendation ve limitation metinleri `SignalResult` içerisinde tutulmaz. Bunlar aynı semantic sonuçtan Application presentation/localization katmanında üretilir.

Örneğin Türkçe presentation:

~~~text
Kurulum Talimatı

Repository üzerinde açık bir kurulum akışı gözlemlenmedi.

Öneri:
README dosyasına bağımlılıkları, yapılandırmayı ve çalıştırma komutlarını ekleyin.
~~~

Aynı semantic sonuç İngilizce presentation ile de gösterilebilir; locale değişimi repository collection, evaluator, score, status veya evidence sonucunu değiştirmez.

### Tasarım kuralı

MVP puanlaması deterministik, sürümlenmiş ve gözlemlenebilir kanıta dayalı olmalıdır.

LLM veya başka bir kara kutu karar mekanizması doğrudan puan üretmemelidir.

### Çıkış kriteri

Rapor üzerindeki her puanın hangi gözlemlenebilir kanıta ve hangi sinyal sürümüne dayanarak verildiği açıklanabilmelidir.

---

## Faz 4 — Rapor Üretimi

**Durum:** COMPLETE — typed localized `RepositoryReport`, deterministic Markdown renderer ve Phase 4 exit validation tamamlandı.

### Amaç

Teknik analiz sonuçlarını, kullanıcının anlayabileceği ve uygulayabileceği bir geliştirme raporuna dönüştürmek.

### MVP raporu

#### Genel özet

```text
Repository Kanıt Profili

Bu repository neyi kanıtlıyor?
- Zamana yayılmış geliştirme geçmişi
- Görünür test yapısı

Kanıtı ne zayıflatıyor?
- Kurulum ve kullanım dokümantasyonu eksik

Önce bunları yap
1. Kurulum adımlarını belgeleyin
2. CI doğrulaması ekleyin
3. Kullanım örneği ekleyin

İkincil skor özeti: 62 / 100
```

#### Kategori sonuçları

- Repository anlaşılabilirliği
- Geliştirme süreci görünürlüğü
- Doğrulama ve kullanılabilirlik
- Portföy sunumu

#### Güçlü sinyaller

En fazla üç madde gösterilir.

#### Kritik eksikler

En fazla üç madde gösterilir.

#### Öncelikli geliştirme planı

Kullanıcıya uzun ve dağınık bir öneri listesi yerine, en yüksek etkili üç eylem sunulur.

### MVP rapor formatları

- Web sonucu
- Markdown çıktısı

### MVP sonrası formatlar

- PDF
- LinkedIn paylaşım kartı
- GitHub badge
- Public rapor bağlantısı

### Çıkış kriteri

Kullanıcı raporu okuduğunda repository'sinde ilk olarak neyi geliştirmesi gerektiğini anlayabilmelidir.

### Phase 4 implementation exit

Phase 4 uygulama çıkışı tamamlandı:

- locale-independent `RepositoryAnalysis`,
- EN/TR typed `RepositoryReport`,
- exact evidence-first section order,
- explicit uncertainty boundary,
- secondary score presentation,
- deterministic Markdown renderer,
- versioned product-boundary disclaimer,
- repository/evidence literal preservation,
- full regression ve deterministic output doğrulaması.

Web sonucu Phase 5'te aynı typed `RepositoryReport` contract'ını doğrudan tüketir; Markdown çıktısını parse etmez.

---

## Faz 5 — Minimal Web Arayüzü

### Amaç

Kullanıcının repository URL'sini girip analiz raporuna ulaşabileceği en küçük çalışan arayüzü geliştirmek.

### Kullanıcı akışı

```text
Ana Sayfa
    ↓
Repository URL Girişi
    ↓
Analiz Ediliyor
    ↓
Rapor Sonucu
```

### Ana sayfa

- Tek cümlelik ürün vaadi
- Repository URL alanı
- `Repository'yi analiz et` butonu
- Ölçülen alanların kısa açıklaması
- Geliştiriciyi değerlendirmediğine dair uyarı

### Sonuç ekranı

Evidence-first ilk okuma sırası korunur:

- Repository identity / context
- Bu repository neyi kanıtlıyor? — en fazla üç güçlü kanıt
- Kanıtı ne zayıflatıyor? — en fazla üç kritik boşluk
- Önce bunları yap — en fazla üç geliştirme adımı
- Explicit uncertainty — varsa `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview`
- Kategori sonuçları ve ikincil skor özeti
- Signal detail ve evidence
- Product boundary disclaimer
- Markdown raporunu kopyalama

Toplam skor kullanıcı deneyiminin ana kahramanı değildir; evidence-first bölümlerden sonra ikincil özet olarak gösterilir.

### MVP kapsamı dışında kalan arayüzler

- Kullanıcı kaydı
- Profil sayfası
- Dashboard
- Analiz geçmişi
- Sosyal giriş
- Ödeme
- Takım özelliği
- Yönetim paneli

### Çıkış kriteri

Yeni bir kullanıcı ek açıklamaya ihtiyaç duymadan repository URL'sini girip rapora ulaşabilmelidir.

---

## Faz 6 — MVP Doğrulama

### Amaç

Kodun çalışmasını değil, ürünün gerçek kullanıcıya değer üretip üretmediğini doğrulamak.

### İlk test kitlesi

- Yazılıma yeni başlayanlar
- Junior geliştiriciler
- Portföy hazırlayanlar
- Eğitmenler ve mentorlar
- Teknik işe alım yapan sınırlı sayıda kullanıcı

### İlk test kapsamı

20–30 public repository analiz edilir.

### Kullanıcı doğrulama soruları

1. Rapor beklemediğiniz bir eksik gösterdi mi?
2. İlk üç öneriden en az biri uygulanabilir miydi?
3. Sonuç adil ve açıklanabilir göründü mü?
4. Repository üzerinde gerçek bir değişiklik yaptınız mı?
5. Bu raporu başka birine gönderir miydiniz?

### Başarı ölçütleri

- Kullanıcıların en az %60'ı bir öneriyi faydalı bulmalı.
- Kullanıcıların en az %30'u repository üzerinde gerçek bir değişiklik yapmalı.
- Analizlerin en az %80'i teknik hata olmadan tamamlanmalı.
- Kullanıcıların çoğu skorun neden oluştuğunu anlayabilmeli.
- Tekrar eden ciddi yanlış değerlendirme şikâyeti bulunmamalı.

### Çıkış kriteri

Kullanıcıların analiz sonrasında repository'lerinde ölçülebilir iyileştirmeler yapması.

---

## Faz 7 — LinkedIn Pilot Lansmanı

### Amaç

MVP'yi gerçek repository'lerle test etmek, kullanıcı dili toplamak ve projenin organik talebini doğrulamak.

### Gönderi vaadi

> GitHub reponuzun ne kadar iyi olduğunu değil, dışarıdan bakıldığında neyi kanıtlayabildiğini analiz eden açık kaynak bir araç geliştirdim.

### İlk çağrı

> İlk sürümü gerçek repolarla test etmek istiyorum.
> Analiz edilmesini istediğiniz public GitHub reponuzu yorumlara bırakın.

### Kullanıcıya verilen değer

- Kişisel repository raporu
- Üç öncelikli geliştirme adımı
- Kopyalanabilir Markdown çıktısı

### Projenin kazanacağı veri

- Gerçek repository örnekleri
- Yanlış pozitifler
- Eksik sinyaller
- Kullanıcıların kullandığı dil
- Yeni içerik örnekleri
- Potansiyel eğitim ve mentorluk ihtiyaçları

### Kritik sınır

İlk pilotta sistemin yüzlerce repository'yi otomatik analiz ettiği izlenimi verilmemelidir. Yarı manuel süreç kullanılıyorsa açıkça belirtilmelidir.

### Çıkış kriteri

Gerçek kullanıcı repository'leriyle tekrarlanabilir analizler üretilmesi ve ürünün sonraki sürüm kararlarını destekleyecek yeterli geri bildirim toplanması.

---

# MVP Sonrası Fazlar

## Faz 8 — Analiz Geçmişi

- Kullanıcı hesabı
- Önceki analizler
- Skor değişimi
- Repository gelişim zaman çizelgesi

## Faz 9 — Paylaşım Katmanı

- LinkedIn sonuç kartı
- GitHub badge
- Public rapor bağlantısı
- Önce ve sonra karşılaştırması

## Faz 10 — Recruiter View

- Hızlı repository özeti
- Kanıt kategorileri
- Eksik veya riskli alanlar
- Adayları sıralamadan karşılaştırılabilir sinyaller

## Faz 11 — Derin Teknik Analiz

- Kod tabanı yapısı
- Test kapsamı sinyalleri
- Dependency riskleri
- Mimari karar kayıtları
- Maintainability göstergeleri

## Faz 12 — LLM Destekli Yorumlama

LLM, kural tabanlı kanıtların üzerine açıklama katmanı olarak eklenebilir.

Örnek kullanım alanları:

- README özetleme
- Proje amacının anlaşılabilirliğini yorumlama
- Teknik karar açıklamalarını değerlendirme
- Kişiselleştirilmiş geliştirme planı üretme

LLM, MVP'de puanı doğrudan belirlememelidir.

---

# Önerilen Geliştirme Sırası

```text
Faz 0 — Ürün tanımı ve sınırlar
Faz 1 — Analiz modeli
Faz 2 — GitHub veri toplama
Faz 3 — Signal Engine
Faz 4 — Rapor üretimi
Faz 5 — Minimal web arayüzü
Faz 6 — Gerçek repository doğrulaması
Faz 7 — LinkedIn pilot lansmanı
```

# Definition of Done — MVP

MVP tamamlanmış sayılır when:

- Public GitHub repository URL'si kabul edilebiliyor.
- Repository verileri GitHub API üzerinden toplanabiliyor.
- 15 açıklanabilir MVP sinyali değerlendiriliyor.
- Sonuçlar dört kategori altında gösteriliyor.
- Her skorun dayandığı kanıt açıklanıyor.
- Kullanıcıya üç öncelikli geliştirme önerisi sunuluyor.
- Web raporu ve Markdown çıktısı üretilebiliyor.
- En az 20 gerçek repository ile doğrulama yapılmış oluyor.


---

# Public Profile Evidence V1 — Expansion Roadmap

## Roadmap Statüsü

Bu bölüm mevcut Repository MVP V1 roadmap'inin replacement'ı değildir.

Repository MVP'nin Faz 0–7 hattı ve mevcut post-MVP Faz 8–12 kayıtları
tarihsel ve ürün bağlamı olarak korunur.

Yeni ürün yönü ayrı bir expansion track olarak ilerler:

~~~text
Repository Evidence
        ↓
preserved atomic evidence layer

Public Profile Evidence
        ↓
new macro aggregation layer
~~~

Canonical product boundary:

~~~text
docs/product/public-profile-evidence-product-contract.md
~~~

Bu profile roadmap boyunca:

- developer-level toplam skor üretilmez,
- seniority üretilmez,
- employability kararı verilmez,
- hire/no-hire sonucu üretilmez,
- sparse public GitHub negatif developer evidence sayılmaz,
- profile claim'leri repository evidence'a geri izlenebilir olmalıdır,
- LLM structured truth'tan önce karar verici olarak kullanılamaz.

---

## Profile P0 — Product Contract

**Durum:** CONTRACT LOCKED — 2026-08-16 / Session 050

### Amaç

Public Profile Evidence'ın:

- neyi temsil ettiğini,
- neyi temsil etmediğini,
- Repository MVP V1 ile ilişkisini,
- recruiter kullanım sınırını,
- evidence traceability zorunluluğunu,
- sparse-profile davranışını,
- LLM sınırını,
- yasaklı developer-level çıkarımları

kilitlemek.

### Çıkış Kriteri

~~~text
Public Profile Evidence V1 product contract:
CONTRACT LOCKED
~~~

Production implementation bu product boundary'i sessizce değiştiremez.

---

## Profile P1 — Profile Domain Model Contract

### Amaç

Profile-level structured truth için gerekli domain kavramlarını kodlanabilir
kesinlikte tanımlamak.

En az şu kavramlar ele alınmalıdır:

~~~text
GitHubProfileIdentity
GitHubProfileSnapshot
ProfileRepository
RepositoryEligibilityResult
CrossRepositoryEvidence
TechnicalFocusArea
EngineeringPracticePattern
ProblemAreaPattern
RepresentativeRepository
RoleEvidenceAlignment
DeveloperPublicEvidenceProfile
ProfileLimitation
ProfileWarning
~~~

### Zorunlu Kararlar

- aggregate root sınırı,
- identity/value object sınırları,
- immutable veri modeli,
- nullable / unavailable semantics,
- evidence reference modeli,
- analysis status,
- versioning,
- timestamps,
- deterministic composer sınırı.

### Çıkış Kriteri

Profile domain modeli implementation başlamadan önce contract-locked olmalıdır.

---

## Profile P2 — Repository Eligibility Contract

### Amaç

Her public repository'nin profile-level çıkarımlara nasıl katkı
sağlayabileceğini deterministik olarak belirlemek.

En az şu durumlar açıkça ele alınmalıdır:

~~~text
Original
Fork
Template
Archived
Empty
InsufficientEvidence
Active
Historical
~~~

### Temel Soru

> Hangi repository hangi profile-level çıkarıma hangi koşullarda katkı
> sağlayabilir?

### Zorunlu Sınırlar

- Fork repetition'ı otomatik şişiremez.
- Template authorship kanıtı değildir.
- Archived repository otomatik olarak değersiz değildir.
- Empty repository negatif developer evidence değildir.
- Insufficient evidence ile Missing birbirine karıştırılmamalıdır.
- Eligibility sonucu geliştirici kalitesi değildir.

### Çıkış Kriteri

Aynı repository metadata/evidence girdisi ve aynı eligibility rule version aynı
eligibility sonucunu üretmelidir.

---

## Profile P3 — GitHub Profile Data Collection

### Amaç

GitHub username veya canonical public profile identity üzerinden normalize
profile snapshot üretmek.

Temel akış:

~~~text
GitHub username
↓
public profile metadata
↓
public repositories
↓
pagination
↓
repository metadata
↓
GitHubProfileSnapshot
~~~

### Teknik Kapsam

- username/profile identity validation,
- public repository pagination,
- fork/template/archive metadata,
- empty repository metadata,
- collection bounds,
- partial API failure,
- rate limit behavior,
- unavailable data semantics,
- deterministic repository ordering.

### Çıkış Kriteri

Aynı observable GitHub state ve aynı collection contract ile aynı normalize
profile snapshot üretilebilmelidir.

---

## Profile P4 — RepositoryAnalysis Orchestration

### Amaç

Mevcut repository analyzer'ı yeniden yazmadan profile içerisinde reuse etmek.

Doğru yön:

~~~text
GitHubProfileSnapshot
↓
eligible repository candidates
↓
existing repository collection / analysis pipeline
↓
RepositoryAnalysis[]
~~~

### Mimari Kural

Profile orchestration:

- mevcut 15 signal evaluator'ı kopyalamaz,
- repository score hesaplamaz,
- RepositoryAnalysis semantics'i değiştirmez,
- Web içerisinde analysis logic oluşturmaz.

### Çıkış Kriteri

Profile orchestration ile repository analysis engine arasında semantic
duplication bulunmamalıdır.

---

## Profile P5 — Cross-Repository Evidence Engine

### Amaç

Repository-level gözlemleri açıklanabilir profile-level aggregate evidence'a
dönüştürmek.

Temel trust chain:

~~~text
Profile claim
↓
CrossRepositoryEvidence
↓
Quantified observations
↓
Repository references
↓
Repository-level evidence
~~~

### Zorunlu Özellikler

- deterministic aggregation,
- versioned rules,
- repository contribution traceability,
- no hidden weighting,
- unavailable/insufficient evidence semantics,
- duplicate/fork inflation koruması.

### Çıkış Kriteri

Her aggregate observation kaynak repository ve underlying evidence'a geri
izlenebilmelidir.

---

## Profile P6 — Technical, Practice ve Problem Patterns

### Amaç

Cross-repository evidence üzerinden üç ayrı profile pattern türü üretmek.

~~~text
TechnicalFocusArea
EngineeringPracticePattern
ProblemAreaPattern
~~~

### Technical Focus

Teknolojinin public üretimde tekrar eden görünürlüğünü ifade eder.

Tek bir README keyword'ü dominant technical focus üretmemelidir.

### Engineering Practice

Örnek evidence alanları:

- automated testing,
- Continuous Integration,
- documentation,
- release usage,
- repository workflow evidence.

Doğru ifade:

> Test evidence birden fazla public repository'de tekrar ediyor.

Yanlış ifade:

> Bu geliştirici test konusunda iyidir.

### Problem Area

Yalnız bounded taxonomy ve açık evidence requirement ile üretilebilir.

README keyword matching tek başına problem-area truth değildir.

### Çıkış Kriteri

Aynı structured evidence ve aynı rule versions aynı pattern sonuçlarını
üretmelidir.

---

## Profile P7 — Representative Repositories ve Role Evidence Alignment

### Amaç

Profile-level örüntüleri incelemek için en açıklayıcı repository'leri ve bounded
technical-role overlap'ı üretmek.

### Representative Repository

Şu soruya cevap verir:

> Bu profile-level örüntüyü incelemek için hangi repository'lere önce
> bakılmalı ve neden?

Şu anlama gelmez:

- best repository,
- highest-quality repository,
- most professional repository.

Popularity varsayılan skill evidence değildir.

### Role Evidence Alignment

Yalnız:

~~~text
public GitHub evidence
↔
versioned technical role evidence definition
~~~

örtüşmesini ifade eder.

Şunları üretmez:

~~~text
Job Fit
Candidate Score
Hire Recommendation
Seniority
Employability
~~~

### Çıkış Kriteri

Her representative selection ve role alignment açık supporting evidence
taşımalıdır.

---

## Profile P8 — Deterministic Profile Composer ve Report Contract

### Amaç

Tüm profile evidence sonuçlarını tek deterministic structured truth altında
birleştirmek.

Temel yapı:

~~~text
RepositoryAnalysis[]
↓
CrossRepositoryEvidence
↓
TechnicalFocusArea[]
↓
EngineeringPracticePattern[]
↓
ProblemAreaPattern[]
↓
RepresentativeRepository[]
↓
RoleEvidenceAlignment[]
↓
DeveloperPublicEvidenceProfile
~~~

### Report Contract

İlk report en az şunları desteklemelidir:

- Public Profile Summary,
- Görünür Teknik Üretim Alanları,
- Engineering Practice Patterns,
- Problem Area Patterns,
- Representative Repositories,
- Role Evidence Alignment,
- Why This Conclusion?,
- Limitations,
- Repository Drill-Down.

### Presentation Boundary

~~~text
DeveloperPublicEvidenceProfile
        ↓
Developer View

DeveloperPublicEvidenceProfile
        ↓
Recruiter View
~~~

İki presentation aynı structured truth'u tüketir.

### Çıkış Kriteri

Aynı profile analysis truth ve aynı presentation contract aynı semantic report
çıktısını üretmelidir.

---

## Profile P9 — Validation Profile Set

### Amaç

Profile aggregation'ın farklı gerçek GitHub kullanım biçimlerinde yanıltıcı
sonuç üretmediğini doğrulamak.

En az şu profile tipleri değerlendirilmelidir:

~~~text
.NET-heavy
frontend-heavy
mixed full-stack
AI/ML-heavy
few repositories
many repositories
mostly forks
mostly archived
student/tutorial-heavy
polyglot
sparse public profile
~~~

### Özellikle Test Edilecek Riskler

- false technical focus,
- false role alignment,
- fork inflation,
- template inflation,
- README keyword inflation,
- single-repository dominance,
- archived-repository distortion,
- sparse-profile negative judgment,
- missing/unavailable data confusion,
- broken evidence traceability.

### Çıkış Kriteri

- prohibited inference: 0,
- unsupported profile claim: 0,
- evidence traceability: required for every meaningful claim,
- determinism: PASS.

---

## Profile P10 — Web Profile Flow

### Amaç

Mevcut repository analysis girişini koruyarak yeni profile analysis akışını
kullanıcıya sunmak.

Ürün girişleri:

~~~text
Analyze a repository

or

Analyze a public GitHub profile
~~~

İlişki:

~~~text
Profile = macro view
Repository = micro view
~~~

### İlk Okuma

Profile raporunun ilk katmanı kısa sürede şunları göstermelidir:

1. ana görünür technical focus,
2. tekrar eden engineering practices,
3. representative repositories,
4. bounded role evidence alignment,
5. neden bu sonuca ulaşıldığı.

### Drill-Down

~~~text
Profile claim
↓
Repository
↓
RepositoryReport
↓
Signal
↓
Evidence
~~~

### Çıkış Kriteri

Kullanıcı profile summary'den underlying repository evidence'a açık biçimde
inebilmeli ve presentation analysis truth'u yeniden üretmemelidir.

---

## Profile P11 — Recruiter Pilot

### Amaç

Recruiter için ürünün hiring decision değil, hızlı evidence orientation ve
routing değeri üretip üretmediğini doğrulamak.

### Pilot Soruları

- Teknik odağı kısa sürede anlayabildiniz mi?
- Sonucun neden üretildiğini görebildiniz mi?
- Önce incelenecek repository'leri bulabildiniz mi?
- Teknik görüşmede doğrulanacak alanlar faydalı mı?
- Yanıltıcı veya aşırı iddialı bir çıkarım gördünüz mü?

### Kritik Sınır

Pilot başarısı:

~~~text
better evidence navigation
better technical orientation
better interview preparation
~~~

ile ölçülür.

Şunlarla ölçülmez:

~~~text
automatic rejection accuracy
candidate ranking accuracy
hire/no-hire accuracy
~~~

---

## Profile P12 — Optional LLM Narration

### Amaç

Yalnız deterministic structured profile truth tamamlandıktan sonra insan
tarafından daha kolay okunabilir narration üretmek.

Doğru akış:

~~~text
structured profile truth
↓
LLM narration
~~~

Yanlış akış:

~~~text
raw GitHub
↓
LLM
↓
canonical profile judgment
~~~

LLM:

- evidence yaratamaz,
- count değiştiremez,
- technical focus uyduramaz,
- role alignment uyduramaz,
- seniority belirleyemez,
- employability kararı veremez.

Bu faz Public Profile Evidence V1 core truth engine için zorunlu değildir.

---

# Public Profile Evidence V1 — Önerilen Geliştirme Sırası

~~~text
P0  Product Contract                         CONTRACT LOCKED
 ↓
P1  Profile Domain Model Contract
 ↓
P2  Repository Eligibility Contract
 ↓
P3  GitHub Profile Data Collection
 ↓
P4  RepositoryAnalysis Orchestration
 ↓
P5  Cross-Repository Evidence Engine
 ↓
P6  Technical / Practice / Problem Patterns
 ↓
P7  Representative Repositories + Role Alignment
 ↓
P8  Profile Composer + Report Contract
 ↓
P9  Validation Profile Set
 ↓
P10 Web Profile Flow
 ↓
P11 Recruiter Pilot
 ↓
P12 Optional LLM Narration
~~~

## Implementation Gate

Profile production implementation başlamadan önce en az:

~~~text
P0 — Product Contract
P1 — Profile Domain Model Contract
P2 — Repository Eligibility Contract
~~~

kilitlenmiş olmalıdır.

Bu sıra, yanlış profile semantics üzerine kod yazılmasını engellemek için
zorunludur.

## Profile Evidence V1 Definition of Done

Public Profile Evidence V1 core capability tamamlanmış sayılmak için en az:

- public GitHub profile identity alınabilmeli,
- bounded public repository seti toplanabilmeli,
- eligibility deterministik uygulanabilmeli,
- mevcut RepositoryAnalysis pipeline'ı reuse edilmeli,
- cross-repository evidence deterministic olmalı,
- technical/practice/problem patterns evidence-backed olmalı,
- representative repositories açıklanabilir seçilmeli,
- role alignment bounded ve traceable olmalı,
- developer-level toplam skor üretilmemeli,
- seniority/employability/hire-no-hire üretilmemeli,
- sparse public GitHub negatif developer judgment üretmemeli,
- her meaningful claim repository evidence'a geri izlenebilmeli,
- validation profile seti PASS olmalı,
- developer ve recruiter views aynı structured truth'u tüketmelidir.

## Existing Post-MVP Phase Compatibility Note

Mevcut:

~~~text
Faz 10 — Recruiter View
~~~

repository-scoped roadmap fikri olarak korunur.

Public Profile Evidence recruiter experience ise yalnız Profile P0–P10
evidence foundation tamamlandıktan sonra Profile P11 kapsamında ele alınır.

Mevcut:

~~~text
Faz 12 — LLM Destekli Yorumlama
~~~

ile Profile P12 aynı temel boundary'i paylaşır:

> LLM deterministic evidence truth'un üstünde açıklama katmanı olabilir;
> canonical truth veya scoring engine olamaz.
