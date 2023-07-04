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
        private List<string>? palettes;
        private List<byte[]>? buffers;
        private List<int>? graphicRoutinesPositions;
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
            return null;
        }
        public void GenerateDefines()
        {
            PatchDynamicX();
            StringBuilder sb = new(File.ReadAllText(Path.Combine("DynamicX", "DynamicXDefines.asm")));
            sb.Append("\n\n");
            if (graphicRoutinesPositions != null)
            {
                int i = 0;
                foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
                {
                    sb.AppendLine($"!GraphicRoutine{grv.GetFlags()} = ${graphicRoutinesPositions[i]:X6}");
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
            }
            File.WriteAllText(Path.Combine("TMP", "DynamicXDefines.asm"), sb.ToString());
        }
        public void GeneratedPoseData()
        {
            if (graphicRoutinesPositions == null)
                return;
            string content = PoseDataGenerator.GenerateData(graphicRoutinesPositions, frameInfos);
            string tablePath = Path.Combine("DynamicX", "Data", "PoseData.asm");
            if (File.Exists(tablePath))
                File.Delete(tablePath);
            File.WriteAllText(tablePath, content);
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
            string s1 = PatchApplier.Apply(rom, patchPath);
            string filepath = Path.Combine("TMP", GraphicRoutineVersion.GraphicRoutineFolder);
            graphicRoutinesPositions = new();
            foreach (var grv in GraphicRoutineVersion.GraphicRoutineVersions)
                graphicRoutinesPositions.Add(0x200 + int.Parse(PatchApplier.Apply(rom, $"{filepath}{grv.ID}.asm")));
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
        public string PatchDynamicX()
        {
            string rompath = Path.Combine("TMP", "tmp.smc");
            SaveROM(rompath);
            string patchPath = Path.Combine("DynamicX", "DynamicX.asm");
            string res = PatchApplier.Apply(rompath, patchPath);
            var err = Asar.GetErrors();
            rom = File.ReadAllBytes(rompath);
            return res;
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
            int resTab = patchLocation + 0x18;
            int gfxroutsTab = patchLocation + 0x1B;
            if (SNESROMUtils.JoinAddress(rom[resTab + 2], rom[resTab + 1], rom[resTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, resTab, mapper));
            if (SNESROMUtils.JoinAddress(rom[gfxroutsTab + 2], rom[gfxroutsTab + 1], rom[gfxroutsTab]) != 0)
                printRemoveList(ResourceTablePatchManager.Remove(rom, gfxroutsTab, mapper));
        }
        private void printRemoveList(List<(int,int)> list)
        {
            var last = list.Last();
            Log.WriteLine($"Removed {last.Item2} bytes. Table at ${last.Item1:X6}");
            foreach(var item in list)
                if(item != last)
                    Log.WriteLine($"Removed {item.Item2} bytes. Resource at ${item.Item1:X6}");
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
            palettes = paldata.Keys.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
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
