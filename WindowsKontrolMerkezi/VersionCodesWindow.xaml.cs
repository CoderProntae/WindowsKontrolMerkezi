using System.Windows;

namespace WindowsKontrolMerkezi;

public partial class VersionCodesWindow : Window
{
    public VersionCodesWindow()
    {
        InitializeComponent();
        LoadFeatures();
    }

    private async void LoadFeatures()
    {
        GridFeatures.ItemsSource = VersionInfo.FeatureVersions;
        
        // Check for updates to show in logs or UI if needed
        try
        {
            var r = await Services.UpdateService.CheckForUpdatesAsync();
            // Future improvement: Bind Durum column to actual update check status
        }
        catch { }
    }
}
