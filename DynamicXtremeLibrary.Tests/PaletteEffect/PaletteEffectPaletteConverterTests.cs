using DynamicXtremeLibrary.PaletteEffec;

namespace DynamicXtremeLibrary.Tests.PaletteEffect;

public sealed class PaletteEffectPaletteConverterTests
{
    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x1F })]
    [InlineData(new byte[] { 0x00, 0x00, 0x00 })]
    public void Convert_RejectsEmptyAndIncompleteBgr555Data(byte[] source)
        => Assert.Throws<InvalidDataException>(() =>
            PaletteEffectPaletteConverter.Convert(source, PaletteEffectPaletteFormat.RGB));

    [Fact]
    public void Convert_RgbDecodesLittleEndianBgr555AndIgnoresBit15()
    {
        byte[] source =
        [
            0x00, 0x00, // black
            0x1F, 0x00, // red
            0xE0, 0x03, // green
            0x00, 0x7C, // blue
            0xFF, 0xFF  // white with ignored bit 15 set
        ];

        byte[] result = PaletteEffectPaletteConverter.Convert(
            source, PaletteEffectPaletteFormat.RGB);

        Assert.Equal([
            0, 0, 0,
            31, 0, 0,
            0, 31, 0,
            0, 0, 31,
            31, 31, 31
        ], result);
    }

    [Fact]
    public void Convert_HslMatchesRuntimeIntegerContractForPrimariesSecondariesAndAchromatics()
    {
        byte[] source =
        [
            0x00, 0x00, // black
            0xFF, 0x7F, // white
            0xEF, 0x3D, // gray 15,15,15
            0x1F, 0x00, // red
            0xE0, 0x03, // green
            0x00, 0x7C, // blue
            0xFF, 0x03, // yellow
            0xE0, 0x7F, // cyan
            0x1F, 0x7C  // magenta
        ];

        byte[] result = PaletteEffectPaletteConverter.Convert(
            source, PaletteEffectPaletteFormat.HSL);

        Assert.Equal([
            0, 0, 0,
            0, 0, 31,
            0, 0, 15,
            0, 31, 15,
            11, 31, 15,
            21, 31, 15,
            5, 31, 15,
            16, 31, 15,
            27, 31, 15
        ], result);
    }
}
