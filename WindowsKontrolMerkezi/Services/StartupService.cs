using System.Diagnostics;
using Microsoft.Win32;

namespace WindowsKontrolMerkezi.Services;

public static class StartupService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WindowsKontrolMerkezi";
    private const string ExeName = "WindowsKontrolMerkezi.exe";

    /// <summary>Çalışan uygulamanın .exe dosyasının tam yolu (Windows ile başlat için).</summary>
    public static string? GetExePath()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path) && path.EndsWith(ExeName, StringComparison.OrdinalIgnoreCase))
            return path;
        try
        {
            path = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                return path;
        }
        catch { }
        return null;
    }

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
            var path = key?.GetValue(ValueName) as string;
            return !string.IsNullOrEmpty(path);
        }
        catch { return false; }
    }

    /// <summary>Windows ile başlat: Kayıt defterine .exe yolunu yazar. Sadece gerçek exe varsa etkinleştirilir.</summary>
    public static void SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, true);
            if (key == null) return;
            if (enabled)
            {
                var exePath = GetExePath();
                if (string.IsNullOrEmpty(exePath))
                    return;
                key.SetValue(ValueName, exePath, RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(ValueName, false);
            }
        }
        catch { }
    }
}
