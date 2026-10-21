using DynamicXtremeLibrary.ExternalTools;

namespace DynamicXtremeLibrary.Tests.ExternalTools;

public sealed class ExternalToolsLinkerTests
{
    [Fact]
    public void Link_RequiresTransactionEvenWhenDestinationDoesNotExist()
    {
        ExternalToolsLinker linker = new("ASM", "ExtraDefines", "Routines");

        Assert.Throws<ArgumentNullException>(() => linker.Link("missing"));
    }

    [Fact]
    public void Link_CopiesDefineTreeAndAddsOnlyMissingSupportedIncludes()
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ExtraDefines", "DXDefines.asm"), "root");
        temp.CreateFile(Path.Combine("ExtraDefines", "Feature.asm"), "feature");
        temp.CreateFile(Path.Combine("ExtraDefines", "nested", "Nested.asm"), "nested");
        temp.CreateFile(Path.Combine("ExtraDefines", "Options.asm"), "excluded");
        string destination = temp.GetPath("tool");
        Directory.CreateDirectory(destination);
        string defines = temp.CreateFile(Path.Combine("tool", "defines.asm"),
            "incsrc \"ExtraDefines/Feature.asm\"\n");
        using CurrentDirectoryScope cwd = new(temp.Path);
        ExternalToolsLinker linker = new("ASM", "ExtraDefines", "Routines");

        using (FileTransaction transaction = new())
        {
            linker.Link("tool", "defines.asm", transaction);
            transaction.Commit();
        }

        Assert.Equal("root", File.ReadAllText(Path.Combine(destination, "ExtraDefines", "DXDefines.asm")));
        Assert.Equal("nested", File.ReadAllText(Path.Combine(destination, "ExtraDefines", "nested", "Nested.asm")));
        string content = File.ReadAllText(defines).Replace('\\', '/');
        Assert.Equal(1, Count(content, "incsrc \"ExtraDefines/Feature.asm\""));
        Assert.Contains("incsrc \"ExtraDefines/DXDefines.asm\"", content);
        Assert.Contains("incsrc \"ExtraDefines/nested/Nested.asm\"", content);
        Assert.DoesNotContain("Options.asm", content);
    }

    [Fact]
    public void Link_InvalidDestinationLeavesTransactionUntouched()
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ExtraDefines", "DXDefines.asm"), "root");
        using CurrentDirectoryScope cwd = new(temp.Path);
        ExternalToolsLinker linker = new("ASM", "ExtraDefines", "Routines");
        using FileTransaction transaction = new();

        linker.Link("missing", "defines.asm", transaction);

        Assert.False(Directory.Exists(temp.GetPath("missing")));
    }

    [Fact]
    public void Link_RecognizesExistingIncludeWithNativeDirectorySeparators()
    {
        using TestDirectory temp = new();
        temp.CreateFile(Path.Combine("ExtraDefines", "DXDefines.asm"), "root");
        temp.CreateFile(Path.Combine("ExtraDefines", "Feature.asm"), "feature");
        Directory.CreateDirectory(temp.GetPath("tool"));
        string defines = temp.CreateFile(Path.Combine("tool", "defines.asm"),
            "incsrc \"ExtraDefines\\Feature.asm\"\n");
        using CurrentDirectoryScope cwd = new(temp.Path);
        ExternalToolsLinker linker = new("ASM", "ExtraDefines", "Routines");

        using (FileTransaction transaction = new())
        {
            linker.Link("tool", "defines.asm", transaction);
            transaction.Commit();
        }

        string content = File.ReadAllText(defines);
        Assert.Equal(1, Count(content.Replace('\\', '/'),
            "incsrc \"ExtraDefines/Feature.asm\""));
    }

    private static int Count(string value, string searched)
        => (value.Length - value.Replace(searched, "").Length) / searched.Length;
}
