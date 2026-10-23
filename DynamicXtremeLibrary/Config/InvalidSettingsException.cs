namespace DynamicXtremeLibrary.Config
{
    /// <summary>Represents a user-facing syntax or schema error in the persisted settings file.</summary>
    public sealed class InvalidSettingsException : Exception
    {
        /// <summary>Initializes a settings error with its user-facing description.</summary>
        /// <param name="message">The description of the invalid setting and its location.</param>
        public InvalidSettingsException(string message) : base(message)
        {
        }

        /// <summary>Initializes a settings error while preserving the parser failure that caused it.</summary>
        /// <param name="message">The description of the invalid setting and its location.</param>
        /// <param name="innerException">The underlying JSON parser failure.</param>
        public InvalidSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
