namespace ErCvApi.Models
{
    public class CVMaster
    {
        public Header Header { get; set; } = new();
        public List<Education> Education { get; set; } = new();
        public List<Experience> Experience { get; set; } = new();
        public Skills Skills { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
    }

    public class Header
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
        public string GitHub { get; set; } = string.Empty;
    }

    public class Education
    {
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string GraduationDate { get; set; } = string.Empty;
        public string GPA { get; set; } = string.Empty;
        public List<string> RelevantCourses { get; set; } = new();
        public string Honors { get; set; } = string.Empty;
    }

    public class Experience
    {
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public List<string> Bullets { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    public class Skills
    {
        public List<string> Languages { get; set; } = new();
        public List<string> Frameworks { get; set; } = new();
        public List<string> Databases { get; set; } = new();
        public List<string> Tools { get; set; } = new();
        public List<string> Cloud { get; set; } = new();
        public List<string> Practices { get; set; } = new();
    }

    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Technologies { get; set; } = new();
        public List<string> Highlights { get; set; } = new();
        public string Link { get; set; } = string.Empty;
    }
}
