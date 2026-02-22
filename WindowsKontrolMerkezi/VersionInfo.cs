using System.IO;
using System.Reflection;

namespace WindowsKontrolMerkezi;

/// <summary>Sürüm: önce version.txt (exe yanında), yoksa assembly, yoksa sabit. version.txt = tek gerçek kaynak.</summary>
public static class VersionInfo
{
    private const string FallbackVersion = "1.3.6";

    private static string ReadVersionFromFile()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
            if (File.Exists(path))
            {
                var v = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(v)) return v.Split('\n')[0].Trim();
            }
        }
        catch { }
        return null!;
    }

    /// <summary>Örn: 1.2.0 veya 1.2.0-beta.</summary>
    public static string FullVersion
    {
        get
        {
            var v = ReadVersionFromFile();
            if (!string.IsNullOrEmpty(v)) return v.Split('+')[0].Trim();
            v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Trim();
            v = (v ?? "").Split('+')[0].Trim();
            return string.IsNullOrEmpty(v) ? FallbackVersion : v;
        }
    }

    /// <summary>Sayısal kısım: 1.2.0</summary>
    public static string Version
    {
        get
        {
            var full = FullVersion;
            var dash = full.IndexOf('-');
            return dash >= 0 ? full.Substring(0, dash).Trim() : full;
        }
    }

    /// <summary>Kanal: önerilir, beta, alpha, önerilmez vb. Yoksa "önerilir".</summary>
    public static string Channel
    {
        get
        {
            var full = FullVersion;
            var dash = full.IndexOf('-');
            if (dash < 0) return "önerilir";
            var channel = full.Substring(dash + 1).Trim().ToLowerInvariant();
            return string.IsNullOrEmpty(channel) ? "önerilir" : channel;
        }
    }

    /// <summary>Görüntüleme: "1.2.0 (beta)"</summary>
    public static string DisplayVersion => Channel == "önerilir" ? Version : $"{Version} ({Channel})";

    public const string AppName = "Windows Kontrol Merkezi";
    public const string BrandName = "Wintak R";

    /// <summary>Sürüm kodları penceresinde listelenecek özellikler (ad, sürüm, tarih).</summary>
    public static readonly IReadOnlyList<FeatureVersion> FeatureVersions = new List<FeatureVersion>
    {
        new("Panel (CPU/RAM/Disk)", "1.0.0", "2025-02-22"),
        new("Modlar (Oyun, Performans, Odak)", "1.0.0", "2025-02-22"),
        new("Güncelleme kontrolü", "1.0.0", "2025-02-22"),
        new("Temalar", "1.1.0", "2025-02-22"),
        new("Detaylı grafik + canlı güncelleme", "1.1.0", "2025-02-22"),
        new("Odak yardımı uygulama içi", "1.1.0", "2025-02-22"),
        new("%TEMP% ve RAM bakım", "1.1.0", "2025-02-22"),
        new("Exe ile başlat + güncelle (indir ve kur)", "1.1.0", "2025-02-22"),
        new("Sürüm kanalı (önerilir/beta/alpha)", "1.2.0", "2025-02-22"),
        new("Mod ekleme/kaldırma/özelleştirme", "1.2.0", "2025-02-22"),
        new("Sürüm kodları penceresi", "1.2.0", "2025-02-22"),
        new("Wintak R markası ve uygulama bilgisi", "1.2.0", "2025-02-22"),
        new("Bildirim Genişletme Penceresi", "1.2.1", "2025-02-23"),
        new("Hata düzeltmeleri (XML & Padding)", "1.2.1", "2025-02-23"),
        new("Gelişmiş Self-Update (Batch Script)", "1.2.2", "2025-02-23"),
        new("Granüler Özellik Güncelleme Takibi", "1.2.2", "2025-02-23"),
        new("Tam Tema Uyumluluğu & Titlebar Sync", "1.2.3", "2025-02-23"),
        new("Sürüm Kodları Durum Göstergesi", "1.2.3", "2025-02-23"),
        new("UI Cilalama (Halkalar & Bildirim Butonu)", "1.2.3", "2025-02-23"),
        new("Mod Özelleştirme (Düzenleme Modu)", "1.2.3", "2025-02-23"),
        new("Bağımsız Rahatsız Etme (Win11 Önizleme)", "1.0.0", "2025-02-23"),
        new("Bildirim Kalıcılığı (JSON Storage)", "1.3.0", "2025-02-23"),
        new("Bildirim Geçmişi & Purge Ayarları", "1.3.0", "2025-02-23"),
        new("Dinamik Ayarlar (Koşullu Slider)", "1.3.1", "2025-02-23"),
        new("UI/UX Cilalama (Animasyon & Iconlar)", "1.3.1", "2025-02-23"),
        new("Tema Senkronizasyonu (Scrollbar & Selection)", "1.3.2", "2025-02-23"),
        new("Sistem Kararlılığı & Git Temizliği", "1.3.2", "2025-02-23"),
        new("Sürüm Senkronizasyonu (Update Fix)", "1.3.3", "2025-02-23"),
        new("Derin Tema Senkronizasyonu & History Fix", "1.3.4", "2025-02-23"),
        new("Özel Temalar (Balıklar & Lav) & History Clean", "1.3.5", "2025-02-23"),
        new("🌅 Gün Batımı Teması & Derin UI Fix", "1.3.5", "2025-02-23"),
        new("Evrensel Güncelleme Sistemi (ZIP/Multi-file)", "1.3.6", "2025-02-23"),
    };
}

public record FeatureVersion(string Name, string Version, string Date);
