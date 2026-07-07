---
layout: default
title: Architecture
description: System architecture, layers, data flow, and service interfaces
---

# Architecture

SwitchcraftKeys follows a layered architecture with strict dependency rules to keep UI, services, and Win32 interop testable.

## Core Principles

1. **Device-centric model** - users think in keyboards, not layout handles.
2. **Deterministic behavior** - no hidden heuristics, explicit mappings only.
3. **No Win32 outside Interop** - P/Invoke stays isolated.
4. **Views know only ViewModels** - no business logic in code-behind.
5. **Services depend on interfaces** - testable through mocks.
6. **Graceful degradation** - log and recover where possible.

## Layer Diagram

```mermaid
flowchart TB
    subgraph UI[UI Layer]
        Views[Views / AXAML]
        VMs[ViewModels / CommunityToolkit]
    end

    subgraph Business[Business Logic]
        Interfaces[Service Interfaces]
        Services[Services]
    end

    subgraph Native[Native Boundary]
        Interop[Interop / PInvoke]
        Win32[Win32 APIs]
    end

    subgraph Data[Data Layer]
        Models[Models]
        Config[JSON Config]
    end

    Views --> VMs
    VMs --> Interfaces
    Services --> Interfaces
    Services --> Interop
    Services --> Models
    Services --> Config
    Interop --> Win32
```

## Dependency Rules

| Layer | Can Depend On | Must Not Depend On |
|-------|---------------|--------------------|
| Views | ViewModels | Services, Interop |
| ViewModels | Service interfaces, Models | Concrete services, Win32 |
| Services | Interfaces, Models, Interop | Avalonia UI types |
| Interop | Native structs/constants | App services/UI |
| Models | Nothing app-specific | UI/Services/Interop |

## Runtime Flow

```mermaid
sequenceDiagram
    participant Program
    participant App
    participant Config as ConfigService
    participant Layout as LayoutService
    participant Device as DeviceService
    participant VM as MainViewModel
    participant UI as MainWindow

    Program->>Config: Load app config
    Program->>App: Configure services
    App->>Layout: GetAvailableLayouts()
    App->>VM: Create ViewModel
    App->>UI: Create MainWindow
    UI->>Device: StartMonitoringAsync(hwnd)
    Device->>UI: WM_INPUT events
    UI->>Device: ProcessRawInput(lParam)
    Device->>VM: DeviceActivated
    VM->>Config: GetMappedLayoutKlid(deviceId)
    VM->>Layout: SwitchLayoutAsync(klid)
```

## Device Activation Flow

```mermaid
flowchart LR
    Key[Keystroke] --> Raw[WM_INPUT]
    Raw --> Device[Resolve hDevice]
    Device --> Id[Normalize device ID]
    Id --> Map[Lookup config mapping]
    Map --> Layout[Resolve loaded HKL]
    Layout --> Focus[Send layout request to focused window]
    Focus --> Toast[Show in-app toast]
```

## Service Interfaces

```csharp
public interface IDeviceService
{
    event EventHandler<DeviceActivatedEventArgs> DeviceActivated;
    IReadOnlyList<DeviceInfo> GetConnectedDevices();
    Task StartMonitoringAsync(IntPtr windowHandle);
    void StopMonitoring();
}

public interface ILayoutService
{
    IReadOnlyList<LayoutInfo> GetAvailableLayouts();
    LayoutInfo? GetCurrentLayout();
    Task<bool> SwitchLayoutAsync(string klid);
}
```

## Error Handling

```mermaid
flowchart TD
    Operation[Service operation] --> Ok{Succeeded?}
    Ok -->|Yes| LogInfo[Info log]
    Ok -->|No, recoverable| Warn[Warn/Error log]
    Warn --> Recover[Fallback or notify]
    Ok -->|No, fatal| Critical[Critical log]
    Critical --> Exit[Controlled shutdown]
```
