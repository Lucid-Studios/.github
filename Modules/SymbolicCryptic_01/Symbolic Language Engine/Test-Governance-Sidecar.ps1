[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$SidecarOutDir,
    [string]$KeyringPath,
    [string[]]$Contracts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ModulePath)) { $ModulePath = $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($SidecarOutDir)) { $SidecarOutDir = Join-Path $ModulePath "telemetry\governance_sidecars" }
if ([string]::IsNullOrWhiteSpace($KeyringPath)) { $KeyringPath = Join-Path $ModulePath "Governance\governance.keyring.v0.1.0.json" }
if (-not $Contracts -or $Contracts.Count -eq 0) {
    $Contracts = @(
      (Join-Path $ModulePath "Governance\oan.oe_header.v0.1.0.json"),
      (Join-Path $ModulePath "Governance\oan.role_manifest.v0.1.0.json"),
      (Join-Path $ModulePath "Governance\oan.career_charter.v0.1.0.json")
    )
}

function Normalize-Node {
  param([object]$n)
  if($null -eq $n){ return $null }
  if($n -is [System.Collections.IDictionary]){ $o=[ordered]@{}; foreach($k in @($n.Keys)|Sort-Object){ $o[[string]$k]=Normalize-Node $n[$k] }; return [pscustomobject]$o }
  if($n -is [pscustomobject]){ $o=[ordered]@{}; foreach($p in @($n.PSObject.Properties.Name)|Sort-Object){ $o[$p]=Normalize-Node $n.$p }; return [pscustomobject]$o }
  if($n -is [System.Collections.IEnumerable] -and -not ($n -is [string])){ $arr=@(); foreach($i in $n){ $arr += ,(Normalize-Node $i) }; return $arr }
  return $n
}
function Get-CanonicalJson {
  param([string]$Path)
  $obj = Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
  $norm = Normalize-Node $obj
  return ($norm | ConvertTo-Json -Depth 40 -Compress)
}
function Get-Sha256 {
  param([string]$Text)
  $sha=[System.Security.Cryptography.SHA256]::Create()
  $bytes=[System.Text.Encoding]::UTF8.GetBytes($Text)
  return ([System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-','').ToLowerInvariant())
}

$keyring = Get-Content -Raw -Encoding utf8 $KeyringPath | ConvertFrom-Json
$failures = New-Object System.Collections.Generic.List[string]
$checks = New-Object System.Collections.Generic.List[object]

foreach ($contract in $Contracts) {
    if (-not (Test-Path -Path $contract -PathType Leaf)) { $failures.Add("Missing contract: $contract"); continue }
    $name = [IO.Path]::GetFileNameWithoutExtension($contract)
    $hashPath = Join-Path $SidecarOutDir ("{0}.hash.json" -f $name)
    $sigPath = Join-Path $SidecarOutDir ("{0}.sig.json" -f $name)
    if (-not (Test-Path $hashPath) -or -not (Test-Path $sigPath)) { $failures.Add("Missing sidecar(s) for $name"); continue }

    $hashSidecar = Get-Content -Raw -Encoding utf8 $hashPath | ConvertFrom-Json
    $sigSidecar = Get-Content -Raw -Encoding utf8 $sigPath | ConvertFrom-Json
    $canonical = Get-CanonicalJson $contract
    $actualHash = Get-Sha256 $canonical

    if ([string]$hashSidecar.artifact_sha256 -ne $actualHash) { $failures.Add("Hash mismatch for $name") }
    if ([string]$sigSidecar.artifact_sha256 -ne $actualHash) { $failures.Add("Signature hash mismatch for $name") }

    $keyId = [string]$sigSidecar.key_id
    $key = @($keyring.keys | Where-Object { $_.key_id -eq $keyId })[0]
    if ($null -eq $key) { $failures.Add("Key id not found: $keyId for $name"); continue }

    $pub = Join-Path $ModulePath ([string]$key.public_key_relpath)
    if (-not (Test-Path -Path $pub -PathType Leaf)) { $failures.Add("Missing public key for $keyId"); continue }

    $principal = [string]$sigSidecar.principal
    $namespace = [string]$sigSidecar.namespace

    $tmp = Join-Path $env:TEMP ("govverify_{0}.json" -f ([Guid]::NewGuid().ToString("N")))
    $tmpSig = "$tmp.sig"
    $allowed = "$tmp.allowed"

    [System.IO.File]::WriteAllText($tmp, $canonical, [System.Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllBytes($tmpSig, [Convert]::FromBase64String([string]$sigSidecar.signature))

    $pubLine = (Get-Content -Raw -Encoding utf8 $pub).Trim()
    ("{0} {1}" -f $principal, $pubLine) | Set-Content -Encoding utf8 $allowed

    cmd /c "ssh-keygen -Y verify -f ""$allowed"" -I ""$principal"" -n ""$namespace"" -s ""$tmpSig"" < ""$tmp""" | Out-Null
    if ($LASTEXITCODE -ne 0) { $failures.Add("Signature verify failed for $name") }

    Remove-Item -Force $tmp,$tmpSig,$allowed -ErrorAction SilentlyContinue
    $checks.Add([pscustomobject]@{ contract = $contract; pass = ($LASTEXITCODE -eq 0); hash = $actualHash })
}

$result = [ordered]@{
  schema = "oan.sidecar.verify.v0.1.0"
  pass = ($failures.Count -eq 0)
  failures = $failures.ToArray()
  checks = $checks.ToArray()
}
$outPath = Join-Path $SidecarOutDir "governance_sidecar_verify.json"
$result | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $outPath
Write-Host "Wrote governance sidecar verification: $outPath"
if ($failures.Count -gt 0) { foreach ($f in $failures) { Write-Error $f }; exit 1 }
exit 0
