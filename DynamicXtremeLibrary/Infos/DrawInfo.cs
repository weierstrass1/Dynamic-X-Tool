using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Infos
{
    public class DrawInfo
    {
        public int ID { get; internal set; }
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
        public DrawInfo(int id, string contextName, string name)
        {
            ID = id;
            ContextName = contextName;
            Name = name;
        }
        public bool OneTile() => GetLength() == 1;
        public bool HasXDisplacement() => XDisplacements != null && XDisplacements.Length > 0;
        public bool HasYDisplacement() => YDisplacements != null && YDisplacements.Length > 0;
        public bool HasXFlip() => FlipXDisplacements != null && FlipXDisplacements.Length > 0 && !EqualArrays(XDisplacements, FlipXDisplacements);
        public bool HasYFlip() => FlipYDisplacements != null && FlipYDisplacements.Length > 0 && !EqualArrays(YDisplacements, FlipYDisplacements);
        public bool HasXYFlip() => HasXFlip() && HasYFlip();
        public bool HasProperties() => Properties != null && Properties!.FirstOrDefault(p => p != 0) != default;
        public bool HasSizes() => Sizes != null;
        public bool AllTilesAreDefault() => allAreDefault(Tiles) && (Tiles == null || Tiles[0] == 0);
        public bool AllPropertiesAreDefault() => !HasProperties() && allAreDefault(Properties);
        public bool AllXDisplacementsAreDefault() => !HasXFlip() && allAreDefault(XDisplacements) && (XDisplacements == null || XDisplacements[0] == 0);
        public bool AllYDisplacementsAreDefault() => !HasYFlip() && allAreDefault(YDisplacements) && (YDisplacements == null || YDisplacements[0] == 0);
        public bool AllSizesAreEqual() => AllSizesAre16() || AllSizesAre8();
        public bool AllSizesAre16() => allAreDefault(Sizes, 2);
        public bool AllSizesAre8() => Sizes != null && Sizes.Length > 0 && allAreDefault(Sizes);
        private static bool allAreDefault(int[]? array, int def = 0)
        {
            return array == null || array.Length == 0 || array.All(v => v == def);
        }
        public bool Validate(LogRegisterSystem logging)
        {
            bool validation = true;

            if (!validateValues(Tiles, 0xFF, out int value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Tile", value, 255));
                validation = false;
            }
            if (!validateValues(Properties, 0xFF, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Property", value, 255));
                validation = false;
            }
            if (!validateValues(XDisplacements, 0xFF, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "X Displacement", value, 255));
                validation = false;
            }
            if (!validateValues(YDisplacements, 0xFF, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Y Displacement", value, 255));
                validation = false;
            }
            if (!validateValues(FlipXDisplacements, 0xFF, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Flip X Displacement", value, 255));
                validation = false;
            }
            if (!validateValues(FlipYDisplacements, 0xFF, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Flip Y Displacement", value, 255));
                validation = false;
            }
            if (!validateAllowedValues(Sizes, [0x00, 0x02], out value))
            {
                logging.Add(new ValueNotAllowed(ContextName, "Size", value, [0x00, 0x02]));
                validation = false;
            }

            int l = GetLength();

            if (l > 128)
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Number of tiles", l, 128));
                validation = false;
            }

            if (hasInvalidLength(Tiles, l) ||
                hasInvalidLength(Properties, l) ||
                hasInvalidLength(XDisplacements, l) ||
                hasInvalidLength(YDisplacements, l) ||
                hasInvalidLength(FlipXDisplacements, l) ||
                hasInvalidLength(FlipYDisplacements, l) ||
                hasInvalidLength(Sizes, l))
            {
                logging.Add(new DrawInfoInconsistentTableSizes(ContextName));
                validation = false;
            }

            return validation;
        }
        private static bool validateValues(int[]? values, int maxValue, out int res)
        {
            res = -1;
            if (values == null)
                return true;
            foreach (int value in values)
                if (value > maxValue)
                {
                    res = value;
                    return false;
                }
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
            key += AllTilesAreDefault() ? 8 : 0;
            key += AllPropertiesAreDefault() ? 16 : 0;
            key += AllXDisplacementsAreDefault() ? 32 : 0;
            key += AllYDisplacementsAreDefault() ? 64 : 0;
            key += AllSizesAreEqual() ? 128 : 0;
            key += AllSizesAre16() ? 256 : 0;
            key += IsDynamic ? 512 : 0;
            return key;
        }
        public override string ToString() => $"{ID}_{ContextName}_{Name}";
        public int GetLength()
        {
            if (Tiles is { Length: > 0 })
                return Tiles.Length;
            if (Properties is { Length: > 0 })
                return Properties.Length;
            if (XDisplacements is { Length: > 0 })
                return XDisplacements.Length;
            if (YDisplacements is { Length: > 0 })
                return YDisplacements.Length;
            if (FlipXDisplacements is { Length: > 0 })
                return FlipXDisplacements.Length;
            if (FlipYDisplacements is { Length: > 0 })
                return FlipYDisplacements.Length;
            if (Sizes is { Length: > 0 })
                return Sizes.Length;
            return 1;
        }
        private static bool hasInvalidLength(int[]? table, int expectedLength)
            => table is { Length: > 0 } && table.Length != expectedLength;
        public int GetRenderBoxXDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(XDisplacements, FlipXDisplacements);
        public int GetRenderBoxYDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(YDisplacements, FlipYDisplacements);
        private int getRenderBoxDistanceOutOfScreen(int[]? disps, int[]? flipdisps)
        {
            int minEdge = 0;
            int maxEdge = 0;
            int length = GetLength();

            for (int i = 0; i < length; i++)
            {
                int tileSize = Sizes is { Length: > 0 } && Sizes[i] == 0 ? 8 : 16;
                int displacement = disps is { Length: > 0 }
                    ? toSignedByte(disps[i])
                    : 0;
                updateEdges(displacement, tileSize, ref minEdge, ref maxEdge);

                if (flipdisps is { Length: > 0 })
                {
                    int flipDisplacement = toSignedByte(flipdisps[i]);
                    updateEdges(flipDisplacement, tileSize, ref minEdge, ref maxEdge);
                }
            }

            return Math.Max(Math.Abs(minEdge), maxEdge);
        }
        private static int toSignedByte(int value)
            => value < 0x80 ? value : value - 0x100;
        private static void updateEdges(int displacement, int tileSize,
            ref int minEdge, ref int maxEdge)
        {
            minEdge = Math.Min(minEdge, displacement);
            maxEdge = Math.Max(maxEdge, displacement + tileSize);
        }
        public static int GetMaximumRenderBoxXDistanceOutOfScreen(IEnumerable<DrawInfo> list)
            => maximumValue(list
                .Select(x => x.GetRenderBoxXDistanceOutOfScreen())
                .ToList());
        public static int GetMaximumRenderBoxYDistanceOutOfScreen(IEnumerable<DrawInfo> list)
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
        public static Dictionary<string, List<DrawInfo>> GroupByContextName(List<DrawInfo> list)
        {
            Dictionary<string, List<DrawInfo>> groups = new();
            foreach(var fi in list)
            {
                if (!groups.ContainsKey(fi.ContextName))
                    groups.Add(fi.ContextName, new());
                groups[fi.ContextName].Add(fi);
            }
            return groups;
        }
        public bool EqualArrays(int[]? arr1, int[]? arr2)
        {
            if (arr1 == null && arr2 == null)
                return true;
            if (arr1 == null || arr2 == null)
                return false;
            if (arr1.Length != arr2.Length)
                return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }
        private static bool validateAllowedValues(int[]? values, int[] allowedValues, out int res)
        {
            res = -1;
            if (values == null)
                return true;
            foreach (int value in values)
                if (!allowedValues.Contains(value))
                {
                    res = value;
                    return false;
                }
            return true;
        }
    }
}
