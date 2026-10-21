using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    /// <summary>Collects rendered non-prefix spans as unformatted text.</summary>
    public class RawTextWrapper
    {
        private readonly StringBuilder builder;
        /// <summary>Initializes an empty text accumulator.</summary>
        public RawTextWrapper() 
        {
            builder = new();
        }
        /// <summary>Appends one rendered span unless it is a message prefix.</summary>
        public void RenderAction(string text, ILogCategory category, SpanType type, bool mustWrite = false)
        {
            if (type == SpanType.Prefix)
                return;
            builder.Append(text);
        }
        /// <summary>Returns all accumulated raw text.</summary>
        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
