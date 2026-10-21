using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports the quantity and optional byte size of a generated item category.</summary>
    public class NumberOf : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => false;
        public ILogCategory Category => new Info();
        public string MessageType => "NUMBER OF";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a quantity summary record.</summary>
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
