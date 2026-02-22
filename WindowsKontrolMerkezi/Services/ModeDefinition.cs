namespace WindowsKontrolMerkezi.Services;

/// <summary>Kullanıcı tanımlı veya yerleşik mod. Özelleştirilebilir.</summary>
public class ModeDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsBuiltIn { get; set; }
    public string? PowerPlanGuid { get; set; }
    public bool? GameModeOn { get; set; }
    public bool? FocusAssistOn { get; set; }
    /// <summary>Özel: açılacak ayar URI veya boş. Örn. gaming-gamemode, powersettings.</summary>
    public string? SpecialActionUri { get; set; }
    public int Order { get; set; }
}
