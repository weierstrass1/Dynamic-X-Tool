using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    public class RawTextWrapper
    {
        private readonly StringBuilder builder;
        public RawTextWrapper() 
        {
            builder = new();
        }
        public void RenderAction(string text, ILogCategory category, SpanType type)
        {
            if (type == SpanType.Prefix)
                return;
            builder.Append(text);
        }
        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
