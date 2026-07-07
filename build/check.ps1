#!/usr/bin/env pwsh
<#
.SYNOPSIS
Health check script for SwitchcraftKeys.

.DESCRIPTION
Runs diagnostics on the SwitchcraftKeys installation, checking configuration,
cache, logs, and runtime environment.

.EXAMPLE
.\build\check.ps1
#>

param()

$root = Split-Path -Parent $PSScriptRoot
$exePath = Join-Path $root "dist\SwitchcraftKeys.exe"

if (-not (Test-Path $exePath)) {
    Write-Warning "SwitchcraftKeys.exe not found at: $exePath"
    Write-Host "Building project first..."
    & "C:\Program Files\dotnet\dotnet.exe" publish `
        -c Release `
        -o (Join-Path $root "dist") `
        (Join-Path $root "src\SwitchcraftKeys\SwitchcraftKeys.csproj")
    
    if (-not (Test-Path $exePath)) {
        Write-Error "Build failed or executable not created"
        exit 1
    }
}

Write-Host "Running health check..." -ForegroundColor Green
& $exePath --check
