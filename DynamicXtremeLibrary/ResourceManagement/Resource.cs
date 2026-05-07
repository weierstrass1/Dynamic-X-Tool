namespace DynamicXtremeLibrary.ResourceManagement
{
    public enum ResourceType
    {
        GeneralResource,
        Palette,
        DynamicPose,
        Buffer
    }
    public class Resource
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public ResourceType Type { get; private set; }
        public virtual byte[] Data { get; protected set; }
        public int Length => Data.Length;
        public Resource(int id, string name, ResourceType type, byte[] data)
        {
            ID = id;
            Type = type;
            Data = data;
            Name = name;
        }
        public override string ToString()
        {
            return $"({ID}, {Name}, {Type}, {Length})";
        }
    }
}
