using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Creates a section heading in rendered Dynamic Xtreme logs.</summary>
    public class Title : ILoggingRegister
    {
        public bool AppearWithoutVerbose { get; set; } = false;
        public bool AppearInErrors { get; set; } = false;
        public ILogCategory Category => new Categories.Title();
        public string MessageType => "TITLE";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a log title.</summary>
        public Title(string title)
        {
            Parameters = new Dictionary<string, string>
            {
                { "title", $"{title}" }
            };
        }
    }
}
