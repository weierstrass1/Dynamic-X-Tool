using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class DrawInfoInconsistentTableSizes : ILoggingRegister
    {
        public ILogCategory Category => new Error();

        public string MessageType => "DRAW INFO INCONSISTENT TABLE SIZES";

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public DrawInfoInconsistentTableSizes(string context)
        {
            Parameters = new Dictionary<string, string>()
            {
                { "context", context }
            };
        }
    }
}
