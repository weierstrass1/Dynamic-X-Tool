using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Tests.Readers;

public sealed class HexReaderTests
{
    [Fact]
    public void ValuesToString_ReturnsEmptyForNullOrEmptyInput()
    {
        Assert.Equal("", HexReader.ValuesToString(null, 2));
        Assert.Equal("", HexReader.ValuesToString([], 2));
    }

    [Theory]
    [InlineData(2, "db", "$01,$AB")]
    [InlineData(4, "dw", "$0001,$00AB")]
    [InlineData(6, "dl", "$000001,$0000AB")]
    public void ValuesToString_SelectsDeclarationFromRequestedWidth(
        int width, string directive, string values)
        => Assert.Equal($"\n\t{directive} {values}",
            HexReader.ValuesToString([1, 0xAB], width));

    [Fact]
    public void ValuesToString_InsertsConfiguredLineBreaks()
        => Assert.Equal("\n\tdb $01,$02\n\tdb $03,$04",
            HexReader.ValuesToString([1, 2, 3, 4], 2, 2));

    [Fact]
    public void GetValues_ExtractsDecimalAndDollarPrefixedHexadecimalValues()
        => Assert.Equal([10, 10, 255, 42],
            HexReader.GetValues("db 10,$0A,$ff ; ignored text 42"));

    [Fact]
    public void GetValues_ReturnsEmptyWhenNoNumbersExist()
        => Assert.Empty(HexReader.GetValues("db nothing"));
}
