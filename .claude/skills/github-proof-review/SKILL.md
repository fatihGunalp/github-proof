---
name: github-proof-review
description: >
  Independently review or verify the current bounded GitHub Proof change
  against canonical contracts, scope, validation evidence, and worklog state.
context: fork
---

# GitHub Proof Independent Review

Implementation yapma; mevcut işi bağımsız değerlendir.

## Establish Scope

1. branch ve working tree'yi incele.
2. `worklog/CURRENT.md` oku.
3. Active Session varsa oku.
4. CURRENT içindeki Active Branch / Session Base ile actual Git state'i doğrula.
5. bounded changed-file set'i belirle.
6. pre-existing/unrelated changes'i ayır.

Meaningful session branch'i varsa primary structural review scope:

`git diff <session-base>...HEAD`

Artı henüz commit edilmemiş relevant working-tree/cached diff.

Worklog'daki prose changed-file listesini Git diff'in yerine authority sayma.

CURRENT operational state'tir; product truth değildir.

## Context

`agents/02-context-loading.md` kullan.

Yalnız review verdict'ini değiştirebilecek canonical docs/code/test context'ini
yükle.

Authority sırası `AGENTS.md` §1'in sorumluluğundadır; burada tekrar
tanımlanmaz.

Implementation davranışı canonical docs'u override edemez.

## Review

Actual diff üzerinde kontrol et:

- scope ownership,
- unrelated changes,
- contract/invariant drift,
- architecture/layer boundary,
- determinism,
- evidence-strength upgrade,
- unnecessary abstraction/dependency,
- required tests/validation,
- worklog claim ile gerçek evidence uyumu.

Domain/architecture etkileniyorsa gerektiği kadar:
`agents/04-development-workflow.md`
`agents/05-domain-development.md`

Validation için:
`agents/06-review-and-validation.md`

Worklog semantiği gerekiyorsa:
`agents/07-worklog-integration.md`

Existing trustworthy validation yeterliyse pahalı kontrolleri ritual olarak
tekrarlama.

Minimum Git review:

- `git status --short`
- relevant diff/cached diff
- `git diff --check`
- `git diff --stat`

## Findings

Finding varsa ver:

- severity,
- concrete evidence,
- violated contract/principle,
- impact,
- blocking status.

Severity:

- BLOCKER
- HIGH
- MEDIUM
- LOW

Hypothetical defect veya kişisel preference'i finding yapma.
Finding üretmek için finding uydurma.

Default olarak dosya değiştirme.
Fix istenirse `github-proof-dev` işidir.

## Verdict

Tek verdict:

- PASS
- PASS WITH FINDINGS
- BLOCKED
- FAILED

Türkçe, kısa ve evidence-first raporla.

PASS ancak bounded scope ve gerekli validation gerçekten yeterliyse ver.
