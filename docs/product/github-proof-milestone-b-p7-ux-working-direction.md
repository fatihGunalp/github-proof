# GitHub Proof — Milestone B UX Direction

**Status:** WORKING UX DIRECTION
**Current Development Position:** Milestone B / Profile P7
**Scope:** Milestone B — Profile Proof Beta
**Document Alignment:** DEFERRED — canonical product/design documentation alignment will be handled later
**Purpose:** Milestone B kapsamında bugüne kadar kilitlenen kullanıcı sancısı, deneyim, sadeleştirme ve presentation kararlarını tek bir çalışma dokümanında toplamak.

---

## 0. Mevcut Konum

GitHub Proof şu anda Milestone B kapsamında ilerlemektedir.

Milestone B'nin amacı, ürünü tek repository analizinden çıkarıp public GitHub profile evidence ürününe dönüştürmektir.

Mevcut development konumu:

> **Profile P7 — Representative Repositories ve Role Evidence Alignment**

Bu doküman şu anda canonical product/design dokümanlarını değiştirmez. Doküman hizalaması daha sonra yapılacaktır.

---

## 1. Temel Problem

Public bir GitHub profili repository'ler, README'ler, commit geçmişleri, teknolojiler, testler, workflow'lar, issue/PR izleri, releases, tags ve metadata gibi çok sayıda gözlemlenebilir bilgi içerir.

Sorun veri azlığı değildir.

> **Sorun, bu kadar dağınık bilgi arasından neyin önemli olduğunu ve ne anlam ifade ettiğini hızlıca çıkarmanın zor olmasıdır.**

Kullanıcının gerçek sancıları:

- “Bu kadar repository arasında neye bakmalıyım?”
- “Hepsini inceleyecek vaktim yok.”
- “Buradan ne anlamam gerekiyor?”
- “Tek seferlik olanla tekrar edeni nasıl ayıracağım?”
- “Bu sonucun gerçek dayanağı var mı?”

Milestone B'nin görevi kullanıcıya daha fazla veri vermek değil, dağınık public GitHub evidence'ını anlamlı hale getirmek ve karar yükünü azaltmaktır.

---

## 2. Ana Tasarım Felsefesi

Ürünün iç yapısı karmaşık olabilir. Kullanıcı deneyiminin karmaşık olması gerekmez.

Temel ilkeler:

> **Teknik özelliği değil, kullanıcı sonucunu göster.**

> **Karmaşıklığı kullanıcıya devretme.**

> **Kullanıcının taşımaması gereken karar ve yorum yükünü kaldır.**

RepositoryEligibilityResult, RepositoryAnalysis[], CrossRepositoryEvidence, RepeatedProfilePattern, TechnicalFocusArea, EngineeringPracticePattern, ProblemAreaPattern, RepresentativeRepository, RoleEvidenceAlignment, thresholds, versioning ve aggregation sistemin iç motorudur.

Kullanıcının ihtiyacı:

> **Nereye bakmalıyım?**

> **Buradan ne anlamalıyım?**

> **İstersem gerçek kanıta ulaşabilir miyim?**

---

## 3. Kullanıcıya Verilen Büyük Fayda

Milestone B'nin kullanıcıya verdiği ana güç:

> **Onlarca repository'ye tek tek bakmadan, bir GitHub profilinin ne anlattığını anlayabilmek.**

Bu fayda insan dilinde şu sorulara indirilebilir:

- Ne üretiyor?
- Nasıl çalışıyor?
- Ne tür problemler üzerinde çalışıyor?
- Vaktim azsa önce hangi projelerine bakmalıyım?
- Bu sonucun gerçek dayanağı nerede?

Bu soruların nihai UI copy'si henüz kilitlenmemiştir.

---

## 4. Milestone B Kullanıcı Yolculuğu

Milestone B ekran koleksiyonu olarak değil, tek bir deneyim olarak düşünülmelidir.

```text
GitHub input
        ↓
Profile analysis
        ↓
Profile-level anlam
        ↓
Tekrar eden üretim izleri
        ↓
Öne çıkan / representative projeler
        ↓
Repository evidence
        ↓
Gerçek GitHub evidence
```

Kullanıcının zihinsel akışı:

```text
Bu GitHub bana ne anlatıyor?
        ↓
Neye bakmaya değer?
        ↓
Önce nereden başlamalıyım?
        ↓
Bu proje bana ne gösteriyor?
        ↓
İstersem kanıtına kadar inebilir miyim?
```

---

## Adım 1 — Gerçek Sancıyı Tanımla

Kullanıcının problemi GitHub'da veri olmaması değildir.

> **Public GitHub profilinde çok fazla dağınık bilgi vardır ve bunlardan anlam çıkarmak zaman ve yorum yükü yaratır.**

Karar:

> **Milestone B'nin çözmesi gereken temel problem, public GitHub profilindeki dağınık kanıtların anlamlı bir profile-level görünüm haline getirilmesidir.**

---

## Adım 2 — Yolculuğu Tek Bütün Yap

Milestone B tek yönlü bir rapor değildir. Kullanıcı üst seviyeden başlayıp ihtiyaç duydukça derine iner.

Karar:

> **Profile summary → repository → evidence zinciri tek ve doğal bir exploration deneyimi olarak çalışacaktır.**

---

## Adım 3 — Teknik Kabiliyeti Kullanıcı Faydasına Çevir

Sistemin içeride ne yaptığı ürün mesajı değildir.

Karar:

> **Milestone B'nin amacı GitHub verisini göstermek değil, GitHub'daki dağınık üretimi hızlıca anlamlı hale getirmektir.**

---

## Adım 4 — Teknik İşleri Kullanıcı Sancısına Bağla

| Sistem Ne Yapıyor? | Kullanıcının Sancısı | Kullanıcıya Verilen Sonuç |
|---|---|---|
| Public repository inventory | “Çok fazla repo var.” | Karmaşayı sistem toplar |
| Repository eligibility | “Hangileri anlamlı bilmiyorum.” | Gürültü ayıklanır |
| RepositoryAnalysis[] | “Hepsini tek tek inceleyemem.” | Sistem kullanıcı adına inceler |
| Cross-repository evidence | “Genel anlamı çıkaramıyorum.” | Dağınık noktalar bir araya gelir |
| RepeatedProfilePattern | “Tek seferlik olanla tekrar edeni ayıramıyorum.” | Tekrar eden üretim görünür olur |
| Technical / Practice / Problem patterns | “Bu profil genel olarak ne anlatıyor?” | Ana üretim yönü sadeleşir |
| RepresentativeRepository | “Nereden başlamalıyım?” | İlk bakılması gereken projeler görünür olur |
| Limitations / warnings | “Bu sonuç ne kadar ileri götürülebilir?” | Kanıtın sınırı görünür olur |
| Repository drill-down | “Dayanak nerede?” | Gerçek kanıta inilebilir |

Üç temel kullanıcı sonucu:

> **Nereye bakacağını bil.**
> **Ne anlaman gerektiğini gör.**
> **İstersen kanıtına in.**

---

## Adım 5 — İlk Etkileşimi Tek Harekete İndir

Kullanıcının tek işi:

> **Bakmak istediği GitHub'ı vermek.**

İstenmeyecekler:

- analiz modu
- filtre
- repository seçimi
- kategori seçimi
- scoring tercihi
- authentication
- teknik ayar

Karar:

> **Bir giriş. Bir aksiyon. Sıfır ayar.**

---

## Adım 6 — Analiz Hareketini Kullanıcı Sancısına Göre Göster

Teknik pipeline gösterilmez.

```text
Kalabalık
    ↓
Ayıklama
    ↓
Anlam
    ↓
Öncelik
```

Başlangıç sancısı:

> “Nereye bakacağımı bilmiyorum.”

Bitiş:

> “Artık nereden başlamam gerektiğini biliyorum.”

Karar:

> **Analiz sırasında gösterilen hareket, sistemin teknik işini değil, kullanıcının karmaşasının giderek azalmasını göstermelidir.**

---

## Adım 7 — Sonuçta Rapor Verme, Yön Ver

Sonuç ekranı yeni bir bilgi kalabalığı üretmemelidir.

Karar:

> **Sonuç ekranı neyin önemli olduğunu önceliklendirecek ve ilk bakılması gereken yeri gösterecektir.**

Ana ilke:

> **Rapor verme; yön ver.**

---

## Adım 8 — İlk Bakışta Açıklama Yapma, Göster

İlk katmanda fayda kendisini göstermelidir.

Karar:

> **İlk bakışta açıklama yapma; kullanıcı isterse dayanağını aç.**

---

## Adım 9 — Basitliği Yüzeyde Tut, Gücü Altında Sakla

```text
İlk bakış
    ↓
Anlam

İhtiyaç olursa
    ↓
Detay
    ↓
Kanıt
```

Karar:

> **Basitliği yüzeyde tut, gücü altında sakla.**

---

## Adım 10 — Derine İndir, Kaybettirme

Karar:

> **Her seviyede yalnızca seçilen konu derinleşecek; yeni karmaşa yaratılmayacaktır.**

Ana ilke:

> **Derine indir, kaybettirme.**

---

## Adım 11 — Kullanıcıya Analiz Türü Seçtirme

Repository Proof ve Profile Proof ayrımı sistem mimarisidir; kullanıcının problemi değildir.

Örnek:

```text
fatihGunalp
        ↓
Profile Proof

github.com/fatihGunalp
        ↓
Profile Proof

github.com/fatihGunalp/github-proof
        ↓
Repository Proof
```

Karar:

> **Seçeneği kullanıcıya yükleme; niyetini anla.**

---

## Adım 12 — Sonuç Çıkmadığında da Yolu Açık Tut

Olası durumlar:

- profile ulaşılamaması
- sparse profile
- çoğunlukla fork
- archive-heavy profile
- insufficient evidence
- partial API failure
- rate limit
- timeout
- unavailable evidence

Karar:

> **Hata mesajı değil, devam yolu göster.**

Davranış:

```text
Basitleştir
    ↓
Kullanıcıyı suçlama
    ↓
Teknik karmaşıklığı sakla
    ↓
Devam yolunu açık tut
```

---

## Adım 13 — Dağınık Teknik İzleri Tek Bir Yöne İndir

P7 kapsamında RoleEvidenceAlignment capability'si vardır.

Kullanıcının sancısı:

> **“Bu kadar teknik iz var ama bu profilin genel yönü ne?”**

Karar:

> **Canonical RoleEvidenceAlignment truth korunur; presentation kullanıcıya mümkün olan en sade teknik yön duygusunu verir.**

Ana ilke:

> **Kategori verme; yönü hissettir.**

Bu yön Job Fit, Candidate Score, Seniority, Hire Recommendation veya Employability judgment değildir.

---

## Adım 14 — Kanıtı Aç, İç Implementasyon Karmaşıklığını Kapalı Tut

Kullanıcı şu zincire ulaşabilmelidir:

```text
Sonuç
    ↓
Dayandığı proje
    ↓
Gerçek evidence
```

Gerektiğinde daha teknik inspection açılabilir.

Karar:

> **Kanıtı ve gerektiğinde metodolojiyi aç; iç implementasyon karmaşıklığını kullanıcıya taşıma.**

Ana ilke:

> **Kanıt görünür, motor sessiz.**

---

## 19. Üç Seviyeli Deneyim Modeli

### LEVEL 1 — UNDERSTAND

Kullanıcı hızlıca:

> **“Bu GitHub bana ne anlatıyor?”**

sorusunun cevabını alır.

Burada skor, analyzer vocabulary, teknik rule isimleri ve uzun açıklamalar ana yüzeyde değildir.

### LEVEL 2 — TRACE

Kullanıcı isterse:

> **“Bu sonuç neye dayanıyor?”**

sorusuna iner.

Supporting repositories, evidence references, observable sources, limitations ve uncertainty görünür olabilir.

### LEVEL 3 — INSPECT

Teknik kullanıcı daha derine inmek isterse repository signal detail, evidence detail, methodology, version ve snapshot/reference bilgilerine ulaşabilir.

Amaç ürünün iç kod yapısını öğretmek değil, canonical evidence truth'u denetlenebilir kılmaktır.

---

## 20. Milestone B / P7 Açısından Tasarım Sonucu

### RepresentativeRepository

Sistem anlamı:

> Profile-level örüntüleri incelemek için en açıklayıcı repository.

Kullanıcı sancısı:

> **“Nereden başlamalıyım?”**

Presentation görevi:

> **İlk bakılması gereken projeyi/projeleri görünür hale getirmek.**

### RoleEvidenceAlignment

Sistem anlamı:

> Public evidence ile bounded technical role evidence definition arasındaki örtüşme.

Kullanıcı sancısı:

> **“Bu profil genel olarak hangi teknik yönde üretim yapıyor?”**

Presentation görevi:

> **Dağınık teknik izleri sade bir yön olarak hissettirmek.**

---

## 21. Tasarım Yönü

Tercih edilen karakter:

- güçlü typography
- geniş ve amaçlı whitespace
- net alignment
- az sayıda surface
- ince divider
- semantic color
- progressive disclosure
- tek odaklı ekranlar
- doğal drill-down
- responsive ve accessible davranış

Kaçınılacaklar:

- dashboard görünümü
- çok sayıda KPI kartı
- radar chart
- dekoratif analytics
- score-first presentation
- her şeyi card içine koymak
- ağır gradient
- glassmorphism
- “AI magic” animasyonu
- yoğun navigation
- mod/filtre paneli

---

## 22. Teknoloji Yönü

```text
ASP.NET Core 10
+
Razor Pages / server-rendered HTML
+
Semantic HTML
+
Native CSS / design tokens
+
Minimal vanilla JavaScript
+
SSE gerektiğinde analysis progress / streaming
+
Blazor yalnız interaction complexity gerçekten gerektirdiğinde
```

Ana prensip:

> **Frontend teknolojisi ürünün kullanıcı karmaşıklığını artırmamalıdır.**

Default:

> **HTML first. Server-rendered first. Interaction only where needed.**

---

## 23. Şu Anda Kilitli Olmayan Konular

Henüz kilitlenmeyenler:

- landing headline
- landing subheadline
- input placeholder
- CTA metinleri
- loading/progress copy
- profile result copy
- Technical Focus presentation copy
- Engineering Practice presentation copy
- Problem Area presentation copy
- Role direction copy
- Representative Repository görünen etiketi
- sparse profile copy
- warning / limitation copy
- evidence detail copy
- exact typography scale
- exact spacing
- final colors
- icon system
- final component system
- exact motion
- final responsive composition
- final navigation behavior
- exact progressive disclosure component choice

---

## 24. Doküman Hizalaması — Bilinçli Olarak Ertelendi

Repository içindeki mevcut product/design dokümanları incelenmiştir.

Ancak şu aşamada:

> **Doküman revizyonu veya canonical hizalama yapılmayacaktır.**

Sebep:

- development şu anda Milestone B / P7 aşamasındadır,
- UX direction önce ayrı bir çalışma gerçeği olarak netleştirilmektedir,
- canonical document alignment sonraki aşamada yapılacaktır.

Bu dosya mevcut kararlarımızın çalışma kaydıdır; canonical product/design contracts'ın replacement'ı değildir.

---

## 25. Özet

> **Karmaşıklığı kullanıcıya devretme.**

> **Bir giriş. Bir aksiyon. Sıfır ayar.**

> **Teknolojinin ne yaptığını değil, kullanıcının probleminin nasıl çözüldüğünü göster.**

> **Rapor verme; yön ver.**

> **İlk bakışta açıklama yapma; kullanıcı isterse dayanağını aç.**

> **Basitliği yüzeyde tut, gücü altında sakla.**

> **Derine indir, kaybettirme.**

> **Seçeneği kullanıcıya yükleme; niyetini anla.**

> **Hata mesajı değil, devam yolu göster.**

> **Dağınık teknik izleri tek bir anlaşılır yöne indir.**

> **Kanıt görünür, motor sessiz.**

Amaç:

> **GitHub Proof'un teknik derinliğini azaltmak değil; kullanıcıya taşınan karmaşıklığı azaltmaktır.**
