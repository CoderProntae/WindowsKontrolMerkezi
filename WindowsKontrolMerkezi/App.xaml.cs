using System.Windows;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var settings = AppSettingsService.Load();
        ThemeService.ApplyTheme(settings.ThemeId);
        if (settings.StartWithWindows != StartupService.IsEnabled())
            StartupService.SetEnabled(settings.StartWithWindows);
        if (settings.CheckUpdatesAtStartup)
            CheckUpdatesAtStartupAsync();
    }

    private static async void CheckUpdatesAtStartupAsync()
    {
        try
        {
            await Task.Delay(2000);
            var r = await UpdateService.CheckForUpdatesAsync();
            if (!r.HasUpdate) return;
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    $"Yeni sürüm mevcut: {r.Latest} (sizde: {r.Current}).\n\nAyarlar sayfasından \"Güncelle\" ile güncelleyebilirsiniz.",
                    "Güncelleme",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }
        catch { }
    }
}
