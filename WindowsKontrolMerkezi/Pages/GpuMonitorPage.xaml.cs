using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi.Pages
{
    public partial class GpuMonitorPage : Page
    {
        private DispatcherTimer _refreshTimer;

        public GpuMonitorPage()
        {
            InitializeComponent();
            InitializeRefreshTimer();
            RefreshGpuInfo();
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(2);
            _refreshTimer.Tick += (s, e) => RefreshGpuInfo();
            _refreshTimer.Start();
        }

        private void RefreshGpuInfo()
        {
            try
            {
                var gpuInfo = GpuMonitorService.GetGpuInfo();
                
                // Update GPU Info
                GpuNameBlock.Text = gpuInfo.Name ?? "GPU Bulunamadı";
                
                // Update Usage
                UsageProgressBar.Value = gpuInfo.Usage;
                UsagePercentBlock.Text = $"{gpuInfo.Usage}%";
                
                // Update Temperature
                if (gpuInfo.Temperature.HasValue)
                {
                    int temp = (int)gpuInfo.Temperature.Value;
                    TempProgressBar.Value = Math.Min(temp, 100);
                    TempBlock.Text = $"{temp}°C";
                    
                    // Color code based on temperature
                    if (temp < 50)
                    {
                        TempStatusBlock.Text = "✓ Normal (< 50°C)";
                        TempProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    }
                    else if (temp < 70)
                    {
                        TempStatusBlock.Text = "⚠ Uyarı (50-70°C)";
                        TempProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    }
                    else
                    {
                        TempStatusBlock.Text = "🔴 Yüksek (> 70°C)";
                        TempProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    }
                }
                else
                {
                    TempBlock.Text = "N/A";
                    TempStatusBlock.Text = "Sıcaklık ölçülemedi";
                }
            }
            catch (Exception ex)
            {
                GpuNameBlock.Text = "GPU Bilgisi Alınamadı";
                UsagePercentBlock.Text = "Hata";
                TempBlock.Text = "Hata";
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
        }
    }
}
