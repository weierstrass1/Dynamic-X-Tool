namespace DynamicXtremePaletteCreatorLibrary
{
    /// <summary>Selects whether effect components represent RGB or HSL channels.</summary>
    public enum Channels { RGB = 0, HSL = 1 }
    /// <summary>Identifies the runtime palette mixing, transition, or function operation.</summary>
    public enum EffectType
    {
        None = -1,
        MixRGB = 0,
        MixHSL = 1,
        MixRG = 2,
        MixHS = 3,
        MixRB = 4,
        MixHL = 5,
        MixGB = 6,
        MixSL = 7,
        MixR = 8,
        MixH = 9,
        MixG = 10,
        MixS = 11,
        MixB = 12,
        MixL = 13,
        PalTransitionRGB = 14,
        PalTransitionHSL = 15,
        PalFunctionRGB = 16,
        PalFunctionHSL = 17
    }
    /// <summary>Stores three channel values, their ratios, and the derived runtime effect type.</summary>
    [Serializable]
    public class PaletteEffect
    {
        /// <summary>Gets or sets the first channel value.</summary>
        public byte Channel1 { get; set; }
        /// <summary>Gets or sets the second channel value.</summary>
        public byte Channel2 { get; set; }
        /// <summary>Gets or sets the third channel value.</summary>
        public byte Channel3 { get; set; }
        /// <summary>Gets or sets the first channel ratio.</summary>
        public byte Ratio1 { get; set; }
        /// <summary>Gets or sets the second channel ratio.</summary>
        public byte Ratio2 { get; set; }
        /// <summary>Gets or sets the third channel ratio.</summary>
        public byte Ratio3 { get; set; }
        /// <summary>Gets or sets the runtime effect operation.</summary>
        public EffectType EffectType { get; set; }
        /// <summary>Initializes an empty palette effect.</summary>
        public PaletteEffect()
        {

        }
        /// <summary>Derives the effect opcode from the color space and nonzero channel ratios.</summary>
        public static EffectType GetEffectType(Channels channels, byte r1, byte r2, byte r3)
        {
            int value = (int)channels;
            if (r1 != 0 && r2 != 0 && r3 != 0)
                return (EffectType)value;
            if (r1 != 0 && r2 != 0)
                return (EffectType)(value + 2);
            if (r1 != 0 && r3 != 0)
                return (EffectType)(value + 4);
            if (r2 != 0 && r3 != 0)
                return (EffectType)(value + 6);
            if (r1 != 0)
                return (EffectType)(value + 8);
            if (r2 != 0)
                return (EffectType)(value + 10);
            if (r3 != 0)
                return (EffectType)(value + 12);
            return EffectType.None;
        }
    }
}
