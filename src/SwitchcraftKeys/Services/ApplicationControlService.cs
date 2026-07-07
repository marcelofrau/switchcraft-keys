using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.Services;

public sealed class ApplicationControlService : IApplicationControlService
{
    private readonly ILogger<ApplicationControlService> _logger;

    public ApplicationControlService(ILogger<ApplicationControlService> logger)
    {
        _logger = logger;
    }

    public string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SwitchcraftKeys");

    public string CacheDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SwitchcraftKeys",
        "cache");

    public void OpenSettingsDirectory()
    {
        OpenDirectory(SettingsDirectory, nameof(OpenSettingsDirectory));
    }

    public void OpenCacheDirectory()
    {
        OpenDirectory(CacheDirectory, nameof(OpenCacheDirectory));
    }

    public void OpenWindowsKeyboardSettings()
    {
        try
        {
            const string settingsUri = "ms-settings:regionlanguage";
            _logger.LogInformation("Opening Windows keyboard settings uri={Uri}", settingsUri);
            Process.Start(new ProcessStartInfo
            {
                FileName = settingsUri,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open Windows keyboard settings");
        }
    }

    public void RestartApplication()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            _logger.LogError("Restart unavailable because process path is empty");
            return;
        }

        var escapedProcessPath = processPath.Replace("\"", "\\\"", StringComparison.Ordinal);
        _logger.LogInformation("Restarting application processPath={ProcessPath}", processPath);
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c timeout /t 1 /nobreak >nul & start \"\" \"{escapedProcessPath}\"",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
        });

        Environment.Exit(0);
    }

    private void OpenDirectory(string path, string operation)
    {
        try
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Opening directory operation={Operation} path={Path}", operation, path);
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open directory operation={Operation} path={Path}", operation, path);
        }
    }
}
