using DynamicXLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXTool
{
    public class DynamicX
    {
        public static void Run(string[]? args)
        {
            if (args != null && args.Length > 0)
            {
                DynamicXProcess dxprocess = new(args[0]);
                string op = (args.Length > 1 ? args[1] : args[0]).Replace("\"", "");

                dxprocess.Process(args[0], op);
                Console.WriteLine("Inserted Successfully");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Enter ROM Path");
            string? rompath = Console.ReadLine()?.Replace("\"", "");
            if (rompath == null || !File.Exists(rompath))
            {
                Console.WriteLine(DynamicXErrors.ROMNotFound);
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Enter Output Path (or press enter to Skip)");
            string? outpath = Console.ReadLine();
            if (outpath == string.Empty)
                outpath = rompath;
            DynamicXProcess dxp = new(rompath!);
            dxp.Process(rompath!, outpath!);
            Console.WriteLine("Inserted Successfully");
            Console.ReadLine();
        }
    }
}
