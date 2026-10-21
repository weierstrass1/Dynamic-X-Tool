using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Infos
{
    /// <summary>Describes one drawable pose and the optional per-tile tables used by its routine.</summary>
    public class DrawInfo
    {
        /// <summary>Gets the final consecutive pose ID.</summary>
        public int ID { get; internal set; }
        /// <summary>Gets or sets whether the pose uses dynamically uploaded graphics.</summary>
        public bool IsDynamic { get; set; } = false;
        /// <summary>Gets or sets the logical context used in generated symbols.</summary>
        public string ContextName { get; set; }
        /// <summary>Gets the pose name within its context.</summary>
        public string Name { get; private set; }
        /// <summary>Gets or sets optional tile indices.</summary>
        public int[]? Tiles { get; set; }
        /// <summary>Gets or sets optional tile properties.</summary>
        public int[]? Properties { get; set; }
        /// <summary>Gets or sets optional horizontal displacements.</summary>
        public int[]? XDisplacements { get; set; }
        /// <summary>Gets or sets optional vertical displacements.</summary>
        public int[]? YDisplacements { get; set; }
        /// <summary>Gets or sets optional horizontal displacements for X-flipped drawing.</summary>
        public int[]? FlipXDisplacements { get; set; }
        /// <summary>Gets or sets optional vertical displacements for Y-flipped drawing.</summary>
        public int[]? FlipYDisplacements { get; set; }
        /// <summary>Gets or sets optional tile-size values, where 0 is 8x8 and 2 is 16x16.</summary>
        public int[]? Sizes { get; set; }
        /// <summary>Initializes a pose identity before its optional tables are populated.</summary>
        public DrawInfo(int id, string contextName, string name)
        {
            ID = id;
            ContextName = contextName;
            Name = name;
        }
        /// <summary>Determines whether the pose contains exactly one tile.</summary>
        public bool OneTile() => GetLength() == 1;
        /// <summary>Determines whether an X-displacement table is present.</summary>
        public bool HasXDisplacement() => XDisplacements != null && XDisplacements.Length > 0;
        /// <summary>Determines whether a Y-displacement table is present.</summary>
        public bool HasYDisplacement() => YDisplacements != null && YDisplacements.Length > 0;
        /// <summary>Determines whether horizontal flipping requires a distinct displacement table.</summary>
        public bool HasXFlip() => FlipXDisplacements != null && FlipXDisplacements.Length > 0 && !EqualArrays(XDisplacements, FlipXDisplacements);
        /// <summary>Determines whether vertical flipping requires a distinct displacement table.</summary>
        public bool HasYFlip() => FlipYDisplacements != null && FlipYDisplacements.Length > 0 && !EqualArrays(YDisplacements, FlipYDisplacements);
        /// <summary>Determines whether both axes require special flip displacement tables.</summary>
        public bool HasXYFlip() => HasXFlip() && HasYFlip();
        /// <summary>Determines whether any tile uses a nonzero property value.</summary>
        public bool HasProperties() => Properties != null && Properties!.FirstOrDefault(p => p != 0) != default;
        /// <summary>Determines whether an explicit tile-size table is present.</summary>
        public bool HasSizes() => Sizes != null;
        /// <summary>Determines whether tile indices can use the routine's default progression.</summary>
        public bool AllTilesAreDefault() => allAreDefault(Tiles) && (Tiles == null || Tiles[0] == 0);
        /// <summary>Determines whether all tile properties use the zero default.</summary>
        public bool AllPropertiesAreDefault() => !HasProperties() && allAreDefault(Properties);
        /// <summary>Determines whether X positions require neither a table nor flip handling.</summary>
        public bool AllXDisplacementsAreDefault() => !HasXFlip() && allAreDefault(XDisplacements) && (XDisplacements == null || XDisplacements[0] == 0);
        /// <summary>Determines whether Y positions require neither a table nor flip handling.</summary>
        public bool AllYDisplacementsAreDefault() => !HasYFlip() && allAreDefault(YDisplacements) && (YDisplacements == null || YDisplacements[0] == 0);
        /// <summary>Determines whether every tile has the same 8x8 or 16x16 size.</summary>
        public bool AllSizesAreEqual() => AllSizesAre16() || AllSizesAre8();
        /// <summary>Determines whether absent or explicit sizes all use the 16x16 value.</summary>
        public bool AllSizesAre16() => allAreDefault(Sizes, 2);
        /// <summary>Determines whether an explicit size table contains only 8x8 values.</summary>
        public bool AllSizesAre8() => Sizes != null && Sizes.Length > 0 && allAreDefault(Sizes);
        /// <summary>Checks whether an optional table is empty or contains only one default value.</summary>
        private static bool allAreDefault(int[]? array, int def = 0)
        {
            return array == null || array.Length == 0 || array.All(v => v == def);
        }
        /// <summary>Validates table values, tile count, allowed sizes, and consistent optional lengths.</summary>
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
        /// <summary>Finds the first value exceeding an inclusive maximum.</summary>
        private static bool validateValues(int[]? values, int maxValue, out int res)
        {
            res = -1;
            if (values == null)
                return true;
            foreach (int value in values)
                if (value > maxValue || value < 0)
                {
                    res = value;
                    return false;
                }
            return true;
        }
        /// <summary>Serializes tile indices as byte-sized Asar data.</summary>
        public string TilesToString() => HexReader.ValuesToString(Tiles, 2);
        /// <summary>Serializes X displacements as byte-sized Asar data.</summary>
        public string XDisplacementsToString() => HexReader.ValuesToString(XDisplacements, 2);
        /// <summary>Serializes Y displacements as byte-sized Asar data.</summary>
        public string YDisplacementsToString() => HexReader.ValuesToString(YDisplacements, 2);
        /// <summary>Serializes horizontally flipped X displacements as byte-sized Asar data.</summary>
        public string FlipXDisplacementsToString() => HexReader.ValuesToString(FlipXDisplacements, 2);
        /// <summary>Serializes vertically flipped Y displacements as byte-sized Asar data.</summary>
        public string FlipYDisplacementsToString() => HexReader.ValuesToString(FlipYDisplacements, 2);
        /// <summary>Serializes tile sizes as byte-sized Asar data.</summary>
        public string SizesToString() => HexReader.ValuesToString(Sizes!, 2);
        /// <summary>Serializes tile properties as byte-sized Asar data.</summary>
        public string PropertiesToString() => HexReader.ValuesToString(Properties!, 2);
        /// <summary>Encodes this pose's drawing characteristics into a graphic-routine key.</summary>
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
        /// <summary>Returns the pose's ID, context, and local name.</summary>
        public override string ToString() => $"{ID}_{ContextName}_{Name}";
        /// <summary>Derives the tile count from the first populated optional table, defaulting to one.</summary>
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
        /// <summary>Checks whether a populated optional table differs from the derived tile count.</summary>
        private static bool hasInvalidLength(int[]? table, int expectedLength)
            => table is { Length: > 0 } && table.Length != expectedLength;
        /// <summary>Calculates the maximum horizontal distance required beyond a screen edge.</summary>
        public int GetRenderBoxXDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(XDisplacements, FlipXDisplacements);
        /// <summary>Calculates the maximum vertical distance required beyond a screen edge.</summary>
        public int GetRenderBoxYDistanceOutOfScreen()
            => getRenderBoxDistanceOutOfScreen(YDisplacements, FlipYDisplacements);
        /// <summary>Calculates a render extent from normal and optional flipped tile displacements.</summary>
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
        /// <summary>Interprets an unsigned byte value as a signed two's-complement displacement.</summary>
        private static int toSignedByte(int value)
            => value < 0x80 ? value : value - 0x100;
        /// <summary>Expands the minimum and maximum occupied edges for one tile rectangle.</summary>
        private static void updateEdges(int displacement, int tileSize,
            ref int minEdge, ref int maxEdge)
        {
            minEdge = Math.Min(minEdge, displacement);
            maxEdge = Math.Max(maxEdge, displacement + tileSize);
        }
        /// <summary>Gets the largest horizontal render distance among a pose collection.</summary>
        public static int GetMaximumRenderBoxXDistanceOutOfScreen(IEnumerable<DrawInfo> list)
            => maximumValue([.. list.Select(x => x.GetRenderBoxXDistanceOutOfScreen())]);
        /// <summary>Gets the largest vertical render distance among a pose collection.</summary>
        public static int GetMaximumRenderBoxYDistanceOutOfScreen(IEnumerable<DrawInfo> list)
            => maximumValue([.. list.Select(x => x.GetRenderBoxYDistanceOutOfScreen())]);
        /// <summary>Returns the maximum integer using an explicit scan.</summary>
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
        /// <summary>Groups poses by the context that owns their generated symbols.</summary>
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
        /// <summary>Compares two nullable integer arrays by null state, length, and ordered values.</summary>
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
        /// <summary>Finds the first table value outside an explicit allowed-value set.</summary>
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
