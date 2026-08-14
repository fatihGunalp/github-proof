# GitHub Proof — MVP Sinyal Tanımları

Bu doküman, GitHub Proof MVP kapsamında değerlendirilecek ilk 15 sinyalin kesin tanımlarını, veri gereksinimlerini, uygulanabilirlik sınırlarını ve puanlama kurallarını belirler.

Amaç, sinyallerin kodlanmadan önce ürün ve domain düzeyinde sabitlenmesidir.

---

## 1. Ortak Sinyal Sözleşmesi

Her sinyal aşağıdaki alanlarla tanımlanmalıdır:

~~~text
SignalDefinition
├── Key
├── Name
├── Category
├── Description
├── MaximumScore
├── RequiredData
├── ApplicabilityRules
├── EvaluationRules
├── StatusRules
├── EvidenceRules
├── ExplanationTemplate
├── RecommendationTemplate
├── Limitations
├── IncludeInOverallScore
└── Version
~~~

### Ortak durum değerleri

~~~text
Strong
Present
Partial
Missing
NotApplicable
UnavailableEvidence
RequiresHumanReview
~~~

### Ortak puan sözleşmesi

- Sinyal skorları yalnızca tam sayı olabilir.
- `Strong`, `Present`, `Partial` ve `Missing` puanlanabilir durumlardır.
- `Missing` yalnızca gerekli veri başarıyla alındığında kullanılabilir.
- `Missing` için `Score: 0` üretilir.
- `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` için `Score: null` üretilir.
- Bu üç puanlanamayan durum numerator ve denominator hesabına dahil edilmez.
- Her evaluator aşağıdaki deterministik kurallardan tek bir puan üretmelidir.
- Serbest puan aralığı seçimi yapılamaz.
- Aynı snapshot, aynı sinyal sürümü ve aynı kural seti aynı sonucu üretmelidir.
- Her sonuç gözlemlenebilir kanıta bağlanmalıdır.
- Her eksik veya kısmi sonuç uygulanabilir bir öneri taşımalıdır.
- `IncludeInOverallScore`, yalnızca genel toplam görünür kanıt skoruna katkıyı belirler.
- `IncludeInOverallScore: false` olan sinyaller kendi kategori skorunda kullanılabilir.
- Hiçbir sinyal geliştirici seviyesi, işe alınabilirlik veya AI kullanımı hakkında hüküm üretmemelidir.

### İlk sürüm kuralı

Bu belge kodlama öncesindeki ilk uygulanabilir sözleşmeyi kilitler.

Henüz yayımlanmış bir analyzer bulunmadığı için mevcut `Version: 1` tanımları MVP v1'in başlangıç sürümüdür.

Bundan sonraki semantik sinyal değişikliklerinde ilgili `Version` artırılmalıdır.

---

# Repository Anlaşılabilirliği

## 2. README Varlığı

~~~text
Key: repository.readme.exists
Name: README Varlığı
Category: RepositoryUnderstandability
MaximumScore: 5
Version: 1
~~~

### Amaç

Repository'nin temel açıklama alanına sahip olup olmadığını değerlendirmek.

### Gerekli veri

- Repository contents
- GitHub README endpoint sonucu
- İlgili veri erişilebilirlik durumu

### Uygulanabilirlik

Public ve boş olmayan bütün repository'lerde uygulanır.

Boş repository için `NotApplicable` yerine genel analiz durumu `InsufficientData` olabilir.

### Değerlendirme

- GitHub tarafından README olarak tanınan dosya varsa: `Present`, 5 puan
- README verisi başarıyla incelendi ancak dosya yoksa: `Missing`, 0 puan
- README verisi alınamadıysa: `UnavailableEvidence`
- Birden fazla README adayı arasında karar verilemiyorsa: `RequiresHumanReview`

### Kanıt örnekleri

~~~text
README.md repository kökünde bulundu.
~~~

~~~text
GitHub README endpoint'i 404 döndürdü ve repository kökünde README adayı bulunamadı.
~~~

### Öneri

Repository köküne projenin amacını, kurulumunu ve kullanımını açıklayan bir README dosyası ekleyin.

### Sınırlar

README bulunması, içeriğinin yeterli, doğru veya güncel olduğunu kanıtlamaz.

---

## 3. Proje Açıklaması

~~~text
Key: repository.description.present
Name: Proje Açıklaması
Category: RepositoryUnderstandability
MaximumScore: 4
Version: 1
~~~

### Amaç

Repository metadata içerisinde projenin amacını açıklayan kısa bir tanım bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- Repository metadata
- Description alanı
- Veri erişilebilirlik durumu

### Uygulanabilirlik

Bütün public repository'lerde uygulanır.

### Değerlendirme

- Description dolu ve yalnızca boşluklardan oluşmuyorsa: `Present`, 4 puan
- Description boş veya null ise: `Missing`, 0 puan
- Metadata alınamadıysa: `UnavailableEvidence`

### Kanıt örneği

~~~text
Repository description alanında 84 karakterlik açıklama bulundu.
~~~

### Öneri

Repository'nin çözdüğü problemi tek cümlede açıklayan kısa bir GitHub description ekleyin.

### Sınırlar

Description bulunması, projenin kapsamının veya değer önerisinin tamamen anlaşılır olduğunu kanıtlamaz.

---

## 4. Kurulum Talimatı

~~~text
Key: documentation.installation-guide
Name: Kurulum Talimatı
Category: RepositoryUnderstandability
MaximumScore: 8
Version: 1
~~~

### Amaç

Başka bir geliştiricinin projeyi kurmasına yardımcı olacak görünür talimatların bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- README içeriği
- Gerekirse docs dizinindeki indekslenebilir Markdown dosyaları
- İçerik erişilebilirlik durumu

### İlk sürüm başlık adayları

~~~text
kurulum
installation
install
setup
getting started
quick start
başlangıç
çalıştırma
running
run locally
~~~

### Değerlendirme

- Strong koşulları sağlanıyorsa: `Strong`, 8 puan
- Açık kurulum başlığı ve en az bir uygulanabilir adım varsa ancak Strong koşulları sağlanmıyorsa: `Present`, 8 puan
- Kurulumla ilgili metin var ancak uygulanabilir adım yoksa: `Partial`, 4 puan
- İçerik başarıyla incelendi ancak kurulum kanıtı yoksa: `Missing`, 0 puan
- README alınamadıysa: `UnavailableEvidence`, `Score: null`
- Otomatik eşleşme güveni düşükse: `RequiresHumanReview`, `Score: null`

### Strong koşulu

Aşağıdaki unsurlardan en az üçünü taşıyorsa `Strong`, 8 puan:

- Ön koşullar
- Dependency kurulumu
- Configuration adımları
- Build komutu
- Çalıştırma komutu
- Migration veya hazırlık adımı

### Öneri

README içerisine ön koşulları, bağımlılıkları, yapılandırmayı ve çalıştırma komutlarını içeren bir kurulum bölümü ekleyin.

### Sınırlar

Talimatların bulunması, gerçekten çalıştığını veya güncel olduğunu garanti etmez.

---

## 5. Lisans

~~~text
Key: repository.license.present
Name: Lisans
Category: RepositoryUnderstandability
MaximumScore: 3
Version: 1
~~~

### Amaç

Repository'nin kullanım ve katkı koşullarının görünür olup olmadığını değerlendirmek.

### Gerekli veri

- GitHub license bilgisi
- Repository root contents
- Veri erişilebilirlik durumu

### Değerlendirme

- GitHub tarafından tanınan lisans varsa: `Present`, 3 puan
- Lisans dosyası var ancak türü tanınamıyorsa: `Partial`, 1 puan
- Veri incelendi ancak lisans bulunamadıysa: `Missing`, 0 puan
- License verisi alınamadıysa: `UnavailableEvidence`

### Öneri

Projenin kullanım ve katkı koşullarını açıklayan uygun bir açık kaynak lisansı ekleyin.

### Sınırlar

Lisans bulunması, yasal uygunluğun bütünüyle doğrulandığı anlamına gelmez.

---

## 6. Kullanım Örneği veya Demo

~~~text
Key: documentation.usage-or-demo
Name: Kullanım Örneği veya Demo
Category: RepositoryUnderstandability
MaximumScore: 7
Version: 1
~~~

### Amaç

Repository sonucunun veya kullanım biçiminin görünür bir örnekle sunulup sunulmadığını değerlendirmek.

### Gerekli veri

- README
- Repository homepage
- Markdown bağlantıları
- Görsel referansları
- Kod blokları
- Veri erişilebilirlik durumu

### Kanıt türleri

- Kullanım komutu
- Örnek istek ve cevap
- Kod örneği
- Ekran görüntüsü
- Video bağlantısı
- Canlı demo
- Örnek çıktı

### Değerlendirme

- Hem kullanım örneği hem demo veya görsel varsa: `Strong`, 7 puan
- En az bir açık ve bağlama uygun kullanım örneği varsa: `Present`, 5 puan
- Yalnızca belirsiz ekran görüntüsü veya kırılgan bağlantı varsa: `Partial`, 4 puan
- İçerik incelendi ancak örnek bulunamadıysa: `Missing`, 0 puan
- İçerik alınamadıysa: `UnavailableEvidence`, `Score: null`

### Öneri

README içerisine projenin nasıl kullanılacağını gösteren kısa bir örnek, ekran görüntüsü veya demo bağlantısı ekleyin.

### Sınırlar

Bağlantıların erişilebilirliği MVP'de her zaman doğrulanamayabilir.

---

# Geliştirme Süreci Görünürlüğü

## 7. Commit Geçmişi

~~~text
Key: process.commit-history
Name: Commit Geçmişi
Category: DevelopmentProcessVisibility
MaximumScore: 7
Version: 1
~~~

### Amaç

Repository'nin tek bir nihai dosya yüklemesi dışında görünür geliştirme geçmişi taşıyıp taşımadığını değerlendirmek.

### Gerekli veri

- Default branch commit listesi
- Commit toplamı
- Sayfalama durumu
- Veri erişilebilirlik durumu

### Değerlendirme

- 10 veya daha fazla commit: `Strong`, 7 puan
- 7–9 commit: `Present`, 6 puan
- 5–6 commit: `Present`, 5 puan
- 3–4 commit: `Present`, 4 puan
- 2 commit: `Partial`, 2 puan
- 1 commit: `Partial`, 1 puan
- Commit verisi başarıyla incelendi ancak commit yoksa: `Missing`, 0 puan
- Commit geçmişi alınamadıysa: `UnavailableEvidence`, `Score: null`

### Öneri

Değişiklikleri amaçlarına göre küçük ve açıklanabilir commitler hâlinde kaydedin.

### Sınırlar

Commit sayısı commit kalitesini, geliştirici yetkinliğini veya özgün üretimi kanıtlamaz.

---

## 8. Commitlerin Zamana Yayılımı

~~~text
Key: process.commit-distribution
Name: Commitlerin Zamana Yayılımı
Category: DevelopmentProcessVisibility
MaximumScore: 8
Version: 1
~~~

### Amaç

Geliştirme faaliyetinin farklı günlere veya dönemlere yayılıp yayılmadığını değerlendirmek.

### Gerekli veri

- Commit tarihleri
- İlk ve son commit zamanı
- Farklı aktif gün sayısı
- Repository oluşturulma zamanı
- Veri erişilebilirlik durumu

### Değerlendirme

- Repository yaşı 14 günden azsa: `NotApplicable`, `Score: null`
- En az 8 aktif gün ve en az 14 günlük ilk-son commit aralığı varsa: `Strong`, 8 puan
- Strong koşulu sağlanmıyor ancak 6 veya daha fazla aktif gün varsa: `Present`, 7 puan
- 5 aktif gün varsa: `Present`, 6 puan
- 4 aktif gün varsa: `Present`, 5 puan
- 3 aktif gün varsa: `Partial`, 4 puan
- 2 aktif gün varsa: `Partial`, 2 puan
- Tek aktif gün varsa: `Partial`, 1 puan
- Commit verisi başarıyla incelendi ancak commit yoksa: `Missing`, 0 puan
- Commit tarihleri veya repository oluşturulma zamanı alınamadıysa: `UnavailableEvidence`, `Score: null`

### Bağlam kuralları

- Repository yaşı 14 günden azsa bu sinyal `NotApplicable` olur ve puanlamaya katılmaz.
- Tek amaçlı küçük araçlarda sınırlı dağılım normal olabilir; bu durum puan kuralını değiştirmez ancak rapor sınırlaması olarak gösterilebilir.
- Import edilmiş repository ihtimali rapor sınırlaması olarak gösterilmelidir.

### Öneri

Geliştirme sürecini yalnızca son hâli yüklemek yerine, anlamlı ilerleme adımlarını ayrı commitlerle görünür kılın.

### Sınırlar

Zamana yayılım planlı, düzenli veya kaliteli çalışma kanıtı değildir.

---

## 9. Issue Kullanımı

~~~text
Key: process.issue-usage
Name: Issue Kullanımı
Category: DevelopmentProcessVisibility
MaximumScore: 5
Version: 1
~~~

### Amaç

Hata, görev veya geliştirme ihtiyaçlarının repository üzerinde görünür biçimde takip edilip edilmediğini değerlendirmek.

### Gerekli veri

- Issue listesi
- Pull requestlerin issue listesinden ayrıştırılması
- Açık ve kapalı issue sayıları
- Veri erişilebilirlik durumu

### Değerlendirme

- Birden fazla anlamlı issue varsa: `Present`, 5 puan
- Tek anlamlı issue varsa: `Partial`, 2 puan
- Issue verisi başarıyla alındı ancak issue yoksa: `Missing`, 0 puan
- GitHub Issues özelliği kapalıysa: `NotApplicable`, `Score: null`
- API verisi alınamadıysa: `UnavailableEvidence`, `Score: null`
- Repository'nin küçük olması tek başına `NotApplicable` üretmez

### Uygulanabilirlik sınırı

Küçük, tek amaçlı veya tamamlanmış repository'lerde issue eksikliği otomatik olarak ciddi eksik sayılmamalıdır.

### Öneri

Hataları, görevleri veya geliştirme fikirlerini görünür ve izlenebilir issue kayıtlarıyla yönetin.

### Sınırlar

Issue bulunması düzenli proje yönetimi veya kaliteli içerik kanıtı değildir.

---

## 10. Pull Request Kullanımı

~~~text
Key: process.pull-request-usage
Name: Pull Request Kullanımı
Category: DevelopmentProcessVisibility
MaximumScore: 6
Version: 1
~~~

### Amaç

Değişikliklerin ayrı bir birleştirme veya inceleme akışından geçtiğine dair görünür kanıt bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- Açık ve kapalı pull requestler
- Merge durumu
- Repository sahiplik ve fork bilgisi
- Veri erişilebilirlik durumu

### Değerlendirme

- Birden fazla merged pull request varsa: `Strong`, 6 puan
- Tam olarak bir merged pull request varsa: `Present`, 4 puan
- Yalnızca açık veya kapanmış ancak birleşmemiş pull request varsa: `Partial`, 2 puan
- PR verisi incelendi ancak PR yoksa: `Missing`, 0 puan
- Veri alınamadıysa: `UnavailableEvidence`, `Score: null`
- Repository'nin küçük veya tek geliştiricili olması tek başına `NotApplicable` üretmez

### Uygulanabilirlik sınırı

Tek geliştiricili küçük repository'lerde pull request eksikliği mutlak bir kalite kusuru olarak sunulmamalıdır.

### Öneri

Önemli değişiklikleri ayrı branch ve pull request akışıyla görünür hâle getirin.

### Sınırlar

Pull request varlığı code review yapıldığını veya ekip çalışmasını garanti etmez.

---

## 11. Release Kullanımı

~~~text
Key: process.release-usage
Name: Release Kullanımı
Category: DevelopmentProcessVisibility
MaximumScore: 6
Version: 1
~~~

### Amaç

Repository'nin belirli sürüm veya teslim noktalarının görünür olup olmadığını değerlendirmek.

### Gerekli veri

- GitHub Releases
- Tags
- Published ve draft durumu
- Veri erişilebilirlik durumu

### Değerlendirme

- En az bir published release ve açıklayıcı release notu varsa: `Present`, 6 puan
- Published release var ancak açıklayıcı release notu yoksa: `Partial`, 4 puan
- Tag var ancak published release yoksa: `Partial`, 3 puan
- Yalnızca draft release varsa: `Partial`, 2 puan
- Veri incelendi ancak release veya tag yoksa: `Missing`, 0 puan
- Veri alınamadıysa: `UnavailableEvidence`, `Score: null`

### Uygulanabilirlik sınırı

MVP v1 repository amacını güvenilir ve teknoloji bağımsız biçimde otomatik sınıflandırmaz.

Bu nedenle repository küçük, eğitim, dokümantasyon veya deneysel olduğu tahmin edilerek release sinyali otomatik `NotApplicable` yapılmaz.

Bu bağlam gerekirse rapor sınırlaması olarak gösterilir.

### Öneri

Anlamlı teslim noktaları için sürüm etiketi ve açıklayıcı GitHub Release oluşturun.

### Sınırlar

Release bulunması semantic versioning, production kararlılığı veya aktif bakım kanıtı değildir.

---

# Doğrulama ve Kullanılabilirlik

## 12. Test Yapısı

~~~text
Key: validation.test-structure
Name: Test Yapısı
Category: ValidationAndUsability
MaximumScore: 10
Version: 1
~~~

### Amaç

Kod davranışlarını otomatik olarak doğrulamaya yönelik görünür yapı bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- File tree
- Proje ve dependency dosyaları
- Test klasörleri
- Yaygın test dosyası desenleri
- Veri erişilebilirlik durumu

### İlk teknoloji bağımsız göstergeler

~~~text
tests/
test/
__tests__/
*.Tests.csproj
*.Test.csproj
*.spec.*
*.test.*
pytest.ini
jest.config.*
vitest.config.*
pom.xml içinde test dependency
build.gradle içinde test dependency
~~~

### Değerlendirme

- Birden fazla test türü veya anlamlı test organizasyonu varsa: `Strong`, 10 puan
- Açık test projesi veya test framework kanıtı varsa: `Present`, 8 puan
- Sınırlı test dosyası varsa: `Partial`, 4 puan
- Yalnızca isim benzerliği taşıyan belirsiz dosyalar varsa: `RequiresHumanReview`, `Score: null`
- File tree başarıyla incelendi ancak test kanıtı yoksa: `Missing`, 0 puan
- File tree alınamadıysa: `UnavailableEvidence`, `Score: null`

### Öneri

Kritik davranışları doğrulayan ve proje yapısından açıkça ayrılan otomatik testler ekleyin.

### Sınırlar

Test dosyalarının bulunması testlerin yeterli, doğru, güncel veya başarılı olduğunu kanıtlamaz.

---

## 13. Continuous Integration

~~~text
Key: validation.continuous-integration
Name: Continuous Integration
Category: ValidationAndUsability
MaximumScore: 8
Version: 1
~~~

### Amaç

Build, test veya başka doğrulamaların otomatikleştirilmesine yönelik görünür workflow bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- `.github/workflows` file tree
- YAML içerikleri
- Veri erişilebilirlik durumu

### Değerlendirme

- Build ve test birlikte çalıştırılıyorsa: `Strong`, 8 puan
- En az bir workflow build veya test adımı içeriyorsa: `Present`, 6 puan
- Yalnızca deploy workflow'u varsa: `Partial`, 3 puan
- Workflow var ancak amacı doğrulanamıyorsa: `Partial`, 2 puan
- Workflow dizini incelendi ancak dosya yoksa: `Missing`, 0 puan
- İçerik alınamadıysa: `UnavailableEvidence`, `Score: null`
- YAML yorumlama güvenilir değilse: `RequiresHumanReview`, `Score: null`

### Öneri

Push ve pull requestlerde build ile test komutlarını çalıştıran temel bir GitHub Actions workflow'u ekleyin.

### Sınırlar

Workflow dosyasının bulunması başarılı çalıştığını, aktif olduğunu veya güvenli yapılandırıldığını garanti etmez.

---


# Portföy Sunumu

## 14. GitHub Topics

~~~text
Key: portfolio.topics
Name: GitHub Topics
Category: PortfolioPresentation
MaximumScore: 4
IncludeInOverallScore: true
Version: 1
~~~

### Amaç

Repository'nin teknoloji, ürün veya problem alanını GitHub metadata üzerinde sınıflandırıp sınıflandırmadığını değerlendirmek.

### Gerekli veri

- Repository metadata
- Topics listesi
- Veri erişilebilirlik durumu

### Uygulanabilirlik

Bütün public repository'lerde uygulanır.

### Değerlendirme

- Üç veya daha fazla topic varsa: `Strong`, 4 puan
- İki topic varsa: `Present`, 3 puan
- Bir topic varsa: `Present`, 2 puan
- Topics verisi başarıyla alındı ancak topic yoksa: `Missing`, 0 puan
- Metadata alınamadıysa: `UnavailableEvidence`, `Score: null`

### Kanıt örneği

~~~text
Repository metadata içerisinde 4 topic bulundu:
dotnet, github-api, portfolio, open-source
~~~

### Öneri

Repository'nin teknoloji ve problem alanını açıklayan doğru ve sınırlı sayıda GitHub topic ekleyin.

### Sınırlar

Topics bulunması, belirtilen teknolojilerin doğru veya etkin kullanıldığını kanıtlamaz.

---

## 15. Homepage veya Canlı Demo Bağlantısı

~~~text
Key: portfolio.homepage-or-live-demo
Name: Homepage veya Canlı Demo Bağlantısı
Category: PortfolioPresentation
MaximumScore: 5
IncludeInOverallScore: true
Version: 1
~~~

### Amaç

Repository'den çalışan ürüne, ayrıntılı dokümantasyona veya canlı demoya ulaşılabilen görünür bir bağlantı bulunup bulunmadığını değerlendirmek.

### Gerekli veri

- Repository homepage alanı
- README bağlantıları
- Veri erişilebilirlik durumu

### Kanıt türleri

- Canlı uygulama bağlantısı
- Proje dokümantasyonu
- Etkileşimli demo
- Yayınlanmış örnek
- Ürün veya proje sayfası

### Değerlendirme

- Repository homepage alanında anlamlı bir bağlantı varsa: `Present`, 5 puan
- README içerisinde açık demo bağlantısı varsa: `Present`, 4 puan
- Bağlantı bulundu ancak amacı otomatik olarak belirlenemiyorsa: `Partial`, 2 puan
- Veri başarıyla incelendi ancak bağlantı bulunamadıysa: `Missing`, 0 puan
- README veya metadata alınamadıysa: `UnavailableEvidence`
- Bağlantının anlamı belirsizse: `RequiresHumanReview`

### Öneri

Çalışan ürün, proje dokümantasyonu veya canlı demo için GitHub homepage alanına açık bir bağlantı ekleyin.

### Sınırlar

MVP'de bağlantının erişilebilir, güncel veya güvenli olduğu her zaman doğrulanmayabilir.

---

## 16. Repository Sunum Bütünlüğü

~~~text
Key: portfolio.presentation-completeness
Name: Repository Sunum Bütünlüğü
Category: PortfolioPresentation
MaximumScore: 5
IncludeInOverallScore: false
Version: 1
~~~

### Amaç

Repository'nin temel sunum parçalarını birlikte taşıyıp taşımadığını deterministik bir kontrol listesiyle özetlemek.

### Gerekli veri

Aşağıdaki sinyal sonuçları veya eşdeğer kanıtlar:

- Proje açıklaması
- README varlığı
- Kullanım örneği veya demo
- Lisans
- GitHub Topics

### Kontrol listesi

~~~text
1. Repository description mevcut
2. README mevcut
3. Kullanım örneği veya demo mevcut
4. Lisans mevcut
5. En az bir GitHub topic mevcut
~~~

### Değerlendirme

- Beş koşulun tamamı sağlanıyorsa: `Strong`, 5 puan
- Dört koşul sağlanıyorsa: `Present`, 4 puan
- Üç koşul sağlanıyorsa: `Partial`, 3 puan
- İki koşul sağlanıyorsa: `Partial`, 2 puan
- Bir koşul sağlanıyorsa: `Partial`, 1 puan
- Hiçbir koşul sağlanmıyorsa: `Missing`, 0 puan
- Gerekli alt verilerden biri erişilemiyorsa: `UnavailableEvidence`, `Score: null`

### Toplam skor davranışı

Bu sinyal, başka sinyallerin birlikte bulunma durumunu özetlediği için toplam görünür kanıt skoruna dahil edilmez.

~~~text
IncludeInOverallScore: false
~~~

Kategori raporunda, genel özette ve geliştirme önerilerinde kullanılabilir.

### Öneri

Repository'nin sunumunu güçlendirmek için eksik description, README, kullanım örneği, lisans veya topic parçalarını tamamlayın.

### Sınırlar

- Teknik kaliteyi değerlendirmez.
- Görsel estetiği değerlendirmez.
- Geliştirici seviyesini değerlendirmez.
- Alt sinyallerden türetildiği için toplam skora yeniden dahil edilmez.

---

## 17. MVP Sinyal Ağırlıkları

| Sinyal | Maksimum puan | Toplam skora dahil |
|---|---:|:---:|
| README Varlığı | 5 | Evet |
| Proje Açıklaması | 4 | Evet |
| Kurulum Talimatı | 8 | Evet |
| Lisans | 3 | Evet |
| Kullanım Örneği veya Demo | 7 | Evet |
| Commit Geçmişi | 7 | Evet |
| Commitlerin Zamana Yayılımı | 8 | Evet |
| Issue Kullanımı | 5 | Evet |
| Pull Request Kullanımı | 6 | Evet |
| Release Kullanımı | 6 | Evet |
| Test Yapısı | 10 | Evet |
| Continuous Integration | 8 | Evet |
| GitHub Topics | 4 | Evet |
| Homepage veya Canlı Demo Bağlantısı | 5 | Evet |
| Repository Sunum Bütünlüğü | 5 | Hayır |
| **Kategori içi maksimum** | **91** |  |
| **Toplam skora dahil maksimum** | **86** |  |

Bu ağırlıklar MVP başlangıç varsayımıdır.

Gerçek repository doğrulaması tamamlanmadan kalıcı ürün gerçeği olarak kabul edilmemelidir.

Repository Sunum Bütünlüğü bileşik bir sinyal olduğu için toplam görünür kanıt skoruna dahil edilmez.

---

## 18. Kategori Dağılımı

| Kategori | Sinyal sayısı | Kategori içi maksimum | Toplam skora dahil maksimum |
|---|---:|---:|---:|
| Repository anlaşılabilirliği | 5 | 27 | 27 |
| Geliştirme süreci görünürlüğü | 5 | 32 | 32 |
| Doğrulama ve kullanılabilirlik | 2 | 18 | 18 |
| Portföy sunumu | 3 | 14 | 9 |
| **Toplam** | **15** | **91** | **86** |

### Puanlama açıklaması

`Kategori içi maksimum`, kategori raporunda gösterilen bütün sinyalleri içerir.

`Toplam skora dahil maksimum`, yalnızca `IncludeInOverallScore: true` olan sinyalleri içerir.

Repository Sunum Bütünlüğü:

- Portföy kategorisi içinde gösterilir.
- Kategori özeti ve öneriler için kullanılır.
- Toplam görünür kanıt skoruna dahil edilmez.
- Alt sinyallerin ikinci kez puanlanmasını engeller.

---

## 19. Contract Lock v1

MVP v1 kodlaması için aşağıdaki kararlar kesindir:

1. Sinyal skorları yalnızca tam sayıdır.
2. Puan aralığı bırakılmaz; her gözlemlenebilir durum deterministik olarak tek bir puan üretir.
3. `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` için `Score: null` üretilir.
4. Bu üç durum numerator ve denominator hesabından çıkarılır.
5. `Missing` yalnızca gerekli veri başarıyla incelendiyse kullanılabilir; `Score: 0` olur ve denominator'a dahil edilir.
6. Repository yaşı 14 günden azsa `process.commit-distribution` `NotApplicable` olur.
7. Repository'nin küçük, tek geliştiricili, eğitim veya deneysel olduğu tahmin edilerek issue, pull request veya release sinyalleri otomatik `NotApplicable` yapılmaz.
8. Issue sinyali yalnız GitHub Issues özelliği kapalıysa otomatik `NotApplicable` olabilir.
9. README ve indekslenebilir dokümantasyon başlık eşleştirmesinde MVP v1 Türkçe ve İngilizce adayları destekler.
10. Demo veya homepage bağlantılarının gerçek erişilebilirliği MVP v1'de zorunlu olarak doğrulanmaz.
11. GitHub Actions workflow dosyasının gerçek run durumu MVP v1'de Actions API üzerinden doğrulanmaz.
12. `portfolio.presentation-completeness` Portföy Sunumu kategori skoruna dahil edilir.
13. `portfolio.presentation-completeness` genel toplam görünür kanıt skoruna dahil edilmez.
14. Kategori ve genel normalize skorları en yakın tam sayıya yuvarlanır.
15. Tam `.5` durumunda yukarı yuvarlama kullanılır.
16. Genel toplam hesaplama yöntemi `RawApplicableScoreNormalization` olarak kilitlenmiştir.

Bu kararlar doğrulama repository setindeki bulgular nedeniyle değiştirilecekse ilgili sinyal `Version` veya `RuleSetVersion` artırılmalıdır.

Portföy Sunumu kategorisinin korunması ve üç sinyalle desteklenmesi ADR 001 ile karara bağlanmıştır.

---

## 20. Faz 1 Çıkış Kriterine Katkısı

Bu belge tamamlandığında:

- İlk 15 MVP sinyalinin anahtarları belirlenmiş olur.
- Her sinyalin maksimum puanı sabitlenmiş olur.
- Her sinyalin toplam skora dahil edilme davranışı görünür olur.
- Dört kategorinin tamamı sinyal sahibi olur.
- Gerekli GitHub verileri görünür hâle gelir.
- `Missing` ile `UnavailableEvidence` ayrımı sinyal bazında tanımlanmış olur.
- Her sinyal için ürün dili ve öneri çerçevesi belirlenmiş olur.
- Teknoloji ve repository bağlamı riskleri görünür hâle gelir.
- ADR 001 kararı analiz sözleşmesine uygulanmış olur.
