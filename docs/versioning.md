---
layout: default
title: Versioning
description: Versioning strategy and release metadata
---

# Versioning Strategy

SwitchcraftKeys adheres to **Semantic Versioning 2.0** with build metadata for commit tracking.

## Version Format

```
MAJOR.MINOR.PATCH+BUILD
```

Where:
- **MAJOR**: Breaking changes (e.g., `1.0.0`, `2.0.0`)
- **MINOR**: New features, backward-compatible (e.g., `0.2.0`, `0.3.0`)
- **PATCH**: Bug fixes, patches (e.g., `0.1.1`, `0.1.2`)
- **BUILD**: Short git commit SHA (7 chars), e.g., `0.1.0+92fb7bb`

The `+BUILD` part is **build metadata** per [SemVer 2.0 spec](https://semver.org/spec/v2.0.0.html) - it doesn't affect version precedence but uniquely identifies which commit produced the binary.

---

## Version Fields in Code

The `.csproj` file defines two separate fields:

| Field | Purpose | Example |
|-------|---------|---------|
| `<Version>` | Base SemVer (no metadata) - used for NuGet, MSBuild | `0.1.0` |
| `<InformationalVersion>` | Full version with commit SHA | `0.1.0+92fb7bb` |

The `<InformationalVersion>` is automatically injected at build time via an MSBuild target that runs `git rev-parse --short HEAD`. If git is unavailable, it defaults to `unknown`.

### Where InformationalVersion Appears

1. **Assembly Metadata**: `dotnet --version` / properties in Windows File Explorer
2. **Build Output**: Logged by `build.ps1` 
3. **Build Artifacts**: Embedded in `.exe` (visible via `Properties` -> `Details` tab in Windows)

---

## Release Workflow

### 1. During Development

```powershell
# Work normally, commits happen
git commit -m "feat: add device aliasing"
git commit -m "fix: handle disconnected USB devices"

# At next build, InformationalVersion auto-updates
.\build\build.ps1
# Output: Version: 0.1.0+2f8c9a1

# View current version anytime
.\build\version.ps1 -Show
# Output:
# Version: 0.1.0
# Build (commit): 2f8c9a1
# Informational: 0.1.0+2f8c9a1
```

### 2. Before Release (Bump Version)

```powershell
# Bump patch version (e.g., 0.1.0 -> 0.1.1)
.\build\version.ps1 -Bump patch

# Or bump minor (e.g., 0.1.0 -> 0.2.0)
.\build\version.ps1 -Bump minor

# Or set explicitly (e.g., to 1.0.0)
.\build\version.ps1 -Set 1.0.0

# Optionally create git tag
.\build\version.ps1 -Bump patch -Tag
# Creates: v0.1.1 (local tag)
```

This updates:
- `<Version>` in `.csproj`
- `CHANGELOG.md` with new `[v0.1.1]` entry

### 3. Release Build

```powershell
# Ensure clean build with new version
.\build\build.ps1 -Config Release

# Output:
# ==> Building SwitchcraftKeys [Release]
#     Version: 0.1.1+abc1234
# [OK] Build succeeded [Release] - 0.1.1+abc1234
```

---

## Changelog Integration

The script automatically updates `CHANGELOG.md` when bumping versions:

```markdown
## [Unreleased]
### Added
- 

### Changed
- 

### Fixed
- 

## [v0.1.1] - 2026-07-15
### Added
- Per-device aliases
### Fixed
- Handle USB device disconnection gracefully

## [v0.1.0] - 2026-07-01
### Added
- Initial MVP...
```

Format follows [Keep a Changelog](https://keepachangelog.com/).

---

## Git Tagging Convention

When using `.\build\version.ps1 -Bump X -Tag`, tags are created as:

```
v0.1.0
v0.1.1
v0.2.0
v1.0.0
```

**Note**: Tags are created locally only. To push:

```powershell
git push origin v0.1.0
# or push all tags
git push origin --tags
```

---

## Examples

### Check current version
```powershell
.\build\version.ps1 -Show
```

### Patch release (bug fix)
```powershell
.\build\version.ps1 -Bump patch -Tag
git commit -am "chore: bump version to 0.1.1"
git push origin main --tags
.\build\build.ps1 -Config Release
```

### Minor release (new features)
```powershell
.\build\version.ps1 -Bump minor -Tag
git commit -am "chore: bump version to 0.2.0"
.\build\build.ps1 -Config Release
```

### Manual version set
```powershell
.\build\version.ps1 -Set 1.0.0 -Tag
```

---

## MSBuild Target: GetGitCommitShort

The `.csproj` includes an MSBuild target that runs before every build:

```xml
<Target Name="GetGitCommitShort" BeforeTargets="Build">
  <Exec Command="git rev-parse --short HEAD" ConsoleToMSBuild="true" ... />
  <PropertyGroup>
    <GitCommitShort Condition="'$(GitCommitShort)' == ''">unknown</GitCommitShort>
  </PropertyGroup>
</Target>
```

This:
1. Runs `git rev-parse --short HEAD` to get the commit SHA
2. Captures it into the `$(GitCommitShort)` property
3. Falls back to `unknown` if git is unavailable
4. Injects it into `$(InformationalVersion)`

---

## Benefits

| Aspect | Benefit |
|--------|---------|
| **SemVer Base** | Clear, standard versioning; compatible with NuGet, package managers |
| **Build Metadata** | Identify exact commit without external tracking |
| **Automation** | MSBuild injects SHA automatically; no manual steps |
| **Changelog** | Each release is documented in `CHANGELOG.md` with date |
| **Git Tags** | Easy navigation: `git checkout v0.1.0` |
| **Fallback** | Works offline or without git (uses `unknown`) |

---

## References

- [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html)
- [Keep a Changelog](https://keepachangelog.com/)
- [Microsoft: AssemblyVersionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyversionattribute)

---

**Maintained by**: Marcelo Frau  
**License**: MIT  
**Last Updated**: 2026-07-01
