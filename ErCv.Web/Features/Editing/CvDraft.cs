using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Editing;

/// <summary>
/// The user's edits to a generated CV (EDIT-01): bullet text, what is included and in which order.
/// The generated CV stays untouched, so every edit can be undone; <see cref="Build"/> gives the CV as edited.
/// </summary>
public class CvDraft
{
    #region Constants

    // Every experience keeps at least this many bullets, as in the page fit
    private const int MinIncludedBullets = 1;

    #endregion

    #region Fields

    private readonly GeneratedCV _source;
    private readonly IReadOnlyList<string> _templateOrder;
    private List<ExperienceDraft> _experiences = new();
    private List<Included<string>> _skills = new();
    private List<Included<Project>> _projects = new();
    private List<string> _sectionOrder = new();
    private bool _includeSummary;
    private bool _orderChanged;

    #endregion

    #region Constructors

    public CvDraft(GeneratedCV source, IReadOnlyList<string> templateOrder)
    {
        _source = CvCloner.Clone(source);
        _templateOrder = templateOrder;
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

    public bool IsChanged =>
        _orderChanged
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

    /// <summary>False when it would leave the experience without bullets.</summary>
    public bool ToggleBullet(int experience, int bullet)
    {
        var bullets = _experiences[experience].Bullets;
        var target = bullets[bullet];
        if (target.IsIncluded && bullets.Count(b => b.IsIncluded) <= MinIncludedBullets) return false;

        target.IsIncluded = !target.IsIncluded;
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

    public void ToggleSummary()
    {
        if (HasSummary) _includeSummary = !_includeSummary;
    }

    public void ToggleSkill(int skill) => _skills[skill].IsIncluded = !_skills[skill].IsIncluded;

    public void ToggleProject(int project) => _projects[project].IsIncluded = !_projects[project].IsIncluded;

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

        for (var i = 0; i < _experiences.Count; i++)
            cv.Experience[i].Bullets = _experiences[i].Bullets.Where(b => b.IsIncluded).Select(b => b.Text).ToList();

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
    }

    public sealed class Included<T>
    {
        public Included(T item) => Item = item;

        public T Item { get; }
        public bool IsIncluded { get; set; } = true;
    }

    #endregion
}
