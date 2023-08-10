using System.Text;

namespace DynamicXLibrary
{
    public class DynamicInfo
    {
        public string ContextName { get; set; }
        public int[]? ResourceSizes { get; set; }
        public int[]? ResourceLastRow { get; set; }
        public string[]? Poses { get; set; }
        public string[]? Palettes { get; set; }
        public string[]? Resources { get; set; }
        public DynamicInfo(string contextName) 
        {
            ContextName = contextName;
        }
        public string Validate(out bool validation)
        {
            StringBuilder sb = new();
            if(Palettes != null)
            {
                for (int i = 0; i < Palettes.Length; i++)
                {
                    if (!File.Exists(Path.Combine("DynamicResources", Palettes[i]))) 
                        sb.AppendLine($"{Palettes[i]} Not Found");
                }
            }
            if(Resources != null)
            {
                for (int i = 0; i < Resources.Length; i++)
                {
                    if (!File.Exists(Path.Combine("DynamicResources", Resources[i])))
                        sb.AppendLine($"{Resources[i]} Not Found");
                }
            }
            if (Poses == null)
            {
                validation = sb.ToString().Equals(string.Empty);
                return sb.ToString();
            }
            for (int i = 0; i < Poses.Length; i++)
            {
                if (!File.Exists(Path.Combine("DynamicResources", Poses[i])))
                    sb.AppendLine($"{Poses[i]} Not Found");
            }
            if (ResourceSizes == null)
                sb.AppendLine($"{ContextName} Includes Dynamic Graphics but doesn't includes ResourceSize");

            validation = sb.ToString().Equals(string.Empty);
            return sb.ToString();
        }
        public static Dictionary<string, byte[]> GetPosesData(List<DynamicInfo> dis)
        {
            Dictionary<string, byte[]> frames = new();
            foreach(var di in dis)
                foreach(var frame in di.GetPosesData())
                    frames.TryAdd(frame.Key, frame.Value);
            return frames;
        }
        public static Dictionary<string, byte[]> GetPaletteData(List<DynamicInfo> dis)
        {
            Dictionary<string, byte[]> pals = new();
            foreach (var di in dis)
                foreach (var pal in di.GetPaletteData())
                    pals.TryAdd(pal.Key, pal.Value);
            return pals;
        }
        public static Dictionary<string, byte[]> GetResourceData(List<DynamicInfo> dis)
        {
            Dictionary<string, byte[]> result = new();
            foreach (var di in dis)
                foreach (var res in di.GetResourceData())
                    result.TryAdd(res.Key, res.Value);
            return result;
        }
        public Dictionary<string, byte[]> GetPaletteData()
        {
            if (Palettes == null || Palettes.Length == 0)
                return new();
            byte[] b;
            Dictionary<string, byte[]> result = new();
            foreach (var path in Palettes)
            {
                b = File.ReadAllBytes(Path.Combine("DynamicResources", path));
                result.Add(path, b);
            }
            return result;
        }
        public Dictionary<string, byte[]> GetResourceData()
        {
            if(Resources == null || Resources.Length == 0)
                return new();
            byte[] b;
            Dictionary<string, byte[]> result = new();
            foreach (var path in Resources)
            {
                b = File.ReadAllBytes(Path.Combine("DynamicResources", path));
                result.Add(path, b);
            }
            return result;
        }
        public Dictionary<string, byte[]> GetPosesData()
        {
            if (Poses == null || Poses.Length == 0)
                return new();
            if (ResourceSizes == null)
                return new();
            Dictionary<string, byte[]> frames = new();

            int totalLength = 0;
            foreach (var value in ResourceSizes)
            {
                totalLength += value;
            }
            totalLength *= 32;
            byte[] wholeGFX = new byte[totalLength];
            int[] lens = new int[Poses.Length];
            byte[] b;
            int index = 0;
            int i = 0;
            foreach (var path in Poses)
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
            for (i = 0; i < ResourceSizes.Length; i += 2)
            {
                newVal += (ResourceSizes[i] + ResourceSizes[i + 1]) * 32;
                b = wholeGFX[index..newVal];
                size += b.Length;
                if(size > lens[gfxInd])
                {
                    gfxInd++;
                    fInd = 0;
                    size = b.Length;
                }
                frames.Add($"{Path.GetFileNameWithoutExtension(Poses[gfxInd])}{fInd:D3}", b);
                fInd++;
                index = newVal;
            }
            return frames;
        }
        public int[] GetPosesSizes()
        {
            if (ResourceSizes == null)
                return Array.Empty<int>();
            int[] res = new int[ResourceSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseSize(i);
            return res;
        }
        public int GetPoseSize(int id)
        {
            int id2 = id * 2;
            return ResourceSizes == null ? 
                        -1 :
                        id2 >= ResourceSizes.Length ? 
                            -1 : 
                            32 * (ResourceSizes[id2] + ResourceSizes[id2 + 1]);
        }
        public int[] GetPosesBlocks()
        {
            if( ResourceSizes == null )
                return Array.Empty<int>();
            int[] res = new int[ResourceSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseBlocks(i);
            return res;
        }
        public int GetPoseBlocks(int id)
        {
            int id2 = id * 2;
            if (ResourceSizes == null || id2 >= ResourceSizes.Length)
                return -1;
            int baseBlocks = ResourceSizes[id2] / 32;
            baseBlocks *= 8;
            int lastRowBlock = Math.Max(ResourceSizes[id2] % 32, ResourceSizes[id2 + 1]);
            if(lastRowBlock < 16)
            {
                lastRowBlock += lastRowBlock % 2;
                lastRowBlock /= 2;
                return baseBlocks + lastRowBlock;
            }
            lastRowBlock += lastRowBlock % 4;
            lastRowBlock /= 4;
            return baseBlocks + lastRowBlock;
        }
        public static int[] GetSizes(List<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                if (di.ResourceSizes != null)
                    res.AddRange(di.ResourceSizes!.Select(x => x * 32).ToList());
            return res.ToArray();
        }
        public int[] GetLastRow()
        {
            if(ResourceLastRow == null || ResourceSizes == null)
                return Array.Empty<int>();
            if (ResourceLastRow.Length == ResourceSizes.Length / 2)
                return ResourceLastRow.Select(x => x*16).ToArray();
            int[] lr = new int[ResourceSizes.Length / 2];
            for (int i = 0; i < lr.Length; i++)
                lr[i] = ResourceLastRow[0] * 16;
            return lr;
        }
        public static int[] GetLastRow(List<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                res.AddRange(di.GetLastRow());
            return res.ToArray();
        }
        public void GenerateLastRow()
        {
            if (ResourceSizes == null || ResourceSizes.Length <= 0)
                return;
            ResourceLastRow = new int[ResourceSizes.Length / 2];
            int val;
            for (int i = 0; i < ResourceSizes.Length; i += 2) 
            {
                val = ResourceSizes[i] / 32;
                val *= 2;
                val++;
                val *= 16;
                ResourceLastRow[i / 2] = val;
            }
        }
    }
}
