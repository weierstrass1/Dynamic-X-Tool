using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class SyntaxError : ILoggingRegister
    {
        public ILogCategory Category => new Error();

        public string MessageType => "SYNTAX ERROR";

        public IReadOnlyDictionary<string, string> Parameters { get; private set; }
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
