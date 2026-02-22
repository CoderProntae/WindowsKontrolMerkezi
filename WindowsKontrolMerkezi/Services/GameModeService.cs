using Microsoft.Win32;

namespace WindowsKontrolMerkezi.Services;

public static class GameModeService
{
    private const string KeyPath = @"Software\Microsoft\GameBar";
    private const string ValueName = "AutoGameModeEnabled";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
            var v = key?.GetValue(ValueName);
            if (v is int i) return i != 0;
            // Varsayılan: birçok sistemde kapalı sayılır
            return false;
        }
        catch { return false; }
    }

    public static bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath, true);
            key?.SetValue(ValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
            return true;
        }
        catch { return false; }
    }
}
