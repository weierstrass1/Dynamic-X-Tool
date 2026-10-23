namespace DynamicXtremeLibrary.Config
{
    /// <summary>Represents an input or output SNES ROM path.</summary>
    [Serializable]
    public class PathOption : Option<string>
    {
        /// <summary>Controls whether validation requires the target file to already exist.</summary>
        public bool ExistCheck;
        /// <summary>Initializes a ROM path option that requires an existing file by default.</summary>
        public PathOption(): base()
        {
            ExistCheck = true;
        }
        /// <summary>Converts a ROM path to its current-directory-relative form.</summary>
        /// <param name="value">The supplied ROM path.</param>
        /// <returns>The normalized relative path.</returns>
        public override string ParseFromString(string value)
        {
            return Path.GetRelativePath(".", value);
        }
        /// <summary>Validates the SMC extension, parent directory, and existence requirement.</summary>
        /// <param name="value">The ROM path to validate.</param>
        /// <returns>Whether the path can be used as configured.</returns>
        public override bool Validate(string value)
        {
            if (Path.GetExtension(value).ToLowerInvariant() != ".smc")
                return false;
            string directory = Path.GetDirectoryName(value)!;
            if(string.IsNullOrWhiteSpace(directory))
                directory = ".";
            if (!Directory.Exists(directory))
            {
                if (File.Exists(directory))
                    return false;
                if (ExistCheck)
                    return false;
                Directory.CreateDirectory(directory);
            }
            return !ExistCheck || File.Exists(value);
        }
    }
}
