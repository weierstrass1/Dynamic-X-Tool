using System.Text.Json;

namespace DynamicXLibrary.JSON
{
    public class SimpleJSON : JSON
    {
        public ASMText PreLoop { get; set; }
        public ASMText InLoop { get; private set; }
        public SimpleJSON(JsonElement je)
        {
            PreLoop = new(getContent(je, "PreLoop"), je.GetProperty("PreLoopSize").GetInt32());
            InLoop = new(getContent(je, "InLoop"), je.GetProperty("InLoopSize").GetInt32());
        }
    }
}
