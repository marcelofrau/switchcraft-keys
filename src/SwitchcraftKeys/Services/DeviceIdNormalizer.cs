using System.Text.RegularExpressions;

namespace SwitchcraftKeys.Services;

/// <summary>
/// Normalizes raw device paths from GetRawInputDeviceInfo(RIDI_DEVICENAME)
/// into stable, human-readable device IDs.
///
/// USB keyboards  → "VID_046D&amp;PID_C31C"  (uppercase hex, no instance suffix)
/// Built-in ACPI/I8042 keyboards → "BUILTIN"
///
/// The normalized ID is stable across reboots and machines with the same hardware.
/// </summary>
public static class DeviceIdNormalizer
{
    // Matches VID_XXXX&PID_XXXX anywhere in USB/HID device paths.
    private static readonly Regex UsbVidPidRegex = new(
        @"VID_([0-9A-F]{4})&PID_([0-9A-F]{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches Bluetooth HID paths such as VID&02046d_PID&b380.
    private static readonly Regex BluetoothVidPidRegex = new(
        @"VID&[0-9A-F]*([0-9A-F]{4})_PID&[0-9A-F]*([0-9A-F]{4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Normalizes a raw device path to a stable device ID string.
    ///
    /// Examples:
    ///   @"\?\HID#VID_046D&amp;PID_C31C&amp;MI_00#..." → "VID_046D&amp;PID_C31C"
    ///   @"\?\ACPI#PNP0303#..."                      → "BUILTIN"
    ///   @"\?\ROOT#RDP_KBD#..."                      → "BUILTIN"
    /// </summary>
    /// <param name="rawPath">Device path from GetRawInputDeviceInfo(RIDI_DEVICENAME).</param>
    /// <returns>Normalized device ID, or null if path cannot be recognized.</returns>
    public static string? Normalize(string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            return null;

        var upper = rawPath.ToUpperInvariant();

        // Built-in keyboard detection: ACPI, PS/2 (I8042), or RDP virtual keyboard
        if (upper.Contains("ACPI") || upper.Contains("I8042") || upper.Contains("RDP_KBD"))
            return "BUILTIN";

        // USB / HID keyboard: extract VID + PID
        var match = UsbVidPidRegex.Match(rawPath);
        if (!match.Success)
        {
            match = BluetoothVidPidRegex.Match(rawPath);
        }

        if (match.Success)
        {
            var vid = match.Groups[1].Value.ToUpperInvariant();
            var pid = match.Groups[2].Value.ToUpperInvariant();
            return $"VID_{vid}&PID_{pid}";
        }

        return null;
    }

    /// <summary>
    /// Returns true if the given device ID represents a built-in keyboard.
    /// </summary>
    public static bool IsBuiltin(string? deviceId) =>
        string.Equals(deviceId, "BUILTIN", StringComparison.OrdinalIgnoreCase);
}
