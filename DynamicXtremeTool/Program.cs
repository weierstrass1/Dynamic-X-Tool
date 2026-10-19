using DynamicXtremeLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremeLibrary.Logging.Wrappers;
using LogRegister;
using System.Reflection;
using System.Reflection.Metadata;

namespace DynamicXtremeTool
{
    public class Program
    {
        private static void Main(string[]? args)
        {
            AppDomain.CurrentDomain.ProcessExit += (_, __) => Console.ResetColor();

            bool useSettings = false;
            bool verbose = false;
           
            if(args != null && args.Length > 0)
            {
                var lowerArgs = args.Select(a => a.ToLower().Trim());
                if(lowerArgs.Count() == 1)
                {
                    string command = lowerArgs.First();
                    string message = "";
                    if (command == "-version")
                    {
                        string stringversion = Assembly.GetExecutingAssembly()
                            .GetName()
                            .Version?
                            .ToString() ?? "Unknown";
                        message = $"Current version: {stringversion}";
                    }
                    else if(command == "-help")
                    {
                        message = """
                            Usage:
                            dxt [options]

                            Options:
                            -help
                            Show this help message.

                            -version
                            Show the current application version.

                            -use-settings, -us
                            Use the settings defined in the Settings file.

                            -verbose, -v
                            Enable verbose logging and display detailed execution information.
                            """;
                    }
                    if (command == "-version" || command == "-help")
                    {
                        Console.WriteLine(message);
                        Console.ResetColor();
                        Console.Read();
                        return;
                    }
                }
                useSettings = lowerArgs.Any(a => a == "-use-settings" || a == "-us");
                verbose = lowerArgs.Any(a => a == "-v" || a == "-verbose");
            }
            var opt = Options.Instance;
            if(!opt.SettingsForm(useSettings))
            {
                Console.WriteLine("The process was interrupted.");
                Console.ResetColor();
                Console.Read();
                return;
            }
            opt.Save();
            LogRegisterSystem logRegisterSystem = new();

            DynamicXtreme dx = new(logRegisterSystem)
            {
                TemplateASMDirectory = "ASM",
                TMPDirectory = "TMP",
                DynamicInfoDirectory = "DynamicInfos",
                DynamicResourcesDirectory = "DynamicResources",
                DrawInfoDirectory = "DrawInfos",
                PaletteEffectsDirectory = "PaletteEffects",
                PatchDirectory = "Patch",
                DataDirectory = "Data",
                ExtraDefinesDirectory = "ExtraDefines",
                PoseDataTemplateFilename = "PoseDataTemplate.asm",
                GraphicRoutinesDirectory = "GraphicRoutines",
                GraphicRoutineTemplateFilename = "GraphicRoutineTemplate",
                GraphicRoutineIncludeTemplateFilename = "GraphicRoutineIncludeTemplate",
                InputDefinesFilename = "DXDefines",
                BufferDataFilename = "BufferData",
                DynamicPoseDataFilename = "DynamicPoseData",
                PaletteDataFilename = "PaletteData",
                PoseDataFilename = "PoseData",
                GraphicRoutineIncludeFilename = "GraphicRoutineIncludes",
                GraphicRoutineProtsFilename = "GraphicRoutineProts",
                PaletteEffectsDataFilename = "PaletteEffectsData",
                OutputDefinesFilename = "DXDefines",
                OptionsDefinesFilename = "Options",
                RoutinesDirectory = "Routines"
            };
            dx.Run(File.ReadAllBytes(opt.InputRomPath.Value));

            LogRenderer renderer = new(Path.Combine("Logging", "LogMessages.json"));
            RawTextWrapper rawText = new();
            MultiWrapper mw = new();
            mw.Actions += ConsoleWrapper.RenderAction;
            mw.Actions += rawText.RenderAction;
            bool hasErrors = logRegisterSystem.HasLogsOfType<Error>();
            renderer.RenderAll(logRegisterSystem.GetRegisters(), mw.RenderAction, error: hasErrors, verbose: verbose);
            File.WriteAllText("log.txt", rawText.ToString());
            Console.ResetColor();
            Console.Read();
        }
    }
}