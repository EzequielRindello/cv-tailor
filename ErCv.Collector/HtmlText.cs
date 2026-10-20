using System.Net;
using System.Text.RegularExpressions;

namespace ErCv.Collector
{
    /// <summary>Turns a job post's HTML into the plain text the app reads: paragraphs, "- " bullets, no markup.</summary>
    public static partial class HtmlText
    {
        public static string ToPlain(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";

            var s = ScriptOrStyle().Replace(html, " ");
            s = LineBreak().Replace(s, "\n");
            s = ListItem().Replace(s, "\n- ");
            s = BlockEnd().Replace(s, "\n\n");
            s = Tag().Replace(s, " ");
            s = WebUtility.HtmlDecode(s).Replace('\u00a0', ' ').Replace("\u200b", "");
            return Normalize(s);
        }

        /// <summary>Trims every line, collapses runs of spaces and of blank lines. Em dashes become hyphens (project rule).</summary>
        public static string Normalize(string text)
        {
            var lines = text.Replace('\u2014', '-').Replace('\u2015', '-').Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
                .Select(l => Spaces().Replace(l, " ").Trim())
                .ToList();

            // Markup such as <li><p>text</p></li> leaves a bare "-" line before the text it belongs to
            var merged = new List<string>();
            var bulletPending = false;
            foreach (var line in lines)
            {
                if (line is "-" or "•") { bulletPending = true; continue; }
                if (line.Length == 0) { merged.Add(line); continue; }
                merged.Add(bulletPending && !line.StartsWith("- ") ? "- " + line : line);
                bulletPending = false;
            }

            // One blank line between paragraphs, none between consecutive bullets
            var result = new List<string>();
            for (var i = 0; i < merged.Count; i++)
            {
                if (merged[i].Length > 0) { result.Add(merged[i]); continue; }
                if (result.Count == 0 || result[^1].Length == 0) continue;
                var next = merged.Skip(i + 1).FirstOrDefault(l => l.Length > 0);
                if (next == null) break;
                if (result[^1].StartsWith("- ") && next.StartsWith("- ")) continue;
                result.Add("");
            }
            return string.Join("\n", result).Trim();
        }

        /// <summary>Cuts long text at a paragraph boundary.</summary>
        public static string Truncate(string text, int max)
        {
            if (text.Length <= max) return text;
            var cut = text.LastIndexOf("\n\n", max, StringComparison.Ordinal);
            return (cut > max / 2 ? text[..cut] : text[..max]).TrimEnd();
        }

        [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex ScriptOrStyle();

        [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
        private static partial Regex LineBreak();

        [GeneratedRegex(@"<li\b[^>]*>", RegexOptions.IgnoreCase)]
        private static partial Regex ListItem();

        [GeneratedRegex(@"</?(p|div|h[1-6]|ul|ol|section|article|header|footer|table|tr|blockquote)\b[^>]*>|</li\s*>", RegexOptions.IgnoreCase)]
        private static partial Regex BlockEnd();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex Tag();

        [GeneratedRegex(@"[ \t\f\v]+")]
        private static partial Regex Spaces();
    }
}
