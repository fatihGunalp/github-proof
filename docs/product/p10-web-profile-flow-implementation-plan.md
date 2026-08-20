# GitHub Proof — P10 Implementation Plan

**Status:** CONTRACT LOCKED
**Milestone:** B — Profile Evidence Experience
**Phase:** P10 — Web Profile Flow
**Depends On:** P9 Complete
**Locked:** 2026-08-20 / Session 093 — reconciled against
`docs/product/p10-presentation-language-contract.md`,
`docs/roadmap/mvp-phases.md` (Profile P10), `docs/product/public-profile-evidence-product-contract.md`
§12/§14/§16/§23/§24, `docs/architecture/public-profile-evidence-report-contract.md`,
and the actual `GitHubProof.Web` composition/routing code. §7 replaces the
prior 4-slice draft with the 6 locked implementation slices (P10-I1..I6).
See `worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md`
for the full decision evidence record.
**Production Code:** NOT IN SCOPE (this document; implementation begins only
once a slice is separately authorized)

---

# 1. Purpose

P8 ile GitHub Proof, public GitHub repository izlerinden deterministik bir profile evidence çıktısı üretme kapasitesine ulaştı.

P8 zinciri:

```
GitHubProfileSnapshot
        ↓
ProfileRepositoryAnalysisOrchestrator
        ↓
RepositoryEligibilityPolicy
        ↓
CrossRepositoryEvidenceComposer
        ↓
ProfilePatternPromotionPolicy
        ↓
RepresentativeRepositorySelectionPolicy
        ↓
ProfileComposer
        ↓
ProfileReportProjector
        ↓
ProfileReport
```

P10'un amacı yeni analiz üretmek değildir.

P10'un amacı:

> Mevcut ProfileReport çıktısını kullanıcının anlayabileceği sade bir deneyime dönüştürmektir.

---

# 2. P8 → P10 Boundary

## P8 Responsibility

P8:

- GitHub verisini analiz eder.
- Evidence üretir.
- Pattern oluşturur.
- Representative repository belirler.
- ProfileReport oluşturur.

## P10 Responsibility

P10:

- ProfileReport'u sunar.
- Kullanıcı anlamını oluşturur.
- Evidence keşif deneyimi sağlar.

Akış:

```
ProfileReport

↓

Presentation

↓

User Understanding
```

---

# 3. Core Principle

GitHub Proof:

Bir geliştiriciyi değerlendirmez.

Bir GitHub profilindeki görünür izlerin ne anlattığını gösterir.

P10:

Yapar:

- gözlemlenebilir izleri gösterir.
- anlamlandırılmış sunum sağlar.
- kanıta ulaşmayı kolaylaştırır.

Yapmaz:

- yetenek puanı verir.
- senior/junior kararı verir.
- işe uygunluk çıkarımı yapar.
- kalite hükmü verir.
- gelecekteki başarı tahmini yapar.

---

# 4. Existing P8 Capability

ProfileReport bölümleri:

- PublicProfileSummary
- TechnicalFocusAreas
- EngineeringPracticePatterns
- ProblemAreaPatterns
- RepresentativeRepositories
- WhyThisConclusion
- Limitations
- RepositoryDrillDown
- RoleEvidenceAlignment

---

# 5. Capability Boundaries

## Supported in P10

### PublicProfileSummary

Kullanıcıya:

"Bu GitHub profili ne anlatıyor?"

sorusunun başlangıç cevabını verir.

### TechnicalFocusAreas

Kullanıcı dili:

"Görünen teknik yön"

### EngineeringPracticePatterns

Kullanıcı dili:

"Tekrar eden üretim pratikleri"

### RepresentativeRepositories

Kullanıcı dili:

"Nereden başlamalı?"

### WhyThisConclusion

Kullanıcı dili:

"Bunu neden görüyoruz?"

### Limitations

Kullanıcı dili:

"Bu görünümün sınırları"

Evidence surface / profile analysis limitations sunar (ör. sparse profile,
eksik/erişilemeyen veri). Negatif geliştirici hükmüne dönüştürülmez; yeni
limitation semantiği icat edilmez.

### RepositoryDrillDown

Kullanıcı dili:

"Kanıt nerede?"

---

# 6. Deferred / Unsupported Boundaries

## RoleEvidenceAlignment

Durum:

Deferred.

P10 içinde aktif kullanıcı çıktısı değildir.

Kullanılmaz.

---

## ProblemAreaPatterns

Durum:

Unsupported.

P10 içinde kullanıcı çıktısı oluşturmaz.

---

# 7. P10 Implementation Slices

Altı slice additive ve implementation-ready'dir; her biri kendi
başlığındaki **sorumluluk/çıktı** kümesiyle non-overlapping'dir (§13.16'da
tanım kesinleştirilmiştir) — bu, dosya seviyesinde ayrık (disjoint) bir
küme anlamına gelmez: sonraki bir slice, önceki bir slice'ın tanıttığı bir
dosyaya bounded, kendi sorumluluğu kapsamında bir ekleme yapabilir (ör.
P10-I5, P10-I1'in servisine ve P10-I2'nin sayfasına failure/partial branch
ekler — bkz. P10-I5). Hiçbir slice `ProfilePipeline`, `ProfileComposer`,
`ProfileReportProjector`, veya herhangi bir Domain/Application pipeline
stage'ini değiştirmez ya da yeniden uygulamaz. P8'in DI'a bağlı çıktısı
(`AddProfilePipeline`, Web,
`src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs`)
tüketilir; ancak profile-level giriş kolleksiyonu için gereken
`IGitHubProfileSnapshotCollector` composition-root registration'ı henüz
mevcut değildir ve P10-I1'in kapsamına dahildir (bkz. P10-I1). Karar
gerekçeleri ve kod-seviyesi kanıt için §13 ve session dosyasına bakın.

## P10-I1 — Live Profile Request Boundary

Amaç:

Kullanıcı kimliğini (username veya canonical profile URL) mevcut profile
collector/orchestrator üzerinden `GitHubProfileSnapshot`'a, oradan mevcut
`ProfilePipeline`'a ve `ProfileReport`'a bağlamak.

Akış:

```
User Identity (raw string)
        ↓
IGitHubProfileSnapshotCollector.CollectAsync   (Infrastructure, pre-existing;
                                                 Web composition-root'ta HENÜZ
                                                 registered değil — bu slice'ın
                                                 kapsamına dahil, bkz. altta)
        ↓
GitHubProfileSnapshotResult
        ↓ (IsSuccessful)
GitHubProfileSnapshot
        ↓
ProfilePipeline.RunAsync                        (Application, pre-existing)
        ↓
ProfilePipelineResult  (Failed | Composed)
        ↓ (Composed)
ProfileReport
```

**Kapsam düzeltmesi (Session 093 — ChatGPT final review sonrası):** Bu
slice yalnız yeni bir Web servisi eklemekle sınırlı değildir. `main@0710bb6`
kod kanıtı (doğrulandı):

- `Program.cs` yalnız `IGitHubRepositorySnapshotCollector`'ı register eder
  (satır 49); `IGitHubProfileSnapshotCollector` için **hiçbir registration
  yoktur.**
- `AddProfilePipeline()`
  (`src/GitHubProof.Web/Profile/ProfilePipelineServiceCollectionExtensions.cs`)
  da `IGitHubProfileSnapshotCollector`'ı register etmez — yalnız
  `IProfileRepositorySnapshotSource`'u (farklı bir arayüz; profile pipeline
  İÇİNDE tek bir repository'nin snapshot'ını almak için) register eder.
- `GitHubProfileSnapshotOrchestrator.CreateDefault(HttpClient, TimeProvider)`
  mevcut production construction path'idir — `GitHubRepositorySnapshotOrchestrator.CreateDefault`'un
  birebir profile karşılığı.

Bu nedenle P10-I1'in implementasyon kapsamı üç parçadır:

1. **Composition-root registration** — `Program.cs`'e, mevcut
   `IGitHubRepositorySnapshotCollector` registration'ının (satır 49-65)
   birebir örüntüsünü izleyen bir `IGitHubProfileSnapshotCollector`
   registration'ı eklenir: aynı `"GitHubProof.GitHub"` `HttpClient` factory
   client'ı ve aynı `TimeProvider` reuse edilerek
   `GitHubProfileSnapshotOrchestrator.CreateDefault(httpClient, timeProvider)`
   çağrılır. `IGitHubProfileSnapshotCollector`'ın interface'i veya
   `GitHubProfileSnapshotOrchestrator`'ın implementasyonu **değiştirilmez** —
   yalnız DI container'a bağlanır.
2. **Live Profile servis/result çifti** — `src/GitHubProof.Web/Profile/`
   altında yeni bir servis (mevcut `LiveRepositoryReportService` /
   `LiveRepositoryAnalysisResult` çiftinin profile karşılığı — aynı
   invariant şekli: `Analysis is null` XOR `!Availability.IsAvailable`).
3. **Targeted Web testleri** — hem yeni DI registration'ının çözüldüğünü
   (mevcut `ProfilePipelineServiceCollectionExtensionsTests.cs` örüntüsünü
   izleyerek) hem de yeni servisin başarı/failure yollarını (mevcut
   `LiveRepositoryReportServiceTests.cs` örüntüsünü izleyerek) kanıtlayan
   testler.

Bu slice analiz pipeline'ını yeniden tasarlamaz.

Değiştirilmeyecek: `ProfilePipeline`, `ProfileComposer`,
`ProfileReportProjector`, `IGitHubProfileSnapshotCollector`'ın kendi
interface/implementasyonu, `GitHubProfileHandle`, evidence generation
logic. (`Program.cs`'e yeni bir DI registration eklemek bu listeye
girmez — mevcut hiçbir registration veya davranış değiştirilmiyor, yalnız
eksik olan eklenıyor.)

Çıkış: raw kimlik string'inden `ProfileReport` (veya tipli bir failure) uçtan
uca elde edilebilir — `IGitHubProfileSnapshotCollector` DI container'dan
resolve edilebilir durumdadır; hiçbir Web sayfası bu slice'a bağlı değildir.

---

## P10-I2 — Profile Entry + Routing

Amaç:

Mevcut Repository Proof davranışını ve semantic boundary'sini koruyarak
Profile Proof giriş yüzeyini eklemek.

Kapsam: `Pages/Profile.cshtml` + `Profile.cshtml.cs` (mevcut
`Index.cshtml`/`IndexModel` şeklini izler — bound identity input,
`OnPostAsync`, P10-I1'in servisini kullanan error branch), yeni
`Pages/Shared/_ProfileInput.cshtml` partial (mevcut `_RepositoryInput.cshtml`
örüntüsünü izler). Route: `/Profile` (`/profile`).

`LiveRepositoryReportService`, `RepositoryReportProjector` ve tüm
`_Repository*.cshtml` partial'ları değişmeden kalır; `/` routing ve
`IndexModel`'in mevcut `OnPostAsync`/`RepositoryUrl`/analiz semantics'i
değişmez (§13.5). `Pages/Index.cshtml` (landing markup) ise Profile Proof'a
giden additive bir giriş noktası (link/kart) alabilir, böylece kullanıcı
her iki girişi de (Profile Proof, Repository Proof) keşfedebilir; bu,
repository formunu bir profile formuna dönüştürmez ve iki semantic
boundary'yi birleştirmez. `_Layout.cshtml`'e de yalnız additive bir
navigasyon linki eklenebilir; mevcut site-brand/tema davranışı korunur.

Çıkış: kullanıcı `/profile`'a ulaşabilir, bir kimlik gönderebilir ve
Repository Proof route'una dokunmadan (başarılı, partial veya failed) bir
sonuç sayfasına ulaşabilir.

---

## P10-I3 — Meaning / Direction / Starting Point

Amaç:

İlk okuma katmanını, mevcut canonical report sıralamasıyla (report contract
§7: pozisyon 1-2-3-5) ve presentation-language-contract §6 Experience
Principle'ıyla (Meaning → Direction → Starting Point) hizalı biçimde
sunmak.

Sunum sırası:

```
PublicProfileSummary        ("Bu GitHub ne anlatıyor?")
        ↓
TechnicalFocusAreas          "Visible technical direction"
        ↓
EngineeringPracticePatterns  "Recurring engineering practices"
        ↓
RepresentativeRepositories   "Nereden başlamalı?"
```

Bölüm etiketleri §13.9'da kilitli, tek İngilizce sözlükten gelir (bkz.
`p10-presentation-language-contract.md` §8) — bu iki bölüm hiçbir zaman
ortak/birleşik tek bir etikete indirgenmez.

`ProblemAreaPatterns` (pozisyon 4) §13.10 gereği render edilmez; sıralama bu
atlamayla bozulmaz (report contract §7).

Yasak: dashboard, score, ranking, seniority, employability, role inference
veya herhangi bir unsupported taxonomy widget'ı. Product contract §18 ve
report contract §16'daki yasaklı liste doğrudan geçerlidir.

Kapsam: yalnız yeni `_Profile*.cshtml` partial'ları; mevcut
`tokens.css`/`components.css`/`base.css` reuse edilir, yeni design token
eklenmez.

---

## P10-I4 — Why + Evidence Drill-Down

Amaç:

```
Meaning
    ↓
"Bunu neden görüyoruz?"   (WhyThisConclusion)
    ↓
supporting repositories   (RepositoryDrillDown)
    ↓
existing Repository Proof micro-view
    ↓
Signal / Evidence
```

Drill-down mekaniği (§13.6/§13.14'te kilitli): her `RepositoryDrillDown`
girdisi (`RepositoryDrillDownReportItem`) her zaman non-nullable bir
`Repository` (`ProfileRepositoryReference`, `Owner`/`Name`/`Url` — daima
mevcut) ve bir `EligibilityStatus` taşır; `AnalysisReference`
(`RepositoryAnalysisReference?`) ise **nullable**'dır — yalnız o repository
gerçekten analiz edildiğinde dolu olur (ör. `Undetermined`/`NotAllowed`/
`NotApplicable` eligibility durumlarında `null` kalır), ve dolu olduğunda da
yalnız kimlik taşır (`AnalysisId`, `RepositoryIdentity`, `AnalysisVersion`),
hiçbir zaman saklanmış bir `RepositoryAnalysis`. Uygulamada persistence/cache
bulunmadığı için `AnalysisReference` zaten bir "stored report" kaynağı
değildir — link hedefi olarak hiçbir zaman kullanılmaz.

Buna göre: drill-down linki her zaman `Repository.Url` üzerinden kurulur
(nullable değildir, `AnalysisReference`'ın doluluğundan bağımsızdır) ve
mevcut `/` Repository Proof girişini (POST/input/analiz semantics'i
değişmeden, §13.5) bu URL ile önceden
doldurup gönderir; bu, doğrudan bir ziyaretçinin tetiklediğiyle aynı canlı
analizi tetikler. `EligibilityStatus` ve (doluysa) `AnalysisReference`
yalnız o satırın kendi durum etiketini belirlemek için kullanılır (ör.
"henüz analiz edilmedi" / eligibility-based bir not) — linkin var olup
olmadığını veya hedefini etkilemez; her satır her zaman tıklanabilir bir
`/` girişine sahiptir.

Profile sayfası kendi içinde `RepositoryAnalysis` şekilli içerik (signal,
score, uncertainty kartı) render etmez — bu tamamen
`_RepositoryReport*.cshtml`'in sorumluluğunda kalır. Repository analizi
profile raporu içinde tekrar üretilmez veya çoğaltılmaz.

---

## P10-I5 — Limitations + Partial / Failure UX

Amaç:

Gerçek profile collection/pipeline contract'larının desteklediği bounded
durumları kapsamak — yeni bir durum icat etmeden.

İki katman (§13.12'de kilitli):

- **Entry-boundary** (pipeline çalışmadan önce):
  `GitHubProfileSnapshotResult.ProfileAvailability`
  (`DataAvailabilityStatus`: `NotApplicable`, `RateLimited`, `Forbidden`,
  `Failed`, `Unavailable`) — Repository Proof'un zaten kullandığı
  (`IndexModel.CreateRepositoryFailureMessage`) aynı switch şeklini,
  profile'a özgü kopya ile tekrar kullanır.
- **Composed-profile** (pipeline başarıyla çalıştıktan sonra):
  `ProfilePipelineResult` (`Failed` | `Composed`); `Composed` içinde
  `AnalysisStatus` (`Complete`/`Partial`), `Limitations[]`
  (`LimitedIndependentEvidenceSurface`, `PartialEvidenceSurface`),
  `Warnings[]` (`RepositoryEvidenceUnavailable`,
  `RepositoryEvidenceRateLimited`, `RepositoryAnalysisFailed`,
  `RepositoryEligibilityUndetermined`).

Sparse profile (product contract §16) yeni bir UI durumu değildir; mevcut
`Partial` / `LimitedIndependentEvidenceSurface` yolunun bir örneğidir.

Hiçbir eksik/erişilemeyen veri negatif geliştirici hükmüne dönüştürülmez
(product contract §16/§18, report contract §6.4).

**Dosya kapsamı notu:** bu slice yeni dosya eklemez; sorumluluğu
P10-I1'in servisine (entry-boundary switch) ve P10-I2'nin sayfasına
(composed-profile limitation/warning presentation) **bounded** eklemeler
yapmaktır. Bu, §7'de kilitli responsibility/outcome-based non-overlap
tanımına uygundur: P10-I1/I2 kendi sorumluluklarını (request boundary,
routing) tamamlamış olarak kapanır; P10-I5 onların üzerine yalnız
failure/partial-durum sorumluluğunu ekler — iki slice aynı sorumluluğu
iki kez üstlenmez.

---

## P10-I6 — Experience Refinement + Closure

Amaç:

Deneyimi mevcut Web design/token/theme mimarisiyle sadeleştirmek.

İlkeler:

- minimum kullanıcı yükü
- progressive disclosure
- teknik karmaşıklığı gizleme
- kanıta ulaşmayı kolaylaştırma

Kapsam: yalnız görsel/etkileşim cilası — spacing, responsive layout, mevcut
`theme.js`/`data-theme` mekanizmasıyla uyum. Yeni profile'a özgü CSS
dosyaları mevcut per-feature-file kuralını izler
(`repository-input.css`/`report.css`/`status-badge.css` örneği —
ör. `profile-input.css`, `profile-report.css`). Yeni bir design-token
sistemi, component library veya CSS framework bağımlılığı eklenmez.

---

# 8. Non Goals

P10 kapsamında yapılmaz:

- scoring sistemi
- ranking sistemi
- developer level
- seniority detection
- hiring recommendation
- AI generated judgement
- yeni evidence üretimi
- yeni pattern üretimi

---

# 9. Development Rules

Her P10 geliştirmesi:

```
Problem
↓
Boundary
↓
Implementation
↓
Validation
```

ile ilerler.

Claude Code görevlerinde:

Claude:

- verilen dosyalar dışında değişiklik yapmaz.
- yeni mimari karar vermez.
- milestone kapsamını genişletmez.
- mevcut contractları değiştirmez.

---

# 10. Validation Strategy

Her slice sonrası:

## Targeted Tests

İlgili davranış test edilir.

## Regression

Kontrol edilir:

- P8 pipeline değişmedi.
- ProfileReport contract değişmedi.
- Evidence sınırları korunuyor.

## Full Validation

Çalıştırılır:

- Domain tests
- Application tests
- Infrastructure tests
- Web tests
- Full build

---

# 11. Success Criteria

P10 başarılıdır:

Eğer kullanıcı:

1. GitHub profilinin ne anlattığını anlayabiliyorsa,
2. Önce nereye bakacağını biliyorsa,
3. İsterse gerçek evidence'a ulaşabiliyorsa,
4. Sistem tarafından yargılanmadan gözlemlenebilir izleri görebiliyorsa.

---

# 12. Next Step

Session 093 ile P10 implementation-ready hâle geldi (bkz. §13). Sonraki
adım, altı slice'ı sırayla (P10-I1 → P10-I6) implementasyona açmaktır; her
slice kendi Claude görev promptuyla ayrı ayrı yetkilendirilir. Bu doküman
kendisi implementasyon yetkisi vermez — yalnız implementation-ready contract'ı
kilitler.

İlk implementasyon slice'ı:

```
P10-I1 — Live Profile Request Boundary
```

ile başlayacaktır.

---

# 13. Locked Decisions (Session 093)

Bu bölüm P10'u implementation-ready yapan kararları kilitler. Her karar
gerçek repository kanıtına dayanır (kod, canonical docs); tahmini veya
icat edilmiş değildir. Tam kanıt kaydı:
`worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md`.

## 13.1 `/profile` Route Boundary

Yeni `Pages/Profile.cshtml` → route `/Profile` (`/profile`, Razor Pages
case-insensitive). `/` (`Pages/Index.cshtml`) Repository Proof girişi olarak
kalır — `OnPostAsync`/`RepositoryUrl`/analiz semantics'i değişmez (§13.5) —
ancak landing markup'ı Profile Proof'a giden additive bir giriş noktası
alabilir. İki route birbirinden bağımsızdır; biri diğerini yeniden
yönlendirmez veya sarmalamaz, ve repository formu profile formuna
dönüştürülmez.

## 13.2 Profile Identity Input Contract

Tek bir serbest metin girişi. Web katmanı kimlik string'ini yeniden parse
etmez — trim edilmiş ham string doğrudan
`IGitHubProfileSnapshotCollector.CollectAsync(profile, ct)`'e geçirilir.
Repository Proof'un aksine (`GitHubRepositoryUrl.Parse` Web/Application
seviyesinde, `LiveRepositoryReportService` içinde çağrılır), profile için
parse adımı zaten Infrastructure seviyesinde
(`GitHubProfileHandle.Parse`, `GitHubProfileSnapshotOrchestrator.CollectAsync`
içinde) gerçekleşir. P10 bu parse'ı Web katmanında tekrar uygulamaz;
geçersiz format `ArgumentException` olarak collector çağrısından yükselir ve
P10-I1'in servis katmanında (Index akışındaki `IndexModel`'in
`ArgumentException` yakalama örüntüsüne paralel biçimde) yakalanır.

## 13.3 Username ve Canonical Profile URL Davranışı

`GitHubProfileHandle.Parse` (Infrastructure, mevcut, P10 tarafından
değiştirilmez): girdi ya `^[A-Za-z0-9](?:[A-Za-z0-9]|-(?=[A-Za-z0-9])){0,38}$`
desenine uyan bir bare login, ya da userinfo/query/fragment içermeyen, tam
olarak bir path segmenti taşıyan mutlak bir `https://github.com/{login}`
URL'idir. Her iki biçim de aynı `Login`/`CanonicalUri` kimliğine çözülür.

## 13.4 Web Composition Boundary

```
Profile input (raw string)
        ↓
Web/Profile — yeni "LiveProfileReportService" benzeri servis  (P10-I1)
        ↓
IGitHubProfileSnapshotCollector.CollectAsync   (Infrastructure tipi mevcut;
                                                 Web composition-root
                                                 registration'ı P10-I1'in
                                                 kapsamına dahil — §7/P10-I1)
        ↓
GitHubProfileSnapshotResult
        ↓
GitHubProfileSnapshot
        ↓
ProfilePipeline.RunAsync                                        (mevcut,
                                                                   zaten DI'a
                                                                   bağlı)
        ↓
ProfilePipelineResult (Failed | Composed)
        ↓
ProfileReport
```

`LiveRepositoryReportService` → `LiveRepositoryAnalysisResult` çiftinin
birebir profile karşılığı; hiçbir stage yeniden uygulanmaz.

## 13.5 Repository Proof Flow Korunması

Korunan **semantics**: `LiveRepositoryReportService`, `RepositoryReportProjector`,
tüm `_Repository*.cshtml` partial'ları, `/` routing, ve `Pages/Index.cshtml(.cs)`'in
mevcut Repository Proof davranışı — `OnPostAsync`, `RepositoryUrl` bound
property, `LiveRepositoryReportService.AnalyzeAsync` çağrısı, hata/partial
mesaj switch'i, repository micro-view render'ı. Bu davranış P10 tarafından
değiştirilmez.

`Pages/Index.cshtml(.cs)`'in kendisi ise **dosya seviyesinde** kilitli
değildir: landing sunumu, yukarıdaki semantics'i bozmadan, Profile Proof'a
giden additive bir giriş noktası (link/kart) içerebilir — bkz. P10-I2 ve
§13.1. `LiveRepositoryReportService`, `RepositoryReportProjector` ve tüm
`_Repository*.cshtml` partial'ları yine de değiştirilmez; yalnız
`Pages/Index.cshtml` (markup) ve gerekirse `_Layout.cshtml` (navigasyon)
additive bir Profile Proof girişi için genişleyebilir. Repository formu
profile formuna dönüştürülmez; iki semantic boundary birleştirilmez.

## 13.6 Profile = Macro View / Repository = Micro View

`docs/roadmap/mvp-phases.md` "Profile P10" bölümünde zaten kilitli
ilişkinin mekanik karşılığı: `RepositoryDrillDown`, kimliği
(`RepositoryDrillDownReportItem.Repository.Url` —
`ProfileRepositoryReference`, her satırda her zaman dolu; tam detay
§13.14) mevcut `/` girişine (POST/input/analiz semantics'i değişmeden,
§13.5) önceden doldurulmuş biçimde yönlendirir ve bir canlı analiz
tetikler — hiçbir `RepositoryAnalysis` persist edilmediği için başka türlü
bir "lookup" mümkün değildir (uygulamada persistence/database katmanı
yoktur). `AnalysisReference` (`RepositoryAnalysisReference?`, nullable)
linkin kaynağı veya ön koşulu **değildir**; yalnız §13.14'te açıklandığı
gibi opsiyonel kimlik/durum context'i sağlar.

## 13.7 Locale Boundary (Site-Wide Localization Genişletilmez)

`ProfileReportProjector.Project(DeveloperPublicEvidenceProfile)` **locale
parametresi almaz** (`src/GitHubProof.Application/Profile/Report/ProfileReportProjector.cs`;
Session 087: "locale independence is structural"). Bu, Repository Proof'un
gerçek bir `SignalPresentationLocalizer` süren
`RepositoryReportProjector.Project(analysis, locale)`'ından farklıdır. P10
`ProfileReportProjector`'a locale parametresi eklemez ve Application
seviyesinde profile localization başlatmaz — bu bir contract değişikliği
olur ve docs-only bir session'ın kapsamı dışındadır. Presentation-language
mapping tamamen Web katmanında, sabit tek dilli (İngilizce — mevcut sitenin
zaten yerleşik render dili; bkz. `Pages/Index.cshtml`, `IndexModel`,
`_Layout.cshtml`) statik bir sözlükle uygulanır. Bu bir locale switcher
değildir; site-wide bir localization projesi başlatmaz.

`docs/product/p10-presentation-language-contract.md`'deki Türkçe örnek
metinler ürün-yönü anlatımıdır (bu doküman gibi Türkçe yazılan diğer
canonical `docs/product/` dosyalarıyla aynı konvansiyon); P10'un fiilen
render edeceği Razor/UI string'leri değildir.

## 13.8 Deterministic Presentation-Language Mapping

Web-owned statik anahtar→etiket sözlüğü
(`p10-presentation-language-contract.md` §8 "User Language Dictionary"
kavramının İngilizce render edilmiş hâli), yalnız zaten canonical/locale-
independent kimliklerle anahtarlanır: `ProfileReportSectionKey`, pattern
family/key, `ProfileLimitationCode`, `ProfileWarningCode`,
`DataAvailabilityStatus`. Report contract §7.1/§9: "localized label !=
semantic truth" — eşitlik anahtarı her zaman canonical identity'dir, etiket
değil.

## 13.9 TechnicalFocusAreas vs EngineeringPracticePatterns Ayrı Dil

- `TechnicalFocusAreas` → **"Visible technical direction"**
  (tekrar eden technical-focus evidence — hangi tür sistemler tekrarlanıyor).
- `EngineeringPracticePatterns` → **"Recurring engineering practices"**
  (tekrar eden practice evidence — test/CI/docs vb. — asla "direction"
  olarak yeniden ifade edilmez).

İki bölüm görsel/metinsel olarak ayrı kalır; tek bir "skills" listesine
birleştirilmez (bağımsız promote edilen, farklı kapsamlı iki pattern family
— `docs/architecture/public-profile-evidence-patterns.md`).

## 13.10 ProblemAreaPatterns — Unsupported

Report contract §6.5 + product contract §24 not 2 (Session 080) zaten
kilitli: varsayılan Milestone B raporu `ProblemAreaPatterns`'ı render etmez;
typed `ProfileReport` yine de `Availability = Unsupported, Items = []`
taşımaya devam eder. P10 bu bölümü render etmez, boş bir "problem area
bulunamadı" kartı göstermez, etiketini başka bir amaçla yeniden kullanmaz.

## 13.11 RoleEvidenceAlignment — Deferred

Report contract §6.3/§6.5 + product contract §14/§24 (Session 076/077/079)
zaten kilitli: varsayılan render'dan çıkarılmıştır (sanctioned omission).
Typed report yine de `Availability = Deferred, Items = []` taşır. P10 bunu
göstermez, `TechnicalFocusAreas`/`EngineeringPracticePatterns`'tan sentetik
bir "role" etiketi türetmez (presentation-language-contract "Visible
Direction" notunda zaten yasaklı), Unsupported ile Deferred'i tek bir duruma
indirgemez.

## 13.12 Partial / Sparse / Unavailable / Rate-Limited / Forbidden / Failed

İki katman, ikisi de mevcut ve olduğu gibi reuse edilir:

- **Entry-boundary:** `GitHubProfileSnapshotResult.ProfileAvailability`
  (`DataAvailabilityStatus`: `NotApplicable`, `RateLimited`, `Forbidden`,
  `Failed`, `Unavailable`) — Repository Proof'un
  `IndexModel.CreateRepositoryFailureMessage`'ıyla aynı switch şekli.
- **Composed-profile:** `ProfilePipelineResult` (`Failed`/`Composed`);
  `Composed` içinde `AnalysisStatus` (`Complete`/`Partial`),
  `Limitations[]` (`LimitedIndependentEvidenceSurface`,
  `PartialEvidenceSurface`), `Warnings[]` (`RepositoryEvidenceUnavailable`,
  `RepositoryEvidenceRateLimited`, `RepositoryAnalysisFailed`,
  `RepositoryEligibilityUndetermined`).

Sparse profile (product contract §16) yeni bir durum değildir; mevcut
`Partial`/`LimitedIndependentEvidenceSurface` yolunun bir örneğidir. Hiçbiri
negatif geliştirici hükmüne dönüştürülmez.

## 13.13 Warnings ve Limitations Sunumu

Report contract §13: `Warnings[]` zorunlu bir ana bölüm değildir; P10-I5 onu
bir durum bildirimi olarak sunar (ör. `PublicProfileSummary` yakınında bir
banner), asla dokuzuncu bir numaralı bölüm olarak değil. `Limitations`
kendi canonical §5/§7 bölümü (pozisyon 7) olarak kalır.

## 13.14 Profile → Repository Evidence Drill-Down

§13.6'da kilitli, tam mekanik §7/P10-I4'te yazılıdır: her
`RepositoryDrillDownReportItem`'ın `Repository`
(`ProfileRepositoryReference.Url`) alanı her zaman doludur ve drill-down
linki her zaman bu URL üzerinden `/` girişini önceden doldurup gönderir —
`AnalysisReference` (`RepositoryAnalysisReference?`) nullable'dır ve yalnız
o satırın durum etiketi için okunur, linkin varlığını veya hedefini
etkilemez. Profile raporu içinde repository analizi çoğaltılmaz.

## 13.15 Presentation Invariant

P10 zaten canonical structured truth'u tüketir. Etiket, sıralama, hiyerarşi
ve açıklama çerçevesi ekleyebilir. Yeni analysis truth üretemez. Yasak örnek
mapping'ler: `C# → backend developer`, `Python → AI engineer` veya başka
herhangi bir role/seniority/employability/capability/ownership/originality
ya da unsupported problem-area çıkarımı (product contract §18, report
contract §16).

## 13.16 Slice Responsibility Boundary

P10-I1..I6 **sorumluluk/çıktı** bazında non-overlapping'dir — **dosya
sahipliği** bazında değil:

- Her slice'ın kendine ait, başka hiçbir slice'ın tekrar üstlenmediği bir
  sorumluluğu vardır (§7'de tanımlı: P10-I1 request/DI boundary, P10-I2
  routing/entry, P10-I3 presentation ordering, P10-I4 drill-down, P10-I5
  failure/partial presentation, P10-I6 visual refinement).
- Daha sonraki bir slice, önceki bir slice'ın tanıttığı veya değiştirdiği
  bir dosyaya **bounded** bir ekleme yapabilir; bu, dosya seviyesinde
  ayrık (disjoint) bir küme gerektirmez.
- Böyle bir ekleme önceki slice'ın sorumluluğunu yeniden üstlenemez veya
  yeniden uygulayamaz — yalnız kendi sorumluluğunu o dosyaya taşır.
- Kanonik örnek: P10-I5, P10-I1'in servisine (entry-boundary failure
  switch) ve P10-I2'nin sayfasına (composed-profile limitation/warning
  presentation) bounded ekleme yapar — P10-I1/I2'nin kendi sorumluluğunu
  (request boundary, routing) yeniden üstlenmeden (bkz. P10-I5 "Dosya
  kapsamı notu", §7).
