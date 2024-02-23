using System.Text.Json;

namespace DynamicXLibrary
{
    [Serializable]
    public class Options
    {
        public static Options Empty => new()
        {
            InputROMPath = Instance.InputROMPath,
            OutputROMPath = Instance.OutputROMPath,
            PixiPath = "",
            UberasmToolPath = "",
            GPSPath = "",
            GraphicChange = false,
            PaletteChange = false,
            PaletteEffects = false,
            DynamicPoses = false,
            DrawingSystem = false,
            ControllerOptimization = false,
            FixedColorOptimization = false,
            StatusBarOptimization = false,
            ScrollingOptimization = false,
            PlayerFeatures = false
        };
        public static Options Instance { get; private set; } = getFromFile();
        public string? InputROMPath { get; set; }
        public string? OutputROMPath { get; set; }
        public string? PixiPath { get; set; }
        public string? UberasmToolPath { get; set; }
        public string? GPSPath { get; set; }
        public bool GraphicChange { get; set; }
        public bool PaletteChange { get; set; }
        public bool PaletteEffects { get; set; }
        public bool DynamicPoses { get; set; }
        public bool DrawingSystem { get; set; }
        public bool ControllerOptimization { get; set; }
        public bool FixedColorOptimization { get; set; }
        public bool StatusBarOptimization { get; set; }
        public bool ScrollingOptimization { get; set; }
        public bool PlayerFeatures { get; set; }
        public Options()
        {

        }
        public void Save()
        {
            if(!GraphicChange)
                DynamicPoses = false;
            if(!PaletteChange) 
                PaletteEffects = false;

            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            if (File.Exists("Json/Settings.json"))
                File.Delete("Json/Settings.json");
            File.WriteAllText("Json/Settings.json", jsonString);
            Instance = this;
        }
        private static Options getFromFile()
        {
            string jsonString = File.ReadAllText("Json/Settings.json");
            return JsonSerializer.Deserialize<Options>(jsonString)!;
        }
        public static void SaveOptions()
            => Instance.Save();
        public string GenerateOptionsFileContent()
        {
            string content = $"""
                !{true} = 1
                !{false} = 0
                !GraphicChange = !{GraphicChange}
                !PaletteChange = !{PaletteChange}
                !PaletteEffects = !{PaletteEffects}
                !DynamicPoses = !{DynamicPoses}
                !DrawingSystem = !{DrawingSystem}
                !ControllerOptimization = !{ControllerOptimization}
                !FixedColorOptimization = !{FixedColorOptimization}
                !ScrollingOptimization = !{ScrollingOptimization}
                !StatusBarOptimization = !{StatusBarOptimization}
                !PlayerFeatures = !{PlayerFeatures}
                """;
            return content + File.ReadAllText(Path.Combine("ASM", "Options.asm"));
        }
    }
}
