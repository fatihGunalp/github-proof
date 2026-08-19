# GitHub Proof — Public Profile Evidence Product Contract

**Status:** CONTRACT LOCKED
**Contract Scope:** Public Profile Evidence V1
**Contract Version:** 2
**Repository Dependency:** Repository MVP V1 preserved
**Locked:** 2026-08-16
**Amendment History:**
- 2026-08-16 — positioning / cross-reference only
- 2026-08-19 / Session 077 — RoleEvidenceAlignment requirement/deferral
  alignment only (§14, §24): clarifies that RoleEvidenceAlignment is
  DEFERRED for current Profile V1 / Milestone B under the Session 076
  decision and that the report is complete/valid without it; no product
  semantics, evidence rules or contract version changed
- 2026-08-19 / Session 079 — P8-1 contract-ownership cross-reference only
  (§24, §27): records that the typed profile report DTO shape, report
  section availability semantics and canonical report ordering are now
  locked by
  `docs/architecture/public-profile-evidence-report-contract.md`. No product
  requirement was added, removed, weakened or reinterpreted; no evidence
  rule and no contract version changed
- 2026-08-19 / Session 080 — §24: locks the previously open product decision
  that the default Milestone B user-facing report does not render the
  `Problem Area Patterns` section while its availability is `Unsupported`,
  while the structured/typed report truth still carries
  `Availability = Unsupported`. `RoleEvidenceAlignment` remains separately
  `Deferred`; the two states may not be collapsed. Empty results may still
  never be presented as "problem area bulunamadı" or any negative developer
  conclusion. No evidence rule and no contract version changed


## 1. Ürün Tanımı

Public Profile Evidence, GitHub Proof'un mevcut repository-level analiz
yeteneğinin üzerinde çalışan ayrı bir ürün katmanıdır.

Amaç, bir GitHub kullanıcısının public GitHub yüzeyinde tekrar eden teknik
alanları, problem alanlarını ve mühendislik pratiklerini açıklanabilir
kanıtlarla görünür hâle getirmektir.

GitHub Proof'un daha geniş ürün yapısı içinde Public Profile Evidence V1:

    reusable, job-agnostic structured evidence foundation

olarak konumlanır.

Bu structured profile truth daha üst seviye consumer'lar tarafından
kullanılabilir.

Contract-locked mevcut consumer:

    Job Evidence Alignment V1

Canonical downstream contract:

    docs/product/job-evidence-alignment-product-contract.md

Bu downstream ilişki JobEvidenceAlignment'ın
DeveloperPublicEvidenceProfile içine taşınması anlamına gelmez.

DeveloperPublicEvidenceProfile job-agnostic kalır.

Temel akış:

~~~text
GitHub username / public profile
        ↓
Public profile snapshot
        ↓
Eligible public repositories
        ↓
RepositoryAnalysis[]
        ↓
Cross-repository evidence
        ↓
Developer Public Evidence Profile
~~~

Bu katman geliştiricinin gerçek dünyadaki bütün yetkinliğini modellemez.

Sistem yalnızca tanımlanmış public GitHub evidence surface'i hakkında konuşur.

---

## 2. Mevcut Repository Analiziyle İlişki

Mevcut repository analyzer korunur.

Repository-level ürün şu soruyu cevaplamaya devam eder:

> Bu public repository dışarıdan gözlemlenebilir olarak neyi kanıtlıyor?

Profile-level ürün ise şu soruyu cevaplar:

> Bu public GitHub profilindeki eligible repository'ler birlikte
> değerlendirildiğinde hangi teknik üretim örüntüleri tekrar eden public
> kanıtlarla görünür hâle geliyor?

Bu iki analiz birimi birbirinin yerine geçmez.

~~~text
RepositoryAnalysis
=
repository-level analysis truth

DeveloperPublicEvidenceProfile
=
profile-level public evidence truth
~~~

Profile-level analiz:

- repository sinyallerini yeniden tanımlamaz,
- repository skorlarını yeniden hesaplamaz,
- repository analyzer kurallarını Razor veya başka bir presentation katmanında
  tekrar üretmez,
- mevcut 15 sinyal / 4 kategori sözleşmesini sessizce değiştirmez.

---

## 3. Kimlik ve Analiz Birimi Sınırı

Aşağıdaki kavramlar birbirinden kesin olarak ayrılır:

~~~text
GitHubProfileSnapshot != Developer

DeveloperPublicEvidenceProfile != Developer

DeveloperPublicEvidenceProfile != DeveloperCapabilityProfile

DeveloperPublicEvidenceProfile != Resume

DeveloperPublicEvidenceProfile != WorkExperienceProfile

DeveloperPublicEvidenceProfile != EmployabilityProfile
~~~

GitHub profile yalnızca public evidence surface'tir.

Public GitHub'da görünmeyen:

- private repository çalışmaları,
- şirket içi kod tabanları,
- NDA kapsamındaki işler,
- offline çalışmalar,
- başka platformlardaki çalışmalar,
- sözlü teknik bilgi,
- gerçek iş deneyiminin tamamı

hakkında sistem olumlu veya olumsuz varsayım yapamaz.

---

## 4. Temel Değer Önerisi

Profile-level rapor, çok sayıda repository'yi tek tek açmadan önce
kullanıcının public GitHub üretiminde tekrar eden görünür örüntüleri hızlı
biçimde anlamayı sağlamalıdır.

Rapor özellikle şu soruları cevaplamalıdır:

1. Public üretimde hangi teknik alanlar tekrar ediyor?
2. Hangi mühendislik pratikleri birden fazla repository'de görünür?
3. Hangi problem alanları tekrar eden kanıtlarla destekleniyor?
4. Bu sonuçların her biri hangi repository'lere dayanıyor?
5. İncelemeye hangi repository'lerden başlamak daha anlamlı?
6. Teknik görüşmede hangi public kanıtlar ayrıca doğrulanabilir?
7. Hangi sonuçlar public evidence yetersizliği nedeniyle üretilemedi?

---

## 5. Hedef Kullanıcılar

İlk profile-level ürün aşağıdaki kullanıcı gruplarına hizmet edebilir:

- kendi GitHub portföyünü değerlendiren geliştiriciler,
- junior veya kariyer değiştiren adaylara mentorluk yapan kişiler,
- eğitmenler,
- teknik mentorlar,
- teknik recruiter'lar,
- hiring sürecinde repository ön incelemesi yapan teknik kişiler.

Recruiter kullanımı bir karar otomasyonu değildir.

GitHub Proof:

- aday kabul etmez,
- aday reddetmez,
- adayları sıralamaz,
- işe uygunluk kararı vermez.

---

## 6. Profile Evidence Truth

Profile-level analysis truth yalnız yapılandırılmış, açıklanabilir ve
repository-level evidence'a geri izlenebilir sonuçlardan oluşur.

Zorunlu trust chain:

~~~text
Profile-level claim
        ↓
Cross-repository pattern
        ↓
Quantified observations
        ↓
Repository references
        ↓
Repository-level evidence
~~~

Bu zincirin herhangi bir halkası kurulamıyorsa ilgili claim canonical
profile truth olamaz.

---

## 7. Repository Katılım İlkesi

Bir public repository'nin profile üzerinde görünür olması, o repository'nin
bütün profile-level çıkarımlara eşit biçimde katkı sağlayacağı anlamına gelmez.

Profile contract en az şu repository durumlarını açıkça ayırmalıdır:

- original repository,
- fork,
- template,
- archived repository,
- empty repository,
- insufficient-evidence repository,
- active repository,
- historical repository.

Bu sınıflar geliştirici kalitesi değildir.

Repository eligibility yalnız:

> Bu repository hangi profile-level evidence çıkarımlarına güvenilir biçimde
> katkı sağlayabilir?

sorusunu cevaplar.

### Fork

Fork görünür profile context olabilir.

Ancak yalnızca fork olması:

- özgün üretim,
- sahiplik,
- bağımsız teknik üretim

kanıtı değildir.

Fork'lar profile repetition count'u otomatik olarak artırmamalıdır.

### Template

Template kullanımı tek başına:

- kod authorship,
- bağımsız mimari karar,
- özgün problem çözümü

kanıtı değildir.

### Archived Repository

Archived repository otomatik olarak değersiz veya geçersiz kanıt değildir.

Güvenilir historical evidence taşıyabilir.

Ancak güncel teknik odağın kanıtı olarak nasıl kullanılacağı ayrı eligibility
kurallarıyla belirlenmelidir.

### Empty / Insufficient Evidence

Empty veya yeterli public evidence üretmeyen repository:

- pozitif technical-focus claim üretmemeli,
- negatif developer judgment üretmemeli.

### Kesin Katılım Kuralları

Repository türlerinin tam katkı ve eligibility kuralları ayrı versioned domain
contract'ta tanımlanacaktır.

Product contract bu kuralları varsaymaz.

---

## 8. Cross-Repository Evidence

`CrossRepositoryEvidence`, birden fazla eligible repository üzerindeki
gözlemlerin yapılandırılmış aggregation sonucudur.

Örnek:

~~~text
Technical area:
C# / .NET

Repositories with supporting evidence:
6

Independent repositories:
6

Repositories with active-development evidence:
4

Repositories with test evidence:
3

Repositories with CI evidence:
2

Repository references:
- repo-a
- repo-b
- repo-c
- repo-d
- repo-e
- repo-f
~~~

Bu yapı:

- geliştirici puanı değildir,
- seniority ölçümü değildir,
- kalite puanı değildir.

Yalnızca profile-level claim'in dayandığı gözlemlerin özetidir.

---

## 9. Tekrar Eden Kanıt İlkesi

Profile-level çıkarımın temel farkı repetition'dır.

Tek bir repository'de gözlemlenen teknoloji veya pratik:

~~~text
repository evidence
~~~

olarak değerlidir.

Ancak bunun profile-level dominant veya repeated focus olarak sunulabilmesi
için ilgili profile-level rule'un tekrar kriterini karşılaması gerekir.

Örneğin:

~~~text
Bir README içerisinde "C#" kelimesi geçti
~~~

ile:

~~~text
Birden fazla bağımsız eligible repository içerisinde
C#/.NET kullanımına ilişkin yapılandırılmış kanıt bulundu
~~~

aynı kanıt gücünde değildir.

Exact repetition eşikleri ve contribution kuralları versioned profile domain
contract'ta tanımlanacaktır.

---

## 10. Görünür Teknik Üretim Alanları

Profile-level ürün `TechnicalFocusArea` benzeri yapılandırılmış sonuçlar
üretebilir.

Kullanıcıya görünen tercih edilen anlam:

> Public kanıtların desteklediği teknik üretim alanı

veya:

> Görünür teknik üretim alanı

Bu kavram doğrudan:

> yetkinlik

olarak adlandırılmamalıdır.

Çünkü public repository evidence:

- gerçek dünya ustalığını,
- profesyonel deneyimin tamamını,
- bağımsız problem çözme kapasitesinin tamamını

kanıtlamaz.

Technical focus claim:

- quantified observation,
- repository repetition,
- repository reference,
- underlying evidence

taşımalıdır.

---

## 11. Engineering Practice Patterns

Profile-level ürün, engineering practice evidence'ın birden fazla repository
üzerinde tekrarını gösterebilir.

Örnek alanlar:

- automated testing,
- Continuous Integration,
- documentation,
- release usage,
- repository workflow evidence.

Doğru ifade:

> Otomatik test yapısı birden fazla public repository'de görünür.

Yanlış ifade:

> Bu geliştirici test konusunda iyidir.

Practice pattern görünür davranış/evidence pattern'idir; developer capability
claim değildir.

---

## 12. Problem Area Patterns

Profile-level ürün güvenilir ve bounded kanıt bulunduğunda tekrar eden problem
ve uygulama alanlarını gösterebilir.

Örnek aday alanlar:

- backend application development,
- HTTP / REST API,
- persistence / data access,
- domain modeling,
- authentication / authorization,
- integrations,
- ML experimentation,
- containerization.

Bu liste doğrudan canonical taxonomy değildir.

Exact problem-area taxonomy ve evidence requirement'ları ayrı versioned domain
contract'ta tanımlanmalıdır.

README keyword matching tek başına problem-area truth üretmemelidir.

---

## 13. Representative Repository

`RepresentativeRepository` şu anlama gelir:

> Public profildeki önemli teknik veya mühendislik örüntülerini yoğun biçimde
> görünür hâle getiren repository.

Bu kavram şu anlamlara gelmez:

- en iyi repository,
- en kaliteli repository,
- en zor repository,
- en profesyonel repository,
- geliştiricinin favori repository'si.

Representative seçim gerekçesi kullanıcıya gösterilebilmelidir.

Örnek:

~~~text
Why representative:

Supports:
- C# / .NET
- Backend / API
- Persistence
- Automated tests
- CI

Evidence references:
...
~~~

Stars, watchers, followers ve popularity metrikleri varsayılan teknik yetkinlik
kanıtı değildir.

---

## 14. Role Evidence Alignment

Profile-level ürün bounded role definitions ile public evidence arasında
`RoleEvidenceAlignment` üretebilir.

**Session 076/077 durumu:** Profile V1 / Milestone B için `RoleEvidenceAlignment`
DEFERRED — NOT_READY_WITH_CURRENT_EVIDENCE
(`worklog/sessions/2026-08-19-076-role-evidence-alignment-feasibility.md`).
Gerekçe: mevcut canonical public evidence yüzeyi, semantic inflation
üretmeden useful, safe ve deterministic bir generic technical-role alignment
için yetersizdir. Bu erteleme kalıcı bir ret değildir; kanonik kanıt yüzeyi
yeterince güçlenirse feasibility yeniden açılabilir. Aşağıdaki örnek —
`.NET Developer` dahil — bu kavramın gelecekte hangi biçimde
sunulabileceğini gösterir; Profile V1'in şu anki, yetkilendirilmiş bir
çıktısı değildir.

Örnek presentation:

~~~text
.NET Developer

Public evidence alignment:
...

Supporting evidence:
- C#/.NET evidence across N repositories
- ASP.NET/API evidence across N repositories
- data-access evidence across N repositories
- test evidence across N repositories
- CI evidence across N repositories
~~~

Bu kavram:

~~~text
Job Fit
Candidate Score
Employability Score
Hire Recommendation
~~~

değildir.

Role alignment yalnız:

> public GitHub evidence ile tanımlanmış technical-role evidence profile'ı
> arasındaki görünür örtüşme

anlamına gelir.

Role taxonomy, thresholds ve alignment kuralları ayrı versioned contract ile
tanımlanmalıdır.

---

## 15. Profile-Level Skor Sınırı

Public Profile Evidence V1 geliştirici için toplam sayısal skor üretmez.

Özellikle aşağıdaki yaklaşım kullanılmaz:

~~~text
Repository score A
+
Repository score B
+
Repository score C
→ Developer Score
~~~

Repository-level görünür evidence score'larının:

- ortalaması,
- toplamı,
- ağırlıklı ortalaması

developer veya candidate score'a dönüştürülemez.

Profile-level rapor evidence-first olmalıdır.

---

## 16. Sparse Public Profile Davranışı

Public repository sayısının az olması veya profile-level aggregation için
yeterli kanıt bulunmaması negatif developer sonucu üretmez.

Yanlış:

~~~text
Weak developer
Inexperienced developer
Unqualified candidate
Inactive developer
~~~

Doğru bounded sonuç:

~~~text
Bu public GitHub profili profile-level aggregation için sınırlı görünür kanıt
sağlıyor.
~~~

Rapor mümkün olduğunda neden sınırlı kaldığını açıklamalıdır.

Örnek:

- eligible repository sayısı düşük,
- repository content evidence sınırlı,
- repository'lerin çoğu fork,
- gerekli data unavailable.

Bu sınırlamalar geliştirici hakkında hüküm değildir.

---

## 17. Public Profile Absence Sınırı

Bir kişinin:

- public GitHub kullanmaması,
- az public repository paylaşması,
- private repository ağırlıklı çalışması

GitHub Proof tarafından negatif candidate veya developer sinyali olarak
yorumlanamaz.

Sistem yalnızca:

> public GitHub üzerinde gözlemlenebilen evidence surface sınırlıdır

sonucunu verebilir.

---

## 18. Yasaklı Profile-Level Çıkarımlar

Hiçbir profile-level evidence birleşiminden otomatik olarak aşağıdaki sonuçlar
üretilmemelidir:

- Bu geliştirici junior seviyesindedir.
- Bu geliştirici mid seviyesindedir.
- Bu geliştirici senior seviyesindedir.
- Bu geliştirici işe alınmalıdır.
- Bu geliştirici işe alınmamalıdır.
- Bu aday diğer adaylardan daha iyidir.
- Bu geliştiricinin genel teknik seviyesi X'tir.
- Bu geliştiricinin zekâsı veya yeteneği X'tir.
- Bu geliştirici ekip çalışmasına uygundur.
- Bu geliştiricinin maaşı şu aralıkta olmalıdır.
- Bu geliştiricinin X yıl deneyimi vardır.
- Public görünmeyen private çalışmaları yoktur.
- Repository içeriğinin tamamını bu kişi yazmıştır.
- Kod özgündür.
- Kod AI tarafından yazılmıştır.
- Kod AI tarafından yazılmamıştır.
- Üretim production-ready'dir.
- Kod güvenlidir.
- Mimari doğrudur veya yanlıştır.
- Public GitHub aktivitesi düşük olduğu için geliştirici zayıftır.

---

## 19. Recruiter Kullanım Sınırı

Recruiter-facing presentation şu amaçlarda kullanılabilir:

- hızlı teknik orientation,
- public technical focus alanlarını görmek,
- tekrar eden engineering-practice evidence'ı görmek,
- önce incelenecek representative repository'leri seçmek,
- teknik görüşmede doğrulanabilecek evidence-backed alanları belirlemek.

Recruiter-facing output:

~~~text
Screening / routing assistance
~~~

olabilir.

Ancak:

~~~text
Hire / No Hire
Candidate Ranking
Automatic Rejection
Automatic Approval
Seniority Assignment
~~~

olamaz.

Zorunlu ürün sınırı:

> Role veya teknik alan eşleşmeleri yalnız public GitHub üretimindeki
> gözlemlenebilir kanıtlara dayanır; iş deneyimi, private çalışmalar, kıdem
> veya işe uygunluk kararı değildir.

---

## 20. Developer-Facing ve Recruiter-Facing Truth

Developer ve recruiter için farklı presentation üretilebilir.

Ancak iki görünüm de aynı structured profile evidence truth'u tüketmelidir.

~~~text
DeveloperPublicEvidenceProfile
              ↓
     ┌────────┴────────┐
     ↓                 ↓
Developer View    Recruiter View
~~~

Presentation farklı olabilir.

Evidence truth farklı olamaz.

Recruiter presentation ayrı bir gizli analiz motoru veya farklı profile truth
üretmemelidir.

---

## 21. LLM Sınırı

LLM veya başka bir generative model raw GitHub verisini okuyarak doğrudan
canonical profile truth üretemez.

Zorunlu yön:

~~~text
Raw GitHub data
        ↓
Normalized observations
        ↓
Repository-level analysis
        ↓
Contracted cross-repository aggregation
        ↓
Structured profile truth
        ↓
Optional narration
~~~

LLM gelecekte:

- structured evidence'ı özetleyebilir,
- kullanıcıya anlaşılır prose üretebilir,
- evidence-backed interview question wording'i oluşturabilir.

LLM:

- evidence icat edemez,
- repository count değiştiremez,
- unsupported technical focus yaratamaz,
- unsupported problem area yaratamaz,
- unsupported role alignment yaratamaz,
- seniority üretemez,
- employability kararı veremez,
- deterministic truth'u override edemez.

Temel ilke:

> LLM anlatabilir. Kanıt motoru profile truth'un hammaddesini üretir.

---

## 22. Determinism ve Versioning

Profile-level analysis tekrar üretilebilir olmalıdır.

Aynı:

- GitHubProfileSnapshot,
- eligible repository set,
- RepositoryAnalysis sonuçları,
- profile rule versions

kullanıldığında aynı structured profile truth üretilmelidir.

Profile-level semantic rule değişiklikleri versioned olmalıdır.

Presentation veya localization değişikliği profile analysis'i yeniden
çalıştırmayı gerektirmemelidir.

---

## 23. Evidence Literal ve Localization Sınırı

Repository'den veya GitHub API'den alınan literal evidence:

- repository adı,
- owner,
- path,
- language adı,
- framework/package adı,
- workflow adı,
- evidence value

localization sırasında değiştirilmemelidir.

Profile-level semantic labels ve açıklamalar presentation/localization
katmanında üretilebilir.

Locale değişimi:

- repository collection,
- repository evaluation,
- cross-repository aggregation,
- role alignment,
- technical-focus analysis

işlemlerini yeniden çalıştırmamalıdır.

---

## 24. İlk Profile-Level Raporun Zorunlu Bölümleri

Bu bölüm ürün seviyesinde zorunlu rapor yüzeyini tanımlar. Typed report DTO
sözleşmesi, bölüm availability semantiği (`Supported` / `Unsupported` /
`Deferred`) ve canonical bölüm sıralaması Session 079'da ayrı bir canonical
architecture contract'ında kilitlenmiştir:
`docs/architecture/public-profile-evidence-report-contract.md`. Aşağıdaki
ürün gereksinimleri değişmemiştir; o contract bunları typed projection
seviyesinde kesinleştirir.

İki not:

1. Aşağıdaki başlık listesi ürün yüzeyini sayar, canonical sıralamayı
   belirlemez. Locale-independent canonical bölüm sıralaması report contract
   §7'de kilitlidir ve `Role Evidence Alignment`'ı conditional extension
   olarak en sona koyar.
2. `Problem Area Patterns` V1'de `Unsupported`'tır
   (`docs/architecture/public-profile-evidence-patterns.md` §9). **Ürün
   kararı (Session 080):** varsayılan Milestone B kullanıcıya dönük raporda
   bu bölüm, availability `Unsupported` olduğu sürece **render edilmez.**

   Bu yalnız bir rendering kararıdır. Structured/typed report truth
   `Availability = Unsupported`, `Items = []` değerini taşımaya devam eder ve
   bir consumer bunu görebilir; ürün sürümünün bu yeteneği üretmediği bilgisi
   kaybolmaz (bkz. report contract §6.5).

   Bölüm render edilse de edilmese de boş sonuç `problem area bulunamadı`,
   yanıltıcı boş kart veya herhangi bir negatif developer sonucu olarak
   sunulamaz.

   `RoleEvidenceAlignment` ayrı biçimde `Deferred` kalır; `Unsupported` ve
   `Deferred` tek bir duruma indirgenemez.

İlk profile-level report contract en az şu alanları desteklemelidir:

### Public Profile Summary

Kısa evidence-backed orientation.

### Görünür Teknik Üretim Alanları

Tekrar eden technical-focus evidence.

### Engineering Practice Patterns

Birden fazla repository'de görünür engineering practices.

### Problem Area Patterns

Yalnız bounded evidence ile desteklenen alanlar.

### Representative Repositories

İncelemeye başlanabilecek evidence-rich repository'ler ve seçim nedenleri.

### Role Evidence Alignment

Yalnız public evidence overlap. Bu bölüm conditional'dır: yalnız
`RoleEvidenceAlignment` fiilen üretildiğinde gösterilir. Profile V1 /
Milestone B kapsamında Session 076/077 kararıyla ertelendiği için bu bölüm
şu an boştur veya gösterilmez; raporun geri kalanı bu bölüm olmadan da
eksiksiz ve geçerli kabul edilir (bkz. §14).

### Why This Conclusion?

Her önemli claim için quantified observations ve repository references.

### Limitations

Sparse profile, unavailable data, eligibility limitations ve diğer uncertainty
durumları.

### Repository Drill-Down

Profile claim'den ilgili repository analysis/evidence'a geçiş.

---

## 25. İlk Okuma Hedefi

Profile-level raporun ilk katmanı bir okuyucunun kısa sürede şu soruları
cevaplayabilmesini hedeflemelidir:

1. Public üretimde baskın görünen teknik alanlar neler?
2. Hangi engineering practices tekrar ediyor?
3. Hangi repository'lere önce bakmalıyım?
4. Sistem neden bu sonuçlara ulaştı?
5. Hangi conclusions public evidence sınırı nedeniyle yapılamaz?

Bu hedef UI implementation detayı değildir.

Report information architecture için ürün hedefidir.

---

## 26. Repository MVP V1 ile Uyumluluk

Mevcut repository MVP V1 geçerliliğini korur.

Bu yeni capability:

~~~text
Repository MVP V1 replacement
~~~

değildir.

Doğru ilişki:

~~~text
Repository Evidence
        ↓
atomic evidence layer

Public Profile Evidence
        ↓
macro aggregation layer
~~~

Mevcut repository analyzer bağımsız olarak çalışmaya devam edebilir.

Profile analysis onun üzerine ek capability olarak inşa edilir.

---

## 27. Bu Contract'ın Bilerek Kilitlemediği Konular

Aşağıdaki konular product contract seviyesinde kesinleştirilmez:

- exact repository eligibility algorithm,
- exact active/historical repository thresholds,
- fork contribution weighting,
- template contribution weighting,
- technical-focus taxonomy,
- problem-area taxonomy,
- repetition thresholds,
- confidence calculation,
- representative-repository ranking algorithm,
- role taxonomy,
- role-alignment thresholds,
- GitHub profile collection API shape,
- pagination bounds,
- persistence,
- authentication,
- billing.

Bunlar sonraki domain/architecture contracts tarafından belirlenmelidir.

`profile report DTO shape` bu listede yer alıyordu ve Session 079'da
beklendiği gibi ayrı bir canonical architecture contract'ı tarafından
belirlenmiştir:
`docs/architecture/public-profile-evidence-report-contract.md`. Bu, product
contract seviyesinde yeni bir ürün kararı kilitlemez; yalnız o kararın artık
hangi canonical owner'a ait olduğunu kaydeder.

Bu contract gelecekteki implementasyonu yönlendirir ancak henüz seçilmemiş
algoritmaları uydurmaz.

---

## 28. V1 Başarı Kriteri

Public Profile Evidence V1 başarılı sayılmak için:

1. GitHub username veya canonical public profile identity kabul edilebilmelidir.
2. Tanımlanmış public repository seti güvenilir biçimde toplanabilmelidir.
3. Repository eligibility açık ve versioned kurallarla belirlenebilmelidir.
4. Eligible repository'ler mevcut RepositoryAnalysis pipeline'ıyla
   analiz edilebilmelidir.
5. Cross-repository claims deterministic olmalıdır.
6. Her meaningful claim repository-level evidence'a geri izlenebilmelidir.
7. Sparse public profile negatif developer judgment üretmemelidir.
8. Profile-level developer score üretilmemelidir.
9. Role alignment hire/no-hire sonucuna dönüşmemelidir.
10. Developer ve recruiter presentation aynı structured evidence truth'u
    kullanmalıdır.
11. Yasaklı çıkarımlar validation setinde üretilmemelidir.
12. Gerçek public profiller üzerinde explanation ve traceability doğrulaması
    yapılmalıdır.

---

## 29. Temel Ürün İlkesi

Public Profile Evidence'ın temel ürünü bir puan değildir.

Temel ürün:

> Dağınık public GitHub üretimini, sınırları açık ve repository evidence'a geri
> izlenebilir teknik kanıta çevirmektir.

Kısa ürün tanımı:

> Public Profile Evidence, public GitHub üretiminde hangi teknik alanların,
> problem türlerinin ve mühendislik pratiklerinin tekrar eden kanıtlarla
> görünür olduğunu açıklayan bir evidence translation layer'dır.

GitHub Proof'un daha geniş ürün yapısında bu katman job-agnostic structured
profile evidence üretir; Job Evidence Alignment gibi higher-level consumer'lar
bu truth'u downstream olarak tüketebilir.

Repository report micro view'dur.

Public Profile Evidence macro view'dur.


---

## 30. Contract Lock

Bu doküman Public Profile Evidence V1 için product-level source of truth olarak
kilitlenmiştir.

### Kilitlenen Ürün Kararları

Aşağıdaki kararlar artık implementation sırasında sessizce değiştirilemez:

1. Public Profile Evidence, Repository MVP V1'in replacement'ı değildir.
2. `RepositoryAnalysis` repository-level atomic evidence truth olarak korunur.
3. Profile-level analysis ayrı bir structured truth üretir.
4. `GitHubProfileSnapshot` geliştiricinin kendisi değildir.
5. `DeveloperPublicEvidenceProfile` developer capability profili değildir.
6. Profile-level meaningful claim'ler repository evidence'a geri izlenebilir
   olmalıdır.
7. Repository participation / eligibility explicit kurallarla belirlenmelidir.
8. Cross-repository repetition profile-level çıkarımın temel kanıt
   mekanizmalarından biridir.
9. Public Profile Evidence V1 developer/candidate toplam skoru üretmez.
10. Repository skorları developer skoruna aggregate edilemez.
11. Sparse veya absent public GitHub negatif developer judgment üretemez.
12. Role Evidence Alignment yalnız public evidence overlap anlamına gelir.
13. Role Evidence Alignment hire/no-hire veya job-fit kararı değildir.
14. Developer-facing ve recruiter-facing presentation aynı structured profile
    truth'u tüketir.
15. LLM canonical profile truth oluşturamaz; yalnız structured truth sonrasında
    bounded narration yapabilir.
16. Seniority, employability, candidate ranking ve diğer yasaklı çıkarımlar
    hiçbir evidence birleşiminden otomatik üretilmez.
17. Profile-level semantic kurallar deterministic ve versioned olmalıdır.
18. Locale değişikliği profile analysis truth'u yeniden üretmemelidir.

### Bilinçli Olarak Kilitlenmeyen Kararlar

Aşağıdaki alanlar bu product contract tarafından belirlenmemiştir:

~~~text
repository eligibility algorithm
active / historical thresholds
fork contribution rules
template contribution rules
technical-focus taxonomy
problem-area taxonomy
repetition thresholds
confidence calculation
representative-repository selection algorithm
role taxonomy
role-alignment thresholds
profile collection API details
pagination bounds
profile domain DTO shape
persistence
authentication
billing
~~~

Bu kararlar ürün contract'ından türetilerek sonraki domain / architecture
contract'larında ayrı ayrı kilitlenmelidir.

### Değişiklik Kuralı

Bu contract'taki kilitli semantic bir karar gelecekte değiştirilecekse:

~~~text
explicit product decision
→ contract update
→ affected domain/architecture review
→ version impact review
→ implementation
→ validation
~~~

sırası izlenmelidir.

Production code veya UI, bu sözleşmenin semantic sınırlarını sessizce
genişletemez.

### Terminoloji

Bu proje içerisinde scope önemli olduğunda aşağıdaki tam ifadeler
kullanılmalıdır:

~~~text
Repository MVP V1
Public Profile Evidence V1
~~~

Tek başına `MVP V1` veya `V1` ifadesi iki capability arasında belirsizlik
oluşturacaksa kullanılmamalıdır.
