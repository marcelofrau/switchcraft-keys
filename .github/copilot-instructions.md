# SwitchcraftKeys — Copilot Instructions

Device-aware keyboard layout manager for Windows · C# · .NET 8.0 (`net8.0-windows`) · Avalonia 12.0.5.
Detects which physical keyboard generated a keystroke (via Raw Input API) and auto-switches the OS
keyboard layout to the one assigned to that device.

## Commands

Run from repo root. Scripts require PowerShell 7+ (`pwsh`).

```powershell
.\build\build.ps1                       # Compile (Debug; add -Config Release)
.\build\test.ps1                        # Unit tests (add -Coverage for HTML report)
.\build\publish.ps1                     # Single .exe → dist/
.\build\clean.ps1                       # Remove bin/, obj/, dist/, TestResults/
.\build\version.ps1 -Bump patch         # Bump version + CHANGELOG + tag
```

Order matters: test before publish. `publish.ps1` reads the version from `SwitchcraftKeys.csproj`.

To run a single test, call `dotnet test` directly with a filter instead of `test.ps1`:

```powershell
dotnet test src\SwitchcraftKeys.Tests\SwitchcraftKeys.Tests.csproj --filter "FullyQualifiedName~DeviceServiceTests.NormalizesUsbId"
```

## Architecture Layers (STRICT)

- **Views** (`.axaml`) → ViewModels only · no code-behind logic
- **ViewModels** → Service interfaces only · never `new ServiceClass()`
- **Services** → Interop + Models · never UI types
- **Interop/** → P/Invoke only · zero app dependencies
- **Models** → no dependencies

Violating this breaks testability and causes architecture creep.

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
└── Interop/                           ⚠️ ONLY P/Invoke + Win32 structs here
    ├── RawInputApi.cs, KeyboardLayoutApi.cs
    ├── RegistryLayoutReader.cs
    └── NativeStructs.cs, NativeConstants.cs

src/SwitchcraftKeys.Tests/   ← xUnit [Fact]/[Theory], FluentAssertions, Coverlet
```

## Critical Implementation Details

### Device IDs
- **USB**: `VID_XXXX&PID_XXXX` (hex, uppercase) · extracted from Raw Input path
- **Built-in**: `BUILTIN` · for ACPI/I8042 keyboards
- Both stable across reboots/machines

### Config Persistence
- **File**: `%APPDATA%\SwitchcraftKeys\config.json` · JSON via `System.Text.Json`
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

## Testing

- **Framework**: xUnit + FluentAssertions + Coverlet
- **Rule**: No Win32 calls in tests · mock Interop via interfaces
- **Coverage**: `.\build\test.ps1 -Coverage` → `TestResults/coverage/index.html`
- **Integration**: Manual for Phase 1 (plug keyboard, type, verify layout change)

## Common Mistakes

| ❌ Don't | ✅ Do |
|----------|-------|
| Call Win32 from ViewModel/Service | Add wrapper in `Interop/`, call via interface |
| `new DeviceService()` in ViewModel | Inject `IDeviceService` via ctor parameter |
| Business logic in `.axaml.cs` code-behind | Push to ViewModel, bind from AXAML |
| Persist HKL (layout handle) | Store 8-char KLID hex, load HKL at runtime |
| Trust `ActivateKeyboardLayout()` instant success | Poll + verify with 3 retries |
| `DllImport` outside `Interop/` | Centralize all P/Invoke in `Interop/` only |

## Workflow

1. Pick a task from `docs/IMPLEMENTATION_PLAN.md` § Phase checklist
2. Feature branch: `git switch -c phase1/1.1-interop`
3. Implement test-first (xUnit + FluentAssertions)
4. Verify: `.\build\build.ps1 && .\build\test.ps1`
5. Commit caveman-style: `feat: device normalization`, `fix: config backup`, etc.

## References

- **Arch + design**: `docs/ARCHITECTURE.md`, `docs/SPECIFICATION.md`, `docs/DESIGN_DECISIONS.md`
- **Windows interop**: `docs/WINDOWS_INTEROP.md` · Raw Input API, HKL, registry details
- **UX**: `docs/UX_DESIGN.md` · tray-first, Luna theme
- **Phase breakdown**: `docs/IMPLEMENTATION_PLAN.md` § Phase Checklist
- **This project also has spec-driven changes tracked under `openspec/`** — see `openspec/config.yaml` and `openspec/changes/`

<!-- rtk-instructions v2 -->
# RTK — Token-Optimized CLI

**rtk** is a CLI proxy that filters and compresses command outputs, saving 60-90% tokens.

## Rule

Always prefix shell commands with `rtk`:

```bash
# Instead of:              Use:
git status                 rtk git status
git log -10                rtk git log -10
cargo test                 rtk cargo test
docker ps                  rtk docker ps
kubectl get pods           rtk kubectl pods
```

## Meta commands (use directly)

```bash
rtk gain              # Token savings dashboard
rtk gain --history    # Per-command savings history
rtk discover          # Find missed rtk opportunities
rtk proxy <cmd>       # Run raw (no filtering) but track usage
```
<!-- /rtk-instructions -->