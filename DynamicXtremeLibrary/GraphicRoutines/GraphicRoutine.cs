using DynamicXtremeLibrary.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.GraphicRoutines
{
    public class GraphicRoutine
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        public int Key { get; private set; }
        public int TileSize { get; private set; }
        public int DataSize { get; private set; }
        public bool OneTile { get; private set; }
        public bool FlipX { get; private set; }
        public bool FlipY { get; private set; }
        public bool DefaultTile { get; private set; }
        public bool DefaultProp { get; private set; }
        public bool DefaultXdisp { get; private set; }
        public bool DefaultYdisp { get; private set; }
        public bool DefaultSize { get; private set; }
        public bool Size16 { get; private set; }
        public bool IsDynamic { get; private set; }
        public IReadOnlyList<DrawInfo> DrawInfos { get => _drawInfos.AsReadOnly(); }
        private List<DrawInfo> _drawInfos { get; set; }
        private GraphicRoutine(int id, DrawInfo drawInfo)
        {
            _drawInfos = [];
            TryAddDrawInfo(drawInfo);
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
        public bool TryAddDrawInfo(DrawInfo drawInfo)
        {
            if(DataSize + drawInfo.GetLength() > 20000)
                return false;
            _drawInfos.Add(drawInfo);
            DataSize += drawInfo.GetLength() * TileSize;
            return true;
        }
        public static IReadOnlyDictionary<int, IReadOnlyList<GraphicRoutine>> GetGraphicRoutines(IEnumerable<DrawInfo> drawInfos)
        {
            Dictionary<int, List<GraphicRoutine>> res = [];
            int id = 0;
            GraphicRoutine gr;
            foreach (DrawInfo drawInfo in drawInfos)
            {
                gr = new(id, drawInfo);
                if (!res.ContainsKey(gr.Key))
                {
                    res[gr.Key] = [];
                    res[gr.Key].Add(gr);
                    id++;
                    continue;
                }
                gr = res[gr.Key].Last();
                if (gr.TryAddDrawInfo(drawInfo))
                    continue;
                gr = new(id, drawInfo);
                res[gr.Key].Add(gr);
                id++;
            }
            return res
                .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<GraphicRoutine>)kvp.Value.AsReadOnly())
                .AsReadOnly();
        }
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
        public override string ToString()
        {
            return Name;
        }
    }
}
