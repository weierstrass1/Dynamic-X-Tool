using DynamicXtremeLibrary.Generators;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.Infos;

namespace DynamicXtremeLibrary.Tests.Generators;

public sealed class PoseDataGeneratorTests
{
    [Fact]
    public void GenerateData_EmptyRoutineSetProducesEmptyTablesWithoutPlaceholders()
    {
        using TestDirectory temp = new();
        string template = temp.CreateFile("template.asm",
            "Offsets:<Offset>\nLengths:<Length>\nIDs:<ID>\nRoutines:<Routine>");

        string result = PoseDataGenerator.GenerateData([], template);

        Assert.DoesNotContain('<', result);
        Assert.Equal("Offsets:\nLengths:\nIDs:\nRoutines:\n", result);
    }

    [Fact]
    public void GenerateData_ReplacesEveryTemplatePlaceholderWithSortedLookupData()
    {
        using TestDirectory temp = new();
        string template = temp.CreateFile("template.asm",
            "Offsets:<Offset>\nLengths:<Length>\nIDs:<ID>\nRoutines:<Routine>");
        DrawInfo first = NonDefaultPose(0, 2);
        DrawInfo second = NonDefaultPose(1, 1);
        IReadOnlyList<GraphicRoutine> routines = GraphicRoutine.GetGraphicRoutines([first, second])
            .Values.SelectMany(x => x).ToArray();

        string result = PoseDataGenerator.GenerateData(routines, template);

        Assert.DoesNotContain("<Offset>", result);
        Assert.DoesNotContain("<Length>", result);
        Assert.DoesNotContain("<ID>", result);
        Assert.DoesNotContain("<Routine>", result);
        Assert.Contains("$0000,$0000", result);
        Assert.Contains("$01,$00", result);
        Assert.Contains("dl GraphicRoutines_", result);
    }

    private static DrawInfo NonDefaultPose(int id, int length) => new(id, "Enemy", $"Pose{id}")
    {
        Tiles = [.. Enumerable.Range(1, length)],
        Properties = new int[length],
        XDisplacements = new int[length],
        YDisplacements = new int[length],
        Sizes = [.. Enumerable.Repeat(2, length)]
    };
}
