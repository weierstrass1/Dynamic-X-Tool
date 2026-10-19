using DynamicXtremeLibrary.Logging.LoggingRegisters;
using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.Defines
{
    public static class AsarSymbolName
    {
        public static string Normalize(string name)
        {
            StringBuilder result = new();
            bool capitalizeNextLetter = true;

            foreach (char character in name)
            {
                bool isLetter = character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
                bool isDigit = character is >= '0' and <= '9';
                if (!isLetter && !isDigit)
                {
                    capitalizeNextLetter = true;
                    continue;
                }

                result.Append(capitalizeNextLetter && isLetter
                    ? char.ToUpperInvariant(character)
                    : character);
                if (isLetter)
                    capitalizeNextLetter = false;
            }
            return result.ToString();
        }

        public static string FromPath(string path)
            => Normalize(Path.GetFileNameWithoutExtension(path));

        public static bool ValidateUnique(IEnumerable<string> paths, LogRegisterSystem logging)
        {
            Dictionary<string, string> generatedNames = new(StringComparer.Ordinal);
            bool validation = true;

            foreach (string path in paths)
            {
                string sourceName = Path.GetFileNameWithoutExtension(path);
                string generatedName = Normalize(sourceName);
                if (string.IsNullOrEmpty(generatedName))
                {
                    logging.Add(new EmptyGeneratedName(sourceName));
                    validation = false;
                    continue;
                }
                if (generatedNames.TryGetValue(generatedName, out string? previousName))
                {
                    logging.Add(new GeneratedNameCollision(previousName, sourceName, generatedName));
                    validation = false;
                    continue;
                }
                generatedNames.Add(generatedName, sourceName);
            }
            return validation;
        }
    }
}
