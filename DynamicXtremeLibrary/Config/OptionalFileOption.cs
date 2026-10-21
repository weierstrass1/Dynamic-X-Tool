namespace DynamicXtremeLibrary.Config
{
    /// <summary>
    /// Represents an optional file path whose empty value disables the associated input.
    /// </summary>
    [Serializable]
    public sealed class OptionalFileOption : Option<string>
    {
        /// <summary>
        /// Converts a non-empty path to the relative form stored by the settings system while
        /// preserving an empty optional value.
        /// </summary>
        /// <param name="value">The path supplied by the user.</param>
        /// <returns>An empty string or the normalized relative path.</returns>
        public override string ParseFromString(string value)
            => string.IsNullOrWhiteSpace(value) ? "" : Path.GetRelativePath(".\\", value);

        /// <summary>
        /// Determines whether the value is empty or references an existing file.
        /// </summary>
        /// <param name="value">The path to validate.</param>
        /// <returns><see langword="true"/> for an empty value or an existing file.</returns>
        public override bool Validate(string value)
            => string.IsNullOrWhiteSpace(value) || File.Exists(value);
    }
}
