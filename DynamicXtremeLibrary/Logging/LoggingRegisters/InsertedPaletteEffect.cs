using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class InsertedPaletteEffect : ILoggingRegister
    {
        public ILogCategory Category => new Info();

        public string MessageType => "PALETTE EFFECT COLLECTION INSERTED";

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public InsertedPaletteEffect(string name, int length)
        {
            Parameters = new Dictionary<string, string>
            {
                { "name", $"'{name}'" },
                { "length", $"{length}" }
            };
        }
    }
}
