using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.ResourceManagement;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Infos;

public sealed class DynamicInfoTests
{
    [Fact]
    public void SetLogicalName_UpdatesContextAndDerivesSuffix()
    {
        DynamicInfo info = new("Enemy");

        info.SetLogicalName("Enemy", "EnemySprite");

        Assert.Equal("EnemySprite", info.ContextName);
        Assert.Equal("Sprite", info.SymbolSuffix);
        Assert.Equal("RedPaletteSprite", info.GetPaletteName("red-palette.bin"));
    }

    [Fact]
    public void SetLogicalName_UsesWholeLogicalNameWhenRootDiffers()
    {
        DynamicInfo info = new("Enemy");

        info.SetLogicalName("Enemy", "Player");

        Assert.Equal("Player", info.SymbolSuffix);
    }

    [Fact]
    public void GeneratedResourceNames_NormalizeFilenameAndApplyResolvedOrExplicitSuffix()
    {
        DynamicInfo info = new("Enemy");
        info.SetLogicalName("Enemy", "EnemySprite");

        Assert.Equal("WalkFramesSprite", info.GetPoseGraphicName("gfx/walk-frames.bin"));
        Assert.Equal("WalkFramesAlt", info.GetPoseGraphicName("gfx/walk-frames.bin", "Alt"));
        Assert.Equal("RedPaletteSprite", info.GetPaletteName("pal/red-palette.bin"));
        Assert.Equal("RedPaletteAlt", info.GetPaletteName("pal/red-palette.bin", "Alt"));
        Assert.Equal("HitboxSprite", info.GetResourceName("data/hitbox.bin"));
        Assert.Equal("HitboxAlt", info.GetResourceName("data/hitbox.bin", "Alt"));
    }

    [Fact]
    public void ResolveResourcePath_PreservesAbsoluteAndResolvesRelativePaths()
    {
        using TestDirectory temp = new();
        DynamicInfo info = new("Enemy") { ResourceDirectory = temp.Path };
        string absolute = Path.GetFullPath(temp.GetPath("absolute.bin"));

        Assert.Equal(Path.Combine(temp.Path, "relative.bin"),
            info.ResolveResourcePath("relative.bin"));
        Assert.Equal(absolute, info.ResolveResourcePath(absolute));
    }

    [Fact]
    public void PosePaletteAndResourceLengths_HandleNullAndPopulatedCollections()
    {
        DynamicInfo info = new("Enemy");
        Assert.Equal(0, info.PoseLength);
        Assert.Equal(0, info.PaletteLength);
        Assert.Equal(0, info.ResourcesLength);

        info.PosesChunksSizes = [1, 0, 2, 0];
        info.Palettes = ["a", "b"];
        info.Resources = ["r"];

        Assert.Equal(2, info.PoseLength);
        Assert.Equal(2, info.PaletteLength);
        Assert.Equal(1, info.ResourcesLength);
    }

    [Fact]
    public void Validate_AcceptsExistingResourcesWithMatchingPoseSize()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[64]);
        temp.CreateFile("palette.bin", new byte[32]);
        temp.CreateFile("resource.bin", new byte[10]);
        DynamicInfo info = new("Enemy")
        {
            PoseGraphics = ["pose.bin"],
            PosesChunksSizes = [1, 1],
            Palettes = ["palette.bin"],
            Resources = ["resource.bin"]
        };

        Assert.True(info.Validate(temp.Path, new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsMissingResourcesOversizedFilesAndPoseSizeMismatch()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[32]);
        temp.CreateFile("large.bin", new byte[32761]);
        DynamicInfo info = new("Enemy")
        {
            PoseGraphics = ["pose.bin"],
            PosesChunksSizes = [2, 0],
            Palettes = ["missing.bin"],
            Resources = ["large.bin"]
        };

        Assert.False(info.Validate(temp.Path, new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsPoseGraphicsWithoutChunkDeclarations()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[32]);
        DynamicInfo info = new("Enemy") { PoseGraphics = ["pose.bin"] };

        Assert.False(info.Validate(temp.Path, new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsOddNumberOfChunkValues()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[32]);
        DynamicInfo info = new("Enemy")
        {
            PoseGraphics = ["pose.bin"],
            PosesChunksSizes = [1]
        };

        Assert.False(info.Validate(temp.Path, new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsNegativeChunkCounts()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[32]);
        DynamicInfo info = new("Enemy")
        {
            PoseGraphics = ["pose.bin"],
            PosesChunksSizes = [2, -1]
        };

        Assert.False(info.Validate(temp.Path, new LogRegisterSystem()));
    }

    [Fact]
    public void GetPoseSizes_ReturnsBytesForEachChunkPair()
    {
        DynamicInfo info = new("Enemy") { PosesChunksSizes = [1, 2, 3, 4] };

        Assert.Equal([96, 224], info.GetPosesSizes());
        Assert.Equal(96, info.GetPoseSize(0));
        Assert.Equal(-1, info.GetPoseSize(2));
    }

    [Fact]
    public void GetPoseSize_NegativeIdIsInvalid()
    {
        DynamicInfo info = new("Enemy") { PosesChunksSizes = [1, 0] };

        Assert.Equal(-1, info.GetPoseSize(-1));
    }

    [Fact]
    public void GetPoseBlocks_NegativeIdIsInvalid()
    {
        DynamicInfo info = new("Enemy") { PosesChunksSizes = [1, 0] };

        Assert.Equal(-1, info.GetPoseBlocks(-1));
    }

    [Fact]
    public void GetPoseBlocks_UsesFullRowsAndRoundedLastRow()
    {
        DynamicInfo info = new("Enemy")
        {
            PosesChunksSizes = [32, 0, 16, 0, 17, 0, 33, 1]
        };

        Assert.Equal([8, 8, 8, 9], info.GetPosesBlocks());
        Assert.Equal(-1, info.GetPoseBlocks(4));
    }

    [Fact]
    public void GetPoseBlocks_HandlesEveryLastRowBoundary()
    {
        DynamicInfo info = new("Enemy")
        {
            PosesChunksSizes = [0, 0, 1, 0, 2, 0, 15, 0, 16, 0, 17, 0, 31, 0, 32, 0]
        };

        Assert.Equal([0, 1, 1, 8, 8, 8, 8, 8], info.GetPosesBlocks());
    }

    [Fact]
    public void GenerateLastRow_DerivesMetadataFromUpperChunkRows()
    {
        DynamicInfo info = new("Enemy") { PosesChunksSizes = [0, 1, 32, 0, 64, 0] };

        info.GenerateLastRow();

        Assert.Equal([16, 48, 80], info.PosesLastRow);
        Assert.Equal([256, 768, 1280], info.GetLastRow());
    }

    [Fact]
    public void GetLastRow_RepeatsSingleConfiguredValueForEveryPose()
    {
        DynamicInfo info = new("Enemy")
        {
            PosesChunksSizes = [1, 0, 1, 0],
            PosesLastRow = [3]
        };

        Assert.Equal([48, 48], info.GetLastRow());
    }

    [Fact]
    public void ResourceLoaders_DeduplicateNamesAndAssignConsecutiveIds()
    {
        using TestDirectory temp = new();
        temp.CreateFile("a.bin", [1]);
        temp.CreateFile("b.bin", [2]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            Palettes = ["a.bin", "a.bin", "b.bin"],
            Resources = ["a.bin", "a.bin"]
        };

        IReadOnlyDictionary<string, Resource> palettes = info.GetPaletteData(5,
            new Dictionary<string, Resource>());
        IReadOnlyDictionary<string, Resource> resources = info.GetResourceData(9,
            new Dictionary<string, Resource>());

        Assert.Equal([5, 6], palettes.Values.Select(x => x.ID));
        Assert.Equal(["A", "B"], palettes.Keys);
        Assert.Single(resources);
        Assert.Equal(9, resources["A"].ID);
    }

    [Fact]
    public void StaticResourceLoaders_DeduplicateAcrossContextsAndKeepConsecutiveIds()
    {
        using TestDirectory temp = new();
        temp.CreateFile("shared.bin", [1]);
        temp.CreateFile("unique.bin", [2]);
        DynamicInfo first = new("First")
        {
            ResourceDirectory = temp.Path,
            Palettes = ["shared.bin"],
            Resources = ["shared.bin"]
        };
        DynamicInfo second = new("Second")
        {
            ResourceDirectory = temp.Path,
            Palettes = ["shared.bin", "unique.bin"],
            Resources = ["shared.bin", "unique.bin"]
        };

        var palettes = DynamicInfo.GetPaletteData([first, second]);
        var resources = DynamicInfo.GetResourceData([first, second]);

        Assert.Equal(["Shared", "Unique"], palettes.Select(resource => resource.Name));
        Assert.Equal([0, 1], palettes.Select(resource => resource.ID));
        Assert.Equal(["Shared", "Unique"], resources.Select(resource => resource.Name));
        Assert.Equal([0, 1], resources.Select(resource => resource.ID));
    }

    [Fact]
    public void GetAllResources_ObeysFeatureGatesForEachResourceCategory()
    {
        using TestDirectory temp = new();
        temp.CreateFile("pose.bin", new byte[32]);
        temp.CreateFile("palette.bin", [1]);
        temp.CreateFile("resource.bin", [2]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            PoseGraphics = ["pose.bin"],
            PosesChunksSizes = [1, 0],
            Palettes = ["palette.bin"],
            Resources = ["resource.bin"]
        };
        using CurrentDirectoryScope cwd = new(AppContext.BaseDirectory);
        var options = DynamicXtremeLibrary.Config.Options.Instance.BoolOptions;
        Dictionary<DynamicXtremeLibrary.Config.BoolOption, bool> original = options
            .ToDictionary(option => option, option => option.Value);
        try
        {
            foreach (var option in options)
                option.Value = false;
            Assert.Empty(DynamicInfo.GetAllResources([info]));

            options.Single(option => option.Name == "DynamicPoses").Value = true;
            Assert.Equal([ResourceType.DynamicPose],
                DynamicInfo.GetAllResources([info]).Select(resource => resource.Type));

            options.Single(option => option.Name == "DynamicPoses").Value = false;
            options.Single(option => option.Name == "PalettesChange").Value = true;
            Assert.Equal([ResourceType.Palette, ResourceType.GeneralResource],
                DynamicInfo.GetAllResources([info]).Select(resource => resource.Type));

            options.Single(option => option.Name == "PalettesChange").Value = false;
            options.Single(option => option.Name == "GraphicsChange").Value = true;
            Assert.Equal([ResourceType.GeneralResource],
                DynamicInfo.GetAllResources([info]).Select(resource => resource.Type));
        }
        finally
        {
            foreach (var pair in original)
                pair.Key.Value = pair.Value;
        }
    }

    [Fact]
    public void GetPosesData_SplitsCombinedGraphicsAndNamesEachSourceSequence()
    {
        using TestDirectory temp = new();
        temp.CreateFile("first.bin", [.. Enumerable.Repeat((byte)1, 32)]);
        temp.CreateFile("second.bin", [.. Enumerable.Repeat((byte)2, 64)]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            PoseGraphics = ["first.bin", "second.bin"],
            PosesChunksSizes = [1, 0, 2, 0]
        };

        IReadOnlyList<Resource> poses = info.GetPosesData(10);

        Assert.Equal([10, 11], poses.Select(x => x.ID));
        Assert.Equal(["First000", "Second000"], poses.Select(x => x.Name));
        Assert.All(poses[0].Data, value => Assert.Equal(1, value));
        Assert.All(poses[1].Data, value => Assert.Equal(2, value));
    }

    [Fact]
    public void GetPoseResourceNames_UsesSameFileBoundariesAsPoseExtraction()
    {
        using TestDirectory temp = new();
        temp.CreateFile("first.bin", new byte[64]);
        temp.CreateFile("second.bin", new byte[96]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            PoseGraphics = ["first.bin", "second.bin"],
            PosesChunksSizes = [1, 0, 1, 0, 3, 0]
        };

        Assert.Equal(["FirstAlt000", "FirstAlt001", "SecondAlt000"],
            info.GetPoseResourceNames("Alt"));
    }

    [Fact]
    public void FromNumberOf16x16Tiles_CreatesConvertedGraphicsAndChunkMetadata()
    {
        using TestDirectory temp = new();
        byte[] source = [.. Enumerable.Range(0, 1024).Select(i => (byte)i)];
        temp.CreateFile("sheet.bin", source);
        string generated = temp.GetPath("generated");
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            TemporaryDirectory = generated,
            PoseGraphics = ["sheet.bin"]
        };

        info.FromNumberOf16x16Tiles(new Dictionary<int, string> { [0] = "1" });

        Assert.Equal([2, 2], info.PosesChunksSizes);
        Assert.Equal([16], info.PosesLastRow);
        string output = Assert.Single(info.PoseGraphics!);
        Assert.True(File.Exists(output));
        Assert.Equal(128, new FileInfo(output).Length);
    }

    [Fact]
    public void FromNumberOf16x16Tiles_EncodesQuarterHalfAndThreeQuarterModifiers()
    {
        using TestDirectory temp = new();
        temp.CreateFile("sheet.bin", [.. Enumerable.Repeat((byte)1, 1024)]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            TemporaryDirectory = temp.GetPath("generated"),
            PoseGraphics = ["sheet.bin"]
        };

        info.FromNumberOf16x16Tiles(new Dictionary<int, string>
        {
            [0] = "1",
            [1] = "1q",
            [2] = "1h",
            [3] = "1q3"
        });

        Assert.Equal([2, 2, 3, 2, 4, 2, 4, 3], info.PosesChunksSizes);
        Assert.Equal(704, new FileInfo(Assert.Single(info.PoseGraphics!)).Length);
    }

    [Fact]
    public void FromNumberOf16x16Tiles_InvalidGraphicDoesNotReplaceDeclarations()
    {
        using TestDirectory temp = new();
        temp.CreateFile("bad.bin", new byte[100]);
        DynamicInfo info = new("Enemy")
        {
            ResourceDirectory = temp.Path,
            TemporaryDirectory = temp.GetPath("generated"),
            PoseGraphics = ["bad.bin"]
        };

        info.FromNumberOf16x16Tiles(new Dictionary<int, string> { [0] = "1" });

        Assert.Equal(["bad.bin"], info.PoseGraphics);
        Assert.Null(info.PosesChunksSizes);
        Assert.False(Directory.Exists(info.TemporaryDirectory));
    }
}
