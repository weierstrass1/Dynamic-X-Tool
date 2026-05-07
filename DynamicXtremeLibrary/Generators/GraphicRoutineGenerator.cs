using DynamicXtremeLibrary;
using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.Infos;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicXtremeLibrary.Generators
{
    public class GraphicRoutineGenerator
    {
        public string IncludeTemplatePath { get; private set; }
        public string TemplatePath { get; private set; }
        public string OutputDirectory { get; private set; }
        public string OutputIncludeFileName { get; private set; }
        public string OutputProtsFileName { get; private set; }
        public GraphicRoutineGenerator(string includeTemplatePath, string templatePath, string outputDirectory, string outputIncludeFileName, string outputProtsFileName)
        {
            TemplatePath = templatePath;
            if (!File.Exists(TemplatePath))
                throw new FileNotFoundException(nameof(TemplatePath));
            IncludeTemplatePath = includeTemplatePath;
            if(!File.Exists(IncludeTemplatePath))
                throw new FileNotFoundException(nameof(IncludeTemplatePath));
            OutputDirectory = outputDirectory;
            if(!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
            OutputIncludeFileName = outputIncludeFileName;
            OutputProtsFileName = outputProtsFileName;
        }
        public void GenerateAllGraphicRoutine(IReadOnlyDictionary<int, IReadOnlyList<GraphicRoutine>> grs)
        {
            string[] files = Directory.GetFiles(OutputDirectory);
            foreach (string file in files) 
            {
                File.Delete(file);
            }
            StringBuilder prots = new();
            StringBuilder sb = new();

            prots.Append("prot ");
            bool first = true;
            foreach (var routineKvP in grs)
            {
                foreach (var gr in routineKvP.Value)
                {
                    sb.AppendLine("freecode");
                    sb.AppendLine($"incsrc \"{gr.Name}.asm\"");
                    if (!first)
                        prots.Append(',');
                    prots.Append($"GraphicRoutines_{gr.Name}");
                    first = false;
                    GenerateGraphicRoutine(gr);
                }
            }
            prots.AppendLine();
            string includeContent = File.ReadAllText(IncludeTemplatePath)
                .Replace("<grs>", sb.ToString());
            File.WriteAllText(Path.Combine(OutputDirectory, OutputProtsFileName), prots.ToString());
            File.WriteAllText(Path.Combine(OutputDirectory, OutputIncludeFileName), includeContent);
        }
        public void GenerateGraphicRoutine(GraphicRoutine gr)
        {
            string title = $"{gr.Name}:";
            var drawinfos = gr.DrawInfos;
            string tiles = gr.DefaultTile ? 
                "" : 
                $"\n.Tiles\n{GetTable(drawinfos, di => di.TilesToString())}";
            string props = gr.DefaultProp ?
                "" :
                $"\n.Properties\n{GetTable(drawinfos, di => di.PropertiesToString())}";
            string xDisps = gr.DefaultXdisp ?
                "" :
                $"\n.XDisplacements\n{GetTable(drawinfos, di => di.XDisplacementsToString())}";
            string xFlipDisps = gr.DefaultXdisp || !gr.FlipX ?
                "" :
                $"\n.XDisplacementsFlip\n{GetTable(drawinfos, di => di.FlipXDisplacementsToString())}";
            string yDisps = gr.DefaultYdisp ?
                "" :
                $"\n.YDisplacements\n{GetTable(drawinfos, di => di.YDisplacementsToString())}";
            string yFlipDisps = gr.DefaultYdisp || !gr.FlipY ?
                "" :
                $"\n.YDisplacementsFlip\n{GetTable(drawinfos, di => di.FlipYDisplacementsToString())}";
            string sizes = gr.DefaultSize ?
                "" :
                $"\n.Sizes\n{GetTable(drawinfos, di => di.SizesToString())}";
            string content = File.ReadAllText(TemplatePath)
                .Replace("<IsDynamic>", gr.IsDynamic ? "1" : "0")
                .Replace("<OneTile>", gr.OneTile ? "1" : "0")
                .Replace("<DefaultXdisp>", gr.DefaultXdisp ? "1" : "0")
                .Replace("<DefaultYdisp>", gr.DefaultYdisp ? "1" : "0")
                .Replace("<FlipX>", gr.FlipX ? "1" : "0")
                .Replace("<FlipY>", gr.FlipY ? "1" : "0")
                .Replace("<DefaultTile>", gr.DefaultTile ? "1" : "0")
                .Replace("<DefaultProp>", gr.DefaultProp ? "1" : "0")
                .Replace("<DefaultSize>", gr.DefaultSize ? "1" : "0")
                .Replace("<Size16>", gr.DefaultSize && gr.Size16 ? "1" : "0")
                .Replace("<Title>", title)
                .Replace("<Tiles>", tiles)
                .Replace("<Properties>", props)
                .Replace("<XDisplacements>", $"{xDisps}{xFlipDisps}")
                .Replace("<YDisplacements>", $"{yDisps}{yFlipDisps}")
                .Replace("<Sizes>", sizes);
            content = Regex.Replace(content, @"(\s*\r?\n)(\s*\r?\n)+", "\n\n");
            content = Regex.Replace(content, @"\n\n$", "\n");

            File.WriteAllText(Path.Combine(OutputDirectory, $"{title[..^1]}.asm"), content);
        }
        private string GetTable(IReadOnlyList<DrawInfo> list, Func<DrawInfo, string> stringValue)
        {
            StringBuilder sb = new();
            int[] values;
            foreach (DrawInfo di in list)
            {
                sb.Append($"..{di.ContextName}_{di.Name}");
                sb.AppendLine(stringValue(di));
            }
            return sb.ToString();
        }
    }
}
