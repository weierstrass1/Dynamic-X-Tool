using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class DynamicInfoSizeMismatch : ILoggingRegister
    {
        public ILogCategory Category => new Error();
        public string MessageType => "DYNAMIC INFO SIZE MISMATCH";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public DynamicInfoSizeMismatch(string contextName, long size1, long size2)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", $"'{contextName}'" },
                { "size1", $"{size1} bytes" },
                { "size2", $"{size2} bytes" }
            };
        }
    }
}
