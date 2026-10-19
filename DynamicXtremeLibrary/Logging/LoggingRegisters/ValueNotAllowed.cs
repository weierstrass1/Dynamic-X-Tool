using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ValueNotAllowed : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "VALUE NOT ALLOWED";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public ValueNotAllowed(string context, string parameter, int value,
            IEnumerable<int> allowedValues)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", context },
                { "parameter", parameter },
                { "value", $"${value:X2}" },
                { "allowedValues", string.Join(", ", allowedValues.Select(v => $"${v:X2}")) }
            }.AsReadOnly();
        }
    }
}
