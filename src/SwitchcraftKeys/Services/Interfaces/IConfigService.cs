using SwitchcraftKeys.Models;

namespace SwitchcraftKeys.Services.Interfaces;

/// <summary>
/// Loads and persists <see cref="AppConfig"/> to disk, with automatic backup
/// rotation and corruption recovery.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Loads the config from disk. Falls back to <c>.bak1</c> → <c>.bak2</c> →
    /// <c>.bak3</c> if the primary file is missing or fails to parse, and
    /// returns a fresh default <see cref="AppConfig"/> if none of them are
    /// readable.
    /// </summary>
    AppConfig Load();

    /// <summary>
    /// Rotates existing backups (<c>.bak2</c>→<c>.bak3</c>, <c>.bak1</c>→<c>.bak2</c>,
    /// active→<c>.bak1</c>) and writes <paramref name="config"/> as the new
    /// active config file.
    /// </summary>
    void Save(AppConfig config);

    string? GetMappedLayoutKlid(string deviceId);

    void AssignLayout(string deviceId, string klid);

    void SetDeviceAlias(string deviceId, string alias);

    void EnsureDeviceExists(string deviceId);
}
