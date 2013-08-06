// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Disable, "LogFile")]
    public class DisableLogFileCommand : PSCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public LogFile InputObject { get; set; }

        protected override void EndProcessing()
        {
            HostIoInterceptor.GetInterceptor().RemoveSubscriber(this.InputObject);
        }
    } // End DisableLogFileCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global