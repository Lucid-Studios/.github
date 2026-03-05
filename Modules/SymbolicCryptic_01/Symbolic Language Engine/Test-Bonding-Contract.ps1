[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$TelemetryDir,
    [string]$SidecarOutDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($TelemetryDir)) { $TelemetryDir = Join-Path $ModulePath "telemetry" }
if ([string]::IsNullOrWhiteSpace($SidecarOutDir)) { $SidecarOutDir = Join-Path $ModulePath "telemetry\governance_sidecars" }
if (-not (Test-Path -Path $TelemetryDir -PathType Container)) { New-Item -ItemType Directory -Path $TelemetryDir | Out-Null }

$oePath = Join-Path $ModulePath "Governance\oan.oe_header.v0.1.0.json"
$manifestPath = Join-Path $ModulePath "Governance\oan.role_manifest.v0.1.0.json"
$charterPath = Join-Path $ModulePath "Governance\oan.career_charter.v0.1.0.json"
$chainPath = Join-Path $ModulePath "Governance\bonding_events.sample.v0.1.0.json"
$eventSchemaPath = Join-Path $ModulePath "Governance\oan.bonding_event.v0.1.0.json"
$operatorManifestPath = Join-Path $ModulePath "Governance\oan.operator_selection_manifest.v0.1.0.json"

$oe = Get-Content -Raw -Encoding utf8 $oePath | ConvertFrom-Json
$manifest = Get-Content -Raw -Encoding utf8 $manifestPath | ConvertFrom-Json
$charter = Get-Content -Raw -Encoding utf8 $charterPath | ConvertFrom-Json
$chain = Get-Content -Raw -Encoding utf8 $chainPath | ConvertFrom-Json
$eventSchema = Get-Content -Raw -Encoding utf8 $eventSchemaPath | ConvertFrom-Json
$operatorManifest = Get-Content -Raw -Encoding utf8 $operatorManifestPath | ConvertFrom-Json

$failures = New-Object System.Collections.Generic.List[string]

# Prime/Cryptic separation
$allowedRefKeys = @("store","locator","sha256","schema","access_tier")
foreach ($r in @($oe.prime_refs)) {
    if ([string]$r.access_tier -ne "prime") { $failures.Add("Prime ref tier invalid") }
}
foreach ($r in @($oe.cryptic_refs)) {
    if ([string]$r.access_tier -ne "cryptic") { $failures.Add("Cryptic ref tier invalid") }
    foreach ($p in $r.PSObject.Properties.Name) {
        if ($allowedRefKeys -notcontains $p) { $failures.Add("Cryptic ref contains non-pointer field: $p") }
    }
}

# Role and charter alignment
$activeCareer = [string]$oe.activation_selectors.active_career_id
$activeJob = [string]$oe.activation_selectors.active_job_id
$role = @($manifest.roles | Where-Object { $_.career_id -eq $activeCareer -and $_.job_id -eq $activeJob })[0]
if ($null -eq $role) { $failures.Add("Active career/job not found in role manifest") }
$career = @($charter.careers | Where-Object { $_.career_id -eq $activeCareer })[0]
if ($null -eq $career) { $failures.Add("Active career not found in charter") }
if ($null -ne $role -and $null -ne $career) {
    if (@($career.allowed_job_classes) -notcontains [string]$role.job_class) { $failures.Add("Role job_class not allowed by charter") }
    foreach ($t in @($oe.activation_selectors.enabled_trades)) {
        if (@($role.allowed_trades) -notcontains [string]$t) { $failures.Add("Enabled trade not allowed by role: $t") }
        if (@($career.allowed_trades) -notcontains [string]$t) { $failures.Add("Enabled trade not allowed by charter: $t") }
    }
}

# Operator profile reference and denial code catalog
if (-not ($oe.PSObject.Properties.Name -contains "operator_profile_ref")) {
    $failures.Add("OE header missing operator_profile_ref")
}
elseif ([string]$oe.operator_profile_ref.schema -ne "oan.operator_selection_manifest.v0.1.0") {
    $failures.Add("OE operator_profile_ref schema mismatch")
}

$supportedCodes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($c in @("ROLE_NOT_ALLOWED","TRADE_NOT_ALLOWED","UNSIGNED_REPO_DENIED","SEAL_ADMISSION_REQUIRED","EXPOSURE_POLICY_WEAKENING_DENIED","TOOL_PERMISSION_OUT_OF_SCOPE")) { [void]$supportedCodes.Add($c) }
foreach ($c in @($operatorManifest.denial_reason_codes)) {
    if (-not $supportedCodes.Contains([string]$c)) { $failures.Add("Operator manifest contains unknown denial reason code: $c") }
}

# Anti-bleed
$ab = $manifest.anti_bleed
foreach ($flag in @("trades_may_not_modify_career_identity","trades_may_not_modify_career_charter","trades_may_not_modify_exposure_policy","trades_may_not_modify_oe_invariants")) {
    if (-not [bool]$ab.$flag) { $failures.Add("Anti-bleed flag false: $flag") }
}

# Bonding chain checks
function Normalize-Node {
  param([object]$n)
  if($null -eq $n){ return $null }
  if($n -is [System.Collections.IDictionary]){ $o=[ordered]@{}; foreach($k in @($n.Keys)|Sort-Object){ $o[[string]$k]=Normalize-Node $n[$k] }; return [pscustomobject]$o }
  if($n -is [pscustomobject]){ $o=[ordered]@{}; foreach($p in @($n.PSObject.Properties.Name)|Sort-Object){ $o[$p]=Normalize-Node $n.$p }; return [pscustomobject]$o }
  if($n -is [System.Collections.IEnumerable] -and -not ($n -is [string])){ $arr=@(); foreach($i in $n){ $arr += ,(Normalize-Node $i) }; return $arr }
  return $n
}
function Event-Hash {
  param([object]$evt)
  $base = [ordered]@{
    sequence = $evt.sequence
    event_id = $evt.event_id
    prev_event_hash = $evt.prev_event_hash
    oe_header_ref = $evt.oe_header_ref
    career_id = $evt.career_id
    job_id = $evt.job_id
    lease = $evt.lease
  }
  $norm = Normalize-Node $base
  $json = $norm | ConvertTo-Json -Depth 20 -Compress
  $sha=[System.Security.Cryptography.SHA256]::Create()
  $bytes=[System.Text.Encoding]::UTF8.GetBytes($json)
  return ([System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-','').ToLowerInvariant())
}

$expectedSeq = 1
$prev = "GENESIS"
foreach ($evt in @($chain.events)) {
    if ([int]$evt.sequence -ne $expectedSeq) { $failures.Add("Sequence out of order at event $($evt.event_id)") }
    if ([string]$evt.prev_event_hash -ne $prev) { $failures.Add("prev_event_hash mismatch at event $($evt.event_id)") }
    $h = Event-Hash $evt
    if ([string]$evt.event_hash -ne $h) { $failures.Add("event_hash mismatch at event $($evt.event_id)") }

    foreach ($req in @($eventSchema.lease_required)) {
        if (-not ($evt.lease.PSObject.Properties.Name -contains [string]$req)) {
            $failures.Add("Lease missing required field '$req' at event $($evt.event_id)")
        }
    }

    $prev = [string]$evt.event_hash
    $expectedSeq++
}

$sidecarVerifyPath = Join-Path $SidecarOutDir "governance_sidecar_verify.json"
if (-not (Test-Path -Path $sidecarVerifyPath -PathType Leaf)) {
    $failures.Add("Missing governance sidecar verification report")
} else {
    $sidecarVerify = Get-Content -Raw -Encoding utf8 $sidecarVerifyPath | ConvertFrom-Json
    if (-not [bool]$sidecarVerify.pass) { $failures.Add("Governance sidecar verification failed") }
}

$operatorReportPath = Join-Path $TelemetryDir "operator_selection_report.json"
if (-not (Test-Path -Path $operatorReportPath -PathType Leaf)) {
    $failures.Add("Missing operator selection report")
}
else {
    $operatorReport = Get-Content -Raw -Encoding utf8 $operatorReportPath | ConvertFrom-Json
    if (-not [bool]$operatorReport.pass) { $failures.Add("Operator selection report failed") }
    foreach ($code in @($operatorReport.denial_reason_codes)) {
        if (-not $supportedCodes.Contains([string]$code)) { $failures.Add("Unknown denial reason code emitted: $code") }
    }
}

$result = [ordered]@{
    schema = "oan.bonding_contract_report.v0.1.0"
    pass = ($failures.Count -eq 0)
    checks = [ordered]@{
      prime_cryptic_separation = ($failures | Where-Object { $_ -like "*ref*" -or $_ -like "*pointer*" } | Measure-Object).Count -eq 0
      role_charter_alignment = ($failures | Where-Object { $_ -like "*role*" -or $_ -like "*charter*" } | Measure-Object).Count -eq 0
      anti_bleed = ($failures | Where-Object { $_ -like "*Anti-bleed*" } | Measure-Object).Count -eq 0
      bonding_chain_monotonic = ($failures | Where-Object { $_ -like "*Sequence*" -or $_ -like "*prev_event_hash*" -or $_ -like "*event_hash*" } | Measure-Object).Count -eq 0
      static_sidecar_verified = ($failures | Where-Object { $_ -like "*sidecar*" } | Measure-Object).Count -eq 0
      operator_profile_ref_valid = ($failures | Where-Object { $_ -like "*operator_profile_ref*" } | Measure-Object).Count -eq 0
      operator_denial_reason_codes_valid = ($failures | Where-Object { $_ -like "*denial reason code*" -or $_ -like "*Unknown denial*" } | Measure-Object).Count -eq 0
    }
    failures = @($failures)
}

$outPath = Join-Path $TelemetryDir "bonding_contract_report.json"
$result | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $outPath
Write-Host "Wrote bonding contract report: $outPath"
if ($failures.Count -gt 0) { foreach ($f in $failures) { Write-Error $f }; exit 1 }
exit 0
