using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a source filename that normalizes to an empty Asar symbol.</summary>
    public class EmptyGeneratedName : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "EMPTY GENERATED NAME";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>Initializes an empty-generated-name record.</summary>
        public EmptyGeneratedName(string sourceName)
        {
            Parameters = new Dictionary<string, string>
            {
                { "name", $"'{sourceName}'" }
            }.AsReadOnly();
        }
    }
}
