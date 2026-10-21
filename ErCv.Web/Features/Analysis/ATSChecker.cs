using ErCv.Web.Features.Matching;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Analysis;

public class ATSChecker
{
    #region Public Methods

    public ATSScore Check(GeneratedCV cv, JobRequirements requirements, CVTemplate template)
    {
        var score = ATSConstants.MaxScore;
        var issues = new List<ATSIssue>();

        ValidateEmail(cv, issues, ref score);
        ValidatePhone(cv, issues, ref score);
        EvaluateKeywordDensity(cv, requirements, issues, ref score);
        ValidateSections(cv, issues, ref score);
        ValidateBulletLength(cv, issues, ref score);
        ValidateMetrics(cv, issues, ref score);
        ValidateTemplate(template, issues, ref score);

        score = Math.Max(0, score);

        return new ATSScore
        {
            Overall = score,
            Issues = issues
                .OrderByDescending(i => GetSeverityWeight(i.Severity))
                .ToList(),
            Recommendations = BuildRecommendations(issues, score),
            ColorClass = ResolveColor(score)
        };
    }

    #endregion

    #region Private Methods

    private void ValidateEmail(GeneratedCV cv, List<ATSIssue> issues, ref int score)
    {
        if (!IsValidEmail(cv.Header?.Email))
        {
            score -= ATSConstants.PenaltyInvalidEmail;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityMedium,
                Issue = ATSConstants.EmailInvalidIssue,
                Fix = ATSConstants.EmailInvalidFix
            });
        }
    }

    private void ValidatePhone(GeneratedCV cv, List<ATSIssue> issues, ref int score)
    {
        if (!IsValidPhone(cv.Header?.Phone))
        {
            score -= ATSConstants.PenaltyInvalidPhone;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityLow,
                Issue = ATSConstants.PhoneInvalidIssue,
                Fix = ATSConstants.PhoneInvalidFix
            });
        }
    }

    private void EvaluateKeywordDensity(
        GeneratedCV cv,
        JobRequirements requirements,
        List<ATSIssue> issues,
        ref int score)
    {
        var cvText = BuildCVText(cv);
        var totalKeywords = Math.Max(requirements.TechnicalSkills.Count, 1);

        var matchedKeywords = requirements.TechnicalSkills
            .Count(s => SkillNormalizer.ContainsSkill(cvText, s));

        var density = (double)matchedKeywords / totalKeywords;

        if (density < ATSConstants.DensityLowThreshold)
        {
            score -= ATSConstants.PenaltyLowDensityHigh;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityHigh,
                Issue = string.Format(ATSConstants.LowDensityIssue, density),
                Fix = ATSConstants.LowDensityFix
            });
        }
        else if (density < ATSConstants.DensityMediumThreshold)
        {
            score -= ATSConstants.PenaltyLowDensityMedium;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityMedium,
                Issue = string.Format(ATSConstants.MediumDensityIssue, density),
                Fix = ATSConstants.MediumDensityFix
            });
        }
    }

    private void ValidateSections(GeneratedCV cv, List<ATSIssue> issues, ref int score)
    {
        var hasExperience = cv.Experience?.Any() == true;
        var hasEducation = cv.Education?.Any() == true;
        var hasSkills = cv.Skills?.Any() == true;

        if (!hasExperience || !hasEducation || !hasSkills)
        {
            score -= ATSConstants.PenaltyMissingSections;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityHigh,
                Issue = ATSConstants.MissingSectionsIssue,
                Fix = ATSConstants.MissingSectionsFix
            });
        }
    }

    private void ValidateBulletLength(GeneratedCV cv, List<ATSIssue> issues, ref int score)
    {
        var shortBullets = cv.Experience?
            .SelectMany(e => e.Bullets ?? new List<string>())
            .Where(b => b.Length < ATSConstants.ShortBulletLength)
            .ToList() ?? new List<string>();

        if (shortBullets.Any())
        {
            score -= ATSConstants.PenaltyShortBullets;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityLow,
                Issue = string.Format(ATSConstants.ShortBulletCountIssue, shortBullets.Count, ATSConstants.ShortBulletLength),
                Fix = ATSConstants.ShortBulletFix
            });
        }
    }

    private void ValidateMetrics(GeneratedCV cv, List<ATSIssue> issues, ref int score)
    {
        var allBullets = cv.Experience?
            .SelectMany(e => e.Bullets ?? new List<string>())
            .ToList() ?? new List<string>();

        if (!allBullets.Any()) return;

        var bulletsWithMetrics = allBullets
            .Count(b => ATSConstants.Metrics().IsMatch(b));

        var ratio = (double)bulletsWithMetrics / allBullets.Count;

        if (bulletsWithMetrics == 0)
        {
            score -= ATSConstants.PenaltyNoMetrics;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityMedium,
                Issue = ATSConstants.NoMetricsIssue,
                Fix = ATSConstants.NoMetricsFix
            });
        }
        else if (ratio < ATSConstants.FewMetricsRatio)
        {
            score -= ATSConstants.PenaltyFewMetrics;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityLow,
                Issue = string.Format(ATSConstants.FewMetricsIssue, bulletsWithMetrics, allBullets.Count),
                Fix = ATSConstants.FewMetricsFix
            });
        }
    }

    private void ValidateTemplate(CVTemplate template, List<ATSIssue> issues, ref int score)
    {
        if (template == CVTemplate.Modern)
        {
            score -= ATSConstants.PenaltyModernTemplate;
            issues.Add(new ATSIssue
            {
                Severity = ATSConstants.SeverityMedium,
                Issue = ATSConstants.ModernTemplateIssue,
                Fix = ATSConstants.ModernTemplateFix
            });
        }
    }

    private bool IsValidEmail(string? email)
    {
        return !string.IsNullOrEmpty(email) &&
               ATSConstants.Email().IsMatch(email);
    }

    private bool IsValidPhone(string? phone)
    {
        return !string.IsNullOrEmpty(phone) && phone.Length >= ATSConstants.MinPhoneLength;
    }

    private string BuildCVText(GeneratedCV cv)
    {
        var parts = new List<string>();
        parts.AddRange(cv.Skills ?? new List<string>());

        foreach (var exp in cv.Experience ?? new List<Experience>())
        {
            parts.Add(exp.Role ?? "");
            parts.AddRange(exp.Bullets ?? new List<string>());
            parts.AddRange(exp.Skills ?? new List<string>());
        }

        return string.Join(" ", parts);
    }

    private int GetSeverityWeight(string severity) => severity switch
    {
        ATSConstants.SeverityCritical => ATSConstants.SeverityWeightCritical,
        ATSConstants.SeverityHigh => ATSConstants.SeverityWeightHigh,
        ATSConstants.SeverityMedium => ATSConstants.SeverityWeightMedium,
        ATSConstants.SeverityLow => ATSConstants.SeverityWeightLow,
        _ => 0
    };

    private string ResolveColor(int score)
    {
        if (score >= ATSConstants.ScoreGreen) return MatchScoreConstants.GreenClass;
        if (score >= ATSConstants.ScoreYellow) return MatchScoreConstants.YellowClass;
        return MatchScoreConstants.RedClass;
    }

    private List<string> BuildRecommendations(List<ATSIssue> issues, int score)
    {
        var recs = new List<string>();

        if (!issues.Any())
        {
            recs.Add(ATSConstants.OptimizedMessage);
            return recs;
        }

        recs.Add(string.Format(ATSConstants.RecommendationsHeader, issues.Count));
        foreach (var issue in issues.Take(ATSConstants.MaxRecommendations))
            recs.Add(string.Format(ATSConstants.RecommendationLine, issue.Issue, issue.Fix));

        return recs;
    }

    #endregion
}
