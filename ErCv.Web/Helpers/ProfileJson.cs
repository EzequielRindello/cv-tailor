using System.Text.Json;
using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    /// <summary>Reads, writes and templates the profile JSON a visitor imports or exports (PROF-02).</summary>
    public static class ProfileJson
    {
        /// <summary>Largest file accepted on import. A real profile is a few KB.</summary>
        public const long MaxImportBytes = 512 * 1024;

        private static readonly JsonSerializerOptions ReadOptions = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static readonly JsonSerializerOptions WriteOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private static readonly ProfileValidator Validator = new();

        /// <summary>Parses and validates a profile. <c>Profile</c> is null when the JSON cannot be read at all.</summary>
        public static (CVMaster? Profile, List<ProfileIssue> Issues) Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return (null, new List<ProfileIssue> { new("file", "The file is empty.") });

            CVMaster? profile;
            try
            {
                profile = JsonSerializer.Deserialize<CVMaster>(json, ReadOptions);
            }
            catch (JsonException ex)
            {
                var where = ex.LineNumber.HasValue ? $" (line {ex.LineNumber + 1})" : string.Empty;
                return (null, new List<ProfileIssue> { new("file", $"Not a valid profile JSON{where}. Download the template to see the expected shape.") });
            }

            return (profile, Validator.Validate(profile).Issues);
        }

        public static string Serialize(CVMaster profile) => JsonSerializer.Serialize(profile, WriteOptions);

        /// <summary>A blank profile with one placeholder entry per section, so the expected shape is visible.</summary>
        public static string Template() => Serialize(new CVMaster
        {
            Header = new Header
            {
                FullName = "Your Name",
                Email = "you@example.com",
                Phone = "+1 555 000 0000",
                Location = "City, Country",
                LinkedIn = "linkedin.com/in/your-handle",
                GitHub = "github.com/your-handle"
            },
            Education = new List<Education>
            {
                new()
                {
                    Degree = "B.S. Your Degree", Institution = "Your University", Location = "City, Country",
                    GraduationDate = "May 2020", GPA = "", RelevantCourses = new List<string> { "Course name" }, Honors = ""
                }
            },
            Experience = new List<Experience>
            {
                new()
                {
                    Role = "Your Role", Company = "Your Company", Location = "City, Country",
                    StartDate = "Jan 2021", EndDate = "Present",
                    Bullets = new List<string> { "Describe one result, with a number if you can." },
                    Skills = new List<string> { "A skill used in this role" }
                }
            },
            Skills = new Skills
            {
                Languages = new List<string> { "C#" },
                Frameworks = new List<string>(),
                Databases = new List<string>(),
                Tools = new List<string>(),
                Cloud = new List<string>(),
                Practices = new List<string>()
            },
            Projects = new List<Project>
            {
                new()
                {
                    Name = "Project name", Description = "One line about it.",
                    Technologies = new List<string> { "C#" },
                    Highlights = new List<string> { "What it does or achieved." }, Link = ""
                }
            }
        });
    }
}
