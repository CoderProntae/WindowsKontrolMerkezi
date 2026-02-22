using System.IO;
using System.Windows;
using System.Windows.Controls;
using WindowsKontrolMerkezi.Services;
using WindowsKontrolMerkezi;

namespace WindowsKontrolMerkezi.Pages;

public partial class AyarlarPage
{
    private string? _downloadUrl;

    public AyarlarPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TbVersion.Text = "Mevcut sürüm: " + VersionInfo.DisplayVersion;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CHANGELOG.md");
        TbChangelog.Text = File.Exists(path) ? File.ReadAllText(path) : "CHANGELOG.md bulunamadı.";

        CmbTheme.ItemsSource = ThemeService.Themes;
        CmbTheme.DisplayMemberPath = "Name";
        CmbTheme.SelectedValuePath = "Id";
        var settings = AppSettingsService.Load();
        CmbTheme.SelectedItem = ThemeService.Themes.FirstOrDefault(t => t.Id == settings.ThemeId) ?? ThemeService.Themes[0];
        ChkStartWithWindows.IsChecked = settings.StartWithWindows;
        ChkCheckUpdatesAtStartup.IsChecked = settings.CheckUpdatesAtStartup;
        ChkNotifStartup.IsChecked = settings.OpenNotificationPanelAtStartup;
        SldOpacity.Value = settings.WindowOpacity;
        TbOpacityVal.Text = $"%{(int)(settings.WindowOpacity * 100)}";
        
        UpdateStartupHint();
    }

    private void ChkNotifStartup_Changed(object sender, RoutedEventArgs e)
    {
        var on = ChkNotifStartup.IsChecked == true;
        var s = AppSettingsService.Load();
        s.OpenNotificationPanelAtStartup = on;
        AppSettingsService.Save(s);
    }

    private void UpdateStartupHint()
    {
        var exePath = StartupService.GetExePath();
        TbStartupHint.Text = string.IsNullOrEmpty(exePath)
            ? "Şu an dotnet ile çalışıyor olabilirsiniz. Yayınlanmış .exe ile çalıştırdığınızda bu seçenek exe yolunu kaydeder."
            : "Kayıtlı yol: " + exePath;
    }

    private void CmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTheme.SelectedItem is not ThemeDef t) return;
        ThemeService.ApplyTheme(t.Id);
        var s = AppSettingsService.Load();
        s.ThemeId = t.Id;
        AppSettingsService.Save(s);
    }

    private void ChkStartWithWindows_Changed(object sender, RoutedEventArgs e)
    {
        var on = ChkStartWithWindows.IsChecked == true;
        if (on && string.IsNullOrEmpty(StartupService.GetExePath()))
        {
            MessageBox.Show("Şu an .exe olarak çalışmıyorsunuz. Uygulamayı yayınlayıp (publish) .exe ile çalıştırdığınızda bu seçenek kullanılabilir.", "Windows ile başlat", MessageBoxButton.OK, MessageBoxImage.Information);
            ChkStartWithWindows.IsChecked = false;
            return;
        }
        StartupService.SetEnabled(on);
        var s = AppSettingsService.Load();
        s.StartWithWindows = on;
        AppSettingsService.Save(s);
        UpdateStartupHint();
    }

    private void SldOpacity_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TbOpacityVal == null) return;
        var val = e.NewValue;
        TbOpacityVal.Text = $"%{(int)(val * 100)}";
        
        var s = AppSettingsService.Load();
        s.WindowOpacity = val;
        AppSettingsService.Save(s);
        
        // Apply instantly
        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Opacity = val;
        }
    }

    private void ChkCheckUpdatesAtStartup_Changed(object sender, RoutedEventArgs e)
    {
        var on = ChkCheckUpdatesAtStartup.IsChecked == true;
        var s = AppSettingsService.Load();
        s.CheckUpdatesAtStartup = on;
        AppSettingsService.Save(s);
    }

    private void BtnMaintenance_OnClick(object sender, RoutedEventArgs e)
    {
        TbMaintenanceResult.Visibility = Visibility.Visible;
        BtnMaintenance.IsEnabled = false;
        try
        {
            var r = TempCleanupService.Clean();
            var mb = r.FreedBytes / 1024.0 / 1024.0;
            var msg = $"%TEMP%: {r.DeletedFiles} dosya, {r.DeletedDirs} klasör silindi, ~{mb:F1} MB boşaldı. ";
            if (RamCleanupService.TrimCurrentProcessWorkingSet())
                msg += "Bu uygulamanın kullanmadığı RAM iade edildi. Önemli uygulamalar kapatılmadı.";
            else
                msg += "RAM küçültme atlandı.";
            if (r.Errors.Count > 0)
                msg += " Bazı öğeler silinemedi (kullanımda olabilir).";
            TbMaintenanceResult.Text = msg;
        }
        catch (Exception ex)
        {
            TbMaintenanceResult.Text = "Hata: " + ex.Message;
        }
        BtnMaintenance.IsEnabled = true;
    }

    private void BtnTempClean_OnClick(object sender, RoutedEventArgs e)
    {
        BtnTempClean.IsEnabled = false;
        TbMaintenanceResult.Visibility = Visibility.Visible;
        try
        {
            var r = TempCleanupService.Clean();
            var mb = r.FreedBytes / 1024.0 / 1024.0;
            TbMaintenanceResult.Text = $"{r.DeletedFiles} dosya, {r.DeletedDirs} klasör silindi. Yaklaşık {mb:F1} MB boşaltıldı. Önemli dosyalar silinmez.";
            if (r.Errors.Count > 0)
                TbMaintenanceResult.Text += " Bazı öğeler silinemedi (kullanımda olabilir).";
        }
        catch (Exception ex)
        {
            TbMaintenanceResult.Text = "Hata: " + ex.Message;
        }
        BtnTempClean.IsEnabled = true;
    }

    private void BtnRamClean_OnClick(object sender, RoutedEventArgs e)
    {
        TbMaintenanceResult.Visibility = Visibility.Visible;
        if (RamCleanupService.TrimCurrentProcessWorkingSet())
            TbMaintenanceResult.Text = "Bu uygulamanın kullanmadığı bellek işletim sistemine iade edildi. Sadece geçici/önemli olmayan bellek; başka uygulama kapatılmaz.";
        else
            TbMaintenanceResult.Text = "İşlem yapılamadı.";
    }

    private async void BtnCheckUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        BtnCheckUpdate.IsEnabled = false;
        BtnCheckUpdate.Content = "Kontrol ediliyor...";
        try
        {
            var r = await UpdateService.CheckForUpdatesAsync();
            _downloadUrl = r.DownloadUrl;
            UpdateResultPanel.Visibility = Visibility.Visible;
            if (r.HasUpdate)
            {
                var featureText = r.Features.Count > 0 
                    ? "\n\nGeliştirilen Özellikler:\n" + string.Join("\n", r.Features.Select(f => $"- {f.Name} (v{f.Version})"))
                    : "";
                    
                TbUpdateResult.Text = $"Yeni sürüm: {r.Latest} (Dal: {r.Channel})\nMevcut: {r.Current}\n\n{r.Notes}{featureText}";
                var hasUrl = !string.IsNullOrWhiteSpace(r.DownloadUrl);
                BtnUpdate.Visibility = hasUrl ? Visibility.Visible : Visibility.Collapsed;
                BtnDownload.Visibility = hasUrl ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                TbUpdateResult.Text = $"Uygulama güncel. (v{r.Current})";
                BtnUpdate.Visibility = Visibility.Collapsed;
                BtnDownload.Visibility = Visibility.Collapsed;
            }
        }
        finally
        {
            BtnCheckUpdate.IsEnabled = true;
            BtnCheckUpdate.Content = "Güncellemeleri kontrol et";
        }
    }

    private async void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_downloadUrl)) return;
        BtnUpdate.IsEnabled = false;
        BtnUpdate.Content = "İndiriliyor...";
        try
        {
            var ok = await UpdateService.DownloadAndRunUpdateAsync(_downloadUrl);
            if (ok)
            {
                MessageBox.Show("Güncelleme indirildi ve kurulum başlatıldı. Uygulama kapanacak; kurulumu tamamlayın.", "Güncelleme", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("İndirme veya çalıştırma başarısız. \"Sadece indir\" ile linki açıp elle indirebilirsiniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                BtnUpdate.IsEnabled = true;
                BtnUpdate.Content = "Güncelle (indir ve kur)";
            }
        }
        catch
        {
            BtnUpdate.IsEnabled = true;
            BtnUpdate.Content = "Güncelle (indir ve kur)";
        }
    }

    private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
    {
        LauncherService.OpenUrl(_downloadUrl);
    }
}
