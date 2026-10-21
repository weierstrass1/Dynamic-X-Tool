namespace LogRegister
{
    /// <summary>Consumes one categorized span produced by the log renderer.</summary>
    public delegate void LogRenderAction(string text, ILogCategory category, SpanType type, bool mustWrite = false);
    /// <summary>Contains rendered text and the categorized spans embedded within it.</summary>
    public struct LogRenderResult
    {
        /// <summary>Gets or sets the default category for unannotated text.</summary>
        public ILogCategory Category;
        /// <summary>Gets or sets the rendered message text.</summary>
        public string Text;
        /// <summary>Gets or sets categorized spans within the text.</summary>
        public IReadOnlyList<LogSpan> Spans;

        /// <summary>Emits normal text and annotated spans in source order.</summary>
        public readonly void Render(LogRenderAction action, bool mustWrite = false)
        {
            int cursor = 0;

            foreach (var span in Spans)
            {
                if (span.Start > cursor)
                    action(Text[cursor..span.Start], Category, SpanType.NormalText, mustWrite: mustWrite);

                var value = Text.Substring(span.Start, span.Length);

                action(value, Category, span.Type, mustWrite: mustWrite);

                cursor = span.Start + span.Length;
            }

            if (cursor < Text.Length)
                action(Text[cursor..], Category, SpanType.NormalText, mustWrite: mustWrite);

            action("\n", Category, SpanType.NormalText, mustWrite: mustWrite);
        }
    }
}
