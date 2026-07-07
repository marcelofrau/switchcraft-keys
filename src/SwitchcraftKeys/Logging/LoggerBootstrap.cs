using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using SwitchcraftKeys.Interop;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.Logging;

/// <summary>
/// Composition-root helper that wires up Serilog (Console + rotating File
/// sinks) from the persisted <see cref="AppConfig"/> and exposes a
/// Microsoft.Extensions.Logging-compatible <see cref="ILoggerFactory"/> so
/// Services/ViewModels only ever depend on <c>ILogger&lt;T&gt;</c>, never on
/// Serilog types directly.
///
/// This is bootstrap/infrastructure code, not a business "Service" — it lives
/// outside <c>Services/</c> on purpose and is only ever touched from the
/// composition root (<c>Program.cs</c> / <c>App.axaml.cs</c>).
/// </summary>
public sealed class LoggerBootstrap : IDisposable
{
    private const int MaxRetainedLogFiles = 5;

    private readonly IConfigService _configService;
    private readonly ApplicationLogService _applicationLogService;
    private readonly string _logDirectory;
    private readonly LoggingLevelSwitch _levelSwitch = new();

    private AppConfig _config = new();

    public LoggerBootstrap(IConfigService configService, ApplicationLogService? applicationLogService = null, string? logDirectory = null)
    {
        _configService = configService;
        _applicationLogService = applicationLogService ?? new ApplicationLogService();
        _logDirectory = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SwitchcraftKeys",
            "logs");
    }

    /// <summary>
    /// Loads config, sets up the global Serilog logger (Console sink only if
    /// a console is already attached — never allocates one — plus a File
    /// sink that keeps the last <see cref="MaxRetainedLogFiles"/> runs), and
    /// returns an <see cref="ILoggerFactory"/> bridged via Serilog.Extensions.Logging.
    /// </summary>
    public ILoggerFactory Configure()
    {
        _config = _configService.Load();
        _levelSwitch.MinimumLevel = ParseLevel(_config.Logging.MinimumLevel);

        Directory.CreateDirectory(_logDirectory);
        CleanupOldLogs();

        var logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        const string outputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_levelSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Sink(new ApplicationLogSink(_applicationLogService))
            .WriteTo.File(logFilePath, outputTemplate: outputTemplate, shared: false);

        // Never allocate/attach a console — only write to one that already
        // exists (e.g. process launched from run.ps1 / an existing terminal).
        // A double-clicked WinExe launch has no console, so this stays silent.
        if (ConsoleApi.HasConsole
            || Environment.GetEnvironmentVariable("SWITCHCRAFTKEYS_FORCE_CONSOLE_LOG") == "1")
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: outputTemplate);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        return new SerilogLoggerFactory(Log.Logger, dispose: true);
    }

    /// <summary>
    /// Changes the active minimum log level at runtime (e.g. from a future
    /// Settings screen) and persists it so it survives restarts.
    /// </summary>
    public void UpdateMinimumLevel(LogLevel level)
    {
        _levelSwitch.MinimumLevel = ToSerilogLevel(level);
        _config.Logging.MinimumLevel = level.ToString();
        _configService.Save(_config);
    }

    private void CleanupOldLogs()
    {
        var oldLogs = Directory.GetFiles(_logDirectory, "log-*.txt")
            .OrderByDescending(path => path, StringComparer.Ordinal)
            .Skip(MaxRetainedLogFiles - 1); // -1: about to create one more file this run.

        foreach (var oldLog in oldLogs)
        {
            try
            {
                File.Delete(oldLog);
            }
            catch (IOException)
            {
                // Best-effort cleanup — a locked file just survives one extra run.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup.
            }
        }
    }

    private static LogEventLevel ParseLevel(string value)
        => ToSerilogLevel(Enum.TryParse<LogLevel>(value, ignoreCase: true, out var level) ? level : LogLevel.Trace);

    private static LogEventLevel ToSerilogLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => LogEventLevel.Verbose,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Critical => LogEventLevel.Fatal,
        _ => LogEventLevel.Verbose,
    };

    public void Dispose() => Log.CloseAndFlush();
}
