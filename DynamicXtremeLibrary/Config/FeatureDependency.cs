namespace DynamicXtremeLibrary.Config
{
    /// <summary>Defines a parent feature required by one or more child features.</summary>
    public class FeatureDependency
    {
        /// <summary>Gets or sets the required parent feature name.</summary>
        public required string Parent;
        /// <summary>Gets or sets the child feature names controlled by the parent.</summary>
        public required string[] Children;
        /// <summary>Checks whether a feature is permitted by one dependency rule.</summary>
        /// <param name="feature">The candidate child feature.</param>
        /// <param name="dependency">The dependency rule to evaluate.</param>
        /// <param name="BoolOptions">The current feature-option values.</param>
        /// <returns>Whether the feature is unrelated to the rule or its parent is enabled.</returns>
        public static bool CheckDependency(string feature, FeatureDependency dependency, BoolOption[] BoolOptions)
        {
            if (dependency.Children.FirstOrDefault(c => c.Equals(feature)) == default)
                return true;
            return BoolOptions.First(opt => opt.Name.Equals(dependency.Parent)).Value;
        }
    }
}
