using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;

namespace ErCv.Tests.Invariants;

/// <summary>
/// EDIT-03: every fact in a cover letter comes from the profile (through the CV) or, for the position, from the
/// job post; everything else is the letter's fixed wording. Runs for every profile x JD in the corpus x language.
/// </summary>
[Trait("Category", "Invariant")]
public class CoverLetterInvariantTests
{
    public static TheoryData<string, string, string> Matrix()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var jd in FixtureLoader.AllJdNames)
        foreach (var language in ApiConstants.Languages)
            data.Add(profile, jd, language);
        return data;
    }

    private static HashSet<string> FixedWording(CoverLetterConstants.Phrases phrases)
    {
        var wording = typeof(CoverLetterConstants.Phrases).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => (string)p.GetValue(phrases)!)
            .ToHashSet();
        wording.Add(phrases.SkillsEnd + " ");
        return wording;
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public void Every_fact_in_the_letter_comes_from_the_cv_or_the_post(string profile, string jdName, string language)
    {
        var master = FixtureLoader.LoadProfile(profile);
        var jd = FixtureLoader.LoadJd(jdName);
        var requirements = new JobDescriptionParser().Parse(jd);
        var cv = new CVSelector().SelectAndAdaptCV(master, requirements, allowTwoPages: false, language);
        var labels = CvLabels.For(language);

        var letter = new CoverLetterGenerator().Generate(cv, requirements, jd, DateTime.Today);

        var facts = new HashSet<string>(StringComparer.Ordinal);
        if (CoverLetterGenerator.JobTitle(jd) is { } title) facts.Add(title);
        foreach (var exp in cv.Experience)
        {
            facts.Add(exp.Role);
            facts.Add(exp.Company.Trim());
            facts.Add(labels.DateRange(exp.StartDate, exp.EndDate));
            foreach (var bullet in exp.Bullets) facts.Add(bullet.Trim().TrimEnd('.'));
        }
        foreach (var skill in cv.Skills) facts.Add(skill);

        var wording = FixedWording(language == ApiConstants.LanguageSpanish ? CoverLetterConstants.Spanish : CoverLetterConstants.English);

        foreach (var part in letter.Paragraphs.SelectMany(p => p.Parts))
        {
            if (part.IsFact)
                Assert.True(facts.Contains(part.Text) || cv.Summary.StartsWith(part.Text, StringComparison.Ordinal),
                    $"'{part.Text}' is not in the CV or the post");
            else
                Assert.Contains(part.Text, wording);
        }

        // The header and sign-off are the profile's own contact details
        Assert.Equal(master.Header.FullName, letter.Header.FullName);
        Assert.Equal(master.Header.Email, letter.Header.Email);
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public void A_letter_never_quotes_a_bullet_twice(string profile, string jdName, string language)
    {
        var master = FixtureLoader.LoadProfile(profile);
        var jd = FixtureLoader.LoadJd(jdName);
        var requirements = new JobDescriptionParser().Parse(jd);
        var cv = new CVSelector().SelectAndAdaptCV(master, requirements, allowTwoPages: false, language);

        var letter = new CoverLetterGenerator().Generate(cv, requirements, jd, DateTime.Today);

        var bullets = cv.Experience.SelectMany(e => e.Bullets).Select(b => b.Trim().TrimEnd('.')).ToHashSet();
        var quoted = letter.Paragraphs.SelectMany(p => p.Parts).Where(p => p.IsFact && bullets.Contains(p.Text)).Select(p => p.Text).ToList();
        Assert.Equal(quoted.Count, quoted.Distinct().Count());
    }
}
