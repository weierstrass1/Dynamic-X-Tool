namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>Identifies the role of binary data managed by Dynamic Xtreme.</summary>
    public enum ResourceType
    {
        GeneralResource,
        Palette,
        DynamicPose,
        Buffer,
        GraphicRoutine
    }
    /// <summary>Represents one named and typed binary resource with a generated ID.</summary>
    public class Resource
    {
        /// <summary>Gets the resource ID within its type.</summary>
        public int ID { get; private set; }
        /// <summary>Gets the generated symbol name.</summary>
        public string Name { get; private set; }
        /// <summary>Gets the resource category.</summary>
        public ResourceType Type { get; private set; }
        /// <summary>Gets the binary resource content.</summary>
        public virtual byte[] Data { get; protected set; }
        /// <summary>Gets the resource byte length.</summary>
        public int Length => Data.Length;
        /// <summary>Initializes a binary resource.</summary>
        public Resource(int id, string name, ResourceType type, byte[] data)
        {
            ID = id;
            Type = type;
            Data = data;
            Name = name;
        }
        /// <summary>Returns the resource identity, type, and byte length.</summary>
        public override string ToString()
        {
            return $"({ID}, {Name}, {Type}, {Length})";
        }
    }
}
