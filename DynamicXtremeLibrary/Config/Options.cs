using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DynamicXtremeLibrary.Config
{
    /// <summary>Loads, validates, prompts for, and persists all Dynamic Xtreme configuration options.</summary>
    [Serializable]
    public class Options
    {
        /// <summary>Gets the root settings directory.</summary>
        public const string SETTINGS_DIR = "Settings";
        public readonly static string OPTIONS_DIR = Path.Combine(SETTINGS_DIR, "Options");
        public readonly static string PATHS_DIR = Path.Combine(OPTIONS_DIR, "Paths");
        public readonly static string INPUT_ROM_PATH_FILE = Path.Combine(PATHS_DIR, "InputROMPath.json");
        public readonly static string OUTPUT_ROM_PATH_FILE = Path.Combine(PATHS_DIR, "OutputROMPath.json");
        public readonly static string PIXI_PATH_FILE = Path.Combine(PATHS_DIR, "PixiPath.json");
        public readonly static string UBERASMTOOL_PATH_FILE = Path.Combine(PATHS_DIR, "UberasmToolPath.json");
        public readonly static string GPS_PATH_FILE = Path.Combine(PATHS_DIR, "GPSPath.json");
        /// <summary>Gets the settings file that defines the optional PIXI list path.</summary>
        public readonly static string PIXI_LIST_PATH_FILE = Path.Combine(PATHS_DIR, "PixiListPath.json");
        /// <summary>Gets the settings file that defines the optional UberASMTool list path.</summary>
        public readonly static string UBERASM_LIST_PATH_FILE = Path.Combine(PATHS_DIR, "UberasmListPath.json");
        public readonly static string DEPENDENCIES_FILE = Path.Combine(SETTINGS_DIR, "FeatureDependencies.json");
        public readonly static string FEATURES_LIST_FILE = Path.Combine(SETTINGS_DIR, "FeaturesList.json");
        public readonly static string SETTINGS_FILE = Path.Combine(SETTINGS_DIR, "Settings.json");
        private static Options? _instance;
        /// <summary>Gets the lazily initialized process-wide option set.</summary>
        public static Options Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new();
                return _instance;
            }
        }
        [JsonIgnore]
        /// <summary>Gets the input ROM option.</summary>
        public PathOption InputRomPath;
        [JsonIgnore]
        /// <summary>Gets the output ROM option.</summary>
        public PathOption OutputRomPath;
        [JsonIgnore]
        /// <summary>Gets the PIXI installation option.</summary>
        public DirectoryOption PixiPath;
        [JsonIgnore]
        /// <summary>Gets the UberASMTool installation option.</summary>
        public DirectoryOption UberasmPath;
        [JsonIgnore]
        /// <summary>Gets the GPS installation option.</summary>
        public DirectoryOption GPSPath;
        /// <summary>Gets the optional PIXI list whose primary ASM files are scanned for inputs.</summary>
        [JsonIgnore]
        public OptionalFileOption PixiListPath;
        /// <summary>Gets the optional UberASMTool list whose primary ASM files are scanned for inputs.</summary>
        [JsonIgnore]
        public OptionalFileOption UberasmListPath;
        [JsonIgnore]
        /// <summary>Gets all feature Boolean options.</summary>
        public BoolOption[] BoolOptions;
        [JsonIgnore]
        /// <summary>Gets feature dependency rules.</summary>
        public FeatureDependency[] Dependencies;
        [JsonIgnore]
        /// <summary>Gets feature names in configuration order.</summary>
        public string[] FeaturesList;
        /// <summary>Gets the persisted settings model.</summary>
        public Settings Settings;
        private readonly JObject _settingsDocument;
        /// <summary>Loads option definitions, dependencies, feature names, and persisted settings.</summary>
        private Options() 
        {
            InputRomPath = Option.FromFile<PathOption>(INPUT_ROM_PATH_FILE)!;
            OutputRomPath = Option.FromFile<PathOption>(OUTPUT_ROM_PATH_FILE)!;
            OutputRomPath.ExistCheck = false;
            PixiPath = Option.FromFile<DirectoryOption>(PIXI_PATH_FILE)!;
            UberasmPath = Option.FromFile<DirectoryOption>(UBERASMTOOL_PATH_FILE)!;
            GPSPath = Option.FromFile<DirectoryOption>(GPS_PATH_FILE)!;
            PixiListPath = Option.FromFile<OptionalFileOption>(PIXI_LIST_PATH_FILE)!;
            UberasmListPath = Option.FromFile<OptionalFileOption>(UBERASM_LIST_PATH_FILE)!;

            PixiPath.GetFromSettings = true;
            UberasmPath.GetFromSettings = true;
            GPSPath.GetFromSettings = true;

            Dependencies = JsonConvert.DeserializeObject<FeatureDependency[]>(File.ReadAllText(DEPENDENCIES_FILE))!;
            FeaturesList = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(FEATURES_LIST_FILE))!;

            BoolOptions = new BoolOption[FeaturesList.Length];

            string path;
            int i = 0;
            foreach (var feature in FeaturesList)
            {
                path = Path.Combine(OPTIONS_DIR, $"{feature}.json");
                BoolOptions[i] = JsonConvert.DeserializeObject<BoolOption>(File.ReadAllText(path))!;
                i++;
            }
            (_settingsDocument, Settings) = loadSettings();
            settingsToOptions();
        }
        /// <summary>Loads the settings file and validates its JSON structure before deserialization.</summary>
        /// <returns>The parsed JSON document and strongly typed settings.</returns>
        /// <exception cref="InvalidSettingsException">The file contains invalid JSON or field types.</exception>
        private (JObject Document, Settings Settings) loadSettings()
        {
            try
            {
                JToken root = JToken.Parse(File.ReadAllText(SETTINGS_FILE));
                if (root is not JObject document)
                    throw invalidField(root, "$", "the root value must be a JSON object");

                validateStringField(document, nameof(Settings.InputROMPath));
                validateStringField(document, nameof(Settings.OutputROMPath));
                validateStringField(document, nameof(Settings.PixiPath));
                validateStringField(document, nameof(Settings.UberasmToolPath));
                validateStringField(document, nameof(Settings.GPSPath));
                validateStringField(document, nameof(Settings.PixiListPath), optional: true);
                validateStringField(document, nameof(Settings.UberasmListPath), optional: true);
                validateFeatures(document);

                Settings? settings = document.ToObject<Settings>();
                return settings == null
                    ? throw invalidField(document, "$", "the root object could not be read")
                    : (document, settings);
            }
            catch (InvalidSettingsException)
            {
                throw;
            }
            catch (JsonReaderException exception)
            {
                throw new InvalidSettingsException(
                    $"'{SETTINGS_FILE}' is invalid at line {exception.LineNumber}, " +
                    $"position {exception.LinePosition}: {jsonReaderProblem(exception)}", exception);
            }
            catch (JsonSerializationException exception)
            {
                throw new InvalidSettingsException(
                    $"'{SETTINGS_FILE}' is invalid: {exception.Message}", exception);
            }
        }
        /// <summary>Removes Newtonsoft's repeated path and line suffix from a JSON parser message.</summary>
        /// <param name="exception">The parser error whose concise description is required.</param>
        /// <returns>The parser description without duplicated source-location metadata.</returns>
        private static string jsonReaderProblem(JsonReaderException exception)
        {
            int pathSuffix = exception.Message.LastIndexOf(" Path '", StringComparison.Ordinal);
            return pathSuffix < 0
                ? exception.Message
                : exception.Message[..pathSuffix].TrimEnd();
        }
        /// <summary>Requires a settings property to contain a JSON string or an allowed null value.</summary>
        /// <param name="document">The settings root object.</param>
        /// <param name="name">The property to validate.</param>
        /// <param name="optional">Whether a JSON null value is accepted.</param>
        private static void validateStringField(JObject document, string name, bool optional = false)
        {
            JToken? token = document[name];
            if (token == null)
                throw invalidField(document, name, "the field is required");
            if (token.Type == JTokenType.String || optional && token.Type == JTokenType.Null)
                return;
            throw invalidField(token, name, optional
                ? "the value must be a string or null"
                : "the value must be a string");
        }
        /// <summary>Validates feature names and requires every Enable value to be a JSON Boolean.</summary>
        /// <param name="document">The settings root object.</param>
        private void validateFeatures(JObject document)
        {
            JToken? token = document[nameof(Settings.Features)];
            if (token is not JArray features)
                throw invalidField(token ?? document, nameof(Settings.Features),
                    "the value must be an array");

            HashSet<string> names = new(StringComparer.Ordinal);
            for (int index = 0; index < features.Count; index++)
            {
                string path = $"Features[{index}]";
                if (features[index] is not JObject feature)
                    throw invalidField(features[index], path, "the value must be an object");

                JToken? nameToken = feature[nameof(FeatureEnable.Name)];
                if (nameToken?.Type != JTokenType.String ||
                    string.IsNullOrWhiteSpace(nameToken.Value<string>()))
                    throw invalidField(nameToken ?? feature, $"{path}.Name",
                        "the value must be a non-empty string");

                string name = nameToken.Value<string>()!;
                if (!FeaturesList.Contains(name, StringComparer.Ordinal))
                    throw invalidField(nameToken, $"{path}.Name",
                        $"'{name}' is not a recognized feature");
                if (!names.Add(name))
                    throw invalidField(nameToken, $"{path}.Name",
                        $"feature '{name}' is duplicated");

                JToken? enableToken = feature[nameof(FeatureEnable.Enable)];
                if (enableToken?.Type != JTokenType.Boolean)
                    throw invalidField(enableToken ?? feature, $"{path}.Enable",
                        "the value must be true or false");
            }
        }
        /// <summary>Creates a settings exception containing the field path and JSON source location.</summary>
        /// <param name="token">The closest token carrying source line information.</param>
        /// <param name="path">The logical settings field path.</param>
        /// <param name="problem">The validation failure.</param>
        /// <returns>The formatted settings exception.</returns>
        private static InvalidSettingsException invalidField(JToken token, string path, string problem)
        {
            IJsonLineInfo lineInfo = token;
            string location = lineInfo.HasLineInfo()
                ? $"line {lineInfo.LineNumber}, position {lineInfo.LinePosition}"
                : "unknown line";
            return new InvalidSettingsException(
                $"'{SETTINGS_FILE}' is invalid at {location}. Field '{path}': {problem}.");
        }
        /// <summary>Copies persisted settings into the interactive option models.</summary>
        private void settingsToOptions()
        {
            InputRomPath.Value = Settings.InputROMPath;
            OutputRomPath.Value = Settings.OutputROMPath;
            PixiPath.Value = Settings.PixiPath;
            UberasmPath.Value = Settings.UberasmToolPath;
            GPSPath.Value = Settings.GPSPath;
            PixiListPath.Value = Settings.PixiListPath ?? "";
            UberasmListPath.Value = Settings.UberasmListPath ?? "";
            updateListBaseDirectories();
            foreach(var feat in Settings.Features)
            {
                BoolOptions.First(bo => bo.Name == feat.Name).Value = feat.Enable;
            }
        }
        /// <summary>Collects detailed path failures for values loaded from the settings file.</summary>
        /// <returns>User-facing path errors annotated with their JSON source lines.</returns>
        private IReadOnlyList<string> validateStoredPaths()
        {
            List<string> errors = [];
            validateRomPath(nameof(Settings.InputROMPath), Settings.InputROMPath,
                requireFile: true, errors);
            validateRomPath(nameof(Settings.OutputROMPath), Settings.OutputROMPath,
                requireFile: false, errors);
            validateDirectoryPath(nameof(Settings.PixiPath), Settings.PixiPath, errors);
            validateDirectoryPath(nameof(Settings.UberasmToolPath), Settings.UberasmToolPath, errors);
            validateDirectoryPath(nameof(Settings.GPSPath), Settings.GPSPath, errors);
            validateOptionalFilePath(nameof(Settings.PixiListPath), Settings.PixiListPath,
                PixiListPath, errors);
            validateOptionalFilePath(nameof(Settings.UberasmListPath), Settings.UberasmListPath,
                UberasmListPath, errors);
            return errors;
        }
        /// <summary>Validates one stored ROM path and records its precise failure reason.</summary>
        /// <param name="field">The settings property containing the path.</param>
        /// <param name="value">The configured ROM path.</param>
        /// <param name="requireFile">Whether the ROM itself must already exist.</param>
        /// <param name="errors">The collection that receives validation failures.</param>
        private void validateRomPath(string field, string value, bool requireFile,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(pathError(field, "the path is empty"));
                return;
            }
            if (!string.Equals(Path.GetExtension(value), ".smc", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(pathError(field, "the path must point to an .smc file"));
                return;
            }

            string? directory = Path.GetDirectoryName(value);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                if (File.Exists(directory))
                {
                    errors.Add(pathError(field,
                        $"the parent path '{directory}' is a file, not a directory"));
                    return;
                }
                if (requireFile && !Directory.Exists(directory))
                {
                    errors.Add(pathError(field, $"the directory '{directory}' does not exist"));
                    return;
                }
            }
            if (requireFile && !File.Exists(value))
                errors.Add(pathError(field, $"the file '{value}' does not exist or cannot be accessed"));
        }
        /// <summary>Validates one optional external-tool directory stored in settings.</summary>
        /// <param name="field">The settings property containing the directory.</param>
        /// <param name="value">The configured directory.</param>
        /// <param name="errors">The collection that receives validation failures.</param>
        private void validateDirectoryPath(string field, string value, ICollection<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(value) && !Directory.Exists(value))
                errors.Add(pathError(field, $"the directory '{value}' does not exist or cannot be accessed"));
        }
        /// <summary>Validates one optional list file stored in settings.</summary>
        /// <param name="field">The settings property containing the file path.</param>
        /// <param name="value">The configured optional file.</param>
        /// <param name="option">The option that resolves the path against its external tool.</param>
        /// <param name="errors">The collection that receives validation failures.</param>
        private void validateOptionalFilePath(string field, string? value, OptionalFileOption option,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            string resolved = option.ResolvePath(value);
            if (!File.Exists(resolved))
                errors.Add(pathError(field,
                    $"'{value}' resolves to '{resolved}' relative to its tool directory, " +
                    "but that file does not exist or cannot be accessed"));
        }
        /// <summary>Assigns each optional list path to its corresponding external-tool directory.</summary>
        private void updateListBaseDirectories()
        {
            PixiListPath.RelativeToDirectory = PixiPath.Value;
            UberasmListPath.RelativeToDirectory = UberasmPath.Value;
        }
        /// <summary>Formats a path validation failure with the source line of its settings property.</summary>
        /// <param name="field">The invalid settings property.</param>
        /// <param name="problem">The path-specific failure.</param>
        /// <returns>The formatted validation message.</returns>
        private string pathError(string field, string problem)
        {
            JToken token = _settingsDocument[field] ?? _settingsDocument;
            IJsonLineInfo lineInfo = token;
            string location = lineInfo.HasLineInfo()
                ? $"Line {lineInfo.LineNumber}, position {lineInfo.LinePosition}"
                : "Unknown line";
            return $"{location}, field '{field}': {problem}.";
        }
        /// <summary>
        /// Prompts only for invalid path values loaded from settings while preserving every valid
        /// stored value. Optional options are cleared before prompting so empty input can disable
        /// them instead of selecting the invalid stored path again.
        /// </summary>
        /// <returns>Whether all invalid stored paths were replaced or intentionally left empty.</returns>
        private bool obtainInvalidStoredPaths()
        {
            if (!InputRomPath.Validate(InputRomPath.Value) &&
                !obtainReplacement(InputRomPath))
                return false;
            if (!OutputRomPath.Validate(OutputRomPath.Value) &&
                !obtainReplacement(OutputRomPath))
                return false;

            if (!PixiPath.Validate(PixiPath.Value))
            {
                PixiPath.GetFromSettings = false;
                if (!obtainReplacement(PixiPath))
                    return false;
            }
            if (!UberasmPath.Validate(UberasmPath.Value))
            {
                UberasmPath.GetFromSettings = false;
                if (!obtainReplacement(UberasmPath))
                    return false;
            }
            if (!GPSPath.Validate(GPSPath.Value))
            {
                GPSPath.GetFromSettings = false;
                if (!obtainReplacement(GPSPath))
                    return false;
            }

            updateListBaseDirectories();
            if (!PixiListPath.Validate(PixiListPath.Value) &&
                !obtainReplacement(PixiListPath))
                return false;
            if (!UberasmListPath.Validate(UberasmListPath.Value) &&
                !obtainReplacement(UberasmListPath))
                return false;
            return true;
        }
        /// <summary>Clears an invalid stored path and asks the user for its replacement.</summary>
        /// <param name="option">The path option whose stored value failed validation.</param>
        /// <returns>Whether the user supplied or selected a valid replacement.</returns>
        private static bool obtainReplacement(Option<string> option)
        {
            option.Value = "";
            return option.ObtainValue();
        }
        /// <summary>Validates stored settings or interactively obtains replacements, then updates the settings model.</summary>
        /// <param name="useSettings">Whether valid persisted values should be accepted without prompting.</param>
        /// <returns>Whether every required option was obtained successfully.</returns>
        public bool SettingsForm(bool useSettings)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            if (useSettings)
            {
                IReadOnlyList<string> pathErrors = validateStoredPaths();
                if (pathErrors.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"'{SETTINGS_FILE}' is invalid:");
                    foreach (string error in pathErrors)
                        Console.WriteLine($"  {error}");
                    Console.ResetColor();
                }
                if (!obtainInvalidStoredPaths())
                    return false;
                foreach (var option in BoolOptions)
                {
                    if (!option.Validate($"{option.Value}"))
                        if (!option.ObtainValue())
                            return false;
                }
            }
            if (!useSettings)
            {
                PixiPath.GetFromSettings = false;
                UberasmPath.GetFromSettings = false;
                GPSPath.GetFromSettings = false;
                if (!InputRomPath.ObtainValue()) return false;
                if (!OutputRomPath.ObtainValue()) return false;
                if (!PixiPath.ObtainValue()) return false;
                if (!UberasmPath.ObtainValue()) return false;
                if (!GPSPath.ObtainValue()) return false;
                updateListBaseDirectories();
                if (!PixiListPath.ObtainValue()) return false;
                if (!UberasmListPath.ObtainValue()) return false;
                bool dependencyFulfilled;
                foreach (var option in BoolOptions)
                {
                    dependencyFulfilled = true;
                    foreach (var dependency in Dependencies)
                    {
                        if (!FeatureDependency.CheckDependency(option.Name, dependency, BoolOptions))
                            dependencyFulfilled = false;
                    }
                    if (dependencyFulfilled)
                        if (!option.ObtainValue()) return false;
                }
            }
            Settings.InputROMPath = InputRomPath.Value;
            Settings.OutputROMPath = OutputRomPath.Value;
            Settings.PixiPath = PixiPath.Value;
            Settings.UberasmToolPath = UberasmPath.Value;
            Settings.GPSPath = GPSPath.Value;
            Settings.PixiListPath = PixiListPath.Value;
            Settings.UberasmListPath = UberasmListPath.Value;
            Dictionary<string, bool> features =
                BoolOptions.ToDictionary(option => option.Name, option => option.Value);
            foreach (var feature in Settings.Features)
            {
                if (features.TryGetValue(feature.Name, out bool value))
                    feature.Enable = value;
            }
            return true;
        }
        /// <summary>Serializes every option definition and the current settings to disk.</summary>
        public void Save()
        {
            
            string jsonString = JsonConvert.SerializeObject(InputRomPath, Formatting.Indented);
            File.WriteAllText(INPUT_ROM_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(OutputRomPath, Formatting.Indented);
            File.WriteAllText(OUTPUT_ROM_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(PixiPath, Formatting.Indented);
            File.WriteAllText(PIXI_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(UberasmPath, Formatting.Indented);
            File.WriteAllText(UBERASMTOOL_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(GPSPath, Formatting.Indented);
            File.WriteAllText(GPS_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(PixiListPath, Formatting.Indented);
            File.WriteAllText(PIXI_LIST_PATH_FILE, jsonString);
            jsonString = JsonConvert.SerializeObject(UberasmListPath, Formatting.Indented);
            File.WriteAllText(UBERASM_LIST_PATH_FILE, jsonString);
            foreach (var option in BoolOptions)
            {
                string path = Path.Combine(OPTIONS_DIR, $"{option.Name}.json");
                jsonString = JsonConvert.SerializeObject(option, Formatting.Indented);
                File.WriteAllText(path, jsonString);
            }
            jsonString = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(SETTINGS_FILE, jsonString);
        }
        /// <summary>Builds the Asar Boolean defines and dependency overrides for enabled features.</summary>
        /// <returns>The generated options-define source.</returns>
        public string GetOptionsDefines()
        {
            StringBuilder sb = new();
            sb.AppendLine("!True = 1");
            sb.AppendLine("!False = 0\n");
            foreach (var option in BoolOptions)
            {
                sb.AppendLine($"!{option.Name} = {(option.Value ? "!True" : "!False")}");
            }
            sb.AppendLine();
            foreach (var dep in Dependencies)
            {
                sb.AppendLine($"if !{dep.Parent} == !False");
                foreach (var ch in dep.Children)
                {
                    sb.AppendLine($"\t!{ch} = !False");
                }
                sb.AppendLine("endif");
            }
            return sb.ToString();
        }
    }
}
