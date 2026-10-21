using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports distinct source names that normalize to the same generated symbol.</summary>
    public class GeneratedNameCollision : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "GENERATED NAME COLLISION";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>Initializes a generated-name collision record.</summary>
        public GeneratedNameCollision(string firstName, string secondName, string generatedName)
        {
            Parameters = new Dictionary<string, string>
            {
                { "firstName", $"'{firstName}'" },
                { "secondName", $"'{secondName}'" },
                { "generatedName", $"'{generatedName}'" }
            }.AsReadOnly();
        }
    }
}
