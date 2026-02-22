using System.Diagnostics;
using Microsoft.Win32;

namespace WindowsKontrolMerkezi.Services;

/// <summary>
/// Odak yardımı (Focus Assist). Windows 10/11.
/// Önce kayıt defteri, olmazsa PowerShell ile dene.
/// </summary>
public static class FocusAssistService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings";
    private const string ValueName = "NOC_GLOBAL_SETTING_TOASTS_ENABLED";
    private const string Win11KeyPath = @"Software\Microsoft\Windows\CurrentVersion\PushNotifications";
    private const string Win11ValueName = "ToastEnabled";

    public static bool IsFocusAssistOn()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
            var v = key?.GetValue(ValueName);
            if (v is int i && i == 0) return true; // 0 = Focus On / Toasts Off
            
            using var key11 = Registry.CurrentUser.OpenSubKey(Win11KeyPath, false);
            var v11 = key11?.GetValue(Win11ValueName);
            if (v11 is int i11 && i11 == 0) return true;

            return false;
        }
        catch { return false; }
    }

    public static string GetStateLabel()
    {
        return IsFocusAssistOn() ? "Açık (Bildirimler Kapalı)" : "Kapalı";
    }

    /// <summary>true = Odak yardımı aç (bildirimleri kapat). Kayıt defteri + gerekirse PowerShell.</summary>
    public static bool SetFocusAssist(bool on)
    {
        int val = on ? 0 : 1; // 0 = Focus On (Toasts Off), 1 = Focus Off (Toasts On)
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath, true);
            key?.SetValue(ValueName, val, RegistryValueKind.DWord);
        }
        catch { }

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(Win11KeyPath, true);
            key?.SetValue(Win11ValueName, val, RegistryValueKind.DWord);
        }
        catch { }

        // Trigger Shell notification refresh (optional but helps)
        try
        {
            var shellVal = on ? "0" : "1";
            var cmd = $"Set-ItemProperty -Path 'HKCU:\\{KeyPath}' -Name '{ValueName}' -Value {shellVal}; " +
                      $"Set-ItemProperty -Path 'HKCU:\\{Win11KeyPath}' -Name '{Win11ValueName}' -Value {shellVal}";
            
            var psi = new ProcessStartInfo("powershell")
            {
                Arguments = $"-NoProfile -NonInteractive -Command \"{cmd}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi)?.WaitForExit(2000);
        }
        catch { }

        return true;
    }
}
