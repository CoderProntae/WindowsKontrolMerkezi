using System.Diagnostics;
using System.Net.NetworkInformation;

namespace WindowsKontrolMerkezi.Services;

public static class NetworkUsageService
{
    private static long _lastBytesReceived = -1;
    private static long _lastBytesSent = -1;
    private static DateTime _lastTime = DateTime.MinValue;

    public static (double DownloadMbps, double UploadMbps) GetSpeedMbps()
    {
        try
        {
            long totalRecv = 0, totalSent = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;
                var stats = ni.GetIPStatistics();
                totalRecv += stats.BytesReceived;
                totalSent += stats.BytesSent;
            }

            var now = DateTime.UtcNow;
            double down = 0, up = 0;
            if (_lastTime != DateTime.MinValue && (_lastBytesReceived >= 0 || _lastBytesSent >= 0))
            {
                var elapsed = (now - _lastTime).TotalSeconds;
                if (elapsed > 0)
                {
                    down = (totalRecv - _lastBytesReceived) / elapsed * 8 / 1_000_000; // Mbps
                    up = (totalSent - _lastBytesSent) / elapsed * 8 / 1_000_000;
                    if (down < 0) down = 0;
                    if (up < 0) up = 0;
                }
            }
            _lastBytesReceived = totalRecv;
            _lastBytesSent = totalSent;
            _lastTime = now;
            return (down, up);
        }
        catch { }
        return (0, 0);
    }
}
