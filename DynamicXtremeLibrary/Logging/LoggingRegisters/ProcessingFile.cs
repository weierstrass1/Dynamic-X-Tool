using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ProcessingFile : ILoggingRegister
    {
        public ILogCategory Category => new Info();
        public string MessageType => "PROCESSING FILE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public ProcessingFile(string filePath)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" }
            };
        }
    }
}
