using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports the byte usage of one generated data category.</summary>
    public class DataUsage : ILoggingRegister
    {
        public bool AppearWithoutVerbose => true;
		public bool AppearInErrors => false;
        public ILogCategory Category => new Info();
        public string MessageType => "DATA USAGE";
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>Initializes a data-usage record.</summary>
        public DataUsage(string name, long size)
		{
			float banks = size / (32 * 1024f);
			float mb = size / (1024 * 1024f);
			Parameters = new Dictionary<string, string>
			{
				{ "name", $"'{name}'" },
				{ "size", $"{mb:0.00}" },
				{ "banks", $"{banks:0.00}" }
			};
		}
	}
}
