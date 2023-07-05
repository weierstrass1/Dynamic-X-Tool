using AsarCLR;
using Dynamic_X_Patch;
using DynamicXSNES;
using System.Text;

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
        private List<ResourceReference>? resourceReferences;
        private List<ResourceReference>? paletteReferences;
        private List<ResourceReference>? bufferReferences;
        private List<string>? graphics;
        private List<int>? graphicsSize;
        private List<string>? palettes;
        private List<int>? palettesSize;
        private List<byte[]>? buffers;
        private List<int>? graphicRoutinesPosition;
        private List<int>? graphicRoutinesSize;
        private int dynamicRoutinesSize;
        private int dynamicRoutinesPosition;
        private int drawRoutineSize;
        private int drawRoutinePosition;
        private int dynamicXSize;

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
            SaveROM(output);
            string definesPath = Path.Combine(Path.GetDirectoryName(output)!, "DynamicXDefines.asm");
            if (File.Exists(@definesPath))
                File.Delete(definesPath);
            File.Copy(Path.Combine("TMP", "DynamicXDefines.asm"),
                Path.Combine(Path.GetDirectoryName(output)!, "DynamicXDefines.asm"));
            return log;
        }
        public string? Process()
        {
            if (!Directory.Exists("TMP"))
                Directory.CreateDirectory("TMP");
            Remove();
            PatchDynamicX();
            string log = GetDynamicInfo(out bool validation);
            if (!validation)
                return log;
            log = GetFramesInfo(out validation);
            if (!validation)
                return log;
            GenerateBuffers();
            DynamicXErrors? err = InsertBuffers();
            if (err != null)
                return err;
            GenerateGraphicRoutines();
            SetUpBufferAddresses();
            SetUpGraphicRoutinesAddresses();
            GenerateDynamicPoseData();
            GeneratedPoseData();
            GenerateDefines();
            PrintSummary();
            if (File.Exists("log.txt"))
                File.Delete("log.txt");
            File.WriteAllText("log.txt", Log.GetLog());
            return null;
        }
        public void PrintSummary()
        {
            Log.WriteLine("\n################# Summary #################\n");
            Log.WriteLine($"Dynamic X Installation: {dynamicXSize} bytes ({Math.Round(dynamicXSize / 327.68f) / 100} banks)");
            Log.WriteLine($"Dynamic Poses Inserted: {graphics!.Count}");
            Log.WriteLine($"Palettes Inserted: {palettes!.Count}");
            Log.WriteLine($"Drawable Poses Inserted: {frameInfos.Count}");
            int countdyn = 0;
            foreach (var sz in graphicsSize!)
                countdyn += sz;
            countdyn += dynamicRoutinesSize;
            countdyn += buffers!.Count * 11 + 11;
            Log.WriteLine($"Space Used in Dynamic Poses: {countdyn} bytes ({Math.Round(countdyn / 327.68f) / 100} banks)");
            int countp = 0;
            foreach (var sz in palettesSize!)
                countp += sz;
            Log.WriteLine($"Space Used in Palettes: {countp} bytes ({Math.Round(countp / 327.68f) / 100} banks)");
            int count = 0;
            foreach (var sz in graphicRoutinesSize!)
                count += sz;
            count += drawRoutineSize;
            count += graphicRoutinesSize!.Count * 11 + 11;
            Log.WriteLine($"Space Used in Drawable Poses: {count} bytes ({Math.Round(count / 327.68f) / 100} banks)");
            Log.WriteLine("");
            int total = dynamicXSize + countdyn + countp + count;
            Log.WriteLine($"Total Space Used: {total} bytes ({Math.Round(total / 327.68f) / 100} banks)");
        }
        public void GenerateDefines()
        {
            StringBuilder sb = new(File.ReadAllText(Path.Combine("DynamicX", "DynamicXDefines.asm")));
            sb.Append("\n\n");
            if (graphicRoutinesPosition != null)
            {
                int i = 0;
                foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
                {
                    sb.AppendLine($"!GraphicRoutine{grv.GetFlags()} = ${graphicRoutinesPosition[i]:X6}");
                    i++;
                }
                sb.Append('\n');
            }
            if (resourceReferences != null && graphics != null)
            {
                sb.AppendLine($"!NumberOfGraphics = ${resourceReferences.Count:X4}");
                sb.Append('\n');
                int i = 0;
                foreach (var resRef in resourceReferences)
                {
                    sb.AppendLine($"!GFX{graphics[i]} = ${SNESROMUtils.PCtoSNES(resRef.Position + 8, mapper):X6}");
                    i++;
                }
                sb.Append('\n');
                i = 0;
                foreach (var resRef in resourceReferences)
                {
                    sb.AppendLine($"!GFXID{graphics[i]} = ${i:X4}");
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
                    sb.AppendLine($"!Pallete{palettes[i]} = ${SNESROMUtils.PCtoSNES(palRef.Position + 8, mapper):X6}");
                    i++;
                }
                sb.Append('\n');
                i = 0;
                foreach (var palRef in paletteReferences)
                {
                    sb.AppendLine($"!PalleteID{palettes[i]} = ${i:X4}");
                    i++;
                }
                sb.Append('\n');
            }
            if(frameInfos != null && frameInfos.Count > 0)
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

            }
            File.WriteAllText(Path.Combine("TMP", "DynamicXDefines.asm"), sb.ToString());
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
            if (resourceReferences == null)
                return;
            string content = DynamicPoseDataGenerator.GenerateData(mapper, resourceReferences, dynamicInfos);
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
                graphicRoutinesPosition.Add(pos);
                graphicRoutinesSize.Add(size);
                Log.WriteLine($"Graphic Routine {grv.GetFlags()} Inserted At ${SNESROMUtils.PCtoSNES(pos,mapper):X6} (PC: {pos:X6}): {size} bytes");
            }
            SaveROM(Path.Combine("TMP", "tmp.smc"));
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
            string rompath = Path.Combine("TMP", "tmp.smc");
            SaveROM(rompath);
            string patchPath = Path.Combine("DynamicX", "DynamicX.asm");
            string[] res = PatchApplier.Apply(rompath, patchPath).Split('\n');
            var err = Asar.GetErrors();
            rom = File.ReadAllBytes(rompath);
            dynamicXSize = int.Parse(res[1]);
            int addr = int.Parse(res[0]);
            Log.WriteLine("\n########## Dynamic X Installation #########\n");
            Log.WriteLine($"Dynamic X Installed At ${addr:X6} (PC: {SNESROMUtils.SNEStoPC(addr,mapper):X6}): {dynamicXSize} bytes");
        }
        public int GetPatchLocation()
        {
            if (rom == null)
                return -1;
            int hijack1 = SNESROMUtils.SNEStoPC(0x008241, mapper);
            int hijack2 = SNESROMUtils.SNEStoPC(0x0082DE, mapper);
            if (rom[hijack2] == 0x09 && rom[hijack1 + 1] == 0x84 && rom[hijack1] == 0x49)
                return -1;
            int snes = SNESROMUtils.JoinAddress(rom[hijack2], rom[hijack1 + 1], rom[hijack1]);
            return SNESROMUtils.SNEStoPC(snes, mapper);
        }
        public void Remove()
        {
            if (rom == null)
                return;
            int patchLocation = GetPatchLocation();
            if (patchLocation < 0)
                return;
            int dynRouts = patchLocation + 0x03;
            int drawRout = patchLocation + 0x06;
            int resTab = patchLocation + 0x18;
            int gfxroutsTab = patchLocation + 0x1B;
            int join3DynRout = SNESROMUtils.JoinAddress(rom, dynRouts);
            int join3DrawRout = SNESROMUtils.JoinAddress(rom, drawRout);
            int dynRoutsAddr = SNESROMUtils.SNEStoPC(join3DynRout, mapper);
            int drawRoutAddr = SNESROMUtils.SNEStoPC(join3DrawRout, mapper);
            if (dynRoutsAddr > 0) 
                Log.WriteLine($"Removed Dynamic Routines At ${join3DynRout:X6} (PC: {dynRoutsAddr:X6}): {SNESROMUtils.RemoveAt(rom, dynRoutsAddr - 8)} bytes");
            if (drawRoutAddr > 0) 
                Log.WriteLine($"Removed Draw Routine At ${join3DrawRout:X6} (PC: {drawRoutAddr:X6}): {SNESROMUtils.RemoveAt(rom, drawRoutAddr - 8)} bytes");
            if (SNESROMUtils.JoinAddress(rom[resTab + 2], rom[resTab + 1], rom[resTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, resTab, mapper));
            if (SNESROMUtils.JoinAddress(rom[gfxroutsTab + 2], rom[gfxroutsTab + 1], rom[gfxroutsTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, gfxroutsTab, mapper));
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
            List<ResourceReference> resRefs;
            List<ResourceReference> palRefs;
            bufferReferences = new();
            int i = 0;
            foreach(var buf in buffers)
            {
                space = spaces.First(x => x.Item2 >= 8 + buf.Length);
                if (space == default)
                    return DynamicXErrors.ROMWithoutFreeSpace;
                SNESROMUtils.InsertDataWithRats(rom, space.Item1, buf);
                bufferReferences.Add(new(i, i, space.Item1));
                resRefs = resourceReferences!
                            .Where(x => x.BufferID == i)
                            .ToList();
                palRefs = paletteReferences!
                            .Where(x => x.BufferID == i)
                            .ToList();
                foreach (var resRef in resRefs)
                    resRef.Position += space.Item1;
                foreach (var palRef in palRefs)
                    palRef.Position += space.Item1;
                i++;
            }
            Log.WriteLine("\n############## GFX Insertion ##############\n");
            foreach (var resRef in resourceReferences!)
                Log.WriteLine($"GFX {graphics![resRef.ID]} Inserted at ${SNESROMUtils.PCtoSNES(resRef.Position, mapper):X6} (PC: {resRef.Position:X6}): {graphicsSize![resRef.ID]} bytes");
            Log.WriteLine("\n############ Palette Insertion ############\n");
            foreach (var palRef in paletteReferences!)
                Log.WriteLine($"Palette {palettes![palRef.ID - graphics!.Count]} Inserted at ${SNESROMUtils.PCtoSNES(palRef.Position, mapper):X6} (PC: {palRef.Position:X6}): {palettesSize![palRef.ID - graphics!.Count]} bytes");

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
            var resdata = DynamicInfo.GetFramesData(dynamicInfos);
            var paldata = DynamicInfo.GetPaletteData(dynamicInfos);
            graphics = resdata.Keys.ToList();
            graphicsSize = resdata.Values.Select(x => x.Length).ToList();
            palettes = paldata.Keys.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
            palettesSize = paldata.Values.Select(x => x.Length).ToList();
            List<byte[]> pals = paldata.Values.ToList();
            List<byte[]> all = new();
            all.AddRange(resdata.Values.ToList());
            all.AddRange(pals);
            var mergedata = SNESROMUtils.MergeResources(all);

            resourceReferences = mergedata.Item1
                                    .Where(x => x.Item1 < graphics.Count)
                                    .Select(x => new ResourceReference(x.Item1, x.Item2, x.Item3))
                                    .ToList();
            paletteReferences = mergedata.Item1
                                    .Where(x => x.Item1 >= graphics.Count)
                                    .Select(x => new ResourceReference(x.Item1, x.Item2, x.Item3))
                                    .ToList();
            resourceReferences.Sort((x1, x2) => x1.ID < x2.ID ? -1 :
                                                x1.ID > x2.ID ? 1 :
                                                0);
            paletteReferences.Sort((x1, x2) => x1.ID < x2.ID ? -1 :
                                                x1.ID > x2.ID ? 1 :
                                                0);
            buffers = mergedata.Item2;
        }
        public string GetDynamicInfo(out bool validation)
        {
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
        public string GetFramesInfo(out bool validation)
        {
            string[] paths = Directory.GetFiles("FramesInfo");
            FrameInfo[] fis;
            validation = true;
            StringBuilder sb = new();
            foreach (string path in paths)
            {
                if (Path.GetExtension(path) != ".framesinfo")
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
