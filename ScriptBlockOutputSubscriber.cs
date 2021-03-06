﻿// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging
{
    using System.Management.Automation;

    public class ScriptBlockOutputSubscriber : HostIOSubscriberBase
    {
        public ScriptBlockOutputSubscriber(ScriptBlock onWriteOutput,
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

        public ScriptBlockOutputSubscriber() : this(null, null, null, null, null) {}

        public ScriptBlock OnWriteDebug { get; set; }
        public ScriptBlock OnWriteOutput { get; set; }
        public ScriptBlock OnWriteError { get; set; }
        public ScriptBlock OnWriteVerbose { get; set; }
        public ScriptBlock OnWriteWarning { get; set; }

        public override void WriteDebug(string message)
        {
            if (OnWriteDebug != null)
            {
                OnWriteDebug.Invoke(message);
            }
        }

        public override void WriteError(string message)
        {
            if (OnWriteError != null)
            {
                OnWriteError.Invoke(message);
            }
        }

        public override void WriteOutput(string message)
        {
            if (OnWriteOutput != null)
            {
                OnWriteOutput.Invoke(message);
            }
        }

        public override void WriteVerbose(string message)
        {
            if (OnWriteVerbose != null)
            {
                OnWriteVerbose.Invoke(message);
            }
        }

        public override void WriteWarning(string message)
        {
            if (OnWriteWarning != null)
            {
                OnWriteWarning.Invoke(message);
            }
        }
    }
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedMember.Global