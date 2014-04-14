
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;
    
    [Cmdlet(VerbsCommon.Add, "LogFile")]
    public class AddLogFileCommand : PSCmdlet
    {
        private ScriptBlock errorCallback;
        private LogFile inputObject;
        private string path;
        private StreamType streams = StreamType.All;

        #region Parameters

        [Parameter(ParameterSetName = "AttachExisting",
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true)]
        public LogFile InputObject
        {
            get { return this.inputObject; }
            set { this.inputObject = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnError
        {
            get { return this.errorCallback; }
            set { this.errorCallback = value; }
        }

        [Parameter(Mandatory = true,
            Position = 0,
            ParameterSetName = "New")]
        public string Path
        {
            get { return this.path; }
            set
            {
                this.path = this.GetUnresolvedProviderPathFromPSPath(value);
            }
        }

        [Parameter(ParameterSetName = "New")]
        public StreamType StreamType
        {
            get { return this.streams; }
            set { this.streams = value; }
        }

        #endregion

        protected override void EndProcessing()
        {
            LogFile logFile;

            if (this.ParameterSetName == "New")
            {
                logFile = new LogFile(this.path, this.streams, this.errorCallback);
                this.WriteObject(logFile);
            }
            else
            {
                logFile = this.inputObject;
            }

            HostIOInterceptor.Instance.AttachToHost(this.Host);
            HostIOInterceptor.Instance.AddSubscriber(logFile);
        }
    } // End AddLogFileCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global