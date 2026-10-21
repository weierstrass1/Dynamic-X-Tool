using Newtonsoft.Json;
using System.Text;

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
            Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SETTINGS_FILE))!;
            settingsToOptions();
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
            foreach(var feat in Settings.Features)
            {
                BoolOptions.First(bo => bo.Name == feat.Name).Value = feat.Enable;
            }
        }
        /// <summary>Validates stored settings or interactively obtains replacements, then updates the settings model.</summary>
        /// <param name="useSettings">Whether valid persisted values should be accepted without prompting.</param>
        /// <returns>Whether every required option was obtained successfully.</returns>
        public bool SettingsForm(bool useSettings)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            if (useSettings)
            {
                useSettings = InputRomPath.Validate(InputRomPath.Value) &&
                    OutputRomPath.Validate(OutputRomPath.Value) &&
                    PixiPath.Validate(PixiPath.Value) &&
                    UberasmPath.Validate(UberasmPath.Value) &&
                    GPSPath.Validate(GPSPath.Value) &&
                    PixiListPath.Validate(PixiListPath.Value) &&
                    UberasmListPath.Validate(UberasmListPath.Value);
                foreach (var option in BoolOptions)
                {
                    if (!option.Validate($"{option.Value}"))
                        useSettings = false;
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
