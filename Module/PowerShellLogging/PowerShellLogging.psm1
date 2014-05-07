# Cmdlets and associated data types are defined in PowerShellLoggingModule.dll.  This script file just handles detaching the HostIOInterceptor object when the module unloads.

$dllPath = Join-Path -Path $MyInvocation.MyCommand.ScriptBlock.Module.ModuleBase -ChildPath PowerShellLoggingModule.dll

try
{
    Import-Module -Name $dllPath -ErrorAction Stop
}
catch
{
    throw
}

$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    [PSLogging.HostIOInterceptor]::Instance.RemoveAllSubscribers()
    [PSLogging.HostIOInterceptor]::Instance.DetachFromHost()
}
