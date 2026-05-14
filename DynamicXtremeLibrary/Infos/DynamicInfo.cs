using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremeLibrary.Config;

namespace DynamicXtremeLibrary.Infos
{
    public class DynamicInfo
    {
        public string ContextName { get; set; }
        public int[]? PosesChunksSizes { get; set; }
        public int[]? PosesLastRow { get; set; }
        public string[]? PoseGraphics { get; set; }
        public string[]? Palettes { get; set; }
        public string[]? Resources { get; set; }
        public int PoseLength => PosesChunksSizes == null ? 0 : PosesChunksSizes.Length / 2;
        public int PaletteLength => Palettes == null ? 0 : Palettes.Length;
        public int ResourcesLength => Resources == null ? 0 : Resources.Length;
        public DynamicInfo(string contextName) 
        {
            ContextName = contextName;
        }
        public bool Validate(string resourceDirectory, LogRegisterSystem logging)
        {
            bool result = true;
            string path;
            if (Palettes != null)
            {
                for (int i = 0; i < Palettes.Length; i++)
                {
                    path = Path.Combine(resourceDirectory, Palettes[i]);
                    if (!File.Exists(path))
                    {
                        logging.Add(new ResourceNotFound(Palettes[i]));
                        result = false;
                        continue;
                    }
                    if (new FileInfo(path).Length > 32760)
                    {
                        logging.Add(new FileIsToBig(Palettes[i], new FileInfo(path).Length, 32760));
                        result = false;
                    }
                }
            }
            if(Resources != null)
            {
                for (int i = 0; i < Resources.Length; i++)
                {
                    path = Path.Combine(resourceDirectory, Resources[i]);
                    if (!File.Exists(path))
                    {
                        logging.Add(new ResourceNotFound(Resources[i]));
                        result = false;
                    }
                    if (new FileInfo(path).Length > 32760)
                    {
                        logging.Add(new FileIsToBig(Resources[i], new FileInfo(path).Length, 32760));
                        result = false;
                    }
                }
            }
            if (PoseGraphics == null || PoseGraphics.Length == 0)
                return result;
            long totalSize = 0;
           
            for (int i = 0; i < PoseGraphics.Length; i++)
            {
                path = Path.Combine(resourceDirectory, PoseGraphics[i]);
                if (!File.Exists(path))
                {
                    logging.Add(new ResourceNotFound(PoseGraphics[i]));
                    result = false;
                    continue;
                }
                totalSize += new FileInfo(path).Length;
            }
            if (PosesChunksSizes == null || PosesChunksSizes.Length == 0)
            {
                logging.Add(new DynamicInfoWithoutChunks(ContextName));
                result = false;
                return result;
            }
            long totalSizeFromResSizes = 0;
            foreach (var size in PosesChunksSizes)
                totalSizeFromResSizes += size;
            totalSizeFromResSizes *= 32;
            if (totalSize != totalSizeFromResSizes )
            {
                logging.Add(new DynamicInfoSizeMismatch(ContextName, totalSize, totalSizeFromResSizes));
                result = false;
            }

            return result;
        }
        public static IReadOnlyList<Resource> GetAllResources(IEnumerable<DynamicInfo> dis)
        {
            var boolOpts = Options.Instance.BoolOptions.ToDictionary(o => o.Name, o => o.Value);
            List<Resource> result = [];

            if (boolOpts["DynamicPoses"])
                result.AddRange(GetPosesData(dis));
            if (boolOpts["PalettesChange"])
                result.AddRange(GetPaletteData(dis));
            if (boolOpts["GraphicsChange"] || boolOpts["PalettesChange"])
                result.AddRange(GetResourceData(dis));
            return result.AsReadOnly();
        }
        public static IReadOnlyList<Resource> GetPosesData(IEnumerable<DynamicInfo> dis)
        {
            List<Resource> poses = [];
            IEnumerable<Resource> currentPoses;
            int i = 0;
            foreach (var di in dis)
            {
                currentPoses = di.GetPosesData(i);
                poses.AddRange(currentPoses);
                i += currentPoses.Count();
            }
            return poses;
        }
        public static IReadOnlyList<Resource> GetPaletteData(IEnumerable<DynamicInfo> dis)
        {
            Dictionary<string, Resource> pals = [];
            int i = 0;
            IReadOnlyDictionary<string, Resource> currentPalettes;
            foreach (var di in dis)
            {
                currentPalettes = di.GetPaletteData(i, pals.AsReadOnly());
                pals = pals.Concat(currentPalettes).ToDictionary(x => x.Key, x => x.Value);
                i += currentPalettes.Count;
            }
            return pals.Values.ToList().AsReadOnly();
        }
        public static IReadOnlyList<Resource> GetResourceData(IEnumerable<DynamicInfo> dis)
        {
            Dictionary<string, Resource> res = [];
            int i = 0;
            IReadOnlyDictionary<string, Resource> currentResources;
            foreach (var di in dis)
            {
                currentResources = di.GetResourceData(i, res.AsReadOnly());
                res = res.Concat(currentResources).ToDictionary(x => x.Key, x => x.Value);
                i += currentResources.Count;
            }
            return res.Values.ToList().AsReadOnly();
        }
        public IReadOnlyDictionary<string, Resource> GetPaletteData(int idOffset, IReadOnlyDictionary<string, Resource> currentPalettes)
        {
            if (Palettes == null || Palettes.Length == 0)
                return new Dictionary<string, Resource>().AsReadOnly();
            byte[] b;
            Dictionary<string, Resource> result = [];
            int i = idOffset;
            foreach (var path in Palettes)
            {
                if(result.ContainsKey(path) || currentPalettes.ContainsKey(path))
                    continue;
                b = File.ReadAllBytes(Path.Combine("DynamicResources", path));
                result.Add(path, new(i, Path.GetFileNameWithoutExtension(path),
                    ResourceType.Palette, b));
                i++;
            }
            return result;
        }
        public IReadOnlyDictionary<string, Resource> GetResourceData(int idOffset, IReadOnlyDictionary<string, Resource> currentResources)
        {
            if(Resources == null || Resources.Length == 0)
                return new Dictionary<string, Resource>().AsReadOnly();
            byte[] b;
            Dictionary<string, Resource> result = [];
            int i = idOffset;
            foreach (var path in Resources)
            {
                if(currentResources.ContainsKey(path) || result.ContainsKey(path))
                    continue;
                b = File.ReadAllBytes(Path.Combine("DynamicResources", path));
                result.Add(path, new(i, Path.GetFileNameWithoutExtension(path),
                    ResourceType.GeneralResource, b));
                i++;
            }
            return result.AsReadOnly();
        }
        public IReadOnlyList<Resource> GetPosesData(int idOffset)
        {
            if (PoseGraphics == null || PoseGraphics.Length == 0)
                return [];
            if (PosesChunksSizes == null)
                return [];

            List<Resource> poses = [];

            int totalLength = 0;
            foreach (var value in PosesChunksSizes)
            {
                totalLength += value;
            }
            totalLength *= 32;
            byte[] wholeGFX = new byte[totalLength];
            int[] lens = new int[PoseGraphics.Length];
            byte[] b;
            int index = 0;
            int i = 0;
            foreach (var path in PoseGraphics)
            {
                b = File.ReadAllBytes(Path.Combine("DynamicResources", path));
                b.CopyTo(wholeGFX, index);
                index += b.Length;
                lens[i] = b.Length;
                i++;
            }
            int newVal = 0;
            index = 0;
            int gfxInd = 0;
            int fInd = 0;
            int size = 0;
            for (i = 0; i < PosesChunksSizes.Length; i += 2)
            {
                newVal += (PosesChunksSizes[i] + PosesChunksSizes[i + 1]) * 32;
                b = wholeGFX[index..newVal];
                size += b.Length;
                if(size > lens[gfxInd])
                {
                    gfxInd++;
                    fInd = 0;
                    size = b.Length;
                }
                poses.Add(new(idOffset + fInd, 
                    $"{Path.GetFileNameWithoutExtension(PoseGraphics[gfxInd])}{fInd:D3}",
                    ResourceType.DynamicPose, b));
                fInd++;
                index = newVal;
            }
            return poses.AsReadOnly();
        }
        public int[] GetPosesSizes()
        {
            if (PosesChunksSizes == null)
                return Array.Empty<int>();
            int[] res = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseSize(i);
            return res;
        }
        public int GetPoseSize(int id)
        {
            int id2 = id * 2;
            return PosesChunksSizes == null ? 
                        -1 :
                        id2 >= PosesChunksSizes.Length ? 
                            -1 : 
                            32 * (PosesChunksSizes[id2] + PosesChunksSizes[id2 + 1]);
        }
        public int[] GetPosesBlocks()
        {
            if( PosesChunksSizes == null )
                return Array.Empty<int>();
            int[] res = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseBlocks(i);
            return res;
        }
        public int GetPoseBlocks(int id)
        {
            int id2 = id * 2;
            if (PosesChunksSizes == null || id2 >= PosesChunksSizes.Length)
                return -1;
            int baseBlocks = PosesChunksSizes[id2] / 32;
            baseBlocks *= 8;
            int lastRowBlock = Math.Max(PosesChunksSizes[id2] % 32, PosesChunksSizes[id2 + 1]);
            if(lastRowBlock <= 16)
            {
                lastRowBlock += lastRowBlock % 2;
                lastRowBlock /= 2;
                return baseBlocks + lastRowBlock;
            }
            lastRowBlock += lastRowBlock % 4;
            lastRowBlock /= 4;
            return baseBlocks + lastRowBlock;
        }
        public static int[] GetSizes(IEnumerable<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                if (di.PosesChunksSizes != null)
                    res.AddRange(di.PosesChunksSizes!.Select(x => x * 32).ToList());
            return res.ToArray();
        }
        public int[] GetLastRow()
        {
            if(PosesLastRow == null || PosesChunksSizes == null)
                return Array.Empty<int>();
            if (PosesLastRow.Length == PosesChunksSizes.Length / 2)
                return PosesLastRow.Select(x => x*16).ToArray();
            int[] lr = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < lr.Length; i++)
                lr[i] = PosesLastRow[0] * 16;
            return lr;
        }
        public static int[] GetLastRow(IEnumerable<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                res.AddRange(di.GetLastRow());
            return res.ToArray();
        }
        public void GenerateLastRow()
        {
            if (PosesChunksSizes == null || PosesChunksSizes.Length <= 0)
                return;
            PosesLastRow = new int[PosesChunksSizes.Length / 2];
            int val;
            for (int i = 0; i < PosesChunksSizes.Length; i += 2) 
            {
                val = PosesChunksSizes[i] / 32;
                val *= 2;
                val++;
                val *= 16;
                PosesLastRow[i / 2] = val;
            }
        }
    }
}
