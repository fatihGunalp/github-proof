# Model Routing Gate

## Amaç

Her görevi gereksiz pahalı modelle çalıştırmamak, fakat karmaşık veya riskli işi de yetersiz modele vermemek.

Codex her **yeni meaningful repository görevi** için önce model + reasoning effort önerir ve kullanıcı `devam et` demeden uygulamaya başlamaz.

## Kullanılacak Model Ailesi

Project routing yalnız şu üç capability tier üzerinden yapılır:

~~~text
Luna  = hızlı ve en düşük maliyetli; açık, mekanik, tekrar edilebilir işler
Terra = günlük coding workhorse; maliyet/yetenek dengesi
Sol   = en karmaşık, belirsiz, yüksek riskli veya uzun-horizon işler
~~~

Reasoning effort için project varsayılan seçenekleri:

~~~text
low
medium
high
~~~

`xhigh`, `max` veya `ultra` bu project'in normal routing tablosunun parçası değildir. Kullanıcı açıkça isterse ayrıca değerlendirilebilir.

## Routing Prensibi

En düşük yeterli model + en düşük yeterli reasoning effort seçilir.

Model seçimini dosya sayısına tek başına bağlama. Şunları birlikte değerlendir:

- Gereksinim açıklığı
- Domain invariant sayısı
- Değişiklik alanı ve katman sayısı
- Hata maliyeti
- Veri/migration etkisi
- Security veya concurrency riski
- Mevcut test güvencesi
- Debug belirsizliği
- Yeni karar üretme gereksinimi
- Geri dönüş zorluğu

## Routing Tablosu

### Luna / low

Şunlar için tercih et:

- küçük ve kesin doküman düzeltmesi
- formatlama / typo / isim düzeltme
- açıkça tarif edilmiş mekanik rename
- tek ve basit config değeri değişikliği
- status/diff gibi read-only kontrol
- kullanıcı tarafından çözümü net tanımlanmış çok küçük patch

Koşul:

~~~text
düşük belirsizlik
+
düşük risk
+
minimum karar üretimi
~~~

### Luna / medium

Şunlar için tercih edilebilir:

- tek dosya veya çok küçük bounded implementation
- mevcut pattern'i birebir izleyen küçük test ekleme
- açık acceptance criteria olan küçük bug fix
- küçük deterministic mapping/table değişikliği

Terra gerektiren domain veya cross-layer karar yoksa kullan.

### Terra / low

Şunlar için tercih edilebilir:

- birkaç dosyada mekanik ama dikkat gerektiren değişiklik
- mevcut pattern'e dayalı küçük refactor
- açık contract'a göre boilerplate/domain primitive üretimi
- basit ve sınırları belli test genişletmesi

### Terra / medium

Normal coding için **default workhorse** budur.

Şunlar için tercih et:

- bounded feature implementation
- birkaç class + test içeren görev
- tek katman içinde non-trivial domain davranışı
- bilinen architecture içinde yeni use-case
- orta zorlukta bug fix
- docs'tan deterministic kod üretimi
- test + implementation + küçük refactor kombinasyonu

### Terra / high

Şunlar için tercih et:

- birden fazla invariant'ı aynı anda koruyan değişiklik
- birkaç modül/katman etkileyen ancak architecture'ı değiştirmeyen iş
- karmaşık test tasarımı
- belirsiz fakat bounded debugging
- scoring/normalization gibi hata maliyeti yüksek deterministic logic
- mevcut modeli anlamayı ve tradeoff kontrolünü gerektiren refactor

### Sol / medium

Şunlar için tercih et:

- yeni bir architecture veya domain sınırının docs'tan uygulanması
- cross-layer feature tasarımı
- geniş code review ve consistency analizi
- karmaşık hatanın root-cause analizi
- birden fazla olası doğru yaklaşım arasında seçim gereken iş

### Sol / high

Şunlar için tercih et:

- foundation architecture değişikliği
- kapsamlı domain model redesign
- büyük refactor veya migration
- data integrity riski
- concurrency / distributed workflow
- security-sensitive değişiklik
- çok sayıda invariant ve edge case
- contract drift + code drift birlikte çözümü
- geniş ve belirsiz debugging
- geri dönüşü pahalı kararlar

## GitHub Proof İçin Özel Routing Örnekleri

~~~text
Enum/value object boilerplate
→ Terra / low

SignalResult invariant'ları + unit tests
→ Terra / medium

ScoreBreakdown normalization engine
→ Terra / high

15 evaluator'ın contract'tan toplu tasarımı
→ Sol / medium veya Terra / high; kapsam ve mevcut altyapıya göre

RepositorySnapshot architecture + GitHub normalization boundary
→ Sol / medium

Scoring contract veya domain semantics değişikliği
→ Sol / high

README typo
→ Luna / low
~~~

## Model Gate Çıktısı

Yeni görev geldiğinde **önce yalnız routing sonucu ver**.

Format:

~~~text
MODEL SEÇİMİ
Model: Terra
Reasoning: medium

Gerekçe:
<1-3 kısa cümle>

Beklenen çalışma:
<çok kısa kapsam>

Modeli Terra / medium olarak ayarla.
Devam etmek için: devam et
~~~

Bu mesajdan sonra DUR.

Kullanıcı `devam et` demeden implementation başlatma.

## Gate Öncesi İzin Verilen Triage

Model seçimi yalnız prompttan güvenilir biçimde yapılamıyorsa en fazla şu read-only kontroller kullanılabilir:

~~~text
git status --short
ilgili dosya/path var mı kontrolü
çok küçük directory listing
worklog/CURRENT.md başlıkları (yalnız GitHub Proof'a ait olduğu doğrulanmışsa)
~~~

Gate öncesi:

- tüm docs'u okuma
- tüm repository'yi tarama
- build/test çalıştırma
- geniş grep/search yapma
- kod değiştirme

Amaç routing için token harcamamaktır.

## Escalation Kuralı

Uygulama sırasında görev beklenenden daha zor/riskli hâle gelirse mevcut modeli zorlamaya çalışma.

Güvenli checkpoint'te DUR ve şöyle raporla:

~~~text
MODEL YÜKSELTME ÖNERİSİ
Mevcut: Terra / medium
Önerilen: Sol / high
Neden: <kısa neden>

Modeli Sol / high olarak ayarla.
Devam etmek için: devam et
~~~

Kullanıcı `devam et` diyene kadar ileri gitme.

## Downgrade Kuralı

Aynı aktif görev sırasında daha ucuz modele geçmek için gereksiz stop üretme.

Yeni bağımsız görev başladığında yeniden route et.

## Belirsizlik

İki tier arasında kararsızsan:

- düşük risk ve kolay geri dönüş → daha ucuz olanı seç
- domain/data/security riski → daha güçlü olanı seç

Sol'u "emin değilim" diye otomatik default yapma. Önce görev riskini değerlendir.
