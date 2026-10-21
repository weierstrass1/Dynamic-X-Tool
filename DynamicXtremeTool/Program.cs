using DynamicXtremeLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Logging.Categories;
using DynamicXtremeLibrary.Logging.Wrappers;
using LogRegister;
using System.Reflection;
using System.Reflection.Metadata;
using DynamicXtremeLibrary.Inputs;

namespace DynamicXtremeTool
{
    /// <summary>Provides the Dynamic Xtreme command-line entry point and interactive setup flow.</summary>
    public class Program
    {
        /// <summary>Processes command-line options, runs installation, and renders the resulting log.</summary>
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
                RoutinesDirectory = "Routines",
                CollisionResolver = ConfirmCollisionRename
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

        /// <summary>
        /// Displays a generated-name collision and asks the user whether Dynamic Xtreme may rename
        /// the affected symbols and update their primary ASM consumers.
        /// </summary>
        /// <param name="collision">The conflicting files and proposed collision-free names.</param>
        /// <returns><see langword="true"/> only when the user explicitly accepts the rename.</returns>
        private static bool ConfirmCollisionRename(NameCollision collision)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine($"Dynamic Xtreme found different inputs that generate '{collision.GeneratedName}'.");
            Console.WriteLine("Proposed renames:");
            foreach (NameCollisionCandidate candidate in collision.Candidates)
            {
                Console.WriteLine($"  {candidate.OriginalName} -> {candidate.ProposedName}");
                foreach (string file in candidate.Files)
                    Console.WriteLine($"    {file}");
            }
            Console.Write("Rename the generated symbols and update the affected primary ASM files? [y/N]: ");
            Console.ResetColor();
            string answer = Console.ReadLine()?.Trim() ?? "";
            return answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                answer.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}
