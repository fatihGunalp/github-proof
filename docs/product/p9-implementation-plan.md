# GitHub Proof — P9 Implementation Plan

**Status:** Draft — pending independent review
**Milestone:** B — Public Profile Evidence
**Phase:** Profile P9 — Validation Profile Set
**Depends On:** P8 Complete
**Canonical Owner:** `docs/roadmap/mvp-phases.md`

---

# 1. Purpose

P8 tamamlandı.

GitHub Proof artık tek bir `GitHubProfileSnapshot` üzerinden mevcut production pipeline'ını çalıştırarak deterministic:

- `DeveloperPublicEvidenceProfile`
- `ProfileReport`

üretebiliyor.

P9'un amacı yeni bir kullanıcı arayüzü oluşturmak veya yeni profile semantics üretmek değildir.

P9'un amacı:

> P8 tarafından üretilen Public Profile Evidence truth'un farklı gerçek GitHub kullanım biçimlerinde yanıltıcı, desteklenmeyen veya izlenemeyen profile-level claim üretmediğini doğrulamaktır.

Canonical P9 çıkış kriterleri:

```text
prohibited inference: 0
unsupported profile claim: 0
evidence traceability: required for every meaningful claim
determinism: PASS
```

---

# 2. P8 → P9 Boundary

P8 production zinciri:

```text
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
DeveloperPublicEvidenceProfile
        ↓
ProfileReportProjector
        ↓
ProfileReport
```

P9 bu zinciri yeniden tasarlamaz.

P9:

```text
P8 Structured Truth
        ↓
Profile Validation Scenarios
        ↓
Falsification / Traceability / Determinism Checks
        ↓
P9 Exit Verdict
```

---

# 3. P9 Responsibility

P9 aşağıdaki soruları cevaplamalıdır:

1. Farklı profile tiplerinde false `TechnicalFocus` oluşuyor mu?
2. Fork veya template yüzeyi profile repetition'ı yanlış biçimde şişiriyor mu?
3. Tek bir repository profile sonucunu orantısız biçimde domine ediyor mu?
4. Archived repository'ler güncel teknik odak gibi yanlış yorumlanıyor mu?
5. Sparse public profile negatif developer judgment üretiyor mu?
6. Missing veya unavailable evidence yanlış profile claim'e dönüşüyor mu?
7. Her meaningful profile claim repository-level evidence'a geri izlenebiliyor mu?
8. Aynı fixed input aynı semantic profile truth'u tekrar üretiyor mu?
9. Unsupported veya Deferred capability durumları negatif sonuç gibi yorumlanıyor mu?
10. Yasaklı profile-level inference'lardan herhangi biri üretiliyor mu?

---

# 4. Canonical Validation Profile Matrix

P9 en az canonical roadmap'ta belirtilen şu profile biçimlerini değerlendirmelidir:

```text
.NET-heavy
frontend-heavy
mixed full-stack
AI/ML-heavy
few repositories
many repositories
mostly forks
mostly archived
student/tutorial-heavy
polyglot
sparse public profile
```

Bu isimler developer capability veya role label değildir.

Bunlar yalnızca validation senaryosu sınıflarıdır.

Bir profile'ın validation scenario olarak `.NET-heavy` seçilmesi:

> Bu geliştirici .NET geliştiricisidir.

anlamına gelmez.

Amaç yalnızca belirli evidence dağılımlarında profile aggregation davranışını sınamaktır.

---

# 5. Existing Validation Assets

P9 sıfırdan validation altyapısı kurmamalıdır.

Repository'de halihazırda:

- `docs/product/validation-repository-set.md`
- `samples/validation/repositories.json`
- `samples/validation/results/`
- repository-level live validation harness
- repository-level determinism coverage
- `ProfilePipelineTests`

bulunmaktadır.

Mevcut `samples/validation/repositories.json` repository-level validation baseline'ıdır.

P9 bunu değiştirmek zorunda değildir.

P9 ihtiyacı:

> repository-level validation'dan ayrı olarak profile-level scenario composition ve profile-level truth validation yapabilmek.

Existing validation assets mümkün olduğunca reuse edilmelidir.

---

# 6. Canonical Truth Boundaries to Validate

## 6.1 Evidence Traceability

Her meaningful profile-level claim şu zincire geri izlenebilmelidir:

```text
Profile-level claim
        ↓
Cross-repository pattern
        ↓
Quantified observations
        ↓
Repository references
        ↓
Repository-level evidence
```

Bu zincirin herhangi bir halkası yoksa claim P9 validation'da başarısızdır.

---

## 6.2 Technical Focus

`TechnicalFocus` yalnız supported repeated evidence üzerinden oluşmalıdır.

Validation özellikle şunları falsify etmeye çalışmalıdır:

- README keyword inflation
- single-repository dominance
- fork inflation
- template inflation
- archived-repository distortion
- unrelated language presence
- sparse-evidence promotion

Bir profile-level technical-focus sonucu developer capability claim'ine yükseltilemez.

---

## 6.3 Engineering Practice Patterns

Engineering practice pattern:

> birden fazla repository'de görünür practice evidence

anlamında kalmalıdır.

Şu tür sonuçlar yasaktır:

```text
This developer is good at testing.
This developer has strong engineering discipline.
This developer follows best practices.
```

P9 yalnız observable repeated evidence'ın doğru biçimde aggregate edildiğini doğrular.

---

## 6.4 Sparse Public Profile

Sparse profile:

- negative developer judgment üretmemeli,
- profile-level score üretmemeli,
- evidence absence'ı skill absence'a dönüştürmemeli.

Doğru bounded sonuç:

> Public profile, profile-level aggregation için sınırlı görünür kanıt sağlıyor.

---

## 6.5 Fork / Template

Fork veya template:

- independent production evidence gibi davranmamalı,
- repetition count'u yanlış biçimde artırmamalı,
- authorship/originality claim üretmemeli.

---

## 6.6 Archived Repositories

Archived repository historical evidence taşıyabilir.

Ancak archived repository'nin varlığı:

- failure,
- low quality,
- inactivity of the developer

anlamına gelemez.

Current-focus semantics mevcut versioned eligibility kurallarına bağlı kalmalıdır.

---

## 6.7 Unsupported / Deferred Capabilities

Canonical V1:

```text
ProblemAreaPatterns
Availability = Unsupported

RoleEvidenceAlignment
Availability = Deferred
```

P9 doğrulamalıdır:

```text
Unsupported != Evidence Absence
Deferred != Negative Result
Supported + empty != Negative Developer Evidence
```

`ProblemAreaPatterns` için yeni inference üretilmez.

`RoleEvidenceAlignment` P9 validation kapsamında aktif capability haline getirilmez.

---

# 7. Prohibited Profile-Level Inferences

P9 validation setinde aşağıdaki veya semantik olarak eşdeğer çıkarımlardan hiçbiri bulunmamalıdır:

- junior / mid / senior assignment
- hire / no-hire recommendation
- candidate ranking
- employability judgment
- general developer score
- capability score
- intelligence / talent judgment
- salary inference
- years-of-experience inference
- private-work absence inference
- code authorship claim
- originality claim
- AI-written / not-AI-written claim
- production-ready claim
- security claim
- architecture quality verdict
- low public activity → weak developer inference

Exit target:

```text
prohibited inference: 0
```

---

# 8. P9 Implementation Slices

## P9-I1 — Validation Contract + Scenario Matrix

### Goal

P9 validation'ın exact bounded scenario setini ve her scenario için doğrulanacak riskleri kilitlemek.

### Deliverables

En az:

- canonical profile scenario matrix,
- scenario → risk mapping,
- expected bounded behavior,
- prohibited claim checklist,
- traceability acceptance rules,
- deterministic comparison surface.

### Constraints

Bu slice:

- production algorithm değiştirmez,
- Web'e dokunmaz,
- P10'u başlatmaz,
- gerçek validation sonucu üretmek zorunda değildir,
- unsupported taxonomy icat etmez.

### Exit

P9-I2'nin hangi fixture/harness'ı kuracağı ambiguity olmadan tanımlanmış olmalıdır.

---

## P9-I2 — Profile Validation Fixtures / Harness

### Goal

Production `ProfilePipeline` üzerinden scenario-level profile validation çalıştırabilen bounded test/harness oluşturmak.

### Principles

Harness:

- production pipeline stage'lerini stub/reimplement etmemeli,
- canonical profile semantics'i test tarafında yeniden yazmamalı,
- deterministic fixed input sağlayabilmeli,
- profile/report semantic output'u karşılaştırabilmeli,
- traceability'yi inceleyebilmeli,
- normal test suite'i network'e bağımlı hale getirmemeli.

Gerçek GitHub validation gerekiyorsa explicit opt-in live path kullanılmalıdır.

### Preferred reuse

Mümkün olduğunda reuse:

- existing repository validation manifest/helpers,
- existing profile test support,
- existing production `ProfilePipeline`,
- existing deterministic ID/time support.

Yeni abstraction yalnız mevcut yapı gerçek bir bounded ihtiyaç karşılamıyorsa eklenir.

---

## P9-I3 — Profile Risk and Traceability Validation

### Goal

Canonical profile matrix üzerinde false-claim ve evidence-traceability risklerini doğrulamak.

### Required Risk Coverage

En az:

- false technical focus
- fork inflation
- template inflation
- README keyword inflation
- single-repository dominance
- archived-repository distortion
- sparse-profile negative judgment
- missing/unavailable data confusion
- broken evidence traceability

RoleEvidenceAlignment V1'de üretilmediği için `false role alignment` dedicated risk'i zorunlu değildir.

Ancak daha geniş prohibited-inference scan devam eder.

### Claim Validation

Her meaningful claim için:

```text
claim
→ pattern
→ observations
→ repository references
→ repository evidence
```

zinciri doğrulanmalıdır.

---

## P9-I4 — Determinism + Exit Validation

### Goal

P9 canonical exit criteria'larını bütün validation surface üzerinde kanıtlamak.

### Determinism

Aynı fixed:

- `GitHubProfileSnapshot`
- repository analysis inputs
- rule versions
- deterministic IDs / relevant fixed time inputs

aynı semantic profile truth ve report truth üretmelidir.

Repository input order gibi semantik olmayan değişkenler sonucu değiştirmemelidir.

### Final Exit Criteria

P9 yalnız şu durumda COMPLETE olabilir:

```text
prohibited inference = 0

unsupported profile claim = 0

every meaningful claim
    → traceable to repository-level evidence

determinism = PASS
```

Açıklanamayan false-positive veya semantic drift varsa P9 kapanmaz.

---

# 9. Production Code Change Rule

P9 bir calibration/refactor phase değildir.

Default:

```text
validation first
production mutation second
```

Production code yalnız validation tarafından kanıtlanmış:

- contract violation,
- false claim,
- determinism defect,
- broken traceability,
- incorrect eligibility/contribution behavior

bulunursa değiştirilebilir.

Bu durumda:

```text
failing validation evidence
        ↓
canonical contract check
        ↓
bounded root-cause analysis
        ↓
minimal production fix
        ↓
targeted regression
        ↓
scenario re-validation
```

izlenir.

Validation sonucu olmadan:

- threshold tuning,
- scoring change,
- new taxonomy,
- new pattern,
- new inference,
- speculative refactor

yapılmaz.

---

# 10. Explicit Non-Goals

P9 kapsamında değildir:

- Web profile flow
- GitHub username form / page
- Razor presentation
- profile UI
- CSS/design work
- P10 implementation
- recruiter pilot
- new score
- ranking
- seniority
- employability
- job fit
- AI-authorship detection
- ProblemArea taxonomy
- RoleEvidenceAlignment implementation
- LLM narration
- new third-party package unless independently justified by an unavoidable validation need

---

# 11. Validation Strategy

Her implementation slice için önce targeted validation çalıştırılır.

P9-I2 ve sonrası için minimum sıra:

```text
1. targeted profile validation tests
2. related Application tests
3. affected Infrastructure/Web tests only if actually touched
4. full solution tests
5. full build
6. formatting / diff checks
```

Normal tests network-free kalmalıdır.

Live GitHub validation:

```text
explicit opt-in only
```

olmalıdır.

Her meaningful session sonunda:

```bash
git status --short
git diff --check
git diff --stat
```

kontrol edilir.

Commit/push kullanıcıya aittir.

---

# 12. Validation Artifacts

P9 sonuçları yalnız test PASS sayılarından ibaret olmamalıdır.

Validation çıktısı en az şu bilgileri görünür kılmalıdır:

- scenario identity,
- input profile composition,
- profile status,
- promoted patterns,
- representative repositories,
- limitations,
- warnings,
- report section availability,
- meaningful claims,
- evidence trace,
- prohibited-inference result,
- unsupported-claim result,
- determinism result.

Artifact formatı implementation sırasında repository'nin mevcut validation conventions'ına göre belirlenir.

Bu plan yeni bir production report generator tanımlamaz.

---

# 13. Existing Tests Already Covering Part of P9

Existing `ProfilePipelineTests` halihazırda bazı önemli davranışları doğrular:

- complete profile + report coherence,
- sparse single-repository behavior,
- unavailable collection → Partial + limitations/warnings,
- snapshot-captured time semantics,
- repository-order independence,
- fixed-input replay determinism,
- Failed composition → no profile/report,
- Unsupported ProblemArea boundary,
- Deferred RoleEvidenceAlignment boundary,
- conclusion evidence trace presence.

P9 bu testleri tekrar yazmamalıdır.

P9'un ek değeri:

> Bu invariant'ları daha geniş profile scenario matrix ve gerçek-world evidence distributions üzerinde falsify etmeye çalışmaktır.

---

# 14. Success Criteria

P9 başarılıdır eğer:

1. canonical profile scenario matrix coverage tamamlanmışsa,
2. prohibited inference sayısı sıfırsa,
3. unsupported profile claim sayısı sıfırsa,
4. her meaningful claim repository-level evidence'a geri izlenebiliyorsa,
5. sparse profile negatif developer judgment üretmiyorsa,
6. fork/template/archived yüzeyleri profile truth'u semantic olarak şişirmiyorsa,
7. missing/unavailable evidence ayrımı korunuyorsa,
8. deterministic replay aynı semantic sonucu üretiyorsa,
9. Unsupported ve Deferred capability sınırları korunuyorsa,
10. açıklanamayan validation failure kalmamışsa.

---

# 15. P9 Completion Boundary

P9 tamamlandığında ürünün iddiası:

> Public Profile Evidence V1'in profile-level structured truth'u, canonical validation setinde bounded, traceable ve deterministic davranıyor.

P9 tamamlandığında henüz şu iddia yapılmaz:

> Web profile experience hazır.

Bu:

```text
Profile P10 — Web Profile Flow
```

kapsamıdır.

P10 ancak P9 exit criteria karşılandıktan sonra implementation'a açılmalıdır.

---

# 16. First Next Action

İlk implementation görevi:

```text
P9-I1 — Validation Contract + Scenario Matrix
```

olmalıdır.

Bu görev önce:

- canonical P9 scenario setini,
- mevcut validation assets'i,
- existing ProfilePipeline test coverage'ını,
- eksik risk coverage'ını

karşılaştırmalı ve yalnız eksik validation boundary'yi tanımlamalıdır.

Henüz production code mutation yapılmamalıdır.
