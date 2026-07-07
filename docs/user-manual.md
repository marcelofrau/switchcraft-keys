---
layout: default
title: User Manual
description: Getting started with SwitchcraftKeys - installation, configuration, and daily usage
---

# User Manual

This guide covers installation, first run, and daily usage of SwitchcraftKeys.

## Installation

### Option 1: Portable (Recommended)

1. Download `SwitchcraftKeys-vX.X.X-win-x64.zip` from [GitHub Releases](https://github.com/marcelofrau/switchcraft-keys/releases)
2. Extract to any folder (e.g., `C:\Tools\SwitchcraftKeys\`)
3. Run `SwitchcraftKeys.exe`
4. The app starts in the system tray

### Option 2: Windows Installer

1. Download `SwitchcraftKeys-vX.X.X-win-x64-setup.exe`
2. Run the installer
3. Choose installation directory (default: `C:\Program Files\SwitchcraftKeys`)
4. Launch from Start Menu or desktop shortcut

## First Run

When you first launch SwitchcraftKeys:

1. **Tray Icon appears** — Look for the keyboard icon in your system tray
2. **No keyboards registered yet** — The app waits for you to type
3. **Type on each keyboard** — Press any key on each physical keyboard
4. **Devices auto-detected** — Each keyboard gets a unique ID

### Device Identification

| Device Type | ID Format | Example |
|-------------|-----------|---------|
| USB Keyboard | `VID_XXXX&PID_XXXX` | `VID_046D&PID_C31C` |
| Built-in | `BUILTIN` | `BUILTIN` |

## Dashboard

Click the tray icon to open the dashboard.

### Device List

Each detected keyboard shows:
- **Device ID** — Unique identifier (VID:PID or BUILTIN)
- **Alias** — Friendly name you can edit
- **Assigned Layout** — Dropdown to select layout

### Assigning Layouts

1. Find the keyboard in the list
2. Click the layout dropdown
3. Select the desired layout (e.g., "English (US)", "Portuguese (Brazil)")
4. Settings save automatically

### Renaming Devices

1. Click the alias field next to a device
2. Type a friendly name (e.g., "Work Keyboard", "Laptop")
3. Press Enter to save

## System Tray

Right-click the tray icon for quick actions:

| Action | Description |
|--------|-------------|
| **Open Dashboard** | Show the main window |
| **Current Layout** | Displays active layout |
| **Minimize** | Hide to tray |
| **Exit** | Close the application |

## Settings

### Reset Cache

Clears cached layout data. Use if layout detection seems incorrect.

1. Open Dashboard
2. Click "Reset Cache" button
3. Or use CLI: `SwitchcraftKeys --reset-cache`

### Reset All Data

Removes all configuration, cache, and logs. Fresh start.

1. Open Dashboard
2. Click "Reset Data" button
3. Confirm the action
4. Or use CLI: `SwitchcraftKeys --reset-data`

## Auto-Start

To run SwitchcraftKeys at Windows startup:

### Option 1: Startup Folder

1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Create a shortcut to `SwitchcraftKeys.exe` in this folder

### Option 2: Task Scheduler

1. Open Task Scheduler
2. Create Basic Task
3. Trigger: "At log on"
4. Action: Start `SwitchcraftKeys.exe`
5. Check "Run with highest privileges" if needed

## Tips

### Multiple Keyboards

- Each USB keyboard gets a unique VID:PID based on its hardware
- If you plug the same keyboard into a different USB port, it keeps its ID
- Different keyboards of the same model have the same VID:PID

### Built-in Keyboards

- Notebook keyboards typically appear as `BUILTIN`
- Some docking station keyboards may also appear as BUILTIN
- Only one BUILTIN device is tracked

### Layout Not Switching?

1. Check the [Troubleshooting](troubleshooting) guide
2. Open the Debug Overlay to see real-time events
3. Verify the layout is installed in Windows Settings → Time & Language → Language

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Esc` | Close dashboard (minimize to tray) |
| `Ctrl+D` | Toggle debug overlay |

---

Next: [Troubleshooting](troubleshooting) | [Architecture](architecture)
