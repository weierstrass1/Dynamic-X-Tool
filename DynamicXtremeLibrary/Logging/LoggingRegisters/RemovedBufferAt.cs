using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    internal class RemovedBufferAt : ILoggingRegister
    {
        public ILogCategory Category => new Info();
        public string MessageType => "REMOVED BUFFER AT";
        public IReadOnlyDictionary<string, string> Parameters { get; }
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
