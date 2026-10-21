namespace DynamicXtremeLibrary.Config
{
    /// <summary>Stores the persisted enabled state of one named feature.</summary>
    [Serializable]
    public class FeatureEnable
    {
        /// <summary>Gets or sets the feature name.</summary>
        public string Name;
        /// <summary>Gets or sets whether the feature is enabled.</summary>
        public bool Enable;
    }
}
