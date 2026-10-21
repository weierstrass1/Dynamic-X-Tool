using LogRegister;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    /// <summary>Broadcasts rendered log spans to multiple subscribed outputs.</summary>
    public class MultiWrapper
    {
        /// <summary>Occurs for each rendered log span.</summary>
        public event LogRenderAction Actions;
        /// <summary>Forwards one rendered span to all subscribers.</summary>
        public void RenderAction(string text, ILogCategory category, SpanType type, bool mustWrite = false)
        {
            if (Actions != null)
                Actions.Invoke(text, category, type, mustWrite: mustWrite);
        }
    }
}
