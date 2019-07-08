#requires -Module PowerShellLogging, ThreadJob
param($Count = 4)

Describe "Working when called in parallel in remote runspaces" -Tag "Remoting" {

    $Path = "TestDrive:\log{0}.txt"
    $Path = (Join-Path (Convert-Path (Split-Path $Path)) (Split-Path $Path -Leaf))

    BeforeAll {
        $script:PowerShell = $(
            foreach($index in 1..$Count) {
                $LocalHost = [System.Management.Automation.Runspaces.WSManConnectionInfo]@{ComputerName = "."; EnableNetworkAccess = $true }
                $Runspace = [runspacefactory]::CreateRunspace($LocalHost)
                $Runspace.Open()
                $PowerShell = [PowerShell]::Create()
                $PowerShell.Runspace = $Runspace
                $PowerShell
            }
        )
    }

    AfterAll {
        foreach($PS in $script:PowerShell) {
            $PS.Runspace.Dispose()
            $PS.Dispose()
        }
    }

    $TestScript = "
        Import-Module PowerShellLogging
        `$Logging = Enable-LogFile -Path '${Path}'
        Write-Host 'This is a host test from attempt {0}'
        '${Path}'
        Start-Sleep 2
        Write-Verbose 'This is a verbose test from attempt {0}' -Verbose
        Disable-LogFile `$Logging
    "

    It "Should not crash when used" {
        $script:Results = & {
            $i = 0
            foreach($PS in $script:PowerShell) {
                $i += 1
                Start-ThreadJob { param($PS, $Script) $PS.AddScript($Script).Invoke() } -ArgumentList $PS, ($TestScript -f $i)
            }
        } | Wait-Job | Receive-Job
    }

    It "Should not interfere with output" {
        $script:Results.Count | Should -Be $script:PowerShell.Count
        $i = 0
        foreach ($resultPath in $script:Results) {
            $i += 1
            $resultPath | Should -Be ($Path -f $i)
        }
    }

    It "Should not cause Enable-LogFile to fail" {
        foreach($PS in $script:PowerShell) {
            $PS.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Enable-LogFile"
        }
    }

    It "Should not cause Write-Host to fail" {
        foreach ($PS in $script:PowerShell) {
            $PS.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Host"
        }
    }

    It "Should not cause Write-Verbose to fail" {
        foreach ($PS in $script:PowerShell) {
            $PS.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Write-Verbose"
        }
    }

    It "Should not cause Disable-LogFile to fail" {
        foreach ($PS in $script:PowerShell) {
            $PS.Streams.Error.InvocationInfo.MyCommand.Name | Should -Not -Contain "Disable-LogFile"
        }
    }

    It "Should not cause any errors" {
        foreach ($PS in $script:PowerShell) {
            $PS.Streams.Error.Count | Should -Be 0
        }
    }

    Write-Warning "Expecting $($script:Results.Count) log files!"

    It "Should create the log file" {
        # this is enough to prove the logging works
        $i = 0
        foreach($PS in $script:PowerShell) {
            $i += 1
            ($Path -f $i) | Should -Exist
        }
    }

    $i = 0
    foreach ($PS in $script:PowerShell) {
        $i += 1
        It "Should log host output to $($Path -f $i)" -Skip:(!(Test-Path ($Path -f $i))) {
            (Get-Content ($Path -f $i)) -match "This is a host test from attempt $i$" | Should -Not -BeNullOrEmpty
        }
    }

    $i = 0
    foreach ($PS in $script:PowerShell) {
        $i += 1
        It "Should log host output to $($Path -f $i)" -Skip:(!(Test-Path ($Path -f $i))) {
            (Get-Content ($Path -f $i)) -match "This is a verbose test from attempt $i$" | Should -Not -BeNullOrEmpty
        }
    }
}