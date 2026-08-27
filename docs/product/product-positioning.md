# GitHub Proof Product Positioning

**Status:** PRODUCT POSITIONING LOCKED
**Version:** 1
**Session:** 069
**Date:** 2026-08-18

Bu doküman GitHub Proof'un ürün konumlandırmasını ve epistemik sınırını sahiplenir.

Sinyal, puanlama, profile, job veya mimari semantiği tanımlamaz; bunlar kendi
canonical contract'larına aittir. Bu doküman roadmap veya architecture contract
değildir.

---

## 1. Ürün Amacı

GitHub Proof, public GitHub üzerindeki dışarıdan gözlemlenebilen mühendislik
kanıtını anlaşılır ve geri izlenebilir hâle getirmeyi amaçlayan evidence-first
bir üründür; aynı zamanda bu kanıtın sınırlarını açıkça belirtir.

Canonical boundary:

> GitHub Proof is an evidence-first product for making externally observable
> engineering evidence from public GitHub understandable and traceable, while
> explicitly stating the limits of that evidence.

Repository kanıtı, arkasındaki insan hakkında desteklenmeyen iddialara
dönüştürülemez.

### Ürün kapsamı ve teslim edilmiş yetenek

Onaylanmış ürün kapsamı hem repository-level hem profile-level analizi içerir.

Bu doküman ürün konumlandırmasını sahiplenir; hangi yeteneğin bugün teslim
edilmiş olduğunu tanımlamaz.

Repository-level ve profile-level yetenekler kendi canonical contract'larına
ve güncel roadmap durumuna göre teslim edilir:

| Kapsam | Canonical contract | Teslim durumu |
|---|---|---|
| Repository-level | `docs/product/mvp-product-contract.md` | ilgili contract ve roadmap durumuna göre |
| Profile-level | `docs/product/public-profile-evidence-product-contract.md` | ilgili contract ve roadmap durumuna göre |
| Job-level | `docs/product/job-evidence-alignment-product-contract.md` | ilgili contract ve roadmap durumuna göre |

Güncel geliştirme ve release durumu için `docs/roadmap/mvp-phases.md`,
`docs/roadmap/deployment-milestones.md` ve `worklog/CURRENT.md` geçerlidir.

Bu doküman hiçbir katmanın bugün tamamlanmış olduğunu iddia etmez.

---

## 2. AI Coding Çağındaki Problem

AI coding agent'ları, sofistike görünen repository çıktısı üretmenin maliyetini
düşürmüştür.

Bir kişi, yazılım bilgisi çok sınırlı olsa bile bir AI coding agent'ından
framework, test, CI, Docker, migration ve güçlü görünen mühendislik yapısı
taşıyan bir repository üretmesini isteyebilir.

Bu nedenle repository çıktısı; insan authorship'inin, insan anlayışının veya
insan yetkinliğinin kanıtı olarak ele alınamaz.

Aşağıdaki çıkarım zinciri public GitHub evidence'ından güvenilir biçimde
kurulamaz:

~~~text
kod var
→ geliştirici yazdı
→ geliştirici anlıyor
→ geliştirici bu konuda yetkin
~~~

İlk geçiş bile garanti değildir.

Temel invariant:

~~~text
Repository Evidence
!= Human Capability
!= Human Authorship
!= Human Understanding
~~~

AI kullanımı olumlu veya olumsuz bir bulgu değildir.

GitHub Proof AI kullanımını tespit etmediği için onu ne ödüllendirir ne
cezalandırır. Değişen şey kanıt yüzeyi değildir; değişen şey, bu kanıtın insan
anlayışı hakkında hiçbir şey söylemediğinin artık çok daha görünür olmasıdır.

---

## 3. GitHub Proof Neyi Gözlemler

Sistem yalnız public GitHub üzerinde dışarıdan gözlemlenebilen yüzey hakkında
konuşur.

Gözlemlenebilir olan:

- sabitlenmiş bir snapshot'ta tanımlı repository artefaktlarının varlığı veya
  yokluğu,
- bu gözlemin hangi dosya, commit, workflow veya manifest'e dayandığı,
- aynı gözlemin birden fazla bağımsız eligible repository'de tekrar edip
  etmediği,
- bir gözlemin neden üretilemediği.

Kurulması zorunlu güven zinciri:

~~~text
Claim
    ↓
Evidence
    ↓
Evidence Reference
    ↓
Observable Source
~~~

Bu zincir positioning seviyesindedir. Belirli evidence tiplerinin adlandırması
ve semantiği `docs/architecture/` altındaki canonical contract'lara aittir ve
bu zincir onların yerine geçmez.

Bu zincirin kurulamadığı bir sonuç canonical truth olamaz.

Sonucun üretilememesi de bir üründür: eksik veya erişilemeyen kanıt ile
otomatik analizin güvenilir deterministik sonuç üretemediği durumlar birbirinden
ayrı ve açık biçimde gösterilir. Bu durumların exact status semantiği
`docs/product/scoring-and-report-contract.md` tarafından sahiplenilir ve bu
doküman onları yeniden tanımlamaz veya tek bir duruma indirgemez.

---

## 4. GitHub Proof Neyi Kurmaz

Public repository kanıtı tek başına aşağıdakileri kuramaz ve GitHub Proof
bunları belirlemeye çalışmaz:

- kodun AI tarafından yazılıp yazılmadığı,
- kodun hangi oranda AI tarafından yazıldığı,
- kesin insan authorship'i,
- özgünlük,
- repository sahibinin implementasyonu anlayıp anlamadığı,
- geliştiricinin yetkinliği,
- kıdem seviyesi,
- profesyonel deneyim süresi,
- işe alınabilirlik,
- işe alım kararı,
- aday sıralaması,
- production güvenilirliği veya güvenlik denetimi.

Kanıtın bulunamaması, ilgili şeyin var olmadığı anlamına gelmez.

Public GitHub'da görünmeyen private çalışmalar, şirket içi kod tabanları, NDA
kapsamındaki işler ve başka platformlardaki üretim hakkında sistem olumlu veya
olumsuz varsayım yapamaz.

---

## 5. "Proof" Ne Anlama Gelir

Ürün adı GitHub Proof olarak korunur.

"Proof" yalnız gözlemlenebilir ve geri izlenebilir repository kanıtını ifade
eder.

Canonical sınır:

> Kanıt gözlemlenebilir repository evidence'ına ilişkindir; arkasındaki kişi
> hakkında bir hüküm değildir.

> The proof concerns observable repository evidence, not a verdict about the
> person behind it.

"Proof" hiçbir zaman bir insanın şunları yaptığının kanıtı anlamına gelmez:

- kodu yazdığı,
- kodu anladığı,
- bir teknolojiyi bildiği,
- belirli bir seviyede olduğu,
- senior olduğu,
- işe alınabilir olduğu,
- işe alınması gerektiği.

Kullanılmaması gereken ifadeler:

~~~text
proven developer
proven skills
verified engineer
GitHub proves your ability
GitHub proves your seniority
kanıtlanmış yetkinlik
GitHub'ınız sizi kanıtlıyor
~~~

---

## 6. Kanıt Sunar, Hüküm Vermez

Temel ürün ilkesi:

> **Kanıt sunar, hüküm vermez.**
>
> **Evidence, not verdicts.**

Destekleyici ilke:

> Kanıt izlenebilir. Sınır görünür.
>
> Evidence is traceable. Its limits are explicit.

Bunun pratik karşılığı:

- her anlamlı sonuç kaynağına kadar izlenebilir olmalıdır,
- belirsizlik gizlenmez, ürünün görünür parçasıdır,
- kanıtın epistemik gücü, onu destekleyen gözlemi hiçbir zaman aşamaz,
- sunum katmanı kanıtın anlamını yükseltemez.

---

## 7. Kullanıcı Değeri

Üç kullanıcı perspektifi de aynı canonical evidence truth'u tüketir. Ayrı bir
truth sistemi kurulmaz; yalnız sunum farklılaşabilir.

### 7.1 Yeni başlayan / early-career geliştirici

Temel soru:

> Public GitHub'ım gerçekten neyi destekliyor?

Değer:

Görünür kanıtı ve kanıt boşluklarını anlaşılır hâle getirmek.

Zorunlu sınır:

~~~text
Evidence gap != skill gap
~~~

Public kanıt boşluğu, yetkinlik boşluğu anlamına gelmez. Bir konuda public
kanıt bulunamaması, o konunun bilinmediği veya yetersiz olunduğu sonucuna
çevrilemez.

Asla:

Yetkinlik veya hazır olma durumu onaylanmaz.

### 7.2 Deneyimli geliştirici

Temel soru:

> Public çalışmamdaki mühendislik kanıtı gerçekten görünüyor mu?

Değer:

Tekrar eden ve geri izlenebilir public mühendislik kanıtını daha kolay
anlaşılır hâle getirmek.

Asla:

Kıdem onaylanmaz.

### 7.3 Recruiter / teknik değerlendirici

Temel soru:

> Bu public GitHub'da gerçekten hangi kanıt görünür?

Mevcut değer:

Public kanıtı incelemek için gereken eforu azaltmak ve her sonucu kaynağına
kadar izlenebilir kılmak.

Gelecek yön:

Bu kanıtın ileride insan doğrulama konularının hazırlanmasında kullanılabilmesi.

Asla:

Aday puanlanmaz, sıralanmaz, kıdem atanmaz, işe alım kararı verilmez.

---

## 8. Mevcut Yetenek ve Gelecek Yön

Bu ayrım zorunludur ve karıştırılmamalıdır.

### Mevcut ürün gerçeği

GitHub Proof gözlemlenebilir kanıtı ve bu kanıtın sınırlarını görünür kılar.

### Gelecek yön — henüz mevcut yetenek değildir

Bu kanıt ileride, insanla yapılacak bir konuşma veya teknik görüşme için
evidence-backed doğrulama konularını destekleyebilir.

Bu yön onaylanmış bir ürün yönüdür ancak bugün üretilen bir çıktı değildir.

Ürün bugün:

- doğrulama noktası üretmez,
- görüşme sorusu üretmez,
- insan anlayışını doğrulamaz.

Bu yönün semantiği, traceability'si ve sınırları ayrı ve onaylanmış bir
contract gerektirir. Bu doküman onu tanımlamaz.

### RequiresHumanReview ile ilişkisi

`RequiresHumanReview` bu gelecek yönle ilgisizdir.

Mevcut kilitli anlamı korunur:

> Otomatik analiz ilgili sinyal için güvenilir ve deterministik bir sonuç
> üretememiştir.

Bu bir analiz durumu kodudur; insan doğrulaması kavramı değildir. İkisi
birbirinin yerine kullanılamaz ve gelecekteki human verification bu status'ü
yeniden yorumlayamaz.

---

## 9. Mevcut Contract'larla İlişki

Bu doküman pozisyonlama ve epistemik sınır sahibidir. Aşağıdaki contract'ların
semantiğini değiştirmez, tekrar etmez veya geçersiz kılmaz.

| Alan | Canonical sahip |
|---|---|
| Repository MVP ürün sınırları | `docs/product/mvp-product-contract.md` |
| Sinyal sınırları ve yasaklı çıkarımlar | `docs/product/analysis-categories-and-signal-boundaries.md` |
| Puanlama, status ve rapor sözleşmesi | `docs/product/scoring-and-report-contract.md` |
| Rapor bilgi mimarisi | `docs/product/report-information-architecture.md` |
| Profile-level evidence truth | `docs/product/public-profile-evidence-product-contract.md` |
| Profile Proof distinctive outcome semantiği ve first-read bilgi önceliği | `docs/product/profile-proof-distinctive-outcomes-contract.md` |
| Job evidence alignment | `docs/product/job-evidence-alignment-product-contract.md` |
| Repository evidence ladder ve mimari semantik | `docs/architecture/` |
| Capability development sırası | `docs/roadmap/mvp-phases.md` |
| Production release sırası | `docs/roadmap/deployment-milestones.md` |

Çelişki durumunda ilgili alanın canonical sahibi geçerlidir; bu doküman
pozisyonlama dilini hizalar, kilitli semantiği değiştirmez.

Repository MVP V1'in 15 sinyali, 4 kategorisi ve puanlama semantiği bu
dokümandan etkilenmez. Skor bir developer skoru değildir ve bu ayrım
`docs/product/scoring-and-report-contract.md` tarafından sahiplenilir.
