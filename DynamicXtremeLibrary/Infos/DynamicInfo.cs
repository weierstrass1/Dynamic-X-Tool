using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.ResourceManagement;
using LogRegister;
using System.IO;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.Infos
{
    /// <summary>Describes dynamic poses, palettes, and binary resources owned by one logical context.</summary>
    public partial class DynamicInfo
    {
        /// <summary>Gets or sets the logical context used by generated symbols.</summary>
        public string ContextName { get; set; }
        /// <summary>Gets or sets upper and lower chunk counts for every pose.</summary>
        public int[]? PosesChunksSizes { get; set; }
        /// <summary>Gets or sets encoded last-row metadata for every pose.</summary>
        public int[]? PosesLastRow { get; set; }
        /// <summary>Gets or sets active graphics files containing pose data.</summary>
        public string[]? PoseGraphics { get; set; }
        /// <summary>Gets or sets active palette resource paths.</summary>
        public string[]? Palettes { get; set; }
        /// <summary>Gets or sets active general-resource paths.</summary>
        public string[]? Resources { get; set; }
        /// <summary>
        /// Gets the pose-graphics declarations exactly as parsed, before legacy conversion or other
        /// processing changes the active resource list.
        /// </summary>
        public string[]? DeclaredPoseGraphics { get; internal set; }
        /// <summary>Gets the palette declarations exactly as parsed from the source file.</summary>
        public string[]? DeclaredPalettes { get; internal set; }
        /// <summary>Gets the general-resource declarations exactly as parsed from the source file.</summary>
        public string[]? DeclaredResources { get; internal set; }
        /// <summary>Gets or sets the base directory used to resolve relative resource paths.</summary>
        public string ResourceDirectory { get; set; } = "DynamicResources";
        /// <summary>Gets or sets the directory used for graphics generated during conversion.</summary>
        public string TemporaryDirectory { get; set; } = "DynamicResources";
        /// <summary>Gets the suffix appended to generated resource symbols after collision resolution.</summary>
        public string SymbolSuffix { get; private set; } = "";
        /// <summary>Gets the number of declared poses.</summary>
        public int PoseLength => PosesChunksSizes == null ? 0 : PosesChunksSizes.Length / 2;
        /// <summary>Gets the number of active palettes.</summary>
        public int PaletteLength => Palettes == null ? 0 : Palettes.Length;
        /// <summary>Gets the number of active general resources.</summary>
        public int ResourcesLength => Resources == null ? 0 : Resources.Length;
        /// <summary>Initializes a DynamicInfo with its source context name.</summary>
        public DynamicInfo(string contextName) 
        {
            ContextName = contextName;
        }
        /// <summary>
        /// Applies the resolved context name and derives the suffix used by all resources owned by
        /// this DynamicInfo.
        /// </summary>
        /// <param name="originalName">The normalized name derived from the source filename.</param>
        /// <param name="logicalName">The final collision-free context name.</param>
        public void SetLogicalName(string originalName, string logicalName)
        {
            ContextName = logicalName;
            SymbolSuffix = logicalName.StartsWith(originalName, StringComparison.Ordinal)
                ? logicalName[originalName.Length..]
                : logicalName;
        }
        /// <summary>
        /// Resolves a resource path against this DynamicInfo's resource directory while preserving
        /// absolute paths.
        /// </summary>
        /// <param name="path">The declared resource path.</param>
        /// <returns>The path used to access the resource.</returns>
        public string ResolveResourcePath(string path)
            => Path.IsPathRooted(path) ? path : Path.Combine(ResourceDirectory, path);

        /// <summary>Builds the final generated name of a pose-graphics resource.</summary>
        /// <param name="path">The declared pose-graphics path.</param>
        /// <returns>The normalized name with this DynamicInfo's resolved suffix.</returns>
        public string GetPoseGraphicName(string path)
            => $"{AsarSymbolName.FromPath(path)}{SymbolSuffix}";

        /// <summary>Builds a pose-graphics resource name using an explicit logical suffix.</summary>
        /// <param name="path">The declared pose-graphics path.</param>
        /// <param name="suffix">The suffix to append after normalization.</param>
        /// <returns>The normalized name with the supplied suffix.</returns>
        public string GetPoseGraphicName(string path, string suffix)
            => $"{AsarSymbolName.FromPath(path)}{suffix}";

        /// <summary>Builds the final generated name of a palette resource.</summary>
        /// <param name="path">The declared palette path.</param>
        /// <returns>The normalized name with this DynamicInfo's resolved suffix.</returns>
        public string GetPaletteName(string path)
            => $"{AsarSymbolName.FromPath(path)}{SymbolSuffix}";

        /// <summary>Builds a palette resource name using an explicit logical suffix.</summary>
        /// <param name="path">The declared palette path.</param>
        /// <param name="suffix">The suffix to append after normalization.</param>
        /// <returns>The normalized name with the supplied suffix.</returns>
        public string GetPaletteName(string path, string suffix)
            => $"{AsarSymbolName.FromPath(path)}{suffix}";

        /// <summary>Builds the final generated name of a general resource.</summary>
        /// <param name="path">The declared general-resource path.</param>
        /// <returns>The normalized name with this DynamicInfo's resolved suffix.</returns>
        public string GetResourceName(string path)
            => $"{AsarSymbolName.FromPath(path)}{SymbolSuffix}";

        /// <summary>Builds a general-resource name using an explicit logical suffix.</summary>
        /// <param name="path">The declared general-resource path.</param>
        /// <param name="suffix">The suffix to append after normalization.</param>
        /// <returns>The normalized name with the supplied suffix.</returns>
        public string GetResourceName(string path, string suffix)
            => $"{AsarSymbolName.FromPath(path)}{suffix}";

        /// <summary>
        /// Reconstructs the generated pose-resource names by distributing pose chunks across their
        /// declared graphics files using each file's byte length.
        /// </summary>
        /// <param name="suffix">The logical suffix to apply to every generated pose name.</param>
        /// <returns>The pose-resource names in the same order used by ROM generation.</returns>
        public IReadOnlyList<string> GetPoseResourceNames(string suffix)
        {
            if (PoseGraphics == null || PoseGraphics.Length == 0 || PosesChunksSizes == null)
                return [];
            int[] lengths = [.. PoseGraphics.Select(path => checked((int)new FileInfo(ResolveResourcePath(path)).Length))];
            List<string> names = [];
            int graphicIndex = 0;
            int indexInGraphic = 0;
            int bytesInGraphic = 0;
            for (int index = 0; index < PosesChunksSizes.Length; index += 2)
            {
                int poseLength = (PosesChunksSizes[index] + PosesChunksSizes[index + 1]) * 32;
                bytesInGraphic += poseLength;
                if (bytesInGraphic > lengths[graphicIndex])
                {
                    graphicIndex++;
                    indexInGraphic = 0;
                    bytesInGraphic = poseLength;
                }
                names.Add($"{GetPoseGraphicName(PoseGraphics[graphicIndex], suffix)}{indexInGraphic:D3}");
                indexInGraphic++;
            }
            return names;
        }
        /// <summary>Validates declared files, sizes, chunk counts, and pose boundaries.</summary>
        public bool Validate(string resourceDirectory, LogRegisterSystem logging)
        {
            bool result = true;
            string path;
            if (Palettes != null)
            {
                for (int i = 0; i < Palettes.Length; i++)
                {
                    path = Path.IsPathRooted(Palettes[i])
                        ? Palettes[i]
                        : Path.Combine(resourceDirectory, Palettes[i]);
                    if (!File.Exists(path))
                    {
                        logging.Add(new ResourceNotFound(Palettes[i]));
                        result = false;
                        continue;
                    }
                    if (new FileInfo(path).Length > 32760)
                    {
                        logging.Add(new FileIsToBig(Palettes[i], new FileInfo(path).Length, 32760));
                        result = false;
                    }
                }
            }
            if(Resources != null)
            {
                for (int i = 0; i < Resources.Length; i++)
                {
                    path = Path.IsPathRooted(Resources[i])
                        ? Resources[i]
                        : Path.Combine(resourceDirectory, Resources[i]);
                    if (!File.Exists(path))
                    {
                        logging.Add(new ResourceNotFound(Resources[i]));
                        result = false;
                        continue;
                    }
                    if (new FileInfo(path).Length > 32760)
                    {
                        logging.Add(new FileIsToBig(Resources[i], new FileInfo(path).Length, 32760));
                        result = false;
                    }
                }
            }
            if (PoseGraphics == null || PoseGraphics.Length == 0)
                return result;
            long totalSize = 0;
           
            for (int i = 0; i < PoseGraphics.Length; i++)
            {
                path = Path.IsPathRooted(PoseGraphics[i])
                    ? PoseGraphics[i]
                    : Path.Combine(resourceDirectory, PoseGraphics[i]);
                if (!File.Exists(path))
                {
                    logging.Add(new ResourceNotFound(PoseGraphics[i]));
                    result = false;
                    continue;
                }
                totalSize += new FileInfo(path).Length;
            }
            if (PosesChunksSizes == null || PosesChunksSizes.Length == 0)
            {
                logging.Add(new DynamicInfoWithoutChunks(ContextName));
                result = false;
                return result;
            }
            if (!ValidatePoseChunks(logging))
                return false;

            long totalSizeFromResSizes = 0;
            foreach (var size in PosesChunksSizes)
                totalSizeFromResSizes += size;
            totalSizeFromResSizes *= 32;
            if (totalSize != totalSizeFromResSizes )
            {
                logging.Add(new DynamicInfoSizeMismatch(ContextName, totalSize, totalSizeFromResSizes));
                result = false;
            }

            return result;
        }
        /// <summary>Validates the structure and VRAM layout represented by every pose-chunk pair.</summary>
        private bool ValidatePoseChunks(LogRegisterSystem logging)
        {
            if (PosesChunksSizes!.Length % 2 != 0)
            {
                logging.Add(new DynamicInfoInvalidChunks(ContextName,
                    "the chunk list must contain complete pairs"));
                return false;
            }

            for (int i = 0; i < PosesChunksSizes.Length; i += 2)
            {
                int first = PosesChunksSizes[i];
                int second = PosesChunksSizes[i + 1];

                if (first < 0 || second < 0)
                {
                    logging.Add(new DynamicInfoInvalidChunks(ContextName,
                        $"pose {i / 2} contains a negative chunk size"));
                    return false;
                }

                if (second == 0)
                    continue;

                int firstChunkLastRow = first % 32;
                if (firstChunkLastRow == 0 || firstChunkLastRow > 16 ||
                    second > firstChunkLastRow)
                {
                    logging.Add(new DynamicInfoInvalidChunks(ContextName,
                        $"pose {i / 2} has an invalid chunk layout ({first}, {second})"));
                    return false;
                }
            }

            return true;
        }
        /// <summary>Collects pose, palette, and general resources from all DynamicInfo definitions.</summary>
        public static IReadOnlyList<Resource> GetAllResources(IEnumerable<DynamicInfo> dis)
        {
            var boolOpts = Options.Instance.BoolOptions.ToDictionary(o => o.Name, o => o.Value);
            List<Resource> result = [];

            if (boolOpts["DynamicPoses"])
                result.AddRange(GetPosesData(dis));
            if (boolOpts["PalettesChange"])
                result.AddRange(GetPaletteData(dis));
            if (boolOpts["GraphicsChange"] || boolOpts["PalettesChange"])
                result.AddRange(GetResourceData(dis));
            return result.AsReadOnly();
        }
        /// <summary>Collects dynamic-pose resources while assigning consecutive IDs.</summary>
        public static IReadOnlyList<Resource> GetPosesData(IEnumerable<DynamicInfo> dis)
        {
            List<Resource> poses = [];
            IEnumerable<Resource> currentPoses;
            int i = 0;
            foreach (var di in dis)
            {
                currentPoses = di.GetPosesData(i);
                poses.AddRange(currentPoses);
                i += currentPoses.Count();
            }
            return poses;
        }
        /// <summary>Collects unique palette resources while assigning consecutive IDs.</summary>
        public static IReadOnlyList<Resource> GetPaletteData(IEnumerable<DynamicInfo> dis)
        {
            Dictionary<string, Resource> pals = [];
            int i = 0;
            IReadOnlyDictionary<string, Resource> currentPalettes;
            foreach (var di in dis)
            {
                currentPalettes = di.GetPaletteData(i, pals.AsReadOnly());
                pals = pals.Concat(currentPalettes).ToDictionary(x => x.Key, x => x.Value);
                i += currentPalettes.Count;
            }
            return pals.Values.ToList().AsReadOnly();
        }
        /// <summary>Collects unique general resources while assigning consecutive IDs.</summary>
        public static IReadOnlyList<Resource> GetResourceData(IEnumerable<DynamicInfo> dis)
        {
            Dictionary<string, Resource> res = [];
            int i = 0;
            IReadOnlyDictionary<string, Resource> currentResources;
            foreach (var di in dis)
            {
                currentResources = di.GetResourceData(i, res.AsReadOnly());
                res = res.Concat(currentResources).ToDictionary(x => x.Key, x => x.Value);
                i += currentResources.Count;
            }
            return res.Values.ToList().AsReadOnly();
        }
        /// <summary>Loads palettes not already present and assigns IDs beginning at an offset.</summary>
        public IReadOnlyDictionary<string, Resource> GetPaletteData(int idOffset, IReadOnlyDictionary<string, Resource> currentPalettes)
        {
            if (Palettes == null || Palettes.Length == 0)
                return new Dictionary<string, Resource>().AsReadOnly();
            byte[] b;
            Dictionary<string, Resource> result = [];
            int i = idOffset;
            foreach (var path in Palettes)
            {
                string name = GetPaletteName(path);
                if(result.ContainsKey(name) || currentPalettes.ContainsKey(name))
                    continue;
                b = File.ReadAllBytes(ResolveResourcePath(path));
                result.Add(name, new(i, name,
                    ResourceType.Palette, b));
                i++;
            }
            return result;
        }
        /// <summary>Loads general resources not already present and assigns IDs beginning at an offset.</summary>
        public IReadOnlyDictionary<string, Resource> GetResourceData(int idOffset, IReadOnlyDictionary<string, Resource> currentResources)
        {
            if(Resources == null || Resources.Length == 0)
                return new Dictionary<string, Resource>().AsReadOnly();
            byte[] b;
            Dictionary<string, Resource> result = [];
            int i = idOffset;
            foreach (var path in Resources)
            {
                string name = GetResourceName(path);
                if(currentResources.ContainsKey(name) || result.ContainsKey(name))
                    continue;
                b = File.ReadAllBytes(ResolveResourcePath(path));
                result.Add(name, new(i, name,
                    ResourceType.GeneralResource, b));
                i++;
            }
            return result.AsReadOnly();
        }
        /// <summary>Splits declared graphics into individual dynamic-pose resources.</summary>
        public IReadOnlyList<Resource> GetPosesData(int idOffset)
        {
            if (PoseGraphics == null || PoseGraphics.Length == 0)
                return [];
            if (PosesChunksSizes == null)
                return [];

            List<Resource> poses = [];

            int totalLength = 0;
            foreach (var value in PosesChunksSizes)
            {
                totalLength += value;
            }
            totalLength *= 32;
            byte[] wholeGFX = new byte[totalLength];
            int[] lens = new int[PoseGraphics.Length];
            byte[] b;
            int index = 0;
            int i = 0;
            foreach (var path in PoseGraphics)
            {
                b = File.ReadAllBytes(ResolveResourcePath(path));
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
            for (i = 0; i < PosesChunksSizes.Length; i += 2)
            {
                newVal += (PosesChunksSizes[i] + PosesChunksSizes[i + 1]) * 32;
                b = wholeGFX[index..newVal];
                size += b.Length;
                if(size > lens[gfxInd])
                {
                    gfxInd++;
                    fInd = 0;
                    size = b.Length;
                }
                poses.Add(new(idOffset + poses.Count,
                    $"{GetPoseGraphicName(PoseGraphics[gfxInd])}{fInd:D3}",
                    ResourceType.DynamicPose, b));
                fInd++;
                index = newVal;
            }
            return poses.AsReadOnly();
        }
        /// <summary>Gets the byte size of every pose in this DynamicInfo.</summary>
        public int[] GetPosesSizes()
        {
            if (PosesChunksSizes == null)
                return [];
            int[] res = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseSize(i);
            return res;
        }
        /// <summary>Calculates one pose's byte size from its upper and lower chunk counts.</summary>
        public int GetPoseSize(int id)
        {
            int id2 = id * 2;
            return PosesChunksSizes == null ? 
                        -1 :
                        id2 >= PosesChunksSizes.Length || id2 < 0 ?
                            -1 : 
                            32 * (PosesChunksSizes[id2] + PosesChunksSizes[id2 + 1]);
        }
        /// <summary>Gets the VRAM block count required by every pose.</summary>
        public int[] GetPosesBlocks()
        {
            if( PosesChunksSizes == null )
                return [];
            int[] res = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < res.Length; i++)
                res[i] = GetPoseBlocks(i);
            return res;
        }
        /// <summary>Calculates one pose's VRAM block count.</summary>
        public int GetPoseBlocks(int id)
        {
            int id2 = id * 2;
            if (PosesChunksSizes == null || id2 >= PosesChunksSizes.Length || id2 < 0)
                return -1;
            int baseBlocks = PosesChunksSizes[id2] / 32;
            baseBlocks *= 8;
            int lastRowBlock = Math.Max(PosesChunksSizes[id2] % 32, PosesChunksSizes[id2 + 1]);
            if(lastRowBlock <= 16)
            {
                lastRowBlock += lastRowBlock % 2;
                lastRowBlock /= 2;
                return baseBlocks + lastRowBlock;
            }
            return baseBlocks + 8;
        }
        /// <summary>Flattens pose chunk-size pairs from multiple DynamicInfo definitions.</summary>
        public static int[] GetSizes(IEnumerable<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                if (di.PosesChunksSizes != null)
                    res.AddRange(di.PosesChunksSizes!.Select(x => x * 32).ToList());
            return [.. res];
        }
        /// <summary>Gets or derives the last-row metadata for this DynamicInfo.</summary>
        public int[] GetLastRow()
        {
            if(PosesLastRow == null || PosesChunksSizes == null)
                return [];
            if (PosesLastRow.Length == PosesChunksSizes.Length / 2)
                return [.. PosesLastRow.Select(x => x*16)];
            int[] lr = new int[PosesChunksSizes.Length / 2];
            for (int i = 0; i < lr.Length; i++)
                lr[i] = PosesLastRow[0] * 16;
            return lr;
        }
        /// <summary>Flattens last-row metadata from multiple DynamicInfo definitions.</summary>
        public static int[] GetLastRow(IEnumerable<DynamicInfo> dis)
        {
            List<int> res = new();
            foreach (var di in dis)
                res.AddRange(di.GetLastRow());
            return [.. res];
        }
        /// <summary>Derives last-row metadata from each pose's chunk sizes.</summary>
        public void GenerateLastRow()
        {
            if (PosesChunksSizes == null || PosesChunksSizes.Length <= 0)
                return;
            PosesLastRow = new int[PosesChunksSizes.Length / 2];
            int val;
            for (int i = 0; i < PosesChunksSizes.Length; i += 2) 
            {
                val = PosesChunksSizes[i] / 32;
                val *= 2;
                val++;
                val *= 16;
                PosesLastRow[i / 2] = val;
            }
        }
        /// <summary>Converts legacy tile-count declarations into chunk tables and generated graphics.</summary>
        public void FromNumberOf16x16Tiles(IDictionary<int, string> numberOf16x16TilesPerPose)
        {
            if(PoseGraphics == null || PoseGraphics.Length == 0)
                return;

            List<(long, byte[])> bs = [];

            long count = 0;
            long tiles = 0;
            long gtiles;
            string file;
            byte[] bytes;
            foreach(var pg in  PoseGraphics) 
            {
                file = ResolveResourcePath(pg);
                if (!File.Exists(file))
                    return;
                bytes = File.ReadAllBytes(file);
                if (bytes.Length % 1024 != 0)
                    return;
                count += bytes.Length;
                gtiles = getNumberOfTilesPerGraphic(file);
                tiles += gtiles;
                bs.Add((gtiles, bytes));
            }

            List<byte[]> poses = [];
            List<int> posechunks = [];

            Regex modifier = tilesRegex();
            Match m;
            int totalTiles = 0;
            int completeTiles;
            int reservedTiles;
            int v1, v2;
            int remainder;
            string mod;

            IEnumerator<(long, byte[])> en = bs.GetEnumerator();
            en.Reset();
            en.MoveNext();
            long bufTiles;
            byte[] buffBytes;
            long bytePointer;
            long grPointer = 0;
            int rows;
            long grPointerBytes = 0;
            long endV1;
            long endV2;
            long lastRowAdder = 0;
            long endTiles;
            List<byte> result = [];

            var tilesValues = numberOf16x16TilesPerPose
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value).ToList();

            foreach (var numOfTiles in tilesValues)
            {
                m = modifier.Match(numOfTiles);
                completeTiles = int.Parse(m.Groups["tiles"].Value);
                reservedTiles = completeTiles;
                v1 = completeTiles / 8;
                v1 *= 32;
                remainder = completeTiles % 8;
                v2 = remainder * 2;
                v1 += v2;
                if (m.Groups["modifier"].Success)
                {
                    mod = m.Groups["modifier"].Value;
                    reservedTiles++;
                    v1 += mod switch
                    {
                        "q3" => 2,
                        "h" => 2,
                        "q" => 1,
                        _ => 0
                    };
                    v2 += mod switch
                    {
                        "q3" => 1,
                        _ => 0
                    };
                }
                totalTiles += reservedTiles;
                if (totalTiles > tiles)
                    return;

                bytes = new byte[(v1 + v2) * 32];
                bytePointer = 0;

                rows = completeTiles / 8;
                rows *= 8;
                endV1 = v1 * 32;
                endV2 = (v1 + v2) * 32;
                lastRowAdder = endV1;

                for (int i = 0; i < reservedTiles; i++)
                {
                    (bufTiles, buffBytes) = en.Current;
                    if (grPointer >= bufTiles)
                    {
                        en.MoveNext();
                        grPointer = 0;
                        grPointerBytes = 0;
                    }

                    if (i < rows)
                    {
                        Array.Copy(buffBytes, grPointerBytes, bytes, bytePointer, 64);
                        Array.Copy(buffBytes, grPointerBytes + 512, bytes, bytePointer + 512, 64);
                    }
                    else
                    {
                        Array.Copy(buffBytes, grPointerBytes, bytes, bytePointer, endV1 - bytePointer);
                        Array.Copy(buffBytes, grPointerBytes + 512, bytes, lastRowAdder, endV2 - lastRowAdder);
                        endTiles = (63 + endV1 - bytePointer) / 64;
                        grPointer += endTiles;
                        for (int j = 0; j < endTiles; j++)
                        {
                            grPointerBytes += 64;
                            if (grPointerBytes % 512 == 0)
                                grPointerBytes += 512;
                        }
                        break;
                    }
                    bytePointer += 64;
                    if (bytePointer % 512 == 0)
                        bytePointer += 512;
                    grPointerBytes += 64;
                    if (grPointerBytes % 512 == 0)
                        grPointerBytes += 512;
                    grPointer++;
                }
                result.AddRange(bytes);

                if (v2 > 0 && v1 % 32 == 16)
                {
                    v1 += v2;
                    v2 = 0;
                }
                posechunks.Add(v1);
                posechunks.Add(v2);
            }

            string generatedGraphics = Path.GetFullPath(
                Path.Combine(TemporaryDirectory, $"{ContextName}.bin.tmp"));
            PoseGraphics = [generatedGraphics];
            PosesChunksSizes = [.. posechunks];
            Directory.CreateDirectory(TemporaryDirectory);
            File.WriteAllBytes(generatedGraphics, [.. result]);
            
            GenerateLastRow();
        }
        /// <summary>Gets the number of 16x16 tiles represented by one declared graphics file.</summary>
        private long getNumberOfTilesPerGraphic(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);

            long end = fs.Length - 512;

            while (end > 0)
            {
                fs.Position = end - 1;
                if (fs.ReadByte() != 0)
                    break;
                end--;
            }

            long tiles = end / 1024 * 8;
            long r = end % 1024;

            long remainder = (r + 63) / 64;

            return tiles + remainder;
        }

        /// <summary>Creates the parser for legacy tile-count expressions and modifiers.</summary>
        [GeneratedRegex("(?<tiles>\\d+)(?<modifier>(q3|h|q))?")]
        private static partial Regex tilesRegex();
    }
}
