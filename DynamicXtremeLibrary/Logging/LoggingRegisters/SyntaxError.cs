using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports an invalid source line with its location and optional detail.</summary>
    public class SyntaxError : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
        public bool AppearInErrors => true;
        public ILogCategory Category => new Error();

        public string MessageType => "SYNTAX ERROR";

        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
        /// <summary>Initializes a syntax-error record.</summary>
        public SyntaxError(string file, int line, string lineContent, string message = "")
        {
            Parameters = new Dictionary<string, string>
            {
                { "file", $"'{file}'" },
                { "line", $"'{line}'" },
                { "message", string.IsNullOrWhiteSpace(message) ? 
                    "" : 
                    $".\n\t\t{message}"},
                { "lineContent", $"'{lineContent}'"   }
            }.AsReadOnly();
        }
    }
}
