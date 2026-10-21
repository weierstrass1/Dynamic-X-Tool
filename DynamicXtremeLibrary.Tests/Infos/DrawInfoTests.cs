using DynamicXtremeLibrary.Infos;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Infos;

public sealed class DrawInfoTests
{
    [Fact]
    public void NewPose_UsesDocumentedDefaults()
    {
        DrawInfo info = new(7, "Enemy", "Idle");

        Assert.Equal(7, info.ID);
        Assert.Equal("Enemy", info.ContextName);
        Assert.Equal("Idle", info.Name);
        Assert.False(info.IsDynamic);
        Assert.True(info.OneTile());
        Assert.True(info.AllTilesAreDefault());
        Assert.True(info.AllPropertiesAreDefault());
        Assert.True(info.AllXDisplacementsAreDefault());
        Assert.True(info.AllYDisplacementsAreDefault());
        Assert.True(info.AllSizesAre16());
        Assert.False(info.AllSizesAre8());
        Assert.Equal("7_Enemy_Idle", info.ToString());
    }

    [Fact]
    public void PresenceAndFlipQueries_DistinguishEqualAndDifferentTables()
    {
        DrawInfo info = new(0, "Context", "Pose")
        {
            XDisplacements = [0, 8],
            YDisplacements = [1, 2],
            FlipXDisplacements = [0, 8],
            FlipYDisplacements = [2, 1],
            Properties = [0, 0x40],
            Sizes = [0, 0]
        };

        Assert.True(info.HasXDisplacement());
        Assert.True(info.HasYDisplacement());
        Assert.False(info.HasXFlip());
        Assert.True(info.HasYFlip());
        Assert.False(info.HasXYFlip());
        Assert.True(info.HasProperties());
        Assert.True(info.HasSizes());
        Assert.True(info.AllSizesAre8());
        Assert.False(info.AllSizesAre16());
    }

    [Fact]
    public void GetLength_UsesFirstPopulatedTableInDocumentedOrder()
    {
        DrawInfo info = new(0, "Context", "Pose")
        {
            Tiles = [],
            Properties = [1, 2, 3],
            XDisplacements = [1, 2]
        };

        Assert.Equal(3, info.GetLength());
    }

    [Fact]
    public void Validate_AcceptsConsistentByteTablesAndSupportedSizes()
    {
        DrawInfo info = CompletePose(3);

        Assert.True(info.Validate(new LogRegisterSystem()));
    }

    [Theory]
    [InlineData(129)]
    [InlineData(200)]
    public void Validate_RejectsMoreThan128Tiles(int count)
    {
        DrawInfo info = new(0, "Context", "Pose") { Tiles = new int[count] };

        Assert.False(info.Validate(new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsValuesAboveByteRange()
    {
        DrawInfo info = CompletePose(1);
        info.Properties = [256];

        Assert.False(info.Validate(new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsValuesBelowByteRange()
    {
        DrawInfo info = CompletePose(1);
        info.Tiles = [-1];

        Assert.False(info.Validate(new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsUnsupportedSizeValues()
    {
        DrawInfo info = CompletePose(1);
        info.Sizes = [1];

        Assert.False(info.Validate(new LogRegisterSystem()));
    }

    [Fact]
    public void Validate_RejectsInconsistentPopulatedTableLengths()
    {
        DrawInfo info = CompletePose(2);
        info.Properties = [0];

        Assert.False(info.Validate(new LogRegisterSystem()));
    }

    [Fact]
    public void GetKey_EncodesEveryDocumentedCharacteristic()
    {
        DrawInfo defaults = new(0, "Context", "Default");
        Assert.Equal(505, defaults.GetKey());

        defaults.IsDynamic = true;
        Assert.Equal(1017, defaults.GetKey());
    }

    [Fact]
    public void RenderBoxDistance_UsesSignedDisplacementsTileSizesAndFlipTables()
    {
        DrawInfo info = new(0, "Context", "Pose")
        {
            XDisplacements = [0xF8, 0x10],
            FlipXDisplacements = [0xE0, 0x08],
            YDisplacements = [0xFC, 0],
            Sizes = [0, 2]
        };

        Assert.Equal(32, info.GetRenderBoxXDistanceOutOfScreen());
        Assert.Equal(16, info.GetRenderBoxYDistanceOutOfScreen());
    }

    [Fact]
    public void MaximumRenderBoxDistance_ReturnsLargestPoseExtent()
    {
        DrawInfo small = new(0, "A", "Small") { XDisplacements = [0], Sizes = [0] };
        DrawInfo large = new(1, "A", "Large") { XDisplacements = [32], Sizes = [2] };

        Assert.Equal(48, DrawInfo.GetMaximumRenderBoxXDistanceOutOfScreen([small, large]));
    }

    [Fact]
    public void GroupByContextName_PreservesPoseOrderWithinEachContext()
    {
        DrawInfo a1 = new(0, "A", "One");
        DrawInfo b = new(1, "B", "Two");
        DrawInfo a2 = new(2, "A", "Three");

        Dictionary<string, List<DrawInfo>> groups = DrawInfo.GroupByContextName([a1, b, a2]);

        Assert.Equal([a1, a2], groups["A"]);
        Assert.Equal([b], groups["B"]);
    }

    [Theory]
    [MemberData(nameof(ArrayComparisons))]
    public void EqualArrays_UsesNullLengthAndOrderedValueSemantics(
        int[]? left, int[]? right, bool expected)
        => Assert.Equal(expected, new DrawInfo(0, "C", "P").EqualArrays(left, right));

    public static TheoryData<int[]?, int[]?, bool> ArrayComparisons => new()
    {
        { null, null, true },
        { null, [], false },
        { [1, 2], [1, 2], true },
        { [1, 2], [2, 1], false },
        { [1], [1, 2], false }
    };

    private static DrawInfo CompletePose(int length) => new(0, "Context", "Pose")
    {
        Tiles = [.. Enumerable.Range(0, length)],
        Properties = new int[length],
        XDisplacements = new int[length],
        YDisplacements = new int[length],
        FlipXDisplacements = new int[length],
        FlipYDisplacements = new int[length],
        Sizes = [.. Enumerable.Repeat(2, length)]
    };
}
