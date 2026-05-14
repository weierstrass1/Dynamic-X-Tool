using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class NumberOf : ILoggingRegister
    {
        public ILogCategory Category => new Info();

        public string MessageType => "NUMBER OF";

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public NumberOf(string name, long quantity, long? size = null)
        {
            var pars = new Dictionary<string, string>
            {
                { "name", $"'{name}'" },
                { "quantity", $"{quantity}" }
            };
            if (size != null)
                pars.Add("size", $" ({size} bytes)");
            Parameters = pars;
        }
    }
}
