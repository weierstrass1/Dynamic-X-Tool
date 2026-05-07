using Newtonsoft.Json.Linq;

namespace DynamicXLibrary.JSON
{
    public class SimpleJSON : JSON
    {
        public ASMText PreLoop { get; set; }
        public ASMText InLoop { get; private set; }
        public SimpleJSON(JToken token)
        {
            PreLoop = new(getContent(token, "PreLoop"), int.Parse(token["PreLoopSize"]!.ToString()));
            InLoop = new(getContent(token, "InLoop"), int.Parse(token["InLoopSize"]!.ToString()));
        }
    }
}
