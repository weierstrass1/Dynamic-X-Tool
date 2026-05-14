using LogRegister;

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
