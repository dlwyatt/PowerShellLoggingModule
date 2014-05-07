// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PSLogging
{
    using System;
    using System.IO;
    using System.Management.Automation;

    public class LogFile : HostIOSubscriberBase
    {
        #region Fields

        private const string DateTimeFormat = "r";
        private readonly string fileName;
        private readonly string path;

        #endregion

        #region Constructors and Destructors

        public LogFile(string filename, StreamType streams = StreamType.All, ScriptBlock errorCallback = null)
        {
            fileName = System.IO.Path.GetFileName(filename);
            path = System.IO.Path.GetDirectoryName(filename);

            Streams = streams;
            ErrorCallback = errorCallback;
        }

        #endregion
        
        #region Properties

        public ScriptBlock ErrorCallback { get; set; }
        
        public string Path
        {
            get { return System.IO.Path.Combine(path, fileName); }
        }

        public StreamType Streams { get; set; }

        #endregion

        #region Public Methods and Operators

        //
        // IPSOutputSubscriber
        //

        public override void WriteDebug(string message)
        {
            if ((Streams & StreamType.Debug) != StreamType.Debug)
            {
                return;
            }
            if (message == null)
            {
                message = String.Empty;
            }

            try
            {
                CheckDirectory();
                if (message != String.Empty)
                {
                    message = String.Format("{0,-29} - [D] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                }

                File.AppendAllText(System.IO.Path.Combine(path, fileName), message);
            }
            catch (Exception e)
            {
                ReportError(e);
            }
        }

        public override void WriteError(string message)
        {
            if ((Streams & StreamType.Error) != StreamType.Error)
            {
                return;
            }
            if (message == null)
            {
                message = String.Empty;
            }

            try
            {
                CheckDirectory();
                if (message.Trim() != String.Empty)
                {
                    message = String.Format("{0,-29} - [E] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                }

                File.AppendAllText(System.IO.Path.Combine(path, fileName), message);
            }
            catch (Exception e)
            {
                ReportError(e);
            }
        }

        public override void WriteOutput(string message)
        {
            if ((Streams & StreamType.Output) != StreamType.Output)
            {
                return;
            }
            if (message == null)
            {
                message = String.Empty;
            }

            try
            {
                CheckDirectory();
                if (message.Trim() != String.Empty)
                {
                    message = String.Format("{0,-29} - {1}", DateTime.Now.ToString(DateTimeFormat), message);
                }

                File.AppendAllText(System.IO.Path.Combine(path, fileName), message);
            }
            catch (Exception e)
            {
                ReportError(e);
            }
        }

        public override void WriteVerbose(string message)
        {
            if ((Streams & StreamType.Verbose) != StreamType.Verbose)
            {
                return;
            }
            if (message == null)
            {
                message = String.Empty;
            }

            try
            {
                CheckDirectory();
                if (message.Trim() != String.Empty)
                {
                    message = String.Format("{0,-29} - [V] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                }

                File.AppendAllText(System.IO.Path.Combine(path, fileName), message);
            }
            catch (Exception e)
            {
                ReportError(e);
            }
        }

        public override void WriteWarning(string message)
        {
            if ((Streams & StreamType.Warning) != StreamType.Warning)
            {
                return;
            }
            if (message == null)
            {
                message = String.Empty;
            }

            try
            {
                CheckDirectory();
                if (message.Trim() != String.Empty)
                {
                    message = String.Format("{0,-29} - [W] {1}", DateTime.Now.ToString(DateTimeFormat), message);
                }

                File.AppendAllText(System.IO.Path.Combine(path, fileName), message);
            }
            catch (Exception e)
            {
                ReportError(e);
            }
        }

        #endregion

        #region Private Methods

        private void CheckDirectory()
        {
            if (!String.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void ReportError(Exception e)
        {
            if (ErrorCallback == null)
            {
                return;
            }

            // ReSharper disable once EmptyGeneralCatchClause
            try
            {
                HostIOInterceptor.Instance.Paused = true;
                ErrorCallback.Invoke(this, e);
                HostIOInterceptor.Instance.Paused = false;
            }
            catch { }
        }

        #endregion
    }
}

// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedMember.Global