using System.Management.Automation;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging
{
    public class ScriptBlockOutputSubscriber : HostIoSubscriberBase
    {
        public ScriptBlock OnWriteDebug { get; set; }
        public ScriptBlock OnWriteOutput { get; set; }
        public ScriptBlock OnWriteError { get; set; }
        public ScriptBlock OnWriteVerbose { get; set; }
        public ScriptBlock OnWriteWarning { get; set; }

        public ScriptBlockOutputSubscriber(
            ScriptBlock onWriteOutput,
            ScriptBlock onWriteDebug,
            ScriptBlock onWriteVerbose,
            ScriptBlock onWriteError,
            ScriptBlock onWriteWarning)
        {
            OnWriteOutput = onWriteOutput;
            OnWriteDebug = onWriteDebug;
            OnWriteVerbose = onWriteVerbose;
            OnWriteError = onWriteError;
            OnWriteWarning = onWriteWarning;
        }

        public ScriptBlockOutputSubscriber()
            : this(null, null, null, null, null)
        { }

        public override void WriteDebug(string message)
        {
            if (OnWriteDebug != null) OnWriteDebug.Invoke(new object[] { message });
        }

        public override void WriteOutput(string message)
        {
            if (OnWriteOutput != null) OnWriteOutput.Invoke(new object[] { message });
        }

        public override void WriteError(string message)
        {
            if (OnWriteError != null) OnWriteError.Invoke(new object[] { message });
        }

        public override void WriteVerbose(string message)
        {
            if (OnWriteVerbose != null) OnWriteVerbose.Invoke(new object[] { message });
        }

        public override void WriteWarning(string message)
        {
            if (OnWriteWarning != null) OnWriteWarning.Invoke(new object[] { message });
        }
    }
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedMember.Global
