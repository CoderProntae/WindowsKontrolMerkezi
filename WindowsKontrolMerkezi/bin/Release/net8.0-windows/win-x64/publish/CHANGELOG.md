# Değişiklik Günlüğü (CHANGELOG)

## [1.3.8] - 2025-02-23
### Değişenler
- **Tema Fix**: Temaların seçilince değişmeme sorunu kökten çözüldü.
- **Kararlılık**: Küçük kod iyileştirmeleri yapıldı.

## [1.3.7] - 2025-02-23
### Değişenler
- **Görünüm Fix**: Değişiklik günlüğündeki (Changelog) HTML kodlarının ham metin olarak görünmesi sorunu düzeltildi.
- **Küçük Güncelleme Amaçlı**: Sistem senkronizasyonu güçlendirildi.

## [1.3.6] - 2025-02-23
### Eklenenler
- **Evrensel Güncelleme Sistemi**: Artık `.zip` paketleri üzerinden tüm uygulama dosyaları (`CHANGELOG.md`, `version.txt` vb.) tek seferde güncellenebiliyor.
- **PowerShell Entegrasyonu**: Güncelleme çıkarma işlemi Windows PowerShell ile daha güvenli hale getirildi.

## [1.3.5] - 2025-02-23
### Eklenenler
- **Özel Temalar**: 🐠 Balıklar, 🌋 Lav ve 🌅 Gün Batımı temaları eklendi.
- **Tema Gruplama**: ComboBox listesi "Standart" ve "Özel" olarak kategorize edildi.
### Değişenler
- **Derin Tema Senkronizasyonu**: Scrollbarlar ve UI başlıkları artık tamamen karanlık mod ve accent renkleriyle uyumlu.
- **Geçmiş Fix**: Bildirim geçmişini temizleme işlemi artık UI üzerinde anında yenileniyor.

## [1.3.4] - 2025-02-23
### Değişenler
- **ScrollBar Revizyonu**: Başlangıç çökmesini önlemek için scrollbar stili sadeleştirildi ve stabilize edildi.
- **Bildirim Silme**: "Tümünü Temizle" artık bildirimleri silmek yerine geçmişe taşır.

## [1.3.3] - 2025-02-23
### Değişenler
- **Sürüm Senkronizasyonu**: Güncelleme sonrası eski sürümün kalmasına neden olan `version.txt` sorunu giderildi.

## [1.3.2] - 2025-02-23
### Değişenler
- **Tema Senkronizasyonu**: Scrollbar ve liste seçim renkleri uygulama temasıyla tam uyumlu hale getirildi (Mavi renkler kaldırıldı).
- **Görsel İyileştirmeler**: Panel kaydırma çubukları ve düğmeler tema renklerine senkronize edildi.

### Kaldırılanlar
- !!! "Git" özelliği artık desteklenmiyor !!!

---

## [1.3.1] - 2025-02-23
### Eklenenler
- Bildirim geçmişi kapalıyken saklama süresi slider'ı dinamik olarak saydamlaşır.
- Yan menü butonlarına Scale animasyonları eklendi.

---

## [1.3.0] - 2025-02-23
### Eklenenler
- Bağımsız Rahatsız Etme (DND) modu.
- Bildirim kalıcılığı (JSON Storage).
- Bildirim geçmişi sekmesi.
