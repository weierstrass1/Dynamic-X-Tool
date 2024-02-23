using System.Globalization;
using System.Text.RegularExpressions;

namespace DynamicXLibrary
{
    public partial class ReadInfo
    {
        private const string DYNAMIC_FLAG_PATTERN = @"\s*Dynamic:\s*\n\s*(true|false)";
        private const string POSES_PATTERN = @"\s*PosesGraphics:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private const string PALETTES_PATTERN = @"\s*Palettes:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private const string RESOURCES_PATTERN = @"\s*Resources:\s*\n([\sA-Za-z0-9]+\.bin\s*(\n|$))+";
        private static readonly Regex dynamicFlagRegex = getDynamicFlagRegex();
        private static readonly Regex posesRegex = getPosesRegex();
        private static readonly Regex palettesRegex = getPalettesRegex();
        private static readonly Regex resourcesRegex = getResourcesRegex();
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

            Match posesMatch = posesRegex.Match(dynamicInfo);
            Match palettesMatch = palettesRegex.Match(dynamicInfo);
            Match resourcesMatch = resourcesRegex.Match(dynamicInfo);

            string[]? posesPaths = posesMatch.Success ?
                posesMatch
                    .ToString()
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)[1..] :
                    null;
            string[]? palettesPaths = palettesMatch.Success ?
                palettesMatch
                    .ToString()
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)[1..] :
                    null;
            string[]? resourcesPaths = resourcesMatch.Success ?
                resourcesMatch
                    .ToString()
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)[1..] :
                    null;
            string SpriteName = Path.GetFileNameWithoutExtension(path);
            DynamicInfo DynamicInfo = TablesReader.ReadDynamicInfoTables(SpriteName, dynamicInfo);
            DynamicInfo.Palettes = palettesPaths?.Select(x => x.Replace("\t", "").Replace(" ", "")).ToArray();
            DynamicInfo.Poses = posesPaths?.Select(x => x.Replace("\t", "").Replace(" ", "")).ToArray();
            DynamicInfo.Resources = resourcesPaths?.Select(x => x.Replace("\t", "").Replace(" ", "")).ToArray();
            DynamicInfo.GenerateLastRow();
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
            {
                fi.IsDynamic = dynFlagMatch.Success && bool.Parse(Regex.Match(dynFlagMatch.Value, "(true|false)").Value);
                Log.WriteLine(fi.IsDynamic.ToString());
            }

            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            SpriteName = info.ToTitleCase(SpriteName).Replace(" ", string.Empty);

            return fis;
        }
        [GeneratedRegex(DYNAMIC_FLAG_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getDynamicFlagRegex();
        [GeneratedRegex(POSES_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getPosesRegex();
        [GeneratedRegex(PALETTES_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getPalettesRegex();
        [GeneratedRegex(RESOURCES_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex getResourcesRegex();
    }
}
