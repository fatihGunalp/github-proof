# GitHub Proof — Claude Code

Bu dosya Claude Code orchestration bootstrap'ıdır.
Product/domain/architecture gerçeği taşımaz.

## Authority

Çelişkide sıra:

1. Kullanıcının mevcut açık talimatı.
2. Canonical `docs/`.
3. Bu `CLAUDE.md`.
4. Uygulanabilir `AGENTS.md`.
5. Uygulanabilir `agents/` playbook'ları.
6. `worklog/` operational state.
7. Code inference.

`docs/` source of truth'tur.
`worklog/CURRENT.md` meaningful işte ilk operational read olabilir; product truth değildir.

## ChatGPT → Claude Çalışma Modeli

Bu repository genellikle şu döngüyle geliştirilir:

ChatGPT analysis / karar
→ worklog/session handoff
→ Claude implementation
→ Claude independent review
→ worklog/session result
→ ChatGPT sonraki analysis

Meaningful görevde önce `CURRENT.md` ve varsa Active Session'ı oku.

ChatGPT tarafından session/worklog'a bırakılmış goal, scope, acceptance criteria ve
next action varsa bunları implementation handoff'u olarak kullan.

Canonical docs ile çelişmiyorsa aynı analizi baştan üretme.
Çelişki, eksik kritik karar veya scope collision varsa implementation öncesi bildir.

Implementation sonrası worklog'u, sonraki ChatGPT oturumunun repository'den
durumu anlayabileceği kadar kısa ve doğrulanabilir bırak.

## Claude Native Model / Effort Gate

Her yeni meaningful repository task'ında implementation veya geniş context
yüklemeden önce task riskini route et.

Yalnız gerekiyorsa çok küçük triage kullan:

- kullanıcının task'ı,
- mevcut ChatGPT worklog/session handoff'u,
- `git status --short`,
- `CURRENT.md` içindeki ilgili operational bölüm.

Gate için bütün docs'u, source tree'yi veya testleri yükleme.

Canonical routing:

- MECHANICAL → `sonnet` / `low`
- BOUNDED → `sonnet` / `medium`
- INVARIANT_HEAVY → `sonnet` / `high`
- COMPLEX → `opus` / `high`
- HIGH_RISK → `opus` / `xhigh`

`max` normal project routing seçeneği değildir.
Yalnız kullanıcı açıkça isterse veya somut exceptional gerekçe varsa önerilebilir.

ChatGPT handoff'u problemi ve acceptance criteria'yı zaten bounded biçimde
çözmüşse aynı analizi Claude'da yeniden üretme; implementation riskini route et.

Gate çıktısı yalnız şu biçimde olsun:

MODEL / EFFORT ÖNERİSİ
Model: <sonnet|opus>
Effort: <low|medium|high|xhigh>

Gerekçe:
<1-3 kısa cümle>

Claude Code'da:
`/model <model>`
`/effort <effort>`

Seçimi yaptıktan sonra `devam et` yazın.

Bu çıktıdan sonra DUR.
Implementation, geniş context loading, build/test veya mutation başlatma.

Pending gate sonrasında kullanıcı `devam et` yazarsa seçim onaylanmış kabul et,
yeniden route etme ve `github-proof-dev` workflow'una geç.

Kullanıcı gate beklerken task'ı değiştirirse eski route'u geçersiz say ve yeni
task'ı yeniden route et.

Aynı approved task içindeki normal continuation'da gereksiz yeni gate üretme.
Risk sınıfı materially yükselirse güvenli checkpoint'te yeni model/effort öner ve DUR.

Model alias'larını kullan; specific model version hard-code etme.
Skill frontmatter'ına sabit `model` veya `effort` koyma; approved session
selection inherit edilsin.

## Claude Model Routing Exception

`agents/01-model-routing.md` içindeki Luna/Terra/Sol + `devam et` gate'i
Codex-specific'tir ve Claude tarafından çalıştırılmaz.

Claude native gate yukarıdaki model/effort kurallarını kullanır.
Bu exception Codex davranışını veya canonical contract'ları değiştirmez.

## Language

Kullanıcıyla varsayılan olarak Türkçe iletişim kur.

Açıklama, progress, review finding, warning ve final rapor Türkçe olsun.
Code, identifier, enum, path, command ve canonical technical term gerektiğinde
orijinal biçiminde kalabilir.

## Skills

Meaningful implementation, debug, refactor, test veya contract işi:
`github-proof-dev`

Independent verification/review:
`github-proof-review`

Git finalization:
yalnız explicit `/github-proof-finalize`

Basit read-only açıklama için skill zorunlu değildir.

## Context

Önce `CURRENT.md`.
Sonra `agents/02-context-loading.md`.

Yalnız mevcut kararı değiştirebilecek context'i yükle.
Tüm docs, tüm session history veya tüm repository'yi eager-load etme.

## Safety

Canonical contract'ı sessizce değiştirme.
Pre-existing/unrelated kullanıcı değişikliklerini koru.
Scope dışı cleanup veya speculative abstraction yapma.

Destructive Git işlemi yapma.
Commit/push/merge/rebase/tag/release/deploy yetkisini varsayma.
