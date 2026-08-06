# GitHub Proof — Doğrulama Repository Seti

Bu doküman, GitHub Proof MVP analiz kurallarının hangi repository türleri üzerinde doğrulanacağını ve test setinin nasıl oluşturulacağını tanımlar.

Amaç popüler repository'leri puanlamak değil, farklı veri koşullarında analiz motorunun tutarlı, açıklanabilir ve adil sonuç üretip üretmediğini sınamaktır.

---

## 1. Doğrulama Amacı

Doğrulama repository seti şu sorulara cevap vermelidir:

1. Aynı repository aynı kural setiyle aynı sonucu üretiyor mu?
2. Eksik kanıt ile erişilemeyen veri doğru ayrılıyor mu?
3. `NotApplicable` sinyalleri doğru işleniyor mu?
4. Farklı teknoloji ekosistemlerinde anlamsız cezalar oluşuyor mu?
5. Küçük repository'ler otomatik olarak düşük kaliteli kabul ediliyor mu?
6. Büyük ve popüler repository'ler yalnızca hacimleri nedeniyle avantaj kazanıyor mu?
7. Arşivlenmiş repository'ler yanlış biçimde başarısız sayılıyor mu?
8. Tek commit içeren repository'ler hakkında yasaklı çıkarımlar yapılıyor mu?
9. Üretilen öneriler uygulanabilir ve kanıta bağlı mı?
10. İnsan değerlendirmesi ile otomatik sonuç arasında ciddi çelişkiler var mı?

---

## 2. İlk Doğrulama Setinin Büyüklüğü

Faz 0 ve Faz 1 sırasında en az 10 repository kullanılmalıdır.

MVP doğrulama aşamasında bu set en az 20 repository'ye çıkarılmalıdır.

İlk 10 repository şu dağılımı taşımalıdır:

| Repository profili | Adet |
|---|---:|
| Güçlü dokümantasyona sahip aktif proje | 1 |
| Zayıf veya çok kısa README içeren proje | 1 |
| Tek commit ile yüklenmiş proje | 1 |
| Zamana yayılmış commit geçmişi bulunan proje | 1 |
| Test ve CI içeren proje | 1 |
| Test ve CI içermeyen proje | 1 |
| Arşivlenmiş fakat tamamlanmış proje | 1 |
| Çok küçük ve tek amaçlı araç | 1 |
| Eğitim veya örnek amaçlı repository | 1 |
| Boş ya da yetersiz veri taşıyan repository | 1 |

---

## 3. Teknoloji Dağılımı

Doğrulama seti yalnızca .NET repository'lerinden oluşmamalıdır.

İlk 10 repository içerisinde mümkün olduğunca şu ekosistemlerden örnek bulunmalıdır:

- C# / .NET
- JavaScript veya TypeScript
- Python
- Java veya Kotlin
- Go veya Rust
- Teknolojiden bağımsız dokümantasyon ya da araç repository'si

Bir sinyal yalnızca belirli bir teknoloji için geçerliyse bu durum kural tanımında açıkça belirtilmelidir.

---

## 4. Repository Seçim İlkeleri

Repository seçilirken aşağıdaki kurallara uyulmalıdır:

- Yalnızca public repository kullanılmalıdır.
- Repository URL'si ve analiz edilen commit SHA kaydedilmelidir.
- Test seti yalnızca çok popüler projelerden oluşmamalıdır.
- Kişisel portföy repository'leri ile topluluk projeleri dengelenmelidir.
- Fork repository'ler ayrıca işaretlenmelidir.
- Template veya generated repository'ler ayrıca işaretlenmelidir.
- Arşivlenmiş repository'ler test setinden çıkarılmamalıdır.
- Boş veya yetersiz repository örnekleri mutlaka bulunmalıdır.
- Repository sahibinin genel geliştirici seviyesi hakkında yorum yapılmamalıdır.
- Test amacıyla repository üzerinde değişiklik yapılmamalıdır.

---

## 5. Repository Kayıt Şeması

Her doğrulama repository'si aşağıdaki bilgilerle kaydedilmelidir:

~~~text
ValidationRepository
├── Id
├── Owner
├── Name
├── Url
├── CommitSha
├── PrimaryLanguage
├── RepositoryProfile
├── IsArchived
├── IsFork
├── IsTemplate
├── ExpectedEdgeCases
├── ManualReviewStatus
└── Notes
~~~

---

## 6. Manuel Değerlendirme Formu

Her repository otomatik analizden önce veya bağımsız olarak en az bir insan tarafından incelenmelidir.

Manuel değerlendirme şu alanları içermelidir:

### Repository anlaşılabilirliği

- Projenin amacı anlaşılabiliyor mu?
- Kurulum adımları bulunuyor mu?
- Kullanım örneği mevcut mu?
- Lisans ve katkı koşulları görünür mü?

### Geliştirme süreci görünürlüğü

- Commit geçmişi anlamlı bir süreç gösteriyor mu?
- Issue veya pull request kullanımı bağlama uygun mu?
- Release eksikliği gerçekten önemli mi?
- Repository sonradan tek seferde taşınmış olabilir mi?

### Doğrulama ve kullanılabilirlik

- Projenin çalıştırılması için yeterli bilgi var mı?
- Test yapısı gerçekten test amacı taşıyor mu?
- CI workflow dosyası anlamlı bir doğrulama yapıyor mu?
- Configuration örnekleri yeterli mi?

### Portföy sunumu

- Repository dışarıdan incelendiğinde amacı ve sonucu görünür mü?
- Demo veya görsel gerçekten gerekli mi?
- Repository'nin küçük olması kullanım amacına uygun mu?
- Arşiv durumu doğru yorumlanıyor mu?

---

## 7. Manuel Değerlendirme Sonuçları

İnsan değerlendirmesi doğrudan nihai doğru kabul edilmemelidir.

Amaç, otomatik analiz ile insan yorumu arasındaki farkları görünür hâle getirmektir.

Her sinyal için şu sonuçlardan biri seçilmelidir:

~~~text
Agreement
MinorDifference
MajorDifference
CannotDetermine
~~~

### Agreement

Otomatik sonuç ile manuel değerlendirme aynı anlamı taşımaktadır.

### MinorDifference

Sonuç genel olarak doğrudur ancak açıklama, puan veya önem derecesinde küçük fark vardır.

### MajorDifference

Otomatik analiz yanıltıcı, haksız veya bağlam dışı sonuç üretmiştir.

### CannotDetermine

İnsan değerlendirmesi de yeterli kanıt nedeniyle kesin sonuç üretememiştir.

---

## 8. Kritik Test Senaryoları

### 8.1 Tek Commit Repository

Beklenen davranış:

- Geliştirme sürecinin sınırlı görünür olduğu söylenebilir.
- AI kullanımı veya kopyalama iddiasında bulunulmamalıdır.
- Repository'nin başka bir sistemden taşınmış olabileceği belirtilmelidir.

### 8.2 Arşivlenmiş Repository

Beklenen davranış:

- Arşiv durumu raporda görünür olmalıdır.
- Repository başarısız veya değersiz olarak etiketlenmemelidir.
- Son güncelleme tarihinin düşük puana otomatik dönüşmesi engellenmelidir.

### 8.3 Küçük Tek Amaçlı Araç

Beklenen davranış:

- Küçük dosya veya commit sayısı otomatik olarak eksiklik sayılmamalıdır.
- Karmaşık branch, issue veya release süreçleri zorunlu kabul edilmemelidir.
- Repository amacıyla orantılı kanıt aranmalıdır.

### 8.4 Eğitim Repository'si

Beklenen davranış:

- Production-ready olmadığı için cezalandırılmamalıdır.
- Eğitim amacı görünürse rapor bağlamı buna göre açıklanmalıdır.
- Portföy sunumu ile production sinyalleri birbirine karıştırılmamalıdır.

### 8.5 Boş Repository

Beklenen davranış:

~~~text
AnalysisStatus: InsufficientData
~~~

- Sahte bir toplam skor üretilmemelidir.
- Veri yetersizliği açıkça gösterilmelidir.

### 8.6 API Verisi Eksik Repository

Beklenen davranış:

~~~text
AnalysisStatus: Partial
~~~

- Erişilemeyen veri `Missing` olarak değerlendirilmemelidir.
- Etkilenen sinyaller `UnavailableEvidence` veya eşdeğer durumla gösterilmelidir.

### 8.7 Fork Repository

Beklenen davranış:

- Fork durumu görünür olmalıdır.
- Commit ve katkı geçmişi repository sahibinin özgün üretimi olarak sunulmamalıdır.
- Geliştiricinin sahipliği hakkında kesin hüküm verilmemelidir.

### 8.8 Template veya Generated Repository

Beklenen davranış:

- Başlangıç yapısının template veya generator kaynaklı olabileceği belirtilmelidir.
- Dosya yapısı tek başına güçlü mühendislik kanıtı sayılmamalıdır.
- Repository'nin sonraki gelişim kanıtları ayrıca değerlendirilmelidir.

---

## 9. Kabul Eşikleri

İlk 10 repository doğrulamasında hedeflenen sonuçlar:

- Analizlerin %100'ü teknik olarak tamamlanmalı veya açıklanabilir bir hata durumu üretmelidir.
- Sinyallerin en az %80'i manuel değerlendirme ile `Agreement` veya `MinorDifference` sonucunda olmalıdır.
- Hiçbir repository için yasaklı çıkarım üretilmemelidir.
- `Missing`, `UnavailableEvidence` ve `NotApplicable` durumları doğru ayrılmalıdır.
- Aynı snapshot üzerinde tekrar analizler aynı skoru üretmelidir.
- Her kritik eksik kanıt ve uygulanabilir öneri taşımalıdır.
- En az üç farklı teknoloji ekosisteminde temel sinyaller anlamlı kalmalıdır.

---

## 10. Başarısızlık Koşulları

Aşağıdaki durumlardan biri tekrar ediyorsa ilgili sinyal MVP'ye kabul edilmemelidir:

- Repository bağlamından bağımsız haksız puan üretiyor.
- Küçük projeleri sistematik olarak cezalandırıyor.
- Belirli bir teknoloji ekosistemini gereksiz biçimde avantajlı hâle getiriyor.
- Eksik API verisini eksik mühendislik kanıtı olarak yorumluyor.
- İnsan değerlendirmesinde sürekli `MajorDifference` sonucu alıyor.
- Kanıtı raporda gösterilemiyor.
- Somut bir geliştirme önerisi üretilemiyor.
- Yasaklı çıkarımlardan birine yol açıyor.
- Aynı veri üzerinde farklı sonuç üretiyor.

---

## 11. İlk Repository Listesinin Kaydı

Gerçek repository listesi ayrı bir veri dosyasında tutulmalıdır.

Planlanan konum:

~~~text
samples/validation/repositories.json
~~~

Bu dosya ilk aşamada aşağıdaki alanları taşımalıdır:

~~~json
[
  {
    "id": "validation-001",
    "owner": "sample-owner",
    "name": "sample-repository",
    "url": "https://github.com/sample-owner/sample-repository",
    "commitSha": "COMMIT_SHA",
    "primaryLanguage": "C#",
    "repositoryProfile": "strong-documentation",
    "isArchived": false,
    "isFork": false,
    "isTemplate": false,
    "expectedEdgeCases": [],
    "manualReviewStatus": "Pending",
    "notes": ""
  }
]
~~~

Gerçek repository'ler Faz 1 başlamadan önce seçilmeli ve commit SHA değerleri sabitlenmelidir.

---

## 12. Doğrulama Çıktıları

Her doğrulama çalışması aşağıdaki çıktıları üretmelidir:

- Repository snapshot özeti
- Otomatik analiz raporu
- Manuel değerlendirme formu
- Sinyal bazlı uyum sonucu
- Major difference kayıtları
- Kural değişikliği önerileri
- Yeniden test sonucu

Planlanan konum:

~~~text
samples/validation/results/
├── validation-001/
│   ├── snapshot.json
│   ├── automated-report.md
│   ├── manual-review.md
│   └── comparison.md
└── ...
~~~

---

## 13. Faz 0 Çıkış Kriteri

Doğrulama repository seti tanımı tamamlanmış sayılır:

- İlk setin büyüklüğü belirlenmişse
- Repository profil dağılımı tanımlanmışsa
- Teknoloji çeşitliliği belirlenmişse
- Manuel değerlendirme yöntemi tanımlanmışsa
- Kritik test senaryoları yazılmışsa
- Kabul ve başarısızlık eşikleri belirlenmişse
- Repository kayıt şeması tanımlanmışsa
- Gerçek repository listesinin saklanacağı konum belirlenmişse
