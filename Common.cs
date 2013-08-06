using System;

// ReSharper disable UnusedMember.Global

namespace PSLogging
{
    [Flags]
    public enum StreamType
    {
        None = 0,
        Output = 1,
        Verbose = 2,
        Warning = 4,
        Error = 8,
        Debug = 16,
        All = Output | Verbose | Warning | Error | Debug
    }
}

// ReSharper restore UnusedMember.Global