using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>Reports the number of runtime effects inserted from one collection.</summary>
    public class InsertedPaletteEffect : ILoggingRegister
    {
        public bool AppearWithoutVerbose => false;
        public bool AppearInErrors => false;
        public ILogCategory Category => new Info();
        public string MessageType => "PALETTE EFFECT COLLECTION INSERTED";
        public IReadOnlyDictionary<string, string> Parameters { get; }
        /// <summary>Initializes an inserted palette-effect record.</summary>
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
