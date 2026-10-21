using Newtonsoft.Json;

namespace DynamicXtremeLibrary.Config
{
    /// <summary>Provides shared loading behavior for serialized option definitions.</summary>
    public abstract class  Option
    {
        /// <summary>Deserializes an option definition when its file exists.</summary>
        /// <typeparam name="K">The concrete option type.</typeparam>
        /// <param name="path">The JSON option-definition path.</param>
        /// <returns>The loaded option, or <see langword="null"/> when the file is absent.</returns>
        public static K? FromFile<K>(string path) where K : Option, new()
        {
            if (!File.Exists(path))
                return null;
            string jsonString = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<K>(jsonString)!;
        }
    }
    /// <summary>Defines the prompts, value, parsing, and validation contract for an interactive option.</summary>
    /// <typeparam name="T">The strongly typed option value.</typeparam>
    public abstract class Option<T> : Option
    {
        /// <summary>Gets or sets the option name.</summary>
        public string Name;
        /// <summary>Gets or sets the current value.</summary>
        public T Value;
        /// <summary>Gets or sets the fallback value used for empty input.</summary>
        public T DefaultValue;
        /// <summary>Gets or sets the description printed before prompting.</summary>
        public string Description;
        /// <summary>Gets or sets the optional warning printed before prompting.</summary>
        public string Warning;
        /// <summary>Gets or sets the input question.</summary>
        public string Question;
        /// <summary>Gets or sets the prompt shown after invalid input.</summary>
        public string RedoError;
        /// <summary>Gets or sets the option-specific validation error text.</summary>
        public string Error;
        /// <summary>Prompts until a valid value is obtained or the user declines to retry.</summary>
        /// <returns>Whether a valid value was obtained.</returns>
        public bool ObtainValue()
        {
            Console.WriteLine();
            Console.WriteLine(Description);
            if (!string.IsNullOrWhiteSpace(Warning))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Warning);
                Console.ResetColor();
            }
            Console.WriteLine(Question);
            T def = Value != null && !string.IsNullOrWhiteSpace(Value.ToString()) ?
                    Value :
                    DefaultValue;
            string value = Console.ReadLine()!;
            value = value.Replace("\'", "").Replace("\"", "").Trim();
            T tValue = !string.IsNullOrWhiteSpace(value) ?
                ParseFromString(value) :
                def;

            while (!Validate(value))
            {
                Console.WriteLine(RedoError);
                value = Console.ReadLine()!;
                if (value.ToLower().Trim() != "y")
                    return false;
                Console.WriteLine(Question);
                value = Console.ReadLine()!;
                value = value.Replace("\'", "").Replace("\"", "").Trim();
                tValue = !string.IsNullOrWhiteSpace(value) ?
                    ParseFromString(value) :
                    def;
            }
            Value = tValue;
            return true;
        }
        /// <summary>Converts validated text into the option value.</summary>
        /// <param name="value">The user-supplied text.</param>
        /// <returns>The parsed value.</returns>
        public abstract T ParseFromString(string value);
        /// <summary>Determines whether user-supplied text is valid for this option.</summary>
        /// <param name="value">The text to validate.</param>
        /// <returns>Whether the text is accepted.</returns>
        public abstract bool Validate(string value);
    }
}
