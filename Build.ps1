[CmdletBinding()]
param(
    # A version number to update the output with (uses gitversion by default)
    $Version = $(gitversion -showvariable nugetversion),

    # The output folder (defaults to the version number)
    $OutputDirectory = $("$PSScriptRoot\$(($Version -split '[-+]',2)[0])"),

    # If set, removes the output folder without prompting!
    [switch]$Force
)

$VersionPrefix, $VersionSuffix = $Version -split '[-+]', 2

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Confirm:(!$Force)
}

Copy-Item $PSScriptRoot\Module\PowerShellLogging -Destination $OutputDirectory -recurse
Set-Content $OutputDirectory\PowerShellLogging.psd1 (
    (Get-Content $OutputDirectory\PowerShellLogging.psd1) -replace
        "(ModuleVersion\s+=\s+)'.*'", "`$1'$VersionPrefix'" -replace
        "(Prerelease\s+=\s+)'.*'", "`$1'$VersionSuffix'"
)

dotnet build -c Release -p:VersionPrefix=$VersionPrefix -p:VersionSuffix=$VersionSuffix

Get-ChildItem bin/Release -recurse -filter *.dll |
    Move-Item -Destination $OutputDirectory\