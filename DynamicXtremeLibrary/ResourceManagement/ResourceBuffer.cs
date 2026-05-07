namespace DynamicXtremeLibrary.ResourceManagement
{
    public class ResourceBuffer : Resource
    {
        private readonly List<ResourceReference> _references;
        private bool _dirty;
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
        public ResourceBuffer(int id) : base(id, $"Buffer{id}", ResourceType.Buffer, [])
        {
            _references = [];
            _dirty = false;
        }
        public void AddReference(ResourceReference reference)
        {
            _references.Add(reference);
            _dirty = true;
        }
        public void AddOffsetPosition(int offset)
        {
            foreach (var reference in _references)
            {
                reference.Position += offset;
            }
        }
        public IReadOnlyList<ResourceReference> GetReferences()
        {
            return _references.AsReadOnly();
        }
        public override string ToString()
        {
            return $"({ID}, {Data?.Length ?? 0}, {_references.Count})";
        }
    }
}
