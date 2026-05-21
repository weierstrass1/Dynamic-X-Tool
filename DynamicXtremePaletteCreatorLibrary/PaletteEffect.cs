namespace DynamicXtremePaletteCreatorLibrary
{
    public enum Channels { RGB = 0, HSL = 1 }
    public enum EffectType
    {
        None = -1,
        MixRGB = 0,
        MixHSL = 1,
        MixRG = 2,
        MixRB = 3,
        MixGB = 4,
        MixHS = 5,
        MixHL = 6,
        MixSL = 7,
        MixR = 8,
        MixG = 9,
        MixB = 10,
        MixH = 11,
        MixS = 12,
        MixL = 13,
        PalTransitionRGB = 14,
        PalTransitionHSL = 15,
        PalFunctionRGB = 16,
        PalFunctionHSL = 17
    }
    [Serializable]
    public class PaletteEffect
    {
        public byte Channel1 { get; set; }
        public byte Channel2 { get; set; }
        public byte Channel3 { get; set; }
        public byte Ratio1 { get; set; }
        public byte Ratio2 { get; set; }
        public byte Ratio3 { get; set; }
        public EffectType EffectType { get; set; }
        public PaletteEffect()
        {

        }
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
