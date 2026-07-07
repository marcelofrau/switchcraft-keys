using System.Runtime.InteropServices;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// Defines a raw input device to register for WM_INPUT messages.
/// Passed to RegisterRawInputDevices.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTDEVICE
{
    /// <summary>Top-level collection usage page (0x01 = Generic Desktop).</summary>
    public ushort usUsagePage;

    /// <summary>Top-level collection usage (0x06 = Keyboard).</summary>
    public ushort usUsage;

    /// <summary>Mode flags (e.g. RIDEV_INPUTSINK).</summary>
    public uint dwFlags;

    /// <summary>Target window handle. Must be non-null when RIDEV_INPUTSINK is set.</summary>
    public IntPtr hwndTarget;
}

/// <summary>
/// Header common to all raw input packets.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTHEADER
{
    /// <summary>Device type: RIM_TYPEMOUSE=0, RIM_TYPEKEYBOARD=1, RIM_TYPEHID=2.</summary>
    public uint dwType;

    /// <summary>Size of the entire RAWINPUT structure in bytes.</summary>
    public uint dwSize;

    /// <summary>Handle to the device that generated the input.</summary>
    public IntPtr hDevice;

    /// <summary>wParam from the WM_INPUT message.</summary>
    public IntPtr wParam;
}

/// <summary>
/// Raw keyboard data contained in a WM_INPUT keyboard message.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWKEYBOARD
{
    /// <summary>Scan code from the key depression.</summary>
    public ushort MakeCode;

    /// <summary>Flags for scan code information (key-make, key-break, E0, E1).</summary>
    public ushort Flags;

    /// <summary>Reserved; must be zero.</summary>
    public ushort Reserved;

    /// <summary>Virtual-key code.</summary>
    public ushort VKey;

    /// <summary>Corresponding window message (WM_KEYDOWN, WM_KEYUP, etc.).</summary>
    public uint Message;

    /// <summary>Device-specific additional information.</summary>
    public uint ExtraInformation;
}

/// <summary>
/// Union data portion of RAWINPUT. Only the keyboard field is used here.
/// Sized to accommodate mouse (48 bytes) and HID data as well.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct RAWINPUTDATA
{
    [FieldOffset(0)]
    public RAWKEYBOARD keyboard;
}

/// <summary>
/// Complete raw input packet as returned by GetRawInputData.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUT
{
    public RAWINPUTHEADER header;
    public RAWINPUTDATA data;
}

/// <summary>
/// Entry returned by GetRawInputDeviceList — one per registered raw input device.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTDEVICELIST
{
    /// <summary>Handle to the raw input device.</summary>
    public IntPtr hDevice;

    /// <summary>Device type: RIM_TYPEMOUSE=0, RIM_TYPEKEYBOARD=1, RIM_TYPEHID=2.</summary>
    public uint dwType;
}

/// <summary>
/// Keyboard-specific device information returned by GetRawInputDeviceInfo(RIDI_DEVICEINFO).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RID_DEVICE_INFO_KEYBOARD
{
    public uint dwType;
    public uint dwSubType;
    public uint dwKeyboardMode;
    public uint dwNumberOfFunctionKeys;
    public uint dwNumberOfIndicators;
    public uint dwNumberOfKeysTotal;
}

/// <summary>
/// HID-specific device information returned by GetRawInputDeviceInfo(RIDI_DEVICEINFO).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RID_DEVICE_INFO_HID
{
    public uint dwVendorId;
    public uint dwProductId;
    public uint dwVersionNumber;
    public ushort usUsagePage;
    public ushort usUsage;
}

/// <summary>
/// Union for device info. cbSize must be set before calling GetRawInputDeviceInfo.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct RID_DEVICE_INFO
{
    [FieldOffset(0)]
    public uint cbSize;

    [FieldOffset(4)]
    public uint dwType;

    [FieldOffset(8)]
    public RID_DEVICE_INFO_KEYBOARD keyboard;

    [FieldOffset(8)]
    public RID_DEVICE_INFO_HID hid;
}
