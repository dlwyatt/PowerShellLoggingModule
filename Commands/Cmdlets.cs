using System.Management.Automation;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    [Cmdlet(VerbsCommon.Add, "LogFile")]
    public class AddLogFileCommand : PSCmdlet
    {
        private ScriptBlock errorCallback;
        private LogFile inputObject;
        private string path;
        private StreamType streams = StreamType.All;

        #region Parameters

        [Parameter(Mandatory = true,
            Position = 0,
            ParameterSetName = "New")]
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

        [Parameter(ParameterSetName = "New")]
        public StreamType StreamType
        {
            get { return this.streams; }
            set { this.streams = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnError
        {
            get { return this.errorCallback; }
            set { this.errorCallback = value; }
        }

        [Parameter(ParameterSetName = "AttachExisting",
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true)]
        public LogFile InputObject
        {
            get { return this.inputObject; }
            set { this.inputObject = value; }
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

            HostIoInterceptor.GetInterceptor().AddSubscriber(logFile);
        }
    } // End AddLogFileCommand class

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

    [Cmdlet(VerbsLifecycle.Suspend, "Logging")]
    public class SuspendLoggingCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            HostIoInterceptor.GetInterceptor().Paused = true;
        }
    } // End SuspendLoggingCommand class

    [Cmdlet(VerbsLifecycle.Resume, "Logging")]
    public class ResumeLoggingCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            HostIoInterceptor.GetInterceptor().Paused = false;
        }
    } // End ResumeLoggingCommand class

    [Cmdlet(VerbsCommon.Add, "OutputSubscriber")]
    public class AddOutputSubscriberCommand : PSCmdlet
    {
        private ScriptBlockOutputSubscriber inputObject;
        private ScriptBlock onWriteDebug;
        private ScriptBlock onWriteError;
        private ScriptBlock onWriteOutput;
        private ScriptBlock onWriteVerbose;
        private ScriptBlock onWriteWarning;

        #region Parameters

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteOutput
        {
            get { return this.onWriteOutput; }
            set { this.onWriteOutput = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteDebug
        {
            get { return this.onWriteDebug; }
            set { this.onWriteDebug = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteVerbose
        {
            get { return this.onWriteVerbose; }
            set { this.onWriteVerbose = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteError
        {
            get { return this.onWriteError; }
            set { this.onWriteError = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteWarning
        {
            get { return this.onWriteWarning; }
            set { this.onWriteWarning = value; }
        }

        [Parameter(ParameterSetName = "AttachExisting",
            Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public ScriptBlockOutputSubscriber InputObject
        {
            get { return this.inputObject; }
            set { this.inputObject = value; }
        }

        #endregion

        protected override void EndProcessing()
        {
            ScriptBlockOutputSubscriber subscriber;

            if (this.ParameterSetName == "New")
            {
                subscriber = new ScriptBlockOutputSubscriber(this.onWriteOutput,
                                                             this.onWriteDebug,
                                                             this.onWriteVerbose,
                                                             this.onWriteError,
                                                             this.onWriteWarning);
                this.WriteObject(subscriber);
            }
            else
            {
                subscriber = this.inputObject;
            }

            HostIoInterceptor.GetInterceptor().AddSubscriber(subscriber);
        }
    } // End AddOutputSubscriberCommand class

    [Cmdlet(VerbsCommon.Get, "OutputSubscriber")]
    public class GetOutputSubscriberCommand : PSCmdlet
    {
        protected override void EndProcessing()
        {
            foreach (IHostIoSubscriber subscriber in HostIoInterceptor.GetInterceptor().Subscribers)
            {
                var scriptBlockSubscriber = subscriber as ScriptBlockOutputSubscriber;
                if (scriptBlockSubscriber != null)
                {
                    this.WriteObject(scriptBlockSubscriber);
                }
            }
        }
    } // End GetOutputSubscriberCommand class

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