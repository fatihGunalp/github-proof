# GitHub Proof — Report Information Architecture

Bu belge, Phase 4 web ve Markdown raporlarının ortak bilgi mimarisini tanımlar.

Raporun ana sorusu:

> Bu repository dışarıdan bakıldığında neyi kanıtlıyor?

Rapor yaklaşık 10 saniye içinde şu iki cevabı görünür kılmalıdır:

1. Repository şu anda hangi görünür kanıtları sunuyor?
2. Kullanıcı önce hangi üç iyileştirmeyi yapmalı?

Bu belge sunum ve report composition sözleşmesidir. 15 canonical sinyali, dört canonical kategoriyi, evaluator kurallarını veya `RawApplicableScoreNormalization` yöntemini değiştirmez.

Türkiye araştırması ilk kullanıcı sancısını kalibre etmek için kullanılmıştır. GitHub Proof global üründür; bu rapor sözleşmesi hiçbir locale'e veya ülkeye özgü product semantics içermez.

---

## 1. Evidence-First İlkesi

Skor raporun ana kahramanı değildir.

İlk okuma katmanı şu sırayı izler:

```text
Repository identity / context
        ↓
What this repository proves       (en fazla 3)
        ↓
What weakens the evidence         (en fazla 3)
        ↓
Do these first                    (en fazla 3)
        ↓
Explicit uncertainty              (varsa)
        ↓
Category summary + secondary score
        ↓
Signal detail
        ↓
Limitations, versions, methodology, boundary disclaimer
```

Web ve Markdown farklı görsel sunum kullanabilir; ikisi aynı `RepositoryAnalysis` alanlarını, aynı seçilmiş sonuçları, aynı sırayı ve aynı anlamı temsil etmelidir.

---

## 2. Bölümler ve Kullanıcı Sancısı

### 2.1 Repository identity / context

Gösterilecek bilgiler:

- Owner ve repository adı
- Repository URL'si
- Default branch
- Analiz edilen commit SHA
- Snapshot zamanı
- Public, archived, fork ve template bağlamı
- Ana dil mevcutsa primary language
- Analysis status

Kullanıcı sancısı bağlantısı:

Raporun hangi repository ve hangi gözlem anı hakkında konuştuğunu sabitler. Recruiter veya teknik değerlendiricinin gördüğü dış yüzey ile raporun dayandığı snapshot'ın karışmasını engeller.

### 2.2 What this repository proves

En fazla üç güçlü görünür kanıt gösterilir.

Her madde:

- kısa ve iddia sınırı belli bir başlık,
- repository üzerinde gözlenen kanıtın özeti,
- neden önemli olduğu,
- ilgili signal key,
- evidence reference

taşır.

Bu bölüm geliştirici hakkında hüküm vermez. Örneğin “iyi test yazarı” demez; “repository test yapısını görünür kılıyor” der.

Kullanıcı sancısı bağlantısı:

“Projem karşı tarafa ne anlatıyor?” sorusunun doğrudan cevabıdır. Benzer projeler arasında yalnız kod miktarını değil, dışarıdan doğrulanabilir mühendislik pratiğini görünür kılar.

### 2.3 What weakens the evidence

En fazla üç kritik kanıt boşluğu veya zayıf kanıt gösterilir.

Her madde:

- eksik veya zayıf sinyali,
- incelemenin ne bulduğunu veya bulamadığını,
- bunun repository anlatısını neden zayıflattığını,
- ilgili recommendation'ı

gösterir.

`UnavailableEvidence`, `NotApplicable` ve `RequiresHumanReview` bu bölümde “eksik” gibi sunulmaz. Bunlar explicit uncertainty bölümüne gider.

Kullanıcı sancısı bağlantısı:

Kısa incelemede karşı tarafın nerede duraksayacağını görünür kılar. Kullanıcının uzun bir sinyal listesi içinden kritik problemi kendisinin çıkarmasını gerektirmez.

### 2.4 Do these first

En fazla üç öncelikli ve repository üzerinde uygulanabilir eylem gösterilir.

Her eylem:

- kesin priority,
- somut title,
- reason,
- expected impact,
- effort,
- related signals,
- evidence references

taşır.

Kullanıcı sancısı bağlantısı:

“Neyi önce düzeltmeliyim?” belirsizliğini giderir. Genel tavsiye listesi yerine başlanabilir üç iş verir.

### 2.5 Explicit uncertainty

Rapor, mevcut olduklarında şu durumları ayrı göstermelidir:

| Status | Kullanıcıya anlatılan anlam | Score davranışı |
|---|---|---|
| `Missing` | Gerekli veri başarıyla incelendi; görünür kanıt bulunamadı | `0`, denominator'a dahil |
| `NotApplicable` | Sinyal bu repository bağlamında uygulanabilir değil | `null`, hesap dışı |
| `UnavailableEvidence` | Gereken veri teknik veya erişim nedeniyle alınamadı | `null`, hesap dışı |
| `RequiresHumanReview` | Otomatik analiz güvenilir karar üretemedi | `null`, hesap dışı |

`Missing` kritik boşluk olabilir; diğer üç durum kanıt yokluğu gibi yorumlanamaz.

Kullanıcı sancısı bağlantısı:

Raporun sahte kesinlik üretmesini engeller ve kullanıcının “bulunamadı” ile “incelenemedi” arasındaki farkı anlamasını sağlar.

### 2.6 Category summary

Canonical kategoriler değişmez:

| Domain category | Önerilen kullanıcı etiketi | Kapsam |
|---|---|---|
| `RepositoryUnderstandability` | Documentation & Context | README, description, installation, license, usage/demo |
| `DevelopmentProcessVisibility` | Development Process | commit history/distribution, issue, pull request, release |
| `ValidationAndUsability` | Validation | test structure, Continuous Integration |
| `PortfolioPresentation` | Portfolio Presentation | topics, homepage/live demo, presentation completeness |

Her kategori:

- summary,
- status,
- normalized score varsa ikincil özet,
- earned/applicable maximum,
- limitation varsa kısa uyarı

gösterir.

“Collaboration” ayrı canonical kategori değildir. Issue ve pull request sinyalleri kullanıcı dilinde “collaboration evidence” alt temasıyla açıklanabilir; fakat yeni kategori üretilemez ve sinyaller `DevelopmentProcessVisibility` dışına taşınamaz.

Kullanıcı sancısı bağlantısı:

Detayları taranabilir dört kümeye böler. Skor burada teşhis için ikincil bir özet olur; repository'nin ana anlatısının yerine geçmez.

### 2.7 Signal detail

On beş signal result canonical sırada ve kategori altında gösterilir.

Her detay en az şunları taşır:

- display name ve signal key,
- status,
- score / maximum score yalnız score varsa,
- overall score'a dahil edilip edilmediği,
- evidence,
- explanation / why it matters,
- recommendation varsa,
- limitations,
- signal version,
- evaluated time

`portfolio.presentation-completeness` detayda ve kategori skorunda görünür; overall skora dahil olmadığı açıkça belirtilir.

Kullanıcı sancısı bağlantısı:

Özetin arkasındaki doğrulanabilir yolu sağlar. Kullanıcı veya teknik değerlendirici, rapor iddiasını kaynak sinyale ve kanıta kadar izleyebilir.

### 2.8 Product boundary disclaimer

Her format aynı anlamı taşıyan kısa ve tam sınır açıklamasını sunmalıdır:

> Bu rapor yalnızca analiz edilen public repository'de dışarıdan gözlemlenebilen sinyalleri değerlendirir. Geliştiricinin kıdemini, işe alınabilirliğini, kodun sahipliğini veya özgünlüğünü, AI kullanımını ya da genel geliştirici kalitesini belirlemez.

Kısa sınır cümlesi ilk okuma katmanında bulunabilir; tam disclaimer rapor sonunda zorunludur.

---

## 3. Deterministic Selection Contract

LLM report truth, score, highlight veya action seçmez.

### 3.1 Strong evidence selection

1. Yalnız `Strong` sonuçlar candidate olur.
2. Üçten az `Strong` varsa kalan yerler `Present` sonuçlarla doldurulur.
3. Her status tier kendi içinde şu sırayla sıralanır:
   - `Score / MaximumScore` azalan,
   - `MaximumScore` azalan,
   - canonical signal order artan.
4. Aynı tier içinde önce farklı kategorilerden en yüksek sıradaki birer sonuç seçilir.
5. Kalan yerler aynı sıralamadaki henüz seçilmemiş sonuçlarla doldurulur.
6. En fazla üç sonuç döndürülür.

`Partial` sonuç güçlü kanıt olarak sunulmaz.

### 3.2 Critical gap selection

1. `Missing` sonuçlar ilk candidate tier'dır.
2. Üçten az `Missing` varsa kalan yerler `Partial` sonuçlarla doldurulur.
3. `Missing` tier'ı:
   - `MaximumScore` azalan,
   - canonical signal order artan
   biçimde sıralanır.
4. `Partial` tier'ı:
   - `MaximumScore - Score` azalan,
   - `MaximumScore` azalan,
   - canonical signal order artan
   biçimde sıralanır.
5. Aynı tier içinde önce farklı kategorilerden en yüksek sıradaki birer sonuç seçilir; kalan yerler aynı sıralamadan doldurulur.
6. En fazla üç sonuç döndürülür.

`NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` critical gap candidate değildir.

### 3.3 Improvement action selection

1. Action yalnız seçilmiş `Partial` veya `Missing` critical gap ve en az bir evidence item'ı varsa üretilebilir.
2. İlk action candidate'ları seçilmiş critical gap sırasını izler.
3. `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` action değildir; explicit uncertainty olarak kalır.
4. Türetilmiş `portfolio.presentation-completeness` için ayrı action üretilmez; upstream gap action'ıyla çakışması önlenir.
5. Bir action en az bir related signal ve evidence reference taşır.
6. Priority değerleri `1..N` kesintisiz ve benzersizdir.
7. En fazla üç action üretilir.

MVP'de serbest metin action birleştirme veya LLM ile yeniden yazma yapılmaz. Birden fazla sinyali tek action altında birleştirmek ancak explicit deterministic template bulunduğunda mümkündür.

---

## 4. Mevcut Domain Alanlarının Kullanımı

| Rapor ihtiyacı | Mevcut alan |
|---|---|
| Repository identity/context | `RepositoryAnalysis.Repository`, `Snapshot.CapturedAt`, `Snapshot.HeadCommitSha`, `Status`, `Version` |
| What this repository proves | `RepositoryAnalysis.Strengths` + bağlı `SignalResult` ve `EvidenceItem` |
| What weakens the evidence | `RepositoryAnalysis.CriticalGaps` + bağlı `SignalResult` |
| Do these first | `RepositoryAnalysis.ImprovementPlan.Actions` |
| Category summary | `CategoryResults` |
| Secondary overall score | `ScoreBreakdown` |
| Signal detail | `SignalResults` |
| Explicit uncertainty | `SignalResult.Status`; localized status limitation; `Warnings` |
| Analysis limitations | `RepositoryAnalysis.LimitationKeys`, `Warnings` |
| Version/reproducibility | `AnalyzerVersion`, `RuleSetVersion`, `ReportSchemaVersion`, signal versions and timestamps |

---

## 5. Report Contract Durumu

Session 044 sonrasında semantic composition tarafının durumu:

1. `RepositoryAnalysisComposer`, canonical 15 result'ı doğrular, canonical sıraya koyar ve dört `CategoryResult` ile `ScoreBreakdown` üretir.
2. Strength, critical gap ve improvement action seçimi deterministic tier, category-diversity ve canonical tie-break kurallarıyla uygulanmıştır.
3. `AnalysisHighlight` ve `ImprovementAction` evidence reference'ları `EvidenceItem.Key` graph'ına karşı fail-fast doğrulanır.
4. Score band copy'si Domain'den çıkarılmış; `ScoreBreakdown.LabelKey` locale-independent semantic identity olmuştur.
5. Signal-level EN/TR catalog vardır; report section, score label, action, warning, limitation ve disclaimer catalog'ları henüz yoktur.
6. Product boundary disclaimer `RepositoryAnalysis` truth alanı değildir. Her renderer'ın kullandığı versioned report presentation policy içinde üretilmelidir.
7. Explicit uncertainty signal status, warnings ve limitation key'lerinden ortak projection'da türetilmelidir; Markdown ve Web UI ayrı filtre/ranking uygulamamalıdır.
8. `AnalysisHighlight.ScoreImpact` semantic seçim izidir; evidence-first ilk okuma katmanında görünür olmak zorunda değildir.

Kalan presentation/projection boşlukları yeni sinyal, yeni scoring yöntemi veya yeni canonical kategori gerektirmez.

---

## 6. Score-First ve Evidence-First Trade-off

| Yaklaşım | Güçlü yanı | Ana riski | MVP kararı |
|---|---|---|---|
| Score-first | Hızlı karşılaştırma ve tek sayı ile özet | Kullanıcıyı veya repository'yi bir nota indirger; neden ve ilk aksiyon görünmez kalır | Kullanılmayacak |
| Evidence-first | “Neyi kanıtlıyor?” ve “ne yapmalıyım?” sorularını doğrudan cevaplar; açıklanabilirliği güçlendirir | Daha fazla seçim/copy disiplini ve deterministic projection gerektirir | Ana yaklaşım |

Skor kaldırılmaz. Category summary içinde ve methodology/detail katmanında ikincil teşhis olarak tutulur.

---

## 7. MVP Öncelikleri

### MUST

- Evidence-first bölüm sırası
- En fazla 3 strength, 3 critical gap, 3 action
- Deterministic selection ve canonical tie-break
- Dört canonical kategori ve 15 canonical signal
- Explicit uncertainty ayrımı
- Signal detail alanlarının eksiksiz gösterimi
- `portfolio.presentation-completeness` overall exclusion açıklaması
- Web ve Markdown için ortak report projection / aynı `RepositoryAnalysis` anlamı
- Product boundary disclaimer
- LLM'siz report truth, selection ve scoring
- Language-neutral domain/report contract; locale değiştiğinde yeniden analiz yapılmaması
- TR ve EN presentation catalog'ları; English global fallback

### SHOULD

- Category ve signal kullanıcı etiketleri için merkezi presentation catalog
- İlk okuma katmanında kısa boundary cümlesi
- Archived/fork/template/partial analysis için bağlamsal notice
- Evidence reference doğrulaması
- Secondary score için “görünür kanıt özeti” açıklaması
- Locale fallback ve locale-switch renderer testleri

### LATER

- Recruiter-specific view
- Repository comparison
- PDF, share card, badge ve public report URL
- Interactive filtering, collapse/expand ve deep-link
- LLM destekli açıklama kişiselleştirmesi
- Tarihsel skor veya before/after görünümü

---

## 8. Language-Neutral Report Contract

### 8.1 Ayrım

`RepositoryAnalysis` repository hakkında dil bağımsız gerçeği taşır. Locale, culture, translated heading veya kullanıcıya görünen sentence domain truth değildir.

```text
RepositorySnapshot + deterministic evaluation
        ↓
RepositoryAnalysis                         ← locale yok
        ↓
Report projection + locale + presentation catalog
        ↓
TR Web / EN Web / TR Markdown / EN Markdown
```

Locale değiştirmek yeni GitHub collection, evaluator çalıştırma, scoring, highlight selection veya improvement ranking üretmez.

### 8.2 Sabit semantic keys

Report section identity şu stable key'lerle temsil edilir:

```text
report.repository-identity
report.what-this-repository-proves
report.what-weakens-the-evidence
report.do-these-first
report.explicit-uncertainty
report.category-summary
report.signal-detail
report.limitations-and-warnings
report.methodology-and-version
report.product-boundary
```

Bu key'ler display heading değildir. TR ve EN başlıkları presentation catalog tarafından üretilir.

### 8.3 Uygulanan signal presentation contract

Signal düzeyindeki kullanıcı copy'si Application katmanında aşağıdaki projection ile temsil edilir:

```text
SignalPresentation
├── Locale
├── SignalKey
├── Title / Description
├── Status / StatusLabel
├── Score / MaximumScore
├── Explanation / WhyItMatters
├── Recommendation / Limitations
├── Evidence
└── SignalVersion
```

`SignalPresentationLocalizer`, `SignalKey + Status + semantic result fields` girdisinden source-controlled EN/TR `.resx` catalog'larını kullanır. Bu projection analysis truth değildir ve yeniden analysis gerektirmez.

Session 043 migration sonucu:

| Eski alan | Yeni language-neutral contract |
|---|---|
| `SignalDefinition.Name`, `Description`, templates | kaldırıldı; signal presentation identity `SignalDefinition.Key` |
| Prose rule listeleri | evaluator implementation + versioned `ApplicabilityRuleKey`, `EvaluationRuleKey`, `EvidenceRequirementKey` |
| `SignalResult.Explanation`, `Recommendation`, `Limitations` | kaldırıldı; `SignalPresentationLocalizer` projection'ı |
| `CategoryResult.Summary`, `Limitations` | kaldırıldı; composer sonrası report projection sorumluluğu |
| `AnalysisHighlight.Title`, `Summary` | kaldırıldı; `Type + SignalKey + EvidenceReferences` semantic identity |
| `ImprovementAction.Title`, `Reason`, `ExpectedImpact` | `PresentationKey`; priority, effort, related signals ve evidence refs locale dışı |
| `AnalysisWarning.Message` | `PresentationKey` |
| `RepositoryAnalysis.Limitations` | `LimitationKeys` |

Bu değişiklikte `SignalKey`, status, category, score, evidence, maximum score, rule semantics, version, timestamp, action priority ve evidence reference asla localized copy'ye dönüşmez.

### 8.4 Literal repository evidence

Repository'den gelen literal değerler çevrilmez:

- file path ve filename,
- command ve code block,
- URL, branch, commit SHA ve tag,
- README heading veya repository description alıntısı,
- issue/PR/release title ve repository metadata değeri,
- evidence key/value.

Yalnız bu değerlerin çevresindeki label, explanatory sentence ve UI heading localized edilir.

### 8.5 TR + EN MVP

MVP presentation catalog'ları en az şunları sağlar:

```text
en  = canonical global default ve fallback
tr  = first-launch locale
```

Locale çözümü renderer girişinde yapılır. İstenen locale desteklenmiyorsa `en` fallback uygulanır; analysis state değişmez. TR ve EN aynı semantic key setini eksiksiz kapsamalıdır.

AI/LLM translation, scoring, report truth, selection, ranking veya canonical copy kaynağı değildir. MVP'de copy sürümleri source-controlled presentation catalog'lardan gelir.

### 8.6 Locale invariants

Bir `RepositoryAnalysis` için TR ve EN render'ları arasında şu alanlar birebir aynı kalmalıdır:

- repository identity ve raw evidence literal'leri,
- 15 signal key, status, score, maximum score ve version,
- category result ve score breakdown,
- selected strengths, critical gaps ve improvement action identity/priority,
- uncertainty sınıflandırması,
- analysis/rule/report schema versions.

Yalnız heading, explanatory label, prose, action copy ve disclaimer localized olabilir.

---

## 9. Session 044 Implementation Result

`RepositoryAnalysisComposer`, `RepositorySnapshot` ve canonical 15 semantic `SignalResult` girdisinden locale kabul etmeden graph-consistent `RepositoryAnalysis` üretir.

Uygulanan contract:

- exact canonical key/metadata/order doğrulaması ve missing/duplicate/unknown fail-fast,
- dört canonical category ve unchanged `RawApplicableScoreNormalization`,
- evidence-backed, deterministic ve en fazla üç strength/gap/action,
- uncertainty status'larının gap/action dışında tutulması,
- `portfolio.presentation-completeness` category-included / overall-excluded davranışı,
- stable action, warning, limitation ve score-label semantic key'leri,
- highlight/action evidence reference bütünlüğü,
- explicit analysis id/version/start/completion girdileriyle gizli clock/id olmadan deterministic composition.

Composer localized prose üretmez, `SignalPresentationLocalizer` çağırmaz ve evaluator çalıştırmaz. Markdown renderer, Web UI ve locale-specific report projection Session 044 kapsamına alınmamıştır.

---

## 10. Session 045 Implementation Result — Localized RepositoryReport Projection

Session 045 tamamlanmış tek bir `RepositoryAnalysis` ve requested locale girdisinden, analiz truth'unu değiştirmeden ortak localized `RepositoryReport` projection üretir.

~~~text
RepositoryAnalysis + requested locale
                ↓
        RepositoryReport
~~~

Projection repository collection, evaluator, scoring, category aggregation, strength/gap/action selection veya ranking çalıştırmaz.

### Uygulanan contract

- Markdown ve ilerideki Web UI'ın ortak kullanacağı typed `RepositoryReport` projection contract'ı eklendi.
- Canonical evidence-first section order sabitlendi:
  identity → proves → weakens → do first → explicit uncertainty → categories/secondary score → signal detail → disclaimer.
- Report-level EN/TR source-controlled resource catalog eklendi.
- Unknown locale deterministic olarak `en` fallback kullanır ve projection resolved locale'i taşır.
- Mevcut `SignalPresentationLocalizer`, canonical 15 signal detail için yeniden kullanılır.
- Score label, analysis/category status, effort, improvement action, warning, limitation ve section copy report presentation boundary'sinde çözülür.
- Product-boundary disclaimer `PolicyVersion = 1` ile versioned presentation policy olarak taşınır.
- Disclaimer seniority, employability, authorship, originality, AI usage ve overall developer quality inference yapılmadığını açıkça belirtir.
- Explicit uncertainty yalnız `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` sonuçlarından canonical signal order ile türetilir.
- `Missing`, critical-gap akışında kalır ve uncertainty olarak yeniden sınıflandırılmaz.
- Strength, critical-gap ve action projection sırası composer çıktısından aynen korunur; projection yeniden ranking yapmaz.
- Repository URL, branch, SHA, language, evidence path, filename, command, URL ve diğer literal değerler locale değişiminde değiştirilmez.
- `PortfolioPresentation` category label'ındaki pre-existing `Collaboration` / `İş Birliği` presentation hatası canonical kategoriyle uyumlu `Portfolio Presentation` / `Portföy Sunumu` olarak düzeltildi.
- Yeni third-party dependency, MediatR, CQRS, LLM translation veya LLM report truth eklenmedi.

### Validation

~~~text
Application tests: 39/39 PASS
Domain tests: 422/422 PASS
Infrastructure tests: 101/101 PASS
Total: 562/562 PASS
Forced rebuild: PASS — 0 warnings, 0 errors
dotnet format --verify-no-changes: PASS
git diff --check: PASS
~~~

Session 045 ile 8 yeni localized report projection testi eklendi.

Doğrulanan davranışlar:

- aynı `RepositoryAnalysis` → EN ve TR copy farklı, semantic identity/order/score/status/evidence aynı,
- locale switch hiçbir analyzer/composer rerun'ı yapmaz,
- unsupported locale → EN fallback,
- EN/TR catalog key completeness ve boş copy bulunmaması,
- evidence path, filename, command, URL, SHA ve repository literal'lerinin aynen korunması,
- exact evidence-first section order,
- maksimum 3 strength/gap/action projection'ının composer sırasını koruması,
- all uncertainty statuses'ın failure copy'sine dönüşmemesi,
- full product-boundary disclaimer'ın iki locale'de bulunması.

### Session 045 out of scope olarak kalanlar

- Markdown string renderer
- HTML/Web UI
- PDF
- LinkedIn card
- yeni evaluator
- scoring/ranking değişikliği
- validation-011
- LLM-generated report truth

---

## 11. Session 046 Exact Handoff — Markdown Report + Phase 4 Exit Validation

Session 046, Session 045'in typed localized projection'ını tüketmelidir:

~~~text
RepositoryReport
      ↓
Deterministic Markdown report
      ↓
Phase 4 exit validation
~~~

### MUST implement

1. `RepositoryReport` dışında analysis input'u kabul etmeyen deterministic Markdown renderer.
2. Canonical section order aynen korunmalı:
   identity → proves → weakens → do first → uncertainty → categories/secondary score → signal detail → disclaimer.
3. Strength, critical-gap ve action sırası renderer tarafından yeniden hesaplanmamalı.
4. Skor ilk okuma katmanının kahramanı yapılmamalı; category/overall score evidence-first bölümlerden sonra secondary summary olarak kalmalı.
5. `UnavailableEvidence`, `NotApplicable` ve `RequiresHumanReview` Markdown'da failure/missing gibi sunulmamalı.
6. Repository/evidence literal değerleri — path, filename, command, URL, SHA, branch ve repository kimliği — değiştirilmeden render edilmeli.
7. Product-boundary disclaimer Markdown çıktısında zorunlu olmalı.
8. Aynı `RepositoryReport` aynı Markdown çıktısını üretmeli.
9. EN ve TR `RepositoryReport` örneklerinin ikisi de render edilebilmeli; Markdown renderer localization yapmamalı.
10. Phase 4 sonunda full solution regression, build, format ve report determinism doğrulaması yapılmalı.

### Phase 4 exit expectation

Phase 4 tamamlandığında:

~~~text
RepositoryAnalysis
        ↓
localized RepositoryReport
        ↓
Markdown output

ve aynı RepositoryReport
        ↓
Phase 5 Web UI
~~~

olmalıdır.

Web UI, Markdown renderer'ın içinden türetilmemeli; ikisi aynı typed `RepositoryReport` contract'ının bağımsız consumer'ları olmalıdır.

### Required tests

- same RepositoryReport → byte-for-byte same Markdown,
- exact section order,
- EN/TR localized projection render,
- max-three strength/gap/action sırasının korunması,
- explicit uncertainty ayrımı,
- repository/evidence literal preservation,
- secondary-score positioning,
- disclaimer presence,
- empty optional sections için deterministic davranış,
- Markdown renderer'ın analyzer/composer/localizer çağırmaması,
- full existing regression.

### Out of scope

HTML/Web UI, PDF, LinkedIn card, public report hosting, yeni evaluator, scoring/ranking değişikliği, validation-011, new dependency, MediatR, CQRS ve LLM report generation Session 046 kapsamına alınmamalıdır.