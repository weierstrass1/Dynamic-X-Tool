// See https://aka.ms/new-console-template for more information

namespace DynamicXTool
{
    public class Program
    {
        private static void Main(string[]? args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(Environment.CurrentDirectory);
            DynamicX.Run(args);
        }
    }
}