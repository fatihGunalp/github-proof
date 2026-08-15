# GitHub Proof — Analiz Kategorileri ve Sinyal Sınırları

Bu doküman, GitHub Proof MVP'sinde kullanılacak analiz kategorilerini, her kategorinin sorumluluğunu ve sinyallerin yorumlanma sınırlarını tanımlar.

## 1. Temel Yaklaşım

GitHub Proof yalnızca public bir GitHub repository'sinde dışarıdan gözlemlenebilen kanıtları analiz eder.

Bir sinyal:

- Gözlemlenebilir olmalıdır.
- GitHub API veya repository içeriği üzerinden doğrulanabilmelidir.
- Açıklanabilir bir değerlendirme üretmelidir.
- Kullanıcıya uygulanabilir bir geliştirme önerisi sunabilmelidir.
- Geliştiricinin kişiliği, zekâsı veya genel seviyesi hakkında hüküm vermemelidir.

Her sinyal yalnızca kendi sınırları içerisinde yorumlanmalıdır.

---

## 2. Repository Anlaşılabilirliği

### Amaç

Repository'yi ilk kez açan bir kişinin projenin amacını, kurulumunu ve kullanım biçimini anlayıp anlayamadığını değerlendirmek.

### MVP sinyalleri

#### 2.1 README Varlığı

**Kanıt**

GitHub tarafından README olarak tanınan bir dosyanın bulunması.

**Olumlu yorum**

Repository için temel bir açıklama alanı bulunmaktadır.

**Sınır**

README'nin bulunması, içeriğinin yeterli, doğru veya güncel olduğunu kanıtlamaz.

---

#### 2.2 Proje Açıklaması

**Kanıt**

Repository metadata içerisinde description alanının bulunması.

**Olumlu yorum**

Repository, GitHub listeleme ve profil görünümünde amacını kısa biçimde ifade etmektedir.

**Sınır**

Description bulunması, projenin kapsamının bütünüyle anlaşılır olduğunu kanıtlamaz.

---

#### 2.3 Kurulum Talimatı

**Kanıt**

README veya indekslenebilir dokümantasyon içerisinde uygulanabilir kurulum adımlarının bulunması.

**Olumlu yorum**

Başka bir geliştiricinin projeyi hazırlamasına yardımcı olacak görünür başlangıç bilgisi bulunmaktadır.

**Sınır**

Talimatların bulunması gerçekten çalıştıklarını veya güncel olduklarını garanti etmez.

---

#### 2.4 Lisans

**Kanıt**

GitHub tarafından tanınan veya repository kökünde bulunan lisans dosyası.

**Olumlu yorum**

Repository'nin kullanım ve katkı koşullarına ilişkin görünür bir çerçevesi bulunmaktadır.

**Sınır**

Lisans bulunması, hukuki uygunluğun bütünüyle doğrulandığı anlamına gelmez.

---

#### 2.5 Kullanım Örneği veya Demo

**Kanıt**

README veya dokümantasyon içerisinde kullanım komutu, örnek istek/cevap, kod örneği, ekran görüntüsü, video, canlı demo veya örnek çıktı bulunması.

**Olumlu yorum**

Projenin nasıl kullanılacağına veya sonucunun nasıl göründüğüne dair gözlemlenebilir kanıt bulunmaktadır.

**Sınır**

Örneğin veya bağlantının bulunması erişilebilirliğini, doğruluğunu veya güncelliğini garanti etmez.

---

## 3. Geliştirme Süreci Görünürlüğü

### Amaç

Repository'nin yalnızca son kod hâlini değil, geliştirme sürecine ait gözlemlenebilir kanıtları taşıyıp taşımadığını değerlendirmek.

### MVP sinyalleri

#### 3.1 Commit Geçmişi

**Kanıt**

Default branch üzerinde bulunan commitler.

**Olumlu yorum**

Repository içerisinde kod tabanının gelişimine ait görünür bir geçmiş bulunmaktadır.

**Sınır**

Commit sayısı commit kalitesini, özgün üretimi veya geliştirici seviyesini kanıtlamaz.

---

#### 3.2 Commitlerin Zamana Yayılımı

**Kanıt**

Commitlerin farklı aktif günlere veya zaman aralıklarına yayılması.

**Olumlu yorum**

Repository tek seferlik yüklemenin ötesinde zamana yayılmış bir gelişim geçmişi gösterebilir.

**Sınır**

Zamana yayılım planlı, düzenli veya kaliteli çalışma kanıtı değildir.

---

#### 3.3 Issue Kullanımı

**Kanıt**

Pull requestlerden ayrıştırılmış açık veya kapalı issue kayıtları.

**Olumlu yorum**

Hata, görev veya geliştirme ihtiyaçlarının repository üzerinde takip edildiğine dair görünür kanıt bulunabilir.

**Sınır**

Issue bulunması düzenli proje yönetimini veya kaliteli issue içeriğini kanıtlamaz.

---

#### 3.4 Pull Request Kullanımı

**Kanıt**

Açık, kapalı veya merged pull request kayıtları.

**Olumlu yorum**

Değişikliklerin ayrı bir birleştirme veya inceleme akışından geçtiğine dair görünür kanıt bulunabilir.

**Sınır**

Pull request bulunması gerçek code review veya ekip çalışması yapıldığını garanti etmez.

---

#### 3.5 Release Kullanımı

**Kanıt**

GitHub Releases, tags ve yayın durumları.

**Olumlu yorum**

Repository'nin belirli sürüm veya teslim noktaları görünür hâle getirilmiştir.

**Sınır**

Release bulunması production kararlılığı, semantic versioning veya aktif bakım kanıtı değildir.

---

## 4. Doğrulama ve Kullanılabilirlik

### Amaç

Kod davranışlarının otomatik olarak doğrulanmasına yönelik görünür yapı ve otomasyon bulunup bulunmadığını değerlendirmek.

### MVP sinyalleri

#### 4.1 Test Yapısı

**Kanıt**

Test projesi, test klasörü, test framework tanımı veya güvenilir test dosyası desenleri.

**Olumlu yorum**

Kod davranışlarını otomatik doğrulamaya yönelik görünür bir yapı bulunmaktadır.

**Sınır**

Test dosyalarının bulunması testlerin yeterli, doğru, güncel veya başarılı olduğunu kanıtlamaz.

---

#### 4.2 Continuous Integration

**Kanıt**

`.github/workflows` altında build veya test adımları içeren workflow dosyaları.

**Olumlu yorum**

Build, test veya başka doğrulamaların otomatikleştirilmesine yönelik görünür altyapı bulunmaktadır.

**Sınır**

Workflow dosyasının bulunması workflow'un aktif, başarılı veya güvenli olduğunu garanti etmez.

---

## 5. Portföy Sunumu

### Amaç

Repository'nin yapılan üretimi dışarıdan inceleyen bir kişiye açık ve anlamlı biçimde sunup sunmadığını değerlendirmek.

### MVP sinyalleri

#### 5.1 GitHub Topics

**Kanıt**

Repository metadata içerisinde bulunan topics listesi.

**Olumlu yorum**

Projenin teknoloji, ürün veya problem alanı GitHub üzerinde sınıflandırılmıştır.

**Sınır**

Topics bulunması belirtilen teknolojilerin doğru veya etkin kullanıldığını kanıtlamaz.

---

#### 5.2 Homepage veya Canlı Demo Bağlantısı

**Kanıt**

Meaningful repository homepage alanı veya README içerisinde açık bir demo/live-app bağlantısı.

**Olumlu yorum**

Repository'den çalışan ürüne veya ayrıntılı proje kaynağına görünür bir geçiş bulunmaktadır.

**Sınır**

Bağlantının bulunması erişilebilirliğini, güncelliğini veya ürün kalitesini garanti etmez.
Documentation-only destination ve generic README linki bu sinyal için tek başına kanıt değildir.

---

#### 5.3 Repository Sunum Bütünlüğü

**Kanıt**

Aşağıdaki beş sunum parçasının birlikte bulunma durumu:

1. Repository description
2. README
3. Kullanım örneği veya demo
4. Lisans
5. En az bir GitHub topic

**Olumlu yorum**

Repository'nin temel sunum parçalarının birlikte bulunma durumu deterministik biçimde özetlenebilir.

**Sınır**

Bu sinyal teknik kaliteyi, estetiği veya geliştirici seviyesini değerlendirmez.

Bu sinyal mevcut alt sinyallerden türetildiği için:

~~~text
IncludeInOverallScore: false
~~~

olarak değerlendirilir ve toplam görünür kanıt skoruna yeniden dahil edilmez.

---

## 6. Kesin MVP Sinyal Listesi

MVP çekirdeği aşağıdaki 15 açıklanabilir sinyali içerir:

1. README varlığı
2. Proje açıklaması
3. Kurulum talimatı
4. Lisans
5. Kullanım örneği veya demo
6. Commit geçmişi
7. Commitlerin zamana yayılımı
8. Issue kullanımı
9. Pull request kullanımı
10. Release kullanımı
11. Test yapısı
12. Continuous Integration
13. GitHub Topics
14. Homepage veya canlı demo bağlantısı
15. Repository sunum bütünlüğü

`Repository sunum bütünlüğü`, kategori içi açıklama ve özet amacı taşır ancak toplam görünür kanıt skoruna katkı yapmaz.

## 7. Yasaklı Çıkarımlar

Aşağıdaki sonuçlar hiçbir sinyal veya sinyal birleşiminden otomatik olarak üretilmemelidir:

- Bu geliştirici junior, mid veya senior seviyesindedir.
- Bu geliştirici işe alınmalıdır veya alınmamalıdır.
- Bu kod AI tarafından yazılmıştır.
- Bu repository kopyalanmıştır.
- Bu geliştirici mühendislik bilmiyor.
- Bu proje production-ready durumdadır.
- Bu repository güvenlidir.
- Bu mimari doğrudur veya yanlıştır.
- Bu geliştirici ekip çalışmasına uygundur.
- Bu geliştirici projeyi gerçekten kendisi geliştirmiştir.

---

## 8. Dil ve Ton İlkesi

Rapor dili yargılayıcı olmamalıdır.

### Kullanılmaması gereken ifadeler

- Başarısız
- Yetersiz geliştirici
- Kötü repository
- Sahte proje
- AI ile üretilmiş
- Profesyonel değil
- İşe alınamaz

### Tercih edilmesi gereken ifadeler

- Kanıt bulunamadı
- Repository üzerinde görünür değil
- Geliştirilebilir
- İnsan değerlendirmesi gerektirir
- Sınırlı kanıt
- Güçlü sinyal
- Dikkat sinyali
- Ek doğrulama önerilir

---

## 9. Sinyal Kabul Kriteri

Yeni bir sinyal MVP'ye yalnızca şu koşullarda dahil edilebilir:

1. GitHub API veya repository içeriğinden güvenilir biçimde üretilebiliyor.
2. Aynı veriyle tekrarlandığında aynı sonucu veriyor.
3. Bulduğu veya bulamadığı kanıt raporda gösterilebiliyor.
4. Kullanıcıya uygulanabilir bir öneri üretilebiliyor.
5. Yasaklı çıkarımlardan birine yol açmıyor.
6. En az üç farklı teknoloji ekosisteminde anlamlı kalabiliyor veya teknolojiye özel olduğu açıkça belirtiliyor.
