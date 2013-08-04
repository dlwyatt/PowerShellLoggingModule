# Cmdlets and associated data types are defined in PowerShellLoggingModule.dll.  This script file just handles attaching and detaching the PSHostIOInterceptor object.

$dllPath = Join-Path -Path $MyInvocation.MyCommand.ScriptBlock.Module.ModuleBase -ChildPath PowerShellLoggingModule.dll

try {
    Import-Module -Name $dllPath -ErrorAction Stop
} catch {
    throw
}

# Attach the interceptor

$flags = [System.Reflection.BindingFlags]([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)

$uiRef = $Host.GetType().GetField('internalUIRef', $flags).GetValue($Host)
$ui = $uiRef.GetType().GetProperty('Value', $flags).GetValue($uiRef, $null)

$externalUIField = $ui.GetType().GetField('externalUI', $flags)
$originalUI = $externalUIField.GetValue($ui)

try {
    $HostIOInterceptor = [PSLogging.HostIOInterceptor]::GetInterceptor()
    $HostIOInterceptor.HostUI = $originalUI

    $externalUIField.SetValue($ui, $HostIOInterceptor)

    # Detach the interceptor when the module is removed.
    $MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
        $script:externalUIField.SetValue($script:ui, $script:originalUI)
    }
} catch {
    throw
}

