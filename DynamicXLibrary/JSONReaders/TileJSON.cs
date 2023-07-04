using System.Text.Json;

namespace DynamicXLibrary.JSON
{
    public class TileJSON : JSON
    {
        public GenericJSON Dynamic { get; private set; }
        public GenericJSON NotDynamic { get; private set; }
        internal TileJSON() : base()
        {
            string content = File.ReadAllText("Json/Tile.json");
            JsonElement je = JsonSerializer.Deserialize<JsonElement>(content);
            Dynamic = new(je.GetProperty("Dynamic"));
            NotDynamic = new(je.GetProperty("NotDynamic"));
        }
        public SimpleJSON SelectReplacement(bool dynamic, bool AllEquals)
            => dynamic?
                    Dynamic.SelectReplacement(AllEquals) :
                    NotDynamic.SelectReplacement(AllEquals);
    }
}
