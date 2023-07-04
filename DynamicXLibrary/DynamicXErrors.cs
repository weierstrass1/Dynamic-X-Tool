namespace DynamicXLibrary
{
    public class DynamicXErrors
    {
        public static DynamicXErrors ROMNotFound = new("ROM Not Found");
        public static DynamicXErrors OutputDirectoryNotFound = new("Output's Directory Not Found");
        public static DynamicXErrors BuffersNotGenerated = new("Buffers must be generated");
        public static DynamicXErrors ROMWithoutFreeSpace = new("ROM Doesn't have enough Free Space");
        public string Description { get; private set; }
        private DynamicXErrors(string description)
        {
            Description = description;
            Log.WriteLine(description);
        }
        public static implicit operator string(DynamicXErrors err)
            => err.Description;
    }
}
