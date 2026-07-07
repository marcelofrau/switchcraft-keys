using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SwitchcraftKeys.Interop;
using SwitchcraftKeys.Models;
using SwitchcraftKeys.Services.Interfaces;

namespace SwitchcraftKeys.Services;

public sealed class LayoutService : ILayoutService
{
    private const int MaxVerifyAttempts = 3;
    private static readonly TimeSpan VerifyDelay = TimeSpan.FromMilliseconds(100);

    private readonly ILogger<LayoutService> _logger;
    private readonly Func<Dictionary<string, string>> _readLayouts;
    private readonly Func<IntPtr[]> _getAllLayoutHandles;
    private readonly Func<uint, IntPtr> _getKeyboardLayout;
    private readonly Func<string, uint, IntPtr> _loadKeyboardLayout;
    private readonly Func<IntPtr, uint, IntPtr> _activateKeyboardLayout;

    public LayoutService(ILogger<LayoutService> logger)
        : this(
            logger,
            RegistryLayoutReader.ReadAll,
            KeyboardLayoutApi.GetAllLayoutHandles,
            KeyboardLayoutApi.GetKeyboardLayout,
            KeyboardLayoutApi.LoadKeyboardLayout,
            KeyboardLayoutApi.ActivateKeyboardLayout)
    {
    }

    public LayoutService(
        ILogger<LayoutService> logger,
        Func<Dictionary<string, string>> readLayouts,
        Func<IntPtr[]> getAllLayoutHandles,
        Func<uint, IntPtr> getKeyboardLayout,
        Func<string, uint, IntPtr> loadKeyboardLayout,
        Func<IntPtr, uint, IntPtr> activateKeyboardLayout)
    {
        _logger = logger;
        _readLayouts = readLayouts;
        _getAllLayoutHandles = getAllLayoutHandles;
        _getKeyboardLayout = getKeyboardLayout;
        _loadKeyboardLayout = loadKeyboardLayout;
        _activateKeyboardLayout = activateKeyboardLayout;
    }

    public IReadOnlyList<LayoutInfo> GetAvailableLayouts()
    {
        _logger.LogInformation("Calling registry keyboard layout enumeration");
        var registryLayouts = _readLayouts();
        _logger.LogInformation("Registry keyboard layout enumeration returned count={Count}", registryLayouts.Count);

        _logger.LogInformation("Calling GetKeyboardLayoutList");
        var loadedHandles = _getAllLayoutHandles();
        _logger.LogInformation("GetKeyboardLayoutList returned count={Count}", loadedHandles.Length);
        var loadedByKlid = loadedHandles.ToDictionary(KeyboardLayoutApi.HklToKlid, hkl => hkl, StringComparer.OrdinalIgnoreCase);

        return registryLayouts
            .OrderBy(pair => pair.Value, StringComparer.CurrentCultureIgnoreCase)
            .Select(pair => new LayoutInfo
            {
                Klid = pair.Key.ToUpperInvariant(),
                DisplayName = pair.Value,
                LanguageTag = TryGetLanguageTag(pair.Key),
                Hkl = loadedByKlid.TryGetValue(pair.Key, out var hkl) ? hkl : IntPtr.Zero,
            })
            .ToArray();
    }

    public LayoutInfo? GetCurrentLayout()
    {
        _logger.LogInformation("Calling GetKeyboardLayout threadId={ThreadId}", 0u);
        var hkl = _getKeyboardLayout(0);
        _logger.LogInformation("GetKeyboardLayout returned hkl={Hkl}", hkl);

        if (hkl == IntPtr.Zero)
        {
            return null;
        }

        var klid = KeyboardLayoutApi.HklToKlid(hkl);
        var displayName = RegistryLayoutReader.GetDisplayName(klid) ?? klid;
        return new LayoutInfo
        {
            Klid = klid,
            DisplayName = displayName,
            LanguageTag = TryGetLanguageTag(klid),
            Hkl = hkl,
        };
    }

    public async Task<bool> SwitchLayoutAsync(string klid)
    {
        if (string.IsNullOrWhiteSpace(klid) || klid.Length != 8)
        {
            _logger.LogError("Invalid layout KLID klid={Klid}", klid);
            return false;
        }

        klid = klid.ToUpperInvariant();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Calling LoadKeyboardLayout klid={Klid}", klid);
        var hkl = _loadKeyboardLayout(klid, NativeConstants.KLF_SETFORPROCESS | NativeConstants.KLF_REORDER);
        _logger.LogInformation("LoadKeyboardLayout returned klid={Klid} hkl={Hkl}", klid, hkl);
        if (hkl == IntPtr.Zero)
        {
            _logger.LogError("LoadKeyboardLayout failed klid={Klid}", klid);
            return false;
        }

        _logger.LogInformation("Calling ActivateKeyboardLayout klid={Klid} hkl={Hkl}", klid, hkl);
        var previousHkl = _activateKeyboardLayout(hkl, NativeConstants.KLF_SETFORPROCESS | NativeConstants.KLF_REORDER);
        _logger.LogInformation("ActivateKeyboardLayout returned previousHkl={PreviousHkl}", previousHkl);
        if (previousHkl == IntPtr.Zero)
        {
            _logger.LogError("ActivateKeyboardLayout failed klid={Klid} hkl={Hkl}", klid, hkl);
            return false;
        }

        for (var attempt = 1; attempt <= MaxVerifyAttempts; attempt++)
        {
            _logger.LogDebug("Layout verify attempt={Attempt} of={Max} klid={Klid}", attempt, MaxVerifyAttempts, klid);
            var current = _getKeyboardLayout(0);
            var currentKlid = current == IntPtr.Zero ? null : KeyboardLayoutApi.HklToKlid(current);
            _logger.LogDebug("Layout verify result attempt={Attempt} expected={ExpectedKlid} actual={ActualKlid}", attempt, klid, currentKlid);

            if (string.Equals(currentKlid, klid, StringComparison.OrdinalIgnoreCase))
            {
                stopwatch.Stop();
                _logger.LogInformation("Layout switched klid={Klid} elapsedMs={ElapsedMs}", klid, stopwatch.ElapsedMilliseconds);
                return true;
            }

            await Task.Delay(VerifyDelay).ConfigureAwait(false);
        }

        stopwatch.Stop();
        _logger.LogWarning("Layout switch unverified after retries klid={Klid} elapsed={ElapsedMs}ms", klid, stopwatch.ElapsedMilliseconds);
        return false;
    }

    private static string? TryGetLanguageTag(string klid)
    {
        if (klid.Length != 8 || !int.TryParse(klid[4..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var languageId))
        {
            return null;
        }

        try
        {
            return CultureInfo.GetCultureInfo(languageId).Name;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
