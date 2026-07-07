# Changelog

All notable changes to SwitchcraftKeys will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

---

## [v0.9.0] - 2026-07-07

### Added
- CLI parameters: `--help`, `--version`, `--check`, `--reset-cache`, `--reset-data`
- Health check system (PreFlightChecker) with config/cache/log validation
- Settings commands: `ResetCacheCommand`, `ResetDataCommand` (UI integration ready)
- Build scripts for portable release (zip) and Windows installer (NSIS)
- GitHub Actions pipeline for CI/CD (Windows only, x64 + arm64 support)
- NSIS installer script with wizard UI and uninstall support
- Jekyll documentation site with Luna/Windows XP-inspired theme
- Tray icon integration with minimize-to-tray and restore menu
- Custom in-app toast notifications for active keyboard/layout changes
- Settings UI actions for app folders, cache reset, data reset, restart, and Windows keyboard settings
- Windows input method scope controls for shared vs per-app input method behavior
- Layout/language ComboBoxes populated from installed Windows layouts

### Changed
- Logging system: console sink only attaches to existing console (never allocates)
- Layout switching now targets the focused Windows app/thread using the loaded HKL when available
- README header now links to website, docs, releases, and issues

### Fixed
- Persist keyboard settings without deselecting the active row
- Restore saved layout selections reliably when reopening keyboard details
- Resolve friendly names for variant HKLs such as `040A0C0A`
- Improve toast placement on high-DPI and multi-monitor setups

---

## [v0.1.0] - 2026-07-01

### Added
- **Phase 1 MVP Release**
- USB keyboard enumeration via Raw Input API (VID:PID identification)
- Built-in keyboard detection (BUILTIN special ID)
- Per-device keyboard layout mapping with persistent JSON config
- Auto-add unknown devices on first keystroke
- Keystroke-driven device activation (no polling)
- Windows API layout switching with retry verification
- Tray icon integration (minimize/restore via tray click)
- Dashboard UI with Luna/Watercolor theme (light mode)
- Device list management (rename, layout assignment, remove stale)
- Config backup rotation (3 versions, auto-recovery on corruption)
- Event logging with rotating file output
- Debug overlay window (always-on-top, shows device/layout state + event log)
- Single instance enforcement (named mutex `Global\SwitchcraftKeys`)
- P/Invoke interop layer (all Win32 calls isolated in `Interop/`)
- Comprehensive documentation (SPECIFICATION, ARCHITECTURE, DESIGN_DECISIONS, etc.)
- Unit tests for device ID normalization and config persistence
- OpenCode integration with C# LSP (csharp-ls)

### Not Included (Phase 2+)
- Global hotkey support
- Per-application layout switching
- Bluetooth keyboard enhancements
- Dark mode theme
- Background service (non-UI) mode
- Per-workspace profiles
- Autostart on login
- System-wide admin configuration

---

## Phase Roadmap

**Phase 1 (v0.1.0)**: MVP complete. Core functionality: device detection, layout switching, basic UI.

**Phase 2 (v0.2.0)**: Enhanced features. Candidates: global hotkeys, per-app layouts, settings UI, dark mode.

**Phase 3 (v0.3.0+)**: Cross-platform and advanced. Candidates: macOS/Linux support, key remapping, macro recording.

---

**Maintained by**: Marcelo Frau <marcelofrau@gmail.com>  
**License**: MIT  
**Repository**: https://github.com/marcelofrau/switchcraft-keys
