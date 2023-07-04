using DynamicXLibrary.JSON;
using System.Text;

namespace DynamicXLibrary
{
    public class GraphicRoutineVersion
    {
        private static int currentID = 0;
        public const string GraphicRoutineFolder = "GraphicRoutine";
        public const string GraphicRoutineFile = "GraphicRoutine.asm";
        public const string LoopSectionFile = "LoopSection.asm";
        public int ID { get; private set; }
        public bool OneTile { get; private set; }
        public bool FlipX { get; private set; }
        public bool FlipY { get; private set; }
        public bool EqualTile { get; private set; }
        public bool EqualProp { get; private set; }
        public bool EqualXdisp { get; private set; }
        public bool EqualYdisp { get; private set; }
        public bool EqualSize { get; private set; }
        private static readonly Dictionary<int, GraphicRoutineVersion> instances = new();
        public static List<GraphicRoutineVersion> GraphicRoutineVersions { get => instances.Values.ToList(); }
        public string Content { get; private set; }
        public List<FrameInfo> FramesInfo { get; private set; }
        private GraphicRoutineVersion(string content)
        {
            Content = content;
            ID = currentID;
            currentID++;
            FramesInfo = new();
        }

        public static GraphicRoutineVersion Create(FrameInfo fi)
        {
            bool oneTile = fi.OneTile();
            bool props = fi.HasProperties();
            bool sizes = fi.HasSizes();
            bool xdisp = fi.HasXDisplacement();
            bool ydisp = fi.HasYDisplacement();
            bool flipxdisp = fi.HasXFlip();
            bool flipydisp = fi.HasYFlip();

            bool equalTile = fi.AllTilesAreEquals();
            bool equalProp = fi.AllPropertiesAreEquals() || !props;
            bool equalXdisp = fi.AllXDisplacementsAreEquals() || !xdisp;
            bool equalYdisp = fi.AllYDisplacementsAreEquals() || !ydisp;
            bool equalSize = fi.AllSizesAreEquals() || !sizes;

            string gr = File.ReadAllText(Path.Combine("ASM", GraphicRoutineFolder, GraphicRoutineFile));
            string ls = File.ReadAllText(Path.Combine("ASM", GraphicRoutineFolder, LoopSectionFile));

            SimpleJSON posRep = JSON.JSON.Position.SelectReplacement(equalXdisp, equalYdisp);
            gr = gr.Replace("<Position_PreLoop>", posRep.PreLoop.Code);
            ls = ls.Replace("<Position_InLoop>", posRep.InLoop.Code);

            SimpleJSON tileRep = JSON.JSON.Tile.SelectReplacement(true, equalTile);

            gr = gr.Replace("<Tile_PreLoop>", tileRep.PreLoop.Code);
            ls = ls.Replace("<Tile_InLoop>", tileRep.InLoop.Code);

            SimpleJSON sizeRep = JSON.JSON.Size.SelectReplacement(equalSize);
            SimpleJSON propRep = JSON.JSON.Property.SelectReplacement(equalProp);
            ls = ls.Replace("<Size_InLoop>", sizeRep.InLoop.Code);
            ls = ls.Replace("<Property_InLoop>", propRep.InLoop.Code);

            SimpleJSON flipRep = JSON.JSON.Flip.SelectReplacement(flipxdisp, flipydisp);

            string lsFx = ls.Replace("XDisplacements,y", "XDisplacementsFlip,y");
            string lsFy = ls.Replace("YDisplacements,y", "YDisplacementsFlip,y");
            string lsFxy = lsFx.Replace("YDisplacements,y", "YDisplacementsFlip,y");

            string loopSection = flipRep.InLoop.Code
                                    .Replace("<Flip_NoFlip>", ls)
                                    .Replace("<Flip_FlipX>", lsFx)
                                    .Replace("<Flip_FlipY>", lsFy)
                                    .Replace("<Flip_FlipXY>", lsFxy)
                                    .Replace("<FixedSize>", fi.AllSizesAre16() ? "#$02" : "#$00");

            gr = gr.Replace("<LoopSection>", loopSection);

            int key = fi.GetKey();

            if (!instances.TryGetValue(key, out GraphicRoutineVersion? grvValue))
            {
                grvValue = new(gr);
                instances.Add(key, grvValue);
            }
            grvValue.OneTile = oneTile;
            grvValue.FlipX = flipxdisp;
            grvValue.FlipY = flipydisp;
            grvValue.EqualTile = equalTile;
            grvValue.EqualProp = equalProp;
            grvValue.EqualXdisp = equalXdisp;
            grvValue.EqualYdisp = equalYdisp ;
            grvValue.EqualSize = equalSize ;

            grvValue.FramesInfo.Add(fi);
            return grvValue;
        }
        public void GenerateTables()
        {
            StringBuilder sb = new();
            if(!EqualTile)
            {
                sb.AppendLine("Tiles:");
                foreach(FrameInfo fi in FramesInfo)
                {
                    sb.Append($"{fi.ContextName}_{fi.Name}_Tiles:");
                    sb.AppendLine(fi.TilesToString());
                }
            }
            if (!EqualProp)
            {
                sb.AppendLine("Properties:");
                foreach (FrameInfo fi in FramesInfo)
                {
                    sb.Append($"{fi.ContextName}_{fi.Name}_Properties:");
                    sb.AppendLine(fi.PropertiesToString());
                }
            }
            if (!EqualXdisp)
            {
                sb.AppendLine("XDisplacements:");
                foreach (FrameInfo fi in FramesInfo)
                {
                    sb.Append($"{fi.ContextName}_{fi.Name}_XDisplacements:");
                    sb.AppendLine(fi.XDisplacementsToString());
                }
                if (FlipX)
                {
                    sb.AppendLine("XDisplacementsFlip:");
                    foreach (FrameInfo fi in FramesInfo)
                    {
                        sb.Append($"{fi.ContextName}_{fi.Name}_XDisplacementsFlip:");
                        sb.AppendLine(fi.FlipXDisplacementsToString());
                    }
                }
            }
            if (!EqualYdisp)
            {
                sb.AppendLine("YDisplacements:");
                foreach (FrameInfo fi in FramesInfo)
                {
                    sb.Append($"{fi.ContextName}_{fi.Name}_YDisplacements:");
                    sb.AppendLine(fi.YDisplacementsToString());
                }
                if (FlipY)
                {
                    sb.AppendLine("YDisplacementsFlip:");
                    foreach (FrameInfo fi in FramesInfo)
                    {
                        sb.Append($"{fi.ContextName}_{fi.Name}_YDisplacementsFlip:");
                        sb.AppendLine(fi.FlipYDisplacementsToString());
                    }
                }
            }
            if (!EqualSize)
            {
                sb.AppendLine("Sizes:");
                foreach (FrameInfo fi in FramesInfo)
                {
                    sb.Append($"{fi.ContextName}_{fi.Name}_Sizes:");
                    sb.AppendLine(fi.SizesToString());
                }
            }
            Content = Content
                        .Replace("<Tables>", sb.ToString())
                        .Replace("<address>",$"${(3*ID):X2}");
        }
        public static GraphicRoutineVersion? Get(int key)
            => instances.ContainsKey(key) ? instances[key] : null;
        public string GetFlags()
        {
            if (OneTile)
                return "OneTile";
            StringBuilder sb = new();
            if ((FlipX || FlipY) && (!EqualXdisp || !EqualYdisp))
            {
                sb.Append("WithFlip");
                if (FlipX)
                    sb.Append("X");
                if(FlipY) 
                    sb.Append("Y");
            }

            if (!EqualTile && !EqualXdisp && !EqualYdisp && !EqualSize && !EqualProp)
                return sb.ToString();
            if(EqualSize)
                sb.Insert(0, "Size");
            if (EqualProp)
                sb.Insert(0, "Prop");
            if (EqualXdisp || EqualYdisp)
            {
                sb.Insert(0, "Disp");
                if (EqualYdisp)
                    sb.Insert(0, "Y");
                if (EqualXdisp)
                    sb.Insert(0, "X");
            };
            if (EqualTile)
                sb.Insert(0, "Tile");
            sb.Insert(0, "Same");
            return sb.ToString();
        }
    }
}
