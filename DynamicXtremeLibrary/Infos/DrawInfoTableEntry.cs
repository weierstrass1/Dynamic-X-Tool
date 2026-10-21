namespace DynamicXtremeLibrary.Infos
{
    /// <summary>Maps one DrawInfo pose to its generated routine and table slice.</summary>
    public class DrawInfoTableEntry
    {
        /// <summary>Gets or sets the generated graphic-routine ID.</summary>
        public int GraphicRoutineID;
        /// <summary>Gets or sets the pose ID.</summary>
        public int DrawInfoID;
        /// <summary>Gets or sets the number of tiles represented by the pose.</summary>
        public int DrawInfoLength;
        /// <summary>Gets or sets the pose's table offset within its routine.</summary>
        public int DrawInfoOffset;
    }
}
