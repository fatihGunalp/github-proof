# GitHub Proof — UI System v1

> **Evidence first. Noise last.**  
> **Few primitives. Strong semantics.**  
> **Az parça. Güçlü anlam.**

**Status:** Draft / Design Contract  
**Scope:** Web MVP + responsive foundation  
**Themes:** Light + Dark  
**Design direction:** Evidence-first developer tool, low visual noise, strong information hierarchy, progressive disclosure  

> **Amended:** 2026-08-20 / Session 095 — açık kullanıcı kararı. Landing artık
> tek birleşik GitHub girişi taşır (`GitHubInput`: username / profile URL /
> repository URL) ve trust şeridi dört kartlık "What GitHub Proof is not"
> yüzeyidir. Etkilenen bölümler: domain component listeleri
> (`RepositoryInput` → `GitHubInput`), §25 minimal home kompozisyonu, §40 build
> order. Profile Proof ve Repository Proof ayrı semantic akışlar olarak kalır;
> hiçbir token, tema veya status semantiği değişmemiştir. Kontrat sahibi:
> `docs/product/p10-web-profile-flow-implementation-plan.md` §13.0.

---

## 1. Amaç

Bu doküman GitHub Proof için kullanılacak UI sisteminin görsel, semantik ve etkileşimsel temelini tanımlar.

UI kit'in amacı yalnızca tutarlı ekranlar üretmek değildir. Sistem, GitHub Proof'un ürün gerçeğini görsel dile doğru çevirmelidir:

- analiz edilen şey geliştirici değil repository'dir,
- yalnızca public ve gözlemlenebilir evidence değerlendirilir,
- 15 deterministic signal bulunur,
- dört canonical kategori bulunur,
- evidence score ürünün merkezi değil ikincil özetidir,
- `Missing`, `UnavailableEvidence`, `NotApplicable` ve `RequiresHumanReview` birbirinden farklı anlam taşır,
- raporun ilk katmanı en fazla üç güçlü kanıt, üç kritik boşluk ve üç öncelikli aksiyondur,
- UI kesinlik üretmemeli; sistemin emin olmadığı durumları açıkça göstermelidir.

Bu nedenle GitHub Proof UI System bir “SaaS dashboard kit” değildir.

Bu sistem:

> **karmaşık repository evidence'ını azaltmadan; doğru sırada, doğru anda ve mümkün olan en az görsel gürültüyle anlaşılır hale getirmek için tasarlanır.**

---

# 2. Tasarım Anayasası

Her UI kararı aşağıdaki sırayla değerlendirilir:

1. Bu gerekli mi?
2. Burada mı olmalı?
3. Şimdi mi görünmeli?
4. Kullanıcı bunu tarayarak anlayabiliyor mu?
5. Anlam yalnız renk üzerinden mi taşınıyor?
6. Evidence ile sonuç arasındaki bağ korunuyor mu?
7. UI gereksiz kesinlik üretiyor mu?
8. Ancak bundan sonra: nasıl görünmeli?

---

## 2.1 Ana İlkeler

### Evidence first

Kullanıcı önce sonucu değil, sonucu destekleyen anlamı görür.

Önerilen hiyerarşi:

```text
Evidence
↓
Meaning
↓
Action
↓
Score
```

Kaçınılacak hiyerarşi:

```text
72 / 100
↓
her şey
```

---

### Noise last

Görsel öğe eklemek varsayılan değildir.

Varsayılan ayırma araçları:

- typography,
- whitespace,
- alignment,
- divider,
- subtle surface change.

Card yalnız bağımsız yüzey gerektiğinde kullanılır.

---

### Few primitives, strong semantics

UI kit mümkün olduğunca az primitive kullanır.

Ama domain component'leri güçlü ve anlamlıdır:

```text
GitHubInput
RepositoryResult
ProofItem
GapItem
PriorityAction
SignalRow
SignalDetail
EvidenceBlock
StatusBadge
UncertaintyNotice
CategorySummary
EvidenceScore
```

---

### Progressive disclosure

İlk bakışta her şey gösterilmez.

Katmanlar:

```text
Layer 1
3 proof + 3 gap + 3 action

Layer 2
Category summary + secondary score

Layer 3
15 signals

Layer 4
Evidence + limitation + version + methodology
```

---

### One screen, one primary question

Her ekran tek bir ana soruyu cevaplamalıdır.

| Ekran | Ana soru |
|---|---|
| Home | Hangi repository incelenecek? |
| Repository picker | Hangi public repository? |
| Analyzing | Şu anda ne oluyor? |
| Report | Bu repository neyi kanıtlıyor ve önce neyi düzeltmeliyim? |
| Signal detail | Bu sonuca neden vardık? |
| Methodology | Bu sistem hangi kurallarla karar veriyor? |

---

# 3. Theme Architecture

Light ve Dark birbirinden bağımsız iki tasarım değildir.

Aynı semantic token seti iki farklı mode değerine bağlanır.

```text
Primitive colors
      ↓
Semantic colors
      ↓
Component tokens
      ↓
Light / Dark mode
```

Component hiçbir zaman doğrudan primitive renge bağımlı olmamalıdır.

Yanlış:

```css
color: #111827;
background: #ffffff;
```

Doğru:

```css
color: var(--fg-primary);
background: var(--surface-default);
```

---

# 4. Color Philosophy

GitHub Proof'un renk sistemi üç gruptan oluşur:

1. **Neutral system** — UI'ın büyük çoğunluğu
2. **Interaction accent** — primary action / focus
3. **Semantic states** — evidence status ve uncertainty

Renk bir dekorasyon unsuru değildir.

Ana UI mümkün olduğunca nötr kalır.

---

# 5. Primitive Palette

Primitive değerler component'lerde doğrudan kullanılmaz.

## 5.1 Neutral

```text
neutral-000   #FFFFFF
neutral-025   #FCFCFD
neutral-050   #F8F9FB
neutral-100   #F1F3F5
neutral-150   #E9ECF0
neutral-200   #DEE2E7
neutral-300   #C8CED6
neutral-400   #9DA6B2
neutral-500   #6F7885
neutral-600   #505965
neutral-700   #353C45
neutral-800   #23282F
neutral-850   #1B1F25
neutral-900   #14171C
neutral-925   #101318
neutral-950   #0B0D11
neutral-975   #07090C
```

## 5.2 Accent Blue

```text
blue-050   #EFF5FF
blue-100   #DCE9FF
blue-200   #B9D2FF
blue-300   #8AB3FF
blue-400   #5A92FF
blue-500   #3478F6
blue-600   #2563EB
blue-700   #1D4ED8
blue-800   #1E40AF
blue-900   #1E3A8A
```

## 5.3 Positive / Strong

```text
green-050   #ECFDF3
green-100   #D1FADF
green-300   #6CE9A6
green-500   #12B76A
green-600   #039855
green-700   #027A48
```

## 5.4 Partial / Attention

```text
amber-050   #FFFAEB
amber-100   #FEF0C7
amber-300   #FEC84B
amber-500   #F79009
amber-600   #DC6803
amber-700   #B54708
```

## 5.5 Missing / Destructive

```text
red-050   #FEF3F2
red-100   #FEE4E2
red-300   #FDA29B
red-500   #F04438
red-600   #D92D20
red-700   #B42318
```

## 5.6 Human Review

```text
violet-050   #F5F3FF
violet-100   #EDE9FE
violet-300   #C4B5FD
violet-500   #8B5CF6
violet-600   #7C3AED
violet-700   #6D28D9
```

---

# 6. Semantic Color Tokens

## 6.1 Light Mode

### Backgrounds

```text
--bg-canvas                neutral-025
--bg-subtle                neutral-050
--bg-muted                 neutral-100

--surface-default          neutral-000
--surface-subtle           neutral-050
--surface-raised           neutral-000
--surface-hover            neutral-050
--surface-selected         blue-050
--surface-disabled         neutral-100
```

### Foregrounds

```text
--fg-primary               neutral-925
--fg-secondary             neutral-600
--fg-tertiary              neutral-500
--fg-muted                 neutral-400
--fg-disabled              neutral-400
--fg-inverse               neutral-000
--fg-link                  blue-600
--fg-link-hover            blue-700
```

### Borders

```text
--border-subtle            neutral-150
--border-default           neutral-200
--border-strong            neutral-300
--border-focus             blue-500
--border-disabled          neutral-150
```

### Accent

```text
--accent-primary           blue-600
--accent-primary-hover     blue-700
--accent-primary-active    blue-800
--accent-soft              blue-050
```

---

## 6.2 Dark Mode

Dark mode saf siyah üzerine beyaz değildir.

Amaç:

- kontrastı korumak,
- göz yorgunluğunu azaltmak,
- status renklerinin bağırmasını engellemek,
- yüzeyleri yalnız ton farkıyla ayırmak.

### Backgrounds

```text
--bg-canvas                neutral-975
--bg-subtle                neutral-950
--bg-muted                 neutral-925

--surface-default          neutral-950
--surface-subtle           neutral-925
--surface-raised           neutral-900
--surface-hover            neutral-900
--surface-selected         rgba(37, 99, 235, 0.14)
--surface-disabled         neutral-925
```

### Foregrounds

```text
--fg-primary               neutral-050
--fg-secondary             neutral-300
--fg-tertiary              neutral-400
--fg-muted                 neutral-500
--fg-disabled              neutral-600
--fg-inverse               neutral-950
--fg-link                  blue-300
--fg-link-hover            blue-200
```

### Borders

```text
--border-subtle            neutral-900
--border-default           neutral-850
--border-strong            neutral-700
--border-focus             blue-400
--border-disabled          neutral-900
```

### Accent

```text
--accent-primary           blue-400
--accent-primary-hover     blue-300
--accent-primary-active    blue-200
--accent-soft              rgba(52, 120, 246, 0.12)
```

---

# 7. Semantic Evidence States

Status renklerinin amacı sonucu hızla taranabilir hale getirmektir.

Renk hiçbir zaman tek başına anlam taşımaz.

Her status en az şu üçünden ikisini kullanmalıdır:

- label,
- icon,
- color.

---

## 7.1 Strong

**Meaning:** Güçlü ve açık evidence.

### Light

```text
foreground   green-700
background   green-050
border       green-100
icon         green-600
```

### Dark

```text
foreground   #6CE9A6
background   rgba(18, 183, 106, 0.10)
border       rgba(18, 183, 106, 0.22)
icon         #32D583
```

---

## 7.2 Present

Present, Strong ile aynı yeşil ailesini kullanabilir ancak görsel yoğunluğu daha düşük olmalıdır.

### Light

```text
foreground   green-700
background   transparent / green-050 optional
icon         green-500
```

### Dark

```text
foreground   #75E0A7
background   transparent
icon         #32D583
```

---

## 7.3 Partial

### Light

```text
foreground   amber-700
background   amber-050
border       amber-100
icon         amber-500
```

### Dark

```text
foreground   #FEC84B
background   rgba(247, 144, 9, 0.10)
border       rgba(247, 144, 9, 0.22)
icon         #FDB022
```

---

## 7.4 Missing

### Light

```text
foreground   red-700
background   red-050
border       red-100
icon         red-500
```

### Dark

```text
foreground   #FDA29B
background   rgba(240, 68, 56, 0.10)
border       rgba(240, 68, 56, 0.24)
icon         #F97066
```

**Important:** Missing yalnız gerekli data başarıyla incelenmiş ancak evidence bulunamamışsa kullanılır.

---

## 7.5 Not Applicable

Bu durum hata değildir.

### Light

```text
foreground   neutral-600
background   neutral-100
border       neutral-150
icon         neutral-500
```

### Dark

```text
foreground   neutral-300
background   neutral-925
border       neutral-850
icon         neutral-400
```

---

## 7.6 Unavailable Evidence

Bu durum `Missing` değildir.

Kullanıcıya “bulamadık” değil, “erişemedik / değerlendiremedik” anlatılmalıdır.

### Light

```text
foreground   blue-700
background   blue-050
border       blue-100
icon         blue-500
```

### Dark

```text
foreground   blue-200
background   rgba(52, 120, 246, 0.10)
border       rgba(52, 120, 246, 0.22)
icon         blue-300
```

---

## 7.7 Requires Human Review

### Light

```text
foreground   violet-700
background   violet-050
border       violet-100
icon         violet-500
```

### Dark

```text
foreground   #C4B5FD
background   rgba(139, 92, 246, 0.11)
border       rgba(139, 92, 246, 0.24)
icon         #A78BFA
```

---

# 8. Typography

UI'ın kişiliği büyük ölçüde typography üzerinden kurulmalıdır.

Önerilen ana font:

```text
Inter
```

Neden:

- developer tooling bağlamında doğal,
- açık kaynak,
- yüksek okunabilirlik,
- küçük boyutlarda güçlü,
- UI/data yoğunluğu için uygun.

Mono kullanımında:

```text
ui-monospace,
SFMono-Regular,
Cascadia Code,
Roboto Mono,
monospace
```

kullanılabilir.

---

## 8.1 Type Scale

```text
Display
48 / 52 / 600

H1
36 / 42 / 600

H2
28 / 34 / 600

H3
22 / 28 / 600

Title
18 / 26 / 600

Body Large
16 / 26 / 400

Body
14 / 22 / 400

Body Strong
14 / 22 / 550

Small
13 / 20 / 400

Label
12 / 16 / 550

Caption
11 / 16 / 450

Mono
12 / 18 / 450
```

---

## 8.2 Typography Rules

- Heading sayısını azalt.
- Uppercase yalnız küçük section label'larında kullanılabilir.
- Uzun uppercase başlık kullanma.
- Score ve sayı için tabular numerals tercih et.
- Evidence paths/code fragments mono olabilir.
- Body text maksimum okunabilir satır uzunluğu yaklaşık 60–75 karakter.
- Bir component içinde üçten fazla text weight kullanma.

---

# 9. Spacing System

Base unit:

```text
4px
```

Scale:

```text
space-0    0
space-1    4
space-2    8
space-3    12
space-4    16
space-5    20
space-6    24
space-8    32
space-10   40
space-12   48
space-16   64
space-20   80
space-24   96
```

---

## 9.1 Density Modes

GitHub Proof iki ana density context kullanır.

### Comfortable

Home, report summary ve detail reading.

```text
row height      44–48
section gap     48–64
card padding    20–24
```

### Compact

Signal list, repository picker, metadata.

```text
row height      36–40
section gap     24–32
row padding     8–12
```

Aynı ekranda gereksiz density karışımı yapılmamalıdır.

---

# 10. Radius

GitHub Proof aşırı yuvarlatılmış SaaS estetiğinden kaçınır.

```text
radius-xs     4
radius-sm     6
radius-md     8
radius-lg     12
radius-xl     16
radius-full   999
```

Kullanım:

```text
Button              6–8
Input               8
Status badge        6
Surface             10–12
Dialog              12–16
Pill / tag           full only when semantically pill
```

---

# 11. Borders

Border sistemin temel yapı araçlarından biridir.

Shadow yerine önce border kullan.

### Light

```text
Default      1px neutral-200
Subtle       1px neutral-150
Strong       1px neutral-300
```

### Dark

```text
Default      1px neutral-850
Subtle       1px neutral-900
Strong       1px neutral-700
```

---

# 12. Elevation

Elevation minimum kullanılmalıdır.

```text
Level 0
No shadow

Level 1
Dropdown / popover / floating selector

Level 2
Dialog only
```

### Light Level 1

```text
0 8px 24px rgba(16, 24, 40, 0.08)
0 2px 6px rgba(16, 24, 40, 0.04)
```

### Dark Level 1

```text
0 10px 28px rgba(0, 0, 0, 0.34)
```

---

# 13. Motion

Motion dekoratif değildir.

Amaç:

- state change açıklamak,
- spatial continuity sağlamak,
- progress göstermek.

Scale:

```text
fast       100ms
standard   160ms
slow       240ms
```

Easing:

```text
standard   cubic-bezier(0.2, 0, 0, 1)
exit       cubic-bezier(0.4, 0, 1, 1)
```

Kaçınılacaklar:

- large bounce,
- fake AI sparkle,
- endless gradient animations,
- decorative loading theatre.

---

# 14. Focus System

Keyboard focus her temada açık olmalıdır.

### Light

```text
outline: 2px solid blue-500
outline-offset: 2px
```

### Dark

```text
outline: 2px solid blue-300
outline-offset: 2px
```

Focus state yalnız border rengi değişimiyle bırakılmamalıdır.

---

# 15. Iconography

Iconlar:

- outline ağırlıklı,
- 1.5–2 px stroke,
- mümkün olduğunca 16 / 18 / 20 px,
- dekoratif değil açıklayıcı,
- aynı ekranda birden fazla ikon dili kullanılmaz.

Önerilen genel karakter:

```text
Lucide-like geometric outline
```

Ama icon set birebir marka kimliği değildir.

---

# 16. Primitive Components

## 16.1 Text

Roles:

```text
Primary
Secondary
Muted
Link
Code
Status
```

---

## 16.2 Divider

Varsayılan section ayrımı için temel araç.

```text
horizontal
vertical
inset
```

---

## 16.3 Surface

Variants:

```text
Default
Subtle
Raised
Interactive
Selected
```

Surface = Card değildir.

Surface yalnız zemin katmanıdır.

---

# 17. Button

Variants:

```text
Primary
Secondary
Ghost
Destructive
Icon
```

Sizes:

```text
sm  32px
md  36px
lg  40px
```

Primary CTA aynı viewport içinde mümkün olduğunca bir adettir.

---

## 17.1 Light Primary Button

```text
bg            blue-600
fg            white
hover         blue-700
active        blue-800
focus         blue-500
```

## 17.2 Dark Primary Button

```text
bg            blue-400
fg            neutral-975
hover         blue-300
active        blue-200
focus         blue-300
```

Dark mode'da primary CTA ışıklı neon yüzeye dönüşmemelidir.

---

# 18. Input

Base anatomy:

```text
Prefix / icon
Value
Placeholder
Clear
Suffix action
Validation message
```

States:

```text
Default
Hover
Focus
Filled
Invalid
Disabled
Loading
```

Repository input domain-specific component'tir; generic text input görünümü kullanabilir ancak davranışı domain contract'ına bağlıdır.

---

# 19. Status Badge

Status badge kompakt semantic label'dır.

Örnek:

```text
● Strong
● Partial
● Missing
```

Badge:

- uppercase olmak zorunda değil,
- çok doygun arka plan kullanmaz,
- text + icon/dot kullanır,
- uzun cümle taşımaz.

---

# 20. Evidence Chip

Evidence kaynağı için küçük referans component'i.

Örnek:

```text
README.md
.github/workflows/ci.yml
Commits
Releases
```

Davranış:

- literal repository evidence çevrilmez,
- code/path hissi taşıyabilir,
- mümkünse tıklanabilir kaynak affordance gösterir,
- çok sayıda evidence varsa `+2` gibi disclosure kullanılabilir.

---

# 21. Domain Components

# 21.1 Repository Input

Amaç:

> Analiz edilecek repository'yi mümkün olan en az sürtünmeyle almak.

Anatomy:

```text
GitHub icon
Input
Arrow / Continue
Helper state
```

Placeholder:

```text
GitHub kullanıcı adı veya repository URL'si
```

Not:

Username → repository discovery akışı uygulanırsa bu, mevcut single-repository analysis contract'ını değiştirmez; yalnız giriş katmanıdır.

---

# 21.2 Repository Result

Repository picker satırı.

Anatomy:

```text
Repository name
Primary language
Visibility
Fork/archive indicators
Updated metadata
Action arrow
```

Avoid:

- stars/forks'u kalite metriği gibi öne çıkarmak,
- gereksiz avatar yüzeyi,
- büyük repository cards.

---

# 21.3 Proof Item

Amaç:

> Repository'nin görünür biçimde neyi kanıtladığını göstermek.

Anatomy:

```text
Status icon
Title
One-line meaning
Optional evidence reference
```

Örnek:

```text
✓ Test ve CI görünür
  Otomatik doğrulama yapısı gözlemlendi.
```

ProofItem büyük başarı kartına dönüşmemelidir.

---

# 21.4 Gap Item

Amaç:

> Evidence'ı zayıflatan `Missing` veya `Partial` sonucu açıklamak.

Anatomy:

```text
Status
Title
Observed state
Why it matters
Optional evidence reference
```

Uncertainty durumları Gap olarak sunulmaz.

---

# 21.5 Priority Action

Amaç:

> Kullanıcıya ilk yapması gereken somut işi göstermek.

Anatomy:

```text
Priority number
Action title
Reason
Effort
Related signal optional
```

Örnek:

```text
01
Gerçek bir kullanım örneği ekle
Low effort
```

Priority number güçlü, dekorasyon zayıf olmalıdır.

---

# 21.6 Signal Row

Compact list component.

Anatomy:

```text
Signal title
Signal key optional
Status
Score when available
Disclosure arrow
```

Rules:

- `Score: null` ise `—` kullanılabilir ama tooltip/context açık olmalıdır.
- score, status'tan daha baskın olmamalıdır.
- row hover subtle olmalıdır.

---

# 21.7 Signal Detail

Bu component GitHub Proof'un en önemli açıklanabilirlik yüzeylerinden biridir.

Anatomy:

```text
Signal title
Status
Score

Observed evidence
Why it matters
Recommended action
Evidence
Limitations
Version / evaluated time
```

Order değişmemelidir.

---

# 21.8 Uncertainty Notice

Variants:

```text
NotApplicable
UnavailableEvidence
RequiresHumanReview
```

Bu component error alert gibi davranmaz.

Örnek:

```text
Evidence unavailable
GitHub API rate limit nedeniyle commit geçmişinin tamamına erişilemedi.
Bu durum evidence'ın eksik olduğu anlamına gelmez.
```

---

# 21.9 Category Summary

Canonical kategoriler:

```text
Documentation & Context
Development Process
Validation
Portfolio Presentation
```

UI category label'ları presentation layer'da yerelleştirilebilir.

Category component:

```text
Name
Normalized score if available
Status
Optional tiny progress visualization
```

Avoid:

- radar chart,
- pie chart,
- giant gauge,
- category yarışması hissi.

---

# 21.10 Evidence Score

Overall score secondary diagnostic summary'dir.

Önerilen anatomy:

```text
Visible evidence
72 / 100
Secondary diagnostic summary
```

Avoid:

```text
巨大 circular gauge
A+ grade
Developer score
Hireability score
```

---

# 22. Report Composition

Canonical first-reading order:

```text
Repository identity
↓
What this repository proves
↓
What weakens the evidence
↓
Do these first
↓
Explicit uncertainty
↓
Category summary + secondary score
↓
Signal detail
↓
Limitations / methodology / disclaimer
```

Bu sıra hem Light hem Dark için aynıdır.

Theme information hierarchy'yi değiştirmez.

---

# 23. Light Report Character

Light mode karakteri:

```text
bright
quiet
precise
editorial
technical
```

Görsel yaklaşım:

- canvas very light gray / off-white,
- default surfaces white,
- border-based grouping,
- accent yalnız CTA / active state,
- status colors muted.

Light mode “kurumsal beyaz dashboard” gibi görünmemelidir.

---

# 24. Dark Report Character

Dark mode karakteri:

```text
calm
focused
low-glare
high-legibility
technical
```

Dark mode:

- pure #000 kullanmaz,
- her surface'i ayrı gri kutuya dönüştürmez,
- neon status kullanmaz,
- mavi accent'i aşırı parlak kullanmaz.

Amaç terminal estetiği değildir.

---

# 25. Home Composition

Minimal home önerisi:

```text
GitHub Proof                                          [theme]


              See what your GitHub communicates.

              Public evidence. Clear direction. No guesses.

      [ GitHub username, profile URL, or repository URL   ]

                        [ Analyze ]

  ──────────────────────────────────────────────────────────

                  What GitHub Proof is not

   No developer   No seniority   No AI authorship   No hire/no-hire
     ranking        judgment         claims             verdict
```

Light ve Dark aynı layout'u kullanır.

Kilitli noktalar (Session 095):

- **Tek `GitHubInput`.** Landing'de yalnız bir giriş alanı vardır; bare
  GitHub login, canonical profile URL ve canonical repository URL kabul
  eder. Ayrı bir profile giriş component'i yoktur.
- **Giriş birleşik, analiz değil.** Profile Proof ve Repository Proof ayrı
  semantic akışlardır; hangisinin çalışacağı gönderilen değerin biçiminden
  deterministik olarak seçilir, tahmin edilmez. Kabul edilmeyen biçim aynı
  giriş yüzeyinde validation error olarak kalır.
- **Boundary şeridi dört kartlık "What GitHub Proof is not" yüzeyidir.**
  Eski tek satırlık `Public data · 15 deterministic signals · No AI
  judgment` şeridinin yerini alır. Dört madde birer decorative line icon
  taşır (`currentColor`, mevcut `ui-icon` sistemi, inline SVG) — ikonlar
  ikincildir, sözleşme metindedir.
- Alan `type="text"`tir: `type="url"` bare login'i tarayıcı seviyesinde
  reddederdi.

---

# 26. Repository Picker Composition

```text
fatihGunalp

Public repositories

 github-proof                 C#             →
 Localora                     C#             →
 softbreak-ai-lab             Jupyter        →

Show all repositories
```

Card stack yerine list-first yaklaşım tercih edilir.

---

# 27. Analysis Progress

Gerçek pipeline durumları kullanılır.

```text
Repository found
Collecting evidence
Evaluating signals
Building report
```

Sahte AI narration yoktur.

Önerilen pattern:

```text
● Repository found
● Collecting evidence
◉ Evaluating 15 signals
○ Building report
```

---

# 28. Empty / Error / Partial States

States:

```text
Invalid repository URL
Repository not found
Private repository
Rate limited
Insufficient data
Partial analysis
Archived repository
Fork repository
Template repository
```

Partial analysis error page değildir.

Rapor mevcut evidence ile gösterilir ve uncertainty ayrıca belirtilir.

---

# 29. Accessibility

Minimum hedef:

```text
WCAG 2.2 AA
```

Kurallar:

- normal text 4.5:1,
- large text 3:1,
- focus görünür,
- semantic status yalnız renk ile anlatılmaz,
- target minimum 40x40 önerilir,
- keyboard navigation desteklenir,
- prefers-reduced-motion desteklenir,
- dark mode contrast ayrıca test edilir.

---

# 30. Theme Switching

Modes:

```text
System
Light
Dark
```

Varsayılan:

```text
System
```

Theme switch UI kritik CTA değildir.

Header veya preferences alanında sessiz bir kontrol olmalıdır.

---

# 31. CSS Variable Contract

Örnek semantic implementation:

```css
:root {
  color-scheme: light;

  --bg-canvas: #FCFCFD;
  --bg-subtle: #F8F9FB;

  --surface-default: #FFFFFF;
  --surface-subtle: #F8F9FB;
  --surface-hover: #F8F9FB;

  --fg-primary: #101318;
  --fg-secondary: #505965;
  --fg-tertiary: #6F7885;
  --fg-muted: #9DA6B2;

  --border-subtle: #E9ECF0;
  --border-default: #DEE2E7;
  --border-strong: #C8CED6;

  --accent-primary: #2563EB;
  --accent-hover: #1D4ED8;
  --accent-soft: #EFF5FF;
}

[data-theme="dark"] {
  color-scheme: dark;

  --bg-canvas: #07090C;
  --bg-subtle: #0B0D11;

  --surface-default: #0B0D11;
  --surface-subtle: #101318;
  --surface-hover: #14171C;

  --fg-primary: #F8F9FB;
  --fg-secondary: #C8CED6;
  --fg-tertiary: #9DA6B2;
  --fg-muted: #6F7885;

  --border-subtle: #14171C;
  --border-default: #1B1F25;
  --border-strong: #353C45;

  --accent-primary: #5A92FF;
  --accent-hover: #8AB3FF;
  --accent-soft: rgba(52, 120, 246, 0.12);
}
```

---

# 32. Semantic State CSS Contract

```css
.status-strong {
  --status-fg: var(--status-strong-fg);
  --status-bg: var(--status-strong-bg);
  --status-border: var(--status-strong-border);
}

.status-present { ... }
.status-partial { ... }
.status-missing { ... }
.status-not-applicable { ... }
.status-unavailable { ... }
.status-human-review { ... }
```

Component kendi içinde doğrudan `green`, `red`, `violet` seçmemelidir.

---

# 33. Design Token Naming

Önerilen pattern:

```text
color.bg.canvas
color.bg.subtle

color.surface.default
color.surface.hover

color.fg.primary
color.fg.secondary

color.border.default
color.border.focus

color.accent.primary

color.status.strong.fg
color.status.strong.bg
color.status.strong.border

space.1
space.2
...

radius.sm
radius.md

font.body
font.label
```

---

# 34. Component Naming

Generic:

```text
Button
Input
Divider
Surface
Badge
Tooltip
Dialog
```

Domain:

```text
GitHubInput
RepositoryPicker
RepositoryIdentity
ProofItem
GapItem
PriorityAction
SignalRow
SignalDetail
EvidenceChip
EvidenceBlock
UncertaintyNotice
CategorySummary
EvidenceScore
AnalysisProgress
```

---

# 35. Light/Dark Equivalence Rule

Her component her iki temada:

- aynı hierarchy,
- aynı spacing,
- aynı state semantics,
- aynı interaction,
- aynı information density

taşımalıdır.

Dark mode yeni bir visual personality değildir.

Yalnız contrast ve luminance adaptasyonudur.

---

# 36. Anti-Patterns

GitHub Proof UI System içinde kaçınılacaklar:

- her şeyi card içine koymak,
- giant score gauge,
- glassmorphism,
- excessive blur,
- neon dark mode,
- gradient CTA spam,
- fake AI animation,
- confetti,
- traffic-light-only semantics,
- purple-everywhere AI product aesthetic,
- terminal cosplay,
- developer seniority izlenimi veren rank/grade,
- red = unavailable varsayımı,
- dozens of icon colors,
- inconsistent radii,
- typography yerine border ile hierarchy kurmak,
- dense table'ı mobile'a aynen sıkıştırmak.

---

# 37. UI Review Checklist

Her yeni component/ekran için:

```text
[ ] Bu component gerçekten gerekli mi?
[ ] Generic primitive ile çözülebilir mi?
[ ] Domain anlamı doğru mu?
[ ] Evidence-first hierarchy korunuyor mu?
[ ] Score gereğinden fazla görünür mü?
[ ] Missing ile unavailable ayrılıyor mu?
[ ] Status yalnız renge mi bağlı?
[ ] Light contrast yeterli mi?
[ ] Dark contrast yeterli mi?
[ ] Keyboard focus görünür mü?
[ ] Compact ve comfortable density doğru yerde mi?
[ ] Gereksiz card/surface var mı?
[ ] Gereksiz shadow var mı?
[ ] Aynı bilgi iki kez gösteriliyor mu?
[ ] Progressive disclosure kullanılabilir mi?
[ ] TR/EN metin uzaması layout'u bozar mı?
```

---

# 38. First 10 Seconds Test

Report açıldıktan sonraki ilk 10 saniyede kullanıcı şu sorulara cevap verebilmelidir:

1. Bu repository neyi güçlü biçimde kanıtlıyor?
2. En önemli evidence boşluğu nedir?
3. İlk yapmam gereken şey nedir?
4. Score'un geliştirici puanı olmadığını anlayabiliyor muyum?
5. Missing ile unavailable farkını ayırt edebiliyor muyum?

Bu test başarısızsa ekran görsel olarak iyi olsa bile tasarım başarısız kabul edilir.

---

# 39. UI Kit v1 Deliverables

İlk UI kit aşağıdaki asset'leri içermelidir.

## Foundations

```text
Light palette
Dark palette
Semantic tokens
Typography
Spacing
Radius
Border
Elevation
Motion
Focus
Icon rules
```

## Generic Components

```text
Button
IconButton
Input
SearchInput
Select
Badge
Tooltip
Divider
Surface
Dialog
Progress
Skeleton
```

## Domain Components

```text
GitHubInput
RepositoryPicker
RepositoryIdentity
ProofItem
GapItem
PriorityAction
SignalRow
SignalDetail
EvidenceChip
EvidenceBlock
StatusBadge
UncertaintyNotice
CategorySummary
EvidenceScore
AnalysisProgress
```

## Compositions

```text
Home
Repository Picker
Analyzing
Report Summary
Signal List
Signal Detail
Uncertainty Section
Error / Partial States
```

---

# 40. Suggested Build Order

```text
01 Semantic color tokens — Light
02 Semantic color tokens — Dark
03 Type scale
04 Spacing / radius / borders
05 Status semantics
06 Button / Input
07 GitHubInput
08 RepositoryPicker
09 Proof / Gap / Action
10 SignalRow
11 SignalDetail
12 UncertaintyNotice
13 CategorySummary
14 EvidenceScore
15 Report composition
16 Home
17 Analysis states
18 Error states
19 Mobile
20 Accessibility audit
```

---

# 41. Canonical UI Motto

Ana ürün ilkesi:

> **Evidence first. Noise last.**

UI system ilkesi:

> **Few primitives. Strong semantics.**

Türkçesi:

> **Kanıt önce. Gürültü sonra.**  
> **Az parça. Güçlü anlam.**

---

# 42. Final Decision Rule

İki tasarım arasında kararsız kalındığında daha modern görünen değil, aşağıdakileri daha iyi yapan seçilir:

1. Evidence'ı daha hızlı okutuyor mu?
2. Anlamı daha az UI elemanıyla taşıyor mu?
3. Status semantiğini daha doğru koruyor mu?
4. Light ve Dark'ta aynı derecede güvenilir mi?
5. Kullanıcıyı daha az düşündürüyor mu?
6. Developer-tool ciddiyetini koruyor mu?
7. Kodda aynı semantic token sistemiyle uygulanabiliyor mu?

Son kriter:

> **Interface görünmek için değil, evidence'ı görünür kılmak için vardır.**

