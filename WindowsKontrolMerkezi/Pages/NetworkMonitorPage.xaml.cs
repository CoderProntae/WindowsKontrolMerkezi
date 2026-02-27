using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi.Pages
{
    public partial class NetworkMonitorPage : Page
    {
        private DispatcherTimer _refreshTimer;
        private ObservableCollection<NetworkAdapter> _adapters = new ObservableCollection<NetworkAdapter>();

        public NetworkMonitorPage()
        {
            InitializeComponent();
            AdaptersItemsControl.ItemsSource = _adapters;
            InitializeRefreshTimer();
            RefreshNetworkInfo();
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(2);
            _refreshTimer.Tick += (s, e) => RefreshNetworkInfo();
            _refreshTimer.Start();
        }

        private void RefreshNetworkInfo()
        {
            try
            {
                // Update speed
                var (downloadKbps, uploadKbps) = NetworkMonitorService.GetNetworkSpeed();
                DownloadSpeedBlock.Text = $"{downloadKbps:F0} KB/s";
                DownloadMbpsBlock.Text = $"{(downloadKbps / 1024):F2} Mbps";
                UploadSpeedBlock.Text = $"{uploadKbps:F0} KB/s";
                UploadMbpsBlock.Text = $"{(uploadKbps / 1024):F2} Mbps";

                // Update adapters
                var adapters = NetworkMonitorService.GetNetworkAdapters();
                
                if (adapters.Count == 0)
                {
                    _adapters.Clear();
                    NoAdaptersMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    NoAdaptersMessage.Visibility = Visibility.Collapsed;
                    
                    // Update or add adapters
                    for (int i = 0; i < adapters.Count; i++)
                    {
                        if (i < _adapters.Count)
                        {
                            // Update existing
                            _adapters[i] = adapters[i];
                        }
                        else
                        {
                            // Add new
                            _adapters.Add(adapters[i]);
                        }
                    }

                    // Remove old adapters
                    while (_adapters.Count > adapters.Count)
                    {
                        _adapters.RemoveAt(_adapters.Count - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                NoAdaptersMessage.Visibility = Visibility.Visible;
                NoAdaptersMessage.Text = "Ağ bilgisi alınamadı: " + ex.Message;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
        }
    }
}
