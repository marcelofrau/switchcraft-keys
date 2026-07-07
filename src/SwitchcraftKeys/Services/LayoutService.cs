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
    private readonly Func<IntPtr> _getForegroundWindow;
    private readonly Func<IntPtr, uint> _getWindowThreadId;
    private readonly Func<IntPtr, IntPtr, bool> _requestInputLanguageChange;

    public LayoutService(ILogger<LayoutService> logger)
        : this(
            logger,
            RegistryLayoutReader.ReadAll,
            KeyboardLayoutApi.GetAllLayoutHandles,
            KeyboardLayoutApi.GetKeyboardLayout,
            KeyboardLayoutApi.LoadKeyboardLayout,
            KeyboardLayoutApi.ActivateKeyboardLayout,
            KeyboardLayoutApi.GetForegroundWindow,
            KeyboardLayoutApi.GetWindowThreadId,
            KeyboardLayoutApi.RequestInputLanguageChange)
    {
    }

    public LayoutService(
        ILogger<LayoutService> logger,
        Func<Dictionary<string, string>> readLayouts,
        Func<IntPtr[]> getAllLayoutHandles,
        Func<uint, IntPtr> getKeyboardLayout,
        Func<string, uint, IntPtr> loadKeyboardLayout,
        Func<IntPtr, uint, IntPtr> activateKeyboardLayout,
        Func<IntPtr>? getForegroundWindow = null,
        Func<IntPtr, uint>? getWindowThreadId = null,
        Func<IntPtr, IntPtr, bool>? requestInputLanguageChange = null)
    {
        _logger = logger;
        _readLayouts = readLayouts;
        _getAllLayoutHandles = getAllLayoutHandles;
        _getKeyboardLayout = getKeyboardLayout;
        _loadKeyboardLayout = loadKeyboardLayout;
        _activateKeyboardLayout = activateKeyboardLayout;
        _getForegroundWindow = getForegroundWindow ?? (() => IntPtr.Zero);
        _getWindowThreadId = getWindowThreadId ?? (_ => 0);
        _requestInputLanguageChange = requestInputLanguageChange ?? ((_, _) => true);
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

        return loadedByKlid
            .Select(pair => new LayoutInfo
            {
                Klid = pair.Key.ToUpperInvariant(),
                DisplayName = ResolveDisplayName(pair.Key, registryLayouts),
                LanguageTag = TryGetLanguageTag(pair.Key),
                Hkl = pair.Value,
            })
            .OrderBy(layout => layout.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(layout => layout.Klid, StringComparer.OrdinalIgnoreCase)
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

        _logger.LogInformation("Calling GetKeyboardLayoutList for switch klid={Klid}", klid);
        var loadedHandles = _getAllLayoutHandles();
        _logger.LogInformation("GetKeyboardLayoutList for switch returned count={Count}", loadedHandles.Length);

        var hkl = loadedHandles.FirstOrDefault(handle => string.Equals(KeyboardLayoutApi.HklToKlid(handle), klid, StringComparison.OrdinalIgnoreCase));

        if (hkl == IntPtr.Zero)
        {
            _logger.LogInformation("Calling LoadKeyboardLayout klid={Klid}", klid);
            hkl = _loadKeyboardLayout(klid, NativeConstants.KLF_SETFORPROCESS | NativeConstants.KLF_REORDER);
            _logger.LogInformation("LoadKeyboardLayout returned klid={Klid} hkl={Hkl}", klid, hkl);
        }
        else
        {
            _logger.LogInformation("Using already-loaded keyboard layout klid={Klid} hkl={Hkl}", klid, hkl);
        }

        if (hkl == IntPtr.Zero)
        {
            _logger.LogError("LoadKeyboardLayout failed klid={Klid}", klid);
            return false;
        }

        var expectedKlid = KeyboardLayoutApi.HklToKlid(hkl);
        var foregroundWindow = _getForegroundWindow();
        var foregroundThreadId = _getWindowThreadId(foregroundWindow);
        _logger.LogInformation("Foreground window resolved hwnd={Hwnd} threadId={ThreadId}", foregroundWindow, foregroundThreadId);

        _logger.LogInformation("Calling ActivateKeyboardLayout klid={Klid} expectedKlid={ExpectedKlid} hkl={Hkl}", klid, expectedKlid, hkl);
        var previousHkl = _activateKeyboardLayout(hkl, NativeConstants.KLF_SETFORPROCESS | NativeConstants.KLF_REORDER);
        _logger.LogInformation("ActivateKeyboardLayout returned previousHkl={PreviousHkl}", previousHkl);
        if (previousHkl == IntPtr.Zero)
        {
            _logger.LogError("ActivateKeyboardLayout failed klid={Klid} hkl={Hkl}", klid, hkl);
            return false;
        }

        if (foregroundWindow != IntPtr.Zero)
        {
            _logger.LogInformation("Posting WM_INPUTLANGCHANGEREQUEST hwnd={Hwnd} threadId={ThreadId} hkl={Hkl}", foregroundWindow, foregroundThreadId, hkl);
            var posted = _requestInputLanguageChange(foregroundWindow, hkl);
            _logger.LogInformation("WM_INPUTLANGCHANGEREQUEST posted={Posted} hwnd={Hwnd} hkl={Hkl}", posted, foregroundWindow, hkl);
            if (!posted)
            {
                _logger.LogError("Failed to post WM_INPUTLANGCHANGEREQUEST hwnd={Hwnd} hkl={Hkl}", foregroundWindow, hkl);
                return false;
            }
        }

        for (var attempt = 1; attempt <= MaxVerifyAttempts; attempt++)
        {
            _logger.LogDebug("Layout verify attempt={Attempt} of={Max} klid={Klid} expectedKlid={ExpectedKlid}", attempt, MaxVerifyAttempts, klid, expectedKlid);
            var current = _getKeyboardLayout(foregroundThreadId);
            var currentKlid = current == IntPtr.Zero ? null : KeyboardLayoutApi.HklToKlid(current);
            _logger.LogDebug("Layout verify result attempt={Attempt} expected={ExpectedKlid} actual={ActualKlid}", attempt, expectedKlid, currentKlid);

            if (string.Equals(currentKlid, expectedKlid, StringComparison.OrdinalIgnoreCase))
            {
                stopwatch.Stop();
                _logger.LogInformation("Layout switched klid={Klid} expectedKlid={ExpectedKlid} elapsedMs={ElapsedMs}", klid, expectedKlid, stopwatch.ElapsedMilliseconds);
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

    private static string ResolveDisplayName(string klid, Dictionary<string, string> registryLayouts)
    {
        if (registryLayouts.TryGetValue(klid, out var displayName))
        {
            return displayName;
        }

        var baseKlid = klid.Length == 8 ? "0000" + klid[4..] : klid;
        if (registryLayouts.TryGetValue(baseKlid, out displayName))
        {
            return displayName;
        }

        if (klid.Length == 8
            && int.TryParse(klid[4..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var languageId))
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(languageId);
                return culture.DisplayName;
            }
            catch (CultureNotFoundException)
            {
                return klid.ToUpperInvariant();
            }
        }

        return klid.ToUpperInvariant();
    }
}
