using Newtonsoft.Json;
using System.Text;

namespace DynamicXtremeLibrary.Config
{
    [Serializable]
    public class Options
    {
        public const string SETTINGS_DIR = "Settings";
        public readonly static string OPTIONS_DIR = Path.Combine(SETTINGS_DIR, "Options");
        public readonly static string PATHS_DIR = Path.Combine(OPTIONS_DIR, "Paths");
        public readonly static string INPUT_ROM_PATH_FILE = Path.Combine(PATHS_DIR, "InputROMPath.json");
        public readonly static string OUTPUT_ROM_PATH_FILE = Path.Combine(PATHS_DIR, "OutputROMPath.json");
        public readonly static string PIXI_PATH_FILE = Path.Combine(PATHS_DIR, "PixiPath.json");
        public readonly static string UBERASMTOOL_PATH_FILE = Path.Combine(PATHS_DIR, "UberasmToolPath.json");
        public readonly static string GPS_PATH_FILE = Path.Combine(PATHS_DIR, "GPSPath.json");
        public readonly static string DEPENDENCIES_FILE = Path.Combine(SETTINGS_DIR, "FeatureDependencies.json");
        public readonly static string FEATURES_LIST_FILE = Path.Combine(SETTINGS_DIR, "FeaturesList.json");
        public readonly static string SETTINGS_FILE = Path.Combine(SETTINGS_DIR, "Settings.json");
        private static Options? _instance;
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
        public PathOption InputRomPath;
        [JsonIgnore]
        public PathOption OutputRomPath;
        [JsonIgnore]
        public DirectoryOption PixiPath;
        [JsonIgnore]
        public DirectoryOption UberasmPath;
        [JsonIgnore]
        public DirectoryOption GPSPath;
        [JsonIgnore]
        public BoolOption[] BoolOptions;
        [JsonIgnore]
        public FeatureDependency[] Dependencies;
        [JsonIgnore]
        public string[] FeaturesList;
        public Settings Settings;
        private Options() 
        {
            InputRomPath = Option.FromFile<PathOption>(INPUT_ROM_PATH_FILE)!;
            OutputRomPath = Option.FromFile<PathOption>(OUTPUT_ROM_PATH_FILE)!;
            OutputRomPath.ExistCheck = false;
            PixiPath = Option.FromFile<DirectoryOption>(PIXI_PATH_FILE)!;
            UberasmPath = Option.FromFile<DirectoryOption>(UBERASMTOOL_PATH_FILE)!;
            GPSPath = Option.FromFile<DirectoryOption>(GPS_PATH_FILE)!;

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
        private void settingsToOptions()
        {
            InputRomPath.Value = Settings.InputROMPath;
            OutputRomPath.Value = Settings.OutputROMPath;
            PixiPath.Value = Settings.PixiPath;
            UberasmPath.Value = Settings.UberasmToolPath;
            GPSPath.Value = Settings.GPSPath;
            foreach(var feat in Settings.Features)
            {
                BoolOptions.First(bo => bo.Name == feat.Name).Value = feat.Enable;
            }
        }
        public void SettingsForm(bool useSettings)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            if (useSettings)
            {
                useSettings = InputRomPath.Validate(InputRomPath.Value) &&
                    OutputRomPath.Validate(InputRomPath.Value) &&
                    PixiPath.Validate(InputRomPath.Value) &&
                    UberasmPath.Validate(InputRomPath.Value) &&
                    GPSPath.Validate(InputRomPath.Value);
                foreach (var option in BoolOptions)
                {
                    if (!option.Validate($"{option.Value}"))
                        useSettings = false;
                }
            }
            if (!useSettings)
            {
                InputRomPath.ObtainValue();
                OutputRomPath.ObtainValue();
                PixiPath.ObtainValue();
                UberasmPath.ObtainValue();
                GPSPath.ObtainValue();
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
                        option.ObtainValue();
                }
            }
            Settings.InputROMPath = InputRomPath.Value;
            Settings.OutputROMPath = OutputRomPath.Value;
            Settings.PixiPath = PixiPath.Value;
            Settings.UberasmToolPath = UberasmPath.Value;
            Settings.GPSPath = GPSPath.Value;
            Dictionary<string, bool> features =
                BoolOptions.ToDictionary(option => option.Name, option => option.Value);
            foreach (var feature in Settings.Features)
            {
                if (features.TryGetValue(feature.Name, out bool value))
                    feature.Enable = value;
            }
        }
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
            foreach (var option in BoolOptions)
            {
                string path = Path.Combine(OPTIONS_DIR, $"{option.Name}.json");
                jsonString = JsonConvert.SerializeObject(option, Formatting.Indented);
                File.WriteAllText(path, jsonString);
            }
            jsonString = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(SETTINGS_FILE, jsonString);
        }
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
