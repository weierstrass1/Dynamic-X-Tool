using System.Text;

namespace DynamicXLibrary
{
    public class FrameInfo
    {
        public bool IsDynamic { get; set; } = false;
        public string ContextName { get; set; }
        public string Name { get; private set; }
        public int[]? Tiles { get; set; }
        public int[]? Properties { get; set; }
        public int[]? XDisplacements { get; set; }
        public int[]? YDisplacements { get; set; }
        public int[]? FlipXDisplacements { get; set; }
        public int[]? FlipYDisplacements { get; set; }
        public int[]? Sizes { get; set; }
        public FrameInfo(string contextName, string name)
        {
            ContextName = contextName;
            Name = name;
        }
        public bool OneTile() => Tiles != null && Tiles.Length == 1;
        public bool HasXDisplacement() => XDisplacements != null;
        public bool HasYDisplacement() => YDisplacements != null;
        public bool HasXFlip() => FlipXDisplacements != null;
        public bool HasYFlip() => FlipYDisplacements != null;
        public bool HasXYFlip() => HasXFlip() && HasYFlip();
        public bool HasProperties() => Properties != null;
        public bool HasSizes() => Sizes != null;
        public bool AllTilesAreEquals() => allAreEquals(Tiles) && (Tiles == null || Tiles[0] == 0);
        public bool AllPropertiesAreEquals() => !HasProperties() && allAreEquals(Properties);
        public bool AllXDisplacementsAreEquals() => allAreEquals(XDisplacements) && (XDisplacements == null || XDisplacements[0] == 0);
        public bool AllYDisplacementsAreEquals() => allAreEquals(YDisplacements) && (YDisplacements == null || YDisplacements[0] == 0);
        public bool AllSizesAreEquals() => allAreEquals(Sizes);
        public bool AllSizesAre16() => AllSizesAreEquals() && Sizes != null && Sizes[0] == 2;
        private static bool allAreEquals(int[]? array)
        {
            if (array == null || array.Length == 0)
                return true;
            int firstTile = array[0];
            for (int i = 1; i < array.Length; i++)
                if (firstTile != array[i])
                    return false;
            return true;
        }
        public string Validate(out bool validation)
        {
            StringBuilder sb = new();

            if (!validateValues(Tiles, 0xFF))
                sb.AppendLine($"A tile value exceeds FF in frame {Name}");
            if (!validateValues(Properties, 0xFF))
                sb.AppendLine($"A property value exceeds FF in frame {Name}");
            if (!validateValues(XDisplacements, 0xFF))
                sb.AppendLine($"A x displacement value exceeds FF in frame {Name}");
            if (!validateValues(YDisplacements, 0xFF))
                sb.AppendLine($"A y displacement value exceeds FF in frame {Name}");
            if (!validateValues(FlipXDisplacements, 0xFF))
                sb.AppendLine($"A flip x displacement value exceeds FF in frame {Name}");
            if (!validateValues(FlipYDisplacements, 0xFF))
                sb.AppendLine($"A flip y displacement value exceeds FF in frame {Name}");
            if (!validateValues(Sizes, 0xFF))
                sb.AppendLine($"A size value exceeds FF in frame {Name}");

            int l = GetLength();

            if ((Tiles != null && Tiles.Length != l) ||
                (Properties != null && Properties.Length != l) ||
                (XDisplacements != null && XDisplacements.Length != l) ||
                (YDisplacements != null && YDisplacements.Length != l) ||
                (Sizes != null && Sizes.Length != l))
                sb.AppendLine($"Tables doesn't have the same amount of values. Frame: {Name}");

            validation = sb.Length == 0;

            return sb.ToString();
        }
        private static bool validateValues(int[]? values, int maxValue)
        {
            if (values == null)
                return true;
            foreach (int value in values)
                if (value > maxValue)
                    return false;
            return true;
        }
        public string TilesToString() => HexReader.ValuesToString(Tiles, 2);
        public string XDisplacementsToString() => HexReader.ValuesToString(XDisplacements, 2);
        public string YDisplacementsToString() => HexReader.ValuesToString(YDisplacements, 2);
        public string FlipXDisplacementsToString() => HexReader.ValuesToString(FlipXDisplacements, 2);
        public string FlipYDisplacementsToString() => HexReader.ValuesToString(FlipYDisplacements, 2);
        public string SizesToString() => HexReader.ValuesToString(Sizes!, 2);
        public string PropertiesToString() => HexReader.ValuesToString(Properties!, 2);
        public int GetKey()
        {
            int key = OneTile() ? 1 : 0;
            key += HasXFlip() ? 2 : 0;
            key += HasYFlip() ? 4 : 0;
            key += AllTilesAreEquals() ? 8 : 0;
            key += AllPropertiesAreEquals() ? 16 : 0;
            key += AllXDisplacementsAreEquals() ? 32 : 0;
            key += AllYDisplacementsAreEquals() ? 64 : 0;
            key += AllSizesAreEquals() ? 128 : 0;
            key += AllSizesAre16() ? 256 : 0;
            return key;
        }
        public override string ToString() => Name;
        public int GetLength()
            => Tiles != null ? Tiles.Length :
                Properties != null ? Properties.Length :
                XDisplacements != null ? XDisplacements.Length :
                YDisplacements != null ? YDisplacements.Length :
                Sizes != null ? Sizes.Length : 1;
        public static int[] GetRoutinesAddresses(List<int> addresses, List<FrameInfo> fis)
        {
            List<int> routAddresses = new();
            GraphicRoutineVersion grv;
            int index;
            foreach (FrameInfo fi in fis)
            {
                grv = GraphicRoutineVersion.Get(fi.GetKey())!;
                index = GraphicRoutineVersion.GraphicRoutineVersions.IndexOf(grv);
                routAddresses.Add(addresses[index]);
            }
            return routAddresses.ToArray();
        }
        public static int[] GetOffset(List<FrameInfo> fis)
        {
            List<int> offsets = new();
            GraphicRoutineVersion grv;
            foreach (FrameInfo fi in fis)
            {
                grv = GraphicRoutineVersion.Get(fi.GetKey())!;
                int offset = 0;
                foreach (FrameInfo fi2 in grv.FramesInfo)
                {
                    if (fi2 == fi)
                        break;
                    offset += fi2.GetLength();
                }
                offsets.Add(offset);
            }
            return offsets.ToArray();
        }
        public int GetRenderBoxXDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(XDisplacements, FlipXDisplacements);
        public int GetRenderBoxYDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(YDisplacements, FlipYDisplacements);
        private static int getRenderBoxDistanceOutOfScreen(int[]? disps, int[]? flipdisps)
        {
            if (disps == null || disps.Length == 0)
            {
                return 16;
            }
            int min = int.MaxValue;
            int max = int.MinValue;
            int v;
            foreach (var x in disps)
            {
                v = x < 128 ? x : 1 + (x^0xFF);
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }
            if (flipdisps != null && flipdisps.Length > 0)
            {
                foreach (var x in flipdisps)
                {
                    v = x < 128 ? x : 1 + (x ^ 0xFF);
                    if (v < min)
                        min = v;
                    if (v > max)
                        max = v;
                }
            }
            return Math.Max(Math.Abs(min), max + 16);
        }
        public static int GetMaximumRenderBoxXDistanceOutOfScreen(List<FrameInfo> list)
            => maximumValue(list
                .Select(x => x.GetRenderBoxXDistanceOutOfScreen())
                .ToList());
        public static int GetMaximumRenderBoxYDistanceOutOfScreen(List<FrameInfo> list)
            => maximumValue(list
                .Select(x => x.GetRenderBoxYDistanceOutOfScreen())
                .ToList());
        private static int maximumValue(List<int> list)
        {
            int max = int.MinValue;
            foreach(var i in list)
            {
                if(i > max)
                    max = i;
            }
            return max;
        }
        public static Dictionary<string, List<FrameInfo>> GroupByContextName(List<FrameInfo> list)
        {
            Dictionary<string, List<FrameInfo>> groups = new();
            foreach(var fi in list)
            {
                if (!groups.ContainsKey(fi.ContextName))
                    groups.Add(fi.ContextName, new());
                groups[fi.ContextName].Add(fi);
            }
            return groups;
        }
    }
}
