using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;

namespace DynamicXtremeLibrary.PaletteEffec
{
    /// <summary>Associates one transition effect with its resolved and converted target palette.</summary>
    public sealed class ResolvedPaletteEffectTarget
    {
        /// <summary>Gets the palette effect that consumes this target.</summary>
        public required PaletteEffect Effect { get; init; }
        /// <summary>Gets the absolute source-palette path.</summary>
        public required string SourcePath { get; init; }
        /// <summary>Gets the generated palette representation.</summary>
        public required PaletteEffectPaletteFormat Format { get; init; }
        /// <summary>Gets the normalized source name before logical collision suffixes.</summary>
        public required string BaseName { get; init; }
        /// <summary>Gets the converted three-byte-per-color data.</summary>
        public required byte[] Data { get; init; }
    }

    /// <summary>
    /// Preserves source context and converted targets for a portable palette-effect collection.
    /// </summary>
    public sealed class ResolvedPaletteEffectCollection
    {
        /// <summary>Gets the portable palette-effect data.</summary>
        public required PaletteEffectCollection Collection { get; init; }
        /// <summary>Gets the absolute `.paleffect` source path.</summary>
        public required string SourcePath { get; init; }
        /// <summary>Gets the directory used to resolve relative palette paths.</summary>
        public required string ResourceDirectory { get; init; }
        /// <summary>Gets the normalized name derived from the original source filename.</summary>
        public required string OriginalName { get; init; }
        /// <summary>Gets every transition target converted during input validation.</summary>
        public required IReadOnlyList<ResolvedPaletteEffectTarget> Targets { get; init; }

        /// <summary>Gets the final collision-free collection name.</summary>
        public string LogicalName => Collection.Name ?? OriginalName;

        /// <summary>Applies the final logical name used by generated effect and resource symbols.</summary>
        public void SetLogicalName(string logicalName) => Collection.Name = logicalName;

        /// <summary>Builds a target resource name using the collection's final logical suffix.</summary>
        public string GetPaletteName(ResolvedPaletteEffectTarget target)
            => GetPaletteName(target, getLogicalSuffix(LogicalName));

        /// <summary>Builds a target resource name using an explicit logical suffix.</summary>
        public string GetPaletteName(ResolvedPaletteEffectTarget target, string suffix)
            => $"{target.BaseName}{suffix}{target.Format}";

        /// <summary>Resolves a declared palette path against this collection's resource directory.</summary>
        public string ResolvePalettePath(string path)
            => InputPath.Resolve(ResourceDirectory, path);

        /// <summary>Extracts the collision suffix from a complete logical collection name.</summary>
        private string getLogicalSuffix(string logicalName)
            => logicalName.StartsWith(OriginalName, StringComparison.Ordinal)
                ? logicalName[OriginalName.Length..]
                : logicalName;
    }

    /// <summary>Associates a transition effect with the generated symbol and logical resource it uses.</summary>
    public sealed class PaletteEffectResourceBinding
    {
        /// <summary>Gets the transition effect.</summary>
        public required PaletteEffect Effect { get; init; }
        /// <summary>Gets the generated `!Palette` symbol suffix.</summary>
        public required string Name { get; init; }
        /// <summary>Gets the logical resource containing the converted target palette.</summary>
        public required Resource Resource { get; init; }
    }

    /// <summary>Contains converted palette resources and their effect-level bindings.</summary>
    public sealed class PaletteEffectResourceSet
    {
        /// <summary>Gets the logical resources that must enter the dynamic-resource pipeline.</summary>
        public required IReadOnlyList<Resource> Resources { get; init; }
        /// <summary>Gets the transition bindings indexed by palette-effect object identity.</summary>
        public required IReadOnlyDictionary<PaletteEffect, PaletteEffectResourceBinding> Bindings { get; init; }
    }
}
