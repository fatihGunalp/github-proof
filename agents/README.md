# agents/ — GitHub Proof Codex Playbook'ları

Bu klasör Codex'in **nasıl çalışacağını** tanımlar.

Ürün/domain gerçeğinin source of truth'u değildir. Gerçek sözleşmeler `docs/` altındadır.

## Dosyalar

| Dosya | Amaç |
|---|---|
| `01-model-routing.md` | Sol / Terra / Luna + low / medium / high seçimi ve `devam et` gate'i |
| `02-context-loading.md` | Göreve göre minimum belge/kod bağlamını seçmek |
| `03-project-contract.md` | GitHub Proof MVP invariant'larının kısa operational özeti |
| `04-development-workflow.md` | Git, session, implementation ve scope akışı |
| `05-domain-development.md` | Faz 1 Domain foundation için C# kuralları |
| `06-review-and-validation.md` | Test, diff, validation ve final rapor kuralları |
| `07-worklog-integration.md` | worklog yaklaşımını GitHub Proof'a güvenli biçimde bağlamak |

## Okuma Stratejisi

Her görevde bütün playbook'ları okuma.

Akış:

~~~text
Yeni görev
→ 01-model-routing
→ kullanıcı model değiştirir
→ "devam et"
→ 02-context-loading
→ göreve uygun playbook + gerekli docs
→ mevcut kod/test
→ implementasyon
→ 06-review-and-validation
~~~

Domain foundation veya domain model görevi ise `05-domain-development.md` zorunludur.

Worklog kullanılıyorsa `07-worklog-integration.md` zorunludur.

## Drift Kuralı

Bu klasördeki herhangi bir özet `docs/` ile çelişirse:

~~~text
DUR
→ docs/ gerçeğini kullan
→ agents/ özetinin stale olduğunu raporla
→ kullanıcı istemeden product contract değiştirme
~~~
