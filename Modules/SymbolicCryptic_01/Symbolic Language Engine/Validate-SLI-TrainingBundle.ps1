[CmdletBinding()]
param(
    [string]$BundlePath,
    [string]$SpecPath,
    [string]$OutDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($BundlePath)) { $BundlePath = "D:\OAN Tech Stack\datasets\samples\sli_training_bundle.sample.v0.1.0.json" }
if ([string]::IsNullOrWhiteSpace($SpecPath)) { $SpecPath = "D:\OAN Tech Stack\datasets\specs\sli_training_bundle.v0.1.0.json" }
if ([string]::IsNullOrWhiteSpace($OutDir)) { $OutDir = Join-Path $PSScriptRoot "telemetry" }
if (-not (Test-Path -Path $OutDir -PathType Container)) { New-Item -ItemType Directory -Path $OutDir | Out-Null }

$bundle = Get-Content -Raw -Encoding utf8 $BundlePath | ConvertFrom-Json
$spec = Get-Content -Raw -Encoding utf8 $SpecPath | ConvertFrom-Json
$reserved = Get-Content -Raw -Encoding utf8 (Join-Path $PSScriptRoot "ReservedIndex.json") | ConvertFrom-Json
$reservedSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($t in @($reserved.reserved_tokens)) { [void]$reservedSet.Add([string]$t) }

$failures = New-Object System.Collections.Generic.List[string]
if ([string]$spec.schema -ne "sli.training_bundle.v0.1.0") { $failures.Add("Invalid training spec schema") }
if (-not ($bundle.PSObject.Properties.Name -contains "id") -or [string]::IsNullOrWhiteSpace([string]$bundle.id)) { $failures.Add("Missing id") }
if (-not ($bundle.PSObject.Properties.Name -contains "version") -or [string]::IsNullOrWhiteSpace([string]$bundle.version)) { $failures.Add("Missing version") }
if (-not ($bundle.PSObject.Properties.Name -contains "pairs")) { $failures.Add("Missing pairs") }

$serial = ($bundle.pairs | ConvertTo-Json -Depth 20 -Compress)
$sha = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($serial)
$hash = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace("-", "").ToLowerInvariant()

if ($bundle.PSObject.Properties.Name -contains "determinism_hash") {
    $current = [string]$bundle.determinism_hash
    if (-not [string]::IsNullOrWhiteSpace($current) -and $current -ne $hash) { $failures.Add("Determinism hash mismatch") }
}

foreach ($token in @("schema", "id", "version", "pairs")) {
    if ($reservedSet.Contains($token)) { $failures.Add("Reserved discipline violated by field name: $token") }
}

$result = [ordered]@{
    schema = "sli.training_gate.v0.1.0"
    pass = ($failures.Count -eq 0)
    determinism_hash = $hash
    failures = @($failures)
}
$out = Join-Path $OutDir "training_gate_report.json"
$result | ConvertTo-Json -Depth 10 | Set-Content -Encoding utf8 $out
Write-Host "Wrote training gate report: $out"
if ($failures.Count -gt 0) { foreach ($f in $failures) { Write-Error $f }; exit 1 }
exit 0
