using DynamicXSNES;
using System.Collections.Generic;
using System.Text;

namespace Dynamic_X_Patch
{
    public class ResourceTablePatchManager
    {
        private static readonly string template_path = Path.Combine("ASM", "ResourceTableTemplate.asm");
        private const string empty_value = "\tdl $000000";
        private const string offset_label = "<Offset>";
        private const string values_label = "<Values>";
        public static string Generate(int numberOfValues, int offset)
        {
            StringBuilder sb = new();
            for(int i=0;i<numberOfValues; i++) 
                sb.AppendLine(empty_value);
            string res = File.ReadAllText(template_path);
            return res
                    .Replace(offset_label, offset.ToString("X4"))
                    .Replace(values_label, sb.ToString());
        }
        public static List<(int,int)> Remove(byte[] rom, int address, Mapper mapper, int offset = 0)
        {
            int pointer = SNESROMUtils.JoinAddress(rom[address + 2], rom[address + 1], rom[address]);
            int snesp = SNESROMUtils.SNEStoPC(pointer, mapper);
            pointer = snesp < 0 ? pointer : snesp;
            int addr = pointer;
            address = pointer;
            List<(int, int)> l = new();
            int size;
            while (pointer != 0xFFFFFF + offset)
            {
                pointer = SNESROMUtils.JoinAddress(rom[addr + 2], rom[addr + 1], rom[addr]) + offset;
                size = SNESROMUtils.RemoveAt(rom, pointer);
                if (size > 0)
                    l.Add((SNESROMUtils.PCtoSNES(pointer,mapper), size));
                addr += 3;
            }
            l.Add((SNESROMUtils.PCtoSNES(address, mapper), SNESROMUtils.RemoveAt(rom, address - 8)));
            return l;
        }
    }
}
