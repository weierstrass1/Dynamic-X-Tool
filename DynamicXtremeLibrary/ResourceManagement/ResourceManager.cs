using SNESLibrary;
using DynamicXtremeLibrary.Defines;
using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using System.Text;
using DynamicXtremeLibrary.Infos;

namespace DynamicXtremeLibrary.ResourceManagement
{
    /// <summary>Packs resources, allocates ROM space, and generates resource lookup tables.</summary>
    public class ResourceManager
    {
        /// <summary>Builds parallel palette ID and address tables in DynamicInfo order.</summary>
        public static string BuildPalettesTable(IEnumerable<DynamicInfo> dynamicInfos)
        {
            StringBuilder ids = new();
            StringBuilder addresses = new();

            string filename;
            foreach (var dyn in dynamicInfos)
            {
                foreach (var pal in dyn.Palettes ?? [])
                {
                    filename = dyn.GetPaletteName(pal);
                    ids.AppendLine($"\tdw !PaletteID{filename}");
                    addresses.AppendLine($"\tdl !Palette{filename}");
                }
            }
            string result = $"""
                PaletteTable:
                .IDs
                {ids}
                .Addresses
                {addresses}
                """;
            return result;
        }
        /// <summary>Builds the terminated long-address table for inserted buffers.</summary>
        public static string BuildBufferTable(IEnumerable<ResourceReference> buffers)
        {
            var res = buffers.OrderBy(r => r.Resource.ID);
            StringBuilder sb = new();
            sb.AppendLine("BufferTable:");
            foreach (var reference in res)
            {
                sb.AppendLine($"\tdl !Buffer{reference.Resource.Name}");
            }
            sb.AppendLine("\tdl $FFFFFF");
            return sb.ToString();
        }
        /// <summary>Builds address defines for a resource-reference sequence.</summary>
        public static string BuildResourceDefines(IEnumerable<ResourceReference> resources, string prefix)
        {
            var res = resources.OrderBy(r=>r.Resource.ID);
            StringBuilder sb = new();
            foreach (var reference in res)
            {
                sb.AppendLine($"!{prefix}{reference.Resource.Name} = ${reference.Position:X6}");
            }
            return sb.ToString();
        }
        /// <summary>Allocates every buffer in free ROM space and returns absolute references.</summary>
        public static ResourceAllReferences? InsertBuffers(LogRegisterSystem logging, byte[] rom, IReadOnlyList<ResourceBuffer> buffers)
        {
            Mapper mapper = SNESROMUtils.GetMapper(rom);
            List<(int, int)> spaces = SNESROMUtils.FindFreeSpace(rom);
            spaces.Sort(comp);
            (int, int) space;
            List<(ResourceBuffer Buffer, (int, int) Space)> allocations = [];
            List<ResourceReference> references = [];
            List<ResourceReference> posesRefs = [];
            List<ResourceReference> palRefs = [];
            List<ResourceReference> resRefs = [];
            ResourceReference bufRef;

            if(buffers.Count == 0)
            {
                return new()
                {
                    Buffers = references.AsReadOnly(),
                    DynamicPoses = posesRefs.AsReadOnly(),
                    Palettes = palRefs.AsReadOnly(),
                    GeneralResources = resRefs.AsReadOnly()
                };
            }
            logging.Add(new Title("Resource Buffers"));
            foreach (var buffer in buffers)
            {
                space = spaces.FirstOrDefault(x => x.Item2 >= 8 + buffer.Length);
                if (space == default)
                {
                    logging.Add(new NotEnoughSpaceInROM());
                    return null;
                }
                spaces.Remove(space);
                allocations.Add((buffer, space));
            }
            foreach (var allocation in allocations)
            {
                ResourceBuffer buffer = allocation.Buffer;
                space = allocation.Space;
                SNESROMUtils.InsertDataWithRats(rom, space.Item1, buffer.Data);
                bufRef = new(buffer.ID, SNESROMUtils.PCtoSNES(space.Item1, mapper) + 8, buffer);
                references.Add(bufRef);
                buffer.AddOffsetPosition(space.Item1 + 8);
                logging.Add(new ResourceInsertedAt(bufRef));
                posesRefs.AddRange(buffer.GetReferences().Where(r => r.Resource.Type == ResourceType.DynamicPose));
                palRefs.AddRange(buffer.GetReferences().Where(r => r.Resource.Type == ResourceType.Palette));
                resRefs.AddRange(buffer.GetReferences().Where(r => r.Resource.Type == ResourceType.GeneralResource));
            }
            posesRefs.Sort((a, b) => a.Resource.ID.CompareTo(b.Resource.ID));
            palRefs.Sort((a, b) => a.Resource.ID.CompareTo(b.Resource.ID));
            resRefs.Sort((a, b) => a.Resource.ID.CompareTo(b.Resource.ID));
            if (posesRefs.Count != 0)
            {
                logging.Add(new Title("Dynamic Poses"));
                foreach (var reference in posesRefs)
                    logging.Add(new ResourceInsertedAt(reference));
            }
            if (palRefs.Count != 0)
            {
                logging.Add(new Title("Palettes"));
                foreach (var reference in palRefs)
                    logging.Add(new ResourceInsertedAt(reference));
            }
            if (resRefs.Count != 0)
            {
                logging.Add(new Title("Resources"));
                foreach (var reference in resRefs)
                    logging.Add(new ResourceInsertedAt(reference));
            }
            return new()
            {
                Buffers = references.AsReadOnly(),
                DynamicPoses = posesRefs!.AsReadOnly(),
                Palettes = palRefs!.AsReadOnly(),
                GeneralResources = resRefs!.AsReadOnly()
            };
        }
        /// <summary>Orders free-space blocks by preferred ROM region, size, and address.</summary>
        private static int comp((int, int) x1, (int, int) x2)
        {

            if (x1.Item1 >= 0x200000 && x2.Item1 < 0x200000)
                return -1;
            if (x1.Item1 < 0x200000 && x2.Item1 >= 0x200000)
                return 1;
            if (x1.Item2 < x2.Item2)
                return -1;
            if (x1.Item2 > x2.Item2)
                return 1;
            if (x1.Item1 < x2.Item1)
                return -1;
            if (x1.Item1 > x2.Item1)
                return 1;
            return 0;
        }
        /// <summary>Packs resources into buffers without exceeding the 32 KiB allocation limit.</summary>
        public static IReadOnlyList<ResourceBuffer> MergeResources(IEnumerable<Resource> resources)
        {
            List<Resource> positions = [.. resources.OrderByDescending(r=>r.Length)];

            byte[] resBuff;
            ResourceBuffer currentbuffer;
            List<ResourceBuffer> result = [];
            int size;
            bool find;
            int buffIndex = 0;
            int counter;
            while (positions.Count > 0)
            {
                size = 0;
                currentbuffer = new(buffIndex);
                find = true;
                while (find)
                {
                    find = false;
                    foreach (var buff in positions)
                    {
                        if (buff.Length + size <= 32760)
                        {
                            find = true;

                            currentbuffer.AddReference(
                                new(buffIndex, size, buff));
                            size += buff.Length;
                            positions.Remove(buff);
                            break;
                        }
                    }
                }
                resBuff = new byte[size];
                counter = 0;
                foreach (var buff in currentbuffer.GetReferences())
                {
                    buff.Resource.Data.CopyTo(resBuff, counter);
                    counter += buff.Resource.Length;
                }
                result.Add(currentbuffer);
                buffIndex++;
            }
            return result.AsReadOnly();
        }
    }
}
