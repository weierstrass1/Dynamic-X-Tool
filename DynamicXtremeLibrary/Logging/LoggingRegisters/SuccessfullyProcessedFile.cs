using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports successful parsing and validation of one input file or pose.</summary>
    public class SuccessfullyProcessedFile : ILoggingRegister
    {
        public bool AppearWithoutVerbose => false;
        public bool AppearInErrors => false;
        public ILogCategory Category => new Success();
        public string MessageType => "SUCCESSFULLY PROCESSED FILE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a successful-file record.</summary>
        public SuccessfullyProcessedFile(string filePath)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" }
            };

        }
    }
}
