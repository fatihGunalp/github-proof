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

Authority sırası (`docs/` > `AGENTS.md` > `agents/` > `worklog/` > koddan
çıkarım) `AGENTS.md` §1'in sorumluluğudur; burada tekrar tanımlanmaz.

`worklog/` bu sıranın en altındadır:

~~~text
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

Üç katman, üç ayrı soru:

~~~text
CURRENT.md
= NOW
= şu an aktif iş, branch/base, pozisyon, blocker, exact next action

INDEX.md
= NAVIGATION
= hangi session dosyasını açmam gerekiyor?

sessions/
= HISTORY
= o session'da tam olarak ne oldu, ne doğrulandı, ne karar verildi
~~~

Active Session, `CURRENT.md` içinden işaret edilen canlı handoff kaydıdır.

Kritik kural:

~~~text
Session log iş bittikten sonra geriye dönük uydurulmaz.

Meaningful development işi gerçekten başlarken oluşturulur.
~~~

# Model Gate ile Worklog İlişkisi

Model gate protokolü (STOP, `devam et` semantiği, escalation) ve tool-specific
tier tabloları `AGENTS.md` §2 / `agents/01-model-routing.md` / `CLAUDE.md`'nin
sorumluluğudur; burada tekrar tanımlanmaz.

Worklog'a özgü tek kural: model gate beklenirken worklog session açılmaz ve
repository değiştirilmez; session yalnız gate onayından sonra oluşturulur.

---

# CURRENT.md

`CURRENT.md`, GitHub Proof'un güncel geliştirme snapshot'ıdır (NOW — aktif
session, branch/base, phase, blocker/open decision, exact next action).

Execution başladıktan sonra ilk okunacak operasyonel dosyadır.

Tam alan listesi ve compaction kuralları
`agents/07-worklog-integration.md` "CURRENT Compaction" bölümünün
sorumluluğudur; burada ikinci bir şema tanımlanmaz.

Yazım kuralı: her alan tek ve somut olmalıdır.

Tercih:

~~~text
Current Task:
Implement SignalResult invariants and unit tests.

Exact Next Action:
RequiresHumanReview için Score=null invariant testini düzelt ve targeted
test'i tekrar çalıştır.
~~~

Kaçın:

~~~text
projeye devam et
backend geliştir
birkaç şey düzelt
~~~

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

Değişen dosya listesi, diff, terminal geçmişi ve validation transcript'i
`CURRENT.md` içine yazılmaz; gerçek repository state her zaman Git'ten okunur,
ayrıntı ise session dosyasında kalır.

---

# INDEX.md

`INDEX.md` NAVIGATION katmanıdır: hangi session dosyasının açılacağına karar
verdirir, o dosyayı okuma ihtiyacını ortadan kaldırmaz.

Session başına tek satır:

~~~text
Date | Topic | Result | Route | Session | Base | Commit
~~~

Kurallar:

~~~text
Result = status + tek satır outcome
paragraf boyutunda result cell yazılmaz
narrative, review round'ları ve validation kanıtı session dosyasında kalır
mevcut satır IN_PROGRESS'ten final result'a güncellenir; ikinci satır açılmaz
Base ve Commit provenance'ı silinmez
INDEX ikinci bir CURRENT değildir; live state taşımaz
~~~

Geçmiş session dosyaları compaction amacıyla toplu olarak yeniden yazılmaz,
kısaltılmaz, taşınmaz veya silinmez.

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
9. doğrulanan base bir önceki session'ın integration commit'iyse, o satırın
   boş `Commit` alanını doldur
10. INDEX'e tek satır ekle
11. CURRENT.Active Session güncelle
12. gerekli context'i yükle
13. implement et
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
→ INDEX result güncelle (`Commit` boş kalır; integration user-owned
  olduğu için bir sonraki session başında doldurulur)
→ CURRENT.Active Session = NONE
→ CURRENT.Exact Next Action güncelle
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

`CURRENT.md` handoff için şunları güncel tutar:

~~~text
Active Session
blocker / open decision
exact next action
~~~

Yeni AI önce aynı session'dan devam eder.

---

# Pre-existing Working Tree Guard / Git Ownership

Destructive-command yasağı ve edit/test/commit/push ownership split'i
`AGENTS.md` §5'in sorumluluğudur; burada tekrar tanımlanmaz.

Worklog'a özgü tek kural: meaningful session başında `git status --short`
snapshot'ı alınır; session öncesi unrelated kullanıcı değişiklikleri
sahiplenilmez, overwrite/revert/stage edilmez.

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
