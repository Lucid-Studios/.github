param(
    [string]$CradleTekRuntimeRoot = $env:CRADLETEK_RUNTIME_ROOT,
    [string]$RuntimeEnvFile = $env:RUNTIME_ENV_FILE,
    [string]$RuntimeScript = $env:RUNTIME_SCRIPT
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CradleTekRuntimeRoot)) {
    $CradleTekRuntimeRoot = Join-Path $env:SystemDrive "CradleTek"
}

if ([string]::IsNullOrWhiteSpace($RuntimeEnvFile)) {
    $RuntimeEnvFile = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.env"
}

if ([string]::IsNullOrWhiteSpace($RuntimeScript)) {
    $RuntimeScript = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.py"
}

$pidFile = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.pid"
$logFile = Join-Path $CradleTekRuntimeRoot "logs\soulframe_runtime.log"
$venvDir = Join-Path $CradleTekRuntimeRoot "runtime\.venv"
$venvPython = Join-Path $venvDir "Scripts\python.exe"

if (-not (Test-Path $RuntimeEnvFile)) {
    Write-Error "[soulframe-start] Missing env file: $RuntimeEnvFile"
}

if (-not (Test-Path $RuntimeScript)) {
    Write-Error "[soulframe-start] Missing runtime script: $RuntimeScript"
}

if (-not (Test-Path $venvPython)) {
    Write-Error "[soulframe-start] Missing virtual env python at '$venvPython'. Run bootstrap.ps1 first."
}

if (Test-Path $pidFile) {
    $existingPid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($existingPid -match "^\d+$") {
        $existingProcess = Get-Process -Id ([int]$existingPid) -ErrorAction SilentlyContinue
        if ($null -ne $existingProcess) {
            Write-Host "[soulframe-start] Runtime already running (pid=$existingPid)."
            return
        }
    }
}

Get-Content $RuntimeEnvFile | ForEach-Object {
    $line = $_.Trim()
    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
        return
    }

    $parts = $line.Split("=", 2)
    if ($parts.Count -eq 2) {
        [System.Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim(), "Process")
    }
}

$hostValue = if ($env:SOULFRAME_API_HOST) { $env:SOULFRAME_API_HOST } else { "127.0.0.1" }
$portValue = if ($env:SOULFRAME_API_PORT) { $env:SOULFRAME_API_PORT } else { "8888" }

New-Item -Path (Split-Path $logFile -Parent) -ItemType Directory -Force | Out-Null

$process = Start-Process `
    -FilePath $venvPython `
    -ArgumentList @($RuntimeScript, "--host", $hostValue, "--port", $portValue) `
    -RedirectStandardOutput $logFile `
    -RedirectStandardError $logFile `
    -PassThru `
    -WindowStyle Hidden

$process.Id | Set-Content $pidFile
Write-Host "[soulframe-start] Runtime started on ${hostValue}:$portValue (pid=$($process.Id))."
