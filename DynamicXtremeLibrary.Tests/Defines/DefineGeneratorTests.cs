using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;
using SNESLibrary;

namespace DynamicXtremeLibrary.Tests.Defines;

public sealed class DefineGeneratorTests
{
    [Fact]
    public void Constructor_RequiresExistingTemplate()
    {
        using TestDirectory temp = new();

        Assert.Throws<FileNotFoundException>(() => new DefineGenerator(
            temp.GetPath("missing.asm"), temp.GetPath("out.asm"), temp.GetPath("external.asm")));
    }

    [Fact]
    public void GeneratePaletteOffsets_UsesContextOrderAndTotalPaletteCount()
    {
        DynamicInfo first = new("First") { Palettes = ["a", "b"] };
        DynamicInfo skipped = new("Skipped") { Palettes = null };
        DynamicInfo second = new("Second") { Palettes = ["c"] };

        string result = DefineGenerator.GeneratePaletteOffsets([first, skipped, second]);

        Assert.Contains("!PaletteTableSize = $0003", result);
        Assert.Contains("!FirstPaletteTableOffset = $0000", result);
        Assert.Contains("!SecondPaletteTableOffset = $0002", result);
        Assert.DoesNotContain("SkippedPaletteTableOffset", result);
    }

    [Fact]
    public void GeneratePaletteOffsets_ReturnsEmptyForNullOrEmptyInput()
    {
        Assert.Equal("", DefineGenerator.GeneratePaletteOffsets(null));
        Assert.Equal("", DefineGenerator.GeneratePaletteOffsets([]));
    }

    [Fact]
    public void GeneratePoseDefines_EmitsIdsAndPerContextMaximumRenderBoxes()
    {
        DrawInfo first = new(2, "Enemy", "Idle")
        {
            XDisplacements = [0], YDisplacements = [0], Sizes = [0]
        };
        DrawInfo second = new(3, "Enemy", "Wide")
        {
            XDisplacements = [32], YDisplacements = [0xF0], Sizes = [2]
        };

        string result = DefineGenerator.GeneratePoseDefines([first, second]);

        Assert.Contains("!NumberOfPoses = $0002", result);
        Assert.Contains("!PoseIDEnemy_Idle = $0002", result);
        Assert.Contains("!PoseIDEnemy_Wide = $0003", result);
        Assert.Contains("!RenderBoxXDistanceOutOfScreenEnemy = $0030", result);
        Assert.Contains("!RenderBoxYDistanceOutOfScreenEnemy = $0010", result);
    }

    [Fact]
    public void GeneratePaletteEffectDefines_SkipsNoneAndAssignsGlobalConsecutiveIdsFromOne()
    {
        PaletteEffectCollection first = new() { Name = "Fade" };
        first.Effects.Add(new() { EffectType = EffectType.None });
        first.Effects.Add(new() { EffectType = EffectType.MixRGB });
        PaletteEffectCollection second = new() { Name = "Flash" };
        second.Effects.Add(new() { EffectType = EffectType.MixHSL });

        string result = DefineGenerator.GeneratePaletteEffectDefines([first, second]);

        Assert.Contains("!NumberOfPaletteEffects = $0003", result);
        Assert.Contains("!PaletteEffectIDFade0 = $0001", result);
        Assert.Contains("!PaletteEffectIDFlash0 = $0002", result);
        Assert.DoesNotContain("Fade1", result);
    }

    [Fact]
    public void GenerateReferenceDefines_EmitsCountsIdsAddressesAndSizesByCategory()
    {
        ResourceReference buffer = Ref(3, "Buffer3", ResourceType.Buffer, 0x123456, 4);
        ResourceReference pose = Ref(2, "Pose", ResourceType.DynamicPose, 0x202, 32);
        ResourceReference palette = Ref(1, "Pal", ResourceType.Palette, 0x402, 16);
        ResourceReference resource = Ref(0, "Data", ResourceType.GeneralResource, 0x602, 8);
        ResourceAllReferences references = new()
        {
            Buffers = [buffer], DynamicPoses = [pose], Palettes = [palette],
            GeneralResources = [resource]
        };

        string result = DefineGenerator.GenerateReferenceDefines(references, Mapper.NoROM);

        Assert.Contains("!NumberOfBuffers = $0001", result);
        Assert.Contains("!BufferIDBuffer3 = $0003", result);
        Assert.Contains("!BufferBuffer3 = $123456", result);
        Assert.Contains("!DynamicPoseIDPose = $0002", result);
        Assert.Contains("!DynamicPosePose = $000002", result);
        Assert.Contains("!PalettePalSize = $0010", result);
        Assert.Contains("!ResourceDataSize = $0008", result);
    }

    [Fact]
    public void GenerateDefinesFile_WritesIdenticalNormalizedOutputs()
    {
        using TestDirectory temp = new();
        string template = temp.CreateFile("base.asm", "base\r\n\r\n\r\nvalue\r\n");
        string output = temp.GetPath("patch.asm");
        string external = temp.GetPath("external.asm");
        DefineGenerator generator = new(template, output, external);

        generator.GenerateDefinesFile(null, Mapper.LoROM, null, null, null);

        Assert.Equal(File.ReadAllText(output), File.ReadAllText(external));
        Assert.DoesNotContain("\r", File.ReadAllText(output));
        Assert.DoesNotContain("\n\n\n", File.ReadAllText(output));
    }

    private static ResourceReference Ref(int id, string name, ResourceType type,
        int position, int length) => new(0, position,
            new Resource(id, name, type, new byte[length]));
}
