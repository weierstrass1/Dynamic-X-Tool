using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ResourceNotFound : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "RESOURCE NOT FOUND";
        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
        public ResourceNotFound(string resourceName)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{resourceName}'" }
            }.AsReadOnly();
        }
    }
}
