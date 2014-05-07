// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Enable, "OutputSubscriber")]
    public class EnableOutputSubscriberCommand : PSCmdlet
    {
        private ScriptBlockOutputSubscriber inputObject;
        private ScriptBlock onWriteDebug;
        private ScriptBlock onWriteError;
        private ScriptBlock onWriteOutput;
        private ScriptBlock onWriteVerbose;
        private ScriptBlock onWriteWarning;

        #region Parameters

        [Parameter(ParameterSetName = "AttachExisting",
            Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public ScriptBlockOutputSubscriber InputObject
        {
            get { return inputObject; }
            set { inputObject = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteDebug
        {
            get { return onWriteDebug; }
            set { onWriteDebug = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteError
        {
            get { return onWriteError; }
            set { onWriteError = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteOutput
        {
            get { return onWriteOutput; }
            set { onWriteOutput = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteVerbose
        {
            get { return onWriteVerbose; }
            set { onWriteVerbose = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteWarning
        {
            get { return onWriteWarning; }
            set { onWriteWarning = value; }
        }

        #endregion

        protected override void EndProcessing()
        {
            ScriptBlockOutputSubscriber subscriber;

            if (ParameterSetName == "New")
            {
                subscriber = new ScriptBlockOutputSubscriber(onWriteOutput,
                                                             onWriteDebug,
                                                             onWriteVerbose,
                                                             onWriteError,
                                                             onWriteWarning);
                WriteObject(subscriber);
            }
            else
            {
                subscriber = inputObject;
            }

            HostIOInterceptor.Instance.AttachToHost(Host);
            HostIOInterceptor.Instance.AddSubscriber(subscriber);
        }
    } // End AddOutputSubscriberCommand class
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global