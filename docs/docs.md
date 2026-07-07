---
layout: default
title: Documentation
description: Complete documentation index for SwitchcraftKeys
---

# Documentation

Welcome to the SwitchcraftKeys documentation. This guide covers everything from basic usage to advanced architecture details.

## Quick Links

| Section | Description |
|---------|-------------|
| [User Manual](user-manual) | Getting started, basic usage, keyboard management |
| [Troubleshooting](troubleshooting) | Common issues and solutions |
| [Architecture](architecture) | System design, layers, data flow |
| [CLI Reference](cli-reference) | Command-line options and automation |

## User Guide

- **[User Manual](user-manual)** — Installation, first run, managing keyboards and layouts
- **[Troubleshooting](troubleshooting)** — Common problems and how to fix them

## Technical Overview

- **[Requirements](requirements)** — System requirements and dependencies
- **[Architecture](architecture)** — Layered architecture, service interfaces, data flow
- **[Windows Interop](windows-interop)** — Raw Input API, HKL, registry layout reader
- **[Design Decisions](design-decisions)** — Why Avalonia, why NSIS, stack choices

## How It Works

- **[Raw Input API](raw-input)** — Device detection via Windows Raw Input
- **[Device Detection](device-detection)** — VID:PID extraction, BUILTIN detection
- **[Layout Switching](layout-switching)** — HKL activation, retry logic, verification
- **[Config & Backup](config-persistence)** — JSON config, 3-version backup rotation

## Development

- **[Implementation Plan](implementation-plan)** — Phase breakdown, progress tracking
- **[CLI Reference](cli-reference)** — All command-line options
- **[Build Scripts](build-scripts)** — PowerShell build, test, publish scripts
- **[Assets Guide](assets-guide)** — Icon management, Luna theme assets
- **[Roadmap](roadmap)** — Future features and planned improvements

## Other

- **[Attributions](attributions)** — Third-party licenses and credits
- **[Changelog](changelog)** — Version history and release notes

---

## Project Status

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 0 | ✅ Complete | Scaffolding, build scripts, project setup |
| Phase 0.5 | ✅ Complete | Logging, config foundation |
| Phase 1 | ✅ Complete | Interop, core logic, device normalization |
| Phase 2 | ✅ Complete | Services, config persistence, single instance |
| Phase 3 | 🚧 In Progress | UI Dashboard, Luna theme |
| Phase 4 | ⏳ Planned | Docs polish, release prep |

## Getting Help

- **GitHub Issues**: [Report bugs or request features](https://github.com/marcelofrau/switchcraft-keys/issues)
- **GitHub Discussions**: [Ask questions, share ideas](https://github.com/marcelofrau/switchcraft-keys/discussions)
- **Email**: [marcelofrau@gmail.com](mailto:marcelofrau@gmail.com)
