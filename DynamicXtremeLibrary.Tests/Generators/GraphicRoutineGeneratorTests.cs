using DynamicXtremeLibrary.Generators;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.Infos;

namespace DynamicXtremeLibrary.Tests.Generators;

public sealed class GraphicRoutineGeneratorTests
{
    [Fact]
    public void Constructor_RejectsEitherMissingTemplateAndCreatesOutputDirectory()
    {
        using TestDirectory temp = new();
        string include = temp.CreateFile("include.asm", "<grs>");
        string routine = temp.CreateFile("routine.asm", "<Title>");

        Assert.Throws<FileNotFoundException>(() => new GraphicRoutineGenerator(
            include, temp.GetPath("missing.asm"), temp.GetPath("out1"), "all.asm", "prots.asm"));
        Assert.Throws<FileNotFoundException>(() => new GraphicRoutineGenerator(
            temp.GetPath("missing.asm"), routine, temp.GetPath("out2"), "all.asm", "prots.asm"));

        string output = temp.GetPath("generated");
        _ = new GraphicRoutineGenerator(include, routine, output, "all.asm", "prots.asm");
        Assert.True(Directory.Exists(output));
    }

    [Fact]
    public void GenerateGraphicRoutine_ReplacesFlagsAndWritesOnlyRequiredTables()
    {
        using TestDirectory temp = new();
        string include = temp.CreateFile("include.asm", "<grs>");
        string template = temp.CreateFile("routine.asm",
            "<Title>|<IsDynamic>,<DefaultXdisp>,<DefaultYdisp>,<FlipX>,<FlipY>," +
            "<DefaultTile>,<DefaultProp>,<DefaultSize>,<Size16>\n" +
            "<Tiles>\n<Properties>\n<XDisplacements>\n<YDisplacements>\n<Sizes>");
        string output = temp.GetPath("generated");
        DrawInfo pose = NonDefaultPose();
        GraphicRoutine routine = GraphicRoutine.GetGraphicRoutines([pose])
            .Values.SelectMany(value => value).Single();
        GraphicRoutineGenerator generator = new(include, template, output, "all.asm", "prots.asm");

        generator.GenerateGraphicRoutine(routine);

        string generated = File.ReadAllText(Path.Combine(output, $"{routine.Name}.asm"));
        Assert.DoesNotContain('<', generated);
        Assert.Contains($"{routine.Name}:|1,0,0,1,1,0,0,0,0", generated);
        Assert.Contains(".Tiles", generated);
        Assert.Contains(".Properties", generated);
        Assert.Contains(".XDisplacements", generated);
        Assert.Contains(".XDisplacementsFlip", generated);
        Assert.Contains(".YDisplacements", generated);
        Assert.Contains(".YDisplacementsFlip", generated);
        Assert.Contains(".Sizes", generated);
        Assert.Contains("..Enemy_Walk", generated);
    }

    [Fact]
    public void GenerateAllGraphicRoutine_RebuildsAggregateFilesAndRemovesStaleFiles()
    {
        using TestDirectory temp = new();
        string include = temp.CreateFile("include.asm", "Header\n<grs>\nFooter");
        string template = temp.CreateFile("routine.asm", "<Title>\n<IsDynamic><Tiles><Properties><XDisplacements><YDisplacements><Sizes>");
        string output = temp.GetPath("generated");
        Directory.CreateDirectory(output);
        string stale = temp.CreateFile(Path.Combine("generated", "stale.asm"), "old");
        GraphicRoutineGenerator generator = new(include, template, output, "all.asm", "prots.asm");
        var groups = GraphicRoutine.GetGraphicRoutines([new DrawInfo(0, "Enemy", "Idle")]);
        GraphicRoutine routine = groups.Values.SelectMany(value => value).Single();

        generator.GenerateAllGraphicRoutine(groups);

        Assert.False(File.Exists(stale));
        Assert.True(File.Exists(Path.Combine(output, $"{routine.Name}.asm")));
        string aggregate = File.ReadAllText(Path.Combine(output, "all.asm"));
        Assert.Contains($"incsrc \"{routine.Name}.asm\"", aggregate);
        Assert.Contains($"print dec(GraphicRoutines_{routine.Name})", aggregate);
        Assert.Equal($"prot GraphicRoutines_{routine.Name}{Environment.NewLine}",
            File.ReadAllText(Path.Combine(output, "prots.asm")));
    }

    private static DrawInfo NonDefaultPose() => new(0, "Enemy", "Walk")
    {
        IsDynamic = true,
        Tiles = [1, 3],
        Properties = [1, 2],
        XDisplacements = [0, 8],
        FlipXDisplacements = [8, 0],
        YDisplacements = [0, 8],
        FlipYDisplacements = [8, 0],
        Sizes = [0, 2]
    };
}
