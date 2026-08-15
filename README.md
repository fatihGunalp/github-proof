# GitHub Proof

> **GitHub reponuz yalnızca kod içermez.**
>
> Dışarıdan bakıldığında hangi mühendislik kanıtlarının görünür olduğunu da gösterir.

GitHub Proof, public bir GitHub repository'sini analiz ederek dışarıdan gözlemlenebilen mühendislik kanıtlarını açıklanabilir sinyaller hâlinde görünür kılan açık kaynak bir analiz platformudur.

Projenin amacı geliştiricileri puanlamak değildir.

Amaç, **analiz edilen repository'nin hangi kanıtları görünür biçimde taşıdığını ve hangi alanların geliştirilebileceğini göstermektir.**

---

## Problem

Bir geliştirici GitHub üzerinde proje yayınlamış olabilir; ancak repository dışarıdan incelendiğinde şu soruların cevapları görünmeyebilir:

- Projenin amacı, kurulumu ve kullanım biçimi anlaşılabiliyor mu?
- Commit, issue, pull request ve release gibi geliştirme süreci kanıtları görünür mü?
- Test ve Continuous Integration yapısı görünür mü?
- Repository topics, homepage/demo ve temel sunum parçalarıyla anlaşılır biçimde sunulmuş mu?

GitHub Proof bu sorulara yalnızca repository içerisinde gözlemlenebilen kanıtlar üzerinden cevap vermeyi amaçlar.

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
- 15 açıklanabilir MVP sinyalini analiz edecek.
- Sonuçları dört kategori altında gösterecek.
- Her puanın dayandığı kanıtı açıklayacak.
- Üç öncelikli geliştirme önerisi sunacak.
- Web raporu ve Markdown çıktısı üretecek.

MVP fazlarının ayrıntıları için:

> [GitHub Proof — MVP Fazları](docs/roadmap/mvp-phases.md)

---

## Ürün Belgeleri

MVP'nin ürün sınırları, analiz kuralları ve raporlama yaklaşımı aşağıdaki belgelerde tanımlanmıştır:

- [MVP Ürün Sözleşmesi](docs/product/mvp-product-contract.md)
- [Analiz Kategorileri ve Sinyal Sınırları](docs/product/analysis-categories-and-signal-boundaries.md)
- [Puanlama ve Rapor Sözleşmesi](docs/product/scoring-and-report-contract.md)
- [Örnek MVP Analiz Raporu](docs/product/sample-analysis-report.md)
- [Doğrulama Repository Seti](docs/product/validation-repository-set.md)

---


## Analiz Alanları

### 1. Repository Anlaşılabilirliği

Repository ilk bakışta anlaşılabiliyor mu?

MVP sinyalleri:

- README varlığı
- Proje açıklaması
- Kurulum talimatı
- Lisans
- Kullanım örneği veya demo

### 2. Geliştirme Süreci Görünürlüğü

Geliştirme sürecine ait gözlemlenebilir kanıtlar repository üzerinde görülebiliyor mu?

MVP sinyalleri:

- Commit geçmişi
- Commitlerin zamana yayılımı
- Issue kullanımı
- Pull request kullanımı
- Release kullanımı

### 3. Doğrulama ve Kullanılabilirlik

Kod davranışlarının otomatik olarak doğrulanmasına yönelik görünür yapı bulunuyor mu?

MVP sinyalleri:

- Test yapısı
- Continuous Integration

### 4. Portföy Sunumu

Repository, yapılan üretimi dışarıdan inceleyen bir kişiye açık ve anlamlı biçimde sunabiliyor mu?

MVP sinyalleri:

- GitHub Topics
- Homepage veya canlı demo bağlantısı
- Repository sunum bütünlüğü

`Repository sunum bütünlüğü` bileşik bir sinyaldir ve toplam görünür kanıt skoruna yeniden dahil edilmez.

## AI Hakkındaki Yaklaşım

GitHub Proof:

> “Bu kod AI tarafından yazılmıştır.”

iddiasında bulunmaz.

MVP yalnızca tanımlı repository sinyallerini değerlendirir; örneğin:

- Commit geçmişi ve zamana yayılımı
- Dokümantasyon ve kullanım kanıtları
- Test ve Continuous Integration yapısı
- Issue, pull request ve release kullanımı
- Topics, homepage/demo ve repository sunum bütünlüğü

Bu sinyallerin hiçbiri tek başına veya birlikte AI kullanıldığını, kodun kopyalandığını ya da geliştiricinin yetkinlik seviyesini kanıtlamaz.

---

## Örnek Rapor

~~~text
Repository Kanıt Profili

Bu repository neyi kanıtlıyor?

✓ Commit geçmişi geliştirme sürecini görünür kılıyor.
✓ Repository üzerinde test yapısı gözlemleniyor.

Kanıtı ne zayıflatıyor?

△ Kurulum ve kullanım kanıtı sınırlı.
✗ CI doğrulaması görünür değil.
✗ Release süreci görünür değil.

Önce bunları yap

1. Kurulum sürecini belgeleyin.
2. Test doğrulamasını CI ile otomatikleştirin.
3. İlk anlamlı release'i yayınlayın.

Kategori özeti — ikincil

Repository anlaşılabilirliği: 59 / 100
Geliştirme süreci görünürlüğü: 66 / 100
Doğrulama ve kullanılabilirlik: 44 / 100
Portföy sunumu: 57 / 100

Toplam görünür kanıt — ikincil: 56 / 100
Etiket: Gelişen görünür kanıt
~~~

Bu örnek temsilîdir ve [Örnek MVP Analiz Raporu](docs/product/sample-analysis-report.md) ile aynı puanlama sözleşmesini kullanır.

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
