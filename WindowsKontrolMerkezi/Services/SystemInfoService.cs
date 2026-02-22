using System.Diagnostics;
using System.IO;
using System.Management;
namespace WindowsKontrolMerkezi.Services;

public static class SystemInfoService
{
    private static PerformanceCounter? _cpuCounter;

    public static double GetCpuUsage()
    {
        try
        {
            _cpuCounter ??= new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue();
            Thread.Sleep(100);
            return Math.Min(100, _cpuCounter.NextValue());
        }
        catch { return 0; }
    }

    public static (ulong Total, ulong Used, double Percent) GetMemory()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var total = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024UL;
                var free = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024UL;
                var used = total - free;
                var percent = total > 0 ? (double)used / total * 100 : 0;
                return (total, used, percent);
            }
        }
        catch { }
        return (0, 0, 0);
    }

    public static (ulong Total, ulong Used, double Percent) GetDiskC()
    {
        try
        {
            var d = new DriveInfo("C");
            if (!d.IsReady) return (0, 0, 0);
            var total = (ulong)d.TotalSize;
            var free = (ulong)d.AvailableFreeSpace;
            var used = total - free;
            var percent = total > 0 ? (double)used / total * 100 : 0;
            return (total, used, percent);
        }
        catch { }
        return (0, 0, 0);
    }

    public static string GetOsDescription()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var cap = obj["Caption"]?.ToString() ?? "";
                var ver = obj["Version"]?.ToString() ?? "";
                return string.IsNullOrEmpty(cap) ? $"Windows {ver}" : $"{cap} ({ver})";
            }
        }
        catch { }
        return Environment.OSVersion.VersionString;
    }

    public static string GetMachineName() => Environment.MachineName;

    /// <summary>RAM toplam boyutu metin (örn. "16 GB").</summary>
    public static string GetRamSizeText()
    {
        var (total, _, _) = GetMemory();
        if (total == 0) return "—";
        var gb = total / (1024.0 * 1024.0 * 1024.0);
        return gb >= 1 ? $"{gb:F1} GB" : $"{total / (1024.0 * 1024.0):F0} MB";
    }
}
