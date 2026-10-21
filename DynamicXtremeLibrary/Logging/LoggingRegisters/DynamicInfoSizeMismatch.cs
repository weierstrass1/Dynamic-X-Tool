using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports a mismatch between declared pose data and graphics-file sizes.</summary>
    public class DynamicInfoSizeMismatch : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "DYNAMIC INFO SIZE MISMATCH";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a DynamicInfo size-mismatch record.</summary>
        public DynamicInfoSizeMismatch(string contextName, long size1, long size2)
        {
            Parameters = new Dictionary<string, string>
            {
                { "context", $"'{contextName}'" },
                { "size1", $"{size1} bytes" },
                { "size2", $"{size2} bytes" }
            };
        }
    }
}
