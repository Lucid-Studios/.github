[CmdletBinding()]
param(
    [string]$ModulePath,
    [string]$SidecarOutDir,
    [string]$KeyringPath,
    [string[]]$Contracts,
    [string]$KeyId = "seed-governance-ed25519-1"
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
if (-not (Test-Path -Path $SidecarOutDir -PathType Container)) { New-Item -ItemType Directory -Path $SidecarOutDir | Out-Null }

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
$key = @($keyring.keys | Where-Object { $_.key_id -eq $KeyId })[0]
if ($null -eq $key) { Write-Error "Key id not found in keyring: $KeyId"; exit 1 }

$priv = Join-Path $ModulePath ([string]$key.private_key_relpath)
$pub = Join-Path $ModulePath ([string]$key.public_key_relpath)
$principal = [string]$key.principal
$namespace = "oan-governance"

$privDir = Split-Path -Parent $priv
if (-not (Test-Path -Path $privDir -PathType Container)) { New-Item -ItemType Directory -Path $privDir | Out-Null }
if (-not (Test-Path -Path $priv -PathType Leaf) -or -not (Test-Path -Path $pub -PathType Leaf)) {
    & ssh-keygen -q -t ed25519 -N '""' -C $principal -f $priv | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to generate Ed25519 keypair for governance sidecars."
        exit 1
    }
}

$results = New-Object System.Collections.Generic.List[object]
foreach ($contract in $Contracts) {
    if (-not (Test-Path -Path $contract -PathType Leaf)) { Write-Error "Missing contract: $contract"; exit 1 }
    $canonical = Get-CanonicalJson $contract
    $hash = Get-Sha256 $canonical
    $name = [IO.Path]::GetFileNameWithoutExtension($contract)

    $hashSidecar = [ordered]@{
      schema = "oan.sidecar.hash.v0.1.0"
      artifact_path = $contract
      artifact_schema = ((Get-Content -Raw -Encoding utf8 $contract | ConvertFrom-Json).schema)
      artifact_sha256 = $hash
      canonicalization = "json_sorted_keys_utf8_compact"
      created_by = "New-Governance-Sidecar.ps1"
      created_at = (Get-Date).ToString("o")
    }

    $tmp = Join-Path $env:TEMP ("govsig_{0}.json" -f ([Guid]::NewGuid().ToString("N")))
    [System.IO.File]::WriteAllText($tmp, $canonical, [System.Text.UTF8Encoding]::new($false))
    & ssh-keygen -Y sign -f $priv -n $namespace $tmp | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to sign contract sidecar payload for $name"
        exit 1
    }
    $sigFile = "$tmp.sig"
    $sigB64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($sigFile))

    $sigSidecar = [ordered]@{
      schema = "oan.sidecar.sig.v0.1.0"
      artifact_path = $contract
      artifact_sha256 = $hash
      key_id = $KeyId
      principal = $principal
      alg = "ed25519"
      namespace = $namespace
      signature_encoding = "openssh_sshsig_base64"
      signature = $sigB64
      created_at = (Get-Date).ToString("o")
    }

    $hashPath = Join-Path $SidecarOutDir ("{0}.hash.json" -f $name)
    $sigPath = Join-Path $SidecarOutDir ("{0}.sig.json" -f $name)
    $hashSidecar | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $hashPath
    $sigSidecar | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $sigPath

    Remove-Item -Force $tmp,$sigFile -ErrorAction SilentlyContinue
    $results.Add([pscustomobject]@{ contract = $contract; hash_sidecar = $hashPath; sig_sidecar = $sigPath; sha256 = $hash })
}

$summary = [ordered]@{
  schema = "oan.sidecar.generation.v0.1.0"
  key_id = $KeyId
  pass = $true
  entries = $results.ToArray()
}
$summaryPath = Join-Path $SidecarOutDir "governance_sidecar_generation.json"
$summary | ConvertTo-Json -Depth 20 | Set-Content -Encoding utf8 $summaryPath
Write-Host "Wrote governance sidecars to: $SidecarOutDir"
exit 0
