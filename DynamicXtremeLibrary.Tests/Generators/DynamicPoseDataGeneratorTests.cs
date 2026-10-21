using DynamicXtremeLibrary.Generators;
using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.ResourceManagement;
using SNESLibrary;

namespace DynamicXtremeLibrary.Tests.Generators;

public sealed class DynamicPoseDataGeneratorTests
{
    [Fact]
    public void GenerateEmpty_ReplacesEveryTableWithItsSentinelEntry()
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ASM", "DynamicPoseDataTemplate.asm"),
            "S:<Size>\nB:<Blocks>\nR:<Resource>\nZ:<Sizes>\nL:<LastRow>");
        using CurrentDirectoryScope cwd = new(temp.Path);

        string result = DynamicPoseDataGenerator.GenerateEmpty();

        Assert.DoesNotContain('<', result);
        Assert.Contains("S:\n\tdw $0000", result);
        Assert.Contains("B:\n\tdb $00", result);
        Assert.Contains("R:\n\tdl $000000", result);
        Assert.Contains("Z:\n\tdw $0000,$0000", result);
        Assert.Contains("L:\n\tdw $0000", result);
    }

    [Fact]
    public void GenerateData_SerializesPoseMetadataAndMappedResourceAddresses()
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ASM", "DynamicPoseDataTemplate.asm"),
            "S:<Size>\nB:<Blocks>\nR:<Resource>\nZ:<Sizes>\nL:<LastRow>");
        using CurrentDirectoryScope cwd = new(temp.Path);
        DynamicInfo info = new("Enemy")
        {
            PosesChunksSizes = [1, 1],
            PosesLastRow = [1]
        };
        Resource resource = new(0, "Pose", ResourceType.DynamicPose, [1]);
        ResourceReference reference = new(0, 0x200, resource);

        string result = DynamicPoseDataGenerator.GenerateData(
            Mapper.NoROM, [reference], [info]);

        Assert.DoesNotContain('<', result);
        Assert.Contains("S:\n\tdw $0040", result);
        Assert.Contains("B:\n\tdb $01", result);
        Assert.Contains("R:\n\tdl $000000", result);
        Assert.Contains("Z:\n\tdw $0020,$0020", result);
        Assert.Contains("L:\n\tdw $0010", result);
    }
}
