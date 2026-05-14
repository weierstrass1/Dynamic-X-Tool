using DynamicXtremeLibrary.Generators;
using LogRegister;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.ResourceManagement;
using SNESLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Readers;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremeLibrary.Asar;
using DynamicXtremeLibrary.Defines;
using DynamicXtremePaletteCreatorLibrary;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.ExternalTools;

namespace DynamicXtremeLibrary
{
    public class DynamicXtreme
    {
        public const int VECTOR_ADDRESS = 0x00821F;
        public const int RESOURCE_TABLE_ADDRESS = VECTOR_ADDRESS;
        public const int PALETTE_TABLE = VECTOR_ADDRESS + 3;
        public const int PALETTE_EFFECT_TABLE = VECTOR_ADDRESS + 6;
        private LogRegisterSystem _log;
        public required string DynamicInfoDirectory { get; init; }
        public required string DrawInfoDirectory { get; init; }
        public required string DynamicResourcesDirectory { get; init; }
        public required string PaletteEffectsDirectory { get; init; }
        public required string TemplateASMDirectory { get; init; }
        public required string PatchDirectory { get; init; }
        public required string ExtraDefinesDirectory { get; init; }
        public required string GraphicRoutinesDirectory { get; init; }
        public required string TMPDirectory { get; init; }
        public required string DataDirectory { get; init; }
        public required string RoutinesDirectory { get; init; }
        public required string OptionsDefinesFilename { get; init; }
        public required string BufferDataFilename {  get; init; }
        public required string DynamicPoseDataFilename { get; init; }
        public required string PaletteDataFilename { get; init; }
        public required string PoseDataFilename { get; init; }
        public required string PaletteEffectsDataFilename { get; init; }
        public required string PoseDataTemplateFilename { get; init; }
        public required string GraphicRoutineTemplateFilename { get; init; }
        public required string GraphicRoutineIncludeTemplateFilename { get; init; }
        public required string GraphicRoutineIncludeFilename { get; init; }
        public required string GraphicRoutineProtsFilename { get; init; }
        public required string InputDefinesFilename { get; init; }
        public required string OutputDefinesFilename { get; init; }
        public DynamicXtreme(LogRegisterSystem log)
        {
            _log = log;
        }
        public bool Run(byte[] rom)
        {
            if (Directory.Exists(TMPDirectory))
                Directory.Delete(TMPDirectory, true);
            Directory.CreateDirectory(TMPDirectory);

            string tmpROM = Path.Combine(TMPDirectory, "tmp.smc");

            File.WriteAllBytes(tmpROM, rom);

            RemoveResources(rom);
            Options opt = Options.Instance;
            ReadInfo ri = new(_log);
            var boolOpts = opt.BoolOptions.ToDictionary(o => o.Name, o => o.Value);
            bool validation = true;
            IReadOnlyList<DynamicInfo>? dynamicInfos = null;
            IReadOnlyList<Resource>? resources = null;
            IReadOnlyList<ResourceBuffer>? buffers = null;
            ResourceAllReferences? allRefs = null;
            if (boolOpts["GraphicsChange"] || boolOpts["PalettesChange"])
            {
                dynamicInfos = ri.GetAllDynamicInfos(DynamicInfoDirectory, DynamicResourcesDirectory, out validation);
                resources = DynamicInfo.GetAllResources(dynamicInfos);
                buffers = ResourceManager.MergeResources(resources);
                allRefs = ResourceManager.InsertBuffers(_log, rom, buffers);
                string bufferTable = ResourceManager.BuildBufferTable(allRefs.Buffers);
                string dynamicPoseData = DynamicPoseDataGenerator.GenerateData(
                    SNESROMUtils.GetMapper(rom), allRefs.DynamicPoses, dynamicInfos);
                string paletteTable = ResourceManager.BuildPalettesTable(allRefs.Palettes);
                string dataDir = Path.Combine(PatchDirectory, DataDirectory);
                File.WriteAllText(Path.Combine(dataDir, $"{BufferDataFilename}.asm"),
                    bufferTable);
                File.WriteAllText(Path.Combine(dataDir, $"{DynamicPoseDataFilename}.asm"),
                    dynamicPoseData);
                File.WriteAllText(Path.Combine(dataDir, $"{PaletteDataFilename}.asm"),
                    paletteTable);
            }
            if (!validation)
                return false;
            IReadOnlyList<DrawInfo>? drawInfos = null;
            IEnumerable<GraphicRoutine>? allGrs = null;
            if (boolOpts["DrawingSystem"])
            {
                drawInfos = ri.GetAllDrawInfos(".\\DrawInfo", out validation);
                var graphicRoutines = GraphicRoutine.GetGraphicRoutines(drawInfos);
                GraphicRoutineGenerator grg = new(
                    Path.Combine(TemplateASMDirectory, $"{GraphicRoutineIncludeTemplateFilename}.asm"),
                    Path.Combine(TemplateASMDirectory, $"{GraphicRoutineTemplateFilename}.asm"),
                    Path.Combine(PatchDirectory, GraphicRoutinesDirectory),
                    $"{GraphicRoutineIncludeFilename}.asm",
                    $"{GraphicRoutineProtsFilename}.asm");
                grg.GenerateAllGraphicRoutine(graphicRoutines);

                allGrs = graphicRoutines
                    .SelectMany(kvp => kvp.Value);

                string poseData = PoseDataGenerator.GenerateData(allGrs, Path.Combine(TemplateASMDirectory, PoseDataTemplateFilename));
                File.WriteAllText(Path.Combine(PatchDirectory, DataDirectory, $"{PoseDataFilename}.asm"), poseData);
            }
            if (!validation)
                return false;
            IReadOnlyList<PaletteEffectCollection>? palEffects = null;
            if (boolOpts["PalettesEffects"])
            {
                palEffects = PaletteEffectExtension.GetCollections(PaletteEffectsDirectory);
                PaletteEffectExtension.ToFile(palEffects,
                    Path.Combine(PatchDirectory, DataDirectory, $"{PaletteEffectsDataFilename}.asm"));
            }
            File.WriteAllBytes(tmpROM, rom);

            string extDefinesDir = Path.Combine(PatchDirectory, ExtraDefinesDirectory);

            string optDefines = opt.GetOptionsDefines();
            File.WriteAllText(Path.Combine(extDefinesDir, $"{OptionsDefinesFilename}.asm"),
                optDefines);

            DefineGenerator dg = new(
                Path.Combine(TemplateASMDirectory, $"{InputDefinesFilename}.asm"),
                Path.Combine(extDefinesDir, $"{OutputDefinesFilename}.asm"),
                Path.Combine(ExtraDefinesDirectory, $"{OutputDefinesFilename}.asm"));
            dg.GenerateDefinesFile(allRefs, drawInfos, palEffects);

            string[] patchResult = [.. AsarPatch.Run().Replace("\r\n","\n").Split('\n')
                .Where(s => !string.IsNullOrWhiteSpace(s))];

            if (boolOpts["DrawingSystem"])
                printGraphicRoutineLog(patchResult, allGrs);

            printSummary(patchResult, allRefs, drawInfos, dynamicInfos, allGrs, palEffects);

            ExternalToolsLinker linker = new(TemplateASMDirectory,
                ExtraDefinesDirectory,
                RoutinesDirectory);
            linker.PixiLinker();
            linker.GPSLinker();
            linker.UberASMToolLinker();

            File.Replace(tmpROM, opt.OutputRomPath.Value, $"Backup_{opt.OutputRomPath.Value}");
            return true;
        }
        public void RemoveResources(byte[] rom)
        {
            
            Mapper mapper = SNESROMUtils.GetMapper(rom);
            int PCAddr = SNESROMUtils.SNEStoPC(VECTOR_ADDRESS, mapper);
            int resAddr = SNESROMUtils.JoinAddress(rom, PCAddr);
            resAddr = SNESROMUtils.SNEStoPC(resAddr, mapper);
            if (resAddr == -1)
                return;
            var removed = SNESROMUtils.Remove(rom, resAddr, mapper);
        }
        private void printSummary(string[] patchResult,
            ResourceAllReferences? allRefs,
            IEnumerable<DrawInfo>? dis,
            IEnumerable<DynamicInfo>? dyns,
            IEnumerable<GraphicRoutine>? grs,
             IEnumerable<PaletteEffectCollection>? pecs)
        {
            long grSizes = 0;
            for (int i = 1; i < patchResult.Length - 1; i += 2) 
            {
                grSizes += int.Parse(patchResult[i]);
            }
            _log.Add(new Title("Summary"));
            _log.Add(new NumberOf("Drawable Poses", dis == null ? 0 : dis.Count()));
            _log.Add(new NumberOf("Graphic Routines", grs == null ? 0 : grs.Count(), grSizes));
            _log.Add(new NumberOf("Dynamic Poses", 
                dyns == null ? 0 : dyns.Sum(dyn => dyn.PoseLength),
                allRefs == null ? 0 : allRefs.DynamicPosesSize));
            _log.Add(new NumberOf("Palettes", 
                dyns == null ? 0 : dyns.Sum(dyn => dyn.PaletteLength),
                allRefs == null ? 0 : allRefs.PaletteSize));
            _log.Add(new NumberOf("Resources", 
                dyns == null ? 0 : dyns.Sum(dyn => dyn.ResourcesLength),
                allRefs == null ? 0 : allRefs.GeneralResourceSize));
            int peL = pecs == null ? 0 : pecs.Sum(pec => pec.Effects.Count);
            _log.Add(new NumberOf("Palette Effects", peL, peL * 7));

            long buffSize = allRefs == null ? 0 : allRefs.BufferSize;
            _log.Add(new DataUsage("Dynamic Resources", buffSize));
            _log.Add(new DataUsage("Dynamic Xtreme Patch", long.Parse(patchResult[^1])));
            _log.Add(new DataUsage("All", buffSize + long.Parse(patchResult[^1])));
        }
        private void printGraphicRoutineLog(string[] patchResult, IEnumerable<GraphicRoutine>? allgrs)
        {
            if (allgrs == null || allgrs.Count() == 0)
                return;

            _log.Add(new Title("Graphic Routines"));
            var grArr = allgrs.ToArray();
            Resource grRef;
            for (int i = 0; i < grArr.Length; i++)
            {
                grRef = new Resource(grArr[i].ID, grArr[i].Name, ResourceType.Buffer,
                    new byte[int.Parse(patchResult[(i * 2) + 1])]);
                _log.Add(new ResourceInsertedAt(new(0, int.Parse(patchResult[i * 2]), grRef)));
            }
        }
    }
}
