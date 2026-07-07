using System.Runtime.InteropServices;

namespace SwitchcraftKeys.Interop;

/// <summary>
/// P/Invoke declarations for the Raw Input API (user32.dll).
/// All methods are internal and unsafe — only call from Services via interfaces.
/// </summary>
internal static unsafe class RawInputApi
{
    // -------------------------------------------------------------------------
    // RegisterRawInputDevices
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers devices that supply raw input data.
    /// Call once at startup with RIDEV_INPUTSINK to receive WM_INPUT even without focus.
    /// </summary>
    /// <param name="pRawInputDevices">Array of RAWINPUTDEVICE structures.</param>
    /// <param name="uiNumDevices">Number of elements in the array.</param>
    /// <param name="cbSize">Size of each RAWINPUTDEVICE structure.</param>
    /// <returns>true on success; false on failure (call Marshal.GetLastWin32Error).</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterRawInputDevices(
        [In] RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

    // -------------------------------------------------------------------------
    // GetRawInputData
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves the raw input from the specified device.
    /// Call from WM_INPUT handler — lParam is the HRAWINPUT handle.
    /// </summary>
    /// <param name="hRawInput">Handle to the RAWINPUT structure (lParam of WM_INPUT).</param>
    /// <param name="uiCommand">RID_INPUT or RID_HEADER.</param>
    /// <param name="pData">Buffer to receive the data; pass null to query required size.</param>
    /// <param name="pcbSize">On input: size of pData buffer. On output: bytes written.</param>
    /// <param name="cbSizeHeader">Size of RAWINPUTHEADER.</param>
    /// <returns>
    /// If pData is null: 0 on success, pcbSize contains required size.
    /// If pData is non-null: bytes copied on success, uint.MaxValue on error.
    /// </returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetRawInputData(
        IntPtr hRawInput,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize,
        uint cbSizeHeader);

    // -------------------------------------------------------------------------
    // GetRawInputDeviceInfo
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves information about the raw input device.
    /// Used with RIDI_DEVICENAME to get the device path string.
    /// </summary>
    /// <param name="hDevice">Handle to the raw input device (from RAWINPUTHEADER.hDevice).</param>
    /// <param name="uiCommand">RIDI_DEVICENAME or RIDI_DEVICEINFO.</param>
    /// <param name="pData">Buffer to receive data; pass null to query size.</param>
    /// <param name="pcbSize">On input: size of buffer. On output: bytes written (or required).</param>
    /// <returns>
    /// If pData is null: 0 on success, pcbSize has required size.
    /// Otherwise: bytes written, or uint.MaxValue on error.
    /// </returns>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize);

    // -------------------------------------------------------------------------
    // GetRawInputDeviceList
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enumerates all raw input devices attached to the system.
    /// Call twice: first with null to get count, then with allocated buffer to fill.
    /// </summary>
    /// <param name="pRawInputDeviceList">
    /// Buffer to receive RAWINPUTDEVICELIST entries; pass null to query count.
    /// </param>
    /// <param name="puiNumDevices">
    /// On first call (null buffer): receives device count.
    /// On second call: must be the count returned by first call.
    /// </param>
    /// <param name="cbSize">Size of one RAWINPUTDEVICELIST structure.</param>
    /// <returns>
    /// If pRawInputDeviceList is null: 0, puiNumDevices has count.
    /// Otherwise: number of devices written, or uint.MaxValue on error.
    /// </returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetRawInputDeviceList(
        [Out] RAWINPUTDEVICELIST[]? pRawInputDeviceList,
        ref uint puiNumDevices,
        uint cbSize);

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads the device name string for a given hDevice handle.
    /// Returns null if the call fails.
    /// </summary>
    public static string? GetDeviceName(IntPtr hDevice)
    {
        uint size = 0;
        GetRawInputDeviceInfo(hDevice, NativeConstants.RIDI_DEVICENAME, IntPtr.Zero, ref size);
        if (size == 0)
            return null;

        var buffer = Marshal.AllocHGlobal((int)(size * 2)); // wide chars
        try
        {
            uint result = GetRawInputDeviceInfo(hDevice, NativeConstants.RIDI_DEVICENAME, buffer, ref size);
            if (result == uint.MaxValue)
                return null;

            return Marshal.PtrToStringUni(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>
    /// Returns all keyboard devices currently attached to the system.
    /// Filters to RIM_TYPEKEYBOARD only.
    /// </summary>
    public static RAWINPUTDEVICELIST[] GetKeyboardDeviceList()
    {
        uint count = 0;
        uint structSize = (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>();

        GetRawInputDeviceList(null, ref count, structSize);
        if (count == 0)
            return [];

        var all = new RAWINPUTDEVICELIST[count];
        uint result = GetRawInputDeviceList(all, ref count, structSize);
        if (result == uint.MaxValue)
            return [];

        return all
            .Where(d => d.dwType == NativeConstants.RIM_TYPEKEYBOARD)
            .ToArray();
    }
}
