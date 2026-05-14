using DynamicXtremeLibrary.Config;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.ExternalTools
{
    public class ExternalToolsLinker
    {
        public const string PIXI_ASM_DIR = "asm";
        public const string PIXI_ROUTINES_DIR = "routines";
        public const string PIXI_SPRITE_DIR = "sprites";
        public const string PIXI_CLUSTER_DIR = "cluster";
        public const string PIXI_EXTENDED_DIR = "extended";
        public const string SPRITE_DEFINES = "NormalSpriteDefines.asm";
        public const string CLUSTER_DEFINES = "ClusterSpriteDefines.asm";
        public const string EXTENDED_DEFINES = "ExtendedSpriteDefines.asm";
        public const string UBERASM_OTHER_DIR = "other"; 
        public const string UBERASMTOOL_DEFINES = "macro_library.asm";
        public const string GPS_DEFINES = "defines.asm";

        public string ASMDirectory { get; set; }
        public string ExtraDefinesDirectory { get; private set; }
        public string RoutinesDirectory { get; private set; }
        public ExternalToolsLinker(string asmDirectory, string extraDefinesDirectory, string routinesDirectory)
        {
            ASMDirectory = asmDirectory;
            ExtraDefinesDirectory = extraDefinesDirectory;
            RoutinesDirectory = routinesDirectory;
        }
        public void PixiLinker()
        {
            if (string.IsNullOrWhiteSpace(Options.Instance.PixiPath.Value))
                return;
            Link(Path.Combine(Options.Instance.PixiPath.Value, PIXI_ASM_DIR));
            copyDirectory(RoutinesDirectory, 
                Path.Combine(Options.Instance.PixiPath.Value, PIXI_ROUTINES_DIR), true);
            string pixiSprites = Path.Combine(Options.Instance.PixiPath.Value, PIXI_SPRITE_DIR);
            string pixiClusters = Path.Combine(Options.Instance.PixiPath.Value, PIXI_CLUSTER_DIR);
            string pixiExtended = Path.Combine(Options.Instance.PixiPath.Value, PIXI_EXTENDED_DIR);
            File.Copy(Path.Combine(ASMDirectory, SPRITE_DEFINES),
                Path.Combine(pixiSprites, SPRITE_DEFINES));
            File.Copy(Path.Combine(ASMDirectory, CLUSTER_DEFINES),
                Path.Combine(pixiClusters, CLUSTER_DEFINES));
            File.Copy(Path.Combine(ASMDirectory, EXTENDED_DEFINES),
                Path.Combine(pixiExtended, EXTENDED_DEFINES));
        }
        public void UberASMToolLinker()
        {
            Link(Path.Combine(Options.Instance.UberasmPath.Value, UBERASM_OTHER_DIR),
                UBERASMTOOL_DEFINES);
        }
        public void GPSLinker()
        {
            Link(Options.Instance.GPSPath.Value, GPS_DEFINES);
        }
        public void Link(string mainDirectory, string definesFile = "")
        {
            if (string.IsNullOrWhiteSpace(mainDirectory))
                return;
            copyExtraDefines(mainDirectory);
            editDefineFiles(Path.Combine(mainDirectory, definesFile));
        }
        private void editDefineFiles(string definePath)
        {
            if (string.IsNullOrWhiteSpace(definePath))
                return;
            string[] files = Directory.GetFiles(ExtraDefinesDirectory, ".*", SearchOption.AllDirectories);
            string[] lines = File.ReadAllLines(definePath);
            StringBuilder content = new(File.ReadAllText(definePath));
            Regex r;
            bool alreadyAdded;
            foreach (var file in files)
            {
                r = new($"incsrc\\s*\"{file}\"");
                alreadyAdded = false;
                foreach (var line in lines)
                {
                    if (r.IsMatch(line))
                    {
                        alreadyAdded = true;
                        break;
                    }
                }
                if (!alreadyAdded)
                    content.AppendLine($"incsrc \"{file}\"");
            }
            File.ReadAllText(definePath);
        }
        private void copyExtraDefines(string destination)
        {
            copyDirectory(ExtraDefinesDirectory, destination, true);
        }
        private static void copyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    copyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
