using System.IO;
using System.Reflection;

namespace WindowsKontrolMerkezi;

/// <summary>Sürüm: önce version.txt (exe yanında), yoksa assembly, yoksa sabit. version.txt = tek gerçek kaynak.</summary>
public static class VersionInfo
{
    public const string FallbackVersion = "1.4.72";

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

    /// <summary>Görüntüleme: "1.2.0 (beta)" veya "G1.0"</summary>
    public static string DisplayVersion
    {
        get
        {
            var settings = Services.AppSettingsService.Load();
            if (settings.Edition == Services.AppEdition.Gamer)
            {
                // Gamer edition has its own versioning cycle
                // For now, let's use a placeholder gamer version or map it
                return $"G{Version.Split('.')[0]}.{Version.Split('.')[1]}"; 
            }
            return Channel == "önerilir" ? Version : $"{Version} ({Channel})";
        }
    }

    public const string AppName = "Windows Kontrol Merkezi";
    public const string BrandName = "Wintak R";

    /// <summary>Sürüm kodları penceresinde listelenecek özellikler (ad, sürüm, tarih).</summary>
    public static readonly IReadOnlyList<FeatureVersion> FeatureVersions = new List<FeatureVersion>
    {
        // Temel Modüller
        new("Sistem Paneli (CPU/RAM/Disk)", "1.0.0", "2025-02-22"),
        new("Modlar Sekmesi (Oyun, Performans, Odak)", "1.0.0", "2025-02-22"),
        new("Ayarlar Sekmesi & Kişiselleştirme", "1.1.0", "2025-02-22"),
        new("Bildirim Sistemi (Genişletilmiş Panel)", "1.2.1", "2025-02-23"),
        
        // Gelişmiş Özellikler
        new("Detaylı Sistem Grafikleri", "1.1.0", "2025-02-22"),
        new("Sistem Bakımı (%TEMP% & RAM)", "1.1.0", "2025-02-22"),
        new("Bağımsız Rahatsız Etme (DND) Sistemi", "1.3.0", "2025-02-23"),
        new("Gelişmiş Tema Motoru & Özel Arka Planlar", "1.4.0", "2025-02-23"),
        
        // Altyapı
        new("Canlı Sürüm ve Kanal Takibi", "1.2.0", "2025-02-22"),
        new("Evrensel Otomatik Güncelleme (ZIP)", "1.3.6", "2025-02-23"),
        new("Bildirim Geçmişi (JSON Veritabanı)", "1.3.0", "2025-02-23"),
        new("Self-Contained Deployment (Net-Free)", "1.4.1", "2025-02-23")
    };
}

public record FeatureVersion(string Name, string Version, string Date);
