using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class DynamicInfoWithoutChunks : ILoggingRegister
    {
        public ILogCategory Category => new Categories.Error();
        public string MessageType => "DYNAMIC INFO WITHOUT CHUNKS";
        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
        public DynamicInfoWithoutChunks(string context)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", $"'{context}'" }
            }.AsReadOnly();
        }
    }
}
