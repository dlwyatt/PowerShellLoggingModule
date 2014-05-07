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
            get { return path; }
            set
            {
                path = GetUnresolvedProviderPathFromPSPath(value);
            }
        }

        protected override void EndProcessing()
        {
            foreach (IHostIOSubscriber subscriber in HostIOInterceptor.Instance.Subscribers)
            {
                var logFile = subscriber as LogFile;

                if (logFile != null &&
                    (path == null || System.IO.Path.GetFullPath(logFile.Path) == System.IO.Path.GetFullPath(path)))
                {
                    WriteObject(logFile);
                }
            }
        }
    } // End GetLogFileCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global