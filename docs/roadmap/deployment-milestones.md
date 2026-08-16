# GitHub Proof — Deployment Milestones

**Status:** CANONICAL ROADMAP CONTRACT
**Version:** 1
**Date:** 2026-08-16

Bu doküman GitHub Proof capability development roadmap'inden bağımsız production
delivery checkpoint'lerini tanımlar.

## 1. Development ve Deployment Ayrımı

GitHub Proof iki ayrı ilerleme eksenine sahiptir.

Stage / Phase
=
ürünün capability development ve contract ilerleyişi

Deployment Milestone
=
kullanıcıya bağımsız değer sağlayan,
doğrulanmış capability'nin production release noktası

Canonical development roadmap:

docs/roadmap/mvp-phases.md

Canonical deployment roadmap:

docs/roadmap/deployment-milestones.md

Bu iki roadmap birbirinin yerine geçmez.

### Temel Kurallar

1. Stage completion deployment anlamına gelmez.
2. Milestone completion sonraki development stage'lerinin tamamlandığı anlamına gelmez.
3. Bir milestone ancak kendi production gate'i ve remote production verification'ı
   tamamlandığında COMPLETE sayılır.
4. Daha sonraki capability development, daha önce release edilmiş bağımsız
   capability'nin çalışmasını gereksiz yere durdurmamalıdır.
5. Deployment milestone product/domain truth üretmez; canonical product ve
   architecture contracts geçerliliğini korur.

## 2. Production Continuity Invariant

A later capability may extend the product,
but must not require an already released,
independently valuable capability to become unavailable.

Örnek:

Repository Proof     ✅ production
Profile Proof        🚧 development

Repository Proof çalışmaya devam eder.

Sonraki aşamada:

Repository Proof     ✅ production
Profile Proof        ✅ beta
Job Proof            🚧 development

Profile Proof ve Repository Proof çalışmaya devam eder.

## 3. Milestone Dependency Matrix

| Milestone | Capability / Development Dependency | Release Level |
|---|---|---|
| A — Repository Proof Production Baseline | Existing Repository Proof capability | PUBLIC / STABLE BASELINE |
| B — Profile Proof Beta | Stage 7 / P3–P9 COMPLETE + P9 validation PASS + P10 COMPLETE | PUBLIC BETA |
| C — Job Proof Engine Alpha | J1–J4 complete | PRIVATE ALPHA |
| D — Candidate Job Proof Beta | J5–J9 complete + J8 validation PASS | PUBLIC BETA |
| E — Candidate Job Proof V1 | Candidate beta production maturity | PUBLIC STABLE / V1 |
| F — Recruiter Evidence View | J10 | PRIVATE PILOT → CONTROLLED |
| G — Optional LLM Narration | J11 | OPTIONAL ENHANCEMENT |

## 4. Milestone Status

Milestone operational status için sade vocabulary kullanılır:

NOT_STARTED
IN_PROGRESS
BLOCKED
COMPLETE

COMPLETE yalnız production deploy + smoke + remote production verification
başarıyla tamamlandığında kullanılabilir.

# Milestone A — Repository Proof Production Baseline

## Amaç

Mevcut Repository Proof yeteneğini Softbreak VPS üzerinde güvenilir production
baseline haline getirmek.

## Ana Kullanıcı Sorusu

Bu repository dışarıdan gözlemlenebilir olarak neyi kanıtlıyor?

## Entry Condition

- Repository Proof capability çalışıyor.
- Repository-level canonical contracts mevcut.
- Evidence-first web report kullanılabilir.
- Mevcut repository analysis validation/test hattı kullanılabilir durumda.

## Kapsam

- public repository URL girişi,
- GitHub repository data collection,
- mevcut 15 signal / 4 category analysis,
- RepositoryAnalysis,
- evidence-first report,
- Türkçe/İngilizce presentation,
- web report,
- improvement recommendations,
- limitations / uncertainty,
- Light/Dark UI,
- responsive report,
- canonical report semantics.

## Kapsam Dışı

- GitHub profile analysis,
- cross-repository pattern,
- job posting,
- JobEvidenceAlignment,
- zorunlu GitHub Sign-In,
- recruiter view.

## Production Gate

- mevcut repository tests PASS,
- canonical repository contracts PASS,
- production configuration development'tan ayrılmış,
- GitHub API credentials production-safe,
- secrets repository dışında,
- VPS reverse proxy / TLS hazır,
- health endpoint doğrulanmış,
- deploy artifact reproducible,
- gerçek public repository ile smoke test PASS,
- locale switch analysis'i yeniden çalıştırmıyor,
- rollback procedure doğrulanmış,
- temel production logging çalışıyor.

## Exit Condition

- Softbreak VPS deployment tamamlandı,
- public production endpoint erişilebilir,
- gerçek public repository smoke PASS,
- production health PASS,
- remote production verification PASS,
- rollback readiness doğrulandı.

## Release Level

PUBLIC / STABLE BASELINE

## Release Sonucu

Repository Proof ✅ canlı

# Milestone B — Profile Proof Beta

## Development Dependency

- Stage 7 / Profile P3–P9 COMPLETE
- P9 Validation PASS
- P10 — Profile Web Flow COMPLETE

Important:

P10 is a deployment/presentation dependency for the public Profile Proof Beta.
It does not become part of Stage 7 Profile Evidence Foundation and does not
change the canonical Stage 7 = P3–P9 execution boundary.

## Amaç

GitHub Proof'u tek repository analizinden çıkarıp gerçek public GitHub profile
evidence ürününe dönüştürmek.

## Ana Kullanıcı Sorusu

Bu public GitHub profilinde hangi teknik üretim örüntüleri tekrar tekrar görünür?

## Entry Condition

- Stage 7 / P3–P9 tamamlanmış,
- profile-level structured truth mevcut,
- deterministic validation PASS,
- prohibited inference kontrolleri PASS.

## Kapsam

- GitHub profile identity,
- GitHubProfileSnapshot,
- public repository inventory,
- P2 repository eligibility policy,
- repository analysis orchestration,
- RepositoryAnalysis[],
- ObservedProfileEvidence,
- cross-repository quantified evidence,
- RepeatedProfilePattern,
- Technical Focus,
- Engineering Practice,
- Problem Area patterns,
- RepresentativeRepository,
- profile limitations / warnings,
- DeveloperPublicEvidenceProfile,
- profile-level report,
- repository-level evidence drill-down,
- deterministic replay,
- validation profile set.

## Safety Sınırları

- Developer Score yok.
- Seniority yok.
- “Bu kişi X biliyor” kesinliği yok.
- Sparse profile negatif developer sonucu değildir.
- Fork/template independent repetition'ı şişiremez.
- Historical repository current-focus olarak sunulamaz.
- Profile truth job-agnostic kalır.

## Production Gate

- Stage 7 / P3–P9 COMPLETE,
- P10 Profile Web Flow COMPLETE,
- unsupported profile claim = 0,
- broken evidence traceability = 0,
- prohibited developer inference = 0,
- deterministic replay PASS,
- varied profile validation PASS,
- sparse/fork-heavy/archive-heavy profile senaryoları PASS,
- GitHub rate-limit davranışı tanımlı,
- partial GitHub API failure davranışı tanımlı,
- profile analysis timeout/failure UX hazır,
- production telemetry profile flow'u izleyebiliyor,
- birden fazla gerçek GitHub kullanıcısı ile smoke PASS.

## Exit Condition

- Profile Proof production'da erişilebilir,
- production profile smoke set PASS,
- evidence drill-down doğrulanmış,
- production telemetry doğrulanmış,
- remote production verification PASS.

## Release Level

PUBLIC BETA

## Release Sonucu

Repository Proof ✅
Profile Proof    ✅ Beta

# Milestone C — Job Proof Engine Alpha

## Dependency

J1 JobRequirementProfile Domain Contract
J2 Job Requirement Extraction
J3 JobEvidenceAlignment Domain Contract
J4 Deterministic Alignment Engine

## Amaç

İlk kez belirli bir iş ilanıyla GitHub evidence'ını deterministic olarak
karşılaştırmak.

## Ana Kullanıcı Sorusu

Bu iş ilanının teknik beklentilerinin hangileri public GitHub evidence ile destekleniyor?

## Entry Condition

- J1–J4 complete,
- requirement extraction source-faithful,
- deterministic alignment contract hazır,
- double traceability korunuyor.

## Kapsam

- job posting text input,
- gerekirse normalized job-source representation,
- source-faithful requirement extraction,
- JobRequirementProfile,
- requirement source traceability,
- DeveloperPublicEvidenceProfile + JobRequirementProfile,
- deterministic requirement-level alignment,
- JobEvidenceAlignment,
- double traceability.

## Safety Sınırları

Yok:

- Job Fit percentage,
- Candidate Score,
- Seniority,
- Hire / Reject,
- Employability score.

Public evidence not found
!=
developer does not know the technology

## Production Kullanım Şekli

PRIVATE ALPHA

Yalnız kontrollü kullanıcılar / test kullanıcıları / davet edilen geliştiriciler.

## Production Gate

- J1–J4 locked/complete,
- raw keyword matching canonical truth değil,
- unsupported requirement extraction = 0,
- requirement source traceability PASS,
- GitHub-side traceability PASS,
- no fit score,
- no candidate score,
- no seniority,
- no hire decision,
- same inputs + same versions => same result,
- diverse real job posting validation,
- malformed/ambiguous postings safe handling,
- unsupported job-source content certainty'ye çevrilmiyor.

## Exit Condition

- private production endpoint doğrulanmış,
- varied real job-posting smoke set PASS,
- double traceability production'da doğrulanmış,
- remote production verification PASS.

## Release Level

PRIVATE ALPHA

# Milestone D — Candidate Job Proof Beta

## Dependency

J5–J9 complete
+
J8 validation PASS

## Amaç

Candidate-facing ilk tam Job Proof ürününü production'a çıkarmak.

## Ana Ürün Mesajı

İlanı ekle. GitHub'ının bu iş için ne kanıtladığını gör.

## Entry Condition

- candidate workflow complete,
- J8 validation PASS,
- auth / identity boundary hazır,
- Job Evidence Alignment presentation contract hazır.

## Kapsam

- GitHub Sign-In,
- trusted GitHub identity,
- profile analysis / reuse,
- job URL veya job text input,
- JobRequirementProfile,
- JobEvidenceAlignment,
- requirement-level candidate report,
- job-specific representative repositories,
- evidence visibility improvement actions,
- why-this-conclusion,
- job-source drill-down,
- GitHub evidence drill-down,
- limitations,
- candidate-facing responsive UI,
- Türkçe/İngilizce.

## Safety Sınırları

Evidence visibility improvement:

Docker kullanımını README'de daha görünür hale getir.

Skill-gap inference değildir:

Docker öğren.

şeklinde unsupported developer judgment üretilemez.

## Production Gate

- J8 validation PASS,
- candidate-facing report contract PASS,
- unsupported alignment claim = 0,
- broken double traceability = 0,
- OAuth/security flow PASS,
- session/authentication behavior PASS,
- job URL/text error states PASS,
- GitHub API/token handling PASS,
- rate limiting / throttling,
- request abuse controls,
- privacy / KVKK assessment,
- production logging,
- error monitoring,
- backup / recovery,
- smoke tests,
- responsive/mobile checks,
- accessibility-relevant states,
- rollback readiness.

## Exit Condition

- public candidate flow production'da tamamlanabiliyor,
- auth/profile/job/report end-to-end smoke PASS,
- security/operations gate PASS,
- remote production verification PASS.

## Release Level

PUBLIC BETA

# Milestone E — Candidate Job Proof V1 / Public Stable

## Amaç

Candidate Beta'yı deneysel seviyeden güvenilir production-ready V1 seviyesine
taşımak.

## Entry Condition

- Milestone D COMPLETE,
- gerçek beta kullanım verisi mevcut,
- kritik beta blockers çözülmüş veya bounded.

## Kapsam

- beta feedback fixes,
- gerçek kullanıcı edge case'leri,
- performance,
- cache stratejisi,
- repeated analysis optimizasyonu,
- reliability,
- profile refresh semantics,
- gerekirse job analysis history,
- observability,
- abuse protection,
- production error recovery,
- user-facing copy refinement,
- onboarding refinement.

## Production Gate / Başarı Ölçütleri

- analysis completion rate kabul edilebilir,
- error rate düşük,
- unsupported claims = 0,
- traceability regressions = 0,
- critical security finding = 0,
- kullanıcı raporu anlayabiliyor,
- kullanıcı “neden?” sorusunun cevabını görebiliyor,
- aynı input tekrarında semantic drift yok.

## Exit Condition

- V1 reliability targets PASS,
- critical production issues kapalı,
- remote production verification PASS,
- candidate product production-ready V1 olarak işaretlenebilir.

## Release Level

PUBLIC STABLE / V1

# Milestone F — Recruiter Evidence View

## Dependency

J10

## Amaç

Candidate için üretilen aynı canonical JobEvidenceAlignment truth'u recruiter
için farklı presentation ile göstermek.

Yeni analysis engine yoktur.

JobEvidenceAlignment
        ↓
Candidate Presentation

JobEvidenceAlignment
        ↓
Recruiter Presentation

## Entry Condition

- canonical JobEvidenceAlignment stable,
- recruiter presentation contract hazır,
- candidate/profile evidence provenance korunuyor.

## Recruiter View

Gösterebilir:

- job requirement'ları,
- görünür GitHub evidence,
- supporting repositories,
- evidence chain,
- profile pattern'ları,
- interview'de doğrulanabilecek alanlar,
- limitations,
- evidence unavailable durumları.

Üretemez:

- Hire,
- Reject,
- Candidate Score,
- Ranking,
- Seniority assignment,
- Employability score,
- “en iyi aday”.

## Production Gate

- same canonical JobEvidenceAlignment consumption verified,
- recruiter UI yeni truth üretmiyor,
- no ranking semantics,
- no hire recommendation,
- privacy/access boundary,
- recruiter-facing auditability,
- candidate evidence provenance korunuyor.

## Exit Condition

- controlled recruiter pilot çalışıyor,
- truth parity candidate presentation ile doğrulanmış,
- privacy/access controls PASS,
- remote production verification PASS.

## Release Level

PRIVATE / PILOT
→
PUBLIC / CONTROLLED

# Milestone G — Optional LLM Narration

## Dependency

J11

## Amaç

Deterministic sonucu daha doğal ve okunabilir biçimde açıklamak.

LLM analysis engine değildir.

Canonical deterministic truth
↓
LLM narration
↓
user-friendly explanation

## Entry Condition

- deterministic product LLM olmadan eksiksiz çalışıyor,
- canonical truth yapısı stable,
- narration input/output boundary tanımlı.

## LLM Sınırı

LLM:

- requirement ekleyemez,
- evidence icat edemez,
- evidence state değiştiremez,
- score üretemez,
- hire recommendation yapamaz,
- seniority belirleyemez.

Kritik acceptance criterion:

LLM OFF
↓
ürün hâlâ tamamen çalışıyor

## Production Gate

- deterministic truth ↔ narration factual consistency tests,
- hallucination guard,
- structured input/output boundaries,
- fallback without LLM,
- timeout/failure behavior,
- cost controls,
- prompt/version management,
- provider outage core product'ı bozmuyor.

## Exit Condition

- LLM-disabled fallback PASS,
- factual consistency PASS,
- provider failure behavior PASS,
- production cost/control limits doğrulanmış,
- remote production verification PASS.

## Release Level

OPTIONAL PRODUCTION ENHANCEMENT

# 5. Canonical Release Gate

Her deployment milestone aşağıdaki ortak gate'i uygular:

Capability complete
↓
Relevant canonical contracts satisfied
↓
Relevant tests PASS
↓
Relevant validation PASS
↓
Security/config review
↓
Production environment check
↓
Backup/recovery readiness
↓
Deploy Softbreak VPS
↓
Smoke test
↓
Remote production verification
↓
Deployment Milestone COMPLETE

Milestone-specific gate ortak gate'i daraltabilir veya genişletebilir; sessizce
zayıflatamaz.

# 6. Roadmap Coordination

Development roadmap ve deployment roadmap birlikte okunur.

Örnek:

Development:
Stage 7 / Profile P3

Production:
Milestone A — COMPLETE
Milestone B — BLOCKED by P3–P9

Başka örnek:

Development:
J5

Production:
Milestone A — COMPLETE
Milestone B — COMPLETE
Milestone C — COMPLETE
Milestone D — BLOCKED by J5–J9

Bu iki koordinat worklog/CURRENT.md içinde kısa operational state olarak
taşınabilir.

# 7. Non-Goals

Bu belge:

- product/domain semantics yeniden tanımlamaz,
- repository/profile/job evidence contracts'ı override etmez,
- scoring contract değiştirmez,
- deployment implementation script'i değildir,
- infrastructure architecture belgesi değildir,
- secrets/config values taşımaz.

Çelişkide canonical product/domain/architecture contracts geçerlidir.
