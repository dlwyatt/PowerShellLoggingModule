
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;
    [Cmdlet(VerbsLifecycle.Disable, "OutputSubscriber")]
    public class DisableOutputSubscriberCommand : PSCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public ScriptBlockOutputSubscriber InputObject { get; set; }

        protected override void EndProcessing()
        {
            HostIoInterceptor.GetInterceptor().RemoveSubscriber(this.InputObject);
        }
    } // End DisableOutputSubscriberCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global