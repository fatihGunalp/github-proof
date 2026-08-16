---
name: github-proof-finalize
description: >
  Manually finalize a validated GitHub Proof checkpoint with exact scope,
  staged-diff checks, authorized commit/push, and remote verification.
disable-model-invocation: true
---

# GitHub Proof Finalization

Manual side-effect workflow.

Skill invocation tek başına commit veya push yetkisi vermez.
Yetki current explicit user instruction + active workflow'dan gelir.

## Read

Önce:

1. branch / working tree,
2. `worklog/CURRENT.md`,
3. Active Session,
4. expected final scope,
5. pre-existing/unrelated changes.

Dirty olan her dosyayı session'a ait varsayma.

## Readiness

Finalization öncesi doğrula:

- intended work complete,
- required validation PASS,
- independent review verdict acceptable,
- unresolved BLOCKER/HIGH yok,
- worklog intended checkpoint'i doğru yansıtıyor,
- expected scope net.

IN_PROGRESS işi sırf skill çağrıldı diye finalize etme.

## Final Validation

Gerektiği kadar `agents/06-review-and-validation.md` uygula.

Minimum:

- `git diff --check`
- `git status --short`
- `git diff --stat`
- bounded diff

Relevant state validation'dan sonra değişmediyse pahalı testleri ritual olarak
tekrarlama.

Worklog finalization gerekiyorsa:
`agents/07-worklog-integration.md`

## Stage

Yalnız exact expected file set'i stage et.
Bounded scope biliniyorsa `git add .` kullanma.

Cached checks:

- `git diff --cached --name-only`
- `git diff --cached --check`
- `git diff --cached --stat`
- `git status --short`

Unexpected staged file veya checkpoint'i eksik bırakan relevant unstaged remainder
varsa DUR.

## Commit

Commit yalnız current user instruction veya active workflow açıkça yetki veriyorsa.

Yetkiyi testlerin geçmesinden veya skill invocation'dan çıkarma.

Commit sonrası:

- SHA,
- parent/base gerektiğinde,
- exact scope,
- working tree

doğrula.

## Push

Push ayrı side effect'tir; ayrıca authorized olmalıdır.

Push sonrası:

1. remote ref fetch et,
2. local HEAD == fetched remote doğrula,
3. exact SHA raporla.

Active workflow independent remote verification istiyorsa local fetch equality
tek başına yeterli değildir; available GitHub integration ile remote commit/scope/
contract/worklog state'i doğrula.

Gerçekte yapılmayan commit/push/remote verification'ı yapılmış gibi söyleme.

## Guard Failure

Herhangi bir finalization guard FAIL olursa:

DUR
→ state'i koru
→ exact failure'ı bildir
→ smallest corrective next action ver.

Destructive cleanup, force push, merge, rebase, tag, release veya deploy yapma.

Türkçe ve yalnız doğrulanmış facts ile raporla.
