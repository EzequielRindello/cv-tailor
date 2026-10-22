using ErCv.Web.Features.Profiles;

namespace ErCv.Tests.Features.Profiles;

// PROF-05: resumes laid out the way people write them, not the way this app prints them
[Trait("Category", "Unit")]
public class ResumeParserTests
{
    private const string WordResume = """
        Jane Doe
        Senior Backend Engineer
        jane.doe@mail.com | +1 (206) 555-0110 | Seattle, WA | linkedin.com/in/jane-doe | github.com/janedoe

        PROFESSIONAL SUMMARY
        Backend engineer with ten years of experience.

        WORK EXPERIENCE
        Acme Corp	Jan 2020 – Present
        Senior Backend Engineer	Remote · Seattle, WA
        • Led the migration of 40 services to Kubernetes
        • Cut p99 latency by 35% with Redis caching
        Globex, Inc.	06/2016 - 12/2019
        Software Developer	Portland, OR
        • Built the billing API in C# and PostgreSQL

        EDUCATION
        University of Washington	Seattle, WA
        B.S. Computer Science | GPA: 3.8	Jun 2016

        Skills & Interests
        Languages: C#, Go, SQL
        Databases: PostgreSQL, Redis
        Cloud: AWS, Kubernetes
        Interests: chess, cycling

        PROJECTS
        Ledger | github.com/janedoe/ledger
        Double-entry bookkeeping library.
        Technologies: C#, .NET
        """;

    [Fact]
    public void Reads_the_contact_details()
    {
        var header = ResumeParser.Parse(WordResume, linesWrap: false).Header;

        Assert.Equal("Jane Doe", header.FullName);
        Assert.Equal("jane.doe@mail.com", header.Email);
        Assert.Equal("+1 (206) 555-0110", header.Phone);
        Assert.Equal("Seattle, WA", header.Location);
        Assert.Equal("linkedin.com/in/jane-doe", header.LinkedIn);
        Assert.Equal("github.com/janedoe", header.GitHub);
    }

    [Fact]
    public void Company_and_dates_above_role_and_place_read_as_one_entry()
    {
        var experience = ResumeParser.Parse(WordResume, linesWrap: false).Experience;

        Assert.Equal(2, experience.Count);
        var acme = experience[0];
        Assert.Equal(("Senior Backend Engineer", "Acme Corp", "Remote · Seattle, WA"), (acme.Role, acme.Company, acme.Location));
        Assert.Equal(("Jan 2020", "Present"), (acme.StartDate, acme.EndDate));
        Assert.Equal(new[] { "Led the migration of 40 services to Kubernetes", "Cut p99 latency by 35% with Redis caching" }, acme.Bullets);

        // "Globex, Inc." keeps its comma: the place is in its own column
        var globex = experience[1];
        Assert.Equal(("Software Developer", "Globex, Inc.", "Portland, OR"), (globex.Role, globex.Company, globex.Location));
        Assert.Equal(("Jun 2016", "Dec 2019"), (globex.StartDate, globex.EndDate));
    }

    [Fact]
    public void Reads_education_with_its_gpa_and_date()
    {
        var education = Assert.Single(ResumeParser.Parse(WordResume, linesWrap: false).Education);

        Assert.Equal("University of Washington", education.Institution);
        Assert.Equal("Seattle, WA", education.Location);
        Assert.Equal("B.S. Computer Science", education.Degree);
        Assert.Equal("3.8", education.GPA);
        Assert.Equal("Jun 2016", education.GraduationDate);
    }

    [Fact]
    public void Skill_labels_pick_the_profile_list_and_interests_are_skipped()
    {
        var skills = ResumeParser.Parse(WordResume, linesWrap: false).Skills;

        Assert.Equal(new[] { "C#", "Go", "SQL" }, skills.Languages);
        Assert.Equal(new[] { "PostgreSQL", "Redis" }, skills.Databases);
        Assert.Equal(new[] { "AWS", "Kubernetes" }, skills.Cloud);
        Assert.DoesNotContain("chess", skills.Tools);
    }

    [Fact]
    public void Reads_a_project_with_its_link_description_and_stack()
    {
        var project = Assert.Single(ResumeParser.Parse(WordResume, linesWrap: false).Projects);

        Assert.Equal("Ledger", project.Name);
        Assert.Equal("github.com/janedoe/ledger", project.Link);
        Assert.Equal("Double-entry bookkeeping library.", project.Description);
        Assert.Equal(new[] { "C#", ".NET" }, project.Technologies);
    }

    [Fact]
    public void A_spanish_resume_reads_with_profile_dates()
    {
        var profile = ResumeParser.Parse("""
            María Gómez
            maria@correo.com · Rosario, Argentina
            EXPERIENCIA LABORAL
            Desarrolladora Backend en Globant | ene. 2021 – Actualidad
            - Diseñé APIs REST en .NET
            FORMACIÓN ACADÉMICA
            Universidad Nacional de Rosario, Rosario, Argentina | 2019
            Licenciatura en Sistemas
            HABILIDADES
            Lenguajes: C#, Python
            Herramientas: Git, Docker
            """, linesWrap: false);

        var job = Assert.Single(profile.Experience);
        Assert.Equal(("Desarrolladora Backend", "Globant"), (job.Role, job.Company));
        Assert.Equal(("Jan 2021", "Present"), (job.StartDate, job.EndDate));
        var school = Assert.Single(profile.Education);
        Assert.Equal(("Universidad Nacional de Rosario", "Rosario, Argentina", "Licenciatura en Sistemas", "2019"),
            (school.Institution, school.Location, school.Degree, school.GraduationDate));
        Assert.Equal(new[] { "C#", "Python" }, profile.Skills.Languages);
        Assert.Equal(new[] { "Git", "Docker" }, profile.Skills.Tools);
    }

    [Fact]
    public void Pdf_lines_that_wrapped_are_joined_back()
    {
        var profile = ResumeParser.Parse("""
            Alex Kim
            alex@kim.dev
            EXPERIENCE
            Mar 2022 – Present
            Senior Software Engineer
            Stripe, Remote
            Architected a reconciliation service in Go processing 2M+
            transactions a day with 99.99% uptime
            Led the zero-downtime migration from
            PostgreSQL 12 to 14
            """, linesWrap: true);

        Assert.Equal(new[]
        {
            "Architected a reconciliation service in Go processing 2M+ transactions a day with 99.99% uptime",
            "Led the zero-downtime migration from PostgreSQL 12 to 14"
        }, Assert.Single(profile.Experience).Bullets);
    }

    [Fact]
    public void The_wrap_mark_joins_a_line_that_goes_on_with_a_capital()
    {
        var mark = ResumeImportConstants.WrapMark;
        var profile = ResumeParser.Parse($"""
            Alex Kim
            EXPERIENCE
            Mar 2022 – Present
            Engineer
            Acme
            Built a portal for Thermo Fisher{mark}
            Scientific labs
            Ran the on-call rotation
            """, linesWrap: true);

        Assert.Equal(new[] { "Built a portal for Thermo Fisher Scientific labs", "Ran the on-call rotation" },
            Assert.Single(profile.Experience).Bullets);
    }

    [Fact]
    public void Letter_spaced_titles_and_names_are_read()
    {
        var profile = ResumeParser.Parse("""
            S A M  C A RT E R
            sam@carter.dev
            E X P E R I E N C E
            Engineer at Acme | 2020 – 2022
            """, linesWrap: true);

        Assert.Equal("Sam Carter", profile.Header.FullName);
        Assert.Equal(("Engineer", "Acme"), (profile.Experience[0].Role, profile.Experience[0].Company));
    }

    [Fact]
    public void An_entry_without_dates_is_found_by_its_role_and_company_lines()
    {
        var experience = ResumeParser.Parse("""
            EXPERIENCE
            Support Technician | 03/2016 – Dec 2018
            HelpDesk Co, Madrid, Spain
            • Solved 30 tickets a day
            Warehouse Assistant
            LogiMart
            • Managed inventory counts
            """, linesWrap: false).Experience;

        Assert.Equal(2, experience.Count);
        Assert.Equal(new[] { "Solved 30 tickets a day" }, experience[0].Bullets);
        Assert.Equal(("Warehouse Assistant", "LogiMart", "", ""), (experience[1].Role, experience[1].Company, experience[1].StartDate, experience[1].EndDate));
        Assert.Equal(new[] { "Managed inventory counts" }, experience[1].Bullets);
    }

    [Fact]
    public void Years_inside_a_bullet_are_not_an_entry()
    {
        var experience = ResumeParser.Parse("""
            EXPERIENCE
            Engineer | Acme | 2018 – 2023
            • Led the 2019 – 2021 rewrite of the payments platform that serves every merchant in the region
            """, linesWrap: false).Experience;

        Assert.Single(experience);
    }

    [Fact]
    public void A_project_line_with_its_stack_is_split()
    {
        var project = Assert.Single(ResumeParser.Parse("""
            PROJECTS
            Temple Trips  .NET · EF Core · PostgreSQL · React
            • Multi-tenant church administration platform
            """, linesWrap: false).Projects);

        Assert.Equal("Temple Trips", project.Name);
        Assert.Equal(new[] { ".NET", "EF Core", "PostgreSQL", "React" }, project.Technologies);
        Assert.Equal(new[] { "Multi-tenant church administration platform" }, project.Highlights);
    }

    [Fact]
    public void Commas_inside_parentheses_do_not_split_a_skill()
    {
        var skills = ResumeParser.Parse("""
            SKILLS
            Databases: PostgreSQL (indexing, raw SQL), MongoDB
            """, linesWrap: false).Skills;

        Assert.Equal(new[] { "PostgreSQL (indexing, raw SQL)", "MongoDB" }, skills.Databases);
    }

    [Fact]
    public void Text_without_sections_gives_an_empty_draft()
    {
        var profile = ResumeParser.Parse("Dear hiring manager, I am writing to apply.", linesWrap: false);

        Assert.Empty(profile.Experience);
        Assert.Empty(profile.Education);
        Assert.Empty(profile.Projects);
    }

    [Theory]
    [InlineData("Jan 2020", "Jan 2020")]
    [InlineData("January 2020", "Jan 2020")]
    [InlineData("Sept. 2018", "Sep 2018")]
    [InlineData("ene. 2021", "Jan 2021")]
    [InlineData("septiembre de 2019", "Sep 2019")]
    [InlineData("03/2016", "Mar 2016")]
    [InlineData("2020-11", "Nov 2020")]
    [InlineData("2019", "2019")]
    [InlineData("Present", "Present")]
    [InlineData("Current", "Present")]
    [InlineData("Actualidad", "Present")]
    [InlineData("Spring 2019", "Spring 2019")]
    public void Dates_are_written_in_the_profile_format(string date, string expected)
    {
        Assert.Equal(expected, ResumeParser.ProfileDate(date));
    }

    // Nothing is invented: every bullet read is text of the resume
    [Fact]
    public void Every_bullet_is_text_of_the_resume()
    {
        var bullets = ResumeParser.Parse(WordResume, linesWrap: false).Experience.SelectMany(e => e.Bullets);

        Assert.All(bullets, b => Assert.Contains(b, WordResume));
    }
}
