using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsKontrolMerkezi.Services;
using System.Globalization;

namespace WindowsKontrolMerkezi;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        RbPanel.IsChecked = true;
        ContentFrame.Navigate(new Uri("Pages/PanelPage.xaml", UriKind.Relative));
        
        // Notification settings
        var settings = AppSettingsService.Load();
        if (settings.OpenNotificationPanelAtStartup)
        {
            ColNotifications.Width = new GridLength(300);
        }
        
        LstNotifications.ItemsSource = NotificationService.Notifications;
        Opacity = settings.WindowOpacity;
        
        StartClock();
    }

    private void StartClock()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (s, e) =>
        {
            TbClock.Text = DateTime.Now.ToString("HH:mm");
            TbDate.Text = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
        };
        timer.Start();
        
        // Initial set
        TbClock.Text = DateTime.Now.ToString("HH:mm");
        TbDate.Text = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TbSidebarRam.Text = "RAM: " + SystemInfoService.GetRamSizeText();
        TbSidebarOs.Text = SystemInfoService.GetOsDescription();
        
        var settings = AppSettingsService.Load();
        ThemeService.ApplyTheme(settings.ThemeId);
        
        // v1.4.0 Background & Toggle Fix
        ApplyBackground(settings.ThemeId);
        BtnFloatingNotificationToggle.Visibility = settings.HideNotificationToggleButton ? Visibility.Collapsed : Visibility.Visible;
    }

    // make public so other pages can refresh it when theme changes
    private static readonly Dictionary<string, System.Windows.Media.Imaging.BitmapImage> _bgCache = new();

    public void ApplyBackground(string themeId)
    {
        var theme = ThemeService.Themes.FirstOrDefault(t => t.Id == themeId);
        if (theme?.BackgroundPath != null && File.Exists(theme.BackgroundPath))
        {
            try
            {
                if (!_bgCache.TryGetValue(theme.BackgroundPath, out var bitmap))
                {
                    bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(theme.BackgroundPath));
                    _bgCache[theme.BackgroundPath] = bitmap;
                }
                ImgBackground.Source = bitmap;
                ImgBackground.Visibility = Visibility.Visible;
            }
            catch { ImgBackground.Visibility = Visibility.Collapsed; }
        }
        else
        {
            ImgBackground.Visibility = Visibility.Collapsed;
        }
    }

    private void OnNavChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || rb.Tag is not string tag) return;
        var uri = tag switch
        {
            "Panel" => "Pages/PanelPage.xaml",
            "Modlar" => "Pages/ModlarPage.xaml",
            "Ayarlar" => "Pages/AyarlarPage.xaml",
            _ => "Pages/PanelPage.xaml"
        };
        ContentFrame.Navigate(new Uri(uri, UriKind.Relative));
    }

    private void BtnVersionCodes_OnClick(object sender, RoutedEventArgs e)
    {
        new VersionCodesWindow { Owner = this }.ShowDialog();
    }

    private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
    {
        LauncherService.OpenUninstall();
    }

    private void BtnToggleNotifications_OnClick(object sender, RoutedEventArgs e)
    {
        if (ColNotifications.Width.Value > 0)
        {
            ColNotifications.Width = new GridLength(0);
        }
        else
        {
            ColNotifications.Width = new GridLength(300);
        }
    }

    private void BtnExpandNotifications_OnClick(object sender, RoutedEventArgs e)
    {
        new NotificationsWindow { Owner = this }.Show();
    }

    private void BtnClearNotifications_OnClick(object sender, RoutedEventArgs e)
    {
        NotificationService.ClearAll();
    }

    private void BtnOpenHistory_OnClick(object sender, RoutedEventArgs e)
    {
        var win = new NotificationsWindow { Owner = this };
        win.Show();
        win.LoadHistory(); // This will be a new method in NotificationsWindow
    }

    private void NotifItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Models.NotificationModel notif)
        {
            // Show message box with details
            MessageBox.Show($"{notif.Message}\n\nKaynak: {notif.SourceApp}\nTarih: {notif.Timestamp:dd.MM.yyyy HH:mm}", notif.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
