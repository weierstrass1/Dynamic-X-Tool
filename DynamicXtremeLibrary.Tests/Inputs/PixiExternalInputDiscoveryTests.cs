using DynamicXtremeLibrary.Inputs;
using System.Text.Json;

namespace DynamicXtremeLibrary.Tests.Inputs;

public sealed class PixiExternalInputDiscoveryTests
{
    [Theory]
    [InlineData("00", "sprites")]
    [InlineData("BF", "sprites")]
    [InlineData("C0", "shooters")]
    [InlineData("CF", "shooters")]
    [InlineData("D0", "generators")]
    [InlineData("DF", "generators")]
    public void Discover_ResolvesNormalEntriesFromTheDirectorySelectedBySlot(
        string slot, string directory)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", $"SPRITE:\n{slot} entry.cfg\n");
        temp.CreateFile($"pixi/{directory}/entry.cfg", PixiCfg("01", "entry.asm"));
        string asmPath = temp.CreateFile($"pixi/{directory}/entry.asm",
            ";#incdri \"entry.drawinfo\"");
        string inputPath = temp.CreateFile($"pixi/{directory}/entry.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        InputOrigin origin = Assert.Single(input.Origins);
        Assert.Equal(Path.GetFullPath(asmPath), origin.AsmPath);
        Assert.Equal(ConsumerKind.Sprite, origin.ConsumerKind);
    }

    [Fact]
    public void Discover_ResolvesPerLevelSpriteFromNormalSpriteDirectory()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "SPRITE:\n105:B0 per_level.cfg\n");
        temp.CreateFile("pixi/sprites/per_level.cfg", PixiCfg("01", "per_level.asm"));
        temp.CreateFile("pixi/sprites/per_level.asm", ";#incdri \"per_level.drawinfo\"");
        string inputPath = temp.CreateFile("pixi/sprites/per_level.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        Assert.Equal(ConsumerKind.Sprite, input.Origins.Single().ConsumerKind);
        Assert.Equal("PerLevel", input.Origins.Single().ConsumerName);
    }

    [Theory]
    [InlineData("CLUSTER", "cluster", ConsumerKind.Cluster)]
    [InlineData("EXTENDED", "extended", ConsumerKind.Extended)]
    [InlineData("MINOREXTENDED", "misc_sprites/minorextended", ConsumerKind.MinorExtended)]
    [InlineData("BOUNCE", "misc_sprites/bounce", ConsumerKind.Bounce)]
    [InlineData("SMOKE", "misc_sprites/smoke", ConsumerKind.Smoke)]
    [InlineData("SPINNINGCOIN", "misc_sprites/spinningcoin", ConsumerKind.SpinningCoin)]
    [InlineData("SCORE", "misc_sprites/score", ConsumerKind.Score)]
    public void Discover_ResolvesOtherSpriteTypesFromTheirDocumentedDirectories(
        string section, string directory, ConsumerKind expectedKind)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", $"{section}:\n00 entry.asm\n");
        string asmPath = temp.CreateFile($"pixi/{directory}/entry.asm",
            ";#incdri \"entry.drawinfo\"");
        string inputPath = temp.CreateFile($"pixi/{directory}/entry.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        InputOrigin origin = Assert.Single(input.Origins);
        Assert.Equal(expectedKind, origin.ConsumerKind);
        Assert.Equal(Path.GetFullPath(asmPath), origin.AsmPath);
    }

    [Theory]
    [InlineData("display")]
    [InlineData("nodisplay")]
    public void Discover_AcceptsDocumentedCfgDisplayOption(string displayOption)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt",
            $"SPRITE:\n00 visible.cfg {displayOption} ; documented list comment\n");
        temp.CreateFile("pixi/sprites/visible.cfg", PixiCfg("01", "visible.asm"));
        temp.CreateFile("pixi/sprites/visible.asm", ";#incdri \"visible.drawinfo\"");
        temp.CreateFile("pixi/sprites/visible.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal("Visible", input.Origins.Single().ConsumerName);
    }

    [Fact]
    public void Discover_RejectsSpriteSlotOutsideDocumentedPixiRanges()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "SPRITE:\nE0 unsupported.cfg\n");
        temp.CreateFile("pixi/generators/unsupported.cfg", PixiCfg("01", "unsupported.asm"));
        temp.CreateFile("pixi/generators/unsupported.asm", ";#incdri \"unsupported.drawinfo\"");
        temp.CreateFile("pixi/generators/unsupported.drawinfo", "");

        Assert.Throws<InvalidDataException>(() => Discover(temp));
    }

    [Fact]
    public void Discover_RejectsMalformedSpriteSlot()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "SPRITE:\nG0 invalid.cfg\n");

        Assert.Throws<InvalidDataException>(() => Discover(temp));
    }

    [Fact]
    public void Discover_CfgUsesTypeAndFifthFieldAsPrimaryAsmFile()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 nested/entry.cfg\n");
        temp.CreateFile("pixi/sprites/nested/entry.cfg",
            "01\nwrong.asm\n00 00 00 00 00 00\n00 00\ncode/right.asm\n04:04\n");
        string asmPath = temp.CreateFile("pixi/sprites/nested/code/right.asm",
            ";#incdri \"../right.drawinfo\"");
        string inputPath = temp.CreateFile("pixi/sprites/nested/right.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        Assert.Equal(Path.GetFullPath(asmPath), input.Origins.Single().AsmPath);
    }

    [Fact]
    public void Discover_CfgTypeZeroHasNoPrimaryAsmAndIsSkipped()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 tweak.cfg\n");
        temp.CreateFile("pixi/sprites/tweak.cfg",
            "00\n36\n00 00 00 00 00 00\n00 00\n\n00:00\n");

        Assert.Empty(Discover(temp));
    }

    [Theory]
    [InlineData("GG\n36\n00 00 00 00 00 00\n00 00\nentry.asm\n00:00\n")]
    [InlineData("01\n36\n00 00 00 00 00 00\n00 00\n")]
    public void Discover_RejectsCfgWithoutValidTypeAndPrimaryAsm(string cfg)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 invalid.cfg\n");
        temp.CreateFile("pixi/sprites/invalid.cfg", cfg);

        Assert.Throws<InvalidDataException>(() => Discover(temp));
    }

    [Fact]
    public void Discover_JsonUsesNumericTypeAndAsmFileRelativeToDescriptor()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 nested/entry.json\n");
        temp.CreateFile("pixi/sprites/nested/entry.json",
            "{\"Type\":1,\"AsmFile\":\"code/right.asm\",\"Other\":\"wrong.asm\"}");
        string asmPath = temp.CreateFile("pixi/sprites/nested/code/right.asm",
            ";#incdri \"../right.drawinfo\"");
        string inputPath = temp.CreateFile("pixi/sprites/nested/right.drawinfo", "");

        InfoInput input = Assert.Single(Discover(temp));

        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        Assert.Equal(Path.GetFullPath(asmPath), input.Origins.Single().AsmPath);
    }

    [Fact]
    public void Discover_JsonTypeZeroHasNoPrimaryAsmAndIsSkipped()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 tweak.json\n");
        temp.CreateFile("pixi/sprites/tweak.json", "{\"Type\":0}");

        Assert.Empty(Discover(temp));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Type\":\"1\",\"AsmFile\":\"entry.asm\"}")]
    [InlineData("{\"Type\":256,\"AsmFile\":\"entry.asm\"}")]
    [InlineData("{\"Type\":-1,\"AsmFile\":\"entry.asm\"}")]
    public void Discover_ReportsJsonWithoutNumericByteTypeAsProblematicEntry(string json)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 invalid.json\n");
        temp.CreateFile("pixi/sprites/invalid.json", json);
        PixiEntryIssue? issue = null;

        Assert.Empty(Discover(temp, reported =>
        {
            issue = reported;
            return true;
        }));
        Assert.NotNull(issue);
        Assert.Equal("invalid.json", issue.ListedPath);
        Assert.False(string.IsNullOrWhiteSpace(issue.Reason));
    }

    [Theory]
    [InlineData("{\"Type\":1}")]
    [InlineData("{\"Type\":1,\"AsmFile\":null}")]
    [InlineData("{\"Type\":1,\"AsmFile\":42}")]
    [InlineData("{\"Type\":1,\"AsmFile\":\"\"}")]
    public void Discover_RejectsNonzeroJsonWithoutStringPrimaryAsm(string json)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 invalid.json\n");
        temp.CreateFile("pixi/sprites/invalid.json", json);

        Assert.Throws<InvalidDataException>(() => Discover(temp));
    }

    [Theory]
    [InlineData("invalid.cfg", "GG\n36\n00 00 00 00 00 00\n00 00\nentry.asm\n00:00\n")]
    [InlineData("invalid.json", "{\"Type\":1}")]
    public void Discover_IgnoreCallbackCanSkipInvalidDescriptor(
        string descriptorName, string descriptorContents)
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", $"00 {descriptorName}\n");
        temp.CreateFile($"pixi/sprites/{descriptorName}", descriptorContents);
        PixiEntryIssue? issue = null;

        Assert.Empty(Discover(temp, reported =>
        {
            issue = reported;
            return true;
        }));
        Assert.NotNull(issue);
        Assert.Equal(descriptorName, issue.ListedPath);
        Assert.False(string.IsNullOrWhiteSpace(issue.Reason));
    }

    [Fact]
    public void Discover_RejectsMalformedJson()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 invalid.json\n");
        temp.CreateFile("pixi/sprites/invalid.json", "{\"Type\":1,");

        Assert.ThrowsAny<JsonException>(() => Discover(temp));
    }

    [Fact]
    public void Discover_MissingPixiEntryAbortsWhenNoIgnoreCallbackApprovesIt()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "SPRITE:\n00 missing.cfg\n");

        Assert.Throws<FileNotFoundException>(() => Discover(temp));
    }

    [Fact]
    public void Discover_IgnoreCallbackReceivesEntryContextAndContinuesWithLaterEntries()
    {
        using TestDirectory temp = new();
        string listPath = temp.CreateFile("pixi/list.txt",
            "; heading comment\nSPRITE:\n00 missing.cfg\n01 valid.cfg\n");
        temp.CreateFile("pixi/sprites/valid.cfg", PixiCfg("01", "valid.asm"));
        temp.CreateFile("pixi/sprites/valid.asm", ";#incdri \"valid.drawinfo\"");
        temp.CreateFile("pixi/sprites/valid.drawinfo", "");
        PixiEntryIssue? captured = null;

        InfoInput input = Assert.Single(Discover(temp, issue =>
        {
            captured = issue;
            return true;
        }));

        Assert.Equal("Valid", input.Origins.Single().ConsumerName);
        Assert.NotNull(captured);
        Assert.Equal(Path.GetFullPath(listPath), captured.ListPath);
        Assert.Equal(3, captured.LineNumber);
        Assert.Equal("missing.cfg", captured.ListedPath);
        Assert.Equal(Path.GetFullPath(temp.GetPath("pixi/sprites/missing.cfg")), captured.ResolvedPath);
        Assert.False(string.IsNullOrWhiteSpace(captured.Reason));
    }

    [Fact]
    public void Discover_IgnoreCallbackCanDeclineAndPreserveTheFailure()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pixi/list.txt", "00 missing.cfg\n");
        int calls = 0;

        Assert.Throws<FileNotFoundException>(() => Discover(temp, _ =>
        {
            calls++;
            return false;
        }));
        Assert.Equal(1, calls);
    }

    private static IReadOnlyList<InfoInput> Discover(TestDirectory temp,
        Func<PixiEntryIssue, bool>? ignoreEntry = null) => ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), temp.GetPath("pixi"),
            null, null, ignoreEntry);

    private static string PixiCfg(string type, string asmFile) =>
        $"{type}\n36\n00 00 00 00 00 00\n00 00\n{asmFile}\n00:00\n";
}
