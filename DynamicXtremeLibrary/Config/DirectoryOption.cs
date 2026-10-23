namespace DynamicXtremeLibrary.Config
{
    /// <summary>Represents an optional external-tool executable or directory setting.</summary>
    [Serializable]
    public class DirectoryOption : Option<string>
    {
        /// <summary>Controls whether a stored directory may be accepted without an executable path.</summary>
        public bool GetFromSettings = false;
        /// <summary>Initializes a directory option.</summary>
        public DirectoryOption() : base()
        {
        }
        /// <summary>Extracts and stores the relative directory containing the supplied executable.</summary>
        /// <param name="value">An executable path entered by the user.</param>
        /// <returns>The executable directory relative to the current working directory.</returns>
        public override string ParseFromString(string value)
        {
            string directory = Path.GetDirectoryName(value)!;
            if (string.IsNullOrWhiteSpace(directory))
                directory = ".";
            return Path.GetRelativePath(".", directory);
        }
        /// <summary>Validates an empty value, an executable path, or a stored directory as appropriate.</summary>
        /// <param name="value">The configured path.</param>
        /// <returns>Whether the option satisfies its current source contract.</returns>
        public override bool Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (GetFromSettings)
                return Directory.Exists(value);

            if (!string.Equals(Path.GetExtension(value), ".exe",
                StringComparison.OrdinalIgnoreCase))
                return false;

            string? directory = Path.GetDirectoryName(value);
            return string.IsNullOrWhiteSpace(directory) ||
                Directory.Exists(directory);
        }
        /// <summary>
        /// Validates the directory form stored after an external-tool executable was selected.
        /// </summary>
        /// <param name="value">The stored external-tool directory.</param>
        /// <returns>Whether the stored directory is empty or still exists.</returns>
        protected override bool ValidateStoredValue(string value)
            => string.IsNullOrWhiteSpace(value) || Directory.Exists(value);
    }
}
