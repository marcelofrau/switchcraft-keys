# Design Decisions — SwitchcraftKeys

**Status**: Phase 1 LOCKED. 10 core decisions confirmed.

> Consolidated from `archive/switchcarft-keys/docs/DESIGN_DECISIONS.md` +
> `QUESTIONS_ANSWERED.md` (25 decisions). Adapted for C#/Avalonia stack.

---

## Summary Table

| # | Area | Decision | Status |
|---|------|----------|--------|
| 1 | Device ID (USB) | `VID_XXXX&PID_XXXX` extracted from Raw Input path | LOCKED |
| 2 | Device ID (Built-in) | `BUILTIN` (ACPI/I8042 detection) | LOCKED |
| 3 | Layout display format | `LANG - NAME` (e.g. `POR - United States-International`) | LOCKED |
| 4 | Config location | `%APPDATA%\SwitchcraftKeys\config.json` | LOCKED |
| 5 | Config backup | 3-version rotation (`.bak1/.bak2/.bak3`) | LOCKED |
| 6 | Device detection mechanism | Raw Input API (`WM_INPUT`), not polling | LOCKED |
| 7 | Auto-add unknown device | On first keystroke, assign current OS layout | LOCKED |
| 8 | Single instance | Named mutex `Global\SwitchcraftKeys` | LOCKED |
| 9 | UX model | Tray-first, dashboard opens via tray click | LOCKED |
| 10 | Win32 isolation | All P/Invoke in `Interop/` only, zero leakage | LOCKED |

---

## Decision Details

### DD-01: Device ID — USB keyboards

**Decision**: `VID_XXXX&PID_XXXX` format (e.g. `VID_046D&PID_C31C`)

**Rationale**:
- Deterministic and portable — same keyboard = same ID on any machine
- Extracted from Raw Input device path via regex
- Path format: `\\?\HID#VID_046D&PID_C31C&...`
- Regex: `VID_([0-9A-F]{4})&PID_([0-9A-F]{4})`
- Uppercase hex to normalize casing differences across Windows versions

**Trade-off**: Two identical keyboards (same VID:PID) get the same ID.
- Documented limitation for Phase 1
- User can give them different aliases even though they share an ID
- Phase 2: consider instance path suffix for disambiguation

---

### DD-02: Device ID — Built-in keyboards

**Decision**: Special ID `BUILTIN` for any keyboard matching ACPI or I8042 patterns

**Rationale**:
- Built-in keyboards don't have VID:PID in their device path
- Path patterns: contains `ACPI\PNP0303`, `ACPI\PNP030B`, `I8042PRT`, or `ROOT\RDP_KBD`
- Single canonical ID simplifies config portability
- Matches behavior of `switchcraft-keys-python` (used `BUILTIN`) and `switchcraft-keys-too`

**Detection logic**:
```csharp
if (path.Contains("ACPI\\PNP0303") || path.Contains("ACPI\\PNP030B") ||
    path.Contains("I8042PRT") || path.Contains("ROOT\\RDP_KBD"))
    return "BUILTIN";
```

---

### DD-03: Layout display name format

**Decision**: `LANG - NAME` where LANG is the 3-letter language code from Windows

**Rationale**:
- Windows can have multiple layouts with the same name (e.g., two "United States-International")
- Adding language prefix disambiguates (e.g., `POR - United States-International` vs `ENG - United States-International`)
- Derived from `switchcraft-keys-python` v0.1.1 fix that solved this exact bug
- Language code read from registry: `HKLM\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{KLID}\Layout Text` + culture info

---

### DD-04: Config location

**Decision**: `%APPDATA%\SwitchcraftKeys\config.json`

**Rationale**:
- Standard Windows convention; survives app updates/moves
- Per-user isolation (no admin needed)
- Preferred over portable (app-dir) because distributing as single .exe
  placed in arbitrary locations (Downloads, Desktop, etc.) makes portable config unreliable
- `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)`

**Note**: Archive `switchcarft-keys` used app-dir portable. Overridden here because
the C# release is a single `.exe` download (not a folder install).

---

### DD-05: Config backup rotation

**Decision**: Keep 3 backup copies, rotate on every save

**Rotation sequence on save**:
```
config.json.bak3 ← deleted
config.json.bak2 → config.json.bak3
config.json.bak1 → config.json.bak2
config.json      → config.json.bak1
config.json.tmp  → config.json       (atomic write)
```

**Recovery on load**:
1. Try `config.json`
2. If corrupted: try `.bak1`, `.bak2`, `.bak3`
3. If all fail: start fresh (empty config), log critical

---

### DD-06: Device detection mechanism — Raw Input API

**Decision**: Windows Raw Input API (`RegisterRawInputDevices` + `WM_INPUT`)

**Rationale**:
- Keystroke-driven: no polling, zero idle overhead
- `RIDEV_INPUTSINK` flag receives input even when window is not focused
- `GetRawInputData` returns `RAWINPUT.header.hDevice` — the exact device handle
- `GetRawInputDeviceInfo(RIDI_DEVICENAME)` converts handle → device path

**Why not WH_KEYBOARD_LL (SetWindowsHookEx)**:
- Low-level hook receives keystrokes but does NOT identify which physical device
- Raw Input is the correct API for per-device identification
- `switchcraft-keys-python` and `switchcraft-keys-too` both use Raw Input — validated approach

---

### DD-07: Auto-add unknown device

**Decision**: When an unrecognized device fires a keystroke, automatically add it to config
with the current OS layout as default.

**Rationale**:
- Zero-friction onboarding — plug in keyboard, type once, it appears in dashboard
- Matches Python v0.1.1 behavior (validated UX)
- User can rename and reassign layout in dashboard afterward
- Alternative (prompt user) creates friction for new keyboards

---

### DD-08: Single instance enforcement

**Decision**: Named mutex `Global\SwitchcraftKeys`

**Behavior on second launch**:
1. Try to acquire mutex → fails (already held)
2. Find existing window via `FindWindow` or IPC
3. Bring existing window to foreground (`SetForegroundWindow`)
4. Exit with code 0

---

### DD-09: UX model — Tray-first

**Decision**: App starts minimized to tray. Dashboard opens on tray click.

**Rationale**:
- Background utility — user shouldn't see a window on startup
- Tray icon always visible as status indicator (tooltip = current device + layout)
- Close button (X) minimizes to tray, not exits
- Exit only via tray menu "Quit"
- Matches behavior of both Python and Rust archive implementations

**Tray menu**:
- Open Dashboard
- ── separator ──
- Quit

---

### DD-10: Win32 isolation — Interop/ layer

**Decision**: Zero P/Invoke calls outside `Interop/`. Services use only public
methods from `Interop/` classes.

**Rationale**:
- Testability: services mockable, Interop layer swappable with fakes in tests
- Maintainability: Windows API changes affect one layer
- Clarity: `grep -r "DllImport" .` should show only files in `Interop/`

**Enforcement**: Code review rule. No `[DllImport]` in Services/, ViewModels/, Views/.

---

## Out of Scope — Phase 1

These were considered and explicitly deferred:

| Topic | Decision | Phase |
|-------|----------|-------|
| Global hotkey | Not included | 2+ |
| Per-app layout switching | Not included | 2+ |
| Bluetooth keyboards | Treated as USB if VID:PID available, else ignored | 2+ |
| Autostart on login | Not included | 2+ |
| Dark mode | Not included (Luna light only) | 2+ |
| Per-workspace profiles | Not included | 2+ |
| Layout verification retry | Max 3 retries × 100ms | Phase 1 (impl detail) |
