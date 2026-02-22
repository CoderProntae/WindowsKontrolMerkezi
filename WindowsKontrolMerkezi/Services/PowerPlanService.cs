using System.Diagnostics;

namespace WindowsKontrolMerkezi.Services;

public record PowerPlanItem(string Guid, string Name, bool IsActive);

public static class PowerPlanService
{
    public static List<PowerPlanItem> GetPlans()
    {
        var list = new List<PowerPlanItem>();
        try
        {
            var psi = new ProcessStartInfo("powercfg", "/list")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return list;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(3000);
            foreach (var line in output.Split('\n'))
            {
                if (!line.Contains("GUID")) continue;
                var guidMatch = System.Text.RegularExpressions.Regex.Match(line, @"([a-f0-9-]{36})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var guid = guidMatch.Success ? guidMatch.Groups[1].Value : "";
                var active = line.Contains('*');
                var paren = line.LastIndexOf('(');
                var name = paren >= 0 ? line.Substring(paren).Trim(' ', '(', ')') : "";
                if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(name))
                    list.Add(new PowerPlanItem(guid, name, active));
            }
        }
        catch { }
        return list;
    }

    public static bool SetActive(string guid)
    {
        try
        {
            var psi = new ProcessStartInfo("powercfg", $"/setactive {guid}")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(3000);
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }
}
