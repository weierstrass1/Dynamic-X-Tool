using DynamicXtremeLibrary.Generators;
using LogRegister;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.ResourceManagement;
using SNESLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremeLibrary.Asar;
using DynamicXtremeLibrary.Defines;
using DynamicXtremePaletteCreatorLibrary;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.ExternalTools;
using DynamicXtremeLibrary.Inputs;

namespace DynamicXtremeLibrary
{
    /// <summary>Orchestrates input resolution, resource generation, patching, and transactional installation.</summary>
    public class DynamicXtreme
    {
        /// <summary>Gets the SNES address containing the Dynamic Xtreme installation marker.</summary>
        public const int DX_MARK = 0x008241;
        /// <summary>Gets the SNES address of the Dynamic Xtreme resource-table vector.</summary>
        public const int VECTOR_ADDRESS = 0x00821F;
        private LogRegisterSystem _log;
        /// <summary>Gets the traditional DynamicInfo directory.</summary>
        public required string DynamicInfoDirectory { get; init; }
        /// <summary>Gets the traditional DrawInfo directory.</summary>
        public required string DrawInfoDirectory { get; init; }
        /// <summary>Gets the resource base for traditional DynamicInfo and palette-effect files.</summary>
        public required string DynamicResourcesDirectory { get; init; }
        /// <summary>Gets the traditional palette-effect directory.</summary>
        public required string PaletteEffectsDirectory { get; init; }
        /// <summary>Gets the directory containing ASM templates and exported defines.</summary>
        public required string TemplateASMDirectory { get; init; }
        /// <summary>Gets the root directory of the Asar patch.</summary>
        public required string PatchDirectory { get; init; }
        /// <summary>Gets the extra-defines directory name or path.</summary>
        public required string ExtraDefinesDirectory { get; init; }
        /// <summary>Gets the generated graphic-routines directory.</summary>
        public required string GraphicRoutinesDirectory { get; init; }
        /// <summary>Gets the temporary working directory.</summary>
        public required string TMPDirectory { get; init; }
        /// <summary>Gets the patch data directory name.</summary>
        public required string DataDirectory { get; init; }
        /// <summary>Gets the shared routines directory exported to external tools.</summary>
        public required string RoutinesDirectory { get; init; }
        /// <summary>Gets the generated options-defines filename without extension.</summary>
        public required string OptionsDefinesFilename { get; init; }
        /// <summary>Gets the generated buffer-table filename without extension.</summary>
        public required string BufferDataFilename {  get; init; }
        /// <summary>Gets the generated dynamic-pose data filename without extension.</summary>
        public required string DynamicPoseDataFilename { get; init; }
        /// <summary>Gets the generated palette-table filename without extension.</summary>
        public required string PaletteDataFilename { get; init; }
        /// <summary>Gets the generated drawing-pose data filename without extension.</summary>
        public required string PoseDataFilename { get; init; }
        /// <summary>Gets the generated palette-effect data filename without extension.</summary>
        public required string PaletteEffectsDataFilename { get; init; }
        /// <summary>Gets the pose-data template filename.</summary>
        public required string PoseDataTemplateFilename { get; init; }
        /// <summary>Gets the individual graphic-routine template filename.</summary>
        public required string GraphicRoutineTemplateFilename { get; init; }
        /// <summary>Gets the aggregate graphic-routine include-template filename.</summary>
        public required string GraphicRoutineIncludeTemplateFilename { get; init; }
        /// <summary>Gets the generated aggregate routine-include filename.</summary>
        public required string GraphicRoutineIncludeFilename { get; init; }
        /// <summary>Gets the generated routine-protection filename.</summary>
        public required string GraphicRoutineProtsFilename { get; init; }
        /// <summary>Gets the base input-defines filename.</summary>
        public required string InputDefinesFilename { get; init; }
        /// <summary>Gets the generated output-defines filename.</summary>
        public required string OutputDefinesFilename { get; init; }
        /// <summary>
        /// Gets the callback used to approve or reject automatic name changes when distinct
        /// external inputs generate colliding Asar symbols.
        /// </summary>
        public Func<NameCollision, bool>? CollisionResolver { get; init; }
        /// <summary>Gets the callback that approves skipping PIXI entries that cannot be inspected.</summary>
        public Func<PixiEntryIssue, bool>? IgnorePixiEntry { get; init; }
        /// <summary>Initializes the installer with its structured log.</summary>
        public DynamicXtreme(LogRegisterSystem log)
        {
            _log = log;
        }

        /// <summary>
        /// Resolves all enabled inputs, generates and applies the Dynamic Xtreme patch, and commits
        /// the output ROM together with any required external-tool updates.
        /// </summary>
        /// <param name="rom">The input ROM bytes to process in memory.</param>
        /// <returns><see langword="true"/> when the complete installation is committed.</returns>
        public bool Run(byte[] rom)
        {
            if (Directory.Exists(TMPDirectory))
                Directory.Delete(TMPDirectory, true);
            Directory.CreateDirectory(TMPDirectory);

            string tmpROM = Path.Combine(TMPDirectory, "tmp.smc");

            File.WriteAllBytes(tmpROM, rom);

            Options opt = Options.Instance;
            var boolOpts = opt.BoolOptions.ToDictionary(o => o.Name, o => o.Value);

            bool validation = true;

            InputSources inputSources = new(
                DynamicInfoDirectory, DrawInfoDirectory, PaletteEffectsDirectory,
                DynamicResourcesDirectory, opt.PixiListPath.Value, opt.PixiPath.Value,
                opt.UberasmListPath.Value, opt.UberasmPath.Value);
            EnabledInputs enabledInputs = new(
                boolOpts["GraphicsChange"] || boolOpts["PalettesChange"],
                boolOpts["DrawingSystem"], boolOpts["PalettesEffects"]);
            InputPreparation inputPreparation = new(_log, TMPDirectory, CollisionResolver, IgnorePixiEntry);
            ResolvedInputs? resolvedInputs = inputPreparation.Prepare(inputSources, enabledInputs);
            if (resolvedInputs == null)
                return false;

            IReadOnlyList<ResolvedPaletteEffectCollection>? resolvedPaletteEffects =
                boolOpts["PalettesEffects"] ? resolvedInputs.PaletteEffects : null;
            PaletteEffectResourceSet paletteEffectResources =
                PaletteEffectExtension.CreateResources(resolvedPaletteEffects ?? []);

            RemoveResources(rom);

            IReadOnlyList<DynamicInfo>? dynamicInfos = boolOpts["GraphicsChange"] ||
                boolOpts["PalettesChange"] ? resolvedInputs.DynamicInfos : null;
            IReadOnlyList<Resource>? resources = null;
            IReadOnlyList<ResourceBuffer>? buffers = null;
            ResourceAllReferences? allRefs = null;
            if (dynamicInfos != null || resolvedPaletteEffects != null)
                processResources(rom, dynamicInfos, paletteEffectResources.Resources,
                    out validation, out resources, out buffers, out allRefs);
            if (!validation)
                return false;

            IReadOnlyList<DrawInfo>? drawInfos = null;
            IEnumerable<GraphicRoutine>? allGrs = null;
            if (boolOpts["DrawingSystem"])
                processDrawInfos(resolvedInputs.DrawInfos, out validation, out drawInfos, out allGrs);
            if (!validation)
                return false;

            IReadOnlyList<PaletteEffectCollection>? palEffects = null;
            if (resolvedPaletteEffects != null)
                palEffects = processPaletteEffects(resolvedPaletteEffects,
                    paletteEffectResources, out validation);
            if (!validation)
                return false;

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

            string outputROM = opt.OutputRomPath.Value;
            ExternalToolsLinker linker = new(TemplateASMDirectory, ExtraDefinesDirectory,
                RoutinesDirectory);
            InstallationCommitter committer = new(_log, linker);
            if (!committer.TryCommit(resolvedInputs.SourceRewrites, tmpROM, outputROM))
                return false;

            foreach (var change in resolvedInputs.NameChanges)
                _log.Add(new GeneratedNameChanged(change.OldName, change.NewName, change.Files));

            return true;
        }
        /// <summary>
        /// Serializes already resolved palette-effect collections into the patch data file.
        /// </summary>
        /// <param name="palEffects">The validated palette-effect collections to serialize.</param>
        /// <param name="resources">The converted target-palette resources bound to the effects.</param>
        /// <param name="validation">Whether palette-effect data generation succeeded.</param>
        /// <returns>The same collections supplied to the method.</returns>
        private IReadOnlyList<PaletteEffectCollection> processPaletteEffects(
            IReadOnlyList<ResolvedPaletteEffectCollection> palEffects,
            PaletteEffectResourceSet resources, out bool validation)
        {
            validation = true;
            PaletteEffectExtension.ToFile(_log, palEffects, resources,
                                Path.Combine(PatchDirectory, DataDirectory, $"{PaletteEffectsDataFilename}.asm"));
            return palEffects.Select(effect => effect.Collection).ToArray();
        }
        /// <summary>
        /// Merges DynamicInfo and additional resources, inserts their buffers into the ROM, and
        /// generates the buffer, pose, and palette tables consumed by the patch.
        /// </summary>
        /// <param name="rom">The in-memory ROM being prepared for Asar.</param>
        /// <param name="dynamicInfos">The enabled validated DynamicInfo models, if any.</param>
        /// <param name="additionalResources">Resources owned by enabled non-DynamicInfo features.</param>
        /// <param name="validation">Whether resource insertion and table generation succeeded.</param>
        /// <param name="resources">All resources passed to the buffer packer.</param>
        /// <param name="buffers">The merged resource buffers.</param>
        /// <param name="allRefs">The ROM references assigned to inserted buffers and resources.</param>
        private void processResources(byte[] rom,
            IReadOnlyList<DynamicInfo>? dynamicInfos,
            IReadOnlyList<Resource> additionalResources,
            out bool validation, out IReadOnlyList<Resource>? resources,
            out IReadOnlyList<ResourceBuffer>? buffers, out ResourceAllReferences? allRefs)
        {
            validation = true;
            string dataDir = Path.Combine(PatchDirectory, DataDirectory);
            IEnumerable<Resource> dynamicResources = dynamicInfos == null
                ? []
                : DynamicInfo.GetAllResources(dynamicInfos);
            resources =
            [
                .. dynamicResources,
                .. additionalResources
            ];
            buffers = ResourceManager.MergeResources(resources);
            allRefs = ResourceManager.InsertBuffers(_log, rom, buffers);
            if (allRefs == null)
            {
                validation = false;
                return;
            }
            string bufferTable = ResourceManager.BuildBufferTable(allRefs.Buffers);
            string dynamicPoseData = dynamicInfos == null || dynamicInfos.Count == 0
                ? DynamicPoseDataGenerator.GenerateEmpty()
                : DynamicPoseDataGenerator.GenerateData(
                    SNESROMUtils.GetMapper(rom), allRefs.DynamicPoses, dynamicInfos);
            string paletteTable = ResourceManager.BuildPalettesTable(dynamicInfos ?? []);

            File.WriteAllText(Path.Combine(dataDir, $"{BufferDataFilename}.asm"),
                bufferTable);
            File.WriteAllText(Path.Combine(dataDir, $"{DynamicPoseDataFilename}.asm"),
                dynamicPoseData);
            File.WriteAllText(Path.Combine(dataDir, $"{PaletteDataFilename}.asm"),
                paletteTable);
        }

        /// <summary>
        /// Generates the unique graphic routines and pose data required by the resolved DrawInfo
        /// poses.
        /// </summary>
        /// <param name="resolvedDrawInfos">The validated DrawInfo poses with final IDs and names.</param>
        /// <param name="validation">Whether the graphic-routine limit and generation succeeded.</param>
        /// <param name="drawInfos">The DrawInfo poses passed to downstream generators.</param>
        /// <param name="allGrs">The generated graphic routines in output order.</param>
        private void processDrawInfos(IReadOnlyList<DrawInfo> resolvedDrawInfos,
            out bool validation, out IReadOnlyList<DrawInfo>? drawInfos,
            out IEnumerable<GraphicRoutine>? allGrs)
        {
            validation = true;
            drawInfos = resolvedDrawInfos;
            if (drawInfos.Count == 0 || !validation)
            {
                drawInfos = null;
                allGrs = null;
                return;
            }
            var graphicRoutines = GraphicRoutine.GetGraphicRoutines(drawInfos);
            GraphicRoutine[] generatedRoutines = [.. graphicRoutines
                .SelectMany(kvp => kvp.Value)];
            if (generatedRoutines.Length > 256)
            {
                _log.Add(new ValueExceedsLimit("Drawing system", "Number of graphic routines",
                    generatedRoutines.Length, 256));
                validation = false;
                allGrs = null;
                return;
            }
            GraphicRoutineGenerator grg = new(
                Path.Combine(TemplateASMDirectory, $"{GraphicRoutineIncludeTemplateFilename}.asm"),
                Path.Combine(TemplateASMDirectory, $"{GraphicRoutineTemplateFilename}.asm"),
                Path.Combine(PatchDirectory, GraphicRoutinesDirectory),
                $"{GraphicRoutineIncludeFilename}.asm",
                $"{GraphicRoutineProtsFilename}.asm");
            grg.GenerateAllGraphicRoutine(graphicRoutines);

            allGrs = generatedRoutines;

            string poseData = PoseDataGenerator.GenerateData(allGrs, Path.Combine(TemplateASMDirectory, PoseDataTemplateFilename));
            File.WriteAllText(Path.Combine(PatchDirectory, DataDirectory, $"{PoseDataFilename}.asm"), poseData);
        }

        /// <summary>Removes resources referenced by a previously installed Dynamic Xtreme vector.</summary>
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
        /// <summary>Records installed item counts and aggregate byte usage after Asar succeeds.</summary>
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
            _log.Add(new Title("Summary")
            {
                AppearWithoutVerbose = true
            });
            _log.Add(new NumberOf("Drawable Poses", dis == null ? 0 : dis.Count()));
            _log.Add(new NumberOf("Graphic Routines", grs == null ? 0 : grs.Count(), grSizes));
            _log.Add(new NumberOf("Dynamic Poses", 
                dyns == null ? 0 : dyns.Sum(dyn => dyn.PoseLength),
                allRefs == null ? 0 : allRefs.DynamicPosesSize));
            _log.Add(new NumberOf("Palettes", 
                dyns == null ? 0 : dyns.Sum(dyn => dyn.PaletteLength),
                allRefs == null ? 0 : allRefs.PaletteSize));
            _log.Add(new NumberOf("Palette Effect Palettes",
                allRefs == null ? 0 : allRefs.PaletteEffectPalettes.Count,
                allRefs == null ? 0 : allRefs.PaletteEffectPaletteSize));
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
        /// <summary>Records each generated graphic routine using addresses and sizes printed by Asar.</summary>
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
