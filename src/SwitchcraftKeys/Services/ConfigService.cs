using System.Text.Json;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.Services;

/// <summary>
/// JSON-backed <see cref="IConfigService"/> with 3-generation backup rotation
/// and cascading recovery on corruption. Pure file I/O — no Win32 calls, so
/// it is fully unit-testable via the <paramref name="configDirectory"/> override.
/// </summary>
public sealed class ConfigService : IConfigService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _configDirectory;
    private readonly string _configPath;

    /// <summary>
    /// Creates a config service rooted at <c>%APPDATA%\SwitchcraftKeys</c>.
    /// </summary>
    public ConfigService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwitchcraftKeys"))
    {
    }

    /// <summary>
    /// Creates a config service rooted at an explicit directory. Used by tests
    /// to avoid touching the real <c>%APPDATA%</c>.
    /// </summary>
    public ConfigService(string configDirectory)
    {
        _configDirectory = configDirectory;
        _configPath = Path.Combine(_configDirectory, "config.json");
    }

    public AppConfig Load()
    {
        var candidates = new[]
        {
            _configPath,
            _configPath + ".bak1",
            _configPath + ".bak2",
            _configPath + ".bak3",
        };

        foreach (var candidate in candidates)
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            try
            {
                var json = File.ReadAllText(candidate);
                var config = JsonSerializer.Deserialize<AppConfig>(json, SerializerOptions);
                if (config is not null)
                {
                    return Normalize(config);
                }
            }
            catch (JsonException)
            {
                // Corrupted — try the next backup in the cascade.
            }
            catch (IOException)
            {
                // Unreadable — try the next backup in the cascade.
            }
        }

        return new AppConfig();
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(_configDirectory);

        var tempPath = _configPath + ".tmp";
        var json = JsonSerializer.Serialize(config, SerializerOptions);
        File.WriteAllText(tempPath, json);

        RotateBackups();
        File.Move(tempPath, _configPath);
    }

    public string? GetMappedLayoutKlid(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return null;
        }

        var config = Load();
        return config.Devices.TryGetValue(deviceId, out var device)
            ? device.AssignedLayoutKlid
            : null;
    }

    public void AssignLayout(string deviceId, string klid)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID is required.", nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(klid) || klid.Length != 8)
        {
            throw new ArgumentException("KLID must be an 8-character hex string.", nameof(klid));
        }

        var config = Load();
        EnsureDeviceExists(config, deviceId);
        config.Devices[deviceId].AssignedLayoutKlid = klid.ToUpperInvariant();
        Save(config);
    }

    public void SetDeviceAlias(string deviceId, string alias)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID is required.", nameof(deviceId));
        }

        var config = Load();
        EnsureDeviceExists(config, deviceId);
        config.Devices[deviceId].Alias = string.IsNullOrWhiteSpace(alias) ? deviceId : alias.Trim();
        Save(config);
    }

    public void EnsureDeviceExists(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Device ID is required.", nameof(deviceId));
        }

        var config = Load();
        EnsureDeviceExists(config, deviceId);
        Save(config);
    }

    private static AppConfig Normalize(AppConfig config)
    {
        config.Devices = new Dictionary<string, DeviceInfo>(config.Devices, StringComparer.OrdinalIgnoreCase);
        return config;
    }

    private static void EnsureDeviceExists(AppConfig config, string deviceId)
    {
        if (config.Devices.ContainsKey(deviceId))
        {
            return;
        }

        config.Devices[deviceId] = new DeviceInfo
        {
            DeviceId = deviceId,
            Alias = deviceId,
        };
    }

    private void RotateBackups()
    {
        var bak1 = _configPath + ".bak1";
        var bak2 = _configPath + ".bak2";
        var bak3 = _configPath + ".bak3";

        File.Delete(bak3);
        if (File.Exists(bak2))
        {
            File.Move(bak2, bak3);
        }

        if (File.Exists(bak1))
        {
            File.Move(bak1, bak2);
        }

        if (File.Exists(_configPath))
        {
            File.Move(_configPath, bak1);
        }
    }
}
