// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Resume, "Logging")]
    public class ResumeLoggingCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            HostIOInterceptor.Instance.Paused = false;
        }
    } // End ResumeLoggingCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global