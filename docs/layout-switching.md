---
layout: default
title: Layout Switching
description: How configured device layouts are applied to Windows
---

# Layout Switching

When a keyboard becomes active, SwitchcraftKeys applies the configured layout to the focused Windows app.

## Switching Pipeline

```mermaid
sequenceDiagram
    participant Device as DeviceService
    participant VM as MainViewModel
    participant Config as ConfigService
    participant Layout as LayoutService
    participant Win as Windows foreground app

    Device->>VM: DeviceActivated(deviceId)
    VM->>Config: GetMappedLayoutKlid(deviceId)
    Config-->>VM: klid
    VM->>Layout: SwitchLayoutAsync(klid)
    Layout->>Layout: Resolve loaded HKL
    Layout->>Win: WM_INPUTLANGCHANGEREQUEST(hkl)
    Layout-->>VM: success/failure
```

## Loaded HKL First

`GetKeyboardLayoutList()` returns layouts enabled in the current Windows session. SwitchcraftKeys uses those HKLs directly, because variant HKLs such as `040A0C0A` may not map 1:1 with registry KLID names.

## Verification

```mermaid
flowchart LR
    Switch[Post language change] --> Poll1[Verify attempt 1]
    Poll1 --> Ok{Expected HKL?}
    Ok -->|Yes| Success
    Ok -->|No| Poll2[Retry after 100ms]
    Poll2 --> Poll3[Retry after 100ms]
    Poll3 --> Failure[Warn + toast]
```
