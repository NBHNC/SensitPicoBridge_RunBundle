$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "Source\Sensit.App.Programmer\bin\Release\net8.0-windows\Sensit.App.Programmer.exe"

if (-not (Test-Path $exe)) {
    throw "EXE not found at $exe. Build the solution first."
}

Start-Process -FilePath $exe
