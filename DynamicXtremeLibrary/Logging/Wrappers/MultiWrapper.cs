using LogRegister;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    public class MultiWrapper
    {
        public event LogRenderAction Actions;
        public void RenderAction(string text, ILogCategory category, SpanType type, bool mustWrite = false)
        {
            if (Actions != null)
                Actions.Invoke(text, category, type, mustWrite: mustWrite);
        }
    }
}
