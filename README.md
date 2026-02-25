# ER CV Generator

ASP.NET Core + React app that generates ATS-optimized, tailored resumes from a job description. It never fabricates content — it selects, reorders, and summarizes what's already in `cv-master.json`.

---

## Architecture

```
POST /api/cv/generate
  JobDescription (string) → JobDescriptionParser → JobRequirements
  JobRequirements + CVMaster → RelevanceScorer → scored experiences
  scored experiences → CVSelector → GeneratedCV (top 3 exp, top 3 bullets each, top 15 skills)
  CVMaster + requirements → SummaryGenerator → professional summary
  GeneratedCV → PageConstraintHelper → enforced 1 or 2 page CV
  GeneratedCV → DocumentRenderer → PDF (PuppeteerSharp) + DOCX (OpenXML)
  GeneratedCV + requirements → [MatchScoreCalculator, MissingSkillsAnalyzer, KeywordAnalyzer, ATSChecker] → AnalysisResult
  → returns { generatedCV, files[], matchScore, missingSkills, keywordAnalysis, atsScore }

POST /api/cv/regenerate
  GeneratedCV + BulletSelections (user-edited) → PageConstraintHelper → DocumentRenderer
  → returns { files[] }

GET /api/cv/download/{pdf|docx}
  → returns file from session
```

---

## Project Structure

```
er-cv-generator/
├── er-cv-api/
│   ├── Controllers/CVController.cs          # POST /generate, /regenerate, GET /download/:format
│   ├── Services/CVGeneratorService.cs       # orchestrates full flow
│   ├── Helpers/
│   │   ├── JobDescriptionParser.cs          # extracts TechnicalSkills, Keywords, SeniorityLevel
│   │   ├── RelevanceScorer.cs               # scores each experience: skills 40%, keywords 30%, recency 20%, seniority 10%
│   │   ├── CVSelector.cs                    # picks top experiences/bullets; deprioritizes non-tech roles for tech JDs
│   │   ├── SummaryGenerator.cs              # builds 2-3 line summary: role + tech YoE + skills + achievement + soft skills
│   │   ├── PageConstraintHelper.cs          # trims bullets → projects → experiences to enforce page limit
│   │   ├── DocumentRenderer.cs              # Mustache HTML → PDF via PuppeteerSharp, DOCX via OpenXML
│   │   ├── MatchScoreCalculator.cs          # overall % = skills(50%) + keywords(30%) + seniority(20%)
│   │   ├── MissingSkillsAnalyzer.cs         # required vs nice-to-have gaps with suggestions
│   │   ├── KeywordAnalyzer.cs               # frequency + present/missing per category
│   │   └── ATSChecker.cs                    # email, phone, keyword density, sections, bullet length, metrics, template
│   ├── Constants/
│   │   ├── JobDescriptionConstants.cs       # 250+ tech keywords, seniority keywords, negation words, stopwords
│   │   ├── KeywordAnalyzerConstants.cs      # soft skills, tech words for categorization, stopwords
│   │   ├── MatchScoreConstants.cs           # weights, seniority map, color thresholds
│   │   ├── MissingSkillsConstants.cs        # nice-to-have keyword list, limits, suggestion templates
│   │   ├── ATSConstants.cs                  # penalties, thresholds, issue messages
│   │   ├── DocumentConstants.cs             # section names, font sizes, language/soft-skill keyword lists
│   │   ├── ApiConstants.cs                  # session keys, error messages, content types
│   │   └── TemplatePaths.cs                 # file path constants for templates and data
│   ├── Models/
│   │   ├── CVMaster.cs                      # Header, Education, Experience, Skills, Project
│   │   ├── GenerateModels.cs                # GeneratedCV, JobRequirements, RelevanceScore
│   │   └── FeatureModels.cs                 # MatchScore, MissingSkillsAnalysis, KeywordAnalysis, ATSScore, enums
│   ├── Templates/
│   │   ├── harvard-template.html            # single-column, Times New Roman — best ATS compatibility
│   │   ├── minimalist-template.html         # single-column, Helvetica — clean modern look, ATS-safe
│   │   └── modern-template.html             # two-column grid — NOT recommended for ATS portals
│   └── Data/cv-master.json                  # master CV source — all experiences, skills, projects
│
└── er-cv-web/
    └── src/
        ├── App.js
        └── components/
            ├── JobDescriptionInput/
            ├── TemplateSelector/            # Harvard | Modern | Minimalist
            ├── FormatSelector/              # PDF | DOCX | both
            ├── PageOptions/                 # 1-page | 2-page toggle
            ├── GenerateButton/
            ├── MatchScore/                  # overall % + skills/keywords/seniority breakdown
            ├── MissingSkills/               # required + nice-to-have gaps
            ├── KeywordAnalysis/             # technical / soft / industry keyword presence
            ├── ATSScore/                    # score + issues + recommendations
            └── BulletEditor/               # select/deselect/reorder bullets → re-download
```

---

## Key Data Shapes

```jsonc
// cv-master.json — the only file to customize
{
  "header": { "fullName", "email", "phone", "location", "linkedin", "github" },
  "education": [{ "degree", "institution", "location", "graduationDate", "gpa", "honors" }],
  "experience": [{
    "role": "Junior Software Developer",
    "company": "Acme Corp",
    "location": "Remote",
    "startDate": "Jun 2024",
    "endDate": "Present",             // use "Present" for current role
    "bullets": ["Did X resulting in Y%..."],
    "skills": ["React", "PostgreSQL"] // used for relevance scoring — be complete
  }],
  "skills": {
    "languages": [], "frameworks": [], "databases": [],
    "tools": [], "cloud": [], "practices": []
  },
  "projects": [{ "name", "description", "technologies": [], "highlights": [], "link" }]
}

// POST /api/cv/generate — request
{ "jobDescription": "...", "formats": ["pdf", "docx"], "template": "Harvard", "allowTwoPages": false }

// POST /api/cv/generate — response
{
  "success": true,
  "files": [{ "format", "filename", "size" }],
  "generatedCV": { "header", "summary", "education", "experience", "skills", "projects" },
  "matchScore": { "overall", "skillsMatched", "totalSkills", "keywordsMatched", "seniorityMatch", "colorClass" },
  "missingSkills": { "requiredMissing": [], "niceToHaveMissing": [], "suggestions": [], "potentialScoreIncrease" },
  "keywordAnalysis": { "technicalKeywords": [], "softSkillKeywords": [], "presentInCV": [], "missingFromCV": [] },
  "atsScore": { "overall", "issues": [{ "severity", "issue", "fix" }], "recommendations": [], "colorClass" }
}

// POST /api/cv/regenerate — request
{
  "generatedCV": { ... },             // full GeneratedCV object from generate response
  "bulletSelections": { "exp0": [0,2], "exp1": [1] }, // indices of selected bullets per experience
  "formats": ["pdf"],
  "template": "Harvard",
  "allowTwoPages": false
}
```

---
### version 2.0.1
---
