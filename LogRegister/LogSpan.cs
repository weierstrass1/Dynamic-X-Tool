namespace LogRegister
{
    public enum SpanType {         
        Timestamp,
        Prefix,
        NormalText,
        Parameter
    }
    public struct LogSpan
    {
        public ILogCategory Category;
        public int Start;
        public int Length;
        public SpanType Type;
    }
}
