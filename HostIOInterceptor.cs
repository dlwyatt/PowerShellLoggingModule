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

        private readonly List<WeakReference> subscribers;
        private readonly StringBuilder writeCache;
        private bool paused;
        private PSHostUserInterface psInterface;

        #endregion

        #region Constructors and Destructors

        private HostIoInterceptor()
        {
            this.psInterface = null;
            this.subscribers = new List<WeakReference>();
            this.writeCache = new StringBuilder();
            this.paused = false;
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

            MethodInfo method = typeof (IHostIoSubscriber).GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException(
                    "Method '" + methodName + "' does not exist in the IHostIoSubscriber interface.",
                    "methodName");
            }

            var deadReferences = new List<WeakReference>();

            foreach (WeakReference reference in this.subscribers)
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
                this.subscribers.Remove(reference);
            }
        }

        #endregion

        #region Public Properties

        public bool Paused
        {
            get { return this.paused; }
            set { this.paused = value; }
        }

        public PSHostUserInterface HostUi
        {
            get { return this.psInterface; }
            set
            {
                if (value != null && value != this.psInterface)
                {
                    this.psInterface = value;
                }
            }
        }

        public IEnumerable<IHostIoSubscriber> Subscribers
        {
            get
            {
                foreach (WeakReference reference in this.subscribers)
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
            get { return this.psInterface.RawUI; }
        }

        #endregion

        #region Public Methods and Operators

        public void AddSubscriber(IHostIoSubscriber subscriber)
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

        public void RemoveSubscriber(IHostIoSubscriber subscriber)
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

        public override Dictionary<string, PSObject> Prompt(string caption,
                                                            string message,
                                                            Collection<FieldDescription> descriptions)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            Dictionary<string, PSObject> result = this.psInterface.Prompt(caption, message, descriptions);

            this.SendToSubscribers("Prompt", result);

            return result;
        }

        public override int PromptForChoice(string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            int result = this.psInterface.PromptForChoice(caption, message, choices, defaultChoice);

            this.SendToSubscribers("ChoicePrompt", choices[result]);

            return result;
        }

        public override PSCredential PromptForCredential(string caption,
                                                         string message,
                                                         string userName,
                                                         string targetName)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = this.psInterface.PromptForCredential(caption, message, userName, targetName);

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
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            PSCredential result = this.psInterface.PromptForCredential(caption,
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
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string result = this.psInterface.ReadLine();

            this.SendToSubscribers("ReadFromHost", result);

            return result;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            return this.psInterface.ReadLineAsSecureString();
        }

        public override void Write(string value)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            this.psInterface.Write(value);

            if (!this.paused)
            {
                this.writeCache.Append(value);
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            this.psInterface.Write(foregroundColor, backgroundColor, value);

            if (!this.paused)
            {
                this.writeCache.Append(value);
            }
        }

        public override void WriteDebugLine(string message)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteDebug", line.TrimEnd() + "\r\n");
            }

            this.psInterface.WriteDebugLine(message);
        }

        public override void WriteErrorLine(string message)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteError", line.TrimEnd() + "\r\n");
            }

            this.psInterface.WriteErrorLine(message);
        }

        public override void WriteLine()
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = this.writeCache.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.psInterface.WriteLine();
        }

        public override void WriteLine(string value)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (this.writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.psInterface.WriteLine(value);
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = (this.writeCache + value).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteOutput", line.TrimEnd() + "\r\n");
            }

            this.writeCache.Length = 0;
            this.psInterface.WriteLine(foregroundColor, backgroundColor, value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            this.SendToSubscribers("WriteProgress", sourceId, record);

            this.psInterface.WriteProgress(sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteVerbose", line.TrimEnd() + "\r\n");
            }

            this.psInterface.WriteVerboseLine(message);
        }

        public override void WriteWarningLine(string message)
        {
            if (this.psInterface == null)
            {
                throw new InvalidOperationException();
            }

            string[] lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                this.SendToSubscribers("WriteWarning", line.TrimEnd() + "\r\n");
            }

            this.psInterface.WriteWarningLine(message);
        }

        #endregion
    }
}

// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedMethodReturnValue.Global