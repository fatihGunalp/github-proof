# GitHub Proof — Job Evidence Alignment Product Contract

**Status:** CONTRACT LOCKED
**Contract Scope:** Job Evidence Alignment V1
**Contract Version:** 1
**Repository Dependency:** Repository MVP V1 preserved
**Profile Dependency:** Public Profile Evidence V1 preserved
**Locked:** 2026-08-16

---

## 1. Ürün Tanımı

Job Evidence Alignment, GitHub Proof'un bir yazılım geliştiricinin public GitHub
üretimini belirli bir iş ilanının teknik beklentileriyle karşılaştıran
top-level ürün katmanıdır.

Temel ürün sorusu:

> Bu iş için public GitHub'ım gerçekten neyi kanıtlıyor?

Candidate-facing kısa ürün mesajı:

> İlanı yapıştır. GitHub'ının bu iş için ne kanıtladığını gör.

Uzun ürün tanımı:

> GitHub Proof, bir yazılım geliştiricinin public GitHub üretimini belirli bir
> iş ilanının teknik beklentileriyle karşılaştırır ve hangi beklentilerin
> gözlemlenebilir public kanıtlarla desteklendiğini açıklar.

Bu ürün:

- geliştiricinin bütün yetkinliğini modellemez,
- iş için genel uygunluk kararı vermez,
- işe alım kararı vermez,
- public GitHub dışında kalan deneyimi varsaymaz.

Sistem yalnızca tanımlanmış job-source truth ile tanımlanmış public GitHub
evidence surface arasındaki görünür ilişki hakkında konuşur.

---

## 2. Ana Kullanıcı Problemi

Bir geliştirici belirli bir iş ilanını gördüğünde genellikle şu soruyu sormak
ister:

> Bu ilandaki teknik beklentilerin hangileri benim public GitHub profilimde
> gerçekten görünür?

Asıl problem yalnız:

> Bu teknolojiyi biliyor muyum?

değildir.

GitHub Proof'un cevaplamaya çalıştığı daha dar ve doğrulanabilir soru:

> Public GitHub profilime bakan biri, bu ilanda istenen teknik alanların
> hangilerinde görünür üretim kanıtı görebilir?

Bu ayrım ürünün temel epistemik sınırıdır.

---

## 3. İlk Hedef Kullanıcı

Job Evidence Alignment V1'in ilk primary user'ı:

    software developer / candidate

Primary use moment:

    belirli bir iş ilanına başvurmadan önce
    veya
    başvuru hazırlığı sırasında

Ürün ilk etapta adaya:

- public evidence durumunu anlamayı,
- hangi requirement'ların görünür kanıtlarla desteklendiğini görmeyi,
- hangi repository'lerin ilgili ilan için daha anlamlı olduğunu görmeyi,
- neden bu sonuca ulaşıldığını incelemeyi,
- evidence visibility iyileştirme fırsatlarını görmeyi

sağlamalıdır.

Recruiter-facing kullanım aynı structured truth'un daha sonraki consumer'ıdır.

---

## 4. Ürün Katmanları

GitHub Proof aşağıdaki birbirinden ayrı fakat birbirini besleyen capability
katmanlarını korur.

    Repository Evidence
            ↓
    Public Profile Evidence
            ↓
    DeveloperPublicEvidenceProfile

ve:

    Job Posting
            ↓
    Job Requirement Truth
            ↓
    JobRequirementProfile

Bu iki structured truth daha sonra:

    DeveloperPublicEvidenceProfile
            +
    JobRequirementProfile
            ↓
    JobEvidenceAlignment

ilişkisiyle birleştirilir.

Job Evidence Alignment bu alt katmanların replacement'ı değildir.

---

## 5. Repository MVP V1 ile İlişki

Repository MVP V1 korunur.

Repository-level temel soru değişmez:

> Bu public repository dışarıdan gözlemlenebilir olarak neyi kanıtlıyor?

RepositoryAnalysis:

    repository-level atomic analysis truth

olarak korunur.

Job Evidence Alignment:

- repository sinyallerini yeniden tanımlamaz,
- 15 signal / 4 category sözleşmesini değiştirmez,
- repository skorlarını yeniden hesaplamaz,
- repository Missing / unavailable semantics'ini değiştirmez,
- Razor veya presentation katmanında ikinci bir repository analyzer üretmez.

Repository evidence, profile foundation üzerinden Job Evidence Alignment'a
ulaşır.

---

## 6. Public Profile Evidence V1 ile İlişki

Public Profile Evidence V1 korunur.

Canonical dependency:

    docs/product/public-profile-evidence-product-contract.md

Public Profile Evidence'ın görevi:

> Bir public GitHub profilinde repository'ler arasında tekrar eden görünür
> teknik üretim, problem alanı ve engineering-practice evidence'ını
> yapılandırılmış biçimde üretmek.

Job Evidence Alignment için Public Profile Evidence:

    reusable job-agnostic evidence foundation

olarak kullanılır.

DeveloperPublicEvidenceProfile job-specific state taşımamalıdır.

Özellikle şunları içermemelidir:

- belirli iş ilanı,
- JobRequirementProfile,
- Required / Preferred benzeri job importance state'i,
- JobEvidenceAlignment,
- job-specific repository selection,
- job-specific evidence visibility action.

---

## 7. Üç Ayrı Structured Truth

Ürün üç farklı semantic truth'u kesin olarak ayırır:

    DeveloperPublicEvidenceProfile

    JobRequirementProfile

    JobEvidenceAlignment

Bu kavramlar birbirinin yerine geçmez.

DeveloperPublicEvidenceProfile:

    public GitHub profile evidence truth

JobRequirementProfile:

    one specific job posting's normalized requirement truth

JobEvidenceAlignment:

    those two truths arasındaki structured comparison truth

JobEvidenceAlignment iki input'u consume eder.

Onları yeniden tanımlamaz.

---

## 8. Job Requirement Truth

Raw job posting doğrudan GitHub evidence ile karşılaştırılmaz.

Zorunlu yön:

    Raw Job Posting
            ↓
    Job Requirement Extraction
            ↓
    JobRequirementProfile

JobRequirementProfile iş ilanının normalized technical expectation truth'udur.

Bu contract exact domain shape'i kilitlemez.

Ancak product-level olarak şu özellikler zorunludur:

- source-faithful olmalı,
- technical requirement'ları yapılandırılmış biçimde taşımalı,
- source'a geri izlenebilir olmalı,
- source'un anlamlı importance ayrımlarını mümkün olduğunda korumalı,
- belirsizliği certainty'ye dönüştürmemeli.

---

## 9. Job Requirement Source Fidelity

Her accepted normalized requirement job source tarafından desteklenmelidir.

Zorunlu trust direction:

    Normalized Job Requirement
            ↓
    source job statement / source span

Sistem source tarafından desteklenmeyen:

- technology,
- framework,
- practice,
- responsibility,
- experience expectation,
- importance level

icat edemez.

Job requirement extraction bir inference invention engine değildir.

---

## 10. Requirement Importance İlkesi

İş ilanı requirement importance'ını güvenilir biçimde ayırıyorsa bu semantik
bilgi korunmalıdır.

Aday semantic örnekleri:

    Required
    Preferred
    Optional

Bu product contract exact enum isimlerini kilitlemez.

Kilitlenen ürün ilkesi:

    source importance may be preserved
    but must not be invented

Source importance belirsizse sistem requirement'ı sessizce Required veya
Optional yapamaz.

---

## 11. Job Evidence Alignment Tanımı

Job Evidence Alignment şu anlama gelir:

> Belirli bir iş ilanındaki teknik beklentilerin defined public GitHub evidence
> surface üzerinde hangi görünür evidence durumuyla desteklendiğini açıklayan
> structured comparison truth.

Alignment insan hakkında bütünsel uygunluk kararı değildir.

Özellikle:

    JobEvidenceAlignment
    !=
    JobFit

    JobEvidenceAlignment
    !=
    CandidateScore

    JobEvidenceAlignment
    !=
    Employability

    JobEvidenceAlignment
    !=
    HireRecommendation

---

## 12. Requirement-Level Alignment

Job Evidence Alignment requirement-level çalışmalıdır.

Örnek conceptual yapı:

    C# / .NET
    → requirement evidence result

    REST API
    → requirement evidence result

    SQL
    → requirement evidence result

    Automated Testing
    → requirement evidence result

    Docker
    → requirement evidence result

    Azure
    → requirement evidence result

Her requirement'ın public evidence durumu farklı olabilir.

Bu nedenle bir opaque overall candidate number requirement-level truth'un
yerine geçemez.

---

## 13. Evidence State Semantic Families

Future domain contract requirement-level evidence sonuçlarını farklı semantic
durumlarda ifade edebilmelidir.

En az aşağıdaki durum aileleri desteklenmelidir:

- substantial / repeated visible evidence,
- partial or limited visible evidence,
- public evidence evaluated but not found,
- required evidence unavailable,
- requires human review.

Bu contract:

- exact enum ismini,
- exact threshold'u,
- numeric weighting'i,
- UI label'ını,
- renk semantiğini

kilitlemez.

Bu state'ler yalnız public evidence durumunu ifade eder.

İnsan yetkinliği hakkında kesin hüküm değildir.

---

## 14. Public Evidence Not Found Sınırı

Zorunlu ürün invariant'ı:

    Public evidence not found
    !=
    user does not know the technology

Örneğin doğru ifade:

> Public GitHub profilinde bu requirement'ı doğrulayacak yeterli görünür kanıt
> bulunamadı.

Yanlış ifade:

> Bu kullanıcı Azure bilmiyor.

GitHub Proof yalnız evidence surface hakkında konuşabilir.

---

## 15. Limited Evidence Sınırı

Zorunlu:

    Limited public evidence
    !=
    weak skill

Zorunlu:

    Sparse public evidence
    !=
    inexperienced developer

Zorunlu:

    Evidence unavailable
    !=
    capability absent

Evidence eksikliği negatif mirror inference üretemez.

---

## 16. Negative Mirror Inference Yasağı

Sistem pozitif evidence yokluğunu geliştirici hakkında ters yönlü negatif
sonuca çeviremez.

Yasak örnekler:

    no visible Docker evidence
    →
    does not know Docker

    limited automated-test evidence
    →
    weak at testing

    no Azure evidence
    →
    no Azure experience

    sparse GitHub
    →
    weak developer

Doğru semantic alan:

> Defined public evidence surface bu claim'i yeterli biçimde desteklemiyor.

---

## 17. Double Traceability

Job Evidence Alignment iki bağımsız truth root'a geri izlenebilir olmalıdır.

### Job Side

    Job alignment result
            ↓
    normalized job requirement
            ↓
    source job statement / source span

### GitHub Side

    Job alignment result
            ↓
    structured profile evidence
            ↓
    repeated pattern or other contracted support
            ↓
    quantified observations
            ↓
    repository references
            ↓
    repository-level evidence

Her iki chain de canonical alignment truth için zorunludur.

---

## 18. Why This Conclusion?

Her meaningful alignment sonucu şu soruyu cevaplayabilmelidir:

> Sistem neden bu sonuca ulaştı?

Yanıt structured truth'tan türetilmelidir.

Gerektiğinde şunları gösterebilmelidir:

- source job requirement,
- source importance,
- evidence state,
- quantified observations,
- supporting repositories,
- repository-level evidence,
- limitations,
- human-review requirement.

Generated prose tek başına explanation source'u değildir.

---

## 19. Raw Keyword Matching Yasağı

Canonical alignment truth basit keyword intersection olamaz.

Yasak semantic shortcut:

    Job says Docker
            +
    README says Docker
            =
    Docker requirement supported

README veya source keyword'ü evidence input'u olabilir.

Ancak canonical Job Evidence Alignment sonucuna dönüşebilmesi için daha sonra
kilitlenecek structured evidence kurallarını karşılamalıdır.

---

## 20. Job Context Kanıt Gücünü Artıramaz

Bir requirement'ın iş ilanında bulunması underlying GitHub evidence'ın
epistemic gücünü artırmaz.

Yasak:

    job asks for Docker
            +
    one weak Docker observation
            →
    practical Docker experience inference

Job requirement yalnız comparison context sağlar.

Weak evidence job yüzünden strong evidence'a dönüşmez.

---

## 21. Evidence Coverage ve Job Fit Ayrımı

Future presentation descriptive evidence coverage gösterebilir ancak yalnız
explicit bounded semantic review sonrasında.

Örnek evidence-only descriptive metric:

> 8 evaluated requirement'ın 5'inde visible public evidence bulundu.

Bu sonuç:

> %62.5 bu işe uygunsun.

anlamına gelemez.

Zorunlu:

    evidence coverage
    !=
    job suitability

Job Evidence Alignment V1 bu contract ile overall fit percentage kilitlemez.

---

## 22. Candidate Suitability Score Yasağı

Ürün aşağıdaki semantic output'ları üretmez:

- JobFit,
- JobFitScore,
- CandidateScore,
- CapabilityScore,
- EmployabilityScore,
- HireScore,
- candidate suitability percentage,
- hire probability.

Repository score'ları veya requirement result'ları candidate score'a
aggregate edilemez.

---

## 23. Hiring Decision Yasağı

GitHub Proof:

- aday kabul etmez,
- aday reddetmez,
- hire/no-hire üretmez,
- automatic approval üretmez,
- automatic rejection üretmez,
- hiring recommendation üretmez.

Job Evidence Alignment evidence-understanding ürünüdür.

Employment decision engine değildir.

---

## 24. Candidate Ranking Yasağı

Job Evidence Alignment adayları sıralamaz.

Yasak:

    Candidate A > Candidate B

    Candidate A is stronger

    Candidate B should rank lower

Bu sınır future recruiter presentation için de geçerlidir.

---

## 25. Seniority ve Experience-Year Sınırı

Public GitHub evidence otomatik olarak:

- Junior,
- Mid,
- Senior,
- Staff,
- Principal

assignment'ına dönüştürülemez.

Repository timeline veya commit history:

    X years of professional experience

sonucuna dönüştürülemez.

---

## 26. Salary Sınırı

GitHub Proof public GitHub evidence'tan:

- salary recommendation,
- compensation band,
- expected salary,
- pay grade

üretemez.

Bu Job Evidence Alignment scope'u dışındadır.

---

## 27. Bounded Inference ile İlişki

Session 051 aşağıdaki design direction'ı preserve etmiştir:

    Observed Evidence
            ↓
    Repeated Pattern
            ↓
    Evidence-backed Bounded Inference

Ancak:

    EvidenceBackedProfileInference

henüz contract-locked canonical profile-domain truth değildir.

Bu nedenle Job Evidence Alignment V1 core product flow'u bounded inference'a
zorunlu dependency yapamaz.

Core alignment en az:

- structured public profile evidence,
- contracted repeated patterns

üzerinden çalışabilmelidir.

Profile P1 ileride bounded inference'ı contract-lock ederse Job Evidence
Alignment bunu optional supporting context olarak kullanabilir.

Job Evidence Alignment kendi başına capability/experience inference
üretmemelidir.

---

## 28. RoleEvidenceAlignment ile İlişki

Existing profile concept:

    RoleEvidenceAlignment

ile:

    JobEvidenceAlignment

aynı değildir.

RoleEvidenceAlignment:

    generic/versioned technical role definition
    ↔
    profile evidence

JobEvidenceAlignment:

    one specific JobRequirementProfile
    ↔
    profile evidence

RoleEvidenceAlignment JobEvidenceAlignment için mandatory core input değildir.

---

## 29. Profile Representative Repository Sınırı

Public Profile Evidence içindeki RepresentativeRepository:

> overall public profile pattern'larını yoğun biçimde görünür hâle getiren
> repository

anlamındadır.

Job-specific repository selection ise:

> bu belirli iş ilanının requirement'ları açısından relevant public evidence'ı
> en görünür biçimde taşıyan repository

anlamındadır.

Zorunlu ayrım:

    Profile RepresentativeRepository
    !=
    Job-specific Representative Repository

---

## 30. Job-Specific Repository Relevance

Job-specific representative/relevant repository şu anlamlara gelmez:

- best repository,
- highest-quality repository,
- hardest repository,
- most professional repository,
- proof of candidate superiority.

Yalnızca:

> Bu repository bu belirli job requirement seti için ilgili public evidence
> taşıyor.

anlamına gelir.

Exact selection/ranking algorithm daha sonraki domain contract'a aittir.

---

## 31. Evidence Visibility Improvement

Ürün job-specific evidence visibility fırsatları gösterebilir.

Temel soru:

> Bu ilan açısından relevant mevcut public çalışmam nasıl daha görünür ve
> doğrulanabilir olabilir?

Bu concept:

    evidence visibility opportunity

olarak ele alınmalıdır.

Zorunlu:

    evidence visibility opportunity
    !=
    skill gap

---

## 32. Evidence Visibility Action Sınırı

Sistem yalnız observable evidence ve repository presentation yüzeyine bağlı
iyileştirme önerileri üretebilir.

Örnek uygun yönler:

- relevant setup evidence'ını daha görünür yapmak,
- README'den ilgili implementation'a yönlendirmek,
- test evidence'ını kolay keşfedilebilir yapmak,
- deployment/configuration evidence'ını açıklamak,
- zaten mevcut public work'ün amacını daha görünür sunmak.

Sistem:

- yapılmamış işi yapılmış gibi gösteremez,
- deneyim icat edemez,
- skill absence varsayamaz,
- kullanıcıyı yanıltıcı claim üretmeye yönlendiremez.

---

## 33. Candidate ve Recruiter Shared Truth

Candidate ve recruiter presentation aynı structured JobEvidenceAlignment
truth'u tüketmelidir.

    JobEvidenceAlignment
            ↓
    Candidate View

ve:

    same JobEvidenceAlignment
            ↓
    Recruiter View

Presentation farklı olabilir.

Canonical evidence truth farklı olamaz.

Hidden recruiter-only candidate score veya hidden recruiter-only alignment
engine yasaktır.

---

## 34. Recruiter Kullanım Sınırı

Future recruiter presentation şu amaçlarda kullanılabilir:

- hızlı technical orientation,
- public evidence navigation,
- repository inspection routing,
- interview preparation,
- evidence-backed interview topics.

Şunlar için kullanılamaz:

- automatic rejection,
- automatic approval,
- hire/no-hire,
- candidate ranking,
- seniority assignment,
- employability score.

---

## 35. LLM Sınırı

LLM canonical evidence authority değildir.

Future LLM şu alanlarda bounded biçimde kullanılabilir:

- Job Requirement Extraction altında normalization desteği,
- explanation wording,
- localization,
- evidence visibility wording,
- interview question wording.

LLM şu kararları tek başına veremez:

- source'da olmayan requirement üretmek,
- requirement importance icat etmek,
- unsupported profile evidence yaratmak,
- supporting repository icat etmek,
- alignment state upgrade etmek,
- capability sonucu üretmek,
- seniority üretmek,
- candidate suitability üretmek,
- hire/no-hire üretmek.

Structured contracted truth authoritative kalır.

---

## 36. Raw LLM Judgment Yasağı

Yasak architecture:

    raw job posting
            +
    raw GitHub
            ↓
    LLM
            ↓
    canonical candidate judgment

LLM fluency evidence strength değildir.

Fluent explanation canonical truth'u yükseltemez.

---

## 37. Determinism

Aynı:

- JobRequirementProfile,
- DeveloperPublicEvidenceProfile,
- alignment semantic rule versions

ile aynı structured JobEvidenceAlignment truth yeniden üretilebilir olmalıdır.

Presentation copy değişimi semantic truth'u değiştirmemelidir.

---

## 38. Versioning

Aşağıdaki semantic değişiklikler version-aware olmalıdır:

- requirement normalization semantics,
- requirement importance semantics,
- alignment semantics,
- evidence-support rules.

Exact version object ve field'lar downstream domain contract'lara aittir.

---

## 39. Localization

Locale değişikliği:

- requirement truth'u,
- importance truth'u,
- profile evidence truth'u,
- alignment truth'u,
- repository support truth'u

değiştirmemelidir.

Localization presentation boundary'de kalır.

---

## 40. Limitations

Product first-class limitation state'lerini desteklemelidir.

Örnekler:

- ambiguous job requirement,
- unavailable job source,
- incomplete extraction,
- unavailable GitHub evidence,
- sparse public profile,
- repository eligibility limitation,
- broken traceability,
- requires human review.

Limitation negatif candidate judgment değildir.

---

## 41. Missing Prerequisite Truth

Required prerequisite truth üretilemiyorsa dependent truth fabricate edilemez.

Örneğin:

    reliable JobRequirementProfile yok
            ↓
    canonical JobEvidenceAlignment varmış gibi davranılamaz

ve:

    required public profile evidence unavailable
            ↓
    GitHub evidence fabricate edilemez

Doğru davranış:

    unavailable / limitation / review state

---

## 42. Job Source Revision

Material olarak değişmiş iş ilanı yeni veya updated source truth olarak ele
alınmalıdır.

Sistem stale normalized requirement'ları current ilan truth'u gibi sessizce
kullanmamalıdır.

Exact source identity/revision model Job Requirement Domain Contract'a
delege edilir.

---

## 43. Public Evidence Surface Sınırı

Current product truth defined public GitHub evidence surface ile sınırlıdır.

GitHub Proof otomatik olarak şunları varsayamaz:

- private GitHub repository content,
- company-internal code,
- NDA projects,
- offline work,
- work hosted elsewhere,
- spoken technical knowledge.

Public GitHub'da görünmemesi:

> bu deneyim yok

anlamına gelmez.

---

## 44. GitHub Identity ve Authentication Sınırı

Candidate flow low-friction olmalıdır.

Trusted GitHub identity ürün tarafından zaten biliniyorsa kullanıcıdan aynı
GitHub username tekrar istenmemelidir.

Ancak product contract yalnız semantic user-flow requirement'ını tanımlar.

Şunları tanımlamaz:

- OAuth protocol,
- token storage,
- claims,
- cookie/session implementation,
- authorization scopes,
- API route shape.

Authenticated identity ayrıca:

- her repository satırının authorship proof'u,
- capability proof'u,
- seniority proof'u,
- professional-history proof'u

değildir.

---

## 45. İlk Candidate Flow

İlk candidate-facing flow en az şu conceptual sırayı desteklemelidir:

    trusted GitHub identity / public profile
            ↓
    DeveloperPublicEvidenceProfile available
            ↓
    user provides one specific job posting
            ↓
    JobRequirementProfile produced
            ↓
    JobEvidenceAlignment produced
            ↓
    requirement-level results
            ↓
    why-this-conclusion
            ↓
    job-relevant repositories
            ↓
    evidence visibility opportunities

Exact Web implementation bu contract'ın konusu değildir.

---

## 46. İlk Okuma Hedefi

Candidate ilk okuma katmanında kısa sürede şu soruları cevaplayabilmelidir:

1. Bu ilanda hangi teknik beklentiler var?
2. Hangileri public GitHub kanıtlarımla görünür biçimde destekleniyor?
3. Hangilerinde yalnız sınırlı public evidence var?
4. Hangileri için public evidence bulunamadı?
5. Sistem neden böyle söyledi?
6. Bu ilan için hangi repository'lerimi öne çıkarmak daha anlamlı?
7. Hangi mevcut public evidence daha görünür hâle getirilebilir?
8. Hangi sonuçlar evidence limitation nedeniyle kesinleştirilemedi?

Bu UI layout specification değildir.

Product information goal'dür.

---

## 47. Candidate / Recruiter Truth Boundary

Candidate ve recruiter farklı first-reading hierarchy kullanabilir.

Ancak:

    JobEvidenceAlignment

tek canonical comparison truth olarak kalmalıdır.

Recruiter için farklı gizli semantic engine kurulamaz.

---

## 48. Job Requirement Extraction Dependency

Raw job source'tan JobRequirementProfile üretimi ayrı contract tarafından
yönetilmelidir.

Canonical dependency:

    docs/product/job-requirement-extraction-contract.md

Bu contract en az:

- accepted source forms,
- source traceability,
- extraction boundary,
- requirement invention prohibition,
- ambiguity handling,
- importance extraction,
- versioning,
- LLM participation

konularını daha sonra kilitlemelidir.

Job Evidence Alignment Product Contract extraction algorithm'ı uydurmaz.

---

## 49. Job Requirement Domain Dependency

Structured job requirement truth ayrı domain contract tarafından
tanımlanmalıdır.

Canonical dependency:

    docs/architecture/job-requirement-domain-model.md

Bu contract daha sonra:

- JobRequirementProfile,
- requirement identity,
- source references,
- importance representation,
- invariants,
- immutable result shape

gibi konuları kilitlemelidir.

Bu Product Contract exact domain DTO shape'i tanımlamaz.

---

## 50. Job Evidence Alignment Domain Dependency

Structured comparison truth ayrı domain contract tarafından tanımlanmalıdır.

Canonical dependency:

    docs/architecture/job-evidence-alignment-domain-model.md

Bu contract daha sonra:

- JobEvidenceAlignment,
- requirement-level evidence result,
- status semantics,
- limitations,
- warnings,
- rule versions,
- job-specific repository results,
- evidence visibility actions,
- deterministic ordering,
- aggregate invariants

konularını kilitlemelidir.

---

## 51. Report Contract Dependency

Candidate/recruiter presentation ayrı report contract tarafından
tanımlanmalıdır.

Canonical dependency:

    docs/product/job-evidence-alignment-report-contract.md

Report contract:

- first-reading hierarchy,
- labels,
- explanation presentation,
- repository drill-down presentation,
- limitation presentation,
- evidence visibility action presentation,
- localization,
- accessibility-relevant wording

konularını daha sonra kilitlemelidir.

Report contract canonical alignment truth'u yeniden hesaplayamaz.

---

## 52. Product Non-Goals

Job Evidence Alignment şu ürünlerden biri değildir:

- general-purpose resume parser,
- ATS replacement,
- automated hiring decision engine,
- candidate ranking engine,
- seniority estimator,
- employability predictor,
- salary recommendation engine,
- years-of-experience estimator,
- code authorship detector,
- AI-code detector,
- plagiarism detector,
- production-readiness certifier,
- security certification system,
- generic developer capability score.

Job Requirement Extraction'ın bir iş ilanını parse etmesi GitHub Proof'u
general-purpose ATS yapmaz.

---

## 53. Bilerek Kilitlenmeyen Kararlar

Bu Product Contract aşağıdaki kararları bilerek kilitlemez:

### Job Requirement Domain

- exact JobRequirementProfile field'ları,
- exact JobRequirement field'ları,
- exact requirement taxonomy,
- exact technical taxonomy,
- exact importance enum isimleri,
- exact ambiguity state'leri,
- exact source-reference type'ları.

### Job Requirement Extraction

- exact parser algorithm,
- exact LLM involvement,
- supported job websites,
- HTML normalization,
- URL acquisition mechanics,
- anti-bot behavior,
- LinkedIn-specific integration,
- retry behavior.

### Job Evidence Alignment Domain

- exact result type names,
- exact status enum names,
- exact evidence thresholds,
- exact weighting,
- exact contribution rules,
- exact warning/limitation enums,
- exact timestamps,
- exact aggregate DTO shape.

### Repository Relevance

- exact ranking algorithm,
- exact repository count,
- exact tie-breaking,
- exact relevance weighting.

### Evidence Visibility Actions

- exact action taxonomy,
- exact priority algorithm,
- exact recommendation-generation rules.

### Report / UI

- exact sections,
- exact labels,
- exact colors,
- exact layout,
- exact progressive disclosure,
- exact mobile behavior,
- exact recruiter layout.

### Authentication

- OAuth implementation,
- token persistence,
- claims,
- scopes,
- account linking.

### Persistence

- database schema,
- alignment history persistence,
- requirement persistence,
- retention policy.

### Commercial Model

- free limits,
- paid plans,
- recruiter pricing,
- quotas,
- subscriptions.

### Future Evidence Sources

- private GitHub,
- GitLab,
- Bitbucket,
- resume evidence,
- LinkedIn profile evidence,
- portfolio websites,
- coding assessments.

Bu kararlar future explicit contracts veya product decisions ile
kilitlenmelidir.

---

## 54. Product Success Criteria

Job Evidence Alignment V1 product-level olarak başarılı sayılmak için en az:

1. Belirli bir job posting structured job requirement truth'a dönüşebilmelidir.
2. Source tarafından desteklenen requirement importance korunabilmelidir.
3. Normalized requirements source'a geri izlenebilmelidir.
4. DeveloperPublicEvidenceProfile job-agnostic kalmalıdır.
5. Alignment requirement-by-requirement üretilebilmelidir.
6. Meaningful alignment result GitHub evidence'a geri izlenebilmelidir.
7. Double traceability korunmalıdır.
8. Public evidence not found skill absence'a dönüşmemelidir.
9. Limited evidence weak skill'e dönüşmemelidir.
10. Aggregate candidate-fit score üretilmemelidir.
11. Hire/no-hire sonucu üretilmemelidir.
12. Candidate ranking üretilmemelidir.
13. Seniority assignment üretilmemelidir.
14. Why-this-conclusion drill-down desteklenmelidir.
15. Job-relevant repositories evidence ile açıklanabilmelidir.
16. Evidence visibility guidance skill-gap judgment'a dönüşmemelidir.
17. Candidate ve recruiter aynı structured alignment truth'u tüketmelidir.
18. LLM structured canonical truth'u override edememelidir.
19. Semantic alignment deterministic/versionable olabilmelidir.
20. Prohibited outputs validation setinde görünmemelidir.

Exact validation fixtures daha sonraki validation contract/workstream'lerine
aittir.

---

## 55. Product Trust Model

Job Evidence Alignment'ın temel güven modeli:

    Job alignment claim
            ↓
    normalized job requirement
            ↓
    source job statement / source span

ve:

    Job alignment claim
            ↓
    structured public profile evidence
            ↓
    contracted pattern / evidence support
            ↓
    quantified observations
            ↓
    repository references
            ↓
    repository-level evidence

GitHub Proof'un farklılaştırıcı değeri:

    keyword matching

değil:

    evidence translation
    +
    explainable job evidence alignment

olmalıdır.

---

## 56. Temel Ürün İlkesi

Job Evidence Alignment'ın temel ürünü bir puan değildir.

Temel ürün:

> Belirli bir iş ilanının teknik beklentilerini, public GitHub üretimindeki
> yapılandırılmış ve geri izlenebilir kanıtlarla açıklanabilir biçimde
> karşılaştırmaktır.

Kısa tanım:

> İlanı yapıştır. GitHub'ının bu iş için ne kanıtladığını gör.

---

## 57. Contract Dependency Order

Job Evidence Alignment V1 için intended contract order:

    Repository MVP V1
            ↓
    Public Profile Evidence V1
            ↓
    Job Evidence Alignment Product Contract
            ↓
    Job Requirement Domain Contract
            ↓
    Job Requirement Extraction Contract
            ↓
    Job Evidence Alignment Domain Contract
            ↓
    Job Evidence Alignment Report Contract

Bu sıra semantic dependency yönünü gösterir.

Lower-level locked contract'lar top-level product contract tarafından sessizce
rewrite edilemez.

---

## 58. Public Profile Evidence P0 Değişiklik Kuralı

Bu draft Public Profile Evidence V1'i değiştirmez.

Current finding:

    hard semantic conflict:
    none identified

Current finding:

    product-positioning drift:
    identified

Job Evidence Alignment Product Contract contract-lock olduktan sonra Public
Profile Evidence V1'in yalnız top-level product wording'i açısından minimal
scope/cross-reference review yapılabilir.

Bu review:

- profile safety semantics'i zayıflatamaz,
- profile evidence meaning'ini değiştiremez,
- developer scoring yasağını değiştiremez,
- sparse GitHub safety'sini değiştiremez.

Bu draft P0 amendment authorize etmez.

---

## 59. Roadmap Değişiklik Kuralı

Bu draft canonical roadmap'i değiştirmez.

Roadmap ancak Job Evidence Alignment Product Contract lock sonrasında ayrı
bounded step ile güncellenmelidir.

Future roadmap:

    Profile Foundation Track

ve:

    Job Product Track

ilişkisini birlikte göstermelidir.

Existing Repository MVP ve Public Profile Evidence history silinmemelidir.

---

## 60. Implementation Gate

Job Evidence Alignment core production implementation başlamadan önce en az:

    Job Evidence Alignment Product Contract
    CONTRACT LOCKED

    Job Requirement Domain Contract
    CONTRACT LOCKED

    Job Requirement Extraction Contract
    CONTRACT LOCKED

    Job Evidence Alignment Domain Contract
    CONTRACT LOCKED

olmalıdır.

Candidate/recruiter report UI implementation öncesinde ayrıca:

    Job Evidence Alignment Report Contract
    CONTRACT LOCKED

olmalıdır.

Profile implementation ayrıca mevcut Profile P1 / P2 gates'ine tabidir.

Yeni product direction bu gates'i bypass etmez.

---

## 61. Draft Review Gate

Contract-lock review tamamlanmıştır.

Lock öncesinde aşağıdaki kontroller PASS olmuştur:

1. Repository MVP V1 consistency.
2. Public Profile Evidence V1 consistency.
3. Session 052 product-direction consistency.
4. Session 053 Step 2.1 responsibility consistency.
5. Session 053 Step 2.2 invariant consistency.
6. No duplicate semantic truth ownership.
7. No accidental domain-algorithm locking.
8. No candidate suitability semantics.
9. No negative mirror inference.
10. Double traceability completeness.
11. Bounded inference remains optional/non-canonical dependency.
12. No unauthorized P0 or roadmap modification.
13. No production implementation changes.

Cross-contract review:

    PASS

Blocking findings:

    NONE

Resolved non-blocking findings:

    1

Known deferred follow-up:

    Public Profile Evidence V1 top-level positioning/cross-reference review

Bu follow-up contract lock'u bloke etmez.

## 62. Draft Contract State

Current status:

    CONTRACT LOCKED

Contract version:

    1

Locked:

    2026-08-16

Product semantics:

    LOCKED

Cross-contract review:

    PASS

Blocking findings:

    NONE

Repository MVP V1:

    PRESERVED

Public Profile Evidence V1:

    PRESERVED / UNCHANGED

Canonical roadmap:

    UNCHANGED

Downstream Job contracts:

    NOT CREATED

Production code:

    UNCHANGED
