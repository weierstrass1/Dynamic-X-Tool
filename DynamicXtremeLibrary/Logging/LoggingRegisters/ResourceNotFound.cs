using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a declared resource file that could not be found.</summary>
    public class ResourceNotFound : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "RESOURCE NOT FOUND";
        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
        /// <summary>Initializes a missing-resource record.</summary>
        public ResourceNotFound(string resourceName)
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{resourceName}'" }
            }.AsReadOnly();
        }
    }
}
