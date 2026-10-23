using DynamicXtremeLibrary.Logging.LoggingRegisters;
using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.Defines
{
    /// <summary>Normalizes filenames into stable alphanumeric Asar symbol components.</summary>
    public static class AsarSymbolName
    {
        /// <summary>Removes separators and invalid characters while applying Pascal-style word starts.</summary>
        /// <param name="name">The source name to normalize.</param>
        /// <returns>An alphanumeric component that preserves existing letter casing.</returns>
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

        /// <summary>Normalizes the filename without its extension.</summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The generated Asar symbol component.</returns>
        public static string FromPath(string path)
            => Normalize(Path.GetFileNameWithoutExtension(InputPath.Normalize(path)));

        /// <summary>Checks that every path generates a non-empty and unique normalized name.</summary>
        /// <param name="paths">The paths whose filenames are normalized.</param>
        /// <param name="logging">The log that receives empty-name and collision errors.</param>
        /// <returns>Whether every generated name is valid and unique.</returns>
        public static bool ValidateUnique(IEnumerable<string> paths, LogRegisterSystem logging)
        {
            Dictionary<string, string> generatedNames = new(StringComparer.Ordinal);
            bool validation = true;

            foreach (string path in paths)
            {
                string sourceName = Path.GetFileNameWithoutExtension(InputPath.Normalize(path));
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
