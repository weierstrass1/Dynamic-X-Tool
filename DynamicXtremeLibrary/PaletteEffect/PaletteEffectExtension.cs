using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Readers;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.PaletteEffec
{
    /// <summary>Loads, validates, resolves, and serializes palette-effect collections.</summary>
    public static class PaletteEffectExtension
    {
        /// <summary>Defines the largest converted palette payload that fits in one RATS resource.</summary>
        private const int MaximumResourceSize = 32760;

        /// <summary>Recursively loads all traditional palette-effect files in deterministic order.</summary>
        public static IReadOnlyList<PaletteEffectCollection> GetCollections(string paletteDirectory,
            LogRegisterSystem log, out bool validation)
        {
            string[] paths = [.. InputPath.GetFiles(paletteDirectory, "*.paleffect")
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)];
            List<PaletteEffectCollection> result = [];
            validation = AsarSymbolName.ValidateUnique(paths, log);
            foreach (string path in paths)
            {
                PaletteEffectCollection? collection = LoadCollection(path,
                    AsarSymbolName.FromPath(path), log, out bool collectionIsValid);
                if (collection == null || !collectionIsValid)
                {
                    validation = false;
                    continue;
                }
                result.Add(collection);
            }
            return result.AsReadOnly();
        }

        /// <summary>
        /// Loads and validates one collection using its own directory as the resource base.
        /// </summary>
        public static PaletteEffectCollection? LoadCollection(string path, string name,
            LogRegisterSystem log, out bool validation)
        {
            ResolvedPaletteEffectCollection? resolved = LoadResolvedCollection(path, name,
                Path.GetDirectoryName(Path.GetFullPath(path))!, log, out validation);
            return resolved?.Collection;
        }

        /// <summary>
        /// Loads one collection, validates its scalar values and source palettes, and converts
        /// every transition target while its source context is still available.
        /// </summary>
        /// <param name="path">The palette-effect file to load.</param>
        /// <param name="name">The normalized original collection name.</param>
        /// <param name="resourceDirectory">The base directory for relative palette paths.</param>
        /// <param name="log">The validation log.</param>
        /// <param name="validation">Whether the complete collection is valid.</param>
        /// <returns>The resolved collection, or <see langword="null"/> when JSON loading fails.</returns>
        public static ResolvedPaletteEffectCollection? LoadResolvedCollection(string path,
            string name, string resourceDirectory, LogRegisterSystem log, out bool validation)
        {
            PaletteEffectCollection collection = new() { Name = name };
            if (!collection.Load(path))
            {
                validation = false;
                log.Add(new FailedToProcessFile(path));
                return null;
            }

            validation = true;
            List<ResolvedPaletteEffectTarget> targets = [];
            foreach (PaletteEffect effect in collection.Effects)
            {
                validateLimit(path, "Ratio1", effect.Ratio1, 127, log, ref validation);
                validateLimit(path, "Ratio2", effect.Ratio2, 127, log, ref validation);
                validateLimit(path, "Ratio3", effect.Ratio3, 127, log, ref validation);

                if (!Enum.IsDefined(effect.EffectType))
                {
                    log.Add(new ValueNotAllowed(path, "EffectType", (int)effect.EffectType,
                        Enum.GetValues<EffectType>().Select(type => (int)type)));
                    validation = false;
                }

                if (!isTransition(effect.EffectType))
                {
                    validateLimit(path, "Channel1", effect.Channel1, 31, log, ref validation);
                    validateLimit(path, "Channel2", effect.Channel2, 31, log, ref validation);
                    validateLimit(path, "Channel3", effect.Channel3, 31, log, ref validation);
                    if (!string.IsNullOrWhiteSpace(effect.Palette))
                    {
                        log.Add(new InputDiscoveryError(
                            $"palette '{effect.Palette}' is only valid for palette-transition effects in '{path}'"));
                        validation = false;
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(effect.Palette))
                {
                    log.Add(new InputDiscoveryError(
                        $"{effect.EffectType} in '{path}' requires a source palette"));
                    validation = false;
                    continue;
                }

                string palettePath = Path.GetFullPath(InputPath.Resolve(resourceDirectory, effect.Palette));
                if (!File.Exists(palettePath))
                {
                    log.Add(new ResourceNotFound(palettePath));
                    validation = false;
                    continue;
                }

                FileInfo paletteFile = new(palettePath);
                if (paletteFile.Length == 0 || paletteFile.Length % 2 != 0)
                {
                    log.Add(new InputDiscoveryError(
                        $"palette '{palettePath}' must contain a non-empty sequence of two-byte BGR555 colors"));
                    validation = false;
                    continue;
                }

                long convertedSize = paletteFile.Length / 2 * 3;
                if (convertedSize > MaximumResourceSize)
                {
                    log.Add(new FileIsToBig(palettePath, convertedSize, MaximumResourceSize));
                    validation = false;
                    continue;
                }

                string baseName = AsarSymbolName.FromPath(effect.Palette);
                if (string.IsNullOrEmpty(baseName))
                {
                    log.Add(new EmptyGeneratedName(Path.GetFileNameWithoutExtension(effect.Palette)));
                    validation = false;
                    continue;
                }

                PaletteEffectPaletteFormat format = effect.EffectType == EffectType.PalTransitionRGB
                    ? PaletteEffectPaletteFormat.RGB
                    : PaletteEffectPaletteFormat.HSL;
                targets.Add(new()
                {
                    Effect = effect,
                    SourcePath = palettePath,
                    Format = format,
                    BaseName = baseName,
                    Data = PaletteEffectPaletteConverter.Convert(File.ReadAllBytes(palettePath), format)
                });
            }

            foreach (IGrouping<string, ResolvedPaletteEffectTarget> group in targets.GroupBy(
                target => $"{target.BaseName}{target.Format}", StringComparer.Ordinal))
            {
                ResolvedPaletteEffectTarget first = group.First();
                if (group.Skip(1).Any(target => !target.Data.AsSpan().SequenceEqual(first.Data)))
                {
                    log.Add(new InputDiscoveryError(
                        $"palette-effect resources in '{path}' generate the same name '{group.Key}' with different data"));
                    validation = false;
                }
            }

            return new()
            {
                Collection = collection,
                SourcePath = Path.GetFullPath(path),
                ResourceDirectory = Path.GetFullPath(resourceDirectory),
                OriginalName = name,
                Targets = targets.AsReadOnly()
            };
        }

        /// <summary>
        /// Creates logical converted-palette resources and binds every transition effect to its
        /// generated symbol while deduplicating equal data within the same representation.
        /// </summary>
        public static PaletteEffectResourceSet CreateResources(
            IEnumerable<ResolvedPaletteEffectCollection> collections)
        {
            List<(Resource Resource, PaletteEffectPaletteFormat Format)> resources = [];
            Dictionary<PaletteEffect, PaletteEffectResourceBinding> bindings = [];
            foreach (ResolvedPaletteEffectCollection collection in collections)
            {
                foreach (ResolvedPaletteEffectTarget target in collection.Targets)
                {
                    string name = collection.GetPaletteName(target);
                    Resource? sameName = resources.Select(item => item.Resource).FirstOrDefault(
                        resource => resource.Names.Contains(name, StringComparer.Ordinal));
                    if (sameName != null && !sameName.Data.AsSpan().SequenceEqual(target.Data))
                    {
                        throw new InvalidOperationException(
                            $"Palette-effect resource name '{name}' refers to different converted data.");
                    }

                    Resource? resource = resources.FirstOrDefault(item =>
                        item.Format == target.Format && item.Resource.Data.Length == target.Data.Length &&
                        item.Resource.Data.AsSpan().SequenceEqual(target.Data)).Resource;
                    if (resource == null)
                    {
                        resource = new(resources.Count, name,
                            ResourceType.PaletteEffectPalette, target.Data);
                        resources.Add((resource, target.Format));
                    }
                    else
                    {
                        resource.AddAlias(name);
                    }

                    bindings.Add(target.Effect, new()
                    {
                        Effect = target.Effect,
                        Name = name,
                        Resource = resource
                    });
                }
            }

            return new()
            {
                Resources = resources.Select(item => item.Resource).ToArray(),
                Bindings = bindings.AsReadOnly()
            };
        }

        /// <summary>Writes unresolved or channel-only effects using the legacy literal table path.</summary>
        public static void ToFile(LogRegisterSystem log,
            IEnumerable<PaletteEffectCollection> effects, string file)
        {
            writeFile(log, effects.Select(collection =>
                (collection, (IReadOnlyDictionary<PaletteEffect, PaletteEffectResourceBinding>?)null)), file);
        }

        /// <summary>Writes resolved effects and uses generated palette defines for transitions.</summary>
        public static void ToFile(LogRegisterSystem log,
            IEnumerable<ResolvedPaletteEffectCollection> effects,
            PaletteEffectResourceSet resources, string file)
        {
            writeFile(log, effects.Select(effect =>
                (effect.Collection, (IReadOnlyDictionary<PaletteEffect, PaletteEffectResourceBinding>?)resources.Bindings)),
                file);
        }

        /// <summary>Serializes non-empty palette effects into the legacy runtime binary layout.</summary>
        public static byte[] ToBin(IEnumerable<PaletteEffectCollection> effects)
            => ToBin(effects, null);

        /// <summary>
        /// Serializes effects while resolving transition target addresses through a caller-supplied
        /// mapping.
        /// </summary>
        public static byte[] ToBin(IEnumerable<PaletteEffectCollection> effects,
            Func<PaletteEffect, int>? paletteAddressResolver)
        {
            PaletteEffectCollection[] collections = [.. effects];
            if (collections.Length == 0)
                return [0, 0, 0, 0];
            List<PaletteEffect> filtered = [.. collections.SelectMany(effect => effect.Effects)
                .Where(effect => effect.EffectType != EffectType.None)];
            if (filtered.Count == 0)
                return [0, 0];

            List<byte> type = [];
            List<byte> value1 = [];
            List<byte> value2 = [];
            List<byte> value3 = [];
            List<byte> ratio1 = [];
            List<byte> ratio2 = [];
            List<byte> ratio3 = [];
            foreach (PaletteEffect effect in filtered)
            {
                type.Add((byte)effect.EffectType);
                if (isTransition(effect.EffectType) && !string.IsNullOrWhiteSpace(effect.Palette))
                {
                    if (paletteAddressResolver == null)
                        throw new InvalidOperationException(
                            "A palette address resolver is required to serialize palette transitions.");
                    int address = paletteAddressResolver(effect);
                    value1.Add((byte)address);
                    value2.Add((byte)(address >> 8));
                    value3.Add((byte)(address >> 16));
                }
                else
                {
                    value1.Add(effect.Channel1);
                    value2.Add(effect.Channel2);
                    value3.Add(effect.Channel3);
                }
                ratio1.Add(effect.Ratio1);
                ratio2.Add(effect.Ratio2);
                ratio3.Add(effect.Ratio3);
            }

            return
            [
                (byte)filtered.Count,
                (byte)(filtered.Count / 256),
                .. type,
                .. value1,
                .. value2,
                .. value3,
                .. ratio1,
                .. ratio2,
                .. ratio3
            ];
        }

        /// <summary>Writes the shared parallel-table format for literal and resolved effects.</summary>
        private static void writeFile(LogRegisterSystem log,
            IEnumerable<(PaletteEffectCollection Collection,
                IReadOnlyDictionary<PaletteEffect, PaletteEffectResourceBinding>? Bindings)> sources,
            string file)
        {
            var sourceList = sources.ToArray();
            if (sourceList.Length == 0)
            {
                File.WriteAllText(file, "PaletteEffectsTable:\n.Length\n\tdw $0000\n");
                return;
            }

            List<string> types = [];
            List<string> value1 = [];
            List<string> value2 = [];
            List<string> value3 = [];
            List<string> ratio1 = [];
            List<string> ratio2 = [];
            List<string> ratio3 = [];
            log.Add(new Title("Palette Effects"));
            foreach (var source in sourceList)
            {
                PaletteEffect[] filtered = [.. source.Collection.Effects.Where(
                    effect => effect.EffectType != EffectType.None)];
                log.Add(new InsertedPaletteEffect(source.Collection.Name ?? "", filtered.Length));
                foreach (PaletteEffect effect in filtered)
                {
                    types.Add($"${(int)effect.EffectType:X2}");
                    bool transitionWithPalette = isTransition(effect.EffectType) &&
                        !string.IsNullOrWhiteSpace(effect.Palette);
                    if (transitionWithPalette)
                    {
                        if (source.Bindings == null || !source.Bindings.TryGetValue(effect,
                            out PaletteEffectResourceBinding? binding))
                        {
                            throw new InvalidOperationException(
                                $"No converted palette resource was bound to {effect.EffectType}.");
                        }
                        value1.Add($"(!Palette{binding.Name})&$FF");
                        value2.Add($"(!Palette{binding.Name}>>8)&$FF");
                        value3.Add($"(!Palette{binding.Name}>>16)&$FF");
                    }
                    else
                    {
                        value1.Add($"${effect.Channel1:X2}");
                        value2.Add($"${effect.Channel2:X2}");
                        value3.Add($"${effect.Channel3:X2}");
                    }
                    ratio1.Add($"${effect.Ratio1:X2}");
                    ratio2.Add($"${effect.Ratio2:X2}");
                    ratio3.Add($"${effect.Ratio3:X2}");
                }
            }

            StringBuilder output = new();
            output.AppendLine($"PaletteEffectsTable:\n.Length\n\tdw ${types.Count:X4}\n");
            appendTable(output, ".Types:", types);
            appendTable(output, ".Value1", value1);
            appendTable(output, ".Value2", value2);
            appendTable(output, ".Value3", value3);
            appendTable(output, ".Ratio1", ratio1);
            appendTable(output, ".Ratio2", ratio2);
            appendTable(output, ".Ratio3", ratio3, false);
            File.WriteAllText(file, output.ToString());
        }

        /// <summary>Appends one Asar byte table with the established sixteen-value line width.</summary>
        private static void appendTable(StringBuilder output, string label,
            IReadOnlyList<string> values, bool trailingBlankLine = true)
        {
            output.Append(label);
            for (int index = 0; index < values.Count; index++)
            {
                if (index % 16 == 0)
                    output.Append("\n\tdb ");
                else
                    output.Append(',');
                output.Append(values[index]);
            }
            output.AppendLine();
            if (trailingBlankLine)
                output.AppendLine();
        }

        /// <summary>Validates one byte-sized field against its format-specific upper bound.</summary>
        private static void validateLimit(string path, string parameter, int value, int limit,
            LogRegisterSystem log, ref bool validation)
        {
            if (value <= limit)
                return;
            log.Add(new ValueExceedsLimit(path, parameter, value, limit));
            validation = false;
        }

        /// <summary>Determines whether an effect consumes a converted target palette.</summary>
        private static bool isTransition(EffectType effectType)
            => effectType is EffectType.PalTransitionRGB or EffectType.PalTransitionHSL;
    }
}
