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

1. Action yalnız `Partial`, `Missing` veya `RequiresHumanReview` sonucu, onun recommendation'ı ve en az bir evidence item'ı varsa üretilebilir.
2. İlk action candidate'ları seçilmiş critical gap sırasını izler.
3. Kalan yer varsa `RequiresHumanReview` sonuçları canonical signal order ile eklenebilir.
4. Bir action en az bir related signal ve evidence reference taşır.
5. Priority değerleri `1..N` kesintisiz ve benzersizdir.
6. En fazla üç action üretilir.

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
| Explicit uncertainty | `SignalResult.Status`, `Score`, `Limitations`; `Warnings` |
| Analysis limitations | `RepositoryAnalysis.Limitations`, `Warnings` |
| Version/reproducibility | `AnalyzerVersion`, `RuleSetVersion`, `ReportSchemaVersion`, signal versions and timestamps |

---

## 5. Eksik Report Contract

Phase 4 implementation öncesinde aşağıdaki boşluklar açıkça kapatılmalıdır:

1. `RepositoryAnalysis` composition service henüz yoktur. Mevcut application katmanı yalnız 15 `SignalResult` üretir.
2. Category summary, strength, critical gap ve improvement action üretimi için deterministic application orchestration yoktur.
3. Signal key → kullanıcı display name / report copy eşlemesi için tek bir report presentation catalog yoktur.
4. `AnalysisHighlight.EvidenceReferences` ve `ImprovementAction.EvidenceReferences` değerlerinin `EvidenceItem.Key` değerlerine referans verdiği constructor veya composer seviyesinde doğrulanmamaktadır.
5. `AnalysisHighlight.ScoreImpact` alanının evidence-first raporda kullanıcıya gösterilmesi gerekmez; alanın seçim amacı açık değildir ve score-first algısı yaratabilir.
6. Product boundary disclaimer `RepositoryAnalysis` verisi değildir. Bu, her renderer'ın uyması gereken versioned report policy olarak tanımlanmalıdır.
7. Explicit uncertainty için ayrı domain koleksiyonu zorunlu değildir; signal status ve warnings üzerinden türetilebilir. Ancak web ve Markdown renderer'larının aynı filtreleme ve sırayı kullanması için ortak report projection gerekir.
8. Mevcut evaluator'lar `SignalDefinition` ve `SignalResult` içine Türkçe name, explanation ve recommendation metinleri yazmaktadır. Bu, aynı `RepositoryAnalysis`ın locale değişmeden TR ve EN render edilmesi hedefiyle uyumlu değildir.

Bu boşluklar yeni sinyal, yeni scoring yöntemi veya yeni canonical kategori gerektirmez.

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

### 8.3 Localized presentation message contract

Kullanıcıya gösterilecek copy, string domain alanı yerine aşağıdaki semantic biçimde temsil edilmelidir:

```text
PresentationMessage
├── Key                 // ör. signal.installation-guide.recommendation.missing
├── Arguments           // named, typed semantic values
└── LiteralArguments    // path, command, filename, URL gibi çevrilmeyecek değerler
```

Bu contract için aşağıdaki mevcut text-bearing domain alanları refactor gerektirir:

| Mevcut alan | Language-neutral replacement |
|---|---|
| `SignalDefinition.Name`, description ve templates | semantic signal/presentation key'leri; rule text locale dışı kalır |
| `SignalResult.Explanation`, `Recommendation`, `Limitations` | message key + arguments veya report projection tarafından `SignalKey + Status + Evidence` üzerinden türetilen copy |
| `CategoryResult.Summary`, `Limitations` | semantic summary/limitation key + arguments |
| `AnalysisHighlight.Title`, `Summary` | highlight type + signal key + semantic message key/arguments |
| `ImprovementAction.Title`, `Reason`, `ExpectedImpact` | action key + arguments; priority, effort, related signals ve evidence refs locale dışı kalır |
| `AnalysisWarning.Message` | warning code + arguments |
| `RepositoryAnalysis.Limitations` | limitation key + arguments |

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

## 9. Session 043 Implementation Handoff

Session 043'ün tek hedefi language-neutral report contract ve deterministic `RepositoryAnalysis` composition foundation olmalıdır.

### Scope

Application katmanında, mevcut `RepositorySnapshot` ve canonical 15 `SignalResult` girdisinden şunları üreten bir composer ekle:

- dört `CategoryResult`,
- `ScoreBreakdown`,
- en fazla üç `Strength`,
- en fazla üç `CriticalGap`,
- en fazla üç `ImprovementAction`,
- `ImprovementPlan`,
- snapshot/analysis warnings ve limitations,
- tamamlanmış `RepositoryAnalysis`.

Composer locale kabul etmez. Locale-aware `ReportProjection` yalnız presentation sınırında, aynı `RepositoryAnalysis` üzerinden üretilir.

### Required implementation rules

- `RepositorySignalAnalyzer` ve 15 evaluator'ın score/status/evidence/rule davranışı değişmeyecek.
- Mevcut evaluator'ların user-facing Türkçe copy üretmesi language-neutral semantic keys ve arguments ile değiştirilecek; bu refactor report contract için zorunludur.
- Scoring yalnız mevcut `CategoryResult` ve `ScoreBreakdown` üzerinden üretilecek.
- Bu belgedeki deterministic selection contract uygulanacak.
- Presentation completeness category içinde kalacak, overall dışında kalacak.
- Uncertainty status'ları critical gap gibi gösterilmeyecek.
- Time/version/id üretimi test edilebilir girdilerle sağlanacak.
- Domain veya composer locale/culture kabul etmeyecek; hard-coded TR veya EN report copy üretmeyecek.
- Stable report section key'leri ve presentation message key/argument contract'ı tanımlanacak.
- EN default/fallback ile TR catalog, aynı semantic key seti için implement edilecek veya renderer session'ına devredilecekse explicit contract/test fixture olarak hazır tutulacak.
- Evidence path, command, filename, URL ve diğer repository literal'leri locale projection sırasında korunacak.
- MediatR, CQRS veya yeni dependency eklenmeyecek.
- Full Markdown ve Web renderer Session 043 kapsamına alınmayacak; ancak aynı analysis'i TR/EN projection fixture'larıyla render edebilen shared presentation boundary oluşturulacak.
- `validation-011` çalıştırılmayacak ve Phase 4 acceptance'a dahil edilmeyecek.

### Targeted tests

- Tam 15 signal → tam dört category ve tutarlı result graph
- Strength selection: status tier, category diversity ve canonical tie-break
- Critical gap selection: Missing/Partial tier ve uncertainty exclusion
- Action count, priority, related signal ve evidence reference kuralları
- Missing / NotApplicable / UnavailableEvidence / RequiresHumanReview ayrımı
- `portfolio.presentation-completeness` category-included / overall-excluded
- Aynı girdiler + aynı id/time/version → semantik olarak aynı `RepositoryAnalysis`
- Aynı `RepositoryAnalysis` → TR ve EN projection'larda aynı semantic section/signal/action identity ve farklı localized copy
- Unsupported locale → EN fallback; yeniden analysis/evaluation yok
- Literal evidence path, command, filename ve URL locale değişiminde byte-for-byte korunur
- `RepositoryAnalysis` constructor graph invariant'larının korunması

### Definition of done

Session 043, language-neutral composer/presentation boundary ve targeted application/domain tests tamamlandığında biter. Full renderer veya Web UI işi sonraki bounded session'a bırakılır.
