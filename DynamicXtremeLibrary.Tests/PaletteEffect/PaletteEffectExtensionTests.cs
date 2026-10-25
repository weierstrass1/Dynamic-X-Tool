using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremeLibrary.ResourceManagement;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;
using PaletteEffectModel = DynamicXtremePaletteCreatorLibrary.PaletteEffect;

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
            EffectType = EffectType.MixHSL,
            Channel1 = 7, Channel2 = 8, Channel3 = 9,
            Ratio1 = 10, Ratio2 = 11, Ratio3 = 12
        });

        byte[] result = PaletteEffectExtension.ToBin([collection]);

        Assert.Equal([
            2, 0,
            (byte)EffectType.MixRGB, (byte)EffectType.MixHSL,
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
            EffectType = EffectType.MixHSL,
            Channel1 = 7, Channel2 = 8, Channel3 = 9,
            Ratio1 = 10, Ratio2 = 11, Ratio3 = 12
        });

        PaletteEffectExtension.ToFile(new LogRegisterSystem(), [first, second], path);

        string result = File.ReadAllText(path);
        Assert.Contains(".Length\n\tdw $0002", result);
        Assert.Contains($".Types:\n\tdb ${(int)EffectType.MixRGB:X2},${(int)EffectType.MixHSL:X2}", result);
        Assert.Contains(".Value1\n\tdb $01,$07", result);
        Assert.Contains(".Value2\n\tdb $02,$08", result);
        Assert.Contains(".Value3\n\tdb $03,$09", result);
        Assert.Contains(".Ratio1\n\tdb $04,$0A", result);
        Assert.Contains(".Ratio2\n\tdb $05,$0B", result);
        Assert.Contains(".Ratio3\n\tdb $06,$0C", result);
    }

    [Fact]
    public void LoadCollection_IgnoresLegacyJsonNameAndAppliesToolLogicalName()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect", """
            {"Name":"LegacyWrongName",
            "Effects":[{"Channel1":31,"Channel2":0,"Channel3":1,
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
        const string valid = "{\"Name\":\"SameLegacyName\",\"Effects\":[]}";
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

    [Theory]
    [InlineData(EffectType.PalTransitionRGB, PaletteEffectPaletteFormat.RGB)]
    [InlineData(EffectType.PalTransitionHSL, PaletteEffectPaletteFormat.HSL)]
    public void LoadResolvedCollection_ResolvesAndConvertsTransitionPalette(
        EffectType effectType, PaletteEffectPaletteFormat expectedFormat)
    {
        using TestDirectory temp = new();
        string resources = temp.GetPath("resources");
        string palette = temp.CreateFile("resources/palettes/target.bin", [0x1F, 0x00]);
        string path = temp.CreateFile("effects/fade.paleffect", $$"""
            {"Name":"LegacyWrongName",
            "Effects":[{"Channel1":255,"Channel2":255,"Channel3":255,
            "Ratio1":127,"Ratio2":0,"Ratio3":1,"EffectType":{{(int)effectType}},
            "Palette":"palettes/target.bin"}]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", resources, new LogRegisterSystem(), out bool valid);

        Assert.True(valid);
        Assert.NotNull(result);
        Assert.Equal("Fade", result.OriginalName);
        Assert.Equal("Fade", result.LogicalName);
        Assert.Equal("Fade", result.Collection.Name);
        ResolvedPaletteEffectTarget target = Assert.Single(result.Targets);
        Assert.Equal(Path.GetFullPath(palette), target.SourcePath);
        Assert.Equal(expectedFormat, target.Format);
        Assert.Equal("Target", target.BaseName);
        Assert.Equal(expectedFormat == PaletteEffectPaletteFormat.RGB
            ? new byte[] { 31, 0, 0 }
            : new byte[] { 0, 31, 15 }, target.Data);
    }

    [Fact]
    public void LoadResolvedCollection_RequiresPaletteForTransitions()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("fade.paleffect", $$"""
            {"Effects":[{"EffectType":{{(int)EffectType.PalTransitionRGB}}}]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", temp.Path, new LogRegisterSystem(), out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Empty(result.Targets);
    }

    [Fact]
    public void LoadResolvedCollection_RejectsPaletteForChannelEffectsIncludingNone()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("fade.paleffect", $$"""
            {"Effects":[
              {"EffectType":{{(int)EffectType.MixRGB}},"Palette":"unused.pal"},
              {"EffectType":{{(int)EffectType.None}},"Palette":"unused.pal"}
            ]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", temp.Path, new LogRegisterSystem(), out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Empty(result.Targets);
    }

    [Theory]
    [InlineData("missing.pal")]
    [InlineData("empty.pal")]
    [InlineData("odd.pal")]
    public void LoadResolvedCollection_RejectsMissingEmptyAndOddLengthPalettes(string paletteName)
    {
        using TestDirectory temp = new();
        if (paletteName == "empty.pal")
            temp.CreateFile(paletteName, []);
        if (paletteName == "odd.pal")
            temp.CreateFile(paletteName, [0x1F]);
        string path = temp.CreateFile("fade.paleffect", $$"""
            {"Effects":[{"EffectType":{{(int)EffectType.PalTransitionRGB}},
            "Palette":"{{paletteName}}"}]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", temp.Path, new LogRegisterSystem(), out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Empty(result.Targets);
    }

    [Fact]
    public void LoadResolvedCollection_RejectsConvertedPaletteLargerThanOneResourceBuffer()
    {
        using TestDirectory temp = new();
        temp.CreateFile("large.pal", new byte[21842]);
        string path = temp.CreateFile("fade.paleffect", $$"""
            {"Effects":[{"EffectType":{{(int)EffectType.PalTransitionRGB}},
            "Palette":"large.pal"}]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", temp.Path, new LogRegisterSystem(), out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Empty(result.Targets);
    }

    [Fact]
    public void LoadResolvedCollection_RejectsSameGeneratedNameWithDifferentConvertedData()
    {
        using TestDirectory temp = new();
        temp.CreateFile("one/shared.pal", [0x1F, 0x00]);
        temp.CreateFile("two/shared.pal", [0xE0, 0x03]);
        string path = temp.CreateFile("fade.paleffect", $$"""
            {"Effects":[
              {"EffectType":{{(int)EffectType.PalTransitionRGB}},"Palette":"one/shared.pal"},
              {"EffectType":{{(int)EffectType.PalTransitionRGB}},"Palette":"two/shared.pal"}
            ]}
            """);

        ResolvedPaletteEffectCollection? result = PaletteEffectExtension.LoadResolvedCollection(
            path, "Fade", temp.Path, new LogRegisterSystem(), out bool valid);

        Assert.NotNull(result);
        Assert.False(valid);
        Assert.Equal(2, result.Targets.Count);
    }

    [Fact]
    public void CreateResources_DeduplicatesEqualDataWithinFormatAndPreservesAliasesAndBindings()
    {
        PaletteEffectModel firstEffect = new() { EffectType = EffectType.PalTransitionRGB, Palette = "a.pal" };
        PaletteEffectModel secondEffect = new() { EffectType = EffectType.PalTransitionRGB, Palette = "b.pal" };
        ResolvedPaletteEffectCollection first = Resolved("First", "FirstSprite", firstEffect,
            "Alpha", PaletteEffectPaletteFormat.RGB, [1, 2, 3]);
        ResolvedPaletteEffectCollection second = Resolved("Second", "SecondCluster", secondEffect,
            "Beta", PaletteEffectPaletteFormat.RGB, [1, 2, 3]);

        PaletteEffectResourceSet result = PaletteEffectExtension.CreateResources([first, second]);

        Resource resource = Assert.Single(result.Resources);
        Assert.Equal(ResourceType.PaletteEffectPalette, resource.Type);
        Assert.Equal(["AlphaSpriteRGB", "BetaClusterRGB"], resource.Names);
        Assert.Same(resource, result.Bindings[firstEffect].Resource);
        Assert.Same(resource, result.Bindings[secondEffect].Resource);
    }

    [Fact]
    public void CreateResources_KeepsRgbAndHslAsSeparateLogicalRepresentations()
    {
        PaletteEffectModel rgbEffect = new() { EffectType = EffectType.PalTransitionRGB, Palette = "a.pal" };
        PaletteEffectModel hslEffect = new() { EffectType = EffectType.PalTransitionHSL, Palette = "a.pal" };
        ResolvedPaletteEffectCollection rgb = Resolved("Rgb", "Rgb", rgbEffect,
            "Shared", PaletteEffectPaletteFormat.RGB, [0, 0, 0]);
        ResolvedPaletteEffectCollection hsl = Resolved("Hsl", "Hsl", hslEffect,
            "Shared", PaletteEffectPaletteFormat.HSL, [0, 0, 0]);

        PaletteEffectResourceSet result = PaletteEffectExtension.CreateResources([rgb, hsl]);

        Assert.Equal(2, result.Resources.Count);
        Assert.NotSame(result.Bindings[rgbEffect].Resource, result.Bindings[hslEffect].Resource);
        Assert.Equal(["SharedRGB", "SharedHSL"], result.Resources.Select(x => x.Name));
    }

    [Fact]
    public void CreateResources_RejectsSameFinalNameWithDifferentData()
    {
        PaletteEffectModel firstEffect = new() { EffectType = EffectType.PalTransitionRGB, Palette = "a.pal" };
        PaletteEffectModel secondEffect = new() { EffectType = EffectType.PalTransitionRGB, Palette = "b.pal" };
        ResolvedPaletteEffectCollection first = Resolved("First", "First", firstEffect,
            "Shared", PaletteEffectPaletteFormat.RGB, [1, 2, 3]);
        ResolvedPaletteEffectCollection second = Resolved("Second", "Second", secondEffect,
            "Shared", PaletteEffectPaletteFormat.RGB, [4, 5, 6]);

        Assert.Throws<InvalidOperationException>(() =>
            PaletteEffectExtension.CreateResources([first, second]));
    }

    [Fact]
    public void ToBin_UsesResolved24BitAddressForTransitionValues()
    {
        PaletteEffectModel transition = new()
        {
            EffectType = EffectType.PalTransitionRGB,
            Palette = "target.pal",
            Ratio1 = 4,
            Ratio2 = 5,
            Ratio3 = 6
        };
        PaletteEffectCollection collection = new() { Name = "Fade", Effects = [transition] };

        byte[] result = PaletteEffectExtension.ToBin([collection], effect =>
        {
            Assert.Same(transition, effect);
            return 0x123456;
        });

        Assert.Equal([
            1, 0, (byte)EffectType.PalTransitionRGB,
            0x56, 0x34, 0x12, 4, 5, 6
        ], result);
    }

    [Fact]
    public void ToBin_DoesNotSilentlySerializeUnresolvedTransitionPalette()
    {
        PaletteEffectCollection collection = new() { Name = "Fade" };
        collection.Effects.Add(new()
        {
            EffectType = EffectType.PalTransitionRGB,
            Palette = "target.pal"
        });

        Assert.Throws<InvalidOperationException>(() => PaletteEffectExtension.ToBin([collection]));
    }

    [Fact]
    public void ToFile_UsesGeneratedPaletteSymbolBytesForResolvedTransition()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("effects.asm");
        PaletteEffectModel effect = new()
        {
            EffectType = EffectType.PalTransitionHSL,
            Palette = "target.pal",
            Ratio1 = 1,
            Ratio2 = 2,
            Ratio3 = 3
        };
        ResolvedPaletteEffectCollection collection = Resolved("Fade", "FadeSprite", effect,
            "Target", PaletteEffectPaletteFormat.HSL, [0, 0, 0]);
        PaletteEffectResourceSet resources = PaletteEffectExtension.CreateResources([collection]);

        PaletteEffectExtension.ToFile(new LogRegisterSystem(), [collection], resources, path);

        string result = File.ReadAllText(path);
        Assert.Contains(".Value1\n\tdb (!PaletteTargetSpriteHSL)&$FF", result);
        Assert.Contains(".Value2\n\tdb (!PaletteTargetSpriteHSL>>8)&$FF", result);
        Assert.Contains(".Value3\n\tdb (!PaletteTargetSpriteHSL>>16)&$FF", result);
        Assert.Contains(".Ratio1\n\tdb $01", result);
        Assert.Contains(".Ratio2\n\tdb $02", result);
        Assert.Contains(".Ratio3\n\tdb $03", result);
    }

    private static ResolvedPaletteEffectCollection Resolved(string originalName,
        string logicalName, PaletteEffectModel effect, string baseName,
        PaletteEffectPaletteFormat format, byte[] data)
    {
        PaletteEffectCollection collection = new() { Name = logicalName, Effects = [effect] };
        return new()
        {
            Collection = collection,
            SourcePath = Path.GetFullPath($"{originalName}.paleffect"),
            ResourceDirectory = Path.GetFullPath("."),
            OriginalName = originalName,
            Targets =
            [
                new()
                {
                    Effect = effect,
                    SourcePath = Path.GetFullPath(effect.Palette!),
                    Format = format,
                    BaseName = baseName,
                    Data = data
                }
            ]
        };
    }
}
