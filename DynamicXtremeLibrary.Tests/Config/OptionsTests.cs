using DynamicXtremeLibrary.Config;

namespace DynamicXtremeLibrary.Tests.Config;

public sealed class OptionsTests
{
    [Fact]
    public void GetOptionsDefines_EmitsEveryFeatureAndDependencyOverride()
    {
        using CurrentDirectoryScope cwd = new(AppContext.BaseDirectory);
        Options options = Options.Instance;
        Dictionary<BoolOption, bool> original = options.BoolOptions
            .ToDictionary(option => option, option => option.Value);
        try
        {
            foreach (BoolOption option in options.BoolOptions)
                option.Value = option.Name == "DynamicPoses";

            string result = options.GetOptionsDefines();

            Assert.StartsWith("!True = 1" + Environment.NewLine + "!False = 0", result);
            Assert.Contains("!DynamicPoses = !True", result);
            Assert.Contains("!GraphicsChange = !False", result);
            foreach (FeatureDependency dependency in options.Dependencies)
            {
                Assert.Contains($"if !{dependency.Parent} == !False", result);
                foreach (string child in dependency.Children)
                    Assert.Contains($"\t!{child} = !False", result);
            }
        }
        finally
        {
            foreach (var pair in original)
                pair.Key.Value = pair.Value;
        }
    }
}
