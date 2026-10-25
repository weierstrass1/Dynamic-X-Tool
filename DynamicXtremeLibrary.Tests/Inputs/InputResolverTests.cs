using DynamicXtremeLibrary.Inputs;
using DynamicXtremeLibrary.PaletteEffec;
using DynamicXtremeLibrary.Readers;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Inputs;

public sealed class InputResolverTests
{
    [Fact]
    public void Resolve_IgnoresDisabledInputKindsWithoutReadingTheirFiles()
    {
        using TestDirectory temp = new();
        InfoInput disabled = Input(InfoKind.DrawInfo, temp.GetPath("missing.drawinfo"), "Missing", 0);
        InputResolver resolver = Resolver(temp);

        ResolvedInputs? result = resolver.Resolve([disabled],
            includeDynamicInfos: false, includeDrawInfos: false, includePaletteEffects: false);

        Assert.NotNull(result);
        Assert.Empty(result.DynamicInfos);
        Assert.Empty(result.DrawInfos);
        Assert.Empty(result.PaletteEffects);
    }

    [Fact]
    public void Resolve_ParsesDrawInfosAndAssignsConsecutiveIdsAfterResolution()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("enemy.drawinfo", DrawInfo("Idle", 1) + DrawInfo("Walk", 2));
        InfoInput input = Input(InfoKind.DrawInfo, path, "Enemy", 0);

        ResolvedInputs? result = Resolver(temp).Resolve([input], false, true, false);

        Assert.NotNull(result);
        Assert.Equal([0, 1], result.DrawInfos.Select(x => x.ID));
        Assert.Equal(["Idle0", "Walk0"], result.DrawInfos.Select(x => x.Name));
        Assert.All(result.DrawInfos, pose => Assert.Equal("Enemy", pose.ContextName));
    }

    [Fact]
    public void Resolve_DeduplicatesSemanticallyEquivalentSameNamedInputs()
    {
        using TestDirectory temp = new();
        string firstPath = temp.CreateFile("one/common.drawinfo", DrawInfo("Idle", 1));
        string secondPath = temp.CreateFile("two/common.drawinfo", DrawInfo("Idle", 1));
        string firstAsm = temp.CreateFile("one/consumer.asm", ";#incdri \"common.drawinfo\"");
        string secondAsm = temp.CreateFile("two/consumer.asm", ";#incdri \"common.drawinfo\"");
        InfoInput first = Input(InfoKind.DrawInfo, firstPath, "Common", 0,
            Origin(firstAsm, ConsumerKind.Sprite, "One"));
        InfoInput second = Input(InfoKind.DrawInfo, secondPath, "Common", 1,
            Origin(secondAsm, ConsumerKind.Cluster, "Two"));

        ResolvedInputs? result = Resolver(temp).Resolve([first, second], false, true, false);

        Assert.NotNull(result);
        Assert.Single(result.DrawInfos);
        Assert.Equal("Common", result.DrawInfos[0].ContextName);
        Assert.Equal(2, result.SourceRewrites.Count);
        Assert.Empty(result.NameChanges);
    }

    [Fact]
    public void Resolve_RequiresApprovalForDistinctInputsWhoseSymbolsCollide()
    {
        using TestDirectory temp = new();
        InfoInput[] inputs = CollidingExternalInputs(temp);

        ResolvedInputs? result = Resolver(temp, collisionResolver: null)
            .Resolve(inputs, false, true, false);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_UsesApprovedDeterministicNamesAndBuildsRewritePlans()
    {
        using TestDirectory temp = new();
        InfoInput[] inputs = CollidingExternalInputs(temp);
        NameCollision? proposed = null;

        ResolvedInputs? result = Resolver(temp, collision =>
        {
            proposed = collision;
            return true;
        }).Resolve(inputs, false, true, false);

        Assert.NotNull(result);
        Assert.NotNull(proposed);
        Assert.Equal("Common", proposed.GeneratedName);
        Assert.Equal(["CommonSprite", "CommonCluster"], proposed.ProposedNames);
        Assert.Equal(["CommonSprite", "CommonCluster"],
            result.DrawInfos.Select(x => x.ContextName));
        Assert.Equal([0, 1], result.DrawInfos.Select(x => x.ID));
        Assert.Equal(2, result.SourceRewrites.Count);
        Assert.Equal(2, result.NameChanges.Count);
        Assert.Contains(result.SourceRewrites.SelectMany(x => x.SymbolChanges),
            change => change.Key == "!PoseIDCommon_Idle0" &&
                change.Value == "!PoseIDCommonSprite_Idle0");
    }

    [Fact]
    public void Resolve_RejectsTwoDistinctTraditionalInputsThatCollide()
    {
        using TestDirectory temp = new();
        string firstPath = temp.CreateFile("one/common.drawinfo", DrawInfo("Idle", 1));
        string secondPath = temp.CreateFile("two/common.drawinfo", DrawInfo("Idle", 2));
        InfoInput first = Input(InfoKind.DrawInfo, firstPath, "Common", 0);
        InfoInput second = Input(InfoKind.DrawInfo, secondPath, "Common", 1);
        first.IsTraditional = true;
        second.IsTraditional = true;

        ResolvedInputs? result = Resolver(temp, _ => true)
            .Resolve([first, second], false, true, false);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_RejectsEmptyGeneratedOriginalNameBeforeParsing()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("input.drawinfo", DrawInfo("Idle", 1));
        InfoInput input = Input(InfoKind.DrawInfo, path, "", 0);

        Assert.Null(Resolver(temp).Resolve([input], false, true, false));
    }

    [Fact]
    public void Resolve_ReusesConsistentPersistedLogicalNamesWithoutNewApproval()
    {
        using TestDirectory temp = new();
        InfoInput[] inputs = CollidingExternalInputs(temp);
        inputs[0].Origins[0] = Origin(inputs[0].Origins[0].AsmPath,
            ConsumerKind.Sprite, "One", "CommonSprite");
        inputs[1].Origins[0] = Origin(inputs[1].Origins[0].AsmPath,
            ConsumerKind.Cluster, "Two", "CommonCluster");

        ResolvedInputs? result = Resolver(temp, _ =>
            throw new Xunit.Sdk.XunitException("Persisted valid names must not request approval"))
            .Resolve(inputs, false, true, false);

        Assert.NotNull(result);
        Assert.Equal(["CommonSprite", "CommonCluster"],
            result.DrawInfos.Select(x => x.ContextName));
    }

    [Fact]
    public void Resolve_PreservesPaletteEffectSourceContextAndConvertedTargets()
    {
        using TestDirectory temp = new();
        string resourceDirectory = temp.GetPath("resources");
        string palettePath = temp.CreateFile("resources/target.pal", [0xFF, 0x7F]);
        string effectPath = temp.CreateFile("effects/fade.paleffect", $$"""
            {"Name":"InjectedLegacyName",
            "Effects":[{"EffectType":{{(int)EffectType.PalTransitionRGB}},
            "Palette":"target.pal"}]}
            """);
        InfoInput input = Input(InfoKind.PaletteEffect, effectPath, "Fade", 0,
            resourceDirectory: resourceDirectory);

        ResolvedInputs? result = Resolver(temp).Resolve([input], false, false, true);

        Assert.NotNull(result);
        ResolvedPaletteEffectCollection collection = Assert.Single(result.PaletteEffects);
        Assert.Equal("Fade", collection.OriginalName);
        Assert.Equal("Fade", collection.LogicalName);
        Assert.Equal("Fade", collection.Collection.Name);
        Assert.Equal(Path.GetFullPath(effectPath), collection.SourcePath);
        Assert.Equal(Path.GetFullPath(resourceDirectory), collection.ResourceDirectory);
        ResolvedPaletteEffectTarget target = Assert.Single(collection.Targets);
        Assert.Equal(Path.GetFullPath(palettePath), target.SourcePath);
        Assert.Equal(PaletteEffectPaletteFormat.RGB, target.Format);
        Assert.Equal([31, 31, 31], target.Data);
    }

    [Fact]
    public void Resolve_RequiresCollisionApprovalWhenDynamicAndEffectPalettesGenerateSameSymbol()
    {
        using TestDirectory temp = new();
        InfoInput[] inputs = CollidingCrossKindPaletteInputs(temp);

        ResolvedInputs? result = Resolver(temp, collisionResolver: null)
            .Resolve(inputs, true, false, true);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_ApprovedCrossKindPaletteCollisionRenamesEffectResourceAndPlansSymbolRewrite()
    {
        using TestDirectory temp = new();
        InfoInput[] inputs = CollidingCrossKindPaletteInputs(temp);
        NameCollision? collision = null;

        ResolvedInputs? result = Resolver(temp, proposed =>
        {
            collision = proposed;
            return true;
        }).Resolve(inputs, true, false, true);

        Assert.NotNull(result);
        Assert.NotNull(collision);
        Assert.Equal("SharedRGB", collision.GeneratedName);
        Assert.Equal("FadeSprite", Assert.Single(result.PaletteEffects).LogicalName);
        Assert.Contains(result.SourceRewrites.SelectMany(rewrite => rewrite.SymbolChanges),
            change => change.Key == "!PaletteSharedRGB" &&
                change.Value == "!PaletteSharedSpriteRGB");
        Assert.Contains(result.SourceRewrites.SelectMany(rewrite => rewrite.SymbolChanges),
            change => change.Key == "!PaletteSharedRGBSize" &&
                change.Value == "!PaletteSharedSpriteRGBSize");
    }

    private static InputResolver Resolver(TestDirectory temp,
        Func<NameCollision, bool>? collisionResolver = null)
    {
        LogRegisterSystem log = new();
        return new(log, new ReadInfo(log), temp.GetPath("temporary"), collisionResolver);
    }

    private static InfoInput[] CollidingExternalInputs(TestDirectory temp)
    {
        string firstPath = temp.CreateFile("one/common.drawinfo", DrawInfo("Idle", 1));
        string secondPath = temp.CreateFile("two/common.drawinfo", DrawInfo("Idle", 2));
        string firstAsm = temp.CreateFile("one/consumer.asm", ";#incdri \"common.drawinfo\"");
        string secondAsm = temp.CreateFile("two/consumer.asm", ";#incdri \"common.drawinfo\"");
        return
        [
            Input(InfoKind.DrawInfo, firstPath, "Common", 0,
                Origin(firstAsm, ConsumerKind.Sprite, "One")),
            Input(InfoKind.DrawInfo, secondPath, "Common", 1,
                Origin(secondAsm, ConsumerKind.Cluster, "Two"))
        ];
    }

    private static InfoInput[] CollidingCrossKindPaletteInputs(TestDirectory temp)
    {
        string dynamicPath = temp.CreateFile("dynamic/context.dynamicinfo",
            "Palettes:\nsharedRGB.bin\n");
        temp.CreateFile("dynamic/sharedRGB.bin", [1, 2, 3]);
        InfoInput dynamic = Input(InfoKind.DynamicInfo, dynamicPath, "Context", 0);
        dynamic.IsTraditional = true;

        string effectPath = temp.CreateFile("sprite/fade.paleffect", $$"""
            {"Effects":[{"EffectType":{{(int)EffectType.PalTransitionRGB}},
            "Palette":"shared.pal"}]}
            """);
        temp.CreateFile("sprite/shared.pal", [0x1F, 0x00]);
        string asm = temp.CreateFile("sprite/consumer.asm", ";#incpale \"fade.paleffect\"");
        InfoInput effect = Input(InfoKind.PaletteEffect, effectPath, "Fade", 1,
            Origin(asm, ConsumerKind.Sprite, "Consumer"));

        return [dynamic, effect];
    }

    private static InfoInput Input(InfoKind kind, string path, string originalName,
        int order, InputOrigin? origin = null, string? resourceDirectory = null)
    {
        InfoInput input = new()
        {
            Kind = kind,
            Path = path,
            ResourceDirectory = resourceDirectory ?? Path.GetDirectoryName(path)!,
            OriginalName = originalName,
            LogicalName = originalName,
            DiscoveryOrder = order
        };
        if (origin != null)
            input.Origins.Add(origin);
        return input;
    }

    private static InputOrigin Origin(string asmPath, ConsumerKind kind, string name,
        string? previous = null) => new()
    {
        AsmPath = asmPath,
        ConsumerKind = kind,
        ConsumerName = name,
        DirectiveLine = 0,
        PreviousLogicalName = previous
    };

    private static string DrawInfo(string name, int property) => $$"""
        {{name}}_Pose0:
        .NumberOfTiles: 1
        .Property0: {{property}}

        """;
}
