using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class NumberOf : ILoggingRegister
    {
        public ILogCategory Category => new Info();

        public string MessageType => "NUMBER OF";

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public NumberOf(string name, long quantity, long? size = null)
        {
            var pars = new Dictionary<string, string>
            {
                { "name", $"'{name}'" },
                { "quantity", $"{quantity}" },
                { "size", size != null ? $" ({size} bytes)" : "" }
            };

            Parameters = pars;
        }
    }
}
