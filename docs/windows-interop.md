---
layout: default
title: Windows Interop
description: Raw Input, keyboard layout, registry, and foreground window interop
---

# Windows Interop

All native calls live in `src/SwitchcraftKeys/Interop/`. No ViewModel or Service owns `[DllImport]` declarations directly.

## Interop Map

```mermaid
flowchart TB
    subgraph Interop[Interop layer]
        RawInputApi[RawInputApi]
        KeyboardLayoutApi[KeyboardLayoutApi]
        RegistryLayoutReader[RegistryLayoutReader]
        WindowsInputSettingsRegistry[WindowsInputSettingsRegistry]
        ConsoleApi[ConsoleApi]
    end

    subgraph Windows[Windows APIs]
        User32[user32.dll]
        Registry[HKLM/HKCU Registry]
        Kernel32[kernel32.dll]
    end

    RawInputApi --> User32
    KeyboardLayoutApi --> User32
    RegistryLayoutReader --> Registry
    WindowsInputSettingsRegistry --> Registry
    ConsoleApi --> Kernel32
```

## Raw Input

Raw Input identifies which physical keyboard generated a keystroke.

```mermaid
sequenceDiagram
    participant Win as Windows message pump
    participant Main as MainWindow
    participant Device as DeviceService
    participant Raw as RawInputApi
    participant Norm as DeviceIdNormalizer

    Win->>Main: WM_INPUT(lParam)
    Main->>Device: ProcessRawInput(lParam)
    Device->>Raw: GetRawInputData(query)
    Raw-->>Device: byte size
    Device->>Raw: GetRawInputData(read)
    Raw-->>Device: RAWINPUT.header.hDevice
    Device->>Raw: GetRawInputDeviceInfo(hDevice)
    Raw-->>Device: raw device path
    Device->>Norm: Normalize(path)
    Norm-->>Device: VID_XXXX&PID_XXXX or BUILTIN
```

## Layout Switching

Keyboard layouts are per thread/window in Windows. SwitchcraftKeys applies the configured HKL to the focused window.

```mermaid
sequenceDiagram
    participant VM as MainViewModel
    participant Layout as LayoutService
    participant User32 as user32.dll
    participant Foreground as Focused app window

    VM->>Layout: SwitchLayoutAsync(klid)
    Layout->>User32: GetKeyboardLayoutList()
    User32-->>Layout: loaded HKLs
    Layout->>User32: ActivateKeyboardLayout(hkl)
    Layout->>User32: GetForegroundWindow()
    User32-->>Layout: hwnd
    Layout->>Foreground: WM_INPUTLANGCHANGEREQUEST(hkl)
    Layout->>User32: GetKeyboardLayout(foregroundThreadId)
    User32-->>Layout: current HKL
```

## Registry Usage

| Path | Purpose |
|------|---------|
| `HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layouts` | Friendly layout names |
| `HKCU\Control Panel\International\User Profile` | Per-app input method toggle |

## Native Boundaries

```mermaid
flowchart LR
    ViewModels --> Interfaces[Service interfaces]
    Interfaces --> Services
    Services --> Interop
    Interop --> Win32[Win32 / Registry]
```
