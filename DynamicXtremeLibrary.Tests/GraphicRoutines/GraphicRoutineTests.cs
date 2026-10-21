using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.Infos;

namespace DynamicXtremeLibrary.Tests.GraphicRoutines;

public sealed class GraphicRoutineTests
{
    [Fact]
    public void GetGraphicRoutines_GroupsCompatiblePosesAndAssignsOneRoutineId()
    {
        DrawInfo first = new(0, "Enemy", "Idle");
        DrawInfo second = new(1, "Enemy", "Walk");

        IReadOnlyDictionary<int, IReadOnlyList<GraphicRoutine>> groups =
            GraphicRoutine.GetGraphicRoutines([first, second]);

        GraphicRoutine routine = Assert.Single(Assert.Single(groups).Value);
        Assert.Equal(0, routine.ID);
        Assert.Equal([first, second], routine.DrawInfos);
        Assert.Equal(0, routine.TileSize);
        Assert.Equal(0, routine.DataSize);
        Assert.Contains("Static_OneTileDefault", routine.Name);
    }

    [Fact]
    public void GetGraphicRoutines_SeparatesDifferentBehaviorKeys()
    {
        DrawInfo staticPose = new(0, "Enemy", "Idle");
        DrawInfo dynamicPose = new(1, "Enemy", "Walk") { IsDynamic = true };

        IReadOnlyDictionary<int, IReadOnlyList<GraphicRoutine>> groups =
            GraphicRoutine.GetGraphicRoutines([staticPose, dynamicPose]);

        Assert.Equal(2, groups.Count);
        Assert.Equal([0, 1], groups.Values.SelectMany(x => x).Select(x => x.ID).Order());
    }

    [Fact]
    public void GetGraphicRoutines_SplitsCompatibleGroupAtThirtyThousandDataBytes()
    {
        List<DrawInfo> poses = [.. Enumerable.Range(0, 34).Select(index => NonDefaultPose(index, 128))];

        IReadOnlyList<GraphicRoutine> routines = Assert.Single(
            GraphicRoutine.GetGraphicRoutines(poses)).Value;

        Assert.Equal(2, routines.Count);
        Assert.Equal(33, routines[0].DrawInfos.Count);
        Assert.Single(routines[1].DrawInfos);
        Assert.True(routines[0].DataSize <= 30000);
        Assert.True(routines[1].DataSize <= 30000);
    }

    [Fact]
    public void GetTableEntries_UsesPerRoutineTileOffsetsAndSortsByPoseId()
    {
        DrawInfo pose2 = NonDefaultPose(2, 2);
        DrawInfo pose0 = NonDefaultPose(0, 3);
        IReadOnlyList<GraphicRoutine> routines = GraphicRoutine
            .GetGraphicRoutines([pose2, pose0]).Values.SelectMany(x => x).ToArray();

        IReadOnlyList<DrawInfoTableEntry> entries = GraphicRoutine.GetTableEntries(routines);

        Assert.Equal([0, 2], entries.Select(x => x.DrawInfoID));
        Assert.Equal(2, entries.Single(x => x.DrawInfoID == 2).DrawInfoLength);
        Assert.Equal(0, entries.Single(x => x.DrawInfoID == 2).DrawInfoOffset);
        Assert.Equal(2, entries.Single(x => x.DrawInfoID == 0).DrawInfoOffset);
    }

    private static DrawInfo NonDefaultPose(int id, int length) => new(id, "Enemy", $"Pose{id}")
    {
        Tiles = [.. Enumerable.Range(1, length)],
        Properties = [.. Enumerable.Repeat(1, length)],
        XDisplacements = [.. Enumerable.Repeat(1, length)],
        YDisplacements = [.. Enumerable.Repeat(1, length)],
        FlipXDisplacements = [.. Enumerable.Repeat(2, length)],
        FlipYDisplacements = [.. Enumerable.Repeat(2, length)],
        Sizes = [.. Enumerable.Range(0, length).Select(i => i % 2 == 0 ? 0 : 2)]
    };
}
