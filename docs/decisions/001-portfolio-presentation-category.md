# ADR 001 — Portföy Sunumu Kategorisinin Korunması

- Durum: Kabul edildi
- Tarih: 2026-08-07
- Karar sahipliği: GitHub Proof
- İlgili faz: Faz 1 — Analiz Modeli

---

## 1. Bağlam

GitHub Proof ürün sözleşmesinde dört analiz kategorisi tanımlanmıştır:

1. Repository anlaşılabilirliği
2. Geliştirme süreci görünürlüğü
3. Doğrulama ve kullanılabilirlik
4. Portföy sunumu

İlk MVP sinyal taslağında ise yalnızca ilk üç kategoriye ait toplam 12 bağımsız sinyal tanımlanmıştır.

Portföy sunumu kategorisi için bağımsız sinyal bulunmaması şu sorunları oluşturur:

- Dört kategorili ürün vaadi ile analiz modeli uyuşmaz.
- Portföy sunumu skoru açıklanabilir kanıtlardan üretilemez.
- Kategori, başka kategorilerdeki sinyallerin tekrarından oluşabilir.
- Aynı sinyal birden fazla kategoride sayılarak toplam skoru yapay biçimde etkileyebilir.
- Rapor sözleşmesindeki dört kategori sonucu üretilemez.

Bu nedenle portföy sunumu kategorisinin kaldırılması, türetilmiş bir görünüm hâline getirilmesi veya bağımsız sinyallerle desteklenmesi değerlendirilmiştir.

---

## 2. Değerlendirilen Seçenekler

### Seçenek A — MVP kategori sayısını üçe indirmek

Portföy sunumu kategorisi MVP kapsamından çıkarılır.

#### Avantajlar

- Daha küçük analiz kapsamı
- Daha az GitHub verisi
- Daha hızlı ilk uygulama

#### Dezavantajlar

- Ürün sözleşmesinin ve README'nin değiştirilmesi gerekir.
- Projenin portföy değerine ilişkin ana kullanıcı problemi zayıflar.
- Kullanıcıya sunulan rapor yalnızca teknik repository sağlığına yaklaşır.
- GitHub Proof'un işe ve portföye dönük değer önerisi daralır.

---

### Seçenek B — Portföy skorunu diğer sinyallerden türetmek

README, demo, description ve release gibi mevcut sinyaller portföy görünümünde tekrar kullanılır.

#### Avantajlar

- Yeni sinyal gerekmez.
- İlk uygulama daha küçük kalır.

#### Dezavantajlar

- Aynı kanıt birden fazla kategoride tekrar kullanılabilir.
- Kategori skoru bağımsız ve açıklanabilir olmaz.
- Toplam puanda çift sayım riski oluşur.
- Domain modelindeki kategori-sinyal ilişkisi belirsizleşir.
- Kategori sonucu kendi sinyallerinden türetilmelidir ilkesini zayıflatır.

---

### Seçenek C — Portföy kategorisini bağımsız sinyallerle korumak

Portföy sunumu kategorisine üç bağımsız MVP sinyali eklenir.

#### Avantajlar

- Ürün sözleşmesi ile analiz modeli uyumlu kalır.
- Dört kategori de bağımsız kanıtlara dayanır.
- Portföy amacı doğrudan ölçülebilir görünür sinyallerle ele alınır.
- Aynı sinyalin birden fazla kategoride puanlanması engellenir.
- Rapor daha anlaşılır ve dengeli olur.

#### Dezavantajlar

- MVP sinyal sayısı 12'den 15'e çıkar.
- Üç yeni sinyalin veri ve puanlama kuralları gerekir.
- Repository sunum bütünlüğü sinyalinin öznel hâle gelmemesi için dikkatli sınırlar gerekir.

---

## 3. Karar

Portföy sunumu kategorisi korunacaktır.

MVP analiz çekirdeği en az 12 sinyal yerine **15 bağımsız sinyal** içerecektir.

Portföy sunumu kategorisine aşağıdaki üç sinyal eklenecektir:

1. GitHub Topics
2. Homepage veya canlı demo bağlantısı
3. Repository sunum bütünlüğü

Her sinyal yalnızca Portföy Sunumu kategorisine ait olacaktır.

Mevcut sinyaller birden fazla kategori altında puanlanmayacaktır.

---

## 4. Yeni Sinyaller

### 4.1 GitHub Topics

Repository'nin teknoloji, ürün veya problem alanını GitHub metadata üzerinde sınıflandırıp sınıflandırmadığını değerlendirir.

Gözlemlenebilir kanıt:

- Repository topics listesi

Sınır:

Topics bulunması, ilgili teknolojilerin doğru veya etkin kullanıldığını kanıtlamaz.

---

### 4.2 Homepage veya Canlı Demo Bağlantısı

Repository metadata veya açık dokümantasyon içerisinde çalışan ürüne, ayrıntılı dokümantasyona veya canlı demoya yönlendiren bağlantı bulunup bulunmadığını değerlendirir.

Gözlemlenebilir kanıt:

- GitHub homepage alanı
- README içerisindeki açık demo bağlantısı

Sınır:

Bağlantının bulunması erişilebilirliği, güncelliği veya ürün kalitesini garanti etmez.

---

### 4.3 Repository Sunum Bütünlüğü

Repository'nin temel sunum parçalarını birlikte taşıyıp taşımadığını deterministik bir kontrol listesi üzerinden değerlendirir.

İlk MVP kontrol listesi:

- Repository description mevcut
- README mevcut
- Kullanım örneği veya demo mevcut
- Lisans mevcut
- En az bir GitHub topic mevcut

Bu sinyal yeni ve öznel bir içerik analizi üretmez.

Yalnızca hâlihazırda gözlemlenen sunum parçalarının birlikte bulunma oranını değerlendirir.

Sınır:

Sunum bütünlüğü, teknik kaliteyi veya geliştirici seviyesini kanıtlamaz.

---

## 5. Çift Sayım Kararı

Repository sunum bütünlüğü, mevcut sinyallerden türetilen bileşik bir sinyal olduğu için toplam skorda çift sayım riski taşır.

Bu nedenle şu kural kabul edilmiştir:

> Repository sunum bütünlüğü kategori içi açıklama ve özet sinyali olarak kullanılacak; MVP toplam görünür kanıt skoruna puan katkısı yapmayacaktır.

Sinyalin maksimum puanı kategori görünümünde kullanılabilir ancak toplam skor hesabında `ExcludedFromOverallScore` olarak işaretlenmelidir.

Alternatif olarak ilk uygulamada yalnızca durum ve açıklama üretebilir.

Bu davranış domain modelinde açık bir alanla temsil edilmelidir.

Önerilen alan:

~~~text
SignalDefinition
└── IncludeInOverallScore
~~~

---

## 6. Sonuçlar

Bu kararla:

- MVP sinyal sayısı 15 olur.
- Dört analiz kategorisinin tamamı bağımsız sinyallere sahip olur.
- Ürün sözleşmesi korunur.
- README ve örnek raporun dört kategorili yapısı değişmez.
- Signal Engine üç ilave evaluator içerir.
- GitHub metadata veri ihtiyacına topics ve homepage alanları kesin olarak eklenir.
- Domain modeline toplam skora dahil edilme davranışı eklenir.
- İlk 12 sinyal ifadesi, ilgili belgelerde ilk 15 MVP sinyali olarak güncellenir.

---

## 7. Uygulama Sınırları

- Topics içeriğinin doğruluğu analiz edilmeyecektir.
- Homepage veya demo bağlantısının erişilebilirlik kontrolü MVP'de zorunlu değildir.
- Repository adının kalitesi otomatik olarak puanlanmayacaktır.
- Görsel tasarım veya estetik kalite değerlendirilmez.
- Star, fork veya follower sayıları portföy kalitesi sinyali olarak kullanılmaz.
- Repository popülerliği puana dahil edilmez.
- Sosyal kanıt, geliştirici seviyesi veya işe alınabilirlik çıkarımı yapılmaz.

---

## 8. Doğrulama Kriterleri

Karar uygulanmış sayılır:

- Portföy sunumu kategorisine üç sinyal eklenmişse
- MVP sinyal sayısı 15 olarak güncellenmişse
- Kategori dağılımı dört kategori için sıfırdan büyükse
- Her yeni sinyal gözlemlenebilir GitHub verisine dayanıyorsa
- Repository sunum bütünlüğünün toplam skora dahil edilme davranışı açıkça tanımlanmışsa
- Aynı sinyal birden fazla kategoride puanlanmıyorsa
- İlgili ürün ve mimari belgelerdeki çelişkiler giderilmişse
