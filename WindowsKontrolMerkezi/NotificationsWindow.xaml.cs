using System.Windows;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi;

public partial class NotificationsWindow : Window
{
    public NotificationsWindow()
    {
        InitializeComponent();
        DgNotifications.ItemsSource = NotificationService.Notifications;
        DgHistory.ItemsSource = NotificationService.GetHistory();
        
        var settings = AppSettingsService.Load();
        var theme = ThemeService.Themes.FirstOrDefault(t => t.Id == settings.ThemeId) ?? ThemeService.Themes[0];
        ThemeService.SetTitlebarMode(this, theme);
    }

    public void LoadHistory()
    {
        // Switch to history tab
        TabHistory.IsSelected = true;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnClear_OnClick(object sender, RoutedEventArgs e)
    {
        NotificationService.ClearAll();
    }
}
