using System.IO;
using System.Net.Http;
using System.Diagnostics;
using WindowsKontrolMerkezi;

namespace WindowsKontrolMerkezi.Services;

public record UpdateFeature(string Name, string Version);
public record UpdateResult(bool HasUpdate, string Current, string Latest, string Notes, string Channel, string? DownloadUrl, string? Hash, List<UpdateFeature> Features);

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
            
            // Cache busting
            var urlWithCacheBust = ManifestUrl + "?t=" + DateTime.Now.Ticks;
            var json = await client.GetStringAsync(urlWithCacheBust);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var latest = root.TryGetProperty("version", out var v) ? v.GetString() ?? current : current;
            var notes = root.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : (root.TryGetProperty("changelog", out var c) ? c.GetString() ?? "" : "");
            var url = root.TryGetProperty("downloadUrl", out var u) ? u.GetString() : null;
            var channel = root.TryGetProperty("channel", out var ch) ? ch.GetString() ?? "önerilir" : "önerilir";
            var hash = root.TryGetProperty("hash", out var h) ? h.GetString() : null;
            
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
                    "Güncelleme Servisi"
                );
            }

            return new UpdateResult(hasUpdate, current, latest, notes ?? "", channel, url, hash, featuresList);
        }
        catch
        {
            return new UpdateResult(false, current, current, "", "önerilir", null, null, new List<UpdateFeature>());
        }
    }

    /// <summary>Güncelleme dosyasını indirir, çalıştırır. Uygulama hemen kapatılmalı ki kurulum devam etsin.</summary>
    /// <returns>İndirilip çalıştırıldıysa true; hata varsa false.</returns>
    /// <summary>Güncelleme dosyasını indirir, yer değiştirme scripti oluşturur ve çalıştırır.</summary>
    public static async Task<bool> DownloadAndRunUpdateAsync(string downloadUrl, string? expectedVersion = null, string? expectedHash = null)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl)) return false;
        try
        {
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe)) return false;

            var ext = Path.GetExtension(new Uri(downloadUrl).LocalPath).ToLower();
            if (string.IsNullOrEmpty(ext)) ext = ".exe";
            
            var tempDir = Path.GetTempPath();
            var updateFile = Path.Combine(tempDir, "WKM_Update_Package" + ext);
            var batchFile = Path.Combine(tempDir, "wkm_updater.bat");
            var appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                var bytes = await client.GetByteArrayAsync(downloadUrl);

                // verify downloaded bytes against expected hash if provided
                if (!string.IsNullOrWhiteSpace(expectedHash))
                {
                    try
                    {
                        using var sha = System.Security.Cryptography.SHA256.Create();
                        var computed = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                        if (!string.Equals(computed, expectedHash.Replace(" ", "").ToLowerInvariant(), StringComparison.Ordinal))
                        {
                            // hash mismatch
                            return false;
                        }
                    }
                    catch
                    {
                        // ignore any hash check failure
                    }
                }

                await File.WriteAllBytesAsync(updateFile, bytes);
            }

            // prepare optional command to write the new version into version.txt
            string versionEcho = string.Empty;
            if (!string.IsNullOrWhiteSpace(expectedVersion))
            {
                // escape any double-quotes that could break the batch file
                var safeVer = expectedVersion.Replace("\"", "\"\"");
                var versionPath = Path.Combine(appDir, "version.txt");
                versionEcho = $"echo {safeVer} > \"{versionPath}\"\r\n";
            }

            // Logic switch based on extension
            string script;
            if (ext == ".zip")
            {
                // ZIP Update:
                // 1. Wait for process to exit
                // 2. Use PowerShell to extract (overwrite all)
                // 3. Optionally write version.txt
                // 4. Start exe
                script = "@echo off\n" +
                    "timeout /t 2 /nobreak > nul\n" +
                    $"powershell -Command \"Expand-Archive -Path '{updateFile}' -DestinationPath '{appDir}' -Force\"\n" +
                    versionEcho +
                    $"start \"\" \"{currentExe}\"\n" +
                    $"del \"{batchFile}\"\n";
            }
            else
            {
                // Legacy EXE Update
                script = "@echo off\n" +
                    "timeout /t 2 /nobreak > nul\n" +
                    ":retry\n" +
                    $"del /f /q \"{currentExe}\"\n" +
                    $"if exist \"{currentExe}\" (\n" +
                    "    timeout /t 1 /nobreak > nul\n" +
                    "    goto retry\n" +
                    ")\n" +
                    "if exist \"version.txt\" del /f /q \"version.txt\"\n" +
                    versionEcho +
                    $"move /y \"{updateFile}\" \"{currentExe}\"\n" +
                    $"start \"\" \"{currentExe}\"\n" +
                    $"del \"{batchFile}\"\n";
            }

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
