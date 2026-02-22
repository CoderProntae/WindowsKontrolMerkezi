using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WindowsKontrolMerkezi;

public partial class DetailChartWindow : Window
{
    private List<UsageSnapshot> _data = new();
    private Func<IReadOnlyList<UsageSnapshot>>? _dataProvider;
    private DispatcherTimer? _liveTimer;
    private const int ChartHeight = 180;
    private const int ChartPadding = 40;

    public DetailChartWindow()
    {
        InitializeComponent();
        Closed += (_, _) => _liveTimer?.Stop();
    }

    public void SetData(IReadOnlyList<UsageSnapshot> snapshots)
    {
        _data = snapshots?.ToList() ?? new List<UsageSnapshot>();
        RefreshGrid();
        DrawChart();
    }

    /// <summary>Canlı güncelleme için veri sağlayıcı verilir; her saniye yenilenir.</summary>
    public void SetDataSource(Func<IReadOnlyList<UsageSnapshot>> dataProvider)
    {
        _dataProvider = dataProvider;
        _liveTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
        _liveTimer.Tick += (_, _) =>
        {
            if (_dataProvider != null)
                SetData(_dataProvider());
        };
        _liveTimer.Start();
        if (_dataProvider != null)
            SetData(_dataProvider());
    }

    private void RefreshGrid()
    {
        DataGridSnapshots.ItemsSource = _data.OrderByDescending(x => x.Time).ToList();
    }

    private void DrawChart()
    {
        ChartCanvas.Children.Clear();
        if (_data.Count < 2) return;

        var w = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 700;
        var h = ChartHeight;
        var count = _data.Count;
        var maxVal = 100.0;
        var stepX = (w - ChartPadding * 2) / Math.Max(1, count - 1);

        var ptsCpu = new PointCollection();
        var ptsMem = new PointCollection();
        var ptsDisk = new PointCollection();

        for (int i = 0; i < count; i++)
        {
            var x = ChartPadding + i * stepX;
            var yCpu = h - (_data[i].CpuPercent / maxVal * (h - 20)) - 10;
            var yMem = h - (_data[i].MemPercent / maxVal * (h - 20)) - 10;
            var yDisk = h - (_data[i].DiskPercent / maxVal * (h - 20)) - 10;
            ptsCpu.Add(new System.Windows.Point(x, yCpu));
            ptsMem.Add(new System.Windows.Point(x, yMem));
            ptsDisk.Add(new System.Windows.Point(x, yDisk));
        }

        var lineCpu = new Polyline { Points = ptsCpu, Stroke = (Brush)Application.Current.FindResource("Accent"), StrokeThickness = 2 };
        var lineMem = new Polyline { Points = ptsMem, Stroke = new SolidColorBrush(Color.FromRgb(6, 182, 212)), StrokeThickness = 2 };
        var lineDisk = new Polyline { Points = ptsDisk, Stroke = new SolidColorBrush(Color.FromRgb(16, 185, 129)), StrokeThickness = 2 };
        ChartCanvas.Children.Add(lineCpu);
        ChartCanvas.Children.Add(lineMem);
        ChartCanvas.Children.Add(lineDisk);
        if (count >= 1)
        {
            AddText(ChartCanvas, _data[0].Time.ToString("HH:mm:ss"), ChartPadding, h + 4);
            if (count > 2) AddText(ChartCanvas, _data[count / 2].Time.ToString("HH:mm:ss"), ChartPadding + (count / 2) * stepX, h + 4);
            AddText(ChartCanvas, _data[count - 1].Time.ToString("HH:mm:ss"), ChartPadding + (count - 1) * stepX, h + 4);
        }
        if (!ChartCanvas.Children.Contains(TooltipBorder))
            ChartCanvas.Children.Add(TooltipBorder);
    }

    private static void AddText(Canvas c, string text, double x, double y)
    {
        var tb = new TextBlock { Text = text, Foreground = (Brush)Application.Current.FindResource("TextDim"), FontSize = 10 };
        Canvas.SetLeft(tb, x - 20);
        Canvas.SetTop(tb, y);
        c.Children.Add(tb);
    }

    private void ChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(ChartCanvas);
        var w = ChartCanvas.ActualWidth;
        if (w <= 0 || _data.Count < 2) return;
        var stepX = (w - ChartPadding * 2) / Math.Max(1, _data.Count - 1);
        var idx = (int)Math.Round((pos.X - ChartPadding) / stepX);
        idx = Math.Clamp(idx, 0, _data.Count - 1);
        var s = _data[idx];
        TbTooltip.Text = $"{s.Time:HH:mm:ss}  |  CPU: {s.CpuPercent:F1}%  RAM: {s.MemPercent:F1}%  Disk: {s.DiskPercent:F1}%";
        TooltipBorder.Visibility = Visibility.Visible;
        Canvas.SetLeft(TooltipBorder, pos.X + 12);
        Canvas.SetTop(TooltipBorder, pos.Y + 8);
    }

    private void ChartCanvas_OnMouseLeave(object sender, MouseEventArgs e)
    {
        TooltipBorder.Visibility = Visibility.Collapsed;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_data.Count >= 2) DrawChart();
    }
}
