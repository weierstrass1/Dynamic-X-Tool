using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a file whose byte size exceeds its allowed limit.</summary>
    public class FileIsToBig : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "FILE IS TOO BIG";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes an oversized-file record.</summary>
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
