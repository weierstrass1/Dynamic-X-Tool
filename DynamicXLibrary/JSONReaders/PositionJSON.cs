using System.Text.Json;

namespace DynamicXLibrary.JSON
{
    public class PositionJSON : JSON
    {
        public SimpleJSON SameXY { get; set; }
        public SimpleJSON SameX { get; set; }
        public SimpleJSON SameY { get; set; }
        public SimpleJSON DifferentXY { get; set; }
        internal PositionJSON() : base()
        {
            string content = File.ReadAllText("Json/Position.json");
            JsonElement je = JsonSerializer.Deserialize<JsonElement>(content);
            SameXY = new(je.GetProperty(nameof(SameXY)));
            SameX = new(je.GetProperty(nameof(SameX)));
            SameY = new(je.GetProperty(nameof(SameY)));
            DifferentXY = new(je.GetProperty(nameof(DifferentXY)));
        }
        public SimpleJSON SelectReplacement(bool AllEqualsX, bool AllEqualsY)
        {
            if (AllEqualsX && AllEqualsY)
                return SameXY;
            if (AllEqualsX)
                return SameY;
            if (AllEqualsY)
                return SameY;
            return DifferentXY;
        }
    }
}
