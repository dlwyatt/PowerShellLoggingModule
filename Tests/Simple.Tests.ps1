#requires -Module @{ModuleName = "PowerShellLogging"; ModuleVersion = "1.3.0"}
Describe "It should work when called from scripts run by PowerShell.Invoke" {

    $Path = "TestDrive:\log.txt"
    $Path = (Join-Path (Convert-Path (Split-Path $Path)) (Split-Path $Path -Leaf))

    $TestScript = {
        Import-Module PowerShellLogging
        $Logging = Enable-LogFile -Path $Path
        Write-Host 'This is a host test'
        'Returned OK'
        Write-Verbose 'This is a verbose test' -Verbose
        # Does not output, because Verbose is suppressed
        Write-Verbose 'This is a another verbose test'
        Disable-LogFile $Logging
    }

    It "Should not crash when used" {
        $script:Result = & $TestScript
    }

    It "Should not interfere with output" {
        $script:Result | Should -Be "Returned OK"
    }

    It "Should not cause Enable-LogFile to fail" {
        $script:PowerShell.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Enable-LogFile"
    }

    It "Should not cause Write-Host to fail" {
        $script:PowerShell.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Host"
    }

    It "Should not cause Write-Verbose to fail" {
        $script:PowerShell.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Verbose"
    }

    It "Should not cause Disable-LogFile to fail" {
        $script:PowerShell.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Disable-LogFile"
    }

    It "Should not cause any errors" {
        $script:PowerShell.Streams.Error.Count | Should -Be 0
    }

    It "Should create the log file" {
        # this proves the logging works
        $Path | Should -Exist
    }

    It "Should log host output" -Skip:(!(Test-Path $Path)) {
        (Get-Content $Path) -match ".*This is a host test$" | Should -Not -BeNullOrEmpty
    }

    It "Should log verbose output" -Skip:(!(Test-Path $Path)) {
        (Get-Content $Path) -match ".*This is a verbose test$" | Should -Not -BeNullOrEmpty
    }

    It "Should not log verbose that's not output" -Skip:(!(Test-Path $Path)) {
        (Get-Content $Path) -match ".*This is another verbose test$" | Should -BeNullOrEmpty
    }

}