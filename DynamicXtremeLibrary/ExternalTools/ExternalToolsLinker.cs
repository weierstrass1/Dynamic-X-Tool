using DynamicXtremeLibrary.Config;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.ExternalTools
{
    /// <summary>Exports Dynamic Xtreme defines and routines into supported external SMW tools.</summary>
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

        /// <summary>Gets or sets the directory containing exported ASM define files.</summary>
        public string ASMDirectory { get; set; }
        /// <summary>Gets the source extra-defines directory.</summary>
        public string ExtraDefinesDirectory { get; private set; }
        /// <summary>Gets the source shared-routines directory.</summary>
        public string RoutinesDirectory { get; private set; }
        /// <summary>Initializes the linker with its source directories.</summary>
        public ExternalToolsLinker(string asmDirectory, string extraDefinesDirectory, string routinesDirectory)
        {
            ASMDirectory = asmDirectory;
            ExtraDefinesDirectory = extraDefinesDirectory;
            RoutinesDirectory = routinesDirectory;
        }
        /// <summary>
        /// Copies Dynamic Xtreme routines and sprite defines into the configured PIXI installation.
        /// </summary>
        /// <param name="transaction">The transaction that tracks every copied or modified file.</param>
        public void PixiLinker(FileTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(Options.Instance.PixiPath.Value) || 
                !Directory.Exists(Options.Instance.PixiPath.Value))
                return;
            Link(Path.Combine(Options.Instance.PixiPath.Value, PIXI_ASM_DIR), transaction: transaction);
            copyDirectory(RoutinesDirectory, 
                Path.Combine(Options.Instance.PixiPath.Value, PIXI_ROUTINES_DIR), true, transaction);
            string pixiSprites = Path.Combine(Options.Instance.PixiPath.Value, PIXI_SPRITE_DIR);
            string pixiClusters = Path.Combine(Options.Instance.PixiPath.Value, PIXI_CLUSTER_DIR);
            string pixiExtended = Path.Combine(Options.Instance.PixiPath.Value, PIXI_EXTENDED_DIR);
            transaction.CopyFile(Path.Combine(ASMDirectory, SPRITE_DEFINES),
                Path.Combine(pixiSprites, SPRITE_DEFINES));
            transaction.CopyFile(Path.Combine(ASMDirectory, CLUSTER_DEFINES),
                Path.Combine(pixiClusters, CLUSTER_DEFINES));
            transaction.CopyFile(Path.Combine(ASMDirectory, EXTENDED_DEFINES),
                Path.Combine(pixiExtended, EXTENDED_DEFINES));
        }
        /// <summary>
        /// Links Dynamic Xtreme defines into the configured UberASMTool installation.
        /// </summary>
        /// <param name="transaction">The transaction that tracks every copied or modified file.</param>
        public void UberASMToolLinker(FileTransaction transaction)
        {
            string path = Options.Instance.UberasmPath.Value;

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return;

            Link(Path.Combine(path, UBERASM_OTHER_DIR), UBERASMTOOL_DEFINES, transaction);
        }
        /// <summary>
        /// Links Dynamic Xtreme defines into the configured GPS installation.
        /// </summary>
        /// <param name="transaction">The transaction that tracks every copied or modified file.</param>
        public void GPSLinker(FileTransaction transaction)
        {
            Link(Options.Instance.GPSPath.Value, GPS_DEFINES, transaction);
        }
        /// <summary>
        /// Copies exported defines into an external tool and appends any missing include directives
        /// to its main defines file.
        /// </summary>
        /// <param name="mainDirectory">The external-tool directory that receives Dynamic Xtreme files.</param>
        /// <param name="definesFile">The optional defines file to update with include directives.</param>
        /// <param name="transaction">The required transaction that protects all file changes.</param>
        public void Link(string mainDirectory, string definesFile = "",
            FileTransaction? transaction = null)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            if (string.IsNullOrWhiteSpace(mainDirectory) || !Directory.Exists(mainDirectory))
                return;
            copyExtraDefines(mainDirectory, transaction);
            editDefineFiles(Path.Combine(mainDirectory, definesFile), transaction);
        }
        private static string[] _dontInclude = [
            "DXDefines",
            "Options",
            "DynamicSpritesMacros",
            "GraphicRoutinesMacros",
            "MultAndDiv",
            "PalettesMacros",
            "PaletteEffectsMacros",
            "PPU"
            ];
        /// <summary>
        /// Appends missing Dynamic Xtreme include directives to an existing external-tool defines
        /// file without duplicating includes already present.
        /// </summary>
        /// <param name="definePath">The defines file to inspect and update.</param>
        /// <param name="transaction">The transaction used to write the modified file.</param>
        private void editDefineFiles(string definePath, FileTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(definePath) || !File.Exists(definePath))
                return;
            var fs = Directory
                .GetFiles(ExtraDefinesDirectory, "*", SearchOption.AllDirectories)
                .Where(l => !_dontInclude.Any(s => s == Path.GetFileNameWithoutExtension(l)))
                .ToList();
            var files = fs.Prepend(Path.Combine(ExtraDefinesDirectory, "DXDefines.asm"));

            var lines = File.ReadAllLines(definePath);

            StringBuilder content = new(File.ReadAllText(definePath));
            content.AppendLine();
            foreach (string file in files)
            {
                string normalizedFile = file.Replace('\\', '/');
                Regex includeRegex = new(
                    $@"incsrc\s*""{Regex.Escape(normalizedFile)}""");

                bool alreadyAdded = lines.Any(line =>
                    includeRegex.IsMatch(line.Replace('\\', '/')));

                if (alreadyAdded)
                    continue;

                content.AppendLine($"incsrc \"{normalizedFile}\"");
            }
            string cont = content.ToString();
            cont = Regex.Replace(cont, @"(\s*\n)(\s*\n)+", "\n\n");
            transaction.WriteAllText(definePath, cont);
        }
        /// <summary>
        /// Copies the exported define tree into the corresponding directory of an external tool.
        /// </summary>
        /// <param name="destination">The external-tool directory that owns the copied tree.</param>
        /// <param name="transaction">The transaction used for directory creation and file copies.</param>
        private void copyExtraDefines(string destination, FileTransaction transaction)
        {
            string destExtDefs = Path.Combine(destination, ExtraDefinesDirectory);
            transaction.EnsureDirectory(destExtDefs);
            copyDirectory(ExtraDefinesDirectory, destExtDefs, true, transaction);
        }
        /// <summary>
        /// Copies a directory through the transaction so every destination can be restored or
        /// removed if installation fails.
        /// </summary>
        /// <param name="sourceDir">The directory whose files are copied.</param>
        /// <param name="destinationDir">The destination directory.</param>
        /// <param name="recursive">Whether child directories are copied recursively.</param>
        /// <param name="transaction">The transaction that protects all destination changes.</param>
        private static void copyDirectory(string sourceDir, string destinationDir, bool recursive,
            FileTransaction transaction)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            transaction.EnsureDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                transaction.CopyFile(file.FullName, targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    copyDirectory(subDir.FullName, newDestinationDir, true, transaction);
                }
            }
        }
    }
}
