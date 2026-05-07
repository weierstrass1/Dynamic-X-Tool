using Newtonsoft.Json.Linq;

namespace DynamicXLibrary.JSON
{
    public class FlipJSON : JSON
    {
        public SimpleJSON NoFlip { get; private set; }
        public SimpleJSON FlipX { get; private set; }
        public SimpleJSON FlipY { get; private set; }
        public SimpleJSON FlipXY { get; private set; }
        internal FlipJSON()
        {
            string content = File.ReadAllText("Json/Flip.json");
            JObject node = JObject.Parse(content);

            NoFlip = new(node[nameof(NoFlip)]!);
            FlipX = new(node[nameof(FlipX)]!);
            FlipY = new(node[nameof(FlipY)]!);
            FlipXY = new(node[nameof(FlipXY)]!);
        }
        public SimpleJSON SelectReplacement(bool flipx, bool flipy)
        {
            if (flipx && flipy)
                return FlipXY;
            if(flipx)
                return FlipX;
            if(flipy)
                return FlipY;

            return NoFlip;
        }
    }
}
