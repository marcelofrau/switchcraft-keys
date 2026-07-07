namespace SwitchcraftKeys.Interop;

/// <summary>
/// Win32 constants used by the Raw Input and Keyboard Layout APIs.
/// </summary>
internal static class NativeConstants
{
    // -------------------------------------------------------------------------
    // Raw Input — RegisterRawInputDevices flags
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enables the caller to receive input in the background (even when not focused).
    /// hwndTarget must be a valid window handle when this flag is set.
    /// </summary>
    public const uint RIDEV_INPUTSINK = 0x00000100;

    /// <summary>
    /// If set, the device is removed from the registration list.
    /// </summary>
    public const uint RIDEV_REMOVE = 0x00000001;

    // -------------------------------------------------------------------------
    // Raw Input — device types (RAWINPUTHEADER.dwType / RAWINPUTDEVICELIST.dwType)
    // -------------------------------------------------------------------------

    public const uint RIM_TYPEMOUSE    = 0;
    public const uint RIM_TYPEKEYBOARD = 1;
    public const uint RIM_TYPEHID      = 2;

    // -------------------------------------------------------------------------
    // Raw Input — GetRawInputDeviceInfo command codes
    // -------------------------------------------------------------------------

    /// <summary>Returns the device name as a null-terminated wide string.</summary>
    public const uint RIDI_DEVICENAME = 0x20000007;

    /// <summary>Returns a RID_DEVICE_INFO structure.</summary>
    public const uint RIDI_DEVICEINFO = 0x2000000B;

    // -------------------------------------------------------------------------
    // Raw Input — GetRawInputData command codes
    // -------------------------------------------------------------------------

    /// <summary>Get the raw data from the RAWINPUT structure.</summary>
    public const uint RID_INPUT  = 0x10000003;

    /// <summary>Get the header information from the RAWINPUT structure.</summary>
    public const uint RID_HEADER = 0x10000005;

    // -------------------------------------------------------------------------
    // Window messages
    // -------------------------------------------------------------------------

    /// <summary>Posted to the window with keyboard/mouse/HID raw input.</summary>
    public const uint WM_INPUT = 0x00FF;

    /// <summary>Sent when the input language changes (pre-notification).</summary>
    public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;

    /// <summary>Sent after the input language has changed.</summary>
    public const uint WM_INPUTLANGCHANGE = 0x0051;

    // -------------------------------------------------------------------------
    // HID Usage Page / Usage for keyboards
    // -------------------------------------------------------------------------

    /// <summary>Generic Desktop Controls usage page.</summary>
    public const ushort HID_USAGE_PAGE_GENERIC = 0x01;

    /// <summary>Keyboard usage within the Generic Desktop page.</summary>
    public const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

    // -------------------------------------------------------------------------
    // Keyboard Layout flags
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates the layout for the entire process (all threads),
    /// not just the calling thread.
    /// </summary>
    public const uint KLF_SETFORPROCESS = 0x00000100;

    /// <summary>
    /// If the layout has been replaced, activates the new layout.
    /// </summary>
    public const uint KLF_REORDER = 0x00000008;
}
