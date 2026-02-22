using System.Windows;

namespace WindowsKontrolMerkezi.Services;

public static class ModesService
{
    /// <summary>Yerleşik modlar (sabit). Her birinin özel aksiyonu var.</summary>
    public static IReadOnlyList<ModeDefinition> BuiltInModes => new[]
    {
        new ModeDefinition { Id = "oyun", Name = "Oyun modu", IsBuiltIn = true, Order = 0, GameModeOn = true, SpecialActionUri = "ms-settings:gaming-gamemode" },
        new ModeDefinition { Id = "performans", Name = "Yüksek performans", IsBuiltIn = true, Order = 1, SpecialActionUri = "powercfg.cpl" },
        new ModeDefinition { Id = "sessiz", Name = "Sessiz mod", IsBuiltIn = true, Order = 2, FocusAssistOn = true, SpecialActionUri = "ms-settings:quiethours" },
        new ModeDefinition { Id = "odak", Name = "Rahatsız etme", IsBuiltIn = true, Order = 3, FocusAssistOn = true, GameModeOn = false, SpecialActionUri = "ms-settings:quiethours" },
        new ModeDefinition { Id = "dengeli", Name = "Dengeli", IsBuiltIn = true, Order = 4, SpecialActionUri = "powercfg.cpl" },
    };

    /// <summary>Tüm modlar (yerleşik + özel, sıralı).</summary>
    public static List<ModeDefinition> GetAllModes(AppSettings settings)
    {
        var list = BuiltInModes.ToList();
        var custom = settings.CustomModes ?? new List<ModeDefinition>();
        foreach (var c in custom.OrderBy(x => x.Order))
            list.Add(c);
        return list.OrderBy(x => x.Order).ToList();
    }

    /// <summary>Modu uygula: güç planı, oyun modu, odak yardımı.</summary>
    public static void ApplyMode(ModeDefinition mode)
    {
        if (!string.IsNullOrEmpty(mode.PowerPlanGuid))
            PowerPlanService.SetActive(mode.PowerPlanGuid);
        if (mode.GameModeOn.HasValue)
            GameModeService.SetEnabled(mode.GameModeOn.Value);
        if (mode.FocusAssistOn.HasValue && !SystemInfoService.IsWindows11())
        {
            var settings = AppSettingsService.Load();
            settings.IsIndependentDndOn = mode.FocusAssistOn.Value;
            AppSettingsService.Save(settings);
            
            // Also try to set system Focus Assist if needed, but our Independent DND is primary now
            FocusAssistService.SetFocusAssist(mode.FocusAssistOn.Value);
        }
        if (mode.Id == "performans" || mode.Id == "dengeli")
        {
            var plans = PowerPlanService.GetPlans();
            var guid = mode.Id == "performans"
                ? plans.FirstOrDefault(p => p.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("High performance", StringComparison.OrdinalIgnoreCase))?.Guid
                : plans.FirstOrDefault(p => p.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase))?.Guid;
            if (!string.IsNullOrEmpty(guid))
                PowerPlanService.SetActive(guid);
        }
        if (!string.IsNullOrEmpty(mode.SpecialActionUri))
        {
            try
            {
                if (mode.SpecialActionUri == "powercfg.cpl")
                    LauncherService.OpenPowerSettings();
                else
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mode.SpecialActionUri) { UseShellExecute = true });
            }
            catch { }
        }
    }

    public static void OpenModeSpecial(ModeDefinition mode)
    {
        if (string.IsNullOrEmpty(mode.SpecialActionUri)) return;
        try
        {
            if (mode.SpecialActionUri == "powercfg.cpl")
                LauncherService.OpenPowerSettings();
            else
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mode.SpecialActionUri) { UseShellExecute = true });
        }
        catch { }
    }
}
