using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace PSLogging
{
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

        private readonly List<WeakReference> _subscribers;
        private readonly StringBuilder _writeCache;

        private bool _paused;

        #endregion

        #region Constructors and Destructors

        private HostIoInterceptor()
        {
            _psInterface = null;
            _subscribers = new List<WeakReference>();
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
                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber != null) yield return subscriber;
                }
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
                if (reference.Target == subscriber) matches.Add(reference);
            }

            foreach (WeakReference reference in matches)
            {
                _subscribers.Remove(reference);
            }
        }

        public override Dictionary<string, PSObject> Prompt(
            string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            var result = _psInterface.Prompt(caption, message, descriptions);

            if (!_paused)
            {
                var deadReferences = new List<WeakReference>();

                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber == null)
                    {
                        deadReferences.Add(reference);
                    }
                    else
                    {
                        subscriber.Prompt(result);
                    }
                }

                foreach (WeakReference reference in deadReferences)
                {
                    _subscribers.Remove(reference);
                }
            }

            return result;
        }

        public override int PromptForChoice(
            string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            var result = _psInterface.PromptForChoice(caption, message, choices, defaultChoice);

            if (!_paused)
            {
                var deadReferences = new List<WeakReference>();

                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber == null)
                    {
                        deadReferences.Add(reference);
                    }
                    else
                    {
                        subscriber.ChoicePrompt(choices[result]);
                    }
                }

                foreach (WeakReference reference in deadReferences)
                {
                    _subscribers.Remove(reference);
                }
            }

            return result;
        }

        public override PSCredential PromptForCredential(
            string caption, string message, string userName, string targetName)
        {
            if (_psInterface == null) throw new InvalidOperationException();

            var result = _psInterface.PromptForCredential(caption, message, userName, targetName);

            if (!_paused)
            {
                var deadReferences = new List<WeakReference>();

                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber == null)
                    {
                        deadReferences.Add(reference);
                    }
                    else
                    {
                        subscriber.CredentialPrompt(result);
                    }
                }

                foreach (WeakReference reference in deadReferences)
                {
                    _subscribers.Remove(reference);
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

            var result = _psInterface.PromptForCredential(
                caption, message, userName, targetName, allowedCredentialTypes, options);

            if (!_paused)
            {
                var deadReferences = new List<WeakReference>();

                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber == null)
                    {
                        deadReferences.Add(reference);
                    }
                    else
                    {
                        subscriber.CredentialPrompt(result);
                    }
                }

                foreach (WeakReference reference in deadReferences)
                {
                    _subscribers.Remove(reference);
                }
            }

            return result;
        }

        public override string ReadLine()
        {
            if (_psInterface == null) throw new InvalidOperationException();

            var result = _psInterface.ReadLine();

            if (!_paused)
            {
                var deadReferences = new List<WeakReference>();

                foreach (WeakReference reference in _subscribers)
                {
                    var subscriber = (IHostIoSubscriber)reference.Target;
                    if (subscriber == null)
                    {
                        deadReferences.Add(reference);
                    }
                    else
                    {
                        subscriber.ReadFromHost(result);
                    }
                }

                foreach (WeakReference reference in deadReferences)
                {
                    _subscribers.Remove(reference);
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
                    var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteDebug(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
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
                    var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteError(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
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
                    var lines = _writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteOutput(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
                    }
                }

                _writeCache.Length = 0;
                _psInterface.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    var lines = (_writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteOutput(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
                    }
                }
                
                _writeCache.Length = 0;
                _psInterface.WriteLine(value);
            }
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    var lines = (_writeCache + value).Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
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
                            foreach (string line in lines)
                            {
                                subscriber.WriteOutput(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
                    }
                }

                _writeCache.Length = 0;
                _psInterface.WriteLine(foregroundColor, backgroundColor, value);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (_psInterface != null)
            {
                if (!_paused)
                {
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            subscriber.WriteProgress(sourceId, record);
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
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
                    var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteVerbose(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
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
                    var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var deadReferences = new List<WeakReference>();

                    foreach (WeakReference reference in _subscribers)
                    {
                        var subscriber = (IHostIoSubscriber)reference.Target;
                        if (subscriber == null)
                        {
                            deadReferences.Add(reference);
                        }
                        else
                        {
                            foreach (string line in lines)
                            {
                                subscriber.WriteWarning(line + "\r\n");
                            }
                        }
                    }

                    foreach (WeakReference reference in deadReferences)
                    {
                        _subscribers.Remove(reference);
                    }
                }

                _psInterface.WriteWarningLine(message);
            }
        }

        #endregion
    }
}

// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMethodReturnValue.Global
