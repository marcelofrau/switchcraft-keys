---
layout: default
title: Icon Set Sync
description: Local icon set sync process
---

# Icon Set Sync - SwitchcraftKeys

The developer's personal Icons8-derived icon set lives outside this repository at:

```
D:\workspace\_non_work_\icons8-personal-set\
```

Icons are **not committed** to this repo. They are copied in on demand
(renamed per convention) via the `asset-manager` skill. Only the final
renamed PNGs under `Assets/Views/` are tracked in git.

---

## Integration Options

### Option A: Local Path (current - development only)

Icons are consumed directly from the local path above.
No setup required on the developer's machine.

**Limitation:** Other contributors need their own copy of the personal set
at the same path, or must update `ICON_SET_ROOT` in the `asset-manager` skill.

### Option B: Git Submodule (recommended if set is on GitHub)

Add the icon set as a submodule so CI and other contributors can resolve it:

```powershell
# From repo root
git submodule add <icon-repo-url> external/icons
git submodule update --init --recursive
```

Update later:
```powershell
git submodule update --remote external/icons
```

**Important:** Do not commit the icon binaries into this repo if the Icons8
license forbids redistribution. The submodule reference (pointer) is fine;
the actual PNGs stay in the submodule repo.

### Option C: Git Subtree (copies files into repo history)

Use only if license and project policy explicitly allow bundling:

```powershell
git subtree add --prefix=external/icons <icon-repo-url> main --squash
git subtree pull --prefix=external/icons <icon-repo-url> main --squash
```

---

## Checklist Before Committing Icons

- [ ] Confirm license allows redistribution and bundling
- [ ] Add entries to `docs/attributions.md` with source and license
- [ ] Icons8 attribution present in app (free tier requires it)
- [ ] No `.ico` files committed under `Assets/Views/` (only `Assets/icon.ico`)
- [ ] Filenames follow `{ViewName}-{descriptor}-{size}.png` convention
- [ ] `.csproj` wildcard glob covers `Assets\**`

---

## Adding New Icons to the Personal Set

If a needed icon is missing from the set, add it via the pipeline:

```
# 1. Download from Icons8 CDN (try 3d-fluency first, fall back to fluency)
https://img.icons8.com/3d-fluency/50/<name>.png   -> 50x50\icons8-<name>-3d-50.png
https://img.icons8.com/3d-fluency/100/<name>.png  -> 100x100\icons8-<name>-3d-100.png

# 2. Run the pipeline to generate all sizes + .ico
python process-icons.py --workers 16
```

Pipeline location: `D:\workspace\_non_work_\icons8-personal-set\process-icons.py`

Requirements: ImageMagick 7 (`magick` in PATH), optipng, Python 3.

---

## Reference

- Personal set docs: `D:\workspace\_non_work_\icons8-personal-set\README.md`
- Pipeline docs: `D:\workspace\_non_work_\icons8-personal-set\PIPELINE.md`
- Icon catalog: `D:\workspace\_non_work_\icons8-personal-set\icon-catalog.md`
- Assets guide: `docs/assets-guide.md`
- Attributions: `docs/attributions.md`
