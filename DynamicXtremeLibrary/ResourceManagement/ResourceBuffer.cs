namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>Packs multiple resources into one RATS-protected ROM allocation.</summary>
    public class ResourceBuffer : Resource
    {
        private readonly List<ResourceReference> _references;
        private bool _dirty;
        /// <summary>Gets lazily rebuilt packed data or replaces the cached buffer contents.</summary>
        public override byte[] Data 
        { 
            get
            {
                if (_dirty)
                {
                    byte[] data = new byte[_references.Max(r => r.Position + r.Resource.Length)];
                    foreach (var reference in _references)
                        Array.Copy(reference.Resource.Data, 0, data, reference.Position, reference.Resource.Length);
                    base.Data = data;
                    _dirty = false;
                }
                return base.Data;
            }
            protected set => base.Data = value; 
        }
        /// <summary>Initializes an empty resource buffer.</summary>
        public ResourceBuffer(int id) : base(id, $"Buffer{id}", ResourceType.Buffer, [])
        {
            _references = [];
            _dirty = false;
        }
        /// <summary>Adds a contained resource reference and invalidates the packed byte cache.</summary>
        public void AddReference(ResourceReference reference)
        {
            _references.Add(reference);
            _dirty = true;
        }
        /// <summary>Converts contained offsets into absolute ROM positions after insertion.</summary>
        public void AddOffsetPosition(int offset)
        {
            foreach (var reference in _references)
            {
                reference.Position += offset;
            }
        }
        /// <summary>Gets a read-only view of contained resource references.</summary>
        public IReadOnlyList<ResourceReference> GetReferences()
        {
            return _references.AsReadOnly();
        }
        /// <summary>Returns the buffer ID, packed size, and contained-resource count.</summary>
        public override string ToString()
        {
            return $"({ID}, {Data?.Length ?? 0}, {_references.Count})";
        }
    }
}
