using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports the address and size of a removed Dynamic Xtreme resource buffer.</summary>
    internal class RemovedBufferAt : ILoggingRegister
    {
        public bool AppearWithoutVerbose => false;
        public bool AppearInErrors => false;
        public ILogCategory Category => new Info();
        public string MessageType => "REMOVED BUFFER AT";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes a record for one removed resource buffer.</summary>
        public RemovedBufferAt(long address, long size)
        {
            Parameters = new Dictionary<string, string>
            {
                { "address", $"'0x{address:X6}'" },
                { "size", $"{size} bytes" }
            };
        }
    }
}
