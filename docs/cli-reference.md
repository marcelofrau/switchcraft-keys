---
layout: default
title: CLI Reference
description: SwitchcraftKeys command-line options
---

# CLI Reference

SwitchcraftKeys is a GUI app, but supports diagnostic CLI flags.

## Options

| Option | Action |
|--------|--------|
| `--help`, `-h`, `-?` | Show help |
| `--version`, `-v` | Print version |
| `--check` | Run health check |
| `--reset-cache`, `-c` | Clear cache |
| `--reset-data`, `-r` | Reset config, cache, and logs |
| `--console` | Force-open a diagnostics console for GUI logging |

## Flow

```mermaid
flowchart TD
    Start[SwitchcraftKeys.exe args] --> Console{--console?}
    Console -->|Yes| Alloc[Allocate console]
    Console -->|No| Cli
    Alloc --> Cli{CLI command?}
    Cli -->|--check| Health[Run health check]
    Cli -->|--reset-cache| Cache[Clear cache]
    Cli -->|--reset-data| Data[Confirm + reset data]
    Cli -->|none| Gui[Start Avalonia GUI]
```

## Examples

```powershell
SwitchcraftKeys --check
SwitchcraftKeys --console
SwitchcraftKeys --reset-cache
```
