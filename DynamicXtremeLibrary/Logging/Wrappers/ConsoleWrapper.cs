using DynamicXtremeLibrary.Logging.Categories;
using LogRegister;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    /// <summary>Renders categorized log spans to the console with semantic colors.</summary>
    public class ConsoleWrapper
    {
        /// <summary>Writes one non-prefix span when the renderer marks it for output.</summary>
        public static void RenderAction(string text, ILogCategory category, SpanType type, bool mustWrite = false)
        {
            if (!mustWrite)
                return;
            if (type == SpanType.Prefix)
                return;
            ConsoleColor color;
            switch (category)
            {
                case Title:
                    color = type == SpanType.NormalText ?
                        ConsoleColor.Magenta :
                        ConsoleColor.DarkMagenta;
                    break;
                case Error:
                    color = type == SpanType.NormalText ?
                        ConsoleColor.Red :
                        ConsoleColor.DarkRed;
                    break;
                case Warning:
                    color = type == SpanType.NormalText ?
                        ConsoleColor.Yellow :
                        ConsoleColor.DarkYellow;
                    break;
                case Success:
                    color = type == SpanType.NormalText ?
                        ConsoleColor.Green :
                        ConsoleColor.DarkGreen;
                    break;
                default:
                    color = type == SpanType.NormalText ?
                        ConsoleColor.Gray :
                        ConsoleColor.Cyan;
                    break;
            }
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
    }
}
