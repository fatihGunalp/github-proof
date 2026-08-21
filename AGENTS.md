# GitHub Proof — Codex Repository Instructions

Bu dosya repository düzeyindeki Codex giriş noktasıdır.

Codex bu repository'de hiçbir meaningful işi doğrudan uygulamaya başlamaz. Önce görev için model ve reasoning effort seçimi yapar, kullanıcıdan `devam et` onayı bekler, sonra yalnız gerekli bağlamı yükleyip çalışır.

## 1. Instruction Precedence

Project içinde çelişki olduğunda şu sıra kullanılır:

~~~text
1. Kullanıcının mevcut açık talimatı
2. docs/ içindeki güncel ürün/domain/mimari sözleşmeleri
3. Bu AGENTS.md
4. agents/ altındaki playbook'lar
5. GitHub Proof'a ait geçerli worklog/ state'i
6. Mevcut koddan yapılan çıkarım
~~~

`docs/` project source of truth'tur.

`agents/` çalışma biçimini tanımlar; ürün gerçeğini değiştiremez.

`worklog/` yalnız geliştirme hafızasıdır; `docs/` ile çelişirse `docs/` kazanır.

## 2. Mandatory Model Gate

Her yeni repository görevi için önce `agents/01-model-routing.md` kurallarını uygula.

Yeni görevde, kullanıcı açıkça `devam et` demeden önce:

- kod yazma veya değiştirme,
- dosya oluşturma/silme,
- test/build çalıştırma,
- dependency ekleme,
- migration üretme,
- geniş repository taraması yapma,
- commit/push yapma.

Model seçimi için yalnız görevin kendisini ve gerekiyorsa çok sınırlı read-only triage bilgisini kullan.

Model önerisini ver ve DUR.

Öneri şu biçimde bitmelidir:

~~~text
Modeli <MODEL> / <EFFORT> olarak ayarla.
Devam etmek için: devam et
~~~

Codex modeli kendisinin değiştirdiğini iddia etmez. Modeli kullanıcı değiştirir.

## 3. `devam et` Semantiği

Eğer bir önceki Codex mesajı model seçimi önerisi ise kullanıcının `devam et` mesajı:

~~~text
model seçimi onaylandı
→ artık görevi uygula
~~~

anlamına gelir.

Bu durumda yeniden model seçimi isteme.

Eğer pending model seçimi yoksa ve kullanıcı `devam et` derse, GitHub Proof'a ait geçerli bir Active Session varsa aynı session'ın devam işi olarak yorumla. Yeni bir bağımsız görev oluşuyorsa yeniden model gate uygula.

Kullanıcı model gate beklenirken görevi değiştirirse eski öneriyi geçersiz say ve yeni görev için yeniden route et.

## 4. Context Loading

Model gate onayından sonra `agents/02-context-loading.md` dosyasını uygula.

Bütün `docs/` klasörünü her görevde okuma.

Yalnız görevin kararını etkileyen belgeleri yükle.

## 5. Project Contract

Uygulama boyunca `agents/03-project-contract.md` içindeki invariant özetine uy.

Özet ile `docs/` çelişirse `docs/` kazanır.

Özellikle:

- GitHub Proof geliştiriciyi değil görünür kanıtı değerlendirir.
- Repository Proof, tek bir public repository için micro evidence view'dur;
  mevcut deterministik 15-sinyal / 4-kategori scoring contract'ını kullanır.
- Profile Proof, bir public GitHub profili üzerindeki görünür repository
  evidence'ını deterministic ve job-agnostic macro aggregation olarak sunar;
  Repository Proof scoring semantics'ini yeniden tanımlamaz.
- Her iki proof surface seniority, employability veya unsupported capability
  inference üretmez.
- LLM doğrudan puan üretmez.
- Puanlama deterministiktir.
- `Missing`, `NotApplicable`, `UnavailableEvidence` ve `RequiresHumanReview` birbirine karıştırılmaz.
- Bileşik presentation completeness sinyali overall skorda ikinci kez sayılmaz.

## 6. Development Workflow

Model gate onayından sonra implementation için `agents/04-development-workflow.md` uygulanır.

`agents/05-domain-development.md`, Repository Proof V1'in original
domain-scoring foundation rehberidir. Yalnız Repository-domain concerns
etkilendiğinde oku. Profile domain işleri için `agents/02-context-loading.md`
Profile routes'larını ve ilgili canonical Profile contract'ını kullan.

İş bitiminde `agents/06-review-and-validation.md` uygulanır.

Worklog kullanılıyorsa `agents/07-worklog-integration.md` kuralları geçerlidir.

Shared session invariant:

~~~text
default: 1 meaningful session → 1 working branch
CURRENT active branch/base bildiriyorsa mutation öncesi Git ile doğrula
branch veya base mismatch → DUR
~~~

## 7. Git Safety

Kullanıcı açıkça istemedikçe Codex:

~~~text
git commit
git push
git merge
git rebase
git tag
release
deploy
~~~

yapmaz.

Meaningful görev başlangıcında pre-existing working tree korunur.

Açık talimat olmadan şu destructive komutları kullanma:

~~~text
git reset --hard
git clean -fd
git restore <file>
git checkout -- <file>
~~~

Unrelated kullanıcı değişikliklerini sahiplenme, stage etme, overwrite etme veya revert etme.

## 8. Scope Discipline

Görevin gerektirmediği architecture refactor, framework ekleme, abstraction üretme veya "ileride lazım olur" işi yapma.

Yeni ürün/domain/mimari kararı gerekiyorsa mevcut docs sözleşmesini sessizce değiştirme. Çelişkiyi veya yeni karar ihtiyacını kullanıcıya açıkça bildir.

## 9. Cost Discipline

Ana hedef:

~~~text
minimum gerekli model
minimum gerekli reasoning effort
minimum gerekli context
minimum gereksiz tool kullanımı
maximum doğrulanabilirlik
~~~

Kolay görevde Sol seçme.

Zor veya yüksek riskli görevi sırf ucuzluk için Luna'ya zorlama.
