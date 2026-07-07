---
layout: default
title: Troubleshooting
description: Common issues and solutions for SwitchcraftKeys
---

# Troubleshooting

This guide covers common issues and their solutions.

## Quick Diagnostics

Run the built-in health check:

```bash
SwitchcraftKeys --check
```

This verifies:
- Config file integrity
- Cache directory access
- Log directory access
- .NET runtime version
- Windows version compatibility

## Common Issues

### Keyboard Not Detected

**Symptoms:**
- Keyboard doesn't appear in the device list
- No layout switch when typing

**Solutions:**

1. **Type on the keyboard** — Devices are detected on first keystroke
2. **Check USB connection** — Try a different port
3. **Run as Administrator** — Some keyboards require elevated privileges
4. **Check Device Manager** — Verify the keyboard is recognized by Windows

### Layout Doesn't Switch

**Symptoms:**
- Keyboard detected, but layout stays the same
- Wrong layout after switching keyboards

**Solutions:**

1. **Verify layout is installed:**
   - Settings → Time & Language → Language → Add a language
   - The layout KLID must exist in Windows

2. **Check assigned layout:**
   - Open Dashboard
   - Verify correct layout is selected for the device

3. **Clear cache:**
   ```bash
   SwitchcraftKeys --reset-cache
   ```

4. **Check debug overlay:**
   - Open Dashboard → Toggle Debug
   - Watch for errors in real-time event log

### "Already Running" Error

**Symptoms:**
- App won't start
- Message about another instance

**Solution:**

```powershell
# Kill all instances
taskkill /IM SwitchcraftKeys.exe /F

# Restart
SwitchcraftKeys.exe
```

### Config Corrupted

**Symptoms:**
- Settings lost after restart
- Error messages about JSON parsing

**Solutions:**

1. **Automatic recovery** — App tries backup files automatically
2. **Manual reset:**
   ```bash
   SwitchcraftKeys --reset-data
   ```

3. **Check backup files:**
   - Location: `%APPDATA%\SwitchcraftKeys\`
   - Files: `config.json`, `config.json.bak1`, `.bak2`, `.bak3`

### High CPU Usage

**Symptoms:**
- SwitchcraftKeys using more than 1% CPU at idle

**Solutions:**

1. **Check for stuck events** — Open Debug Overlay
2. **Restart the application**
3. **Report issue** with log files from `%APPDATA%\SwitchcraftKeys\logs\`

### Tray Icon Missing

**Symptoms:**
- App running but no tray icon visible

**Solutions:**

1. **Check hidden icons** — Click the ^ arrow in the system tray
2. **Customize tray icons:**
   - Settings → Personalization → Taskbar → System tray icons
3. **Restart Explorer:**
   ```powershell
   Stop-Process -Name explorer -Force; Start-Process explorer
   ```

## Error Messages

### "RuntimeIdentifier not recognized"

**.NET SDK not installed or outdated.**

```powershell
# Check version
dotnet --version

# Should show 8.0.xxx or higher
```

Download: [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### "Access Denied" on Config

**Permissions issue with AppData folder.**

1. Check folder permissions: `%APPDATA%\SwitchcraftKeys\`
2. Run as Administrator once to create folders
3. Or manually create the folder with your user account

### "Layout KLID not found"

**The keyboard layout isn't installed in Windows.**

1. Note the KLID from the error (e.g., `00000409`)
2. Install the corresponding language pack:
   - Settings → Time & Language → Language
   - Add a language → Select the language with that layout

Common KLIDs:
| KLID | Layout |
|------|--------|
| `00000409` | English (US) |
| `00000416` | Portuguese (Brazil) |
| `00000809` | English (UK) |
| `0000040C` | French |
| `00000407` | German |

## Debug Mode

### Enable Debug Overlay

1. Open Dashboard
2. Click "Debug" button or press `Ctrl+D`
3. A floating window shows real-time events

### Debug Overlay Shows:

- **Device Events** — Keyboard detection, hDevice values
- **Layout Switches** — KLID, HKL, timing
- **Errors** — Win32 error codes, exceptions
- **Config Events** — Load, save, backup operations

### Log Files

Location: `%APPDATA%\SwitchcraftKeys\logs\`

Log levels:
- **Trace** — All events (very verbose)
- **Debug** — Intermediate values
- **Info** — Normal operations
- **Warn** — Recoverable issues
- **Error** — Failures

To view logs:
```powershell
Get-Content "$env:APPDATA\SwitchcraftKeys\logs\log-*.txt" | Select-Object -Last 100
```

## Reset Options

### Reset Cache Only

Clears cached layout data. Keeps device mappings.

```bash
SwitchcraftKeys --reset-cache
```

### Reset All Data

Removes everything. Complete fresh start.

```bash
SwitchcraftKeys --reset-data
```

This deletes:
- `config.json` and all backups
- Cache directory
- Log files

## Reporting Issues

When reporting a bug, include:

1. **Windows version**: `winver`
2. **SwitchcraftKeys version**: `SwitchcraftKeys --version`
3. **Health check output**: `SwitchcraftKeys --check`
4. **Log files** from `%APPDATA%\SwitchcraftKeys\logs\`
5. **Steps to reproduce** the issue

Submit issues: [GitHub Issues](https://github.com/marcelofrau/switchcraft-keys/issues)

---

Previous: [User Manual](user-manual) | Next: [Architecture](architecture)
