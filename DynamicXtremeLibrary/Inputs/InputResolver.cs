using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremeLibrary.Readers;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;

namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Parses discovered inputs, deduplicates semantically equivalent definitions, resolves symbol
    /// collisions, and prepares source rewrites for renamed external inputs.
    /// </summary>
    public sealed class InputResolver
    {
        private readonly LogRegisterSystem _log;
        private readonly ReadInfo _reader;
        private readonly string _temporaryDirectory;
        private readonly Func<NameCollision, bool>? _collisionResolver;

        /// <summary>
        /// Initializes an input resolver with the parsers and policies required by the current run.
        /// </summary>
        /// <param name="log">The log that receives processing and validation results.</param>
        /// <param name="reader">The reader used to parse DynamicInfo and DrawInfo files.</param>
        /// <param name="temporaryDirectory">The directory available for temporary DynamicInfo data.</param>
        /// <param name="collisionResolver">The optional callback used to approve generated collision-free names.</param>
        public InputResolver(LogRegisterSystem log, ReadInfo reader, string temporaryDirectory,
            Func<NameCollision, bool>? collisionResolver)
        {
            _log = log;
            _reader = reader;
            _temporaryDirectory = temporaryDirectory;
            _collisionResolver = collisionResolver;
        }

        /// <summary>
        /// Resolves the enabled input categories into one validated, collision-free installation
        /// model and assigns consecutive DrawInfo IDs after deduplication.
        /// </summary>
        /// <param name="inputs">The physically discovered inputs and their consumer origins.</param>
        /// <param name="includeDynamicInfos">Whether DynamicInfo inputs must be resolved.</param>
        /// <param name="includeDrawInfos">Whether DrawInfo inputs must be resolved.</param>
        /// <param name="includePaletteEffects">Whether palette-effect inputs must be resolved.</param>
        /// <returns>The resolved installation model, or <see langword="null"/> when validation or collision resolution fails.</returns>
        public ResolvedInputs? Resolve(IEnumerable<InfoInput> inputs,
            bool includeDynamicInfos, bool includeDrawInfos, bool includePaletteEffects)
        {
            List<Unit> parsed = [];
            int temporaryIndex = 0;
            foreach (InfoInput input in inputs.Where(input =>
                input.Kind == InfoKind.DynamicInfo && includeDynamicInfos ||
                input.Kind == InfoKind.DrawInfo && includeDrawInfos ||
                input.Kind == InfoKind.PaletteEffect && includePaletteEffects))
            {
                if (string.IsNullOrEmpty(input.OriginalName))
                {
                    _log.Add(new EmptyGeneratedName(Path.GetFileNameWithoutExtension(input.Path)));
                    return null;
                }
                _log.Add(new ProcessingFile(input.Path));
                object? value;
                bool valid;
                switch (input.Kind)
                {
                    case InfoKind.DynamicInfo:
                        string tempDirectory = Path.Combine(_temporaryDirectory, "Inputs",
                            temporaryIndex++.ToString("D4"));
                        DynamicInfo? dynamicInfo = _reader.ReadDynamicInfo(input.Path,
                            input.ResourceDirectory, tempDirectory, out valid);
                        value = dynamicInfo;
                        if (valid && dynamicInfo != null)
                            valid = dynamicInfo.Validate(input.ResourceDirectory, _log);
                        if (valid && dynamicInfo != null)
                        {
                            valid = AsarSymbolName.ValidateUnique(
                                dynamicInfo.DeclaredPoseGraphics ?? [], _log) &
                                AsarSymbolName.ValidateUnique(
                                    (dynamicInfo.DeclaredPalettes ?? [])
                                        .Distinct(InputPath.Comparer), _log) &
                                AsarSymbolName.ValidateUnique(
                                    (dynamicInfo.DeclaredResources ?? [])
                                        .Distinct(InputPath.Comparer), _log);
                        }
                        break;
                    case InfoKind.DrawInfo:
                        DrawInfo[] drawInfos = _reader.ReadDrawInfo(input.Path, 0, out valid);
                        value = drawInfos;
                        if (valid)
                            valid = drawInfos.All(drawInfo => drawInfo.Validate(_log));
                        break;
                    default:
                        value = PaletteEffectExtension.LoadResolvedCollection(input.Path,
                            input.OriginalName, input.ResourceDirectory, _log, out valid);
                        break;
                }
                if (!valid || value == null)
                {
                    _log.Add(new FailedToProcessFile(input.Path));
                    return null;
                }
                _log.Add(new SuccessfullyProcessedFile(input.Path));
                parsed.Add(new(input, value));
            }

            List<Unit> units = deduplicate(parsed);
            List<HashSet<Unit>> conflicts = buildConflictComponents(units);
            if (!resolveNames(units, conflicts))
                return null;

            foreach (Unit unit in units)
                applyLogicalName(unit);
            if (hasFinalSymbolCollision(units))
            {
                _log.Add(new InputDiscoveryError(
                    "generated symbols still collide after applying the selected names"));
                return null;
            }

            Dictionary<string, SourceRewrite> rewrites = new(InputPath.Comparer);
            List<(string OldName, string NewName, IReadOnlyList<string> Files)> changes = [];
            foreach (Unit unit in units)
                buildRewrites(unit, rewrites, changes);

            List<DrawInfo> allDrawInfos = [];
            int drawInfoId = 0;
            foreach (Unit unit in units.Where(unit => unit.Input.Kind == InfoKind.DrawInfo))
            {
                foreach (DrawInfo drawInfo in (DrawInfo[])unit.Value)
                {
                    drawInfo.ID = drawInfoId++;
                    allDrawInfos.Add(drawInfo);
                }
            }

            return new()
            {
                DynamicInfos = units.Where(unit => unit.Input.Kind == InfoKind.DynamicInfo)
                    .Select(unit => (DynamicInfo)unit.Value).ToArray(),
                DrawInfos = allDrawInfos,
                PaletteEffects = units.Where(unit => unit.Input.Kind == InfoKind.PaletteEffect)
                    .Select(unit => (ResolvedPaletteEffectCollection)unit.Value).ToArray(),
                SourceRewrites = rewrites.Values.OrderBy(rewrite => rewrite.Path,
                    StringComparer.OrdinalIgnoreCase).ToArray(),
                NameChanges = changes
            };
        }

        /// <summary>
        /// Merges semantically equivalent inputs only within groups that share the same kind and
        /// normalized original name. Distinct contents remain separate collision candidates.
        /// </summary>
        /// <param name="parsed">The individually parsed physical inputs.</param>
        /// <returns>The logical units that remain after semantic deduplication.</returns>
        private List<Unit> deduplicate(IEnumerable<Unit> parsed)
        {
            List<Unit> result = [];
            foreach (IGrouping<(InfoKind Kind, string OriginalName), Unit> group in parsed.GroupBy(
                unit => (unit.Input.Kind, unit.Input.OriginalName)))
            {
                foreach (Unit candidate in group)
                {
                    Unit? equal = result.FirstOrDefault(unit =>
                        unit.Input.Kind == group.Key.Kind &&
                        unit.Input.OriginalName == group.Key.OriginalName &&
                        semanticEquals(unit, candidate));
                    if (equal == null)
                    {
                        result.Add(candidate);
                        continue;
                    }
                    equal.Input.IsTraditional |= candidate.Input.IsTraditional;
                    equal.Input.Origins.AddRange(candidate.Input.Origins);
                    equal.SourcePaths.Add(candidate.Input.Path);
                }
            }
            return result;
        }

        /// <summary>
        /// Builds connected collision components by linking same-kind units whose base generated
        /// symbols overlap, including transitive conflicts.
        /// </summary>
        /// <param name="units">The deduplicated logical inputs.</param>
        /// <returns>Connected components containing at least two conflicting units.</returns>
        private static List<HashSet<Unit>> buildConflictComponents(IReadOnlyList<Unit> units)
        {
            Dictionary<Unit, HashSet<Unit>> edges = units.ToDictionary(unit => unit, _ => new HashSet<Unit>());
            for (int leftIndex = 0; leftIndex < units.Count; leftIndex++)
            {
                for (int rightIndex = leftIndex + 1; rightIndex < units.Count; rightIndex++)
                {
                    Unit left = units[leftIndex];
                    Unit right = units[rightIndex];
                    if (!claimsConflict(left, right, useLogicalNames: false))
                        continue;
                    edges[left].Add(right);
                    edges[right].Add(left);
                }
            }

            List<HashSet<Unit>> components = [];
            HashSet<Unit> visited = [];
            foreach (Unit unit in units)
            {
                if (edges[unit].Count == 0 || !visited.Add(unit))
                    continue;
                HashSet<Unit> component = [unit];
                Queue<Unit> queue = new([unit]);
                while (queue.TryDequeue(out Unit? current))
                {
                    foreach (Unit next in edges[current])
                    {
                        if (!visited.Add(next))
                            continue;
                        component.Add(next);
                        queue.Enqueue(next);
                    }
                }
                components.Add(component);
            }
            return components;
        }

        /// <summary>
        /// Assigns deterministic collision-free logical names, reuses valid persisted names, and
        /// requests approval before accepting newly generated names.
        /// </summary>
        /// <param name="units">Every deduplicated unit participating in the installation.</param>
        /// <param name="conflicts">The connected symbol-collision components.</param>
        /// <returns><see langword="true"/> when every component receives an approved valid name.</returns>
        private bool resolveNames(IReadOnlyList<Unit> units, IReadOnlyList<HashSet<Unit>> conflicts)
        {
            HashSet<string> usedNames = units
                .Where(unit => conflicts.All(component => !component.Contains(unit)))
                .Select(unit => unit.Input.OriginalName)
                .ToHashSet(StringComparer.Ordinal);
            HashSet<string> usedSymbols = units
                .Where(unit => conflicts.All(component => !component.Contains(unit)))
                .SelectMany(finalSymbols)
                .ToHashSet(StringComparer.Ordinal);

            foreach (HashSet<Unit> component in conflicts)
            {
                if (component.Count(unit => unit.Input.IsTraditional) > 1)
                {
                    string[] files = [.. component.Where(unit => unit.Input.IsTraditional).SelectMany(unit => unit.SourcePaths)];
                    _log.Add(new InputDiscoveryError(
                        $"traditional resources collide and have no primary ASM that can be rewritten: {string.Join(", ", files)}"));
                    return false;
                }

                bool existingNamesWork = true;
                HashSet<string> componentNames = new(StringComparer.Ordinal);
                foreach (Unit unit in ordered(component))
                {
                    string? previous = getPreviousLogicalName(unit);
                    string name = unit.Input.IsTraditional
                        ? unit.Input.OriginalName
                        : previous ?? "";
                    if (string.IsNullOrEmpty(name) || !name.StartsWith(unit.Input.OriginalName, StringComparison.Ordinal) ||
                        !componentNames.Add(name) || usedNames.Contains(name))
                    {
                        existingNamesWork = false;
                        break;
                    }
                    unit.Input.LogicalName = name;
                }
                if (existingNamesWork && !hasFinalSymbolCollision(component) &&
                    !component.SelectMany(finalSymbols).Any(usedSymbols.Contains))
                {
                    usedNames.UnionWith(component.Select(unit => unit.Input.LogicalName));
                    usedSymbols.UnionWith(component.SelectMany(finalSymbols));
                    continue;
                }

                Dictionary<ConsumerKind, int> kindCounts = component
                    .Where(unit => !unit.Input.IsTraditional)
                    .GroupBy(unit => owner(unit).Kind)
                    .ToDictionary(group => group.Key, group => group.Count());
                foreach (Unit unit in ordered(component))
                {
                    if (unit.Input.IsTraditional)
                    {
                        unit.Input.LogicalName = unit.Input.OriginalName;
                        usedNames.Add(unit.Input.LogicalName);
                        usedSymbols.UnionWith(finalSymbols(unit));
                        continue;
                    }
                    (ConsumerKind Kind, string Name) ownerInfo = owner(unit);
                    string suffix = kindCounts[ownerInfo.Kind] == 1
                        ? ownerInfo.Kind.ToString()
                        : $"{AsarSymbolName.Normalize(ownerInfo.Name)}{ownerInfo.Kind}";
                    string proposal = $"{unit.Input.OriginalName}{suffix}";
                    int number = 2;
                    unit.Input.LogicalName = proposal;
                    while (usedNames.Contains(proposal) ||
                        finalSymbols(unit).Any(usedSymbols.Contains))
                    {
                        proposal = $"{unit.Input.OriginalName}{suffix}{number++}";
                        unit.Input.LogicalName = proposal;
                    }
                    unit.Input.LogicalName = proposal;
                    usedNames.Add(proposal);
                    usedSymbols.UnionWith(finalSymbols(unit));
                }

                NameCollision collision = new()
                {
                    GeneratedName = getCollisionName(component),
                    Candidates = [.. ordered(component).Select(unit => new NameCollisionCandidate
                    {
                        OriginalName = unit.Input.OriginalName,
                        ProposedName = unit.Input.LogicalName,
                        Files = unit.SourcePaths.Distinct(InputPath.Comparer).ToArray()
                    })]
                };
                if (_collisionResolver == null || !_collisionResolver(collision))
                {
                    _log.Add(new InputDiscoveryError(
                        $"name collision for '{collision.GeneratedName}' was not approved"));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Orders units deterministically by discovery order and physical path.
        /// </summary>
        /// <param name="units">The units to order.</param>
        /// <returns>The units in stable collision-resolution order.</returns>
        private static IEnumerable<Unit> ordered(IEnumerable<Unit> units)
            => units.OrderByDescending(unit => unit.Input.IsTraditional)
                .ThenBy(unit => unit.SortKey, StringComparer.OrdinalIgnoreCase)
                .ThenBy(unit => unit.SortKey, StringComparer.Ordinal);

        /// <summary>
        /// Selects the normalized name used to describe a collision component.
        /// </summary>
        /// <param name="units">The units in one collision component.</param>
        /// <returns>The first stable original name in the component.</returns>
        private static string getCollisionName(IEnumerable<Unit> units)
        {
            Unit[] candidates = [.. units];
            string? symbol = null;
            for (int leftIndex = 0; leftIndex < candidates.Length; leftIndex++)
            {
                for (int rightIndex = leftIndex + 1; rightIndex < candidates.Length; rightIndex++)
                {
                    string? candidate = firstConflictingSymbol(candidates[leftIndex],
                        candidates[rightIndex], useLogicalNames: false);
                    if (candidate != null && (symbol == null ||
                        StringComparer.Ordinal.Compare(candidate, symbol) < 0))
                    {
                        symbol = candidate;
                    }
                }
            }
            if (symbol == null)
                return "unknown";
            int separator = symbol.IndexOf(':');
            return separator >= 0 ? symbol[(separator + 1)..] : symbol;
        }

        /// <summary>
        /// Verifies that no two resolved units of the same kind still emit the same final symbol.
        /// </summary>
        /// <param name="units">The units after logical names have been assigned.</param>
        /// <returns><see langword="true"/> when at least one final generated symbol is duplicated.</returns>
        private static bool hasFinalSymbolCollision(IEnumerable<Unit> units)
        {
            Unit[] candidates = [.. units];
            for (int leftIndex = 0; leftIndex < candidates.Length; leftIndex++)
            {
                for (int rightIndex = leftIndex + 1; rightIndex < candidates.Length; rightIndex++)
                {
                    if (claimsConflict(candidates[leftIndex], candidates[rightIndex],
                        useLogicalNames: true))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether two units emit the same symbol for different semantic content.
        /// Identical binary resources are intentionally shareable across DynamicInfo inputs.
        /// </summary>
        private static bool claimsConflict(Unit left, Unit right, bool useLogicalNames)
        {
            return firstConflictingSymbol(left, right, useLogicalNames) != null;
        }

        /// <summary>Finds the first symbol whose claims cannot safely share one generated define.</summary>
        private static string? firstConflictingSymbol(Unit left, Unit right,
            bool useLogicalNames)
        {
            bool differentInputKinds = left.Input.Kind != right.Input.Kind;
            foreach (SymbolClaim leftClaim in left.Claims)
            {
                if (differentInputKinds && leftClaim.Kind != SymbolKind.Palette)
                    continue;
                string leftSymbol = leftClaim.GetSymbol(left, useLogicalNames);
                foreach (SymbolClaim rightClaim in right.Claims)
                {
                    if (differentInputKinds && rightClaim.Kind != SymbolKind.Palette)
                        continue;
                    if (leftSymbol != rightClaim.GetSymbol(right, useLogicalNames))
                        continue;
                    if (!leftClaim.IsEquivalentTo(rightClaim))
                        return leftSymbol;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves a persisted logical name only when all recorded origins agree on the same
        /// non-empty value.
        /// </summary>
        /// <param name="unit">The logical input whose origins are inspected.</param>
        /// <returns>The reusable logical name, or <see langword="null"/> when none is unambiguous.</returns>
        private static string? getPreviousLogicalName(Unit unit)
        {
            string[] previous = [.. unit.Input.Origins
                .Select(origin => origin.PreviousLogicalName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .Cast<string>()];
            return previous.Length == 1 ? previous[0] : null;
        }

        /// <summary>
        /// Selects the first deterministic external consumer used to derive a rename suffix.
        /// </summary>
        /// <param name="unit">The external unit requiring a generated name.</param>
        /// <returns>The selected consumer kind and normalized consumer name.</returns>
        private static (ConsumerKind Kind, string Name) owner(Unit unit)
        {
            InputOrigin? origin = unit.Input.Origins
                .OrderBy(origin => origin.AsmPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(origin => origin.DirectiveLine)
                .FirstOrDefault();
            if (origin == null)
                throw new InvalidOperationException(
                    $"Cannot rename '{unit.Input.Path}' because it has no external consumer.");
            return (origin.ConsumerKind, origin.ConsumerName);
        }

        /// <summary>
        /// Applies the resolved logical name to the parsed model used by downstream generators.
        /// </summary>
        /// <param name="unit">The resolved unit whose parsed value must be updated.</param>
        private static void applyLogicalName(Unit unit)
        {
            switch (unit.Value)
            {
                case DynamicInfo dynamicInfo:
                    dynamicInfo.SetLogicalName(unit.Input.OriginalName, unit.Input.LogicalName);
                    break;
                case DrawInfo[] drawInfos:
                    foreach (DrawInfo drawInfo in drawInfos)
                        drawInfo.ContextName = unit.Input.LogicalName;
                    break;
                case ResolvedPaletteEffectCollection paletteEffects:
                    paletteEffects.SetLogicalName(unit.Input.LogicalName);
                    break;
            }
        }

        /// <summary>
        /// Adds every symbol and directive update required by one unit to its consumers' deferred
        /// rewrite plans and records user-visible name changes.
        /// </summary>
        /// <param name="unit">The resolved logical unit.</param>
        /// <param name="rewrites">The rewrite plan indexed by ASM path.</param>
        /// <param name="changes">The committed-name-change log entries being assembled.</param>
        private static void buildRewrites(Unit unit,
            IDictionary<string, SourceRewrite> rewrites,
            ICollection<(string OldName, string NewName, IReadOnlyList<string> Files)> changes)
        {
            foreach (IGrouping<string, InputOrigin> origins in unit.Input.Origins.GroupBy(
                origin => origin.AsmPath, InputPath.Comparer))
            {
                if (!rewrites.TryGetValue(origins.Key, out SourceRewrite? rewrite))
                {
                    rewrite = new() { Path = origins.Key };
                    rewrites.Add(origins.Key, rewrite);
                }
                foreach (InputOrigin origin in origins)
                {
                    string previous = origin.PreviousLogicalName ?? unit.Input.OriginalName;
                    addSymbolChanges(unit, previous, unit.Input.LogicalName, rewrite);
                    if (unit.Input.LogicalName != unit.Input.OriginalName || origin.PreviousLogicalName != null)
                        rewrite.SetDirectiveLogicalName(origin.DirectiveLine, unit.Input.LogicalName);
                }
            }

            string oldName = getPreviousLogicalName(unit) ?? unit.Input.OriginalName;
            if (oldName != unit.Input.LogicalName)
            {
                changes.Add((oldName, unit.Input.LogicalName,
                    unit.Input.Origins.Select(origin => origin.AsmPath)
                        .Distinct(InputPath.Comparer).ToArray()));
            }
        }

        /// <summary>
        /// Registers every exact generated Asar symbol that changes when one input receives a new
        /// logical name.
        /// </summary>
        /// <param name="unit">The parsed input whose symbol family is being renamed.</param>
        /// <param name="oldLogicalName">The logical name currently referenced by the consumer.</param>
        /// <param name="newLogicalName">The final logical name generated for the input.</param>
        /// <param name="rewrite">The consumer rewrite plan that receives the mappings.</param>
        private static void addSymbolChanges(Unit unit, string oldLogicalName,
            string newLogicalName, SourceRewrite rewrite)
        {
            switch (unit.Value)
            {
                case DrawInfo[] drawInfos:
                    foreach (DrawInfo drawInfo in drawInfos)
                    {
                        rewrite.AddSymbolChange($"!PoseID{oldLogicalName}_{drawInfo.Name}",
                            $"!PoseID{newLogicalName}_{drawInfo.Name}");
                    }
                    rewrite.AddSymbolChange($"!RenderBoxXDistanceOutOfScreen{oldLogicalName}",
                        $"!RenderBoxXDistanceOutOfScreen{newLogicalName}");
                    rewrite.AddSymbolChange($"!RenderBoxYDistanceOutOfScreen{oldLogicalName}",
                        $"!RenderBoxYDistanceOutOfScreen{newLogicalName}");
                    break;
                case DynamicInfo dynamicInfo:
                    string oldSuffix = logicalSuffix(unit.Input.OriginalName, oldLogicalName);
                    string newSuffix = logicalSuffix(unit.Input.OriginalName, newLogicalName);
                    addResourceChanges(dynamicInfo.GetPoseResourceNames(oldSuffix),
                        dynamicInfo.GetPoseResourceNames(newSuffix), "DynamicPose", rewrite);
                    addResourceChanges((dynamicInfo.Palettes ?? []).Select(path =>
                            dynamicInfo.GetPaletteName(path, oldSuffix)),
                        (dynamicInfo.Palettes ?? []).Select(path =>
                            dynamicInfo.GetPaletteName(path, newSuffix)), "Palette", rewrite);
                    addResourceChanges((dynamicInfo.Resources ?? []).Select(path =>
                            dynamicInfo.GetResourceName(path, oldSuffix)),
                        (dynamicInfo.Resources ?? []).Select(path =>
                            dynamicInfo.GetResourceName(path, newSuffix)), "Resource", rewrite);
                    rewrite.AddSymbolChange($"!{oldLogicalName}PaletteTableOffset",
                        $"!{newLogicalName}PaletteTableOffset");
                    break;
                case ResolvedPaletteEffectCollection paletteEffects:
                    int count = paletteEffects.Collection.Effects.Count(
                        effect => effect.EffectType != EffectType.None);
                    for (int index = 0; index < count; index++)
                        rewrite.AddSymbolChange($"!PaletteEffectID{oldLogicalName}{index}",
                            $"!PaletteEffectID{newLogicalName}{index}");
                    string paletteOldSuffix = logicalSuffix(unit.Input.OriginalName, oldLogicalName);
                    string paletteNewSuffix = logicalSuffix(unit.Input.OriginalName, newLogicalName);
                    foreach (ResolvedPaletteEffectTarget target in paletteEffects.Targets)
                    {
                        string oldName = paletteEffects.GetPaletteName(target, paletteOldSuffix);
                        string newName = paletteEffects.GetPaletteName(target, paletteNewSuffix);
                        rewrite.AddSymbolChange($"!Palette{oldName}", $"!Palette{newName}");
                        rewrite.AddSymbolChange($"!Palette{oldName}Size", $"!Palette{newName}Size");
                    }
                    break;
            }
        }

        /// <summary>
        /// Adds ID, address, and size define replacements for paired DynamicInfo resource names.
        /// </summary>
        /// <param name="oldNames">The resource names produced by the previous logical suffix.</param>
        /// <param name="newNames">The corresponding names produced by the final suffix.</param>
        /// <param name="prefix">The Asar define family prefix.</param>
        /// <param name="rewrite">The consumer rewrite plan that receives the mappings.</param>
        private static void addResourceChanges(IEnumerable<string> oldNames,
            IEnumerable<string> newNames, string prefix, SourceRewrite rewrite)
        {
            foreach ((string oldName, string newName) in oldNames.Zip(newNames))
            {
                rewrite.AddSymbolChange($"!{prefix}ID{oldName}", $"!{prefix}ID{newName}");
                rewrite.AddSymbolChange($"!{prefix}{oldName}", $"!{prefix}{newName}");
                rewrite.AddSymbolChange($"!{prefix}{oldName}Size", $"!{prefix}{newName}Size");
            }
        }

        /// <summary>
        /// Builds the generated-symbol claims of one parsed input and captures binary content once
        /// for subsequent collision comparisons.
        /// </summary>
        private static IReadOnlyList<SymbolClaim> createClaims(InfoInput input, object value)
        {
            List<SymbolClaim> claims = [];
            switch (value)
            {
                case DynamicInfo dynamicInfo:
                    claims.Add(new(SymbolKind.Context, input.OriginalName));

                    IReadOnlyList<Resource> poses = dynamicInfo.GetPosesData(0);
                    int[] lastRows = dynamicInfo.GetLastRow();
                    for (int index = 0; index < poses.Count; index++)
                    {
                        Resource pose = poses[index];
                        claims.Add(new(SymbolKind.Pose, pose.Name, pose.Name.Length - 3,
                            pose.Data,
                            [
                                dynamicInfo.GetPoseSize(index),
                                dynamicInfo.GetPoseBlocks(index),
                                dynamicInfo.PosesChunksSizes![index * 2] * 32,
                                dynamicInfo.PosesChunksSizes[index * 2 + 1] * 32,
                                index < lastRows.Length ? lastRows[index] : 0
                            ]));
                    }

                    foreach (string path in dynamicInfo.Palettes ?? [])
                    {
                        claims.Add(new(SymbolKind.Palette,
                            dynamicInfo.GetPaletteName(path, ""), -1,
                            File.ReadAllBytes(dynamicInfo.ResolveResourcePath(path)), [0]));
                    }
                    foreach (string path in dynamicInfo.Resources ?? [])
                    {
                        claims.Add(new(SymbolKind.Resource,
                            dynamicInfo.GetResourceName(path, ""), -1,
                            File.ReadAllBytes(dynamicInfo.ResolveResourcePath(path)), []));
                    }
                    break;
                case DrawInfo[]:
                    claims.Add(new(SymbolKind.Context, input.OriginalName));
                    break;
                case ResolvedPaletteEffectCollection paletteEffects:
                    claims.Add(new(SymbolKind.Collection, input.OriginalName));
                    foreach (ResolvedPaletteEffectTarget target in paletteEffects.Targets)
                    {
                        string baseName = paletteEffects.GetPaletteName(target, "");
                        claims.Add(new(SymbolKind.Palette, baseName,
                            target.BaseName.Length, target.Data,
                            [(int)target.Format + 1]));
                    }
                    break;
            }
            return claims.AsReadOnly();
        }

        /// <summary>
        /// Extracts the suffix appended to an original name during collision resolution.
        /// </summary>
        /// <param name="original">The normalized name derived from the source filename.</param>
        /// <param name="logical">The resolved logical name.</param>
        /// <returns>The appended suffix, or the complete logical name when it has another root.</returns>
        private static string logicalSuffix(string original, string logical)
            => logical.StartsWith(original, StringComparison.Ordinal)
                ? logical[original.Length..]
                : logical;

        /// <summary>
        /// Enumerates the semantic symbol identities emitted by a unit before collision renaming.
        /// Prefixes distinguish contexts, poses, palettes, resources, and collections.
        /// </summary>
        /// <param name="unit">The parsed unit to inspect.</param>
        /// <returns>The base semantic symbols used to build the collision graph.</returns>
        private static IEnumerable<string> baseSymbols(Unit unit)
        {
            switch (unit.Value)
            {
                case DynamicInfo dynamicInfo:
                    yield return $"Context:{unit.Input.OriginalName}";
                    foreach (string name in dynamicInfo.GetPoseResourceNames(""))
                        yield return $"Pose:{name}";
                    foreach (string path in dynamicInfo.Palettes ?? [])
                        yield return $"Palette:{dynamicInfo.GetPaletteName(path, "")}";
                    foreach (string path in dynamicInfo.Resources ?? [])
                        yield return $"Resource:{dynamicInfo.GetResourceName(path, "")}";
                    break;
                case DrawInfo[]:
                    yield return $"Context:{unit.Input.OriginalName}";
                    break;
                case ResolvedPaletteEffectCollection paletteEffects:
                    yield return $"Collection:{unit.Input.OriginalName}";
                    foreach (ResolvedPaletteEffectTarget target in paletteEffects.Targets)
                        yield return $"Palette:{paletteEffects.GetPaletteName(target, "")}";
                    break;
            }
        }

        /// <summary>
        /// Enumerates the semantic symbol identities emitted after applying a unit's resolved
        /// logical suffix.
        /// </summary>
        /// <param name="unit">The resolved unit to inspect.</param>
        /// <returns>The final semantic symbols used by the safety validation.</returns>
        private static IEnumerable<string> finalSymbols(Unit unit)
        {
            string suffix = logicalSuffix(unit.Input.OriginalName, unit.Input.LogicalName);
            switch (unit.Value)
            {
                case DynamicInfo dynamicInfo:
                    yield return $"Context:{unit.Input.LogicalName}";
                    foreach (string name in dynamicInfo.GetPoseResourceNames(suffix))
                        yield return $"Pose:{name}";
                    foreach (string path in dynamicInfo.Palettes ?? [])
                        yield return $"Palette:{dynamicInfo.GetPaletteName(path, suffix)}";
                    foreach (string path in dynamicInfo.Resources ?? [])
                        yield return $"Resource:{dynamicInfo.GetResourceName(path, suffix)}";
                    break;
                case DrawInfo[]:
                    yield return $"Context:{unit.Input.LogicalName}";
                    break;
                case ResolvedPaletteEffectCollection paletteEffects:
                    yield return $"Collection:{unit.Input.LogicalName}";
                    foreach (ResolvedPaletteEffectTarget target in paletteEffects.Targets)
                        yield return $"Palette:{paletteEffects.GetPaletteName(target, suffix)}";
                    break;
            }
        }

        /// <summary>
        /// Dispatches to the format-specific conceptual equality comparison for two same-kind
        /// inputs.
        /// </summary>
        /// <param name="left">The first parsed unit.</param>
        /// <param name="right">The second parsed unit.</param>
        /// <returns><see langword="true"/> when both inputs represent the same logical content.</returns>
        private static bool semanticEquals(Unit left, Unit right)
        {
            if (left.Value is DrawInfo[] leftDraw && right.Value is DrawInfo[] rightDraw)
                return drawInfoEquals(leftDraw, rightDraw);
            if (left.Value is DynamicInfo leftDynamic && right.Value is DynamicInfo rightDynamic)
                return dynamicInfoEquals(leftDynamic, rightDynamic);
            if (left.Value is ResolvedPaletteEffectCollection leftPalette &&
                right.Value is ResolvedPaletteEffectCollection rightPalette)
                return paletteEffectEquals(leftPalette, rightPalette);
            return false;
        }

        /// <summary>
        /// Compares DrawInfo sequences by pose order, names, dynamic state, and every generated
        /// table.
        /// </summary>
        /// <param name="left">The first DrawInfo sequence.</param>
        /// <param name="right">The second DrawInfo sequence.</param>
        /// <returns><see langword="true"/> when both sequences are conceptually identical.</returns>
        private static bool drawInfoEquals(IReadOnlyList<DrawInfo> left, IReadOnlyList<DrawInfo> right)
        {
            if (left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                DrawInfo a = left[index];
                DrawInfo b = right[index];
                if (a.Name != b.Name || a.IsDynamic != b.IsDynamic ||
                    !arrayEquals(a.Tiles, b.Tiles) ||
                    !arrayEquals(a.Properties, b.Properties) ||
                    !arrayEquals(a.XDisplacements, b.XDisplacements) ||
                    !arrayEquals(a.YDisplacements, b.YDisplacements) ||
                    !arrayEquals(a.FlipXDisplacements, b.FlipXDisplacements) ||
                    !arrayEquals(a.FlipYDisplacements, b.FlipYDisplacements) ||
                    !arrayEquals(a.Sizes, b.Sizes))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares DynamicInfo tables and every declared resource list, including the referenced
        /// file contents.
        /// </summary>
        /// <param name="left">The first DynamicInfo.</param>
        /// <param name="right">The second DynamicInfo.</param>
        /// <returns><see langword="true"/> when both definitions and resources are equivalent.</returns>
        private static bool dynamicInfoEquals(DynamicInfo left, DynamicInfo right)
        {
            return arrayEquals(left.PosesChunksSizes, right.PosesChunksSizes) &&
                arrayEquals(left.PosesLastRow, right.PosesLastRow) &&
                resourceListsEqual(left, left.DeclaredPoseGraphics, right, right.DeclaredPoseGraphics) &&
                resourceListsEqual(left, left.DeclaredPalettes, right, right.DeclaredPalettes) &&
                resourceListsEqual(left, left.DeclaredResources, right, right.DeclaredResources);
        }

        /// <summary>
        /// Compares corresponding resource declarations by normalized name, file existence, byte
        /// length, and finally byte-for-byte content.
        /// </summary>
        /// <param name="leftInfo">The DynamicInfo used to resolve the first resource list.</param>
        /// <param name="left">The first declared resource list.</param>
        /// <param name="rightInfo">The DynamicInfo used to resolve the second resource list.</param>
        /// <param name="right">The second declared resource list.</param>
        /// <returns><see langword="true"/> when both lists reference equivalent files in the same order.</returns>
        private static bool resourceListsEqual(DynamicInfo leftInfo, string[]? left,
            DynamicInfo rightInfo, string[]? right)
        {
            left ??= [];
            right ??= [];
            if (left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
            {
                if (AsarSymbolName.FromPath(left[index]) != AsarSymbolName.FromPath(right[index]))
                    return false;
                string leftPath = leftInfo.ResolveResourcePath(left[index]);
                string rightPath = rightInfo.ResolveResourcePath(right[index]);
                FileInfo leftFile = new(leftPath);
                FileInfo rightFile = new(rightPath);
                if (!leftFile.Exists || !rightFile.Exists || leftFile.Length != rightFile.Length)
                    return false;
                if (!File.ReadAllBytes(leftPath).AsSpan().SequenceEqual(File.ReadAllBytes(rightPath)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares palette-effect collections by effect order, channels, ratios, and effect type.
        /// </summary>
        /// <param name="left">The first palette-effect collection.</param>
        /// <param name="right">The second palette-effect collection.</param>
        /// <returns><see langword="true"/> when both collections are conceptually identical.</returns>
        private static bool paletteEffectEquals(ResolvedPaletteEffectCollection left,
            ResolvedPaletteEffectCollection right)
        {
            if (left.Collection.Effects.Count != right.Collection.Effects.Count)
                return false;
            for (int index = 0; index < left.Collection.Effects.Count; index++)
            {
                PaletteEffect leftEffect = left.Collection.Effects[index];
                PaletteEffect rightEffect = right.Collection.Effects[index];
                if (leftEffect.EffectType != rightEffect.EffectType ||
                    leftEffect.Ratio1 != rightEffect.Ratio1 ||
                    leftEffect.Ratio2 != rightEffect.Ratio2 ||
                    leftEffect.Ratio3 != rightEffect.Ratio3)
                    return false;

                bool transition = leftEffect.EffectType is EffectType.PalTransitionRGB or
                    EffectType.PalTransitionHSL;
                if (!transition)
                {
                    if (leftEffect.Channel1 != rightEffect.Channel1 ||
                        leftEffect.Channel2 != rightEffect.Channel2 ||
                        leftEffect.Channel3 != rightEffect.Channel3)
                        return false;
                    continue;
                }

                if (AsarSymbolName.FromPath(leftEffect.Palette!) !=
                    AsarSymbolName.FromPath(rightEffect.Palette!))
                    return false;
                ResolvedPaletteEffectTarget leftTarget = left.Targets.First(
                    target => ReferenceEquals(target.Effect, leftEffect));
                ResolvedPaletteEffectTarget rightTarget = right.Targets.First(
                    target => ReferenceEquals(target.Effect, rightEffect));
                if (leftTarget.Format != rightTarget.Format ||
                    leftTarget.Data.Length != rightTarget.Data.Length ||
                    !leftTarget.Data.AsSpan().SequenceEqual(rightTarget.Data))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares nullable integer tables while treating <see langword="null"/> as an empty table.
        /// </summary>
        /// <param name="left">The first optional table.</param>
        /// <param name="right">The second optional table.</param>
        /// <returns><see langword="true"/> when both tables contain the same ordered values.</returns>
        private static bool arrayEquals(int[]? left, int[]? right)
            => (left ?? []).SequenceEqual(right ?? []);

        /// <summary>Classifies generated symbols so unrelated define families never collide.</summary>
        private enum SymbolKind
        {
            Context,
            Pose,
            Palette,
            Resource,
            Collection
        }

        /// <summary>
        /// Describes one generated symbol and the content that permits safe sharing with another
        /// DynamicInfo resource.
        /// </summary>
        private sealed class SymbolClaim
        {
            private readonly int _suffixIndex;
            private readonly byte[]? _data;
            private readonly int[] _metadata;

            /// <summary>Gets the generated-symbol category.</summary>
            public SymbolKind Kind { get; }
            /// <summary>Gets the unsuffixed generated name.</summary>
            public string BaseName { get; }

            /// <summary>Initializes a non-shareable context or collection claim.</summary>
            public SymbolClaim(SymbolKind kind, string baseName)
                : this(kind, baseName, -1, null, [])
            {
            }

            /// <summary>Initializes a shareable binary-resource claim.</summary>
            public SymbolClaim(SymbolKind kind, string baseName, int suffixIndex, byte[]? data,
                int[] metadata)
            {
                Kind = kind;
                BaseName = baseName;
                _suffixIndex = suffixIndex;
                _data = data;
                _metadata = metadata;
            }

            /// <summary>Builds the complete categorized symbol for original or resolved naming.</summary>
            public string GetSymbol(Unit unit, bool useLogicalName)
            {
                string logicalName = useLogicalName
                    ? unit.Input.LogicalName
                    : unit.Input.OriginalName;
                if (Kind is SymbolKind.Context or SymbolKind.Collection)
                    return $"{Kind}:{logicalName}";

                string suffix = logicalSuffix(unit.Input.OriginalName, logicalName);
                string name = _suffixIndex >= 0
                    ? BaseName.Insert(_suffixIndex, suffix)
                    : $"{BaseName}{suffix}";
                return $"{Kind}:{name}";
            }

            /// <summary>
            /// Determines whether two claims may safely use the same symbol and canonical binary.
            /// </summary>
            public bool IsEquivalentTo(SymbolClaim other)
                => Kind == other.Kind && _data != null && other._data != null &&
                    _metadata.SequenceEqual(other._metadata) &&
                    _data.Length == other._data.Length &&
                    _data.AsSpan().SequenceEqual(other._data);
        }

        /// <summary>
        /// Couples one physical input with its parsed value and the source paths merged into the
        /// same semantic unit.
        /// </summary>
        private sealed class Unit
        {
            /// <summary>Gets the mutable discovery metadata and final logical name.</summary>
            public InfoInput Input { get; }
            /// <summary>Gets the parsed DynamicInfo, DrawInfo array, or palette-effect collection.</summary>
            public object Value { get; }
            /// <summary>Gets the generated-symbol claims cached for collision analysis.</summary>
            public IReadOnlyList<SymbolClaim> Claims { get; }
            /// <summary>Gets every physical definition merged into this semantic unit.</summary>
            public List<string> SourcePaths { get; } = [];
            /// <summary>Gets the deterministic path used to order collision candidates.</summary>
            public string SortKey => Input.Origins.FirstOrDefault()?.AsmPath ?? Input.Path;

            /// <summary>
            /// Initializes a semantic unit with its first physical source.
            /// </summary>
            /// <param name="input">The discovery metadata for the physical input.</param>
            /// <param name="value">The parsed format-specific value.</param>
            public Unit(InfoInput input, object value)
            {
                Input = input;
                Value = value;
                Claims = createClaims(input, value);
                SourcePaths.Add(input.Path);
            }
        }
    }
}
