namespace LogRegister
{
    public delegate void LogRenderAction(string text, ILogCategory category, SpanType type);
    public struct LogRenderResult
    {
        public ILogCategory Category;
        public string Text;
        public IReadOnlyList<LogSpan> Spans;

        public readonly void Render(LogRenderAction action)
        {
            int cursor = 0;

            foreach (var span in Spans)
            {
                if (span.Start > cursor)
                    action(Text[cursor..span.Start], Category, SpanType.NormalText);

                var value = Text.Substring(span.Start, span.Length);

                action(value, Category, span.Type);

                cursor = span.Start + span.Length;
            }

            if (cursor < Text.Length)
                action(Text[cursor..], Category, SpanType.NormalText);

            action("\n", Category, SpanType.NormalText);
        }
    }
}
