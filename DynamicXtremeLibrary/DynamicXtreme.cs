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
        public const int DX_MARK = 0x008241;
        public const int VECTOR_ADDRESS = 0x00821F;
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
                processDynamicInfos(rom, ri, out validation, out dynamicInfos, out resources, out buffers, out allRefs);
            if (!validation)
                return false;

            IReadOnlyList<DrawInfo>? drawInfos = null;
            IEnumerable<GraphicRoutine>? allGrs = null;
            if (boolOpts["DrawingSystem"])
                processDrawInfos(ri, out validation, out drawInfos, out allGrs);
            if (!validation)
                return false;

            IReadOnlyList<PaletteEffectCollection>? palEffects = null;
            if (boolOpts["PalettesEffects"])
                palEffects = processPaletteEffects();

            File.WriteAllBytes(tmpROM, rom);

            string extDefinesDir = Path.Combine(PatchDirectory, ExtraDefinesDirectory);

            if (drawInfos == null || drawInfos.Count == 0)
                opt.BoolOptions.First(bo => bo.Name == "DrawingSystem").Value = false;
            if (dynamicInfos == null || dynamicInfos.Count == 0)
                opt.BoolOptions.First(bo => bo.Name == "DynamicPoses").Value = false;
            if (palEffects == null || palEffects.Count == 0) 
                opt.BoolOptions.First(bo => bo.Name == "PalettesEffects").Value = false;

            string optDefines = opt.GetOptionsDefines();
            File.WriteAllText(Path.Combine(extDefinesDir, $"{OptionsDefinesFilename}.asm"),
                optDefines);
            File.WriteAllText(Path.Combine(ExtraDefinesDirectory, $"{OptionsDefinesFilename}.asm"),
                optDefines);

            DefineGenerator dg = new(
                Path.Combine(TemplateASMDirectory, $"{InputDefinesFilename}.asm"),
                Path.Combine(extDefinesDir, $"{OutputDefinesFilename}.asm"),
                Path.Combine(ExtraDefinesDirectory, $"{OutputDefinesFilename}.asm"));
            dg.GenerateDefinesFile(allRefs, SNESROMUtils.GetMapper(rom), drawInfos, dynamicInfos, palEffects);

            Task<(bool, string)> asarTask = AsarPatch.Run();
            asarTask.Wait();
            (bool success, string asarLog) = asarTask.Result;
            if (!success)
            {
                Console.WriteLine(asarLog);
                return false;
            }
            string[] patchResult = [.. asarLog.Replace("\r\n","\n").Split('\n')
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

            File.Replace(tmpROM, opt.OutputRomPath.Value, $"{opt.OutputRomPath.Value}.bak");
            return true;
        }
        private IReadOnlyList<PaletteEffectCollection> processPaletteEffects()
        {
            IReadOnlyList<PaletteEffectCollection>? palEffects = PaletteEffectExtension.GetCollections(PaletteEffectsDirectory);
            PaletteEffectExtension.ToFile(_log, palEffects,
                                Path.Combine(PatchDirectory, DataDirectory, $"{PaletteEffectsDataFilename}.asm"));
            return palEffects;
        }
        private void processDynamicInfos(byte[] rom, ReadInfo ri, out bool validation, out IReadOnlyList<DynamicInfo>? dynamicInfos, out IReadOnlyList<Resource>? resources, out IReadOnlyList<ResourceBuffer>? buffers, out ResourceAllReferences? allRefs)
        {
            dynamicInfos = ri.GetAllDynamicInfos(DynamicInfoDirectory, DynamicResourcesDirectory, out validation);
            string dataDir = Path.Combine(PatchDirectory, DataDirectory);
            if(dynamicInfos == null)
            {
                resources = null;
                buffers = null;
                allRefs = null;
                File.WriteAllText(Path.Combine(dataDir, $"{BufferDataFilename}.asm"),
                    "BufferTable:\n\tdl $FFFFFF\n");
                File.WriteAllText(Path.Combine(dataDir, $"{PaletteDataFilename}.asm"),
                    "PaletteTable:\n.IDs\n.Addresses\n");
                return;
            }
            resources = DynamicInfo.GetAllResources(dynamicInfos);
            buffers = ResourceManager.MergeResources(resources);
            allRefs = ResourceManager.InsertBuffers(_log, rom, buffers);
            string bufferTable = ResourceManager.BuildBufferTable(allRefs.Buffers);
            string dynamicPoseData = DynamicPoseDataGenerator.GenerateData(
                SNESROMUtils.GetMapper(rom), allRefs.DynamicPoses, dynamicInfos);
            string paletteTable = ResourceManager.BuildPalettesTable(dynamicInfos);

            File.WriteAllText(Path.Combine(dataDir, $"{BufferDataFilename}.asm"),
                bufferTable);
            File.WriteAllText(Path.Combine(dataDir, $"{DynamicPoseDataFilename}.asm"),
                dynamicPoseData);
            File.WriteAllText(Path.Combine(dataDir, $"{PaletteDataFilename}.asm"),
                paletteTable);
        }

        private void processDrawInfos(ReadInfo ri, out bool validation, out IReadOnlyList<DrawInfo>? drawInfos, out IEnumerable<GraphicRoutine>? allGrs)
        {
            drawInfos = ri.GetAllDrawInfos(DrawInfoDirectory, out validation);
            if (drawInfos.Count == 0)
            {
                drawInfos = null;
                allGrs = null;
                return;
            }
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

        public void RemoveResources(byte[] rom)
        {
            Mapper mapper = SNESROMUtils.GetMapper(rom);
            int markAddr = SNESROMUtils.SNEStoPC(DX_MARK, mapper);
            int markValue = (rom[markAddr + 1] << 8) | rom[markAddr];
            if (markValue != 0x5844)
                return;
            int PCAddr = SNESROMUtils.SNEStoPC(VECTOR_ADDRESS, mapper);
            int vectorAddr = SNESROMUtils.JoinAddress(rom, PCAddr);
            vectorAddr = SNESROMUtils.SNEStoPC(vectorAddr, mapper);
            if (vectorAddr == -1)
                return;
            int resAddr = SNESROMUtils.JoinAddress(rom, vectorAddr);
            resAddr = SNESROMUtils.SNEStoPC(resAddr, mapper);
            if (resAddr == -1)
                return;
            var resources = SNESROMUtils.Remove(rom, resAddr, mapper);
            if (resources == null || resources.Count == 0)
                return;
            _log.Add(new Title("Removing Resources"));
            foreach (var res in resources)
            {
                _log.Add(new RemovedBufferAt(res.Item1, res.Item2));
            }
        }
        private void printSummary(string[] patchResult,
            ResourceAllReferences? allRefs,
            IEnumerable<DrawInfo>? dis,
            IEnumerable<DynamicInfo>? dyns,
            IEnumerable<GraphicRoutine>? grs,
            IEnumerable<PaletteEffectCollection>? pecs)
        {
            long grSizes = 0;
            if (dis != null && dis.Any())
            {
                for (int i = 1; i < patchResult.Length - 1; i += 2)
                {
                    grSizes += int.Parse(patchResult[i]);
                }
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
            if (allgrs == null || !allgrs.Any())
                return;

            _log.Add(new Title("Graphic Routines"));
            var grArr = allgrs.ToArray();
            Resource grRef;
            for (int i = 0; i < grArr.Length; i++)
            {
                grRef = new Resource(grArr[i].ID, grArr[i].Name, ResourceType.GraphicRoutine,
                    new byte[int.Parse(patchResult[(i * 2) + 1])]);
                _log.Add(new ResourceInsertedAt(new(0, int.Parse(patchResult[i * 2]), grRef)));
            }
        }
    }
}
