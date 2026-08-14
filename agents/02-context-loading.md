# Minimum Context Loading

## Amaç

Her task'ta bütün repository ve bütün docs'u okumak yerine yalnız karar vermek için gerekli bağlamı yüklemek.

Model gate onayından sonra bu dosya uygulanır.

## Genel Başlangıç

İlk read-only adımlar:

~~~text
1. git status --short
2. git branch --show-current
3. git rev-parse --short HEAD
4. geçerli GitHub Proof worklog/CURRENT.md varsa oku
5. Active Session varsa aynı session'ı değerlendir
6. task tipine göre gerekli agents/ playbook'unu oku
7. yalnız gerekli docs/ dosyalarını oku
8. ilgili kod ve testleri incele
~~~

Pre-existing working tree değişikliklerini session/task diff'i ile karıştırma.

## Belge Haritası

### Domain primitive / domain model

Öncelik:

~~~text
docs/architecture/analysis-domain-model.md
agents/05-domain-development.md
~~~

Scoring davranışı etkileniyorsa ayrıca:

~~~text
docs/product/scoring-and-report-contract.md
docs/architecture/mvp-signal-definitions.md
~~~

### Signal evaluator

Oku:

~~~text
docs/architecture/mvp-signal-definitions.md
docs/product/analysis-categories-and-signal-boundaries.md
docs/architecture/analysis-domain-model.md
docs/product/scoring-and-report-contract.md
~~~

Yalnız uygulanacak sinyal bölümünü ayrıntılı oku; diğer 14 sinyali gereksiz yere tekrar yükleme.

### Score / category / normalization

Oku:

~~~text
docs/product/scoring-and-report-contract.md
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md — Contract Lock + weight/category bölümleri
~~~

### GitHub data collection / RepositorySnapshot

Oku:

~~~text
docs/architecture/analysis-domain-model.md
docs/architecture/mvp-signal-definitions.md — ilgili RequiredData bölümleri
docs/roadmap/mvp-phases.md — Faz 2
~~~

### Report / Markdown / web result

Oku:

~~~text
docs/product/scoring-and-report-contract.md
docs/product/sample-analysis-report.md
docs/product/mvp-product-contract.md
~~~

### Validation repository work

Oku:

~~~text
docs/product/validation-repository-set.md
docs/architecture/mvp-signal-definitions.md
docs/product/scoring-and-report-contract.md
~~~

### UI

Oku:

~~~text
docs/product/mvp-product-contract.md
docs/product/sample-analysis-report.md
docs/roadmap/mvp-phases.md — Faz 5
~~~

### Portfolio presentation semantics

Oku:

~~~text
docs/decisions/001-portfolio-presentation-category.md
docs/architecture/mvp-signal-definitions.md — Portföy Sunumu
docs/product/scoring-and-report-contract.md
~~~

## Source-of-Truth Kuralı

Bir task sırasında code ile docs farklıysa otomatik olarak code'u veya worklog'u doğru kabul etme.

~~~text
DUR
→ ilgili canonical docs'u doğrula
→ fark implementation bug mı, stale docs mu belirle
→ contract değişikliği gerekiyorsa kullanıcıya bildir
~~~

## Token Kuralı

Bir dosyayı okumadan önce sor:

~~~text
Bu dosya mevcut kararımı değiştirebilir mi?
~~~

Hayırsa yükleme.

Aynı içerik bu session'da zaten doğrulandıysa sırf alışkanlık için tekrar okuma.
