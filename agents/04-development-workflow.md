# Development Workflow

Bu workflow model gate onayından sonra uygulanır.

## 1. Repository Baseline

Meaningful task başlangıcında doğrula:

~~~text
git status --short
git branch --show-current
git rev-parse --short HEAD
~~~

Pre-existing değişiklikleri not et.

Unrelated kullanıcı değişikliklerine dokunma.

Session/branch invariant'ı (default 1 session → 1 branch, mismatch → DUR)
`AGENTS.md` §4'ün sorumluluğundadır; burada tekrar tanımlanmaz.

## 2. Active Work

GitHub Proof'a ait geçerli bir `worklog/CURRENT.md` varsa:

~~~text
CURRENT
→ Active Session
→ varsa ilgili session
→ task'a uygun agents playbook
→ gerekli docs
→ ilgili code/test
~~~

Active Session `IN_PROGRESS` ise aynı iş için yeni session açma.

~~~text
same active session + same bounded objective
→ existing branch/session üzerinde devam et

new independent objective
→ kullanıcı açıkça aksini seçmedikçe new meaningful session/branch
~~~

Kullanıcı bağımsız başka bir task istiyorsa mevcut session'ı sessizce terk etme; scope çatışmasını raporla.

## 3. Meaningful Session

Worklog aktif kullanılıyorsa meaningful development işi başlarken session oluşturulur; iş bittikten sonra geriye dönük uydurulmaz.

Session başlangıcında en az:

~~~text
Goal
Result = IN_PROGRESS
Base HEAD
Pre-existing Working Tree
Context Read
Model Route
~~~

bilgileri tutulabilir.

`Model Route` örneği:

~~~text
Terra / medium
~~~

Bu alan yalnız operasyon hafızasıdır; model availability için source of truth değildir.

## 4. Implementation

- En küçük doğru değişikliği yap.
- Existing architecture ve naming pattern'lerini önce keşfet.
- Gereksiz abstraction üretme.
- Yeni dependency eklemeden önce gerçek ihtiyaç olduğunu göster.
- Product/domain contract'ı code kolaylığı için gevşetme.
- Scope dışı "cleanup" yapma.
- Test edilebilir invariant'ı comment ile değil test ile güvenceye al.

## 5. Docs Change Boundary

Implementation yeni bir:

~~~text
product
domain
architecture
business
integration
delivery
~~~

kararı üretiyorsa yalnız code/worklog güncellemek yeterli değildir.

Canonical docs değişmesi gerekiyorsa bunu kullanıcıya açıkça bildir.

## 6. Git Safety

Ownership, destructive-command listesi ve unrelated-changes kuralı
`AGENTS.md` §5'in sorumluluğundadır; burada tekrar tanımlanmaz.

## 7. Mid-task Model Escalation

Görev seçilen modelden daha karmaşık hâle gelirse `AGENTS.md` §2 escalation
disiplinini ve `01-model-routing.md` (Codex) tier detaylarını uygula.

Safe checkpoint bırak.

Model yükseltilmeden kritik karara devam etme.

## 8. Completion

İş bittiğinde:

~~~text
targeted tests
→ broader tests/build gerekiyorsa
→ git diff --check
→ git diff/status review
→ contract/invariant review
→ worklog finalize gerekiyorsa
→ kullanıcıya kısa rapor
~~~

Commit atma.
