# SwitchcraftKeys

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-blue.svg)]()
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)]()
[![Avalonia 12](https://img.shields.io/badge/Avalonia-12.0-blue.svg)]()

Device-aware keyboard layout manager for Windows. Automatically switches the OS keyboard layout when you type on a different physical keyboard.

## The Problem

Windows assigns one keyboard layout globally. If you have two physical keyboards — a US layout desktop keyboard and a notebook with a PT-BR layout — Windows does not know which one you are typing on. Switching layouts manually every time you change keyboards is tedious and error-prone.

## The Solution

SwitchcraftKeys uses the Windows Raw Input API to detect which physical keyboard generated each keystroke. When you switch keyboards, it automatically activates the layout you previously assigned to that device.

## Features

- Auto-detects physical keyboards via Raw Input API (USB + built-in)
- Per-device keyboard layout assignment, persisted across reboots
- Tray-first UX — runs silently in the system tray
- Dashboard to manage device/layout mappings
- Auto-adds unknown devices on first keystroke
- Single `.exe`, no installer, no registry pollution
- Debug overlay window for troubleshooting

## Requirements

- Windows 10 or Windows 11
- .NET 8.0 Runtime (pre-installed on most Win10/11 machines)

## Installation

1. Download `switchcraft-keys-vX.X.X-win-x64.exe` from Releases
2. Place it anywhere (e.g. `C:\Tools\`)
3. Run it — it starts in the system tray
4. Type on each keyboard once to register it
5. Assign layouts via the dashboard (click tray icon)

## Building from Source

See [`build/README.md`](build/README.md) for full instructions.

```powershell
# Debug build
.\build\build.ps1

# Release build
.\build\build.ps1 -Config Release

# Release + portable zip
.\build\build.ps1 -Config Release -Release

# Release + portable zip + Windows installer (NSIS)
.\build\build.ps1 -Config Release -Release -Installer

# Run tests
.\build\test.ps1

# Health check
.\build\check.ps1
```

## CLI

```bash
SwitchcraftKeys --help              # Show help
SwitchcraftKeys --version           # Show version
SwitchcraftKeys --check             # Run health check
SwitchcraftKeys --reset-cache       # Clear device layout cache
SwitchcraftKeys --reset-data        # Reset all app data (config, cache, logs)
```

## Project Structure

```
SwitchcraftKeys/
├── docs/          Documentation and design decisions
├── build/         Build scripts
├── dist/          Published executables (git-ignored)
└── src/
    ├── SwitchcraftKeys/        Main application
    └── SwitchcraftKeys.Tests/  Unit tests
```

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Specification](docs/SPECIFICATION.md)
- [Design Decisions](docs/DESIGN_DECISIONS.md)
- [Windows Interop](docs/WINDOWS_INTEROP.md)
- [UX Design](docs/UX_DESIGN.md)
- [Implementation Plan](docs/IMPLEMENTATION_PLAN.md)

## License

[MIT](LICENSE) — Marcelo Frau, 2026
