namespace LogRegister
{
    /// <summary>Identifies the semantic role of a range in rendered log text.</summary>
    public enum SpanType {         
        Timestamp,
        Prefix,
        NormalText,
        Parameter
    }
    /// <summary>Describes one categorized range within a rendered log message.</summary>
    public struct LogSpan
    {
        /// <summary>Gets or sets the rendering category.</summary>
        public ILogCategory Category;
        /// <summary>Gets or sets the zero-based start index.</summary>
        public int Start;
        /// <summary>Gets or sets the span length.</summary>
        public int Length;
        /// <summary>Gets or sets the semantic span type.</summary>
        public SpanType Type;
    }
}
