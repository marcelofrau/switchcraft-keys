using System.Text.Json.Serialization;

namespace SwitchcraftKeys.Models;

/// <summary>
/// Represents a physical keyboard device detected via Raw Input.
/// DeviceId is stable across reboots: "VID_XXXX&amp;PID_XXXX" for USB, "BUILTIN" for integrated.
/// </summary>
public sealed class DeviceInfo
{
    /// <summary>
    /// Normalized device identifier.
    /// USB devices: "VID_046D&amp;PID_C31C" (uppercase hex).
    /// Built-in ACPI/I8042 keyboards: "BUILTIN".
    /// </summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>
    /// User-defined friendly name for this device.
    /// Defaults to DeviceId until the user renames it.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// KLID of the keyboard layout assigned to this device.
    /// 8-char uppercase hex string (e.g. "00000409" for US English, "00000416" for PT-BR).
    /// Null means no layout assigned — do not switch when this device is active.
    /// </summary>
    public string? AssignedLayoutKlid { get; set; }

    /// <summary>
    /// Raw device path as returned by GetRawInputDeviceInfo(RIDI_DEVICENAME).
    /// Not persisted — populated at runtime for diagnostics only.
    /// </summary>
    [JsonIgnore]
    public string? RawPath { get; init; }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Alias) || Alias == DeviceId
            ? DeviceId
            : $"{Alias} ({DeviceId})";
}
