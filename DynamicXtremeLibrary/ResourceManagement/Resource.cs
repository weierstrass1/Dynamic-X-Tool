namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>Identifies the role of binary data managed by Dynamic Xtreme.</summary>
    public enum ResourceType
    {
        GeneralResource,
        Palette,
        /// <summary>Represents a converted three-byte-per-color palette used by a transition effect.</summary>
        PaletteEffectPalette,
        DynamicPose,
        Buffer,
        GraphicRoutine
    }
    /// <summary>Represents one named and typed binary resource with a generated ID.</summary>
    public class Resource
    {
        private readonly List<string> _names;

        /// <summary>Gets the resource ID within its type.</summary>
        public int ID { get; private set; }
        /// <summary>Gets the primary generated symbol name.</summary>
        public string Name { get; private set; }
        /// <summary>Gets every generated symbol name that refers to this resource.</summary>
        public IReadOnlyList<string> Names => _names.AsReadOnly();
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
            _names = [name];
        }
        /// <summary>Adds another generated symbol name for the same binary resource.</summary>
        /// <param name="name">The additional collision-free resource name.</param>
        public void AddAlias(string name)
        {
            if (!_names.Contains(name, StringComparer.Ordinal))
                _names.Add(name);
        }
        /// <summary>Returns the resource identity, type, and byte length.</summary>
        public override string ToString()
        {
            return $"({ID}, {Name}, {Type}, {Length})";
        }
    }
}
