namespace DynamicXtremeLibrary.Config
{
    /// <summary>Represents an interactive Boolean configuration option.</summary>
    [Serializable]
    public class BoolOption : Option<bool>
    {
        /// <summary>Initializes a Boolean option.</summary>
        public BoolOption() : base()
        {
        }
        /// <summary>Converts an accepted affirmative string into a Boolean value.</summary>
        /// <param name="value">The user-supplied text.</param>
        /// <returns><see langword="true"/> for accepted affirmative forms.</returns>
        public override bool ParseFromString(string value)
        {
            if(string.IsNullOrWhiteSpace(value)) 
                return false;
            string v = value.ToLower().Trim();
            return v.Equals("true") || v.Equals("yes") || v.Equals("y") || v.Equals("t");
        }
        /// <summary>Checks whether text is empty or uses a supported affirmative or negative form.</summary>
        /// <param name="value">The user-supplied text.</param>
        /// <returns>Whether the text can be parsed as a Boolean response.</returns>
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
