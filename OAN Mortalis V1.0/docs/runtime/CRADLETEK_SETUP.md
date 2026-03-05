# CradleTek Toolchain Setup (Windows Native)

## Purpose

Install the Windows-native runtime toolchain for the CME stack while keeping repository contents source-only.

Runtime artifacts must remain outside the repository under:

```text
C:\CradleTek\
```

## Runtime Layout

Bootstrap creates:

```text
C:\CradleTek\
  runtime\
  models\
  logs\
  telemetry\
  vm\
  cme\
```

## Installer Script

```text
scripts/windows-bootstrap.ps1
```

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1
```

Optional switches:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1 -SkipLlamaBuild
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1 -SkipStartService
```

## What the Script Installs

1. Runtime directory layout under `C:\CradleTek`.
2. Hyper-V and CPU virtualization checks.
3. Python runtime venv at `C:\CradleTek\runtime\venv`.
4. Python dependencies (`flask`, `requests`).
5. `llama.cpp` source/build/binaries under `C:\CradleTek\runtime\llama.cpp`.
6. Inference service deployment and optional service start.

## Required Dependencies

1. Windows 10/11 with PowerShell.
2. Python 3 on `PATH`.
3. `git` on `PATH`.
4. `cmake` plus Visual Studio C++ build tools.
5. Internet access for dependency fetch.

## Repository Purity Rule

Do not commit runtime artifacts:

1. model files
2. vm disks/images
3. runtime logs
4. compiled runtime payloads

Only source, templates, and orchestration scripts belong in this repository.
