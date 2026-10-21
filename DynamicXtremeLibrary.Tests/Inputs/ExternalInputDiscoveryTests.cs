using DynamicXtremeLibrary.Inputs;

namespace DynamicXtremeLibrary.Tests.Inputs;

public sealed class ExternalInputDiscoveryTests
{
    [Fact]
    public void Discover_FindsTraditionalInputsRecursivelyInStablePathOrder()
    {
        using TestDirectory temp = new();
        string dyn = temp.GetPath("DynamicInfos");
        string draw = temp.GetPath("DrawInfos");
        string effects = temp.GetPath("Effects");
        string resources = temp.GetPath("Resources");
        temp.CreateFile("DynamicInfos/z/zeta.dynamicinfo", "");
        temp.CreateFile("DynamicInfos/a/alpha.dynamicinfo", "");
        temp.CreateFile("DrawInfos/pose.drawinfo", "");
        temp.CreateFile("Effects/fade.paleffect", "{}");

        IReadOnlyList<InfoInput> result = ExternalInputDiscovery.Discover(
            dyn, draw, effects, resources, null, null, null, null);

        Assert.Equal(4, result.Count);
        Assert.All(result, input => Assert.True(input.IsTraditional));
        Assert.Equal(["Pose", "Alpha", "Zeta", "Fade"],
            result.Select(x => x.OriginalName));
        Assert.Equal(Path.GetFullPath(resources),
            result.First(input => input.Kind == InfoKind.DynamicInfo).ResourceDirectory);
    }

    [Fact]
    public void Discover_ReadsPixiConsumerDirectiveAndPersistedLogicalName()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("pixi");
        temp.CreateFile("pixi/list.txt", "SPRITE:\n00 test.cfg");
        temp.CreateFile("pixi/sprites/test.cfg", "test.asm");
        string asm = temp.CreateFile("pixi/sprites/test.asm",
            ";#incdri \"inputs/enemy.drawinfo\" ; dxt-name: EnemySprite\n");
        string inputPath = temp.CreateFile("pixi/sprites/inputs/enemy.drawinfo", "");

        IReadOnlyList<InfoInput> result = ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), root, null, null);

        InfoInput input = Assert.Single(result);
        Assert.False(input.IsTraditional);
        Assert.Equal(InfoKind.DrawInfo, input.Kind);
        Assert.Equal(Path.GetFullPath(inputPath), input.Path);
        InputOrigin origin = Assert.Single(input.Origins);
        Assert.Equal(Path.GetFullPath(asm), origin.AsmPath);
        Assert.Equal(ConsumerKind.Sprite, origin.ConsumerKind);
        Assert.Equal("Test", origin.ConsumerName);
        Assert.Equal("EnemySprite", origin.PreviousLogicalName);
    }

    [Fact]
    public void Discover_PixiJsonPrefersAsmFileProperty()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("pixi");
        temp.CreateFile("pixi/list.txt", "SPRITE:\nentry.json");
        temp.CreateFile("pixi/sprites/entry.json",
            "{\"Other\":\"wrong.asm\",\"AsmFile\":\"right.asm\"}");
        temp.CreateFile("pixi/sprites/right.asm", ";#incpale \"effect.paleffect\"");
        temp.CreateFile("pixi/sprites/effect.paleffect", "{}");

        InfoInput result = Assert.Single(ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), root, null, null));

        Assert.Equal(InfoKind.PaletteEffect, result.Kind);
        Assert.Equal("Entry", result.Origins.Single().ConsumerName);
    }

    [Fact]
    public void Discover_DeduplicatesTraditionalAndExternalReferenceToSamePhysicalInput()
    {
        using TestDirectory temp = new();
        string drawDirectory = temp.GetPath("draw");
        string input = temp.CreateFile("draw/enemy.drawinfo", "");
        temp.CreateFile("pixi/list.txt", "SPRITE:\nconsumer.asm");
        temp.CreateFile("pixi/sprites/consumer.asm",
            $";#incdri \"{input.Replace("\\", "/")}\"");

        InfoInput result = Assert.Single(ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), drawDirectory, temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), temp.GetPath("pixi"),
            null, null));

        Assert.True(result.IsTraditional);
        Assert.Single(result.Origins);
    }

    [Fact]
    public void Discover_RejectsMalformedDirectiveAndWrongExtension()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("pixi");
        temp.CreateFile("pixi/list.txt", "SPRITE:\nconsumer.asm");
        string asm = temp.CreateFile("pixi/sprites/consumer.asm", ";#incdri no-quotes.drawinfo");

        Assert.Throws<InvalidDataException>(() => ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), root, null, null));

        File.WriteAllText(asm, ";#incdri \"wrong.dynamicinfo\"");
        Assert.Throws<InvalidDataException>(() => ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("pixi/list.txt"), root, null, null));
    }

    [Fact]
    public void Discover_ReadsUberasmSectionsAndResolvesSectionDirectory()
    {
        using TestDirectory temp = new();
        string root = temp.GetPath("uber");
        temp.CreateFile("uber/list.txt", "LEVEL:\n105 code.asm\nGLOBAL:\nglobal.asm\n");
        temp.CreateFile("uber/level/code.asm", ";#incdri \"inputs/level.drawinfo\"");
        temp.CreateFile("uber/level/inputs/level.drawinfo", "");
        temp.CreateFile("uber/global/global.asm", ";#incpale \"global.paleffect\"");
        temp.CreateFile("uber/global/global.paleffect", "{}");

        IReadOnlyList<InfoInput> result = ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), null, null, temp.GetPath("uber/list.txt"), root);

        Assert.Equal(2, result.Count);
        Assert.Equal(ConsumerKind.Level,
            result.Single(input => input.Kind == InfoKind.DrawInfo).Origins.Single().ConsumerKind);
        Assert.Equal(ConsumerKind.Global,
            result.Single(input => input.Kind == InfoKind.PaletteEffect).Origins.Single().ConsumerKind);
    }

    [Fact]
    public void Discover_MissingConfiguredExternalListIsAnError()
    {
        using TestDirectory temp = new();

        Assert.Throws<FileNotFoundException>(() => ExternalInputDiscovery.Discover(
            temp.GetPath("none1"), temp.GetPath("none2"), temp.GetPath("none3"),
            temp.GetPath("resources"), temp.GetPath("missing-list.txt"), null, null, null));
    }
}
