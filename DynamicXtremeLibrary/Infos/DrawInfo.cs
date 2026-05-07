using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Infos
{
    public class DrawInfo
    {
        public int ID { get; private set; }
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
        public bool OneTile() => Tiles != null && Tiles.Length == 1;
        public bool HasXDisplacement() => XDisplacements != null || !EqualArrays(XDisplacements, FlipXDisplacements);
        public bool HasYDisplacement() => YDisplacements != null || !EqualArrays(YDisplacements, FlipYDisplacements);
        public bool HasXFlip() => FlipXDisplacements != null && !EqualArrays(XDisplacements, FlipXDisplacements);
        public bool HasYFlip() => FlipYDisplacements != null && !EqualArrays(YDisplacements, FlipYDisplacements);
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
            if (!validateValues(Sizes, 0x02, out value))
            {
                logging.Add(new ValueExceedsLimit(ContextName, "Size", value, 2));
                validation = false;
            }

            int l = GetLength();

            if ((Tiles != null && Tiles.Length != l) ||
                (Properties != null && Properties.Length != l) ||
                (XDisplacements != null && XDisplacements.Length != l) ||
                (YDisplacements != null && YDisplacements.Length != l) ||
                (Sizes != null && Sizes.Length != l))
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
            => Tiles != null ? Tiles.Length :
                Properties != null ? Properties.Length :
                XDisplacements != null ? XDisplacements.Length :
                YDisplacements != null ? YDisplacements.Length :
                Sizes != null ? Sizes.Length : 1;
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
        public bool EqualArrays(int[] arr1, int[] arr2)
        {
            if (arr1 == null && arr2 == null)
                return true;
            if (arr1.Length != arr2.Length)
                return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }
    }
}
