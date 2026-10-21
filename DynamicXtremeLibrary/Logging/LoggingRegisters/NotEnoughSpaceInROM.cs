using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports that no free ROM block can contain the next resource buffer.</summary>
    public class NotEnoughSpaceInROM : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "NOT ENOUGH SPACE IN ROM";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes an insufficient-ROM-space record.</summary>
        public NotEnoughSpaceInROM()
        {
            Parameters = new Dictionary<string, string>().AsReadOnly();
        }
    }
}
