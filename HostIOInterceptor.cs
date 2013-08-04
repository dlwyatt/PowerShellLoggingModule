namespace PSLogging
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Security;
    using System.Text;

    public class HostIoInterceptor : PSHostUserInterface
    {
        #region Static Fields

        private readonly static HostIoInterceptor Instance = new HostIoInterceptor();

        #endregion

        #region Static Methods

        public static HostIoInterceptor GetInterceptor()
        {
            return Instance;
        }

        #endregion

        #region Fields

        private PSHostUserInterface _psInterface;

        private readonly List<IHostIoSubscriber> _subscribers;
        private readonly StringBuilder _writeCache;

        private bool _paused;

        #endregion

        #region Constructors and Destructors

        private HostIoInterceptor()
        {
            _psInterface = null;
            _subscribers = new List<IHostIoSubscriber>();
            _writeCache = new StringBuilder();
        }

        #endregion

        #region Public Properties

        public bool Paused
        {
            get { return _paused; }
            set { _paused = value; }
        }

        public PSHostUserInterface HostUi
        {
            get { return _psInterface; }
            set { if (value != null && value != _psInterface) _psInterface = value; }
        }

        public IEnumerable<IHostIoSubscriber> Subscribers
        {
            get
            {
                return _subscribers;
            }
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return _psInterface.RawUI;
            }
        }

        #endregion

        #region Public Methods and Operators

        public void AddSubscriber(IHostIoSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber)) _subscribers.Add(subscriber);
        }

        public bool RemoveSubscriber(IHostIoSubscriber subscriber)
        {
            return _subscribers.Remove(subscriber);
        }

        public override Dictionary<string, PSObject> Prompt(
            string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            Dictionary<string, PSObject> result = _psInterface.Prompt(caption, message, descriptions);

            if (!_paused)
            {
                foreach (IHostIoSubscriber subscriber in _subscribers)
                {
                    subscriber.Prompt(result);
                }
            }

            return result;
        }

        public override int PromptForChoice(
            string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            int result = _psInterface.PromptForChoice(caption, message, choices, defaultChoice);

            if (!_paused)
            {
                foreach (IHostIoSubscriber subscriber in _subscribers)
                {
                    subscriber.ChoicePrompt(choices[result]);
                }
            }

            return result;
        }

        public override PSCredential PromptForCredential(
            string caption, string message, string userName, string targetName)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            PSCredential result = _psInterface.PromptForCredential(caption, message, userName, targetName);

            if (!_paused)
            {
                foreach (IHostIoSubscriber subscriber in _subscribers)
                {
                    subscriber.CredentialPrompt(result);
                }
            }

            return result;
        }

        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            PSCredential result = _psInterface.PromptForCredential(
                caption, message, userName, targetName, allowedCredentialTypes, options);

            if (!_paused)
            {
                foreach (IHostIoSubscriber subscriber in _subscribers)
                {
                    subscriber.CredentialPrompt(result);
                }
            }

            return result;
        }

        public override string ReadLine()
        {
            if (_psInterface == null) throw new InvalidOperationException();

            string result = _psInterface.ReadLine();

            if (!_paused)
            {
                foreach (IHostIoSubscriber subscriber in _subscribers)
                {
                    subscriber.ReadFromHost(result);
                }
            }

            return result;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (_psInterface == null) throw new InvalidOperationException();
            return _psInterface.ReadLineAsSecureString();
        }

        public override void Write(string value)
        {
            if (_psInterface != null)
            {
                _psInterface.Write(value);

                if (!_paused)
                {
                    _writeCache.Append(value);
                }
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (_psInterface != null)
            {
                _psInterface.Write(foregroundColor, backgroundColor, value);

                if (!_paused)
                {
                    _writeCache.Append(value);
                }
            }
        }

        public override void WriteDebugLine(string message)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteDebug(line + "\r\n");
                        }
                    }
                }

                _psInterface.WriteDebugLine(message);
            }
        }

        public override void WriteErrorLine(string message)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteError(line + "\r\n");
                        }
                    }
                }

                _psInterface.WriteErrorLine(message);
            }
        }

        public override void WriteLine()
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = _writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteOutput(line + "\r\n");
                        }
                    }

                    _writeCache.Length = 0;
                }

                _psInterface.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = (_writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteOutput(line + "\r\n");
                        }
                    }

                    _writeCache.Length = 0;
                }
                _psInterface.WriteLine(value);
            }
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = (_writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteOutput(line + "\r\n");
                        }
                    }

                    _writeCache.Length = 0;
                }
                _psInterface.WriteLine(foregroundColor, backgroundColor, value);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        subscriber.WriteProgress(sourceId, record);
                    }
                }

                _psInterface.WriteProgress(sourceId, record);
            }
        }

        public override void WriteVerboseLine(string message)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteVerbose(line + "\r\n");
                        }
                    }
                }

                _psInterface.WriteVerboseLine(message);
            }
        }

        public override void WriteWarningLine(string message)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (IHostIoSubscriber subscriber in _subscribers)
                    {
                        foreach (string line in lines)
                        {
                            subscriber.WriteWarning(line + "\r\n");
                        }
                    }
                }

                _psInterface.WriteWarningLine(message);
            }
        }

        #endregion
    }
}