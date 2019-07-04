@{
    ModuleToProcess        = 'PowerShellLogging.psm1'
    NestedModules           = @( "PowerShellLoggingModule.dll" )
    ModuleVersion          = '1.4.0'
    GUID                   = '345320b5-bdc3-4ab6-a13e-fcb019362fe6'
    CompanyName            = 'Home'
    CopyRight              = 'Copyright 2017 David Wyatt'
    Author                 = 'Dave Wyatt'
    Description            = 'Captures PowerShell console output to a log file.'
    PowerShellVersion      = '2.0'
    DotNetFrameworkVersion = '2.0'
    FunctionsToExport      = '*'
    CmdletsToExport        = '*'
    VariablesToExport      = '*'
    AliasesToExport = '*'

    FileList = @('PowerShellLogging.psm1','PowerShellLogging.psd1','PowerShellLoggingModule.dll','en-US\about_PowerShellLogging.help.txt','en-US\PowerShellLoggingModule.dll-help.xml')

    PrivateData = @{
        PSData = @{
            Prerelease   = ''
            LicenseUri   = 'http://www.apache.org/licenses/LICENSE-2.0.txt'
            ProjectUri   = 'https://github.com/dlwyatt/PowerShellLoggingModule'
            ReleaseNotes = 'Added support for PowerShell 6 & 7
Added support for logging from runspaces in pools (i.e. without an actual host)'
        }
    }
}
