using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class FailedToProcessFile : ILoggingRegister
    {
        public ILogCategory Category => new Error();
        public string MessageType => "FAILED TO PROCESS FILE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public FailedToProcessFile(string filePath)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" }
            };

        }
    }
}
