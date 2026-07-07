# Architecture — SwitchcraftKeys

**Version**: 1.0  
**Author**: Marcelo Frau  
**Stack**: C# · .NET 8.0 · Avalonia 12.0.5 · CommunityToolkit.Mvvm · P/Invoke

> Adapted from `archive/switchcarft-keys/docs/ARCHITECTURE.md` (Rust/egui original).
> Architecture patterns translated to C#/Avalonia idioms.

---

## 1. Core Principles

1. **Device-centric model** — users think in devices, not layouts
2. **Deterministic behavior** — no hidden heuristics, explicit mappings
3. **No Win32 outside Interop/** — all P/Invoke calls isolated in one layer
4. **Views know only ViewModels** — no business logic in code-behind
5. **Services testable via interfaces** — Interop layer mockable in tests
6. **Graceful degradation** — errors logged and recovered, never crash

---

## 2. Layer Diagram

```
┌──────────────────────────────────────────────────────────┐
│  Views (.axaml)         ViewModels (CommunityToolkit)    │  UI
│  MainWindow             MainViewModel                    │
│  DeviceListView         DeviceItemViewModel              │
│  DebugOverlayWindow     DebugOverlayViewModel            │
├──────────────────────────────────────────────────────────┤
│  Services (Interfaces in Services/Interfaces/)           │  Business Logic
│  DeviceService          IDeviceService                   │
│  LayoutService          ILayoutService                   │
│  ConfigService          IConfigService                   │
├──────────────────────────────────────────────────────────┤
│  Interop/ (P/Invoke — zero deps on Services/VMs)        │  Windows API
│  RawInputApi            RegisterRawInputDevices          │
│  KeyboardLayoutApi      ActivateKeyboardLayout           │
│  RegistryLayoutReader   HKLM keyboard layouts            │
│  NativeStructs          RAWINPUT, RAWINPUTDEVICE, etc.   │
├──────────────────────────────────────────────────────────┤
│  Models/                                                  │  Data
│  DeviceInfo             LayoutInfo          AppConfig    │
└──────────────────────────────────────────────────────────┘
```

**Dependency rules (strictly enforced)**:
- Views → ViewModels only
- ViewModels → Service interfaces only (no `new DeviceService()` in VM)
- Services → Interop + Models
- Interop → zero application dependencies
- Models → zero dependencies

---

## 3. Directory Structure

```
src/SwitchcraftKeys/
├── Program.cs                        Entry point, Avalonia AppBuilder
├── App.axaml + App.axaml.cs         Application, theme loading
│
├── Assets/
│   ├── icon.ico
│   └── Themes/
│       ├── LunaTheme.axaml          Color tokens, brushes, radii, spacing
│       └── LunaControls.axaml       ControlTheme overrides (Button, TextBox, etc.)
│
├── Models/
│   ├── DeviceInfo.cs                DeviceId (string), Alias, AssignedLayoutKlid
│   ├── LayoutInfo.cs                Klid, DisplayName, LanguageCode, Hkl
│   └── AppConfig.cs                 Root: Dictionary<string,DeviceInfo>, UiSettings
│
├── ViewModels/
│   ├── MainViewModel.cs             Dashboard state, commands
│   ├── DeviceListViewModel.cs       ObservableCollection<DeviceItemViewModel>
│   ├── DeviceItemViewModel.cs       Per-device row: alias, layout selector
│   └── DebugOverlayViewModel.cs     Live debug data, event log
│
├── Views/
│   ├── MainWindow.axaml             Dashboard (device list + toolbar + status bar)
│   ├── MainWindow.axaml.cs
│   ├── DeviceListView.axaml         ItemsControl for device rows
│   ├── DeviceListView.axaml.cs
│   ├── DebugOverlayWindow.axaml     Always-on-top floating debug window
│   └── DebugOverlayWindow.axaml.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IDeviceService.cs
│   │   ├── ILayoutService.cs
│   │   └── IConfigService.cs
│   ├── DeviceService.cs             Raw Input processing, device detection
│   ├── LayoutService.cs             Registry enum, HKL switch, retry
│   └── ConfigService.cs             JSON load/save, backup rotation
│
└── Interop/
    ├── NativeStructs.cs             RAWINPUTDEVICE, RAWINPUT, RAWINPUTHEADER
    ├── NativeConstants.cs           RIDEV_INPUTSINK, RIM_TYPEKEYBOARD, WM_INPUT
    ├── RawInputApi.cs               RegisterRawInputDevices, GetRawInputData
    ├── KeyboardLayoutApi.cs         ActivateKeyboardLayout, GetKeyboardLayoutList
    └── RegistryLayoutReader.cs      HKLM\...\Keyboard Layouts → KLID:name dict
```

---

## 4. Data Flow

### Startup

```
Program.Main()
  → Avalonia AppBuilder.Configure<App>()
  → App.OnFrameworkInitializationCompleted()
    → SingleInstanceGuard.EnsureSingleInstance()   [named mutex]
    → ConfigService.Load()                          [JSON + backup]
    → LayoutService.GetAvailableLayouts()           [registry]
    → new MainViewModel(deviceSvc, layoutSvc, configSvc)
    → new MainWindow { DataContext = vm }
    → vm.InitializeAsync()
      → DeviceService.StartMonitoring(hwnd)         [RegisterRawInputDevices]
      → DeviceService.GetConnectedDevices()         [GetRawInputDeviceList]
```

### Keystroke → Layout Switch

```
User types on keyboard K
  → WM_INPUT message → MainWindow.WndProc (override)
  → DeviceService.ProcessRawInput(lParam)
    → GetRawInputData() → extract hDevice
    → GetRawInputDeviceInfo() → device path
    → DeviceIdNormalizer.Normalize(path) → "VID_046D&PID_C31C"
    → fire DeviceActivated event
  → MainViewModel handles DeviceActivated
    → ConfigService.GetMappedLayout(deviceId) → klid
    → LayoutService.SwitchLayoutAsync(klid)
      → ActivateKeyboardLayout(hkl)
      → poll GetKeyboardLayout() × 3 to verify
    → update CurrentDevice, CurrentLayout observables
    → update tray tooltip
```

### Dashboard Interaction

```
User opens dashboard, selects device row, picks layout from ComboBox
  → DeviceItemViewModel.SelectedLayout setter
    → ConfigService.AssignLayout(deviceId, klid)   [auto-saves JSON]
    → LayoutService.SwitchLayoutAsync(klid)        [apply immediately]
```

---

## 5. Service Interfaces

```csharp
public interface IDeviceService
{
    event EventHandler<DeviceActivatedEventArgs> DeviceActivated;
    IReadOnlyList<DeviceInfo> GetConnectedDevices();
    Task StartMonitoringAsync(IntPtr windowHandle);
    void StopMonitoring();
}

public interface ILayoutService
{
    IReadOnlyList<LayoutInfo> GetAvailableLayouts();
    LayoutInfo? GetCurrentLayout();
    Task<bool> SwitchLayoutAsync(string klid);
}

public interface IConfigService
{
    AppConfig Load();
    void Save(AppConfig config);
    string? GetMappedLayoutKlid(string deviceId);
    void AssignLayout(string deviceId, string klid);
    void SetDeviceAlias(string deviceId, string alias);
    void EnsureDeviceExists(string deviceId);
}
```

---

## 6. Raw Input Integration

Avalonia windows run on Win32 underneath. To receive `WM_INPUT`:

1. `DeviceService.StartMonitoringAsync(hwnd)` calls `RegisterRawInputDevices`
   with `RIDEV_INPUTSINK` so messages arrive even without focus.
2. `MainWindow` overrides `HandleWindowMessage` (Avalonia Win32 hook) to intercept
   `WM_INPUT` and forward the `lParam` to `DeviceService`.
3. `DeviceService` calls `GetRawInputData`, reads `RAWINPUT.header.hDevice`,
   calls `GetRawInputDeviceInfo(RIDI_DEVICENAME)` to get the device path,
   then normalizes to `VID_XXXX&PID_XXXX` or `BUILTIN`.

**Thread safety**: Raw Input messages arrive on the UI thread (message pump). All
`DeviceActivated` handling and subsequent `SwitchLayoutAsync` calls must be async
to avoid blocking the UI thread during the `ActivateKeyboardLayout` + verification
polling.

---

## 7. Config Persistence

- **File**: `%APPDATA%\SwitchcraftKeys\config.json`
- **Format**: `System.Text.Json` with camelCase property names
- **Auto-save**: `ConfigService.AssignLayout` / `SetDeviceAlias` save immediately
- **Atomic write**: write to `config.json.tmp` then `File.Move` (replace)
- **Backup rotation**:
  ```
  config.json.bak3 ← deleted
  config.json.bak2 → config.json.bak3
  config.json.bak1 → config.json.bak2
  config.json      → config.json.bak1
  config.json.tmp  → config.json
  ```
- **Recovery on load**: if JSON parse fails, try `.bak1`, `.bak2`, `.bak3` in order

---

## 7b. Logging

### Infrastructure

- **Library**: Serilog, bridged to `Microsoft.Extensions.Logging.ILoggerFactory`
  via `Serilog.Extensions.Logging` — Services/ViewModels only depend on
  `ILogger<T>`, never on Serilog types (`Logging/LoggerBootstrap.cs` is the
  only place that touches Serilog directly).
- **Level**: single `LoggingLevelSwitch`, seeded from `AppConfig.Logging.MinimumLevel`
  (default `Trace`). Changeable at runtime (e.g. future Settings screen) via
  `LoggerBootstrap.UpdateMinimumLevel`, which also persists the new value.
- **Console sink**: only registered if the process already has a console
  attached (`Interop/ConsoleApi.GetConsoleWindow()` returns non-zero). The app
  is `WinExe`, so double-clicking never allocates one, and `LoggerBootstrap`
  never calls `AllocConsole`/`AttachConsole` — a console only appears if one
  already existed (e.g. launched via `run.ps1` or an existing terminal).
  Colored by level via the built-in `AnsiConsoleTheme.Code` theme.
- **File sink**: one file per process run at
  `%APPDATA%\SwitchcraftKeys\logs\log-{timestamp}.txt`; the last 5 runs are
  kept, older ones deleted on startup.

### Level Semantics

| Level | Use for |
|-------|---------|
| **Trace** | Every observable event: button clicks, screen navigations, state transitions, property changes, event dispatches, WM_INPUT received, hDevice resolved, Raw Input message pump events, ObservableProperty setters relevant to flow |
| **Debug** | Intermediate state: values resolved mid-flow, retry attempt N of N, cache hit/miss, branch decisions, intermediate computed values, config keys read |
| **Info** | Transitions + external calls: screen/window activated, action/command dispatched (name + params), Win32 API called (function name, args, return value), any external call initiated and completed |
| **Warn** | Degraded but recoverable: all retries exhausted but fallback succeeded, layout switch took >500ms, config backup used instead of primary, unexpected Win32 return code but handled |
| **Error** | Recoverable failure: layout KLID not found in registry, JSON parse failed, Win32 call returned error code (log HRESULT), file I/O failure with recovery attempted |
| **Critical** | Fatal: named mutex acquire failed, unrecoverable startup error, no backup config available after corruption |

### Rules

**Always structured parameters — never string interpolation:**
```csharp
// ✅
_logger.LogTrace("WM_INPUT received hDevice={HDevice}", hDevice);
_logger.LogInformation("ActivateKeyboardLayout called klid={Klid} hkl={Hkl}", klid, hkl);

// ❌
_logger.LogTrace($"WM_INPUT received hDevice={hDevice}");
```

**Win32 / external calls — INFO before + after with args + result:**
```csharp
_logger.LogInformation("Calling ActivateKeyboardLayout klid={Klid}", klid);
var result = KeyboardLayoutApi.ActivateKeyboardLayout(hkl, 0);
_logger.LogInformation("ActivateKeyboardLayout returned hkl={Result}", result);
```

**Screen/window activation:**
```csharp
_logger.LogInformation("View activated view={View}", nameof(MainWindow));
```

**Command/action execution:**
```csharp
_logger.LogInformation("Command dispatched command={Command} deviceId={DeviceId}", nameof(AssignLayoutCommand), deviceId);
_logger.LogTrace("Command args layoutKlid={Klid}", klid);
```

**Retry loops:**
```csharp
_logger.LogDebug("Layout verify attempt={Attempt} of={Max} klid={Klid}", attempt, maxRetries, klid);
_logger.LogWarning("Layout switch unverified after retries klid={Klid} elapsed={ElapsedMs}ms", klid, sw.ElapsedMilliseconds);
```

**State transitions:**
```csharp
_logger.LogInformation("Device activated deviceId={DeviceId} previousDevice={PreviousDevice}", deviceId, _currentDeviceId);
_logger.LogInformation("Layout switched klid={Klid} elapsedMs={ElapsedMs}", klid, sw.ElapsedMilliseconds);
```

---

## 8. Theming (Luna/Watercolor)

Loaded in `App.axaml`:
```xml
<Application.Styles>
  <FluentTheme />                                         ← base
  <StyleInclude Source="avares://SwitchcraftKeys/Assets/Themes/LunaTheme.axaml" />
  <StyleInclude Source="avares://SwitchcraftKeys/Assets/Themes/LunaControls.axaml" />
</Application.Styles>
```

`LunaTheme.axaml` defines color tokens as `Color` and `SolidColorBrush` resources.  
`LunaControls.axaml` overrides `Style Selector="Button"`, `TextBox`, `ListBoxItem`, etc.  
Style class `"card"` on `Border` applies surface + shadow + rounded corner treatment.

---

## 9. Error Handling Strategy

| Severity | Example | Action |
|----------|---------|--------|
| Info | Layout switched OK | Log only |
| Warning | Layout switch took >500ms | Log + status bar message |
| Error (recoverable) | Layout not found in registry | Remove from config, notify user in UI |
| Error (recoverable) | Config JSON corrupted | Restore from backup, log critical |
| Fatal | Named mutex acquire fails | Show "already running" message, exit |

No unhandled exceptions. All service methods return `bool`/`Task<bool>` or use
`Result`-style patterns where callers must handle failures.

---

## 10. Performance Targets

| Metric | Target |
|--------|--------|
| Startup (devices enumerated) | < 2 seconds |
| Layout switch (end-to-end) | < 500 ms |
| Raw Input hook latency | < 1 ms |
| Idle CPU | < 1% |
| Memory | < 50 MB |
| Binary size | < 15 MB |

---

## 11. Testing Strategy

**Unit tests** (`SwitchcraftKeys.Tests/`):
- `DeviceNormalizationTests` — VID:PID regex, BUILTIN detection, edge cases
- `ConfigServiceTests` — JSON round-trip, backup rotation logic, migration
- `LayoutServiceTests` — display name format (`LANG - NAME`), KLID parsing

**Integration tests**: Manual Phase 1. Automated in Phase 2 with virtual keyboard simulation.

**Mocking**: Interop layer accessed via interfaces → mockable with `NSubstitute` or hand-rolled fakes.
