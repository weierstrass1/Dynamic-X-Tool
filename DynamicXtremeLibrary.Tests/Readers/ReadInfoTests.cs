using DynamicXtremeLibrary.Readers;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Readers;

public sealed class ReadInfoTests
{
    [Fact]
    public void CleanFileContent_RemovesCommentsAndNormalizesWhitespaceAndNewlines()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("input.drawinfo",
            "  Label:   ; comment\r\n\t db   $01,   $02  \r\n");

        Assert.Equal("Label:\ndb $01, $02\n", ReadInfo.CleanFileContent(path));
    }

    [Fact]
    public void ReadCurrentDrawInfo_ExpandsPoseRangeDefaultsAndPivots()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", """
            Walk_Pose0..1:
            .NumberOfTiles: 2
            .TileCodes: $00,$01
            .Properties: $00,$40
            .XOffsets: $00,$08
            .YOffsets: $00,$F8
            .Sizes: $02,$00
            .XPivot: $10
            .YPivot: $10
            .IsStatic
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.ReadDrawInfo(path, 5, out bool valid);

        Assert.True(valid);
        Assert.Equal(2, result.Length);
        Assert.Equal([5, 6], result.Select(x => x.ID));
        Assert.Equal(["Walk0", "Walk1"], result.Select(x => x.Name));
        Assert.All(result, pose => Assert.False(pose.IsDynamic));
        Assert.Equal([8, 8], result[0].FlipXDisplacements);
        Assert.Equal([8, 24], result[0].FlipYDisplacements);
    }

    [Fact]
    public void ReadCurrentDrawInfo_LaterBlockOverridesSameContextAndPoseNameWithoutChangingId()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", """
            Idle_Pose0:
            .NumberOfTiles: 1
            .Property0: $01
            Idle_Pose0:
            .NumberOfTiles: 1
            .Property0: $02
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.ReadDrawInfo(path, 7, out bool valid);

        Assert.True(valid);
        var pose = Assert.Single(result);
        Assert.Equal(7, pose.ID);
        Assert.Equal([2], pose.Properties);
    }

    [Fact]
    public void ReadLegacyDrawInfo_CombinesNamedTablesAndDynamicFlag()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", """
            Dynamic:
            true
            Tiles:
            Idle_Tiles:
            db $00,$01
            Properties:
            Idle_Properties:
            db $00,$40
            XDisplacements:
            Idle_XDisp:
            db $00,$08
            Idle_XDispFlipX:
            db $08,$00
            Sizes:
            Idle_Sizes:
            db $02,$02
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.ReadDrawInfo(path, 3, out bool valid);

        Assert.True(valid);
        var pose = Assert.Single(result);
        Assert.True(pose.IsDynamic);
        Assert.Equal(3, pose.ID);
        Assert.Equal("Enemy", pose.ContextName);
        Assert.Equal([0, 1], pose.Tiles);
        Assert.Equal([8, 0], pose.FlipXDisplacements);
    }

    [Fact]
    public void ReadDrawInfo_RejectsMixedLegacyAndCurrentFormats()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", """
            Dynamic:
            true
            Idle_Pose0:
            .NumberOfTiles: 1
            """);

        var result = new ReadInfo(new LogRegisterSystem())
            .ReadDrawInfo(path, 0, out bool valid);

        Assert.False(valid);
        Assert.Empty(result);
    }

    [Fact]
    public void ReadDrawInfo_RejectsInvalidSizeAndPluralValueCount()
    {
        using TestDirectory temp = new();
        string badSize = temp.CreateFile("size.drawinfo", """
            Idle_Pose0:
            .NumberOfTiles: 1
            .Size0: 1
            """);
        string badCount = temp.CreateFile("count.drawinfo", """
            Idle_Pose0:
            .NumberOfTiles: 2
            .TileCodes: $00
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        Assert.Empty(reader.ReadDrawInfo(badSize, 0, out bool sizeValid));
        Assert.False(sizeValid);
        Assert.Empty(reader.ReadDrawInfo(badCount, 0, out bool countValid));
        Assert.False(countValid);
    }

    [Fact]
    public void ReadLegacyDrawInfo_EmptyDynamicSectionReportsValidationFailureWithoutThrowing()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", "Dynamic:\n");
        ReadInfo reader = new(new LogRegisterSystem());

        Exception? exception = Record.Exception(() =>
        {
            var result = reader.ReadDrawInfo(path, 0, out bool valid);
            Assert.False(valid);
            Assert.Empty(result);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void ReadDynamicInfo_ParsesLegacySectionsAndKeepsDeclaredLists()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.dynamicinfo", """
            PosesGraphics:
            poses.bin
            Palettes:
            palette.bin
            Resources:
            misc.bin
            PosesChunksSizes:
            Enemy0_PoseChunksSizes:
            db $01,$02
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.ReadDynamicInfo(path, temp.Path, temp.GetPath("tmp"), out bool valid);

        Assert.True(valid);
        Assert.NotNull(result);
        Assert.Equal("Enemy", result.ContextName);
        Assert.Equal(["poses.bin"], result.DeclaredPoseGraphics);
        Assert.Equal(["palette.bin"], result.DeclaredPalettes);
        Assert.Equal(["misc.bin"], result.DeclaredResources);
        Assert.Equal([1, 2], result.PosesChunksSizes);
        Assert.Equal([16], result.PosesLastRow);
    }

    [Fact]
    public void ReadDynamicInfo_RejectsRepeatedSectionsAndMixedChunkFormats()
    {
        using TestDirectory temp = new();
        string repeated = temp.CreateFile("repeated.dynamicinfo", "Palettes:\na.bin\nPalettes:\nb.bin");
        string mixed = temp.CreateFile("mixed.dynamicinfo", """
            PosesChunksSizes:
            Enemy0_PoseChunksSizes:
            db $01,$00
            NumberOf16x16TilesPerPose:
            .Pose0 1
            """);
        ReadInfo reader = new(new LogRegisterSystem());

        Assert.Null(reader.ReadDynamicInfo(repeated, out bool repeatedValid));
        Assert.False(repeatedValid);
        Assert.Null(reader.ReadDynamicInfo(mixed, out bool mixedValid));
        Assert.False(mixedValid);
    }

    [Fact]
    public void ReadDynamicInfo_CurrentPoseIdsMustStartAtZeroAndBeConsecutive()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.dynamicinfo", """
            NumberOf16x16TilesPerPose:
            .Pose1 1
            """);

        var result = new ReadInfo(new LogRegisterSystem())
            .ReadDynamicInfo(path, temp.Path, temp.GetPath("tmp"), out bool valid);

        Assert.Null(result);
        Assert.False(valid);
    }

    [Fact]
    public void GetAllDrawInfos_UsesDeterministicFilenameOrderAndConsecutiveIds()
    {
        using TestDirectory temp = new();
        temp.CreateFile("z/zeta.drawinfo", "Z_Pose0:\n.NumberOfTiles: 1");
        temp.CreateFile("a/alpha.drawinfo", "A_Pose0:\n.NumberOfTiles: 1");
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.GetAllDrawInfos(temp.Path, out bool valid);

        Assert.True(valid);
        Assert.Equal(["Alpha", "Zeta"], result.Select(x => x.ContextName));
        Assert.Equal([0, 1], result.Select(x => x.ID));
    }

    [Fact]
    public void GetAllDynamicInfos_UsesDeterministicFilenameOrderAndResourceDirectory()
    {
        using TestDirectory temp = new();
        string infos = temp.GetPath("infos");
        string resources = temp.GetPath("resources");
        Directory.CreateDirectory(infos);
        Directory.CreateDirectory(resources);
        temp.CreateFile(Path.Combine("resources", "alpha.bin"), [1]);
        temp.CreateFile(Path.Combine("resources", "zeta.bin"), [2]);
        temp.CreateFile(Path.Combine("infos", "z", "zeta.dynamicinfo"),
            "Resources:\nzeta.bin\n");
        temp.CreateFile(Path.Combine("infos", "a", "alpha.dynamicinfo"),
            "Resources:\nalpha.bin\n");
        ReadInfo reader = new(new LogRegisterSystem());

        var result = reader.GetAllDynamicInfos(infos, resources, out bool valid);

        Assert.True(valid);
        Assert.Equal(["Alpha", "Zeta"], result.Select(x => x.ContextName));
        Assert.All(result, info => Assert.Equal(resources, info.ResourceDirectory));
    }

    [Fact]
    public void GetAllDynamicInfos_StopsAfterFirstInvalidFileAndReportsFailure()
    {
        using TestDirectory temp = new();
        string infos = temp.GetPath("infos");
        string resources = temp.GetPath("resources");
        Directory.CreateDirectory(infos);
        Directory.CreateDirectory(resources);
        temp.CreateFile(Path.Combine("resources", "good.bin"), [1]);
        temp.CreateFile(Path.Combine("infos", "a.dynamicinfo"), "Resources:\ngood.bin\n");
        temp.CreateFile(Path.Combine("infos", "b.dynamicinfo"), "Resources:\nmissing.bin\n");

        var result = new ReadInfo(new LogRegisterSystem())
            .GetAllDynamicInfos(infos, resources, out bool valid);

        Assert.False(valid);
        Assert.Single(result);
        Assert.Equal("A", result[0].ContextName);
    }
}
