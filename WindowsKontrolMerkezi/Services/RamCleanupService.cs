using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsKontrolMerkezi.Services;

/// <summary>
/// Sadece bu uygulamanın çalışma kümesini küçültür (güvenli).
/// Sistem genelinde "RAM temizleme" yapmaz; yalnızca uygulama kendi kullanılmayan belleğini işletim sistemine iade eder.
/// </summary>
public static class RamCleanupService
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr minSize, IntPtr maxSize);

    /// <summary>Mevcut uygulamanın çalışma kümesini küçültür. Güvenli.</summary>
    public static bool TrimCurrentProcessWorkingSet()
    {
        try
        {
            using var p = Process.GetCurrentProcess();
            return SetProcessWorkingSetSize(p.Handle, (IntPtr)(-1), (IntPtr)(-1));
        }
        catch { return false; }
    }
}
