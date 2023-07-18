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
            if (!Asar.Patch(patchPath, ref withoutHeader))
                throw new Exception(string
                    .Join('\n', $"Asar patch {patchPath} failed.", 
                        Asar.GetErrors()
                            .Select(x => $"Line {x.Line} : {x.Rawerrdata}")
                            .ToArray()));
            withoutHeader.CopyTo(romData, 512);
            return string.Join('\n', Asar.GetPrints());
        }
    }
}
