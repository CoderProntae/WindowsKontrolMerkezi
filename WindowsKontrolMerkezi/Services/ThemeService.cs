using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.IO; // Added for Path.Combine

namespace WindowsKontrolMerkezi.Services;

public record ThemeDef(string Id, string Name, Color Surface, Color Card, Color CardAlt, Color Border, Color BorderAlt, Color Accent, Color AccentDim, Color Text, Color TextDim, bool IsDark, string? BackgroundPath = null);

public static class ThemeService
{
    private const string ArtifactDir = @"C:\Users\ilkn4\.gemini\antigravity\brain\454b312a-ab14-4d26-931d-4b46d8c1ad8d";

    public static readonly ThemeDef[] Themes =
    {
        new("dark", "Koyu (Varsayılan)", ColorFromHex("#0B0B0E"), ColorFromHex("#121216"), ColorFromHex("#1A1A1F"), ColorFromHex("#1A1A1F"), ColorFromHex("#2A2A2E"), ColorFromHex("#8B5CF6"), ColorFromHex("#4C1D95"), ColorFromHex("#F4F4F5"), ColorFromHex("#A1A1AA"), true),
        new("light", "Açık", ColorFromHex("#FFFFFF"), ColorFromHex("#F3F4F6"), ColorFromHex("#E5E7EB"), ColorFromHex("#E5E7EB"), ColorFromHex("#D1D5DB"), ColorFromHex("#7C3AED"), ColorFromHex("#A78BFA"), ColorFromHex("#111827"), ColorFromHex("#4B5563"), false),
        new("blue", "Mavi", ColorFromHex("#0C1222"), ColorFromHex("#111827"), ColorFromHex("#1F2937"), ColorFromHex("#1F2937"), ColorFromHex("#1E3A5F"), ColorFromHex("#3B82F6"), ColorFromHex("#1D4ED8"), ColorFromHex("#E5E7EB"), ColorFromHex("#9CA3AF"), true),
        new("green", "Yeşil", ColorFromHex("#0D1117"), ColorFromHex("#161B22"), ColorFromHex("#1F242C"), ColorFromHex("#1F242C"), ColorFromHex("#21262D"), ColorFromHex("#22C55E"), ColorFromHex("#15803D"), ColorFromHex("#E6EDF3"), ColorFromHex("#8B949E"), true),
        new("orange", "Turuncu", ColorFromHex("#1C1917"), ColorFromHex("#292524"), ColorFromHex("#44403C"), ColorFromHex("#44403C"), ColorFromHex("#57534E"), ColorFromHex("#F97316"), ColorFromHex("#C2410C"), ColorFromHex("#FAFAF9"), ColorFromHex("#A8A29E"), true),
        new("rose", "Pembe", ColorFromHex("#1C1917"), ColorFromHex("#292524"), ColorFromHex("#44403C"), ColorFromHex("#44403C"), ColorFromHex("#57534E"), ColorFromHex("#F43F5E"), ColorFromHex("#BE123C"), ColorFromHex("#FAFAF9"), ColorFromHex("#A8A29E"), true),
        new("fish", "🐠 Balıklar (Özel)", ColorFromHex("#041B2D"), ColorFromHex("#062841"), ColorFromHex("#093A5E"), ColorFromHex("#093A5E"), ColorFromHex("#0D4D7A"), ColorFromHex("#00D2FF"), ColorFromHex("#0095B6"), ColorFromHex("#E0F7FA"), ColorFromHex("#81D4FA"), true, Path.Combine(ArtifactDir, "fish_theme_background_1771796077793.png")),
        new("lava", "🌋 Lav (Özel)", ColorFromHex("#120000"), ColorFromHex("#1A0000"), ColorFromHex("#2A0000"), ColorFromHex("#2A0000"), ColorFromHex("#3A0000"), ColorFromHex("#FF4500"), ColorFromHex("#B22222"), ColorFromHex("#FFF5F5"), ColorFromHex("#FFBABA"), true, Path.Combine(ArtifactDir, "lava_theme_background_1771796093605.png")),
        new("sunset", "🌅 Gün Batımı (Özel)", ColorFromHex("#1A0B2E"), ColorFromHex("#2D144A"), ColorFromHex("#4A1F6E"), ColorFromHex("#4A1F6E"), ColorFromHex("#632B94"), ColorFromHex("#FF7E5F"), ColorFromHex("#FEB47B"), ColorFromHex("#FFF1F1"), ColorFromHex("#FFD1D1"), true),
    };

    private static Color ColorFromHex(string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex);
    }

    public static void ApplyTheme(string themeId)
    {
        var theme = Themes.FirstOrDefault(t => t.Id == themeId) ?? Themes[0];
        var app = Application.Current;
        if (app?.Resources == null) return;
        
        app.Resources["Surface"] = new SolidColorBrush(theme.Surface);
        app.Resources["Card"] = new SolidColorBrush(theme.Card);
        app.Resources["CardAlt"] = new SolidColorBrush(theme.CardAlt);
        app.Resources["Border"] = new SolidColorBrush(theme.Border);
        app.Resources["Accent"] = new SolidColorBrush(theme.Accent);
        app.Resources["AccentDim"] = new SolidColorBrush(theme.AccentDim);
        app.Resources["Text"] = new SolidColorBrush(theme.Text);
        app.Resources["TextDim"] = new SolidColorBrush(theme.TextDim);
        
        var glassColor = theme.CardAlt;
        glassColor.A = 200;
        app.Resources["GlassBg"] = new SolidColorBrush(glassColor);

        foreach (Window window in app.Windows)
        {
            SetTitlebarMode(window, theme);
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    public static void SetTitlebarMode(Window window, ThemeDef theme)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) => SetTitlebarMode(window, theme);
            return;
        }

        // Dark mode attribute (Win10+)
        int useImmersiveDarkMode = theme.IsDark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, 20, ref useImmersiveDarkMode, sizeof(int));
        DwmSetWindowAttribute(hwnd, 19, ref useImmersiveDarkMode, sizeof(int));

        // Win11 Titlebar colors (Caption and Text)
        // Convert COLORREF format (0x00BBGGRR)
        int captionColor = (theme.Surface.B << 16) | (theme.Surface.G << 8) | theme.Surface.R;
        int textColor = (theme.Text.B << 16) | (theme.Text.G << 8) | theme.Text.R;
        
        // DWMWA_CAPTION_COLOR = 35
        // DWMWA_TEXT_COLOR = 36
        DwmSetWindowAttribute(hwnd, 35, ref captionColor, sizeof(int));
        DwmSetWindowAttribute(hwnd, 36, ref textColor, sizeof(int));
    }
}
