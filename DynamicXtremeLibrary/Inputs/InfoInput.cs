namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Identifies the semantic format of a discovered Dynamic Xtreme input.
    /// </summary>
    public enum InfoKind
    {
        /// <summary>A DynamicInfo definition and its referenced binary resources.</summary>
        DynamicInfo,
        /// <summary>A DrawInfo definition containing drawable poses.</summary>
        DrawInfo,
        /// <summary>A palette-effect collection.</summary>
        PaletteEffect
    }

    /// <summary>
    /// Identifies the external-tool context that consumes a Dynamic Xtreme input.
    /// </summary>
    public enum ConsumerKind
    {
        /// <summary>A normal PIXI sprite.</summary>
        Sprite,
        /// <summary>A PIXI cluster sprite.</summary>
        Cluster,
        /// <summary>A PIXI extended sprite.</summary>
        Extended,
        /// <summary>A PIXI minor-extended sprite.</summary>
        MinorExtended,
        /// <summary>A PIXI bounce sprite.</summary>
        Bounce,
        /// <summary>A PIXI smoke sprite.</summary>
        Smoke,
        /// <summary>A PIXI spinning-coin sprite.</summary>
        SpinningCoin,
        /// <summary>A PIXI score sprite.</summary>
        Score,
        /// <summary>An UberASMTool level code file.</summary>
        Level,
        /// <summary>An UberASMTool overworld code file.</summary>
        Overworld,
        /// <summary>An UberASMTool game-mode code file.</summary>
        GameMode,
        /// <summary>An UberASMTool global code file.</summary>
        Global,
        /// <summary>An UberASMTool status-bar code file.</summary>
        StatusBar,
        /// <summary>An UberASMTool macro-library file.</summary>
        MacroLibrary
    }

    /// <summary>
    /// Describes one ASM consumer and the include directive through which it references an input.
    /// </summary>
    public sealed class InputOrigin
    {
        /// <summary>Gets the primary ASM file containing the include directive.</summary>
        public required string AsmPath { get; init; }
        /// <summary>Gets the external-tool category of the ASM consumer.</summary>
        public required ConsumerKind ConsumerKind { get; init; }
        /// <summary>Gets the normalized name of the consumer entry.</summary>
        public required string ConsumerName { get; init; }
        /// <summary>Gets the zero-based line containing the include directive.</summary>
        public required int DirectiveLine { get; init; }
        /// <summary>Gets the logical name persisted by a previous Dynamic Xtreme installation.</summary>
        public string? PreviousLogicalName { get; init; }
    }

    /// <summary>
    /// Represents one physically discovered input together with its logical identity and all known
    /// ASM consumers.
    /// </summary>
    public sealed class InfoInput
    {
        /// <summary>Gets the semantic input format.</summary>
        public required InfoKind Kind { get; init; }
        /// <summary>Gets the absolute path of the input definition.</summary>
        public required string Path { get; init; }
        /// <summary>Gets the base directory used to resolve resources referenced by the input.</summary>
        public required string ResourceDirectory { get; init; }
        /// <summary>Gets the normalized name derived from the original filename.</summary>
        public required string OriginalName { get; init; }
        /// <summary>Gets or sets the collision-free name used to generate Asar symbols.</summary>
        public string LogicalName { get; set; } = "";
        /// <summary>Gets the stable order in which this physical input was discovered.</summary>
        public int DiscoveryOrder { get; init; }
        /// <summary>Gets or sets whether the input came from a traditional Dynamic Xtreme directory.</summary>
        public bool IsTraditional { get; set; }
        /// <summary>Gets every external ASM location that references this physical input.</summary>
        public List<InputOrigin> Origins { get; } = [];
    }

    /// <summary>
    /// Describes a set of distinct inputs whose generated Asar symbols collide and the names
    /// proposed to separate them.
    /// </summary>
    public sealed class NameCollision
    {
        /// <summary>Gets the common normalized name that initiated collision resolution.</summary>
        public required string GeneratedName { get; init; }
        /// <summary>Gets each colliding input together with its files and proposed name.</summary>
        public required IReadOnlyList<NameCollisionCandidate> Candidates { get; init; }
        /// <summary>Gets the input files participating in the collision.</summary>
        public IReadOnlyList<string> Files => Candidates.SelectMany(candidate => candidate.Files).ToArray();
        /// <summary>Gets the collision-free logical names proposed for those files.</summary>
        public IReadOnlyList<string> ProposedNames => Candidates.Select(candidate => candidate.ProposedName).ToArray();
    }

    /// <summary>Associates one colliding input with its proposed name and physical definitions.</summary>
    public sealed class NameCollisionCandidate
    {
        /// <summary>Gets the normalized name currently generated by the input.</summary>
        public required string OriginalName { get; init; }
        /// <summary>Gets the collision-free logical name proposed for the input.</summary>
        public required string ProposedName { get; init; }
        /// <summary>Gets every equivalent physical definition represented by the input.</summary>
        public required IReadOnlyList<string> Files { get; init; }
    }
}
