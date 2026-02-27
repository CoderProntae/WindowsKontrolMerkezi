using System;
using System.Management;
using System.Linq;

namespace WindowsKontrolMerkezi.Services;

public record GpuInfo(string Name, uint Usage, uint? Temperature);

public static class GpuMonitorService
{
    /// <summary>GPU bilgisi (NVIDIA/AMD/Intel) - kullanım % + sıcaklık</summary>
    public static GpuInfo GetGpuInfo()
    {
        try
        {
            // WMI üzerinden GPU sorgusu
            var scope = new ManagementScope(@"\\.\root\cimv2");
            scope.Connect();

            var query = new ObjectQuery("SELECT Name, CurrentPercentage FROM Win32_PerfFormattedData_GpuPerformanceCounters_GPUEngine WHERE Name LIKE '%_Total'");
            var searcher = new ManagementObjectSearcher(scope, query);
            var collection = searcher.Get();

            if (collection.Count > 0)
            {
                using (var obj = collection.Cast<ManagementObject>().FirstOrDefault())
                {
                    if (obj != null)
                    {
                        var name = obj["Name"]?.ToString() ?? "Unknown GPU";
                        var usage = uint.TryParse(obj["CurrentPercentage"]?.ToString() ?? "0", out var u) ? u : 0u;
                        
                        // Sıcaklık için ek sorgu (NVIDIA/AMD spesifik)
                        var temp = GetGpuTemperature();
                        
                        return new GpuInfo(name, usage, temp);
                    }
                }
            }

            // Fallback: GPU bulunamadı
            return new GpuInfo("GPU Bulunamadı", 0, null);
        }
        catch
        {
            return new GpuInfo("GPU Bilgisi Alınamadı", 0, null);
        }
    }

    /// <summary>GPU sıcaklığını almaya çalış (NVIDIA için WMI)</summary>
    private static uint? GetGpuTemperature()
    {
        try
        {
            // NVIDIA WDDM Driver sıcaklık sorgusu (eğer mevcut ise)
            var scope = new ManagementScope(@"\\.\root\wmi");
            scope.Connect();

            var query = new ObjectQuery("SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature WHERE Name LIKE '%GPU%'");
            var searcher = new ManagementObjectSearcher(scope, query);
            var collection = searcher.Get();

            if (collection.Count > 0)
            {
                using (var obj = collection.Cast<ManagementObject>().FirstOrDefault())
                {
                    if (obj != null)
                    {
                        // Kelvin'den Celsius'a çevir (bazen raw data hemen Celsius cinsinden gelir)
                        if (ulong.TryParse(obj["CurrentTemperature"]?.ToString() ?? "0", out var k))
                        {
                            // Eğer 100'den büyükse Kelvin, küçükse zaten Celsius
                            return (uint)(k > 100 ? k - 273.15 : k);
                        }
                    }
                }
            }
        }
        catch { }

        return null; // Sıcaklık alınamadı
    }
}
