# GitHub Proof — Design Principles v1

> **Durum:** Product/UI/UX tasarım sözleşmesi taslağı  
> **Amaç:** GitHub Proof için ekran, interaction, component, copy ve görsel hiyerarşi kararlarını tek bir tutarlı tasarım ilkeleri sistemi altında toplamak.  
> **Ana motto:** **Evidence first. Noise last.**  
> **Temel soru:** **“Bu repository neyi kanıtlıyor ve kullanıcı ilk neyi düzeltmeli?”**

> **Amended:** 2026-08-20 / Session 095 — açık kullanıcı kararı. Landing artık
> tek birleşik GitHub girişi taşır (username / profile URL / repository URL) ve
> trust yüzeyi "What GitHub Proof is not"tur. Etkilenen bölümler: Principle 13
> component ailesi (`RepositoryInput` → `GitHubInput`), "Ana içerik" landing
> kompozisyonu, "Product components" listesi. Profile Proof ve Repository Proof
> ayrı semantic akışlar olarak kalır; hiçbir evidence, scoring veya inference
> ilkesi değişmemiştir. Kontrat sahibi:
> `docs/product/p10-web-profile-flow-implementation-plan.md` §13.0.

> **Amended:** 2026-08-21 / Session 101 — Web presentation now supports
> exactly EN/TR. Navigation includes a quiet persistent `EN | TR` utility
> alongside theme control. Analysis progress is an immediate truthful
> indeterminate state, not a telemetry-backed completed/active/pending stage
> tracker. The accessibility and localization sections now carry the shared
> P10-I2A Web foundation contract.
> **Session 138 supersedes the analysis-progress sentence in this banner: see
> §7.3's Session 138 note.** Completed/active/pending stage state is now
> authorized within the bounds of
> `docs/architecture/live-analysis-progress-runtime-contract.md` and
> `p10-presentation-language-contract.md` §8.3.1–§8.3.6, and the indeterminate
> state remains the canonical fallback.

> **Amended:** 2026-08-21 / Session 102 — açık kullanıcı kararı. Landing
> boundary composition is now one collapsed native `details` / `summary`
> disclosure. Its concise EN/TR statement remains visible; the same four
> boundary meanings are short text-only items when expanded. The superseded
> four-card/icon presentation is removed. Product boundaries and analysis
> semantics are unchanged.

---

## 1. Neden Bu Belge Var?

GitHub Proof sıradan bir dashboard, GitHub istatistik ekranı veya geliştirici puanlama ürünü değildir.

Ürün iki ilişkili proof surface taşır: Repository Proof tek bir public
repository için deterministic micro view; Profile Proof ise public bir GitHub
profilindeki görünür repository evidence'ını canonical Profile contracts
üzerinden aggregate eden deterministic, job-agnostic macro view üretir.

Bu nedenle tasarımın görevi yalnızca veriyi “güzel göstermek” değildir. Tasarımın görevi:

1. karmaşık repository evidence'ını anlaşılır hale getirmek,
2. önemli olanı ilk birkaç saniyede görünür kılmak,
3. kanıt ile yorum arasındaki sınırı korumak,
4. eksiklik ile belirsizliği birbirine karıştırmamak,
5. score'u ürünün merkezine yerleştirmemek,
6. kullanıcıya ne yapması gerektiğini açıkça göstermek,
7. teknik kullanıcı ile teknik olmayan kullanıcı arasında gereksiz bir dil bariyeri oluşturmamaktır.

Bu belge, Karri Saarinen / Linear yaklaşımından ilham alan bazı prensipleri GitHub Proof'un kendi problem alanına dönüştürür. Amaç Linear'ı görsel olarak kopyalamak değildir. Amaç; **bilgi yoğunluğunu korurken görsel ve bilişsel gürültüyü azaltan, hızlı, sakin ve açıklanabilir bir ürün deneyimi** oluşturmaktır.

---

# 2. Tasarımın Ürün Sözleşmesine Bağlılığı

UI ürün contract'ını yeniden tanımlayamaz.

Canonical ürün belgeleri tasarımdan üstündür. Bu belge aşağıdaki mevcut ürün gerçeklerini korur:

- **Repository Proof**, tek bir public repository için deterministic micro
  view'dur; mevcut 15-signal / 4-category contract'ı, repository
  score/categories/proves/weakens/do-first semantics'i yalnız bu surface'e
  aittir.
- **Profile Proof**, public profile üzerindeki visible repository evidence'ını
  canonical Profile contracts üzerinden aggregate eden deterministic,
  job-agnostic macro view'dur; Repository Proof score'unu developer/profile
  score'una dönüştürmez.
- Her iki surface yalnız public ve dışarıdan gözlemlenebilir evidence/traces
  üzerinden çalışır.
- Her iki surface deterministiktir.
- LLM veya serbest metin yorumu doğrudan scoring gerçeği oluşturmaz.
- Repository Proof'ta 15 canonical signal ve 4 canonical kategori vardır.
- Repository Proof ilk katmanı en fazla 3 güçlü sinyal, 3 kritik boşluk ve 3
  öncelikli aksiyonu öne çıkarır.
- Repository Proof overall score'u vardır ancak ürünün birincil anlamı değildir
  ve Profile Proof'a taşınmaz.
- `Missing`, `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` birbirinden farklı semantik durumlardır.
- Hiçbir surface seniority, employability, role, AI authorship, ownership,
  personality veya unsupported capability inference üretmez.

Canonical referanslar:

```text
docs/product/mvp-product-contract.md
docs/product/scoring-and-report-contract.md
docs/product/analysis-categories-and-signal-boundaries.md
docs/architecture/mvp-signal-definitions.md
docs/product/sample-analysis-report.md
docs/roadmap/mvp-phases.md
```

## 2.1 Current Unified Entry and Proof-Surface Routing

Current Web contract tek bir unified input kullanır:

```text
GitHub username veya canonical profile URL
→ Profile Proof (macro)

canonical repository URL
→ Repository Proof (micro)
```

Giriş birleşiktir; analiz semantics'i değildir. Routing input shape'ten
deterministically seçilir; AI/intent guess veya existence-discovery GitHub
çağrısı yapılmaz. Profile Proof ve Repository Proof kendi canonical
pipeline/report sınırlarını korur.

Historical note: bu belgenin ilk v1 taslağı username → repository discovery →
single-repository analysis akışını proposed extension olarak kaydetmişti.
Session 095 unified entry kararı ve tamamlanan P10-I2 bu öneriyi current Web
contract olarak supersede etti; ayrı repository picker akışı current entry
contract'ı değildir.

---

# 3. GitHub Proof'un Tasarım Tezi

GitHub Proof'un tasarım problemi “çok bilgiyi az göstermek” değildir.

Asıl problem:

> **Çok bilgiyi, önem sırasını bozmadan ve kullanıcıyı boğmadan göstermek.**

Bu yüzden minimalizm tanımımız:

> **Minimalizm = az veri değil; doğru zamanda, doğru seviyede, doğru veri.**

GitHub Proof bir bilgi azaltma ürünü değil; **bilgi hiyerarşisi ürünüdür.**

---

# 4. Ana Tasarım İlkesi

## Evidence First. Noise Last.

Her tasarım kararında önce evidence görünür olmalıdır.

Aşağıdaki `Action → Score` katmanları Repository Proof micro report'una
aittir. Profile Proof macro view score üretmez; kendi canonical meaning →
direction → starting point → evidence sırasını kullanır.

Öncelik sırası:

```text
Evidence
↓
Meaning
↓
Action
↓
Score
```

Asla:

```text
Score
↓
Grafikler
↓
Kartlar
↓
Evidence
```

olmamalıdır.

Score bir özet mekanizmasıdır. Evidence ürünün gerçeğidir.

---

# 5. GitHub Proof Design Principles

## Principle 01 — Her ekran yalnızca bir ana soruyu cevaplar

Bir ekranın birincil görevi tek cümleyle ifade edilemiyorsa tasarım henüz yeterince sade değildir.

### Ana sayfa

```text
Hangi GitHub kullanıcısını veya repository'yi incelemek istiyorsun?
```

### Profile Proof

```text
Bu public GitHub profilinde hangi görünür teknik üretim örüntüleri tekrarlanıyor?
```

### Analysis state

```text
Şu anda ne oluyor?
```

### Repository Proof report

```text
Bu repository neyi kanıtlıyor ve ilk neyi düzeltmeliyim?
```

### Signal detail

```text
Bu sonuca neden vardık?
```

### Methodology

```text
GitHub Proof bu sonucu hangi kurallarla üretti?
```

Bir ekran birden fazla ana soruyu aynı anda cevaplamaya çalışıyorsa progressive disclosure kullanılmalıdır.

---

## Principle 02 — Bilgiyi azaltma, gürültüyü azalt

Repository Proof doğal olarak bilgi yoğun bir micro view'dur:

- repository identity,
- 15 signal,
- 4 category,
- evidence,
- status,
- score,
- uncertainty,
- recommendation,
- limitations,
- version metadata.

Bunları silmek yanlış olur.

Bu 15-signal / 4-category / score katmanları Profile Proof'a taşınmaz.
Profile Proof kendi canonical macro-report meaning/evidence yapısını kullanır.

Doğru yaklaşım katmanlamaktır.

### Katman 1 — İlk bakış

```text
What this repository proves     max 3
What weakens the evidence       max 3
Do these first                  max 3
```

### Katman 2 — Diagnostic summary

```text
Explicit uncertainty
Category summary
Secondary overall score
```

### Katman 3 — Deep inspection

```text
15 signal detail
Evidence
Why it matters
Recommendation
Limitations
Version / methodology
```

Bilgi miktarı korunur; aynı anda görünür olan bilgi miktarı azaltılır.

---

## Principle 03 — İlk 10 saniye kritik

Report ekranı açıldıktan sonra kullanıcı ilk 10 saniyede şu üç soruyu cevaplayabilmelidir:

1. Bu repository neyi iyi kanıtlıyor?
2. Kanıtı en çok ne zayıflatıyor?
3. İlk hangi işi yapmalıyım?

Kullanıcının bu üç cevabı bulması için category tablosunu, 15 signal'ı veya methodology metnini okuması gerekiyorsa hierarchy başarısızdır.

---

## Principle 04 — Kullanıcı önce tarar, sonra okur

İlk reading mode “okuma” değil “tarama”dır.

Bu nedenle first-reading layer:

- kısa section title,
- kısa item title,
- tek cümle supporting copy,
- güçlü alignment,
- anlamlı whitespace,
- minimum visual chrome

kullanmalıdır.

Uzun açıklama detail layer'a taşınmalıdır.

---

## Principle 05 — Her iddia açıklanabilir olmalı

GitHub Proof'un temel güven modeli:

```text
Result
↓
Observed evidence
↓
Why it matters
↓
Viewer consequence
↓
Recommended action
↓
Evidence reference
```

### Örnek

```text
Usage / Demo
PARTIAL

Observed evidence
README proje açıklaması içeriyor ancak somut kullanım örneği gözlemlenmedi.

Why it matters
Repository'yi ilk kez açan kişi projenin nasıl kullanıldığını hızlıca doğrulayamıyor.

Recommended action
README'ye gerçek bir kullanım örneği ve beklenen çıktıyı ekle.

Evidence
README.md
```

UI sonucu kanıttan koparamaz.

---

## Principle 06 — Interface mümkün olduğunca kendini açıklamalı

Kullanıcıya uzun kullanım talimatları vermek yerine interface doğru davranışı yönlendirmelidir.

Current unified entry tek input kullanır:

```text
GitHub username, profile URL veya repository URL
```

Davranış:

```text
username veya profile URL girildi
→ Profile Proof

repository URL girildi
→ Repository Proof
```

Kullanıcının bunu üç ayrı açıklama paragrafından öğrenmesi gerekmemelidir.

---

## Principle 07 — Progressive disclosure varsayılandır

Detay, kullanıcı istemeden bağırmamalıdır.

### Collapsed signal

```text
Usage / Demo             PARTIAL        4 / 7
Somut kullanım örneği gözlemlenmedi.          →
```

### Expanded signal

```text
Observed evidence
Why it matters
Recommended action
Evidence
Limitations
Signal version
```

Bu yaklaşım:

- signal detail,
- category explanation,
- uncertainty,
- methodology,
- metadata

alanlarında kullanılmalıdır.

---

## Principle 08 — Renk dekorasyon değil anlamdır

Renk yalnızca semantic state veya primary action gibi gerçekten anlam taşıdığı yerlerde kullanılmalıdır.

Önerilen semantic model:

```text
Strong               success / green
Present              positive / green-neutral
Partial              warning / amber
Missing              danger / red
NotApplicable        neutral
UnavailableEvidence  neutral-warning
RequiresHumanReview  review / violet
```

Ancak hiçbir state yalnızca renkle ifade edilmemelidir.

Her state en az iki kanalla anlaşılmalıdır:

```text
color + text
color + icon
icon + label
```

Accessibility nedeniyle kırmızı/yeşil ayrımı tek başına anlam taşımamalıdır.

---

## Principle 09 — Sessiz yüzey, güçlü hierarchy

GitHub Proof kendisini “modern SaaS template” gibi sergilememelidir.

Interface geri çekilmelidir; evidence öne çıkmalıdır.

### Tercih edilen görsel karakter

- geniş ve amaçlı whitespace,
- güçlü typography,
- az sayıda surface,
- ince divider,
- minimum shadow,
- sınırlı radius,
- tek ana accent,
- küçük ve ölçülü status göstergeleri,
- çok net grid/alignment,
- minimum dekoratif illustration,
- yalnız gerektiğinde gradient.

### Kaçınılacak karakter

- her yerde card,
- neon accent,
- büyük dashboard gauge,
- gereksiz chart,
- glassmorphism gösterisi,
- çoklu gradient,
- terminal cosplay,
- yapay “AI magic” animasyonu,
- her section için ayrı arka plan rengi.

---

## Principle 10 — Card varsayılan container değildir

Bir bilgi bloğu görsel olarak ayrılmak zorunda olduğu için card yapılmamalıdır.

Önce şu araçlar düşünülmelidir:

1. whitespace,
2. typography,
3. alignment,
4. divider,
5. subtle surface,
6. en son card.

Card yalnızca içerik gerçekten bağımsız bir nesne veya interaction surface ise kullanılmalıdır.

---

## Principle 11 — Speed bir tasarım özelliğidir

Hız yalnız backend latency değildir.

Bilişsel hız da tasarımın parçasıdır.

Kullanıcı en kısa yoldan hedefine ulaşmalıdır:

```text
Input
→ Repository
→ Analyze
→ Report
```

Gereksiz:

- wizard,
- onboarding carousel,
- modal zinciri,
- hesap oluşturma zorunluluğu,
- gereksiz confirmation,
- “AI hazırlanıyor” ekranları

MVP'de kullanılmamalıdır.

---

## Principle 12 — Uncertainty hata değildir

Aşağıdaki durumlar semantic olarak farklıdır:

### Missing

```text
Gerekli evidence başarıyla incelendi fakat kanıt bulunamadı.
Score = 0
Denominator'a dahil.
```

### NotApplicable

```text
Signal repository bağlamında uygulanabilir değil.
Score = null
```

### UnavailableEvidence

```text
Evidence güvenilir şekilde alınamadı veya incelenemedi.
Score = null
```

### RequiresHumanReview

```text
Otomasyon güvenilir karar veremedi.
Score = null
```

UI bunların hepsini “problem” veya “failed” şeklinde göstermemelidir.

Özellikle:

> **Unavailable ≠ Missing**

Bu ayrım GitHub Proof'un güvenilirliğinin parçasıdır.

---

## Principle 13 — Component domain kavramını temsil etmeli

Generic design-system adları yerine product semantics tercih edilmelidir.

### Temel component ailesi

```text
GitHubInput
RepositoryIdentity
ProfileRepositoryList
RepositoryEvidenceRow
AnalysisProgress
ProofItem
EvidenceGapItem
PriorityAction
SignalRow
SignalDetail
EvidenceBlock
StatusBadge
UncertaintyNotice
CategorySummary
SecondaryScore
BoundaryDisclaimer
MetadataItem
```

Bu yapı UI ile domain'in aynı dili konuşmasını sağlar.

---

## Principle 14 — Görsel şıklık son aşamadır

Her yeni UI fikrinde karar sırası:

```text
1. Bu bilgi gerekli mi?
2. Bu ekranda gerekli mi?
3. Şimdi görünmesi gerekli mi?
4. Kullanıcı bunu anlayabiliyor mu?
5. Hangi hierarchy seviyesinde olmalı?
6. Hangi interaction modelinde olmalı?
7. Ancak sonra: nasıl görünmeli?
```

Bir ekran güzel olduğu için doğru değildir.

---

## Principle 15 — Score psikolojik merkez değildir

Overall score yalnız Repository Proof MVP contract'ında vardır ve kaldırılmaz.
Profile Proof score üretmez; repository score'u developer/profile score'una
dönüştürmez.

Ancak tasarımda score:

- hero olmaz,
- büyük gauge olmaz,
- ürün logosundan sonra ilk görünen sayı olmaz,
- developer rating gibi görünmez.

Tercih:

```text
Visible evidence · 71 / 100
Secondary diagnostic summary
```

Score'un hemen yakınında veya methodology/boundary alanında şu anlam korunmalıdır:

> Bu skor yalnız repository içerisinde gözlemlenebilen ve applicable olan MVP sinyallerinin özetidir; geliştiricinin genel yetkinliği, kıdemi veya işe alınabilirliği değildir.

---

## Principle 16 — Birincil eylem her zaman açıktır

Her ekranda tek bir primary action bulunmalıdır.

Örnek:

### Home

```text
Continue / Analyze
```

### Profile repository evidence exploration

```text
Inspect repository evidence
```

Bu action entry routing değildir. Profile Proof içindeki repository evidence
exploration / drill-down'a aittir; final link mechanics P10-I4'ün
sorumluluğudur ve bu design contract amendment onları implement etmez.

### Report

Primary action zorunlu değildir; rapor özü zaten sonuçtur.

Secondary action:

```text
Copy Markdown
```

Aynı ağırlıkta 3–4 CTA gösterilmez.

---

## Principle 17 — Literal evidence çevrilmez ve süslenmez

Localization presentation copy'yi değiştirir; repository truth'u değiştirmez.

Aşağıdaki literal evidence olduğu gibi korunmalıdır:

- file path,
- URL,
- branch,
- commit SHA,
- tag,
- command,
- code,
- raw repository title,
- raw evidence value.

Örneğin:

```text
README.md
.github/workflows/ci.yml
main
8ab24f1
```

Türkçeleştirilmez.

---

## Principle 18 — Trust, polish'ten önce gelir

GitHub Proof'un güven üretmesi için UI şu özellikleri desteklemelidir:

- evidence referansları görünür,
- signal status açık,
- uncertainty dürüst,
- analysis commit SHA görünür,
- snapshot zamanı erişilebilir,
- methodology ulaşılabilir,
- limitation saklanmaz,
- boundary disclaimer kaybolmaz.

UI “kesinlik hissi” yaratmak için belirsizliği gizlememelidir.

---

# 6. Canonical Information Hierarchy

Report için önerilen evidence-first sıra:

```text
1. Repository identity / context
2. What this repository proves
3. What weakens the evidence
4. Do these first
5. Explicit uncertainty
6. Category summary + secondary score
7. Signal detail
8. Limitations / versions / methodology / boundary disclaimer
```

Bu sıra tasarımın temel navigation modelidir.

---

# 7. Ekran Bazlı Tasarım İlkeleri

## 7.1 Home

### Ana soru

```text
Hangi GitHub kullanıcısını veya repository'yi incelemek istiyorsun?
```

### Ana içerik

Tercih edilen minimum yapı:

```text
GitHub Proof

See what your GitHub communicates.

Public evidence. Clear direction. No guesses.

[ GitHub username, profile URL, or repository URL        ]

                    [ Analyze ]

────────────────────────────────────────────────────────

GitHub Proof doesn't rank developers or judge seniority, AI authorship,
or hiring.
View boundaries →

  (expanded)
  - No developer ranking
  - No seniority judgment
  - No AI authorship claims
  - No hire/no-hire verdict
```

Kilitli landing yapısı (Session 095, açık kullanıcı kararı):

- **Tek birleşik giriş.** Bir tane input alanı vardır ve üç biçimi kabul
  eder: bare GitHub login, canonical profile URL, canonical repository URL.
  Ayrı bir "profile girişi" / "repository girişi" yoktur.
- **Analiz semantics'i ayrı kalır.** Profile Proof (macro) ve Repository
  Proof (micro) iki ayrı akıştır; giriş birleşiktir, analiz değil. Hangi
  akışın çalışacağı gönderilen string'in biçiminden deterministik olarak
  belirlenir — tahmin, AI/intent sınıflandırması veya "hangisi acaba" diye
  GitHub'a sorma yoktur. Çözülemeyen biçim giriş seviyesinde validation
  error olur.
- **Trust/boundary yüzeyi tek bir progressive disclosure'dır.** Collapsed
  native `details` / `summary`, ürünün geliştiricileri sıralamadığını; kıdem,
  AI yazarlığı veya işe alım hükmü vermediğini kısa EN/TR cümleyle görünür
  tutar. `View boundaries →` / `Sınırları gör →` ile açıldığında no ranking,
  no seniority judgment, no AI authorship claim ve no hire/no-hire verdict
  anlamları dört kısa text-only madde olarak görünür. İkon, card/column veya
  JavaScript disclosure yoktur; Analyze tek primary CTA olarak kalır.

### İlk viewport'ta olmaması tercih edilenler

- büyük report preview,
- feature card / yetenek pazarlaması,
- hedef kitle kartları,
- pricing,
- testimonial,
- uzun methodology,
- büyük score mockup,
- karmaşık navigation.

Not: Landing boundary bu listeye girmez. Collapsed cümle ilk ekranda kalır;
dört kısa anlam yalnız kullanıcı native disclosure'ı açtığında görünür.
Bu yüzey feature grid'i değil, boundary beyanıdır ve görsel olarak primary
giriş/Analyze görevine ikincildir.

### Navigation

MVP için mümkün olan en az öğe:

```text
Methodology
GitHub
EN | TR          (quiet persistent language utility)
Theme            (quiet utility)
```

Gerekli olduğunda `Example Report` eklenebilir; ancak first action ile
yarışmamalıdır. Language ve theme controls navigation destination veya primary
CTA değildir. `EN | TR` current selection'ı perceivable, keyboard-accessible
ve screen-reader-accessible yapar; explicit choice persist edilir.

---

## 7.2 Unified Entry — Current Contract

Current landing bir username/profile URL'yi Profile Proof'a, canonical
repository URL'yi Repository Proof'a route eder. Ayrı repository discovery
veya repository picker entry step'i yoktur.

```text
fatihGunalp                         → Profile Proof
https://github.com/fatihGunalp      → Profile Proof
https://github.com/owner/repository → Repository Proof
```

Profile result içindeki later evidence drill-down, canonical Profile report
contract'ının ayrı sorumluluğudur; unified entry'yi repository selection
flow'una dönüştürmez.

### Repository-specific listelerde gösterilebilecek düşük riskli metadata

- repository name,
- primary language,
- updated date,
- fork state,
- archived state.

### Gösterilmemesi gereken çıkarımlar

- “Best repo”,
- “Most impressive”,
- “Interview-worthy”,
- “Strongest project”,

çünkü bunlar yeni bir ranking/selection contract'ı gerektirir.

---

## 7.3 Analysis Progress

> **Session 138 notu bölümün sonundadır ve önce okunmalıdır.** Aşağıdaki
> `Mevcut synchronous POST runtime backend stage telemetry üretmediği için`
> premisi artık güncel değildir.

Amaç uygulamanın request üzerinde çalıştığını derhal ve dürüstçe
göstermektir. Mevcut synchronous POST runtime backend stage telemetry
üretmediği için UI indeterminate kalır.

Primary status örneği:

```text
GitHub Proof is processing your request…
GitHub Proof isteğinizi işliyor…
```

Optional neutral explanation `What this analysis includes` / `Bu analiz
neleri içerir` altında target validation, public-data reading, visible-evidence
collection, trace analysis ve view preparation genel olarak anlatılabilir.
Bu satırlar individually tracked sequential runtime states değildir.

Şunlar runtime telemetry açıkça kanıtlamadıkça yasaktır:

- completed / active / pending stage,
- checkmark veya progress position,
- percentage veya ETA,
- SSE, SignalR, polling, streaming response ya da sırf UI için yeni backend
  telemetry.

Processing state tek unified input'ta native constraint validation başarılı
olduktan sonra başlar; duplicate submit engellenir ve `GitHubTarget` form
serialization'dan çıkaracak biçimde disabled yapılmaz. Back/BFCache dönüşü
stale busy/disabled state'i temizler. Animation varsa düşük yoğunluklu olmalı
ve `prefers-reduced-motion`'a uymalıdır.

Kaçınılacak:

```text
AI is thinking...
Magic is happening...
Understanding your engineering style...
```

Bunlar ürün contract'ını yanlış temsil eder.

**Session 138 notu.** Yukarıdaki *"Mevcut synchronous POST runtime backend stage
telemetry üretmediği için UI indeterminate kalır"* premisi artık güncel
değildir. Session 138, bu bölümün `runtime telemetry açıkça kanıtlamadıkça`
koşulunu karşılayan sözleşmeyi üretti:
`docs/architecture/live-analysis-progress-runtime-contract.md` (runtime
semantics) ve `p10-presentation-language-contract.md` §8.3.1–§8.3.6 (language ve
shape). Yalnız o iki owner'ın authorize ettiği kadarıyla completed/active/pending
stage state ve read-only bounded polling serbesttir; percentage, ETA, progress
position, SSE, SignalR ve streaming response yasak kalır, ve indeterminate state
her koşulda canonical fallback'tir. Yukarıdaki `checkmark` yasağı
**server'dan gelmeyen** bir checkmark'ı kastediyordu; `COMPLETED` state'i için
checkmark glyph'inin tek sahibi artık p10 §8.3.3'tür.

---

## 7.4 Report — First Reading

İlk viewport mümkün olduğunca şu üç bölümü görünür hale getirmelidir:

```text
WHAT THIS REPOSITORY PROVES

WHAT WEAKENS THE EVIDENCE

DO THESE FIRST
```

Her bölüm max 3 item.

### ProofItem anatomy

```text
Title
Short evidence-backed statement
Optional evidence reference
```

### GapItem anatomy

```text
Title
Observed weakness
Why it matters — short
```

### PriorityAction anatomy

```text
Priority number
Action title
Reason
Effort — only if contract/projection supports it
Related signal/evidence — optional detail
```

---

## 7.5 Explicit Uncertainty

Uncertainty sadece varsa gösterilir.

UI kuralı:

```text
No uncertainty
→ section yok

Uncertainty exists
→ compact notice / section
```

Uncertainty first-reading hierarchy'yi boğmamalı ancak saklanmamalıdır.

---

## 7.6 Category Summary

Canonical kategori isimleri domain seviyesinde:

```text
RepositoryUnderstandability
DevelopmentProcessVisibility
ValidationAndUsability
PortfolioPresentation
```

Presentation labels daha anlaşılır olabilir:

```text
Documentation & Context
Development Process
Validation
Portfolio Presentation
```

Ayrı bir `Collaboration` kategorisi oluşturulmaz.

Category summary score-first dashboard gibi görünmemelidir.

Tercih edilen model:

```text
Documentation & Context       78
Development Process           64
Validation                    86
Portfolio Presentation        52
```

Chart zorunlu değildir.

---

## 7.7 Signal List

15 signal canonical order ile gösterilmelidir.

Her row minimum:

```text
Signal title
Semantic status
Score — varsa
Disclosure affordance
```

Secondary:

```text
signal key
```

Detail açıldığında evidence ve explanation görünür.

---

## 7.8 Signal Detail

Önerilen içerik sırası:

```text
Signal title
Status
Score / max — varsa
Overall inclusion — gerektiğinde

Observed evidence
Why it matters
Recommended action
Evidence references
Limitations
Version / evaluated time
```

Signal detail ürünün açıklanabilirlik katmanıdır.

---

## 7.9 Methodology & Boundary

Kolay ulaşılabilir ancak first-reading layer'da baskın olmamalıdır.

Minimum boundary:

GitHub Proof şunları belirlemez:

- developer seniority,
- employability,
- authorship/originality,
- AI usage,
- general developer quality,
- production readiness,
- complete security/code-quality verdict.

---

# 8. Canonical Categories and Signals — UI Reference

## Repository Understandability

```text
repository.readme.exists
documentation.project-description / repository.description.present
documentation.installation-guide
repository.license.present
documentation.usage-or-demo
```

> Not: Exact canonical keys implementation/docs source of truth'tan alınmalıdır; UI hardcode edilmiş eski key'lere güvenmemelidir.

## Development Process Visibility

```text
process.commit-history
process.commit-distribution
process.issue-usage
process.pull-request-usage
process.release-usage
```

## Validation and Usability

```text
validation.test-structure
validation.continuous-integration
```

## Portfolio Presentation

```text
portfolio.topics
portfolio.homepage-or-live-demo
portfolio.presentation-completeness
```

`portfolio.presentation-completeness` kategori içinde görünür ancak overall score'da double-count edilmez.

---

# 9. Status UX Contract

Canonical states:

```text
Strong
Present
Partial
Missing
NotApplicable
UnavailableEvidence
RequiresHumanReview
```

## Strong

Anlam: güçlü görünür kanıt.

UI:

- positive icon,
- semantic label,
- sakin green accent.

## Present

Anlam: kanıt mevcut fakat Strong seviyesinde değil.

UI Strong'dan daha düşük görsel vurgu taşımalıdır.

## Partial

Anlam: bazı kanıtlar mevcut ancak eksik/sınırlı.

UI:

- amber,
- “warning” hissi ama error hissi değil.

## Missing

Anlam: veri başarıyla incelendi, kanıt yok.

UI:

- red/danger semantic,
- score 0 olabilir,
- gap/action için aday olabilir.

## NotApplicable

Anlam: repository bağlamında uygulanabilir değil.

UI:

- neutral,
- hata değildir,
- recommendation üretilmez.

## UnavailableEvidence

Anlam: kontrol tamamlanamadı.

UI:

- neutral-warning,
- missing gibi görünmez,
- kullanıcıya uncertainty nedenini açıklar.

## RequiresHumanReview

Anlam: otomasyon güvenilir hüküm veremedi.

UI:

- ayrı review semantic,
- insan kararına ihtiyaç olduğunu açıklar,
- otomatik failure değildir.

---

# 10. Typography Principles

Font seçimi brand çalışmasında ayrıca kilitlenebilir. Ancak typography davranışı şimdiden tanımlanabilir.

## Hiyerarşi

### Display

Yalnız ana değer önerisi veya report'un birincil sorusu.

### H1

Screen title.

### H2

Major report section.

### H3

Signal/action/group title.

### Body

Evidence explanation.

### Meta

SHA, branch, timestamp, signal key, version.

## Kurallar

- Typography hierarchy surface hierarchy'den güçlü olmalıdır.
- Çok fazla font weight kullanılmaz.
- All-caps yalnız kısa section labels için dikkatli kullanılabilir.
- Uzun technical text küçük fonta zorlanmaz.
- Meta text sessizdir ama okunamaz hale gelmez.

---

# 11. Spacing and Layout Principles

GitHub Proof spacing sistemi dekoratif ferahlık için değil reading rhythm için kullanılır.

## Temel kurallar

- related content yakın,
- unrelated content belirgin uzak,
- section separation card border yerine whitespace ile çözülebilir,
- detail view'da line length kontrol edilir,
- desktop rapor tek dev dashboard grid'ine dönüşmez.

## Desktop

Önerilen design reference width:

```text
1440 px
```

Ancak content maksimum genişliği kontrollü tutulmalıdır.

## Mobile

Primary reference:

```text
390 px
```

Mobile'da sıra korunur:

```text
Proves
↓
Weakens
↓
Actions
↓
Uncertainty
↓
Categories
↓
Signals
```

Desktop iki/üç kolon olduğu için mobile'da semantic sıra değişmemelidir.

---

# 12. Interaction Principles

## Keyboard first uyumu

Developer-oriented ürün olduğu için temel flow keyboard ile kullanılabilmelidir.

Örnek:

```text
Input
Enter
Repo select
Enter
Report
```

## Hover

Hover bilgi taşıyan tek kanal değildir.

## Focus

Focus state belirgin ve erişilebilirdir.

## Expand / collapse

Signal detail için doğal interaction olabilir.

## Copy

Markdown/report copy secondary action olarak erişilebilir olabilir.

## External links

Repository, evidence URL veya GitHub linkleri external behavior'ı açıkça göstermelidir.

---

# 13. Motion Principles

Motion yalnızca üç durumda kullanılmalıdır:

1. state transition açıklamak,
2. spatial continuity korumak,
3. interaction feedback vermek.

Kullanılmamalıdır:

- dekoratif hero animation,
- particle background,
- fake AI thinking,
- sürekli moving chart,
- attention-grabbing pulse.

Motion hızlı, düşük genlikli ve gerektiğinde `prefers-reduced-motion` ile kapatılabilir olmalıdır.

---

# 14. Copy Principles

## Evidence language

Kesin olmayan şeyi kesin söyleme.

Yanlış:

```text
This developer writes good tests.
```

Doğru:

```text
A visible test structure was observed.
```

Yanlış:

```text
This is a production-ready project.
```

Doğru:

```text
Build/test automation evidence is visible.
```

## Recommendation language

Generic checklist yerine consequence-aware copy tercih edilir.

Zayıf:

```text
README eksik.
```

Daha iyi:

```text
Kurulum adımları görünür değil; repository'yi ilk kez açan biri projeyi hızlıca çalıştıramayabilir.
```

Aksiyon:

```text
README'ye minimum kurulum adımlarını ve ilk çalıştırma komutunu ekle.
```

## Tone

- sakin,
- açıklayıcı,
- yargılayıcı olmayan,
- teknik olarak net,
- recruiter için de anlaşılır,
- pazarlama abartısından uzak.

---

# 15. Accessibility Principles

GitHub Proof accessibility'yi polish sonrası yapılacak iş olarak görmez.

Minimum gereklilikler:

- semantic HTML,
- keyboard navigation,
- visible focus,
- yeterli contrast,
- state yalnız renkle ifade edilmez,
- icon'ların text equivalent'i vardır,
- expandable content ARIA semantics taşır,
- mobile touch target yeterlidir,
- reduced motion desteklenir,
- heading hierarchy anlamlıdır,
- request işlenirken ilgili form/container `aria-busy="true"` taşır,
- visible processing status `role="status"`, `aria-live="polite"` ve
  `aria-atomic="true"` kullanır,
- duplicate-submit prevention keyboard ve screen-reader interaction'ı bozmaz,
- language selection current choice'i yalnız renkle anlatmaz ve existing
  target/focus contract'ına uyar.

---

# 16. Localization Principles

Web presentation exactly English (`en`) ve Turkish (`tr`) destekler.

## Kurallar

- component width yalnız İngilizce kısa metne göre tasarlanmaz,
- Auto-layout / fluid layout kullanılır,
- semantic IDs aynı kalır,
- locale değişimi analysis'i yeniden çalıştırmaz,
- evidence literal'ları çevrilmez,
- explicit persistent user choice browser preference'tan önce gelir,
- browser `Accept-Language` supported family/q ordering ile çözülür,
- supported preference yoksa English fallback kullanılır,
- geolocation, IP/device location ve browser Geolocation API kullanılmaz,
- selected request culture bütün user-facing Web copy ve accessibility
  mesajlarını sürer.

Türkçe daha uzun olduğu için responsive test özellikle TR copy ile yapılmalıdır.

---

# 17. Error and Edge-State Design

Minimum tasarlanması gereken durumlar:

```text
Invalid repository URL
Repository not found
Private repository
Rate limited
Insufficient data
Partial analysis
Archived repository
Fork repository
Template repository
```

## Kural

`Partial analysis` tam error page değildir.

Elde bulunan evidence raporlanır; uncertainty ayrıca gösterilir.

## Private repository

MVP private repository analiz etmiyorsa UI bunu net şekilde sınır olarak açıklar; login/OAuth çağrısı uydurmaz.

---

# 18. Component Architecture

## Foundation

```text
Color
Typography
Spacing
Radius
Border
Surface
Motion
State
```

## Product components

```text
GitHubInput
ProfileRepositoryList
RepositoryEvidenceRow
RepositoryIdentity
AnalysisProgress
ProofItem
GapItem
PriorityAction
UncertaintyNotice
CategorySummary
SecondaryScore
SignalRow
SignalDetail
EvidenceBlock
StatusBadge
MetadataItem
BoundaryDisclaimer
```

## Component rule

Her component şu soruları cevaplamalıdır:

1. Hangi product concept'i temsil ediyor?
2. Hangi semantic states'i var?
3. Hangi data zorunlu?
4. Hangi data optional?
5. Loading state var mı?
6. Error/uncertainty state var mı?
7. Mobile davranışı ne?
8. Accessibility davranışı ne?

---

# 19. Anti-Patterns

Aşağıdaki kalıplar bilinçli olarak kaçınılacak tasarım davranışlarıdır.

## 19.1 Giant score hero

```text
        87
      / 100
```

Ürünü developer rating aracına benzetir.

## 19.2 Card soup

Her signal, metric, metadata ve action'ı ayrı kart yapmak görsel gürültüyü artırır.

## 19.3 Dashboard cosplay

Grafik gerektiği için değil dashboard gibi görünmek için chart kullanmak yasaktır.

## 19.4 AI theatre

Ürün deterministic analiz yaparken “AI intelligence” görsel dili kullanmak yanlış mental model üretir.

## 19.5 Recruiter judgment copy

```text
Hire
Reject
Interview-worthy
Senior-level repo
```

MVP boundary dışıdır.

## 19.6 Hidden uncertainty

Başarılı görünmek için unavailable/human-review state'lerini gizlemek güveni bozar.

## 19.7 Marketing-first home

Ana sayfada primary input'tan önce onlarca feature, logo, testimonial veya audience section göstermek ana görevi geciktirir.

## 19.8 Excessive brand decoration

Brand görünürlüğü evidence readability'nin önüne geçemez.

---

# 20. Design Review Checklist

Her mockup/review'da aşağıdaki checklist uygulanır.

## Product alignment

- [ ] Ekran tek ana soruyu cevaplıyor mu?
- [ ] UI canonical product boundary'yi bozuyor mu?
- [ ] Developer rating hissi yaratıyor mu?
- [ ] AI usage/authorship gibi unsupported bir çıkarım var mı?

## Hierarchy

- [ ] Evidence score'dan önce mi?
- [ ] İlk bakışta en önemli 3 proof/gap/action bulunabiliyor mu?
- [ ] Secondary metadata sessiz mi?
- [ ] Gereksiz visual surface var mı?

## Explainability

- [ ] Sonuç evidence'a kadar izlenebiliyor mu?
- [ ] “Why it matters” anlaşılır mı?
- [ ] Recommendation somut mu?

## Uncertainty

- [ ] Missing ve unavailable ayırt ediliyor mu?
- [ ] NotApplicable hata gibi görünmüyor mu?
- [ ] RequiresHumanReview ayrı semantic taşıyor mu?

## Visual noise

- [ ] Gereksiz card var mı?
- [ ] Gereksiz border var mı?
- [ ] Gereksiz shadow var mı?
- [ ] Gereksiz icon var mı?
- [ ] Gereksiz renk var mı?
- [ ] Gereksiz chart var mı?

## Interaction

- [ ] Primary action tek ve net mi?
- [ ] Kullanıcı gereksiz step geçiyor mu?
- [ ] Keyboard flow çalışıyor mu?
- [ ] Detail progressive disclosure ile mi açılıyor?

## Accessibility

- [ ] State sadece renge bağlı değil mi?
- [ ] Contrast yeterli mi?
- [ ] Focus görünür mü?
- [ ] Mobile touch target yeterli mi?

## Localization

- [ ] TR ve EN aynı component modelini kullanabiliyor mu?
- [ ] Long-copy overflow var mı?
- [ ] Evidence literal korunuyor mu?

---

# 21. The Three-Question Test

Her UI öğesi için üç soru:

> **1. Bu gerekli mi?**  
> **2. Şimdi gerekli mi?**  
> **3. Bu kadar görünür olması gerekli mi?**

Üçüncü sorunun cevabı özellikle önemlidir.

Bir bilgi gerekli olabilir ancak prominent olmak zorunda değildir.

Örneğin:

- SHA gerekli → küçük metadata.
- Score gerekli → secondary summary.
- methodology gerekli → detail/reference layer.
- top 3 actions gerekli → high prominence.

---

# 22. GitHub Proof Visual Hierarchy Formula

Genel hierarchy:

```text
Primary question
↓
Primary action / primary evidence
↓
Evidence-backed conclusion
↓
Recommended action
↓
Diagnostic summary
↓
Deep detail
↓
Methodology / metadata
```

Bu formül ana sayfa dışındaki çoğu ekrana uygulanabilir.

---

# 23. UX Success Criteria

Bir tasarım güzel görünse bile aşağıdaki sorulara cevap veremiyorsa başarısız sayılır.

## Report first-10-second test

Kullanıcı 10 saniye içinde:

- [ ] repository'nin en güçlü kanıtını söyleyebiliyor mu?
- [ ] en büyük evidence gap'i söyleyebiliyor mu?
- [ ] ilk aksiyonu söyleyebiliyor mu?
- [ ] score'u developer grade sanmıyor mu?
- [ ] Missing ile Unavailable farkını anlayabiliyor mu?

## Home task test

Yeni kullanıcı:

- [ ] nereye input gireceğini anlıyor mu?
- [ ] hangi GitHub bilgisinin kabul edildiğini anlıyor mu?
- [ ] private data access hakkında yanlış varsayım yapıyor mu?
- [ ] gereksiz açıklama okumadan başlayabiliyor mu?

---

# 24. Design-to-Code Handoff Principles

Tasarım implementation'dan kopuk olmamalıdır.

Her final component için handoff minimum:

```text
Component name
Purpose
Anatomy
Variants
Semantic states
Data contract
Responsive behavior
Interaction behavior
Accessibility behavior
Localization behavior
Empty/loading/error behavior
```

UI implementation, Figma veya mockup'taki piksel görüntüyü körlemesine kopyalamamalıdır.

Asıl korunacak şey:

```text
information hierarchy
semantic state
interaction contract
spacing rhythm
component responsibility
```

---

# 25. MVP Tasarım Scope'u

MVP'de tasarlanması gereken temel ekranlar:

```text
1. Home
2. Unified entry routing
3. Profile Proof macro result
4. Repository Proof micro report
5. Analyzing
6. Repository signal detail
7. Error states
8. Uncertainty states
9. Mobile variants
10. Methodology / boundary access
```

MVP'de tasarım scope'u dışında tutulması tercih edilenler:

```text
Account
User profile
Dashboard
Analysis history
Team workspace
Billing
Payment
Admin panel
Repository comparison
Recruiter ranking
Public badge
Social share studio
```

Bunlar gelecekte ayrıca ürün kararı gerektirir.

---

# 26. Brand Character

GitHub Proof'un karakteri:

```text
Calm
Precise
Evidence-driven
Technical but understandable
Trustworthy
Fast
Restrained
Explainable
```

Olmaması gereken karakter:

```text
Flashy
Gamified
Judgmental
AI-magical
Recruiter-scoring machine
Crypto-dashboard-like
Over-designed
```

---

# 27. Tasarım Karar Matrisi

Yeni bir UI öğesi eklenmeden önce:

| Soru | Evet | Hayır |
|---|---|---|
| Kullanıcının ana görevini destekliyor mu? | Devam | Kaldır |
| Canonical evidence'a dayanıyor mu? | Devam | Yeniden düşün |
| İlk katmanda gerekli mi? | Göster | Progressive disclosure |
| Ayrı bir surface gerekiyor mu? | Card/panel değerlendir | Typography/spacing kullan |
| State semantic taşıyor mu? | Renk/icon kullanılabilir | Dekoratif renk kullanma |
| Kullanıcının kararını değiştiriyor mu? | Prominence artır | Sessizleştir |

---

# 28. GitHub Proof'un Tasarım Manifestosu

GitHub Proof kullanıcıyı görsel efektlerle etkilemeye çalışmaz.

Kullanıcının repository'si zaten yeterince karmaşıktır. Interface bu karmaşıklığa yenisini eklememelidir.

GitHub Proof:

- kanıtı skorun önüne koyar,
- önemli bilgiyi ilk bakışta gösterir,
- detayı gerektiğinde açar,
- belirsizliği gizlemez,
- kullanıcıyı yargılamaz,
- unsupported çıkarım üretmez,
- hızlı hareket eder,
- sakin görünür,
- her sonucu açıklayabilir,
- kullanıcının bir sonraki adımını netleştirir.

Tasarımın başarısı ekranın ne kadar “premium” göründüğüyle değil, kullanıcının ne kadar hızlı ve doğru anladığıyla ölçülür.

---

# 29. Tek Cümlelik Canonical İlke

> **GitHub Proof, karmaşık repository evidence'ını azaltmadan; doğru sırada, doğru anda ve mümkün olan en az görsel gürültüyle anlaşılır hale getirir.**

Kısa internal motto:

> **Evidence first. Noise last.**

---

# 30. Change Rule

Bu belge product contract'ın üzerinde değildir.

Bir tasarım kararı aşağıdakilerden birini değiştirmeyi gerektiriyorsa:

- analiz edilen entity,
- canonical signal sayısı,
- kategori yapısı,
- scoring semantics,
- uncertainty semantics,
- product boundary,
- ranking/selection behavior,
- supported repository scope,

şu süreç uygulanmalıdır:

```text
STOP
→ Etkilenen canonical product docs'u belirle
→ Tasarım ihtiyacının neden mevcut contract ile karşılanamadığını açıkla
→ Product kararı al
→ Canonical docs'u güncelle
→ Ardından UI contract'ını güncelle
```

UI üzerinden sessiz product scope değişikliği yapılmaz.

---

# 31. Version Notes

## v1

İlk tasarım ilkeleri seti.

Ana kararlar:

- Evidence-first hierarchy.
- Score secondary.
- One primary question per screen.
- Progressive disclosure.
- Semantic uncertainty.
- Domain-aware components.
- Minimal visual chrome.
- First-10-second report test.
- Historical v1 username → repository discovery proposal was superseded by
  the Session 095 unified-entry and P10-I2 two-proof-surface contract.

---

# 32. Son Kontrol

Bir GitHub Proof ekranına baktığımızda şu hissi vermelidir:

> **“Araç geri çekiliyor; kanıt konuşuyor.”**

Ekran kullanıcıya şunu hissettiriyorsa tasarım doğru yöndedir:

```text
Ne olduğunu anladım.
Neden böyle olduğunu görebiliyorum.
İlk ne yapmam gerektiğini biliyorum.
```

Bunun ötesindeki her görsel unsur kendini gerekçelendirmek zorundadır.
