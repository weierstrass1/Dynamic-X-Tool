using System.Text.Json;

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
            JsonElement je = JsonSerializer.Deserialize<JsonElement>(content);
            NoFlip = new(je.GetProperty(nameof(NoFlip)));
            FlipX = new(je.GetProperty(nameof(FlipX)));
            FlipY = new(je.GetProperty(nameof(FlipY)));
            FlipXY = new(je.GetProperty(nameof(FlipXY)));
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
