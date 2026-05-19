namespace DynamicXtremeLibrary.Config
{
    [Serializable]
    public class DirectoryOption : Option<string>
    {
        public bool GetFromSettings = false;
        public DirectoryOption() : base()
        {
        }
        public override string ParseFromString(string value)
        {
            string directory = Path.GetDirectoryName(value)!;
            if (string.IsNullOrWhiteSpace(directory))
                directory = ".\\";
            return Path.GetRelativePath(".\\", directory);
        }
        public override bool Validate(string value)
        {
            if(Path.GetExtension(value) != ".exe" && !GetFromSettings)
                return false;
            string? directory = Path.GetDirectoryName(value);
            return string.IsNullOrWhiteSpace(directory) || Directory.Exists(directory);
        }
    }
}
