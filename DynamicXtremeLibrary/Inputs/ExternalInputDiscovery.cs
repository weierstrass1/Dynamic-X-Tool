using DynamicXtremeLibrary.Defines;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Discovers traditional Dynamic Xtreme inputs and inputs referenced by primary PIXI and
    /// UberASMTool ASM files.
    /// </summary>
    public static partial class ExternalInputDiscovery
    {
        /// <summary>
        /// Recursively discovers traditional inputs, resolves the consumers declared by external
        /// tool lists, and reads Dynamic Xtreme include directives from their primary ASM files.
        /// </summary>
        /// <param name="dynamicInfoDirectory">The directory containing traditional DynamicInfo inputs.</param>
        /// <param name="drawInfoDirectory">The directory containing traditional DrawInfo inputs.</param>
        /// <param name="paletteEffectsDirectory">The directory containing traditional palette-effect inputs.</param>
        /// <param name="dynamicResourcesDirectory">The resource directory used by traditional DynamicInfo inputs.</param>
        /// <param name="pixiListPath">The optional PIXI list to inspect.</param>
        /// <param name="pixiExecutablePath">The optional PIXI executable used to locate its installation directory.</param>
        /// <param name="uberasmListPath">The optional UberASMTool list to inspect.</param>
        /// <param name="uberasmExecutablePath">The optional UberASMTool executable used to locate its installation directory.</param>
        /// <returns>Inputs ordered with traditional files first and external files in discovery order.</returns>
        public static IReadOnlyList<InfoInput> Discover(string dynamicInfoDirectory,
            string drawInfoDirectory, string paletteEffectsDirectory, string dynamicResourcesDirectory,
            string? pixiListPath, string? pixiExecutablePath, string? uberasmListPath,
            string? uberasmExecutablePath)
        {
            Dictionary<(InfoKind Kind, string Path), InfoInput> inputs = new();
            int discoveryOrder = 0;

            addTraditional(inputs, dynamicInfoDirectory, "*.dynamicinfo",
                InfoKind.DynamicInfo, dynamicResourcesDirectory, ref discoveryOrder);
            addTraditional(inputs, drawInfoDirectory, "*.drawinfo",
                InfoKind.DrawInfo, drawInfoDirectory, ref discoveryOrder);
            addTraditional(inputs, paletteEffectsDirectory, "*.paleffect",
                InfoKind.PaletteEffect, paletteEffectsDirectory, ref discoveryOrder);

            foreach (var consumer in readPixiConsumers(pixiListPath, pixiExecutablePath)
                .Concat(readUberasmConsumers(uberasmListPath, uberasmExecutablePath)))
            {
                readDirectives(consumer, inputs, ref discoveryOrder);
            }

            return
            [
                .. inputs.Values.Where(input => input.IsTraditional)
                                .OrderBy(input => input.Path, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(input => input.Path, StringComparer.Ordinal)
,
                .. inputs.Values.Where(input => !input.IsTraditional)
                        .OrderBy(input => input.DiscoveryOrder),
            ];
        }

        /// <summary>
        /// Recursively adds traditional inputs of one kind and marks shared physical files as
        /// traditional without discarding origins discovered elsewhere.
        /// </summary>
        /// <param name="inputs">The physical-input index being assembled.</param>
        /// <param name="directory">The traditional directory to search recursively.</param>
        /// <param name="pattern">The file pattern associated with the input kind.</param>
        /// <param name="kind">The semantic kind assigned to matching files.</param>
        /// <param name="resourceDirectory">The resource base used by the discovered files.</param>
        /// <param name="discoveryOrder">The next stable discovery index.</param>
        private static void addTraditional(IDictionary<(InfoKind Kind, string Path), InfoInput> inputs,
            string directory, string pattern, InfoKind kind, string resourceDirectory, 
            ref int discoveryOrder)
        {
            if (!Directory.Exists(directory))
                return;
            foreach (string path in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => path, StringComparer.Ordinal))
            {
                string fullPath = Path.GetFullPath(path);
                var key = (kind, pathKey(fullPath));
                if (!inputs.TryGetValue(key, out InfoInput? input))
                {
                    input = createInput(kind, fullPath, resourceDirectory, discoveryOrder++);
                    inputs.Add(key, input);
                }
                input.IsTraditional = true;
            }
        }

        /// <summary>
        /// Creates the physical input model after verifying that its definition file exists.
        /// </summary>
        /// <param name="kind">The semantic input kind.</param>
        /// <param name="path">The definition file path.</param>
        /// <param name="resourceDirectory">The base directory for referenced resources.</param>
        /// <param name="discoveryOrder">The stable discovery index assigned to the input.</param>
        /// <returns>A normalized physical input model.</returns>
        private static InfoInput createInput(InfoKind kind, string path, string resourceDirectory,
            int discoveryOrder)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Included {kind} file was not found", path);
            return new()
            {
                Kind = kind,
                Path = Path.GetFullPath(path),
                ResourceDirectory = Path.GetFullPath(resourceDirectory),
                OriginalName = AsarSymbolName.FromPath(path),
                LogicalName = AsarSymbolName.FromPath(path),
                DiscoveryOrder = discoveryOrder
            };
        }

        /// <summary>
        /// Reads supported Dynamic Xtreme directives from one primary ASM consumer, resolves paths
        /// relative to that ASM file, and records the consumer origin on each included input.
        /// </summary>
        /// <param name="consumer">The external-tool ASM consumer to inspect.</param>
        /// <param name="inputs">The physical-input index being assembled.</param>
        /// <param name="discoveryOrder">The next stable discovery index.</param>
        private static void readDirectives(Consumer consumer,
            IDictionary<(InfoKind Kind, string Path), InfoInput> inputs, ref int discoveryOrder)
        {
            if (!File.Exists(consumer.AsmPath))
                throw new FileNotFoundException("Primary ASM file from list was not found", consumer.AsmPath);

            string[] lines = File.ReadAllLines(consumer.AsmPath);
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (!lines[lineIndex].TrimStart().StartsWith(";#inc", StringComparison.OrdinalIgnoreCase))
                    continue;
                Match match = directiveRegex().Match(lines[lineIndex]);
                if (!match.Success)
                    throw new InvalidDataException(
                        $"Invalid Dynamic Xtreme include directive in '{consumer.AsmPath}', line {lineIndex + 1}");

                InfoKind kind = match.Groups["kind"].Value.ToLowerInvariant() switch
                {
                    "dyn" => InfoKind.DynamicInfo,
                    "dri" => InfoKind.DrawInfo,
                    "pale" => InfoKind.PaletteEffect,
                    _ => throw new InvalidDataException("Unknown Dynamic Xtreme include directive")
                };
                string extension = kind switch
                {
                    InfoKind.DynamicInfo => ".dynamicinfo",
                    InfoKind.DrawInfo => ".drawinfo",
                    _ => ".paleffect"
                };
                string path = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(consumer.AsmPath)!, match.Groups["path"].Value));
                if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(
                        $"Directive in '{consumer.AsmPath}', line {lineIndex + 1} must target a '{extension}' file");

                var key = (kind, pathKey(path));
                if (!inputs.TryGetValue(key, out InfoInput? input))
                {
                    string resourceDirectory = kind == InfoKind.DynamicInfo
                        ? Path.GetDirectoryName(path)!
                        : Path.GetDirectoryName(path)!;
                    input = createInput(kind, path, resourceDirectory, discoveryOrder++);
                    inputs.Add(key, input);
                }
                input.Origins.Add(new()
                {
                    AsmPath = consumer.AsmPath,
                    ConsumerKind = consumer.Kind,
                    ConsumerName = consumer.Name,
                    DirectiveLine = lineIndex,
                    PreviousLogicalName = match.Groups["logical"].Success
                        ? match.Groups["logical"].Value
                        : null
                });
            }
        }

        /// <summary>
        /// Enumerates primary ASM consumers referenced by a PIXI list while tracking the active
        /// sprite section.
        /// </summary>
        /// <param name="listPath">The optional PIXI list path.</param>
        /// <param name="executablePath">The optional PIXI executable or installation path.</param>
        /// <returns>The PIXI ASM consumers in list order.</returns>
        private static IEnumerable<Consumer> readPixiConsumers(string? listPath, string? executablePath)
        {
            if (string.IsNullOrWhiteSpace(listPath))
                yield break;
            string fullListPath = Path.GetFullPath(listPath);
            if (!File.Exists(fullListPath))
                throw new FileNotFoundException("PIXI list was not found", fullListPath);

            string root = getToolRoot(fullListPath, executablePath);
            ConsumerKind kind = ConsumerKind.Sprite;
            foreach (string originalLine in File.ReadLines(fullListPath))
            {
                string line = stripListComment(originalLine);
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (tryPixiSection(line, out ConsumerKind sectionKind))
                {
                    kind = sectionKind;
                    continue;
                }

                Match pathMatch = listPathRegex().Match(line);
                if (!pathMatch.Success)
                    continue;
                string listedPath = pathMatch.Groups["quoted"].Success
                    ? pathMatch.Groups["quoted"].Value
                    : pathMatch.Groups["plain"].Value;
                string entryPath = resolveListedPath(root, pixiSubdirectory(kind), listedPath);
                string asmPath = resolvePixiAsm(entryPath);
                yield return new()
                {
                    AsmPath = asmPath,
                    Kind = kind,
                    Name = AsarSymbolName.FromPath(entryPath)
                };
            }
        }

        /// <summary>
        /// Enumerates primary ASM consumers referenced by an UberASMTool list while tracking the
        /// active code section.
        /// </summary>
        /// <param name="listPath">The optional UberASMTool list path.</param>
        /// <param name="executablePath">The optional UberASMTool executable or installation path.</param>
        /// <returns>The UberASMTool ASM consumers in list order.</returns>
        private static IEnumerable<Consumer> readUberasmConsumers(string? listPath, string? executablePath)
        {
            if (string.IsNullOrWhiteSpace(listPath))
                yield break;
            string fullListPath = Path.GetFullPath(listPath);
            if (!File.Exists(fullListPath))
                throw new FileNotFoundException("UberASMTool list was not found", fullListPath);

            string root = getToolRoot(fullListPath, executablePath);
            ConsumerKind? kind = null;
            foreach (string originalLine in File.ReadLines(fullListPath))
            {
                string line = stripListComment(originalLine);
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (tryUberasmSection(line, out ConsumerKind sectionKind))
                {
                    kind = sectionKind;
                    continue;
                }
                if (kind == null)
                    continue;
                Match pathMatch = asmPathRegex().Match(line);
                if (!pathMatch.Success)
                    continue;
                string listedPath = pathMatch.Groups["quoted"].Success
                    ? pathMatch.Groups["quoted"].Value
                    : pathMatch.Groups["plain"].Value;
                string asmPath = resolveListedPath(root, uberasmSubdirectory(kind.Value), listedPath);
                yield return new()
                {
                    AsmPath = asmPath,
                    Kind = kind.Value,
                    Name = AsarSymbolName.FromPath(asmPath)
                };
            }
        }

        /// <summary>
        /// Resolves a PIXI ASM entry directly or through the referenced CFG or JSON descriptor.
        /// JSON properties named <c>AsmFile</c> are preferred over generic ASM strings.
        /// </summary>
        /// <param name="entryPath">The resolved PIXI list-entry path.</param>
        /// <returns>The absolute path of the primary ASM source.</returns>
        private static string resolvePixiAsm(string entryPath)
        {
            if (entryPath.EndsWith(".asm", StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(entryPath);
            if (!File.Exists(entryPath))
                throw new FileNotFoundException("PIXI list entry was not found", entryPath);

            string? asmFile = null;
            if (entryPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(entryPath));
                asmFile = findAsmString(document.RootElement);
            }
            else
            {
                asmFile = File.ReadLines(entryPath)
                    .Select(stripListComment)
                    .Select(line => line.Trim().Trim('"'))
                    .FirstOrDefault(line => line.EndsWith(".asm", StringComparison.OrdinalIgnoreCase));
            }
            if (string.IsNullOrWhiteSpace(asmFile))
                throw new InvalidDataException($"Could not find the primary ASM file in PIXI entry '{entryPath}'");
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(entryPath)!, asmFile));
        }

        /// <summary>
        /// Recursively searches a PIXI JSON value for an ASM path, prioritizing properties named
        /// <c>AsmFile</c> before other strings.
        /// </summary>
        /// <param name="element">The JSON value to inspect.</param>
        /// <returns>The first matching ASM path, or <see langword="null"/> when none exists.</returns>
        private static string? findAsmString(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                string? value = element.GetString();
                return value?.EndsWith(".asm", StringComparison.OrdinalIgnoreCase) == true
                    ? value
                    : null;
            }
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject()
                    .Where(property => property.Name.Equals("AsmFile",
                        StringComparison.OrdinalIgnoreCase)))
                {
                    string? result = findAsmString(property.Value);
                    if (result != null)
                        return result;
                }
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string? result = findAsmString(property.Value);
                    if (result != null)
                        return result;
                }
            }
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    string? result = findAsmString(child);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines the external tool's root directory from its configured executable or, when
        /// omitted, from the directory containing its list.
        /// </summary>
        /// <param name="listPath">The absolute list path used as fallback.</param>
        /// <param name="executablePath">The optional executable or installation path.</param>
        /// <returns>The directory against which tool-relative entries are resolved.</returns>
        private static string getToolRoot(string listPath, string? executablePath)
        {
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                string full = Path.GetFullPath(executablePath);
                return Directory.Exists(full) ? full : Path.GetDirectoryName(full)!;
            }
            return Path.GetDirectoryName(listPath)!;
        }

        /// <summary>
        /// Produces an operating-system-aware key for physical-path deduplication.
        /// </summary>
        /// <param name="path">The absolute path to normalize for dictionary comparison.</param>
        /// <returns>An ordinal path key that is case-insensitive on Windows.</returns>
        private static string pathKey(string path)
            => OperatingSystem.IsWindows() ? path.ToUpperInvariant() : path;

        /// <summary>
        /// Resolves a list entry by checking the tool root first and then the conventional
        /// section-specific subdirectory.
        /// </summary>
        /// <param name="root">The external tool's root directory.</param>
        /// <param name="subdirectory">The conventional directory for the active list section.</param>
        /// <param name="listedPath">The absolute or tool-relative path read from the list.</param>
        /// <returns>The normalized absolute entry path.</returns>
        private static string resolveListedPath(string root, string subdirectory, string listedPath)
        {
            if (Path.IsPathRooted(listedPath))
                return Path.GetFullPath(listedPath);
            string direct = Path.GetFullPath(Path.Combine(root, listedPath));
            if (File.Exists(direct))
                return direct;
            return Path.GetFullPath(Path.Combine(root, subdirectory, listedPath));
        }

        /// <summary>
        /// Removes the semicolon comment suffix from an external-tool list line.
        /// </summary>
        /// <param name="line">The original list line.</param>
        /// <returns>The trimmed non-comment portion of the line.</returns>
        private static string stripListComment(string line)
        {
            int comment = line.IndexOf(';');
            return (comment >= 0 ? line[..comment] : line).Trim();
        }

        /// <summary>
        /// Interprets a PIXI section header and maps it to its consumer category.
        /// </summary>
        /// <param name="line">The candidate section-header line.</param>
        /// <param name="kind">The recognized category, or the default sprite category on failure.</param>
        /// <returns><see langword="true"/> when the line is a supported PIXI section header.</returns>
        private static bool tryPixiSection(string line, out ConsumerKind kind)
        {
            string section = line.Trim().TrimEnd(':').Replace("_", "").Replace(" ", "").ToUpperInvariant();
            kind = section switch
            {
                "SPRITE" => ConsumerKind.Sprite,
                "CLUSTER" => ConsumerKind.Cluster,
                "EXTENDED" => ConsumerKind.Extended,
                "MINOREXTENDED" => ConsumerKind.MinorExtended,
                "BOUNCE" => ConsumerKind.Bounce,
                "SMOKE" => ConsumerKind.Smoke,
                "SPINNINGCOIN" => ConsumerKind.SpinningCoin,
                "SCORE" => ConsumerKind.Score,
                _ => ConsumerKind.Sprite
            };
            return line.TrimEnd().EndsWith(':') && section is "SPRITE" or "CLUSTER" or
                "EXTENDED" or "MINOREXTENDED" or "BOUNCE" or "SMOKE" or
                "SPINNINGCOIN" or "SCORE";
        }

        /// <summary>
        /// Interprets an UberASMTool section header and maps it to its consumer category.
        /// </summary>
        /// <param name="line">The candidate section-header line.</param>
        /// <param name="kind">The recognized category, or the default level category on failure.</param>
        /// <returns><see langword="true"/> when the line is a supported UberASMTool section header.</returns>
        private static bool tryUberasmSection(string line, out ConsumerKind kind)
        {
            string section = line.Trim().TrimEnd(':').Replace("_", "").Replace(" ", "").ToUpperInvariant();
            kind = section switch
            {
                "LEVEL" => ConsumerKind.Level,
                "OVERWORLD" => ConsumerKind.Overworld,
                "GAMEMODE" => ConsumerKind.GameMode,
                "GLOBAL" => ConsumerKind.Global,
                "STATUSBAR" => ConsumerKind.StatusBar,
                "MACROLIBRARY" or "MACROLIB" => ConsumerKind.MacroLibrary,
                _ => ConsumerKind.Level
            };
            return line.TrimEnd().EndsWith(':') && section is "LEVEL" or "OVERWORLD" or
                "GAMEMODE" or "GLOBAL" or "STATUSBAR" or "MACROLIBRARY" or "MACROLIB";
        }

        /// <summary>
        /// Maps a PIXI consumer category to its conventional installation subdirectory.
        /// </summary>
        /// <param name="kind">The PIXI consumer category.</param>
        /// <returns>The conventional relative directory for that category.</returns>
        private static string pixiSubdirectory(ConsumerKind kind) => kind switch
        {
            ConsumerKind.Sprite => "sprites",
            ConsumerKind.Cluster => "cluster",
            ConsumerKind.Extended => "extended",
            ConsumerKind.MinorExtended => "minor_extended",
            ConsumerKind.Bounce => "bounce",
            ConsumerKind.Smoke => "smoke",
            ConsumerKind.SpinningCoin => "spinning_coin",
            ConsumerKind.Score => "score",
            _ => "sprites"
        };

        /// <summary>
        /// Maps an UberASMTool consumer category to its conventional installation subdirectory.
        /// </summary>
        /// <param name="kind">The UberASMTool consumer category.</param>
        /// <returns>The conventional relative directory for that category.</returns>
        private static string uberasmSubdirectory(ConsumerKind kind) => kind switch
        {
            ConsumerKind.Level => "level",
            ConsumerKind.Overworld => "overworld",
            ConsumerKind.GameMode => "gamemode",
            ConsumerKind.Global => "global",
            ConsumerKind.StatusBar => "statusbar",
            ConsumerKind.MacroLibrary => "library",
            _ => ""
        };

        /// <summary>
        /// Represents one primary ASM file obtained from an external-tool list.
        /// </summary>
        private sealed class Consumer
        {
            /// <summary>Gets the absolute primary ASM path.</summary>
            public required string AsmPath { get; init; }
            /// <summary>Gets the external-tool category of the consumer.</summary>
            public required ConsumerKind Kind { get; init; }
            /// <summary>Gets the normalized consumer name used during collision resolution.</summary>
            public required string Name { get; init; }
        }

        /// <summary>
        /// Creates the parser for Dynamic Xtreme include directives and optional persisted names.
        /// </summary>
        [GeneratedRegex("""^\s*;#inc(?<kind>dyn|dri|pale)\s+"(?<path>[^"]+)"(?:\s*;\s*dxt-name\s*:\s*(?<logical>[A-Za-z0-9]+))?\s*$""", RegexOptions.IgnoreCase)]
        private static partial Regex directiveRegex();

        /// <summary>
        /// Creates the parser for PIXI list entries ending in CFG, JSON, or ASM paths.
        /// </summary>
        [GeneratedRegex("""(?:"(?<quoted>[^"]+\.(?:cfg|json|asm))"|(?<plain>[^\s]+\.(?:cfg|json|asm)))\s*$""", RegexOptions.IgnoreCase)]
        private static partial Regex listPathRegex();

        /// <summary>
        /// Creates the parser for UberASMTool list entries ending in ASM paths.
        /// </summary>
        [GeneratedRegex("""(?:"(?<quoted>[^"]+\.asm)"|(?<plain>[^\s]+\.asm))\s*$""", RegexOptions.IgnoreCase)]
        private static partial Regex asmPathRegex();
    }
}
