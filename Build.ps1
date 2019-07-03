[CmdletBinding()]
param($OutputDirectory = $("$PSScriptRoot\Output"), [switch]$Force)


Remove-Item $OutputDirectory -Recurse -Confirm:(!$Force)


Copy-Item $PSScriptRoot\Module -Destination $OutputDirectory -recurse

dotnet build -c Release
Get-ChildItem bin/Release -recurse -filter *.dll |
    Move-Item -Destination $OutputDirectory\PowerShellLogging\