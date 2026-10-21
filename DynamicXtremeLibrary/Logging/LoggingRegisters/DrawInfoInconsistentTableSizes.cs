using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports DrawInfo tables whose populated lengths disagree.</summary>
    public class DrawInfoInconsistentTableSizes : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();
        public string MessageType => "DRAW INFO INCONSISTENT TABLE SIZES";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes the inconsistency record for a DrawInfo context.</summary>
        public DrawInfoInconsistentTableSizes(string context)
        {
            Parameters = new Dictionary<string, string>()
            {
                { "context", context }
            };
        }
    }
}
