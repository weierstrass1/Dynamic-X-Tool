using AsarCLR;
using Dynamic_X_Patch;
using DynamicXSNES;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicXLibrary
{
    public class ResourceReference
    {
        public int ID { get; private set; }
        public int BufferID { get; private set; }
        public int Position { get; set; }
        public ResourceReference(int id, int bufferID, int position)
        {
            ID = id;
            BufferID = bufferID;
            Position = position;
        }
        public override string ToString()
            => $"({ID}, {BufferID}, {Position})";
    }
    public class DynamicXProcess
    {
        private Mapper mapper;
        private byte[] rom;
        private readonly List<FrameInfo> frameInfos = new();
        private readonly List<DynamicInfo> dynamicInfos = new();
        private List<ResourceReference>? posesReferences;
        private List<ResourceReference>? paletteReferences;
        private List<ResourceReference>? resourceReferences;
        private List<ResourceReference>? bufferReferences;
        private List<string>? palEffects;
        private List<string>? poses;
        private List<int>? posesSize;
        private List<string>? palettes;
        private List<int>? palettesSize;
        private List<string>? resources;
        private List<int>? resourcesSize;
        private List<byte[]>? buffers;
        private List<int>? graphicRoutinesPosition;
        private List<int>? graphicRoutinesSize;
        private int dynamicRoutinesSize;
        private int dynamicRoutinesPosition;
        private int drawRoutineSize;
        private int drawRoutinePosition;
        private int dynamicXSize;
        private int palEffectSize;
        private int paletteTablesSize;
        public DynamicXProcess(string romPath)
        {
            if (!File.Exists(romPath))
                throw new FileNotFoundException(nameof(romPath));
            rom = File.ReadAllBytes(romPath);
            mapper = SNESROMUtils.GetMapper(rom);
        }
        public bool ReadROM(string rompath)
        {
            if (!File.Exists(rompath))
                return false;
            rom = File.ReadAllBytes(rompath);
            mapper = SNESROMUtils.GetMapper(rom);
            return true;
        }
        public void SaveROM(string output)
        {
            if (rom == null)
                return;
            if(File.Exists(output))
                File.Delete(output);
            File.WriteAllBytes(output, rom);
        }
        public string? Process(string romPath)
            => Process(romPath, romPath);
        public string? Process(string romPath, string output)
        {
            if (!ReadROM(romPath))
                return DynamicXErrors.ROMNotFound;
            if (!Directory.Exists(Path.GetDirectoryName(output)))
                return DynamicXErrors.OutputDirectoryNotFound;
            string? log = Process();
            if (log != null)
            {
                Log.WriteLine(log);
                Log.WriteLine("Installation failed");
                return null;
            }
            SaveROM(output);
            return log;
        }
        public string? Process()
        {
            if (!Directory.Exists("TMP"))
            {
                Directory.CreateDirectory("TMP");
                File.Copy(Options.Instance.InputROMPath!, Path.Combine("TMP", "tmp.smc"));
            }
            Remove();
            PatchDynamicX();
            
            string log = GetDynamicInfo(out bool validation);
            if (!validation)
                return log;
            log = GetDrawInfos(out validation);
            if (!validation)
                return log;
            if (Options.Instance.DynamicPoses)
            {
                GenerateBuffers();
                DynamicXErrors? err = InsertBuffers();
                if (err != null)
                    return err;
                SetUpBufferAddresses();
                GenerateDynamicPoseData();
                InsertPalettesTables();
            }
            if (Options.Instance.DrawingSystem)
            {
                GenerateGraphicRoutines();
                SetUpGraphicRoutinesAddresses();
                GeneratedPoseData();
            }
            if (Options.Instance.PaletteEffects)
                InsertPaletteEffects();
            GenerateDefines();

            PixiInstallation();
            UberasmToolInstallation();
            GPSInstallation();

            PrintSummary();
            if (File.Exists("log.txt"))
                File.Delete("log.txt");
            File.WriteAllText("log.txt", Log.GetLog());
            return null;
        }
        private void generateOptionsFile(Options opt)
        {
            string tmpFile = Path.Combine("TMP", "Options.asm");
            if (File.Exists(tmpFile))
                File.Delete(tmpFile);
            File.WriteAllText(tmpFile, opt.GenerateOptionsFileContent());
            string extraDefines = Path.Combine("DynamicX", "ExtraDefines", "Options.asm");
            if(File.Exists(extraDefines))
                File.Delete(extraDefines);
            File.Copy(tmpFile, extraDefines);
            string rom = Path.Combine(Path.GetDirectoryName(opt.OutputROMPath)!, "Options.asm");
            if (File.Exists(rom))
                File.Delete(rom);
            File.Copy(tmpFile, rom);
        }
        private bool toolInstallation(string? path, string folderName, params string[] relativeFolder)
        {
            if (path == null || path == "")
                return false;
            foreach(var d in relativeFolder)
            {
                if (Directory.Exists(Path.Combine(path, d)))
                    continue;
                Log.WriteLine($"{folderName} Folder is not valid, {folderName} Installation Failed.");
                return false;
            }
            return true;
        }
        private void copyDirectoryFiles(string src, string dst)
        {
            string fpath;
            foreach (var p in Directory.GetFiles(src))
            {
                fpath = Path.Combine(dst, Path.GetFileName(p));
                if (File.Exists(fpath))
                    File.Delete(fpath);
                File.Copy(p, fpath);
            }
        }
        private void CopyExtraDefines(string extradefinesPath)
        {
            if (!Directory.Exists(extradefinesPath))
                Directory.CreateDirectory(extradefinesPath);
            copyDirectoryFiles(Path.Combine("DynamicX", "ExtraDefines"), extradefinesPath);
            string macrosFolderDst = Path.Combine(extradefinesPath, "Macros");
            string macrosFolderSrc = Path.Combine("DynamicX", "ExtraDefines", "Macros");
            if (!Directory.Exists(macrosFolderDst))
                Directory.CreateDirectory(macrosFolderDst);
            copyDirectoryFiles(macrosFolderSrc, macrosFolderDst);
            string dynXDefinesPath = Path.Combine(Path.GetDirectoryName(Options.Instance.OutputROMPath)!,"DynamicXDefines.asm");
            string dynXDefinesDstPath = Path.Combine(extradefinesPath, "DynamicXDefines.asm");
            if (File.Exists(dynXDefinesDstPath))
                File.Delete(dynXDefinesDstPath);
            File.Copy(dynXDefinesPath, dynXDefinesDstPath);
        }
        public void PixiInstallation()
        {
            if (!toolInstallation(Options.Instance.PixiPath, "Pixi", "asm", "routines", "sprites", "extended", "cluster"))
                return;
            CopyExtraDefines(Path.Combine(Options.Instance.PixiPath!, "asm", "ExtraDefines"));
            string ndefines = Path.Combine(Options.Instance.PixiPath!, "sprites", "NormalSpriteDefines.asm");
            if (File.Exists(ndefines))
                File.Delete(ndefines);
            File.Copy(Path.Combine("ASM", "NormalSpriteDefines.asm"), ndefines);
            string cdefines = Path.Combine(Options.Instance.PixiPath!, "cluster", "ClusterSpriteDefines.asm");
            if (File.Exists(cdefines))
                File.Delete(cdefines);
            File.Copy(Path.Combine("ASM", "ClusterSpriteDefines.asm"), cdefines);
            string edefines = Path.Combine(Options.Instance.PixiPath!, "extended", "ExtendedSpriteDefines.asm");
            if (File.Exists(edefines))
                File.Delete(edefines);
            File.Copy(Path.Combine("ASM", "ExtendedSpriteDefines.asm"), edefines);
            copyDirectoryFiles("Routines", Path.Combine(Options.Instance.PixiPath!, "routines"));
            Log.WriteLine("Dynamic X was installed on Pixi");
        }
        public void UberasmToolInstallation()
        {
            if (!toolInstallation(Options.Instance.UberasmToolPath, "Uberasm Tool", "other"))
                return;
            string macrospath = Path.Combine(Options.Instance.UberasmToolPath!, "other", "macro_library.asm");
            if (!File.Exists(macrospath))
            {
                Log.WriteLine($"Uberasm Tool Folder is not valid, Uberasm Tool Installation Failed.");
                return;
            }
            string extradefinespath = Path.Combine(Options.Instance.UberasmToolPath!, "other", "ExtraDefines");
            CopyExtraDefines(extradefinespath);
            string content = File.ReadAllText(macrospath);
            Regex r = new("incsrc \\\"\\.\\/ExtraDefines\\/.+\\.asm\\\"");
            content = r.Replace(content, "").Trim();
            string[] files = Directory.GetFiles(Path.Combine("DynamicX", "ExtraDefines"));
            StringBuilder sb = new(content);
            sb.AppendLine("");
            sb.AppendLine("");
            foreach (string file in files)
                sb.AppendLine($"incsrc \"./ExtraDefines/{Path.GetFileName(file)}\"");
            File.Delete(macrospath);
            File.WriteAllText(macrospath, sb.ToString());
            Log.WriteLine("Dynamic X was installed on Uberasm Tool");
        }
        public void GPSInstallation()
        {
            if (Options.Instance.GPSPath == null || Options.Instance.GPSPath == "")
                return;
            string definesPath = Path.Combine(Options.Instance.GPSPath!, "defines.asm");
            if (!File.Exists(definesPath))
            {
                Log.WriteLine($"GPS Folder is not valid, GPS Installation Failed.");
                return;
            }
            string extradefinespath = Path.Combine(Options.Instance.GPSPath!, "ExtraDefines");
            CopyExtraDefines(extradefinespath);
            string content = File.ReadAllText(definesPath);
            Regex r = new("incsrc \\\"\\.\\/ExtraDefines\\/.+\\.asm\\\"");
            content = r.Replace(content, "").Trim();
            string[] files = Directory.GetFiles(Path.Combine("DynamicX", "ExtraDefines"));
            StringBuilder sb = new(content);
            sb.AppendLine("");
            sb.AppendLine("");
            foreach (string file in files)
                sb.AppendLine($"incsrc \"./ExtraDefines/{Path.GetFileName(file)}\"");
            File.Delete(definesPath);
            File.WriteAllText(definesPath, sb.ToString());
            Log.WriteLine("Dynamic X was installed on GPS");
        }
        public void PrintSummary()
        {
            Log.WriteLine("\n################# Summary #################\n");
            Log.WriteLine($"Dynamic X Installation: {dynamicXSize} bytes ({Math.Round(dynamicXSize / 327.68f) / 100} banks)");
            if (poses != null)
                Log.WriteLine($"Dynamic Poses Inserted: {poses.Count}");
            if (palettes != null)
                Log.WriteLine($"Palettes Inserted: {palettes.Count}");
            if(resources != null)
                Log.WriteLine($"Resources Inserted: {resources.Count}");
            if (Options.Instance.DrawingSystem)
                Log.WriteLine($"Drawable Poses Inserted: {frameInfos.Count}");
            int countdyn = 0;
            int count = 0;
            int countp = paletteTablesSize;
            int countpeff = 0;
            int countRes = 0;
            if (posesSize != null)
            {
                foreach (var sz in posesSize)
                    countdyn += sz;
                countdyn += dynamicRoutinesSize;
                countdyn += buffers!.Count * 11 + 11;
                Log.WriteLine($"Space Used in Dynamic Poses: {countdyn} bytes ({Math.Round(countdyn / 327.68f) / 100} banks)");
            }
            if (palettesSize != null)
            {
                foreach (var sz in palettesSize)
                    countp += sz;
                Log.WriteLine($"Space Used in Palettes: {countp} bytes ({Math.Round(countp / 327.68f) / 100} banks)");
            }
            if (resourcesSize != null)
            {
                foreach (var sz in resourcesSize)
                    countRes += sz;
                Log.WriteLine($"Space Used in Resources: {countRes} bytes ({Math.Round(countRes / 327.68f) / 100} banks)");
            }
            if (Options.Instance.DrawingSystem && graphicRoutinesSize != null)
            {
                foreach (var sz in graphicRoutinesSize)
                    count += sz;
                count += drawRoutineSize;
                count += graphicRoutinesSize!.Count * 11 + 11;
                Log.WriteLine($"Space Used in Drawable Poses: {count} bytes ({Math.Round(count / 327.68f) / 100} banks)");
            }
            if(Options.Instance.PaletteEffects && palEffects != null)
            {
                countpeff = palEffectSize;
                Log.WriteLine($"Space Used in PaletteEffects: {countpeff} bytes ({Math.Round(countpeff / 327.68f) / 100} banks)");
            }
            Log.WriteLine("");
            int total = dynamicXSize + countdyn + countp + count + countpeff + countRes;
            Log.WriteLine($"Total Space Used: {total} bytes ({Math.Round(total / 327.68f) / 100} banks)");
        }
        public void GenerateDefines()
        {
            StringBuilder sb = new(File.ReadAllText(Path.Combine("DynamicX", "ExtraDefines", "DynamicXDefines.asm")));
            sb.Append("\n\n");
            if (Options.Instance.DrawingSystem && graphicRoutinesPosition != null)
            {
                int i = 0;
                foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
                {
                    sb.AppendLine($"!GraphicRoutine{grv.GetFlags()} = ${graphicRoutinesPosition[i]:X6}");
                    i++;
                }
                sb.Append('\n');
            }
            if (posesReferences != null && poses != null)
            {
                sb.AppendLine($"!NumberOfGraphics = ${posesReferences.Count:X4}");
                sb.Append('\n');
                int i = 0;
                foreach (var resRef in posesReferences)
                {
                    sb.AppendLine($"!DynamicPose{poses[i]} = ${SNESROMUtils.PCtoSNES(resRef.Position + 8, mapper):X6}");
                    i++;
                }
                sb.Append('\n');
                i = 0;
                foreach (var resRef in posesReferences)
                {
                    sb.AppendLine($"!DynamicPoseID{poses[i]} = ${i:X4}");
                    i++;
                }
                sb.Append('\n');
            }
            if(paletteReferences != null && palettes != null)
            {
                sb.AppendLine($"!NumberOfPalettes = ${paletteReferences.Count:X4}");
                sb.Append('\n');
                int i = 0;
                foreach (var palRef in paletteReferences)
                {
                    sb.AppendLine($"!Palette{palettes[i]} = ${SNESROMUtils.PCtoSNES(palRef.Position + 8, mapper):X6}");
                    i++;
                }
                sb.Append('\n');
                i = 0;
                foreach (var palRef in paletteReferences)
                {
                    sb.AppendLine($"!PaletteID{palettes[i]} = ${i:X4}");
                    i++;
                }
                sb.Append('\n');
                int palCounter = 0;
                i = 0;
                foreach (var dynInfo in dynamicInfos)
                {
                    if (dynInfo.Palettes == null)
                        continue;
                    sb.AppendLine($"!{dynInfo.ContextName}PaletteTableOffset = ${palCounter:X4}");
                    palCounter += dynInfo.Palettes.Length;
                    i++;
                }
                sb.AppendLine($"!NumberOfPaletteTables = ${i:X4}");
                sb.AppendLine($"!PaletteTableSize = ${palCounter:X4}");
                sb.AppendLine("!PaletteIDTables #= read3(!PaletteTables)");
                sb.AppendLine("!PaletteAddrTables = !PaletteIDTables+(!PaletteTableSize*2)");
                sb.Append('\n');
            }
            if(resourceReferences != null && resources != null && resourcesSize != null)
            {
                sb.AppendLine($"!NumberOfResources = ${resourceReferences.Count:X4}");
                sb.Append('\n');
                int i = 0;
                foreach (var resRef in resourceReferences)
                {
                    sb.AppendLine($"!Resource{resources[i]} = ${SNESROMUtils.PCtoSNES(resRef.Position + 8, mapper):X6}");
                    i++;
                }
                sb.Append('\n');
                i = 0;
                foreach (var resRef in resourceReferences)
                {
                    sb.AppendLine($"!Resource{resources[i]}Size = ${resourcesSize[i]:X6}");
                    i++;
                }
                sb.Append('\n');
            }
            if(Options.Instance.DrawingSystem && frameInfos != null && frameInfos.Count > 0)
            {
                sb.AppendLine($"!NumberOfPoses = ${frameInfos.Count:X4}");
                sb.Append('\n');
                int i = 0;
                foreach (var fi in frameInfos)
                {
                    sb.AppendLine($"!PoseID{fi.ContextName}_{fi.Name} = ${i:X4}");
                    i++;
                }
                sb.Append('\n');

                var groups = FrameInfo.GroupByContextName(frameInfos);
                
                foreach (var kvp in groups)
                    sb.AppendLine($"!RenderBoxXDistanceOutOfScreen{kvp.Key} = ${FrameInfo.GetMaximumRenderBoxXDistanceOutOfScreen(kvp.Value):X4}");
                sb.Append('\n');
                foreach (var kvp in groups)
                    sb.AppendLine($"!RenderBoxYDistanceOutOfScreen{kvp.Key} = ${FrameInfo.GetMaximumRenderBoxYDistanceOutOfScreen(kvp.Value):X4}");
                sb.Append('\n');
            }
            if (Options.Instance.PaletteEffects)
            {
                if (palEffects != null && palEffects.Count > 0)
                {
                    string[] split;
                    string name;
                    int size;
                    int count = 1;
                    foreach (var pal in palEffects)
                    {
                        split = pal.Split(',');
                        name = split[0];
                        size = int.Parse(split[1]);
                        for (int i = 0; i < size; i++, count++)
                            sb.AppendLine($"!PaletteEffectID{name}{i} = ${count:X4}");
                    }
                    sb.AppendLine($"!NumberOfPaletteEffects = ${(palEffects == null ? 0 : count):X4}");
                }
            }
            File.WriteAllText(Path.Combine("TMP", "DynamicXDefines.asm"), sb.ToString());

            string definesPath = Path.Combine(Path.GetDirectoryName(Options.Instance.OutputROMPath)!, "DynamicXDefines.asm");
            if (File.Exists(definesPath))
                File.Delete(definesPath);
            string outputDefinesPath = Path.Combine(Path.GetDirectoryName(Options.Instance.OutputROMPath)!, "DynamicXDefines.asm");
            File.Copy(Path.Combine("TMP", "DynamicXDefines.asm"),
                Path.Combine(Path.GetDirectoryName(Options.Instance.OutputROMPath)!, "DynamicXDefines.asm"));
            Log.WriteLine($"Defines file created: {outputDefinesPath}");
        }
        public void GeneratedPoseData()
        {
            if (graphicRoutinesPosition == null)
                return;
            string content = PoseDataGenerator.GenerateData(graphicRoutinesPosition, frameInfos);
            string tablePath = Path.Combine("DynamicX", "Data", "PoseData.asm");
            if (File.Exists(tablePath))
                File.Delete(tablePath);
            File.WriteAllText(tablePath, content);
            string[] data = PatchApplier.Apply(rom, "ASM/Draw.asm").Split('\n');
            SaveROM(Path.Combine("TMP", "tmp.smc"));
            drawRoutinePosition = int.Parse(data[0]) + 0x200;
            drawRoutineSize = int.Parse(data[1]);
            Log.WriteLine("\n########## Draw Routine Insertion #########\n");
            Log.WriteLine($"Draw Routine Inserted At ${SNESROMUtils.PCtoSNES(drawRoutinePosition, mapper):X6} (PC: {drawRoutinePosition:X6}): {drawRoutineSize} bytes");
        }
        public void GenerateDynamicPoseData()
        {
            if (posesReferences == null)
                return;
            string content = DynamicPoseDataGenerator.GenerateData(mapper, posesReferences, dynamicInfos);
            string tablePath = Path.Combine("DynamicX", "Data", "DynamicPoseData.asm");
            if (File.Exists(tablePath))
                File.Delete(tablePath);
            File.WriteAllText(tablePath, content);
            string[] data = PatchApplier.Apply(rom, "ASM/DynamicRoutines.asm").Split('\n');
            SaveROM(Path.Combine("TMP", "tmp.smc"));
            dynamicRoutinesPosition = int.Parse(data[0]) + 0x200;
            dynamicRoutinesSize = int.Parse(data[1]);
            Log.WriteLine("\n######## Dynamic Routines Insertion #######\n");
            Log.WriteLine($"Dynamic Routines Inserted At ${SNESROMUtils.PCtoSNES(dynamicRoutinesPosition, mapper):X6} (PC: {dynamicRoutinesPosition:X6}): {dynamicRoutinesSize} bytes");
        }
        public void SetUpGraphicRoutinesAddresses()
        {
            if (rom == null)
                return;
            string patch = ResourceTablePatchManager.Generate(GraphicRoutineVersion.GraphicRoutineVersions.Count, 0x1B);
            string patchPath = Path.Combine("TMP", "GraphicRoutineTable.asm");
            if (File.Exists(patchPath))
                File.Delete(patchPath);
            File.WriteAllText(patchPath, patch);
            PatchApplier.Apply(rom, patchPath);
            string filepath = Path.Combine("TMP", GraphicRoutineVersion.GraphicRoutineFolder);
            graphicRoutinesPosition = new();
            graphicRoutinesSize = new();
            int pos, size;
            Log.WriteLine("\n######## Graphic Routines Insertion #######\n");
            foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
            {
                string[] patchingResult = PatchApplier.Apply(rom, $"{filepath}{grv.ID}.asm").Split('\n');
                pos = 0x200 + int.Parse(patchingResult[0]);
                size = int.Parse(patchingResult[1]);
                graphicRoutinesPosition.Add(SNESROMUtils.PCtoSNES(pos + 8, mapper));
                graphicRoutinesSize.Add(size);
                Log.WriteLine($"Graphic Routine {grv.GetFlags()} Inserted At ${SNESROMUtils.PCtoSNES(pos,mapper):X6} (PC: {pos:X6}): {size} bytes");
            }
            SaveROM(Path.Combine("TMP", "tmp.smc"));
        }
        public void InsertPaletteEffects()
        {
            if (rom == null) 
                return;
            int patchLocation = GetPatchLocation();
            if (patchLocation < 0)
                return;
            string patch = ResourceTablePatchManager.Generate(1, 0x30);
            string patchPath = Path.Combine("TMP", "PaletteEffects.asm");
            string palDataPath = Path.Combine("TMP", "PaletteEffectsData.bin");
            var palsColls = PaletteEffectExtension.GetCollections();
            Log.WriteLine($"{palsColls.Count}");
            palEffects = palsColls.Select(x => $"{x.Name!},{x.Effects.Count}").ToList();
            byte[] bin = PaletteEffectExtension.ToBin(palsColls);
            if (File.Exists(palDataPath))
                File.Delete(palDataPath);
            File.WriteAllBytes(palDataPath, bin);
            patch = patch.Replace("dl $000000", "dl Data");
            patch = patch.Replace("dl $FFFFFF", """
                dl $FFFFFF
                Data:
                    incbin "PaletteEffectsData.bin"
                """);
            if (File.Exists(patchPath))
                File.Delete(patchPath);
            File.WriteAllText(patchPath, patch);
            int output = int.Parse(PatchApplier.Apply(rom, patchPath)) + 0x200;
            palEffectSize = bin.Length + 6 + 8;
            Log.WriteLine("\n######## Palette Effects Insertion ########\n");
            Log.WriteLine($"Palette Effects Inserted At ${SNESROMUtils.PCtoSNES(output, mapper):X6} (PC: {output:X6}): {palEffectSize} bytes");

            string[] res = PatchApplier.Apply(rom, Path.Combine("ASM", "PaletteEffects.asm")).Split('\n');
            output = int.Parse(res[0]) + 0x200;
            int size = int.Parse(res[1]);
            palEffectSize += 8+ size;

            Log.WriteLine($"Palette Effects Patch Inserted At ${SNESROMUtils.PCtoSNES(output, mapper):X6} (PC: {output:X6}): {size} bytes");
            Log.WriteLine("");
        }
        public void InsertPalettesTables()
        {
            if (rom == null)
                return;
            int patchLocation = GetPatchLocation();
            if (patchLocation < 0)
                return;
            string patch = ResourceTablePatchManager.Generate(1, 0x42);
            string patchPath = Path.Combine("TMP", "PosePaletteTables.asm");
            string palDataPath = Path.Combine("TMP", "PosePaletteTables.bin");
            var palsColls = PaletteEffectExtension.GetCollections();
            Log.WriteLine($"{palsColls.Count}");
            palEffects = palsColls.Select(x => $"{x.Name!},{x.Effects.Count}").ToList();
            List<byte> binIDs = new();
            List<byte> binAddrs = new();
            Dictionary<string, List<(int, int, int)>> tables = new();
            int index, position;
            List<(int, int, int)> current;
            int count = 0;

            foreach (var dyninfo in dynamicInfos)
            {
                if (dyninfo.Palettes == null)
                    continue;
                current = new();
                tables.Add(dyninfo.ContextName, current);
                for (int i = 0;i < dyninfo.Palettes.Length;i++)
                {
                    index = palettes.IndexOf(Path.GetFileNameWithoutExtension(dyninfo.Palettes[i]));
                    position = SNESROMUtils.PCtoSNES(paletteReferences[index].Position + 8, mapper);
                    current.Add((count, index, position));
                    binIDs.Add((byte)index);
                    binIDs.Add((byte)(index >> 8));
                    binAddrs.Add((byte)position);
                    binAddrs.Add((byte)(position >> 8));
                    binAddrs.Add((byte)(position >> 16));
                }
                count++;
            }
            binIDs = binIDs.Concat(binAddrs).ToList();
            if (binIDs.Count == 0)
                binIDs.Add(0);
            if (File.Exists(palDataPath))
                File.Delete(palDataPath);
            File.WriteAllBytes(palDataPath, binIDs.ToArray());
            patch = patch.Replace("dl $000000", "dl Data");
            patch = patch.Replace("dl $FFFFFF", """
                dl $FFFFFF
                Data:
                    incbin "PosePaletteTables.bin"
                """);
            if (File.Exists(patchPath))
                File.Delete(patchPath);
            File.WriteAllText(patchPath, patch);
            int output = int.Parse(PatchApplier.Apply(rom, patchPath)) + 0x200;
            paletteTablesSize = binIDs.Count + 6 + 8;
            Log.WriteLine("\n######### Palette Tables Insertion ########\n");
            Log.WriteLine($"Palette Tables Inserted At ${SNESROMUtils.PCtoSNES(output, mapper):X6} (PC: {output:X6}): {paletteTablesSize} bytes");
            Log.WriteLine("");
        }
        public void SetUpBufferAddresses()
        {
            if (rom == null)
                return;
            int patchLocation = GetPatchLocation();
            if (patchLocation < 0)
                return;
            if (bufferReferences == null)
                return;
            string patch = ResourceTablePatchManager.Generate(bufferReferences.Count, 0x18);
            string patchPath = Path.Combine("TMP", "ResourceTable.asm");
            if (File.Exists(patchPath))
                File.Delete(patchPath);
            File.WriteAllText(patchPath, patch);
            int output = int.Parse(PatchApplier.Apply(rom, patchPath)) + 0x200;
            foreach (var bufref in bufferReferences)
            {
                setRom(output, bufref.Position);
                output += 3;
            }
            SaveROM(Path.Combine("TMP", "tmp.smc"));
        }
        private void setRom(int address, int value)
        {
            rom[address + 2] = (byte)((value >> 16) & 0xFF);
            rom[address + 1] = (byte)((value >> 8) & 0xFF);
            rom[address] = (byte)(value & 0xFF);
        }
        public void PatchDynamicX()
        {
            generateOptionsFile(Options.Instance);
            string rompath = Path.Combine("TMP", "tmp.smc");
            SaveROM(rompath);
            string patchPath = Path.Combine("DynamicX", "DynamicX.asm");
            string[] res = PatchApplier.Apply(rompath, patchPath).Split('\n');
            string? symbs = Asar.GetSymbolsFile();
            string sympath = Path.Combine(Path.GetDirectoryName(Options.Instance.OutputROMPath)!,
                                           Path.GetFileNameWithoutExtension(Options.Instance.OutputROMPath)!);
            if (File.Exists($"{sympath}.sym"))
                File.Delete($"{sympath}.sym");
            if (File.Exists($"{sympath}.sa1.sym"))
                File.Delete($"{sympath}.sa1.sym");
            File.WriteAllText($"{sympath}.sym", symbs);
            File.WriteAllText($"{sympath}.sa1.sym", symbs);
            var err = Asar.GetErrors();
            rom = File.ReadAllBytes(rompath);
            if (res == null || res.Length < 2)
                return;
            dynamicXSize = int.Parse(res[1]);
            int addr = int.Parse(res[0]);
            Log.WriteLine("\n########## Dynamic X Installation #########\n");
            Log.WriteLine($"Dynamic X Installed At ${addr:X6} (PC: {SNESROMUtils.SNEStoPC(addr,mapper):X6}): {dynamicXSize} bytes");
        }
        public int GetPatchLocation()
        {
            if (rom == null)
                return -1;
            int hijack1 = SNESROMUtils.SNEStoPC(0x00821F, mapper);
            if (rom[hijack1+2] == 0x1B && rom[hijack1 + 1] == 0x80 && rom[hijack1] == 0xA3)
                return -1;
            int snes = SNESROMUtils.JoinAddress(rom[hijack1 + 2], rom[hijack1 + 1], rom[hijack1]);
            return SNESROMUtils.SNEStoPC(snes, mapper);
        }
        public void Remove()
        {
            if (rom == null)
                return;
            int patchLocation = GetPatchLocation();
            if (patchLocation < 0)
                return;
            int dynRouts = patchLocation + 0x00;
            int drawRout = patchLocation + 0x06;
            int resTab = patchLocation + 0x18;
            int gfxroutsTab = patchLocation + 0x1B;
            int palEffTab = patchLocation + 0x30;
            int palEffPatch = patchLocation + 0x33;
            int palTabsPatch = patchLocation + 0x42;
            int join3DynRout = SNESROMUtils.JoinAddress(rom, dynRouts);
            int join3DrawRout = SNESROMUtils.JoinAddress(rom, drawRout);
            int join3palEffTab = SNESROMUtils.JoinAddress(rom, palEffTab);
            int join3palEffPatch = SNESROMUtils.JoinAddress(rom, palEffPatch);
            int join3palTabsPatch = SNESROMUtils.JoinAddress(rom, palTabsPatch);
            int dynRoutsAddr = SNESROMUtils.SNEStoPC(join3DynRout, mapper);
            int drawRoutAddr = SNESROMUtils.SNEStoPC(join3DrawRout, mapper);
            int palEffTabAddr = SNESROMUtils.SNEStoPC(join3palEffTab, mapper);
            int palEffPatchAddr = SNESROMUtils.SNEStoPC(join3palEffPatch, mapper);
            int palTabsPatchAddr = SNESROMUtils.SNEStoPC(join3palTabsPatch, mapper);
            if (dynRoutsAddr > 0) 
                Log.WriteLine($"Removed Dynamic Routines At ${join3DynRout:X6} (PC: {dynRoutsAddr:X6}): {SNESROMUtils.RemoveAt(rom, dynRoutsAddr - 8)} bytes");
            if (drawRoutAddr > 0) 
                Log.WriteLine($"Removed Draw Routine At ${join3DrawRout:X6} (PC: {drawRoutAddr:X6}): {SNESROMUtils.RemoveAt(rom, drawRoutAddr - 8)} bytes");
            if (palEffTabAddr > 0)
                Log.WriteLine($"Removed Palette Effects At ${join3palEffTab:X6} (PC: {palEffTabAddr:X6}): {SNESROMUtils.RemoveAt(rom, palEffTabAddr - 8)} bytes");
            if (palEffPatchAddr > 0)
                Log.WriteLine($"Removed Palette Effects Patch At ${join3palEffPatch:X6} (PC: {palEffPatchAddr:X6}): {SNESROMUtils.RemoveAt(rom, palEffPatchAddr - 8)} bytes");
            if (palTabsPatchAddr > 0)
                Log.WriteLine($"Removed Palette Tables Patch At ${join3palTabsPatch:X6} (PC: {palTabsPatchAddr:X6}): {SNESROMUtils.RemoveAt(rom, palTabsPatchAddr - 8)} bytes");

            if (SNESROMUtils.JoinAddress(rom[resTab + 2], rom[resTab + 1], rom[resTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, resTab, mapper));
            if (SNESROMUtils.JoinAddress(rom[gfxroutsTab + 2], rom[gfxroutsTab + 1], rom[gfxroutsTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, gfxroutsTab, mapper));
            generateOptionsFile(Options.Empty);
            string patchPath = Path.Combine("DynamicX", "DynamicX.asm");
            string rompath = Path.Combine("TMP", "tmp.smc");
            PatchApplier.Apply(rompath, patchPath).Split('\n');
            Log.WriteLine($"Dynamic X Desinstalled At ${SNESROMUtils.PCtoSNES(patchLocation, mapper):X6} (PC: {patchLocation:X6}): {SNESROMUtils.RemoveAt(rom, patchLocation - 8)} bytes");
            SaveROM(Path.Combine("TMP", "tmp.smc"));
        }
        private void printRemoveList(List<(int,int)> list)
        {
            var last = list.Last();
            Log.WriteLine($"Removed Table at ${last.Item1:X6} (PC: {SNESROMUtils.SNEStoPC(last.Item1,mapper):X6}): {last.Item2} bytes.");
            foreach(var item in list)
                if(item != last)
                    Log.WriteLine($"Removed Resource at ${item.Item1:X6} (PC: {SNESROMUtils.SNEStoPC(item.Item1, mapper):X6}): {item.Item2} bytes.");
        }
        public DynamicXErrors? InsertBuffers()
        {
            if (rom == null)
                return DynamicXErrors.ROMNotFound;
            if (buffers == null)
                return DynamicXErrors.BuffersNotGenerated;
            var spaces = SNESROMUtils.FindFreeSpace(rom!);
            spaces.Sort(comp);
            (int, int) space;
            List<ResourceReference> posesRefs;
            List<ResourceReference> palRefs;
            List<ResourceReference> resRefs;
            bufferReferences = new();
            int i = 0;
            foreach(var buf in buffers)
            {
                space = spaces.First(x => x.Item2 >= 8 + buf.Length);
                if (space == default)
                    return DynamicXErrors.ROMWithoutFreeSpace;
                spaces.Remove(space);
                SNESROMUtils.InsertDataWithRats(rom, space.Item1, buf);
                bufferReferences.Add(new(i, i, space.Item1));
                if (posesReferences != null)
                {
                    posesRefs = posesReferences!
                                .Where(x => x.BufferID == i)
                                .ToList();
                    foreach (var resRef in posesRefs)
                        resRef.Position += space.Item1;
                }
                if (paletteReferences != null)
                {
                    palRefs = paletteReferences
                                .Where(x => x.BufferID == i)
                                .ToList();
                    foreach (var palRef in palRefs)
                        palRef.Position += space.Item1;
                }
                if (resourceReferences != null)
                {
                    resRefs = resourceReferences
                                .Where(x => x.BufferID == i)
                                .ToList();
                    foreach (var resRef in resRefs)
                        resRef.Position += space.Item1;
                }
                i++;
            }
            if (posesReferences != null)
            {
                Log.WriteLine("\n############## Pose Insertion #############\n");
                foreach (var resRef in posesReferences!)
                    Log.WriteLine($"GFX {poses![resRef.ID]} Inserted at ${SNESROMUtils.PCtoSNES(resRef.Position, mapper):X6} (PC: {resRef.Position:X6}): {posesSize![resRef.ID]} bytes");
            }
            if (paletteReferences != null)
            {
                int disp = poses != null ? poses.Count : 0;
                Log.WriteLine("\n############ Palette Insertion ############\n");
                foreach (var palRef in paletteReferences!)
                    Log.WriteLine($"Palette {palettes![palRef.ID - disp]} Inserted at ${SNESROMUtils.PCtoSNES(palRef.Position, mapper):X6} (PC: {palRef.Position:X6}): {palettesSize![palRef.ID - disp]} bytes");
            }
            if (resourceReferences != null)
            {
                int disp = poses != null ? poses.Count : 0;
                disp += palettes != null ? palettes.Count : 0;
                Log.WriteLine("\n############ Resource Insertion ###########\n");
                foreach (var resRef in resourceReferences!)
                    Log.WriteLine($"Resource {resources![resRef.ID - disp]} Inserted at ${SNESROMUtils.PCtoSNES(resRef.Position, mapper):X6} (PC: {resRef.Position:X6}): {resourcesSize![resRef.ID - disp]} bytes");
            }
            return null;
        }
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
        public void GenerateBuffers()
        {
            var posedata = DynamicInfo.GetPosesData(dynamicInfos);
            var paldata = DynamicInfo.GetPaletteData(dynamicInfos);
            var resdata = DynamicInfo.GetResourceData(dynamicInfos);
            poses = posedata.Keys.ToList();
            posesSize = posedata.Values.Select(x => x.Length).ToList();
            palettes = paldata.Keys.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
            palettesSize = paldata.Values.Select(x => x.Length).ToList();
            resources = resdata.Keys.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
            resourcesSize = resdata.Values.Select(x => x.Length).ToList();
            List<byte[]> res = resdata.Values.ToList();
            List<byte[]> pals = paldata.Values.ToList();
            List<byte[]> all = new();
            all.AddRange(posedata.Values.ToList());
            all.AddRange(pals);
            all.AddRange(res);
            var mergedata = SNESROMUtils.MergeResources(all);

            posesReferences = mergedata.Item1
                                    .Where(x => x.Item1 < poses.Count)
                                    .Select(x => new ResourceReference(x.Item1, x.Item2, x.Item3))
                                    .ToList();
            paletteReferences = mergedata.Item1
                                    .Where(x => x.Item1 >= poses.Count && x.Item1 < poses.Count + palettes.Count)
                                    .Select(x => new ResourceReference(x.Item1, x.Item2, x.Item3))
                                    .ToList();
            resourceReferences = mergedata.Item1
                                    .Where(x => x.Item1 >= poses.Count + palettes.Count)
                                    .Select(x => new ResourceReference(x.Item1, x.Item2, x.Item3))
                                    .ToList();
            posesReferences.Sort((x1, x2) => x1.ID < x2.ID ? -1 :
                                                x1.ID > x2.ID ? 1 :
                                                0);
            paletteReferences.Sort((x1, x2) => x1.ID < x2.ID ? -1 :
                                                x1.ID > x2.ID ? 1 :
                                                0);
            resourceReferences.Sort((x1, x2) => x1.ID < x2.ID ? -1 :
                                                x1.ID > x2.ID ? 1 :
                                                0);
            buffers = mergedata.Item2;
        }
        public string GetDynamicInfo(out bool validation)
        {
            if (!Options.Instance.DynamicPoses)
            {
                validation = true;
                return "";
            }
            string[] paths = Directory.GetFiles("DynamicInfo");
            DynamicInfo di;
            validation = true;
            StringBuilder sb = new();
            foreach (string path in paths)
            {
                if (Path.GetExtension(path) != ".dynamicinfo")
                    continue;
                di = ReadInfo.ReadDynamicInfo(path);
                sb.AppendLine(di.Validate(out validation));
                dynamicInfos.Add(di);
            }
            return sb.ToString();
        }
        public string GetDrawInfos(out bool validation)
        {
            if(!Options.Instance.DrawingSystem)
            {
                validation = true;
                return "";
            }
            string[] paths = Directory.GetFiles("DrawInfo");
            FrameInfo[] fis;
            validation = true;
            StringBuilder sb = new();
            foreach (string path in paths)
            {
                if (Path.GetExtension(path) != ".drawinfo")
                    continue;
                fis = ReadInfo.ReadFrameInfo(path);
                foreach (FrameInfo fi in fis)
                {
                    sb.AppendLine(fi.Validate(out validation));
                    frameInfos.Add(fi);
                }
            }
            return sb.ToString();
        }
        public void GenerateGraphicRoutines()
        { 
            foreach(var fi in  frameInfos)
                GraphicRoutineVersion.Create(fi);

            Directory.CreateDirectory("TMP");

            string filepath;
            foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
            {
                filepath = Path.Combine("TMP", $"GraphicRoutine{grv.ID}.asm");
                if (File.Exists(filepath))
                    File.Delete(filepath);
                grv.GenerateTables();
                File.WriteAllText(filepath, grv.Content);
            }
        }
    }
}
