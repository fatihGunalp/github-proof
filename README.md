# GitHub Proof

> **GitHub reponuz yalnızca kod içermez.**
>
> Nasıl düşündüğünüzü, nasıl ürettiğinizi ve bir yazılım geliştiricisi olarak neyi gerçekten kanıtlayabildiğinizi de gösterir.

GitHub Proof, public bir GitHub repository'sini analiz ederek dışarıdan gözlemlenebilen mühendislik sinyallerini görünür hâle getiren açık kaynak bir analiz platformudur.

Projenin amacı geliştiricileri puanlamak değildir.

Amaç, **bir repository'nin gerçekten neyi kanıtlayabildiğini açıklanabilir kanıtlarla göstermektir.**

---

## Problem

Bugün çok sayıda geliştiricinin GitHub hesabında projeler bulunuyor. Ancak bir recruiter, teknik lider veya mentor repository'yi açtığında çoğu zaman şu sorunun cevabını net biçimde göremiyor:

> **Bu repository bana geliştirici ve üretim süreci hakkında gerçekten ne söylüyor?**

Bir projenin çalışması önemlidir; fakat tek başına yeterli bir mühendislik kanıtı değildir.

Bir repository aynı zamanda şu alanlarda da sinyal üretir:

- Üretim disiplini
- Teknik iletişim
- Geliştirme sürecinin görünürlüğü
- Doğrulama yaklaşımı
- Bakım kültürü
- Sahiplenme
- Portföy sunumu

GitHub Proof bu sinyalleri toplayarak anlaşılır ve geliştirilebilir bir rapora dönüştürür.

---

## Temel İlke

> **Çalışan kod, tek başına mühendisliği kanıtlamaz.**

GitHub Proof geliştiriciyi değerlendirmez.

Yalnızca repository içerisinde dışarıdan gözlemlenebilen kanıtları değerlendirir.

Bir geliştiricinin seviyesi tek bir repository'ye veya skora indirgenemez. Bu nedenle üretilen sonuçlar karar vermek için nihai hüküm değil, repository'yi geliştirmek için açıklanabilir bir rehberdir.

---

## MVP

İlk sürümün temel kullanıcı akışı:

```text
Public repository URL'si gir
        ↓
Repository verilerini topla
        ↓
Açıklanabilir sinyalleri analiz et
        ↓
Dört kategoride sonuç üret
        ↓
Üç öncelikli geliştirme önerisi göster
```

MVP tamamlandığında sistem:

- Public GitHub repository URL'si kabul edecek.
- En az 12 açıklanabilir sinyal analiz edecek.
- Sonuçları dört kategori altında gösterecek.
- Her puanın dayandığı kanıtı açıklayacak.
- Üç öncelikli geliştirme önerisi sunacak.
- Web raporu ve Markdown çıktısı üretecek.

MVP fazlarının ayrıntıları için:

> [GitHub Proof — MVP Fazları](docs/roadmap/mvp-phases.md)

---

## Analiz Alanları

### 1. Repository Anlaşılabilirliği

Repository ilk bakışta anlaşılabiliyor mu?

- README
- Proje açıklaması
- Kurulum adımları
- Kullanım örneği
- Lisans
- Demo veya ekran görüntüsü
- Repository yapısı

### 2. Geliştirme Süreci Görünürlüğü

Gerçek geliştirme süreci repository üzerinde görülebiliyor mu?

- Commit geçmişi
- Commit dağılımı
- Issue kullanımı
- Pull request kullanımı
- Release geçmişi
- Branch ve version sinyalleri

### 3. Doğrulama ve Kullanılabilirlik

Başka bir geliştirici projeyi çalıştırabilir ve doğrulayabilir mi?

- Build tanımı
- Dependency dosyaları
- Test yapısı
- CI workflow
- Ortam değişkeni örneği
- Configuration açıklığı

### 4. Portföy Sunumu

Repository dışarıdan bakıldığında profesyonel ve anlaşılır bir üretim kanıtı sunuyor mu?

- Repository adı ve açıklaması
- Topics
- Demo bağlantısı
- Düzenli sunum
- Bakım sinyalleri
- Kullanıcıya açık amaç

---

## AI Hakkındaki Yaklaşım

GitHub Proof:

> “Bu kod AI tarafından yazılmıştır.”

iddiasında bulunmaz.

Bunun yerine insan değerlendirmesi gerektiren görünür sinyalleri gösterir:

- Tek seferde oluşmuş büyük commitler
- Açıklanmayan teknik kararlar
- Generic dokümantasyon
- Eksik doğrulama
- Görünmeyen mimari kararlar

Bunlar AI tespiti değildir.

Bunlar **mühendislik kanıtlarının görünürlüğüyle ilgili sinyallerdir.**

---

## Örnek Rapor

```text
Repository Kanıt Profili

Toplam görünür kanıt: 74 / 100

Güçlü alanlar

✓ Repository yapısı
✓ Commit geçmişi
✓ Gelişim sinyalleri

Geliştirilmeli

✗ Kurulum dokümantasyonu
✗ Release kullanımı
✗ Teknik karar görünürlüğü

Öncelikli adımlar

1. README içerisine kurulum ve kullanım bölümü ekle.
2. İlk anlamlı release'i yayınla.
3. Kritik teknik kararları docs/decisions altında belgele.
```

Toplam skor karşılaştırma veya geliştirici sıralaması için kullanılmamalıdır.

Her skor, gözlemlenebilir kanıt ve açıklamayla birlikte gösterilmelidir.

---

## Proje Yapısı

```text
github-proof/

├── docs/
│   ├── architecture/
│   ├── decisions/
│   ├── roadmap/
│   │   └── mvp-phases.md
│   └── screenshots/
│
├── src/
│   ├── GitHubProof.Api/
│   ├── GitHubProof.Application/
│   │   ├── Analysis/
│   │   ├── Portfolio/
│   │   ├── Recommendations/
│   │   ├── Reports/
│   │   └── Scoring/
│   │
│   ├── GitHubProof.Domain/
│   │   ├── Portfolio/
│   │   ├── Reports/
│   │   ├── Rules/
│   │   └── Signals/
│   │
│   ├── GitHubProof.Infrastructure/
│   │   ├── Caching/
│   │   ├── GitHub/
│   │   ├── Markdown/
│   │   └── Persistence/
│   │
│   └── GitHubProof.Web/
│       ├── Assets/
│       ├── Components/
│       ├── Layout/
│       └── Pages/
│
├── tests/
│   ├── GitHubProof.UnitTests/
│   ├── GitHubProof.IntegrationTests/
│   └── GitHubProof.SnapshotTests/
│
├── samples/
│   ├── Reports/
│   └── Repositories/
│
├── .github/
├── LICENSE
└── README.md
```

---

## Tasarım İlkeleri

GitHub Proof aşağıdaki ilkelerle geliştirilecektir:

- Açıklanabilir analiz
- Şeffaf puanlama
- Tekrarlanabilir sonuçlar
- Gözlemlenebilir kanıtlar
- Küçük ve doğrulanabilir MVP
- Kural tabanlı çekirdek
- Genişletilebilir sinyal altyapısı
- Kara kutu karar mekanizmalarından kaçınma

---

## MVP Kapsamı Dışında Kalanlar

İlk sürümde aşağıdaki özellikler bulunmayacaktır:

- Private repository analizi
- Kullanıcı hesabı
- Analiz geçmişi
- Repository karşılaştırması
- Recruiter puanlaması
- Takım raporları
- Ödeme
- AI kullanım tespiti
- Kod kalitesinin bütünüyle değerlendirilmesi
- Mimari olgunluk hakkında kesin hüküm

---

## Yol Haritası

### MVP

- Ürün sınırları
- Analiz modeli
- GitHub veri toplama
- Kural tabanlı Signal Engine
- Rapor üretimi
- Minimal web arayüzü
- Gerçek repository doğrulaması
- LinkedIn pilot lansmanı

### MVP Sonrası

- Analiz geçmişi
- Repository gelişim zaman çizelgesi
- LinkedIn paylaşım kartı
- GitHub badge
- Public rapor bağlantısı
- Recruiter görünümü
- Derin teknik analiz
- LLM destekli açıklama katmanı

Ayrıntılı yol haritası:

> [GitHub Proof — MVP Fazları](docs/roadmap/mvp-phases.md)

---

## Planlanan Teknolojiler

Teknoloji tercihleri ürün ihtiyaçları doğrulandıktan sonra kesinleştirilecektir.

İlk teknik yönelim:

- C#
- ASP.NET Core
- GitHub REST API
- Blazor veya ASP.NET Core tabanlı minimal web arayüzü

PostgreSQL, Docker ve LLM desteği yalnızca gerçek ihtiyaç oluştuğunda projeye dahil edilmelidir.

---

## Katkıda Bulunma

GitHub Proof açık kaynak olarak geliştirilecektir.

Bug bildirimleri, sinyal önerileri, test repository'leri ve kod katkıları memnuniyetle karşılanır.

Katkı kuralları proje geliştikçe `.github/CONTRIBUTING.md` altında yayınlanacaktır.

---

## Lisans

Bu projenin MIT lisansı altında yayınlanması planlanmaktadır.

---

## Son Söz

GitHub Proof'un amacı çalışan yazılımları ödüllendiren yeni bir skor üretmek değildir.

Amaç:

> **Repository içerisinde zaten bulunan mühendislik kanıtlarını görünür, açıklanabilir ve geliştirilebilir hâle getirmektir.**
