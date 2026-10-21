using System.Text.RegularExpressions;
using System.Text;

namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Describes a deferred rewrite of one ASM consumer while preserving its encoding and newline
    /// convention.
    /// </summary>
    public sealed class SourceRewrite
    {
        private readonly Dictionary<string, string> _symbolChanges = new(StringComparer.Ordinal);
        private readonly Dictionary<int, string> _directiveNames = [];

        /// <summary>
        /// Gets the ASM source file to rewrite.
        /// </summary>
        public required string Path { get; init; }

        /// <summary>
        /// Gets the exact old-to-new Asar symbol replacements registered for this source file.
        /// </summary>
        public IReadOnlyDictionary<string, string> SymbolChanges => _symbolChanges;

        /// <summary>
        /// Adds an exact symbol replacement and rejects ambiguous mappings for the same old symbol.
        /// </summary>
        /// <param name="oldName">The complete symbol currently used by the ASM source.</param>
        /// <param name="newName">The complete replacement symbol.</param>
        public void AddSymbolChange(string oldName, string newName)
        {
            if (oldName == newName)
                return;
            if (_symbolChanges.TryGetValue(oldName, out string? current) && current != newName)
                throw new InvalidOperationException(
                    $"ASM symbol '{oldName}' in '{Path}' would need two different replacements.");
            _symbolChanges[oldName] = newName;
        }

        /// <summary>
        /// Records the logical-name marker that must be added to or updated on an include directive.
        /// </summary>
        /// <param name="zeroBasedLine">The zero-based source line containing the directive.</param>
        /// <param name="logicalName">The final logical name assigned to the included input.</param>
        public void SetDirectiveLogicalName(int zeroBasedLine, string logicalName)
            => _directiveNames[zeroBasedLine] = logicalName;

        /// <summary>
        /// Applies all planned directive and symbol changes in memory without modifying the source
        /// file on disk.
        /// </summary>
        /// <returns>The complete rewritten file content using the original encoding and newline style.</returns>
        public byte[] BuildContent()
        {
            byte[] original = File.ReadAllBytes(Path);
            (Encoding encoding, int preambleLength) = detectEncoding(original);
            string content = encoding.GetString(original, preambleLength,
                original.Length - preambleLength);
            string newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            bool endsWithNewline = content.EndsWith("\n", StringComparison.Ordinal);
            string[] lines = content.Replace("\r\n", "\n").Split('\n');

            foreach (var directive in _directiveNames)
            {
                if (directive.Key < 0 || directive.Key >= lines.Length)
                    throw new InvalidOperationException($"Directive line changed in '{Path}'.");
                lines[directive.Key] = Regex.Replace(lines[directive.Key],
                    @"\s*;\s*dxt-name\s*:\s*[A-Za-z0-9]+\s*$", "",
                    RegexOptions.IgnoreCase);
                lines[directive.Key] += $" ; dxt-name: {directive.Value}";
            }

            content = string.Join(newline, lines);
            if (!endsWithNewline && content.EndsWith(newline, StringComparison.Ordinal))
                content = content[..^newline.Length];

            foreach (var change in _symbolChanges.OrderByDescending(x => x.Key.Length))
            {
                content = Regex.Replace(content,
                    $@"(?<![A-Za-z0-9_]){Regex.Escape(change.Key)}(?![A-Za-z0-9_])",
                    _ => change.Value);
            }
            byte[] body = encoding.GetBytes(content);
            byte[] preamble = preambleLength == 0 ? [] : encoding.GetPreamble();
            return [.. preamble, .. body];
        }

        /// <summary>
        /// Detects UTF-8 and UTF-16 byte-order marks, accepts strict UTF-8 without a mark, and uses
        /// Latin-1 as a byte-preserving fallback for legacy source files.
        /// </summary>
        /// <param name="bytes">The original source bytes.</param>
        /// <returns>The selected encoding and the number of preamble bytes to skip while decoding.</returns>
        private static (Encoding Encoding, int PreambleLength) detectEncoding(byte[] bytes)
        {
            if (bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()))
                return (new UTF8Encoding(true), Encoding.UTF8.GetPreamble().Length);
            if (bytes.AsSpan().StartsWith(Encoding.Unicode.GetPreamble()))
                return (Encoding.Unicode, Encoding.Unicode.GetPreamble().Length);
            if (bytes.AsSpan().StartsWith(Encoding.BigEndianUnicode.GetPreamble()))
                return (Encoding.BigEndianUnicode, Encoding.BigEndianUnicode.GetPreamble().Length);
            try
            {
                Encoding utf8 = new UTF8Encoding(false, true);
                _ = utf8.GetString(bytes);
                return (utf8, 0);
            }
            catch (DecoderFallbackException)
            {
                return (Encoding.Latin1, 0);
            }
        }
    }
}
