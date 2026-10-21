using System.Text;
using DynamicXtremeLibrary.Inputs;

namespace DynamicXtremeLibrary.Tests.Inputs;

public sealed class SourceRewriteTests
{
    [Fact]
    public void AddSymbolChange_IgnoresIdentityAndRejectsAmbiguousMapping()
    {
        SourceRewrite rewrite = new() { Path = "consumer.asm" };

        rewrite.AddSymbolChange("!Old", "!Old");
        rewrite.AddSymbolChange("!Old", "!New");
        rewrite.AddSymbolChange("!Old", "!New");

        Assert.Single(rewrite.SymbolChanges);
        Assert.Throws<InvalidOperationException>(() =>
            rewrite.AddSymbolChange("!Old", "!Different"));
    }

    [Fact]
    public void BuildContent_ReplacesExactSymbolsAndUpdatesDirectiveMarker()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("consumer.asm", """
            ;#incdri "pose.drawinfo" ; dxt-name: Old
            LDA !PoseIDOld_Idle
            LDA !PoseIDOld_IdleExtra
            """);
        SourceRewrite rewrite = new() { Path = path };
        rewrite.AddSymbolChange("!PoseIDOld_Idle", "!PoseIDNew_Idle");
        rewrite.SetDirectiveLogicalName(0, "New");

        string result = Encoding.UTF8.GetString(rewrite.BuildContent());

        Assert.Contains(";#incdri \"pose.drawinfo\" ; dxt-name: New", result);
        Assert.Contains("LDA !PoseIDNew_Idle", result);
        Assert.Contains("LDA !PoseIDOld_IdleExtra", result);
    }

    [Fact]
    public void BuildContent_PreservesCrLfFinalNewlineAndUtf8Bom()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("consumer.asm");
        File.WriteAllBytes(path,
            [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes("LDA !Old\r\n")]);
        SourceRewrite rewrite = new() { Path = path };
        rewrite.AddSymbolChange("!Old", "!New");

        byte[] result = rewrite.BuildContent();

        Assert.True(result.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
        Assert.Equal("LDA !New\r\n", Encoding.UTF8.GetString(result, 3, result.Length - 3));
    }

    [Fact]
    public void BuildContent_UsesLatin1AsBytePreservingFallback()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("consumer.asm", [0xE9, 0x20, 0x21]);
        SourceRewrite rewrite = new() { Path = path };

        Assert.Equal([0xE9, 0x20, 0x21], rewrite.BuildContent());
    }

    [Fact]
    public void BuildContent_PreservesUtf16LittleEndianBomAndCrLf()
    {
        using TestDirectory temp = new();
        string path = temp.GetPath("consumer.asm");
        System.Text.Encoding encoding = System.Text.Encoding.Unicode;
        string original = ";#incdri \"enemy.drawinfo\"\r\n!PoseIDEnemy_Idle\r\n";
        File.WriteAllBytes(path, [.. encoding.GetPreamble(), .. encoding.GetBytes(original)]);
        SourceRewrite rewrite = new() { Path = path };
        rewrite.AddSymbolChange("!PoseIDEnemy_Idle", "!PoseIDEnemySprite_Idle");
        rewrite.SetDirectiveLogicalName(0, "EnemySprite");

        byte[] result = rewrite.BuildContent();

        Assert.True(result.AsSpan().StartsWith(encoding.GetPreamble()));
        string content = encoding.GetString(result, encoding.GetPreamble().Length,
            result.Length - encoding.GetPreamble().Length);
        Assert.Equal(
            ";#incdri \"enemy.drawinfo\" ; dxt-name: EnemySprite\r\n!PoseIDEnemySprite_Idle\r\n",
            content);
    }

    [Fact]
    public void BuildContent_RejectsDirectiveLineThatNoLongerExists()
    {
        using TestDirectory temp = new();
        string path = temp.CreateFile("consumer.asm", "one line");
        SourceRewrite rewrite = new() { Path = path };
        rewrite.SetDirectiveLogicalName(4, "Name");

        Assert.Throws<InvalidOperationException>(() => rewrite.BuildContent());
    }
}
