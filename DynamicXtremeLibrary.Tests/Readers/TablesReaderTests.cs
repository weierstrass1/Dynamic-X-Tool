using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Tests.Readers;

public sealed class TablesReaderTests
{
    [Fact]
    public void ReadDynamicInfoTables_ParsesChunkPairs()
    {
        const string input = """
            PosesChunksSizes:
            Idle_PoseChunksSizes:
                db $01,$02
            Walk_PoseChunksSizes:
                db $03,$04
            """;

        var result = TablesReader.ReadDynamicInfoTables("Enemy", input);

        Assert.Equal("Enemy", result.ContextName);
        Assert.Equal([1, 2, 3, 4], result.PosesChunksSizes);
    }

    [Fact]
    public void ReadDynamicInfoTables_EmptyInputReturnsEmptyModel()
    {
        var result = TablesReader.ReadDynamicInfoTables("Enemy", "");

        Assert.Equal("Enemy", result.ContextName);
        Assert.Null(result.PosesChunksSizes);
    }

    [Fact]
    public void ReadDrawInfoTables_CombinesNormalAndFlipTables()
    {
        const string input = """
            Tiles:
            Idle_Tiles:
                db $00,$01
            XDisplacements:
            Idle_XDisp:
                db $00,$08
            Idle_FlipX:
                db $08,$00
            Sizes:
            Idle_Sizes:
                db $02,$02
            """;

        var result = TablesReader.ReadDrawInfoTables(4, "Enemy", input);

        var pose = Assert.Single(result);
        Assert.Equal(4, pose.ID);
        Assert.Equal("Idle", pose.Name);
        Assert.Equal([0, 1], pose.Tiles);
        Assert.Equal([0, 8], pose.XDisplacements);
        Assert.Equal([8, 0], pose.FlipXDisplacements);
    }

    [Fact]
    public void ReadDrawInfoTables_ReusesShorterPropertyTablesAcrossPoses()
    {
        const string input = """
            Tiles:
            Idle_Tiles:
                db $00
            Walk_Tiles:
                db $01
            Properties:
            Shared_Properties:
                db $40
            """;

        DrawInfo[] result = TablesReader.ReadDrawInfoTables(10, "Enemy", input);

        Assert.Equal([10, 11], result.Select(pose => pose.ID));
        Assert.Equal(["Idle", "Walk"], result.Select(pose => pose.Name));
        Assert.All(result, pose => Assert.Equal([0x40], pose.Properties));
    }
}
