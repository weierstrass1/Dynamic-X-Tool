using System.Text.RegularExpressions;

namespace DynamicXLibrary
{
    public partial class TablesReader
    {
        private const string BIG_TABLE_PATTERN = @"[A-Za-z]([A-Za-z0-9]|(_|-))*:\s*\n([A-Za-z]([A-Za-z0-9]|(_|-))*:\s*\n(\s*d(b|w|l)\s*(\$[A-Fa-f0-9]+\s*,?\s*)+)+\s*\n?)+";
        private const string SUB_TABLE_PATTERN = @"^(?!(ResourceLastRow))[A-Za-z]([A-Za-z0-9]|(_|-))*:\s*\n(\s*d(b|w|l)\s*(\$[A-Fa-f0-9]+\s*\,?\s*)+\s*\n?)+";
        private static readonly Regex bigTableRegex = getBigTableRegex();
        private static readonly Regex subTableRegex = getSubTableRegex();
        public static DynamicInfo ReadDynamicInfoTables(string contextName, string input)
        {
            (string, string)[] tables = split(input, bigTableRegex);
            Dictionary<string, (string, string)[]> subTables = new();
            foreach (var table in tables)
                subTables.Add(table.Item1.Replace(":", "").Trim(), split(table.Item2, subTableRegex));
            if (subTables.Count == 0)
                return new(contextName);
            DynamicInfo di = new(contextName)
            {
                ResourceSizes = new int[subTables["PosesChunksSizes"].Length * 2]
            };
            int[] values;
            for (int i = 0; i < subTables["PosesChunksSizes"].Length; i++)
            {
                values = getValues(i, subTables["PosesChunksSizes"])!;
                di.ResourceSizes[i * 2] = values[0];
                di.ResourceSizes[(i * 2) + 1] = values[1];
            }
            return di;
        }
        public static FrameInfo[] ReadFrameInfoTables(string contextName, string input)
        {
            (string, string)[] tables = split(input, bigTableRegex);
            Dictionary<string, (string, string)[]> subTables = new();

            foreach (var table in tables)
                subTables.Add(table.Item1.Replace(":","").Trim(), split(table.Item2, subTableRegex));

            int l = subTables["Tiles"].Length;

            Dictionary<string, FrameInfo> fis = new();
            int[]? tiles, xdisp, ydisp, props, sizes;
            string frameName;

            for (int i = 0; i < l; i++)
            {
                frameName = subTables["Tiles"][i].Item1
                    .Split('_')[0]
                    .Replace("FlipX", "")
                    .Replace("FlipY", "")
                    .Replace("FlipXY", "")
                    .Replace(":", "")
                    .Trim();
                if (subTables.TryGetValue("XDisplacements", out (string, string)[]? xds) && xds[i].Item1.Contains("FlipX"))
                {
                    fis[frameName].FlipXDisplacements = HexReader.GetValues(xds[i].Item2);
                    continue;
                }
                if (subTables.TryGetValue("YDisplacements", out (string, string)[]? yds) && yds[i].Item1.Contains("FlipY"))
                {
                    fis[frameName].FlipYDisplacements = HexReader.GetValues(yds[i].Item2);
                    continue;
                }
                tiles = getValues("Tiles", i, subTables);
                xdisp = getValues(i, xds);
                ydisp = getValues(i, yds);
                props = getValues("Properties", i, subTables);
                sizes = getValues("Sizes", i, subTables);

                fis.Add(frameName, new(contextName, frameName)
                {
                    Tiles = tiles,
                    Properties = props,
                    XDisplacements = xdisp,
                    YDisplacements = ydisp,
                    Sizes = sizes,
                });
            }
            return fis.Values.ToArray();
        }
        private static int[]? getValues(int id, (string, string)[]? values)
            => values != null?
                    HexReader.GetValues(values![id].Item2) :
                    null;
        private static int[]? getValues(string prop, int id, Dictionary<string, (string, string)[]> dic)
        {
            dic.TryGetValue(prop, out (string, string)[]? values);

            return getValues(id, values);
        }
        private static (string, string)[] split(string input, Regex reg)
        {
            MatchCollection matches = reg.Matches(input);
            (string, string)[] vals = new (string, string)[matches.Count];

            string strMatch;
            string title;
            string content;
            int i = 0;
            foreach (var match in matches)
            {
                strMatch = match.ToString()!;
                title = strMatch.Split('\n')[0];
                content = strMatch[title.Length..];

                vals[i] = (title, content);
                i++;
            }
            return vals;
        }
        [GeneratedRegex(BIG_TABLE_PATTERN, RegexOptions.Multiline)]
        private static partial Regex getBigTableRegex();
        [GeneratedRegex(SUB_TABLE_PATTERN, RegexOptions.Multiline)]
        private static partial Regex getSubTableRegex();
    }
}
