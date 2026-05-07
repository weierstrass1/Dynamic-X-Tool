using Newtonsoft.Json.Linq;

namespace DynamicXLibrary.JSON
{
    public class JSON
    {
        public static PositionJSON Position { get; private set; } = new();
        public static GenericJSON Property { get; private set; } = new(Path.Combine("Json","Property.json"));
        public static GenericJSON Size { get; private set; } = new(Path.Combine("Json", "Size.json"));
        public static TileJSON Tile { get; private set; } = new();
        public static FlipJSON Flip { get; private set; } = new();
        protected JSON() { }
        protected static string getContent(JToken token, string prop)
        {
            string[] vals = token[prop]!.Values<string>().ToArray()!;

            if (vals.Length == 0 || (vals.Length == 1 && (vals[0] == "" || vals[0] == null)))
                return "";

            for (int i = 0; i < vals.Length; i++)
                if (vals[i][0] == ' ')
                    vals[i] = "\t" + vals[i][1..];
            return string.Join("\n", vals);
        }
    }
}
