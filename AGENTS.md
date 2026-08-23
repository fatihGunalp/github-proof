# GitHub Proof — Repository Agent Contract

Ortak davranış sözleşmesi (Codex, Claude Code). Ürün/domain gerçeği taşımaz —
source of truth `docs/`. Tool-specific routing: Codex → `agents/01-model-routing.md`,
Claude → `CLAUDE.md`.

## 1. Authority

~~~text
1. kullanıcının mevcut talimatı
2. docs/
3. AGENTS.md
4. agents/ playbook'ları
5. worklog/
6. koddan çıkarım
~~~

`agents/` çalışma biçimini tanımlar, ürün gerçeğini değiştiremez. `worklog/`
yalnız geliştirme hafızasıdır; `docs/` ile çelişirse `docs/` kazanır.

## 2. Model Gate

Her yeni meaningful görevde ajan bir model/effort route'u önerir ve kullanıcı
açıkça onaylamadan (örn. `devam et`) mutasyon yapmaz: kod/dosya değişikliği,
test/build, dependency, migration, geniş tarama, commit/push yok.

`devam et`: pending öneri varsa → onaylandı, uygula. Pending yok + Active
Session varsa → aynı session'a devam. İkisi de yoksa → yeniden route et.
Görev pending sırasında değişirse öneri geçersizdir. Risk yükselirse güvenli
checkpoint'te DUR ve yeniden öner.

Tier vocabulary ve çıktı formatı tool-specific dosyanın sorumluluğudur;
burada tekrar tanımlanmaz.

## 3. Context & Workflow

Gate onayından sonra önce `agents/02-context-loading.md` okunur; o, task/
context route'una göre hangi ek playbook'un gerekli olduğunu belirler.
Yalnız route'un işaret ettiği playbook(lar) yüklenir — `agents/03`..`07`'nin
hepsi her görevde eager-load edilmez. Sahiplik referansı:

~~~text
03 = invariant özeti
04 = implementation/session
05 = Repository domain (yalnız etkileniyorsa)
06 = validation
07 = worklog (yalnız kullanılıyorsa)
~~~

Kararı değiştirmeyecek dosya yüklenmez; bütün `docs/`/repository eager-load
edilmez.

## 4. Session / Branch

Default 1 meaningful session → 1 working branch (trivial/mekanik iş istisna
olabilir). Push count branch count'u belirlemez. CURRENT'in bildirdiği Active
Branch/Session Base, mutation öncesi Git ile doğrulanır; mismatch → DUR,
sessizce repair edilmez. Session-base → HEAD diff, primary scope evidence'dır.

## 5. Git Safety

~~~text
Ajan: edit, test, diff/status review, worklog update, suggested commit
Kullanıcı: commit, push
~~~

Açık talimat olmadan commit/push/merge/rebase/tag/release/deploy yok. Ajan
kullanıcı için Git identity (author/committer) oluşturmaz/değiştirmez.
Destructive komut (`reset --hard`, `clean -fd`, `restore <file>`,
`checkout -- <file>`) açık talimatsız kullanılmaz. Unrelated kullanıcı
değişikliği sahiplenilmez, stage/overwrite/revert edilmez.

## 6. Discipline

Gerekmeyen refactor/abstraction/framework/"ileride lazım olur" işi yapılmaz.
Docs sözleşmesi sessizce değiştirilmez.

Mismatch, çelişki, eksik kritik karar veya scope collision → DUR, kullanıcıya
açıkça bildir; sessizce ilerleme veya scope genişletme yok.

Meaningful mutation, final rapordan önce bağımsız review'den geçer (bkz.
`agents/06-review-and-validation.md`); yazan ajan kendi işi için closure
review authority'si sayılmaz.
