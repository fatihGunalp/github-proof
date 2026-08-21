# Review and Validation

Her meaningful code değişikliği final rapordan önce doğrulanır.

## 1. Scope Review

Kontrol et:

- Yalnız istenen task değişti mi?
- Pre-existing unrelated değişikliklere dokunuldu mu?
- Yeni dependency veya architecture eklendi mi?
- Docs contract sessizce değiştirildi mi?

## 2. Contract Review

İlgili task için canonical docs ile code'u karşılaştır.

Özellikle scoring/domain işinde:

~~~text
15 signal contract
4 categories
91 category-internal max
86 overall-included max
nullable Score semantics
Missing = 0 + denominator included
NotApplicable = null + excluded
UnavailableEvidence = null + excluded
RequiresHumanReview = null + excluded
RawApplicableScoreNormalization
presentation completeness excluded from overall
~~~

## 3. Tests

Code task'larında önce en dar ilgili testleri çalıştır.

Sonra risk gerektiriyorsa broader suite/build çalıştır. Docs/worklog-only task
otomatik olarak build/test gerektirmez.

Repository'nin gerçek solution/project yapısını keşfetmeden command uydurma.

Genel .NET beklentileri mevcut yapı destekliyorsa:

~~~text
dotnet build
dotnet test
~~~

Ama gerçek solution path ve project names repository'den doğrulanmalıdır.

## 4. Git Validation

Final öncesi en az:

~~~text
git diff --check
git status --short
git diff --stat
~~~

ve gerektiğinde ilgili diff incelenir.

Codex'in task öncesi olmayan untracked/pre-existing dosyaları sahiplenmediğini doğrula.

## 5. Worklog

GitHub Proof'a ait live worklog kullanılıyorsa meaningful session sonunda:

~~~text
session final status
CURRENT update
INDEX update
Next / Next Immediate Action
validation summary
suggested commit
~~~

güncellenir.

Worklog'a full terminal dump koyma.

## 6. Final User Report

Kısa ve actionable olmalı:

~~~text
Sonuç: PASS / PASS WITH FINDINGS / BLOCKED / FAILED
Değişenler: <özet>
Validation: <çalıştırılan kontroller>
Risk / kalan iş: <varsa>
Suggested commit: <mesaj>
~~~

Commit/push yapıldı diye yalnız gerçekten yapıldıysa söyle.

Varsayılan olarak yalnız suggested commit mesajı ver.

`PARTIAL` review verdict değildir. Canonical product/domain analysis status'u
olarak anlamlı olabilir; review vocabulary ile karıştırma.

## 7. Independent Closure Review

Codex self-validation implementation validation'dır; final independent closure
review değildir.

Meaningful bir session commit edilmeden önce resulting scope/diff/contract
evidence, riskle orantılı independent review'den geçmelidir. Reviewer ChatGPT,
kullanıcının yönlendirdiği reviewer veya açıkça atanmış başka bir review path
olabilir.

Bu, her docs/worklog-only task için ikinci agent ya da build/test zorunluluğu
getirmez. Codex, kendi yazdığı implementation için ayrı bir review role'ünde
ve author olmadığı evidence üzerinde açıkça çalışmadıkça independent PASS iddia
etmez.
