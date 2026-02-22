using System.IO;
using System.Net.Http;
using System.Diagnostics;
using WindowsKontrolMerkezi;

namespace WindowsKontrolMerkezi.Services;

public record UpdateFeature(string Name, string Version);
public record UpdateResult(bool HasUpdate, string Current, string Latest, string Notes, string Channel, string? DownloadUrl, List<UpdateFeature> Features);

public static class UpdateService
{
    public static string CurrentVersion => VersionInfo.Version;
    public static string CurrentChannel => VersionInfo.Channel;
    public static string DisplayVersion => VersionInfo.DisplayVersion;
    private const string ManifestUrl = "https://raw.githubusercontent.com/CoderProntae/WindowsKontrolMerkezi/main/version.json";

    private static int[] ParseVersion(string v)
    {
        var s = (v ?? "").TrimStart('v');
        return s.Split('.').Select(x => int.TryParse(x, out var n) ? n : 0).ToArray();
    }

    private static bool IsNewer(string latest, string current)
    {
        var a = ParseVersion(latest);
        var b = ParseVersion(current);
        for (int i = 0; i < Math.Max(a.Length, b.Length); i++)
        {
            var x = i < a.Length ? a[i] : 0;
            var y = i < b.Length ? b[i] : 0;
            if (x > y) return true;
            if (x < y) return false;
        }
        return false;
    }

    public static async Task<UpdateResult> CheckForUpdatesAsync()
    {
        var current = CurrentVersion.Trim();
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            var json = await client.GetStringAsync(ManifestUrl);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var latest = root.TryGetProperty("version", out var v) ? v.GetString() ?? current : current;
            var notes = root.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : (root.TryGetProperty("changelog", out var c) ? c.GetString() ?? "" : "");
            var url = root.TryGetProperty("downloadUrl", out var u) ? u.GetString() : null;
            var channel = root.TryGetProperty("channel", out var ch) ? ch.GetString() ?? "önerilir" : "önerilir";
            
            var featuresList = new List<UpdateFeature>();
            if (root.TryGetProperty("features", out var feats) && feats.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in feats.EnumerateArray())
                {
                    var fName = item.TryGetProperty("name", out var fn) ? fn.GetString() : "";
                    var fVer = item.TryGetProperty("version", out var fv) ? fv.GetString() : "";
                    if (!string.IsNullOrEmpty(fName)) featuresList.Add(new UpdateFeature(fName, fVer ?? ""));
                }
            }

            var hasUpdate = IsNewer(latest, current);
            
            if (hasUpdate)
            {
                var featureText = featuresList.Count > 0 
                    ? "\nGüncellenen Özellikler:\n" + string.Join("\n", featuresList.Select(f => $"- {f.Name} (v{f.Version})"))
                    : "";

                NotificationService.AddNotification(
                    "Yeni Güncelleme Mevcut", 
                    $"Sürüm {latest} ({channel}) indirilebilir.{featureText}",
                    "Güncelleme Servisi",
                    "page:Ayarlar",
                    "Detaylara Git"
                );
            }

            return new UpdateResult(hasUpdate, current, latest, notes ?? "", channel, url, featuresList);
        }
        catch
        {
            return new UpdateResult(false, current, current, "", "önerilir", null, new List<UpdateFeature>());
        }
    }

    /// <summary>Güncelleme dosyasını indirir, çalıştırır. Uygulama hemen kapatılmalı ki kurulum devam etsin.</summary>
    /// <returns>İndirilip çalıştırıldıysa true; hata varsa false.</returns>
    /// <summary>Güncelleme dosyasını indirir, yer değiştirme scripti oluşturur ve çalıştırır.</summary>
    public static async Task<bool> DownloadAndRunUpdateAsync(string downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl)) return false;
        try
        {
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe)) return false;

            var ext = Path.GetExtension(new Uri(downloadUrl).LocalPath);
            if (string.IsNullOrEmpty(ext)) ext = ".exe";
            
            var tempDir = Path.GetTempPath();
            var updateFile = Path.Combine(tempDir, "WKM_New_Version" + ext);
            var batchFile = Path.Combine(tempDir, "wkm_updater.bat");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                var bytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(updateFile, bytes);
            }

            // Batch script: 
            // 1. Bekle (uygulama kapansın)
            // 2. Eski exe'yi sil
            // 3. Yeni dosyayı eski exe ismine taşı/kopyala
            // 4. Uygulamayı başlat
            // 5. Kendini sil
            var script = $@"
@echo off
timeout /t 2 /nobreak > nul
:retry
del /f /q ""{currentExe}""
if exist ""{currentExe}"" (
    timeout /t 1 /nobreak > nul
    goto retry
)
move /y ""{updateFile}"" ""{currentExe}""
start """" ""{currentExe}""
del ""{batchFile}""
";
            await File.WriteAllTextAsync(batchFile, script);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{batchFile}\"\"",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}
