// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Suspend, "Logging")]
    public class SuspendLoggingCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            HostIoInterceptor.GetInterceptor().Paused = true;
        }
    } // End ResumeLoggingCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global