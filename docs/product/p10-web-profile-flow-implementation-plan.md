# GitHub Proof — P10 Implementation Plan

**Status:** CONTRACT LOCKED
**Milestone:** B — Profile Evidence Experience
**Phase:** P10 — Web Profile Flow
**Depends On:** P9 Complete
**Historical lock record:** 2026-08-20 / Session 093 — reconciled against
`docs/product/p10-presentation-language-contract.md`,
`docs/roadmap/mvp-phases.md` (Profile P10), `docs/product/public-profile-evidence-product-contract.md`
§12/§14/§16/§23/§24, `docs/architecture/public-profile-evidence-report-contract.md`,
and the actual `GitHubProof.Web` composition/routing code. §7 replaces the
prior 4-slice draft with the 6 locked implementation slices (P10-I1..I6).
See `worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md`
for the full decision evidence record. This six-slice sequence was the
Session 093 state and is superseded for current execution sequencing by the
Session 101 P10-I2A amendment below; existing slice identities remain intact.
**Amended:** 2026-08-20 / Session 095 — explicit user decision after visual
review of the running application. The landing entry surface is now ONE
unified GitHub input; the separate Profile Proof / Repository Proof entry
surfaces are superseded. See §13.0. Affected clauses: §7/P10-I2, §13.1,
§13.2, §13.5. Underlying analysis semantics, evidence rules and all
inference prohibitions are unchanged.
**Amended:** 2026-08-20 / Session 096 — added §13.17 only: a cross-reference to
the new `docs/architecture/public-profile-runtime-collection.md` runtime
prerequisite, so §13.4 is not read as requiring Profile Proof to reuse the
complete Repository Proof collection composition. No slice responsibility,
route, entry, presentation-language, evidence, pattern or prohibition clause
changed
**Amended:** 2026-08-21 / Session 098 — §13.17 cross-reference refreshed only:
the omission list is now three surfaces (`Contributors` added), and the
`DecisionBounded` pagination stop rule (runtime contract §15) plus the fork
admission rule (§16, eligibility §6.3) are pointed at. No slice responsibility,
route, entry, presentation-language, evidence, pattern or prohibition clause
changed
**Amended:** 2026-08-21 / Session 101 — the previous fixed-English Web
presentation boundary is superseded by the explicit EN/TR product decision.
P10 now has one additional prerequisite slice, `P10-I2A — Web Experience
Foundation`, between the already-complete P10-I2 and P10-I3. Existing slice
identities are not renumbered. Request localization, the persistent EN/TR
utility, common Web localization, Repository Proof locale wiring, the
Profile Web-localization foundation and a truthful immediate processing state
belong to P10-I2A. Domain, evidence, eligibility, scoring and report semantics
are unchanged.
**Amended:** 2026-08-21 / Session 103 — added §13.18 only, plus a one-line
pointer inside §7/P10-I3. Both are cross-references to the P10-I3 Meaning
layer contract that `docs/product/p10-presentation-language-contract.md`
§8.4/§8.5 now owns normatively. This document defines no new presentation
semantics, copy, state or template of its own. §7/P10-I3's scope clause was
additionally corrected: its former "yalnız yeni `_Profile*.cshtml`
partial'ları" restriction contradicted the actual Web boundary --- today
`Pages/Profile.cshtml.cs` records only `HasComposedProfile` and never exposes
the successful `ProfilePipelineResult.Report` --- and is replaced by a
minimum-change responsibility description. No slice identity, route, entry,
section label, ordering, evidence, pattern or prohibition clause changed
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

Current P10 execution sequence yedi additive slice içerir: P10-I1 ve P10-I2
complete'tir; P10-I2A mandatory Web experience prerequisite'tir; P10-I3'ten
P10-I6'ya kadar olan slice'lar previously locked semantic responsibilities'ı
korur. Her slice kendi
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

Repository Proof analiz semantics'ini koruyarak birleşik giriş yüzeyini
(§13.0) ve Profile Proof route/result sınırını eklemek.

**Session 095 amendment.** Bu slice'ın önceki kapsamı ayrı bir
`_ProfileInput.cshtml` giriş partial'ı ve `/profile` üzerinde ayrı bir
bound identity input öngörüyordu. §13.0'daki açık kullanıcı kararı bunu
geçersiz kılar: giriş tek ve birleşiktir, landing'de yaşar; `/profile`
yalnız sonuç route'udur.

Kapsam:

- `src/GitHubProof.Web/Entry/` — `GitHubEntryClassifier`,
  `GitHubEntryRoute`, `GitHubEntryKind`: §13.0/3'teki deterministik
  shape-based routing sınırı.
- `Pages/Shared/_GitHubInput.cshtml` + `GitHubInputModel` — tek birleşik
  giriş (`_RepositoryInput.cshtml` / `RepositoryInputModel` yerine).
- `Pages/Index.cshtml(.cs)` — `GitHubTarget` bound property; `OnPostAsync`
  sınıflandırır, profile-shaped değeri `/Profile`'a yönlendirir,
  repository-shaped değeri mevcut analiz akışında çalıştırır,
  sınıflandırılamayan değeri validation error olarak gösterir.
- `Pages/Profile.cshtml` + `Profile.cshtml.cs` — `/Profile` (`/profile`)
  route'u. `OnGet()` analiz çalıştırmaz, `/`'a yönlendirir.
  `OnPostAsync(ct)` biçimi yeniden sınıflandırır ve P10-I1'in
  `LiveProfileReportService`'ini çağırarak tipli failure/composed sonucunu
  korur (§13.0/6).

`LiveRepositoryReportService`, `RepositoryReportProjector` ve tüm
`_Repository*.cshtml` rapor partial'ları değişmeden kalır; repository analiz
semantics'i değişmez (§13.5).

Kapsam dışı: report-section sunumu (P10-I3), drill-down (P10-I4), final
failure/partial UX (P10-I5). `/profile` bu slice'ta yalnız minimal result
boundary render eder.

Çıkış: kullanıcı tek bir alandan username, profile URL veya repository URL
gönderebilir; gönderim deterministik olarak doğru analiz akışına gider ve
profile akışı `/profile` üzerinde bir sonuç sınırına ulaşır.

---

## P10-I2A — Web Experience Foundation

Amaç:

P10-I3 başlamadan önce Repository Proof ve Profile Proof'un paylaştığı Web
request-language ve request-in-progress deneyimini kurmak; bunu yaparken
analiz semantics'ine, transient POST-result modeline veya Profile report
architecture sınırına yeni davranış eklememek.

Sequence içindeki zorunlu konumu:

```text
P10-I1 — COMPLETE
P10-I2 — COMPLETE
P10-I2A — Web Experience Foundation
P10-I3 — Meaning / Direction / Starting Point
P10-I4 — Why + Evidence Drill-Down
P10-I5 — Limitations + Partial / Failure UX
P10-I6 — Experience Refinement + Closure
```

Kapsam:

- framework-native ASP.NET Core Request Localization; desteklenen culture'lar
  yalnız `en` ve `tr`, resolution precedence explicit persistent user choice
  (`CookieRequestCultureProvider`) → browser preference/q-order
  (`AcceptLanguageHeaderRequestCultureProvider`) → `en` fallback,
- `tr`, `tr-TR`, `tr-*` → `tr`; `en`, `en-*` → `en`; unsupported browser
  language family'leri supported bir sonraki preference aranırken atlanır;
  supported preference yoksa `en`,
- ordinary resolution için `QueryStringRequestCultureProvider` yok;
  geolocation, IP-derived location, device location ve browser Geolocation API
  hiçbir biçimde language selection'a katılmaz,
- header'da theme control ile birlikte sessiz, persistent ve accessible
  `EN | TR` language utility; current selection algılanabilir ve Analyze CTA
  ile yarışmaz,
- selected request culture'ın `<html lang>`, navbar/header utility copy,
  theme accessibility copy, landing/input/validation/processing copy,
  mevcut user-facing failure copy ve rendered Repository/Profile Web
  presentation'ı sürmesi,
- Repository Proof'ta mevcut Application EN/TR catalog'ları ve
  `RepositoryReportProjector` korunarak hard-coded Web locale yerine resolved
  request UI culture'ın projection'a geçirilmesi; `RepositoryAnalysis`
  yeniden çalıştırılmaz,
- `ProfileReportProjector` locale-neutral kalırken Web-owned deterministic
  Profile presentation/localization mapping foundation'ının kurulması;
  `ProfileReportSectionKey`, `ProfilePatternFamily`, `PatternKey`,
  `ProfileLimitationCode`, `ProfileWarningCode` ve mevcut availability/status
  identities dışında yeni semantic identity üretilmez,
- tek unified girişte native constraint validation başarılı olduktan hemen
  sonra başlayan truthful indeterminate processing state: duplicate submit
  engeli, serialization-safe control davranışı, `aria-busy="true"`,
  `role="status"`, `aria-live="polite"`, `aria-atomic="true"`, BFCache/back
  restoration reset'i ve `prefers-reduced-motion` uyumu,
- no percentage, ETA, completed/active/pending stage, checkmark, SSE, SignalR,
  polling, streaming response veya sırf UI için yeni backend progress
  telemetry. Existing `ProfileExecutionTelemetry` live progress source değildir.

Language utility transient bir Repository/Profile POST result üzerinde
değiştirildiğinde yeni culture persist edilir; GitHub analizi sessizce yeniden
çalıştırılmaz ve cache/persistence icat edilmez. Kullanıcı localized landing'e
döner. Mevcut entry model ile güvenli ise son `GitHubTarget` prefilled taşınır;
yeni analiz yalnız açık bir Analyze submit'iyle başlar. Bu interaction P10-I6
tarafından daha sonra, user testing gerektirirse refine edilebilir.

P10-I2A mevcut failure copy'yi localize edebilir; final partial/failure semantic
branch'leri ve bunların kapsamlı UX'i P10-I5'in sorumluluğu olarak kalır.

Literal evidence çevrilmez: repository/user names, URL, path, branch, SHA,
tag, raw evidence value ve C#/JavaScript/Python gibi programming/GitHub
language identities aynı kalır.

Çıkış kriteri:

- cookie → Accept-Language → English fallback precedence'i ve EN/TR family
  normalization'ı targeted Web testleriyle kanıtlanmış,
- persistent selector, whole-Web culture ownership ve dynamic `<html lang>`
  doğrulanmış,
- Repository projection resolved request culture'ı kullanıyor ve locale
  değişimi analysis'i rerun etmiyor,
- Profile Web mapping locale-neutral `ProfileReport` sınırını koruyor,
- unified input processing state iki analysis flow'un başlangıcında truthful,
  accessible ve duplicate-submit-safe,
- hiçbir yeni external/NuGet localization package, result cache/persistence
  veya progress infrastructure eklenmemiş.

---

## P10-I3 — Meaning / Direction / Starting Point

Amaç:

İlk okuma katmanını, mevcut canonical report sıralamasıyla (report contract
§7: pozisyon 1-2-3-5) ve presentation-language-contract §6 Experience
Principle'ıyla (Meaning → Direction → Starting Point) hizalı biçimde
sunmak.

Sunum sırası:

```
PublicProfileSummary        (localized meaning summary)
        ↓
TechnicalFocusAreas          "Visible technical direction" /
                             "Görünen teknik yön"
        ↓
EngineeringPracticePatterns  "Recurring engineering practices" /
                             "Tekrar eden üretim pratikleri"
        ↓
RepresentativeRepositories   "Starting point" /
                             "Başlangıç noktası"
```

Bölüm etiketleri §13.9'da kilitli, selected request culture için deterministic
EN/TR Web presentation mapping'inden gelir (bkz.
`p10-presentation-language-contract.md` §8) — bu iki bölüm hiçbir zaman
ortak/birleşik tek bir etikete indirgenmez.

`ProblemAreaPatterns` (pozisyon 4) §13.10 gereği render edilmez; sıralama bu
atlamayla bozulmaz (report contract §7).

Meaning katmanının exact deterministic sentezi, kapalı state matrisi ve EN/TR
copy'si `p10-presentation-language-contract.md` §8.4'te; üç bölümün
`Supported`-but-empty EN/TR copy'si §8.5'te kilitlidir (bkz. §13.18).

Yasak: dashboard, score, ranking, seniority, employability, role inference
veya herhangi bir unsupported taxonomy widget'ı. Product contract §18 ve
report contract §16'daki yasaklı liste doğrudan geçerlidir.

Kapsam (Session 103'te düzeltildi — önceki "yalnız yeni `_Profile*.cshtml`
partial'ları" kısıtı gerçek Web boundary'siyle uyumsuzdu ve geçersizdir):
P10-I3, typed report'u presentation'a taşımak için gereken **minimum** Web
değişikliklerini kapsar. Sorumluluk bazında:

- `Pages/Profile.cshtml.cs` mevcut başarılı `ProfilePipelineResult.Report`
  (`ProfileReport`) değerini presentation yüzeyine **expose edebilir**. Bugün
  yalnız `HasComposedProfile = true` kaydediyor ve raporu hiç açmıyor; bu
  yüzden slice yalnız partial ekleyerek uygulanamaz. Bu sayfa report truth'unu
  yeniden hesaplayamaz, değiştiremez veya yeniden yorumlayamaz; yalnız zaten
  üretilmiş typed değeri taşır.
- `Pages/Profile.cshtml` P10-I3 result bölümlerini kompoze edebilir.
- Yeni `_Profile*.cshtml` partial'ları şunları render edebilir: Meaning,
  `TechnicalFocusAreas`, `EngineeringPracticePatterns`,
  `RepresentativeRepositories`.
- `ProfileWebPresentationLocalizer` yalnız
  `p10-presentation-language-contract.md` §8.4/§8.5'te kilitlenmiş
  deterministic mapping'leri alabilir; başka hiçbir yeni presentation
  semantiği eklenemez.
- Targeted Web testleri eklenebilir/güncellenebilir.
- Gerekiyorsa bounded Profile-result CSS eklenebilir/güncellenebilir; mevcut
  `tokens.css`/`components.css`/`base.css` ve mevcut design token'lar reuse
  edilir. Yeni design token, dashboard veya yeni component sistemi authorize
  edilmemiştir.

Değişmeyenler: Domain ve Application, `ProfileReportProjector` (locale-neutral
kalır), Repository Proof davranışı, ve P10-I4 (`WhyThisConclusion` +
drill-down) ile P10-I5 (final Limitations / Partial / Failure UX, `Warnings`
bildirimi) sahipliği.

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

P10-I1 ve P10-I2 complete'tir. Session 101 ile current remaining sequence:

```
P10-I2A — Web Experience Foundation
→ P10-I3
→ P10-I4
→ P10-I5
→ P10-I6
```

P10-I2A, P10-I3 başlamadan önce tamamlanmalıdır. Her slice ayrıca
yetkilendirilir; bu document tek başına production implementation yetkisi
vermez.

---

# 13. Locked Decisions (Session 093)

Bu bölüm P10'u implementation-ready yapan kararları kilitler. Her karar
gerçek repository kanıtına dayanır (kod, canonical docs); tahmini veya
icat edilmiş değildir. Tam kanıt kaydı:
`worklog/sessions/2026-08-20-093-p10-contract-implementation-closure.md`.

## 13.0 Unified Entry Input (Session 095 — Supersedes Separate Entry Surfaces)

Açık kullanıcı kararı (çalışan uygulamanın görsel incelemesinden sonra):
landing sayfası **tek bir birleşik GitHub girişi** taşır. Ayrı bir "Analyze
a profile instead" keşif linki ve ayrı Profile Proof / Repository Proof
giriş yüzeyleri kaldırılmıştır. Bu karar yalnız **giriş deneyimini**
birleştirir; analiz semantics'ini birleştirmez.

Kilitli gerçekler:

1. **Tek birleşik giriş.** Landing'de tam olarak bir input alanı vardır;
   etiketi `GitHub username, profile URL, or repository URL`. Alan üç
   biçimi de kabul eder.
2. **Alttaki analiz semantics'i ayrı kalır.** Profile Proof (macro,
   `LiveProfileReportService` → `ProfilePipeline`) ve Repository Proof
   (micro, `LiveRepositoryReportService` → repository analiz hattı) iki
   ayrı akıştır; pipeline'lar, evidence kuralları ve raporlar
   birleştirilmez.
3. **Deterministik iki aşamalı sınıflandırma.** Aşama 1 yalnız *biçime*
   bakarak canonical sahibi seçer; aşama 2 kabul kararını o canonical
   parser'a devreder:

   ~~~text
   scheme yok, '/' yok        -> Profile adayı    (bare login)
   scheme + 1 path segment    -> Profile adayı    (profile URL)
   scheme + 2 path segment    -> Repository adayı (repository URL)
   diğer her biçim            -> kabul edilmez
   ~~~

   Aday seçildikten sonra `GitHubProfileHandle.Parse` /
   `GitHubRepositoryUrl.Parse` kabul etmezse giriş reddedilir.

   `owner/repo` ve `github.com/owner/repo` **bare login değildir** (path
   taşırlar) ve kabul edilen giriş sözleşmesinde yoktur; Profile'a
   yönlendirilmezler, giriş seviyesinde reddedilirler.
4. **Asla tahmin yok.** AI/intent sınıflandırması, olasılıksal skorlama,
   repository/profile varlığını keşfetmek için GitHub çağrısı, veya
   bozuk/non-canonical bir GitHub URL'inin sessizce canonical bir biçime
   normalize edilmesi yasaktır. Aynı girdi her zaman aynı sonucu verir.
5. **Canonical parsing sahipliği değişmez.** Login deseni, scheme, host,
   userinfo, query ve fragment kuralları `GitHubProfileHandle.Parse` ve
   `GitHubRepositoryUrl.Parse` sahipliğindedir. Web classifier bu kuralların
   hiçbirini yeniden uygulamaz; yalnız *çağırır* ve kabul/ret cevabını alır.
   Parse sonucu atılır, ham trim'lenmiş string ileri taşınır — yani
   collector'ın kendi `GitHubProfileHandle.Parse`'ı hâlâ kayıt altındaki
   parse'tır ve Web katmanı kendi kimliğini türetmez (P10-I1 sınırı
   korunur). Bu ön-kontrol, kabul edilmeyen biçimin akışa özgü bir hata
   sayfasında çıkmaz sokağa düşmek yerine birleşik girişte, kabul edilen
   biçimleri listeleyen tek bir mesajla kalmasını sağlar.
6. **Route'lar değişmez, ama profile analizi POST-only'dir.** `/profile`
   Profile Proof route/result akışı, `/` birleşik giriş + Repository Proof
   akışı olarak kalır.

   ~~~text
   GET  /profile  -> analiz YOK; birleşik girişe (/) yönlendirir
   POST /profile  -> LiveProfileReportService; canlı profile analizi
   ~~~

   `/` üzerindeki profile-shaped gönderim `/profile`'a **preserve-method
   (307)** ile yönlendirilir; kimlik yeniden gönderilen form body'sinde
   taşınır, query string'e yazılmaz. Gerekçe: profile analizi hesabın
   repository'lerine fan-out eder ve rate limit tüketir; refresh, browser
   back/forward, prefetch, crawler veya link-preview kaynaklı bir GET asla
   canlı toplama başlatmamalıdır. `/profile` POST'u ayrıca biçimi yeniden
   sınıflandırır, böylece doğrudan POST ile repository-shaped veya
   non-canonical bir değer profile akışına sokulamaz.
7. **Macro/micro ilişkisi değişmez.** Profile Proof macro view, Repository
   Proof micro view olarak kalır (§13.6).
8. **Tüm evidence/inference yasakları aynen geçerlidir.** Bu karar hiçbir
   evidence, pattern, scoring, role inference veya presentation kuralını
   değiştirmez.

Runtime sahibi: `GitHubEntryClassifier` (`src/GitHubProof.Web/Entry/`),
`GitHubEntryRoute`, `GitHubEntryKind`.

## 13.1 `/profile` Route Boundary

Yeni `Pages/Profile.cshtml` → route `/Profile` (`/profile`, Razor Pages
case-insensitive). `/` (`Pages/Index.cshtml`) birleşik giriş yüzeyini
taşır (§13.0) ve repository-shaped gönderimler için Repository Proof
akışını yerinde çalıştırır. İki route ayrı semantic boundary olmayı
sürdürür; hiçbiri diğerinin analiz akışını sarmalamaz. `/`'ın Profile
Proof'a yönlendirmesi (§13.0/6) bir *routing* adımıdır — iki analiz
semantics'ini birleştirmez.

## 13.2 Profile Identity Input Contract

Birleşik girişten gelen profile-shaped değer.

Web katmanı canonical parsing kurallarını yeniden implemente etmez ve parse
sonucundan kendi profile/repository identity'sini türetmez.
`GitHubEntryClassifier` yalnız acceptance preflight amacıyla mevcut canonical
parser'ı çağırır; parse sonucunu discard eder ve raw trimmed input'u seçilen
analysis boundary'ye taşır (§13.0/5).

Bu nedenle `GitHubEntryClassifier`'ın iki aşaması farklı şeyler yapar:
Stage 1'in segment sayımı bir doğrulama değil, yalnız canonical sahip
seçimidir; Stage 2 ise doğrulamayı kendisi yapmaz, canonical parser'ı
çağırıp yalnız kabul/ret cevabını kullanır.

Analiz için kayıt altındaki parse hâlâ Infrastructure'dadır: trim edilmiş
ham string `IGitHubProfileSnapshotCollector.CollectAsync(profile, ct)`'e
geçirilir ve `GitHubProfileHandle.Parse`,
`GitHubProfileSnapshotOrchestrator.CollectAsync` içinde çalışır. Repository
Proof'ta karşılık gelen analiz parse'ı `GitHubRepositoryUrl.Parse` olarak
`LiveRepositoryReportService` içinde çalışır. Preflight bu parse'ların
yerine geçmez; onları yalnız erkenden bir kez daha çağırır.

Geçersiz format `/profile`'a hiç ulaşmaz: birleşik giriş, aynı canonical
parser'ı çağırarak kabul kontrolünü zaten yapmıştır (§13.0/5) ve
`/profile` POST'u bunu bir kez daha doğrular. Bu nedenle page model'de
`ArgumentException` yakalayan bir catch **yoktur** — collector aynı string'i
aynı parser'la işlediği için bu yol ulaşılamazdır. Kullanıcıya görünen
format hatası, akışa özgü bir hata sayfasında değil, kabul edilen üç biçimi
listeleyen tek bir giriş-seviyesi mesajıyla birleşik girişte gösterilir.

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
tüm `_Repository*.cshtml` rapor partial'ları, `/` route'u, ve repository
analiz davranışı — `OnPostAsync`, `LiveRepositoryReportService.AnalyzeAsync`
çağrısı, `GitHubRepositoryUrl.Parse` doğrulaması, hata/partial mesaj
switch'i, repository micro-view render'ı. Bu davranış P10 tarafından
değiştirilmez: canonical bir repository URL'i gönderildiğinde çalışan analiz
akışı Session 095 öncesiyle aynıdır.

**Session 095 amendment.** Bu bölümün önceki hâli `RepositoryUrl` bound
property'sini ve ayrı repository-only giriş formunu da korunan semantics
listesine koyuyordu. §13.0'daki açık kullanıcı kararı bunu geçersiz kılar:

- bound property `IndexModel.RepositoryUrl` → `IndexModel.GitHubTarget`,
- giriş partial'ı `_RepositoryInput.cshtml` → `_GitHubInput.cshtml`,
- presentation modeli `RepositoryInputModel` → `GitHubInputModel`,
- alan `type="url"` → `type="text"` (bare login'i tarayıcı doğrulaması
  reddetmesin diye).

Bunlar **giriş yüzeyi** değişiklikleridir; repository analiz semantics'ini
değiştirmezler. P10-I4 drill-down'ı (§13.6) `/` girişini önceden doldururken
`GitHubTarget` alan adını kullanmalıdır.

`LiveRepositoryReportService`, `RepositoryReportProjector` ve tüm
`_Repository*.cshtml` rapor partial'ları değiştirilmez. Artık ayrı bir
"repository formu" ve ayrı bir "profile formu" yoktur; tek birleşik giriş
vardır (§13.0) ve iki analiz semantic boundary'si ayrı kalır.

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

## 13.7 Web EN/TR Locale Boundary (Session 101)

`ProfileReportProjector.Project(DeveloperPublicEvidenceProfile)` **locale
parametresi almaz** (`src/GitHubProof.Application/Profile/Report/ProfileReportProjector.cs`;
Session 087: "locale independence is structural"). Bu, Repository Proof'un
gerçek bir `SignalPresentationLocalizer` süren
`RepositoryReportProjector.Project(analysis, locale)`'ından farklıdır. P10
`ProfileReportProjector`'a locale parametresi eklemez ve Application
seviyesinde profile localization başlatmaz. Profile presentation mapping
tamamen Web katmanında kalır.

`GitHubProof.Web` yalnız `en` ve `tr` culture'larını destekler. Request culture
resolution sırası şöyledir:

```text
explicit persistent user choice (culture cookie)
→ Accept-Language browser preference / q order
→ English fallback
```

Browser negotiation ilk supported language family'yi seçer: `tr`, `tr-TR`,
`tr-*` → `tr`; `en`, `en-*` → `en`. Unsupported family'ler atlanır; supported
preference yoksa `en` kullanılır. Ordinary resolution'da query string yoktur.
Geolocation, IP-derived location, device location ve browser Geolocation API
language selection'a katılamaz.

Selected request culture bütün user-facing Web surface'i sürer: `<html lang>`,
header utilities, language selector, theme accessibility copy, landing/input,
validation, processing state, Repository Proof, Profile Proof Web
presentation, failures, rendered limitations/warnings ve accessibility
labels/status messages. Literal evidence ve canonical semantic identities
locale-independent kalır.

## 13.8 Deterministic Presentation-Language Mapping

Web-owned deterministic EN/TR presentation mapping
(`p10-presentation-language-contract.md` §8) yalnız zaten canonical/locale-
independent kimliklerle anahtarlanır: `ProfileReportSectionKey`, pattern
family/key, `ProfileLimitationCode`, `ProfileWarningCode`,
`DataAvailabilityStatus` ve mevcut typed availability/status codes. Report
contract §7.1/§9: "localized label != semantic truth" — eşitlik anahtarı her
zaman canonical identity'dir, etiket değildir.

Required labels:

| Canonical identity | EN | TR |
|---|---|---|
| `TechnicalFocusAreas` | Visible technical direction | Görünen teknik yön |
| `EngineeringPracticePatterns` | Recurring engineering practices | Tekrar eden üretim pratikleri |
| `RepresentativeRepositories` | Starting point | Başlangıç noktası |
| `WhyThisConclusion` | Why this view | Bu görünümü neden görüyoruz? |
| Profile Report | Profile view | Profil görünümü |

Technical-focus identities such as C#, JavaScript and Python are GitHub/literal
language values and are never translated. `ProblemAreaPatterns` remains
Unsupported and unrendered; `RoleEvidenceAlignment` remains Deferred and
unrendered.

## 13.9 TechnicalFocusAreas vs EngineeringPracticePatterns Ayrı Dil

- `TechnicalFocusAreas` → EN **"Visible technical direction"** / TR
  **"Görünen teknik yön"**
  (tekrar eden technical-focus evidence — hangi tür sistemler tekrarlanıyor).
- `EngineeringPracticePatterns` → EN **"Recurring engineering practices"** /
  TR **"Tekrar eden üretim pratikleri"**
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

Current P10 sequence (`P10-I1`, `P10-I2`, `P10-I2A`, `P10-I3`, `P10-I4`,
`P10-I5`, `P10-I6`) **sorumluluk/çıktı** bazında non-overlapping'dir — **dosya
sahipliği** bazında değil:

- Her slice'ın kendine ait, başka hiçbir slice'ın tekrar üstlenmediği bir
  sorumluluğu vardır (§7'de tanımlı: P10-I1 request/DI boundary, P10-I2
  routing/entry, P10-I2A shared Web locale/processing foundation, P10-I3
  presentation ordering, P10-I4 drill-down, P10-I5 failure/partial
  presentation, P10-I6 visual refinement).
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

## 13.17 Profile Runtime Collection Prerequisite (Session 096)

Canonical owner:
`docs/architecture/public-profile-runtime-collection.md` (CONTRACT LOCKED).

§13.4'teki Web composition boundary'si, Profile Proof'un her admitted
repository için **tam Repository Proof collection composition'ını** yeniden
kullanmasını gerektirmez ve gerektiriyormuş gibi okunmamalıdır.

Profile Proof, mevcut `IProfileRepositorySnapshotSource` seam'inde
Profile'a özgü bir collection composition kullanabilir. Bu prerequisite'in
P10 için pratik sonuçları:

- Profile V1 aggregation path'i bounded raw `SourceFiles`,
  `DependencyDeclarations` ve (Session 098) `Contributors` yüzeylerini
  **toplamaz** (runtime contract §5); bu omission asla negative evidence,
  warning, limitation veya status değişikliği üretmez (§5.2).
- Profile V1, gereken yüzeylerde karar geri döndürülemez hâle geldiğinde
  sayfalamayı durdurabilir (runtime contract §15, `DecisionBounded`); bu bir
  page cap, örnekleme veya yaklaşıklama değildir ve `IsComplete` asla sahte
  `true` yapılmaz.
- Bilinen fork'lar Profile V1'de admission aşamasında `Skip` / `ForkDetected`
  olur (runtime contract §16, eligibility §6.3); bu bir performans
  optimizasyonu değil, açık bir eligibility semantik kararıdır.
- Repository Proof collection path'i **değişmez** (§3.1) — `/` üzerinden
  çalışan micro-view analizi bu üç yüzeyi bugünkü gibi toplamaya devam eder ve
  hiçbir zaman `DecisionBounded` üretmez.
- Görünmez bir "ilk N repository" kısıtlaması yasaktır (§7); mevcut
  `GitHubProfileRepositoryCollector` sayfa bound'u ve completeness semantics'i
  değişmeden authoritative kalır.
- Concurrency bir request-cost optimizasyonu değildir (§8); P10 kapsamında
  fan-out authorize edilmez.
- Profile runtime telemetry (§9) ve bounded end-to-end execution guard (§10)
  production beta öncesi gereklidir; guard expiration asla `Complete` profile
  olarak sunulamaz. Guard expiration ile ordinary request cancellation ayrı
  şeylerdir (§11.1 / §11.2): mevcut request-cancellation davranışı —
  `OperationCanceledException`'ın propagate etmesi ve hiçbir analiz/pipeline/
  partial/failure sonucunun uydurulmaması — değişmez.

Bu bölüm hiçbir P10 slice'ının sorumluluğunu, presentation dilini, evidence
kuralını veya inference yasağını değiştirmez; yalnız runtime collection
prerequisite'ini işaret eder. Somut implementation ayrı bir authorized
session'a aittir.

**Session 098 amendment notu.** Yukarıdaki liste Session 098'in kilitlediği
kararlarla güncellendi: omission listesi artık üç yüzeydir, `DecisionBounded`
sayfalama durdurma ve fork admission kuralı eklendi. P10 slice sorumlulukları,
route'ları, presentation dili ve inference yasakları değişmedi.

## 13.18 P10-I3 Meaning Layer Synthesis Cross-Reference (Session 103)

Canonical owner:
`docs/product/p10-presentation-language-contract.md` §8.4 (Meaning layer
deterministic synthesis) and §8.5 (`Supported`-but-empty section copy).

Bu bölüm normatif metin taşımaz; yalnız işaret eder. Çelişki bulunursa
presentation-language contract kazanır.

- §13.8'in "yalnız canonical/locale-independent kimliklerle anahtarlanır"
  kuralı Meaning katmanını da kapsar: sentez copy'si iki presence bit'i
  (`TechnicalFocusAreas`, `EngineeringPracticePatterns`) artı selected request
  culture ile anahtarlanır, hiçbir zaman presentation text'iyle.
- `RepresentativeRepositories` Meaning state derivation'ına **katılmaz**. O,
  canonical pozisyon 5'te kendi kilitli etiketiyle (EN "Starting point" / TR
  "Başlangıç noktası") ayrıca render edilir ve kendi `Supported`-but-empty
  copy'sini taşır.
- §13.9'daki iki ayrı etiket kararı değişmedi; §13.10 (`ProblemAreaPatterns`
  Unsupported/unrendered) ve §13.11 (`RoleEvidenceAlignment`
  Deferred/unrendered) değişmedi.
- §13.15 Presentation Invariant aynen geçerlidir: P10 yeni analysis truth
  üretemez.
- `WhyThisConclusion` + drill-down P10-I4'ün, final Limitations/Partial/
  Failure UX ve `Warnings` bildirimi P10-I5'in sorumluluğunda kalır.
- P10-I3 kapsam clause'u (§7/P10-I3) Session 103'te düzeltildi: slice, typed
  `ProfilePipelineResult.Report`'u presentation'a taşımak için gereken minimum
  Web değişikliklerini kapsar; "yalnız yeni partial'lar" kısıtı geçersizdir.
