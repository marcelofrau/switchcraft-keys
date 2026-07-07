# Assets Guide — SwitchcraftKeys

## Directory Structure

```
src/SwitchcraftKeys/Assets/
├── icon.ico                    ← App window icon (do not replace)
├── Themes/
│   ├── LunaTheme.axaml         ← Color tokens (Luna/Watercolor light theme)
│   └── LunaControls.axaml      ← Control style overrides
└── Views/                      ← Per-view/window icons and images
    ├── MainWindow/
    ├── DebugOverlay/
    └── {ViewName}/             ← Created on demand by asset-manager skill
```

Each view or window that needs icons gets its own subfolder under `Views/`.
View-agnostic icons shared across multiple views go in `Assets/Icons/` (see below).

---

## Naming Convention

**Pattern:** `{ViewName}-{descriptor}[-{size}].png`

| Part | Rule | Example |
|------|------|---------|
| `ViewName` | CamelCase, matches view filename without extension | `MainWindow`, `DebugOverlay` |
| `descriptor` | kebab-case, meaningful, no style suffix | `settings`, `device-connected` |
| `size` (optional) | Pixel dimension integer | `16`, `32`, `100` |

**Examples:**

- `MainWindow-settings-32.png` — settings icon in toolbar, 32px
- `MainWindow-close-app-32.png` — close/exit action, 32px
- `DebugOverlay-device-connected-16.png` — connection status indicator, 16px
- `MainWindow-splash-banner.png` — full-width banner, no size

**Rules:**
- ViewName is CamelCase; descriptor is kebab-case
- Omit size only for full-width backgrounds, banners, or logos that fill a container
- AXAML references the file via `avares://SwitchcraftKeys/Assets/Views/{ViewName}/{filename}`

---

## Icon Source: Personal Set

All PNG icons come from the developer's personal Icons8-derived collection at:

```
D:\workspace\_non_work_\icons8-personal-set\
```

The set is organized by pixel size:

```
icons8-personal-set/
├── 16x16/
├── 32x32/
├── 48x48/
├── 50x50/
├── 100x100/
├── 128x128/
├── 256x256/
├── ico/              ← .ico variants — do not use unless explicitly asked
├── catalog/          ← Per-category .md metadata files
├── icon-catalog.md   ← Full catalog with previews
└── icon-catalog-ai.md ← AI-friendly catalog (low context, use for queries)
```

**Workflow to add a new icon — use the `asset-manager` skill:**

```
/asset-manager query "keyboard"        ← find matching icons
/asset-manager add MainWindow keyboard-3d 32   ← copy + rename + document
```

Or manually:

1. Identify needed size from context (see Size Selection below)
2. Copy from `icons8-personal-set/{size}x{size}/{name}-{size}.png`
3. Rename following the convention: `{ViewName}-{descriptor}-{size}.png`
4. Place in `src/SwitchcraftKeys/Assets/Views/{ViewName}/`
5. Reference in AXAML as `avares://SwitchcraftKeys/Assets/Views/{ViewName}/{filename}`
6. Add entry to `docs/ATTRIBUTIONS.md`

---

## Format Rules

- **Always use PNG.** Never use `.ico` files inside `Assets/Views/`.
  - The sole exception is `Assets/icon.ico` (application/window icon), which must be `.ico`.
- Do not convert `.ico` files to `.png` — always source PNG from the personal set directly.
- Do not use JPG, BMP, GIF, SVG, or WebP for icons.

---

## Size Selection

Match icon size to UI context:

| Context | Recommended size | Available in personal set |
|---------|-----------------|--------------------------|
| Tray icon / inline status indicator | 16px | ✓ 16x16 |
| Toolbar buttons, compact actions | 32px | ✓ 32x32 |
| Standalone buttons, dialog icons | 64px | ✗ — use 50px or 100px (see note) |
| Large indicators, empty states | 100px | ✓ 100x100 |
| Full-width backgrounds / banners | variable | omit size in filename |

**Gap at 64px:** The personal set has no 64px files. Options when 64px is needed:
- Use `50x50/` source → render at 64px in AXAML via `Width="64" Height="64"`
- Use `100x100/` source → render at 64px in AXAML
- Resize with ImageMagick: `magick source.png -resize 64x64 -filter Lanczos dest.png`

When in doubt between two sizes, prefer the larger — scale down in AXAML.

---

## AXAML Reference Pattern

```xml
<!-- Standard icon reference -->
<Image Source="avares://SwitchcraftKeys/Assets/Views/MainWindow/MainWindow-settings-32.png"
       Width="32" Height="32" />

<!-- Small status/tray icon — add interpolation hint -->
<Image Source="avares://SwitchcraftKeys/Assets/Views/DebugOverlay/DebugOverlay-device-connected-16.png"
       Width="16" Height="16"
       RenderOptions.BitmapInterpolationMode="HighQuality" />

<!-- Full-width banner (no explicit size) -->
<Image Source="avares://SwitchcraftKeys/Assets/Views/MainWindow/MainWindow-splash-banner.png"
       Stretch="UniformToFill" />
```

---

## View-Agnostic Icons

If an icon is needed by multiple views and is not specific to any single one,
place it in `Assets/Icons/`. Currently this folder only contains `icon.ico`.

---

## Updating the .csproj

Avalonia includes assets via `avares://` when they are referenced in the `.csproj`.
Verify the project file includes a wildcard glob for Assets:

```xml
<ItemGroup>
  <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

If assets are added but the glob already covers `Assets\**`, no change is needed.

---

## Attribution

All third-party icons must be attributed in `docs/ATTRIBUTIONS.md`.
The `asset-manager` skill updates this file automatically.
See that file for current attributions and licenses.

**Quick reference:**

| Source | License | Requirement |
|--------|---------|-------------|
| Icons8 (3d-fluency / fluency) | Free with attribution | Credit "Icons by Icons8 (https://icons8.com)" |
| FluentUI Emoji (Microsoft) | MIT | No requirement |
| Twemoji (Twitter/X) | CC-BY 4.0 | Credit Twemoji |
| Retro console icons (KyleBing) | GPL-3.0 | ⚠️ Entire app must be GPL-3.0 — avoid |
