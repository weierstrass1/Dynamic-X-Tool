using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    /// <summary>
    /// Reports an automatically generated logical-name change and the ASM files affected by it.
    /// </summary>
    public sealed class GeneratedNameChanged : ILoggingRegister
    {
        /// <summary>Gets whether this informational record is shown when verbose output is disabled.</summary>
        public bool AppearWithoutVerbose => true;
        /// <summary>Gets whether this informational record belongs in the error output.</summary>
        public bool AppearInErrors => false;
        /// <summary>Gets the informational log category.</summary>
        public ILogCategory Category => new Info();
        /// <summary>Gets the message-template identifier.</summary>
        public string MessageType => "GENERATED NAME CHANGED";
        /// <summary>Gets the old name, new name, and affected files used by the message template.</summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// Initializes an informational record for one committed logical-name change.
        /// </summary>
        /// <param name="oldName">The logical name used before collision resolution.</param>
        /// <param name="newName">The final collision-free logical name.</param>
        /// <param name="files">The ASM consumer files updated to use the new name.</param>
        public GeneratedNameChanged(string oldName, string newName, IEnumerable<string> files)
        {
            Parameters = new Dictionary<string, string>
            {
                { "oldName", $"'{oldName}'" },
                { "newName", $"'{newName}'" },
                { "files", string.Join(", ", files.Select(file => $"'{file}'")) }
            }.AsReadOnly();
        }
    }
}
