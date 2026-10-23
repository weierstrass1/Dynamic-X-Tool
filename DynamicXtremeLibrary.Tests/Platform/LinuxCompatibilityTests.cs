using DynamicXtremeLibrary.Asar;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.ExternalTools;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.Inputs;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using LogRegister;
using System.Reflection;

namespace DynamicXtremeLibrary.Tests.Platform;

public sealed class LinuxCompatibilityTests
{
    [Theory]
    [InlineData("folder/subfolder/my-sprite.drawinfo")]
    [InlineData(@"folder\subfolder\my-sprite.drawinfo")]
    public void AsarSymbolName_AcceptsBothDirectorySeparatorStyles(string path)
        => Assert.Equal("MySprite", AsarSymbolName.FromPath(path));

    [Fact]
    public void OptionalFileOption_ResolvesForeignSeparatorsInBaseAndRelativePath()
    {
        using TestDirectory temp = new();
        string file = temp.CreateFile("tool/lists/list.txt", "data");
        OptionalFileOption option = new()
        {
            RelativeToDirectory = WithForeignSeparators(temp.GetPath("tool"))
        };

        string configuredPath = WithForeignSeparators(Path.Combine("lists", "list.txt"));

        Assert.Equal(Path.GetFullPath(file), option.ResolvePath(configuredPath));
        Assert.True(option.Validate(configuredPath));
    }

    [Fact]
    public void PathOption_ValidatesExistingRomWrittenWithForeignSeparators()
    {
        using TestDirectory temp = new();
        string rom = temp.CreateFile("roms/game.smc", [0]);

        Assert.True(new PathOption().Validate(WithForeignSeparators(rom)));
    }

    [Fact]
    public void PathOption_StoresForeignSeparatorPathRelativeToCurrentDirectory()
    {
        using TestDirectory temp = new();
        string rom = temp.GetPath("roms/game.smc");

        Assert.Equal(Path.GetRelativePath(".", rom),
            new PathOption().ParseFromString(WithForeignSeparators(rom)));
    }

    [Fact]
    public void DirectoryOption_AcceptsTheHostExecutableNamingConvention()
    {
        using TestDirectory temp = new();
        string executableName = OperatingSystem.IsWindows() ? "pixi.exe" : "pixi";
        string executable = temp.CreateFile($"tools/{executableName}", []);

        Assert.True(new DirectoryOption().Validate(WithForeignSeparators(executable)));
    }

    [Fact]
    public void DirectoryOption_StoresForeignSeparatorExecutableDirectory()
    {
        using TestDirectory temp = new();
        string executableName = OperatingSystem.IsWindows() ? "pixi.exe" : "pixi";
        string executable = temp.GetPath($"tools/{executableName}");

        Assert.Equal(Path.GetRelativePath(".", temp.GetPath("tools")),
            new DirectoryOption().ParseFromString(WithForeignSeparators(executable)));
    }

    [Fact]
    public void DynamicInfo_ResolvesAndValidatesResourcesWrittenWithEitherSeparatorStyle()
    {
        using TestDirectory temp = new();
        string resources = temp.GetPath("resources");
        string pose = temp.CreateFile("resources/gfx/pose.bin", new byte[32]);
        temp.CreateFile("resources/palettes/main.bin", [1, 2]);
        temp.CreateFile("resources/data/table.bin", [3]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = WithForeignSeparators(resources),
            PoseGraphics = [WithForeignSeparators("gfx/pose.bin")],
            PosesChunksSizes = [1, 0],
            Palettes = [WithForeignSeparators("palettes/main.bin")],
            Resources = [WithForeignSeparators("data/table.bin")]
        };

        Assert.Equal(Path.GetFullPath(pose), Path.GetFullPath(
            info.ResolveResourcePath(info.PoseGraphics[0])));
        Assert.True(info.Validate(WithForeignSeparators(resources), new LogRegisterSystem()));
    }

    [Fact]
    public void Discover_NormalizesSeparatorsAcrossEntirePixiPathChain()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("pixi");
        temp.CreateFile("pixi/lists/list.txt",
            $"SPRITE:\n00 {WithForeignSeparators("nested/entry.cfg")}\n");
        temp.CreateFile("pixi/sprites/nested/entry.cfg",
            PixiCfg("01", WithForeignSeparators("code/entry.asm")));
        string asm = temp.CreateFile("pixi/sprites/nested/code/entry.asm",
            $";#incdri \"{WithForeignSeparators("../inputs/enemy.drawinfo")}\"");
        string input = temp.CreateFile("pixi/sprites/nested/inputs/enemy.drawinfo", "");

        InfoInput discovered = Assert.Single(ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), WithForeignSeparators("lists/list.txt"),
            WithForeignSeparators(root), null, null));

        Assert.Equal(Path.GetFullPath(input), discovered.Path);
        Assert.Equal(Path.GetFullPath(asm), discovered.Origins.Single().AsmPath);
    }

    [Fact]
    public void Discover_NormalizesSeparatorsAcrossEntireUberasmPathChain()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("uber");
        temp.CreateFile("uber/lists/list.txt",
            $"LEVEL:\n105 {WithForeignSeparators("nested/code.asm")}\n");
        string asm = temp.CreateFile("uber/level/nested/code.asm",
            $";#incdri \"{WithForeignSeparators("../inputs/enemy.drawinfo")}\"");
        string input = temp.CreateFile("uber/level/inputs/enemy.drawinfo", "");

        InfoInput discovered = Assert.Single(ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), null, null,
            WithForeignSeparators("lists/list.txt"), WithForeignSeparators(root)));

        Assert.Equal(Path.GetFullPath(input), discovered.Path);
        Assert.Equal(Path.GetFullPath(asm), discovered.Origins.Single().AsmPath);
    }

    [Fact]
    public void Discover_MatchesTraditionalDefinitionExtensionsCaseInsensitively()
    {
        using TestDirectory temp = new();
        temp.CreateFile("definitions/enemy.DYNAMICINFO", "");
        temp.CreateFile("draw/enemy.DRAWINFO", "");
        temp.CreateFile("effects/fade.PALEFFECT", "{}");

        IReadOnlyList<InfoInput> discovered = ExternalInputDiscovery.Discover(
            temp.GetPath("definitions"), temp.GetPath("draw"), temp.GetPath("effects"),
            temp.GetPath("resources"), null, null, null, null);

        Assert.Equal(3, discovered.Count);
        Assert.Contains(discovered, input => input.Kind == InfoKind.DynamicInfo);
        Assert.Contains(discovered, input => input.Kind == InfoKind.DrawInfo);
        Assert.Contains(discovered, input => input.Kind == InfoKind.PaletteEffect);
    }

    [Fact]
    public void Discover_UsesHostFilenameCasingRulesForIncludedInputs()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return;

        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "CLUSTER:\n00 entry.asm\n");
        temp.CreateFile("pixi/cluster/entry.asm", ";#incdri \"case.drawinfo\"");
        temp.CreateFile("pixi/cluster/Case.drawinfo", "");

        Func<IReadOnlyList<InfoInput>> discover = () => ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), temp.GetPath("pixi"),
            null, null);

        if (OperatingSystem.IsLinux())
            Assert.Throws<FileNotFoundException>(() => discover());
        else
            Assert.Single(discover());
    }

    [LinuxFact]
    public void FileTransaction_RollsBackCaseDistinctFilesIndependentlyOnLinux()
    {
        using TestDirectory temp = new();
        string lower = temp.CreateFile("data.txt", "lower");
        string upper = temp.CreateFile("DATA.txt", "upper");

        using (FileTransaction transaction = new())
        {
            transaction.WriteAllText(lower, "changed lower");
            transaction.WriteAllText(upper, "changed upper");
        }

        Assert.Equal("lower", File.ReadAllText(lower));
        Assert.Equal("upper", File.ReadAllText(upper));
    }

    [Fact]
    public void ResourceNotFound_ExplainsLinuxFilenameCasingRequirementOnlyOnLinux()
    {
        string file = new ResourceNotFound("Enemy.bin").Parameters["file"];

        Assert.Equal(OperatingSystem.IsLinux(),
            file.Contains("exact filename casing", StringComparison.Ordinal));
    }

    [Fact]
    public void AsarPatch_SelectsAnExistingBinaryForTheHostPlatform()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return;

        MethodInfo ensureExecutable = typeof(AsarPatch).GetMethod("EnsureExecutable",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        string path = Assert.IsType<string>(ensureExecutable.Invoke(null, null));

        Assert.Equal(OperatingSystem.IsLinux() ? "asar" : "asar.exe", Path.GetFileName(path));
        Assert.True(File.Exists(path));
        if (OperatingSystem.IsLinux())
        {
            UnixFileMode executeBits = UnixFileMode.UserExecute |
                UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            Assert.Equal(executeBits, File.GetUnixFileMode(path) & executeBits);
        }
    }

    private static string WithForeignSeparators(string path)
        => path.Replace(Path.DirectorySeparatorChar,
            Path.DirectorySeparatorChar == '/' ? '\\' : '/');

    private static string PixiCfg(string type, string asmFile) =>
        $"{type}\n36\n00 00 00 00 00 00\n00 00\n{asmFile}\n00:00\n";
}

internal sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
            Skip = "This contract requires a case-sensitive Linux filesystem.";
    }
}
