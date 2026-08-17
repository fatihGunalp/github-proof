# GitHub Proof — Puanlama ve Rapor Sözleşmesi

Bu doküman, GitHub Proof MVP'sinde sinyallerin nasıl puanlanacağını, kategori sonuçlarının nasıl üretileceğini ve raporun hangi kurallarla sunulacağını tanımlar.

## 1. Temel İlke

GitHub Proof puanı, geliştiricinin seviyesini temsil etmez.

Puan yalnızca analiz edilen repository içerisinde gözlemlenebilen mühendislik kanıtlarının görünürlüğünü özetler.

Bu nedenle:

- Puan geliştiricileri sıralamak için kullanılmamalıdır.
- Puan işe alım kararı vermek için kullanılmamalıdır.
- Puan repository kalitesinin tamamını temsil etmez.
- Puan yalnızca değerlendirilen sinyallerin sonucudur.
- Her puan, dayandığı kanıtlarla birlikte gösterilmelidir.

Puan tanımlı repository kanıt yüzeyini tarif eder; repository'nin arkasındaki insanı tarif etmez.

Bu nedenle puan şu şekillerde sunulmamalıdır:

- geliştirici kalitesi,
- mühendislik yeteneği,
- yetkinlik,
- kıdem,
- kod kalitesi,
- işe uygunluk.

Repository'nin AI destekli üretilmiş olması bu sınırı değiştirmez; puan hiçbir zaman insan üretimi veya insan anlayışı iddiasında bulunmamıştır.

---

## 2. Puanlama Modeli

MVP'de her sinyal aşağıdaki alanları üretir:

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

Bu yapı locale-independent analysis truth'tür. Signal title, description, explanation, recommendation ve limitation metinleri `SignalResult` içinde tutulmaz; Application presentation boundary bunları `SignalKey + Status + Evidence` üzerinden deterministik olarak localize eder.

### Score türü

`Score`, nullable tam sayıdır.

Aşağıdaki status değerlerinde sayısal puan zorunludur:

~~~text
Strong
Present
Partial
Missing
~~~

`Missing` için `Score: 0` olmalıdır.

Aşağıdaki status değerlerinde:

~~~text
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

`Score: null` üretilir.

### Status açıklamaları

#### Strong

Sinyal güçlü, açık ve anlamlı kanıtlarla desteklenmektedir.

#### Present

Sinyal için yeterli bir kanıt bulunmaktadır.

#### Partial

Sinyal mevcuttur ancak eksik, sınırlı veya sınıra yakın kanıt taşımaktadır.

#### Missing

Gerekli veri başarıyla incelenmiştir ancak sinyal için görünür kanıt bulunamamıştır.

`Score: 0` olur ve sinyal maksimum puanı denominator'a dahil edilir.

#### NotApplicable

Sinyal repository bağlamında uygulanabilir değildir.

`Score: null` olur ve numerator ile denominator hesabından çıkarılır.

#### UnavailableEvidence

Sinyalin değerlendirilmesi için gereken veri teknik veya erişim kaynaklı bir nedenle alınamamıştır.

`Score: null` olur.

Bu durum görünür kanıtın eksik olduğu anlamına gelmez.

#### RequiresHumanReview

Otomatik analiz deterministik ve güvenilir bir sonuç üretememiştir.

`Score: null` olur ve çözülene kadar numerator ile denominator hesabından çıkarılır.

---

## 3. Puan Aralıkları

Her sinyal kendi maksimum puanına sahiptir.

Örnek:

~~~text
README varlığı: 0–5
Kurulum talimatı: 0–8
Test yapısı: 0–10
CI workflow: 0–8
Release kullanımı: 0–6
~~~

Bir sinyalin puanı yalnızca önceden tanımlanmış kurallarla hesaplanmalıdır.

MVP'de serbest metin yorumu veya LLM çıktısı doğrudan puan üretmemelidir.

---

## 4. Puanlanamayan Durumların Davranışı

Aşağıdaki üç status sayısal puan üretmez:

~~~text
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

Bu sonuçlarda `Score: null` olur.

İlgili sinyal numerator ve denominator hesabına katılmaz.

`Missing` bu gruba dahil değildir.

Gerekli veri başarıyla incelenmiş ve kanıt bulunamamışsa:

~~~text
Status: Missing
Score: 0
~~~

üretilir.

Bu sinyal maksimum puanı denominator'a dahil edilir.

Bu ayrım, eksik mühendislik kanıtı ile eksik veya kararsız veri durumunun birbirine karıştırılmasını engeller.

---

## 5. Kategori Skoru

Her kategori skoru, kategoriye ait puanlanabilir sinyal sonuçlarından türetilir.

Puanlanabilir status değerleri:

~~~text
Strong
Present
Partial
Missing
~~~

Kategori hesabı:

~~~text
Kategori skoru =
Puanlanabilir kategori sinyallerinden kazanılan puan
/
Aynı sinyallerin maksimum puanı
× 100
~~~

`NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` kategori numerator ve denominator hesabından çıkarılır.

`IncludeInOverallScore` kategori skorunu etkilemez.

Bu nedenle `portfolio.presentation-completeness` Portföy Sunumu kategori skoruna dahil edilir ve genel toplam görünür kanıt skoruna dahil edilmez.

Kategori normalize skoru en yakın tam sayıya yuvarlanır.

Tam `.5` durumunda yukarı yuvarlanır.

Kategori için uygulanabilir sinyal kalmadıysa `CategoryStatus: NotEvaluated` kullanılır.

Uygulanabilir sinyaller bulunmasına rağmen hiç puanlanabilir sonuç üretilemediyse `CategoryStatus: InsufficientData` kullanılır.

Puanlanabilir sonuçlarla birlikte `UnavailableEvidence` veya `RequiresHumanReview` bulunuyorsa `CategoryStatus: Partial` kullanılır.

---

## 6. Toplam Görünür Kanıt Skoru

MVP için kesin hesaplama yöntemi:

~~~text
RawApplicableScoreNormalization
~~~

Genel toplam yalnızca:

- `IncludeInOverallScore: true`
- ve status değeri `Strong`, `Present`, `Partial` veya `Missing`

olan sinyalleri kullanır.

Hesap:

~~~text
Toplam skor =
Genel skora dahil puanlanabilir sinyallerden kazanılan puan
/
Aynı sinyallerin maksimum puanı
× 100
~~~

`NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` numerator ile denominator hesabından çıkarılır.

`IncludeInOverallScore: false` olan bileşik sinyaller genel toplamdan çıkarılır ancak raporda ve kendi kategori skorunda gösterilebilir.

Normalize toplam skor en yakın tam sayıya yuvarlanır.

Tam `.5` durumunda yukarı yuvarlanır.

Uygulanabilir maksimum puan `0` ise sahte toplam skor üretilmez.

Kategori ağırlıklı puanlama MVP v1 kapsamında kullanılmaz.

Böyle bir değişiklik yalnız doğrulama sonrasında yeni bir `RuleSetVersion` ile yapılabilir.

---

## 7. Skor Etiketleri

Toplam skor, yargılayıcı seviye isimleriyle sunulmamalıdır.

### Kullanılmaması gereken etiketler

- Kötü
- Başarısız
- Acemi
- Junior
- Senior
- Profesyonel
- İşe alınabilir
- İşe alınamaz

### MVP etiketleri

~~~text
0–24   : score.label.limited-visible-evidence
25–49  : score.label.basic-visible-evidence
50–69  : score.label.developing-visible-evidence
70–84  : score.label.strong-visible-evidence
85–100 : score.label.very-strong-visible-evidence
~~~

Bu değerler Domain'de `ScoreBreakdown.LabelKey` olarak taşınan semantic key'lerdir. EN/TR kullanıcı metni presentation catalog tarafından üretilir. Localized etiketler geliştiriciyi değil, repository üzerinde görülebilen kanıt yoğunluğunu ifade eder.

| LabelKey | EN | TR |
|---|---|---|
| `score.label.limited-visible-evidence` | Limited visible evidence | Sınırlı görünür kanıt |
| `score.label.basic-visible-evidence` | Basic visible evidence | Temel görünür kanıt |
| `score.label.developing-visible-evidence` | Developing visible evidence | Gelişen görünür kanıt |
| `score.label.strong-visible-evidence` | Strong visible evidence | Güçlü görünür kanıt |
| `score.label.very-strong-visible-evidence` | Very strong visible evidence | Çok güçlü görünür kanıt |

---

## 8. Sinyal Kanıtı

Her sinyal sonucu en az bir kanıt açıklaması taşımalıdır.

### Olumlu kanıt örneği

~~~text
README dosyası bulundu.
README içerisinde "Kurulum" başlığı ve üç çalıştırma adımı tespit edildi.
~~~

### Eksik kanıt örneği

~~~text
README ve `docs/**/*.md` corpus'u başarıyla incelendi; desteklenen kurulum, setup veya installation başlığı bulunamadı.
~~~

### Belirsiz kanıt örneği

~~~text
Test klasörü bulundu ancak test framework'ü güvenilir biçimde belirlenemedi.
İnsan değerlendirmesi önerilir.
~~~

Kanıt alanı, sistemin kararını tekrar üretmeye yardımcı olacak kadar açık olmalıdır.

---

## 9. Açıklama Alanı

Her sinyal, kanıtın neden önemli olduğunu kısa ve anlaşılır biçimde açıklamalıdır.

Örnek:

~~~text
Kurulum talimatı, repository'yi inceleyen başka bir geliştiricinin projeyi yeniden çalıştırabilmesini kolaylaştırır.
~~~

Açıklama:

- Yargılayıcı olmamalıdır.
- Geliştirici hakkında varsayım yapmamalıdır.
- Sinyalin sınırlarını aşmamalıdır.
- Teknik olmayan kullanıcı tarafından anlaşılabilir olmalıdır.

---

## 10. Öneri Alanı

Her `Partial`, `Missing` veya `RequiresHumanReview` sonucu uygulanabilir bir öneri üretmelidir.

### İyi öneri

~~~text
README içerisine gerekli bağımlılıkları, yapılandırma adımlarını ve çalıştırma komutlarını içeren bir "Kurulum" bölümü ekleyin.
~~~

### Zayıf öneri

~~~text
README'yi geliştirin.
~~~

Öneriler:

- Somut olmalıdır.
- Repository üzerinde uygulanabilir olmalıdır.
- Tek bir eylem veya açık bir küçük adım içermelidir.
- Kullanıcıyı suçlamamalıdır.

---

## 11. Güçlü Sinyallerin Seçimi

Raporda en fazla üç güçlü sinyal gösterilir.

Deterministic seçim sırası:

1. Yalnız `Strong` sonuçlar candidate olur.
2. Üçten az `Strong` varsa kalan yerler `Present` sonuçlarla doldurulur.
3. Her status tier `Score / MaximumScore` azalan, `MaximumScore` azalan ve canonical signal order artan sırayla değerlendirilir.
4. Aynı tier içinde önce farklı kategorilerden en yüksek sıradaki birer sonuç, sonra kalan sıralı sonuçlar seçilir.
5. `Partial` sonuç güçlü kanıt olarak sunulmaz.

Amaç, kullanıcıya yalnızca eksikleri değil, repository'nin hâlihazırda neyi iyi kanıtladığını da göstermektir.

---

## 12. Kritik Eksiklerin Seçimi

Raporda en fazla üç kritik eksik gösterilir.

Deterministic öncelik sırası:

1. `Missing` sonuçlar `MaximumScore` azalan ve canonical signal order artan sırayla değerlendirilir.
2. Üçten az `Missing` varsa kalan yerler `Partial` sonuçlarla doldurulur.
3. `Partial` sonuçlar `MaximumScore - Score` azalan, `MaximumScore` azalan ve canonical signal order artan sırayla değerlendirilir.
4. Aynı tier içinde önce farklı kategorilerden en yüksek sıradaki birer sonuç, sonra kalan sıralı sonuçlar seçilir.
5. `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` kritik eksik gibi sunulmaz; explicit uncertainty olarak gösterilir.

Aynı eksikliğin farklı biçimlerde tekrar edilmesinden kaçınılmalıdır.

---

## 13. Geliştirme Planı

Rapor, kullanıcıya üç öncelikli geliştirme adımı sunar.

Her adım şu alanları taşımalıdır:

~~~text
ImprovementAction
├── Priority
├── PresentationKey
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

`PresentationKey`, action identity'sidir; title, reason ve expected-impact copy'si locale catalog'undan üretilir.

### Localized örnek

~~~text
Öncelik: 1
Başlık: Kurulum adımlarını belgeleyin

Neden:
Repository başka bir geliştirici tarafından tekrar çalıştırılamıyor.

Beklenen etki:
Repository anlaşılabilirliği ve kullanılabilirliği artar.

İlgili sinyaller:
documentation.installation-guide

Effort:
Low
~~~

Her geliştirme adımı en az bir sinyal sonucuna dayanmalıdır.

MVP composer yalnız seçilmiş `Missing` veya `Partial` critical gap'lerden action üretir. `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` action veya failure değildir; explicit uncertainty olarak sunulur. Türetilmiş `portfolio.presentation-completeness` için ayrı action üretilemez çünkü upstream gap action'larıyla aynı işi tekrarlar.

Eylemler repository üzerinde uygulanabilir olmalı ve geliştirici hakkında kişisel yargı içermemelidir.

---

## 14. Rapor Yapısı

MVP web ve Markdown raporu evidence-first bilgi mimarisini izlemelidir:

1. Repository kimliği
2. Repository'nin neyi kanıtladığı — en fazla üç güçlü kanıt
3. Kanıtı neyin zayıflattığı — en fazla üç kritik boşluk
4. Önce yapılacaklar — en fazla üç geliştirme adımı
5. Explicit uncertainty — varsa `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview`
6. Dört kategori sonucu ve ikincil skor özeti
7. Tüm sinyal sonuçları
8. Sınırlamalar ve uyarılar
9. Analiz zamanı, sürümler ve yöntem
10. Product boundary disclaimer

Toplam ve kategori skorları korunur ancak ilk okuma katmanının ana mesajı olmaz.

Ayrıntılı bölüm sözleşmesi, deterministic seçim kuralları ve Phase 4 implementation sınırı için:

~~~text
docs/product/report-information-architecture.md
~~~

Rapor semantics locale bağımsızdır. Locale yalnız presentation katmanında çözülür; aynı `RepositoryAnalysis` TR veya EN render edilirken signal sonucu, score, evidence, selection veya ranking değişmez.

---

## 15. Repository Kimliği

Rapor şu repository bilgilerini içermelidir:

- Owner
- Repository adı
- Repository URL'si
- Default branch
- Analiz edilen commit SHA veya snapshot zamanı
- Public veya archived durumu
- Ana dil bilgisi mevcutsa ana dil

Bu bilgiler raporun hangi veri üzerinde üretildiğini açıklamak için gereklidir.

---

## 16. Analiz Sürümü

Her rapor şu sürüm bilgilerini taşımalıdır:

~~~text
AnalyzerVersion
RuleSetVersion
ReportSchemaVersion
~~~

Örnek:

~~~text
AnalyzerVersion: 0.1.0
RuleSetVersion: 2026.08.01
ReportSchemaVersion: 1
~~~

Bu alanlar sonuçların gelecekte tekrar üretilebilmesini ve sürümler arasında karşılaştırılmasını destekler.

`AnalyzerVersion` collection ve snapshot normalization semantiğini sahiplenir. Bump kuralı canonical olarak `docs/architecture/repository-dependency-evidence.md` §9 içinde tanımlıdır ve yalnız dependency evidence için değil, her collection/normalization değişikliği için geçerlidir.

---

## 17. Rapor Uyarısı

Her raporda aşağıdaki anlamı taşıyan bir uyarı bulunmalıdır:

> Bu rapor yalnızca analiz edilen public GitHub repository'sinde gözlemlenebilen sinyalleri değerlendirir. Geliştiricinin genel yetkinliğini, kıdemini, işe alınabilirliğini, kodun sahipliğini veya özgünlüğünü, AI kullanımını ya da projenin production güvenilirliğini belirlemez. Repository üzerinde gözlemlenen kanıt, kodun repository sahibi tarafından yazıldığını veya anlaşıldığını kurmaz.

---

## 18. Boş veya Yetersiz Veri

Repository yeterli veri taşımıyorsa sistem sahte kesinlik üretmemelidir.

Örnek durumlar:

- Repository boş
- Yalnızca tek dosya var
- GitHub API rate limit nedeniyle veri eksik
- Commit geçmişi alınamadı
- Repository çok yeni
- İçerik analiz sınırını aşıyor

Bu durumlarda:

~~~text
AnalysisStatus: Partial
~~~

veya:

~~~text
AnalysisStatus: InsufficientData
~~~

kullanılmalıdır.

Eksik veri, otomatik olarak sıfır puan olarak değerlendirilmemelidir.

---

## 19. Hata ve Belirsizlik Ayrımı

Aşağıdaki durumlar ayrılmalıdır.

### Missing Evidence

Veri başarıyla incelendi ancak sinyal bulunamadı.

### Unavailable Evidence

GitHub API veya erişim sınırı nedeniyle veri alınamadı.

### Not Applicable

Sinyal repository için anlamlı değil.

### Requires Human Review

Otomatik analiz güvenilir karar üretemedi.

`Missing`, tanımlı sinyal kurallarına göre sıfır puan alabilir. `UnavailableEvidence`, `NotApplicable` ve `RequiresHumanReview` ise otomatik olarak sıfır puanlanmamalıdır.

---

## 20. MVP Çıkış Kriteri

Puanlama ve rapor katmanı tamamlanmış sayılır:

- Her sinyal deterministik sonuç üretiyorsa
- Her puan bir kanıta dayanıyorsa
- `NotApplicable` doğru normalize ediliyorsa
- Eksik veri ile eksik kanıt ayrılıyorsa
- Dört kategori skoru üretilebiliyorsa
- Toplam skor açıklanabiliyorsa
- Üç güçlü sinyal seçilebiliyorsa
- Üç kritik eksik seçilebiliyorsa
- Üç öncelikli geliştirme adımı üretilebiliyorsa
- Web ve Markdown raporu aynı analiz sonucunu temsil ediyorsa
