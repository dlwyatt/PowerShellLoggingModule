# The tests in here do not work properly in PowerShell 5.x
# If you run them all at once, you will get a lot of FALSE PASSES using the old code
# Running one test at a time in a new powershell session solves this problem:
if (Get-Command powershell.exe -ErrorAction SilentlyContinue) {
    foreach ($testcase in ls $PSScriptRoot\Tests\*.Tests.ps1) {
        powershell -NoProfile -Command Invoke-Pester $testcase.FullName
    }
} else {
    Write-Warning "Skipping Windows PowerShell tests"
}

if (Get-Command pwsh -ErrorAction SilentlyContinue) {
    foreach ($testcase in ls $PSScriptRoot\Tests\*.Tests.ps1) {
        pwsh -NoProfile -Command Invoke-Pester $testcase.FullName
    }
} else {
    Write-Warning "Skipping PowerShell Core tests"
}
