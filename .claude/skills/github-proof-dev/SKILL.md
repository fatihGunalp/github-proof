---
name: github-proof-dev
description: >
  Implement, modify, debug, refactor, test, or update a GitHub Proof contract
  from the current bounded task or ChatGPT worklog handoff, using minimum
  context and validating the result before independent review.
---

# GitHub Proof Development

Bu skill yalnız Claude native model/effort gate approved olduktan sonra
meaningful development için çalışır.

Pending gate varken implementation veya broad context loading başlatma.

## Start

1. `git status --short`, branch ve HEAD'i kontrol et.
2. `worklog/CURRENT.md` oku.
3. Active Session varsa aynı scope'a ait session'ı oku.
4. CURRENT içindeki Active Branch / Session Base ile actual Git state'i doğrula.
5. Pre-existing changes ve scope collision kontrol et.

Meaningful session branch'i kullanılıyorsa structural working set için öncelikle:

`git diff <session-base>...HEAD`

ve gerektiğinde working-tree diff'ini kullan.

Git'in zaten taşıdığı changed-file/commit bilgisini worklog prose'undan yeniden
üretmeye çalışma.

ChatGPT handoff'u varsa canonical docs ile uyumlu olduğu sürece goal/scope/
acceptance criteria/next action'ı yeniden tasarlama; uygula.

Kritik karar eksikse veya handoff canonical docs ile çelişiyorsa DUR ve bildir.

## Classify

Görevi birincil olarak sınıflandır:

- READ_ONLY
- DOCS_CONTRACT
- IMPLEMENTATION
- DEBUG_FIX

Basit işi gereksiz workflow'a büyütme.

## Context

`agents/02-context-loading.md` kullan.

Her ek read öncesi sor:

Bu dosya mevcut kararımı değiştirebilir mi?

Hayırsa yükleme.

Canonical `docs/`, skill/worklog/code inference üzerinde authority'dir.
`agents/03-project-contract.md` yalnız operational summary'dir.

Implementation/debug için gerektiğinde:
`agents/04-development-workflow.md`

Domain için ayrıca:
`agents/05-domain-development.md`

Worklog için gerektiğinde:
`agents/07-worklog-integration.md`

Claude, `agents/01-model-routing.md` içindeki Codex model gate'ini çalıştırmaz.

## Change

En küçük doğru değişikliği yap; genel implementation disiplini
`agents/04-development-workflow.md` §4'ün sorumluluğundadır.

Skill'e özgü ek kural: unavailable evidence'i stronger truth'e dönüştürme.

DEBUG_FIX:
failure evidence → narrow reproduction → root cause → bounded patch → regression validation.

DOCS_CONTRACT:
canonical semantic owner'ı belirle ve yalnız etkilenebilecek downstream contract'ları incele.

## Validate

Mutation sonrası `agents/06-review-and-validation.md` uygula.

Narrow → broad:

1. exact changed behavior,
2. targeted tests,
3. risk gerektiriyorsa broader test/build,
4. contract/invariant review,
5. bounded diff review,
6. Git validation.

Minimum Git checks:

- `git diff --check`
- `git status --short`
- `git diff --stat`

Repository structure doğrulanmadan command/path uydurma.

## Independent Review

Meaningful mutation self-validation PASS olduktan sonra
`github-proof-review` ile bağımsız review yap.

Review forked context'te çalışır; implementation reasoning'ini authority saymaz.

BLOCKER/HIGH finding varsa finalize etme.
Accepted fix gerekiyorsa bu dev workflow'unda bounded correction yap ve review'ı
bir kez daha çalıştır.

Sonsuz review/fix döngüsü üretme.

## Handoff

Meaningful work sonunda live workflow gerektiriyorsa Active Session/CURRENT'a
kısa ve doğrulanabilir handoff bırak:

- result,
- changed scope,
- validation,
- review verdict,
- remaining risk,
- exact next action.

Bu handoff sonraki ChatGPT analysis için repository memory'dir.
Worklog product truth değildir.

## Stop

Development completion Git side-effect authority değildir (bkz. `AGENTS.md`
§5). Explicit finalization gerekiyorsa `/github-proof-finalize` kullan.

Kullanıcıya Türkçe ve kısa rapor ver.
