#requires -Module PowerShellLogging, ThreadJob
param($Count = 2)

Describe "Working when called simultaneously in parallel runspaces" -Tag "ThreadJob", "WIP" {

    $Path = "TestDrive:\log{0}.txt"
    $Path = (Join-Path (Convert-Path (Split-Path $Path)) (Split-Path $Path -Leaf))

    $TestScript = "
        Import-Module PowerShellLogging
        `$Logging = Enable-LogFile -Path '${Path}'
        Write-Host 'This is a host test from attempt {0}'
        '${Path}'
        Start-Sleep 2
        Write-Verbose 'This is a verbose test from attempt {0}' -Verbose
        Disable-LogFile `$Logging
    "

    $TestScript = {
        param($Path, $Index)
        Import-Module PowerShellLogging
        $Logging = Enable-LogFile -Path $Path
        Microsoft.PowerShell.Utility\Write-Host "This is a host test from attempt $index"
        $Path
        Microsoft.PowerShell.Utility\Write-Verbose 'This is a verbose test' -Verbose
        # Does not output, because Verbose is suppressed
        Microsoft.PowerShell.Utility\Write-Verbose "This is a verbose test from attempt $index" -Verbose
        Disable-LogFile $Logging
    }


    It "Should not crash when used" {
        $script:Results = & {
            foreach ($i in 1..$Count) {
                Start-ThreadJob $TestScript -ArgumentList ($Path -f $i), $i
            }
        } | Wait-Job | Receive-Job -ErrorVariable Ev
        $Script:Ev = $Ev
    }

    It "Should not interfere with output" {
        $script:Results.Count | Should -Be $Count
        $i = 0
        foreach ($resultPath in $script:Results) {
            $i += 1
            $resultPath | Should -Be ($Path -f $i)
        }
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


    Write-Warning "Expecting $($script:Results.Count) log files!"

    It "Should create the log file" {
        # this is enough to prove the logging works
        foreach ($i in 1..$Count) {
            ($Path -f $i) | Should -Exist
        }
    }


    foreach ($i in 1..$Count) {
        It "Should log host output to $($Path -f $i)" -Skip:(!(Test-Path ($Path -f $i))) {
            (Get-Content ($Path -f $i)) -match "This is a host test from attempt $i$" | Should -Not -BeNullOrEmpty
        }
    }


    foreach ($i in 1..$Count) {
        It "Should log host output to $($Path -f $i)" -Skip:(!(Test-Path ($Path -f $i))) {
            (Get-Content ($Path -f $i)) -match "This is a verbose test from attempt $i$" | Should -Not -BeNullOrEmpty
        }
    }
}