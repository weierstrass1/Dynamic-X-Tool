using AsarCLR;

namespace DynamicXLibrary
{
    public class PatchApplier
    {
        public static string Apply(string rompath, string patchPath)
            => Apply(rompath, patchPath, rompath);
        public static string Apply(string rompath, string patchPath, string outputPath)
        {
            byte[] romData = File.ReadAllBytes(rompath);
            string result = Apply(romData, patchPath);
            if (File.Exists(outputPath)) 
                File.Delete(outputPath);
            File.WriteAllBytes(outputPath, romData);
            return result;
        }
        public static string Apply(byte[] romData, string patchPath)
        {
            byte[] withoutHeader = romData[512..];
            _ = Asar.Patch(patchPath, ref withoutHeader);
            var err = Asar.GetErrors();
            withoutHeader.CopyTo(romData, 512);
            return string.Join('\n', Asar.GetPrints());
        }
    }
}
