using DynamicXtremeLibrary;
using DynamicXtremeLibrary.Generators;
using LogRegister;
using DynamicXtremeLibrary;
using DynamicXtremeLibrary.Generators;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.ResourceManagement;
using SNESLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Readers;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.PaletteEffec;

namespace DynamicXtremeLibrary
{
    public class DynamicXtreme
    {
        public const int VECTOR_ADDRESS = 0x008220;
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
        public required string DataDirectory { get; init; }
        public required string TMPDirectory { get; init; }
        public required string BufferDataFilename {  get; init; }
        public required string DynamicPoseDataFilename { get; init; }
        public required string PaletteDataFilename { get; init; }
        public required string PoseDataFilename { get; init; }
        public required string PaletteEffectsDataFilename { get; init; }
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
            File.WriteAllBytes(Path.Combine(TMPDirectory, "tmp.smc"), rom);
            
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
            }
            if (!validation)
                return false;
            if (boolOpts["PalettesEffects"])
            {
                var palEffects = PaletteEffectExtension.GetCollections(PaletteEffectsDirectory);
                PaletteEffectExtension.ToFile(palEffects, 
                    Path.Combine(PatchDirectory, DataDirectory,$"{PaletteEffectsDataFilename}.asm"));
            }
            File.WriteAllBytes(Path.Combine(TMPDirectory, "tmp.smc"), rom);
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
    }
}
