using System.Management.Automation;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    [Cmdlet(VerbsCommon.Add, "LogFile")]
    public class AddLogFileCommand : PSCmdlet
    {
        private StreamType _streams = StreamType.All;
        private ScriptBlock _errorCallback;
        private string _path;
        private LogFile _inputObject;

        #region Parameters
        [Parameter(Mandatory = true,
                   Position = 0,
                   ParameterSetName = "New")]
        public string Path
        {
            get { return _path; }
            set { _path = System.IO.Path.IsPathRooted(value) ? value : System.IO.Path.Combine(SessionState.Path.CurrentLocation.Path, value); }
        }

        [Parameter(ParameterSetName = "New")]
        public StreamType StreamType
        {
            get { return _streams; }
            set { _streams = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnError
        {
            get { return _errorCallback; }
            set { _errorCallback = value; }
        }

        [Parameter(ParameterSetName = "AttachExisting",
                   Mandatory = true,
                   Position = 0,
                   ValueFromPipeline = true)]
        public LogFile InputObject
        {
            get { return _inputObject; }
            set { _inputObject = value; }
        }

        #endregion

        protected override void EndProcessing()
        {
            LogFile logFile;

            if (ParameterSetName == "New")
            {
                logFile = new LogFile(_path, _streams, _errorCallback);
                WriteObject(logFile);
            }
            else
            {
                logFile = _inputObject;
            }

            HostIoInterceptor.GetInterceptor().AddSubscriber(logFile);
        }
    } // End AddLogFileCommand class

    [Cmdlet(VerbsCommon.Get, "LogFile")]
    public class GetLogFileCommand : PSCmdlet
    {
        private string _path;

        [Parameter(Mandatory = false,
                   Position = 0,
                   ValueFromPipeline = true,
                   ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get { return _path; }
            set
            {
                _path = System.IO.Path.IsPathRooted(value) ? value : System.IO.Path.Combine(SessionState.Path.CurrentLocation.Path, value);
            }
        }

        protected override void EndProcessing()
        {
            foreach (IHostIoSubscriber subscriber in HostIoInterceptor.GetInterceptor().Subscribers)
            {
                var logFile = subscriber as LogFile;

                if (logFile != null && (_path == null || System.IO.Path.GetFullPath(logFile.Path) == System.IO.Path.GetFullPath(_path)))
                {
                    WriteObject(logFile);
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
            HostIoInterceptor.GetInterceptor().RemoveSubscriber(InputObject);
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
        private ScriptBlock _onWriteOutput;
        private ScriptBlock _onWriteDebug;
        private ScriptBlock _onWriteVerbose;
        private ScriptBlock _onWriteError;
        private ScriptBlock _onWriteWarning;

        private ScriptBlockOutputSubscriber _inputObject;

        #region Parameters
        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteOutput
        {
            get { return _onWriteOutput; }
            set { _onWriteOutput = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteDebug
        {
            get { return _onWriteDebug; }
            set { _onWriteDebug = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteVerbose
        {
            get { return _onWriteVerbose; }
            set { _onWriteVerbose = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteError
        {
            get { return _onWriteError; }
            set { _onWriteError = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteWarning
        {
            get { return _onWriteWarning; }
            set { _onWriteWarning = value; }
        }

        [Parameter(ParameterSetName = "AttachExisting",
                   Mandatory = true,
                   ValueFromPipeline = true,
                   Position = 0)]
        public ScriptBlockOutputSubscriber InputObject
        {
            get { return _inputObject; }
            set { _inputObject = value; }
        }
        #endregion

        protected override void EndProcessing()
        {
            ScriptBlockOutputSubscriber subscriber;

            if (ParameterSetName == "New")
            {
                subscriber = new ScriptBlockOutputSubscriber(_onWriteOutput, _onWriteDebug, _onWriteVerbose, _onWriteError, _onWriteWarning);
                WriteObject(subscriber);
            }
            else
            {
                subscriber = _inputObject;
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
                if (scriptBlockSubscriber != null) WriteObject(scriptBlockSubscriber);
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
            HostIoInterceptor.GetInterceptor().RemoveSubscriber(InputObject);
        }
    } // End DisableOutputSubscriberCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global
