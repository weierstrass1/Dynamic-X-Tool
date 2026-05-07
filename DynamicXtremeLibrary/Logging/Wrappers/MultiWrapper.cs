using LogRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    public class MultiWrapper
    {
        public event LogRenderAction Actions;
        public void RenderAction(string text, ILogCategory category, SpanType type)
        {
            if (Actions != null)
                Actions.Invoke(text, category, type);
        }
    }
}
