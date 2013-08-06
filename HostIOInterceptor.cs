using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Security;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace PSLogging
{
    public class HostIoInterceptor : PSHostUserInterface
    {
        #region Static Fields

        private static readonly HostIoInterceptor Instance = new HostIoInterceptor();

        #endregion

        #region Static Methods

        public static HostIoInterceptor GetInterceptor()
        {
            return Instance;
        }

        #endregion

        #region Fields

        private readonly List<WeakReference> _subscribers;
        private readonly StringBuilder _writeCache;
        private bool _paused;
        private PSHostUserInterface _psInterface;

        #endregion

        #region Constructors and Destructors

        private HostIoInterceptor()
        {
            _psInterface = null;
            _subscribers = new List<WeakReference>();
            _writeCache = new StringBuilder();
            _paused = false;
        }

        #endregion

        #region Private Methods

        private void SendToSubscribers(string methodName, params object[] args)
        {
            // Refactored the duplicate code that enumerates the _subscribers list and removes dead
            // references in to this method.  It uses Reflection to invoke the methods on
            // subscriber objects because I'm not sure how to accomplish the same thing using
            // delegates (assuming it is even possible), when the target methods have different
            // signatures.

            if (_paused)
            {
                return;
            }

            MethodInfo method = typeof (IHostIoSubscriber).GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException(
                    "Method '" + methodName + "' does not exist in the IHostIoSubscriber interface.",
                    "methodName");
            }

            var deadReferences = new List<WeakReference>();

            foreach (WeakReference reference in _subscribers)
            {
                var subscriber = (IHostIoSubscriber) reference.Target;
                if (subscriber == null)
                {
                    deadReferences.Add(reference);
                }
                else
                {
                    method.Invoke(subscriber, args);
                }
            }

            foreach (WeakReference reference in deadReferences)
            {
                _subscribers.Remove(reference);
            }
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
            set
            {
                if (value != null && value != _psInterface)
                {
                    _psInterface = value;
                }
            }
        }

        public IEnumerable<IHostIoSubscriber> Subscribers
        {
            get
            {
                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber) reference.Target;
                    if (subscriber != null)
                    {
                        yield return subscriber;
                    }
                }
            }
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return _psInterface.RawUI; }
        }

        #endregion

        #region Public Methods and Operators

        public void AddSubscriber(IHostIoSubscriber subscriber)
        {
            foreach (WeakReference reference in _subscribers)
            {
                if (reference.Target == subscriber)
                {
                    return;
                }
            }

            _subscribers.Add(new WeakReference(subscriber));
        }

        public void RemoveSubscriber(IHostIoSubscriber subscriber)
        {
            var matches = new List<WeakReference>();

            foreach (WeakReference reference in _subscribers)
            {
                if (reference.Target == subscriber)
                {
                    matches.Add(reference);
                }
            }

            foreach (WeakReference reference in matches)
            {
                _subscribers.Remove(reference);
            }
        }

        public override Dictionary<string, PSObject> Prompt(string caption,
                                                            string message,
                                                            Collection<FieldDescription> descriptions)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            Dictionary<string, PSObject> result = _psInterface.Prompt(caption, message, descriptions);

            SendToSubscribers("Prompt", result);

            return result;
        }

        public override int PromptForChoice(string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            int result = _psInterface.PromptForChoice(caption, message, choices, defaultChoice);

            SendToSubscribers("ChoicePrompt", choices[result]);

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = _psInterface.PromptForCredential(caption, message, userName, targetName);

            SendToSubscribers("CredentialPrompt", result);

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName,
                                                         PSCredentialTypes allowedCredentialTypes,
                                                         PSCredentialUIOptions options)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = _psInterface.PromptForCredential(caption,
                                                                   message,
                                                                   userName,
                                                                   targetName,
                                                                   allowedCredentialTypes,
                                                                   options);

            SendToSubscribers("CredentialPrompt", result);

            return result;
        }

        public override string ReadLine()
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string result = _psInterface.ReadLine();

            SendToSubscribers("ReadFromHost", result);

            return result;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            return _psInterface.ReadLineAsSecureString();
        }

        public override void Write(string value)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            _psInterface.Write(value);

            if (!_paused)
            {
                _writeCache.Append(value);
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            _psInterface.Write(foregroundColor, backgroundColor, value);

            if (!_paused)
            {
                _writeCache.Append(value);
            }
        }

        public override void WriteDebugLine(string message)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteDebug", line.TrimEnd() + "\r\n");
            }

            _psInterface.WriteDebugLine(message);
        }

        public override void WriteErrorLine(string message)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteError", line.TrimEnd() + "\r\n");
            }

            _psInterface.WriteErrorLine(message);
        }

        public override void WriteLine()
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = _writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            _writeCache.Length = 0;
            _psInterface.WriteLine();
        }

        public override void WriteLine(string value)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (_writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            _writeCache.Length = 0;
            _psInterface.WriteLine(value);
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (_writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            _writeCache.Length = 0;
            _psInterface.WriteLine(foregroundColor, backgroundColor, value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            SendToSubscribers("WriteProgress", sourceId, record);

            _psInterface.WriteProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteVerbose", line.TrimEnd() + "\r\n");
            }

            _psInterface.WriteVerboseLine(message);
        }

        public override void WriteWarningLine(string message)
        {
            if (_psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                SendToSubscribers("WriteWarning", line.TrimEnd() + "\r\n");
            }

            _psInterface.WriteWarningLine(message);
        }

        #endregion
    }
}

// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMethodReturnValue.Global