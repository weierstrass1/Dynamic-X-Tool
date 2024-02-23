using System.Text;

namespace DynamicXLibrary
{
    public class Log
    {
        private static readonly Log instance = new();
        private readonly StringBuilder builder = new();
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            instance.builder.AppendLine(message);
        }
        public static string GetLog()
            => instance.builder.ToString();
    }
}
