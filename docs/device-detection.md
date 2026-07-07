---
layout: default
title: Device Detection
description: Device ID normalization rules for USB, Bluetooth, and built-in keyboards
---

# Device Detection

SwitchcraftKeys normalizes noisy Raw Input paths into stable device IDs.

## Normalization Flow

```mermaid
flowchart TD
    Path[Raw Input device path] --> Usb{Contains VID/PID?}
    Usb -->|Yes| VidPid[VID_XXXX&PID_XXXX]
    Usb -->|No| Builtin{ACPI/I8042/RDP?}
    Builtin -->|Yes| BuiltInId[BUILTIN]
    Builtin -->|No| Unknown[Ignore / log]
```

## ID Formats

| Device | ID | Example |
|--------|----|---------|
| USB keyboard | `VID_XXXX&PID_XXXX` | `VID_046D&PID_C31C` |
| Bluetooth HID | `VID_XXXX&PID_XXXX` when exposed | `VID_046D&PID_B380` |
| Built-in laptop | `BUILTIN` | `BUILTIN` |

## Trade-Offs

Two identical keyboards from the same vendor/model may share VID/PID. Future versions can include instance suffixes if needed.
