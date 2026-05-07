namespace DynamicXtremeLibrary.Config
{
    [Serializable]
    public class PathOption : Option<string>
    {
        public bool ExistCheck;
        public PathOption(): base()
        {
            ExistCheck = true;
        }
        public override string ParseFromString(string value)
        {
            return Path.GetRelativePath(".\\", value);
        }
        public override bool Validate(string value)
        {
            string directory = Path.GetDirectoryName(value)!;
            if(string.IsNullOrWhiteSpace(directory))
                directory = ".\\";
            if (!Directory.Exists(directory))
            {
                if (ExistCheck)
                    return false;
                Directory.CreateDirectory(directory);
            }
            return !ExistCheck || File.Exists(value);
        }
    }
}
