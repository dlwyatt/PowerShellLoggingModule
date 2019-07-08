#requires -Module PowerShellLogging
Describe "Working when called synchronously in a parallel runspace (as long as we Remove-Module)" {

    $Path = "TestDrive:\log.txt"
    $Path = (Join-Path (Convert-Path (Split-Path $Path)) (Split-Path $Path -Leaf))

    BeforeAll {
        $script:PowerShell = [PowerShell]::Create()
        $PowerShell.Runspace = [runspacefactory]::CreateRunspace()
        $PowerShell.Runspace.Open()
    }

    AfterAll {
        if ($script:PowerShell) {
            $script:PowerShell.Runspace.Dispose()
            $script:PowerShell.Dispose()
        }
    }

    $TestScript = "
        Import-Module PowerShellLogging
        `$Logging = Enable-LogFile -Path '$Path'
        Write-Host 'This is a host test'
        'Returned OK'
        Write-Verbose 'This is a verbose test' -Verbose
        Disable-LogFile `$Logging
        Remove-Module PowerShellLogging
    "

    It "Should not crash when used" {
        $script:Result = $script:PowerShell.AddScript($TestScript).Invoke()
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


}