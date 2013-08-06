// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging.Commands
{
    using System.Management.Automation;

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

        [Parameter(ParameterSetName = "AttachExisting",
            Mandatory = true,
            ValueFromPipeline = true,
            Position = 0)]
        public ScriptBlockOutputSubscriber InputObject
        {
            get { return this.inputObject; }
            set { this.inputObject = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteDebug
        {
            get { return this.onWriteDebug; }
            set { this.onWriteDebug = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteError
        {
            get { return this.onWriteError; }
            set { this.onWriteError = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteOutput
        {
            get { return this.onWriteOutput; }
            set { this.onWriteOutput = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteVerbose
        {
            get { return this.onWriteVerbose; }
            set { this.onWriteVerbose = value; }
        }

        [Parameter(ParameterSetName = "New")]
        public ScriptBlock OnWriteWarning
        {
            get { return this.onWriteWarning; }
            set { this.onWriteWarning = value; }
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
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore UnusedMember.Global