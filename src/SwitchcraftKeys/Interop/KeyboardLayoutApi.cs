using System.Runtime.InteropServices;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// P/Invoke declarations for the Keyboard Layout API (user32.dll).
/// All methods are internal — only call from Services via interfaces.
/// </summary>
internal static class KeyboardLayoutApi
{
    // -------------------------------------------------------------------------
    // GetKeyboardLayoutList
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves the handles (HKLs) for all keyboard layouts currently loaded
    /// in the system. Call twice: first with 0/null to get count, then with buffer.
    /// </summary>
    /// <param name="nBuff">Size of the phkl array. Pass 0 to query count.</param>
    /// <param name="lpList">Buffer to receive HKL handles; pass null to query count.</param>
    /// <returns>
    /// If lpList is null or nBuff is 0: total number of layouts loaded.
    /// Otherwise: number of HKLs copied into lpList.
    /// </returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[]? lpList);

    // -------------------------------------------------------------------------
    // GetKeyboardLayout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the active input locale identifier (HKL) for the specified thread.
    /// Pass 0 for the current thread.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetKeyboardLayout(uint idThread);

    // -------------------------------------------------------------------------
    // LoadKeyboardLayout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads a keyboard layout from a KLID string and returns its HKL handle.
    /// The KLID must be an 8-character hex string (e.g. "00000409").
    /// Returns IntPtr.Zero on failure.
    /// </summary>
    /// <param name="pwszKLID">8-char hex KLID string (e.g. "00000409").</param>
    /// <param name="Flags">Load flags (KLF_SETFORPROCESS, KLF_REORDER, etc.).</param>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    // -------------------------------------------------------------------------
    // ActivateKeyboardLayout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets the input locale identifier (active keyboard layout) for the calling thread
    /// or, if KLF_SETFORPROCESS is specified, for the entire process.
    /// Returns the previous HKL, or IntPtr.Zero on failure.
    /// </summary>
    /// <param name="hkl">HKL returned by LoadKeyboardLayout or GetKeyboardLayoutList.</param>
    /// <param name="Flags">KLF_SETFORPROCESS to apply process-wide.</param>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint Flags);

    // -------------------------------------------------------------------------
    // UnloadKeyboardLayout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unloads a keyboard layout. Only call for layouts loaded via LoadKeyboardLayout.
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnloadKeyboardLayout(IntPtr hkl);

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all HKL handles currently loaded in the system.
    /// </summary>
    public static IntPtr[] GetAllLayoutHandles()
    {
        int count = GetKeyboardLayoutList(0, null);
        if (count <= 0)
            return [];

        var handles = new IntPtr[count];
        GetKeyboardLayoutList(count, handles);
        return handles;
    }

    /// <summary>
    /// Extracts the KLID (8-char hex) from an HKL handle.
    /// HKL encoding: low 16 bits = language ID, high 16 bits = layout ID.
    /// Returns the 8-char hex string representation of the full HKL value,
    /// which matches the subkey name under HKLM\...\Keyboard Layouts.
    /// </summary>
    public static string HklToKlid(IntPtr hkl)
    {
        // HKL is a 32-bit value stored as IntPtr.
        // The KLID used in the registry is the full 32-bit value as 8 hex chars.
        uint raw = (uint)(hkl.ToInt64() & 0xFFFFFFFF);
        return raw.ToString("X8");
    }
}
