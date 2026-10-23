namespace DynamicXtremeLibrary.PaletteEffec
{
    /// <summary>Identifies the three-byte color representation required by a transition effect.</summary>
    public enum PaletteEffectPaletteFormat
    {
        /// <summary>Stores each color as red, green, and blue 5-bit components.</summary>
        RGB,
        /// <summary>Stores each color as hue, saturation, and lightness 5-bit components.</summary>
        HSL
    }

    /// <summary>Converts raw SNES BGR555 palettes to the component format consumed by transitions.</summary>
    public static class PaletteEffectPaletteConverter
    {
        /// <summary>
        /// Stores the signed, fixed-point hue-sector values used by the runtime RGB-to-HSL
        /// conversion for component differences from -31 through 31.
        /// </summary>
        private static readonly int[] X60Degrees =
        [
            -165, -160, -155, -149, -144, -139, -133, -128,
            -123, -117, -112, -107, -101, -96, -91, -85,
            -80, -75, -69, -64, -59, -53, -48, -43,
            -37, -32, -27, -21, -16, -11, -5, 0,
            5, 11, 16, 21, 27, 32, 37, 43,
            48, 53, 59, 64, 69, 75, 80, 85,
            91, 96, 101, 107, 112, 117, 123, 128,
            133, 139, 144, 149, 155, 160, 165
        ];

        /// <summary>
        /// Converts every little-endian BGR555 source color to an RGB or HSL component triplet.
        /// </summary>
        /// <param name="source">The even-length raw SNES palette data.</param>
        /// <param name="format">The component representation required by the transition routine.</param>
        /// <returns>Three bytes per source color in the selected component order.</returns>
        /// <exception cref="InvalidDataException">The source length is empty or not divisible by two.</exception>
        public static byte[] Convert(ReadOnlySpan<byte> source, PaletteEffectPaletteFormat format)
        {
            if (source.Length == 0 || source.Length % 2 != 0)
                throw new InvalidDataException(
                    "A palette-effect source palette must contain complete BGR555 colors.");

            byte[] result = new byte[source.Length / 2 * 3];
            int outputIndex = 0;
            for (int inputIndex = 0; inputIndex < source.Length; inputIndex += 2)
            {
                int color = (source[inputIndex] | source[inputIndex + 1] << 8) & 0x7FFF;
                byte red = (byte)(color & 0x1F);
                byte green = (byte)((color >> 5) & 0x1F);
                byte blue = (byte)((color >> 10) & 0x1F);

                if (format == PaletteEffectPaletteFormat.RGB)
                {
                    result[outputIndex++] = red;
                    result[outputIndex++] = green;
                    result[outputIndex++] = blue;
                    continue;
                }

                (byte hue, byte saturation, byte lightness) = convertToHsl(red, green, blue);
                result[outputIndex++] = hue;
                result[outputIndex++] = saturation;
                result[outputIndex++] = lightness;
            }
            return result;
        }

        /// <summary>Reproduces the integer RGB-to-HSL conversion used by the 65C816 runtime.</summary>
        private static (byte Hue, byte Saturation, byte Lightness) convertToHsl(
            byte red, byte green, byte blue)
        {
            int maximum = Math.Max(red, Math.Max(green, blue));
            int minimum = Math.Min(red, Math.Min(green, blue));
            int lightness = (minimum + maximum) >> 1;
            int chroma = maximum - minimum;
            if (chroma == 0)
                return (0, 0, (byte)lightness);

            int divisor = 31 - Math.Abs(minimum + maximum - 31);
            int saturation = chroma * 31 / divisor;
            int hue;
            if (maximum == red)
                hue = hueDelta(green - blue, chroma);
            else if (maximum == green)
                hue = 11 + hueDelta(blue - red, chroma);
            else
                hue = 21 + hueDelta(red - green, chroma);

            hue = (hue % 32 + 32) % 32;
            return ((byte)hue, (byte)saturation, (byte)lightness);
        }

        /// <summary>Calculates the signed hue-sector displacement using the runtime lookup table.</summary>
        private static int hueDelta(int componentDifference, int chroma)
        {
            int scaledDifference = X60Degrees[componentDifference + 31];
            int magnitude = Math.Abs(scaledDifference) / chroma;
            return scaledDifference < 0 ? -magnitude : magnitude;
        }
    }
}
