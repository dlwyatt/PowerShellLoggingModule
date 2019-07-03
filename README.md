PowerShellLoggingModule
=======================
Uses Reflection to intercept text output headed for the PowerShell console.
All lines of output are sent to any number of subscriber objects
producing complete log files of script output (adding date/timestamps)
without needing extra code by the script author.

The upside is that any text which would have shown up in the console is logged,
and the downside is that only that text is logged.
For example, verbose output is only logged if VerbosePreference is Continue...


Supports PowerShell 2, 3, 4, 5, 6 and 7


Install from the PowerShell Gallery
==================================

```posh
Install-Module PowerShellLogging
```

Compile your own copy
====================

You can run `build.ps1` to assembly a copy in an `Output` folder,
or you can just compile the assembly with:

```posh
dotnet build -c Release
```

Testing
=======

The test cases are very minimal (basically just covering the fact that it logs, and testing a couple of edge cases where it used to fail to log). Despite that, weand have some problems due to the way that WindowsPowerShell breaks when they fail.

 As a result, _to be sure that the tests are actually working_ (reporting the correct results), you should run each test case in a new session. There is a wrapper script `test.ps1` which you can use to do that, it basically just runs each test case in `PowerShell` and `pwsh` to ensure everything is working in both WindowsPowerShell and PowerShell core. E.g.:

```
foreach ($testcase in Get-ChildItem Tests\*.Tests.ps1) {
    powershell -NoProfile -Command Invoke-Pester $testcase.FullName
}
```

Alternative Download
===================

Note, the original version is also available from:

http://gallery.technet.microsoft.com/scriptcenter/Enhanced-Script-Logging-27615f85
