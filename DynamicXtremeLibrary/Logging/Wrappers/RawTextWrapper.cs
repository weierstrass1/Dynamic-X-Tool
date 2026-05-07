using LogRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    public class RawTextWrapper
    {
        private StringBuilder builder;
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
    }
}
