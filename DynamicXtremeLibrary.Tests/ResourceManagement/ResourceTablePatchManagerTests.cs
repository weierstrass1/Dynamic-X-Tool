namespace DynamicXtremeLibrary.Tests.ResourceManagement;

using SNESLibrary;

public sealed class ResourceTablePatchManagerTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 0x2A)]
    public void Generate_ReplacesOffsetAndWritesRequestedNumberOfEmptyPointers(
        int numberOfValues, int offset)
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ASM", "ResourceTableTemplate.asm"),
            "Offset=<Offset>\nValues:\n<Values>End");
        using CurrentDirectoryScope cwd = new(temp.Path);

        string result = ResourceTablePatchManager.Generate(numberOfValues, offset);

        Assert.DoesNotContain("<Offset>", result);
        Assert.DoesNotContain("<Values>", result);
        Assert.Contains($"Offset={offset:X4}", result);
        Assert.Equal(numberOfValues,
            result.Split("\tdl $000000", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void Remove_ClearsReferencedRatsBlocksAndThePointerTable()
    {
        byte[] rom = new byte[0x1000];
        const int vector = 0x100;
        const int tableTag = 0x4F8;
        const int table = 0x500;
        const int resourceTag = 0x800;
        Write24(rom, vector, table - SNESROMUtils.HEADER_SIZE);
        byte[] tableData = [
            (byte)(resourceTag & 0xFF), (byte)(resourceTag >> 8), (byte)(resourceTag >> 16),
            0xFF, 0xFF, 0xFF
        ];
        SNESROMUtils.InsertDataWithRats(rom, tableTag, tableData);
        SNESROMUtils.InsertDataWithRats(rom, resourceTag, [0xAA, 0xBB]);

        List<(int, int)> removed = ResourceTablePatchManager.Remove(
            rom, vector, Mapper.NoROM);

        Assert.Equal([(0x608, 2), (0x300, 6)], removed);
        Assert.All(rom[tableTag..(table + tableData.Length)], value => Assert.Equal(0, value));
        Assert.All(rom[resourceTag..(resourceTag + 10)], value => Assert.Equal(0, value));
    }

    private static void Write24(byte[] data, int index, int value)
    {
        data[index] = (byte)value;
        data[index + 1] = (byte)(value >> 8);
        data[index + 2] = (byte)(value >> 16);
    }
}
