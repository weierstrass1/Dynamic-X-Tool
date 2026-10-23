namespace DynamicXtremeLibrary.Config
{
    /// <summary>
    /// Represents an optional file path whose empty value disables the associated input.
    /// </summary>
    [Serializable]
    public sealed class OptionalFileOption : Option<string>
    {
        /// <summary>
        /// Gets or sets the directory used to resolve relative file paths. An empty value resolves
        /// paths from Dynamic Xtreme's current working directory.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string? RelativeToDirectory { get; set; }

        /// <summary>
        /// Converts a non-empty path to the relative form stored by the settings system while
        /// preserving an empty optional value.
        /// </summary>
        /// <param name="value">The path supplied by the user.</param>
        /// <returns>An empty string or the normalized relative path.</returns>
        public override string ParseFromString(string value)
            => string.IsNullOrWhiteSpace(value) ? "" : Path.GetRelativePath(
                string.IsNullOrWhiteSpace(RelativeToDirectory) ? "." : InputPath.Normalize(RelativeToDirectory),
                ResolvePath(value));

        /// <summary>
        /// Determines whether the value is empty or references an existing file.
        /// </summary>
        /// <param name="value">The path to validate.</param>
        /// <returns><see langword="true"/> for an empty value or an existing file.</returns>
        public override bool Validate(string value)
            => string.IsNullOrWhiteSpace(value) || File.Exists(ResolvePath(value));

        /// <summary>Resolves an absolute path or a path relative to the configured tool directory.</summary>
        /// <param name="value">The configured optional file path.</param>
        /// <returns>The absolute path used for validation and discovery.</returns>
        public string ResolvePath(string value)
        {
            value = InputPath.Normalize(value);
            if (Path.IsPathRooted(value))
                return Path.GetFullPath(value);
            return Path.GetFullPath(string.IsNullOrWhiteSpace(RelativeToDirectory)
                ? value
                : InputPath.Resolve(RelativeToDirectory, value));
        }
    }
}
