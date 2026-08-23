# Worklog Integration

## Authority Boundary

Foreign/example project state GitHub Proof authority değildir ve `CURRENT.md`e
import edilmez.

## GitHub Proof Worklog Kuralları

Şunlar reuse edilir:

~~~text
docs/ > agents/ > worklog
CURRENT = current snapshot
Active Session = canlı handoff
session meaningful iş başında açılır
IN_PROGRESS aynı session'da devam eder
Base HEAD git'ten doğrulanır
pre-existing working tree korunur
checkpoint kısa tutulur
final validation yazılır
agent commit/push yapmaz
~~~

## Model Gate ile `devam et` Çakışması

Eski worklog pattern'inde `devam et` yalnız Active Session continuation
komutuydu. `devam et`'in güncel önceliği (pending gate onayı > Active Session
continuation > yeni route) `AGENTS.md` §2'nin sorumluluğundadır; burada
tekrar tanımlanmaz.

Bu ayrım zorunludur; aksi hâlde model gate sonsuz döngüye girebilir.

## GitHub Proof Worklog Başlangıç Kuralı

GitHub Proof için live worklog kullanılacaksa `CURRENT.md` sıfırdan GitHub Proof gerçekleriyle kurulmalıdır.

Kaynaklar:

~~~text
git status / HEAD
docs/ current contract
gerçek code state
son doğrulanmış development task
~~~

Başka project worklog'undan current decisions kopyalama.

## Session'a Model Route Ekleme

GitHub Proof session template'inde optional operational alan olarak şu bilgi tutulabilir:

~~~text
Model Route: Terra / medium
~~~

Bu, neden o model seçildiğini tekrar tartışmayı azaltır.

Model route ürün/domain source of truth değildir.

## Token Discipline

Normal task başlangıcı:

~~~text
AGENTS model gate
→ model seçimi
→ devam et
→ CURRENT
→ git status
→ Active Session
→ ilgili agent playbook
→ yalnız gerekli docs
→ code/test
~~~

Bütün session history'yi varsayılan olarak yükleme.

`INDEX.md` yalnız geçmiş session'ı bulmak gerektiğinde okunur.

## Security

Worklog içine secret, token, private key, credential veya gereksiz PII koyma.


## CURRENT Compaction

`CURRENT.md` **SESSION REPORT DEĞİLDİR.** Append-only history değildir; live
operational snapshot'tır.

CURRENT yalnız gerekli semantic state'i taşır:

- current/next stage,
- active session,
- active branch + session base,
- goal/scope,
- blocker/risk,
- exact next action,
- last remote-verified main.

Closed implementation detail, review-round history, defect chronology,
validation transcript ve live-check telemetry detail session dosyasında kalır;
yalnız next task için gerekli tek bir concise operational fact istisnadır.
Git structural facts'i worklog prose'unda tekrar etme.

Dedicated session branch'te:

Git = structural memory.
Worklog = semantic memory.

Primary structural scope evidence:
session base → branch HEAD diff.

Session kapatılırken obsolete current state'i replace et; ikinci bir historical
layer append etme. Detailed history session dosyasına taşınır.

Amaç ucuz ve güvenilir first operational read'dir.
