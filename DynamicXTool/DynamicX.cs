using DynamicXLibrary;

namespace DynamicXTool
{
    public class DynamicX
    {
        public static void Run(string[]? args)
        {
            settings(args);
            DynamicXProcess dxp = new(Options.Instance.InputROMPath!);
            dxp.Process(Options.Instance.InputROMPath!, Options.Instance.OutputROMPath!);
            Console.WriteLine("Inserted Successfully. Please reinsert all tools and resources that depends on Dynamic X.");
            Console.ReadLine();
        }
        private static bool settings(string[]? args)
        {
            if (args != null && args.Length > 0 && args[0].ToLower().Trim() == "-use-settings" &&
                Options.Instance.InputROMPath != null && Options.Instance.InputROMPath != "" && File.Exists(Options.Instance.InputROMPath) &&
                Options.Instance.OutputROMPath != null && Options.Instance.OutputROMPath != "" && File.Exists(Options.Instance.OutputROMPath) &&
                Path.GetExtension(Options.Instance.InputROMPath) == ".smc" && Path.GetExtension(Options.Instance.OutputROMPath) == ".smc")
                return true;
            if (!setPath(out string? rompath, "Enter ROM Path (or press Enter to use path in Settings.json)",
            args, 0, Options.Instance.InputROMPath))
                return false;

            Options.Instance.InputROMPath = rompath;
            string? outputOption = Options.Instance.OutputROMPath != null && Options.Instance.OutputROMPath != "" ?
                Options.Instance.OutputROMPath :
                rompath;

            if (!setPath(out string? outpath, "Enter Output Path (or press Enter to use path in Settings.json)",
                        args, 1, outputOption, false)) 
                return false;

            Options.Instance.OutputROMPath = outpath;

            Options.Instance.PixiPath = setDirectory("Enter Pixi Directory Path (or press Enter to use path in Settings.json)", Options.Instance.PixiPath);

            Options.Instance.UberasmToolPath = setDirectory("Enter Uberasm Tool Directory Path (or press Enter to use path in Settings.json)", Options.Instance.UberasmToolPath);

            Options.Instance.GPSPath = setDirectory("Enter GPS Directory Path (or press Enter to use path in Settings.json)", Options.Instance.GPSPath);

            Options.Instance.GraphicChange = setFeatureOption(Options.Instance.GraphicChange,
                """
                Do you want to activate Graphic Change Feature?
                This allows to change graphics dynamically on the fly,
                also is a requirement for Dynamic Poses.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            Options.Instance.PaletteChange = setFeatureOption(Options.Instance.PaletteChange,
                """
                Do you want to activate Palette Change Feature?
                This allows to change palettes dynamically on the fly,
                also is a requirement for Palettes Effects.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            if (Options.Instance.GraphicChange)
                Options.Instance.DynamicPoses = setFeatureOption(Options.Instance.DynamicPoses,
                    """
                    Do you want to activate Dynamic Poses Feature?
                    This allows to load poses dynamically for dynamic sprites
                    poses are shared between all resources that uses it
                    also can be uploaded from any asm code.
                    Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                    """);

            if (Options.Instance.PaletteChange)
                Options.Instance.PaletteEffects = setFeatureOption(Options.Instance.PaletteEffects,
                    """
                    Do you want to activate Palette Effects Feature?
                    This allows to apply RGB or HSL effects on colors
                    It allows to mix each individual channel based on RGB or HSL palette 
                    color with the channels of a base color, using different weights for each 
                    channel.
                    Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                    """);

            Options.Instance.DrawingSystem = setFeatureOption(Options.Instance.DrawingSystem,
                """
                Do you want to activate Drawing System Feature?
                This allows to draw poses on any place of the screen,
                it can be used from any asm code such sprites, blocks, uberasm or external patches.
                Also allows to draw those poses with any object clipping, flipping it horizontally
                or vertically, any palette and also if you use SA-1 you can draw it with any
                max tile priority.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            Options.Instance.ControllerOptimization = setFeatureOption(Options.Instance.ControllerOptimization,
                """
                Do you want to activate Controller Optimization Feature?
                This moves controller out of NMI Handler, saving some cycles
                during V-Blank and helps to decrease flickering.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            Options.Instance.FixedColorOptimization = setFeatureOption(Options.Instance.FixedColorOptimization,
                """
                Do you want to activate Fixed Color Optimization Feature?
                This optimize Fixed Color Routine savind some cycles
                during V-Blank and helps to decrease flickering.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            Options.Instance.ScrollingOptimization = setFeatureOption(Options.Instance.ScrollingOptimization,
                """
                Do you want to activate Scrolling Optimization Feature?
                This optimize Scrolling Routine savind some cycles
                during V-Blank and helps to decrease flickering.
                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);

            Options.Instance.StatusBarOptimization = setFeatureOption(Options.Instance.StatusBarOptimization,
                """
                Do you want to activate Status Bar Optimization Feature?
                This optimize Status Bar Routine savind some cycles
                during V-Blank and helps to decrease flickering.

                WARNING: THIS CAN BE INCOMPATIBLE WITH SOME STATUS BAR PATCHES

                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);


            Options.Instance.PlayerFeatures = setFeatureOption(Options.Instance.PlayerFeatures,
                """
                Do you want to activate Player Feature?
                This allows to change graphic and palette of player,
                there are 2 ways:
                1) Using the same graphic layout of GFX32, this is limited to 
                the number of poses of the vanilla game.
                2) Do your own dynamic and graphic routine, disable Player NMI Routine
                You still can use vanilla player if you want.
                Also this optimize DMA for player, yoshi and podoboos making that
                they only upload graphics when it is necessary, this also saves a couple of
                16x16 tiles in SP1 if yoshi and podoboos aren't in the level.

                WARNING: THIS CAN BE INCOMPATIBLE WITH SOME PLAYER PATCHES SUCH PLAYER 32X32,
                PLAYER 8X8 DMAER OR LX5'S POWERUPS.

                Press Yes or Y to Activate, No or N to deactivate or skip to use option in Settings.json)
                """);
            Options.Instance.Save();
            return true;
        }
        private static bool setPath(out string? rompath, string  message, string[]? args, int arg, string? option, bool existCheck = true)
        {
            rompath = null;
            if (args != null && args.Length > arg)
                rompath = args[arg];
            if (rompath == null || rompath == "" || (existCheck && !File.Exists(rompath)))
            {
                Console.WriteLine(message);
                rompath = Console.ReadLine()?.Replace("\"", "");
            }
            if (rompath == null || rompath == "" || (existCheck && !File.Exists(rompath)))
                rompath = option;
            if (rompath != null && rompath != "" && (!existCheck || File.Exists(rompath)))
                return true;
            Console.WriteLine("Error: Path is Empty. Try again writting a path or changing Settings.json.");
            Console.ReadLine();
            return false;
        }
        private static string? setDirectory(string message, string? option)
        {
            Console.WriteLine(message);
            string? directory = Console.ReadLine()?.Replace("\"", "");
            if (directory == null || !Directory.Exists(directory))
                directory = option;
            return directory;
        }
        private static bool setFeatureOption(bool option, string message)
        {
            Console.WriteLine(message);
            string? featOpt = Console.ReadLine()?.Replace("\"", "");
            if (featOpt != null)
                featOpt = featOpt.ToLower().Trim();
            return featOpt switch
            {
                "true" or "t" or "yes" or "y" or "s" or "si" => true,
                "false" or "f" or "no" or "n" => false,
                _ => option
            };
        }
    }
}
