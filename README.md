# ER CV Generator

> Automatically generate ATS-optimized, tailored resumes from any job description.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://reactjs.org/)

---

## What it does

You paste a job description. The system analyzes it, scores your experiences from a master CV file, selects the most relevant content, and generates a downloadable PDF and/or DOCX — tailored to that specific role, in seconds.

It never fabricates content. It only selects, reorders, and summarizes what's already in your `cv-master.json`.

---

## Features

- **Smart matching** — Scores each experience by skill overlap, keyword presence, recency, and seniority alignment
- **Auto-selected bullets** — Picks the 3 best bullets per experience based on relevance and quantifiable results
- **Dynamic summary** — Generates a 2–3 line professional summary with matched skills and a key achievement
- **Match Score** — Shows overall match % with breakdown (skills, keywords, seniority)
- **Missing Skills** — Lists required skills from the JD absent in your CV Master, with suggestions
- **Keyword Analysis** — Categorizes JD keywords (technical, soft skills) and shows which are present/missing
- **ATS Score** — Checks contact format, keyword density, section structure, and bullet quality
- **Bullet Editor** — After generation, select/deselect and reorder bullets before re-downloading
- **3 Templates** — Harvard (classic), Modern (two-column), Minimalist (clean)
- **1 or 2 page** — Strict page enforcement; optional 2-page mode for senior profiles
- **PDF + DOCX** — Both formats from the same generation

---

## How it works

```
Job Description
      │
      ▼
JobDescriptionParser        → extracts skills, keywords, seniority level
      │
      ▼
RelevanceScorer             → scores each experience (skill match 40%, keywords 30%,
      │                        recency 20%, seniority 10%)
      ▼
CVSelector                  → picks top 3 experiences, top 3 bullets each, top 15 skills
      │
      ▼
SummaryGenerator            → builds summary: role + years + skills + achievement + soft skills
      │
      ▼
PageConstraintHelper        → enforces 1-page (or 2-page) limit by trimming bullets/experiences
      │
      ▼
DocumentRenderer            → renders Mustache HTML template → PDF (PuppeteerSharp) + DOCX (OpenXML)
      │
      ▼
Analysis (returned with CV)
  ├─ MatchScoreCalculator   → overall % with skill/keyword/seniority breakdown
  ├─ MissingSkillsAnalyzer  → required vs nice-to-have gaps + suggestions
  ├─ KeywordAnalyzer        → frequency + presence per keyword category
  └─ ATSChecker             → contact format, keyword density, structure, bullets
```

---

## Project structure

```
er-cv-generator/
├── er-cv-api/                    # ASP.NET Core backend
│   ├── Controllers/
│   │   └── CVController.cs       # POST /generate, POST /regenerate, GET /download/:format
│   ├── Services/
│   │   └── CVGeneratorService.cs # Orchestrates the full flow
│   ├── Helpers/
│   │   ├── JobDescriptionParser.cs
│   │   ├── RelevanceScorer.cs
│   │   ├── CVSelector.cs
│   │   ├── SummaryGenerator.cs
│   │   ├── PageConstraintHelper.cs
│   │   ├── DocumentRenderer.cs
│   │   ├── MatchScoreCalculator.cs
│   │   ├── MissingSkillsAnalyzer.cs
│   │   ├── KeywordAnalyzer.cs
│   │   └── ATSChecker.cs
│   ├── Models/
│   │   ├── CVMaster.cs
│   │   ├── GenerateModels.cs
│   │   └── FeatureModels.cs
│   ├── Templates/
│   │   ├── harvard-template.html
│   │   ├── modern-template.html
│   │   └── minimalist-template.html
│   ├── Data/
│   │   └── cv-master.json        # ← customize this
│   └── Program.cs
│
└── er-cv-web/                    # React frontend
    └── src/
        ├── App.js
        └── components/
            ├── JobDescriptionInput/
            ├── TemplateSelector/
            ├── FormatSelector/
            ├── PageOptions/
            ├── GenerateButton/
            ├── MatchScore/
            ├── MissingSkills/
            ├── KeywordAnalysis/
            ├── ATSScore/
            └── BulletEditor/
```

---

## Setup

**Prerequisites:** .NET 8.0 SDK, Node.js 18+

```bash
# Backend
cd er-cv-api
dotnet restore && dotnet run
# → http://localhost:5000
# Note: first run downloads Chromium (~150MB, one-time)

# Frontend (new terminal)
cd er-cv-web
npm install && npm start
# → http://localhost:3000
```

Create `.env` in `er-cv-web/`:
```
REACT_APP_API_URL=http://localhost:5000/api
```

---

## Customizing your CV

Edit `er-cv-api/Data/cv-master.json`:

```json
{
  "header": {
    "fullName": "Your Name",
    "email": "you@example.com",
    "phone": "+1 234 567 8900",
    "location": "City, Country",
    "linkedin": "linkedin.com/in/yourprofile",
    "github": "github.com/yourusername"
  },
  "education": [...],
  "experience": [
    {
      "role": "Software Developer",
      "company": "Company Name",
      "location": "Remote",
      "startDate": "Jan 2023",
      "endDate": "Present",
      "bullets": ["Did X resulting in Y%...", "Built Z using A and B..."],
      "skills": ["React", "PostgreSQL", "Docker"]
    }
  ],
  "skills": {
    "languages": ["C#", "JavaScript"],
    "frameworks": ["React", "ASP.NET Core"],
    "databases": ["PostgreSQL"],
    "tools": ["Docker", "Git"],
    "cloud": ["AWS"],
    "practices": ["Agile", "Scrum"]
  },
  "projects": [...]
}
```

**Tips:**
- Include all your experiences — the system picks the best 3 for each JD
- Write bullets with numbers and percentages; they score higher
- Tag each experience with its `skills` array for better matching

---

## Stack

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core 8, C# |
| PDF | PuppeteerSharp (headless Chromium) |
| DOCX | DocumentFormat.OpenXml |
| Templating | Stubble (Mustache) |
| Frontend | React 18, React Bootstrap |
| HTTP | Axios |

---

*Version 2.0 · Last updated February 2026*
