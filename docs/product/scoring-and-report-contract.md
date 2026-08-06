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

---

## 2. Puanlama Modeli

MVP'de her sinyal aşağıdaki alanları üretir:

~~~text
SignalResult
├── Key
├── Category
├── Status
├── Score
├── MaximumScore
├── Evidence
├── Explanation
└── Recommendation
~~~

### Status değerleri

~~~text
Strong
Present
Partial
Missing
NotApplicable
RequiresHumanReview
~~~

### Status açıklamaları

#### Strong

Sinyal güçlü, açık ve anlamlı kanıtlarla desteklenmektedir.

#### Present

Sinyal için yeterli bir kanıt bulunmaktadır.

#### Partial

Sinyal mevcuttur ancak eksik, sınırlı veya belirsizdir.

#### Missing

Sinyal için görünür kanıt bulunamamıştır.

#### NotApplicable

Sinyal repository türü veya teknoloji bağlamı için anlamlı değildir.

#### RequiresHumanReview

Otomatik analiz güvenilir bir sonuç üretememiştir ve insan değerlendirmesi gerekmektedir.

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

## 4. NotApplicable Davranışı

`NotApplicable` durumundaki sinyaller toplam maksimum puandan çıkarılmalıdır.

Örnek:

~~~text
Toplam kazanılan puan: 42
Uygulanabilir maksimum puan: 60

Normalize skor:
42 / 60 = 70 / 100
~~~

Bu yaklaşım, farklı teknoloji ve repository türlerinin ilgisiz sinyaller nedeniyle cezalandırılmasını önler.

---

## 5. Kategori Skoru

Her kategori skoru, o kategoriye bağlı uygulanabilir sinyallerin normalize edilmiş sonucudur.

MVP kategorileri:

1. Repository anlaşılabilirliği
2. Geliştirme süreci görünürlüğü
3. Doğrulama ve kullanılabilirlik
4. Portföy sunumu

Kategori hesabı:

~~~text
Kategori skoru =
Kazanılan kategori puanı
/
Uygulanabilir maksimum kategori puanı
× 100
~~~

Kategori içerisinde uygulanabilir sinyal yoksa:

~~~text
Kategori sonucu: Değerlendirilemedi
~~~

olarak gösterilmelidir.

---

## 6. Toplam Görünür Kanıt Skoru

Toplam skor, dört kategori skorunun basit ortalaması olmak zorunda değildir.

MVP'de iki seçenekten biri ürün sözleşmesinde sabitlenmelidir.

### Seçenek A — Ham puan normalizasyonu

~~~text
Toplam skor =
Tüm uygulanabilir sinyallerden kazanılan puan
/
Tüm uygulanabilir sinyallerin maksimum puanı
× 100
~~~

### Seçenek B — Kategori ağırlıklı puan

~~~text
Repository anlaşılabilirliği: %25
Geliştirme süreci görünürlüğü: %25
Doğrulama ve kullanılabilirlik: %30
Portföy sunumu: %20
~~~

MVP için varsayılan tercih:

> Ham puan normalizasyonu

Bu yöntem, erken aşamada gereksiz ürün varsayımlarını azaltır.

Kategori ağırlıkları ancak gerçek kullanıcı doğrulaması sonrasında değiştirilebilir.

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
0–24   : Sınırlı görünür kanıt
25–49  : Temel görünür kanıt
50–69  : Gelişen görünür kanıt
70–84  : Güçlü görünür kanıt
85–100 : Çok güçlü görünür kanıt
~~~

Bu etiketler geliştiriciyi değil, repository üzerinde görülebilen kanıt yoğunluğunu ifade eder.

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
README içerisinde kurulum, setup veya installation başlığı bulunamadı.
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

Seçim sırası:

1. `Strong` durumundaki en yüksek puanlı sinyaller
2. Kullanıcı açısından yüksek açıklama değeri taşıyan sinyaller
3. Aynı kategoriden tekrar eden sonuçların azaltılması

Amaç, kullanıcıya yalnızca eksikleri değil, repository'nin hâlihazırda neyi iyi kanıtladığını da göstermektir.

---

## 12. Kritik Eksiklerin Seçimi

Raporda en fazla üç kritik eksik gösterilir.

Öncelik sırası:

1. Yüksek maksimum puanlı `Missing` sinyaller
2. Repository'nin çalıştırılabilirliğini engelleyen eksikler
3. Portföy anlaşılabilirliğini ciddi biçimde azaltan eksikler
4. İnsan değerlendirmesi gerektiren yüksek riskli belirsizlikler

Aynı eksikliğin farklı biçimlerde tekrar edilmesinden kaçınılmalıdır.

---

## 13. Geliştirme Planı

Rapor, kullanıcıya üç öncelikli geliştirme adımı sunar.

Her adım şu alanları taşımalıdır:

~~~text
ImprovementAction
├── Priority
├── Title
├── Reason
├── ExpectedImpact
└── RelatedSignals
~~~

### Örnek

~~~text
Öncelik: 1
Başlık: Kurulum adımlarını belgeleyin

Neden:
Repository başka bir geliştirici tarafından tekrar çalıştırılamıyor.

Beklenen etki:
Repository anlaşılabilirliği ve kullanılabilirliği artar.

İlgili sinyaller:
InstallationGuideSignal
BuildRunDefinitionSignal
EnvironmentExampleSignal
~~~

---

## 14. Rapor Yapısı

MVP web ve Markdown raporu aşağıdaki sırayı izlemelidir:

1. Repository kimliği
2. Analiz zamanı
3. Kural seti sürümü
4. Toplam görünür kanıt skoru
5. Skor etiketi
6. Dört kategori sonucu
7. En güçlü üç sinyal
8. En kritik üç eksik
9. Üç öncelikli geliştirme adımı
10. Tüm sinyal sonuçları
11. Sınırlamalar ve uyarılar

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

---

## 17. Rapor Uyarısı

Her raporda aşağıdaki anlamı taşıyan bir uyarı bulunmalıdır:

> Bu rapor yalnızca analiz edilen public GitHub repository'sinde gözlemlenebilen sinyalleri değerlendirir. Geliştiricinin genel yetkinliğini, kıdemini, işe alınabilirliğini veya projenin production güvenilirliğini belirlemez.

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

Bu durumların tamamına sıfır puan vermek yasaktır.

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
