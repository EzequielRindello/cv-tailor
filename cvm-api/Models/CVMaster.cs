namespace ErCvApi.Models
{
    public class CVMaster
    {
        public Header Header { get; set; }
        public List<Education> Education { get; set; }
        public List<Experience> Experience { get; set; }
        public Skills Skills { get; set; }
        public List<Project> Projects { get; set; }
    }

    public class Header
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Location { get; set; }
        public string LinkedIn { get; set; }
        public string GitHub { get; set; }
    }

    public class Education
    {
        public string Degree { get; set; }
        public string Institution { get; set; }
        public string Location { get; set; }
        public string GraduationDate { get; set; }
        public string GPA { get; set; }
        public List<string> RelevantCourses { get; set; }
        public string Honors { get; set; }
    }

    public class Experience
    {
        public string Role { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<string> Bullets { get; set; }
        public List<string> Skills { get; set; }
    }

    public class Skills
    {
        public List<string> Languages { get; set; }
        public List<string> Frameworks { get; set; }
        public List<string> Databases { get; set; }
        public List<string> Tools { get; set; }
        public List<string> Cloud { get; set; }
        public List<string> Practices { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Technologies { get; set; }
        public List<string> Highlights { get; set; }
        public string Link { get; set; }
    }
}
