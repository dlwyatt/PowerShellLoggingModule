# Cmdlets and associated data types are defined in PowerShellLoggingModule.dll.
# This script file just handles detaching the HostIOInterceptor object when the module unloads.

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [PSLogging.HostIOInterceptor]::Instance.RemoveAllSubscribers()
    [PSLogging.HostIOInterceptor]::Instance.DetachFromHost()
}
