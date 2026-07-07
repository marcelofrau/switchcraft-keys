# AGENTS.md — SwitchcraftKeys

**Project**: Device-aware keyboard layout manager for Windows · C# · .NET 8.0 · Avalonia 12.0.5  
**Phase**: 0 ✅ (scaffolding complete) → Phase 1 in progress  
**Scope locked**: See `docs/IMPLEMENTATION_PLAN.md`

---

## Commands

All from repo root. Scripts use PowerShell 7+.

```powershell
.\build\build.ps1                       # Compile (Debug; add -Config Release)
.\build\test.ps1                        # Unit tests (add -Coverage for HTML report)
.\build\publish.ps1                     # Single .exe → dist/
.\build\clean.ps1                       # Remove bin/, obj/, dist/, TestResults/
.\build\version.ps1 -Bump patch         # Bump version + CHANGELOG + tag
```

**Order matters**: Test before publish. Publish auto-reads version from `.csproj`.

---

## Architecture Layers (STRICT)

- **Views** (`.axaml`) → ViewModels only · no code-behind logic
- **ViewModels** → Service interfaces only · never `new ServiceClass()`
- **Services** → Interop + Models · never UI types
- **Interop/** → P/Invoke only · zero app dependencies
- **Models** → no dependencies

Violating this breaks testability and causes architecture creep. Enforce on review.

---

## Project Structure

```
src/SwitchcraftKeys/
├── Program.cs, App.axaml(.cs)
├── Assets/Themes/
│   ├── LunaTheme.axaml               ← Color tokens (Luna/Watercolor light theme)
│   └── LunaControls.axaml            ← Control style overrides
├── Models/                            ← DeviceInfo, LayoutInfo, AppConfig
├── ViewModels/                        ← [ObservableProperty], no service instantiation
├── Views/                             ← Avalonia AXAML + minimal code-behind
├── Services/
│   ├── Interfaces/                    ← IDeviceService, ILayoutService, IConfigService
│   ├── DeviceService.cs               ← Raw Input + device detection
│   ├── LayoutService.cs               ← Registry enum + retry logic
│   └── ConfigService.cs               ← JSON ± 3-backup rotation
└── Interop/                           ⚠️ **ONLY** P/Invoke + Win32 structs here
    ├── RawInputApi.cs, KeyboardLayoutApi.cs
    ├── RegistryLayoutReader.cs
    └── NativeStructs.cs, NativeConstants.cs

src/SwitchcraftKeys.Tests/
├── xUnit [Fact]/[Theory] tests
├── FluentAssertions fluent syntax
└── Coverlet for coverage (HTML → TestResults/coverage/)
```

---

## Critical Implementation Details

### Device IDs
- **USB**: `VID_XXXX&PID_XXXX` (hex, uppercase) · extracted from Raw Input path
- **Built-in**: `BUILTIN` · for ACPI/I8042 keyboards
- Both stable across reboots/machines

### Config Persistence
- **File**: `%APPDATA%\SwitchcraftKeys\config.json` · JSON System.Text.Json
- **Backup rotation**: Before save, rotate `.bak3` ← `.bak2` ← `.bak1` ← active
- **Recovery**: On parse error, try `.bak1`, `.bak2`, `.bak3` in order
- **Auto-save**: Every `AssignLayout()` / `SetDeviceAlias()` call writes immediately

### Win32 Quirks
- **Raw Input**: Arrives on UI thread (message pump) · use async for blocking Win32 calls
- **Layout switch**: `ActivateKeyboardLayout()` doesn't always succeed immediately · poll with 3 retries × 100ms
- **KLID format**: Always store 8-char hex string (e.g., `00000409`), load HKL at runtime · never persist HKL

### Avalonia 12 Specific
- Compiled bindings enabled by default (`AvaloniaUseCompiledBindingsByDefault`)
- Use `x:DataType` on all control templates
- No XAML triggers · use pseudo-classes (`:pointerover`, etc.) in AXAML
- `AllowUnsafeBlocks` enabled for P/Invoke marshalling

---

## Testing

- **Framework**: xUnit + FluentAssertions + Coverlet
- **Rule**: No Win32 calls in tests · mock Interop via interfaces
- **Coverage target**: Generate with `.\build\test.ps1 -Coverage` → TestResults/coverage/index.html
- **Integration**: Manual Phase 1 (plug keyboard, type, verify layout change)

---

## Common Mistakes

| ❌ Don't | ✅ Do |
|----------|-------|
| Call Win32 from ViewModel/Service | Add wrapper in `Interop/`, call via interface |
| `new DeviceService()` in ViewModel | Inject `IDeviceService` via ctor parameter |
| Business logic in `.axaml.cs` code-behind | Push to ViewModel, bind from AXAML |
| Persist HKL (layout Handle) | Store 8-char KLID hex, load HKL at runtime |
| Trust `ActivateKeyboardLayout()` instant success | Poll + verify with 3 retries |
| DllImport outside `Interop/` | Centralize all P/Invoke in `Interop/` only |

---

## Workflow

1. **Pick Phase 1 task** from `docs/IMPLEMENTATION_PLAN.md` § Phase 1
2. **Add feature branch**: `git switch -c phase1/1.1-interop`
3. **Implement** test-first (xUnit + FluentAssertions)
4. **Verify build + tests**: `.\build\build.ps1 && .\build\test.ps1`
5. **Commit caveman-style**: `feat: device normalization`, `fix: config backup`, etc.
6. **Push + PR** (if team model; single-author can merge directly)

---

## Assets & Icons

**Rule**: Any time an icon or image is needed in the UI — dialogs, toolbars, tray,
status indicators, empty states, buttons — load the `asset-manager` skill first.
Do not hardcode `avares://` paths or copy files manually without it.

**Auto-trigger (agent-initiated)**: If you are about to write any `<Image>` element,
reference any `avares://` asset path, or implement any UI component that visually
needs an icon or image — STOP and load the `asset-manager` skill before proceeding.
This applies even when the user did not mention icons. You decide when an icon would
improve the UI, and you use the skill to source it correctly.

```
skill: asset-manager
docs: docs/ASSETS-GUIDE.md, docs/ICON-SET-SYNC.md, docs/ATTRIBUTIONS.md
icon set: D:\workspace\_non_work_\icons8-personal-set
```

---

## Logging Policy

Every class that logs receives `ILogger<T>` via constructor injection. Never use Serilog types directly outside `Logging/LoggerBootstrap.cs`.

### Level Semantics

| Level | Use for |
|-------|---------|
| **Trace** | Every observable event: button clicks, screen navigations, state transitions, property changes, event dispatches, WM_INPUT received, hDevice resolved, Raw Input message pump events, ObservableProperty setters relevant to flow |
| **Debug** | Intermediate state: values resolved mid-flow, retry attempt N of N, cache hit/miss, branch decisions, intermediate computed values, config keys read |
| **Info** | Transitions + external calls: screen/window activated, action/command dispatched (name + params), Win32 API called (function name, args, return value), any REST or external call initiated and completed |
| **Warn** | Degraded but recoverable: all retries exhausted but fallback succeeded, layout switch took >500ms, config backup used instead of primary, unexpected Win32 return code but handled |
| **Error** | Recoverable failure: layout KLID not found in registry, JSON parse failed, Win32 call returned error code (log HRESULT), file I/O failure with recovery attempted |
| **Critical** | Fatal: named mutex acquire failed, unrecoverable startup error, no backup config available after corruption |

### Rules

**Always use structured parameters — never string interpolation:**
```csharp
// ✅
_logger.LogTrace("WM_INPUT received hDevice={HDevice}", hDevice);
_logger.LogInformation("ActivateKeyboardLayout called klid={Klid} hkl={Hkl}", klid, hkl);

// ❌
_logger.LogTrace($"WM_INPUT received hDevice={hDevice}");
```

**Win32 / external calls — log INFO before + after with args + result:**
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

**State transitions (DeviceActivated, config loaded, layout switched):**
```csharp
_logger.LogInformation("Device activated deviceId={DeviceId} previousDevice={PreviousDevice}", deviceId, _currentDeviceId);
_logger.LogInformation("Layout switched klid={Klid} elapsedMs={ElapsedMs}", klid, sw.ElapsedMilliseconds);
```

---

## References

- **Arch + Design**: `docs/ARCHITECTURE.md`, `docs/SPECIFICATION.md`, `docs/DESIGN_DECISIONS.md`
- **Windows Interop**: `docs/WINDOWS_INTEROP.md` · Raw Input API, HKL, registry details
- **UX**: `docs/UX_DESIGN.md` · tray-first, Luna theme
- **Assets**: `docs/ASSETS-GUIDE.md` · naming, sizes, AXAML refs · `docs/ATTRIBUTIONS.md`
- **Phase breakdown**: `docs/IMPLEMENTATION_PLAN.md` § Phase Checklist
- **Prior art**: `../archive/` · three prior implementations (Rust, Python)
- **Local Avalonia refs**: `D:\workspace\_non_work_\xb-homebrew-vault` and `D:\workspace\_non_work_\openburningsuite` · use for site/docs organization, Avalonia UI patterns, and release/CI reference when available
- **Avalonia**: https://docs.avaloniaui.net/ · compiled bindings, DataTemplate, Transitions
- **MVVM**: https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/

---

**Updated**: 2026-07-01 · Version 0.1.0 · Phase 1 in progress
