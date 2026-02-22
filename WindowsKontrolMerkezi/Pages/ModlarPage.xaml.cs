using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsKontrolMerkezi.Services;

namespace WindowsKontrolMerkezi.Pages;

public partial class ModlarPage
{
    private ModeDefinition? _editingMode;

    public ModlarPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void RefreshState()
    {
        TbPowerState.Text = PowerPlanService.GetPlans().FirstOrDefault(p => p.IsActive)?.Name ?? "—";
        TbGameModeState.Text = GameModeService.IsEnabled() ? "Açık" : "Kapalı";
        
        var settings = AppSettingsService.Load();
        TbFocusState.Text = settings.IsIndependentDndOn ? "Açık (Uygulama İçi)" : "Kapalı";
    }

    private void BuildModeCards()
    {
        ModeCardsPanel.Items.Clear();
        var settings = AppSettingsService.Load();
        var modes = ModesService.GetAllModes(settings);
        foreach (var mode in modes)
        {
            var card = CreateModeCard(mode, settings);
            ModeCardsPanel.Items.Add(card);
        }
    }

    private Border CreateModeCard(ModeDefinition mode, AppSettings settings)
    {
        var border = new Border
        {
            Background = (Brush)FindResource("Card"),
            BorderBrush = (Brush)FindResource("Border"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 8)
        };
        var stack = new StackPanel();
        var titleRow = new Grid();
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var nameTb = new TextBlock
        {
            Text = mode.Name + (mode.IsBuiltIn ? "" : " (özel)"),
            Foreground = (Brush)FindResource("Text"),
            FontSize = 13,
            FontWeight = FontWeights.Medium
        };
        Grid.SetColumn(nameTb, 0);
        titleRow.Children.Add(nameTb);
        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };
        var btnUygula = new Button
        {
            Content = "Uygula",
            Tag = mode,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0, 0, 6, 0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Background = (Brush)FindResource("Accent"),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        btnUygula.Click += (_, _) => { ModesService.ApplyMode(mode); RefreshState(); };
        btnPanel.Children.Add(btnUygula);
        if (!string.IsNullOrEmpty(mode.SpecialActionUri))
        {
            var btnAyar = new Button
            {
                Content = "Ayarları aç",
                Tag = mode,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = Brushes.Transparent,
                Foreground = (Brush)FindResource("TextDim"),
                BorderBrush = (Brush)FindResource("Border"),
                BorderThickness = new Thickness(1)
            };
            btnAyar.Click += (_, _) => ModesService.OpenModeSpecial(mode);
            btnPanel.Children.Add(btnAyar);
        }
        if (!mode.IsBuiltIn)
        {
            var btnDuzenle = new Button
            {
                Content = "Düzenle",
                Tag = mode,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = Brushes.Transparent,
                Foreground = (Brush)FindResource("Text"),
                BorderBrush = (Brush)FindResource("Border"),
                BorderThickness = new Thickness(1)
            };
            btnDuzenle.Click += (_, _) => EnterEditMode(mode);
            btnPanel.Children.Add(btnDuzenle);

            var btnKaldir = new Button
            {
                Content = "Kaldır",
                Tag = mode,
                Padding = new Thickness(10, 4, 10, 4),
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = Brushes.IndianRed,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            btnKaldir.Click += (_, _) =>
            {
                if (MessageBox.Show($"{mode.Name} modunu silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    settings.CustomModes.RemoveAll(m => m.Id == mode.Id);
                    AppSettingsService.Save(settings);
                    BuildModeCards();
                }
            };
            btnPanel.Children.Add(btnKaldir);
        }
        Grid.SetColumn(btnPanel, 1);
        titleRow.Children.Add(btnPanel);
        stack.Children.Add(titleRow);
        var desc = GetModeDescription(mode);
        if (!string.IsNullOrEmpty(desc))
        {
            stack.Children.Add(new TextBlock
            {
                Text = desc,
                Foreground = (Brush)FindResource("TextDim"),
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0)
            });
        }
        border.Child = stack;
        return border;
    }

    private static string GetModeDescription(ModeDefinition m)
    {
        var parts = new List<string>();
        if (m.GameModeOn == true) parts.Add("Oyun modu açık");
        if (m.FocusAssistOn == true) parts.Add("Rahatsız etme açık");
        if (!string.IsNullOrEmpty(m.PowerPlanGuid)) parts.Add("Güç planı seçili");
        if (m.Id == "oyun") return "Oyunlarda kaynak önceliği, Game Bar ayarları.";
        if (m.Id == "performans") return "Yüksek performans güç planı.";
        if (m.Id == "sessiz" || m.Id == "odak") return "Bildirimleri azaltır.";
        if (m.Id == "dengeli") return "Dengeli güç planı.";
        return parts.Count > 0 ? string.Join(", ", parts) : "";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshState();
        var settings = AppSettingsService.Load();
        
        // v1.3.0 Independent DND logic
        ChkIndependentDnd.IsChecked = settings.IsIndependentDndOn;
        if (SystemInfoService.IsWindows11())
        {
            ChkIndependentDnd.IsEnabled = false;
            // Cross out look
            PanelWin11Warning.Visibility = Visibility.Visible;
            CanvasStrikethrough.Visibility = Visibility.Visible;
            // Opacity for the restricted look
            PanelDndContent.Opacity = 0.6;
        }

        CmbNewPowerPlan.ItemsSource = PowerPlanService.GetPlans();
        CmbNewPowerPlan.DisplayMemberPath = "Name";
        CmbNewPowerPlan.SelectedValuePath = "Guid";
        if (CmbNewPowerPlan.Items.Count > 0) CmbNewPowerPlan.SelectedIndex = 0;
        BuildModeCards();
    }

    private void EnterEditMode(ModeDefinition mode)
    {
        _editingMode = mode;
        TbFormTitle.Text = "Modu Düzenle: " + mode.Name;
        TxtNewModeName.Text = mode.Name;
        ChkNewGameMode.IsChecked = mode.GameModeOn == true;
        ChkNewFocusAssist.IsChecked = mode.FocusAssistOn == true;
        CmbNewPowerPlan.SelectedValue = mode.PowerPlanGuid;
        BtnAddMode.Content = "Değişiklikleri Kaydet";
        BtnCancelEdit.Visibility = Visibility.Visible;
    }

    private void ExitEditMode()
    {
        _editingMode = null;
        TbFormTitle.Text = "Özel mod ekle";
        TxtNewModeName.Clear();
        ChkNewGameMode.IsChecked = false;
        ChkNewFocusAssist.IsChecked = false;
        if (CmbNewPowerPlan.Items.Count > 0) CmbNewPowerPlan.SelectedIndex = 0;
        BtnAddMode.Content = "Mod ekle";
        BtnCancelEdit.Visibility = Visibility.Collapsed;
    }

    private void BtnCancelEdit_OnClick(object sender, RoutedEventArgs e)
    {
        ExitEditMode();
    }

    private void BtnAddMode_OnClick(object sender, RoutedEventArgs e)
    {
        var name = (TxtNewModeName.Text ?? "").Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Mod adı girin.", "Özel mod", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var settings = AppSettingsService.Load();
        settings.CustomModes ??= new List<ModeDefinition>();

        var planGuid = CmbNewPowerPlan.SelectedValue as string;
        
        if (_editingMode != null)
        {
            var target = settings.CustomModes.FirstOrDefault(m => m.Id == _editingMode.Id);
            if (target != null)
            {
                target.Name = name;
                target.GameModeOn = ChkNewGameMode.IsChecked == true ? true : null;
                target.FocusAssistOn = ChkNewFocusAssist.IsChecked == true ? true : null;
                target.PowerPlanGuid = string.IsNullOrEmpty(planGuid) ? null : planGuid;
            }
            ExitEditMode();
        }
        else
        {
            var id = "custom_" + Guid.NewGuid().ToString("N")[..8];
            var mode = new ModeDefinition
            {
                Id = id,
                Name = name,
                IsBuiltIn = false,
                Order = 100 + settings.CustomModes.Count,
                GameModeOn = ChkNewGameMode.IsChecked == true ? true : null,
                FocusAssistOn = ChkNewFocusAssist.IsChecked == true ? true : null,
                PowerPlanGuid = string.IsNullOrEmpty(planGuid) ? null : planGuid
            };
            settings.CustomModes.Add(mode);
            TxtNewModeName.Clear();
            ChkNewGameMode.IsChecked = false;
            ChkNewFocusAssist.IsChecked = false;
        }
        
        AppSettingsService.Save(settings);
        BuildModeCards();
    }

    private void ChkIndependentDnd_OnChanged(object sender, RoutedEventArgs e)
    {
        if (ChkIndependentDnd == null) return;
        var settings = AppSettingsService.Load();
        settings.IsIndependentDndOn = ChkIndependentDnd.IsChecked == true;
        AppSettingsService.Save(settings);
    }

    private void BtnWindowsAyarlari_OnClick(object sender, RoutedEventArgs e) => LauncherService.OpenWindowsSettings();
}
