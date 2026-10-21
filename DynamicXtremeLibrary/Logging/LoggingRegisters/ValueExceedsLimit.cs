using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a parameter value greater than its inclusive maximum.</summary>
    public class ValueExceedsLimit : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "VALUE EXCEEDS LIMIT";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes an exceeded-limit record.</summary>
        public ValueExceedsLimit(string context, string parameter, int value, int limit)
        {
            Parameters = new Dictionary<string, string>()
            {
                { "context", context },
                { "parameter", parameter },
                { "value", $"${value:X4}" },
                { "limit", $"${limit:X2}" }
            };
        }
    }
}
