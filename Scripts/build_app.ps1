$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location (Join-Path $root 'Source\Sensit.App.Programmer')
try {
    dotnet restore
    dotnet build -c Release
}
finally {
    Pop-Location
}
