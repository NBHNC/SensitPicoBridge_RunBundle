$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location (Join-Path $root 'Source\Sensit.App.Programmer')
try {
    dotnet run -c Release
}
finally {
    Pop-Location
}
