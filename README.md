# SwitchcraftKeys

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-blue.svg)]()
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)]()
[![Avalonia 12](https://img.shields.io/badge/Avalonia-12.0.5-blue.svg)]()
[![GitHub Release](https://img.shields.io/github/v/release/marcelofrau/switchcraft-keys?include_prereleases)](https://github.com/marcelofrau/switchcraft-keys/releases)
[![GitHub Issues](https://img.shields.io/github/issues/marcelofrau/switchcraft-keys)](https://github.com/marcelofrau/switchcraft-keys/issues)
[![C#](https://img.shields.io/badge/Language-C%23-239120.svg)]()

Device-aware keyboard layout manager for Windows. Automatically switches the OS keyboard layout when you type on a different physical keyboard.

## 🎯 The Problem

Windows assigns one keyboard layout **globally**. If you have two physical keyboards — a US layout desktop keyboard and a notebook with a PT-BR layout — Windows does not know which one you are typing on. Switching layouts manually every time you change keyboards is tedious and error-prone.

```
Keyboard 1 (USB, US)  ──┐
                         └─→ Windows Global Layout: ???
Notebook Keyboard (PT-BR) ─┘

Type on Keyboard 1 → Layout is PT-BR (wrong!)
Type on Notebook   → Layout is US (wrong!)
```

## ✨ The Solution

SwitchcraftKeys uses the **Windows Raw Input API** to detect which physical keyboard generated each keystroke. When you switch keyboards, it automatically activates the layout you previously assigned to that device.

```
Keyboard 1 (USB, US)  ──┐
                         ├─→ Raw Input detects hDevice
Notebook Keyboard (PT-BR)─┤    ↓
                         ├─→ Map device → layout
                         ├─→ Activate layout
                         └─→ No manual switching!
```

## 🚀 Features

| Feature | Details |
|---------|---------|
| **Auto-Detection** | Detects USB keyboards (VID:PID) and built-in keyboards |
| **Per-Device Mappings** | Assign different layouts to each keyboard, persisted across reboots |
| **Zero Delay** | Keystroke-driven activation (no polling, minimal latency) |
| **Portable** | Single `.exe`, no installer required, runs anywhere |
| **Tray Integration** | Minimizes to system tray, out of your way |
| **Dashboard UI** | Manage devices and layouts with a clean Luna-themed interface |
| **Config Backup** | 3-version automatic backup with corruption recovery |
| **Debug Tools** | Always-on-top debug overlay for troubleshooting |
| **CLI Support** | Full command-line interface for automation |
| **Single Instance** | Prevents multiple instances via named mutex |

## 💾 Requirements

- **Windows 10** or **Windows 11** (x64 or ARM64)
- **.NET 8.0 Runtime** (pre-installed on most modern Windows machines)
  - Check: `dotnet --version`
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0

## 📦 Installation

### Option 1: Portable (Recommended)

1. Download `SwitchcraftKeys-v0.1.0-win-x64.zip` from [Releases](https://github.com/marcelofrau/switchcraft-keys/releases)
2. Extract anywhere (e.g., `C:\Tools\SwitchcraftKeys\`)
3. Run `SwitchcraftKeys.exe`
4. It launches in the system tray
5. Click the tray icon to open the dashboard

### Option 2: Installer

1. Download `SwitchcraftKeys-v0.1.0-win-x64-setup.exe` from [Releases](https://github.com/marcelofrau/switchcraft-keys/releases)
2. Run the installer (wizard UI, Portuguese + English)
3. Choose installation directory (default: `C:\Program Files\SwitchcraftKeys`)
4. Installer creates Start Menu shortcuts and desktop shortcut
5. Launch from Start Menu or desktop

## ⚡ Quick Start

1. **Run the application**
   ```
   SwitchcraftKeys.exe
   ```

2. **Register keyboards**
   - Type once on each physical keyboard
   - SwitchcraftKeys detects and adds them automatically
   - Each device gets a unique ID (USB keyboards: `VID_XXXX&PID_XXXX`, built-in: `BUILTIN`)

3. **Assign layouts**
   - Open dashboard (click tray icon)
   - Select each keyboard
   - Choose the layout you want for that device
   - Settings saved automatically

4. **Done!**
   - Switch keyboards → layout switches automatically
   - No manual intervention needed

## 📸 Screenshots

### Dashboard
![Dashboard](docs/screenshots/dashboard.png)  
*Main interface showing detected keyboards, layout assignments, and settings.*

### Tray Integration
![System Tray](docs/screenshots/tray-menu.png)  
*System tray icon with quick access menu.*

### Debug Overlay
![Debug Overlay](docs/screenshots/debug-overlay.png)  
*Real-time event log and device detection diagnostics.*

### Installer
![NSIS Wizard](docs/screenshots/installer-wizard.png)  
*Windows installer with Portuguese/English language selection.*

> Screenshots coming soon! [View placeholder images here.](docs/screenshots/)

## 🖥️ Dashboard

The dashboard allows you to:
- View all detected keyboards (name, device ID, current layout)
- Rename keyboards for easy reference (e.g., "Work USB", "Laptop")
- Assign keyboard layouts per device
- Clear app cache
- Reset all app data
- View detailed event logs
- Adjust logging level

## 🔧 CLI Usage

```bash
SwitchcraftKeys --help              # Show help and version
SwitchcraftKeys --version           # Display version

SwitchcraftKeys --check             # Run health checks (config, cache, logs, .NET, OS)
SwitchcraftKeys --reset-cache       # Clear cached layout data
SwitchcraftKeys --reset-data        # Reset everything (config, cache, logs) + confirm prompt
```

**Example**: Automated reset for CI/CD
```powershell
SwitchcraftKeys --reset-cache  # Clear cache silently
SwitchcraftKeys --check        # Verify system readiness
```

## 🏗️ Building from Source

### Requirements
- **Windows 10/11**
- **.NET SDK 8.0+** (https://dotnet.microsoft.com/download/dotnet/8.0)
- **PowerShell 7+** (for build scripts)
- **NSIS** (optional, for building installer) — install via:
  ```powershell
  scoop install nsis
  ```

### Build Commands

```powershell
# Debug build
.\build\build.ps1

# Release build (optimized)
.\build\build.ps1 -Config Release

# Release + portable zip
.\build\build.ps1 -Config Release -Release
# → Creates: dist\SwitchcraftKeys-v0.1.0-win-x64.zip

# Release + portable zip + installer
.\build\build.ps1 -Config Release -Release -Installer
# → Creates: dist\SwitchcraftKeys-v0.1.0-win-x64.zip
#            dist\SwitchcraftKeys-v0.1.0-win-x64-setup.exe

# Run unit tests
.\build\test.ps1

# Run health check
.\build\check.ps1
```

## 📁 Project Structure

```
switchcraft-keys/
├── src/
│   ├── SwitchcraftKeys/              Main application
│   │   ├── Program.cs                Entry point + CLI parsing
│   │   ├── App.axaml(.cs)            Application shell
│   │   ├── Models/                   DeviceInfo, LayoutInfo, AppConfig
│   │   ├── ViewModels/               MainViewModel, etc.
│   │   ├── Views/                    AXAML UI (MainWindow, DebugOverlay)
│   │   ├── Services/                 IDeviceService, ILayoutService, IConfigService
│   │   ├── Interop/                  P/Invoke (Raw Input, Keyboard Layout API)
│   │   ├── Logging/                  Serilog bootstrap, ApplicationLogService
│   │   └── Assets/
│   │       ├── icon.ico              App icon
│   │       ├── Themes/               Luna theme (colors, styles)
│   │       └── Views/                Per-view icons (asset-manager managed)
│   └── SwitchcraftKeys.Tests/        Unit tests (xUnit + FluentAssertions)
├── build/                            Build scripts (PowerShell)
│   ├── build.ps1                     Main build script
│   ├── build-release.ps1             Release packaging (zip + installer)
│   ├── test.ps1                      Run tests + coverage
│   ├── check.ps1                     Health check
│   └── clean.ps1                     Clean build artifacts
├── installer/
│   └── SwitchcraftKeys.nsi           NSIS installer script
├── .github/
│   └── workflows/
│       └── build.yml                 GitHub Actions CI/CD pipeline
├── docs/                             Architecture, design decisions
├── CHANGELOG.md                      Version history
├── LICENSE                           MIT License
└── README.md                         This file
```

## 📋 Architecture & Documentation

- **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** — Layered design, data flow, service interfaces
- **[SPECIFICATION.md](docs/SPECIFICATION.md)** — Functional requirements, device detection, layout switching
- **[DESIGN_DECISIONS.md](docs/DESIGN_DECISIONS.md)** — Why Avalonia, why NSIS, why Serilog, etc.
- **[WINDOWS_INTEROP.md](docs/WINDOWS_INTEROP.md)** — Raw Input API, HKL details, registry layout reader
- **[UX_DESIGN.md](docs/UX_DESIGN.md)** — Tray-first, Luna theme, dashboard flows
- **[IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md)** — Phase breakdown (0-4), stack decisions

## 🐛 Troubleshooting

### "The specified RuntimeIdentifier is not recognized"
**Cause**: .NET SDK not installed or PATH not updated  
**Fix**: 
```powershell
dotnet --version
# Should show 8.0.xxx or higher
# If not: https://dotnet.microsoft.com/download/dotnet/8.0
```

### "Cannot detect keyboards"
**Possible causes**:
- Keyboard not yet plugged in when app started
- Built-in keyboard driver issues
- **Fix**: Run health check first
  ```powershell
  SwitchcraftKeys --check
  ```

### "Layout doesn't switch automatically"
**Possible causes**:
- Layout KLID not installed in Windows
- Device not recognized by Raw Input API
- **Fix**: 
  1. Open dashboard, check device ID
  2. Verify layout appears in "Available Layouts" dropdown
  3. Check debug overlay (toggle in dashboard)
  4. View event log for errors

### "Health check shows warnings"
```
Config file ...... CORRUPTED: JSON parse error
Cache directory .. ERROR: access denied
```

**Fix**: Reset app data
```powershell
SwitchcraftKeys --reset-data
# Removes config, cache, logs. Rebuilds on next run.
```

### "Multiple instances running"
**Cause**: Named mutex not acquired  
**Fix**: 
```powershell
# Kill all instances
taskkill /IM SwitchcraftKeys.exe /F
# Restart
SwitchcraftKeys.exe
```

## 🤝 Contributing

Contributions welcome! Please:

1. **Fork** the repository
2. **Create a feature branch** (`git checkout -b feature/your-feature`)
3. **Commit** with clear messages (`git commit -m "feat: add X"`)
4. **Push** to your fork (`git push origin feature/your-feature`)
5. **Open a Pull Request** against `main`

### Code Style
- Follow C# naming conventions (PascalCase for classes, camelCase for locals)
- Use CommunityToolkit.MVVM for ViewModels (`[ObservableProperty]`, `[RelayCommand]`)
- All Win32 calls isolated in `Interop/` folder
- Structured logging (never string interpolation in log calls)
- Unit tests for business logic (xUnit + FluentAssertions)

### Commit Messages
Follow [Conventional Commits](https://www.conventionalcommits.org/):
- `feat: add device alias feature`
- `fix: layout switch retry logic`
- `docs: update architecture guide`
- `chore: update dependencies`

## 📄 License

[MIT](LICENSE) — Marcelo Frau, 2026

You are free to use, modify, and distribute this software for personal or commercial purposes.

## 📞 Support

- **Issues**: https://github.com/marcelofrau/switchcraft-keys/issues
- **Discussions**: https://github.com/marcelofrau/switchcraft-keys/discussions
- **Email**: marcelofrau@gmail.com

## 🙏 Acknowledgments

- **Windows Raw Input API** — Microsoft documentation
- **Avalonia** — Cross-platform UI framework
- **Serilog** — Structured logging
- **CommunityToolkit.MVVM** — Lightweight MVVM utilities
- **Icons8** — Icon set (attribution in `ATTRIBUTIONS.md`)

---

**Status**: 🚀 Phase 1 MVP complete — Core functionality stable, ready for production use.  
**Latest Release**: [v0.1.0](https://github.com/marcelofrau/switchcraft-keys/releases/tag/v0.1.0)  
**Repository**: https://github.com/marcelofrau/switchcraft-keys
