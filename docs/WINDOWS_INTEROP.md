# Windows Interop Reference — SwitchcraftKeys

**Scope**: All Windows API calls used in Phase 1.  
**Layer**: `src/SwitchcraftKeys/Interop/`  
**Rule**: Zero `[DllImport]` outside this layer.

> Adapted from `archive/switchcraft-keys-too/IMPLEMENTATION_STATUS.md` (Rust/windows crate)
> and `archive/switchcraft-keys-python` (ctypes). Translated to C# P/Invoke.

---

## 1. Raw Input API

### Purpose
Identify which physical keyboard generated a keystroke. This is the core mechanism
for per-device detection. Regular keyboard hooks (`WH_KEYBOARD_LL`) do NOT expose
device identity — Raw Input is required.

### Flow

```
App startup
  → RegisterRawInputDevices(RIDEV_INPUTSINK)
  → WM_INPUT arrives on message pump for every keystroke
  → GetRawInputData(lParam) → RAWINPUT struct
  → RAWINPUT.header.hDevice → device handle
  → GetRawInputDeviceInfo(hDevice, RIDI_DEVICENAME) → device path string
  → DeviceIdNormalizer.Normalize(path) → "VID_046D&PID_C31C" or "BUILTIN"
```

### P/Invoke Declarations

```csharp
// NativeStructs.cs
[StructLayout(LayoutKind.Sequential)]
public struct RAWINPUTDEVICE
{
    public ushort UsagePage;   // 0x01 = Generic Desktop
    public ushort Usage;       // 0x06 = Keyboard
    public uint   Flags;       // RIDEV_INPUTSINK = 0x00000100
    public IntPtr Target;      // HWND; null if RIDEV_NOLEGACY
}

[StructLayout(LayoutKind.Sequential)]
public struct RAWINPUTHEADER
{
    public uint   Type;        // RIM_TYPEKEYBOARD = 1
    public uint   Size;
    public IntPtr Device;      // hDevice
    public IntPtr WParam;
}

[StructLayout(LayoutKind.Sequential)]
public struct RAWKEYBOARD
{
    public ushort MakeCode;
    public ushort Flags;
    public ushort Reserved;
    public ushort VKey;
    public uint   Message;
    public uint   ExtraInformation;
}

[StructLayout(LayoutKind.Explicit)]
public struct RAWINPUT
{
    [FieldOffset(0)] public RAWINPUTHEADER header;
    [FieldOffset(24)] public RAWKEYBOARD keyboard;  // offset = sizeof(RAWINPUTHEADER) on x64
}

// RawInputApi.cs
[DllImport("user32.dll", SetLastError = true)]
public static extern bool RegisterRawInputDevices(
    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
    RAWINPUTDEVICE[] pRawInputDevices,
    int uiNumDevices,
    int cbSize);

[DllImport("user32.dll", SetLastError = true)]
public static extern uint GetRawInputData(
    IntPtr hRawInput,
    uint uiCommand,          // RID_INPUT = 0x10000003
    IntPtr pData,
    ref uint pcbSize,
    uint cbSizeHeader);

[DllImport("user32.dll", SetLastError = true)]
public static extern uint GetRawInputDeviceInfo(
    IntPtr hDevice,
    uint uiCommand,          // RIDI_DEVICENAME = 0x20000007
    IntPtr pData,
    ref uint pcbSize);

[DllImport("user32.dll", SetLastError = true)]
public static extern uint GetRawInputDeviceList(
    [Out] RAWINPUTDEVICELIST[]? pRawInputDeviceList,
    ref uint puiNumDevices,
    uint cbSize);

[StructLayout(LayoutKind.Sequential)]
public struct RAWINPUTDEVICELIST
{
    public IntPtr hDevice;
    public uint   dwType;    // RIM_TYPEKEYBOARD = 1
}
```

### Constants

```csharp
// NativeConstants.cs
public static class NativeConstants
{
    public const uint WM_INPUT           = 0x00FF;
    public const uint RIM_TYPEKEYBOARD   = 1;
    public const uint RIDEV_INPUTSINK    = 0x00000100;
    public const uint RIDI_DEVICENAME    = 0x20000007;
    public const uint RID_INPUT          = 0x10000003;
    public const uint RID_HEADER         = 0x10000005;
}
```

### Integration with Avalonia

Avalonia on Windows uses Win32 underneath. To intercept `WM_INPUT`:

```csharp
// MainWindow.axaml.cs
protected override void HandleWindowMessage(object sender, RawInputEventArgs e)
{
    // Not available directly — use Win32Interop to get HWND and subclass
}

// Better approach: use Win32Interop.GetWindowHandle
// and PlatformImpl.HandleMessage override via Avalonia's Win32 hooks
```

Recommended: get HWND after window opens, call `RegisterRawInputDevices(hwnd)`,
then override `WndProc` via `HwndSource` or platform message hook.

In Avalonia 12: use `IPlatformHandle` from `IWindowImpl`:
```csharp
var handle = (window.PlatformImpl as Avalonia.Win32.WindowImpl)?.Handle;
```

---

## 2. Keyboard Layout API

### Purpose
Enumerate installed layouts from registry and switch the active layout.

### P/Invoke Declarations

```csharp
// KeyboardLayoutApi.cs
[DllImport("user32.dll", SetLastError = true)]
public static extern IntPtr LoadKeyboardLayout(
    string pwszKLID,          // e.g., "00000409"
    uint Flags);              // KLF_ACTIVATE = 0x00000001

[DllImport("user32.dll", SetLastError = true)]
public static extern IntPtr ActivateKeyboardLayout(
    IntPtr hkl,
    uint Flags);              // KLF_SETFORPROCESS = 0x00000100

[DllImport("user32.dll")]
public static extern int GetKeyboardLayoutList(
    int nBuff,
    [Out] IntPtr[]? lpList);

[DllImport("user32.dll")]
public static extern IntPtr GetKeyboardLayout(
    uint idThread);           // 0 = current thread

[DllImport("user32.dll")]
public static extern bool PostMessage(
    IntPtr hWnd,
    uint Msg,
    IntPtr wParam,
    IntPtr lParam);

// For broadcasting layout change to all windows:
// PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, ...)
```

### Constants

```csharp
public const uint KLF_ACTIVATE      = 0x00000001;
public const uint KLF_SETFORPROCESS = 0x00000100;
public const IntPtr HWND_BROADCAST  = new IntPtr(0xFFFF);
public const uint WM_INPUTLANGCHANGE = 0x0051;
```

### Layout Switch Strategy

`ActivateKeyboardLayout` switches layout for the current thread only.
For system-wide switching (affects all windows), use:

```csharp
// Option A: SystemParametersInfo (recommended)
[DllImport("user32.dll", SetLastError = true)]
public static extern bool SystemParametersInfo(
    uint uiAction,    // SPI_SETDEFAULTINPUTLANG = 0x005A
    uint uiParam,
    ref IntPtr pvParam,
    uint fWinIni);

// Option B: PostMessage to HWND_BROADCAST
// Option C: ActivateKeyboardLayout on foreground window thread
```

**Phase 1 approach**: `ActivateKeyboardLayout` + verification. If verification fails
(layout did not change within 3 polls × 100ms), log error.

### Verification

```csharp
async Task<bool> VerifyLayoutSwitched(IntPtr expectedHkl, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var current = GetKeyboardLayout(0);
        if (current == expectedHkl) return true;
        await Task.Delay(100);
    }
    return false;
}
```

---

## 3. Registry Layout Enumeration

### Purpose
Read the list of installed keyboard layouts with their display names and language codes.

### Registry Path

```
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{KLID}
  Layout Text     REG_SZ    "United States-International"
  Layout File     REG_SZ    "KBDUSX.DLL"
  Layout Id       REG_SZ    "0409"
```

**KLID format**: 8-char hex string, e.g., `00000409` (US English), `00000416` (PT-BR)

### C# Implementation

```csharp
// RegistryLayoutReader.cs
using Microsoft.Win32;

public static IReadOnlyList<LayoutInfo> ReadInstalledLayouts()
{
    const string keyPath = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
    var results = new List<LayoutInfo>();

    using var root = Registry.LocalMachine.OpenSubKey(keyPath);
    if (root is null) return results;

    foreach (var klid in root.GetSubKeyNames())
    {
        using var sub = root.OpenSubKey(klid);
        if (sub is null) continue;

        var layoutText = sub.GetValue("Layout Text") as string ?? klid;
        var layoutId   = sub.GetValue("Layout Id")   as string ?? klid[4..]; // last 4 chars

        // Get language name from culture
        string langCode = GetLanguageCode(klid);
        string displayName = $"{langCode} - {layoutText}";

        results.Add(new LayoutInfo
        {
            Klid        = klid,
            DisplayName = displayName,
            LanguageCode = langCode,
        });
    }

    return results;
}

private static string GetLanguageCode(string klid)
{
    try
    {
        // KLID low word = LANGID
        int langId = Convert.ToInt32(klid[4..], 16); // last 4 hex chars
        var culture = CultureInfo.GetCultureInfo(langId);
        // 3-letter abbreviation, uppercase
        return culture.ThreeLetterISOLanguageName.ToUpperInvariant();
    }
    catch
    {
        return klid[4..].ToUpperInvariant();
    }
}
```

### HKL ↔ KLID Conversion

`GetKeyboardLayoutList` returns HKL handles. To convert HKL → KLID:
```csharp
// Low word of HKL = LANGID, high word = device handle or layout ID
int klid = (int)(hkl.ToInt64() & 0xFFFF); // simplified; exact mapping via GetKeyboardLayoutName
```

Use `GetKeyboardLayoutName` for exact KLID:
```csharp
[DllImport("user32.dll", SetLastError = true)]
public static extern bool GetKeyboardLayoutName(
    [Out] StringBuilder pwszKLID);  // 9-char buffer, e.g. "00000409\0"
```

---

## 4. Device Path Normalization

### Regex patterns

```csharp
// Device path examples:
// USB:     \\?\HID#VID_046D&PID_C31C&MI_00#8&1234ABCD&0&0000#{...}
// BUILTIN: \\?\ACPI#PNP0303#4&1234ABCD&0#{...}
//          \\?\Root\RDP_KBD#...
//          \\?\ACPI#PNP030B#...

private static readonly Regex VidPidRegex =
    new(@"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4})", RegexOptions.IgnoreCase);

public static string Normalize(string devicePath)
{
    // Check BUILTIN patterns first
    if (devicePath.Contains("ACPI\\PNP0303", StringComparison.OrdinalIgnoreCase) ||
        devicePath.Contains("ACPI\\PNP030B", StringComparison.OrdinalIgnoreCase) ||
        devicePath.Contains("I8042PRT",      StringComparison.OrdinalIgnoreCase) ||
        devicePath.Contains("ROOT\\RDP_KBD", StringComparison.OrdinalIgnoreCase))
    {
        return "BUILTIN";
    }

    var match = VidPidRegex.Match(devicePath);
    if (match.Success)
    {
        return $"VID_{match.Groups[1].Value.ToUpperInvariant()}&PID_{match.Groups[2].Value.ToUpperInvariant()}";
    }

    // Fallback: hash the full path for determinism
    return "UNKNOWN_" + devicePath.GetHashCode().ToString("X8");
}
```

---

## 5. Avalonia WndProc Hook

To receive `WM_INPUT` in an Avalonia 12 window on Windows:

```csharp
// MainWindow.axaml.cs
public partial class MainWindow : Window
{
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd != IntPtr.Zero)
            _deviceService.StartMonitoring(hwnd);
    }

    // Avalonia 12: override HandleWindowMessage is not public
    // Use platform-specific message hook:
    protected override void OnPlatformInitialized()
    {
        base.OnPlatformInitialized();
        // Hook message pump via Avalonia.Win32 internals or
        // subclass via SetWindowLongPtr / CallWindowProc
    }
}
```

**Recommended for Phase 1**: Use a hidden message-only window (`CreateWindowEx` with
`HWND_MESSAGE` parent) dedicated to receiving Raw Input. Decouples Raw Input from the
Avalonia window lifecycle.

---

## 6. Known Issues & Gotchas

| Issue | Details | Mitigation |
|-------|---------|-----------|
| `RAWINPUT` struct offset varies | `RAWKEYBOARD` offset = 24 on x64, 16 on x86 | Use `[FieldOffset]` with explicit x64 offsets; target `win-x64` only |
| `ActivateKeyboardLayout` scope | Only affects calling thread | Broadcast or use `PostMessage(HWND_BROADCAST)` for system-wide |
| Multiple same VID:PID | Two identical USB keyboards share one device ID | Documented limitation; Phase 2 uses instance path |
| BUILTIN on VMs | RDP keyboards may appear as `ROOT\RDP_KBD` | Handle `ROOT\RDP_KBD` as `BUILTIN` |
| Raw Input + UAC | `RIDEV_INPUTSINK` doesn't need admin | Confirm: works at user privilege level |
| HKL vs KLID confusion | HKL is runtime handle, KLID is persistent string | Always store KLID in config; load HKL at runtime |
