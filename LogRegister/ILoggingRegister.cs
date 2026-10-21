namespace LogRegister
{
    /// <summary>Defines a structured log event resolved through a message template.</summary>
    public interface ILoggingRegister
    {
        /// <summary>Gets whether the record is visible outside verbose mode.</summary>
        public bool AppearWithoutVerbose { get; }
        /// <summary>Gets whether the record is included in error-only output.</summary>
        public bool AppearInErrors { get; }
        /// <summary>Gets the semantic rendering category.</summary>
        public ILogCategory Category { get; }
        /// <summary>Gets the message-template identifier.</summary>
        public string MessageType { get; }
        /// <summary>Gets values substituted into the message template.</summary>
        IReadOnlyDictionary<string, string> Parameters { get; }
    }
}
