# Changelog

All notable changes to SwitchcraftKeys will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v0.1.0] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [v0.1.1] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [v0.1.0] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [Unreleased]

### Added
- (Placeholder for development changes)

### Changed
- (Placeholder for development changes)

### Fixed
- (Placeholder for development changes)

---

## [v0.1.0] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [v0.1.1] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [v0.1.0] - 2026-07-01

### Added
- 

### Changed
- 

### Fixed
- 

## [0.1.0] - 2026-07-01

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

## Phase roadmap

**Phase 1 (v0.1.0)**: MVP complete. Focuses on core functionality: device detection, layout switching, basic UI.

**Phase 2 (v0.2.0)**: Enhanced features. Candidates: global hotkeys, per-app layouts, settings UI, dark mode.

**Phase 3 (v0.3.0+)**: Cross-platform and advanced. Candidates: macOS/Linux support, key remapping, macro recording.

---

**Maintained by**: Marcelo Frau <marcelofrau@gmail.com>  
**License**: MIT  
**Repository**: https://github.com/fraumar/switchcraft-keys (coming soon)
