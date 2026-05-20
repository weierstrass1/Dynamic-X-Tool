using DynamicXtremeLibrary.GraphicRoutines;
using DynamicXtremeLibrary.Readers;

namespace DynamicXtremeLibrary.Generators
{
    public class PoseDataGenerator
    {
        public static string GenerateData(IEnumerable<GraphicRoutine> grs, string path)
        {
            string content = File.ReadAllText(path);

            var entries = GraphicRoutine.GetTableEntries(grs);
            var grSorted = grs.OrderBy(grs => grs.ID);

            string routines = string.Join('\n', 
                [.. grSorted.Select(gr => $"\tdl GraphicRoutines_{gr.Name}")]);

            content = content.Replace("<Offset>",
                HexReader.ValuesToString(
                            [.. entries.Select(e => e.DrawInfoOffset)], 4));

            content = content.Replace("<Length>",
                        HexReader.ValuesToString(
                            [.. entries.Select(e => e.DrawInfoLength)], 2));

            content = content.Replace("<ID>",
                        HexReader.ValuesToString(
                            [.. entries.Select(e => e.GraphicRoutineID)], 2));

            content = content.Replace("<Routine>", $"\n{routines}");

            return content;
        }
    }
}
