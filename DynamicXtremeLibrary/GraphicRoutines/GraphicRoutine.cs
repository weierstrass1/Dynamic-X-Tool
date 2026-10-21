using DynamicXtremeLibrary.Infos;
using System.Text;

namespace DynamicXtremeLibrary.GraphicRoutines
{
    /// <summary>Represents one specialized drawing routine shared by compatible DrawInfo poses.</summary>
    public class GraphicRoutine
    {
        /// <summary>Gets the generated Asar routine label.</summary>
        public string Name { get; private set; }
        /// <summary>Gets the routine's consecutive table ID.</summary>
        public int ID { get; private set; }
        /// <summary>Gets the bit-field key describing this routine's behavior.</summary>
        public int Key { get; private set; }
        /// <summary>Gets the number of table bytes stored per tile.</summary>
        public int TileSize { get; private set; }
        /// <summary>Gets the aggregate table size contributed by assigned poses.</summary>
        public int DataSize { get; private set; }
        /// <summary>Gets whether the routine specializes in one-tile poses.</summary>
        public bool OneTile { get; private set; }
        /// <summary>Gets whether a separate horizontal-flip table is required.</summary>
        public bool FlipX { get; private set; }
        /// <summary>Gets whether a separate vertical-flip table is required.</summary>
        public bool FlipY { get; private set; }
        /// <summary>Gets whether tile indices use the default progression.</summary>
        public bool DefaultTile { get; private set; }
        /// <summary>Gets whether tile properties use their default value.</summary>
        public bool DefaultProp { get; private set; }
        /// <summary>Gets whether X displacements can be omitted.</summary>
        public bool DefaultXdisp { get; private set; }
        /// <summary>Gets whether Y displacements can be omitted.</summary>
        public bool DefaultYdisp { get; private set; }
        /// <summary>Gets whether tile sizes can be represented by one routine flag.</summary>
        public bool DefaultSize { get; private set; }
        /// <summary>Gets whether the default tile size is 16x16.</summary>
        public bool Size16 { get; private set; }
        /// <summary>Gets whether the routine draws dynamic tiles.</summary>
        public bool IsDynamic { get; private set; }
        /// <summary>Gets the poses assigned to this routine.</summary>
        public IReadOnlyList<DrawInfo> DrawInfos { get => _drawInfos.AsReadOnly(); }
        private readonly List<DrawInfo> _drawInfos;
        /// <summary>Creates a routine from the behavioral requirements of its first pose.</summary>
        private GraphicRoutine(int id, DrawInfo drawInfo)
        {
            _drawInfos = [];
            ID = id;
            OneTile = drawInfo.OneTile();
            FlipX = drawInfo.HasXFlip();
            FlipY = drawInfo.HasYFlip();
            DefaultTile = drawInfo.AllTilesAreDefault();
            DefaultProp = drawInfo.AllPropertiesAreDefault();
            DefaultXdisp = drawInfo.AllXDisplacementsAreDefault();
            DefaultYdisp = drawInfo.AllYDisplacementsAreDefault();
            DefaultSize = drawInfo.AllSizesAreEqual();
            Size16 = drawInfo.AllSizesAre16();
            IsDynamic = drawInfo.IsDynamic;
            TileSize = 0;

            StringBuilder name = new();
            name.Append(IsDynamic ? "Dynamic_" : "Static_");
            if (!DefaultTile)
                TileSize++;
            if (!DefaultProp)
                TileSize++;
            if (!DefaultXdisp)
                TileSize += FlipX ? 2 : 1;
            if (!DefaultYdisp)
                TileSize += FlipY ? 2 : 1;
            if (!DefaultSize)
                TileSize++;
            DataSize = 0;
            TryAddDrawInfo(drawInfo);
            Key = GetKey(drawInfo);

            if (OneTile)
                name.Append("OneTile");
            name.Append(!DefaultXdisp && !DefaultYdisp && !DefaultProp && !DefaultSize ?
                "NotDefault" : "Default");

            if (DefaultTile)
                name.Append("Tiles");
            if (DefaultProp)
                name.Append("Props");
            if (DefaultXdisp)
                name.Append('X');
            if (DefaultYdisp) 
                name.Append('Y');
            if (DefaultSize)
                name.Append(Size16 ? "16x16" : "8x8");

            if (!FlipX && !FlipY)
            {
                Name = $"{name}_{ID}";
                return;
            }
            name.Append("With");
            if (FlipX)
                name.Append('X');
            if (FlipY)
                name.Append('Y');
            name.Append("Flip");
            Name = $"{name}_{ID}";
        }
        /// <summary>Adds a compatible pose when its table data remains within the routine-size limit.</summary>
        /// <param name="drawInfo">The pose to append.</param>
        /// <returns>Whether the pose was accepted.</returns>
        public bool TryAddDrawInfo(DrawInfo drawInfo)
        {
            int addedSize = drawInfo.GetLength() * TileSize;
            if(DataSize + addedSize > 30000)
                return false;
            _drawInfos.Add(drawInfo);
            DataSize += addedSize;
            return true;
        }
        /// <summary>Groups poses by behavior and splits groups when their data reaches the size limit.</summary>
        public static IReadOnlyDictionary<int, IReadOnlyList<GraphicRoutine>> GetGraphicRoutines(IEnumerable<DrawInfo> drawInfos)
        {
            Dictionary<int, List<GraphicRoutine>> res = [];
            int id = 0;
            GraphicRoutine gr;
            foreach (DrawInfo drawInfo in drawInfos)
            {
                gr = new(id, drawInfo);
                if (!res.TryGetValue(gr.Key, out List<GraphicRoutine>? value))
                {
                    value = [];
                    res[gr.Key] = value;
                    value.Add(gr);
                    id++;
                    continue;
                }
                gr = value.Last();
                if (gr.TryAddDrawInfo(drawInfo))
                    continue;
                gr = new(id, drawInfo);
                value.Add(gr);
                id++;
            }
            return res
                .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<GraphicRoutine>)kvp.Value.AsReadOnly())
                .AsReadOnly();
        }
        /// <summary>Encodes the drawing characteristics of a pose into a grouping bit field.</summary>
        public static int GetKey(DrawInfo di)
        {
            return (di.OneTile() ? 1 : 0) |
                (di.HasXFlip() ? 2 : 0) |
                (di.HasYFlip() ? 4 : 0) |
                (di.AllTilesAreDefault() ? 8 : 0) |
                (di.AllPropertiesAreDefault() ? 16 : 0) |
                (di.AllXDisplacementsAreDefault() ? 32 : 0) |
                (di.AllYDisplacementsAreDefault() ? 64 : 0) |
                (di.AllSizesAreEqual() ? 128 : 0) |
                (di.AllSizesAre16() ? 256 : 0) |
                (di.IsDynamic ? 512 : 0);
        }
        /// <summary>Builds pose lookup entries with routine IDs and per-routine data offsets.</summary>
        public static IReadOnlyList<DrawInfoTableEntry> GetTableEntries(IEnumerable<GraphicRoutine> routines)
        {
            List<DrawInfoTableEntry> entries = [];

            int offset;
            foreach (GraphicRoutine rout in routines)
            {
                offset = 0;
                foreach (DrawInfo di in rout.DrawInfos)
                {
                    entries.Add(new()
                    {
                        GraphicRoutineID = rout.ID,
                        DrawInfoID = di.ID,
                        DrawInfoLength = di.GetLength(),
                        DrawInfoOffset = offset
                    });
                    offset += di.GetLength();
                }
            }

            return [.. entries.OrderBy(e => e.DrawInfoID)];
        }
        /// <summary>Returns the generated routine label.</summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
