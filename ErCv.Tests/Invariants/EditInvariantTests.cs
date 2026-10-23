using System.Text.Json;
using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Tests.Invariants;

/// <summary>
/// EDIT-07: adding back what the selection left out only ever brings in what the master profile says.
/// For each profile x JD in the corpus x 1-page / 2-page, everything left out is added, and the edited CV is checked.
/// </summary>
[Trait("Category", "Invariant")]
public class EditInvariantTests
{
    public static TheoryData<string, string, bool> Matrix()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var jd in FixtureLoader.AllJdNames)
        foreach (var twoPages in new[] { false, true })
            data.Add(profile, jd, twoPages);
        return data;
    }

    private static string Key(Experience e) => $"{e.Role}|{e.Company}|{e.StartDate}|{e.EndDate}";

    [Theory]
    [MemberData(nameof(Matrix))]
    public void Everything_added_back_is_in_the_profile_and_the_profile_is_untouched(string profileId, string jdName, bool twoPages)
    {
        var master = FixtureLoader.LoadProfile(profileId);
        var untouched = JsonSerializer.Serialize(master);
        var requirements = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jdName));
        var generated = new CVSelector().SelectAndAdaptCV(master, requirements, twoPages);
        var draft = new CvDraft(generated, DocumentConstants.HarvardSectionOrder, master);

        AddEverything(draft);
        var cv = draft.Build();

        var keys = cv.Experience.Select(Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
        foreach (var exp in cv.Experience)
        {
            var source = master.Experience.SingleOrDefault(m => Key(m) == Key(exp));
            Assert.NotNull(source);
            Assert.All(exp.Bullets, b => Assert.Contains(b, source!.Bullets));
        }

        var profileSkills = Web.Features.Selection.RelevanceScorer.FlattenSkills(master.Skills);
        Assert.All(cv.Skills, s => Assert.Contains(s, profileSkills, StringComparer.OrdinalIgnoreCase));
        Assert.All(cv.Projects, p => Assert.Contains(master.Projects, m => m.Name == p.Name && m.Link == p.Link));

        Assert.Equal(untouched, JsonSerializer.Serialize(master));
        Assert.Empty(draft.LeftOutSkills);
        Assert.Empty(draft.LeftOutProjects);
        Assert.Empty(draft.LeftOutExperiences);
    }

    private static void AddEverything(CvDraft draft)
    {
        while (draft.LeftOutExperiences.Count > 0) draft.AddExperience(0);
        for (var e = 0; e < draft.Experiences.Count; e++)
            while (draft.LeftOutBullets(e).Count > 0) draft.AddBullet(e, draft.LeftOutBullets(e)[0].ProfileIndex);
        while (draft.LeftOutSkills.Count > 0) draft.AddSkill(draft.LeftOutSkills[0]);
        while (draft.LeftOutProjects.Count > 0) draft.AddProject(0);
    }
}
