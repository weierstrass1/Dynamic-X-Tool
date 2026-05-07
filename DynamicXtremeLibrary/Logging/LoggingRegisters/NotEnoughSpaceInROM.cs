using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class NotEnoughSpaceInROM : ILoggingRegister
    {
        public ILogCategory Category => new Error();
        public string MessageType => "NOT ENOUGH SPACE IN ROM";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public NotEnoughSpaceInROM()
        {
            Parameters = new Dictionary<string, string>().AsReadOnly();
        }
    }
}
