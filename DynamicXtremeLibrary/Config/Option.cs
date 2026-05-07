using Newtonsoft.Json;

namespace DynamicXtremeLibrary.Config
{
    public abstract class  Option
    {
        public static K? FromFile<K>(string path) where K : Option, new()
        {
            if (!File.Exists(path))
                return null;
            string jsonString = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<K>(jsonString)!;
        }
    }
    public abstract class Option<T> : Option
    {
        public string Name;
        public T Value;
        public T DefaultValue;
        public string Description;
        public string Warning;
        public string Question;
        public string RedoError;
        public string Error;
        public void ObtainValue()
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
            string value = Console.ReadLine()!;
            T tValue = string.IsNullOrWhiteSpace(value) ?
                DefaultValue :
                ParseFromString(value);

            while (!Validate(value))
            {
                Console.WriteLine(RedoError);
                Console.WriteLine(Question);
                value = Console.ReadLine()!;
                tValue = string.IsNullOrWhiteSpace(value) ?
                    DefaultValue :
                    ParseFromString(value);
            }
            Value = tValue;
        }
        public abstract T ParseFromString(string value);
        public abstract bool Validate(string value);
    }
}
