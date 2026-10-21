namespace DynamicXtremeLibrary.Config
{
    /// <summary>Contains user paths and feature states persisted between tool runs.</summary>
    public class Settings
    {
        /// <summary>Gets or sets the persisted input ROM path.</summary>
        public string InputROMPath;
        /// <summary>Gets or sets the persisted output ROM path.</summary>
        public string OutputROMPath;
        /// <summary>Gets or sets the persisted PIXI installation path.</summary>
        public string PixiPath;
        /// <summary>Gets or sets the persisted UberASMTool installation path.</summary>
        public string UberasmToolPath;
        /// <summary>Gets or sets the persisted GPS installation path.</summary>
        public string GPSPath;
        /// <summary>Gets or sets the persisted optional PIXI list path.</summary>
        public string? PixiListPath;
        /// <summary>Gets or sets the persisted optional UberASMTool list path.</summary>
        public string? UberasmListPath;
        /// <summary>Gets or sets the persisted feature states.</summary>
        public FeatureEnable[] Features;
    }
}
