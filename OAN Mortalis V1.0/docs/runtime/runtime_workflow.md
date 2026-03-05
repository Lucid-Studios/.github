# CradleTek Prime Runtime Workflow

## Purpose

Operate the hosted SoulFrame runtime as infrastructure outside the repository while keeping the OAN cognition stack deterministic and symbolic-first.

## Pre-requisites

1. Windows host with PowerShell.
2. Python 3 available.
3. Runtime root write access at `C:\CradleTek`.
4. Model weights staged manually outside repo at `C:\CradleTek\models\seed.gguf` (or custom path in config).

## Bootstrap

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1
```

Bootstrap performs:

1. Runtime directory creation under `C:\CradleTek`.
2. Hyper-V and CPU virtualization checks.
3. Python virtual environment setup.
4. Flask dependency install.
5. `llama.cpp` clone/build/install.
6. Inference service deploy and startup.

## Start Runtime

```powershell
powershell -ExecutionPolicy Bypass -File tools\cradletek\start_runtime.ps1
```

This starts the hosted API service and writes a PID file to:

```text
C:\CradleTek\runtime\soulframe_runtime.pid
```

## Build/Test Stack

```powershell
dotnet build Oan.sln
dotnet test Oan.sln
```

## Stop Runtime

```powershell
powershell -ExecutionPolicy Bypass -File tools\cradletek\stop_runtime.ps1
```

## Repository Purity Rules

Do not place these in repository history:

1. VM disks/images.
2. Model weights.
3. Runtime logs and caches.
4. Container layers.

These remain external runtime infrastructure owned by CradleTek.

## Windows Runtime Guide

See:

```text
docs/runtime/WINDOWS_RUNTIME.md
```
