using DynamicXtremeLibrary.Infos;
using DynamicXtremeLibrary.ResourceManagement;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.ResourceManagement;

public sealed class ResourceManagerTests
{
    [Fact]
    public void MergeResources_ReturnsEmptyForNoResources()
        => Assert.Empty(ResourceManager.MergeResources([]));

    [Fact]
    public void MergeResources_PacksLargestFirstWithoutExceedingLimit()
    {
        Resource large = Resource(0, "Large", 20000);
        Resource exactFit = Resource(1, "Exact", 12760);
        Resource remaining = Resource(2, "Remaining", 10000);

        IReadOnlyList<ResourceBuffer> buffers =
            ResourceManager.MergeResources([remaining, exactFit, large]);

        Assert.Equal(2, buffers.Count);
        Assert.Equal(32760, buffers[0].Length);
        Assert.Equal([large, exactFit], buffers[0].GetReferences().Select(x => x.Resource));
        Assert.Equal([0, 20000], buffers[0].GetReferences().Select(x => x.Position));
        Assert.Equal(10000, buffers[1].Length);
        Assert.Equal(0, buffers[1].GetReferences().Single().Position);
    }

    [Fact]
    public void BuildBufferTable_SortsByResourceIdAndAppendsTerminator()
    {
        ResourceReference second = new(0, 0, Resource(2, "Two", 1));
        ResourceReference first = new(0, 0, Resource(1, "One", 1));

        string result = ResourceManager.BuildBufferTable([second, first]);

        Assert.Equal("BufferTable:\r\n\tdl !BufferOne\r\n\tdl !BufferTwo\r\n\tdl $FFFFFF\r\n",
            result.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"));
    }

    [Fact]
    public void BuildResourceDefines_SortsByResourceIdAndFormatsSixHexDigits()
    {
        ResourceReference second = new(0, 0xABCDEF, Resource(2, "Two", 1));
        ResourceReference first = new(0, 0x123, Resource(1, "One", 1));

        string result = ResourceManager.BuildResourceDefines([second, first], "Palette");

        Assert.Contains("!PaletteOne = $000123", result);
        Assert.Contains("!PaletteTwo = $ABCDEF", result);
        Assert.True(result.IndexOf("One", StringComparison.Ordinal) <
            result.IndexOf("Two", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildPalettesTable_PreservesDynamicInfoAndPaletteOrder()
    {
        DynamicInfo first = new("First") { Palettes = ["red.bin", "blue.bin"] };
        DynamicInfo second = new("Second") { Palettes = ["green.bin"] };

        string result = ResourceManager.BuildPalettesTable([first, second]);

        Assert.Contains("\tdw !PaletteIDRed", result);
        Assert.Contains("\tdw !PaletteIDBlue", result);
        Assert.Contains("\tdw !PaletteIDGreen", result);
        Assert.Contains("\tdl !PaletteRed", result);
        Assert.True(result.IndexOf("PaletteIDRed", StringComparison.Ordinal) <
            result.IndexOf("PaletteIDBlue", StringComparison.Ordinal));
    }

    [Fact]
    public void InsertBuffers_EmptyInputReturnsEmptyReferenceSetsWithoutChangingRom()
    {
        byte[] rom = new byte[0x101D6];
        byte[] original = [.. rom];

        ResourceAllReferences? result = ResourceManager.InsertBuffers(
            new LogRegisterSystem(), rom, []);

        Assert.NotNull(result);
        Assert.Empty(result.Buffers);
        Assert.Empty(result.DynamicPoses);
        Assert.Empty(result.Palettes);
        Assert.Empty(result.GeneralResources);
        Assert.Equal(original, rom);
    }

    [Fact]
    public void InsertBuffers_ReturnsNullWhenNoEligibleFreeSpaceExists()
    {
        byte[] rom = new byte[0x101D6];
        ResourceBuffer buffer = new(0);
        buffer.AddReference(new ResourceReference(0, 0, Resource(0, "Data", 1)));

        ResourceAllReferences? result = ResourceManager.InsertBuffers(
            new LogRegisterSystem(), rom, [buffer]);

        Assert.Null(result);
        Assert.DoesNotContain((byte)0x53, rom);
    }

    private static Resource Resource(int id, string name, int length)
        => new(id, name, ResourceType.GeneralResource,
            [.. Enumerable.Repeat((byte)id, length)]);
}
