using System.IO;
using System.Text.Json;

namespace WindowsKontrolMerkezi.Services;

public class AppSettings
{
    public string ThemeId { get; set; } = "dark";
    public bool StartWithWindows { get; set; }
    public bool CheckUpdatesAtStartup { get; set; } = true;
    public bool OpenNotificationPanelAtStartup { get; set; } = false;
    public double WindowOpacity { get; set; } = 1.0;
    /// <summary>Güncelleme kanalı: önerilir, beta, alpha, önerilmez.</summary>
    public string UpdateChannel { get; set; } = "önerilir";
    /// <summary>Kullanıcının eklediği modlar (yerleşikler hariç).</summary>
    public List<ModeDefinition> CustomModes { get; set; } = new();
}

public static class AppSettingsService
{
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowsKontrolMerkezi", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path, json);
        }
        catch { }
    }
}
