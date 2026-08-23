@AGENTS.md

# Claude Code Adapter

Yalnız Claude'a özgü delta. `AGENTS.md`'deki authority/git-safety/session-
branch/context/scope/STOP kuralları burada tekrar tanımlanmaz.

## Model / Effort Gate

`AGENTS.md` §2 disiplinini kullanır (STOP, `devam et`, escalation). Claude'a
özgü olan yalnız tier vocabulary'si ve çıktı formatıdır:

- MECHANICAL → `sonnet` / `low`
- BOUNDED → `sonnet` / `medium`
- INVARIANT_HEAVY → `sonnet` / `high`
- COMPLEX → `opus` / `high`
- HIGH_RISK → `opus` / `xhigh`

`max` yalnız kullanıcı açıkça isterse veya somut exceptional gerekçe varsa.
Model alias kullan, spesifik versiyon hard-code etme; skill frontmatter'ına
sabit `model`/`effort` koyma.

Çıktı:

~~~text
MODEL / EFFORT ÖNERİSİ
Model: <sonnet|opus>
Effort: <low|medium|high|xhigh>
Gerekçe: <1-3 kısa cümle>
Claude Code'da: /model <model>  /effort <effort>
Seçimi yaptıktan sonra `devam et` yazın.
~~~

Bu çıktıdan sonra DUR.

`agents/01-model-routing.md` içindeki Luna/Terra/Sol gate'i Codex-specific'tir
ve Claude tarafından çalıştırılmaz; bu exception canonical contract'ları
değiştirmez.

## ChatGPT → Claude

Döngü: ChatGPT analysis → worklog handoff → Claude implementation → Claude
independent review → worklog result → ChatGPT devamı. Handoff'un goal/scope/
acceptance criteria'sı canonical docs'la çelişmiyorsa yeniden üretilmez;
çelişki/eksik kritik karar/scope collision varsa `AGENTS.md` §6 gereği
implementation öncesi bildirilir.

## Language

Kullanıcıyla varsayılan Türkçe iletişim: açıklama, progress, review finding,
warning, final rapor. Code/identifier/enum/path/command gerektiğinde orijinal
biçiminde kalabilir.

## Skills

Implementation/debug/refactor/test/contract: `github-proof-dev`.
Independent verification/review: `github-proof-review`.
Git finalization: yalnız explicit `/github-proof-finalize`.
Basit read-only açıklama için skill zorunlu değildir.
