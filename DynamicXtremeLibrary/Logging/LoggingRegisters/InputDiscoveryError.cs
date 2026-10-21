using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>
    /// Reports a failure while discovering, resolving, or transactionally applying external inputs.
    /// </summary>
    public sealed class InputDiscoveryError : ILoggingRegister
    {
        /// <summary>Gets whether this error is shown when verbose output is disabled.</summary>
        public bool AppearWithoutVerbose => true;
        /// <summary>Gets whether this record belongs in the error output.</summary>
        public bool AppearInErrors => true;
        /// <summary>Gets the error log category.</summary>
        public ILogCategory Category => new Error();
        /// <summary>Gets the message-template identifier.</summary>
        public string MessageType => "INPUT DISCOVERY ERROR";
        /// <summary>Gets the failure description used by the message template.</summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// Initializes an error record with the failure reported by the input pipeline.
        /// </summary>
        /// <param name="message">The human-readable failure description.</param>
        public InputDiscoveryError(string message)
        {
            Parameters = new Dictionary<string, string>
            {
                { "message", message }
            }.AsReadOnly();
        }
    }
}
