
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;
    
    [Cmdlet(VerbsLifecycle.Enable, "LogFile")]
    public class EnableLogFileCommand : PSCmdlet
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
            get { return inputObject; }
            set { inputObject = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnError
        {
            get { return errorCallback; }
            set { errorCallback = value; }
        }

        [Parameter(Mandatory = true,
            Position = 0,
            ParameterSetName = "New")]
        public string Path
        {
            get { return path; }
            set
            {
                path = GetUnresolvedProviderPathFromPSPath(value);
            }
        }

        [Parameter(ParameterSetName = "New")]
        public StreamType StreamType
        {
            get { return streams; }
            set { streams = value; }
        }

        #endregion

        protected override void EndProcessing()
        {
            LogFile logFile;

            if (ParameterSetName == "New")
            {
                logFile = new LogFile(path, streams, errorCallback);
                WriteObject(logFile);
            }
            else
            {
                logFile = inputObject;
            }

            HostIOInterceptor.Instance.AttachToHost(Host);
            HostIOInterceptor.Instance.AddSubscriber(logFile);
        }
    } // End AddLogFileCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global