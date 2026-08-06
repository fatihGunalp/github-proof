# GitHub Proof — MVP Fazları

Bu doküman, GitHub Proof projesinin MVP'ye ulaşması için izlenecek geliştirme fazlarını, kapsam sınırlarını ve çıkış kriterlerini tanımlar.

## MVP Tanımı

GitHub Proof MVP'si:

> Public bir GitHub repository URL'si alan, en az 12 açıklanabilir sinyali analiz eden, dört kategoride sonuç ve üç öncelikli geliştirme önerisi üreten çalışan bir web uygulamasıdır.

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

### İlk sinyaller

#### Repository anlaşılabilirliği

- README var mı?
- README yeterli içerik taşıyor mu?
- Proje açıklaması var mı?
- Lisans var mı?
- Kurulum bölümü var mı?
- Kullanım örneği, ekran görüntüsü veya demo bağlantısı var mı?

#### Geliştirme süreci görünürlüğü

- Commit sayısı
- Commitlerin zamana yayılımı
- Tek büyük başlangıç commit'i
- Issue kullanımı
- Pull request kullanımı
- Release varlığı

#### Doğrulama ve kullanılabilirlik

- Test projesi veya test klasörü var mı?
- Build tanımı bulunuyor mu?
- CI workflow var mı?
- Ortam değişkenleri örneklenmiş mi?
- Dependency dosyaları mevcut mu?

#### Portföy sunumu

- Repository adı ve açıklaması anlaşılır mı?
- Topics kullanılmış mı?
- Demo veya canlı bağlantı var mı?
- Repository arşivlenmiş mi?
- Varsayılan branch sağlıklı mı?

### Puanlama ilkesi

Toplam skor tek başına gösterilmemelidir. Her puan, kanıtı ve açıklamasıyla birlikte sunulmalıdır.

```text
README bulundu: +8
Kurulum adımı bulunamadı: +0
Lisans bulundu: +3
Test yapısı bulundu: +10
Release bulunamadı: +0
```

### Çıkış kriteri

En az 10 farklı public repository elle değerlendirilir ve sistem sonuçları insan değerlendirmesiyle karşılaştırılır.

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

### Amaç

Toplanan repository verilerinden açıklanabilir analiz sonuçları üretmek.

### İlk sinyal yapısı

```text
Signals/
├── ReadmeExistsSignal
├── InstallationGuideSignal
├── LicenseSignal
├── CommitDistributionSignal
├── LargeInitialCommitSignal
├── IssueUsageSignal
├── PullRequestUsageSignal
├── ReleaseSignal
├── TestPresenceSignal
├── ContinuousIntegrationSignal
├── EnvironmentExampleSignal
└── DemoVisibilitySignal
```

### Her sinyalin çıktısı

```text
SignalResult
├── Key
├── Status
├── Score
├── MaximumScore
├── Evidence
├── Explanation
└── Recommendation
```

### Örnek sinyal sonucu

```text
Sinyal: Kurulum Talimatı

Durum: Eksik
Puan: 0 / 8

Kanıt:
README içerisinde installation, setup veya kurulum bölümü bulunamadı.

Neden önemli:
Projeyi inceleyen kişi uygulamayı tekrar çalıştıramıyor.

Öneri:
README dosyasına bağımlılıkları, yapılandırmayı ve çalıştırma komutlarını ekleyin.
```

### Tasarım kuralı

LLM veya kara kutu karar mekanizması, MVP puanlamasının temelini oluşturmamalıdır.

### Çıkış kriteri

Rapor üzerindeki her puanın hangi gözlemlenebilir kanıta dayanarak verildiği açıklanabilmelidir.

---

## Faz 4 — Rapor Üretimi

### Amaç

Teknik analiz sonuçlarını, kullanıcının anlayabileceği ve uygulayabileceği bir geliştirme raporuna dönüştürmek.

### MVP raporu

#### Genel özet

```text
Repository Kanıt Profili

Toplam görünür kanıt: 62 / 100

Güçlü alan:
Geliştirme süreci

En kritik eksik:
Kurulum ve kullanım dokümantasyonu
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

- Repository bilgisi
- Toplam görünür kanıt skoru
- Kategori skorları
- Güçlü sinyaller
- Kritik eksikler
- İlk üç geliştirme adımı
- Markdown raporunu kopyalama

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
- En az 12 açıklanabilir sinyal değerlendiriliyor.
- Sonuçlar dört kategori altında gösteriliyor.
- Her skorun dayandığı kanıt açıklanıyor.
- Kullanıcıya üç öncelikli geliştirme önerisi sunuluyor.
- Web raporu ve Markdown çıktısı üretilebiliyor.
- En az 20 gerçek repository ile doğrulama yapılmış oluyor.
