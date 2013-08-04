namespace PSLogging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    public class LogFile : HostIOSubscriberBase
    {
        #region Constants

        private const string DateTimeFormat = "r";

        #endregion

        #region Fields

        private string _fileName;
        private string _path;

        #endregion

        #region Public Properties

        public string Path
        {
            get
            {
                return System.IO.Path.Combine(_path, _fileName);
            }
        }

        public StreamType Streams { get; set; }
        public ScriptBlock ErrorCallback { get; set; }

        #endregion

        #region Constructors and Destructors

        public LogFile(string filename, StreamType streams, ScriptBlock errorCallback)
        {
            _fileName = System.IO.Path.GetFileName(filename);
            _path = System.IO.Path.GetDirectoryName(filename);

            Streams = streams;
            ErrorCallback = errorCallback;
        }

        public LogFile(string filename)
            : this(filename, StreamType.All, (ScriptBlock)null)
        {
        }

        public LogFile(string filename, ScriptBlock errorCallback)
            : this(filename, StreamType.All, errorCallback)
        {
        }

        public LogFile(string filename, StreamType streams)
            : this(filename, streams, (ScriptBlock)null)
        {
        }

        #endregion

        #region Private Methods

        private void CheckDirectory()
        {
            if (!String.IsNullOrEmpty(_path) && !Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        #endregion

        #region Public Methods and Operators

        //
        // IPSOutputSubscriber
        //

        //
        // I'm not sure yet how best to handle errors here.  I don't want exceptions thrown from these methods,
        // or we may screw up PowerShell.  Maybe give the class an option to attach ScriptBlock handlers for
        // error conditions, giving the calling script control over how it wants to handle (or ignore) them.
        //

        public override void WriteDebug(string message)
        {
            if ((Streams & StreamType.Debug) == StreamType.Debug)
            {
                try
                {
                    CheckDirectory();
                    if (message != String.Empty)
                    {
                        message = String.Format("{0,-29} - [D] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        HostIOInterceptor.GetInterceptor().Paused = true;
                        ErrorCallback.Invoke(new object[] { this, e });
                        HostIOInterceptor.GetInterceptor().Paused = false;
                    }
                }
            }
        }

        public override void WriteError(string message)
        {
            if ((Streams & StreamType.Error) == StreamType.Error)
            {
                if (message == null) message = String.Empty;

                try
                {
                    CheckDirectory();
                    if (message.Trim() != String.Empty)
                    {
                        message = String.Format("{0,-29} - [E] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        HostIOInterceptor.GetInterceptor().Paused = true;
                        ErrorCallback.Invoke(new object[] { this, e });
                        HostIOInterceptor.GetInterceptor().Paused = false;
                    }
                }
            }
        }

        public override void WriteOutput(string message)
        {
            if ((Streams & StreamType.Output) == StreamType.Output)
            {
                if (message == null) message = String.Empty;

                try
                {
                    CheckDirectory();
                    if (message.Trim() != String.Empty)
                    {
                        message = String.Format("{0,-29} - {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        HostIOInterceptor.GetInterceptor().Paused = true;
                        ErrorCallback.Invoke(new object[] { this, e });
                        HostIOInterceptor.GetInterceptor().Paused = false;
                    }
                }
            }
        }

        public override void WriteHost(string message)
        {
            if ((Streams & StreamType.Output) == StreamType.Output)
            {
                if (message == null) message = String.Empty;

                try
                {
                    CheckDirectory();
                    if (message.Trim() != String.Empty)
                    {
                        message = String.Format("{0,-29} - {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        HostIOInterceptor.GetInterceptor().Paused = true;
                        ErrorCallback.Invoke(new object[] { this, e });
                        HostIOInterceptor.GetInterceptor().Paused = false;
                    }
                }
            }
        }


        public override void WriteVerbose(string message)
        {
            if ((Streams & StreamType.Verbose) == StreamType.Verbose)
            {
                if (message == null) message = String.Empty;

                try
                {
                    CheckDirectory();
                    if (message.Trim() != String.Empty)
                    {
                        message = String.Format("{0,-29} - [V] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        HostIOInterceptor.GetInterceptor().Paused = true;
                        ErrorCallback.Invoke(new object[] { this, e });
                        HostIOInterceptor.GetInterceptor().Paused = false;
                    }
                }
            }
        }

        public override void WriteWarning(string message)
        {
            if ((Streams & StreamType.Warning) == StreamType.Warning)
            {
                if (message == null) message = String.Empty;

                try
                {
                    CheckDirectory();
                    if (message.Trim() != String.Empty)
                    {
                        message = String.Format("{0,-29} - [W] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                    }

                    File.AppendAllText(System.IO.Path.Combine(_path, _fileName), message);
                }
                catch (Exception e)
                {
                    if (ErrorCallback != null)
                    {
                        try
                        {
                            HostIOInterceptor.GetInterceptor().Paused = true;
                            ErrorCallback.Invoke(new object[] { this, e });
                            HostIOInterceptor.GetInterceptor().Paused = false;
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion
    }
}