using LogRegister;

namespace DynamicXtremeLibrary.Tests;

public sealed class DynamicXtremeTests
{
    [Fact]
    public void RemoveResources_RomWithoutInstallationMarkerIsUnchanged()
    {
        byte[] rom = new byte[0x101D6];
        rom[0x81D5] = 0x20;
        byte[] original = [.. rom];
        DynamicXtreme installer = CreateInstaller();

        installer.RemoveResources(rom);

        Assert.Equal(original, rom);
    }

    private static DynamicXtreme CreateInstaller() => new(new LogRegisterSystem())
    {
        DynamicInfoDirectory = "DynamicInfos",
        DrawInfoDirectory = "DrawInfos",
        DynamicResourcesDirectory = "DynamicResources",
        PaletteEffectsDirectory = "PaletteEffects",
        TemplateASMDirectory = "ASM",
        PatchDirectory = "Patch",
        ExtraDefinesDirectory = "ExtraDefines",
        GraphicRoutinesDirectory = "GraphicRoutines",
        TMPDirectory = "TMP",
        DataDirectory = "Data",
        RoutinesDirectory = "Routines",
        OptionsDefinesFilename = "Options",
        BufferDataFilename = "Buffers",
        DynamicPoseDataFilename = "DynamicPoses",
        PaletteDataFilename = "Palettes",
        PoseDataFilename = "Poses",
        PaletteEffectsDataFilename = "PaletteEffects",
        PoseDataTemplateFilename = "PoseDataTemplate.asm",
        GraphicRoutineTemplateFilename = "GraphicRoutineTemplate",
        GraphicRoutineIncludeTemplateFilename = "GraphicRoutineIncludeTemplate",
        GraphicRoutineIncludeFilename = "GraphicRoutines",
        GraphicRoutineProtsFilename = "GraphicRoutinesProts",
        InputDefinesFilename = "DXDefines",
        OutputDefinesFilename = "DXDefines"
    };
}
