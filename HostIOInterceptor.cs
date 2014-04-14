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
            this.externalUI = null;
            this.subscribers = new List<WeakReference>();
            this.writeCache = new StringBuilder();
            this.paused = false;
            this.host = null;
        }

        #endregion

        #region Properties
        
        public bool Paused
        {
            get { return this.paused; }
            set { this.paused = value; }
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return this.externalUI == null ? null : this.externalUI.RawUI;
            }
        }

        public IEnumerable<IHostIOSubscriber> Subscribers
        {
            get
            {
                foreach (WeakReference reference in this.subscribers)
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
            foreach (WeakReference reference in this.subscribers)
            {
                if (reference.Target == subscriber)
                {
                    return;
                }
            }

            this.subscribers.Add(new WeakReference(subscriber));
        }

        public void AttachToHost(PSHost host)
        {
            if (this.host != null) { return; }

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            object uiRef = host.GetType().GetField("internalUIRef", flags).GetValue(host);
            object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);

            FieldInfo externalUIField = ui.GetType().GetField("externalUI", flags);

            this.externalUI = (PSHostUserInterface)externalUIField.GetValue(ui);
            externalUIField.SetValue(ui, this);
            this.host = host;
        }

        public void DetachFromHost()
        {
            if (this.host == null) { return; }

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            object uiRef = this.host.GetType().GetField("internalUIRef", flags).GetValue(this.host);
            object ui = uiRef.GetType().GetProperty("Value", flags).GetValue(uiRef, null);

            FieldInfo externalUIField = ui.GetType().GetField("externalUI", flags);

            if (externalUIField.GetValue(ui) == this)
            {
                externalUIField.SetValue(ui, this.externalUI);
            }

            this.externalUI = null;
            this.host = null;
        }

        public override Dictionary<string, PSObject> Prompt(string caption,
                                                            string message,
                                                            Collection<FieldDescription> descriptions)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            Dictionary<string, PSObject> result = this.externalUI.Prompt(caption, message, descriptions);

            this.SendToSubscribers("Prompt", result);

            return result;
        }

        public override int PromptForChoice(string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            int result = this.externalUI.PromptForChoice(caption, message, choices, defaultChoice);

            this.SendToSubscribers("ChoicePrompt", choices[result]);

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = this.externalUI.PromptForCredential(caption, message, userName, targetName);

            this.SendToSubscribers("CredentialPrompt", result);

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName,
                                                         PSCredentialTypes allowedCredentialTypes,
                                                         PSCredentialUIOptions options)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = this.externalUI.PromptForCredential(caption,
                                                                       message,
                                                                       userName,
                                                                       targetName,
                                                                       allowedCredentialTypes,
                                                                       options);

            this.SendToSubscribers("CredentialPrompt", result);

            return result;
        }

        public override string ReadLine()
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string result = this.externalUI.ReadLine();

            this.SendToSubscribers("ReadFromHost", result);

            return result;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            return this.externalUI.ReadLineAsSecureString();
        }

        public void RemoveAllSubscribers()
        {
            this.subscribers.Clear();
        }

        public void RemoveSubscriber(IHostIOSubscriber subscriber)
        {
            var matches = new List<WeakReference>();

            foreach (WeakReference reference in this.subscribers)
            {
                if (reference.Target == subscriber)
                {
                    matches.Add(reference);
                }
            }

            foreach (WeakReference reference in matches)
            {
                this.subscribers.Remove(reference);
            }
        }

        public override void Write(string value)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            this.externalUI.Write(value);

            if (!this.paused)
            {
                this.writeCache.Append(value);
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            this.externalUI.Write(foregroundColor, backgroundColor, value);

            if (!this.paused)
            {
                this.writeCache.Append(value);
            }
        }

        public override void WriteDebugLine(string message)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteDebug", line.TrimEnd() + "\r\n");
            }

            this.externalUI.WriteDebugLine(message);
        }

        public override void WriteErrorLine(string message)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteError", line.TrimEnd() + "\r\n");
            }

            this.externalUI.WriteErrorLine(message);
        }

        public override void WriteLine()
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = this.writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.externalUI.WriteLine();
        }

        public override void WriteLine(string value)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (this.writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.externalUI.WriteLine(value);
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (this.writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.externalUI.WriteLine(foregroundColor, backgroundColor, value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            this.SendToSubscribers("WriteProgress", sourceId, record);

            this.externalUI.WriteProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteVerbose", line.TrimEnd() + "\r\n");
            }

            this.externalUI.WriteVerboseLine(message);
        }

        public override void WriteWarningLine(string message)
        {
            if (this.externalUI == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteWarning", line.TrimEnd() + "\r\n");
            }

            this.externalUI.WriteWarningLine(message);
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

            if (this.paused)
            {
                return;
            }

            MethodInfo method = typeof(IHostIOSubscriber).GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException(
                    "Method '" + methodName + "' does not exist in the IHostIoSubscriber interface.",
                    "methodName");
            }

            var deadReferences = new List<WeakReference>();

            foreach (WeakReference reference in this.subscribers)
            {
                var subscriber = (IHostIOSubscriber) reference.Target;
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
                this.subscribers.Remove(reference);
            }
        }

        #endregion
    }
}

// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMethodReturnValue.Global