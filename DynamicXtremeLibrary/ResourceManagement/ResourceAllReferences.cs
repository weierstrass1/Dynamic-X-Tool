namespace DynamicXtremeLibrary.ResourceManagement
{
    public class ResourceAllReferences
    {
        public required IReadOnlyList<ResourceReference> Buffers { get; init; }
        public required IReadOnlyList<ResourceReference> DynamicPoses { get; init; }
        public required IReadOnlyList<ResourceReference> Palettes { get; init; }
        public required IReadOnlyList<ResourceReference> GeneralResources { get; init; }
        public long BufferSize { get => Buffers.Sum(b => b.Resource.Length); }
        public long DynamicPosesSize { get => DynamicPoses.Sum(b => b.Resource.Length); }
        public long PaletteSize { get => Palettes.Sum(b => b.Resource.Length); }
        public long GeneralResourceSize { get => GeneralResources.Sum(b => b.Resource.Length); }
    }
}
