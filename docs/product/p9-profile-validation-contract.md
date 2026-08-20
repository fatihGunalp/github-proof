# GitHub Proof — P9 Profile Validation Contract

**Status:** CONTRACT LOCKED
**Slice:** P9-I1 — Validation Contract + Scenario Matrix
**Milestone:** B — Public Profile Evidence
**Phase:** Profile P9 — Validation Profile Set
**Depends On:** P8 Complete (`docs/product/p9-implementation-plan.md`)
**Canonical Owners Referenced (not superseded):**
`docs/product/public-profile-evidence-product-contract.md`,
`docs/architecture/public-profile-evidence-report-contract.md`,
`docs/architecture/public-profile-evidence-domain-model.md`,
`docs/product/validation-repository-set.md`
**Production Code:** NOT IN SCOPE

---

## 1. Canonical Validation Purpose

P9'un amacı, `docs/product/p9-implementation-plan.md` §1'de tanımlanan
production truth'u — P8'in tek çalışan pipeline'ından üretilen
`DeveloperPublicEvidenceProfile` + `ProfileReport` — farklı gerçek GitHub
kullanım biçimlerinde (profile composition) test etmektir.

Bu doküman P9'un **exact bounded validation surface'ını** kilitler:

> Hangi profile compositions (scenario) doğrulanacak, her scenario hangi
> risk(ler)i falsify etmeye çalışacak, her scenario için beklenen bounded
> davranış ne, hangi claim'ler yasak, evidence traceability'nin PASS/FAIL
> kuralı ne, determinism için hangi alanlar karşılaştırılacak.

Bu doküman:

- yeni bir threshold, taxonomy, evidence source, score, ranking veya
  confidence hesaplaması **tanımlamaz**,
- `RepositoryEligibilityPolicy` / `RepositoryContributionPolicy` /
  `CrossRepositoryEvidenceComposer` / `ProfilePatternPromotionPolicy` /
  `RepresentativeRepositorySelectionPolicy` / `ProfileComposer` /
  `ProfileReportProjector` semantiğini **değiştirmez**,
- yalnızca zaten kilitli canonical contract'lardan (product contract, report
  contract, domain model) derlenmiş bir **doğrulama sözleşmesi**dir.

Canonical P9 çıkış kriterleri (`p9-implementation-plan.md` §1, §8 P9-I4,
§14) bu dokümanın referans aldığı, ama bu slice'ın kanıtlamadığı hedeflerdir:

```text
prohibited inference: 0
unsupported profile claim: 0
evidence traceability: required for every meaningful claim
determinism: PASS
```

---

## 2. Exact Profile Scenario Matrix

P9 en az aşağıdaki 11 canonical validation scenario'sunu değerlendirir
(`p9-implementation-plan.md` §4, roadmap-required).

**Önemli sınır:** bu isimler yalnızca validation scenario etiketleridir.

```text
.NET-heavy validation scenario
!=
"Bu geliştirici .NET Developer'dır."
```

Hiçbir scenario etiketi:

- developer role,
- capability claim,
- seniority claim,
- job-fit claim

olarak yorumlanamaz veya raporlanamaz. Bir scenario yalnızca "bu profile
composition, şu evidence dağılımını temsil ediyor" anlamına gelir.

| # | Scenario | Composition tanımı |
|---|---|---|
| 1 | `.NET-heavy` | Çoğu eligible repository'de C#/.NET teknik evidence yoğunluğu |
| 2 | `frontend-heavy` | Çoğu eligible repository'de JavaScript/TypeScript/frontend framework evidence yoğunluğu |
| 3 | `mixed full-stack` | Aynı profilde birden fazla bağımsız technical area (ör. backend + frontend) evidence'ının birlikte var olması |
| 4 | `AI/ML-heavy` | Çoğu eligible repository'de Python/ML dependency-semantic-category evidence yoğunluğu |
| 5 | `few repositories` | Eligible repository sayısı düşük (P7/P8 repetition eşiğine yakın veya altında) |
| 6 | `many repositories` | Eligible repository sayısı yüksek, karışık collection-availability durumları içerebilir |
| 7 | `mostly forks` | Repository seti çoğunlukla `IsFork = true` |
| 8 | `mostly archived` | Repository seti çoğunlukla `Archived = true` |
| 9 | `student/tutorial-heavy` | Küçük, tekrarlayan, tutorial/boilerplate yapısına benzer repository'ler |
| 10 | `polyglot` | Birden fazla bağımsız technical area, hiçbiri baskın olmadan yan yana |
| 11 | `sparse public profile` | Toplam eligible + collectible evidence yüzeyi düşük veya erişilemez |

Bu 11 scenario ile `p9-implementation-plan.md` §4 arasında 1:1 eşleşme
sağlanmıştır; hiçbiri eklenmemiş, hiçbiri çıkarılmamıştır.

---

## 3. Scenario → Risk Mapping

Required risk seti (`p9-implementation-plan.md` §8 P9-I3, 9 risk):

```text
R1  false technical focus
R2  fork inflation
R3  template inflation
R4  README keyword inflation
R5  single-repository dominance
R6  archived-repository distortion
R7  sparse-profile negative judgment
R8  missing/unavailable data confusion
R9  broken evidence traceability
```

Eşleme (her hücre neyin falsify edildiğini söyler; ornamental değildir):

| Scenario | Mapped Risk(s) | Falsify edilen iddia |
|---|---|---|
| `.NET-heavy` | R1, R4 | Tek bir teknoloji alanının repeated-evidence eşiğini gerçekten karşıladığı, yalnızca README kelime geçişleriyle şişirilmediği |
| `frontend-heavy` | R1, R4 | Aynı yukarıdaki gibi, frontend teknoloji alanı için |
| `mixed full-stack` | R1, R5 | Birden fazla technical area'nın her biri kendi bağımsız repetition kanıtına dayandığı, tek repository'nin ikisini birden domine etmediği |
| `AI/ML-heavy` | R1, R4 | ML/Python evidence'ının dependency-semantic-category kanıtına dayandığı, yalnız README/anahtar kelime geçişine dayanmadığı |
| `few repositories` | R5, R7, R9 | Az sayıda repository'de tek repository'nin orantısız dominance kazanmadığı; azlığın negatif developer judgment'e dönüşmediği; kısıtlı evidence yüzeyinde trace zincirinin yine de sağlam kaldığı |
| `many repositories` | R5, R8 | Çok sayıda repository arasında tek repository'nin yine de orantısız domine etmediği; karışık collection-availability durumlarının (`Complete`/`Partial`/`Unavailable`) doğru ayrıştığı |
| `mostly forks` | R2, R9 | Fork ağırlıklı bir profilde repetition count'un fork'lar tarafından yapay şişirilmediği; fork evidence'ının orijinal-üretim iddiasına dönüşmediği |
| `mostly archived` | R6, R8 | Archived repository ağırlıklı bir profilde tarihsel kanıtın otomatik olarak "başarısız/düşük kalite/inaktif" yorumuna dönüşmediği; current-focus ile historical-evidence ayrımının bozulmadığı |
| `student/tutorial-heavy` | R1, R3, R4 | Şablon/tutorial yapısının tek başına bağımsız teknik üretim veya technical-focus kanıtı sayılmadığı |
| `polyglot` | R1, R5 | Birden fazla bağımsız technical area'nın her biri kendi repetition eşiğine göre değerlendirildiği, hiçbirinin diğerini bastırmadığı veya yapay biçimde birleştirilmediği |
| `sparse public profile` | R7, R8, R9 | Sınırlı evidence yüzeyinin negatif developer sonucuna dönüşmediği; missing/unavailable evidence ile gerçek negatif bulgunun karıştırılmadığı; az sayıdaki claim'in dahi tam trace zincirine sahip olduğu |

Coverage doğrulaması:

```text
Scenarios: 11 / 11
Required risks: 9 / 9 (R1..R9 hepsi en az bir scenario'da)
Unmapped scenarios: 0
Unmapped required risks: 0
```

---

## 4. Expected Bounded Behavior per Scenario

Aşağıdaki beklentiler yalnızca zaten kilitli canonical contract'lardan
(product contract §7, §10, §12, §16; report contract §6) türetilmiştir.
Yeni bir eşik, algoritma veya taksonomi tanımlanmamıştır.

### 4.1 `.NET-heavy` / `frontend-heavy` / `AI/ML-heavy` (single-dominant-area)

May produce (product contract §10):

```text
TechnicalFocusAreas
Supported + [technical area destekleyen repeated pattern]
```

Must not produce:

```text
".NET Developer" / "Frontend Developer" / "AI/ML Developer" role claim
yetkinlik (competence) claim
```

Kaynak: TechnicalFocus yalnız "public kanıtların desteklediği teknik üretim
alanı" anlamına gelir, yetkinlik değil (product contract §10).

### 4.2 `mixed full-stack` / `polyglot`

May produce: her bağımsız technical area için ayrı `TechnicalFocusAreas`
item'ı, her biri kendi repetition kanıtına dayanarak.

Must not produce: tek bir birleşik "full-stack developer" veya "polyglot
developer" role/capability iddiası; bir alanın diğerini gölgelediği yapay
bir birleştirme.

### 4.3 `few repositories`

May produce (P8-I2/domain model §13):

```text
ProfileLimitation: LimitedIndependentEvidenceSurface
```

veya eşdeğer mevcut canonical limitation.

Must not produce:

```text
weak developer / inexperienced developer / unqualified candidate
```

Kaynak: `ProfilePipelineTests.RunAsync_SparseSingleRepository_...` bu
davranışı zaten empirik olarak kanıtlıyor (tek repository → Complete +
`LimitedIndependentEvidenceSurface`, negatif judgment yok).

### 4.4 `many repositories`

May produce: karışık `Complete`/`Partial` collection sonuçları, buna bağlı
`ProfileWarning` kimlikleri (`RepositoryEvidenceRateLimited`,
`RepositoryEligibilityUndetermined` vb., domain model §13).

Must not produce: repository sayısının kendisinin bir kalite/kapsamlılık
göstergesi olarak sunulması (product contract §15 — skor toplanamaz).

### 4.5 `mostly forks`

Forks may remain visible context (product contract §7 Fork).

Must not: profile repetition count'u sessizce şişirmek, authorship/
original-production claim'e dönüşmek.

### 4.6 `mostly archived`

Historical evidence may remain valid (product contract §7 Archived
Repository).

Archived repositories must not automatically become:

```text
failure / low quality / inactive developer
```

Current-focus contribution mevcut versioned eligibility kurallarına
(`RepositoryEligibilityPolicy`, `ActivityClassification`) bağlı kalmalıdır —
bu kural bu slice tarafından değiştirilmez.

### 4.7 `student/tutorial-heavy`

Şablon/tutorial yapısı tek başına:

```text
code authorship / bağımsız mimari karar / özgün problem çözümü
```

kanıtı değildir (product contract §7 Template). Tekrarlayan boilerplate
yapı, repeated technical-focus eşiğini yalnızca gerçek repetition kanıtı
varsa karşılayabilir; şablon benzerliğinin kendisi ek kanıt değildir.

### 4.8 `sparse public profile`

May produce:

```text
LimitedIndependentEvidenceSurface
```

veya eşdeğer mevcut canonical limitation (domain model §13, product
contract §16).

Must not produce:

```text
weak developer / inexperienced developer / unqualified candidate /
inactive developer
```

Doğru bounded sonuç (product contract §16):

> Bu public GitHub profili profile-level aggregation için sınırlı görünür
> kanıt sağlıyor.

### 4.9 Technical-focus / Engineering-practice genel sınırı (tüm scenario'lara uygulanır)

Repeated technical evidence bounded `TechnicalFocus` veya
`EngineeringPractice` pattern'i üretebilir.

Must not become:

```text
capability / mastery / seniority / role identity
```

(product contract §10, §11; report contract §4, §16)

---

## 5. Prohibited Profile Claims Checklist

P9 validation seti, aşağıdaki veya semantik olarak eşdeğer hiçbir çıkarımı
üretmemelidir. Liste `public-profile-evidence-product-contract.md` §18 ve
`public-profile-evidence-report-contract.md` §16'nın birleşimidir; bu
doküman listeyi genişletmez, yalnızca P9 validation checklist biçiminde
konsolide eder.

```text
P1  junior / mid / senior assignment
P2  hire / no-hire recommendation
P3  candidate ranking ("bu aday diğerlerinden iyidir")
P4  employability judgment
P5  developer/capability score (genel teknik seviye X'tir)
P6  intelligence / talent judgment
P7  salary inference
P8  years-of-experience inference
P9  private-work absence inference ("public görünmeyen private çalışması yoktur")
P10 code authorship claim ("içeriğin tamamını bu kişi yazmıştır")
P11 originality claim ("kod özgündür")
P12 AI-written claim
P13 not-AI-written claim
P14 production-ready claim
P15 security verdict ("kod güvenlidir")
P16 architecture quality verdict ("mimari doğrudur/yanlıştır")
P17 low public activity → weak developer inference
```

Ek olarak (report contract §16'dan, aynı kategori altında):

```text
P18 developer/profile/repository quality score, confidence score
P19 seniority, employability, hire/no-hire, candidate ranking, job fit
     (P1-P4 ile aynı aile, report-projection seviyesinde tekrar yasaklanır)
P20 inferred role (RoleEvidenceAlignment henüz Deferred; §7 ile tutarlı)
```

Bu checklist P9-I3'ün prohibited-inference scan'inin doğrudan girdisidir.

---

## 6. Evidence Traceability Acceptance Rules

Reuse edilen tek trace modeli (`p9-implementation-plan.md` §3, §6.1;
domain model §13; report contract §11 WhyThisConclusion):

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

P9 bu zincire ikinci bir trace modeli eklemez; `ProfileComposer`'ın zaten
uyguladığı reference-based coherence kontrolüyle (Canonical
Observation/Pattern Trace, worklog Session 086) ve `WhyThisConclusion`'ın
evidence-first trace'iyle (report contract §11) aynı zinciri kullanır.

### 6.1 PASS Kriteri

Bir profile-level claim traceability PASS sayılır ancak ve ancak:

1. claim, `RepeatedProfilePattern` referansına (Pattern) sahiptir,
2. o pattern'in `SupportingObservations`'ı, claim'in dayandığı
   `CrossRepositoryEvidence`'a reference-identity ile aittir,
3. her observation, gerçek bir repository reference'ına (eligible
   repository) sahiptir,
4. o repository reference, gerçek bir `RepositoryAnalysisReference` /
   `RepositoryEligibilityResult`'a geri izlenebilir (geçerli `AnalysisId`,
   `Guid.Empty` değil),
5. (varsa) `RepresentativeRepository` referansı, aynı `ProfilePatternSet`
   içindeki pattern'e reference-identity ile aittir.

### 6.2 FAIL Kriteri

Bir claim, zincirin herhangi bir halkası şu durumlardaysa traceability
FAIL sayılır:

- **broken**: pattern → observation → repository referans zincirinde bir
  halka eksik veya `null`,
- **fabricated**: claim, gerçek bir `CrossRepositoryEvidence`/
  `RepositoryEligibilityResult`'a değil, harici/varsayılan bir değere
  dayanıyor,
- **orphaned**: referans edilen observation veya pattern, claim'in ait
  olduğu `ProfilePatternSet`/`CrossRepositoryEvidence`'a reference-identity
  ile ait değil (`ProfileComposer`'ın zaten reddettiği durum — bu durumda
  composition zaten `Failed()` döner, dolayısıyla P9 seviyesinde bu, PASS
  bir `Composed` sonucu içinde asla gözlemlenmemesi gereken bir invariant
  olarak doğrulanır),
- claim, gerçek repository-level evidence'a ulaşamıyor (ör. `AnalysisId ==
  Guid.Empty`, veya evidence `RepositoryAnalysis` içinde bulunamıyor).

### 6.3 Kapsam

Bu kural yalnızca "meaningful" claim'lere uygulanır: `TechnicalFocusAreas`,
`EngineeringPracticePatterns`, `RepresentativeRepositories`,
`WhyThisConclusion` item'ları. `PublicProfileSummary` kendi başına yeni
kanıt taşımaz (report contract §8) — o, altındaki section'ların
trace'ini miras alır.

---

## 7. Unsupported-Claim Acceptance Rules

Canonical V1 durumu (report contract §6.2, §6.3; product contract §14):

```text
ProblemAreaPatterns
Availability = Unsupported

RoleEvidenceAlignment
Availability = Deferred
```

P9 validation'ın kabul kuralları:

```text
Unsupported != Evidence Absence
Deferred != Negative Result
Supported + empty != Negative Developer Evidence
```

Somut kabul/red kriterleri:

- `ProblemAreaPatterns.Availability == Unsupported` ve `Items == []`
  olması PASS'tır; bunun "hiç problem alanı bulunamadı" biçiminde
  yorumlanması veya raporlanması FAIL'dir.
- `RoleEvidenceAlignment.Availability == Deferred` ve `Items == []`
  olması PASS'tır; bunun "rol eşleşmesi yok" biçiminde yorumlanması FAIL'dir.
- P9, `ProblemAreaPatterns` için yeni bir inference üretmez ve
  `RoleEvidenceAlignment`'ı P9 kapsamında aktif bir capability haline
  getirmez (`p9-implementation-plan.md` §6.7).
- `false role alignment` dedicated bir P9 risk/scenario'su değildir, çünkü
  `RoleEvidenceAlignment` Profile V1'de hiç üretilmez — üretilmeyen bir
  claim falsify edilemez. Daha geniş prohibited-inference scan (§5) bunun
  yerine geçer ve devam eder.
- `TechnicalFocusAreas`/`EngineeringPracticePatterns` gibi `Supported`
  section'lar boş (`Items == []`) döndüğünde bu, "no repeated pattern
  promoted under current rules" anlamına gelir (report contract §6.1);
  "bu geliştiricinin teknik odağı yok" biçiminde yorumlanması FAIL'dir.

---

## 8. Deterministic Comparison Surface

Bu yüzey, mevcut P8 production modelleri ve
`tests/GitHubProof.Application.Tests/Profile/ProfilePipelineTests.cs`
içindeki `AssertSemanticallyEquivalent` yardımcı metodunun **inceleme
sonucudur** — yeni bir determinism algoritması icat edilmemiştir; P9-I2/I4
bu alanları genişletmeden doğrudan reuse eder.

### 8.1 Karşılaştırılacak Semantik Alanlar

| Yüzey | Karşılaştırma anahtarı |
|---|---|
| `ProfilePipelineResult.Status` | değer eşitliği |
| `Profile.RepositoryEligibilityResults` | `(Reference, Status)` çiftleri, sırasız (`Order()`) karşılaştırma |
| `Profile.Limitations` | `Code` sırası (zaten canonical ordering, domain model §13) |
| `Profile.Warnings` | `(Code, AffectedRepositories sıralı)` çiftleri |
| `Profile.RepeatedProfilePatterns` | `(Family, PatternKey)` sırası |
| `Profile.RepresentativeRepositories` | `Repository` referans sırası |
| `Report.Sections` | `(SectionKey, Availability)` sırası (report contract §7 canonical ordering) |
| `Report.WhyThisConclusion.Items` | `Pattern.PatternKey` sırası |
| `Report.RepositoryDrillDown.Items` | `(Repository, EligibilityStatus)` sırası |

Bu 9 alan, P9-I2/I4'ün "composition status, eligibility status/results,
limitations, warnings, promoted patterns, representative repositories,
report section availability, conclusion evidence, repository drill-down"
(görev talimatındaki aday liste) için gerçek karşılığıdır — kör kopya
değil, `ProfilePipelineTests.cs` içindeki fiili assertion'lardan
türetilmiştir.

### 8.2 Determinism Kapsamı Dışında Kalanlar

Aşağıdakiler semantic determinism yüzeyinin **parçası değildir** (runtime/
object identity, girdi olarak sabitlenmediği sürece):

- `AnalysisId` (Guid) somut değerleri — yalnız fixed bir
  `analysisIdFactory` ile sabitlenmiş bir determinism testinde (P9-I4
  benzeri) karşılaştırılır; genel scenario validation'ında `AnalysisId`'nin
  varlığı (`!= Guid.Empty`) kontrol edilir, değeri değil,
- `.NET` nesne referans kimlikleri (`ReferenceEquals`) — bunlar
  `ProfileComposer`'ın kendi iç coherence kontrolüdür (§6), P9'un dışarıdan
  gözlemlediği semantic yüzey değildir,
- wall-clock zaman damgaları (`StartedAt`/`CompletedAt` gibi audit alanları)
  — semantik sonucu etkilemez, yalnız `snapshot.CapturedAt` girdi olarak
  sabitlenir (domain model §14, §16).

### 8.3 Determinism Invariant'ı

Aynı fixed `GitHubProfileSnapshot` + aynı rule version seti + (varsa) aynı
sabitlenmiş `analysisIdFactory`/`TimeProvider`, §8.1'deki 9 alanda
semantik olarak özdeş sonuç üretmelidir. Repository girdi sırası
semantik olmayan bir değişkendir ve sonucu değiştirmemelidir
(`RunAsync_RepositoryOrderInSnapshot_DoesNotAlterSemanticResult` zaten bunu
kanıtlıyor).

---

## 9. Existing Validation Asset Reuse Boundary

İki farklı validation seviyesi kesin biçimde ayrılır:

```text
Repository-level validation baseline
!=
Profile-level P9 validation scenario set
```

### 9.1 Repository-level baseline (değiştirilmez, genişletilmez)

- `docs/product/validation-repository-set.md` — Faz 0 (15 MVP sinyali)
  kapsamında, tek bir repository'nin signal-engine sonucunu insan
  değerlendirmesiyle karşılaştıran bir contract. En az 10, MVP'de en az 20
  repository hedefler; her repository tek başına bir
  `RepositoryProfile` (ör. "strong-documentation", "archived-but-complete")
  taşır.
- `samples/validation/repositories.json`, `samples/validation/results/` —
  bu repository-level baseline'ın veri dosyaları.
- `ProfilePipelineTests.cs` — Application seviyesinde P8 pipeline wiring'ini
  doğrulayan mevcut integration testleri (§13'te zaten listelenen
  invariant'lar).

P9 bu üçünü **değiştirmez, yeniden tanımlamaz ve tek bir validation
repository'sini tek bir P9 profile scenario'suna eşit saymaz.**

### 9.2 Profile-level P9 scenario set (bu doküman)

P9 scenario'ları (§2), **birden fazla repository'nin bir profile içinde
nasıl bir araya geldiğini** (composition / evidence distribution) test
eder — tek bir repository'nin kendi içindeki sinyalleri değil.

Bir P9 scenario'su (ör. `mostly forks`), birden fazla repository-level
validation baseline kaydından (ör. birkaç "fork" işaretli repository) inşa
edilebilir, ama bu ilişki 1:1 değildir: bir P9 scenario'su tipik olarak
N eligible repository'nin bir kombinasyonunu (bazıları fork, bazıları
orijinal, karışık dil dağılımı vb.) temsil eder.

### 9.3 P9-I2 için reuse sınırı

P9-I2 (fixture/harness) inşa ederken:

- repository-level `samples/validation/repositories.json` kayıtlarından
  gerçek/gerçekçi repository örnekleri **girdi olarak** ödünç alınabilir,
- ancak yeni bir "profile validation repository set" dosyası bu belgenin
  §9.1'deki dosyaların yerine geçmez veya onları çoğaltmaz,
- profile-level fixture'lar, mevcut `ProfileAnalysisTestSupport` /
  `ProfileCompositionTestSupport` (Application.Tests) yardımcılarını reuse
  etmelidir — P9-I1 bu yardımcıları değiştirmez, yalnızca onların hangi
  scenario'ları besleyeceğini (§2) tanımlar.

---

## 10. P9-I2 Handoff Requirements

P9-I2, bu contract'tan ambiguity olmadan şunları türetebilmelidir
(`p9-implementation-plan.md` §8 P9-I2 exit kriteri):

1. **11 scenario fixture'ı**: her biri §2'deki composition tanımına uyan,
   production `ProfilePipeline` üzerinden çalıştırılabilen bir
   `GitHubProfileSnapshot` + collected evidence girdisi (network'e bağımlı
   olmayan, deterministic fixed input).
2. **Risk-check hook'ları**: her scenario çalıştırıldıktan sonra, §3'te o
   scenario'ya eşlenen risk(ler) için §4'teki "may/must not produce"
   beklentisini kontrol edebilecek bir doğrulama noktası (assertion veya
   eşdeğer).
3. **Prohibited-claim scan**: §5'teki checklist'e karşı, üretilen
   `ProfileReport` üzerinde semantik bir tarama (yeni bir NLP/keyword
   motoru icat etmeden — mevcut structured section/availability/pattern
   alanları üzerinden).
4. **Traceability check**: §6.1/§6.2 PASS/FAIL kriterlerini, her
   `TechnicalFocusAreas`/`EngineeringPracticePatterns`/
   `RepresentativeRepositories`/`WhyThisConclusion` item'ı için
   uygulayabilecek bir yardımcı.
5. **Unsupported/Deferred check**: §7'deki `ProblemAreaPatterns`/
   `RoleEvidenceAlignment` availability + emptiness invariant'ının her
   scenario'da korunduğunu doğrulayan bir kontrol.
6. **Determinism hook**: §8.1'deki 9 alanı, aynı fixed input üzerinde iki
   bağımsız çalıştırma arasında karşılaştırabilecek bir yardımcı (mevcut
   `AssertSemanticallyEquivalent` deseninin scenario-parametrik hâli).

P9-I2 bu gereksinimleri karşılarken:

- production pipeline stage'lerini stub/reimplement etmez,
- canonical profile semantics'i test tarafında yeniden yazmaz,
- yeni bir semantic taksonomi veya threshold icat etmez.

---

## 11. Explicit Non-Goals

Bu doküman ve P9-I1 slice'ı aşağıdakileri yapmaz:

- production algoritması değiştirmek (`RepositoryEligibilityPolicy`,
  `RepositoryContributionPolicy`, `CrossRepositoryEvidenceComposer`,
  `ProfilePatternPromotionPolicy`, `RepresentativeRepositorySelectionPolicy`,
  `ProfileComposer`, `ProfileReportProjector` dahil),
- yeni threshold, taksonomi, evidence source, score, ranking, confidence
  hesaplaması eklemek,
- `ProblemArea` implementasyonu yapmak,
- `RoleEvidenceAlignment` implementasyonu yapmak,
- LLM narration eklemek,
- yeni third-party paket eklemek,
- gerçek fixture, harness veya test dosyası oluşturmak (P9-I2 kapsamı),
- gerçek GitHub üzerinde canlı validation çalıştırmak,
- P10 (Web Profile Flow) implementasyonuna başlamak,
- `Web` / `Razor` / `CSS` / UI'a dokunmak,
- var olan validation-repository-set.md'yi veya
  `samples/validation/repositories.json`'ı değiştirmek/yeniden tanımlamak.

---

## 12. P9 Exit Relationship

Bu doküman, P9'un kendisini `COMPLETE` yapmaz.

`p9-implementation-plan.md` §8 P9-I4'te tanımlanan final exit kriteri
(`prohibited inference = 0`, `unsupported profile claim = 0`, her meaningful
claim traceable, `determinism = PASS`) yalnızca P9-I2 (fixture/harness),
P9-I3 (risk + traceability validation) ve P9-I4 (determinism + exit
validation) fiilen çalıştırıldıktan ve sonuç kanıtlandıktan sonra
sağlanabilir.

Bu doküman, o üç slice'ın **neye göre** doğrulama yapacağını, hangi
scenario/risk/beklenti/kural setine göre PASS/FAIL kararı vereceğini sabit
biçimde kilitler — böylece P9-I2/I3/I4 hiçbir noktada yeni semantics icat
etmek zorunda kalmaz; yalnızca bu kilitli sözleşmeyi production
`ProfilePipeline` çıktısına karşı çalıştırır.

```text
P9-I1 (bu doküman): validation contract locked
        ↓
P9-I2: fixture/harness bu contract'a göre inşa edilir
        ↓
P9-I3: §3/§4/§5/§6 risk + traceability kontrolleri çalıştırılır
        ↓
P9-I4: §8 determinism + toplam exit kriteri kanıtlanır
        ↓
P9 COMPLETE (yalnız §14 p9-implementation-plan.md success criteria
tamamen karşılanırsa)
```
