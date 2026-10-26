using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using static ErCv.Web.Features.CoverLetters.CoverLetterConstants;

namespace ErCv.Web.Features.CoverLetters;

/// <summary>
/// Writes a cover letter from the CV as it stands (EDIT-03): an opening, a paragraph for each of the two most
/// relevant experiences and a closing. The app writes no claim of its own: the wording is fixed, and every name,
/// role, date, bullet and skill is quoted from the CV, the position from the job post.
/// </summary>
public class CoverLetterGenerator
{
    #region Public Methods

    public CoverLetter Generate(GeneratedCV cv, JobRequirements requirements, string jobDescription, DateTime today, string? company = null)
    {
        company = string.IsNullOrWhiteSpace(company) ? null : company.Trim();
        var phrases = cv.Language == ApiConstants.LanguageSpanish ? Spanish : English;
        var labels = CvLabels.For(cv.Language);
        var jobTitle = JobTitle(jobDescription);

        var letter = new CoverLetter
        {
            Language = labels.Language,
            Accent = cv.Accent,
            Fonts = cv.Fonts,
            Header = CvCloner.Clone(cv.Header ?? new Header()),
            Date = string.Format(phrases.DateFormat, today.Day, phrases.MonthNames[today.Month - 1], today.Year),
            JobTitle = jobTitle,
            Subject = jobTitle == null ? string.Empty : phrases.Subject + jobTitle,
            Company = company,
            Salutation = company == null ? phrases.Salutation : phrases.SalutationCompanyLead + company + phrases.SalutationCompanyEnd,
            Closing = phrases.SignOff
        };

        letter.Paragraphs.Add(Opening(cv, jobTitle, phrases));

        var experiences = (cv.Experience ?? new List<Experience>())
            .Where(e => e.Bullets?.Any() == true)
            .Take(ExperiencesWritten)
            .ToList();
        for (var i = 0; i < experiences.Count; i++)
            letter.Paragraphs.Add(ExperienceParagraph(experiences[i], i == 0, phrases, labels));

        letter.Paragraphs.Add(ClosingParagraph(cv, requirements, phrases, company));
        return letter;
    }

    /// <summary>The first line of the post when it reads as a position ("Senior Backend Engineer (Go)"), else null.</summary>
    public static string? JobTitle(string? jobDescription)
    {
        var first = (jobDescription ?? string.Empty).Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
        if (first == null || first.Length > MaxJobTitleLength) return null;

        var lower = first.ToLowerInvariant();
        return RoleNouns.Any(lower.Contains) ? first : null;
    }

    #endregion

    #region Private Methods

    private static CoverLetterParagraph Opening(GeneratedCV cv, string? jobTitle, Phrases phrases)
    {
        var paragraph = new CoverLetterParagraph { Key = OpeningKey };
        if (jobTitle != null)
        {
            Fixed(paragraph, phrases.ApplyForTitle);
            Fact(paragraph, jobTitle);
            Fixed(paragraph, phrases.ApplyForTitleEnd);
        }
        else
        {
            Fixed(paragraph, phrases.ApplyGeneric);
        }

        // The summary's first sentence already states the title, the years and the matching skills from the profile
        if (FirstSentence(cv.Summary) is { } sentence) Fact(paragraph, sentence);
        return paragraph;
    }

    private static CoverLetterParagraph ExperienceParagraph(Experience exp, bool first, Phrases phrases, CvLabels labels)
    {
        // Keyed by the experience, not its position: another experience can come to be written about
        var paragraph = new CoverLetterParagraph { Key = ExperienceKeyPrefix + ExperienceExplanation.KeyOf(exp) };
        Fixed(paragraph, first ? phrases.FirstRole : phrases.NextRole);
        Fact(paragraph, exp.Role);
        if (!string.IsNullOrWhiteSpace(exp.Company))
        {
            Fixed(paragraph, phrases.RoleAt);
            Fact(paragraph, exp.Company.Trim());
        }

        var dates = labels.DateRange(exp.StartDate, exp.EndDate);
        if (dates.Length > 0)
        {
            Fixed(paragraph, phrases.DatesOpen);
            Fact(paragraph, dates);
            Fixed(paragraph, phrases.DatesCloseLead);
        }
        else
        {
            Fixed(paragraph, phrases.NoDatesLead);
        }

        foreach (var bullet in exp.Bullets.Take(BulletsPerExperience))
        {
            Fact(paragraph, bullet.Trim().TrimEnd('.'));
            Fixed(paragraph, phrases.SentenceEnd);
        }
        return paragraph;
    }

    private static CoverLetterParagraph ClosingParagraph(GeneratedCV cv, JobRequirements requirements, Phrases phrases, string? company)
    {
        var paragraph = new CoverLetterParagraph { Key = ClosingKey };

        // Only skills that are both on the CV and in the post: the letter names nothing the profile does not list
        var shared = (cv.Skills ?? new List<string>())
            .Where(s => requirements.TechnicalSkills.Any(ts => SkillNormalizer.FuzzyMatch(s, ts)))
            .Take(MaxSkillsNamed)
            .ToList();
        if (shared.Any())
        {
            Fixed(paragraph, phrases.SkillsLead);
            for (var i = 0; i < shared.Count; i++)
            {
                if (i > 0)
                {
                    var last = i == shared.Count - 1;
                    var beforeI = LanguagePatterns.StartsWithISound().IsMatch(shared[i]);
                    Fixed(paragraph, !last ? phrases.ListSeparator : beforeI ? phrases.ListLastSeparatorBeforeI : phrases.ListLastSeparator);
                }
                Fact(paragraph, shared[i]);
            }
            Fixed(paragraph, phrases.SkillsEnd + " ");
        }

        if (company == null)
        {
            Fixed(paragraph, phrases.ClosingParagraph);
        }
        else
        {
            // The company is a fact: it is the visitor's or the post's, between two fixed pieces of wording
            Fixed(paragraph, phrases.ClosingCompanyLead);
            Fact(paragraph, company);
            Fixed(paragraph, phrases.ClosingCompanyEnd);
        }
        return paragraph;
    }

    private static string? FirstSentence(string? summary)
    {
        var text = summary?.Trim();
        if (string.IsNullOrEmpty(text)) return null;

        var end = text.IndexOf(". ", StringComparison.Ordinal);
        return end < 0 ? text : text[..(end + 1)];
    }

    private static void Fixed(CoverLetterParagraph paragraph, string text) => paragraph.Parts.Add(new CoverLetterPart(text, false));

    private static void Fact(CoverLetterParagraph paragraph, string text) => paragraph.Parts.Add(new CoverLetterPart(text, true));

    #endregion
}
