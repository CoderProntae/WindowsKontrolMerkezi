using System.IO;

namespace WindowsKontrolMerkezi.Services;

public record TempCleanupResult(int DeletedFiles, int DeletedDirs, long FreedBytes, List<string> Errors);

public static class TempCleanupService
{
    public static TempCleanupResult Clean()
    {
        int files = 0, dirs = 0;
        long freed = 0;
        var errors = new List<string>();
        var tempPath = Path.GetTempPath();
        try
        {
            foreach (var file in Directory.EnumerateFiles(tempPath, "*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var fi = new FileInfo(file);
                    var len = fi.Length;
                    fi.Delete();
                    files++;
                    freed += len;
                }
                catch (Exception ex) { errors.Add($"{System.IO.Path.GetFileName(file)}: {ex.Message}"); }
            }
            foreach (var dir in Directory.EnumerateDirectories(tempPath))
            {
                try
                {
                    var di = new DirectoryInfo(dir);
                    var size = DirSize(di);
                    di.Delete(recursive: true);
                    dirs++;
                    freed += size;
                }
                catch (Exception ex) { errors.Add($"{System.IO.Path.GetFileName(dir)}: {ex.Message}"); }
            }
        }
        catch (Exception ex) { errors.Add(ex.Message); }
        return new TempCleanupResult(files, dirs, freed, errors);
    }

    private static long DirSize(DirectoryInfo d)
    {
        long s = 0;
        try
        {
            foreach (var f in d.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try { s += f.Length; } catch { }
            }
        }
        catch { }
        return s;
    }
}
