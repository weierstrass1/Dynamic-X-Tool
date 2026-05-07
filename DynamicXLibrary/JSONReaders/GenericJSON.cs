using Newtonsoft.Json.Linq;

namespace DynamicXLibrary.JSON
{
    public class GenericJSON : JSON
    {
        public SimpleJSON? Same { get; private set; }
        public SimpleJSON? Different { get; private set; }
        internal GenericJSON(string path) : base()
        {
            string content = File.ReadAllText(path);
            JObject node = JObject.Parse(content);
            initialize(node);
        }
        internal GenericJSON(JToken token)
        {
            initialize(token);
        }
        private void initialize(JToken token)
        {
            Same = new(token[nameof(Same)]!);
            Different = new(token[nameof(Different)]!);
        }
        public SimpleJSON SelectReplacement(bool AllEquals)
            => AllEquals ?
                    Same! :
                    Different!;
    }
}
