using DynamicXtremeLibrary.Config;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DynamicXtremeLibrary.Tests.Config;

public sealed class SettingsValidationTests
{
    [Fact]
    public void Instance_RejectsMalformedJsonWithParserLocationAndCause()
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings("{\n  \"InputROMPath\": \"game.smc\",\n", _ => true));

        Assert.Contains("Settings.json", exception.Message);
        Assert.Contains("line", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("position", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Instance_RequiresAnObjectAtTheJsonRoot()
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings("[]", _ => true));

        Assert.Contains("Field '$'", exception.Message);
        Assert.Contains("root value must be a JSON object", exception.Message);
    }

    [Theory]
    [InlineData("InputROMPath")]
    [InlineData("OutputROMPath")]
    [InlineData("PixiPath")]
    [InlineData("UberasmToolPath")]
    [InlineData("GPSPath")]
    [InlineData("PixiListPath")]
    [InlineData("UberasmListPath")]
    public void Instance_RequiresEveryDeclaredPathField(string field)
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document.Remove(field);
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains($"Field '{field}'", exception.Message);
        Assert.Contains("field is required", exception.Message);
    }

    [Theory]
    [InlineData("InputROMPath")]
    [InlineData("OutputROMPath")]
    [InlineData("PixiPath")]
    [InlineData("UberasmToolPath")]
    [InlineData("GPSPath")]
    public void Instance_RejectsNullOrNonStringRequiredPaths(string field)
    {
        foreach (JsonNode? invalidValue in new JsonNode?[] { null, JsonValue.Create(123), new JsonArray() })
        {
            InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
                WithSettings(temp =>
                {
                    JsonObject document = ValidDocument(temp);
                    document[field] = invalidValue?.DeepClone();
                    return document.ToJsonString();
                }, _ => true));

            Assert.Contains($"Field '{field}'", exception.Message);
            Assert.Contains("must be a string", exception.Message);
        }
    }

    [Fact]
    public void Instance_AllowsNullForOptionalListPathsAndMapsItToDisabledEmptyValues()
    {
        WithSettings(temp =>
        {
            JsonObject document = ValidDocument(temp);
            document["PixiListPath"] = null;
            document["UberasmListPath"] = null;
            return document.ToJsonString();
        }, options =>
        {
            Assert.Equal("", options.PixiListPath.Value);
            Assert.Equal("", options.UberasmListPath.Value);
            return true;
        });
    }

    [Theory]
    [InlineData("PixiListPath")]
    [InlineData("UberasmListPath")]
    public void Instance_RejectsNonStringNonNullOptionalListPaths(string field)
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document[field] = 123;
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains($"Field '{field}'", exception.Message);
        Assert.Contains("string or null", exception.Message);
    }

    [Fact]
    public void Instance_RequiresFeaturesToBeAnArrayOfObjects()
    {
        InvalidSettingsException wrongContainer = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document["Features"] = new JsonObject();
                return document.ToJsonString();
            }, _ => true));
        Assert.Contains("Field 'Features'", wrongContainer.Message);
        Assert.Contains("must be an array", wrongContainer.Message);

        InvalidSettingsException wrongEntry = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document["Features"] = new JsonArray("GraphicsChange");
                return document.ToJsonString();
            }, _ => true));
        Assert.Contains("Field 'Features[0]'", wrongEntry.Message);
        Assert.Contains("must be an object", wrongEntry.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UnknownFeature")]
    [InlineData("graphicschange")]
    public void Instance_RejectsMissingEmptyOrUnknownFeatureNames(string? name)
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                JsonObject feature = new() { ["Enable"] = true };
                if (name != null)
                    feature["Name"] = name;
                document["Features"] = new JsonArray(feature);
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains("Field 'Features[0].Name'", exception.Message);
        Assert.Contains(name is "UnknownFeature" or "graphicschange"
            ? "not a recognized feature"
            : "non-empty string", exception.Message);
    }

    [Fact]
    public void Instance_RejectsDuplicateFeatureNames()
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document["Features"] = new JsonArray(
                    Feature("GraphicsChange", true),
                    Feature("GraphicsChange", false));
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains("Field 'Features[1].Name'", exception.Message);
        Assert.Contains("duplicated", exception.Message);
    }

    [Theory]
    [InlineData("\"true\"")]
    [InlineData("1")]
    [InlineData("null")]
    public void Instance_RequiresFeatureEnableToBeAJsonBoolean(string invalidJson)
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                JsonObject feature = Feature("GraphicsChange", true);
                feature["Enable"] = JsonNode.Parse(invalidJson);
                document["Features"] = new JsonArray(feature);
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains("Field 'Features[0].Enable'", exception.Message);
        Assert.Contains("true or false", exception.Message);
    }

    [Fact]
    public void Instance_RequiresEveryFeatureToDeclareEnable()
    {
        InvalidSettingsException exception = Assert.Throws<InvalidSettingsException>(() =>
            WithSettings(temp =>
            {
                JsonObject document = ValidDocument(temp);
                document["Features"] = new JsonArray(new JsonObject
                {
                    ["Name"] = "GraphicsChange"
                });
                return document.ToJsonString();
            }, _ => true));

        Assert.Contains("Field 'Features[0].Enable'", exception.Message);
        Assert.Contains("true or false", exception.Message);
    }

    [Fact]
    public void SettingsForm_UseSettingsAcceptsValidPathsAndToolRelativeLists()
    {
        WithSettings(temp =>
        {
            string input = temp.CreateFile("rom/input.smc", [1]);
            string pixiList = temp.CreateFile("pixi/list.txt", "");
            string uberList = temp.CreateFile("uber/list.txt", "");
            Directory.CreateDirectory(temp.GetPath("gps"));
            JsonObject document = ValidDocument(temp);
            document["InputROMPath"] = input;
            document["OutputROMPath"] = temp.GetPath("rom/output.smc");
            document["PixiPath"] = temp.GetPath("pixi");
            document["UberasmToolPath"] = temp.GetPath("uber");
            document["GPSPath"] = temp.GetPath("gps");
            document["PixiListPath"] = "list.txt";
            document["UberasmListPath"] = "list.txt";
            Assert.True(File.Exists(pixiList));
            Assert.True(File.Exists(uberList));
            return document.ToJsonString();
        }, options =>
        {
            Assert.True(options.SettingsForm(useSettings: true));
            Assert.True(File.Exists(options.PixiListPath.ResolvePath(options.PixiListPath.Value)));
            Assert.True(File.Exists(options.UberasmListPath.ResolvePath(options.UberasmListPath.Value)));
            return true;
        });
    }

    [Fact]
    public void SettingsForm_UseSettingsPromptsOnlyForInvalidPathsAndPreservesValidValues()
    {
        WithSettings(temp =>
        {
            string validUber = temp.GetPath("uber");
            string validGps = temp.GetPath("gps");
            Directory.CreateDirectory(validUber);
            Directory.CreateDirectory(validGps);
            JsonObject document = ValidDocument(temp);
            document["InputROMPath"] = temp.GetPath("missing/input.smc");
            document["OutputROMPath"] = temp.GetPath("output.txt");
            document["PixiPath"] = temp.GetPath("missing-pixi");
            document["UberasmToolPath"] = validUber;
            document["GPSPath"] = validGps;
            document["PixiListPath"] = "missing-list.txt";
            document["UberasmListPath"] = "";
            return document.ToJsonString();
        }, options =>
        {
            string validInput = Path.GetFullPath("replacement-input.smc");
            File.WriteAllBytes(validInput, [1]);
            string validOutput = Path.GetFullPath("replacement-output.smc");
            string pixiExecutable = Path.GetFullPath("pixi/pixi.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(pixiExecutable)!);
            File.WriteAllBytes(pixiExecutable, []);

            string originalUber = options.UberasmPath.Value;
            string originalGps = options.GPSPath.Value;
            TextReader originalInput = Console.In;
            TextWriter originalOutput = Console.Out;
            StringWriter output = new();
            try
            {
                Console.SetIn(new StringReader(string.Join(Environment.NewLine,
                    validInput,
                    validOutput,
                    pixiExecutable,
                    "") + Environment.NewLine));
                Console.SetOut(output);

                Assert.True(options.SettingsForm(useSettings: true));
            }
            finally
            {
                Console.SetIn(originalInput);
                Console.SetOut(originalOutput);
            }

            string console = output.ToString();
            Assert.Contains("field 'InputROMPath'", console);
            Assert.Contains("field 'OutputROMPath'", console);
            Assert.Contains("field 'PixiPath'", console);
            Assert.Contains("field 'PixiListPath'", console);
            Assert.Contains("Line", console);
            Assert.Contains("relative to its tool directory", console);
            Assert.Equal(validInput, Path.GetFullPath(options.InputRomPath.Value));
            Assert.Equal(validOutput, Path.GetFullPath(options.OutputRomPath.Value));
            Assert.Equal(Path.GetDirectoryName(pixiExecutable),
                Path.GetFullPath(options.PixiPath.Value));
            Assert.Equal("", options.PixiListPath.Value);
            Assert.Equal(originalUber, options.UberasmPath.Value);
            Assert.Equal(originalGps, options.GPSPath.Value);
            return true;
        });
    }

    [Fact]
    public void SettingsForm_UseSettingsPromptsForReplacementWhenOutputParentIsAFile()
    {
        WithSettings(temp =>
        {
            string input = temp.CreateFile("input.smc", [1]);
            string parentFile = temp.CreateFile("not-a-directory", "data");
            JsonObject document = ValidDocument(temp);
            document["InputROMPath"] = input;
            document["OutputROMPath"] = Path.Combine(parentFile, "output.smc");
            return document.ToJsonString();
        }, options =>
        {
            string replacement = Path.GetFullPath("replacement-output.smc");
            TextReader originalInput = Console.In;
            TextWriter originalOutput = Console.Out;
            StringWriter output = new();
            try
            {
                Console.SetIn(new StringReader(replacement + Environment.NewLine));
                Console.SetOut(output);

                Assert.True(options.SettingsForm(useSettings: true));
            }
            finally
            {
                Console.SetIn(originalInput);
                Console.SetOut(originalOutput);
            }

            Assert.Contains("field 'OutputROMPath'", output.ToString());
            Assert.Equal(replacement, Path.GetFullPath(options.OutputRomPath.Value));
            return true;
        });
    }

    private static JsonObject ValidDocument(TestDirectory temp) => new()
    {
        ["InputROMPath"] = temp.GetPath("input.smc"),
        ["OutputROMPath"] = temp.GetPath("output.smc"),
        ["PixiPath"] = "",
        ["UberasmToolPath"] = "",
        ["GPSPath"] = "",
        ["PixiListPath"] = "",
        ["UberasmListPath"] = "",
        ["Features"] = new JsonArray(Feature("GraphicsChange", true))
    };

    private static JsonObject Feature(string name, bool enabled) => new()
    {
        ["Name"] = name,
        ["Enable"] = enabled
    };

    private static T WithSettings<T>(string json, Func<Options, T> action)
        => WithSettings(_ => json, action);

    private static T WithSettings<T>(Func<TestDirectory, string> jsonFactory,
        Func<Options, T> action)
    {
        using TestDirectory temp = new();
        CopyDirectory(Path.Combine(AppContext.BaseDirectory, "Settings"),
            temp.GetPath("Settings"));
        File.WriteAllText(temp.GetPath("Settings/Settings.json"), jsonFactory(temp));
        using CurrentDirectoryScope cwd = new(temp.Path);
        Options options = CreateOptions();
        return action(options);
    }

    private static Options CreateOptions()
    {
        try
        {
            return (Options)Activator.CreateInstance(typeof(Options), nonPublic: true)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(directory.Replace(source, destination, StringComparison.Ordinal));
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string target = file.Replace(source, destination, StringComparison.Ordinal);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }
}
