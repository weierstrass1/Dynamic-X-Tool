using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace LogRegister
{
    public partial class LogRenderer
    {
        private readonly Dictionary<string, string> _messageTypeMapper;
        public LogRenderer(string jsonConfigPath)
        {
            if (!File.Exists(jsonConfigPath))
                throw new FileNotFoundException("Config file not found", jsonConfigPath);
            string json = File.ReadAllText(jsonConfigPath);
            _messageTypeMapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                ?? [];
        }
        public void RenderAll(IEnumerable<ILoggingRegister> logRegister, LogRenderAction action)
        {
            foreach (var log in logRegister)
            {
                var result = Render(log);
                result.Render(action);
            }
        }
        public LogRenderResult Render(ILoggingRegister logRegister)
        {
            if (!_messageTypeMapper.TryGetValue(logRegister.MessageType, out var message))
            {
                return new LogRenderResult
                {
                    Text = $"[UNKNOWN]: {logRegister.MessageType} (no template found)",
                    Spans =
                    [
                        new()
                {
                    Category = logRegister.Category,
                    Start = 0,
                    Length = 11,
                    Type = SpanType.Prefix
                },
                new()
                {
                    Category = logRegister.Category,
                    Start = 11,
                    Length = logRegister.MessageType.Length,
                    Type = SpanType.Parameter
                }
                    ]
                };
            }

            var parameters = logRegister.Parameters;

            message = $"[{logRegister.MessageType}]: " + message;

            List<LogSpan> spans =
            [
                new()
                {
                    Category = logRegister.Category,
                    Start = 0,
                    Length = logRegister.MessageType.Length + 4,
                    Type = SpanType.Prefix
                }
            ];

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (Match match in ParameterRegex().Matches(message))
            {
                if (match.Index > lastIndex)
                {
                    sb.Append(message, lastIndex, match.Index - lastIndex);
                }

                string key = match.Groups[1].Value;
                if (!parameters.TryGetValue(key, out string? value))
                    value = match.Value;

                int start = sb.Length;

                sb.Append(value);

                spans.Add(new()
                {
                    Category = logRegister.Category,
                    Start = start,
                    Length = value.Length,
                    Type = SpanType.Parameter
                });

                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < message.Length)
            {
                sb.Append(message, lastIndex, message.Length - lastIndex);
            }

            return new LogRenderResult
            {
                Category = logRegister.Category,
                Text = sb.ToString(),
                Spans = spans.AsReadOnly()
            };
        }

        [GeneratedRegex(@"\{(\w+)\}")]
        private static partial Regex ParameterRegex();
    }
}
