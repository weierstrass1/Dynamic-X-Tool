using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports that processing has started for one input file.</summary>
    public class ProcessingFile : ILoggingRegister
    {
        public bool AppearWithoutVerbose => false;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Info();
        public string MessageType => "PROCESSING FILE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a processing-file record.</summary>
        public ProcessingFile(string filePath)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{filePath}'" }
            };
        }
    }
}
