using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ParameterLessRegister : ILoggingRegister
    {
        public required ILogCategory Category { get; init; }
        public required string MessageType { get; init; }
        public IReadOnlyDictionary<string, string> Parameters 
            => new Dictionary<string, string>().AsReadOnly();
    }
}
