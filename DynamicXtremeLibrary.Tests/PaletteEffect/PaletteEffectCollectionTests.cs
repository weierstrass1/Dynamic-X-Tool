using DynamicXtremePaletteCreatorLibrary;
using System.Text.Json;

namespace DynamicXtremeLibrary.Tests.PaletteEffect;

public sealed class PaletteEffectCollectionTests
{
    [Theory]
    [InlineData("\"LegacyName\"")]
    [InlineData("null")]
    [InlineData("123")]
    [InlineData("{\"Nested\":true}")]
    public void Load_IgnoresLegacyNameRegardlessOfItsJsonValue(string legacyName)
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect", $$"""
            {"Name":{{legacyName}},"Effects":[{"EffectType":0,"Channel1":7}]}
            """);
        PaletteEffectCollection collection = new() { Name = "ToolAssigned" };

        bool loaded = collection.Load(path);

        Assert.True(loaded);
        Assert.Equal("ToolAssigned", collection.Name);
        Assert.Equal(7, Assert.Single(collection.Effects).Channel1);
    }

    [Fact]
    public void Load_DoesNotImportLegacyNameWhenNoRuntimeNameWasAssigned()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("effect.paleffect",
            "{\"Name\":\"LegacyName\",\"Effects\":[]}");
        PaletteEffectCollection collection = new();

        Assert.True(collection.Load(path));
        Assert.Null(collection.Name);
    }

    [Fact]
    public void Save_ExcludesRuntimeNameFromPaletteEffectJson()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("effect.paleffect");
        PaletteEffectCollection collection = new()
        {
            Name = "RuntimeOnly",
            Effects = [new() { EffectType = EffectType.MixRGB, Channel1 = 5 }]
        };

        collection.Save(path);

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.False(document.RootElement.TryGetProperty("Name", out _));
        JsonElement effect = Assert.Single(
            document.RootElement.GetProperty("Effects").EnumerateArray());
        Assert.Equal(5, effect.GetProperty("Channel1").GetByte());
        Assert.Equal("RuntimeOnly", collection.Name);
    }
}
