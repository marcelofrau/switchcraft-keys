# Implementation Plan — SwitchcraftKeys

**Author:** Marcelo Frau <marcelofrau@gmail.com>
**Started:** 2026-07
**Stack:** .NET 8.0 · Avalonia 12.0.5 · CommunityToolkit.Mvvm 8.4.2 · P/Invoke

---

## Project Summary

Device-aware keyboard layout manager for Windows. Auto-detects which physical keyboard
generated a keystroke via Raw Input API and switches the OS keyboard layout to the one
mapped for that device. Config persisted in JSON under `%APPDATA%\SwitchcraftKeys\`.

---

## Phase Checklist

### Phase 0 — Scaffolding
- [x] `git init`, branch `main`
- [x] Directory structure created
- [x] `.gitignore`, `.editorconfig`
- [x] `LICENSE` (MIT, 2026, Marcelo Frau)
- [x] `README.md` initial
- [x] `SwitchcraftKeys.slnx` + 2 projects (app + tests)
- [x] NuGet packages configured
- [x] Build scripts in `build/`
- [x] Initial docs in `docs/`
- [x] First commit: `chore: initial project scaffold`

### Phase 0.5 — Logging & Config Foundation
- [x] `Models/AppConfig.cs` — `Version` + `LoggingConfig { MinimumLevel }`
- [x] `Services/Interfaces/IConfigService.cs`, `Services/ConfigService.cs` — JSON
      load/save at `%APPDATA%\SwitchcraftKeys\config.json`, `.bak1/.bak2/.bak3`
      backup rotation, cascading recovery on corruption
- [x] `Interop/ConsoleApi.cs` — `GetConsoleWindow()` detection only (never
      allocates/attaches a console — GUI launches stay silent)
- [x] `Logging/LoggerBootstrap.cs` — Serilog bootstrap: `LoggingLevelSwitch`
      driven by config (default `Trace`), Console sink (colored theme, only
      if a console is already attached) + rotating File sink (one file per
      run under `logs/`, keeps last 5), bridged to
      `Microsoft.Extensions.Logging.ILoggerFactory` via
      `Serilog.Extensions.Logging`
- [x] Composition root wiring in `Program.cs` / `App.axaml.cs` — `MainViewModel`
      receives `ILogger<MainViewModel>` + `IConfigService` via constructor
- [x] Temporary log-level smoke test in `Program.cs::Main` (remove once
      Phase 1 UI can be used to validate logging manually)
- [x] Unit tests: `ConfigServiceTests` (round-trip, backup rotation, corruption
      recovery)
- [x] Commit: `feat: logging + config foundation`

### Phase 1 — Interop + Core Logic
- [x] `Interop/NativeStructs.cs` — RAWINPUTDEVICE, RAWINPUT, RAWINPUTHEADER, RID_DEVICE_INFO
- [x] `Interop/NativeConstants.cs` — RIDEV_INPUTSINK, RIM_TYPEKEYBOARD, WM_INPUT, etc.
- [x] `Interop/RawInputApi.cs` — RegisterRawInputDevices, GetRawInputData, GetRawInputDeviceInfo, GetRawInputDeviceList
- [x] `Interop/KeyboardLayoutApi.cs` — GetKeyboardLayoutList, ActivateKeyboardLayout, LoadKeyboardLayout
- [x] `Interop/RegistryLayoutReader.cs` — HKLM\...\Keyboard Layouts enum → LANG - NAME format
- [x] `Services/DeviceIdNormalizer.cs` — VID:PID extraction, BUILTIN detection
- [x] `Models/DeviceInfo.cs`
- [x] `Models/LayoutInfo.cs`
- [x] `Models/AppConfig.cs` — expanded in Phase 0.5 (Version, LoggingConfig, Devices, UiSettings)
- [x] Unit tests: DeviceNormalizationTests, RegistryLayoutReaderTests
- [x] `Views/DebugOverlayWindow.axaml` — always-on-top debug float
- [x] `ViewModels/DebugOverlayViewModel.cs`
- [x] Commit: `feat: windows interop layer + device normalization`

### Phase 2 — Services + Config
- [x] `Services/Interfaces/IDeviceService.cs`
- [x] `Services/Interfaces/ILayoutService.cs`
- [x] `Services/Interfaces/IConfigService.cs`
- [x] `Services/DeviceService.cs` — Raw Input + message loop integration
- [x] `Services/LayoutService.cs` — registry enum + retry/verification
- [x] `Services/ConfigService.cs` — JSON + 3-version backup rotation
- [x] Single instance enforcement (named mutex `Global\SwitchcraftKeys`)
- [ ] Commit: `feat: core services + config persistence`

### Phase 3 — UI Dashboard
- [ ] Refine `Assets/Themes/LunaTheme.axaml` — finalize color tokens
- [ ] Refine `Assets/Themes/LunaControls.axaml` — all control themes
- [ ] `MainWindow` with device list + layout selectors
- [ ] `Views/DeviceListView.axaml` — per-device row with alias + ComboBox
- [ ] Tray icon via Avalonia built-in `<TrayIcon>`
- [ ] Minimize-to-tray behavior (close → tray, not exit)
- [ ] Status bar (current device + layout)
- [ ] Commit: `feat: dashboard UI + Luna theme`

### Phase 4 — Docs + Polish
- [ ] Consolidate archive docs into `docs/`
- [ ] `README.md` with screenshots
- [ ] `CHANGELOG.md`
- [ ] Build script end-to-end tested → `dist/*.exe`
- [ ] Commit: `chore: docs, changelog, build polish`

---

## Stack Decisions

| Item | Decision | Rationale |
|------|----------|-----------|
| Framework | .NET 8.0 LTS | Pre-installed on most Win10/11; stable LTS |
| UI | Avalonia 12.0.5 | Latest preview; already used in other personal project |
| MVVM | CommunityToolkit.Mvvm 8.4.2 | Simple, source-gen based, no extra deps |
| Win32 Interop | P/Invoke manual | Full control, no extra packages |
| Config | JSON → %APPDATA% | Standard Windows, survives exe updates |
| Distribution | Single `.exe` | No installer, portable |
| Tray | Avalonia built-in TrayIcon | No extra packages |
| License | MIT | Permissive, personal tool |
| Platform | Windows-only | Raw Input + registry = Win32 core logic |
| Theme | Luna/Watercolor light | XP Luna aesthetic, soft gradients |

---

## Architecture Layers

```
┌──────────────────────────────────────────────────┐
│  Views (AXAML)  ←→  ViewModels (CommunityToolkit) │  UI Layer
├──────────────────────────────────────────────────┤
│  Services (IDeviceService, ILayoutService, ...)   │  Business Logic
├──────────────────────────────────────────────────┤
│  Interop (P/Invoke — Raw Input, Keyboard Layout)  │  Windows API
├──────────────────────────────────────────────────┤
│  Models (DeviceInfo, LayoutInfo, AppConfig)       │  Data
└──────────────────────────────────────────────────┘
```

Rules:
- Views depend on ViewModels only
- ViewModels depend on Service interfaces only
- Services depend on Interop + Models
- Interop has zero dependencies on Services/ViewModels
- No Win32 calls outside `Interop/`

---

## Config Schema

`%APPDATA%\SwitchcraftKeys\config.json`

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

Backup rotation: `config.json.bak1`, `.bak2`, `.bak3` — auto-recovered on corruption.

---

## Performance Targets (from archive)

| Metric | Target |
|--------|--------|
| Device enumeration | < 2 seconds |
| Layout switching | < 500 ms |
| Keystroke hook latency | < 1 ms |
| Memory usage | < 50 MB |
| Binary size | < 15 MB |

---

## Out of Scope (Phase 1)

- Global hotkey override
- Per-application layout switching
- Bluetooth keyboard detection
- Background service (non-UI) mode
- Per-workspace profiles
- Dark mode (Phase 2+)
- Cloud sync
- macOS / Linux support

---

## Archive Reference

Three prior implementations informed this design:

| Project | Lang | Status | Key contribution |
|---------|------|--------|-----------------|
| `switchcarft-keys` | Rust/egui | Planning only | 25 design decisions, 73 tickets, extensive docs |
| `switchcraft-keys-python` | Python | Released v0.1.1 | Proven behavior reference |
| `switchcraft-keys-too` | Rust | 95% (type errors) | Raw Input P/Invoke patterns |
