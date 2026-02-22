using System.Diagnostics;

namespace WindowsKontrolMerkezi.Services;

public static class LauncherService
{
    public static void OpenWindowsSettings() => OpenUri("ms-settings:");
    public static void OpenGameModeSettings() => OpenUri("ms-settings:gaming-gamemode");
    public static void OpenFocusAssist() => OpenUri("ms-settings:quiethours");
    public static void OpenUninstall()
    {
        try { Process.Start(new ProcessStartInfo("appwiz.cpl") { UseShellExecute = true }); }
        catch { try { Process.Start(new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true }); } catch { } }
    }

    public static void OpenPowerSettings()
    {
        try { Process.Start(new ProcessStartInfo("control.exe", "powercfg.cpl") { UseShellExecute = true }); }
        catch { try { Process.Start(new ProcessStartInfo("powercfg.cpl") { UseShellExecute = true }); } catch { } }
    }

    private static void OpenUri(string uri)
    {
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch { }
    }

    public static void OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }
}
