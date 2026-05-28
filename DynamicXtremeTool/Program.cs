using DynamicXtremeLibrary;
using DynamicXtremeLibrary.Config;
using DynamicXtremeLibrary.Logging.Wrappers;
using LogRegister;

namespace DynamicXtremeTool
{
    public class Program
    {
        private static void Main(string[]? args)
        {
            AppDomain.CurrentDomain.ProcessExit += (_, __) => Console.ResetColor();

            var opt = Options.Instance;

            if(!opt.SettingsForm(args != null && args.Length > 0 && 
                args[0].ToLower().Trim() == "-use-settings"))
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
            renderer.RenderAll(logRegisterSystem.GetRegisters(), mw.RenderAction);
            File.WriteAllText("log.txt", rawText.ToString());
            Console.ResetColor();
            Console.Read();
        }
    }
}