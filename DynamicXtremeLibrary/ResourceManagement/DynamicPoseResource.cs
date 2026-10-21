namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>
    /// Represents one canonical dynamic pose together with the metadata required by its runtime
    /// lookup tables.
    /// </summary>
    public sealed class DynamicPoseResource : Resource
    {
        /// <summary>Gets the total pose size in bytes.</summary>
        public int PoseSize { get; }
        /// <summary>Gets the number of VRAM blocks reserved by the pose.</summary>
        public int Blocks { get; }
        /// <summary>Gets the first chunk size in bytes.</summary>
        public int FirstChunkSize { get; }
        /// <summary>Gets the second chunk size in bytes.</summary>
        public int SecondChunkSize { get; }
        /// <summary>Gets the encoded last-row value in bytes.</summary>
        public int LastRow { get; }

        /// <summary>Initializes a canonical dynamic pose and its runtime metadata.</summary>
        public DynamicPoseResource(int id, string name, byte[] data, int poseSize, int blocks,
            int firstChunkSize, int secondChunkSize, int lastRow)
            : base(id, name, ResourceType.DynamicPose, data)
        {
            PoseSize = poseSize;
            Blocks = blocks;
            FirstChunkSize = firstChunkSize;
            SecondChunkSize = secondChunkSize;
            LastRow = lastRow;
        }

        /// <summary>Determines whether another pose has identical bytes and runtime metadata.</summary>
        public bool IsEquivalentTo(byte[] data, int poseSize, int blocks, int firstChunkSize,
            int secondChunkSize, int lastRow)
            => PoseSize == poseSize && Blocks == blocks && FirstChunkSize == firstChunkSize &&
                SecondChunkSize == secondChunkSize && LastRow == lastRow &&
                Data.Length == data.Length && Data.AsSpan().SequenceEqual(data);
    }
}
