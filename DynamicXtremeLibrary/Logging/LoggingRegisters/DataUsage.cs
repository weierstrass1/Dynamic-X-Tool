using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class DataUsage : ILoggingRegister
    {
        public ILogCategory Category => new Info();

        public string MessageType => "DATA USAGE";

        public IReadOnlyDictionary<string, string> Parameters { get; }
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
