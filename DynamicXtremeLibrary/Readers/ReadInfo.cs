using LogRegister;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DynamicXtremeLibrary.Infos;

namespace DynamicXtremeLibrary.Readers
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
        private readonly LogRegisterSystem _logRegisterSystem;
        public ReadInfo(LogRegisterSystem logRegisterSystem)
        {
            _logRegisterSystem = logRegisterSystem;
        }
        public IReadOnlyList<DrawInfo> GetAllDrawInfos(string drawInfoDirectory, out bool validation)
        {
            _logRegisterSystem.Add(new Title("Reading DrawInfo files"));
            string[] paths = Directory.GetFiles("DrawInfo", "*.drawinfo");
            List<DrawInfo> drawInfos = [];
            DrawInfo[] dis;
            validation = true;
            int i = 0;

            foreach (string path in paths)
            {
                _logRegisterSystem.Add(new ProcessingFile(path));
                dis = ReadDrawInfo(path, i);
                foreach (DrawInfo di in dis)
                {
                    if (!di.Validate(_logRegisterSystem))
                    {
                        validation = false;
                        _logRegisterSystem.Add(new FailedToProcessFile(di.ToString()));
                        return drawInfos.AsReadOnly();
                    }
                    _logRegisterSystem.Add(new SuccessfullyProcessedFile(di.ToString()));
                    drawInfos.Add(di);
                }
                i += dis.Length;
            }
            return drawInfos.OrderBy(di => di.ID).ToList().AsReadOnly();
        }
        public IReadOnlyList<DynamicInfo> GetAllDynamicInfos(string dynamicInfoDirectory, string resourceDirectory, out bool validation)
        {
            _logRegisterSystem.Add(new Title("Reading DynamicInfo files"));
            string[] paths = Directory.GetFiles(dynamicInfoDirectory, "*.dynamicinfo");
            List<DynamicInfo> dynamicInfos = [];
            validation = true;
            DynamicInfo di;
            foreach (string path in paths)
            {
                _logRegisterSystem.Add(new ProcessingFile(path));
                di = ReadDynamicInfo(path);
                if (!di.Validate(resourceDirectory, _logRegisterSystem))
                {
                    validation = false;
                    _logRegisterSystem.Add(new FailedToProcessFile(path));
                    return dynamicInfos.AsReadOnly();
                }
                _logRegisterSystem.Add(new SuccessfullyProcessedFile(path));
                dynamicInfos.Add(di);
            }
            return dynamicInfos.AsReadOnly();
        }
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
        public static DrawInfo[] ReadDrawInfo(string path, int idOffset)
        {
            string drawInfos = File.ReadAllText(path).Replace("\r", "");

            drawInfos = string.Join('\n', drawInfos
                                    .Split('\n')
                                    .Where(delegate (string line)
                                    {
                                        string? ltrim = line?.Trim();
                                        return line != null &&
                                            ltrim != null &&
                                            ltrim.Length > 0 &&
                                            ltrim[0] != ';';
                                    }));
            Match dynFlagMatch = dynamicFlagRegex.Match(drawInfos);

            string SpriteName = Path.GetFileNameWithoutExtension(path);

            DrawInfo[] fis = TablesReader.ReadDrawInfoTables(idOffset, SpriteName, drawInfos);

            foreach (var fi in fis)
            {
                fi.IsDynamic = dynFlagMatch.Success && bool.Parse(Regex.Match(dynFlagMatch.Value, "(true|false)").Value);
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
