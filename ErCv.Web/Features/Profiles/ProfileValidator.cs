using ErCv.Web.Features.Analysis;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Profiles;

/// <summary>One problem found in a profile. <see cref="Path"/> points at the field, e.g. "experience[1].startDate".</summary>
public record ProfileIssue(string Path, string Message);

public class ProfileValidationResult
{
    public List<ProfileIssue> Issues { get; } = new();

    public bool IsValid => Issues.Count == 0;
}

/// <summary>
/// Field-level validation for a profile the visitor typed or imported (PROF-02). It only checks that the
/// profile can produce a sensible CV; it never changes the profile.
/// </summary>
public class ProfileValidator
{
    #region Public Methods

    public ProfileValidationResult Validate(CVMaster? profile)
    {
        var result = new ProfileValidationResult();
        if (profile == null)
        {
            result.Issues.Add(new ProfileIssue("profile", "Profile is empty."));
            return result;
        }

        ValidateHeader(profile.Header, result);
        ValidateEducation(profile.Education, result);
        ValidateExperience(profile.Experience, result);
        ValidateProjects(profile.Projects, result);
        return result;
    }

    #endregion

    #region Private Methods

    private static void ValidateHeader(Header? header, ProfileValidationResult result)
    {
        if (header == null || string.IsNullOrWhiteSpace(header.FullName))
            result.Issues.Add(new ProfileIssue("header.fullName", "Full name is required."));

        if (header == null || string.IsNullOrWhiteSpace(header.Email))
            result.Issues.Add(new ProfileIssue("header.email", "Email is required."));
        else if (!ATSConstants.Email().IsMatch(header.Email.Trim()))
            result.Issues.Add(new ProfileIssue("header.email", "Email does not look valid."));
    }

    private static void ValidateEducation(List<Education>? education, ProfileValidationResult result)
    {
        for (var i = 0; i < (education?.Count ?? 0); i++)
        {
            var e = education![i];
            if (e == null) { result.Issues.Add(new ProfileIssue($"education[{i}]", "Entry is empty.")); continue; }

            if (string.IsNullOrWhiteSpace(e.Degree))
                result.Issues.Add(new ProfileIssue($"education[{i}].degree", "Degree is required."));
            if (string.IsNullOrWhiteSpace(e.Institution))
                result.Issues.Add(new ProfileIssue($"education[{i}].institution", "Institution is required."));
            if (!string.IsNullOrWhiteSpace(e.GraduationDate) && !DateParser.IsPresent(e.GraduationDate) &&
                !DateParser.TryParseStrict(e.GraduationDate, out _, out _))
                result.Issues.Add(new ProfileIssue($"education[{i}].graduationDate", DateHint(e.GraduationDate)));
        }
    }

    private static void ValidateExperience(List<Experience>? experience, ProfileValidationResult result)
    {
        if (experience == null || experience.Count == 0)
        {
            result.Issues.Add(new ProfileIssue("experience", "Add at least one experience."));
            return;
        }

        for (var i = 0; i < experience.Count; i++)
        {
            var x = experience[i];
            if (x == null) { result.Issues.Add(new ProfileIssue($"experience[{i}]", "Entry is empty.")); continue; }

            if (string.IsNullOrWhiteSpace(x.Role))
                result.Issues.Add(new ProfileIssue($"experience[{i}].role", "Role is required."));
            if (string.IsNullOrWhiteSpace(x.Company))
                result.Issues.Add(new ProfileIssue($"experience[{i}].company", "Company is required."));

            ValidateDates(x, i, result);

            for (var b = 0; b < (x.Bullets?.Count ?? 0); b++)
                if (string.IsNullOrWhiteSpace(x.Bullets![b]))
                    result.Issues.Add(new ProfileIssue($"experience[{i}].bullets[{b}]", "Bullet is empty."));

            for (var s = 0; s < (x.Skills?.Count ?? 0); s++)
                if (string.IsNullOrWhiteSpace(x.Skills![s]))
                    result.Issues.Add(new ProfileIssue($"experience[{i}].skills[{s}]", "Skill is empty."));
        }
    }

    private static void ValidateDates(Experience x, int i, ProfileValidationResult result)
    {
        var startOk = false;
        if (string.IsNullOrWhiteSpace(x.StartDate))
            result.Issues.Add(new ProfileIssue($"experience[{i}].startDate", "Start date is required."));
        else if (DateParser.IsPresent(x.StartDate) || !DateParser.TryParseStrict(x.StartDate, out _, out _))
            result.Issues.Add(new ProfileIssue($"experience[{i}].startDate", DateHint(x.StartDate)));
        else
            startOk = true;

        // An empty end date means the role is ongoing, so only a non-empty one is checked.
        if (string.IsNullOrWhiteSpace(x.EndDate) || DateParser.IsPresent(x.EndDate)) return;

        if (!DateParser.TryParseStrict(x.EndDate, out _, out _))
        {
            result.Issues.Add(new ProfileIssue($"experience[{i}].endDate", DateHint(x.EndDate)));
            return;
        }

        if (startOk &&
            DateParser.TryGetMonthIndex(x.StartDate, isEnd: false, out var start) &&
            DateParser.TryGetMonthIndex(x.EndDate, isEnd: true, out var end) && end < start)
            result.Issues.Add(new ProfileIssue($"experience[{i}].endDate", "End date is before the start date."));
    }

    private static void ValidateProjects(List<Project>? projects, ProfileValidationResult result)
    {
        for (var i = 0; i < (projects?.Count ?? 0); i++)
        {
            var p = projects![i];
            if (p == null) { result.Issues.Add(new ProfileIssue($"projects[{i}]", "Entry is empty.")); continue; }

            if (string.IsNullOrWhiteSpace(p.Name))
                result.Issues.Add(new ProfileIssue($"projects[{i}].name", "Project name is required."));

            for (var h = 0; h < (p.Highlights?.Count ?? 0); h++)
                if (string.IsNullOrWhiteSpace(p.Highlights![h]))
                    result.Issues.Add(new ProfileIssue($"projects[{i}].highlights[{h}]", "Highlight is empty."));
        }
    }

    private static string DateHint(string value) =>
        $"'{value.Trim()}' is not a date. Use a format such as 'Jun 2024', '2024-06', '06/2024' or '2024'.";

    #endregion
}
