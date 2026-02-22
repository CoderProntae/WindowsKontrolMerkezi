using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WindowsKontrolMerkezi.Services;
using UsageSnapshot = WindowsKontrolMerkezi.UsageSnapshot;

namespace WindowsKontrolMerkezi.Pages;

public partial class PanelPage
{
    private readonly DispatcherTimer _timer;
    private const int MaxSamples = 25;
    private const int DetailHistoryCount = 120; // ~2 dakika, 1 sn aralık
    private readonly List<double> _cpuSamples = new(), _memSamples = new(), _diskSamples = new();
    private readonly List<UsageSnapshot> _detailHistory = new();
    private static readonly double RingCircumference = (120 - 8) * Math.PI / 8; // (D-T)*PI / T

    public PanelPage()
    {
        InitializeComponent();
        _timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
    }

    private static void SetRing(Ellipse ring, double percent)
    {
        if (double.IsNaN(percent)) percent = 0;
        if (percent < 0) percent = 0;
        if (percent > 100) percent = 100;
        
        var dash = (percent / 100.0) * RingCircumference;
        ring.StrokeDashArray = new DoubleCollection { dash, RingCircumference };
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var cpu = SystemInfoService.GetCpuUsage();
        var (_, _, memPct) = SystemInfoService.GetMemory();
        var (_, _, diskPct) = SystemInfoService.GetDiskC();

        TbCpu.Text = $"{cpu:F0}%";
        TbMem.Text = $"{memPct:F0}%";
        TbDisk.Text = $"{diskPct:F0}%";
        SetRing(CpuRing, cpu);
        SetRing(MemRing, memPct);
        SetRing(DiskRing, diskPct);

        _cpuSamples.Add(cpu);
        _memSamples.Add(memPct);
        _diskSamples.Add(diskPct);
        if (_cpuSamples.Count > MaxSamples) _cpuSamples.RemoveAt(0);
        if (_memSamples.Count > MaxSamples) _memSamples.RemoveAt(0);
        if (_diskSamples.Count > MaxSamples) _diskSamples.RemoveAt(0);

        _detailHistory.Add(new UsageSnapshot(DateTime.Now, cpu, memPct, diskPct));
        while (_detailHistory.Count > DetailHistoryCount) _detailHistory.RemoveAt(0);

        UpdateBars(CpuBars, _cpuSamples, "#7C3AED");
        UpdateBars(MemBars, _memSamples, "#06B6D4");
        UpdateBars(DiskBars, _diskSamples, "#10B981");

        var (down, up) = NetworkUsageService.GetSpeedMbps();
        TbNetwork.Text = $"↓ {down:F1} Mbps  ↑ {up:F1} Mbps";
        TbSystem.Text = $"{SystemInfoService.GetMachineName()} — {SystemInfoService.GetOsDescription()}";
    }

    private static void UpdateBars(System.Windows.Controls.Panel panel, List<double> samples, string color)
    {
        panel.Children.Clear();
        if (samples.Count == 0) return;
        var h = 120.0 / MaxSamples;
        for (int i = 0; i < samples.Count; i++)
        {
            var pct = samples[i] / 100.0;
            var r = new Rectangle
            {
                Height = Math.Max(2, h - 1),
                Width = 40 + pct * 80,
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(color)!,
                Margin = new Thickness(0, 0, 0, 1)
            };
            panel.Children.Add(r);
        }
    }

    private void BtnDetayliInceleme_OnClick(object sender, RoutedEventArgs e)
    {
        var win = new DetailChartWindow { Owner = Window.GetWindow(this) };
        win.SetDataSource(() => _detailHistory);
        win.ShowDialog();
    }

    private void BtnUyku_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Bilgisayar uyku moduna alınsın mı?", "Uyku", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            QuickActionsService.Sleep();
    }

    private void BtnYenidenBaslat_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Bilgisayar şimdi yeniden başlatılacak. Devam edilsin mi?", "Yeniden başlat", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            QuickActionsService.Restart();
    }
}
