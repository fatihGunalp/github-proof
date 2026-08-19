# GitHub Proof — Worklog

## Amaç

`worklog/`, GitHub Proof geliştirmesinde AI ve geliştiricinin en son nerede kaldığını minimum bağlamla bulması için kullanılan operasyonel geliştirme hafızasıdır.

Bu klasör:

~~~text
ürün gerçeği değildir
domain sözleşmesi değildir
mimari karar kaynağı değildir
~~~

Kalıcı kararların source of truth'u `docs/` altındadır.

---

## Bilgi Katmanları

Çelişki durumunda öncelik:

~~~text
1. Kullanıcının mevcut açık talimatı
2. docs/
3. AGENTS.md
4. agents/
5. worklog/
6. mevcut koddan yapılan çıkarım
~~~

Katmanların amacı:

~~~text
docs/
= ne doğru?
= ürün/domain/mimari sözleşmesi

AGENTS.md
= Codex repository giriş kuralları
= model gate ve üst seviye çalışma disiplini

agents/
= görev tipine göre nasıl çalışılmalı?
= routing / context / development / review playbook'ları

worklog/
= en son ne yapıldı?
= şu anda aktif iş var mı?
= nerede kalındı?
= sıradaki somut aksiyon ne?
~~~

`worklog/`, `docs/` veya `AGENTS.md` kararlarını override edemez.

---

## Foreign Project State Kuralı

Bu worklog yalnız GitHub Proof'un doğrulanmış current state'ini taşır.

Başka bir project'ten gelen:

~~~text
domain invariant
business rule
database/runtime state
deployment boundary
session history
active task
validation sonucu
~~~

GitHub Proof worklog'una live state olarak taşınmaz.

Başka project worklog'larından yalnız genel operational memory pattern'i yeniden kullanılabilir.

---

## Yapı

~~~text
worklog/
├── README.md
├── CURRENT.md
├── INDEX.md
└── sessions/
    ├── TEMPLATE.md
    └── YYYY-MM-DD-NNN-short-topic.md
~~~

---

# Temel Hafıza Modeli

~~~text
CURRENT.md
= güncel snapshot

Active Session
= şu anda yürüyen meaningful işin canlı handoff kaydı

sessions/
= tamamlanan veya yarım kalan meaningful development geçmişi

INDEX.md
= session kataloğu
~~~

Kritik kural:

~~~text
Session log iş bittikten sonra geriye dönük uydurulmaz.

Meaningful development işi gerçekten başlarken oluşturulur.
~~~

---

# Model Gate ile Worklog İlişkisi

Model routing'in source of truth'u:

~~~text
AGENTS.md
agents/01-model-routing.md
~~~

Yeni bağımsız görevde sıra:

~~~text
Kullanıcı görevi verir
        ↓
Codex görevi uygulatmadan zorluk/risk analizi yapar
        ↓
Model + reasoning önerir
        ↓
"Modeli <MODEL> / <EFFORT> olarak ayarla.
 Devam etmek için: devam et"
        ↓
DUR
        ↓
Kullanıcı modeli değiştirir
        ↓
Kullanıcı: devam et
        ↓
Execution başlar
        ↓
Git baseline
        ↓
CURRENT
        ↓
Active Session kontrolü
        ↓
Meaningful işse session oluştur
        ↓
İlgili agents/ + gerekli docs/
        ↓
code / test / review
~~~

Model gate beklenirken worklog session açılmaz ve repository değiştirilmez.

`devam et` semantiği:

~~~text
Pending model gate varsa:
devam et = model seçimi onaylandı, görevi uygula

Pending model gate yok + Active Session varsa:
devam et = mevcut session'a devam et

İkisi de yoksa:
yeni meaningful görev için model gate gerekir
~~~

---

# CURRENT.md

`CURRENT.md`, GitHub Proof'un güncel geliştirme snapshot'ıdır.

Execution başladıktan sonra ilk okunacak operasyonel dosyadır.

Beklenen alanlar:

~~~text
Current Phase
Active Session
Model Gate State
Current Task
In Progress
Current Problem
Next Immediate Action
Working Set
Repository State
Last Completed Work
Last Validation
Stable Decisions
Critical Invariants
Known Problems
Open Issues
Do Not Reinvestigate
~~~

Aktif çalışma varsa en az:

~~~text
Active Session
Current Task
Next Immediate Action
~~~

güncel tutulmalıdır.

---

## Active Session

Aktif meaningful development işi varsa:

~~~text
worklog/sessions/YYYY-MM-DD-NNN-short-topic.md
~~~

dosyasının yolu `CURRENT.md` içinde tutulur.

Aktif session yoksa:

~~~text
Active Session:
NONE
~~~

kullanılır.

`IN_PROGRESS` session varken aynı iş için yeni session açılmaz.

---

## Model Gate State

Worklog model seçiminin source of truth'u değildir.

Ancak handoff için operasyonel state tutulabilir:

~~~text
Model Gate State:
NONE
~~~

veya active session sırasında:

~~~text
Model Gate State:
APPROVED — Terra / medium
~~~

Model escalation olursa session dosyasında ayrıca kaydedilir.

---

## Current Task

Tek ana işi söyler.

Tercih:

~~~text
Implement SignalResult invariants and unit tests.
~~~

Kaçın:

~~~text
projeye devam et
backend geliştir
birkaç şey düzelt
~~~

---

## In Progress

Yalnız meaningful ilerleme.

Örnek:

~~~text
- SignalStatus enum oluşturuldu.
- SignalResult constructor invariant'ları eklendi.
- Null score testlerinden biri henüz fail.
~~~

Terminal geçmişi veya full diff koyma.

---

## Current Problem

Aktif işi etkileyen gerçek problem varsa yaz.

Yoksa:

~~~text
None.
~~~

---

## Next Immediate Action

Bir sonraki somut adım.

Örnek:

~~~text
RequiresHumanReview için Score=null invariant testini düzelt ve targeted test'i tekrar çalıştır.
~~~

---

## Working Set

Hızlı orientation için kısa özet.

Örnek:

~~~text
Modified:
- src/GitHubProof.Domain/Signals/SignalResult.cs
- tests/GitHubProof.UnitTests/Signals/SignalResultTests.cs
~~~

Gerçek repository state için her zaman Git kullanılır.

---

# Session Lifecycle

## Yeni Meaningful Session

Model gate onaylandıktan sonra:

~~~text
1. git status --short
2. git branch --show-current
3. git rev-parse --short HEAD
4. pre-existing working tree snapshot al
5. sessions/TEMPLATE.md kopyala
6. Result = IN_PROGRESS
7. Model Route alanını yaz
8. Base HEAD'i yaz
9. INDEX'e tek satır ekle
10. CURRENT.Active Session güncelle
11. gerekli context'i yükle
12. implement et
~~~

Session numarası o gün için artar:

~~~text
2026-08-15-001-domain-primitives.md
2026-08-15-002-signal-result.md
~~~

---

## Session Tamamlama

~~~text
targeted validation
→ gerekiyorsa broader build/test
→ git diff --check
→ git diff/status review
→ invariant/contract review
→ session finalize
→ INDEX result güncelle
→ CURRENT.Active Session = NONE
→ CURRENT.Next Immediate Action güncelle
→ kullanıcıya rapor
~~~

Agent varsayılan olarak commit/push yapmaz.

---

## Session Yarım Kalırsa

Session:

~~~text
Result: IN_PROGRESS
~~~

olarak kalır.

`CURRENT.md`:

~~~text
Active Session
Current Problem
Next Immediate Action
Working Set
~~~

alanlarını handoff için güncel tutar.

Yeni AI önce aynı session'dan devam eder.

---

# Pre-existing Working Tree Guard

Meaningful session başında:

~~~text
git status --short
~~~

snapshot'ı alınır.

Session öncesi unrelated kullanıcı değişikliklerini:

~~~text
sahiplenme
overwrite etme
revert etme
silme
stage etme
~~~

Açık talimat olmadan destructive cleanup yok:

~~~text
git reset --hard
git clean -fd
git restore <file>
git checkout -- <file>
~~~

---

# Git Ownership

Varsayılan:

~~~text
Codex:
- edit
- test
- diff/status review
- worklog update
- suggested commit message

Kullanıcı:
- commit
- push
~~~

Açık kullanıcı talimatı olmadan Codex commit/push yapmaz.

---

## Transient Git State Rule

`CURRENT.md` Git'in ikinci bir kopyası değildir.

`CURRENT.md` içine aşağıdaki geçici Git durumları live state olarak yazılmaz:

~~~text
working tree clean / dirty
uncommitted / committed
pushed / not pushed
merged / not merged
current HEAD
branch main ile aynı commit'te
branch remote'a gönderildi / gönderilmedi
~~~

Bunlar birkaç Git operasyonu sonra stale hale gelir.

Gerçek operational Git state her zaman doğrudan Git'ten okunur:

~~~text
git status
git branch --show-current
git log
git rev-parse HEAD
git merge-base
git fetch
~~~

`CURRENT.md` yalnız gerektiğinde zamana dayanıklı Git bağlamı tutabilir:

~~~text
Session Branch
Session Base
~~~

Session completion, commit, push veya merge sonrasında yalnız transient Git
state değişti diye `CURRENT.md` yeniden düzenlenmez.

Bu kuralın amacı worklog-only follow-up commit döngülerini engellemektir.

---
# Token Disiplini

Normal execution başlangıcı:

~~~text
CURRENT
→ git baseline
→ Active Session
→ ilgili session
→ ilgili agents/ playbook
→ yalnız gerekli docs/
→ ilgili code/test
~~~

`INDEX.md` varsayılan olarak okunmak zorunda değildir.

Bütün session history veya bütün `docs/` her görevde yüklenmez.

Bir dosyayı okumadan önce:

~~~text
Bu dosya mevcut kararı değiştirebilir mi?
~~~

sorusu uygulanır.

---

# Security

Worklog içine:

~~~text
secret
token
private key
credential
gereksiz kişisel veri
~~~

yazılmaz.
