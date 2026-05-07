using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class FileIsToBig : ILoggingRegister
    {
        public ILogCategory Category => new Error();
        public string MessageType => "FILE IS TOO BIG";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public FileIsToBig(string filePath, long fileSize, long maxSize)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" },
                { "size", $"{fileSize} bytes" },
                { "maxSize", $"{maxSize} bytes" }
            };
        }
    }
}
