using DynamicXtremeLibrary.ResourceManagement;

namespace DynamicXtremeLibrary
{
    public class ResourceReference
    {
        public int BufferID { get; private set; }
        public int Position { get; set; }
        public Resource Resource { get; private set; }
        public ResourceReference(int bufferID, int position, Resource resource)
        {
            BufferID = bufferID;
            Position = position;
            Resource = resource;
        }
        public override string ToString()
            => $"({Resource.ID}, {Resource.Type}, {BufferID}, {Position})";
    }
}
