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

$sidecarGenScript = Join-Path $PSScriptRoot "New-Governance-Sidecar.ps1"
if (-not (Test-Path -Path $sidecarGenScript -PathType Leaf)) {
    throw "Missing governance sidecar generation script: $sidecarGenScript"
}
& $sidecarGenScript -ModulePath $PSScriptRoot -SidecarOutDir (Join-Path $PSScriptRoot "telemetry\\governance_sidecars")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$sidecarVerifyScript = Join-Path $PSScriptRoot "Test-Governance-Sidecar.ps1"
if (-not (Test-Path -Path $sidecarVerifyScript -PathType Leaf)) {
    throw "Missing governance sidecar verification script: $sidecarVerifyScript"
}
& $sidecarVerifyScript -ModulePath $PSScriptRoot -SidecarOutDir (Join-Path $PSScriptRoot "telemetry\\governance_sidecars")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$operatorSelectionScript = Join-Path $PSScriptRoot "Test-Operator-Selection.ps1"
if (-not (Test-Path -Path $operatorSelectionScript -PathType Leaf)) {
    throw "Missing operator selection test script: $operatorSelectionScript"
}
& $operatorSelectionScript -ModulePath $PSScriptRoot -TelemetryDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$firstBootScript = Join-Path $PSScriptRoot "Invoke-First-Boot.ps1"
if (-not (Test-Path -Path $firstBootScript -PathType Leaf)) {
    throw "Missing first boot script: $firstBootScript"
}
& $firstBootScript -ModulePath $PSScriptRoot -TelemetryDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$operatorBondingScript = Join-Path $PSScriptRoot "Test-Operator-Bonding.ps1"
if (-not (Test-Path -Path $operatorBondingScript -PathType Leaf)) {
    throw "Missing operator bonding script: $operatorBondingScript"
}
& $operatorBondingScript -ModulePath $PSScriptRoot -TelemetryDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$reentryScript = Join-Path $PSScriptRoot "Invoke-Continuous-Reentry.ps1"
if (-not (Test-Path -Path $reentryScript -PathType Leaf)) {
    throw "Missing continuous use reentry script: $reentryScript"
}
& $reentryScript -ModulePath $PSScriptRoot -TelemetryDir (Join-Path $PSScriptRoot "telemetry") -CognitionTelemetryPath (Join-Path $PSScriptRoot "telemetry\\cognition_telemetry.json") -SidecarOutDir (Join-Path $PSScriptRoot "telemetry\\governance_sidecars")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$bondingContractScript = Join-Path $PSScriptRoot "Test-Bonding-Contract.ps1"
if (-not (Test-Path -Path $bondingContractScript -PathType Leaf)) {
    throw "Missing bonding contract test script: $bondingContractScript"
}
& $bondingContractScript -ModulePath $PSScriptRoot -TelemetryDir (Join-Path $PSScriptRoot "telemetry") -SidecarOutDir (Join-Path $PSScriptRoot "telemetry\\governance_sidecars")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

exit 0
