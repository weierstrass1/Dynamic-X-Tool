using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a file that could not be parsed or validated.</summary>
    public class FailedToProcessFile : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "FAILED TO PROCESS FILE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a failed-file record.</summary>
        public FailedToProcessFile(string filePath)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" }
            };

        }
    }
}
