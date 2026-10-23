namespace DynamicXtremeLibrary.Inputs;

/// <summary>Describes a PIXI entry that could not be resolved to a primary ASM source.</summary>
/// <param name="ListPath">The absolute PIXI list path.</param>
/// <param name="LineNumber">The one-based list line number.</param>
/// <param name="ListedPath">The entry path written in the list.</param>
/// <param name="ResolvedPath">The descriptor or source path inspected.</param>
/// <param name="Reason">The reason discovery could not inspect this entry.</param>
public sealed record PixiEntryIssue(string ListPath, int LineNumber, string ListedPath,
    string ResolvedPath, string Reason);
