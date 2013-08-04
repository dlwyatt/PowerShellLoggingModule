namespace PSLogging
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    /// <summary>
    /// The Logger interface.
    /// </summary>
    public interface IHostIoSubscriber
    {
        #region Public Methods and Operators

        #region From MWalker Solution (unused by this module)
        // These methods intercept input, which is not really useful for the type of logging I intend this module to perform.
        // The script that called these methods will already have access to the results, and the script author can choose
        // to display it or not (at which point it will be caught by the logging module).
        //
        // WriteProgress is also included in this unused category, because this doesn't seem to make much sense in a log file.

        /// <summary>
        /// This method is called when a ProgressRecord is written to the progress stream.
        /// </summary>
        /// <param name="sourceId">
        /// The source id.
        /// </param>
        /// <param name="record">
        /// The ProgressRecord record.
        /// </param>
        void WriteProgress(long sourceId, ProgressRecord record);

        /// <summary>
        /// This method is called when the user is prompted for a choice.
        /// </summary>
        /// <param name="choice">
        /// The ChoiceDescription of the selected choice.
        /// </param>
        /// <remarks>
        /// This method intercepts calls to $Host.Ui.PromptForChoice() calls. This can be a manual call from a script or user, or also a built-in call, 
        /// such as when using CmdletBinding and SupportsShouldProcess or the Debug parameter.
        /// </remarks>
        void ChoicePrompt(ChoiceDescription choice);

        /// <summary>
        /// This method is called when the system prompts for credetials.
        /// </summary>
        /// <param name="credential">
        /// The credential object.
        /// </param>
        /// <remarks>
        /// This methos is called when the system prompts for credentials, such as when the -Credential parameter is passed as $null to a cmdlet. 
        /// Calling Get-Credential directly will trigger the Prompt() method which returns a PSCredential as the returnValue.
        /// </remarks>
        void CredentialPrompt(PSCredential credential);

        /// <summary>
        /// This method is called when the user is prompted for information.
        /// </summary>
        /// <param name="returnValue">
        /// This parameter contains information about the message prompt and what was entered.
        /// </param>
        /// <remarks>
        /// This method is called when the user is prompted for information, like Read-Host "Enter text" or Get-Credential.
        /// </remarks>
        void Prompt(Dictionary<string, PSObject> returnValue);

        /// <summary>
        /// This method is called when there is a request to read text from the host without a prompt.
        /// </summary>
        /// <param name="inputText">
        /// The input text.
        /// </param>
        /// <remarks>
        /// This method is called when Read-Host is used without a message. If a message is specified, then Prompt() is called.
        /// </remarks>
        void ReadFromHost(string inputText);

        #endregion

        /// <summary>
        /// This method is called when a message is written to the debug stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <remarks>
        /// Messages are only written to the debug stream from within a cmdlet with the CmdletBinding attribute and when the -Debug parameter has been specified.
        /// </remarks>
        void WriteDebug(string message);

        /// <summary>
        /// This method is called when a message is written to the error stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void WriteError(string message);

        /// <summary>
        /// This method is called when output is written to the output stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void WriteOutput(string message);

        /// <summary>
        /// The method is called when output is written with the Write-Host cmdlet to the output stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void WriteHost(string message);

        /// <summary>
        /// This method is called when a message is written to the Verbose stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void WriteVerbose(string message);

        /// <summary>
        /// This method is called when a message is written to the Warning stream.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void WriteWarning(string message);

        #endregion
    }
}