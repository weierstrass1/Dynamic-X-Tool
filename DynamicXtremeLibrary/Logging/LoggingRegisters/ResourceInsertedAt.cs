using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremeLibrary.ResourceManagement;

namespace DynamicXtremeLibrary.Logging.LoggingRegisters
{
    public class ResourceInsertedAt : ILoggingRegister
    {
        public ILogCategory Category => new Info();

        public string MessageType => "RESOURCE INSERTED AT";

        public IReadOnlyDictionary<string, string> Parameters { get; }
        public ResourceInsertedAt(ResourceReference reference)
        {
            string type = reference.Resource.Type switch
            {
                ResourceType.GeneralResource => "Resource",
                ResourceType.Palette => "Palette",
                ResourceType.DynamicPose => "Dynamic Pose",
                ResourceType.Buffer => "Buffer",
                ResourceType.GraphicRoutine => "Graphic Routine",
                _ => "Unknown"
            };
            var pars = new Dictionary<string, string>
            {
                { "type", type },
                { "id", $"'{reference.Resource.ID}'" },
                { "name",
                     reference.Resource.Type != ResourceType.Buffer?
                        $" {type}":
                        ""},
                { "address", $"${reference.Position:X6}" },
                { "size", $"{reference.Resource.Length} bytes" },
                {
                    "buffer",
                    reference.Resource.Type != ResourceType.Buffer &&
                    reference.Resource.Type != ResourceType.GraphicRoutine ?
                        $" at buffer {reference.BufferID}" :
                        ""
                }
            };

            Parameters = pars;
        }
    }
}
