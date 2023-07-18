using System.Globalization;
using System.Text.RegularExpressions;

namespace DynamicXLibrary
{
    public partial class ReadInfo
    {
        private const string DYNAMIC_FLAG_PATTERN = @"\s*Dynamic:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private const string GFXS_PATTERN = @"\s*GFXS:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private const string PALETTES_PATTERN = @"\s*Palettes:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private const string LASTROW_PATTERN = @"\s*ResourceLastRow:\s*\n(\s*db\s*\$[A-Fa-z0-9]{2}(\s*,\s*\$[A-Fa-z0-9]{2})*\s*(\n|$))*";
        private static readonly Regex dynamicFlagRegex = getDynamicFlagRegex();
        private static readonly Regex gfxsRegex = getGFXsRegex();
        private static readonly Regex palettesRegex = getPalettesRegex();
        private static readonly Regex lastRowRegex = getLastRowRegex();
        public static DynamicInfo ReadDynamicInfo(string path)
        {
            string dynamicInfo = File.ReadAllText(path).Replace("\r", "");

            dynamicInfo = string.Join('\n', dynamicInfo
                                    .Split('\n')
                                    .Where(delegate (string line)
                                    {
                                        string? ltrim = line?.Trim();
                                        return line != null &&
                                            ltrim != null &&
                                            ltrim.Length > 0 &&
                                            ltrim[0] != ';';
                                    }));

            Match gfxsMatch = gfxsRegex.Match(dynamicInfo);
            Match palettesMatch = palettesRegex.Match(dynamicInfo);
            Match lastRowMatch = lastRowRegex.Match(dynamicInfo);

            string[]? gfxsPaths = gfxsMatch.Success ?
                gfxsMatch
                    .ToString()
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)[1..] :
                    null;
            string[]? palettesPaths = palettesMatch.Success ?
                palettesMatch
                    .ToString()
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)[1..] :
                    null;
            int[]? lastrow = lastRowMatch.Success ?
                HexReader.GetValues(lastRowMatch.ToString()) :
                null;
            string SpriteName = Path.GetFileNameWithoutExtension(path);
            DynamicInfo DynamicInfo = TablesReader.ReadDynamicInfoTables(SpriteName, dynamicInfo);
            DynamicInfo.Palettes = palettesPaths?.Select(x => x.Replace("\t", "").Replace(" ", "")).ToArray();
            DynamicInfo.Resources = gfxsPaths?.Select(x => x.Replace("\t", "").Replace(" ", "")).ToArray();
            DynamicInfo.ResourceLastRow = lastrow;
            return DynamicInfo;
        }
        public static FrameInfo[] ReadFrameInfo(string path)
        {
            string framesInfo = File.ReadAllText(path).Replace("\r", "");

            framesInfo = string.Join('\n', framesInfo
                                    .Split('\n')
                                    .Where(delegate (string line)
                                    {
                                        string? ltrim = line?.Trim();
                                        return line != null &&
                                            ltrim != null &&
                                            ltrim.Length > 0 &&
                                            ltrim[0] != ';';
                                    }));
            Match dynFlagMatch = dynamicFlagRegex.Match(framesInfo);

            string SpriteName = Path.GetFileNameWithoutExtension(path);

            FrameInfo[] fis = TablesReader.ReadFrameInfoTables(SpriteName, framesInfo);

            foreach (var fi in fis)
                fi.IsDynamic = dynFlagMatch.Success && bool.Parse(dynFlagMatch.ToString());

            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            SpriteName = info.ToTitleCase(SpriteName).Replace(" ", string.Empty);

            return fis;
        }
        [GeneratedRegex(DYNAMIC_FLAG_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getDynamicFlagRegex();
        [GeneratedRegex(GFXS_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getGFXsRegex();
        [GeneratedRegex(PALETTES_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getPalettesRegex();
        [GeneratedRegex(LASTROW_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getLastRowRegex();
    }
}
