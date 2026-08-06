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

Repository'yi ilk kez açan bir kişinin projenin amacını, kullanımını ve temel yapısını anlayıp anlayamadığını değerlendirmek.

### MVP sinyalleri

#### 2.1 README Varlığı

**Kanıt**

Repository kökünde GitHub tarafından README olarak tanınan bir dosyanın bulunması.

**Olumlu yorum**

Repository için temel bir açıklama alanı mevcuttur.

**Sınır**

README'nin bulunması, içeriğinin yeterli veya doğru olduğunu tek başına kanıtlamaz.

---

#### 2.2 Proje Açıklaması

**Kanıt**

GitHub repository metadata alanında açıklama bulunması.

**Olumlu yorum**

Repository, listeleme ve profil görünümünde temel amacını ifade etmektedir.

**Sınır**

Kısa bir açıklama, projenin kapsamının tamamen anlaşılır olduğunu kanıtlamaz.

---

#### 2.3 Kurulum Talimatı

**Kanıt**

README içerisinde kurulum, setup, installation veya eşdeğer bir bölümün bulunması.

**Olumlu yorum**

Başka bir geliştiricinin projeyi çalıştırmasına yardımcı olacak başlangıç bilgisi mevcuttur.

**Sınır**

Bölüm başlığının bulunması, talimatların güncel veya çalışır olduğunu garanti etmez.

---

#### 2.4 Kullanım Örneği

**Kanıt**

README veya docs içerisinde komut, ekran görüntüsü, örnek istek, örnek çıktı veya kullanım senaryosu bulunması.

**Olumlu yorum**

Projenin nasıl kullanılacağına dair görünür kanıt bulunmaktadır.

**Sınır**

Bir kullanım örneği, bütün kullanım senaryolarının belgelendiğini kanıtlamaz.

---

#### 2.5 Lisans

**Kanıt**

GitHub tarafından tanınan bir lisans dosyasının bulunması.

**Olumlu yorum**

Repository'nin kullanım ve katkı koşulları hakkında hukuki bir çerçeve bulunmaktadır.

**Sınır**

Lisansın bulunması, projenin ticari veya operasyonel olarak kullanıma hazır olduğunu göstermez.

---

#### 2.6 Demo veya Görsel Kanıt

**Kanıt**

README içerisinde ekran görüntüsü, video, canlı demo veya çalışan örnek bağlantısı bulunması.

**Olumlu yorum**

Repository'nin sonucu veya kullanım biçimi görsel ya da etkileşimli olarak sunulmaktadır.

**Sınır**

Demo bağlantısının bulunması, bağlantının erişilebilir veya güncel olduğunu tek başına garanti etmez.

---

## 3. Geliştirme Süreci Görünürlüğü

### Amaç

Repository'nin yalnızca son kod hâlini değil, geliştirme sürecine ait görünür kanıtları taşıyıp taşımadığını değerlendirmek.

### MVP sinyalleri

#### 3.1 Commit Geçmişi

**Kanıt**

Repository'de birden fazla commit bulunması.

**Olumlu yorum**

Kod tabanının gelişimine ait bir geçmiş mevcuttur.

**Sınır**

Commit sayısı tek başına commit kalitesini veya geliştiricinin katkı seviyesini kanıtlamaz.

---

#### 3.2 Commitlerin Zamana Yayılımı

**Kanıt**

Commitlerin farklı tarih ve zaman aralıklarında yapılmış olması.

**Olumlu yorum**

Repository, tek seferlik yükleme yerine zamana yayılan bir gelişim süreci gösterebilir.

**Sınır**

Zamana yayılmış commitler düzenli, planlı veya kaliteli çalışma anlamına gelmez.

---

#### 3.3 Büyük Başlangıç Commit'i

**Kanıt**

Repository içeriğinin önemli bölümünün ilk veya tek commit içerisinde eklenmiş olması.

**Dikkat sinyali**

Geliştirme sürecinin repository üzerinde görünürlüğü sınırlı olabilir.

**Sınır**

Bu sinyal AI kullanımı, kopyalama veya düşük mühendislik kalitesi kanıtı değildir. Mevcut bir projenin GitHub'a sonradan taşınması gibi geçerli nedenler olabilir.

---

#### 3.4 Issue Kullanımı

**Kanıt**

Repository'de en az bir issue bulunması.

**Olumlu yorum**

İş, hata veya geliştirme ihtiyaçlarının görünür biçimde takip edildiğine dair kanıt bulunabilir.

**Sınır**

Issue bulunması, düzenli proje yönetimi yapıldığını tek başına kanıtlamaz.

---

#### 3.5 Pull Request Kullanımı

**Kanıt**

Repository'de en az bir pull request bulunması.

**Olumlu yorum**

Değişikliklerin ayrı bir inceleme veya birleştirme akışından geçtiğine dair kanıt bulunabilir.

**Sınır**

Pull request kullanımı, gerçek ekip çalışmasını veya kaliteli code review yapıldığını garanti etmez.

---

#### 3.6 Release Kullanımı

**Kanıt**

Repository'de en az bir GitHub Release bulunması.

**Olumlu yorum**

Projenin belirli sürümler veya teslim noktaları üzerinden sunulduğuna dair kanıt vardır.

**Sınır**

Release bulunması semantic versioning, üretim kararlılığı veya aktif bakım anlamına gelmez.

---

## 4. Doğrulama ve Kullanılabilirlik

### Amaç

Başka bir geliştiricinin projeyi kurabilmesi, çalıştırabilmesi ve temel davranışlarını doğrulayabilmesi için görünür kanıtların bulunup bulunmadığını değerlendirmek.

### MVP sinyalleri

#### 4.1 Dependency Tanımı

**Kanıt**

Proje teknolojisine uygun dependency veya proje tanım dosyalarının bulunması.

Örnekler:

- `.csproj`
- `package.json`
- `requirements.txt`
- `pom.xml`
- `go.mod`

**Olumlu yorum**

Projenin bağımlılıklarının makine tarafından okunabilir biçimde tanımlandığına dair kanıt vardır.

**Sınır**

Dependency dosyasının bulunması, bağımlılıkların güncel veya güvenli olduğunu göstermez.

---

#### 4.2 Build veya Çalıştırma Tanımı

**Kanıt**

README, script veya proje dosyaları içerisinde build ve çalıştırma sürecinin tanımlanması.

**Olumlu yorum**

Projenin nasıl derleneceği veya başlatılacağına dair görünür bir yol bulunmaktadır.

**Sınır**

Komutların bulunması, farklı ortamlarda başarıyla çalışacağını garanti etmez.

---

#### 4.3 Test Yapısı

**Kanıt**

Test klasörü, test projesi veya yaygın test dosyası desenlerinin bulunması.

**Olumlu yorum**

Kod davranışlarının otomatik olarak doğrulanmasına yönelik yapı bulunmaktadır.

**Sınır**

Test dosyalarının bulunması, testlerin yeterli, güncel veya başarılı olduğunu kanıtlamaz.

---

#### 4.4 Continuous Integration

**Kanıt**

`.github/workflows` altında en az bir workflow dosyasının bulunması.

**Olumlu yorum**

Build, test veya başka doğrulamaların otomatikleştirilmesine yönelik altyapı bulunmaktadır.

**Sınır**

Workflow dosyasının bulunması, workflow'un aktif, başarılı veya güvenli olduğunu garanti etmez.

---

#### 4.5 Ortam Değişkeni Örneği

**Kanıt**

`.env.example`, örnek configuration dosyası veya README içerisinde gerekli ortam değişkenlerinin açıklanması.

**Olumlu yorum**

Projeyi çalıştırmak için gereken dış yapılandırmalar görünür hâle getirilmiştir.

**Sınır**

Örnek configuration dosyasının bulunması, gerçek değerlerin güvenli yönetildiğini kanıtlamaz.

---

## 5. Portföy Sunumu

### Amaç

Repository'nin, yapılan üretimi dışarıdan inceleyen bir kişiye açık, düzenli ve anlamlı biçimde sunup sunmadığını değerlendirmek.

### MVP sinyalleri

#### 5.1 Repository Adı

**Kanıt**

Repository adının varsayılan, geçici veya anlamsız bir ad yerine projeyi ifade eden bir yapıda olması.

**Olumlu yorum**

Repository adı projenin kimliğini anlamaya yardımcı olmaktadır.

**Sınır**

İyi bir ad, projenin teknik veya ürün kalitesini kanıtlamaz.

---

#### 5.2 GitHub Topics

**Kanıt**

Repository metadata içerisinde en az bir topic bulunması.

**Olumlu yorum**

Projenin teknoloji veya problem alanı GitHub üzerinde sınıflandırılmıştır.

**Sınır**

Topic kullanımı, projenin belirtilen teknolojileri doğru veya etkin kullandığını göstermez.

---

#### 5.3 Homepage veya Demo Bağlantısı

**Kanıt**

Repository metadata veya README içerisinde canlı uygulama, dokümantasyon veya demo bağlantısı bulunması.

**Olumlu yorum**

Repository dışındaki çalışan ürüne veya ayrıntılı kaynağa ulaşılabilir.

**Sınır**

Bağlantının bulunması, erişilebilirliğini veya kalitesini garanti etmez.

---

#### 5.4 Arşiv Durumu

**Kanıt**

Repository'nin GitHub üzerinde archived olarak işaretlenip işaretlenmediği.

**Olumlu yorum**

Aktif veya arşivlenmiş olma durumu dışarıdan görülebilmektedir.

**Sınır**

Arşivlenmiş repository başarısız veya değersiz değildir. Tamamlanmış ya da artık bakım gerektirmeyen bir proje olabilir.

---

#### 5.5 Son Güncelleme Tarihi

**Kanıt**

Repository'nin son push veya güncelleme zamanı.

**Olumlu yorum**

Repository'nin yakın zamanda aktif olup olmadığı hakkında sınırlı bir bakım sinyali sunar.

**Sınır**

Eski tarih düşük kaliteyi, yakın tarih ise aktif ve sürdürülebilir bakımı tek başına kanıtlamaz.

---

## 6. MVP Sinyal Listesi

MVP çekirdeği en az aşağıdaki 12 açıklanabilir sinyali içermelidir:

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

İlave aday sinyaller:

13. Dependency tanımı
14. Build veya çalıştırma tanımı
15. Ortam değişkeni örneği
16. GitHub Topics
17. Homepage bağlantısı
18. Arşiv durumu

---

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
