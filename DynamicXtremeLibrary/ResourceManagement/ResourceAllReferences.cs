namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>Groups inserted buffer references and their contained resources by type.</summary>
    public class ResourceAllReferences
    {
        /// <summary>Gets inserted buffer references.</summary>
        public required IReadOnlyList<ResourceReference> Buffers { get; init; }
        /// <summary>Gets absolute dynamic-pose references.</summary>
        public required IReadOnlyList<ResourceReference> DynamicPoses { get; init; }
        /// <summary>Gets absolute palette references.</summary>
        public required IReadOnlyList<ResourceReference> Palettes { get; init; }
        /// <summary>Gets absolute references to converted palette-effect target palettes.</summary>
        public IReadOnlyList<ResourceReference> PaletteEffectPalettes { get; init; } = [];
        /// <summary>Gets absolute general-resource references.</summary>
        public required IReadOnlyList<ResourceReference> GeneralResources { get; init; }
        /// <summary>Gets total packed-buffer bytes.</summary>
        public long BufferSize { get => Buffers.Sum(b => b.Resource.Length); }
        /// <summary>Gets total dynamic-pose bytes.</summary>
        public long DynamicPosesSize { get => DynamicPoses.Sum(b => b.Resource.Length); }
        /// <summary>Gets total palette bytes.</summary>
        public long PaletteSize { get => Palettes.Sum(b => b.Resource.Length); }
        /// <summary>Gets total converted palette-effect target bytes.</summary>
        public long PaletteEffectPaletteSize { get => PaletteEffectPalettes.Sum(b => b.Resource.Length); }
        /// <summary>Gets total general-resource bytes.</summary>
        public long GeneralResourceSize { get => GeneralResources.Sum(b => b.Resource.Length); }
    }
}
