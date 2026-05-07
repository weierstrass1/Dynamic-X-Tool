using Newtonsoft.Json.Linq;

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
            string content = File.ReadAllText(Path.Combine("Json", "Position.json"));
            JObject node = JObject.Parse(content);
            
            SameXY = new(node[nameof(SameXY)]!);
            SameX = new(node[nameof(SameX)]!);
            SameY = new(node[nameof(SameY)]!);
            DifferentXY = new(node[nameof(DifferentXY)]!);
        }
        public SimpleJSON SelectReplacement(bool AllEqualsX, bool AllEqualsY)
        {
            if (AllEqualsX && AllEqualsY)
                return SameXY;
            if (AllEqualsX)
                return SameX;
            if (AllEqualsY)
                return SameY;
            return DifferentXY;
        }
    }
}
