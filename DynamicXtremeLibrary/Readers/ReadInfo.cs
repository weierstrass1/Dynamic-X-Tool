using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using LogRegister;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.Readers
{
    /// <summary>Parses current and legacy DynamicInfo and DrawInfo source formats with validation logs.</summary>
    public partial class ReadInfo
    {
        private readonly LogRegisterSystem _logRegisterSystem;
        /// <summary>Initializes the reader with its validation log.</summary>
        public ReadInfo(LogRegisterSystem logRegisterSystem)
        {
            _logRegisterSystem = logRegisterSystem;
        }
        /// <summary>Recursively loads, validates, and assigns IDs to traditional DrawInfo files.</summary>
        public IReadOnlyList<DrawInfo> GetAllDrawInfos(string drawInfoDirectory, out bool validation)
        {
            string[] paths = [.. InputPath.GetFiles(drawInfoDirectory, "*.drawinfo")
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)];
            if (paths == null || paths.Length == 0)
            {
                validation = true;
                return new List<DrawInfo>().AsReadOnly();
            }
            _logRegisterSystem.Add(new Title("Reading DrawInfo files")
            {
                AppearInErrors = true                
            });
            if (!AsarSymbolName.ValidateUnique(paths, _logRegisterSystem))
            {
                validation = false;
                return Array.Empty<DrawInfo>();
            }
            List<DrawInfo> drawInfos = [];
            DrawInfo[] dis;
            validation = true;
            int i = 0;

            foreach (string path in paths)
            {
                _logRegisterSystem.Add(new ProcessingFile(path));
                dis = ReadDrawInfo(path, i, out validation);
                if(!validation)
                {
                    validation = false;
                    _logRegisterSystem.Add(new FailedToProcessFile(path));
                    return drawInfos.AsReadOnly();
                }
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
        /// <summary>Recursively loads and validates traditional DynamicInfo files and generated names.</summary>
        public IReadOnlyList<DynamicInfo> GetAllDynamicInfos(string dynamicInfoDirectory, string resourceDirectory, out bool validation)
        {
            string[] paths = [.. InputPath.GetFiles(dynamicInfoDirectory, "*.dynamicinfo")
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)];
            if (paths == null || paths.Length == 0)
            {
                validation = true;
                return new List<DynamicInfo>().AsReadOnly();
            }
            _logRegisterSystem.Add(new Title("Reading DynamicInfo files")
            {
                AppearInErrors = true
            });
            if (!AsarSymbolName.ValidateUnique(paths, _logRegisterSystem))
            {
                validation = false;
                return Array.Empty<DynamicInfo>();
            }
            List<DynamicInfo> dynamicInfos = [];
            validation = true;
            DynamicInfo? di;
            foreach (string path in paths)
            {
                _logRegisterSystem.Add(new ProcessingFile(path));
                di = ReadDynamicInfo(path, resourceDirectory, resourceDirectory, out validation);
                if (!validation || !di!.Validate(resourceDirectory, _logRegisterSystem))
                {
                    validation = false;
                    _logRegisterSystem.Add(new FailedToProcessFile(path));
                    return dynamicInfos.AsReadOnly();
                }
                _logRegisterSystem.Add(new SuccessfullyProcessedFile(path));
                dynamicInfos.Add(di);
            }
            bool generatedNamesAreValid =
                AsarSymbolName.ValidateUnique(
                    dynamicInfos.SelectMany(info => info.PoseGraphics ?? []),
                    _logRegisterSystem) &
                AsarSymbolName.ValidateUnique(
                    dynamicInfos.SelectMany(info => info.Palettes ?? [])
                        .Distinct(InputPath.Comparer),
                    _logRegisterSystem) &
                AsarSymbolName.ValidateUnique(
                    dynamicInfos.SelectMany(info => info.Resources ?? [])
                        .Distinct(InputPath.Comparer),
                    _logRegisterSystem);
            if (!generatedNamesAreValid)
                validation = false;
            return dynamicInfos.AsReadOnly();
        }
        /// <summary>
        /// Reads a DynamicInfo using the traditional Dynamic Xtreme resource and temporary
        /// directories.
        /// </summary>
        /// <param name="path">The DynamicInfo definition to read.</param>
        /// <param name="validation">Whether parsing completed successfully.</param>
        /// <returns>The parsed DynamicInfo, or <see langword="null"/> on failure.</returns>
        public DynamicInfo? ReadDynamicInfo(string path, out bool validation)
            => ReadDynamicInfo(path, "DynamicResources", "DynamicResources", out validation);

        /// <summary>
        /// Reads a DynamicInfo with explicit directories for resolving declared resources and
        /// storing temporary converted graphics.
        /// </summary>
        /// <param name="path">The DynamicInfo definition to read.</param>
        /// <param name="resourceDirectory">The base directory for relative resource declarations.</param>
        /// <param name="temporaryDirectory">The directory used for generated intermediate graphics.</param>
        /// <param name="validation">Whether parsing completed successfully.</param>
        /// <returns>The parsed DynamicInfo configured with the supplied directories, or <see langword="null"/> on failure.</returns>
        public DynamicInfo? ReadDynamicInfo(string path, string resourceDirectory,
            string temporaryDirectory, out bool validation)
        {
            string dynamicInfo = CleanFileContent(path);

            string[] lines = dynamicInfo.Split('\n');

            bool palettesDone = false;
            bool posesGraphicsDone = false;
            bool resourcesDone = false;
            bool legacyDone = false;
            bool currentDone = false;

            IReadOnlyList<string> palettes = [];
            IReadOnlyList<string> posesGraphics = [];
            IReadOnlyList<string> resources = [];
            IReadOnlyList<(int, int)> legacyPoseChunkSizes = [];
            IDictionary<int, string> currentNumberOf16x16TilesPerPose = new Dictionary<int, string>().AsReadOnly();

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;
                switch(lines[i])
                {
                    case "PosesGraphics:":
                        if(posesGraphicsDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated PosesGraphics label"));
                            validation = false;
                            return null;
                        }
                        posesGraphics = readDynamicInfoTables(i, lines, out i, path, out validation);
                        if (!validation)
                            return null;
                        posesGraphicsDone = true;
                        break;
                    case "Palettes:":
                        if (palettesDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Palettes label"));
                            validation = false;
                            return null;
                        }
                        palettes = readDynamicInfoTables(i, lines, out i, path, out validation);
                        if (!validation)
                            return null;
                        palettesDone = true;
                        break;
                    case "Resources:":
                        if (resourcesDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Resources label"));
                            validation = false;
                            return null;
                        }
                        resources = readDynamicInfoTables(i, lines, out i, path, out validation);
                        if (!validation)
                            return null;
                        resourcesDone = true;
                        break;
                    case "PosesChunksSizes:":
                        if(legacyDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated PosesChunksSizes label"));
                            validation = false;
                            return null;
                        }
                        if (currentDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use NumberOf16x16TilesPerPose and PosesChunksSizes at the same time"));
                            validation = false;
                            return null;
                        }
                        legacyPoseChunkSizes = readDynamicInfoLegacy(i, lines, out i, path, out validation);
                        if (!validation)
                            return null;
                        legacyDone = true;
                        break;
                    case "NumberOf16x16TilesPerPose:":
                        if (legacyDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use NumberOf16x16TilesPerPose and PosesChunksSizes at the same time"));
                            validation = false;
                            return null;
                        }
                        if (currentDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated NumberOf16x16TilesPerPose label"));
                            validation = false;
                            return null;
                        }
                        currentNumberOf16x16TilesPerPose = readDynamicInfoCurrent(i, lines, out i, path, out validation);
                        if (!validation)
                            return null;
                        currentDone = true;
                        break;
                    default:
                        _logRegisterSystem.Add(new SyntaxError(path, i, lines[i]));
                        validation = false;
                        return null;
                }
            }
            DynamicInfo di = new(AsarSymbolName.FromPath(path))
            {
                Palettes = [.. palettes],
                PoseGraphics = [.. posesGraphics],
                Resources = [.. resources],
                DeclaredPalettes = [.. palettes],
                DeclaredPoseGraphics = [.. posesGraphics],
                DeclaredResources = [.. resources],
                ResourceDirectory = resourceDirectory,
                TemporaryDirectory = temporaryDirectory
            };
            if (legacyDone)
            {
                List<int> pcs = [];
                foreach (var tuple in legacyPoseChunkSizes)
                    pcs.AddRange([tuple.Item1, tuple.Item2]);
                di.PosesChunksSizes = [.. pcs];
                di.GenerateLastRow();
            }
            else if(currentDone)
            {
                di.FromNumberOf16x16Tiles(currentNumberOf16x16TilesPerPose);
            }
            validation = true;
            return di;
        }
        /// <summary>Reads every pose in one DrawInfo file using either its current or legacy format.</summary>
        public DrawInfo[] ReadDrawInfo(string path, int idOffset, out bool validation)
        {
            List<DrawInfo> dis = [];
            validation = true;
            string drawInfos = CleanFileContent(path);

            string[] lines = drawInfos.Split('\n');

            bool? isLegacy = null;
            bool dynamicDone = false;

            bool isDynamic = false;
            bool tilesDone = false;
            bool propertiesDone = false;
            bool xDisplacementsDone = false;
            bool yDisplacementsDone = false;
            bool sizesDone = false;

            Dictionary<string, (List<int>, List<int>)> tiles = [];
            Dictionary<string, (List<int>, List<int>)> properties = [];
            Dictionary<string, (List<int>, List<int>)> xDisplacements = [];
            Dictionary<string, (List<int>, List<int>)> yDisplacements = [];
            Dictionary<string, (List<int>, List<int>)> sizes = [];

            Regex currentPoses = drawInfoCurrent();
            IEnumerable<DrawInfo> dradd;

            int idoff = idOffset;

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;
                switch (lines[i])
                {
                    case "Dynamic:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (dynamicDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Dynamic label"));
                            validation = false;
                            return [];
                        }
                        isDynamic = readDrawInfoIsDynamic(i, lines, out i, path, out validation);
                        if (!validation)
                            return [];
                        dynamicDone = true;
                        isLegacy = true;
                        break;
                    case "Tiles:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (tilesDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Tiles label"));
                            validation = false;
                            return [];
                        }
                        tiles = readDrawInfoTable("Tiles", i, lines, out i, path, out validation);
                        if (!validation)
                            return [];
                        tilesDone = true;
                        isLegacy = true;
                        break;
                    case "Properties:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (propertiesDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Properties label"));
                            validation = false;
                            return [];
                        }
                        properties = readDrawInfoTable("Properties", i, lines, out i, path, out validation);
                        if (!validation)
                            return [];
                        propertiesDone = true;
                        isLegacy = true;
                        break;
                    case "XDisplacements:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (xDisplacementsDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated XDisplacements label"));
                            validation = false;
                            return [];
                        }
                        xDisplacements = readDrawInfoTable("XDisp", i, lines, out i, path, out validation, "FlipX");
                        if (!validation)
                            return [];
                        xDisplacementsDone = true;
                        isLegacy = true;
                        break;
                    case "YDisplacements:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (yDisplacementsDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated YDisplacements label"));
                            validation = false;
                            return [];
                        }
                        yDisplacements = readDrawInfoTable("YDisp", i, lines, out i, path, out validation, "FlipY");
                        if (!validation)
                            return [];
                        yDisplacementsDone = true;
                        isLegacy = true;
                        break;
                    case "Sizes:":
                        if (isLegacy == false)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if (sizesDone)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "Repeated Sizes label"));
                            validation = false;
                            return [];
                        }
                        sizes = readDrawInfoTable("Sizes", i, lines, out i, path, out validation);
                        if (!validation)
                            return [];
                        sizesDone = true;
                        isLegacy = true;
                        break;
                    default:
                        if(isLegacy == true)
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i], "You can't use legacy and current format at the same time."));
                            validation = false;
                            return [];
                        }
                        if(!currentPoses.IsMatch(lines[i]))
                        {
                            _logRegisterSystem.Add(new SyntaxError(path, i, lines[i]));
                            validation = false;
                            return [];
                        }
                        dradd = readDrawInfoCurrent(idoff, i, lines, out i, path, out validation);
                        if (!validation)
                            return [];
                        foreach (DrawInfo drawInfo in dradd)
                        {
                            int existingIndex = dis.FindIndex(di => di.ContextName == drawInfo.ContextName && di.Name == drawInfo.Name);
                            if (existingIndex >= 0)
                            {
                                drawInfo.ID = dis[existingIndex].ID;
                                dis[existingIndex] = drawInfo;
                                continue;
                            }
                            drawInfo.ID = idoff++;
                            dis.Add(drawInfo);
                        }
                        isLegacy = false;
                        break;
                }
            }
            if (isLegacy == false)
                return [.. dis];

            Dictionary<string, DrawInfo> drawInfosDic = [];

            List<string> names = [.. tiles.Keys.Union(properties.Keys)
                    .Union(xDisplacements.Keys)
                    .Union(yDisplacements.Keys)
                    .Union(sizes.Keys)
                    .Distinct()];
            string context = AsarSymbolName.FromPath(path);

            foreach (var name in names)
            {
                drawInfosDic.Add(name, new DrawInfo(idOffset + drawInfosDic.Count, context, name)
                {
                    IsDynamic = isDynamic,
                    Tiles = tiles.TryGetValue(name, out var value) ? 
                        [.. value.Item1] : 
                        null,
                    Properties = properties.TryGetValue(name, out value) ? 
                        [.. value.Item1] : 
                        null,
                    XDisplacements = xDisplacements.TryGetValue(name, out value) ? 
                        [.. value.Item1] : 
                        null,
                    YDisplacements = yDisplacements.TryGetValue(name, out value) ? 
                        [.. value.Item1] : 
                        null,
                    FlipXDisplacements = xDisplacements.TryGetValue(name, out value) ? 
                        [.. value.Item2] : 
                        null,
                    FlipYDisplacements = yDisplacements.TryGetValue(name, out value) ? 
                        [.. value.Item2] : null,
                    Sizes = sizes.TryGetValue(name, out value) ? [.. value.Item1] : 
                        null
                });
            }
             
            return [.. drawInfosDic.Values];
        }
        /// <summary>Determines whether a line begins a recognized DynamicInfo section.</summary>
        private static bool isDynamicInfoLabel(string line)
        {
            return line == "PosesGraphics:" ||
                   line == "Palettes:" ||
                   line == "Resources:" ||
                   line == "PosesChunksSizes:" ||
                   line == "NumberOf16x16TilesPerPose:";
        }
        /// <summary>Determines whether a line begins a recognized DrawInfo section.</summary>
        private static bool isDrawInfoLabel(string line)
        {
            return line == "Dynamic:" ||
                   line == "Tiles:" ||
                   line == "Properties:" ||
                   line == "XDisplacements:" ||
                   line == "YDisplacements:" ||
                   line == "Sizes:" ||
                   drawInfoCurrent().IsMatch(line);
        }
        /// <summary>Removes comments and normalizes whitespace before parsing an info file.</summary>
        public static string CleanFileContent(string path)
        {
            string content = File.ReadAllText(path).Replace("\r\n", "\n");
            content = commentRegex().Replace(content, "");

            Regex space = spaceRegex();

            content = string.Join('\n', content
                                    .Split('\n')
                                    .Select(l => space.Replace(l, " ").Trim()));
            return content;
        }
        /// <summary>Reads the single Boolean value beneath a legacy Dynamic label.</summary>
        private bool readDrawInfoIsDynamic(int line, string[] content, out int outLine, string path, out bool validation)
        {
            validation = true;
            string lowercase;
            for (outLine = line + 1; outLine < content.Length; outLine++)
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if (isDynamicInfoLabel(content[outLine]))
                {
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Empty Dynamic label"));
                    validation = false;
                    return true;
                }
                lowercase = content[outLine].ToLower();
                if (lowercase != "true" && lowercase != "false")
                {
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Invalid Dynamic label content"));
                    validation = false;
                    return true;
                }
                return lowercase == "true";
            }
            validation = false;
            _logRegisterSystem.Add(new SyntaxError(path, outLine - 1, content[outLine - 1], "Empty Dynamic label"));
            validation = false;
            return true;
        }
        /// <summary>Reads a list of resource paths beneath a DynamicInfo section label.</summary>
        private IReadOnlyList<string> readDynamicInfoTables(int line, string[] content, out int outLine, string path, out bool validation)
        {
            validation = true;
            List<string> listMembers = [];

            for (outLine = line + 1; outLine < content.Length; outLine++) 
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if(isDynamicInfoLabel(content[outLine]))
                {
                    outLine--;
                    return listMembers.AsReadOnly();
                }
                if (content[outLine].IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Invalid path"));
                    validation = false;
                    return listMembers.AsReadOnly();
                }
                listMembers.Add(content[outLine]);
            }
            return listMembers.AsReadOnly();
        }
        
        /// <summary>Reads legacy upper-and-lower pose chunk pairs.</summary>
        private IReadOnlyList<(int, int)> readDynamicInfoLegacy(int line, string[] content, out int outLine, string path, out bool validation)
        {
            validation = true;
            Dictionary<string, (int,int)> posechunkSizes = [];
            Regex legDI = dynInfoLegacy();
            Regex table = numberTableRegex();
            int[] vals;
            for (outLine = line + 1; outLine < content.Length; outLine++)
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if (isDynamicInfoLabel(content[outLine]))
                {
                    outLine--;
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                if (!legDI.IsMatch(content[outLine]))
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine]));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                if(posechunkSizes.ContainsKey(content[outLine]))
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Repeated Frame"));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                int valuesLine = outLine + 1;
                while (valuesLine < content.Length && string.IsNullOrWhiteSpace(content[valuesLine]))
                    valuesLine++;
                if (valuesLine >= content.Length || isDynamicInfoLabel(content[valuesLine]))
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Incomplete PosesChunksSizes structure"));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                if (!table.IsMatch(content[valuesLine]))
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, valuesLine, content[valuesLine]));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                if (content[valuesLine][0..2] != "db")
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, valuesLine, content[valuesLine], "Should use db"));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                vals = HexReader.GetValues(content[valuesLine]);
                if(vals.Length != 2)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, valuesLine, content[valuesLine], "You Should use 2 values per PosesChunksSizes"));
                    return posechunkSizes.Values.ToList().AsReadOnly();
                }
                posechunkSizes.Add(content[outLine], (vals[0], vals[1]));
                outLine = valuesLine;
            }
            return posechunkSizes.Values.ToList().AsReadOnly();
        }
        /// <summary>Reads current pose ranges and their tile-count expressions.</summary>
        private IDictionary<int, string> readDynamicInfoCurrent(int line, string[] content, out int outLine, string path, out bool validation)
        {
            validation = true;
            Dictionary<int, string> numberOf16x16TilesPerPose = [];
            Regex table = dynInfoCurrent();
            Match m;
            int start;
            int end;
            int tiles;
            string modifier;
            for (outLine = line + 1; outLine < content.Length; outLine ++)
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if (isDynamicInfoLabel(content[outLine]))
                {
                    outLine--;
                    break;
                }
                m = table.Match(content[outLine]);
                if(!m.Success)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine]));
                    return numberOf16x16TilesPerPose.AsReadOnly();
                }
                start = int.Parse(m.Groups["start"].Value);
                end = start;
                if (m.Groups["end"].Success)
                {
                    end = int.Parse(m.Groups["end"].Value);
                    if (start > end)
                    {
                        validation = false;
                        _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Start Pose ID must be lower or equal to End Pose ID"));
                        return numberOf16x16TilesPerPose.AsReadOnly();
                    }
                }
                tiles = int.Parse(m.Groups["tiles"].Value);
                if(tiles < 0)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "You can't use negative values"));
                    return numberOf16x16TilesPerPose.AsReadOnly();
                }
                modifier = "";
                if (m.Groups["modifier"].Success)
                    modifier = m.Groups["modifier"].Value;
                else if(tiles == 0)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "You can't 0 tiles without a modifier (q, h or q3)"));
                    return numberOf16x16TilesPerPose.AsReadOnly();
                }
                for (int i = start; i <= end; i++)
                {
                    if(numberOf16x16TilesPerPose.ContainsKey(i))
                    {
                        validation = false;
                        _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Repeated pose"));
                        return numberOf16x16TilesPerPose.AsReadOnly();
                    }
                    numberOf16x16TilesPerPose.Add(i, $"{tiles}{modifier}");
                }
            }
            int[] poseIDs = [.. numberOf16x16TilesPerPose.Keys.Order()];
            if (!poseIDs.SequenceEqual(Enumerable.Range(0, poseIDs.Length)))
            {
                validation = false;
                _logRegisterSystem.Add(new SyntaxError(path, line, content[line],
                    "Pose IDs must be consecutive and start at 0"));
            }
            return numberOf16x16TilesPerPose;
        }
        /// <summary>Reads a legacy named DrawInfo table and its optional flipped values.</summary>
        private Dictionary<string, (List<int>, List<int>)> readDrawInfoTable(string suffix, int line, string[] content, out int outLine, string path, out bool validation, string? allowFlip = null)
        {
            Dictionary<string, (List<int>, List<int>)> table = [];
            validation = true;
            string suffixGroup = allowFlip != null ?
                $"(?<suffix>((?<noflip>{suffix})|(?<flip>{suffix}{allowFlip})))" :
                $"(?<suffix>(?<noflip>{suffix}))";
            Regex isLabel = new(@"(?<name>[a-zA-Z][a-zA-Z0-9_]*)_" + suffixGroup + ":");
            Regex values = numberTableRegex();
            string? poseName = null;
            int[] vals;
            Match m;
            Match? mSuccess = null;
            for (outLine = line + 1; outLine < content.Length; outLine++)
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if (isDrawInfoLabel(content[outLine]))
                {
                    outLine--;
                    return table;
                }
                m = isLabel.Match(content[outLine]);
                if (m.Success)
                {
                    mSuccess = m;
                    poseName = content[outLine].Split('_')[0];
                    table.TryAdd(poseName, ([], []));
                    continue;
                }
                else if(poseName == null)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"There are values without a Pose Definition"));
                    return table;
                }
                if (!values.IsMatch(content[outLine]))
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine]));
                    return table;
                }
                if (content[outLine][0..2] != "db")
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], "Should use db"));
                    return table;
                }
                vals = HexReader.GetValues(content[outLine]);
                if (suffix == null || mSuccess!.Groups["noflip"].Success)
                    table[poseName!].Item1.AddRange(vals);
                else
                    table[poseName!].Item2.AddRange(vals);
            }
            return table;
        }
        private static string[] pivotDirectives = [
            "XPivot",
            "YPivot"
            ];
        private static string[] pruralDirectives = [
            "TileCodes",
            "Properties",
            "XOffsets",
            "YOffsets",
            "Sizes"
            ];
        private static string[] singularDirectives = [
            "TileCode",
            "Property",
            "XOffset",
            "YOffset",
            "Size"
            ];
        /// <summary>Reads one current-format pose block and expands all declared pose IDs.</summary>
        private IEnumerable<DrawInfo> readDrawInfoCurrent(int offset, int line, string[] content, out int outLine, string path, out bool validation)
        {
            validation = true;

            bool isDynamic = true;

            Match m = drawInfoCurrent().Match(content[line]);
            int start = int.Parse(m.Groups["start"].Value);
            int end = start;
            outLine = line;
            if (m.Groups["end"].Success)
            {
                end = int.Parse(m.Groups["end"].Value);
                if (start > end)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, line, content[line], "Start Pose ID must be lower or equal to End Pose ID"));
                    return [];
                }
            }
            string name = m.Groups["name"].Value;

            outLine++;

            if (outLine >= content.Length)
            {
                validation = false;
                _logRegisterSystem.Add(new SyntaxError(path, line, content[line], "Incomplete Pose Drawing Structure"));
                return [];
            }

            m = numOfTilesRegex().Match(content[outLine]);

            if (!m.Success)
            {
                validation = false;
                _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine]));
                return [];
            }

            int tiles = int.Parse(m.Groups["tiles"].Value);

            outLine++;

            int[] tileCodes = [.. Enumerable.Repeat(0, tiles)];
            int[] props = [.. Enumerable.Repeat(0, tiles)];
            int[] xdisps = [.. Enumerable.Repeat(0, tiles)];
            int[]? xdispsFlip = null;
            int[] ydisps = [.. Enumerable.Repeat(0, tiles)];
            int[]? ydispsFlip = null;
            int[] sizes = [.. Enumerable.Repeat(2, tiles)];

            Regex directives = directiveRegex();
            int? xpivot = null;
            int? ypivot = null;
            int[] vals;

            int tileStart;
            int tileEnd;

            for (; outLine < content.Length; outLine++)
            {
                if (string.IsNullOrWhiteSpace(content[outLine]))
                    continue;
                if (isDrawInfoLabel(content[outLine]))
                {
                    outLine--;
                    break;
                }
                if (content[outLine] == ".IsStatic")
                {
                    isDynamic = false;
                    continue;
                }
                m = directives.Match(content[outLine]);
                if (!m.Success)
                {
                    validation = false;
                    _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine]));
                    return [];
                }
                vals = [..m.Groups["values"].Value
                            .Split(',')
                            .Select(v =>
                            {
                                if(v.StartsWith("$"))
                                    return Convert.ToInt32(v[1..], 16);
                                else
                                    return int.Parse(v);
                            })];
                tileStart = 0;

                if (m.Groups["start"].Success)
                    tileStart = int.Parse(m.Groups["start"].Value);
                tileEnd = tileStart;
                if (m.Groups["end"].Success)
                    tileEnd = int.Parse(m.Groups["end"].Value);
                switch (m.Groups["directives"].Value)
                {
                    case string s when pruralDirectives.Contains(s):
                        if (vals.Length != tiles)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Directive {m.Groups["directives"]} should have exactly the same number of values as the number of tiles ({tiles})"));
                            return [];
                        }
                        if (m.Groups["start"].Success || m.Groups["end"].Success)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Directive {m.Groups["directives"]} should not have a Tile ID or range"));
                            return [];
                        }
                        break;
                    case string s when singularDirectives.Contains(s):
                        if (vals.Length != 1)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Directive {m.Groups["directives"]} should have exactly one value"));
                            return [];
                        }
                        if (!m.Groups["start"].Success)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"You must use a Tile ID or range"));
                            return [];
                        }
                        if (tileStart > tileEnd)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"You can't use a range with {m.Groups["directives"]} directive"));
                            return [];
                        }
                        if (tileStart >= tiles || tileEnd >= tiles)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Tile ID or range exceeds the number of tiles ({tiles})"));
                            return [];
                        }
                        break;
                    case string s when pivotDirectives.Contains(s):
                        if (vals.Length != 1)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Directive {m.Groups["directives"]} should have exactly one value"));
                            return [];
                        }
                        if (m.Groups["start"].Success || m.Groups["end"].Success)
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Directive {m.Groups["directives"]} should not have a Tile ID or range"));
                            return [];
                        }
                        break;
                    default:
                        validation = false;
                        _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Unknown directive {m.Groups["directives"]}"));
                        return [];
                }
                switch (m.Groups["directives"].Value)
                {
                    case "XPivot":
                        xpivot = vals[0];
                        break;
                    case "YPivot":
                        ypivot = vals[0];
                        break;
                    case "TileCodes":
                        tileCodes = vals;
                        break;
                    case "Properties":
                        props = vals;
                        break;
                    case "XOffsets":
                        xdisps = vals;
                        break;
                    case "YOffsets":
                        ydisps = vals;
                        break;
                    case "Sizes":
                        if (vals.Any(v => v != 0 && v != 2))
                        {
                            validation = false;
                            _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Sizes directive only accepts values 0 and 2"));
                            return [];
                        }
                        sizes = vals;
                        break;
                    case "TileCode":
                        for (int i = tileStart; i <= tileEnd; i++)
                            tileCodes[i] = vals[0];
                        break;
                    case "Property":
                        for (int i = tileStart; i <= tileEnd; i++)
                            props[i] = vals[0];
                        break;
                    case "XOffset":
                        for (int i = tileStart; i <= tileEnd; i++)
                            xdisps[i] = vals[0];
                        break;
                    case "YOffset":
                        for (int i = tileStart; i <= tileEnd; i++)
                            ydisps[i] = vals[0];
                        break;
                    case "Size":
                        for (int i = tileStart; i <= tileEnd; i++)
                        {
                            if (vals[0] != 0 && vals[0] != 2)
                            {
                                validation = false;
                                _logRegisterSystem.Add(new SyntaxError(path, outLine, content[outLine], $"Size directive only accepts values 0 and 2"));
                                return [];
                            }
                            sizes[i] = vals[0];
                        }
                        break;
                }
            }
            if (xpivot != null)
            {
                xdispsFlip = new int[tiles];
                for (int i = 0; i < tiles; i++)
                {
                    xdispsFlip[i] = xpivot.Value - 8 - xdisps[i];
                    if (sizes[i] == 0)
                        xdispsFlip[i] += 8;
                    xdispsFlip[i] = (byte)xdispsFlip[i];
                }
            }
            if (ypivot != null)
            {
                ydispsFlip = new int[tiles];
                for (int i = 0; i < tiles; i++)
                {
                    ydispsFlip[i] = ypivot.Value - 8 - ydisps[i];
                    if (sizes[i] == 0)
                        ydispsFlip[i] += 8;
                    ydispsFlip[i] = (byte)ydispsFlip[i];
                }
            }
            List<DrawInfo> dis = [];
            for (int i = start; i <= end; i++, offset++)
            {
                dis.Add(new DrawInfo(offset, AsarSymbolName.FromPath(path), $"{name}{i}")
                {
                    IsDynamic = isDynamic,
                    Tiles = tileCodes,
                    Properties = props,
                    XDisplacements = xdisps,
                    YDisplacements = ydisps,
                    FlipXDisplacements = xdispsFlip,
                    FlipYDisplacements = ydispsFlip,
                    Sizes = sizes
                });
            }
            return dis;
        }

        /// <summary>Creates the parser used to collapse runs of whitespace.</summary>
        [GeneratedRegex(@"\s+")]
        private static partial Regex spaceRegex();
        /// <summary>Creates the parser used to remove semicolon comments.</summary>
        [GeneratedRegex(@";.*")]
        private static partial Regex commentRegex();
        /// <summary>Creates the recognizer for legacy DynamicInfo pose-chunk labels.</summary>
        [GeneratedRegex(@"[a-zA-Z_][a-zA-Z0-9_]*\d+_PoseChunksSizes:")]
        private static partial Regex dynInfoLegacy();
        /// <summary>Creates the parser for Asar byte, word, and long data rows.</summary>
        [GeneratedRegex(@"(db (\$[a-fA-F0-9]{2}|[0-9]+)(,(\$[a-fA-F0-9]{2}|[0-9]+))*|dw (\$[a-fA-F0-9]{4}|[0-9]+)(,(\$[a-fA-F0-9]{4}|[0-9]+))*|dl (\$[a-fA-F0-9]{6}|[0-9]+)(,(\$[a-fA-F0-9]{6}|[0-9]+))*)")]
        private static partial Regex numberTableRegex();
        /// <summary>Creates the parser for current DynamicInfo pose ranges and tile counts.</summary>
        [GeneratedRegex(@"^\.Pose(?<start>\d+)(?:\.\.(?<end>\d+))? (?<tiles>\d+)(?<modifier>q3|h|q)?$")]
        private static partial Regex dynInfoCurrent();
        /// <summary>Creates the parser for current DrawInfo pose-block labels.</summary>
        [GeneratedRegex(@"^(?<name>[a-zA-Z][a-zA-Z0-9]*)_Pose(?<start>\d+)(?:\.\.(?<end>\d+))?:$")]
        private static partial Regex drawInfoCurrent();
        /// <summary>Creates the parser for current DrawInfo tile-count directives.</summary>
        [GeneratedRegex(@"^\.NumberOfTiles: (?<tiles>\d+)$")]
        private static partial Regex numOfTilesRegex();
        /// <summary>Creates the parser for current DrawInfo table directives and optional tile ranges.</summary>
        [GeneratedRegex(@"^\.(?<directives>[a-zA-Z]+)((?<start>\d+)(\.\.(?<end>\d+))?)?:? (?<values>(\$[a-fA-F0-9]{2}|\d+)(,(\$[a-fA-F0-9]{2}|\d+))*)$")]
        private static partial Regex directiveRegex();
    }
}
