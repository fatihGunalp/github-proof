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
