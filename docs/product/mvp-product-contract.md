# GitHub Proof — MVP Ürün Sözleşmesi

## 1. Ürün Tanımı

GitHub Proof, public bir GitHub repository'sinde dışarıdan gözlemlenebilen mühendislik kanıtlarını analiz ederek açıklanabilir bir rapor üreten açık kaynak bir araçtır.

GitHub Proof bir geliştiricinin genel yetkinliğini, işe alınabilirliğini veya kıdem seviyesini ölçmez.

Sistem yalnızca analiz edilen repository içerisinde erişilebilen veriler üzerinden değerlendirme yapar.

---

## 2. MVP Kullanıcı Problemi

Bir geliştirici GitHub üzerinde proje yayınlamış olabilir; ancak repository dışarıdan incelendiğinde şu soruların cevapları görünmeyebilir:

- Proje hangi problemi çözüyor?
- Başka biri projeyi çalıştırabilir mi?
- Geliştirme süreci repository üzerinde görülebiliyor mu?
- Test, doğrulama ve otomasyon sinyalleri mevcut mu?
- Repository portföy amacıyla anlaşılır biçimde sunulmuş mu?
- Repository hangi mühendislik pratiklerini gerçekten kanıtlıyor?

GitHub Proof, bu sorulara gözlemlenebilir kanıtlar üzerinden cevap vermeyi amaçlar.

---

## 3. MVP Değer Önerisi

Kullanıcı public bir GitHub repository URL'si verir.

Sistem:

1. Repository verilerini toplar.
2. Açıklanabilir sinyalleri değerlendirir.
3. Sonuçları dört analiz kategorisinde gösterir.
4. Her sonucun dayandığı kanıtı açıklar.
5. Repository için üç öncelikli geliştirme önerisi üretir.
6. Web raporu ve Markdown çıktısı sunar.

---

## 4. MVP Hedef Kullanıcıları

İlk sürüm aşağıdaki kullanıcı gruplarına odaklanır:

- Yazılıma yeni başlayanlar
- Junior geliştiriciler
- Portföy hazırlayan geliştiriciler
- Kariyer değiştiren yazılım adayları
- Eğitmenler ve mentorlar
- Teknik repository incelemesi yapan kişiler

MVP, kurumsal işe alım karar sistemi olarak konumlandırılmaz.

---

## 5. MVP Analiz Kategorileri

### 5.1 Repository Anlaşılabilirliği

Repository'nin amacı, kurulumu ve kullanımı dışarıdan anlaşılabiliyor mu?

### 5.2 Geliştirme Süreci Görünürlüğü

Commit, issue, pull request ve release gibi geliştirme süreci sinyalleri görülebiliyor mu?

### 5.3 Doğrulama ve Kullanılabilirlik

Başka bir geliştirici projeyi çalıştırabilir, doğrulayabilir ve geliştirmeye devam edebilir mi?

### 5.4 Portföy Sunumu

Repository, yapılan üretimi anlaşılır ve profesyonel biçimde sunabiliyor mu?

---

## 6. MVP Çıktısı

Her analiz şu bölümleri içermelidir:

- Repository özeti
- Toplam görünür kanıt skoru
- Dört kategori sonucu
- Sinyal bazlı puan ve kanıt açıklaması
- En güçlü üç sinyal
- En kritik üç eksik
- Üç öncelikli geliştirme adımı
- Markdown raporu

Toplam skor tek başına gösterilmemeli; dayandığı sinyallerle birlikte açıklanmalıdır.

---

## 7. MVP'nin Yapmayacağı Şeyler

GitHub Proof MVP:

- Geliştiricinin seviyesini belirlemez.
- İşe alınabilirlik kararı vermez.
- Junior, mid veya senior sınıflandırması yapmaz.
- Kodun AI tarafından yazılıp yazılmadığını tespit etmez.
- Kod kalitesini bütünüyle değerlendirmez.
- Güvenlik denetimi gerçekleştirmez.
- Mimari olgunluk hakkında kesin hüküm vermez.
- Private repository analiz etmez.
- Birden fazla repository'yi karşılaştırmaz.
- Kullanıcıları veya geliştiricileri sıralamaz.
- Recruiter adına nihai karar vermez.

---

## 8. Açıklanabilirlik İlkesi

Her analiz sonucu şu dört soruya cevap vermelidir:

1. Hangi sinyal değerlendirildi?
2. Hangi kanıt bulundu veya bulunamadı?
3. Bu kanıt neden önemli?
4. Kullanıcı sonucu geliştirmek için ne yapabilir?

Kanıtı açıklanamayan bir puan MVP sonucuna dahil edilmemelidir.

---

## 9. Tekrarlanabilirlik İlkesi

Aynı repository:

- Aynı veri,
- Aynı analiz zamanı,
- Aynı kural seti

ile tekrar analiz edildiğinde aynı sonucu üretmelidir.

LLM veya yoruma dayalı kara kutu kararlar MVP puanlama çekirdeğinde kullanılmamalıdır.

---

## 10. MVP Tamamlanma Tanımı

MVP tamamlanmış sayılır:

- Public GitHub repository URL'si kabul edilebiliyor.
- Repository verileri GitHub API üzerinden toplanabiliyor.
- 15 açıklanabilir MVP sinyali değerlendiriliyor.
- Sonuçlar dört kategori altında gösteriliyor.
- Her skorun dayandığı kanıt açıklanıyor.
- Üç öncelikli geliştirme önerisi sunuluyor.
- Web raporu ve Markdown çıktısı üretilebiliyor.
- En az 20 gerçek public repository ile doğrulama yapılmış oluyor.

---

## 11. Başarı Ölçütü

MVP'nin başarısı yalnızca analiz üretebilmesiyle ölçülmez.

Temel başarı sinyali:

> Kullanıcının raporu aldıktan sonra repository'sinde anlamlı ve doğrulanabilir bir iyileştirme yapmasıdır.
