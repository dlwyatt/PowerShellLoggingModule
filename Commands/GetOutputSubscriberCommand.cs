
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Get, "OutputSubscriber")]
    public class GetOutputSubscriberCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            foreach (IHostIOSubscriber subscriber in HostIOInterceptor.Instance.Subscribers)
            {
                var scriptBlockSubscriber = subscriber as ScriptBlockOutputSubscriber;
                if (scriptBlockSubscriber != null)
                {
                    this.WriteObject(scriptBlockSubscriber);
                }
            }
        }
    } // End GetOutputSubscriberCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global