using Microsoft.Win32;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// Reads installed keyboard layouts from the Windows registry.
/// Key: HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layouts
/// Each subkey name is an 8-char hex KLID; "Layout Text" value is the display name.
/// </summary>
public static class RegistryLayoutReader
{
    private const string KeyboardLayoutsPath =
        @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";

    /// <summary>
    /// Returns all keyboard layouts installed on this machine as a dictionary
    /// mapping KLID (8-char uppercase hex, e.g. "00000409") to display name
    /// (e.g. "US" or "Portuguese (Brazil ABNT)").
    /// Never throws — returns empty dictionary on any failure.
    /// </summary>
    public static Dictionary<string, string> ReadAll()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(KeyboardLayoutsPath, writable: false);
            if (baseKey is null)
                return result;

            foreach (var subKeyName in baseKey.GetSubKeyNames())
            {
                // Subkey names are 8-char hex KLIDs (e.g. "00000409", "00000416")
                if (subKeyName.Length != 8)
                    continue;

                using var subKey = baseKey.OpenSubKey(subKeyName, writable: false);
                if (subKey is null)
                    continue;

                var displayName = subKey.GetValue("Layout Text") as string;
                if (string.IsNullOrWhiteSpace(displayName))
                    continue;

                // Normalize KLID to uppercase for consistent keying
                result[subKeyName.ToUpperInvariant()] = displayName;
            }
        }
        catch
        {
            // Registry access can fail in constrained environments — return partial results
        }

        return result;
    }

    /// <summary>
    /// Looks up the display name for a single KLID.
    /// Returns null if not found.
    /// </summary>
    public static string? GetDisplayName(string klid)
    {
        if (string.IsNullOrWhiteSpace(klid) || klid.Length != 8)
            return null;

        try
        {
            var subKeyPath = $@"{KeyboardLayoutsPath}\{klid}";
            using var key = Registry.LocalMachine.OpenSubKey(subKeyPath, writable: false);
            return key?.GetValue("Layout Text") as string;
        }
        catch
        {
            return null;
        }
    }
}
