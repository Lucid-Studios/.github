[CmdletBinding()]
param(
    [string]$ModulePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
$script = Join-Path $ModulePath "Validate-SLI-TrainingBundle.ps1"
& $script
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$telemetry = Join-Path $ModulePath "telemetry\training_gate_report.json"
$data = Get-Content -Raw -Encoding utf8 $telemetry | ConvertFrom-Json
$reportPath = "D:\OAN Tech Stack\docs\audits\TRAINING_GATE_REPORT.md"
if (-not (Test-Path -Path (Split-Path -Parent $reportPath) -PathType Container)) { New-Item -ItemType Directory -Path (Split-Path -Parent $reportPath) | Out-Null }

$lines = @(
"# Training Gate Report",
"",
"- Schema: $($data.schema)",
"- Pass: $($data.pass)",
"- Determinism Hash: $($data.determinism_hash)",
"- Failures: $((@($data.failures)).Count)",
""
)
if (@($data.failures).Count -gt 0) {
  $lines += "## Failures"
  foreach ($f in @($data.failures)) { $lines += "- $f" }
} else {
  $lines += "No failures detected."
}
$lines | Set-Content -Encoding utf8 $reportPath
Write-Host "Wrote training gate markdown report: $reportPath"
exit 0
