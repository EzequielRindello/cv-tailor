using ErCv.Web.Constants;
using ErCv.Web.Models;

namespace ErCv.Web.Features.CoverLetters;

/// <summary>
/// The visitor's edits to a generated cover letter (EDIT-09). The letter is written again whenever the CV or the
/// company changes; what the visitor edited is theirs and is never overwritten. A block (the salutation or a
/// paragraph, found by its key) they did not edit keeps following the CV. When the generated text of an edited block
/// changes afterwards, it is marked outdated and the visitor decides: nothing changes until they revert it.
/// </summary>
public class CoverLetterDraft
{
    #region Constants

    public const string SalutationKey = "salutation";

    #endregion

    #region Fields

    private CoverLetter _generated;

    // The visitor's text per block, and the generated text it was written over (to tell when that moved on)
    private readonly Dictionary<string, (string Text, string Baseline)> _edits = new();

    #endregion

    #region Constructors

    public CoverLetterDraft(CoverLetter generated) => _generated = generated;

    #endregion

    #region Properties

    /// <summary>The blocks the editor shows, in the letter's order; a block whose paragraph is no longer written stays, marked.</summary>
    public IReadOnlyList<LetterBlock> Blocks
    {
        get
        {
            var blocks = new List<LetterBlock> { Block(SalutationKey, _generated.Salutation) };
            blocks.AddRange(_generated.Paragraphs.Select(p => Block(p.Key, p.GeneratedText)));
            foreach (var orphan in _edits.Keys.Where(k => k != SalutationKey && _generated.Paragraphs.All(p => p.Key != k)))
                blocks.Insert(blocks.Count - 1, Block(orphan, string.Empty));
            return blocks;
        }
    }

    public string? Company => _generated.Company;

    #endregion

    #region Public Methods

    /// <summary>The letter was written again (the CV or the company changed): edits are kept as they are.</summary>
    public void Update(CoverLetter regenerated) => _generated = regenerated;

    /// <summary>False when the key is unknown, or the text is empty or too long.</summary>
    public bool Edit(string key, string? text)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0 || trimmed.Length > ApiConstants.MaxEditedLetterBlockLength) return false;
        if (!Blocks.Any(b => b.Key == key)) return false;

        var generated = GeneratedTextOf(key);
        if (trimmed == generated)
            _edits.Remove(key);
        else
            _edits[key] = (trimmed, generated);
        return true;
    }

    /// <summary>The generated text again ("Use the new text").</summary>
    public void Revert(string key) => _edits.Remove(key);

    public bool IsChanged => _edits.Count > 0;

    /// <summary>The letter as edited: what the preview, the downloads and the copy button use.</summary>
    public CoverLetter Build()
    {
        var letter = new CoverLetter
        {
            Language = _generated.Language,
            Accent = _generated.Accent,
            Fonts = _generated.Fonts,
            Header = _generated.Header,
            Date = _generated.Date,
            JobTitle = _generated.JobTitle,
            Company = _generated.Company,
            Subject = _generated.Subject,
            Salutation = _edits.TryGetValue(SalutationKey, out var salutation) ? salutation.Text : _generated.Salutation,
            Closing = _generated.Closing
        };

        letter.Paragraphs = _generated.Paragraphs
            .Select(p => new CoverLetterParagraph
            {
                Key = p.Key,
                Parts = p.Parts,
                EditedText = _edits.TryGetValue(p.Key, out var edit) ? edit.Text : null
            })
            .ToList();

        // An edited paragraph whose subject is no longer in the letter stays, before the closing one
        foreach (var orphan in _edits.Where(e => e.Key != SalutationKey && _generated.Paragraphs.All(p => p.Key != e.Key)))
            letter.Paragraphs.Insert(Math.Max(letter.Paragraphs.Count - 1, 0), new CoverLetterParagraph { Key = orphan.Key, EditedText = orphan.Value.Text });
        return letter;
    }

    #endregion

    #region Private Methods

    private LetterBlock Block(string key, string generated)
    {
        var edited = _edits.TryGetValue(key, out var edit);
        var outdated = edited && edit.Baseline != generated;
        return new LetterBlock(key, edited ? edit.Text : generated, generated, edited, outdated);
    }

    private string GeneratedTextOf(string key) =>
        key == SalutationKey
            ? _generated.Salutation
            : _generated.Paragraphs.FirstOrDefault(p => p.Key == key)?.GeneratedText ?? string.Empty;

    #endregion
}

/// <summary>One editable block of the letter: what it says now, what the app would write, and where the two stand.</summary>
public sealed record LetterBlock(string Key, string Text, string GeneratedText, bool IsEdited, bool IsOutdated);
