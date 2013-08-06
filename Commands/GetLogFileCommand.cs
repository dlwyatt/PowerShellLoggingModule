// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;
    
    [Cmdlet(VerbsCommon.Get, "LogFile")]
    public class GetLogFileCommand : PSCmdlet
    {
        private string path;

        [Parameter(Mandatory = false,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get { return this.path; }
            set
            {
                this.path = System.IO.Path.IsPathRooted(value)
                                ? value
                                : System.IO.Path.Combine(this.SessionState.Path.CurrentLocation.Path, value);
            }
        }

        protected override void EndProcessing()
        {
            foreach (IHostIoSubscriber subscriber in HostIoInterceptor.GetInterceptor().Subscribers)
            {
                var logFile = subscriber as LogFile;

                if (logFile != null &&
                    (this.path == null || System.IO.Path.GetFullPath(logFile.Path) == System.IO.Path.GetFullPath(this.path)))
                {
                    this.WriteObject(logFile);
                }
            }
        }
    } // End GetLogFileCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global