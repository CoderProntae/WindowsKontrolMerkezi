using System.Windows;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi;

public partial class NotificationsWindow : Window
{
    public NotificationsWindow()
    {
        InitializeComponent();
        DgNotifications.ItemsSource = NotificationService.Notifications;
        
        var settings = AppSettingsService.Load();
        var theme = ThemeService.Themes.FirstOrDefault(t => t.Id == settings.ThemeId) ?? ThemeService.Themes[0];
        ThemeService.SetTitlebarMode(this, theme);
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnClear_OnClick(object sender, RoutedEventArgs e)
    {
        NotificationService.ClearAll();
    }

    private void BtnGo_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Models.NotificationModel notif)
        {
            if (!string.IsNullOrEmpty(notif.ActionUrl))
            {
                try
                {
                    if (notif.ActionUrl.StartsWith("ms-settings:"))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(notif.ActionUrl) { UseShellExecute = true });
                    }
                    else if (notif.ActionUrl.StartsWith("page:"))
                    {
                        // Note: This requires access to MainWindow's Frame. 
                        // In a real app, you'd use a navigation service or event.
                        // For simplicity, we assume the user will navigate from the main side panel.
                    }
                }
                catch { }
            }
        }
    }
}
