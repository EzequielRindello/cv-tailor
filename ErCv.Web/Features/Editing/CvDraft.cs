using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Editing;

/// <summary>
/// The user's edits to a generated CV (EDIT-01): bullet text, what is included and in which order. With the profile
/// the CV was made from (EDIT-07) it also lists what the selection and the page fit left out, so the visitor can add it
/// back. The generated CV stays untouched, so every edit can be undone; <see cref="Build"/> gives the CV as edited.
/// </summary>
public class CvDraft
{
    #region Constants

    // Every experience keeps at least this many bullets, as in the page fit
    private const int MinIncludedBullets = 1;

    // A bullet that is not in the profile's list (it always is) sorts before the rest
    private const int NotInProfile = -1;

    #endregion

    #region Fields

    private readonly GeneratedCV _source;
    private readonly IReadOnlyList<string> _templateOrder;

    // Copies of the profile's parts the CV is made from; null profile means nothing is ever left out
    private readonly List<Experience> _profileExperiences = new();
    private readonly List<string> _profileSkills = new();
    private readonly List<Project> _profileProjects = new();

    private List<ExperienceDraft> _experiences = new();
    private List<Included<string>> _skills = new();
    private List<Included<Project>> _projects = new();
    private List<string> _sectionOrder = new();
    private bool _includeSummary;
    private bool _orderChanged;

    #endregion

    #region Constructors

    public CvDraft(GeneratedCV source, IReadOnlyList<string> templateOrder, CVMaster? profile = null)
    {
        _source = CvCloner.Clone(source);
        _templateOrder = templateOrder;

        // The draft never holds the caller's profile: it keeps its own copies
        if (profile != null)
        {
            _profileExperiences.AddRange(profile.Experience.Select(CvCloner.Clone));
            _profileSkills.AddRange(RelevanceScorer.FlattenSkills(profile.Skills)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            _profileProjects.AddRange(profile.Projects.Select(CvCloner.Clone));
        }
        Reset();
    }

    #endregion

    #region Properties

    public IReadOnlyList<ExperienceDraft> Experiences => _experiences;
    public IReadOnlyList<Included<string>> Skills => _skills;
    public IReadOnlyList<Included<Project>> Projects => _projects;
    public IReadOnlyList<string> SectionOrder => _sectionOrder;
    public bool HasSummary => !string.IsNullOrWhiteSpace(_source.Summary);
    public bool IncludeSummary => _includeSummary;

    /// <summary>Profile experiences that are not in the CV (computed, so it cannot drift from the draft).</summary>
    public IReadOnlyList<Experience> LeftOutExperiences => LeftOutExperienceIndexes().Select(i => _profileExperiences[i]).ToList();

    /// <summary>Profile skills that are not in the CV, case-insensitively.</summary>
    public IReadOnlyList<string> LeftOutSkills => _profileSkills
        .Where(s => !_skills.Any(d => string.Equals(d.Item, s, StringComparison.OrdinalIgnoreCase)))
        .ToList();

    /// <summary>Profile projects that are not in the CV.</summary>
    public IReadOnlyList<Project> LeftOutProjects => LeftOutProjectIndexes().Select(i => _profileProjects[i]).ToList();

    /// <summary>True when the visitor added something the generated CV did not have.</summary>
    public bool HasAdditions =>
        _experiences.Any(e => e.IsAdded || e.Bullets.Any(b => b.IsAddedBack))
        || _skills.Any(s => s.IsAddedBack)
        || _projects.Any(p => p.IsAddedBack);

    public bool IsChanged =>
        _orderChanged
        || HasAdditions
        || _includeSummary != HasSummary
        || _skills.Any(s => !s.IsIncluded)
        || _projects.Any(p => !p.IsIncluded)
        || _experiences.Any(e => e.Bullets.Any(b => !b.IsIncluded || b.IsEdited)
                                 || !e.Bullets.Select(b => b.Original).SequenceEqual(e.Source.Bullets));

    #endregion

    #region Public Methods

    public void Reset()
    {
        _experiences = _source.Experience
            .Select(e => new ExperienceDraft(e, e.Bullets.Select(b => new BulletDraft(b)).ToList()))
            .ToList();
        _skills = _source.Skills.Select(s => new Included<string>(s)).ToList();
        _projects = _source.Projects.Select(p => new Included<Project>(p)).ToList();
        _sectionOrder = DocumentRenderer.ResolveSectionOrder(_source.SectionOrder, _templateOrder);
        _includeSummary = HasSummary;
        _orderChanged = false;
    }

    /// <summary>
    /// The bullets of the profile's version of this experience that the CV does not have, with their position in
    /// the profile (the position decides where an added bullet goes).
    /// </summary>
    public IReadOnlyList<(int ProfileIndex, string Text)> LeftOutBullets(int experience)
    {
        var profile = ProfileExperienceOf(_experiences[experience]);
        if (profile == null) return Array.Empty<(int, string)>();

        var used = MatchBullets(_experiences[experience], profile, out _);
        return Enumerable.Range(0, profile.Bullets.Count)
            .Where(i => !used.Contains(i))
            .Select(i => (i, profile.Bullets[i]))
            .ToList();
    }

    /// <summary>False when it would leave the experience without bullets. A bullet that was added back leaves the CV instead.</summary>
    public bool ToggleBullet(int experience, int bullet)
    {
        var bullets = _experiences[experience].Bullets;
        var target = bullets[bullet];
        if (target.IsIncluded && bullets.Count(b => b.IsIncluded) <= MinIncludedBullets) return false;

        if (target.IsAddedBack) bullets.RemoveAt(bullet);
        else target.IsIncluded = !target.IsIncluded;
        return true;
    }

    /// <summary>
    /// Rewrites a bullet. False when the text is empty or too long; the profile's text again undoes the edit.
    /// </summary>
    public bool EditBullet(int experience, int bullet, string? text)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0 || trimmed.Length > ApiConstants.MaxEditedBulletLength) return false;

        _experiences[experience].Bullets[bullet].Text = trimmed;
        return true;
    }

    public void RevertBullet(int experience, int bullet)
    {
        var target = _experiences[experience].Bullets[bullet];
        target.Text = target.Original;
    }

    public void MoveBullet(int experience, int from, int to) => Move(_experiences[experience].Bullets, from, to);

    /// <summary>Adds a left-out bullet back, where the profile has it among the bullets that are in the CV.</summary>
    public bool AddBullet(int experience, int profileIndex)
    {
        var draft = _experiences[experience];
        var profile = ProfileExperienceOf(draft);
        if (profile == null || LeftOutBullets(experience).All(b => b.ProfileIndex != profileIndex)) return false;

        MatchBullets(draft, profile, out var profileIndexes);
        var at = profileIndexes.FindIndex(i => i > profileIndex);
        draft.Bullets.Insert(at < 0 ? draft.Bullets.Count : at, new BulletDraft(profile.Bullets[profileIndex]) { IsAddedBack = true });
        return true;
    }

    /// <summary>Adds a left-out experience back in the profile's order, with its first bullet (the rest can be added after).</summary>
    public bool AddExperience(int leftOutIndex)
    {
        var indexes = LeftOutExperienceIndexes();
        if (leftOutIndex < 0 || leftOutIndex >= indexes.Count) return false;

        var profile = _profileExperiences[indexes[leftOutIndex]];
        var first = profile.Bullets.Take(MinIncludedBullets).ToList();
        var added = new ExperienceDraft(CvCloner.Clone(profile), first.Select(b => new BulletDraft(b)).ToList()) { IsAdded = true };

        var order = indexes[leftOutIndex];
        var at = _experiences.FindIndex(e => ProfileIndexOf(e) > order);
        _experiences.Insert(at < 0 ? _experiences.Count : at, added);
        return true;
    }

    /// <summary>Takes an added experience out again (a generated one is only toggled by its bullets).</summary>
    public bool RemoveExperience(int experience)
    {
        if (!_experiences[experience].IsAdded) return false;

        _experiences.RemoveAt(experience);
        return true;
    }

    public bool AddSkill(string skill)
    {
        if (!LeftOutSkills.Contains(skill, StringComparer.OrdinalIgnoreCase)) return false;

        _skills.Add(new Included<string>(_profileSkills.First(s => string.Equals(s, skill, StringComparison.OrdinalIgnoreCase))) { IsAddedBack = true });
        return true;
    }

    public bool AddProject(int leftOutIndex)
    {
        var indexes = LeftOutProjectIndexes();
        if (leftOutIndex < 0 || leftOutIndex >= indexes.Count) return false;

        _projects.Add(new Included<Project>(CvCloner.Clone(_profileProjects[indexes[leftOutIndex]])) { IsAddedBack = true });
        return true;
    }

    public void ToggleSummary()
    {
        if (HasSummary) _includeSummary = !_includeSummary;
    }

    /// <summary>A skill that was added back leaves the CV instead of staying unchecked.</summary>
    public void ToggleSkill(int skill)
    {
        if (_skills[skill].IsAddedBack) _skills.RemoveAt(skill);
        else _skills[skill].IsIncluded = !_skills[skill].IsIncluded;
    }

    /// <summary>A project that was added back leaves the CV instead of staying unchecked.</summary>
    public void ToggleProject(int project)
    {
        if (_projects[project].IsAddedBack) _projects.RemoveAt(project);
        else _projects[project].IsIncluded = !_projects[project].IsIncluded;
    }

    public void MoveSection(int from, int to)
    {
        if (!Move(_sectionOrder, from, to)) return;
        _orderChanged = !_sectionOrder.SequenceEqual(DocumentRenderer.ResolveSectionOrder(_source.SectionOrder, _templateOrder));
    }

    /// <summary>The CV as edited: what the preview and the downloads show.</summary>
    public GeneratedCV Build()
    {
        var cv = CvCloner.Clone(_source);
        cv.Summary = _includeSummary ? _source.Summary : string.Empty;
        cv.Skills = _skills.Where(s => s.IsIncluded).Select(s => s.Item).ToList();
        cv.Projects = _projects.Where(p => p.IsIncluded).Select(p => CvCloner.Clone(p.Item)).ToList();
        cv.SectionOrder = _orderChanged ? _sectionOrder.ToList() : cv.SectionOrder;

        cv.Experience = _experiences.Select(e =>
        {
            var experience = CvCloner.Clone(e.Source);
            experience.Bullets = e.Bullets.Where(b => b.IsIncluded).Select(b => b.Text).ToList();
            return experience;
        }).ToList();

        // An added-back bullet that was not rewritten is the profile's own text: only rewrites are listed
        cv.EditedBullets = _experiences
            .SelectMany(e => e.Bullets)
            .Where(b => b.IsIncluded && b.IsEdited)
            .Select(b => new BulletEdit(b.Original, b.Text))
            .ToList();
        return cv;
    }

    #endregion

    #region Private Methods

    private static bool Move<T>(List<T> list, int from, int to)
    {
        if (from == to || from < 0 || to < 0 || from >= list.Count || to >= list.Count) return false;

        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
        return true;
    }

    // The profile's version of an experience, matched by role, company and dates
    private Experience? ProfileExperienceOf(ExperienceDraft draft)
    {
        var index = ProfileIndexOf(draft);
        return index < 0 ? null : _profileExperiences[index];
    }

    private int ProfileIndexOf(ExperienceDraft draft) =>
        _profileExperiences.FindIndex(p => ExperienceExplanation.KeyOf(p) == ExperienceExplanation.KeyOf(draft.Source));

    // Profile experiences with no counterpart in the draft; counted, so two identical entries are told apart
    private List<int> LeftOutExperienceIndexes()
    {
        var present = _experiences.Select(e => ExperienceExplanation.KeyOf(e.Source)).ToList();
        var left = new List<int>();
        for (var i = 0; i < _profileExperiences.Count; i++)
        {
            var key = ExperienceExplanation.KeyOf(_profileExperiences[i]);
            if (!present.Remove(key)) left.Add(i);
        }
        return left;
    }

    private List<int> LeftOutProjectIndexes()
    {
        var present = _projects.Select(p => ProjectKey(p.Item)).ToList();
        var left = new List<int>();
        for (var i = 0; i < _profileProjects.Count; i++)
            if (!present.Remove(ProjectKey(_profileProjects[i]))) left.Add(i);
        return left;
    }

    private static string ProjectKey(Project p) => $"{p.Name}|{p.Link}";

    // Matches each bullet of the draft to a bullet of the profile by its original text, each profile bullet once.
    // Gives the profile positions in use, and the position of each draft bullet.
    private static HashSet<int> MatchBullets(ExperienceDraft draft, Experience profile, out List<int> profileIndexes)
    {
        var used = new HashSet<int>();
        profileIndexes = new List<int>();
        foreach (var bullet in draft.Bullets)
        {
            var found = NotInProfile;
            for (var i = 0; i < profile.Bullets.Count; i++)
            {
                if (used.Contains(i) || profile.Bullets[i] != bullet.Original) continue;
                found = i;
                break;
            }
            if (found != NotInProfile) used.Add(found);
            profileIndexes.Add(found);
        }
        return used;
    }

    #endregion

    #region Nested Types

    public sealed class ExperienceDraft
    {
        public ExperienceDraft(Experience source, List<BulletDraft> bullets)
        {
            Source = source;
            Bullets = bullets;
        }

        public Experience Source { get; }
        public List<BulletDraft> Bullets { get; }

        /// <summary>The visitor added this experience back; the generated CV did not have it.</summary>
        public bool IsAdded { get; init; }
    }

    public sealed class BulletDraft
    {
        public BulletDraft(string original)
        {
            Original = original;
            Text = original;
        }

        public string Original { get; }
        public string Text { get; set; }
        public bool IsIncluded { get; set; } = true;
        public bool IsEdited => Text != Original;

        /// <summary>The visitor added this bullet back from the profile.</summary>
        public bool IsAddedBack { get; init; }
    }

    public sealed class Included<T>
    {
        public Included(T item) => Item = item;

        public T Item { get; }
        public bool IsIncluded { get; set; } = true;

        /// <summary>The visitor added this back from the profile.</summary>
        public bool IsAddedBack { get; init; }
    }

    #endregion
}
