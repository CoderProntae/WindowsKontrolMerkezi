using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi.Pages
{
    public partial class HealthWidgetPage : Page
    {
        private DispatcherTimer _refreshTimer;

        public HealthWidgetPage()
        {
            InitializeComponent();
            InitializeRefreshTimer();
            RefreshHealthInfo();
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(2);
            _refreshTimer.Tick += (s, e) => RefreshHealthInfo();
            _refreshTimer.Start();
        }

        private void RefreshHealthInfo()
        {
            try
            {
                var health = HealthReportService.GetHealthReport();

                // Update Disk
                DiskProgressBar.Value = health.DiskUsedPercent;
                DiskPercentBlock.Text = $"{health.DiskUsedPercent:F1}%";
                DiskInfoBlock.Text = health.StorageSpace;
                UpdateDiskStatus(health.DiskStatus);

                // Update RAM
                RamProgressBar.Value = health.RamUsedPercent;
                RamPercentBlock.Text = $"{health.RamUsedPercent:F1}%";
                RamInfoBlock.Text = $"İçinde: {(health.RamUsedPercent):F1}% / Boş: {(100 - health.RamUsedPercent):F1}%";
                UpdateRamStatus(health.RamStatus);

                // Update CPU
                CpuProgressBar.Value = health.CpuUsagePercent;
                CpuPercentBlock.Text = $"{health.CpuUsagePercent:F1}%";
                CpuUsageBlock.Text = $"Kullanım: {health.CpuUsagePercent:F1}%";
                CpuTempBlock.Text = $"Sıcaklık: {health.CpuTemperature}°C";
                UpdateCpuStatus(health.CpuStatus);

                // Update GPU
                GpuNameBlock.Text = health.GpuStatus ?? "GPU Bulunamadı";
                if (health.GpuStatus != null && health.GpuStatus != "GPU Bulunamadı")
                {
                    GpuStatusIconBlock.Text = "✓";
                    GpuStatusIconBlock.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                }
                else
                {
                    GpuStatusIconBlock.Text = "✗";
                    GpuStatusIconBlock.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                }

                // Update overall status
                UpdateOverallStatus(health.DiskStatus, health.RamStatus, health.CpuStatus);

                // Storage Info
                StorageBlock.Text = health.StorageSpace;
            }
            catch (Exception ex)
            {
                StatusTitleBlock.Text = "Sistem Durumu: Hata";
                StatusDescBlock.Text = "Sistem bilgisi alınamadı: " + ex.Message;
            }
        }

        private void UpdateDiskStatus(string status)
        {
            switch (status)
            {
                case "OK":
                    DiskProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    break;
                case "AVISO":
                    DiskProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    break;
                case "CRÍTICO":
                    DiskProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    break;
            }
        }

        private void UpdateRamStatus(string status)
        {
            switch (status)
            {
                case "OK":
                    RamProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    break;
                case "AVISO":
                    RamProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    break;
                case "CRÍTICO":
                    RamProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    break;
            }
        }

        private void UpdateCpuStatus(string status)
        {
            switch (status)
            {
                case "OK":
                    CpuProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    CpuStatusBlock.Text = "✓ Normal";
                    CpuStatusBlock.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    break;
                case "AVISO":
                    CpuProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    CpuStatusBlock.Text = "⚠ Yüksek";
                    CpuStatusBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    break;
                case "CRÍTICO":
                    CpuProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    CpuStatusBlock.Text = "🔴 Kritik";
                    CpuStatusBlock.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    break;
            }
        }

        private void UpdateOverallStatus(string diskStatus, string ramStatus, string cpuStatus)
        {
            string? worstStatus = GetWorstStatus(diskStatus, ramStatus, cpuStatus);

            switch (worstStatus)
            {
                case "OK":
                    StatusTitleBlock.Text = "Sistem Durumu: Normal";
                    StatusDescBlock.Text = "Tüm sistem kaynakları normal seviyelerde çalışıyor";
                    break;
                case "AVISO":
                    StatusTitleBlock.Text = "Sistem Durumu: Uyarı";
                    StatusDescBlock.Text = "Bazı sistem kaynakları yüksek seviyelerdedir";
                    break;
                case "CRÍTICO":
                    StatusTitleBlock.Text = "Sistem Durumu: Kritik";
                    StatusDescBlock.Text = "Sistem kaynakları kritik seviyelerdedir";
                    break;
            }
        }

        private string? GetWorstStatus(params string[] statuses)
        {
            foreach (var status in statuses)
            {
                if (status == "CRÍTICO") return "CRÍTICO";
            }
            foreach (var status in statuses)
            {
                if (status == "AVISO") return "AVISO";
            }
            return "OK";
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
        }
    }
}
