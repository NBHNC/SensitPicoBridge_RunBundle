$ErrorActionPreference = "Stop"

# Scripts folder is:
# Source\Sensit.App.O2Characterizer\Scripts
# Repo root is 3 levels up from here.
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$solution = Join-Path $repoRoot "SensitPicoBridge_RunBundle.sln"
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"

if (-not (Test-Path $solution)) {
    throw "Solution not found at $solution"
}

if (-not (Test-Path $vswhere)) {
    throw "vswhere.exe not found. Install Visual Studio 2022 or Visual Studio 2022 Build Tools."
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild.exe not found. Install Visual Studio 2022 or Visual Studio 2022 Build Tools."
}

Write-Host "Repo root:" $repoRoot
Write-Host "Using MSBuild:" $msbuild
Write-Host "Building solution:" $solution

& $msbuild $solution /t:Restore,Build /p:Configuration=Release /p:Platform="Any CPU"
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}
