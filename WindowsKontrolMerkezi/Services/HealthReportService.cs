using System;
using System.IO;
using System.Management;
using System.Linq;

namespace WindowsKontrolMerkezi.Services;

public record HealthReport(
    string DiskStatus,  // OK, WARNING, CRITICAL
    string RamStatus,   // OK, WARNING, CRITICAL
    string CpuStatus,   // OK, WARNING, CRITICAL
    double DiskUsedPercent,
    double RamUsedPercent,
    double CpuUsagePercent,
    string? CpuTemperature,
    string? GpuStatus,
    string? StorageSpace  // B, KB, MB, GB cinsinden
);

public static class HealthReportService
{
    /// <summary>Sistem sağlığı raporu oluştur</summary>
    public static HealthReport GetHealthReport()
    {
        var (diskUsed, diskTotal) = GetDiskUsage();
        var ramUsed = GetRamUsage();
        var cpuUsage = GetCpuUsage();
        var (cpuTemp, cpuStatus) = GetCpuHealth();
        var (gpuName, gpuUsage, gpuTemp) = GetGpuHealth();

        var diskPercent = diskTotal > 0 ? (diskUsed / (double)diskTotal) * 100 : 0;
        var diskStatus = diskPercent > 90 ? "CRÍTICO" : diskPercent > 70 ? "AVISO" : "OK";

        var ramStatus = ramUsed > 90 ? "CRÍTICO" : ramUsed > 70 ? "AVISO" : "OK";
        var cpuStatusStr = cpuUsage > 90 ? "CRÍTICO" : cpuUsage > 70 ? "AVISO" : "OK";

        var storageSpace = FormatBytes(diskTotal - diskUsed);

        return new HealthReport(
            diskStatus,
            ramStatus,
            cpuStatusStr,
            diskPercent,
            ramUsed,
            cpuUsage,
            cpuTemp,
            gpuName,
            storageSpace
        );
    }

    private static (long used, long total) GetDiskUsage()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name.StartsWith("C:"));
            if (drive != null)
            {
                return (drive.TotalSize - drive.AvailableFreeSpace, drive.TotalSize);
            }
        }
        catch { }
        return (0, 0);
    }

    private static double GetRamUsage()
    {
        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            
            // Sistem RAM bilgisi için WMI
            var scope = new ManagementScope();
            scope.Connect();
            
            var query = new ObjectQuery("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            var searcher = new ManagementObjectSearcher(scope, query);
            
            foreach (var obj in searcher.Get())
            {
                var totalMem = ulong.Parse(obj["TotalVisibleMemorySize"].ToString()) * 1024;
                var freeMem = ulong.Parse(obj["FreePhysicalMemory"].ToString()) * 1024;
                var usedMem = totalMem - freeMem;
                
                return (usedMem / (double)totalMem) * 100;
            }
        }
        catch { }

        return 0;
    }

    private static double GetCpuUsage()
    {
        try
        {
            var scope = new ManagementScope();
            scope.Connect();
            
            var query = new ObjectQuery("SELECT LoadPercentage FROM Win32_Processor");
            var searcher = new ManagementObjectSearcher(scope, query);
            
            var totalLoad = 0u;
            var count = 0;
            
            foreach (var obj in searcher.Get())
            {
                if (uint.TryParse(obj["LoadPercentage"]?.ToString() ?? "0", out var load))
                {
                    totalLoad += load;
                    count++;
                }
            }
            
            return count > 0 ? totalLoad / (double)count : 0;
        }
        catch { }

        return 0;
    }

    private static (string? temp, string status) GetCpuHealth()
    {
        try
        {
            // WMI üzerinden CPU sıcaklığı (eğer mevcut ise)
            var scope = new ManagementScope(@"\\.\root\wmi");
            scope.Connect();
            
            var query = new ObjectQuery("SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
            var searcher = new ManagementObjectSearcher(scope, query);
            
            foreach (var obj in searcher.Get())
            {
                if (ulong.TryParse(obj["CurrentTemperature"]?.ToString() ?? "0", out var k))
                {
                    var celsius = (int)(k > 100 ? k - 273.15 : k);
                    var status = celsius > 85 ? "CRÍTICO" : celsius > 70 ? "AVISO" : "OK";
                    return ($"{celsius}°C", status);
                }
            }
        }
        catch { }

        return (null, "DESCONOCIDO");
    }

    private static (string name, double usage, uint? temp) GetGpuHealth()
    {
        try
        {
            var gpu = GpuMonitorService.GetGpuInfo();
            return (gpu.Name, gpu.Usage, gpu.Temperature);
        }
        catch { }

        return ("GPU Desconocido", 0, null);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}
