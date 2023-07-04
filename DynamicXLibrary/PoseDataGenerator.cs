namespace DynamicXLibrary
{
    public class PoseDataGenerator
    {
        private static readonly string template_path = Path.Combine("ASM", "PoseDataTemplate.asm");
        private const string offset_label = "<Offset>";
        private const string length_label = "<Length>";
        private const string routine_label = "<Routine>";
        public static string GenerateEmpty()
        {
            string content = File.ReadAllText(template_path);
            content = content.Replace(offset_label, "\n\tdw $0000");

            content = content.Replace(length_label, "\n\tdw $0000");

            content = content.Replace(routine_label, "\n\tdl $000000");

            return content;
        }
        public static string GenerateData(List<int> addresses, List<FrameInfo> fis)
        {
            string content = File.ReadAllText(template_path);

            content = content.Replace(offset_label,
                        HexReader.ValuesToString(
                            FrameInfo.GetOffset(fis), 4));

            content = content.Replace(length_label,
                        HexReader.ValuesToString(
                            fis.Select(x => x.GetLength() - 1).ToArray(), 4));

            content = content.Replace(routine_label,
                        HexReader.ValuesToString(
                            FrameInfo.GetRoutinesAddresses(addresses, fis), 6, 8));
            return content;
        }
    }
}
