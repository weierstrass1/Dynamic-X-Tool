using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a DynamicInfo that does not define pose chunks.</summary>
    public class DynamicInfoWithoutChunks : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Categories.Error();
        public string MessageType => "DYNAMIC INFO WITHOUT CHUNKS";
        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
        /// <summary>Initializes the missing-chunks record.</summary>
        public DynamicInfoWithoutChunks(string context)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", $"'{context}'" }
            }.AsReadOnly();
        }
    }
}
