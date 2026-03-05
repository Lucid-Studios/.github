[CmdletBinding()]
param(
    [switch]$StrictReservedKeyCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $PSScriptRoot "Validate-SLE.ps1"
if (-not (Test-Path -Path $scriptPath -PathType Leaf)) {
    throw "Missing validator script: $scriptPath"
}

& $scriptPath -ModulePath $PSScriptRoot -OutDir (Join-Path $PSScriptRoot "telemetry") -StrictReservedKeyCheck:$StrictReservedKeyCheck
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$scarScriptPath = Join-Path $PSScriptRoot "Generate-SCAR.ps1"
if (-not (Test-Path -Path $scarScriptPath -PathType Leaf)) {
    throw "Missing SCAR generator script: $scarScriptPath"
}

& $scarScriptPath -ModulePath $PSScriptRoot -OutDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$coverageScriptPath = Join-Path $PSScriptRoot "Test-TokenNodeCoverage.ps1"
if (-not (Test-Path -Path $coverageScriptPath -PathType Leaf)) {
    throw "Missing coverage script: $coverageScriptPath"
}
& $coverageScriptPath -OutDir (Join-Path $PSScriptRoot "telemetry") -MinCoverage 0.70
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$conformanceScriptPath = Join-Path $PSScriptRoot "Test-SCAR-Conformance.ps1"
if (-not (Test-Path -Path $conformanceScriptPath -PathType Leaf)) {
    throw "Missing conformance script: $conformanceScriptPath"
}
& $conformanceScriptPath -OutDir (Join-Path $PSScriptRoot "telemetry") -ModulePath $PSScriptRoot
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$governanceScriptPath = Join-Path $PSScriptRoot "Invoke-Governance-DryRun.ps1"
if (-not (Test-Path -Path $governanceScriptPath -PathType Leaf)) {
    throw "Missing governance dry-run script: $governanceScriptPath"
}
& $governanceScriptPath -ModulePath $PSScriptRoot -OutDir (Join-Path $PSScriptRoot "telemetry")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

exit 0
