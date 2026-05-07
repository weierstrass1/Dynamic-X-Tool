namespace DynamicXtremeLibrary.Config
{
    [Serializable]
    public class BoolOption : Option<bool>
    {
        public BoolOption() : base()
        {
        }
        public override bool ParseFromString(string value)
        {
            if(string.IsNullOrWhiteSpace(value)) 
                return false;
            string v = value.ToLower().Trim();
            return v.Equals("true") || v.Equals("yes") || v.Equals("y") || v.Equals("t");
        }
        public override bool Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            string v = value.ToLower().Trim();
            return v.Equals("true") || v.Equals("yes") || v.Equals("y") || v.Equals("t") ||
                v.Equals("false") || v.Equals("no") || v.Equals("n") || v.Equals("f");
        }
    }
}
