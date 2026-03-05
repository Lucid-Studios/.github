param(
    [string]$CradleTekRuntimeRoot = $env:CRADLETEK_RUNTIME_ROOT
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CradleTekRuntimeRoot)) {
    $CradleTekRuntimeRoot = Join-Path $env:SystemDrive "CradleTek"
}

$pidFile = Join-Path $CradleTekRuntimeRoot "runtime\soulframe_runtime.pid"
if (-not (Test-Path $pidFile)) {
    Write-Host "[soulframe-stop] No runtime PID file found."
    return
}

$pidValue = Get-Content $pidFile -ErrorAction SilentlyContinue
if ($pidValue -match "^\d+$") {
    $process = Get-Process -Id ([int]$pidValue) -ErrorAction SilentlyContinue
    if ($null -ne $process) {
        Stop-Process -Id ([int]$pidValue) -ErrorAction SilentlyContinue
        Write-Host "[soulframe-stop] Sent termination signal to pid=$pidValue."
    }
    else {
        Write-Host "[soulframe-stop] Process $pidValue not running."
    }
}
else {
    Write-Host "[soulframe-stop] PID file contents invalid: '$pidValue'"
}

Remove-Item -Path $pidFile -Force -ErrorAction SilentlyContinue
Write-Host "[soulframe-stop] Runtime stopped."
