using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.Readers;
using DynamicXtremeLibrary.ResourceManagement;
using SNESLibrary;

namespace DynamicXtremeLibrary.Generators
{
    /// <summary>Populates the Asar tables used to locate and describe dynamic poses.</summary>
    public class DynamicPoseDataGenerator
    {
        private static readonly string template_path = Path.Combine("ASM", "DynamicPoseDataTemplate.asm");
        private const string size_label = "<Size>";
        private const string blocks_label = "<Blocks>";
        private const string resource_label = "<Resource>";
        private const string sizes_label = "<Sizes>";
        private const string last_row_label = "<LastRow>";
        /// <summary>Builds a valid template containing only sentinel and zero-sized entries.</summary>
        /// <returns>The empty dynamic-pose table source.</returns>
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
        /// <summary>Builds dynamic-pose size, block, address, chunk-size, and last-row tables.</summary>
        /// <param name="mapper">The ROM mapping used for address conversion.</param>
        /// <param name="refs">The inserted dynamic-pose resource references.</param>
        /// <param name="dis">The DynamicInfo definitions that describe each pose.</param>
        /// <returns>The populated Asar table source.</returns>
        public static string GenerateData(Mapper mapper, IEnumerable<ResourceReference> refs, IEnumerable<DynamicInfo> dis)
        {
            string content = File.ReadAllText(template_path);
            ResourceReference[] poseReferences = [.. refs.OrderBy(reference => reference.Resource.ID)];
            DynamicPoseResource[] canonicalPoses =
                [.. poseReferences.Select(reference => reference.Resource)
                    .OfType<DynamicPoseResource>()];
            bool hasCanonicalMetadata = poseReferences.Length > 0 &&
                canonicalPoses.Length == poseReferences.Length;

            List<int> PoseSizes = hasCanonicalMetadata
                ? [.. canonicalPoses.Select(pose => pose.PoseSize)]
                : [.. dis.SelectMany(info => info.GetPosesSizes())];
            content = content.Replace(size_label,
                HexReader.ValuesToString([.. PoseSizes], 4));

            List<int> PoseBlocks = hasCanonicalMetadata
                ? [.. canonicalPoses.Select(pose => pose.Blocks)]
                : [.. dis.SelectMany(info => info.GetPosesBlocks())];
            content = content.Replace(blocks_label, 
                HexReader.ValuesToString([.. PoseBlocks], 2));

            content = content.Replace(resource_label,
                        HexReader.ValuesToString(
                            [.. poseReferences.Select(r => SNESROMUtils.PCtoSNES(r.Position, mapper))], 6, 8));

            int[] chunkSizes = hasCanonicalMetadata
                ? [.. canonicalPoses.SelectMany(pose =>
                    new[] { pose.FirstChunkSize, pose.SecondChunkSize })]
                : DynamicInfo.GetSizes(dis);
            content = content.Replace(sizes_label,
                        HexReader.ValuesToString(chunkSizes, 4, 2));

            int[] lastRows = hasCanonicalMetadata
                ? [.. canonicalPoses.Select(pose => pose.LastRow)]
                : DynamicInfo.GetLastRow(dis);
            content = content.Replace(last_row_label,
                        HexReader.ValuesToString(lastRows, 4));

            return content;
        }
    }
}
