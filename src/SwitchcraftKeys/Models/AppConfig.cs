namespace SwitchcraftKeys.Models;

/// <summary>
/// Root application configuration, persisted as JSON under
/// <c>%APPDATA%\SwitchcraftKeys\config.json</c>.
/// </summary>
public sealed class AppConfig
{
    public int Version { get; set; } = 1;

    public LoggingConfig Logging { get; set; } = new();

    public Dictionary<string, DeviceInfo> Devices { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public UiSettings Ui { get; set; } = new();
}

/// <summary>
/// Logging-related settings. <see cref="MinimumLevel"/> is stored as a string so the
/// config file stays human-readable/editable, and is parsed into a
/// <see cref="Microsoft.Extensions.Logging.LogLevel"/> at startup.
/// </summary>
public sealed class LoggingConfig
{
    /// <summary>
    /// One of: Trace, Debug, Information, Warning, Error, Critical.
    /// Defaults to Trace (most verbose) so nothing is missed until the user
    /// tunes it down.
    /// </summary>
    public string MinimumLevel { get; set; } = "Trace";
}

public sealed class UiSettings
{
    public bool HudEnabled { get; set; }

    public bool StartMinimized { get; set; } = true;
}
