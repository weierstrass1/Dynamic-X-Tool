using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ValueExceedsLimit : ILoggingRegister
    {
        public ILogCategory Category => new Error();

        public string MessageType => "VALUE EXCEEDS LIMIT";

        public IReadOnlyDictionary<string, string> Parameters { get; }
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
