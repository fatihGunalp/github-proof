# GitHub Proof — Session Template

> Yeni meaningful development session başlarken bu dosyayı kopyala.
>
> Session yalnız model gate onaylandıktan sonra oluşturulur.
>
> Dosya adı:
>
> ~~~text
> YYYY-MM-DD-NNN-short-topic.md
> ~~~

---

# <Session Title>

**Date:** YYYY-MM-DD
**Result:** IN_PROGRESS
**Scope:** <domain / application / infrastructure / report / web / docs / validation>
**Model Route:** <Luna|Terra|Sol> / <low|medium|high>
**Model Gate:** APPROVED — user wrote `devam et`

---

## Goal

Bu session'ın tek ana hedefini yaz.

Tercih:

~~~text
Implement SignalResult score/status invariants and targeted unit tests.
~~~

Kaçın:

~~~text
domain'i bitir
projeye devam et
backend geliştir
~~~

---

## Starting State

Yalnız doğrulanmış Git bilgisi:

~~~text
Branch:
<git branch --show-current>

Base HEAD:
<git rev-parse --short HEAD>

Pre-existing Working Tree:
<git status --short snapshot veya CLEAN>

Relevant Previous Session:
<path veya NONE>
~~~

Unrelated pre-existing değişiklikleri session'a sahiplenme.

---

## Model Route

Gate sonucu:

~~~text
Selected:
Terra / medium

Why:
<1-3 kısa cümle>

User Approval:
devam et
~~~

Bu alan operasyonel hafızadır.

Model seçimi için source of truth:

~~~text
AGENTS.md
agents/01-model-routing.md
~~~

---

## Model Escalation

Escalation yoksa:

~~~text
None.
~~~

Varsa:

~~~text
Original:
Terra / medium

Proposed:
Sol / high

Reason:
<neden>

User Approval:
devam et

Current Route:
Sol / high
~~~

Kullanıcı onayı gelmeden escalation sonrası kritik işe devam etme.

---

## Context Read

Bu session'da gerçekten karar üzerinde etkili olan kaynakları yaz.

Örnek:

~~~text
AGENTS.md
agents/04-development-workflow.md
agents/05-domain-development.md
worklog/CURRENT.md
docs/architecture/analysis-domain-model.md
docs/product/scoring-and-report-contract.md
~~~

Bütün repository veya bütün docs listesini sırf doldurmak için yazma.

---

## Contract / Invariants Checked

Bu session'ı gerçekten etkileyen contract'ları yaz.

GitHub Proof örnekleri:

~~~text
- SignalResult.Score is nullable integer.
- Missing => Score 0.
- NotApplicable => Score null.
- UnavailableEvidence => Score null.
- RequiresHumanReview => Score null.
- Score cannot exceed MaximumScore.
~~~

İlgisiz invariant'ları ekleme.

Kalıcı contract değişmesi gerekiyorsa worklog tek başına yeterli değildir; ilgili `docs/` güncellenmelidir.

---

## Changes

Meaningful değişiklik özeti.

~~~text
Created:
- ...

Modified:
- ...

Deleted:
- ...
~~~

Full diff koyma.

---

## Decisions

Bu session sırasında alınan implementation-level kararları yaz.

Örnek:

~~~text
- SignalStatus enum Domain altında tutuldu.
- Invalid score/status combinations constructor seviyesinde reddedildi.
~~~

Yeni ürün/domain/mimari contract oluştuysa:

~~~text
STOP
→ canonical docs gerekip gerekmediğini değerlendir
→ kullanıcıya bildir
~~~

Worklog source of truth değildir.

---

## Validation

Çalıştırılan anlamlı kontroller:

~~~text
dotnet test <target>
PASS — <summary>

dotnet build
PASS

git diff --check
PASS
~~~

Çalıştırılmadıysa:

~~~text
NOT RUN — <sebep>
~~~

Uzun terminal çıktısı koyma.

---

## Problems

Yoksa:

~~~text
None.
~~~

Varsa:

~~~text
Problem:
<kısa açıklama>

Cause:
<biliniyorsa>

Resolution:
<uygulanan çözüm veya BLOCKED>
~~~

---

## Not Done

Scope dışında bilinçli olarak bırakılan işler.

~~~text
- ...
~~~

Yoksa:

~~~text
None.
~~~

Bu bölüm scope creep'i engeller.

---

## Known Risks

Session sonunda kalan doğrulanmış riskler.

~~~text
- ...
~~~

Yoksa:

~~~text
None.
~~~

Kanıtlanmamış teorik risklerle doldurma.

---

## Working Set / Final Git State

~~~text
Modified:
- ...

Untracked:
- ...

Pre-existing unrelated:
- ...
~~~

Gerçek state'i `git status --short` ile doğrula.

---

## Next

Tek ve somut sonraki aksiyon.

Tercih:

~~~text
Implement RepositoryIdentity with invariant tests.
~~~

Kaçın:

~~~text
projeye devam et
domain'i ilerlet
~~~

---

## Git

~~~text
Suggested commit:
<type(scope): message>

Commit:
PENDING — user-owned unless explicitly requested

Push:
PENDING — user-owned unless explicitly requested
~~~

Codex varsayılan olarak commit/push yapmaz.

---

## Completion Checklist

~~~text
[ ] Goal tamamlandı veya Result doğru final state'e getirildi
[ ] Targeted validation çalıştı
[ ] Gerekliyse broader validation çalıştı
[ ] git diff --check kontrol edildi
[ ] git diff/status gözden geçirildi
[ ] Contract/invariant drift kontrol edildi
[ ] Session Result güncellendi
[ ] INDEX aynı satır güncellendi
[ ] CURRENT.Active Session güncellendi
[ ] CURRENT.Next Immediate Action güncellendi
[ ] Kullanıcıya kısa final rapor verildi
~~~
