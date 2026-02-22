using System.Diagnostics;

namespace WindowsKontrolMerkezi.Services;

public static class QuickActionsService
{
    public static void Sleep()
    {
        try { Process.Start(new ProcessStartInfo("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0") { UseShellExecute = false }); }
        catch { }
    }

    public static void Hibernate()
    {
        try { Process.Start(new ProcessStartInfo("shutdown", "/h") { UseShellExecute = false }); }
        catch { }
    }

    public static void Restart()
    {
        try { Process.Start(new ProcessStartInfo("shutdown", "/r /t 0") { UseShellExecute = false }); }
        catch { }
    }

    public static void Shutdown()
    {
        try { Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") { UseShellExecute = false }); }
        catch { }
    }
}
