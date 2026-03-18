$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location (Join-Path $root 'Source\Sensit.App.Programmer')
try {
    dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false
}
finally {
    Pop-Location
}
