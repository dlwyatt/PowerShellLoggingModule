# Cmdlets and associated data types are defined in PowerShellLoggingModule.dll.  This script file just handles attaching and detaching the HostIoInterceptor object.

$dllPath = Join-Path -Path $MyInvocation.MyCommand.ScriptBlock.Module.ModuleBase -ChildPath PowerShellLoggingModule.dll

try {
    Import-Module -Name $dllPath -ErrorAction Stop
} catch {
    throw
}

try {
    # Attach the interceptor

    $flags = [System.Reflection.BindingFlags]([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)

    $uiRef = $Host.GetType().GetField('internalUIRef', $flags).GetValue($Host)
    $ui = $uiRef.GetType().GetProperty('Value', $flags).GetValue($uiRef, $null)

    $externalUIField = $ui.GetType().GetField('externalUI', $flags)
    
    $originalUI = $externalUIField.GetValue($ui)
    
    $HostIoInterceptor = [PSLogging.HostIoInterceptor]::GetInterceptor()
    $HostIoInterceptor.HostUi = $originalUI

    $externalUIField.SetValue($ui, $HostIoInterceptor)
    
    # Detach the interceptor when the module is removed.
    $MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
        $script:externalUIField.SetValue($script:ui, $script:originalUI)
    }
} catch {
    throw
}
