using System.Text.Json;

namespace DynamicXLibrary.JSON
{
    public class GenericJSON : JSON
    {
        public SimpleJSON? Same { get; private set; }
        public SimpleJSON? Different { get; private set; }
        internal GenericJSON(string path) : base()
        {
            string content = File.ReadAllText(path);
            JsonElement je = JsonSerializer.Deserialize<JsonElement>(content);
            initialize(je);
        }
        internal GenericJSON(JsonElement je)
        {
            initialize(je);
        }
        private void initialize(JsonElement je)
        {
            Same = new(je.GetProperty(nameof(Same)));
            Different = new(je.GetProperty(nameof(Different)));
        }
        public SimpleJSON SelectReplacement(bool AllEquals)
            => AllEquals ?
                    Same! :
                    Different!;
    }
}
