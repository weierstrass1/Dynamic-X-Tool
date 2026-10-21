using DynamicXtremeLibrary.Defines;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Defines;

public sealed class AsarSymbolNameTests
{
    [Theory]
    [InlineData("goomba", "Goomba")]
    [InlineData("red goomba-file", "RedGoombaFile")]
    [InlineData("alreadyCamel", "AlreadyCamel")]
    [InlineData("123 sprite", "123Sprite")]
    [InlineData("a__b...c", "ABC")]
    [InlineData("---", "")]
    public void Normalize_ProducesStableAlphanumericComponent(string input, string expected)
        => Assert.Equal(expected, AsarSymbolName.Normalize(input));

    [Fact]
    public void FromPath_UsesOnlyFilenameWithoutExtension()
        => Assert.Equal("MySprite", AsarSymbolName.FromPath(
            Path.Combine("some", "folder", "my-sprite.drawinfo")));

    [Fact]
    public void ValidateUnique_AcceptsDistinctGeneratedNames()
        => Assert.True(AsarSymbolName.ValidateUnique(
            ["one.drawinfo", "two.drawinfo"], new LogRegisterSystem()));

    [Fact]
    public void ValidateUnique_RejectsEmptyGeneratedName()
    {
        LogRegisterSystem log = new();

        bool valid = AsarSymbolName.ValidateUnique(["---.drawinfo"], log);

        Assert.False(valid);
        Assert.Single(log.GetRegisters());
    }

    [Fact]
    public void ValidateUnique_RejectsNamesThatNormalizeToSameSymbol()
    {
        LogRegisterSystem log = new();

        bool valid = AsarSymbolName.ValidateUnique(
            ["red-goomba.drawinfo", "red_goomba.drawinfo"], log);

        Assert.False(valid);
        Assert.Single(log.GetRegisters());
    }
}
