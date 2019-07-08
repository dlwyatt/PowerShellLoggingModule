#requires -Module PowerShellLogging
Describe "Working in scripts run locally in the host (as long as we Remove-Module)" {

    $Path = "TestDrive:\log.txt"
    $Path = (Join-Path (Convert-Path (Split-Path $Path)) (Split-Path $Path -Leaf))

    $TestScript = {
        [CmdletBinding()]param()

        Import-Module PowerShellLogging
        $Logging = Enable-LogFile -Path $Path
        Write-Host 'This is a host test'
        'Returned OK'
        Write-Verbose 'This is a verbose test' -verbose
        Write-Verbose 'This is a another verbose test'
        Disable-LogFile $Logging
        Remove-Module PowerShellLogging
    }

    It "Should not crash when used" {
        $script:Result = & $TestScript -ErrorVariable Ev
        $script:Ev = $Ev
    }

    It "Should not interfere with output" {
        $script:Result | Should -Be "Returned OK"
    }

    It "Should not cause Enable-LogFile to fail" {
        $script:Ev.InvocationInfo.MyCommand.Name | Should -Not -Contain "Enable-LogFile"
    }

    It "Should not cause Write-Host to fail" {
        $script:Ev.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Host"
    }

    It "Should not cause Write-Verbose to fail" {
        $script:Ev.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Verbose"
    }

    It "Should not cause Disable-LogFile to fail" {
        $script:Ev.InvocationInfo.MyCommand.Name | Should -Not -Contain "Disable-LogFile"
    }

    It "Should not cause any errors" {
        $script:Ev.Count | Should -Be 0
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