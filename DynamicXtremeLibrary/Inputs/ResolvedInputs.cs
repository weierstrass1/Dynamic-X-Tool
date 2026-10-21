using DynamicXtremeLibrary.Infos;
using DynamicXtremePaletteCreatorLibrary;

namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Contains the deduplicated inputs, planned source rewrites, and reported name changes for one
    /// Dynamic Xtreme installation.
    /// </summary>
    public sealed class ResolvedInputs
    {
        /// <summary>
        /// Gets the resolved DynamicInfo definitions in deterministic installation order.
        /// </summary>
        public IReadOnlyList<DynamicInfo> DynamicInfos { get; init; } = [];

        /// <summary>
        /// Gets the resolved DrawInfo poses with their final consecutive IDs.
        /// </summary>
        public IReadOnlyList<DrawInfo> DrawInfos { get; init; } = [];

        /// <summary>
        /// Gets the resolved palette-effect collections.
        /// </summary>
        public IReadOnlyList<PaletteEffectCollection> PaletteEffects { get; init; } = [];

        /// <summary>
        /// Gets the deferred modifications for external ASM consumers.
        /// </summary>
        public IReadOnlyList<SourceRewrite> SourceRewrites { get; init; } = [];

        /// <summary>
        /// Gets the logical name changes that must be reported after a successful commit.
        /// </summary>
        public IReadOnlyList<(string OldName, string NewName, IReadOnlyList<string> Files)> NameChanges { get; init; } = [];
    }
}
