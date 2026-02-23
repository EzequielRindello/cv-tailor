using ErCvApi.Constants;
using ErCvApi.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class ATSChecker
    {
        public ATSScore Check(GeneratedCV cv, JobRequirements requirements, CVTemplate template)
        {
            var score = 100;
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

        private void ValidatePhone(GeneratedCV cv, List<ATSIssue> issues,ref int score)
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

        private void EvaluateKeywordDensity(GeneratedCV cv, JobRequirements requirements, List<ATSIssue> issues, ref int score)
        {
            var cvText = BuildCVText(cv);

            var totalKeywords =
                Math.Max(requirements.TechnicalSkills.Count, 1);

            var matchedKeywords =
                requirements.TechnicalSkills.Count(s =>
                    cvText.Contains(s, StringComparison.OrdinalIgnoreCase));

            var density = (double)matchedKeywords / totalKeywords;

            if (density < ATSConstants.DensityLowThreshold)
            {
                score -= ATSConstants.PenaltyLowDensityHigh;
                issues.Add(new ATSIssue
                {
                    Severity = ATSConstants.SeverityHigh,
                    Issue = $"Low keyword density ({density:P0}) — ATS may reject the CV",
                    Fix = "Include more keywords from the job description in bullets and skills"
                });
            }
            else if (density < ATSConstants.DensityMediumThreshold)
            {
                score -= ATSConstants.PenaltyLowDensityMedium;
                issues.Add(new ATSIssue
                {
                    Severity = ATSConstants.SeverityMedium,
                    Issue = $"Medium keyword density ({density:P0})",
                    Fix = "Add 2–3 more keywords from the job description to improve ranking"
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
            var hasShortBullets = cv.Experience?
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Any(b => b.Length < ATSConstants.ShortBulletLength) == true;

            if (hasShortBullets)
            {
                score -= ATSConstants.PenaltyShortBullets;
                issues.Add(new ATSIssue
                {
                    Severity = ATSConstants.SeverityLow,
                    Issue = ATSConstants.ShortBulletIssue,
                    Fix = ATSConstants.ShortBulletFix
                });
            }
        }

        private void ValidateMetrics(GeneratedCV cv, List<ATSIssue> issues, ref int score)
        {
            var hasMetrics = cv.Experience?
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Any(b => Regex.IsMatch(b, ATSConstants.MetricsRegex)) == true;

            if (!hasMetrics)
            {
                score -= ATSConstants.PenaltyNoMetrics;
                issues.Add(new ATSIssue
                {
                    Severity = ATSConstants.SeverityMedium,
                    Issue = ATSConstants.NoMetricsIssue,
                    Fix = ATSConstants.NoMetricsFix
                });
            }
        }

        private void ValidateTemplate(CVTemplate template,List<ATSIssue> issues,ref int score)
        {
            if (template == CVTemplate.Modern)
            {
                score -= ATSConstants.PenaltyModernTemplate;
                issues.Add(new ATSIssue
                {
                    Severity = ATSConstants.SeverityLow,
                    Issue = ATSConstants.ModernTemplateIssue,
                    Fix = ATSConstants.ModernTemplateFix
                });
            }
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) &&
                   Regex.IsMatch(email, ATSConstants.EmailRegex);
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrEmpty(phone) &&
                   phone.Length >= 8;
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

        private int GetSeverityWeight(string severity)
        {
            if (severity == ATSConstants.SeverityCritical) return 4;
            if (severity == ATSConstants.SeverityHigh) return 3;
            if (severity == ATSConstants.SeverityMedium) return 2;
            if (severity == ATSConstants.SeverityLow) return 1;
            return 0;
        }

        private string ResolveColor(int score)
        {
            if (score >= ATSConstants.ScoreGreen) return "green";
            if (score >= ATSConstants.ScoreYellow) return "yellow";
            return "red";
        }

        private List<string> BuildRecommendations(List<ATSIssue> issues, int score)
        {
            var recs = new List<string>();

            if (!issues.Any())
            {
                recs.Add(ATSConstants.OptimizedMessage);
                return recs;
            }

            recs.Add($"{issues.Count} improvement point(s) detected:");

            foreach (var issue in issues.Take(4))
                recs.Add("• " + issue.Issue + " → " + issue.Fix);

            return recs;
        }
    }
}