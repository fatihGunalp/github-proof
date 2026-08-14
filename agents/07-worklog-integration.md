# Worklog Integration

## İncelenen worklog.zip Hakkında

Paylaşılan `worklog.zip` güçlü bir **operational memory pattern** içeriyor:

- `docs/` source of truth
- `agents/` çalışma playbook'ları
- `worklog/CURRENT.md` current snapshot
- `worklog/INDEX.md` session catalog
- `worklog/sessions/` meaningful development history
- Active Session + `IN_PROGRESS`
- Base HEAD
- pre-existing working tree guard
- checkpoint/handoff
- destructive Git guard
- agent edit/test eder, kullanıcı commit/push eder
- minimum context / minimum token yaklaşımı

Bu pattern GitHub Proof için uygundur.

## Kritik Uyarı: Zip İçeriği GitHub Proof State'i Değildir

Paylaşılan worklog'un live içerikleri başka bir project'e aittir.

Özellikle şu kavramlar GitHub Proof current state'i olarak **import edilmemelidir**:

~~~text
Localora
Partner != User
Product != Offer
Offer != Opportunity
Workspace / Inbox / Outbox foundation
Creator SaaS
softbreak_db
softbreak_test
Localora PostgreSQL
~~~

Bunlar worklog pattern'inin örnek/önceki project içeriğidir; GitHub Proof ürün sözleşmesi değildir.

Bu nedenle Codex mevcut `worklog/` içinde bu tür foreign state görürse:

~~~text
onu GitHub Proof current state'i olarak kullanma
onu docs/ üstünde authority sayma
onu implementation kararı olarak taşıma
~~~

## GitHub Proof İçin Reuse Edilecek Kurallar

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

Eski worklog pattern'inde `devam et` Active Session continuation komutuydu.

GitHub Proof Codex workflow'unda yeni öncelik:

~~~text
1. Pending MODEL SEÇİMİ varsa:
   "devam et" = kullanıcı modeli değiştirdi, mevcut task'ı uygula

2. Pending model gate yoksa ve Active Session varsa:
   "devam et" = aynı session'a devam et

3. Pending gate yok ve yeni bağımsız iş gerekiyorsa:
   yeniden model route et
~~~

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
