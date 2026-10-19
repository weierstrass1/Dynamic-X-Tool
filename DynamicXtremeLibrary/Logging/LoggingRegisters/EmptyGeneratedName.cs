using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class EmptyGeneratedName : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "EMPTY GENERATED NAME";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public EmptyGeneratedName(string sourceName)
        {
            Parameters = new Dictionary<string, string>
            {
                { "name", $"'{sourceName}'" }
            }.AsReadOnly();
        }
    }
}
