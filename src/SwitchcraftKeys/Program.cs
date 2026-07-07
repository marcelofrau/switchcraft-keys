using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Logging;
using SwitchcraftKeys.Services;

namespace SwitchcraftKeys;

internal sealed class Program
{
    // Avalonia configuration — do not remove
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Parse CLI args before any logging setup
            foreach (var arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "--help":
                    case "-h":
                    case "-?":
                        ShowHelp();
                        return;

                    case "--version":
                    case "-v":
                        ShowVersion();
                        return;

                    case "--check":
                        HandleCheck();
                        return;

                    case "--reset-cache":
                    case "-c":
                        HandleResetCache();
                        return;

                    case "--reset-data":
                    case "-r":
                        HandleResetData();
                        return;
                }
            }

            var configService = new ConfigService();
            var applicationLogService = new ApplicationLogService();
            using var loggerBootstrap = new LoggerBootstrap(configService, applicationLogService);
            var loggerFactory = loggerBootstrap.Configure();

            RunLogLevelSmokeTest(loggerFactory);

            using var singleInstanceGuard = SingleInstanceGuard.TryAcquire(loggerFactory.CreateLogger<SingleInstanceGuard>());
            if (singleInstanceGuard is null)
            {
                return;
            }

            App.LoggerFactory = loggerFactory;
            App.ConfigService = configService;
            App.DeviceService = new DeviceService(loggerFactory.CreateLogger<DeviceService>());
            App.LayoutService = new LayoutService(loggerFactory.CreateLogger<LayoutService>());
            App.ApplicationLogService = applicationLogService;
            App.UpdateMinimumLogLevel = loggerBootstrap.UpdateMinimumLevel;

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // If startup fails, try to show error
            try
            {
                Console.Error.WriteLine($"FATAL: {ex}");
            }
            catch
            {
                // Last resort
            }

            Environment.Exit(1);
        }
    }

    // TEMP: smoke test log colors — remover após validar em Fase 1.
    // Não há GUI funcional ainda; isso permite conferir visualmente as cores
    // de cada nível no console (quando anexado) e no arquivo de log.
    private static void RunLogLevelSmokeTest(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SmokeTest");
        logger.LogTrace("Smoke test: Trace level");
        logger.LogDebug("Smoke test: Debug level");
        logger.LogInformation("Smoke test: Information level");
        logger.LogWarning("Smoke test: Warning level");
        logger.LogError("Smoke test: Error level");
        logger.LogCritical("Smoke test: Critical level");
    }

    private static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("SwitchcraftKeys — Device-aware keyboard layout manager for Windows");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  SwitchcraftKeys [options]");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  --help, -h, -?       Show this help message");
        Console.WriteLine("  --version, -v        Show version information");
        Console.WriteLine("  --check              Run health checks and print report");
        Console.WriteLine("  --reset-cache, -c    Clear device layout cache");
        Console.WriteLine("  --reset-data, -r     Reset all app data (config, cache, logs)");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  SwitchcraftKeys                Normal startup");
        Console.WriteLine("  SwitchcraftKeys --check        Run diagnostics");
        Console.WriteLine("  SwitchcraftKeys --reset-cache  Clear cache and exit");
        Console.WriteLine("  SwitchcraftKeys --reset-data   Reset everything and exit");
        Console.WriteLine();
    }

    private static void ShowVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version ?? new Version(0, 1, 0, 0);
        Console.WriteLine($"SwitchcraftKeys v{version.Major}.{version.Minor}.{version.Build}");
    }

    private static void HandleCheck()
    {
        PreFlightChecker.RunHealthCheck();
    }

    private static void HandleResetCache()
    {
        Console.WriteLine("Resetting device layout cache...");
        
        var localAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SwitchcraftKeys");
        var cacheDir = Path.Combine(localAppData, "cache");

        if (!Directory.Exists(cacheDir))
        {
            Console.WriteLine("Cache directory not found. Nothing to reset.");
            return;
        }

        try
        {
            Directory.Delete(cacheDir, true);
            Directory.CreateDirectory(cacheDir);
            Console.WriteLine($"Cache cleared: {cacheDir}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to clear cache: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void HandleResetData()
    {
        Console.WriteLine();
        Console.WriteLine("WARNING: This will delete all application data.");
        Console.WriteLine();
        Console.WriteLine("Affected directories:");

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchcraftKeys");
        var localAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SwitchcraftKeys");

        Console.WriteLine($"  • Config & Logs:  {appData}");
        Console.WriteLine($"  • Cache:          {localAppData}");
        Console.WriteLine();

        Console.Write("Type 'yes' to confirm reset (or press Enter to cancel): ");
        var response = Console.ReadLine();

        if (response?.ToLowerInvariant() != "yes")
        {
            Console.WriteLine("Reset cancelled.");
            return;
        }

        Console.WriteLine();
        var errors = new List<string>();

        // Delete config + logs
        if (Directory.Exists(appData))
        {
            try
            {
                Directory.Delete(appData, true);
                Console.WriteLine($"Deleted: {appData}");
            }
            catch (Exception ex)
            {
                errors.Add($"Config/logs: {ex.Message}");
            }
        }

        // Delete cache
        if (Directory.Exists(localAppData))
        {
            try
            {
                Directory.Delete(localAppData, true);
                Console.WriteLine($"Deleted: {localAppData}");
            }
            catch (Exception ex)
            {
                errors.Add($"Cache: {ex.Message}");
            }
        }

        Console.WriteLine();
        if (errors.Count > 0)
        {
            Console.Error.WriteLine("Some data could not be deleted:");
            foreach (var error in errors)
            {
                Console.Error.WriteLine($"  • {error}");
            }
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("All application data has been reset.");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .WithDeveloperTools()
            .LogToTrace();
}
