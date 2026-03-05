[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$OutDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($OutDir)) { $OutDir = Join-Path $ModulePath "telemetry" }
if (-not (Test-Path -Path $OutDir -PathType Container)) { New-Item -ItemType Directory -Path $OutDir | Out-Null }

$contractPath = Join-Path $ModulePath "governance.boot_contract.v0.1.0.json"
if (-not (Test-Path -Path $contractPath -PathType Leaf)) {
    Write-Error "Missing governance contract: $contractPath"
    exit 1
}
$contract = Get-Content -Raw -Encoding utf8 $contractPath | ConvertFrom-Json
if ([string]$contract.schema -ne "governance.boot_contract.v0.1.0") {
    Write-Error "Invalid governance schema id."
    exit 1
}

$events = @(
    [pscustomobject]@{ step = 1; event = "seed_loaded"; state = "SEED_READY" },
    [pscustomobject]@{ step = 2; event = "boot_start"; state = "AGENTS_BOOTING" },
    [pscustomobject]@{ step = 3; event = "boot_complete"; state = "WATCHING" },
    [pscustomobject]@{ step = 4; event = "heartbeat_ok"; state = "WATCHING" }
)

$result = [ordered]@{
    schema = "governance.boot_dryrun.v0.1.0"
    deterministic = $true
    final_state = "WATCHING"
    heartbeat_timeout_seconds = [int]$contract.stall_detection.heartbeat_timeout_seconds
    max_missed_heartbeats = [int]$contract.stall_detection.max_missed_heartbeats
    watchdog_ok = $true
    duplex = [ordered]@{
        prime_summary = "Boot sequence reached WATCHING with stable heartbeat."
        cryptic_detail = "Events: seed_loaded -> boot_start -> boot_complete -> heartbeat_ok"
    }
    events = $events
}

$outPath = Join-Path $OutDir "governance_boot_dryrun.json"
$result | ConvertTo-Json -Depth 10 | Set-Content -Encoding utf8 $outPath
Write-Host "Wrote governance dry-run telemetry: $outPath"
exit 0
