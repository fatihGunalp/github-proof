# Profile Proof — Distinctive Outcomes Contract

**Status:** CONTRACT LOCKED
**Contract Version:** 1
**Contract Scope:** Profile Proof distinctive outcome semantics and first-read
information priority
**Session:** 135
**Locked:** 2026-08-27
**Production Code:** NOT IN SCOPE

---

## 1. Purpose

Bu doküman tek bir ürün sorusunun canonical sahibidir:

> Bir kişinin Profile Proof sonucunda gördüğü hangi sonuçlar, GitHub'ın kendi
> arayüzünden ya da generic bir AI modeline "bu GitHub profilini incele"
> demekten yapısal olarak daha değerli olmak zorundadır?

Cevap outcome seviyesinde kilitlenir: hangi sonuçların ürün tarafından **var
olması gerektiği**, her birinin hangi yapısal farkı taşıdığı ve kullanıcının o
farkı ilk okumada nasıl fark edebileceği.

Bu contract daha sonra yapılacak UI/UX çalışmasının outcome-level owner'ıdır.

---

## 2. Canonical Boundary

Bu doküman **hiçbir** evidence rule, threshold, taxonomy, promotion kuralı,
eligibility kuralı, report DTO şekli veya user-facing copy tanımlamaz. Bunların
tamamı mevcut canonical owner'lara aittir ve burada tekrar edilmez:

| Alan | Canonical sahip |
|---|---|
| Ürün konumlandırması ve epistemik sınır | `docs/product/product-positioning.md` |
| Profile-level ürün semantiği ve yasaklı çıkarımlar | `docs/product/public-profile-evidence-product-contract.md` |
| Genel Profile first-read hedefi (raporun ilk katmanının cevaplaması beklenen sorular) | `docs/product/public-profile-evidence-product-contract.md` §25 |
| Profile domain modeli, limitation/warning/AnalysisStatus | `docs/architecture/public-profile-evidence-domain-model.md` |
| Repository admission ve eligibility | `docs/architecture/public-profile-evidence-repository-eligibility.md` |
| Repository collection ve completeness semantiği | `docs/architecture/public-profile-runtime-collection.md` |
| Pattern taxonomy, repetition threshold, promotion | `docs/architecture/public-profile-evidence-patterns.md` |
| Representative repository seçimi | `docs/architecture/public-profile-representative-repositories.md` |
| Typed report bölümleri, availability, sıralama | `docs/architecture/public-profile-evidence-report-contract.md` |
| Profile web presentation şekli, dil ve copy | `docs/product/p10-presentation-language-contract.md` |

Bu contract yalnız şunu ekler: bu owner'ların ürettiği yeteneklerden
**hangilerinin distinctive outcome sayıldığı**, her birinin yapısal farkının ne
olduğu ve ilk okumada hangi bilgi önceliğinin zorunlu olduğu.

Çelişki hâlinde yukarıdaki owner kazanır. Bu doküman bir sonucu distinctive
sayabilir; onun semantiğini genişletemez.

DistinctiveOutcome bir yetenek sınıflandırmasıdır. Kişi hakkında bir hüküm
değildir; `product-positioning.md` §4-§6 ve
`public-profile-evidence-product-contract.md` §18'in çizdiği sınırı hiçbir
biçimde gevşetmez.

---

## 3. Differentiation Problem

Bir ziyaretçi GitHub'da zaten repository'leri, commit'leri, dilleri, README'leri,
workflow'ları, issue'ları, pull request'leri ve release'leri görebilir. Aynı
ziyaretçi generic bir AI modeline profili inceletip "CI/CD kullanıyor", "test
yazıyor", "backend teknolojileri var", "şu repo'lara bakılabilir" cevaplarını
makul biçimde alabilir.

Dolayısıyla şunlar tek başına distinctive değildir:

- daha iyi düzenlenmiş dashboard,
- doğal dil özet üretimi,
- teknoloji listesi,
- aktivite listesi,
- skor.

Distinctive fark yalnız şu zincirin yapısal olarak gösterilebilmesindedir:

~~~text
observable public GitHub traces
        ↓
declared collection surface
        ↓
deterministic evidence rules
        ↓
cross-repository recurrence
        ↓
bounded conclusion
        ↓
traceability
        ↓
concrete repository evidence
~~~

Ürün hipotezi:

> Profile Proof, veriyi özetlemekten çok, birbirinden kopuk public GitHub
> izlerinin **birlikte** neyi tekrar ettirdiğini tekrar üretilebilir ve
> izlenebilir biçimde ortaya çıkarır.

---

## 4. Distinctive Outcome Acceptance Test

Bir sonuç ancak aşağıdaki özellikleri somut biçimde taşıdığında distinctive
outcome sayılır.

| Kod | Kriter | Anlamı |
|---|---|---|
| A | Cross-repository | Sonuç tek repository yorumu değildir; birden fazla bağımsız repository'deki kanıt birlikte anlam kurar |
| B | Deterministic | Aynı observable input + aynı ruleset aynı sonucu üretir; prompt wording sonucu değiştiremez |
| C | Traceable | Sonuç, conclusion → recurring pattern → supporting repositories → repository → concrete evidence zinciri boyunca izlenebilir |
| D | Reproducible | Aynı public evidence surface yeniden analiz edildiğinde semantic sonuç yeniden üretilir |
| E | Coverage-declared | Sistem hangi repository'leri ve hangi evidence yüzeyini değerlendirdiğini bilir ve bunu **declared** biçimde taşır |
| F | Evidence-bounded | Observable evidence'ten insan hükmüne sıçramaz |
| G | Abstention-capable | Kanıt yetmediğinde sonuç üretmemeyi seçebilir ve bunu kişi hakkında olumsuz sonuca çevirmez |
| H | Starting-point value | Dağınık profilde anlamlı bir başlangıç noktası verebilir, ranking üretmeden |

**E hakkında zorunlu daralt:** "tam coverage" ifadesi kullanılamaz. Canonical
repository manifest'i `public-profile-runtime-collection.md` §7 uyarınca gizli
bir top-N kısıtı taşımaz, ancak declared bir collection bound veya bir failure
manifest'i incomplete yapabilir. Distinctive olan **eksiksizlik iddiası
değil**, değerlendirilen yüzeyin declared olması ve daralmasının görünür hâle
gelmesidir.

---

## 5. Distinctive Outcomes

Kabul edilen set tam olarak dört outcome'dur.

### 5.1 Priority Etiketleri

Her outcome iki priority etiketinden birini taşır:

~~~text
CORE        Bu outcome olmadan GitHub Proof'un GitHub + generic AI
            karşısındaki varlık nedeni maddi olarak zayıflar.

SUPPORTING  Distinctive değeri güçlendirir; tek başına core moat değildir.
~~~

Numeric `P<n>` notasyonu priority için **kullanılmaz**. Repository genelinde
`P<n>` yerleşik olarak Profile **phase** kimliğidir — örneğin patterns, report
ve representative contract'larındaki "P1 result ownership, P2 eligibility
semantics, P5 observation semantics" ifadeleri. Priority için aynı notasyonu
kullanmak iki farklı anlamı yan yana getirirdi.

### 5.2 Örneklerin Statüsü

Aşağıdaki outcome'ların `EXAMPLE` blokları **illustrative semantic example**'dır;
**locked user-facing copy değildir.** Gerçek kullanıcı metninin sahibi
`p10-presentation-language-contract.md`'dir ve bu contract onu değiştirmez.

---

## Outcome 1 — Cross-Repository Recurring Evidence

**PRIORITY**

CORE

**USER QUESTION**

"Bu profilde tek bir projeye özgü olmayan, tekrar eden ne var?"

**USER-VISIBLE RESULT**

Kullanıcı, hangi görünür teknik yönün ve hangi çalışma biçimi izinin birden
fazla bağımsız public repository'de yeniden göründüğünü öğrenir. Tek bir
repository'deki gözlem profile-level sonuç sayılmaz.

**EXAMPLE**

> C# farklı repository'lerde yeniden görülüyor.
>
> Otomatik test kullanımı farklı repository'lerde tekrar görülüyor.

**CURRENT CANONICAL BASIS**

`public-profile-evidence-patterns.md` §4 (pattern aileleri), §5 (repetition
threshold), §7 (EngineeringPractice anahtarları ve signal mapping), §8
(TechnicalFocus dominant language kuralı);
`public-profile-evidence-product-contract.md` §9-§11;
`public-profile-evidence-domain-model.md` §10;
`p10-presentation-language-contract.md` §8.4.

**REQUIRED OBSERVABLE EVIDENCE**

Repository-level canonical signal gözlemleri (`validation.test-structure`,
`validation.continuous-integration`, `documentation.installation-guide`,
`documentation.usage-or-demo`, `process.release-usage`, `process.issue-usage`,
`process.pull-request-usage`) ve `RepositoryAnalysis.Snapshot.Languages` byte
sayıları. Mapping'in sahibi patterns §7.2/§8.1'dir.

**CROSS-REPOSITORY RULE**

Tek repository asla yeterli değildir. Existing contract decides recurrence; no
new numeric threshold is selected here — threshold
`public-profile-evidence-patterns.md` §5'in sahipliğindedir ve yalnız
`IndependentRepetition` katkısı `Allowed` olan repository'ler sayılır. Fork ve
template-derived lineage repetition'ı şişiremez; `Undetermined` katkı ne destek
ne de olumsuz kanıttır.

**DETERMINISM RULE**

Aynı `CrossRepositoryEvidence` + aynı `ObservationRuleVersion` +
`EligibilityPolicyVersion` + `PatternRuleVersion` altında promoted pattern seti
semantik olarak aynıdır (patterns §12). Enumeration order, execution order,
sistem saati ve locale sonucu değiştiremez.

**TRACEABILITY RULE**

Her promoted pattern patterns §10 zincirini destekler: pattern →
`ObservedProfileEvidence[]` → `ProfileEvidenceReference[]` →
`RepositoryAnalysisReference[]` → repository-level evidence.

**COVERAGE RULE**

Yalnız canonical repository manifest'inden admission ve eligibility geçmiş
repository'ler. "All of GitHub", "bütün repository'ler" veya "complete GitHub
history" iddiası kullanılamaz.

**ABSTENTION RULE**

Threshold karşılanmıyorsa pattern üretilmez ve boş sonuç bir iddia değildir
(patterns §6). Boş pattern koleksiyonu teknolojinin veya pratiğin yokluğu
anlamına gelmez.

**GENERIC AI CAN**

Profile'a bakıp "bu kişi C# kullanıyor ve test yazıyor" cümlesini kurabilir;
hangi repository'lere baktığını gevşek biçimde sayabilir.

**GITHUB PROOF STRUCTURALLY ADDS**

Cümleyi değil, **cümlenin üretilme koşulunu** ekler: bir gözlem ancak versioned
bir promotion kuralının gerektirdiği sayıda bağımsız destekleyici repository
bulunduğunda ve fork/template lineage sayımdan dışlandığında profile-level
sonuca yükselir; o sayı patterns §5'in sahipliğindedir ve burada tekrar
edilmez. Yükselmeyen gözlem profile-level sonuç olarak hiç görünmez.

**USER-VISIBLE DIFFERENCE**

Kullanıcının gördüğü her profile-level ifade bir **tekrar** ifadesidir ("farklı
repository'lerde yeniden görülüyor"), tek seferlik bir gözlem değil; ve
tekrarın hangi repository'lerde gerçekleştiği evidence katmanında adıyla
listelenir.

**WHY USER CARES**

Tek bir repository'deki iz tesadüf, şablon veya tek seferlik deneme olabilir.
Kullanıcının kendi profilinde bile göremediği şey, aynı iz kalıbının farklı
projelerinde yeniden ortaya çıkmasıdır. Bu, "GitHub'ımda şunlar var"dan
"GitHub'ımdaki şeyler birlikte şunu tekrar ettiriyor"a geçiştir.

**NON-CLAIMS**

Tekrar eden iz; yetkinlik, ustalık, kıdem, deneyim süresi, authorship,
originality veya repository kalitesi anlamına gelmez.

**UI CONSEQUENCE**

Sonraki UI/UX çalışması şu semantic bilgileri önceliklendirmelidir: pattern'in
canonical kimliği, tekrarın kendisi (sayı olarak değil, ilişki olarak) ve
pattern ailesi ayrımı. Tekrar ilişkisi cümlenin yan bilgisi değil, çekirdeği
olmalıdır.

---

## Outcome 2 — Traceable Conclusion-to-Repository Evidence Chain

**PRIORITY**

CORE

**USER QUESTION**

"Bunu neden söylüyorsun?"

**USER-VISIBLE RESULT**

Kullanıcı **promote edilmiş tekrar eden pattern sonuçlarının her biri** için —
yani `TechnicalFocus` ve `EngineeringPractice` conclusion'ları için — (a)
sonucun neden orada olduğunun bounded açıklamasını ve (b) o sonucu destekleyen
public repository'lerin literal kimliklerini görür; oradan ilgili repository
üzerinde **yeni bir (fresh) Repository Proof analizi** çalıştırarak signal ve
concrete evidence seviyesine geçebilir.

Bu trace iddiası `ProfileSummaryFact`, `ProfileLimitation`, `ProfileWarning` ve
destekleyici repository sahipliği olmayan diğer profile-level yüzeyleri
**kapsamaz**; bu contract onlar için bir traceability yükümlülüğü yaratmaz.

**EXAMPLE**

> "Otomatik test kullanımı neden burada?" → tekrar eden bir çalışma biçimi
> olarak gösteriliyor → destekleyen repository'ler: `owner/repo-a`,
> `owner/repo-b`, `owner/repo-c` → o repository'nin Repository Proof analizi.

**CURRENT CANONICAL BASIS**

`public-profile-evidence-report-contract.md` §11 (`WhyThisConclusion` evidence
trace) ve §12 (`RepositoryDrillDown` navigation references);
`public-profile-evidence-patterns.md` §10;
`public-profile-evidence-product-contract.md` §6 (zorunlu trust chain);
`p10-presentation-language-contract.md` §8.6.3-§8.6.5.

**REQUIRED OBSERVABLE EVIDENCE**

Pattern'in kendi supporting repository seti ve her repository için canonical
`RepositoryAnalysisReference`. Repository Proof tarafındaki signal/evidence
truth kendi canonical owner'ında kalır ve profile tarafında ikinci bir truth
store olarak kopyalanmaz.

**CROSS-REPOSITORY RULE**

Trace'in başlangıcı Outcome 1'in cross-repository sonucudur; dolayısıyla
Outcome 1'in recurrence kuralı burada da geçerlidir. Bu outcome kendi başına
yeni bir recurrence kuralı tanımlamaz.

**DETERMINISM RULE**

Aynı `DeveloperPublicEvidenceProfile` ve aynı report contract sürümü altında
projeksiyon semantik olarak aynıdır; bölüm seti, availability durumları ve
sıralama sabittir (report contract §17). Locale yalnız metni değiştirir.

**TRACEABILITY RULE**

Zincir tam olarak şudur ve kısaltılamaz:

~~~text
Profile conclusion
        ↓
Repeated pattern
        ↓
Supporting repositories
        ↓
Repository analysis
        ↓
Concrete evidence
~~~

Trace **iki aşamalıdır** ve bu ayrım korunur: `WhyThisConclusion` destekleyen
repository kimliklerini adlandırır; repository başına tek Repository Proof
geçişi `RepositoryDrillDown`'a aittir
(`p10-presentation-language-contract.md` §8.6.3-§8.6.5). Bu contract o iki
aşamalı yapıyı tek adıma indirmez.

**Son hop'un niteliği:** teslim edilen Web akışında zincirin son adımı, o
repository üzerinde çalıştırılan **fresh** bir Repository Proof analizidir
(`p10-presentation-language-contract.md` §8.6.5: "a fresh existing Repository
Proof run"). Profile sonucunu üreten analiz instance'ının dondurulmuş bir
kopyası değildir; GitHub durumu değiştiyse fresh run farklı sonuç verebilir.
Bu contract immutable historical replay iddia etmez ve böyle bir yetenek
gerektirmez.

**COVERAGE RULE**

`RepositoryDrillDown` canonical repository universe'ünün tamamını taşır: her
eligibility sonucu, kanıta katkı vermeyenler dâhil, kendi status'üyle görünür.
Trace hedefleri bu declared kümenin dışına çıkamaz.

**ABSTENTION RULE**

Promoted pattern yoksa `WhyThisConclusion` `Supported` + boş kalır; bu bir
analiz sonucudur ve olumsuz bir bulgu gibi sunulamaz (report contract §6.1,
§6.4).

**GENERIC AI CAN**

"Şu repository'lerde test var" diyebilir, hatta repository adları sayabilir.
İkna edici bir gerekçe cümlesi de üretebilir.

**GITHUB PROOF STRUCTURALLY ADDS**

Gerekçenin **kanıtla aynı üretim yolundan gelmesini** ekler. Gösterilen
destekleyici repository seti, sonucu üreten pattern'in kendi seti olduğu için
sonradan yeniden anlatılmış bir gerekçe değil, sonucun üretim girdisidir; ve
zincirin son halkası profile tarafında yeniden yorumlanmayan, repository-level
canonical analiz truth'udur.

**USER-VISIBLE DIFFERENCE**

Kullanıcı gerekçeyi okumakla kalmaz; aynı ekrandan tek tek repository'ye ve
oradan o repository'nin kendi signal/evidence analizine inebilir. Anlatı ile
kanıt arasında kullanıcının kapatması gereken bir boşluk kalmaz.

**WHY USER CARES**

"Neden böyle diyorsun?" sorusunun cevabı bir modelin ikna kabiliyetine değil,
aynı gözleme dayanır. Kullanıcı sonucu kabul etmek zorunda değildir; kontrol
edebilir.

**NON-CLAIMS**

İzlenebilirlik doğruluk garantisi değildir ve gösterilen kanıt zincirinin
kişinin yetkinliği, authorship'i veya deneyimi hakkında hüküm verdiği anlamına
gelmez.

**UI CONSEQUENCE**

Sonraki UI/UX çalışması şunları önceliklendirmelidir: **promote edilmiş tekrar
eden pattern sonuçları** için "neden" yolunun **var olduğunun** görünürlüğü,
destekleyen repository kimliklerinin literal korunması ve repository-level
analize geçişin tek ve net olması. Analyzer iç kelimeleri (evidence key,
observation key, rule version, sayım) kullanıcı yüzeyine taşınmaz.

Limitation ve warning yüzeyleri için "neden" yolu inşa edilmez; onlar bu
trace'in kapsamı dışındadır ve bir bulguya dönüştürülemez.

---

## Outcome 3 — Declared Evidence Surface and Its Boundary

**PRIORITY**

CORE

**USER QUESTION**

"Bu profile bakarak ne söylenebiliyor, neye bakıldı ve nerede durmak
gerekiyor?"

**USER-VISIBLE RESULT**

Kullanıcı üç ayrı şeyi ayırt edebilir: (a) hangi repository'lerin
değerlendirildiği ve her birinin hangi durumda olduğu, (b) sonucun ne kadar
ileri yorumlanabileceğini sınırlayan evidence-surface koşulları, (c) bir
bölümün boş olmasının bir eksiklik değil, mevcut kurallar altında bir gözlem
sonucu olduğu.

**EXAMPLE**

> Değerlendirilen repository'ler kendi durumlarıyla listelenir; bir kısmı
> kanıta katkı vermemiş olabilir ve yine de listede görünür.
>
> Bu profilde tekrar için kullanılabilen bağımsız kanıt yüzeyi dar olduğu için
> sonuç bu sınır içinde okunmalıdır.

**CURRENT CANONICAL BASIS**

`public-profile-evidence-domain-model.md` §13 (`ProfileLimitation` taksonomisi
ve emission koşulu, `ProfileWarning`, `AnalysisStatus` ve expected evidence
surface accounting); `public-profile-evidence-repository-eligibility.md`
(iki aşamalı admission/eligibility ve `Undetermined`'ın korunması);
`public-profile-runtime-collection.md` §7 (gizli cap yasağı, declared bound
semantiği); `public-profile-evidence-report-contract.md` §6 (availability
durumları) ve §13 (warning presentation);
`public-profile-evidence-product-contract.md` §16-§17;
`p10-presentation-language-contract.md` §8.5, §8.6.5-§8.6.6.

**REQUIRED OBSERVABLE EVIDENCE**

Canonical repository manifest'i ve completeness durumu; repository başına
eligibility status; profile-level analysis status; canonical limitation ve
warning kimlikleri; bölüm availability durumları.

**CROSS-REPOSITORY RULE**

Bu outcome tek bir repository yorumundan doğmaz: değerlendirilen yüzeyin
kendisi profil genelindeki repository kümesidir.
`LimitedIndependentEvidenceSurface` koşulu doğrudan Outcome 1'in threshold'una
bağlıdır ve ayrı bir eşik tanımlamaz (domain model §13).

**DETERMINISM RULE**

Limitation ve warning kimlikleri stable semantic identity taşır; localize
mesajdan türetilmez, sıralama ve deduplication canonical kimliğe bağlıdır
(domain model §13). Availability durumları aynı ürün sürümünde sabittir.

**TRACEABILITY RULE**

Her repository satırı kendi eligibility status'üne ve analiz üretilmişse kendi
`RepositoryAnalysisReference`'ına bağlıdır. Limitation profil seviyesinde
scope'lanır ve hangi evidence-surface koşulundan doğduğu canonical kimliğiyle
okunur.

**COVERAGE RULE**

Kapsam, profil için üretilen canonical repository manifest'idir. Gizli bir
top-N, örnekleme veya heuristic alt küme seçimi yasaktır
(`public-profile-runtime-collection.md` §7). Manifest declared bir collection
bound veya bir failure nedeniyle daralırsa bu, incomplete manifest olarak
taşınır ve profile-level sonuç `Partial` tarafına düşer. Bu contract "complete
GitHub history", "bütün repository'ler" veya "proof of experience" gibi bir
kapsam iddiasını yasaklar.

**ABSTENTION RULE**

Bu outcome'un kendisi abstention'ın taşıyıcısıdır. Üç ayrım asla tek duruma
indirilemez: `Supported + empty` (kural çalıştı, sonuç üretmedi), `Unsupported`
(ürün bu sürümde bu yeteneği üretmiyor) ve `Deferred` (yetenek sözleşmeli ama
ertelendi). Hiçbiri "bulunamadı" veya kişi hakkında olumsuz bir sonuç olarak
sunulamaz.

**GENERIC AI CAN**

"Elimdeki bilgi sınırlı" veya "birkaç repo'ya baktım" diyebilir; hatta
temkinli bir uyarı cümlesi ekleyebilir.

**GITHUB PROOF STRUCTURALLY ADDS**

Temkinli cümle yerine **denetlenebilir bir kapsam kaydı** ekler:
değerlendirilen repository kümesi seçilmiş değil declared'dır, her repository
kendi durumuyla listelenir, kanıta katkı vermeyen repository listeden
düşürülmez ve yüzeyin daralması ayrı ve versioned bir limitation kimliği
olarak taşınır. Ayrıca "yetenek yok" ile "kanıt bu kurallarla oluşmadı"yı
yapısal olarak ayrı tutar.

**USER-VISIBLE DIFFERENCE**

Kullanıcı, sistemin hangi repository'lere baktığını listeden doğrulayabilir;
boş bir bölümü gördüğünde bunun bir eksiklik yargısı olmadığını metnin kendisi
söyler; ve sonucun sınırlı olduğu durumlarda sınırın kendisi ayrı bir bölüm
olarak durur.

**WHY USER CARES**

Sessizce örnekleme yapan bir sistem, kullanıcının kendi profili hakkında yanlış
bir genelleme kurmasına yol açar. Kapsamın declared olması, kullanıcının sonuca
ne kadar yaslanabileceğine kendisinin karar vermesini sağlar. Ayrıca kanıt
boşluğunun yetkinlik boşluğu olmadığı ürünün görünür parçası hâline gelir.

**NON-CLAIMS**

Kapsamın declared olması eksiksiz olduğu anlamına gelmez. Bir limitation veya
warning kişi hakkında olumsuz bir sonuç değildir; provider, sistem veya
operasyonel başarısızlık kanıt, negatif kanıt, eksik yetkinlik veya profile
truth değildir.

**UI CONSEQUENCE**

Sonraki UI/UX çalışması şunları önceliklendirmelidir: repository listesinin
filtrelenmemiş olması, satır başına durum ayrımı, limitation'ın ayrı ve
okunabilir bir bölüm olarak var olması ve availability üç durumunun tek bir
"bulunamadı" görüntüsüne çökmemesi.

---

## Outcome 4 — Pattern-Scoped Starting Point

**PRIORITY**

SUPPORTING

**USER QUESTION**

"Bu kadar repository arasından nereden başlamalıyım?"

**USER-VISIBLE RESULT**

Kullanıcı nötr bir başlangıç repository'si ve oradan doğrudan Repository Proof
analizine geçiş alır.

Seçim canonical olarak pattern-scoped'dur: her tekrar eden pattern için ayrı ve
yalnız o pattern'in kendi kanıtıyla hesaplanır. **Ancak mevcut Web yüzeyi bu
pattern ilişkisini göstermez** — başlangıç noktası yalnız repository kimliği,
GitHub'ın kendi description'ı ve analiz eylemiyle sunulur. Dolayısıyla seçimi
distinctive kılan bağ bugün kullanıcıya görünür değildir; bunu görünür kılmak
bu contract'ın sonraki UI/UX session'a devrettiği ana iştir (bkz. UI
CONSEQUENCE ve §9).

**EXAMPLE**

> Başlangıç noktası: `owner/repo-b` — GitHub'daki kendi kısa açıklamasıyla
> birlikte ve "bu repository'yi analiz et" eylemiyle.

**CURRENT CANONICAL BASIS**

`public-profile-representative-repositories.md` §3 (pattern başına tek seçim),
§4 (aday kümesi), §5 (karşılaştırma kuralı), §6 (tie-break), §8 (cross-pattern
bağımsızlık), §3.3 (boş pattern seti);
`public-profile-evidence-product-contract.md` §13;
`public-profile-evidence-report-contract.md` §10;
`p10-presentation-language-contract.md` §8.6.1-§8.6.1a.

**REQUIRED OBSERVABLE EVIDENCE**

Pattern'in `IndependentSupportingRepositories` kümesi ve o pattern'e ait
`SupportingObservations` içinde adayın pozitif destek referansları.

**CROSS-REPOSITORY RULE**

Seçim yalnız bir pattern'in kendi bağımsız destekleyici repository'leri
arasından yapılır; dolayısıyla Outcome 1'in recurrence koşulu ön koşuldur.
Global top-N, top-K, set-cover veya profil genelinde breadth ölçümü yasaktır.

**DETERMINISM RULE**

Seçim, canonical representative-repository kuralını **tüketir**; bu contract o
kuralı tanımlamaz. Karşılaştırma, tie-break, aday kimliği önkoşulu ve
determinizm garantilerinin tek sahibi
`public-profile-representative-repositories.md` §4-§6'dır.

> **CONSUMED, NOT OWNED** — bu outcome için gereken tek semantic özet:
> karşılaştırma yalnız o pattern'e ait kanıtla yapılır ve eşitlik canonical
> kimlik sırasıyla çözülür; popülerlik, güncellik, boyut, aktivite veya gizli
> ağırlık hiçbir aşamada katılmaz. Kesin kural, alan adları ve sürüm
> sahipliği owner'dadır.

**TRACEABILITY RULE**

Seçimin gerekçesi yalnız Representative Support Evidence Trace'ten okunur:
seçilen repository + tam olarak temsil ettiği pattern'e scope'lu observation ve
evidence referansları. Canonical Pattern Trace kimlik/bağlam içindir ve
representative-specific gerekçe kaynağı olarak kullanılamaz.

**COVERAGE RULE**

Aday kümesi canonical representative-repository kuralından **tüketilir** ve
burada yeniden tanımlanmaz; provenance, lifecycle ve bağımsızlık davranışının
sahibi `public-profile-representative-repositories.md` §4 ile
`public-profile-evidence-repository-eligibility.md`'dir.

> **CONSUMED, NOT OWNED** — bu outcome için gereken tek semantic özet: adaylık
> Outcome 1'in bağımsız tekrar koşulunu geçmiş repository'lerle sınırlıdır ve
> belirsiz bağımsızlık iyimser biçimde adaylığa çevrilmez.

**ABSTENTION RULE**

Promoted pattern yoksa representative seçimi de yoktur; boş sonuç geçerlidir ve
uydurulmuş bir başlangıç noktası üretilemez.

**GENERIC AI CAN**

"Şu repo'dan başla" diyebilir ve makul bir gerekçe uydurabilir; genellikle
popülerlik, isim tanınırlığı veya README kalitesi üzerinden.

**GITHUB PROOF STRUCTURALLY ADDS**

Öneriyi bir beğeni yargısından çıkarıp **belirli bir pattern'e bağlı, o
pattern'in kendi kanıtıyla hesaplanmış** bir seçime dönüştürür: "bu repository
bu pattern'i oluşturan yapılandırılmış kanıtın daha fazlasını gösteriyor". Bu
ölçü tek bir pattern dışında hiçbir anlam taşımaz, pattern'ler arasında
toplanmaz ve karşılaştırılmaz.

**USER-VISIBLE DIFFERENCE**

Başlangıç noktası bir "en iyi proje" listesi gibi değil, tekrar eden sonuçların
yanında duran nötr bir giriş noktası olarak görünür; ve oradan Repository Proof
analizine geçiş tek adımdır.

**WHY USER CARES**

Onlarca repository arasında ilk bakılacak yeri seçmek gerçek bir maliyettir.
Popülerliğe göre seçim kullanıcıyı yanlış yere götürebilir; pattern'e bağlı
seçim, kullanıcıyı sonucu üreten kanıtın en yoğun göründüğü yere götürür.

**NON-CLAIMS**

En iyi, en güçlü, en kaliteli, en profesyonel, en popüler veya önerilen
repository değildir. Portfolio sıralaması değildir. Web tarafında görünen
sınırlı sayıdaki öğe stable bir presentation örneğidir; önem sırası değildir.

**UI CONSEQUENCE**

**Birinci ve en yüksek öncelikli gereksinim:** sonraki UI/UX çalışması şu
ilişkiyi görünür kılmalıdır:

~~~text
tekrar eden pattern
        →
o pattern için seçilen başlangıç repository'si
~~~

Bu ilişki görünür olmadan Outcome 4 kullanıcı yüzeyinde sıradan bir "şu
repo'dan başla" önerisinden ayrışmaz. İlişki görünür kılınırken **en iyi**, **en
güçlü**, **önerilen** veya **kalite sıralaması** anlamına çevrilemez; gösterilecek
olan yalnız hangi tekrar eden pattern için hangi repository'nin seçildiğidir.

**Bu gereksinimin canonical statüsü:** mevcut Web yüzeyi pattern →
başlangıç-repository ilişkisini göstermiyor ve bu eksiklik **bugün hiçbir
canonical doküman tarafından kayıtlı bir gap olarak tutulmuyor**. Görünür
kılınması `p10-presentation-language-contract.md` §8.6.1'in explicit bir
presentation-contract amendment'ini gerektirir; o bölüm başlangıç noktası
öğesinin **"renders exactly"** üç öğesini (repository kimliği, GitHub
description'ı, analiz eylemi) kilitlemektedir.

Bu gereksinim §8.6.1a'da kayıtlı **Starting Point Prioritization Contract
Gap** ile **karıştırılmamalıdır**: o gap canonical seçim üzerinde bir
importance/priority *ordering* — yani ranking — konusudur ve bu contract
ranking'i açıkça yasaklar. Pattern ilişkisini göstermek ranking değildir ve o
gap'in kapatılmasını yetkilendirmez. Yeni bir seçim semantiği de gerekmez;
mevcut canonical seçim zaten pattern başına yapılmaktadır.

Ayrıca önceliklendirilmelidir: başlangıç noktasının Outcome 1'e göre ikincil
kalması, repository kimliğinin literal korunması, GitHub'ın kendi
description'ının kanıt olarak sunulmaması ve sıralamanın öncelik gibi
okunmasını engelleyecek semantic çerçeve. Skor, yıldız, fork, dil yüzdesi,
aktivite veya kalite alanı eklenmez.

---

## 6. Rejected Candidates

| Aday | Karar | Gerekçe |
|---|---|---|
| Evidence convergence (farklı evidence yüzeylerinin aynı sonucu birlikte desteklemesi) | **REJECTED — NOT SUPPORTED** | Mevcut model ayrı bir convergence semantiği üretmiyor. patterns §7.2'de yalnız `documentation` iki signal'e map'lenir ve union repository başına bir kez sayılır; bu bir repetition kuralıdır, convergence değil. Ayrı bir promotion semantiği yeni domain semantics ve yeni `PatternRuleVersion` gerektirir. Bu session bunu üretmez; owner kararı gerekir (§9) |
| Profile / developer score'u moat olarak kullanmak | REJECTED | `public-profile-evidence-product-contract.md` §15 profile-level developer skorunu zaten yasaklar; ayrıca skor §4'ün C ve F kriterlerini geçmez ve ilk okuma değerini kanıta değil sayıya bağlar |
| En çok kullanılan diller, teknoloji bulutu, toplam repository/commit sayısı, en aktif repository, stars, followers, aktivite sıralaması | REJECTED | GitHub aynı değeri doğrudan veriyor. Supporting context olabilir; moat değildir |
| AI özet / doğal dil anlatım | REJECTED (outcome olarak) | Generic AI daha akıcı prose üretebilir. Mevcut anlatım katmanının distinctive yanı prose olması değil, promoted pattern'ler üzerinde deterministik bir şablon olmasıdır; yani Outcome 1'in sunumudur, ayrı bir outcome değil |
| Problem area patterns | REJECTED | V1'de `Unsupported`; taxonomy, promotion kuralı ve production code path yok (patterns §9) |
| Role evidence alignment | REJECTED | Profile V1 / Milestone B için `Deferred` (product contract §14) |
| "Bütün profili tam kapsamla değerlendirdik" iddiası | REJECTED | Manifest declared bir collection bound taşıyabilir; dürüst outcome eksiksizlik değil declared kapsamdır ve Outcome 3'e katlanmıştır |

---

## 7. Product Moat Statement

Canonical, normative:

> Profile Proof public GitHub verisini özetlemez: declared bir repository kanıt
> yüzeyi üzerinde, yalnız birden fazla bağımsız repository'de tekrar eden
> observable pattern'leri versioned ve deterministik bir promotion kuralıyla
> profile-level sonuca yükseltir; **promote edilmiş tekrar eden pattern
> sonuçlarını** destekleyen repository'ler üzerinden repository-level kanıta
> geri bağlar; promotion kuralının hiçbir sonuç üretmediği durumu mevcut
> kurallar altında **açık bir analiz sonucu** olarak belirtir ve bunu kişi
> hakkında olumsuz bir sonuca çevirmez; gözlenen kanıt yüzeyinin kendisi
> daraldığında veya eksik kaldığında bunu **ayrıca ve ayrı olarak** canonical
> limitation / partial-evidence semantiği üzerinden gösterir.

> Profile Proof does not summarize public GitHub data: over a declared
> repository evidence surface, it promotes only those observable patterns that
> recur across more than one independent repository into profile-level results
> under a versioned deterministic rule; it links **the promoted recurring
> pattern results** back to repository-level evidence through their supporting
> repositories; it states the case where the promotion rule produced nothing as
> an **explicit analytical outcome** under the current rules, never as a
> person-level negative claim; and, separately from that, it exposes genuine
> narrowing or incompleteness of the observed evidence surface through the
> canonical limitation / partial-evidence semantics.

Bu iki durum **aynı şey değildir** ve tek bir "sınır" kavramına indirilemez:

~~~text
promotion sonuç üretmedi     = Supported + empty, bir analiz sonucudur
evidence surface daraldı     = ProfileLimitation / partial-evidence
~~~

`Supported + empty` bir limitation **değildir** ve her boş sonuç bir
`ProfileLimitation` üretmez; ayrımın sahibi
`docs/architecture/public-profile-evidence-domain-model.md` §13'tür ve bu
contract yeni bir limitation kimliği tanımlamaz.

Bu ifade bir slogan değildir; §5'teki dört outcome'un ortak normative
özetidir. Genişletilemez: "all repositories", "complete GitHub history",
"proof of skill" ve "proof of experience" ifadeleri bu moat'ın parçası değildir
ve kullanılamaz.

---

## 8. First-Read Wow Contract

Bu bölüm bir layout tanımı değildir. Bilgi önceliği sözleşmesidir: hangi
semantic bilginin ilk okumada zorunlu, hangisinin erişilebilir, hangisinin ilk
okumadan dışlanmış olduğunu belirler.

**Sahiplik mutabakatı.** Genel Profile first-read hedefinin — raporun ilk
katmanının hangi soruları cevaplayabilmesi gerektiğinin — sahibi
`public-profile-evidence-product-contract.md` §25'tir ve öyle kalır. Bu bölüm
§25'i değiştirmez, daraltmaz veya yerine geçmez; o soruların burada tekrarı da
yapılmaz. Bu bölümün sahiplendiği tek ek kısıt şudur: **distinctive
outcome'lar — özellikle recurrence — §25'in tanımladığı first-read çerçevesi
içinde algılanabilir olmalıdır.** Yani §25 *neyin cevaplanacağını*, bu bölüm
*distinctive değerin o cevap içinde nasıl önceliklendirileceğini* belirler.

Somut bölüm sırası, disclosure davranışı, etiketler ve exact copy
`p10-presentation-language-contract.md` §8.4-§8.6'nın sahipliğindedir ve burada
tekrar edilmez. Bu bölüm yalnız outcome-level önceliği ekler.

Hedeflenen ilk tepki:

> "Bunların GitHub'ımda olduğunu biliyordum ama birlikte böyle bir şey
> anlattığını daha önce görememiştim."

Hedeflenmeyen ilk tepki: "dashboard güzel", "skorum yüksek", "AI güzel
özetlemiş".

### 8.1 Zorunlu ilk okuma sırası (semantic prominence)

1. **Recurrence önce gelir.** İlk ve en belirgin profile-level ifade Outcome
   1'in sonucudur. Sayı, liste, skor, rozet veya kişi-seviyesi bir tanım bu
   konumu alamaz.
2. **Tek repo olmadığı aynı okumada anlaşılır.** Sonucun birden fazla
   repository'de yeniden görüldüğü, hiçbir şey açılmadan ve sayı gösterilmeden,
   ifadenin kendi dilinden anlaşılabilmelidir.
3. **Sonuç, kişi hakkında hüküm olmadığı için öyle okunur.** Bu, uyarı
   etiketiyle değil, ifadenin kendi diliyle ve her zaman bulunan kapsam
   cümlesiyle sağlanır.
4. **"Neden" yolunun var olduğu görünür.** Kullanıcı, gerekçe ve destekleyici
   repository'lere ulaşabileceğini ilk okumada anlamalıdır; ancak bu yol
   Meaning ile yarışmaz.
5. **Başlangıç noktası ikincildir.** İlk okumada bulunabilir, fakat Outcome
   1'in önüne geçemez ve "en iyi/önerilen" çerçevesiyle sunulamaz.
6. **Abstention dürüsttür.** Promotion kuralı hiçbir sonuç üretmediyse ilk
   okuma bunu mevcut kurallar altında bir gözlem sonucu olarak söyler ve
   eksiklik gibi okunmaz. Evidence surface'in kendisi daraldıysa bu sınır aynı
   sayfadan erişilebilir olmalıdır; ancak ilk okumaya liderlik etmesi zorunlu
   değildir.
7. **Başlangıç noktasının pattern bağı henüz görünmüyor.** Mevcut Web yüzeyi,
   bir başlangıç repository'sinin hangi tekrar eden pattern'e ait olduğunu
   göstermiyor. Bu bir gelecek gereksinimidir, mevcut yetenek beyanı değildir:
   sonraki UI/UX session'ının kapatması gereken ana boşluk budur (Outcome 4 UI
   CONSEQUENCE ve §9). Kapatılırken bağ, "en iyi/önerilen repository" veya
   kalite sıralaması anlamına çevrilemez.

### 8.2 Supporting repository breadth kararı

Breadth **ilişki olarak** ilk okumada zorunludur (madde 2), **kimlik olarak**
ilk okumada yasaktır.

Gerekçe canonical'dır: Meaning katmanı repository kimliği, seçim detayı veya
evidence trace taşıyamaz ve destekleyici repository kimlikleri varsayılan
olarak kapalı evidence katmanına aittir
(`p10-presentation-language-contract.md` §8.4.7, §8.6.1-§8.6.3). Bu contract o
kararı değiştirmez; ilk okuma önceliği olarak teyit eder.

Sayı gösterimi ayrıca yasaktır: destekleyici repository sayısı da dâhil olmak
üzere hiçbir count ilk okuma yüzeyine getirilemez.

Bu yasağın sahipliği bölünmüştür ve doğru biçimde şöyle okunur:

~~~text
p10 §8.4.7   Meaning katmanı için count exclusion — o katmanın sahibi
p10 §8.6.3   Why / evidence katmanı davranışı — o katmanın sahibi
bu contract   ilk okuma yüzeyinin tamamını kapsayan geniş "count yok" kısıtı
~~~

Yani ilk okumanın **bütününü** kapsayan geniş kısıt bu contract'ın kendi
first-read priority sahipliği içinde **kendi kuralıdır**; başka bir owner'ın
kuralı olarak sunulmaz. Mevcut owner'larla aynı yöndedir, onlardan daha
katıdır ve bugün render edilen hiçbir yüzeyle çelişmez.

### 8.3 İlk okumadan dışlananlar

İlk okuma yüzeyi şunları taşıyamaz: skor, sayı/oran/rozet/sayaç, sıralama veya
güçlü-zayıf ekseni, analyzer iç kelimeleri (evidence key, observation key,
rule/policy version, analiz kimliği), seçim veya promotion mekaniği ve kişi
hakkında rol/kıdem/yetkinlik/işe uygunluk anlamına gelen herhangi bir ifade.
Bu yasakların canonical sahipleri §2 tablosundadır.

---

## 9. Current Capability Gap

Sınıflandırmalar gerçek canonical contract ve mevcut kod/projection üzerinden
doğrulanmıştır.

### Outcome 1 — Cross-Repository Recurring Evidence

**SUPPORTED NOW.**

Promotion kuralı ve threshold domain'de mevcut
(`ProfilePatternPromotionPolicy`, `RepeatedProfilePattern`); typed report
`TechnicalFocusAreas` ve `EngineeringPracticePatterns` bölümlerini `Supported`
olarak üretir.

Kullanıcı yüzeyinde tekrar ilişkisi iki farklı yoldan geliyor ve bu ayrım
kayda değer: `TechnicalFocusAreas` kendi bölümünde bir tekrar ipucu taşıyor,
`EngineeringPractice` tarafında ise tekrar ilişkisi **birincil olarak
deterministik anlatım (Meaning) katmanından** algılanıyor — practice bölümü
etiketleri kendi başına tekrar ifadesi taşımıyor. Meaning her zaman render
edildiği ve ilk okumada bulunduğu için `SUPPORTED NOW` sınıflandırması
geçerlidir.

### Outcome 2 — Traceable Conclusion-to-Repository Evidence Chain

**SUPPORTED NOW**, fresh-run niteliğiyle.

`WhyThisConclusion` ve `RepositoryDrillDown` bölümleri `Supported` olarak
projeksiyona giriyor; Why katmanı destekleyici repository kimliklerini
listeliyor; repository evidence katmanı repository başına mevcut Repository
Proof girişini sağlıyor. Zincirin son halkası profile tarafında kopyalanmıyor,
referansla gidiliyor.

**Kayıtlı nitelik:** son hop teslim edilen akışta fresh bir Repository Proof
çalıştırmasıdır, profile sonucunu üreten analiz instance'ının dondurulmuş
kopyası değildir. Outcome 2 bu niteliğiyle destekleniyor; immutable historical
replay ne iddia ediliyor ne de gerekiyor.

### Outcome 3 — Declared Evidence Surface and Its Boundary

**SUPPORTED NOW.**

`RepositoryDrillDown` her eligibility sonucunu status'üyle taşıyor ve
filtrelenmiyor; `Limitations` bölümü `Supported` ve iki canonical limitation
kimliği emit ediliyor; incomplete manifest profile-level `Partial` durumuna ve
`PartialEvidenceSurface` limitation'ına bağlanıyor; warning kimlikleri ayrı bir
bildirim yüzeyine sahip; availability üç durumu typed report'ta korunuyor.

**Kayıtlı gözlem (yeni kural değil, blocker değil):** mevcut limitation
taksonomisi yüzeyin **neden** daraldığını ayırmaz — declared bir collection
bound ile değerlendirilemeyen bir repository aynı `PartialEvidenceSurface`
kimliğini üretir. Bu kasıtlıdır: domain model §13 yeni limitation kimliğinin
spekülatif eklenmesini yasaklar ve versioned bir contract değişikliği ister.
Daha ince bir ayrım istenirse bu ayrı bir owner kararıdır.

### Outcome 4 — Pattern-Scoped Starting Point

**PARTIALLY SUPPORTED.**

Semantic/model capability mevcut: seçim politikası domain'de var
(`RepresentativeRepositorySelectionPolicy`), seçim deterministik ve
pattern-scoped, tie-break canonical kimlik sırasıyla çözülüyor, stars/recency/
activity/popularity veya herhangi bir ranking katılmıyor, ve
`RepresentativeRepositories` `Supported` bir report bölümü olarak üretiliyor.
Web başlangıç noktası yüzeyi canonical sırayı koruyan bounded bir presentation
örneği ve repository başına Repository Proof eylemi sunuyor.

**Distinctive user-visible sonuç ise bugün teslim edilmiyor.**
`RepresentativePatternSupport` Web'in hiçbir yerinde render edilmiyor;
başlangıç noktası yüzeyi yalnız repository kimliği, GitHub'ın kendi
description'ı ve analiz eylemini gösteriyor. Kullanıcı hangi tekrar eden
pattern için hangi repository'nin seçildiğini göremediği için, outcome'u
distinctive kılan pattern-scoping kullanıcı tarafından algılanamıyor.

Sınıflandırma bu yüzden standart anlamıyla `PARTIALLY SUPPORTED`'dır: model
capability var, distinctive user-visible result henüz yok. Kapatılması gereken
iş UI CONSEQUENCE'ta tanımlıdır ve yeni bir seçim semantiği gerektirmez.

### Reddedilen aday — Evidence convergence

**NOT SUPPORTED.**

Ayrı bir convergence semantiği bugün üretilemez ve üretilmesi yeni domain
semantics + yeni `PatternRuleVersion` gerektirir. Bu contract onu bir outcome
olarak tanımaz ve bir kural uydurmaz. İstenirse ayrı bir owner kararıyla
açılır.

---

## 10. Change Rule

Bu contract'taki bir outcome'un priority'si, kabul gerekçesi, moat statement'ı
veya first-read önceliği değişecekse:

~~~text
explicit product decision
→ bu contract'ın versioned güncellemesi
→ etkilenen canonical owner'ların review'ü
→ presentation/UI etkisinin değerlendirilmesi
~~~

sırası izlenir.

Bu contract'ın yeni bir versiyonu şunlar için gereklidir:

- kabul edilen outcome setinin değişmesi,
- bir outcome'un priority'sinin değişmesi,
- acceptance test kriterlerinin değişmesi,
- moat statement'ının değişmesi,
- first-read zorunlu sırasının değişmesi.

Bu contract hiçbir zaman başka bir canonical owner'ın semantiğini genişletmek
için kullanılamaz. Bir outcome'un gerektirdiği yeni evidence kuralı, threshold,
taxonomy veya presentation copy, ilgili owner'ın kendi versioned
değişikliğidir.
