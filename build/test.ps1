#Requires -Version 7.0
<#
.SYNOPSIS
    Executa os testes unitarios do SwitchcraftKeys.
.PARAMETER Coverage
    Gera relatorio HTML de coverage em TestResults/coverage/.
.EXAMPLE
    .\build\test.ps1
    .\build\test.ps1 -Coverage
#>
[CmdletBinding()]
param(
    [switch]$Coverage
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$TestProj = Join-Path $RepoRoot "src\SwitchcraftKeys.Tests\SwitchcraftKeys.Tests.csproj"
$ResultsDir = Join-Path $RepoRoot "TestResults"

Write-Host "==> Running tests" -ForegroundColor Cyan

New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null

if ($Coverage) {
    Write-Host "  Coverage enabled — output: TestResults/coverage/" -ForegroundColor DarkGray

    $result = dotnet test $TestProj `
        --results-directory $ResultsDir `
        --collect:"XPlat Code Coverage" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura 2>&1
} else {
    $result = dotnet test $TestProj `
        --results-directory $ResultsDir `
        --logger "console;verbosity=normal" 2>&1
}

$exitCode = $LASTEXITCODE
$result | ForEach-Object { Write-Host $_ }

if ($exitCode -ne 0) {
    Write-Host "`n[FAIL] Tests failed (exit $exitCode)" -ForegroundColor Red
    exit $exitCode
}

if ($Coverage) {
    # Generate HTML report if reportgenerator is available
    $rg = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
    if ($rg) {
        $coverageFile = Get-ChildItem $ResultsDir -Recurse -Filter "*.cobertura.xml" | Select-Object -First 1
        if ($coverageFile) {
            reportgenerator -reports:$coverageFile.FullName -targetdir:"$ResultsDir\coverage" -reporttypes:Html
            Write-Host "  Coverage report: $ResultsDir\coverage\index.html" -ForegroundColor DarkCyan
        }
    } else {
        Write-Host "  Tip: install reportgenerator for HTML reports:" -ForegroundColor DarkYellow
        Write-Host "       dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor DarkYellow
    }
}

Write-Host "`n[OK] All tests passed" -ForegroundColor Green
