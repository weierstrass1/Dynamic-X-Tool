using LogRegister;
using DynamicXtremeLibrary.Logging.Categories;

namespace DynamicXtremeLibrary.Logging.Wrappers
{
    public class ConsoleWrapper
    {
        public static void RenderAction(string text, ILogCategory category, SpanType type)
        {
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
