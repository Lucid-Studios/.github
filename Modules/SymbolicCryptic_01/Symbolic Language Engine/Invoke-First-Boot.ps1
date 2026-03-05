[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$TelemetryDir,
    [string]$CognitionTelemetryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($TelemetryDir)) { $TelemetryDir = Join-Path $ModulePath "telemetry" }
if ([string]::IsNullOrWhiteSpace($CognitionTelemetryPath)) { $CognitionTelemetryPath = Join-Path $TelemetryDir "cognition_telemetry.json" }
if (-not (Test-Path -Path $TelemetryDir -PathType Container)) { New-Item -ItemType Directory -Path $TelemetryDir | Out-Null }

. (Join-Path $ModulePath "Governance-Founding-Helpers.ps1")

$gov = Join-Path $ModulePath "Governance"
$firstBootPath = Join-Path $gov "oan.first_boot_contract.v0.1.0.json"
$identityPath = Join-Path $gov "oan.operator_identity_record.v0.1.0.json"
$bondingPath = Join-Path $gov "oan.operator_bonding_set.v0.1.0.json"
$manifestPath = Join-Path $gov "oan.operator_selection_manifest.v0.1.0.json"
$foundingEventPath = Join-Path $gov "oan.sanctuary_founding_event.v0.1.0.json"
$authPath = Join-Path $gov "oan.authentication_chain.v0.1.0.json"
$dataPolicyPath = Join-Path $gov "oan.data_protection_policy.v0.1.0.json"

$required = @($firstBootPath,$identityPath,$bondingPath,$manifestPath,$foundingEventPath,$authPath,$dataPolicyPath)
foreach ($f in $required) {
    if (-not (Test-Path -Path $f -PathType Leaf)) { Write-Error "Missing founding input: $f"; exit 1 }
}

$firstBoot = Read-FoundingJson $firstBootPath
$identity = Read-FoundingJson $identityPath
$bonding = Read-FoundingJson $bondingPath
$manifest = Read-FoundingJson $manifestPath
$foundingEvent = Read-FoundingJson $foundingEventPath
$authChain = Read-FoundingJson $authPath
$dataPolicy = Read-FoundingJson $dataPolicyPath

$errors = New-Object System.Collections.Generic.List[string]
$codes = New-Object System.Collections.Generic.List[string]
function Add-Code { param([string]$Code, [string]$Message)
    if (-not $codes.Contains($Code)) { $codes.Add($Code) }
    $errors.Add($Message)
}

if ([string]$firstBoot.schema -ne "oan.first_boot_contract.v0.1.0") { Add-Code "FOUNDING_CONTRACT_INVALID" "Invalid first boot schema." }
if ([string]$identity.schema -ne "oan.operator_identity_record.v0.1.0") { Add-Code "FOUNDING_IDENTITY_INVALID" "Invalid operator identity schema." }
if ([string]$bonding.schema -ne "oan.operator_bonding_set.v0.1.0") { Add-Code "FOUNDING_BONDING_INVALID" "Invalid operator bonding schema." }
if ([string]$foundingEvent.schema -ne "oan.sanctuary_founding_event.v0.1.0") { Add-Code "FOUNDING_EVENT_INVALID" "Invalid founding event schema." }

$states = @($firstBoot.state_machine.states)
if (@($states) -notcontains "UNFORMED") { Add-Code "FOUNDING_UNFORMED_REQUIRED" "UNFORMED state missing from first boot contract." }
if (@($states) -notcontains "READY") { Add-Code "FOUNDING_READY_MISSING" "READY state missing from first boot contract." }

$requiredTransitions = @("UNFORMED->FOUNDING","FOUNDING->BONDED","BONDED->READY")
$transitionSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($t in @($firstBoot.state_machine.valid_transitions)) {
    if ($null -eq $t) { continue }
    [void]$transitionSet.Add(("{0}->{1}" -f ([string]$t.from), ([string]$t.to)))
}
foreach ($rt in $requiredTransitions) {
    if (-not $transitionSet.Contains($rt)) { Add-Code "FOUNDING_TRANSITION_INVALID" "Missing transition: $rt" }
}

if (-not [bool]$firstBoot.anti_downgrade.profile_constraints_monotonic) { Add-Code "POLICY_PROFILE_CONSTRAINT_WEAKENING_DENIED" "Profile constraints must be monotonic." }
if (-not [bool]$firstBoot.anti_downgrade.protection_depth_monotonic) { Add-Code "POLICY_PROTECTION_DEPTH_WEAKENING_DENIED" "Protection depth must be monotonic." }
if (-not [bool]$firstBoot.anti_downgrade.seal_admission_monotonic) { Add-Code "POLICY_SEAL_STRICTNESS_WEAKENING_DENIED" "Seal admission strictness must be monotonic." }
if (-not [bool]$firstBoot.anti_downgrade.anti_bleed_monotonic) { Add-Code "POLICY_ANTIBLEED_WEAKENING_DENIED" "Anti-bleed invariants must be monotonic." }
if (-not [bool]$firstBoot.anti_downgrade.exposure_policy_monotonic) { Add-Code "POLICY_EXPOSURE_WEAKENING_DENIED" "Exposure policy must be monotonic." }

if ([string]$authChain.mode -ne "single_actor") { Add-Code "FOUNDING_AUTH_MODE_INVALID" "v0.1 requires single_actor baseline." }
if (-not [bool]$authChain.founding_actor_required) { Add-Code "FOUNDING_AUTH_CHAIN_INVALID" "Founding actor must be required in authentication chain." }
if (-not [bool]$dataPolicy.anti_downgrade_required) { Add-Code "POLICY_PROTECTION_DEPTH_WEAKENING_DENIED" "Data protection policy must require anti-downgrade." }

if ([string]$identity.founding_actor.actor_id -ne [string]$foundingEvent.founding_actor_id) {
    Add-Code "FOUNDING_ACTOR_MISMATCH" "Founding actor mismatch between identity and founding event."
}
if ([string]$identity.bonded_operator.operator_id -ne [string]$foundingEvent.bonded_operator_id) {
    Add-Code "FOUNDING_BONDED_OPERATOR_MISMATCH" "Bonded operator mismatch between identity and founding event."
}
if (-not [bool]$foundingEvent.sanctuary_constituted) { Add-Code "FOUNDING_CONSTITUTION_INCOMPLETE" "Founding event must set sanctuary_constituted=true." }

$formationSequence = @("UNFORMED","FOUNDING","BONDED","READY")
$report = [ordered]@{
    schema = "oan.first_boot_report.v0.1.0"
    pass = ($errors.Count -eq 0)
    sanctuary_constituted = [bool]$foundingEvent.sanctuary_constituted
    runtime_ready = [bool]$foundingEvent.runtime_ready
    runtime_state = [string]$foundingEvent.runtime_state
    formation_sequence = $formationSequence
    founding_actor_id = [string]$identity.founding_actor.actor_id
    bonded_operator_id = [string]$identity.bonded_operator.operator_id
    profile_id = [string]$manifest.profile_id
    denial_reason_codes = @($codes)
    errors = @($errors)
}

$outPath = Join-Path $TelemetryDir "first_boot_report.json"
Write-FoundingJson -Path $outPath -Object $report
Merge-FoundingLayer0Telemetry -CognitionTelemetryPath $CognitionTelemetryPath -SectionName "founding" -SectionValue ([ordered]@{
    sanctuary_constituted = $report.sanctuary_constituted
    runtime_ready = $report.runtime_ready
    runtime_state = $report.runtime_state
    denial_reason_codes = $report.denial_reason_codes
})

Write-Host "Wrote first boot report: $outPath"
if (-not $report.pass) {
    foreach ($e in @($errors)) { Write-Error $e }
    exit 1
}
exit 0
