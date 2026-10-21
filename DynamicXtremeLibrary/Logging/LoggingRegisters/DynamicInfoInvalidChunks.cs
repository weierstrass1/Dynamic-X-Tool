using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports an invalid pose-chunk declaration in a DynamicInfo.</summary>
    public class DynamicInfoInvalidChunks : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "DYNAMIC INFO INVALID CHUNKS";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>Initializes an invalid pose-chunk record.</summary>
        public DynamicInfoInvalidChunks(string context, string reason)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", $"'{context}'" },
                { "reason", reason }
            }.AsReadOnly();
        }
    }
}
