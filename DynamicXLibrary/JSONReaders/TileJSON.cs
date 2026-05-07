using Newtonsoft.Json.Linq;

namespace DynamicXLibrary.JSON
{
    public class TileJSON : JSON
    {
        public GenericJSON Dynamic { get; private set; }
        public GenericJSON NotDynamic { get; private set; }
        internal TileJSON() : base()
        {
            string content = File.ReadAllText("Json/Tile.json");
            JObject node = JObject.Parse(content); 
            Dynamic = new(node["Dynamic"]!);
            NotDynamic = new(node["NotDynamic"]!);
        }
        public SimpleJSON SelectReplacement(bool dynamic, bool AllEquals)
            => dynamic?
                    Dynamic.SelectReplacement(AllEquals) :
                    NotDynamic.SelectReplacement(AllEquals);
    }
}
