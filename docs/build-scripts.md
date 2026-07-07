---
layout: default
title: Build Scripts
description: Build, test, package, and release scripts
---

# Build Scripts

All scripts run from the repository root with PowerShell 7+.

## Commands

| Script | Purpose |
|--------|---------|
| `build/build.ps1` | Compile app/tests |
| `build/test.ps1` | Run xUnit tests |
| `build/build-release.ps1` | Publish zip and optional installer |
| `build/check.ps1` | Run health check through built app |
| `build/clean.ps1` | Remove build outputs |

## Release Pipeline

```mermaid
flowchart LR
    Tag[vX.Y.Z tag] --> CI[GitHub Actions]
    CI --> Build[Build]
    Build --> PublishX64[Publish win-x64]
    Build --> PublishArm[Publish win-arm64]
    PublishX64 --> Zip[Portable zip]
    PublishX64 --> Installer[NSIS installer]
    PublishArm --> ZipArm[Portable zip]
    Zip --> Release[GitHub Release]
    Installer --> Release
    ZipArm --> Release
```

## Local Release

```powershell
./build/build.ps1 -Config Release -Release
./build/build.ps1 -Config Release -Release -Installer
```
