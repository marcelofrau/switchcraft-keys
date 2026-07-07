# Specification — SwitchcraftKeys

**Version**: 1.0  
**Author**: Marcelo Frau  
**Platform**: Windows 10 / 11  
**Stack**: C# / .NET 8.0 / Avalonia 12 / CommunityToolkit.Mvvm

> Adapted from `archive/switchcarft-keys/docs/SPECIFICATION.md` (Rust/egui original)
> and `archive/switchcraft-keys-python` behavior reference (v0.1.1, released).

---

## 1. Overview

**SwitchcraftKeys** solves the non-deterministic keyboard layout switching problem by
introducing device-aware, deterministic keyboard layout management.

Windows manages layouts at the OS level — not per physical device. When you switch
between keyboards (laptop built-in, external USB, mechanical), Windows does not know
which one you are typing on and applies hidden heuristics, leading to wrong layouts.

**SwitchcraftKeys** bridges this gap by:
1. Detecting connected physical keyboards (USB and built-in)
2. Tracking which keyboard generated each keystroke via Windows Raw Input API
3. Maintaining persistent device-to-layout mappings in JSON
4. Automatically switching the OS layout when a device becomes active

---

## 2. Functional Requirements

### 2.1 Device Detection

**FR-DET-001**: Enumerate all connected keyboards on startup.
- USB: extract VID:PID from Raw Input device path
- Built-in: detect ACPI/I8042 devices → assign special ID `BUILTIN`
- Show friendly name (user-editable alias)
- Persist device identifiers across restarts

**FR-DET-002**: Detect device activation via keystroke (Raw Input, not polling).
- Register `RegisterRawInputDevices` on app window
- Parse `WM_INPUT` message to identify source device
- Auto-add unknown devices on first keystroke

### 2.2 Device-to-Layout Mapping

**FR-MAP-001**: Allow users to assign a keyboard layout to each device.
- Persist mappings in `%APPDATA%\SwitchcraftKeys\config.json`
- Layout identified by Windows KLID (e.g., `00000409`)
- Validate that mapped layout exists on the system

**FR-MAP-002**: Configuration persists across restarts.
- Auto-save on every change
- Backup rotation: 3 versions (`config.json.bak1`, `.bak2`, `.bak3`)
- Auto-recover from most recent valid backup on corruption

### 2.3 Layout Enforcement

**FR-ENF-001**: When a device becomes active, apply its configured layout.
- Call `ActivateKeyboardLayout` via P/Invoke
- Verify layout change with polling (up to 3 retries, 100ms apart)
- Log failed attempts

**FR-ENF-002**: Auto-add unknown device on first keystroke.
- Assign current OS layout as default
- Show device in dashboard immediately

### 2.4 User Interface

**FR-UI-001**: Dashboard window (opens via tray click).
- Shows list of known devices with assigned layout
- Each device row: alias (editable) + layout selector (ComboBox)
- Status bar: current active device + current layout

**FR-UI-002**: Tray-first UX.
- App starts minimized to tray
- Tray menu: "Open Dashboard", "Quit"
- Close button minimizes to tray (not exit)
- Tooltip: `SwitchcraftKeys — [Device]: [Layout]`

**FR-UI-003**: Debug overlay (Phase 1 simplified).
- Always-on-top floating window
- Shows: current device ID, full device path, active layout, last Raw Input event
- Scrollable event log (last 50 entries)
- Toggle via toolbar button in dashboard

**FR-UI-004**: Layout selector format.
- Display: `LANG - NAME` (e.g., `POR - United States-International`, `ENG - US`)
- Cascading: language group → layout name

### 2.5 System Integration

**FR-SYS-001**: Single instance enforcement.
- Named mutex `Global\SwitchcraftKeys`
- Second launch: bring existing window to foreground, then exit

**FR-SYS-002**: No admin privileges required.
- Raw Input API works at user level
- `%APPDATA%` config — no system-wide write

---

## 3. Non-Functional Requirements

### 3.1 Performance

**NFR-PERF-001**: Device enumeration on startup < 2 seconds.

**NFR-PERF-002**: Layout switching < 500 ms from keystroke detection.

**NFR-PERF-003**: UI remains responsive — no blocking calls on UI thread.

**NFR-PERF-004**: Keystroke hook latency < 1 ms (Raw Input is passive, minimal overhead).

### 3.2 Reliability

**NFR-REL-001**: Missing layout → log warning, remove from config, notify user.

**NFR-REL-002**: Config corruption → auto-recover from backup, start fresh if all fail.

**NFR-REL-003**: Layout switch failure → retry up to 3×, log error, do not crash.

### 3.3 Security

**NFR-SEC-001**: Config stored in `%APPDATA%\SwitchcraftKeys\` (user-scoped).

**NFR-SEC-002**: No admin privileges required for normal operation.

### 3.4 Deployment

**NFR-DEP-001**: Single `.exe` — no installer, no extra runtime downloads beyond .NET 8.

**NFR-DEP-002**: Binary size target < 15 MB.

**NFR-DEP-003**: Memory usage < 50 MB.

---

## 4. Config Schema

Location: `%APPDATA%\SwitchcraftKeys\config.json`

```json
{
  "version": 1,
  "devices": {
    "VID_046D&PID_C31C": {
      "alias": "Logitech MX Keys",
      "layoutKlid": "00000409"
    },
    "BUILTIN": {
      "alias": "Notebook Keyboard",
      "layoutKlid": "00000416"
    }
  },
  "ui": {
    "hudEnabled": false,
    "startMinimized": true
  }
}
```

**Device ID formats**:
- USB: `VID_XXXX&PID_XXXX` (hex, uppercase, extracted from Raw Input device path)
- Built-in: `BUILTIN`

**KLID**: Windows Keyboard Layout Identifier, 8-char hex string (e.g., `00000409` = US English)

---

## 5. Out of Scope (Phase 1)

| Feature | Phase |
|---------|-------|
| Global hotkey override | 2+ |
| Per-application layout switching | 2+ |
| Bluetooth keyboard detection | 2+ |
| Background service (no UI) | 2+ |
| Per-workspace profiles | 2+ |
| Dark mode | 2+ |
| Cloud sync | 3+ |
| macOS / Linux support | 3+ |
| Key remapping | 3+ |

---

## 6. Success Criteria (Phase 1)

1. Detects 90%+ of common keyboards (USB + built-in)
2. Switches layout within 500 ms of device activation
3. Config persists across restarts without data loss
4. Dashboard shows correct device/layout state at all times
5. No admin privileges required
6. Ships as single `.exe` < 15 MB
7. CPU < 1% idle, memory < 50 MB

---

## 7. Glossary

| Term | Definition |
|------|-----------|
| VID/PID | Vendor ID / Product ID — unique USB device identifiers |
| KLID | Keyboard Layout Identifier — 8-char hex string used by Windows |
| HKL | Handle to Keyboard Layout — runtime handle returned by Windows |
| Raw Input | Windows API for low-level device input before OS processing |
| BUILTIN | Special device ID assigned to built-in/laptop keyboards |
| ACPI | Advanced Configuration and Power Interface — how BIOS keyboards appear in device tree |
