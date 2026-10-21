using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;
using SNESLibrary;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.Defines
{
    /// <summary>Generates the Asar defines that expose installed resources, poses, and palette effects.</summary>
    public class DefineGenerator
    {
        /// <summary>Gets the template prepended to generated defines.</summary>
        public string BaseDefineFile { get; private set; }
        /// <summary>Gets the patch-local output path.</summary>
        public string OutputFile { get; private set; }
        /// <summary>Gets the externally exported output path.</summary>
        public string OutputFileExternal { get; private set; }
        /// <summary>Initializes a define generator and verifies its template exists.</summary>
        /// <param name="baseDefineFile">The define template path.</param>
        /// <param name="outputFile">The patch-local output path.</param>
        /// <param name="outputFileExternal">The externally exported output path.</param>
        public DefineGenerator(string baseDefineFile, string outputFile, string outputFileExternal)
        {
            BaseDefineFile = baseDefineFile;
            if (!File.Exists(BaseDefineFile))
                throw new FileNotFoundException(nameof(BaseDefineFile));
            OutputFile = outputFile;
            OutputFileExternal = outputFileExternal;
        }
        /// <summary>Combines the template with every generated define family and writes both outputs.</summary>
        public void GenerateDefinesFile(ResourceAllReferences? refs, Mapper mapper, IEnumerable<DrawInfo>? drawInfos, IEnumerable<DynamicInfo>? dynamicInfos, IEnumerable<PaletteEffectCollection>? paletteEffects)
        {
            string content = File.ReadAllText(BaseDefineFile);
            content = $"{content}\n{GenerateReferenceDefines(refs, mapper)}\n{GeneratePoseDefines(drawInfos)}\n{GeneratePaletteEffectDefines(paletteEffects)}\n{GeneratePaletteOffsets(dynamicInfos)}";
            content = content.Replace("\r\n", "\n");
            content = Regex.Replace(content, @"(\s*\n)(\s*\n)+", "\n\n");
            File.WriteAllText(OutputFile, content);
            File.WriteAllText(OutputFileExternal, content);
        }
        /// <summary>Builds palette-table offsets for each DynamicInfo context.</summary>
        /// <param name="dynamicInfos">The installed DynamicInfo definitions.</param>
        /// <returns>The generated palette-table define source.</returns>
        public static string GeneratePaletteOffsets(IEnumerable<DynamicInfo>? dynamicInfos)
        {
            if (dynamicInfos == null || dynamicInfos.Count() == 0)
                return "";
            StringBuilder sb = new();
            int palCounter = 0;
            foreach (var dynInfo in dynamicInfos)
            {
                if (dynInfo.Palettes == null)
                    continue;
                sb.AppendLine($"!{dynInfo.ContextName}PaletteTableOffset = ${palCounter:X4}");
                palCounter += dynInfo.Palettes.Length;
            }
            string start = $"""
            !PaletteTableSize = ${palCounter:X4}
            !PaletteIDTables = !PaletteTables
            !PaletteAddrTables = !PaletteIDTables+(!PaletteTableSize*2)

            """;
            return $"{start}\n{sb}";
        }
        /// <summary>Builds IDs, ROM addresses, and sizes for every inserted resource.</summary>
        /// <param name="refs">The inserted resource references.</param>
        /// <param name="mapper">The ROM mapping used to convert PC addresses.</param>
        /// <returns>The generated resource define source.</returns>
        public static string GenerateReferenceDefines(ResourceAllReferences? refs, Mapper mapper)
        {
            if (refs == null)
                return "";
            var buffs = refs.Buffers.OrderBy(r => r.Resource.ID);
            var dynP = refs.DynamicPoses.OrderBy(r => r.Resource.ID);
            var pals = refs.Palettes.OrderBy(r => r.Resource.ID);
            var res = refs.GeneralResources.OrderBy(r => r.Resource.ID);
            StringBuilder ids = new();
            StringBuilder addrs = new();
            StringBuilder sizes = new();
            StringBuilder result = new();
            foreach (var b in buffs)
            {
                ids.AppendLine($"!BufferID{b.Resource.Name} = ${b.Resource.ID:X4}");
                addrs.AppendLine($"!Buffer{b.Resource.Name} = ${b.Position:X6}");
                sizes.AppendLine($"!Buffer{b.Resource.Name}Size = ${b.Resource.Length:X4}");
            }
            result.AppendLine($"!NumberOfBuffers = ${buffs.Count():X4}\n");
            result.Append($"{ids}\n{addrs}\n{sizes}\n");
            ids.Clear();
            addrs.Clear();
            sizes.Clear();
            foreach (var dp in dynP)
            {
                ids.AppendLine($"!DynamicPoseID{dp.Resource.Name} = ${dp.Resource.ID:X4}");
                addrs.AppendLine($"!DynamicPose{dp.Resource.Name} = ${SNESROMUtils.PCtoSNES(dp.Position, mapper):X6}");
                sizes.AppendLine($"!DynamicPose{dp.Resource.Name}Size = ${dp.Resource.Length:X4}");
            }
            result.AppendLine($"!NumberOfDynamicPoses = ${dynP.Count():X4}\n");
            result.Append($"{ids}\n{addrs}\n{sizes}\n");
            ids.Clear();
            addrs.Clear();
            sizes.Clear();
            foreach (var p in pals)
            {
                ids.AppendLine($"!PaletteID{p.Resource.Name} = ${p.Resource.ID:X4}");
                addrs.AppendLine($"!Palette{p.Resource.Name} = ${SNESROMUtils.PCtoSNES(p.Position, mapper):X6}");
                sizes.AppendLine($"!Palette{p.Resource.Name}Size = ${p.Resource.Length:X4}");
            }
            result.AppendLine($"!NumberOfPalettes = ${pals.Count():X4}\n");
            result.Append($"{ids}\n{addrs}\n{sizes}\n");
            ids.Clear();
            addrs.Clear();
            sizes.Clear();
            foreach (var r in res)
            {
                ids.AppendLine($"!ResourceID{r.Resource.Name} = ${r.Resource.ID:X4}");
                addrs.AppendLine($"!Resource{r.Resource.Name} = ${SNESROMUtils.PCtoSNES(r.Position, mapper):X6}");
                sizes.AppendLine($"!Resource{r.Resource.Name}Size = ${r.Resource.Length:X4}");
            }
            result.AppendLine($"!NumberOfResources = ${res.Count():X4}\n");
            result.Append($"{ids}\n{addrs}\n{sizes}");
            return result.ToString();
        }
        /// <summary>Builds pose IDs and maximum render-box distances grouped by DrawInfo context.</summary>
        /// <param name="drawInfos">The installed drawable poses.</param>
        /// <returns>The generated pose define source.</returns>
        public static string GeneratePoseDefines(IEnumerable<DrawInfo>? drawInfos)
        {
            if (drawInfos == null)
                return "";
            Dictionary<string, List<DrawInfo>> contextdic = [];
            StringBuilder renderx = new();
            StringBuilder rendery = new();
            StringBuilder result = new();
            result.AppendLine($"!NumberOfPoses = ${drawInfos.Count():X4}\n");
            foreach (var drawInfo in drawInfos)
            {
                if (!contextdic.ContainsKey(drawInfo.ContextName))
                    contextdic.Add(drawInfo.ContextName, []);
                contextdic[drawInfo.ContextName].Add(drawInfo);
                result.AppendLine($"!PoseID{drawInfo.ContextName}_{drawInfo.Name} = ${drawInfo.ID:X4}");
            }
            int rx;
            int ry;
            foreach (var kvp in contextdic)
            {
                rx = DrawInfo.GetMaximumRenderBoxXDistanceOutOfScreen(kvp.Value);
                ry = DrawInfo.GetMaximumRenderBoxYDistanceOutOfScreen(kvp.Value);
                renderx.AppendLine($"!RenderBoxXDistanceOutOfScreen{kvp.Key} = ${rx:X4}");
                rendery.AppendLine($"!RenderBoxYDistanceOutOfScreen{kvp.Key} = ${ry:X4}");
            }
            result.Append($"\n{renderx}\n{rendery}");
            return result.ToString();
        }
        /// <summary>Builds consecutive IDs for non-empty palette effects.</summary>
        /// <param name="paletteEffects">The installed palette-effect collections.</param>
        /// <returns>The generated palette-effect define source.</returns>
        public static string GeneratePaletteEffectDefines(IEnumerable<PaletteEffectCollection>? paletteEffects)
        {
            if (paletteEffects == null)
                return "";
            StringBuilder sb = new();
            int i = 1;
            int j;
            IEnumerable<PaletteEffect> filtered;
            foreach (var effect in paletteEffects)
            {
                filtered = effect.Effects.Where(x => x.EffectType != EffectType.None);
                j = 0;
                foreach (var ef in filtered)
                {
                    sb.AppendLine($"!PaletteEffectID{effect.Name}{j} = ${i:X4}");
                    i++;
                    j++;
                }
            }
            return $"!NumberOfPaletteEffects = ${i:X4}\n\n{sb}";
        }
    }
}
