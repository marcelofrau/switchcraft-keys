---
name: asset-manager
description: >-
  Icon and image asset manager for SwitchcraftKeys. Use when querying the
  personal Icons8-derived set, copying or renaming icons to Assets/Views/{ViewName}/,
  generating AXAML snippets, validating licenses, or updating ATTRIBUTIONS.md.
  PROACTIVE USE REQUIRED: load this skill automatically before writing any Image
  element, any avares:// path, or any UI component that needs an icon, even if
  the user did not ask for it. Triggers include /asset-manager, add icon, find
  icon, need an image, put an icon, add image to dialog, or any intent to place
  a visual asset in AXAML.
license: MIT
metadata:
  author: Marcelo Frau
  version: "1.0"
  project: SwitchcraftKeys
---

# Skill: asset-manager

Asset helper for SwitchcraftKeys. Integrates icons from the personal Icons8-derived
set into the Avalonia project with correct naming, licensing, and documentation.

---

## Constants

```
ICON_SET_ROOT   = D:\workspace\_non_work_\icons8-personal-set
ICON_CATALOG    = {ICON_SET_ROOT}\icon-catalog-ai.md
ASSETS_ROOT     = src\SwitchcraftKeys\Assets
VIEWS_ROOT      = {ASSETS_ROOT}\Views
ATTRIBUTIONS    = docs\ATTRIBUTIONS.md
AVARES_PREFIX   = avares://SwitchcraftKeys/Assets
```

---

## Commands

### `/asset-manager query <term>`

Search the icon catalog for icons matching `<term>`.

1. Read `{ICON_CATALOG}` (AI-friendly catalog — low context).
2. Filter entries where name contains `<term>` (case-insensitive, supports `*` wildcard).
3. Show top 10 matches in a table:
   ```
   # | Icon name (no size)              | Style      | Available sizes    | License
   --|----------------------------------|------------|--------------------|------------------
   1 | icons8-keyboard-3d               | 3d-fluency | 16,32,48,50,100,128,256 | Icons8 (free+attr)
   2 | icons8-keyboard-mechanical-3d    | 3d-fluency | 16,32,48,50,100,128,256 | Icons8 (free+attr)
   3 | icons8-keyboard-2d               | fluency    | 16,32,48,50,100,128,256 | Icons8 (free+attr)
   ```
4. If zero results: suggest alternate terms.
5. Do NOT auto-add. Wait for explicit `/asset-manager add` or user confirmation.

---

### `/asset-manager add <ViewName> <icon-name> <size>`

Copy one icon from the personal set into the project.

**Parameters:**
- `ViewName` — CamelCase view name matching a folder or view file (e.g. `MainWindow`, `DebugOverlay`)
- `icon-name` — base name without size (e.g. `icons8-keyboard-3d`, `icons8-settings-3d`)
- `size` — pixel size: `16`, `32`, `64`, `100`, `128`, `256`

**Steps:**

1. **Resolve source file (best-match, no auto-resize):**

   Available sizes in the personal set: `16`, `32`, `48`, `50`, `100`, `128`, `256`.

   **Exact match:** Check if `{ICON_SET_ROOT}\{size}x{size}\{icon-name}-{size}.png` exists.
   - If yes: use it directly.

   **No exact match:** Find nearest available sizes (one smaller, one larger).
   - Present options to user:
     ```
     ⚠️  Size {size}px not available for {icon-name}.
     Nearest options:
       A) {smaller}px — copy as-is (will appear smaller in AXAML at {size}px)
       B) {larger}px  — copy as-is (will appear larger in AXAML at {size}px)
       C) {larger}px  — resize to {size}px using ImageMagick (magick)
     Choose A/B/C:
     ```
   - Wait for user choice. **Never resize without explicit confirmation.**

   **ImageMagick resize (option C only, if user confirms):**
   ```powershell
   magick "{source}" -resize {size}x{size} -filter Lanczos "{destination}"
   ```
   Note in report: "Resized from {source_size}px using ImageMagick Lanczos."

   **If file not found at any size:** try removing style suffix (`-3d`, `-2d`) and retry.
   If still not found: report and suggest `/asset-manager query <term>`.

2. **Validate license:**
   - `icons8-*` → Icons8 free with attribution → OK, note attribution needed
   - `fluentui-*` → MIT → OK, no special requirement
   - `retro-*` → **GPL-3.0** → STOP, warn user:
     ```
     ⚠️  This icon is retro-* style (KyleBing / GPL-3.0).
     GPL-3.0 requires the entire application to be GPL-3.0 licensed.
     SwitchcraftKeys is MIT. Using this icon may create a license conflict.
     Proceed anyway? [yes/no]
     ```
     Only continue if user explicitly confirms.

3. **Resolve destination:**
   - Target dir: `{VIEWS_ROOT}\{ViewName}\`
   - Derive descriptor: strip `icons8-`, `-3d`, `-2d`, `-{size}` from icon name
     - e.g. `icons8-keyboard-mechanical-3d` → descriptor = `keyboard-mechanical`
   - Destination filename: `{ViewName}-{descriptor}-{size}.png`
     - e.g. `MainWindow-keyboard-mechanical-32.png`
   - If target dir does not exist: create it.
   - If destination file already exists: warn and ask before overwriting.

4. **Copy file:**
   ```powershell
   Copy-Item -Path "<source>" -Destination "<dest>"
   ```

5. **Update ATTRIBUTIONS.md:**
   - If `docs/ATTRIBUTIONS.md` doesn't exist: create it from the template (see "ATTRIBUTIONS template" below).
   - If icon source is Icons8 and no Icons8 section exists yet: add section.
   - Append entry under the correct source section:
     ```
     | {ViewName}-{descriptor}-{size}.png | {icon-name} | Icons8 {style} | Free with attribution |
     ```
   - Never duplicate existing entries (check before appending).

6. **Generate AXAML snippet:**
   - Emit a ready-to-paste snippet:
     ```xml
     <Image Source="avares://SwitchcraftKeys/Assets/Views/{ViewName}/{ViewName}-{descriptor}-{size}.png"
            Width="{size}" Height="{size}" />
     ```
   - If resized via ImageMagick: add comment `<!-- resized from {source_size}px -->`.
   - For tray/status-bar context (size=16): suggest adding `RenderOptions.BitmapInterpolationMode="HighQuality"`.

7. **Report:**
   ```
   ✓ Copied  → src/SwitchcraftKeys/Assets/Views/MainWindow/MainWindow-keyboard-32.png
   ✓ Updated → docs/ATTRIBUTIONS.md
   
   AXAML snippet:
   <Image Source="avares://SwitchcraftKeys/Assets/Views/MainWindow/MainWindow-keyboard-32.png"
          Width="32" Height="32" />
   ```

---

### `/asset-manager batch <ViewName> --icons <name1>,<name2>,... [--size <size>]`

Add multiple icons to one view in a single command.

- Default size: `32` (toolbar context).
- Runs the same steps as `add` for each icon in order.
- Emits one consolidated ATTRIBUTIONS update.
- Emits all AXAML snippets grouped at the end.
- Aborts the batch and reports if any icon triggers a GPL-3.0 warning — user must confirm or skip.

---

### `/asset-manager list <ViewName>`

List all icon assets already copied into a given view.

- Scans `{VIEWS_ROOT}\{ViewName}\`.
- Prints a table: filename, size, last modified.

---

### `/asset-manager help`

Print command reference.

---

## Naming Convention

```
{ViewName}-{descriptor}-{size}.png
```

| Part        | Rule                                                              | Example                      |
|-------------|-------------------------------------------------------------------|------------------------------|
| `ViewName`  | CamelCase, matches view filename without extension                | `MainWindow`, `DebugOverlay` |
| `descriptor`| kebab-case, meaningful, no redundant style suffix                 | `keyboard`, `device-connected`|
| `size`      | Pixel dimension (integer). Omit only for full-width backgrounds.  | `16`, `32`, `64`, `100`      |

**Examples:**
- `MainWindow-settings-32.png`
- `DebugOverlay-device-connected-16.png`
- `MainWindow-splash-banner.png` ← background (no size)

---

## Size Selection Guide

| Context                                   | Recommended size | Available in set |
|-------------------------------------------|-----------------|-----------------|
| Tray icon / inline status indicator       | 16              | ✓ 16x16         |
| Toolbar buttons, compact actions          | 32              | ✓ 32x32         |
| Standalone buttons, dialog icons          | 64              | ✗ → ask: 50 or 100? |
| Large indicators, empty states            | 100             | ✓ 100x100       |
| Full-width backgrounds / banners          | omit size       | n/a             |

**Gap at 64px:** No 64px files in the personal set. When 64px is needed, skill
will ask whether to use 50px (slightly smaller) or 100px (slightly larger), or
resize with ImageMagick. User decides.

When in doubt between two sizes, prefer larger — scale down in AXAML.

---

## Style Priority

1. `3d-fluency` (default — try first)
2. `fluency` (flat 2D fallback)
3. `fluentui-emoji` — only if user explicitly requests
4. `retro-*` — only if user explicitly requests; always validate GPL-3.0

---

## Icon Set Structure

```
D:\workspace\_non_work_\icons8-personal-set\
├── 16x16\          ← size=16  (exact)
├── 32x32\          ← size=32  (exact)
├── 48x48\          ← size=48  (exact)
├── 50x50\          ← size=50  (exact) | nearest-smaller for size=64
├── 100x100\        ← size=100 (exact) | nearest-larger  for size=64
├── 128x128\        ← size=128 (exact)
├── 256x256\        ← size=256 (exact)
├── ico\            ← do NOT use unless explicitly asked
├── catalog\        ← per-category .md files
├── icon-catalog.md ← full catalog with previews
└── icon-catalog-ai.md ← AI-friendly catalog (use this for queries)
```

**ImageMagick available** (`magick` in PATH). Used only when user explicitly
chooses resize option C in the size resolution flow. Resize uses Lanczos filter.

---

## Assets Structure (Project)

```
src/SwitchcraftKeys/Assets/
├── icon.ico                         ← app window icon (do not replace)
├── Themes/
│   ├── LunaTheme.axaml
│   └── LunaControls.axaml
└── Views/                           ← created on first /asset-manager add
    ├── MainWindow/
    ├── DebugOverlay/
    └── {ViewName}/                  ← created automatically on demand
```

---

## ATTRIBUTIONS Template

Used when `docs/ATTRIBUTIONS.md` does not exist:

```markdown
# Asset Attributions — SwitchcraftKeys

All third-party icons and images used in this project are listed here with their
source, license, and attribution requirement.

---

## Icons8 (3d-fluency / fluency)

**License:** Free with attribution  
**Required attribution:** "Icons by Icons8 (https://icons8.com)"  
**Source:** https://icons8.com  

| File | Icon name | Style | License |
|------|-----------|-------|---------|
```

After the Icons8 section, additional sections may be added for:
- `## FluentUI Emoji (Microsoft)` — MIT, no special requirement
- `## Twemoji (Twitter/X)` — CC-BY 4.0, credit required
- `## Retro Console Icons (KyleBing)` — GPL-3.0, ⚠️ use with caution

---

## Format Rules

- **Always PNG.** Never `.ico` except `Assets/icon.ico` (app window icon).
- Never convert `.ico` → `.png`. Always source PNG from personal set directly.
- Never use JPG, BMP, GIF, SVG, or WebP for icons.
- `.csproj` must include new assets as `<None Include=... CopyToOutputDirectory=...>` or via wildcard glob — verify if needed.

---

## Guardrails

- Never copy icons without explicit user request (`add` or `batch` command).
- Never overwrite existing files without warning.
- Always validate license before copying retro-* icons.
- Always update ATTRIBUTIONS.md when copying Icons8 or Twemoji assets.
- Keep queries read-only — no file writes during `query`.
- If personal set path not found: report `ICON_SET_ROOT not found` and stop.
