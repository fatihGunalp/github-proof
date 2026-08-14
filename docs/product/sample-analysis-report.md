# GitHub Proof — Örnek MVP Analiz Raporu

Bu doküman, GitHub Proof MVP'sinin üreteceği web ve Markdown raporunun örnek yapısını tanımlar.

Buradaki repository, puanlar ve kanıtlar temsilîdir.

---

## Repository Kanıt Profili

### Repository

~~~text
Owner: sample-owner
Repository: sample-project
URL: https://github.com/sample-owner/sample-project
Default Branch: main
Analiz Edilen Commit: 8ab24f1
Snapshot Zamanı: 2026-08-06T20:30:00Z
Durum: Public
Archived: Hayır
Ana Dil: C#
~~~

### Analiz Sürümü

~~~text
AnalyzerVersion: 0.1.0
RuleSetVersion: 2026.08.01
ReportSchemaVersion: 1
AnalysisStatus: Completed
~~~

---

## Genel Sonuç

~~~text
Toplam Görünür Kanıt Skoru: 56 / 100
Etiket: Gelişen görünür kanıt
~~~

Bu skor yalnızca repository içerisinde gözlemlenebilen ve MVP kural seti tarafından değerlendirilen sinyalleri özetler.

Geliştiricinin genel yetkinliğini, kıdemini veya işe alınabilirliğini temsil etmez.

---

## Kategori Sonuçları

| Kategori | Skor | Kısa değerlendirme |
|---|---:|---|
| Repository anlaşılabilirliği | 59 / 100 | README ve açıklama mevcut, kurulum talimatı eksik |
| Geliştirme süreci görünürlüğü | 66 / 100 | Commit geçmişi güçlü, issue ve release kullanımı sınırlı |
| Doğrulama ve kullanılabilirlik | 44 / 100 | Test yapısı mevcut, CI kanıtı bulunamadı |
| Portföy sunumu | 57 / 100 | Topics ve temel sunum parçaları mevcut, canlı demo bağlantısı eksik |

---

## Güçlü Sinyaller

### 1. Commit geçmişi zamana yayılmış

**Durum:** Strong  
**Puan:** 8 / 8

**Kanıt**

Son 45 gün içerisinde 27 commit bulundu.

Commitler 11 farklı güne yayılmış.

İlk ve son commit arasında en az 14 günlük zaman aralığı bulunmaktadır.

**Neden önemli?**

Repository, tek seferlik dosya yüklemesinin ötesinde zamana yayılmış görünür bir geliştirme geçmişi sunmaktadır.

---

### 2. Commit geçmişi güçlü

**Durum:** Strong  
**Puan:** 7 / 7

**Kanıt**

Default branch üzerinde 27 commit bulundu.

**Neden önemli?**

Repository yalnızca nihai kod durumunu değil, değişiklik geçmişini de görünür hâle getirmektedir.

**Sınır**

Commit sayısı commit kalitesini, özgün üretimi veya geliştirici seviyesini kanıtlamaz.

---

### 3. Test yapısı bulunuyor

**Durum:** Present  
**Puan:** 8 / 10

**Kanıt**

`tests/SampleProject.UnitTests` altında bir test projesi ve 14 test dosyası bulundu.

**Neden önemli?**

Kod davranışlarını otomatik olarak doğrulamaya yönelik görünür bir yapı bulunmaktadır.

**Sınır**

Bu analiz testlerin kapsamını, doğruluğunu veya başarıyla çalıştığını garanti etmez.

---

## Kritik Eksikler

### 1. Kurulum talimatı eksik

**Durum:** Missing  
**Puan:** 0 / 8

**Kanıt**

README içerisinde `Kurulum`, `Setup` veya `Installation` bölümü bulunamadı.

**Neden önemli?**

Başka bir geliştirici, projeyi kendi ortamında nasıl çalıştıracağını doğrudan anlayamıyor.

**Öneri**

README içerisine bağımlılıkları, yapılandırma adımlarını ve çalıştırma komutlarını içeren bir `Kurulum` bölümü ekleyin.

---

### 2. Continuous Integration kanıtı bulunamadı

**Durum:** Missing  
**Puan:** 0 / 8

**Kanıt**

`.github/workflows` altında workflow dosyası bulunamadı.

**Neden önemli?**

Build ve test süreçlerinin değişiklikler sonrasında otomatik olarak doğrulandığı görünür değildir.

**Öneri**

En azından build ve test komutlarını çalıştıran temel bir GitHub Actions workflow'u ekleyin.

---

### 3. Release bulunamadı

**Durum:** Missing  
**Puan:** 0 / 6

**Kanıt**

Repository için yayınlanmış GitHub Release bulunamadı.

**Neden önemli?**

Projenin belirli teslim veya sürüm noktaları dışarıdan görünür değildir.

**Öneri**

Kararlı ilk sürüm için açıklayıcı release notlarıyla birlikte `v0.1.0` etiketi ve GitHub Release oluşturun.

---

## Öncelikli Geliştirme Planı

### Öncelik 1 — Kurulum sürecini belgeleyin

**Neden?**

Repository başka bir geliştirici tarafından kolayca tekrar çalıştırılamıyor.

**Beklenen etki**

Repository anlaşılabilirliği ve kullanılabilirliği artar.

**İlgili sinyaller**

- `documentation.installation-guide`

---

### Öncelik 2 — Test doğrulamasını CI ile otomatikleştirin

**Neden?**

Test yapısı bulunmasına rağmen değişikliklerin otomatik doğrulandığına dair görünür CI kanıtı yok.

**Beklenen etki**

Doğrulama süreci görünür ve tekrarlanabilir hâle gelir.

**İlgili sinyaller**

- `validation.test-structure`
- `validation.continuous-integration`

---

### Öncelik 3 — İlk anlamlı release'i yayınlayın

**Neden?**

Repository'nin kararlı teslim noktaları görünür değil.

**Beklenen etki**

Proje gelişimi ve kullanıcıya sunulan teslim noktaları daha anlaşılır olur.

**İlgili sinyaller**

- `process.release-usage`

---

## Tüm Sinyal Sonuçları

| Sinyal | Durum | Puan |
|---|---|---:|
| README varlığı | Present | 5 / 5 |
| Proje açıklaması | Present | 4 / 4 |
| Kurulum talimatı | Missing | 0 / 8 |
| Lisans | Present | 3 / 3 |
| Kullanım örneği veya demo | Partial | 4 / 7 |
| Commit geçmişi | Strong | 7 / 7 |
| Commitlerin zamana yayılımı | Strong | 8 / 8 |
| Issue kullanımı | Partial | 2 / 5 |
| Pull request kullanımı | Present | 4 / 6 |
| Release kullanımı | Missing | 0 / 6 |
| Test yapısı | Present | 8 / 10 |
| Continuous Integration | Missing | 0 / 8 |
| GitHub Topics | Present | 3 / 4 |
| Homepage veya canlı demo bağlantısı | Missing | 0 / 5 |
| Repository sunum bütünlüğü | Strong | 5 / 5 — toplam skora dahil değil |

---

## Analiz Sınırlamaları

- Yalnızca public repository verileri değerlendirilmiştir.
- Kodun doğruluğu veya güvenliği bütünüyle analiz edilmemiştir.
- Testlerin gerçekten başarılı çalıştığı doğrulanmamıştır.
- Commitlerin kim tarafından ve hangi araçla üretildiği hakkında hüküm verilmemiştir.
- Kodun AI tarafından yazılıp yazılmadığı değerlendirilmemiştir.
- Repository dışındaki iş deneyimi ve yetkinlikler rapora dahil edilmemiştir.
- Eksik görünen bir kanıt, repository dışında mevcut olabilir.

---

## Rapor Uyarısı

> Bu rapor yalnızca analiz edilen public GitHub repository'sinde gözlemlenebilen sinyalleri değerlendirir. Geliştiricinin genel yetkinliğini, kıdemini, işe alınabilirliğini veya projenin production güvenilirliğini belirlemez.

---

## Markdown Çıktı İlkesi

Markdown raporu ile web raporu aynı analiz sonucunu temsil etmelidir.

İki format arasında şu alanlarda fark bulunmamalıdır:

- Toplam skor
- Kategori skorları
- Sinyal durumları
- Kanıtlar
- Kritik eksikler
- Öncelikli geliştirme adımları
- Analiz sürümü
- Sınırlamalar

Görsel sunum farklı olabilir; analiz anlamı değişmemelidir.
