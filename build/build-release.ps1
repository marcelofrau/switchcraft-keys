param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$Arch = "x64",
    [string]$Project = "SwitchcraftKeys",
    [string]$OutputDir = "dist",
    [switch]$Installer
)

# Strip leading v prefix if present
$Version = $Version -replace '^v', ''

$root = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $root "src" $Project
$dist = Join-Path $root $OutputDir
$rid = "win-$Arch"
$zipName = "SwitchcraftKeys-v$Version-$rid.zip"
$zipPath = Join-Path $dist $zipName
$publishDir = Join-Path $dist "publish"

if (-not (Test-Path $proj)) {
    Write-Error "Project not found: $proj"
    exit 1
}

Write-Host "Building SwitchcraftKeys v$Version for $rid..." -ForegroundColor Green

# Prefer dotnet on PATH, fallback to default install path
$dotnet = (Get-Command "dotnet" -ErrorAction SilentlyContinue).Source
if ([string]::IsNullOrEmpty($dotnet)) { $dotnet = "C:\Program Files\dotnet\dotnet.exe" }

# Publish (skip PublishReadyToRun for arm64 cross-compile)
$r2r = if ($Arch -eq "arm64") { "false" } else { "true" }
& $dotnet publish "$proj\SwitchcraftKeys.csproj" `
    -c Release `
    -r $rid `
    --self-contained true `
    -p:PublishReadyToRun=$r2r `
    -p:Version=$Version `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}

# Zip
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath)

Write-Host "✓ Portable release created: $zipPath" -ForegroundColor Green

# Installer (NSIS)
if ($Installer) {
    $nsiPath = Join-Path $root "installer\SwitchcraftKeys.nsi"
    if (-not (Test-Path $nsiPath)) {
        Write-Error "NSIS script not found: $nsiPath"
        exit 1
    }

    $makensis = Get-Command "makensis.exe" -ErrorAction SilentlyContinue
    if (-not $makensis) {
        # Try common paths
        $paths = @(
            "$([Environment]::GetEnvironmentVariable('ProgramFiles(x86)'))\NSIS\makensis.exe",
            "$([Environment]::GetEnvironmentVariable('ProgramFiles'))\NSIS\makensis.exe"
        )
        foreach ($p in $paths) {
            if (Test-Path $p) { $makensis = $p; break }
        }
    }

    if (-not $makensis) {
        Write-Error "NSIS (makensis.exe) not found. Install NSIS or add it to PATH."
        exit 1
    }

    Write-Host "Building installer..." -ForegroundColor Green
    & $makensis /DVERSION=$Version (Resolve-Path $nsiPath)

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Installer build failed"
        exit 1
    }

    Write-Host "✓ Installer created" -ForegroundColor Green
}

# Cleanup publish dir
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`nRelease artifacts in: $dist" -ForegroundColor Green
