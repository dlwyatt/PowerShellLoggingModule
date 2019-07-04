// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace PSLogging
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Reflection;
    using System.Security;
    using System.Text;

    public class HostIOInterceptor : PSHostUserInterface
    {
        #region Fields

        private PSHostUserInterface externalUI;
        private PSHost host;
        public static readonly HostIOInterceptor Instance = new HostIOInterceptor();
        private bool paused;
        private readonly List<WeakReference> subscribers;
        private readonly StringBuilder writeCache;

        #endregion

        #region Constructors and Destructors

        private HostIOInterceptor()
        {
            externalUI = null;
            subscribers = new List<WeakReference>();
            writeCache = new StringBuilder();
            paused = false;
            host = null;
        }

        #endregion

        #region Properties

        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return externalUI == null ? null : externalUI.RawUI;
            }
        }

        public IEnumerable<IHostIOSubscriber> Subscribers
        {
            get
            {
                foreach (WeakReference reference in subscribers)
                {
                    var subscriber = (IHostIOSubscriber) reference.Target;
                    if (subscriber != null)
                    {
                        yield return subscriber;
                    }
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        public void AddSubscriber(IHostIOSubscriber subscriber)
        {
            foreach (WeakReference reference in subscribers)
            {
                if (reference.Target == subscriber)
                {
                    return;
                }
            }

            subscribers.Add(new WeakReference(subscriber));
        }

        public void AttachToHost(PSHost host)
        {
            if (this.host != null) { return; }
            if (host == null) { return; }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            // When PowerShell went open source, they renamed the private variables to have _underbarPrefixes
            if (host.Version >= new Version(6, 0))
            {
                object uiRef = host.GetType().GetField("_internalUIRef", flags)?.GetValue(host);
                object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);
                FieldInfo externalUIField = ui.GetType().GetField("_externalUI", flags);
                externalUI = (PSHostUserInterface)externalUIField.GetValue(ui);
                externalUIField.SetValue(ui, this);
            }
            else
            {
                // Try the WindowsPowerShell version:
                object uiRef = host.GetType().GetField("internalUIRef", flags).GetValue(host);
                object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);
                FieldInfo externalUIField = ui.GetType().GetField("externalUI", flags);
                externalUI = (PSHostUserInterface)externalUIField.GetValue(ui);
                externalUIField.SetValue(ui, this);
            }

            this.host = host;
        }

        public void DetachFromHost()
        {
            if (host == null) { return; }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            // When PowerShell went open source, they renamed the private variables to have _underbarPrefixes
            if (host.Version >= new Version(6, 0))
            {
                object uiRef = host.GetType().GetField("_internalUIRef", flags)?.GetValue(host);
                object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);
                FieldInfo externalUIField = ui.GetType().GetField("_externalUI", flags);
                if (externalUIField.GetValue(ui) == this)
                {
                    externalUIField.SetValue(ui, externalUI);
                }
            }
            else
            {
                // Try the WindowsPowerShell version:
                object uiRef = host.GetType().GetField("internalUIRef", flags).GetValue(host);
                object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);
                FieldInfo externalUIField = ui.GetType().GetField("externalUI", flags);
                if (externalUIField.GetValue(ui) == this)
                {
                    externalUIField.SetValue(ui, externalUI);
                }
            }

            externalUI = null;
            host = null;
        }

        public override Dictionary<string, PSObject> Prompt(string caption,
                                                            string message,
                                                            Collection<FieldDescription> descriptions)
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to prompt user in headless session");
            }

            Dictionary<string, PSObject> result = externalUI.Prompt(caption, message, descriptions);

            SendToSubscribers(s => s.Prompt(result));

            return result;
        }

        public override int PromptForChoice(string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to prompt user for choice in headless session");
            }

            int result = externalUI.PromptForChoice(caption, message, choices, defaultChoice);

            SendToSubscribers(s => s.ChoicePrompt(choices[result]));

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName)
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to prompt user for credential in headless session");
            }

            PSCredential result = externalUI.PromptForCredential(caption, message, userName, targetName);

            SendToSubscribers(s => s.CredentialPrompt(result));

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName,
                                                         PSCredentialTypes allowedCredentialTypes,
                                                         PSCredentialUIOptions options)
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to prompt user for credential in headless session");
            }

            PSCredential result = externalUI.PromptForCredential(caption,
                                                                       message,
                                                                       userName,
                                                                       targetName,
                                                                       allowedCredentialTypes,
                                                                       options);

            SendToSubscribers(s => s.CredentialPrompt(result));

            return result;
        }

        public override string ReadLine()
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to ReadLine from host in headless session");
            }

            string result = externalUI.ReadLine();

            SendToSubscribers(s => s.ReadFromHost(result));

            return result;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (externalUI == null)
            {
                throw new InvalidOperationException("Unable to ReadLineAsSecureString from host in headless session");
            }

            return externalUI.ReadLineAsSecureString();
        }

        public void RemoveAllSubscribers()
        {
            subscribers.Clear();
        }

        public void RemoveSubscriber(IHostIOSubscriber subscriber)
        {
            var matches = new List<WeakReference>();

            foreach (WeakReference reference in subscribers)
            {
                if (reference.Target == subscriber)
                {
                    matches.Add(reference);
                }
            }

            foreach (WeakReference reference in matches)
            {
                subscribers.Remove(reference);
            }
        }

        public override void Write(string value)
        {
            if (externalUI != null)
            {
                externalUI.Write(value);
            }

            if (!paused)
            {
                writeCache.Append(value);
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (externalUI != null)
            {
                externalUI.Write(foregroundColor, backgroundColor, value);
            }

            if (!paused)
            {
                writeCache.Append(value);
            }
        }

        public override void WriteDebugLine(string message)
        {
            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteDebug(temp.TrimEnd() + "\r\n"));
            }

            if (externalUI != null)
            {
                externalUI.WriteDebugLine(message);
            }
        }

        public override void WriteErrorLine(string message)
        {
            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteError(temp.TrimEnd() + "\r\n"));
            }

            if (externalUI != null)
            {
                externalUI.WriteErrorLine(message);
            }
        }

        public override void WriteLine()
        {
            string[] lines = writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteOutput(temp.TrimEnd() + "\r\n"));
            }

            writeCache.Length = 0;
            if (externalUI != null) {
                externalUI.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            string[] lines = (writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteOutput(temp.TrimEnd() + "\r\n"));
            }

            writeCache.Length = 0;
            if (externalUI != null) {
                externalUI.WriteLine(value);
            }
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            string[] lines = (writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteOutput(temp.TrimEnd() + "\r\n"));
            }

            writeCache.Length = 0;
            if (externalUI != null){
                externalUI.WriteLine(foregroundColor, backgroundColor, value);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            SendToSubscribers(s => s.WriteProgress(sourceId, record));

            if (externalUI != null)
            {
                externalUI.WriteProgress(sourceId, record);
            }
        }

        public override void WriteVerboseLine(string message)
        {
            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteVerbose(temp.TrimEnd() + "\r\n"));
            }

            if (externalUI != null)
            {
                externalUI.WriteVerboseLine(message);
            }
        }

        public override void WriteWarningLine(string message)
        {
            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string temp = line;
                SendToSubscribers(s => s.WriteWarning(temp.TrimEnd() + "\r\n"));
            }

            if (externalUI != null)
            {
                externalUI.WriteWarningLine(message);
            }
        }

        #endregion

        #region Private Methods

        public void SendToSubscribers(Action<IHostIOSubscriber> action)
        {
            if (paused) { return; }

            var deadReferences = new List<WeakReference>();

            foreach (WeakReference reference in subscribers)
            {
                var subscriber = (IHostIOSubscriber) reference.Target;
                if (subscriber == null)
                {
                    deadReferences.Add(reference);
                }
                else
                {
                    action(subscriber);
                }
            }

            foreach (WeakReference reference in deadReferences)
            {
                subscribers.Remove(reference);
            }
        }

        #endregion
    }
}

// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMethodReturnValue.Global