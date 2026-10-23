using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ErCv.Web.Features.Profiles;

/// <summary>
/// The text of an uploaded resume (PROF-05), one line per printed line, "- " before Word list items, " | " where
/// a line has columns (a tab, a right-aligned date).
/// </summary>
public static class ResumeText
{
    #region Constants

    private static readonly byte[] PdfMagic = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipMagic = { 0x50, 0x4B, 0x03, 0x04 };

    #endregion

    #region Public Methods

    public static ResumeFormat? Detect(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(PdfMagic)) return ResumeFormat.Pdf;
        if (bytes.AsSpan().StartsWith(ZipMagic)) return ResumeFormat.Docx;
        return null;
    }

    /// <summary>
    /// The text of a PDF, its first <see cref="ResumeImportConstants.MaxPdfPages"/> pages, top to bottom. Words far
    /// apart on a line are joined with " | " (a right-aligned date). A two-column page is read one column after
    /// the other. The LinkedIn and GitHub addresses behind links ("LinkedIn" printed, the address in the link)
    /// come first.
    /// </summary>
    public static string FromPdf(byte[] bytes)
    {
        using var document = PdfDocument.Open(bytes);
        var pages = document.GetPages().Take(ResumeImportConstants.MaxPdfPages).ToList();

        var text = new StringBuilder();
        foreach (var link in ProfileLinks(pages.SelectMany(p => p.GetHyperlinks()).Select(h => h.Uri)))
            text.AppendLine(link);

        foreach (var page in pages)
        {
            var words = page.GetWords().Where(w => !string.IsNullOrWhiteSpace(w.Text)).ToList();
            foreach (var line in PageLines(words)) text.AppendLine(line);
            text.AppendLine();
        }
        return text.ToString();
    }

    /// <summary>
    /// The text of a Word document's body, list paragraphs marked as items. A file that would expand past
    /// <see cref="ResumeImportConstants.MaxDocxUncompressedBytes"/> is refused before it is opened.
    /// </summary>
    public static string FromDocx(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, writable: false);
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
        {
            if (zip.Entries.Sum(e => e.Length) > ResumeImportConstants.MaxDocxUncompressedBytes)
                throw new InvalidDataException("The document expands past the size limit.");
        }

        stream.Position = 0;
        using var document = WordprocessingDocument.Open(stream, isEditable: false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        var text = new StringBuilder();
        foreach (var link in ProfileLinks(document.MainDocumentPart!.HyperlinkRelationships.Select(r => r.Uri.ToString())))
            text.AppendLine(link);

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var line = ParagraphText(paragraph);
            if (line.Length == 0) continue;
            text.AppendLine(IsListItem(paragraph) ? ResumeImportConstants.ItemPrefix + line : line);
        }
        return text.ToString();
    }

    #endregion

    #region Private Methods

    private static IEnumerable<string> ProfileLinks(IEnumerable<string?> uris) =>
        uris.OfType<string>()
            .Where(u => ResumeImportConstants.ProfileLinkHosts.Any(h => u.Contains(h, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<string> PageLines(List<Word> words)
    {
        var lines = GroupLines(words);
        if (lines.Count == 0) return Enumerable.Empty<string>();
        if (FindGutter(lines) is not { } gutter) return Joined(lines);

        // Above the first line that has both columns is the full-width header
        var firstSplit = lines.FindIndex(l => SideOf(l, gutter) == LineSide.Both);
        var top = lines.Take(firstSplit).ToList();
        var rest = lines.Skip(firstSplit).ToList();
        var left = rest.Select(l => l.Where(w => w.BoundingBox.Left < gutter).ToList()).Where(l => l.Count > 0).ToList();
        var right = rest.Select(l => l.Where(w => w.BoundingBox.Left >= gutter).ToList()).Where(l => l.Count > 0).ToList();
        return Joined(top).Concat(Joined(left)).Concat(Joined(right));
    }

    // A line was wrapped when the next line's first word would not have fit in the room left before the right
    // edge of the column: it ends with the wrap mark, so the parser joins the two
    // ("…a multi-tenant SaaS platform" / "(ASP.NET Core, React)…")
    private static IEnumerable<string> Joined(List<List<Word>> lines)
    {
        if (lines.Count == 0) yield break;
        var edge = lines.Max(l => l[^1].BoundingBox.Right);
        // Lines of one paragraph are the closest together for their font size; list items and entries have a
        // margin between them. A few lines (a header) do not show where the edge is
        var leadings = lines.Zip(lines.Skip(1), (a, b) => (Size: FontSize(a), Leading: Leading(a, b)))
            .Where(x => x.Leading > 0)
            .GroupBy(x => x.Size)
            .ToDictionary(g => g.Key, g => g.Min(x => x.Leading));
        var marks = lines.Count >= ResumeImportConstants.MinColumnLines;
        for (var i = 0; i < lines.Count; i++)
        {
            var text = JoinWords(lines[i]);
            var last = lines[i][^1];
            var wrapped = marks && i + 1 < lines.Count && !text.Contains(ResumeImportConstants.ColumnSeparator)
                          && leadings.TryGetValue(FontSize(lines[i]), out var leading)
                          && Leading(lines[i], lines[i + 1]) <= leading * ResumeImportConstants.MaxParagraphLeading
                          && edge - last.BoundingBox.Right < lines[i + 1][0].BoundingBox.Width + ResumeImportConstants.WrapSlackChars * CharWidth(last);
            yield return wrapped ? text + ResumeImportConstants.WrapMark : text;
        }
    }

    // The distance between two baselines in units of the upper line's font size
    private static double Leading(List<Word> above, List<Word> below) =>
        (above[0].BoundingBox.Bottom - below[0].BoundingBox.Bottom) / Math.Max(1, above[0].Letters[0].PointSize);

    private static double FontSize(List<Word> line) => Math.Round(line[0].Letters[0].PointSize, 1);

    private static double CharWidth(Word word) => word.BoundingBox.Width / Math.Max(1, word.Letters.Count);

    // Words on the same baseline, top to bottom, each line left to right
    private static List<List<Word>> GroupLines(List<Word> words)
    {
        var lines = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(w => w.BoundingBox.Bottom))
        {
            var line = lines.Count > 0 ? lines[^1] : null;
            var height = Math.Min(word.BoundingBox.Height, line?[0].BoundingBox.Height ?? word.BoundingBox.Height);
            if (line != null && Math.Abs(line[0].BoundingBox.Bottom - word.BoundingBox.Bottom) <= ResumeImportConstants.SameLineTolerance * height)
                line.Add(word);
            else
                lines.Add(new List<Word> { word });
        }
        foreach (var line in lines) line.Sort((a, b) => a.BoundingBox.Left.CompareTo(b.BoundingBox.Left));
        return lines;
    }

    private static string JoinWords(List<Word> line)
    {
        // A letter-spaced title is read one letter per word: a wider gap between letters is a space between words,
        // written as two spaces so the parser can join the letters back ("A L E X  R I V E R A")
        var gaps = line.Zip(line.Skip(1), (a, b) => b.BoundingBox.Left - a.BoundingBox.Right).ToList();
        var singles = line.Count(w => w.Letters.Count == 1);
        var letterSpaced = singles >= ResumeImportConstants.MinLetterSpacedLetters
                           && line.All(w => w.Letters.Count <= ResumeImportConstants.MaxKernedLetters)
                           && singles >= line.Count * ResumeImportConstants.MinSingleLetterShare;
        var letterGap = letterSpaced ? gaps.OrderBy(g => g).ElementAt(gaps.Count / 2) : 0;

        var text = new StringBuilder(line[0].Text);
        for (var i = 1; i < line.Count; i++)
        {
            var gap = gaps[i - 1];
            if (letterSpaced) text.Append(gap > ResumeImportConstants.LetterSpacedWordGap * letterGap ? "  " : " ");
            else text.Append(IsWideGap(line[i - 1], gap) ? ResumeImportConstants.ColumnSeparator : " ");
            text.Append(line[i].Text);
        }
        return text.ToString();
    }

    private static bool IsWideGap(Word before, double gap) => gap > ResumeImportConstants.ColumnGapChars * CharWidth(before);

    /// <summary>
    /// Where a line sits against a vertical band at <paramref name="x"/>. A line with words on both sides counts as
    /// in both columns only when the gap at the band is wide; a header line that merely has a word break there
    /// ("…555-0192 | San Francisco") counts as crossing it.
    /// </summary>
    private static LineSide SideOf(List<Word> line, double x)
    {
        var lastLeft = line.LastOrDefault(w => w.BoundingBox.Right <= x);
        var firstRight = line.FirstOrDefault(w => w.BoundingBox.Left >= x);
        if (line.Any(w => w.BoundingBox.Left < x && w.BoundingBox.Right > x)) return LineSide.Crosses;
        if (lastLeft == null) return LineSide.Right;
        if (firstRight == null) return LineSide.Left;
        return IsWideGap(lastLeft, firstRight.BoundingBox.Left - lastLeft.BoundingBox.Right) ? LineSide.Both : LineSide.Crosses;
    }

    /// <summary>
    /// The x of an empty vertical band that splits the page in two columns: (almost) no line crosses it and both
    /// sides have lines. Null for a one-column page, whose body lines cross any band in the middle of the page.
    /// </summary>
    private static double? FindGutter(List<List<Word>> lines)
    {
        if (lines.Count < 2 * ResumeImportConstants.MinColumnLines) return null;

        var minX = lines.Min(l => l[0].BoundingBox.Left);
        var maxX = lines.Max(l => l[^1].BoundingBox.Right);
        var margin = (maxX - minX) * ResumeImportConstants.GutterMargin;
        double? best = null;
        var bestCrossing = int.MaxValue;
        for (var x = minX + margin; x <= maxX - margin; x += ResumeImportConstants.GutterStep)
        {
            var sides = lines.Select(l => SideOf(l, x)).ToList();
            var crossing = sides.Count(s => s == LineSide.Crosses);
            var leftSide = sides.Count(s => s is LineSide.Left or LineSide.Both);
            var rightSide = sides.Count(s => s is LineSide.Right or LineSide.Both);
            if (crossing > lines.Count * ResumeImportConstants.MaxGutterCrossing
                || leftSide < ResumeImportConstants.MinColumnLines || rightSide < ResumeImportConstants.MinColumnLines) continue;
            if (crossing < bestCrossing) (best, bestCrossing) = (x, crossing);
        }
        return best;
    }

    private static string ParagraphText(Paragraph paragraph)
    {
        var text = new StringBuilder();
        foreach (var element in paragraph.Descendants())
        {
            switch (element)
            {
                case Text t: text.Append(t.Text); break;
                case TabChar: text.Append(ResumeImportConstants.ColumnSeparator); break;
                case Break: text.Append(' '); break;
            }
        }
        return text.ToString().Trim();
    }

    private static bool IsListItem(Paragraph paragraph)
    {
        var properties = paragraph.ParagraphProperties;
        if (properties?.NumberingProperties != null) return true;
        var style = properties?.ParagraphStyleId?.Val?.Value;
        return style != null && style.StartsWith(ResumeImportConstants.ListStylePrefix, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Nested Types

    private enum LineSide { Left, Right, Both, Crosses }

    #endregion
}

public enum ResumeFormat { Pdf, Docx }
