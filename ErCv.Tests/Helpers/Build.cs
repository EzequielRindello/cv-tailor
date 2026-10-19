using ErCvApi.Models;

namespace ErCv.Tests.Helpers
{
    /// <summary>Small object builders so each test only states what it cares about.</summary>
    public static class Build
    {
        public static Experience Exp(
            string role = "Software Engineer",
            string end = "Present",
            string start = "Jan 2020",
            string[]? bullets = null,
            string[]? skills = null,
            string company = "Acme") => new()
        {
            Role = role,
            Company = company,
            StartDate = start,
            EndDate = end,
            Bullets = (bullets ?? Array.Empty<string>()).ToList(),
            Skills = (skills ?? Array.Empty<string>()).ToList()
        };

        public static JobRequirements Req(
            string[]? skills = null,
            string[]? keywords = null,
            string seniority = "mid-level") => new()
        {
            TechnicalSkills = (skills ?? Array.Empty<string>()).ToList(),
            Keywords = (keywords ?? Array.Empty<string>()).ToList(),
            SeniorityLevel = seniority
        };

        public static CVMaster Master(
            Experience[]? exps = null,
            string[]? languages = null,
            string[]? tools = null) => new()
        {
            Header = new Header { FullName = "Test User", Email = "t@x.com", Phone = "+1 555 123 4567" },
            Experience = (exps ?? Array.Empty<Experience>()).ToList(),
            Skills = new Skills
            {
                Languages = (languages ?? Array.Empty<string>()).ToList(),
                Tools = (tools ?? Array.Empty<string>()).ToList()
            }
        };

        public static GeneratedCV Generated(
            Experience[]? exps = null,
            string[]? skills = null,
            string email = "t@x.com",
            string phone = "+1 555 123 4567",
            bool education = true) => new()
        {
            Header = new Header { FullName = "Test User", Email = email, Phone = phone },
            Education = education ? new List<Education> { new() { Degree = "B.S." } } : new List<Education>(),
            Experience = (exps ?? Array.Empty<Experience>()).ToList(),
            Skills = (skills ?? Array.Empty<string>()).ToList()
        };

        public static string Year(int yearsAgo) => (DateTime.Now.Year - yearsAgo).ToString();
    }
}
