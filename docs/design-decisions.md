---
layout: default
title: Design Decisions
description: Key product and architecture decisions behind SwitchcraftKeys
---

# Design Decisions

These decisions keep the app small, deterministic, and testable.

## Summary

| Area | Decision |
|------|----------|
| Device identity | USB uses `VID_XXXX&PID_XXXX`; built-in uses `BUILTIN` |
| Detection | Raw Input, not polling or keyboard hooks |
| Layout source | Loaded layouts from `GetKeyboardLayoutList()` |
| Persistence | JSON in `%APPDATA%\SwitchcraftKeys` |
| UI model | Tray-first dashboard |
| Native calls | P/Invoke isolated under `Interop/` |

## Why Raw Input

```mermaid
flowchart TD
    Need[Need physical keyboard identity] --> Hook{Keyboard hook?}
    Hook -->|No hDevice| Reject[Reject]
    Need --> Poll{Device polling?}
    Poll -->|Latency + heuristics| Reject
    Need --> RawInput[Raw Input API]
    RawInput --> DeviceHandle[hDevice per event]
    DeviceHandle --> StableId[Normalize stable device ID]
```

## Why Loaded Layouts Only

The registry lists every layout Windows knows about. Users can only switch to layouts installed/enabled in their profile/session.

```mermaid
flowchart LR
    Registry[Registry catalog] --> Many[Many possible KLIDs]
    Loaded[GetKeyboardLayoutList] --> Installed[Enabled user layouts]
    Installed --> UI[Layout ComboBox]
    Many -.not offered.-> UI
```

## Why Tray-First

SwitchcraftKeys runs in the background and reacts to keystrokes. Dashboard is configuration, not primary daily workflow.

```mermaid
stateDiagram-v2
    [*] --> Tray
    Tray --> Dashboard: click tray icon
    Dashboard --> Tray: minimize / close to tray
    Dashboard --> Exit: explicit Exit
    Tray --> Exit: tray menu Exit
```
