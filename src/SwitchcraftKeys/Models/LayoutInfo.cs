namespace SwitchcraftKeys.Models;

/// <summary>
/// Represents a keyboard layout installed on the system.
/// Sourced from HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layouts.
/// </summary>
public sealed class LayoutInfo
{
    /// <summary>
    /// 8-character uppercase hex KLID (e.g. "00000409", "00000416").
    /// This is the registry subkey name and the value stored in config.json.
    /// Never persist the HKL — always load it at runtime via LoadKeyboardLayout(Klid).
    /// </summary>
    public string Klid { get; init; } = string.Empty;

    /// <summary>
    /// Display name from the registry "Layout Text" value
    /// (e.g. "US", "Portuguese (Brazil ABNT)").
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Language tag derived from the low 16 bits of the KLID
    /// (e.g. "en-US", "pt-BR"). Populated at runtime, not persisted.
    /// </summary>
    public string? LanguageTag { get; init; }

    /// <summary>
    /// Runtime HKL handle returned by LoadKeyboardLayout or GetKeyboardLayoutList.
    /// Never persist this — it changes across sessions and machines.
    /// </summary>
    public IntPtr Hkl { get; set; } = IntPtr.Zero;

    public override string ToString() => $"{DisplayName} ({Klid})";
}
