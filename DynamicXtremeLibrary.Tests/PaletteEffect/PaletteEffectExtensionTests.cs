using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.PaletteEffect;

public sealed class PaletteEffectExtensionTests
{
    [Fact]
    public void ToBin_ReturnsEmptyRuntimeTableForNoCollections()
        => Assert.Equal([0, 0, 0, 0], PaletteEffectExtension.ToBin([]));

    [Fact]
    public void ToBin_FiltersNoneAndWritesParallelArraysInCollectionOrder()
    {
        PaletteEffectCollection collection = new() { Name = "Effects" };
        collection.Effects.Add(new() { EffectType = EffectType.None });
        collection.Effects.Add(new()
        {
            EffectType = EffectType.MixRGB,
            Channel1 = 1, Channel2 = 2, Channel3 = 3,
            Ratio1 = 4, Ratio2 = 5, Ratio3 = 6
        });
        collection.Effects.Add(new()
        {
            EffectType = EffectType.PalTransitionHSL,
            Channel1 = 7, Channel2 = 8, Channel3 = 9,
            Ratio1 = 10, Ratio2 = 11, Ratio3 = 12
        });

        byte[] result = PaletteEffectExtension.ToBin([collection]);

        Assert.Equal([
            2, 0,
            (byte)EffectType.MixRGB, (byte)EffectType.PalTransitionHSL,
            1, 7, 2, 8, 3, 9, 4, 10, 5, 11, 6, 12
        ], result);
    }

    [Fact]
    public void ToFile_WritesEmptyTableWhenNoEffectsExist()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("effects.asm");

        PaletteEffectExtension.ToFile(new LogRegisterSystem(), [], path);

        Assert.Equal("PaletteEffectsTable:\n.Length\n\tdw $0000\n", File.ReadAllText(path));
    }

    [Fact]
    public void ToFile_FiltersNoneAndWritesParallelTablesInCollectionOrder()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("effects.asm");
        PaletteEffectCollection first = new() { Name = "First" };
        first.Effects.Add(new() { EffectType = EffectType.None });
        first.Effects.Add(new()
        {
            EffectType = EffectType.MixRGB,
            Channel1 = 1, Channel2 = 2, Channel3 = 3,
            Ratio1 = 4, Ratio2 = 5, Ratio3 = 6
        });
        PaletteEffectCollection second = new() { Name = "Second" };
        second.Effects.Add(new()
        {
            EffectType = EffectType.PalTransitionHSL,
            Channel1 = 7, Channel2 = 8, Channel3 = 9,
            Ratio1 = 10, Ratio2 = 11, Ratio3 = 12
        });

        PaletteEffectExtension.ToFile(new LogRegisterSystem(), [first, second], path);

        string result = File.ReadAllText(path);
        Assert.Contains(".Length\n\tdw $0002", result);
        Assert.Contains($".Types:\n\tdb ${(int)EffectType.MixRGB:X2},${(int)EffectType.PalTransitionHSL:X2}", result);
        Assert.Contains(".Value1\n\tdb $01,$07", result);
        Assert.Contains(".Value2\n\tdb $02,$08", result);
        Assert.Contains(".Value3\n\tdb $03,$09", result);
        Assert.Contains(".Ratio1\n\tdb $04,$0A", result);
        Assert.Contains(".Ratio2\n\tdb $05,$0B", result);
        Assert.Contains(".Ratio3\n\tdb $06,$0C", result);
    }

    [Fact]
    public void LoadCollection_LoadsValidJsonAndAppliesLogicalName()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect", """
            {"Effects":[{"Channel1":31,"Channel2":0,"Channel3":1,
            "Ratio1":127,"Ratio2":0,"Ratio3":1,"EffectType":0}]}
            """);

        PaletteEffectCollection? result = PaletteEffectExtension.LoadCollection(
            path, "Fade", new LogRegisterSystem(), out bool valid);

        Assert.True(valid);
        Assert.NotNull(result);
        Assert.Equal("Fade", result.Name);
        Assert.Single(result.Effects);
    }

    [Fact]
    public void LoadCollection_ReportsOutOfRangeValuesAndUnknownEffectType()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect", """
            {"Effects":[{"Channel1":32,"Channel2":0,"Channel3":0,
            "Ratio1":128,"Ratio2":0,"Ratio3":0,"EffectType":99}]}
            """);
        LogRegisterSystem log = new();

        PaletteEffectCollection? result = PaletteEffectExtension.LoadCollection(
            path, "Bad", log, out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Equal(3, log.GetRegisters().Count);
    }

    [Fact]
    public void LoadCollection_ReturnsNullForInvalidJson()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect", "not json");

        PaletteEffectCollection? result = PaletteEffectExtension.LoadCollection(
            path, "Bad", new LogRegisterSystem(), out bool valid);

        Assert.False(valid);
        Assert.Null(result);
    }

    [Fact]
    public void GetCollections_UsesFilenameOrderAndRejectsNormalizedNameCollisions()
    {
        using TestDirectory temp = new();
        const string valid = "{\"Effects\":[]}";
        temp.CreateFile("z.paleffect", valid);
        temp.CreateFile("a.paleffect", valid);

        IReadOnlyList<PaletteEffectCollection> ordered = PaletteEffectExtension.GetCollections(
            temp.Path, new LogRegisterSystem(), out bool orderedValid);

        Assert.True(orderedValid);
        Assert.Equal(["A", "Z"], ordered.Select(x => x.Name));

        temp.CreateFile("red-effect.paleffect", valid);
        temp.CreateFile("red_effect.paleffect", valid);
        _ = PaletteEffectExtension.GetCollections(temp.Path, new LogRegisterSystem(),
            out bool collisionValid);
        Assert.False(collisionValid);
    }
}
