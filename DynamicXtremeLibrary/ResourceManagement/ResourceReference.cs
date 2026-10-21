using DynamicXtremeLibrary.ResourceManagement;

namespace DynamicXtremeLibrary
{
    /// <summary>Associates a resource with its containing buffer and current position.</summary>
    public class ResourceReference
    {
        /// <summary>Gets the ID of the containing buffer.</summary>
        public int BufferID { get; private set; }
        /// <summary>Gets or sets the buffer-relative or absolute ROM position.</summary>
        public int Position { get; set; }
        /// <summary>Gets the referenced resource.</summary>
        public Resource Resource { get; private set; }
        /// <summary>Initializes a resource reference.</summary>
        public ResourceReference(int bufferID, int position, Resource resource)
        {
            BufferID = bufferID;
            Position = position;
            Resource = resource;
        }
        /// <summary>Returns the resource, type, buffer, and position tuple.</summary>
        public override string ToString()
            => $"({Resource.ID}, {Resource.Type}, {BufferID}, {Position})";
    }
}
