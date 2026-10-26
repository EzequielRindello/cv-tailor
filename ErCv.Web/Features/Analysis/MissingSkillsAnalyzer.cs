using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Analysis;

public class MissingSkillsAnalyzer
{
    #region Public Methods

    /// <summary>
    /// The skills the post asks for that this CV does not show (EDIT-08). Of those, the ones the profile does have are
    /// listed apart: they were left out of this CV, not missing from the person.
    /// </summary>
    public MissingSkillsAnalysis Analyze(GeneratedCV cv, CVMaster profile, JobRequirements requirements)
    {
        var analysis = Analyze(CvAsProfile.Of(cv), requirements);
        var all = analysis.RequiredMissing.Concat(analysis.NiceToHaveMissing);
        analysis.InProfileNotInCv = all.Where(skill => IsInProfile(profile, skill)).ToList();
        return analysis;
    }

    public MissingSkillsAnalysis Analyze(CVMaster cv, JobRequirements requirements)
    {
        var allCVSkills = RelevanceScorer.FlattenSkills(cv.Skills);

        var allBulletsText = BulletsText(cv);

        // Required or nice to have is what the post says (MATCH-06), not a fixed list of technologies
        var niceToHave = new HashSet<string>(requirements.NiceToHaveSkills, StringComparer.OrdinalIgnoreCase);
        var requiredMissing = new List<string>();
        var niceToHaveMissing = new List<string>();

        foreach (var skill in requirements.TechnicalSkills)
        {
            if (!IsInProfile(skill, allCVSkills, allBulletsText))
            {
                if (niceToHave.Contains(skill))
                    niceToHaveMissing.Add(skill);
                else
                    requiredMissing.Add(skill);
            }
        }

        var suggestions = BuildSuggestions(requiredMissing, niceToHaveMissing);

        var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);
        var currentMatched = totalSkills - requiredMissing.Count - niceToHaveMissing.Count;
        var potentialMatched = currentMatched + requiredMissing.Count;

        var potentialIncrease = (int)Math.Round(
            (double)(potentialMatched - currentMatched) / totalSkills *
            MissingSkillsConstants.SkillWeightImpact);

        return new MissingSkillsAnalysis
        {
            RequiredMissing = requiredMissing.Take(MissingSkillsConstants.MaxRequiredMissing).ToList(),
            NiceToHaveMissing = niceToHaveMissing.Take(MissingSkillsConstants.MaxNiceToHaveMissing).ToList(),
            Suggestions = suggestions,
            PotentialScoreIncrease = Math.Min(potentialIncrease, MissingSkillsConstants.MaxPotentialIncrease)
        };
    }

    /// <summary>True when the profile lists the skill or mentions it in a bullet.</summary>
    public static bool IsInProfile(CVMaster cv, string skill) =>
        IsInProfile(skill, RelevanceScorer.FlattenSkills(cv.Skills), BulletsText(cv));

    #endregion

    #region Private Methods

    private static bool IsInProfile(string skill, List<string> profileSkills, string bulletsTextLower) =>
        SkillNormalizer.MatchesAny(skill, profileSkills) || SkillNormalizer.ContainsTerm(bulletsTextLower, skill);

    private static string BulletsText(CVMaster cv) =>
        string.Join(" ", cv.Experience.SelectMany(e => e.Bullets ?? new List<string>())).ToLower();

    private List<string> BuildSuggestions(List<string> required, List<string> niceToHave)
    {
        var suggestions = new List<string>();

        foreach (var skill in required.Take(MissingSkillsConstants.MaxRequiredSuggestions))
            suggestions.Add(string.Format(MissingSkillsConstants.RequiredSuggestionTemplate, skill));

        foreach (var skill in niceToHave.Take(MissingSkillsConstants.MaxNiceToHaveSuggestions))
            suggestions.Add(string.Format(MissingSkillsConstants.NiceToHaveSuggestionTemplate, skill));

        if (required.Count > MissingSkillsConstants.MaxRequiredSuggestions)
            suggestions.Add(string.Format(
                MissingSkillsConstants.ExtraRequiredSuggestionTemplate,
                required.Count - MissingSkillsConstants.MaxRequiredSuggestions));

        return suggestions;
    }

    #endregion
}
