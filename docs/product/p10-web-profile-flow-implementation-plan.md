# GitHub Proof — P10 Implementation Plan

**Status:** Draft
**Milestone:** B — Profile Evidence Experience
**Phase:** P10 — Web Profile Flow
**Depends On:** P9 Complete

---

# 1. Purpose

P8 ile GitHub Proof, public GitHub repository izlerinden deterministik bir profile evidence çıktısı üretme kapasitesine ulaştı.

P8 zinciri:

```
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
ProfileReportProjector
        ↓
ProfileReport
```

P10'un amacı yeni analiz üretmek değildir.

P10'un amacı:

> Mevcut ProfileReport çıktısını kullanıcının anlayabileceği sade bir deneyime dönüştürmektir.

---

# 2. P8 → P10 Boundary

## P8 Responsibility

P8:

- GitHub verisini analiz eder.
- Evidence üretir.
- Pattern oluşturur.
- Representative repository belirler.
- ProfileReport oluşturur.

## P10 Responsibility

P10:

- ProfileReport'u sunar.
- Kullanıcı anlamını oluşturur.
- Evidence keşif deneyimi sağlar.

Akış:

```
ProfileReport

↓

Presentation

↓

User Understanding
```

---

# 3. Core Principle

GitHub Proof:

Bir geliştiriciyi değerlendirmez.

Bir GitHub profilindeki görünür izlerin ne anlattığını gösterir.

P10:

Yapar:

- gözlemlenebilir izleri gösterir.
- anlamlandırılmış sunum sağlar.
- kanıta ulaşmayı kolaylaştırır.

Yapmaz:

- yetenek puanı verir.
- senior/junior kararı verir.
- işe uygunluk çıkarımı yapar.
- kalite hükmü verir.
- gelecekteki başarı tahmini yapar.

---

# 4. Existing P8 Capability

ProfileReport bölümleri:

- PublicProfileSummary
- TechnicalFocusAreas
- EngineeringPracticePatterns
- ProblemAreaPatterns
- RepresentativeRepositories
- WhyThisConclusion
- Limitations
- RepositoryDrillDown
- RoleEvidenceAlignment

---

# 5. Capability Boundaries

## Supported in P10

### PublicProfileSummary

Kullanıcıya:

"Bu GitHub profili ne anlatıyor?"

sorusunun başlangıç cevabını verir.

### TechnicalFocusAreas

Kullanıcı dili:

"Görünen teknik yön"

### EngineeringPracticePatterns

Kullanıcı dili:

"Tekrar eden üretim pratikleri"

### RepresentativeRepositories

Kullanıcı dili:

"Nereden başlamalı?"

### WhyThisConclusion

Kullanıcı dili:

"Bunu neden görüyoruz?"

### Limitations

Kullanıcı dili:

"Bu görünümün sınırları"

Evidence surface / profile analysis limitations sunar (ör. sparse profile,
eksik/erişilemeyen veri). Negatif geliştirici hükmüne dönüştürülmez; yeni
limitation semantiği icat edilmez.

### RepositoryDrillDown

Kullanıcı dili:

"Kanıt nerede?"

---

# 6. Deferred / Unsupported Boundaries

## RoleEvidenceAlignment

Durum:

Deferred.

P10 içinde aktif kullanıcı çıktısı değildir.

Kullanılmaz.

---

## ProblemAreaPatterns

Durum:

Unsupported.

P10 içinde kullanıcı çıktısı oluşturmaz.

---

# 7. P10 Implementation Slices

## P10-I1 — Profile Experience Entry Boundary

Amaç:

Kullanıcıdan GitHub kullanıcı adı alarak ProfilePipeline akışına bağlanmak.

Input:

```
GitHub Username
```

Output:

```
ProfileReport
```

Constraints:

Değiştirilmeyecek:

- ProfilePipeline
- ProfileComposer
- ProfileReportProjector
- Evidence generation logic

---

## P10-I2 — Profile Overview Presentation

Amaç:

ProfileReport başlangıç deneyimini oluşturmak.

Kullanıcı ilk olarak:

> GitHub'ınızda ne olduğunu değil, ne anlattığını görün.

fikrini anlamalıdır.

---

## P10-I3 — Evidence Exploration Experience

Amaç:

Kullanıcı:

```
Meaning

↓

Evidence
```

akışında ilerleyebilmelidir.

---

## P10-I4 — Presentation Refinement

Amaç:

Deneyimi sadeleştirmek.

İlkeler:

- minimum kullanıcı yükü
- progressive disclosure
- teknik karmaşıklığı gizleme
- kanıta ulaşmayı kolaylaştırma

---

# 8. Non Goals

P10 kapsamında yapılmaz:

- scoring sistemi
- ranking sistemi
- developer level
- seniority detection
- hiring recommendation
- AI generated judgement
- yeni evidence üretimi
- yeni pattern üretimi

---

# 9. Development Rules

Her P10 geliştirmesi:

```
Problem
↓
Boundary
↓
Implementation
↓
Validation
```

ile ilerler.

Claude Code görevlerinde:

Claude:

- verilen dosyalar dışında değişiklik yapmaz.
- yeni mimari karar vermez.
- milestone kapsamını genişletmez.
- mevcut contractları değiştirmez.

---

# 10. Validation Strategy

Her slice sonrası:

## Targeted Tests

İlgili davranış test edilir.

## Regression

Kontrol edilir:

- P8 pipeline değişmedi.
- ProfileReport contract değişmedi.
- Evidence sınırları korunuyor.

## Full Validation

Çalıştırılır:

- Domain tests
- Application tests
- Infrastructure tests
- Web tests
- Full build

---

# 11. Success Criteria

P10 başarılıdır:

Eğer kullanıcı:

1. GitHub profilinin ne anlattığını anlayabiliyorsa,
2. Önce nereye bakacağını biliyorsa,
3. İsterse gerçek evidence'a ulaşabiliyorsa,
4. Sistem tarafından yargılanmadan gözlemlenebilir izleri görebiliyorsa.

---

# 12. Next Step

P10-I1 için:

- mevcut Web boundary incelenir.
- gerekli dosyalar belirlenir.
- Claude görev promptu hazırlanır.

İlk implementasyon:

```
ProfileReport

↓

Web Presentation Boundary
```

ile başlayacaktır.
