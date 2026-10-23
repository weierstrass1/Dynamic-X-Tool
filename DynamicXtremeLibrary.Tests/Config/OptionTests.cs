using DynamicXtremeLibrary.Config;

namespace DynamicXtremeLibrary.Tests.Config;

public sealed class OptionTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData(" YES ", true)]
    [InlineData("y", true)]
    [InlineData("T", true)]
    [InlineData("false", false)]
    [InlineData("no", false)]
    [InlineData("", false)]
    public void BoolOption_ParseFromString_UsesDocumentedForms(string input, bool expected)
        => Assert.Equal(expected, new BoolOption().ParseFromString(input));

    [Theory]
    [InlineData("true")]
    [InlineData("FALSE")]
    [InlineData("yes")]
    [InlineData("n")]
    [InlineData("")]
    [InlineData("  ")]
    public void BoolOption_Validate_AcceptsSupportedForms(string input)
        => Assert.True(new BoolOption().Validate(input));

    [Theory]
    [InlineData("1")]
    [InlineData("sure")]
    [InlineData("trueish")]
    public void BoolOption_Validate_RejectsUnsupportedForms(string input)
        => Assert.False(new BoolOption().Validate(input));

    [Fact]
    public void OptionalFileOption_AcceptsEmptyAndExistingFileOnly()
    {
        using TestDirectory temp = new();
        string existing = temp.CreateFile("input.txt", "data");
        OptionalFileOption option = new();

        Assert.True(option.Validate(""));
        Assert.True(option.Validate(existing));
        Assert.False(option.Validate(temp.GetPath("missing.txt")));
    }

    [Fact]
    public void OptionalFileOption_ResolvesAndStoresRelativePathsFromExternalToolDirectory()
    {
        using TestDirectory temp = new();
        string toolDirectory = temp.GetPath("pixi");
        string list = temp.CreateFile("pixi/list.txt", "data");
        OptionalFileOption option = new() { RelativeToDirectory = toolDirectory };

        Assert.Equal(Path.GetFullPath(list), option.ResolvePath("list.txt"));
        Assert.True(option.Validate("list.txt"));
        Assert.Equal("list.txt", option.ParseFromString(list));
        Assert.False(option.Validate("missing.txt"));
    }

    [Fact]
    public void PathOption_ExistingInputRequiresSmcFile()
    {
        using TestDirectory temp = new();
        string smc = temp.CreateFile("game.smc", [1]);
        string txt = temp.CreateFile("game.txt", [1]);
        PathOption option = new();

        Assert.True(option.Validate(smc));
        Assert.False(option.Validate(txt));
        Assert.False(option.Validate(temp.GetPath("missing.smc")));
    }

    [Fact]
    public void PathOption_SmcExtensionIsCaseInsensitive()
    {
        using TestDirectory temp = new();
        string smc = temp.CreateFile("GAME.SMC", [1]);

        Assert.True(new PathOption().Validate(smc));
    }

    [Fact]
    public void PathOption_OutputCreatesMissingParentDirectory()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath(Path.Combine("new", "output.smc"));
        PathOption option = new() { ExistCheck = false };

        Assert.True(option.Validate(path));
        Assert.True(Directory.Exists(Path.GetDirectoryName(path)));
    }

    [Fact]
    public void DirectoryOption_InteractiveValueMustBeExeInExistingDirectory()
    {
        using TestDirectory temp = new();
        DirectoryOption option = new();

        Assert.True(option.Validate(""));
        Assert.True(option.Validate(temp.GetPath("tool.exe")));
        Assert.False(option.Validate(temp.GetPath("tool.txt")));
        Assert.False(option.Validate(Path.Combine(temp.GetPath("missing"), "tool.exe")));
    }

    [Fact]
    public void DirectoryOption_SettingsValueMayBeExistingDirectory()
    {
        using TestDirectory temp = new();
        DirectoryOption option = new() { GetFromSettings = true };

        Assert.True(option.Validate(temp.Path));
    }

    [Fact]
    public void DirectoryOption_SettingsValueRejectsAFilePath()
    {
        using TestDirectory temp = new();
        string file = temp.CreateFile("not-a-directory.txt", "data");
        DirectoryOption option = new() { GetFromSettings = true };

        Assert.False(option.Validate(file));
    }

    [Fact]
    public void FeatureDependency_AllowsUnrelatedFeatureAndRequiresEnabledParent()
    {
        FeatureDependency dependency = new()
        {
            Parent = "Graphics",
            Children = ["Poses"]
        };
        BoolOption parent = new() { Name = "Graphics", Value = false };

        Assert.True(FeatureDependency.CheckDependency("Other", dependency, [parent]));
        Assert.False(FeatureDependency.CheckDependency("Poses", dependency, [parent]));
        parent.Value = true;
        Assert.True(FeatureDependency.CheckDependency("Poses", dependency, [parent]));
    }

    [Fact]
    public void FromFile_ReturnsNullForMissingFileAndDeserializesExistingFile()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("option.json", "{\"Name\":\"Feature\",\"Value\":true}");

        BoolOption? loaded = Option.FromFile<BoolOption>(path);

        Assert.NotNull(loaded);
        Assert.Equal("Feature", loaded.Name);
        Assert.True(loaded.Value);
        Assert.Null(Option.FromFile<BoolOption>(temp.GetPath("missing.json")));
    }

    [Fact]
    public void PathParsers_StorePathsRelativeToCurrentDirectory()
    {
        using TestDirectory temp = new();
        string file = temp.GetPath(Path.Combine("tool", "game.smc"));

        Assert.Equal(Path.GetRelativePath(".\\", file),
            new PathOption().ParseFromString(file));
        Assert.Equal(Path.GetRelativePath(".\\", Path.GetDirectoryName(file)!),
            new DirectoryOption().ParseFromString(file));
        Assert.Equal(Path.GetRelativePath(".\\", file),
            new OptionalFileOption().ParseFromString(file));
        Assert.Equal("", new OptionalFileOption().ParseFromString("  "));
    }

    [Fact]
    public void ObtainValue_UsesCurrentValueAsDefaultWhenInputIsEmpty()
    {
        BoolOption option = InteractiveBool(value: true);
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetIn(new StringReader(Environment.NewLine));
            Console.SetOut(new StringWriter());

            Assert.True(option.ObtainValue());
            Assert.True(option.Value);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void ObtainValue_InvalidInputCanBeDeclinedWithoutChangingValue()
    {
        BoolOption option = InteractiveBool(value: false);
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetIn(new StringReader($"maybe{Environment.NewLine}n{Environment.NewLine}"));
            Console.SetOut(new StringWriter());

            Assert.False(option.ObtainValue());
            Assert.False(option.Value);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void DirectoryOption_EmptyInputValidatesTheExistingStoredDirectoryForm()
    {
        using TestDirectory temp = new();
        DirectoryOption option = InteractiveDirectory(temp.Path);
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetIn(new StringReader(Environment.NewLine));
            Console.SetOut(new StringWriter());

            Assert.True(option.ObtainValue());
            Assert.Equal(temp.Path, option.Value);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void DirectoryOption_EmptyInputRejectsAStoredDirectoryThatNoLongerExists()
    {
        using TestDirectory temp = new();
        string missing = temp.GetPath("missing");
        DirectoryOption option = InteractiveDirectory(missing);
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetIn(new StringReader($"{Environment.NewLine}n{Environment.NewLine}"));
            Console.SetOut(new StringWriter());

            Assert.False(option.ObtainValue());
            Assert.Equal(missing, option.Value);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    private static BoolOption InteractiveBool(bool value) => new()
    {
        Name = "Feature",
        Value = value,
        DefaultValue = false,
        Description = "Description",
        Warning = "",
        Question = "Question",
        RedoError = "Retry?",
        Error = "Error"
    };

    private static DirectoryOption InteractiveDirectory(string value) => new()
    {
        Name = "Tool",
        Value = value,
        DefaultValue = "",
        Description = "Description",
        Warning = "",
        Question = "Question",
        RedoError = "Retry?",
        Error = "Error",
        GetFromSettings = false
    };
}
