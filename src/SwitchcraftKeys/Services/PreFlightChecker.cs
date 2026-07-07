using System;
using System.IO;
using System.Text.Json;
using SwitchcraftKeys.Models;

namespace SwitchcraftKeys.Services;

/// <summary>
/// Startup health check result.
/// </summary>
public class PreFlightReport
{
    public bool SettingsReset { get; set; }
    public bool CacheCleared { get; set; }
    public bool LogDirUnavailable { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// Pre-flight validation and recovery (config corruption, cache, log dir).
/// </summary>
public static class PreFlightChecker
{
    // %APPDATA%\SwitchcraftKeys
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SwitchcraftKeys");

    private static readonly string ConfigPath = Path.Combine(AppDataDir, "config.json");

    // %LOCALAPPDATA%\SwitchcraftKeys
    private static readonly string LocalAppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SwitchcraftKeys");

    private static readonly string CacheDir = Path.Combine(LocalAppDataDir, "cache");
    private static readonly string LogDir = Path.Combine(AppDataDir, "logs");

    /// <summary>
    /// Run startup checks: config, cache, log dir. Auto-repair corruption.
    /// </summary>
    public static PreFlightReport Run()
    {
        var report = new PreFlightReport();

        CheckConfig(report);
        CheckCache(report);
        CheckLogDir(report);

        return report;
    }

    /// <summary>
    /// Validate AppConfig JSON. Auto-reset on corruption, keep .corrupted backup.
    /// </summary>
    private static void CheckConfig(PreFlightReport report)
    {
        if (!File.Exists(ConfigPath))
            return;

        try
        {
            var json = File.ReadAllText(ConfigPath);
            JsonSerializer.Deserialize<AppConfig>(json);
        }
        catch (Exception ex)
        {
            report.Warnings.Add($"Config corrupted: {ex.Message}. Resetting to defaults.");
            report.SettingsReset = true;

            var backupPath = ConfigPath + ".corrupted";
            try
            {
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(ConfigPath, backupPath);
                report.Warnings.Add($"Backup saved: {backupPath}");
            }
            catch
            {
                try { File.Delete(ConfigPath); }
                catch { }
            }
        }
    }

    /// <summary>
    /// Check cache directory accessibility and integrity.
    /// </summary>
    private static void CheckCache(PreFlightReport report)
    {
        if (!Directory.Exists(CacheDir))
            return;

        // Try listing files. If fails, clear entire cache.
        try
        {
            var dirs = Directory.GetDirectories(CacheDir);
            foreach (var dir in dirs)
            {
                try
                {
                    Directory.GetFiles(dir);
                }
                catch
                {
                    report.Warnings.Add($"Cache subdirectory inaccessible: {dir}. Clearing cache.");
                    ClearCache(report);
                    return;
                }
            }
        }
        catch
        {
            report.Warnings.Add("Cache directory inaccessible. Clearing cache.");
            ClearCache(report);
        }
    }

    /// <summary>
    /// Clear cache directory and recreate empty.
    /// </summary>
    private static void ClearCache(PreFlightReport report)
    {
        try
        {
            if (Directory.Exists(CacheDir))
            {
                Directory.Delete(CacheDir, true);
                Directory.CreateDirectory(CacheDir);
                report.CacheCleared = true;
            }
        }
        catch (Exception ex)
        {
            report.Errors.Add($"Failed to clear cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Test log directory write access.
    /// </summary>
    private static void CheckLogDir(PreFlightReport report)
    {
        try
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);

            var testFile = Path.Combine(LogDir, ".write-test");
            File.WriteAllText(testFile, "");
            File.Delete(testFile);
        }
        catch
        {
            report.Warnings.Add("Log directory not writable. File logging disabled.");
            report.LogDirUnavailable = true;
        }
    }

    /// <summary>
    /// Run interactive health check and print report (for CLI --check).
    /// </summary>
    public static void RunHealthCheck()
    {
        Console.WriteLine("=== SwitchcraftKeys Health Check ===\n");

        // Config
        Console.Write("Config file ...... ");
        if (File.Exists(ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(ConfigPath);
                JsonSerializer.Deserialize<AppConfig>(json);
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CORRUPTED: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("NOT FOUND (fresh install)");
        }

        // Cache
        Console.Write("Cache directory .. ");
        if (Directory.Exists(CacheDir))
        {
            try
            {
                var files = Directory.GetFiles(CacheDir, "*", SearchOption.AllDirectories);
                var size = files.Sum(f => new FileInfo(f).Length);
                Console.WriteLine($"OK ({files.Length} files, {FormatBytes(size)})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("EMPTY");
        }

        // Log dir
        Console.Write("Log directory .... ");
        try
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);
            var testFile = Path.Combine(LogDir, ".write-test");
            File.WriteAllText(testFile, "");
            File.Delete(testFile);
            Console.WriteLine("OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NOT WRITABLE: {ex.Message}");
        }

        // .NET runtime
        Console.Write(".NET runtime ..... ");
        Console.WriteLine($"OK (v{Environment.Version})");

        // OS
        Console.Write("OS platform ...... ");
        Console.WriteLine(Environment.OSVersion);

        // Avalonia
        Console.Write("Avalonia UI ...... ");
        var avaloniaAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Avalonia");
        if (avaloniaAsm is not null)
            Console.WriteLine($"OK (v{avaloniaAsm.GetName().Version})");
        else
            Console.WriteLine("PRESENT (not loaded yet)");

        Console.WriteLine("\n=== End ===");
    }

    /// <summary>
    /// Format bytes to human-readable (B, KB, MB, GB, TB).
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double n = bytes;
        foreach (var u in units)
        {
            if (n < 1024) return $"{n:F1} {u}";
            n /= 1024;
        }
        return $"{n:F1} TB";
    }
}
