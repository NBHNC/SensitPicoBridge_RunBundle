$ErrorActionPreference = "Stop"

# Scripts folder is:
# Source\Sensit.App.O2Characterizer\Scripts
# Repo root is 3 levels up from here.
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$exe = Join-Path $repoRoot "Source\Sensit.App.O2Characterizer\bin\Release\net8.0-windows\Sensit.App.O2Characterizer.exe"

if (-not (Test-Path $exe)) {
    throw "EXE not found at $exe. Run build_o2_characterizer.ps1 first."
}

Write-Host "Launching:" $exe
Start-Process -FilePath $exe
