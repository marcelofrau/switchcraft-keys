# UX Design — SwitchcraftKeys

**Version**: 1.0  
**Theme**: Luna/Watercolor (light, soft gradients, rounded corners)  
**Platform**: Windows 10/11 only

> Adapted from `archive/switchcarft-keys/docs/UX_DESIGN.md` (Rust/egui).
> UI patterns informed by `switchcraft-keys-python` v0.1.1 (working reference).

---

## 1. User Mental Model

**User thinks**: "I have two keyboards. Keyboard A uses US layout, Keyboard B uses Portuguese. When I type on A, it should switch to US. When I type on B, it should switch to Portuguese."

**App does**:
1. Detects which keyboard you're typing on (Raw Input)
2. Looks up your config (device → layout mapping)
3. Switches the OS layout automatically
4. Shows current device + layout in tray tooltip

---

## 2. Key User Flows

### Flow 1: First Launch

```
User downloads switchcraft-keys-v0.1.0-win-x64.exe
  ↓
Clicks it → app starts minimized to tray (silent)
  ↓
User types on keyboard A
  ↓
App detects device, auto-adds to config with current OS layout as default
  ↓
User types on keyboard B
  ↓
App detects device, auto-adds to config with current OS layout as default
  ↓
Tray tooltip now shows: "SwitchcraftKeys — Device A: US; Device B: POR"
```

**Zero friction**: User never opens dashboard in this flow. Typing is enough.

### Flow 2: Adjust Layout Assignment

```
User clicks tray icon → dashboard opens
  ↓
Sees list: [Device A (currently using layout 1), Device B (currently using layout 2)]
  ↓
Clicks Device B row → expands or selects
  ↓
ComboBox shows available layouts
  ↓
Selects "Portuguese (ABNT)" → layout immediately switches (if Device B is active)
  ↓
Dashboard shows ✓ checkmark next to Device B
  ↓
User types on Device B again → layout stays Portuguese
```

### Flow 3: Rename Device

```
User right-clicks device row or clicks edit icon
  ↓
Inline edit appears (editable TextBox over device name)
  ↓
User types "Gaming Keyboard USB"
  ↓
Presses Enter → saved, stored in config
  ↓
Tray tooltip updates: "SwitchcraftKeys — Gaming Keyboard USB: US"
```

---

## 3. Main Window (Dashboard)

### Layout

```
┌─────────────────────────────────────────┐
│  SwitchcraftKeys                    [_][□][X]
├─────────────────────────────────────────┤
│                                         │
│  [📊 Status]  [⌨️ Devices] [⚙️ Config] [🐛 Debug]
│  ─────────────────────────────────────  │
│                                         │
│  ✓ Active device: Gaming Keyboard USB   │
│    Layout: US (00000409)                │
│                                         │
│  ⚙️ Auto-switch: ON                      │
│  🔄 Refresh devices                     │
│                                         │
└─────────────────────────────────────────┘
```

### Toolbar (top)
- Tab buttons: `Status`, `Devices`, `Config`, `Debug`
- Icons from Avalonia Fluent icon set (if available; fallback to simple ◻)
- Currently selected tab highlighted in Luna primary blue

### Status Tab
- **Current Device**: friendly name + device ID (VID:PID or BUILTIN)
- **Current Layout**: display name + KLID
- **Status**: ✓ "Connected" or ⚠ "Disconnected"
- **Auto-switch toggle**: ON/OFF
- **Refresh button**: manual device re-enumeration

### Devices Tab

```
╔═══════════════════════════════════════════════════╗
║ Devices                                     [🔄]  ║
╠═══════════════════════════════════════════════════╣
║                                                   ║
║  ✓ Gaming Keyboard USB                           ║
║    ├─ Layout: US (00000409)                       ║
║    ├─ Device ID: VID_046D&PID_C31C               ║
║    └─ [Change Layout ▼]                          ║
║                                                   ║
║  ✓ Notebook Keyboard                             ║
║    ├─ Layout: Portuguese (00000416)              ║
║    ├─ Device ID: BUILTIN                         ║
║    └─ [Change Layout ▼]                          ║
║                                                   ║
║  ⚠ Old USB Keyboard (disconnected)               ║
║    ├─ Layout: Spanish (0c0a)                     ║
║    ├─ Device ID: VID_1234&PID_5678               ║
║    └─ [Change Layout ▼] [Remove device]          ║
║                                                   ║
╚═══════════════════════════════════════════════════╝
```

**Device rows**:
- Device icon (⌨) + friendly name (editable inline on double-click)
- Status icon (✓ connected / ⚠ disconnected)
- Current layout selector (ComboBox, sorted by language)
- Device ID (small gray text, VID:PID or BUILTIN)
- Remove button (for disconnected/stale devices)

**Cascading layout selector**:
```
┌─ Language ─────┐
│ ENG            │
│ POR            │
│ SPA            │
└────────────────┘
          ↓ (on selection)
┌─ Layouts ──────────────┐
│ US (Standard)          │
│ US-International       │
│ Dvorak                 │
└────────────────────────┘
```

### Config Tab

```
╔═══════════════════════════════════════════════════╗
║ Configuration                               [💾]  ║
╠═══════════════════════════════════════════════════╣
║                                                   ║
║  ☑ Auto-switch on device change                  ║
║  ☑ Start minimized                               ║
║  ☐ Show debug panel on startup                   ║
║                                                   ║
║  Config location:                                ║
║  C:\Users\me\AppData\Roaming\SwitchcraftKeys\   ║
║  config.json                                     ║
║                                                   ║
║  ☐ Portable mode (app directory)                 ║
║                                                   ║
║  [Export config]  [Import config]                ║
║  [Reset to defaults]                             ║
║                                                   ║
╚═══════════════════════════════════════════════════╝
```

---

## 4. Debug Overlay (Phase 1 simple)

**Always-on-top floating window** (separate from dashboard, opened via toolbar button)

```
┌────────────────────────────┐
│ SwitchcraftKeys Debug [X]  │
├────────────────────────────┤
│ Current Device:            │
│ VID_046D&PID_C31C          │
│                            │
│ Device Path:               │
│ \\?\HID#VID_046D&PID_...   │
│                            │
│ Active Layout: 00000409    │
│                            │
│ Last event: [14:23:45]     │
│ Keystroke from USB device  │
│                            │
│ ─────────────────────────  │
│ Event log (last 20):       │
│                            │
│ [14:23:45] USB device      │
│            activated       │
│ [14:23:46] Layout switched │
│            to 00000409     │
│ [14:23:46] Verified: OK    │
│                            │
└────────────────────────────┘
```

---

## 5. Tray Icon

### Icon
- Keyboard symbol (⌨) in Luna primary blue
- On hover: tooltip = `SwitchcraftKeys — [Device]: [Layout]`
- Example: `SwitchcraftKeys — Gaming USB: US`

### Context Menu
```
┌──────────────────────────┐
│ Open Dashboard       [▶]  │
│ ──────────────────────── │
│ Quit                 [▶]  │
└──────────────────────────┘
```

### Click behavior
- Left click: toggle dashboard (show if hidden, hide if visible)
- Right click: context menu
- Double-click: same as left click

---

## 6. Luna/Watercolor Theme

### Color Palette

| Element | Color | Hex |
|---------|-------|-----|
| Primary (buttons, active) | Royal Blue | #1C5FA8 |
| Primary hover | Light Blue | #3A7CC8 |
| Primary pressed | Dark Blue | #144A85 |
| Secondary (accents) | Sage Green | #7B9E6E |
| Background | Watercolor white | #F0F4F8 |
| Surface (cards) | Pure white | #FFFFFF |
| Surface alt | Light blue | #EAF0F8 |
| Border | Soft blue | #A8C4E0 |
| Border focus | Primary blue | #1C5FA8 |
| Text primary | Dark blue-gray | #1A2A3A |
| Text secondary | Medium blue-gray | #4A6A8A |
| Text disabled | Light blue-gray | #9AB0C8 |

### Typography

- **Font**: Inter (Avalonia.Fonts.Inter NuGet)
- **Size scale**: 12, 14 (default), 16 (heading), 20 (title)
- **Weight**: Regular (400), Bold (700)

### Spacing & Radius

- **Small**: 4px
- **Medium**: 8px
- **Large**: 16px
- **Button radius**: 4px
- **Card radius**: 4px
- **Padding (buttons)**: 8px h × 6px v
- **Padding (cards)**: 12px all

---

## 7. Error States & Feedback

### Layout Switch Failed
```
⚠ Layout switch failed for "Gaming USB"
  Retried 3 times, still on old layout.
  [Retry] [Ignore]
```

### Config Corrupted (Recovery)
```
⚠ Config file corrupted. Restored from backup.
  Check C:\Users\me\AppData\Roaming\SwitchcraftKeys\
  [Open folder] [OK]
```

### Missing Layout
```
⚠ Portuguese layout no longer installed.
  Reassign layout for "Notebook Keyboard"?
  [Choose layout] [Cancel]
```

---

## 8. Keyboard Shortcuts (Phase 2+)

Not planned for Phase 1. Candidate Phase 2 additions:

| Shortcut | Action |
|----------|--------|
| Alt+Shift+L | Force re-apply current device's layout |
| Ctrl+T | Toggle minimize/restore tray |
| F5 | Refresh device list |

---

## 9. Animation & Transitions

**Minimal Phase 1** — focus on clarity:

- Tab switches: instant (no animation)
- Device list updates: instant
- Tray tooltip: fade-in on hover (100ms)
- Status bar: fade-in on layout change (50ms)

---

## 10. Accessibility

- All buttons: tab-navigable
- Focus indicators: visible (Luna border focus color)
- Tooltips: shown on hover 500ms (Windows standard)
- No color-only meaning (always include text + icon)
- Font size: 14pt minimum for body text
