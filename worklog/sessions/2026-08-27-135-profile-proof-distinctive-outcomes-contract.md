# Profile Proof — Distinctive Outcomes Product Contract

**Date:** 2026-08-27
**Result:** DOCS
**Scope:** docs (product contract)
**Model Route:** Opus / high
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

"GitHub Proof neden var?" sorusunu outcome seviyesinde kesin bir ürün
sözleşmesine çevirmek: bir Profile Proof sonucundaki hangi 3-5 bilginin
GitHub'ın kendi arayüzü veya generic bir AI okuması karşısında yapısal olarak
ayrışmak zorunda olduğunu, mevcut canonical contract'lara ve gerçek code/
projection yeteneğine karşı test ederek kilitlemek.

Bu bir product/architecture-level ürün kararı session'ıdır. Implementation,
UI/UX veya domain değişikliği değildir.

---

## Starting State

~~~text
Branch:
session/135-profile-proof-distinctive-outcomes-contract

Base HEAD:
9c4c90d  (main == origin/main == HEAD doğrulandı)

Pre-existing Working Tree:
?? artifacts/   (kullanıcıya ait, dokunulmadı)

Relevant Previous Session:
worklog/sessions/2026-08-27-134-milestone-b-production-candidate-runtime.md
~~~

---

## Model Route

~~~text
Selected:
Opus / high

Why:
Cross-contract product decision; yeni normative semantic üretme riski ve
canonical ownership/duplication riski yüksek.

User Approval:
devam et  (prompt içinde APPROVED olarak verildi)
~~~

---

## Model Escalation

~~~text
None.
~~~

---

## Context Read

~~~text
AGENTS.md
CLAUDE.md
agents/02-context-loading.md
agents/03-project-contract.md
agents/06-review-and-validation.md
agents/07-worklog-integration.md
worklog/CURRENT.md
worklog/INDEX.md (legend + son satırlar)
docs/product/product-positioning.md
docs/product/public-profile-evidence-product-contract.md
docs/product/p10-presentation-language-contract.md §7, §8.4, §8.5, §8.6
docs/architecture/public-profile-evidence-patterns.md
docs/architecture/public-profile-representative-repositories.md §1-§11
docs/architecture/public-profile-evidence-report-contract.md
docs/architecture/public-profile-evidence-domain-model.md §13
docs/architecture/public-profile-evidence-repository-eligibility.md §6, başlık haritası
docs/architecture/public-profile-runtime-collection.md §7, §15
src/GitHubProof.Application/Profile/Report/*
src/GitHubProof.Application/Profile/ProfileComposer.cs
src/GitHubProof.Domain/Profile/Enums/ProfileLimitationCode.cs, ProfileWarningCode.cs
src/GitHubProof.Domain/Profile/Models/RepeatedProfilePattern.cs
src/GitHubProof.Infrastructure/GitHub/Profile/GitHubProfileRepositoryCollector.cs
src/GitHubProof.Web/Pages/Profile.cshtml ve ilgili partial'lar
~~~

---

## Contract / Invariants Checked

~~~text
- Repetition threshold patterns §5'in sahipliğinde (IndependentSupportingRepositoryCount >= 2);
  yeni numeric threshold seçilmedi, kod sabiti doğrulandı
  (RepeatedProfilePattern.MinimumIndependentSupportingRepositories = 2).
- Profile-level developer/candidate score yasağı (product contract §15) korunuyor;
  score moat olarak kullanılmadı.
- Representative repository ranking'e dönüştürülmedi (rep contract §2, §3.2, §8).
- İki aşamalı trace korunuyor: Why = supporting repository kimlikleri,
  RepositoryDrillDown = repository başına tek Repository Proof geçişi
  (p10 §8.6.3-§8.6.5).
- Unsupported / Deferred / Supported+empty üç durumu tek duruma indirilmedi
  (report contract §6).
- Coverage iddiası "declared" ile sınırlandı; manifest declared bir collection
  bound taşıyabilir (runtime collection §7 + collector maxPageCount).
- ProblemArea Unsupported, RoleEvidenceAlignment Deferred — outcome sayılmadı.
- Yasaklı developer inference'ların canonical sahiplerine pointer verildi,
  yeni listede tekrar edilmedi.
~~~

---

## Changes

~~~text
Created:
- docs/product/profile-proof-distinctive-outcomes-contract.md
- worklog/sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md

Modified:
- docs/product/product-positioning.md          (§9 owner tablosuna 1 satır pointer)
- docs/product/public-profile-evidence-product-contract.md
                                               (amendment history + §4 pointer paragrafı)
- worklog/INDEX.md
- worklog/CURRENT.md

Deleted:
- none
~~~

---

## Decisions

- **Canonical owner:** distinctive-outcome semantiği mevcut hiçbir contract'ın
  doğal sahipliğinde değildi. `public-profile-evidence-product-contract.md`
  CONTRACT LOCKED ve capability semantiğinin sahibi; `product-positioning.md`
  epistemik sınırın sahibi; ikisi de "hangi sonuçlar yapısal olarak ayrışmak
  zorunda" sorusunu sahiplenmiyor. Uzun ömürlü ve UI/UX çalışmasının owner'ı
  olacağı için yeni bir `docs/product` contract'ı açıldı; diğer iki dokümana
  yalnız pointer eklendi.
- **Final set 4 outcome:** 3 CORE (cross-repository recurring evidence;
  traceable conclusion-to-repository evidence chain; declared evidence surface
  and its boundary) + 1 SUPPORTING (pattern-scoped starting point).
  **Correction pass:** priority etiketleri ilk yazımda `P0`/`P1` idi;
  repository genelinde `P<n>` zaten Profile **phase** kimliği olduğu için
  (patterns/report/representative contract'larındaki "P1 result ownership,
  P2 eligibility semantics, P5 observation semantics") `CORE`/`SUPPORTING`
  ile değiştirildi ve numeric priority notasyonu contract'ta yasaklandı.
- **Coverage-aware yerine coverage-declared.** Collector declared bir
  `maxPageCount` bound taşıyor (varsayılan 10 sayfa x 100), bu manifest'i
  incomplete yapıp profile-level `Partial` ve `PartialEvidenceSurface`
  üretiyor. "Tam coverage" iddiası bu yüzden contract'ta yasaklandı.
- **Evidence convergence reddedildi (NOT SUPPORTED).** patterns §7.2'de yalnız
  `documentation` iki signal'e map'lenir ve union repository başına bir kez
  sayılır; bu repetition'dır, ayrı bir convergence semantiği değil. Yeni domain
  semantics + yeni `PatternRuleVersion` gerektirir; uydurulmadı, owner kararına
  bırakıldı.
- **AI özet outcome sayılmadı.** Mevcut Meaning katmanı distinctive çünkü
  promoted pattern'ler üzerinde deterministik bir şablon; prose olduğu için
  değil. Outcome 1'in sunumu olarak sınıflandırıldı.
- **First-read wow contract bilgi önceliği olarak yazıldı**, layout olarak
  değil. Supporting repository breadth: ilişki olarak ilk okumada zorunlu,
  kimlik olarak yasak — p10 §8.4.7/§8.6.1-§8.6.3'ün mevcut kararı teyit edildi,
  değiştirilmedi.

- **Outcome 4 yeniden sınıflandırıldı (correction pass).** İlk yazımda
  `SUPPORTED NOW` idi. Web doğrulaması `RepresentativePatternSupport`'un
  hiçbir yerde render edilmediğini gösterdi — başlangıç noktası yüzeyi yalnız
  repository kimliği, GitHub description'ı ve analiz eylemi sunuyor. Model
  capability var, distinctive user-visible sonuç yok → `PARTIALLY SUPPORTED`.
  Eksik gereksinim UI CONSEQUENCE'a taşındı. Yeni seçim semantics'i üretilmedi.
  **Final correction pass (F-N1):** bu gereksinim ilk düzeltmede yanlışlıkla
  p10 §8.6.1a'daki **Starting Point Prioritization Contract Gap**'e
  bağlanmıştı. O gap canonical seçim üzerinde importance/priority *ordering*
  — yani ranking — konusudur ve bu contract ranking'i açıkça yasaklar.
  Atfıf kaldırıldı; doğru kayıt şudur: pattern → başlangıç-repository
  ilişkisinin görünmemesi **bugün hiçbir canonical dokümanın kayıtlı gap'i
  değildir** ve görünür kılınması p10 §8.6.1'in explicit bir amendment'ini
  gerektirir (o bölüm "renders exactly" üç öğeyi kilitliyor).
- **Moat daraltıldı (correction pass).** İlk yazımdaki son cümlecik
  "promotion sonuç üretmedi" ile "evidence surface daraldı"yı tek bir "sınır"
  kavramına çöktürüyordu. Domain model §13 bu ikisinin ayrı kalmasını zorunlu
  kılıyor ve boş Limitations durumunda ürün zaten "sınır kaydedilmedi" diyor.
  Moat ikiye ayrıldı; `Supported + empty` artık limitation olarak
  adlandırılmıyor ve yeni limitation kimliği tanımlanmadı.
- **Evrensel trace iddiası daraltıldı (correction pass).** "Her profile-level
  sonuç" üç yerde geçiyordu ve `ProfileSummaryFact` / `ProfileLimitation` /
  `ProfileWarning` için literal yanlıştı (bunlar destekleyici repository
  taşımıyor). İddia "promote edilmiş tekrar eden pattern sonuçları" olarak
  daraltıldı ve dışarıda kalan yüzeyler açıkça listelendi.
- **First-read sahipliği mutabakata bağlandı (correction pass).**
  `public-profile-evidence-product-contract.md` §25 genel Profile first-read
  hedefinin sahibi olarak kalır; yeni contract §8 yalnız distinctive
  outcome'ların o çerçeve içinde algılanabilir olması kuralını sahiplenir.
  §25'in beş sorusu tekrar edilmedi.

Yeni ürün/domain kuralı üretilmedi. Yeni threshold, yeni taxonomy, yeni
limitation kimliği, yeni availability durumu veya yeni presentation copy yok.

---

## Validation

~~~text
git diff --check
PASS — whitespace hatası yok

git status --short
PASS — yalnız beklenen docs/worklog değişiklikleri + pre-existing ?? artifacts/

PROHIBITED INFERENCE SCAN
PASS — yeni contract'ta junior/mid/senior, score, hire/reject, employability,
       candidate quality, inferred role, expert, "good/bad developer",
       "production-ready developer", AI authorship, originality iddiası yok.
       Yasaklı çıkarımlar yalnız canonical owner'lara pointer ile bırakıldı.

CANONICAL DUPLICATION CHECK
PASS — threshold, taxonomy, availability semantiği, limitation emission koşulu,
       representative seçim kuralı ve exact copy yeniden tanımlanmadı; her biri
       owner referansıyla tüketildi. Diğer iki dokümana yalnız pointer eklendi.

GENERIC AI DIFFERENTIATION CHECK
PASS — dört outcome'un her biri GENERIC AI CAN / GITHUB PROOF STRUCTURALLY ADDS
       / USER-VISIBLE DIFFERENCE / WHY USER CARES alanlarını dolduruyor;
       hiçbiri yalnız "deterministic" gerekçesine dayanmıyor.

CAPABILITY CLAIM CHECK
PASS (review sonrası düzeltilmiş hali) — doğrulama iki katmanda yapıldı:
       (a) canonical/domain/application semantics
           (ProfileReport bölüm availability sabitleri, ProfileReportProjector
           drill-down projeksiyonu, ProfileComposer limitation/status türetimi,
           RepeatedProfilePattern threshold sabiti,
           RepresentativeRepositorySelectionPolicy),
       (b) Web / user-visible projection
           (Profile.cshtml, _ProfileMeaning, _ProfilePatternSection,
           _ProfileWhyThisView, _ProfileRepositoryEvidence,
           _ProfileStartingPoint, ProfileWebPresentationLocalizer).
       (b) katmanı ilk geçişte yapılmamıştı ve F-1'i ortaya çıkardı.

       Düzeltilmiş sınıflandırma:
       Outcome 1 — SUPPORTED NOW
       Outcome 2 — SUPPORTED NOW, fresh-run niteliğiyle (son hop dondurulmuş
                    analiz kopyası değil, fresh Repository Proof çalıştırması)
       Outcome 3 — SUPPORTED NOW, §9'da zaten kayıtlı evidence-surface
                    taxonomy sınırlamasıyla (yüzeyin neden daraldığı ayrılmıyor)
       Outcome 4 — PARTIALLY SUPPORTED (RepresentativePatternSupport Web'de
                    hiç render edilmiyor; distinctive pattern ilişkisi
                    kullanıcıya görünür değil)

dotnet build / dotnet test
NOT RUN — docs/worklog-only session; production code, test veya config
          değişmedi (agents/06 §3).
~~~

---


## Review

~~~text
REVIEW RESULT: CHANGES REQUIRED
BLOCKER: 0
HIGH:    3  (F-1, F-2, F-3)
MEDIUM:  3  (F-4, F-5, F-6)
LOW:     4  (F-7, F-8, F-9, F-10)
~~~

F-1 Outcome 4 `SUPPORTED NOW` yanlıştı — `RepresentativePatternSupport` Web'de
render edilmiyor. F-2 moat son cümleciği `Supported + empty` ile
`ProfileLimitation`'ı çöktürüyordu. F-3 "her sonuç" evrensel trace iddiası
`ProfileSummaryFact` / `ProfileLimitation` / `ProfileWarning` için literal
yanlıştı. F-4 son hop'un fresh-run niteliği eksikti. F-5 `P0`/`P1` repository'de
zaten Profile phase kimliği. F-6 first-read sahipliği §25 ile mutabakatsızdı.
F-7 count yasağı citation'ı yanlış katmanı gösteriyordu. F-8 örnek blokları
copy sanılabilirdi. F-9 worklog capability kaydı. F-10 header alan adı.

**On bulgunun tamamı bu aynı-session correction pass'inde kapatıldı.**
Outcome seti değişmedi (dört outcome); evidence convergence `NOT SUPPORTED`
olarak kaldı; hiçbir production/test/domain değişikliği yapılmadı.

### Review Authority — zorunlu kayıt

Session 135'i yazan Claude ajanı **aynı zamanda** yukarıdaki review raporunu
üreten ajandır. `AGENTS.md` §6 ve `agents/06-review-and-validation.md` §7
gereği yazan ajan kendi işi için closure review authority sayılmaz.

Bu nedenle o rapor **geçerli finding input'tur, final independent closure
authority değildir.**

~~~text
INDEPENDENT REVIEW: PENDING
SESSION 135 CLOSURE: PASS OLARAK İŞARETLENMEDİ
~~~

Gerçekten bağımsız bir closure review farklı bir review agent/context tarafından
yapılmalıdır.


### Final Independent Closure Review (forked cold context)

~~~text
REVIEW RESULT: PASS WITH FINDINGS
BLOCKER: 0
HIGH:    0
MEDIUM:  1  (F-N1)
LOW:     3  (F-N2, F-N3, F-N4)
~~~

Önceki F-1..F-10'un tamamı **CLOSED** olarak doğrulandı; dört outcome'luk ürün
modeli **ACCEPTED**; capability sınıflandırmaları gerçek koda karşı teyit edildi.

Bu ikinci correction pass'te kapatılanlar:

- **F-N1 (MEDIUM) — CLOSED.** Outcome 4 UI CONSEQUENCE'taki yanlış gap atfı
  kaldırıldı; pattern ilişkisinin kayıtlı bir canonical gap olmadığı ve p10
  §8.6.1 amendment'i gerektirdiği açıkça yazıldı; anti-ranking kuralı korundu.
- **F-N2 (LOW) — CLOSED.** §9 Outcome 1 gerekçesi daraltıldı: practice tarafında
  tekrar ilişkisi birincil olarak Meaning katmanından algılanıyor.
  `SUPPORTED NOW` değişmedi.
- **F-N3 (LOW) — CLOSED.** Outcome 4 DETERMINISM/COVERAGE kurallarındaki
  canonical algoritma tekrarı pointer + **CONSUMED, NOT OWNED** özetiyle
  değiştirildi; ikinci owner riski kaldırıldı.
- **F-N4 (LOW) — CLOSED.** §8.2'deki geniş "count yok" kısıtı artık açıkça bu
  contract'ın kendi kuralı olarak işaretli; p10 §8.4.7 ve §8.6.3 kendi
  katmanlarının sahibi olarak ayrı ayrı gösteriliyor.

### Review Authority — son durum

Bu closure review, Session 135'i yazan context'in **dışında**, forked bir cold
context'te çalıştırıldı ve bulguları dosyalar üzerinden ayrıca doğrulandı.
Ancak Session 135'i yazan ajan ile bu review'ı orkestre eden ajan aynı
oturumdadır; **önceki author-context review hiçbir zaman independent olarak
kaydedilmemiştir ve öyle sayılmaz.**

~~~text
SESSION 135 CLOSURE: READY FOR ARCHITECTURE OWNER RATIFICATION
~~~

Final closure kararı architecture owner'a aittir.

## Problems

~~~text
None.
~~~

---

## Not Done

~~~text
- UI/UX tasarımı yapılmadı (kart, grid, renk, tipografi, animasyon yok).
- Evidence convergence implement edilmedi; NOT SUPPORTED olarak kaydedildi.
- Limitation taksonomisinin "neden daraldı" ayrımı açılmadı; domain model §13
  spekülatif kimlik eklemeyi yasakladığı için owner kararına bırakıldı.
- Hiçbir production/domain/application/web kodu veya testi değiştirilmedi.
- Commit / push / merge yapılmadı.
- `agents/02-context-loading.md` P10 route'u yeni contract'ı listelemiyor.
  Bu correction pass'te **bilerek** dokunulmadı: `agents/` değişikliği kendi
  bounded session'ına aittir ve bu docs/product düzeltmesi agent-routing işine
  genişletilmez. Ayrı follow-up olarak kaydedildi.
~~~

---

## Known Risks

~~~text
- Contract, "declared evidence surface" iddiasını collector'ın mevcut
  maxPageCount davranışına dayandırıyor. Bu bound ileride değişirse Outcome 3'ün
  COVERAGE RULE ifadesi yeniden okunmalıdır (contract semantiği değişmez, ama
  gerçeklikle hizası doğrulanmalıdır).
- Contract'ın **gerçekten bağımsız** closure review'ü henüz yapılmadı. Bu
  session içindeki review raporunu Session 135'i yazan ajan üretti; bulguları
  geçerlidir ama closure authority değildir (bkz. Review bölümü).
- Outcome 4 `PARTIALLY SUPPORTED`; distinctive değerinin kullanıcıya ulaşması
  sonraki UI/UX session'ına bağlı.
~~~

---

## Working Set / Final Git State

~~~text
Modified:
- docs/product/product-positioning.md
- docs/product/public-profile-evidence-product-contract.md
- worklog/CURRENT.md
- worklog/INDEX.md

Untracked (bu session):
- docs/product/profile-proof-distinctive-outcomes-contract.md
- worklog/sessions/2026-08-27-135-profile-proof-distinctive-outcomes-contract.md

Pre-existing unrelated:
- artifacts/   (kullanıcıya ait; dokunulmadı)
~~~

---

## Next

Gerçekten bağımsız bir Session 135 closure review'ünü **farklı bir review
agent/context** ile çalıştır; ardından bu contract'ı input alan bounded bir
Profile first-read UI/UX session'ı aç (ana iş: Outcome 4'ün pattern
ilişkisini görünür kılmak).

---

## Git

~~~text
Suggested commit:
docs(profile): lock Profile Proof distinctive outcomes contract

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
~~~

---

## Completion Checklist

~~~text
[x] Goal tamamlandı
[x] Docs-only validation çalıştı (git diff --check, status, semantic taramalar)
[x] Broader validation gerekmedi (production code/test değişmedi)
[x] git diff --check kontrol edildi
[x] git diff/status gözden geçirildi
[x] Contract/invariant drift kontrol edildi
[x] Session Result güncellendi (DOCS)
[x] INDEX aynı satır güncellendi
[x] CURRENT.Active Session güncellendi
[x] CURRENT.Exact Next Action güncellendi
[x] Kullanıcıya kısa final rapor verildi
~~~
