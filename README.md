# Windows Kontrol Merkezi

Native Windows masaüstü uygulaması (C# + WPF). Sistem izleme, modlar ve güncelleme kontrolü.

## Özellikler

- **Panel**: CPU, RAM ve Disk (C:) kullanımı canlı; halka grafikler ve mini çubuk grafik
- **Modlar**: Oyun Modu ayarları, güç planı (Yüksek performans / Dengeli), Odak yardımı (sessiz mod)
- **Sürüm & güncelleme**: Sürüm bilgisi, güncelleme kontrolü, değişiklik günlüğü (CHANGELOG.md)

## Gereksinimler

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (geliştirme için)

## Çalıştırma

```bash
dotnet run --project WindowsKontrolMerkezi
```

Veya önce derleyip exe’yi çalıştır:

```bash
dotnet build WindowsKontrolMerkezi\WindowsKontrolMerkezi.csproj
# Çıktı: WindowsKontrolMerkezi\bin\Debug\net8.0-windows\WindowsKontrolMerkezi.exe
```

## Yayın (tek exe)

```bash
dotnet publish WindowsKontrolMerkezi\WindowsKontrolMerkezi.csproj -c Release -r win-x64 --self-contained
```

**GitHub üzerinden otomatik yayın**

1. `git commit` ile değişiklikleri kaydet.
2. `git tag vX.Y.Z` şeklinde yeni sürüm etiketi oluşturun.
3. `git push && git push --tags` ile ana dalı ve etiketi gönderin.

Action workflow (`.github/workflows/release.yml`) tetiklenecek; derleme yapılacak, ZIP paketlenecek ve GitHub Release oluşturulacak. Manifest (`version.json`) da hash ile birlikte güncellenecek ve otomatik olarak repo'ya itilecektir.

Çıktı: `WindowsKontrolMerkezi\bin\Release\net8.0-windows\win-x64\publish\` — bu klasörde **WindowsKontrolMerkezi.exe** bulunur. Bu exe ile çalıştırdığında:

- **Windows ile başlat** (Ayarlar > Özelleştirme) exe yolunu kayıt defterine yazar; böylece Windows açılışında uygulama exe ile başlar.
- **Güncelle (indir ve kur)** düğmesi güncelleme dosyasını indirir, çalıştırır ve uygulamayı kapatır; kurulumu sen tamamlarsın.

## Güncelleme mantığı

1. **Sürüm**: Teorik olarak `WindowsKontrolMerkezi.csproj` içindeki `<Version>` kullanılır ancak proje artık derleme sırasında kök dizindeki `version.txt` dosyasını okuyor; bu yüzden `version.txt` ana kaynak olarak düşünülmelidir.
2. **Güncelleme kontrolü**: `Services\UpdateService.cs` içindeki `ManifestUrl` adresinden bir JSON indirilir. Örnek:

```json
{
  "version": "1.0.1",
  "notes": "Hata düzeltmeleri.",
  "changelog": "[1.0.1] Hata düzeltmeleri",
  "downloadUrl": "https://example.com/WindowsKontrolMerkezi-1.0.1.zip",
  "hash": "<SHA256 of zip>"
}
```

`version` mevcut sürümden büyükse uygulama “güncelleme var” der ve isteğe bağlı `downloadUrl` ile güncelleme uyarısı verir. İndirme sırasında eğer `hash` sağlanmışsa dosya yazılmadan önce hash kontrolü yapılır; uyuşmuyorsa işlem iptal edilir. Ayarlar’da **Güncelle (indir ve kur)** ile dosya indirilir, çalıştırılır ve uygulama kapanır; güncelleme betiği yeni sürümü `version.txt` dosyasına yazar, böylece yeniden açıldığında eski sürüm tekrardan bulunmaz. **Açılışta güncelleme kontrolü** açılıp kapatılabilir.

`version` mevcut sürümden büyükse uygulama “güncelleme var” der ve isteğe bağlı `downloadUrl` ile güncelleme uyarısı verir. Ayarlar’da **Güncelle (indir ve kur)** ile dosya indirilir, çalıştırılır ve uygulama kapanır; güncelleme betiği yeni sürümü `version.txt` dosyasına yazar, böylece yeniden açıldığında eski sürüm tekrardan bulunmaz. **Açılışta güncelleme kontrolü** açılıp kapatılabilir.

## Değişiklik günlüğü

Uygulama içinde **Ayarlar** sayfasında gösterilir. İçerik proje kökündeki `CHANGELOG.md` dosyasından okunur (derleme sırasında çıktı klasörüne kopyalanır).

## Proje yapısı

- `WindowsKontrolMerkezi/` — WPF uygulaması
  - `MainWindow.xaml` — Ana pencere, sidebar, sayfa çerçevesi
  - `Pages/` — Panel, Modlar, Ayarlar sayfaları
  - `Services/` — SystemInfo (CPU/RAM/Disk), PowerPlan, Launcher, Update

npm/Node kullanılmaz; tamamen .NET ile native Windows uygulamasıdır.
