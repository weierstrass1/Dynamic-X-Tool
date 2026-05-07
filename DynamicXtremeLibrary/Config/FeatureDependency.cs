namespace DynamicXtremeLibrary.Config
{
    public class FeatureDependency
    {
        public required string Parent;
        public required string[] Children;
        public static bool CheckDependency(string feature, FeatureDependency dependency, BoolOption[] BoolOptions)
        {
            if (dependency.Children.FirstOrDefault(c => c.Equals(feature)) == default)
                return true;
            return BoolOptions.First(opt => opt.Name.Equals(dependency.Parent)).Value;
        }
    }
}
