namespace DynamicXtremeLibrary;

/// <summary>Converts user-authored paths to the host platform's separator convention.</summary>
internal static class InputPath
{
    /// <summary>Gets physical-path equality rules: case-insensitive on Windows and ordinal elsewhere.</summary>
    internal static StringComparer Comparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    /// <summary>Accepts both slash styles without changing casing or resolving a base directory.</summary>
    /// <param name="path">The path read from a definition, descriptor, list, or setting.</param>
    /// <returns>The path using the operating system's directory separator.</returns>
    internal static string Normalize(string path)
        => path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

    /// <summary>Combines an input path with its base using native separators.</summary>
    /// <param name="directory">The base for relative paths.</param>
    /// <param name="path">The relative or absolute input path.</param>
    /// <returns>A native path preserving the base and absolute-path semantics.</returns>
    internal static string Resolve(string directory, string path)
        => Path.Combine(Normalize(directory), Normalize(path));

    /// <summary>Finds input definitions recursively with case-insensitive extension matching on every OS.</summary>
    /// <param name="directory">The directory containing traditional definitions.</param>
    /// <param name="pattern">The wildcard pattern for the definition format.</param>
    /// <returns>The matching paths; inaccessible directories are reported rather than silently skipped.</returns>
    internal static string[] GetFiles(string directory, string pattern)
        => Directory.GetFiles(directory, pattern, new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            IgnoreInaccessible = false,
            AttributesToSkip = 0
        });
}
