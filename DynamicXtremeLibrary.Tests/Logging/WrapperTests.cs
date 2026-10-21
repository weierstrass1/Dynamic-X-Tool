using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremeLibrary.Logging.Wrappers;
using LogRegister;

namespace DynamicXtremeLibrary.Tests.Logging;

public sealed class WrapperTests
{
    [Fact]
    public void ConsoleWrapper_WritesOnlyRequiredNonPrefixSpans()
    {
        TextWriter original = Console.Out;
        StringWriter output = new();
        try
        {
            Console.SetOut(output);
            ConsoleWrapper.RenderAction("ignored", new Info(), SpanType.NormalText);
            ConsoleWrapper.RenderAction("prefix", new Info(), SpanType.Prefix, mustWrite: true);
            ConsoleWrapper.RenderAction("visible", new Info(), SpanType.NormalText, mustWrite: true);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal("visible", output.ToString());
    }

    [Fact]
    public void RawTextWrapper_ExcludesPrefixesAndAppendsOtherSpans()
    {
        RawTextWrapper wrapper = new();
        Info category = new();

        wrapper.RenderAction("[INFO]", category, SpanType.Prefix);
        wrapper.RenderAction("hello", category, SpanType.NormalText);
        wrapper.RenderAction(" world", category, SpanType.Parameter);

        Assert.Equal("hello world", wrapper.ToString());
    }

    [Fact]
    public void MultiWrapper_BroadcastsAllArgumentsToSubscribers()
    {
        MultiWrapper wrapper = new();
        List<(string Text, SpanType Type, bool MustWrite)> calls = [];
        wrapper.Actions += (text, _, type, mustWrite) => calls.Add((text, type, mustWrite));

        wrapper.RenderAction("value", new Info(), SpanType.Parameter, mustWrite: true);

        Assert.Equal(("value", SpanType.Parameter, true), Assert.Single(calls));
    }
}
