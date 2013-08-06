// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging
{
    using System.Management.Automation;

    public class ScriptBlockOutputSubscriber : HostIoSubscriberBase
    {
        public ScriptBlockOutputSubscriber(ScriptBlock onWriteOutput,
                                           ScriptBlock onWriteDebug,
                                           ScriptBlock onWriteVerbose,
                                           ScriptBlock onWriteError,
                                           ScriptBlock onWriteWarning)
        {
            this.OnWriteOutput = onWriteOutput;
            this.OnWriteDebug = onWriteDebug;
            this.OnWriteVerbose = onWriteVerbose;
            this.OnWriteError = onWriteError;
            this.OnWriteWarning = onWriteWarning;
        }

        public ScriptBlockOutputSubscriber() : this(null, null, null, null, null) {}

        public ScriptBlock OnWriteDebug { get; set; }
        public ScriptBlock OnWriteOutput { get; set; }
        public ScriptBlock OnWriteError { get; set; }
        public ScriptBlock OnWriteVerbose { get; set; }
        public ScriptBlock OnWriteWarning { get; set; }

        public override void WriteDebug(string message)
        {
            if (this.OnWriteDebug != null)
            {
                this.OnWriteDebug.Invoke(message);
            }
        }

        public override void WriteError(string message)
        {
            if (this.OnWriteError != null)
            {
                this.OnWriteError.Invoke(message);
            }
        }

        public override void WriteOutput(string message)
        {
            if (this.OnWriteOutput != null)
            {
                this.OnWriteOutput.Invoke(message);
            }
        }

        public override void WriteVerbose(string message)
        {
            if (this.OnWriteVerbose != null)
            {
                this.OnWriteVerbose.Invoke(message);
            }
        }

        public override void WriteWarning(string message)
        {
            if (this.OnWriteWarning != null)
            {
                this.OnWriteWarning.Invoke(message);
            }
        }
    }
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedMember.Global