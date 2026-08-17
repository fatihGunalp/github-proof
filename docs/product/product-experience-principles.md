# GitHub Proof Product Experience Principles

**Status:** PRODUCT EXPERIENCE LOCKED
**Version:** 1
**Session:** 070
**Date:** 2026-08-18

Bu doküman, gelecekteki **Profile P10 — Web Profile Flow**'un uygulaması
gereken, yüzeyler arası ürün deneyimi ilkelerini sahiplenir.

Görsel tasarımı ve implementasyonu sahiplenmez.

---

## 0. Canonical Foundation

Session 069 ürün gerçeği aynen korunur ve bu doküman tarafından
zayıflatılamaz veya yeniden yorumlanamaz:

~~~text
Repository Evidence
!= Human Capability
!= Human Authorship
!= Human Understanding
~~~

~~~text
Evidence gap != skill gap
~~~

> Kanıt sunar, hüküm vermez.
>
> Evidence, not verdicts.

Visible Evidence Score ürünün parçasıdır ve bir developer skoru değildir.

Evidence-backed human verification yalnız gelecek yetenektir; bugün teslim
edilmiş bir çıktı değildir.

Canonical sahibi: `docs/product/product-positioning.md`.

---

## 1. Human-First Product Model

Kilitli sıra:

~~~text
HUMAN
→ PROBLEM
→ EXPERIENCE
→ TECHNOLOGY
~~~

Ters sıra — teknoloji, özellik, ürün, kullanıcı — ürün kararı üretmez.

### Operating principle

Teknik karmaşıklık içeride derin olabilir; ancak kullanıcı sonucu anlamak için
analyzer'ın nasıl çalıştığını anlamak zorunda kalmamalıdır.

### Minimalizm tanımı

Bu dokümanda minimalizm, kullanıcının taşımaması gereken **karar ve yorum
yükünün kaldırılması** anlamına gelir.

Görsel minimalizm bu dokümanın konusu değildir ve burada tanımlanmaz.

---

## 2. Human Problem

Canonical deneyim problemi:

> Public GitHub kullanışlı gözlemlenebilir kanıt taşır; ancak bu kanıtı anlamak
> fazla manuel yorum gerektirir ve kolayca desteklenmeyen sonuçlara götürebilir.

GitHub Proof belirsizliği ortadan kaldırmaz.

GitHub Proof:

- yorum yükünü azaltır,
- gözlemlenebilir kanıtı anlaşılır kılar,
- ürün ifadelerinin neye dayandığını gösterir,
- kanıtın sınırlarını açık hâle getirir.

---

## 3. Before → After

~~~text
BEFORE
GitHub'ı inceliyorum ve anlamını kendim kurmak zorundayım.

AFTER
Hangi kanıtın gözlemlenebilir olduğunu, neye dayandığını ve public GitHub'ın
daha fazlasını kurmak için nerede yetersiz kaldığını açıkça görebiliyorum.
~~~

Bu dönüşüm belirsizliğin kaldırılması değildir. Kullanıcı hâlâ yorum yapar;
fark, yorumun artık kanıtın ve kanıt sınırının üzerine oturmasıdır.

---

## 4. Three Core Experience Questions

Kilitli deneyim semantiği:

~~~text
1. WHAT CAN WE SEE?
2. WHAT IS IT BASED ON?
3. WHERE DOES THE EVIDENCE STOP?
~~~

Bunlar deneyim semantiğidir; zorunlu nihai UI copy'si değildir. Nihai
kullanıcı metni presentation ve P10 sorumluluğundadır.

---

## 5. Experience Architecture

Üç seviye kilitlenir.

### LEVEL 1 — UNDERSTAND

**Amaç**

Kullanıcı, analyzer'ı anlamadan gözlemlenebilir kanıtı anlayabilmelidir.

**Kullanıcı sorusu**

> Bu public GitHub yüzeyi dışarıdan neyi gösteriyor?

**Desteklemesi zorunlu**

- kimlik / bağlam,
- düz dille kanıt özeti,
- en güçlü gözlemlenebilir kanıt,
- önemli public kanıt boşlukları,
- kısa kanıt sınırı ifadesi.

**Bu seviyede öne çıkarılamaz**

- Visible Evidence Score,
- kategoriler,
- tek tek sinyaller,
- exact status taksonomisi,
- analyzer kelime dağarcığı,
- teknik sürümler,
- bağlama özgü kullanıcı aksiyonları.

**Zorunlu sınır**

Public kanıtın bulunamaması; implementasyonun, bilginin, becerinin veya
yetkinliğin bulunmadığını kanıtlamaz ve bu seviyede öyle sunulamaz.

~~~text
missing public evidence
!= missing implementation
!= missing knowledge
!= missing skill
!= missing capability
~~~

LEVEL 1 evrenseldir. İçeriği kullanıcı tipine göre değişmez.

### LEVEL 2 — TRACE

**Amaç**

Kullanıcı, GitHub Proof'un bir ürün ifadesini neden kurduğunu anlayabilmelidir.

**Kullanıcı sorusu**

> Bunu neye dayanarak söylüyorsunuz?

**Desteklemesi zorunlu**

- kanıt referansları,
- gözlemlenebilir kaynaklar,
- kanıt ayrıntıları,
- düz dille availability / uncertainty açıklaması,
- Visible Evidence Score,
- kategori kırılımı.

**Adlandırma sınırı**

TRACE yalnız **evidence traceability** anlamına gelir.

TRACE, gelecekteki human verification ile karıştırılamaz. Kanıtın kaynağına
inmek, kanıtın arkasındaki insan anlayışını doğrulamak değildir.

### LEVEL 3 — INSPECT

**Amaç**

Canonical evidence truth'un teknik derinlikte, bilinçli olarak incelenebilmesi.

**Kullanıcı sorusu**

> Bunu kendim denetleyebilir miyim?

**Açığa çıkarabileceği**

- tek tek 15 sinyal,
- exact status'ler,
- `AnalyzerVersion`,
- `RuleSetVersion`,
- `ReportSchemaVersion`,
- snapshot commit SHA,
- metodoloji,
- desteklenen teknik / export temsilleri.

**Kilitli ifade**

Canonical evidence truth teknik derinlikte incelenebilir; ancak ürün gerçeği
olmayan iç implementasyon ayrıntıları içeride kalır ve yasaklı insan
çıkarımları her seviyede yasaklı kalır.

Bu doküman "hiçbir şey gizlenmez" demez ve böyle bir iddia üretilemez.

---

## 6. First Meaningful Result

Kilitli yapı:

~~~text
BİR kısa düz dil sentezi
+
EN FAZLA ÜÇ kanıt ifadesi
~~~

### Semantik

Sonuç şunları ifade eder:

- public olarak kanıtlanan nedir,
- önemli public kanıtın nerede eksik, erişilemez veya yetersiz olduğu.

Sonuç şunu ifade **etmez**:

> repository'nin göstermediği şeyler

Kanıtın yokluğu, implementasyon / beceri / yetkinlik yokluğuna çevrilemez.
Eksik, erişilemez ve yetersiz kanıt birbirinden ayrı anlamlardır ve bu ayrımın
exact status semantiği `docs/product/scoring-and-report-contract.md`
tarafından sahiplenilir.

### Türetilebilirlik

First Meaningful Result, halihazırda onaylanmış deterministik truth'un bir
**presentation projection**'ıdır.

Şunları üretemez:

- yeni sinyal,
- yeni skor,
- yeni çıkarım,
- authorship iddiası,
- capability iddiası,
- human-understanding iddiası.

### Üretim sınırı

Sentez bir LLM tarafından üretilemez.

UI / view katmanı, localize edilmiş görüntü metinlerinden semantic mapping
mantığı icat edemez. Kullanıcıya görünen hiçbir semantik ifade sunum metni
eşleştirmesinden türetilemez.

### Bilinçli olarak kilitlenmeyen

Bu doküman exact template'leri, slot eşleşmelerini veya projection kurallarını
tanımlamaz.

P10 implementasyonundan önce exact deterministic projection / template
kuralları gerekiyorsa, bunlar ayrıca onaylanmış sınırlı bir presentation
contract'ı veya mevcut bir presentation contract'ının genişletilmesini
gerektirir.

---

## 7. Score Priority

Kilitli karar:

~~~text
Visible Evidence Score = SECONDARY
~~~

Skor ürünün parçası olarak kalır ve semantiği değişmez. 15 sinyal, 4 kategori,
ağırlıklar, normalizasyon, status semantiği ve `RuleSetVersion` bu dokümandan
etkilenmez.

Kilitli deneyim kuralı:

> Kanıtın anlaşılması, skorun yorumlanmasından önce gelir.

Bu doküman skorun hangi component, grafik veya görsel biçimde sunulacağını
belirlemez. Mevcut görsel temsilin korunup korunmayacağı bu dokümanın konusu
değildir ve P10 implementasyon/tasarım sorumluluğundadır.

---

## 8. Shared Truth / Contextual Emphasis

Tek canonical evidence truth vardır.

Evrensel deneyim gerçeği hedef kitleye göre değişmez.

Bağlamsal vurgu ileride farklılaşabilir:

| Bağlam | Vurgulanabilir |
|---|---|
| Repository owner | iyileştirme rehberi anlamlı hâle gelebilir |
| Deneyimli geliştirici | profile / tekrar eden kanıt — yalnız ilgili yetenek gerçekten teslim edildiğinde |
| Recruiter / reviewer | izlenebilirlik ve kanıt sınırları daha güçlü vurgulanabilir |

Yasak:

- ayrı skor,
- ayrı evidence engine,
- ayrı candidate truth,
- ayrı recruiter truth.

Bağlamsal vurgu yalnız sunum önceliğidir; canonical truth'u değiştiremez.

### 8.1 Universal Evidence Layer / Contextual Action Layer

Bu iki katman kesin olarak ayrılır.

**UNIVERSAL EVIDENCE LAYER**

- bütün hedef kitleler arasında ortaktır,
- canonical evidence understanding'i taşır,
- bağlamsal aksiyon gerektirmez ve bağlamsal aksiyona bağımlı olamaz.

Kullanıcı, kendisine bir aksiyon önerilmeden de bu katmanı tek başına
anlayabilmelidir.

**CONTEXTUAL ACTION LAYER**

- kullanıcı bağlamı onu anlamlı kıldığında, ortak evidence truth'unu takip
  edebilir veya ona eşlik edebilir,
- repository owner iyileştirme rehberi alabilir,
- recruiter / reviewer repository-owner aksiyonlarına zorlanamaz,
- ayrı bir analysis truth üretmez.

Kilitli cross-surface kural:

~~~text
contextual actions
!= universal evidence truth
~~~

Bağlamsal aksiyonlar canonical evidence truth'un parçası değildir. Bir
aksiyonun var olması, yok olması veya bağlama göre değişmesi, ortak kanıt
gerçeğini değiştiremez.

### 8.2 Ownership Boundary for Prioritized Actions

Bu doküman yalnız yukarıdaki **cross-surface** kuralı sahiplenir.

Mevcut repository raporuna özgü prioritized-action sunumu — ilk okuma
katmanındaki yeri dâhil — kendi mevcut report ve design contract'larının
sahipliğinde kalır:

- `docs/product/report-information-architecture.md`
- `docs/product/scoring-and-report-contract.md`
- `docs/product/github-proof-design-principles-v1.md`

Bu doküman o sözleşmeleri geçersiz kılmaz, yeniden tanımlamaz ve mevcut
repository raporu davranışının değiştirilmesini gerektirmez.

Somut yüzey ile bu cross-surface ilkelerin uzlaştırılması **P10** tarafından
yapılır. O uzlaştırma gerçekleşene kadar mevcut repository raporu sunumu
geçerliliğini korur.

Bu bir sahiplik açıklamasıdır; bir UI kararı değildir.

---

## 9. Say-No Rules

1. Score-first ürün kimliği.
2. Birinci katmanda analyzer kelime dağarcığı.
3. Dashboard için dashboard.
4. Kullanıcı sorusu olmayan metrik çoğaltma.
5. Candidate ranking.
6. Seniority prediction.
7. AI-written percentage / authorship inference.
8. Kullanıcı sorusu olmayan dekoratif analitik.

Bunlar ilke seviyesindedir. Bu liste kapsamlı bir yasaklı özellik kaydı
değildir ve öyle kullanılamaz.

---

## 10. Relationship to Current UI

Mevcut repository UI, Session 070 sırasında **kanıt olarak** incelenmiştir.

Bu contract Session 070 kapsamında mevcut UI'ın düzeltilmesini gerektirmez.

Session 070'te tespit edilen mevcut UI bulguları, gelecekteki P10 için
**kısıt** hâline gelir; bu dokümanda component seviyesinde düzeltme
tanımlanmaz.

---

## 11. Relationship to P10

Kesin sınır:

> Session 070, P10'a **hangi ürün deneyimi gerçeğini** uygulaması gerektiğini
> söyler.
>
> Session 070, P10'a **nasıl uygulayacağını** söylemez.

P10 daha sonra şunları sahiplenir:

- gerçek Web Profile Flow,
- sayfa / component implementasyonu,
- exact UI copy entegrasyonu,
- localization entegrasyonu,
- responsive davranış,
- accessibility davranışı,
- görsel hiyerarşinin uygulanması,
- mevcut design contract'ları içinde görsel / component tasarımı,
- çalışan UI doğrulaması.

Bu yeteneklerin hiçbiri bugün teslim edilmiş değildir ve bu doküman öyle
olduğunu iddia etmez.

---

## 12. Relationship to Existing Contracts

Bu doküman aşağıdaki alanları sahiplenmez ve tekrar etmez:

| Alan | Canonical sahip |
|---|---|
| Puanlama semantiği | `docs/product/scoring-and-report-contract.md` |
| Sinyal semantiği | `docs/product/analysis-categories-and-signal-boundaries.md`, `docs/architecture/mvp-signal-definitions.md` |
| Repository raporu bölüm yapısı | `docs/product/report-information-architecture.md` |
| Ürün konumlandırması / epistemik sınır | `docs/product/product-positioning.md` |
| Profile evidence semantiği | `docs/product/public-profile-evidence-product-contract.md` |
| Job evidence semantiği | `docs/product/job-evidence-alignment-product-contract.md` |
| Mimari | `docs/architecture/` |
| Görsel tasarım | `docs/product/github-proof-design-principles-v1.md`, `docs/product/github-proof-ui-system-light-dark-v1.md` |
| Roadmap sırası | `docs/roadmap/mvp-phases.md`, `docs/roadmap/deployment-milestones.md` |

Bu doküman yüzeyler arası deneyim gerçeğini taşır ve
`report-information-architecture.md`'nin sahip olduğu repository raporu
bölüm sözleşmesini yeniden sahiplenmez.

Çelişki bulunursa: başka bir kilitli sahibi değiştirmeden **DUR** ve bildir.
