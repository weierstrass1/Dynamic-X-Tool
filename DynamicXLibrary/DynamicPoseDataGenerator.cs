using DynamicXSNES;

namespace DynamicXLibrary
{
    public class DynamicPoseDataGenerator
    {
        private static readonly string template_path = Path.Combine("ASM", "DynamicPoseDataTemplate.asm");
        private const string size_label = "<Size>";
        private const string blocks_label = "<Blocks>";
        private const string resource_label = "<Resource>";
        private const string sizes_label = "<Sizes>";
        private const string last_row_label = "<LastRow>";
        public static string GenerateEmpty()
        {
            string content = File.ReadAllText(template_path);
            content = content.Replace(size_label, "\n\tdw $0000");

            content = content.Replace(blocks_label, "\n\tdb $00");

            content = content.Replace(resource_label, "\n\tdl $000000");

            content = content.Replace(sizes_label, "\n\tdw $0000,$0000");

            content = content.Replace(last_row_label, "\n\tdw $0000");

            return content;
        }
        public static string GenerateData(Mapper mapper, List<ResourceReference> refs, List<DynamicInfo> dis)
        {
            string content = File.ReadAllText(template_path);
            List<int> PoseSizes = new();
            foreach (var di in dis)
                PoseSizes.AddRange(di.GetPosesSizes());
            content = content.Replace(size_label,HexReader.ValuesToString(PoseSizes.ToArray(), 4));

            List<int> PoseBlocks = new();
            foreach (var di in dis)
                PoseBlocks.AddRange(di.GetPosesBlocks());
            content = content.Replace(blocks_label, HexReader.ValuesToString(PoseBlocks.ToArray(), 2));

            content = content.Replace(resource_label,
                        HexReader.ValuesToString(
                            refs.Select(r => SNESROMUtils.PCtoSNES(r.Position + 8, mapper))
                            .ToArray(), 6, 8));

            content = content.Replace(sizes_label,
                        HexReader.ValuesToString(DynamicInfo.GetSizes(dis), 4, 2));

            content = content.Replace(last_row_label,
                        HexReader.ValuesToString(DynamicInfo.GetLastRow(dis), 4));

            return content;
        }
    }
}
