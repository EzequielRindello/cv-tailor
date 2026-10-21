using ErCv.Web.Models;

namespace ErCv.Web.Features.Selection;

/// <summary>Deep copies of CV models, so a generated CV never shares mutable state with its source.</summary>
public static class CvCloner
{
    public static Header Clone(Header h) => new()
    {
        FullName = h.FullName,
        Email = h.Email,
        Phone = h.Phone,
        Location = h.Location,
        LinkedIn = h.LinkedIn,
        GitHub = h.GitHub
    };

    public static Education Clone(Education e) => new()
    {
        Degree = e.Degree,
        Institution = e.Institution,
        Location = e.Location,
        GraduationDate = e.GraduationDate,
        GPA = e.GPA,
        RelevantCourses = (e.RelevantCourses ?? new List<string>()).ToList(),
        Honors = e.Honors
    };

    public static Experience Clone(Experience e) => new()
    {
        Role = e.Role,
        Company = e.Company,
        Location = e.Location,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Bullets = (e.Bullets ?? new List<string>()).ToList(),
        Skills = (e.Skills ?? new List<string>()).ToList()
    };

    public static Project Clone(Project p) => new()
    {
        Name = p.Name,
        Description = p.Description,
        Technologies = (p.Technologies ?? new List<string>()).ToList(),
        Highlights = (p.Highlights ?? new List<string>()).ToList(),
        Link = p.Link
    };

    public static GeneratedCV Clone(GeneratedCV cv) => new()
    {
        Header = Clone(cv.Header ?? new Header()),
        Summary = cv.Summary,
        Education = (cv.Education ?? new List<Education>()).Select(Clone).ToList(),
        Experience = (cv.Experience ?? new List<Experience>()).Select(Clone).ToList(),
        Skills = (cv.Skills ?? new List<string>()).ToList(),
        Projects = (cv.Projects ?? new List<Project>()).Select(Clone).ToList(),
        SectionOrder = (cv.SectionOrder ?? new List<string>()).ToList(),
        EditedBullets = (cv.EditedBullets ?? new List<BulletEdit>()).ToList()
    };
}
