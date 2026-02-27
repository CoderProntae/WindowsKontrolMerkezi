using System;
using System.Net.NetworkInformation;
using System.Linq;
using System.Collections.Generic;

namespace WindowsKontrolMerkezi.Services;

public record NetworkAdapter(string Name, string Status, string? IpAddress, long BytesReceivedPerSec, long BytesSentPerSec);

public static class NetworkMonitorService
{
    private static Dictionary<string, (long recv, long sent)> _lastCounters = new();

    /// <summary>Tüm aktif ağ adaptörlerinin bilgisi ve bant genişliği</summary>
    public static List<NetworkAdapter> GetNetworkAdapters()
    {
        var adapters = new List<NetworkAdapter>();

        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var iface in interfaces)
            {
                // Sadece aktif adaptörleri göster
                if (iface.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = iface.GetIPProperties();
                var ipAddr = ipProps.UnicastAddresses
                    .FirstOrDefault(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                    .Address.ToString() ?? "N/A";

                var stats = iface.GetIPStatistics();
                var bytesRecv = stats.BytesReceived;
                var bytesSent = stats.BytesSent;

                // Hız hesapla (son çağrıdan bu yana)
                long bytesRecvPerSec = 0, bytesSentPerSec = 0;
                var key = iface.Name;

                if (_lastCounters.ContainsKey(key))
                {
                    var (lastRecv, lastSent) = _lastCounters[key];
                    bytesRecvPerSec = Math.Max(0, bytesRecv - lastRecv);
                    bytesSentPerSec = Math.Max(0, bytesSent - lastSent);
                }

                _lastCounters[key] = (bytesRecv, bytesSent);

                adapters.Add(new NetworkAdapter(
                    iface.Description,
                    iface.OperationalStatus.ToString(),
                    ipAddr,
                    bytesRecvPerSec,
                    bytesSentPerSec
                ));
            }
        }
        catch { }

        return adapters;
    }

    /// <summary>Toplam ağ hızı (KB/s cinsinden)</summary>
    public static (double downloadKbps, double uploadKbps) GetNetworkSpeed()
    {
        var adapters = GetNetworkAdapters();
        var totalDownload = adapters.Sum(a => a.BytesReceivedPerSec) / 1024.0; // KB/s
        var totalUpload = adapters.Sum(a => a.BytesSentPerSec) / 1024.0; // KB/s
        return (totalDownload, totalUpload);
    }
}
