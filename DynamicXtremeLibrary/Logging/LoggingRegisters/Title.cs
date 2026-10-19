using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class Title : ILoggingRegister
    {
        public bool AppearWithoutVerbose { get; set; } = false;
        public bool AppearInErrors { get; set; } = false;
        public ILogCategory Category => new Categories.Title();
        public string MessageType => "TITLE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public Title(string title)
        {
            Parameters = new Dictionary<string, string>
            {
                { "title", $"{title}" }
            };
        }
    }
}
