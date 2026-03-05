Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Read-FoundingJson {
    param([Parameter(Mandatory = $true)][string]$Path)
    return Get-Content -Raw -Encoding utf8 $Path | ConvertFrom-Json
}

function Write-FoundingJson {
    param([Parameter(Mandatory = $true)][string]$Path, [Parameter(Mandatory = $true)][object]$Object)
    $Object | ConvertTo-Json -Depth 40 | Set-Content -Encoding utf8 $Path
}

function Normalize-FoundingNode {
    param([object]$Node)
    if ($null -eq $Node) { return $null }
    if ($Node -is [System.Collections.IDictionary]) {
        $ordered = [ordered]@{}
        foreach ($k in @($Node.Keys) | Sort-Object) { $ordered[[string]$k] = Normalize-FoundingNode $Node[$k] }
        return [pscustomobject]$ordered
    }
    if ($Node -is [pscustomobject]) {
        $ordered = [ordered]@{}
        foreach ($p in @($Node.PSObject.Properties.Name) | Sort-Object) { $ordered[$p] = Normalize-FoundingNode $Node.$p }
        return [pscustomobject]$ordered
    }
    if ($Node -is [System.Collections.IEnumerable] -and -not ($Node -is [string])) {
        $arr = @()
        foreach ($i in $Node) { $arr += ,(Normalize-FoundingNode $i) }
        return $arr
    }
    return $Node
}

function Get-FoundingCanonicalJson {
    param([Parameter(Mandatory = $true)][object]$Node)
    return ((Normalize-FoundingNode $Node) | ConvertTo-Json -Depth 50 -Compress)
}

function Get-FoundingSha256 {
    param([Parameter(Mandatory = $true)][string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return ([System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant())
}

function Merge-FoundingLayer0Telemetry {
    param(
        [Parameter(Mandatory = $true)][string]$CognitionTelemetryPath,
        [Parameter(Mandatory = $true)][string]$SectionName,
        [Parameter(Mandatory = $true)][object]$SectionValue
    )

    if (-not (Test-Path -Path $CognitionTelemetryPath -PathType Leaf)) { return }

    $cognition = Read-FoundingJson -Path $CognitionTelemetryPath
    if (-not ($cognition.PSObject.Properties.Name -contains "metrics") -or $null -eq $cognition.metrics) {
        $cognition | Add-Member -MemberType NoteProperty -Name metrics -Value ([ordered]@{})
    }

    $metricsMap = [ordered]@{}
    foreach ($prop in $cognition.metrics.PSObject.Properties) { $metricsMap[$prop.Name] = $prop.Value }

    if (-not $metricsMap.Contains("layer0")) { $metricsMap["layer0"] = [ordered]@{} }
    $layer0 = [ordered]@{}
    if ($metricsMap["layer0"] -is [pscustomobject]) { foreach ($prop in $metricsMap["layer0"].PSObject.Properties) { $layer0[$prop.Name] = $prop.Value } }
    elseif ($metricsMap["layer0"] -is [System.Collections.IDictionary]) { foreach ($k in $metricsMap["layer0"].Keys) { $layer0[[string]$k] = $metricsMap["layer0"][$k] } }
    $layer0[$SectionName] = $SectionValue
    $metricsMap["layer0"] = [pscustomobject]$layer0

    if (-not $metricsMap.Contains("layer_0")) { $metricsMap["layer_0"] = [ordered]@{} }
    $layer_0 = [ordered]@{}
    if ($metricsMap["layer_0"] -is [pscustomobject]) { foreach ($prop in $metricsMap["layer_0"].PSObject.Properties) { $layer_0[$prop.Name] = $prop.Value } }
    elseif ($metricsMap["layer_0"] -is [System.Collections.IDictionary]) { foreach ($k in $metricsMap["layer_0"].Keys) { $layer_0[[string]$k] = $metricsMap["layer_0"][$k] } }
    $layer_0[$SectionName] = $SectionValue
    $metricsMap["layer_0"] = [pscustomobject]$layer_0

    $cognition.metrics = [pscustomobject]$metricsMap
    Write-FoundingJson -Path $CognitionTelemetryPath -Object $cognition
}
