# ER CV Generator - Intelligent Resume Tailoring System

> **Automatically generate ATS-optimized, one-page professional resumes tailored to any job description**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://reactjs.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [How It Works](#how-it-works)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Installation & Setup](#installation--setup)
- [Usage Guide](#usage-guide)
- [Project Structure](#project-structure)
- [Core Components](#core-components)
- [Future Features](#future-features)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

**ER CV Generator** is an intelligent resume generation system that automatically adapts your master CV to match any job description. Instead of manually tailoring your resume for each application, this system uses NLP-based matching algorithms to:

- **Analyze** job descriptions to extract required skills, keywords, and seniority levels
- **Score** your experiences based on relevance to the specific role
- **Select** the most impactful bullets and experiences
- **Generate** a professional, ATS-optimized one-page resume
- **Ensure** strict adherence to one-page format without sacrificing quality

### The Problem It Solves

Manual resume tailoring is:
- ⏰ **Time-consuming** - 30-60 minutes per application
- 😓 **Tedious** - Copy-pasting and reformatting
- 🎲 **Inconsistent** - Human error in keyword optimization
- 📉 **Suboptimal** - Hard to objectively select best content

### The Solution

Automated, intelligent resume generation that:
- ⚡ **Generates in seconds** - From job description to download
- 🎯 **Maximizes relevance** - Algorithmic scoring ensures best match
- 📊 **ATS-optimized** - Keywords, formatting, and structure designed for Applicant Tracking Systems
- 🔒 **Never fabricates** - Only selects, reorders, and summarizes existing content

---

## ✨ Key Features

### Current Features (v1.0)

#### 🧠 Intelligent Job Description Analysis
- Extracts technical skills, keywords, and requirements
- Identifies seniority level (Junior, Mid, Senior, Lead)
- Detects years of experience mentioned
- Parses requirement bullets and nice-to-haves

#### 🎯 Smart Content Selection
- **Multi-criteria scoring algorithm:**
  - 40% - Technical skill matching
  - 30% - Keyword presence in bullets
  - 20% - Recency (current role prioritized)
  - 10% - Seniority level alignment
- Selects top 3 most relevant experiences
- Chooses 3 best bullets per experience
- Prioritizes quantifiable achievements (metrics, percentages)

#### 📝 Dynamic Professional Summary
- Automatically generates 2-3 line summary
- Includes years of experience
- Lists top 3-5 matched technical skills
- Highlights key achievement with metrics
- Incorporates soft skills from job description

#### 🎨 ATS-Optimized Formatting
- **Harvard-style professional template**
- Clean, parseable structure (no tables, columns, images)
- Times New Roman font (ATS-friendly)
- Proper heading hierarchy
- Clickable contact links (email, LinkedIn, GitHub)
- Categorized skills (Technical, Languages, Soft Skills)

#### 📄 Multi-Format Export
- **PDF** - Industry standard, preserves formatting
- **DOCX** - Editable Word document format
- Identical content in both formats

#### ⚖️ Strict One-Page Enforcement
- Iterative reduction algorithm
- Removes least relevant bullets first
- Never removes header or education
- Calculates approximate line count
- Guarantees single-page output

#### 🚫 Ethical Content Handling
- **Never invents** experiences, technologies, or dates
- **Only selects** from existing CV master data
- **Only reorders** based on relevance
- **Only summarizes** existing bullets

---

## 🔄 How It Works

### End-to-End Flow

```
┌─────────────────┐
│  User pastes    │
│ Job Description │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  1. JobDescriptionParser            │
│  - Extract technical skills         │
│  - Identify keywords                │
│  - Detect seniority level           │
│  - Parse requirements               │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  2. RelevanceScorer                 │
│  - Score each experience (0-100)    │
│  - Match skills with JD             │
│  - Calculate recency bonus          │
│  - Evaluate seniority fit           │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  3. CVSelector                      │
│  - Select top 3 experiences         │
│  - Choose best 3 bullets each       │
│  - Pick top 15 relevant skills      │
│  - Include projects if space allows │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  4. SummaryGenerator                │
│  - Build dynamic 2-3 line summary   │
│  - Include matched skills           │
│  - Highlight key achievement        │
│  - Add relevant soft skills         │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  5. PageConstraintHelper            │
│  - Calculate total line count       │
│  - Reduce bullets if exceeds limit  │
│  - Remove projects if needed        │
│  - Ensure exactly 1 page            │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  6. DocumentRenderer                │
│  - Render HTML with Mustache        │
│  - Generate PDF with PuppeteerSharp │
│  - Generate DOCX with OpenXML       │
│  - Return files to frontend         │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────┐
│  User downloads │
│  PDF and/or     │
│  DOCX files     │
└─────────────────┘
```

### Detailed Component Behavior

#### 1. Job Description Parsing
**Input:** Raw job description text

**Processing:**
```csharp
// Extract technical skills using regex and keyword matching
var technicalSkills = ExtractTechnicalSkills(jobDescription);
// Examples: "React", "PostgreSQL", "Docker", ".NET"

// Identify most frequent non-common words
var keywords = ExtractKeywords(jobDescription);
// Examples: "microservices", "agile", "cloud"

// Detect seniority from keywords and years mentioned
var seniority = ExtractSeniorityLevel(jobDescription);
// Examples: "senior", "junior", "lead"
```

**Output:** `JobRequirements` object with structured data

#### 2. Relevance Scoring
**Input:** CV Master experiences + Job Requirements

**Scoring Formula:**
```
Score = (Skill Match × 4) + (Keyword Presence × 2) + Recency Bonus + Seniority Match

Where:
- Skill Match: +4 points per matched technical skill
- Keyword Presence: +2 points per keyword in bullets
- Recency Bonus: 
  - Current role: +20 points
  - Ended 1 year ago: +18 points
  - Ended 2 years ago: +15 points
  - Ended 3+ years ago: +5-12 points
- Seniority Match:
  - Perfect match: +10 points
  - Close match (±1 level): +7 points
  - Partial match: +4 points
```

**Example:**
```
Experience: "Junior Software Developer at Primero Systems"
Job: "Full Stack Developer - React & .NET"

Scoring:
+ React mentioned in bullets: +4
+ .NET mentioned in bullets: +4
+ PostgreSQL mentioned: +4
+ Docker mentioned: +4
+ "API" keyword present: +2
+ "agile" keyword present: +2
+ Current role (2025-Present): +20
+ Seniority match (Junior vs Mid): +7
─────────────────────────────────
TOTAL: 47 points
```

#### 3. Content Selection
**Experience Selection:**
1. Sort all experiences by score (descending)
2. Take top 3 experiences
3. For each experience, score individual bullets
4. Select top 3 bullets per experience

**Bullet Scoring:**
```
Bullet Score = (Matched Skills × 10) + (Keywords × 3) + (Tech Skills × 8) + Metrics Bonus

Where:
- Matched Skills: Previously identified skills from experience
- Keywords: JD keywords present in bullet
- Tech Skills: Technical terms from JD present in bullet
- Metrics Bonus: +5 if bullet contains numbers (%, +, K, M)
```

**Skills Selection:**
- Flatten all skills from CV Master (languages, frameworks, tools, etc.)
- Score each skill based on JD presence and experience usage
- Select top 15 skills
- Categorize into: Technical, Languages, Soft Skills

#### 4. Summary Generation
**Components:**
1. **Role identification:** Based on most relevant experience and JD seniority
2. **Years calculation:** From earliest start date to present
3. **Top skills:** 3-5 most relevant technical skills from JD
4. **Achievement:** First quantifiable achievement from top experience
5. **Soft skills:** 2 soft skills mentioned in JD or demonstrated in bullets

**Template:**
```
{Role} with {Years}+ years of experience specializing in {Skills}. 
Proven track record of {Achievement}. Strong {Soft Skill 1} and {Soft Skill 2} skills.
```

**Example Output:**
```
Software Developer with 5+ years of experience specializing in React, ASP.NET Core, 
PostgreSQL, Docker, and REST API Design. Proven track record of improving user 
productivity by 40%. Strong agile methodologies and team collaboration skills.
```

#### 5. Page Constraint Enforcement
**Line Calculation:**
```
Total Lines = 
  Header (3) +
  Summary Section (4) +
  Education Section (1 + entries × 2) +
  Experience Section (1 + experiences × (2 + bullets)) +
  Skills Section (4) +
  Projects Section (1 + projects × 4)

Target: ≤ 48 lines (fits on 1 page with current margins/font)
```

**Reduction Strategy (if exceeds 48 lines):**
1. Remove last bullet from experience with most bullets
2. Repeat until under 48 lines or all experiences have 2 bullets
3. If still over, remove least relevant experience
4. If still over, remove all projects
5. Header and Education are NEVER removed

---

## 🏗️ Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         FRONTEND                            │
│  ┌────────────────────────────────────────────────────┐    │
│  │  React Application (Port 3000)                     │    │
│  │  - Job Description Input (textarea)                │    │
│  │  - Format Selection (PDF/DOCX checkboxes)          │    │
│  │  - Generate Button                                 │    │
│  │  - Success Screen with download                    │    │
│  └────────────────┬───────────────────────────────────┘    │
└───────────────────┼────────────────────────────────────────┘
                    │
                    │ HTTP POST /api/cv/generate
                    │ { jobDescription, formats: ["pdf", "docx"] }
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│                         BACKEND                             │
│  ┌────────────────────────────────────────────────────┐    │
│  │  ASP.NET Core Web API (Port 5000)                  │    │
│  │                                                     │    │
│  │  Controllers/                                      │    │
│  │  └─ CVController                                   │    │
│  │     └─ Generate(GenerateRequest)                   │    │
│  │     └─ Download(format)                            │    │
│  │                                                     │    │
│  │  Services/                                         │    │
│  │  └─ CVGeneratorService                             │    │
│  │     ├─ LoadCVMaster()                              │    │
│  │     ├─ GenerateCVAsync()                           │    │
│  │     └─ Orchestrates entire flow                    │    │
│  │                                                     │    │
│  │  Helpers/                                          │    │
│  │  ├─ JobDescriptionParser                           │    │
│  │  │  └─ Parse(jobDescription) → JobRequirements    │    │
│  │  │                                                  │    │
│  │  ├─ RelevanceScorer                                │    │
│  │  │  ├─ ScoreExperiences()                          │    │
│  │  │  └─ GetTopSkills()                              │    │
│  │  │                                                  │    │
│  │  ├─ CVSelector                                     │    │
│  │  │  └─ SelectAndAdaptCV()                          │    │
│  │  │                                                  │    │
│  │  ├─ SummaryGenerator                               │    │
│  │  │  └─ GenerateSummary()                           │    │
│  │  │                                                  │    │
│  │  ├─ PageConstraintHelper                           │    │
│  │  │  └─ EnforceOnePageConstraint()                  │    │
│  │  │                                                  │    │
│  │  └─ DocumentRenderer                               │    │
│  │     ├─ RenderPdfAsync()                            │    │
│  │     └─ RenderDocx()                                │    │
│  │                                                     │    │
│  │  Data/                                             │    │
│  │  └─ cv-master.json (Your complete CV data)        │    │
│  │                                                     │    │
│  │  Templates/                                        │    │
│  │  └─ harvard-template.html (Mustache template)     │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### Backend Architecture - Layered Pattern

```
┌────────────────────────────────────────────────────────┐
│                    CONTROLLERS                         │
│  - Handle HTTP requests/responses                      │
│  - Validate input                                      │
│  - Return file downloads or errors                     │
└────────────────┬───────────────────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────────────────┐
│                     SERVICES                           │
│  - Orchestrate business logic                          │
│  - Load CV Master from JSON                            │
│  - Coordinate between helpers                          │
│  - Handle file generation workflow                     │
└────────────────┬───────────────────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────────────────┐
│                     HELPERS                            │
│  - Single-responsibility utilities                      │
│  - JobDescriptionParser: NLP text analysis             │
│  - RelevanceScorer: Scoring algorithms                 │
│  - CVSelector: Content selection logic                 │
│  - SummaryGenerator: Dynamic summary creation          │
│  - PageConstraintHelper: Page fitting logic            │
│  - DocumentRenderer: PDF/DOCX rendering                │
└────────────────┬───────────────────────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────────────────────┐
│                     MODELS                             │
│  - CVMaster: Complete CV data structure                │
│  - GeneratedCV: Selected/adapted content               │
│  - JobRequirements: Parsed JD data                     │
│  - RelevanceScore: Experience with score               │
└────────────────────────────────────────────────────────┘
```

### Data Flow

```
CV Master JSON → Parse → In-Memory CVMaster Object
                            │
Job Description → Parse → JobRequirements Object
                            │
                            ▼
                 ┌──────────────────────┐
                 │  Scoring & Selection │
                 └──────────┬───────────┘
                            │
                            ▼
                    GeneratedCV Object
                    (Header, Summary, 
                     Education, Top 3 Experiences,
                     Top 15 Skills, Projects)
                            │
                            ▼
                 ┌──────────────────────┐
                 │  Template Rendering  │
                 │  (Mustache + HTML)   │
                 └──────────┬───────────┘
                            │
                    ┌───────┴────────┐
                    │                │
                    ▼                ▼
              PDF Generation    DOCX Generation
              (PuppeteerSharp)  (OpenXML SDK)
                    │                │
                    └───────┬────────┘
                            │
                            ▼
                  Binary Files (byte[])
                            │
                            ▼
                    Session Storage
                            │
                            ▼
                    HTTP Download
```

---

## 🛠️ Technology Stack

### Backend (.NET 8.0)
- **ASP.NET Core** - Web API framework
- **C#** - Primary language
- **PuppeteerSharp** (19.0.0) - PDF generation via headless Chromium
- **DocumentFormat.OpenXml** (3.0.2) - DOCX generation
- **Stubble.Core** (1.10.8) - Mustache templating engine
- **Swashbuckle** (6.5.0) - Swagger/OpenAPI documentation

### Frontend (React 18)
- **React** - UI library
- **React Bootstrap** (2.10.0) - UI components
- **Bootstrap** (5.3.2) - CSS framework
- **Axios** (1.6.5) - HTTP client
- **React Scripts** (5.0.1) - Build tooling

### Development Tools
- **Visual Studio 2022** / **VS Code** - IDEs
- **Postman** - API testing
- **Git** - Version control

### Libraries & Patterns
- **Dependency Injection** - ASP.NET Core built-in DI
- **Session Management** - For temporary file storage
- **CORS** - Cross-origin resource sharing for frontend/backend communication
- **Regex** - Pattern matching for JD parsing
- **LINQ** - Query operations for data processing

---

## 📦 Installation & Setup

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm** or **yarn** - Package manager

### Step 1: Clone Repository

```bash
git clone <repository-url>
cd er-cv-generator
```

### Step 2: Backend Setup

```bash
cd er-cv-api

# Restore NuGet packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run
```

Backend will start on **http://localhost:5000**

**Important:** On first run, PuppeteerSharp will download Chromium (~150MB). This is a one-time download and may take 1-2 minutes.

### Step 3: Frontend Setup

Open a new terminal:

```bash
cd er-cv-web

# Install dependencies
npm install

# Start development server
npm start
```

Frontend will start on **http://localhost:3000**

### Step 4: Verify Setup

1. Open browser to **http://localhost:3000**
2. You should see the CV Generator interface
3. Backend API docs available at **http://localhost:5000/swagger** (if Swagger enabled)

### Step 5: Customize Your CV Master

Edit `er-cv-api/Data/cv-master.json` with your information:

```json
{
  "header": {
    "fullName": "Your Name",
    "email": "your.email@example.com",
    "phone": "+1 234 567 8900",
    "location": "Your City, Country",
    "linkedin": "linkedin.com/in/yourprofile",
    "github": "github.com/yourusername"
  },
  "education": [...],
  "experience": [...],
  "skills": {...},
  "projects": [...]
}
```

**Tips for CV Master:**
- Include ALL your experiences (the system will select the best ones)
- Write detailed bullets with metrics and achievements
- Associate specific skills with each experience
- Use action verbs and quantifiable results
- The more data, the better the selection

---

## 📖 Usage Guide

### Basic Workflow

1. **Prepare Job Description**
   - Copy entire job description from job posting
   - Include requirements, responsibilities, and nice-to-haves
   - More detail = better matching

2. **Open Application**
   - Navigate to http://localhost:3000
   - Paste job description in textarea

3. **Select Format(s)**
   - Check **PDF** for standard resume format
   - Check **DOCX** if you need to edit later
   - Can select both

4. **Generate CV**
   - Click **"Generate CV"** button
   - Wait 2-5 seconds for processing
   - Files will download automatically

5. **Review & Use**
   - Open downloaded file(s)
   - Verify content is relevant
   - Use for job application

### Advanced Usage

#### Testing Different Job Types

Try these different job descriptions to see how the system adapts:

**Technical Role:**
```
Full Stack Developer - React & .NET
- 3+ years React experience
- Strong .NET/C# skills
- PostgreSQL database experience
- REST API design
- Agile/Scrum environment
```

**Operations Role:**
```
Operations Coordinator
- Process automation experience
- Data analysis skills
- Portfolio management (1000+ accounts)
- Training and mentoring abilities
- Remote work experience
```

**Manufacturing Role:**
```
Production Supervisor
- Manufacturing floor experience
- Quality control expertise
- ERP systems knowledge
- 5S methodology
- Team leadership
```

Each should produce a different CV with different experiences prioritized.

#### Customizing the Template

Edit `er-cv-api/Templates/harvard-template.html`:

**Change colors:**
```css
.section-title {
    border-bottom: 1.5pt solid #000; /* Change to #0066cc for blue */
}
```

**Change fonts:**
```css
body {
    font-family: 'Times New Roman', Times, serif; /* Change to 'Arial', sans-serif */
}
```

**Adjust spacing:**
```css
.section {
    margin-bottom: 6pt; /* Increase/decrease spacing */
}
```

After changes:
```bash
dotnet clean
dotnet build
dotnet run
```

---

## 📁 Project Structure

```
er-cv-generator/
│
├── er-cv-api/                          # Backend ASP.NET Core
│   ├── Controllers/
│   │   └── CVController.cs             # HTTP endpoints
│   │
│   ├── Services/
│   │   └── CVGeneratorService.cs       # Business logic orchestration
│   │
│   ├── Helpers/
│   │   ├── JobDescriptionParser.cs     # JD analysis & extraction
│   │   ├── RelevanceScorer.cs          # Experience scoring algorithms
│   │   ├── CVSelector.cs               # Content selection logic
│   │   ├── SummaryGenerator.cs         # Dynamic summary creation
│   │   ├── PageConstraintHelper.cs     # One-page enforcement
│   │   └── DocumentRenderer.cs         # PDF/DOCX generation
│   │
│   ├── Models/
│   │   ├── CVMaster.cs                 # CV Master data model
│   │   └── GenerateModels.cs           # Request/response models
│   │
│   ├── Templates/
│   │   └── harvard-template.html       # Mustache HTML template
│   │
│   ├── Data/
│   │   └── cv-master.json              # Your CV data (CUSTOMIZE THIS)
│   │
│   ├── Program.cs                      # Application entry point
│   ├── appsettings.json                # Configuration (ports, logging)
│   ├── ErCvApi.csproj                  # Project file & dependencies
│   └── .gitignore
│
├── er-cv-web/                          # Frontend React
│   ├── public/
│   │   └── index.html                  # HTML shell
│   │
│   ├── src/
│   │   ├── App.js                      # Main React component
│   │   ├── App.css                     # Styling
│   │   ├── index.js                    # React entry point
│   │   └── index.css                   # Global styles
│   │
│   ├── package.json                    # Dependencies & scripts
│   └── .gitignore
│
├── README.md                           # This file
└── QUICKSTART.md                       # Quick reference guide
```

---

## 🔧 Core Components

### 1. JobDescriptionParser

**Purpose:** Extract structured data from unstructured job description text

**Key Methods:**
- `Parse(string jobDescription)` → `JobRequirements`
- `ExtractTechnicalSkills(string text)` → `List<string>`
- `ExtractKeywords(string text)` → `List<string>`
- `ExtractSeniorityLevel(string text)` → `string`

**Algorithm:**
```csharp
// Technical skills extraction
foreach (var tech in commonTechKeywords)
{
    if (Regex.IsMatch(text, $@"\b{tech}\b", IgnoreCase))
        foundSkills.Add(tech);
}

// Years of experience detection
var match = Regex.Match(text, @"(\d+)\+?\s*(?:years?|yrs?)");
if (match.Success && int.TryParse(match.Groups[1].Value, out int years))
{
    if (years >= 7) return "senior";
    if (years >= 4) return "mid-level";
    if (years >= 1) return "junior";
}
```

**Input Example:**
```
Senior Full Stack Developer

Requirements:
- 5+ years with React and TypeScript
- Strong C# and .NET Core experience
- PostgreSQL database skills
- Docker containerization
- Agile/Scrum methodologies
```

**Output:**
```json
{
  "technicalSkills": ["React", "TypeScript", "C#", ".NET", "PostgreSQL", "Docker"],
  "keywords": ["full", "stack", "developer", "agile", "scrum", "containerization"],
  "seniorityLevel": "senior",
  "requiredExperience": [
    "5+ years with React and TypeScript",
    "Strong C# and .NET Core experience",
    "PostgreSQL database skills"
  ]
}
```

---

### 2. RelevanceScorer

**Purpose:** Calculate relevance score for each experience based on job requirements

**Key Methods:**
- `ScoreExperiences(experiences, requirements)` → `List<RelevanceScore>`
- `GetTopSkills(experiences, requirements, skills, maxSkills)` → `List<string>`

**Scoring Components:**

| Component | Weight | Max Points | Description |
|-----------|--------|------------|-------------|
| Skill Matching | 40% | 4 per skill | Exact match of technical skills |
| Keyword Presence | 30% | 2 per keyword | Keywords found in bullets |
| Recency | 20% | 20 max | How recent the experience is |
| Seniority Match | 10% | 10 max | Alignment with target level |

**Example Calculation:**
```
Experience: Junior Developer at TechCorp (2023-Present)
Job Requirement: Senior Full Stack Developer (React, .NET, PostgreSQL)

Skill Matching:
  React in skills → +4
  .NET in skills → +4
  PostgreSQL in skills → +4
  Total: +12

Keyword Presence:
  "API" in bullets → +2
  "agile" in bullets → +2
  Total: +4

Recency:
  Current role (Present) → +20

Seniority:
  Junior vs Senior (2 levels apart) → +4

FINAL SCORE: 40 points
```

---

### 3. CVSelector

**Purpose:** Select and adapt the most relevant content from CV Master

**Key Methods:**
- `SelectAndAdaptCV(cvMaster, requirements)` → `GeneratedCV`
- `SelectExperiencesForOnePage(scoredExperiences)` → `List<Experience>`
- `AdaptExperience(experience, matchedSkills)` → `Experience`

**Selection Logic:**

**Experiences:**
1. Sort by relevance score (highest first)
2. Take top 3 experiences
3. For each experience:
   - Score individual bullets
   - Select top 3 bullets
   - Maintain original chronological order within bullets

**Skills:**
1. Flatten all skills from CV Master
2. Score based on:
   - Presence in job requirements (+10)
   - Usage in selected experiences (+3 per experience)
3. Sort by score, take top 15
4. Categorize into Technical, Languages, Soft Skills

**Projects:**
1. Score projects based on technology overlap with JD
2. Select highest scoring project
3. Include only if space allows (checked by PageConstraintHelper)

---

### 4. SummaryGenerator

**Purpose:** Create compelling, keyword-rich professional summary

**Key Methods:**
- `GenerateSummary(cvMaster, requirements, experiences, skills)` → `string`

**Summary Template:**
```
{PrimaryRole} with {Years}+ years of experience specializing in {TopSkills}. 
Proven track record of {KeyAchievement}. Strong {SoftSkill1} and {SoftSkill2} skills.
```

**Generation Steps:**

1. **Determine Primary Role:**
   - Check JD seniority keywords (senior, lead, junior)
   - Fall back to most recent experience role
   - Generalize if needed ("Software Developer" vs "Junior React Developer")

2. **Calculate Years:**
   - Find earliest start date across all experiences
   - Calculate `current_year - earliest_year`

3. **Select Top Skills (3-5):**
   - Filter skills present in job requirements
   - Order by relevance score
   - Format as comma-separated list with "and" before last

4. **Extract Achievement:**
   - Search bullets for patterns: "improving X by Y%", "increasing Z by N%"
   - Extract first match with quantifiable metric
   - Format: "improving user productivity by 40%"

5. **Identify Soft Skills (2):**
   - Check JD keywords for: agile, scrum, leadership, communication, mentoring
   - Match against bullet content
   - Select top 2

**Character Limit:** 300 characters (fits in 3 lines at 10pt font)

---

### 5. PageConstraintHelper

**Purpose:** Ensure generated CV fits exactly on one page

**Constants:**
```csharp
private const int MaxLinesPerPage = 48;      // Based on margins and font size
private const int HeaderLines = 3;
private const int SummaryLines = 3;
private const int EducationLinesPerEntry = 2;
private const int ExperienceTitleLines = 2;
private const int BulletLines = 1;
private const int SkillsSectionLines = 3;
private const int ProjectLines = 4;
```

**Calculation:**
```csharp
int totalLines = 
    HeaderLines +
    (hasSummary ? SummaryLines + 1 : 0) +
    (1 + education.Count * EducationLinesPerEntry) +
    (1 + experiences.Sum(e => ExperienceTitleLines + e.Bullets.Count)) +
    SkillsSectionLines +
    (projects.Any() ? 1 + projects.Count * ProjectLines : 0);
```

**Reduction Strategy:**
```csharp
while (totalLines > MaxLinesPerPage)
{
    // 1. Remove bullets (starting from experience with most bullets)
    if (experiences.Any(e => e.Bullets.Count > 2))
    {
        var exp = experiences.OrderByDescending(e => e.Bullets.Count).First();
        exp.Bullets.RemoveAt(exp.Bullets.Count - 1);
        totalLines--;
        continue;
    }
    
    // 2. Remove last experience (if more than 2 remain)
    if (experiences.Count > 2)
    {
        var lastExp = experiences.Last();
        totalLines -= (ExperienceTitleLines + lastExp.Bullets.Count);
        experiences.RemoveAt(experiences.Count - 1);
        continue;
    }
    
    // 3. Remove all projects
    if (projects.Any())
    {
        totalLines -= (1 + projects.Count * ProjectLines);
        projects.Clear();
        continue;
    }
    
    // 4. Reduce skills (if more than 8)
    if (skills.Count > 8)
    {
        skills.RemoveAt(skills.Count - 1);
        if (skills.Count % 5 == 0) totalLines--; // Assumes wrapping every 5 skills
        continue;
    }
    
    break; // Cannot reduce further
}
```

**Priorities (Never Removed):**
1. Header (contact info)
2. Education
3. At least 2 experiences with 2 bullets each

---

### 6. DocumentRenderer

**Purpose:** Generate PDF and DOCX files from selected content

**Key Methods:**
- `RenderPdfAsync(GeneratedCV cv)` → `byte[]`
- `RenderDocx(GeneratedCV cv)` → `byte[]`

**PDF Generation Flow:**
```csharp
// 1. Render HTML with Mustache
var html = RenderHtml(cv);

// 2. Launch headless Chromium
var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync(); // First run only
var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

// 3. Create new page and set content
var page = await browser.NewPageAsync();
await page.SetContentAsync(html);

// 4. Generate PDF with specific settings
var pdfBytes = await page.PdfAsync(new PdfOptions
{
    Format = PaperFormat.Letter,
    MarginOptions = new MarginOptions
    {
        Top = "0.65in",
        Bottom = "0.55in",
        Left = "0.7in",
        Right = "0.7in"
    },
    PrintBackground = true
});

// 5. Close browser and return bytes
await browser.CloseAsync();
return pdfBytes;
```

**DOCX Generation Flow:**
```csharp
using var memoryStream = new MemoryStream();
using (var document = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
{
    var mainPart = document.AddMainDocumentPart();
    mainPart.Document = new Document();
    var body = mainPart.Document.AppendChild(new Body());

    // Add header
    AddHeader(body, cv.Header);
    
    // Add summary
    if (!string.IsNullOrEmpty(cv.Summary))
        AddSummary(body, cv.Summary);
    
    // Add education
    AddSectionTitle(body, "EDUCATION");
    foreach (var edu in cv.Education)
        AddEducation(body, edu);
    
    // Add experience
    AddSectionTitle(body, "EXPERIENCE");
    foreach (var exp in cv.Experience)
        AddExperience(body, exp);
    
    // Add skills
    AddSectionTitle(body, "SKILLS");
    AddSkills(body, cv.Skills);
    
    // Add projects
    if (cv.Projects.Any())
    {
        AddSectionTitle(body, "PROJECTS");
        foreach (var project in cv.Projects)
            AddProject(body, project);
    }

    mainPart.Document.Save();
}

return memoryStream.ToArray();
```

---

## 🚀 Future Features

### Priority 1: Quick Wins (High Impact, Low Effort)

---

#### FEATURE-001: CV Preview Before Download

**Epic:** User Experience Enhancement  
**Story Points:** 5  
**Priority:** P0 - Critical  
**Labels:** `enhancement`, `ux`, `frontend`

**User Story:**
```
AS A user generating a CV
I WANT TO preview the CV before downloading
SO THAT I can verify the content is correct without downloading
```

**Acceptance Criteria:**
- [ ] After clicking "Generate CV", user sees preview in browser
- [ ] Preview shows exact formatting as PDF will have
- [ ] Preview is rendered in an iframe or modal
- [ ] User can choose to download or regenerate after preview
- [ ] Preview maintains responsive design on mobile/tablet

**Technical Implementation:**
```javascript
// Backend: Return HTML in addition to PDF/DOCX
POST /api/cv/generate
Response: {
  html: "<html>...</html>",
  files: [
    { format: "pdf", size: 123456 },
    { format: "docx", size: 67890 }
  ]
}

// Frontend: Render HTML in iframe
<iframe 
  srcDoc={previewHtml} 
  width="8.5in" 
  height="11in"
  style={{ border: '1px solid #ccc' }}
/>
```

**Design Notes:**
- Use modal overlay for preview (not inline)
- Add "Download PDF" and "Download DOCX" buttons below preview
- Add "Generate New CV" button to go back
- Consider adding zoom controls for preview

**Dependencies:** None

**Risks:**
- HTML rendering may differ slightly from PDF (acceptable)
- Large HTML may cause performance issues (mitigate with lazy loading)

---

#### FEATURE-002: Match Score Visualization

**Epic:** Feedback & Transparency  
**Story Points:** 3  
**Priority:** P0 - Critical  
**Labels:** `enhancement`, `analytics`, `frontend`

**User Story:**
```
AS A user submitting a job description
I WANT TO see how well my CV matches the job
SO THAT I understand the quality of the match and can improve
```

**Acceptance Criteria:**
- [ ] After generation, user sees match score (0-100%)
- [ ] Score is color-coded: Green (80-100%), Yellow (60-79%), Red (<60%)
- [ ] Score breakdown shows:
  - Technical skills matched: X/Y
  - Keywords present: X/Y
  - Experience level match: ✓/✗
- [ ] Visual indicator (progress bar or radial chart)
- [ ] Score is calculated before generation (fast feedback)

**Technical Implementation:**
```csharp
// New method in RelevanceScorer
public MatchScore CalculateMatchScore(CVMaster cv, JobRequirements req)
{
    var totalSkills = req.TechnicalSkills.Count;
    var matchedSkills = cv.Skills.Languages
        .Concat(cv.Skills.Frameworks)
        .Concat(cv.Skills.Tools)
        .Where(s => req.TechnicalSkills.Contains(s, StringComparer.OrdinalIgnoreCase))
        .Count();
    
    var skillPercentage = (double)matchedSkills / totalSkills * 100;
    
    var totalKeywords = req.Keywords.Take(15).Count();
    var matchedKeywords = cv.Experience
        .SelectMany(e => e.Bullets)
        .Count(b => req.Keywords.Any(k => b.Contains(k, StringComparison.OrdinalIgnoreCase)));
    
    var keywordPercentage = (double)matchedKeywords / (totalKeywords * 3) * 100; // 3 bullets avg
    
    var seniorityMatch = DetermineSeniorityMatch(cv, req);
    
    var overallScore = (skillPercentage * 0.5) + (keywordPercentage * 0.3) + (seniorityMatch * 0.2);
    
    return new MatchScore
    {
        Overall = Math.Round(overallScore, 0),
        SkillsMatched = matchedSkills,
        TotalSkills = totalSkills,
        KeywordsMatched = matchedKeywords,
        TotalKeywords = totalKeywords,
        SeniorityMatch = seniorityMatch > 0
    };
}
```

**UI Mockup:**
```
┌─────────────────────────────────────┐
│  Match Score: 85%        🟢         │
│  ████████████████░░░░               │
│                                     │
│  ✓ Technical Skills: 8/10 matched  │
│  ✓ Keywords: 12/15 present         │
│  ✓ Seniority Level: Match          │
│                                     │
│  [Generate CV] [Improve Match]     │
└─────────────────────────────────────┘
```

**Dependencies:** None

---

#### FEATURE-003: Missing Skills Detection

**Epic:** Feedback & Transparency  
**Story Points:** 3  
**Priority:** P1 - High  
**Labels:** `enhancement`, `analytics`, `backend`

**User Story:**
```
AS A user with a low match score
I WANT TO see which skills I'm missing
SO THAT I can add them to my CV Master or know what to learn
```

**Acceptance Criteria:**
- [ ] System identifies skills in JD not present in CV Master
- [ ] Displays "Missing Skills" section with list
- [ ] Categorizes as "Required" vs "Nice to Have"
- [ ] Shows actionable suggestions:
  - "Add to cv-master.json if you have this skill"
  - "Consider learning this skill for better match"
- [ ] Max 10 missing skills shown (prioritized by frequency in JD)

**Technical Implementation:**
```csharp
public class MissingSkillsAnalysis
{
    public List<string> RequiredMissing { get; set; }
    public List<string> NiceToHaveMissing { get; set; }
    public List<string> Suggestions { get; set; }
}

public MissingSkillsAnalysis AnalyzeMissingSkills(CVMaster cv, JobRequirements req)
{
    var allCVSkills = cv.Skills.Languages
        .Concat(cv.Skills.Frameworks)
        .Concat(cv.Skills.Databases)
        .Concat(cv.Skills.Tools)
        .Concat(cv.Skills.Cloud)
        .Select(s => s.ToLower())
        .ToHashSet();
    
    var requiredMissing = req.TechnicalSkills
        .Where(s => !allCVSkills.Contains(s.ToLower()))
        .ToList();
    
    var suggestions = requiredMissing.Select(skill => 
        $"Add '{skill}' to cv-master.json if you have experience with it"
    ).ToList();
    
    return new MissingSkillsAnalysis
    {
        RequiredMissing = requiredMissing,
        Suggestions = suggestions
    };
}
```

**UI Display:**
```
⚠️ Missing Skills (3)

Required:
  • Docker - Add to cv-master.json if you have experience
  • Kubernetes - Consider highlighting in bullets if used
  • GraphQL - Mentioned in JD but not in your CV

💡 Tip: Adding these skills could increase your match score to 95%
```

**Dependencies:** FEATURE-002 (Match Score)

---

#### FEATURE-004: CV Generation History (Local Storage)

**Epic:** User Experience Enhancement  
**Story Points:** 5  
**Priority:** P1 - High  
**Labels:** `enhancement`, `frontend`, `persistence`

**User Story:**
```
AS A user who generates multiple CVs
I WANT TO access previously generated CVs
SO THAT I can re-download or reference past applications
```

**Acceptance Criteria:**
- [ ] Last 10 generated CVs stored in browser localStorage
- [ ] Each entry shows:
  - Job title (extracted from JD)
  - Company name (if detected)
  - Generation date/time
  - Match score
  - Quick re-download button
- [ ] "History" button in header shows modal with list
- [ ] User can clear history
- [ ] User can regenerate with same JD (pre-fills textarea)

**Technical Implementation:**
```javascript
// Store in localStorage after generation
const cvHistory = JSON.parse(localStorage.getItem('cv-history') || '[]');

cvHistory.unshift({
  id: Date.now(),
  jobTitle: extractedTitle,
  company: extractedCompany,
  generatedAt: new Date().toISOString(),
  matchScore: 85,
  jobDescription: jobDescriptionText,
  formats: ['pdf', 'docx'],
  // Store base64 or blob URL for quick re-download
  pdfBlob: pdfBlobUrl,
  docxBlob: docxBlobUrl
});

// Keep only last 10
if (cvHistory.length > 10) cvHistory.length = 10;

localStorage.setItem('cv-history', JSON.stringify(cvHistory));
```

**UI Mockup:**
```
┌─────────────────────────────────────────┐
│  CV Generation History                  │
│                                         │
│  1. Full Stack Developer @ TechCorp     │
│     Feb 16, 2025 • Match: 85%          │
│     [📄 PDF] [📝 DOCX] [🔄 Regenerate] │
│                                         │
│  2. Senior Backend Engineer @ Startup  │
│     Feb 15, 2025 • Match: 78%          │
│     [📄 PDF] [📝 DOCX] [🔄 Regenerate] │
│                                         │
│  ...                                    │
│                                         │
│  [Clear History]                        │
└─────────────────────────────────────────┘
```

**Dependencies:** None

**Data Retention:**
- Client-side only (localStorage)
- No backend persistence in Phase 1
- Consider IndexedDB for larger storage in future

---

### Priority 2: Game Changers (High Impact, Medium Effort)

---

#### FEATURE-005: Inline Bullet Editing

**Epic:** Content Control  
**Story Points:** 8  
**Priority:** P1 - High  
**Labels:** `enhancement`, `ux`, `frontend`, `backend`

**User Story:**
```
AS A user previewing my CV
I WANT TO manually select which bullets to include
SO THAT I have final control over the generated content
```

**Acceptance Criteria:**
- [ ] Preview shows checkboxes next to each bullet
- [ ] User can uncheck bullets to exclude them
- [ ] User can reorder bullets via drag-and-drop
- [ ] "Regenerate" button creates new CV with selections
- [ ] Page constraint still enforced (warning if exceeds 1 page)
- [ ] Selections persist in session (not lost on regenerate)

**Technical Implementation:**

**Frontend:**
```javascript
const [selectedBullets, setSelectedBullets] = useState({
  exp1: [true, true, true],
  exp2: [true, true, true],
  exp3: [true, true, true]
});

const toggleBullet = (expId, bulletIndex) => {
  setSelectedBullets(prev => ({
    ...prev,
    [expId]: prev[expId].map((val, i) => 
      i === bulletIndex ? !val : val
    )
  }));
};

// Send to backend
POST /api/cv/regenerate
{
  generatedCV: {...},
  bulletSelections: {
    "exp1": [0, 2], // Include bullets at index 0 and 2
    "exp2": [1, 2],
    "exp3": [0, 1, 2]
  }
}
```

**Backend:**
```csharp
[HttpPost("regenerate")]
public async Task<IActionResult> Regenerate([FromBody] RegenerateRequest request)
{
    // Apply user's bullet selections
    foreach (var exp in request.GeneratedCV.Experience)
    {
        var expId = $"exp{request.GeneratedCV.Experience.IndexOf(exp)}";
        if (request.BulletSelections.ContainsKey(expId))
        {
            var selectedIndices = request.BulletSelections[expId];
            exp.Bullets = selectedIndices
                .Select(i => exp.Bullets[i])
                .ToList();
        }
    }
    
    // Re-check page constraints
    var constrainedCV = _pageConstraint.EnforceOnePageConstraint(request.GeneratedCV);
    
    // Generate documents
    var documents = await _renderer.RenderDocumentsAsync(constrainedCV);
    
    return Ok(documents);
}
```

**UI Design:**
- Drag handles: ⋮⋮ icon before each bullet
- Checkbox: □ or ☑
- Disabled state: If unchecking would make experience empty
- Warning: "⚠️ Unchecking these bullets may remove entire experience"

**Dependencies:** FEATURE-001 (Preview)

---

#### FEATURE-006: Multiple Resume Templates

**Epic:** Customization  
**Story Points:** 13  
**Priority:** P2 - Medium  
**Labels:** `enhancement`, `design`, `backend`

**User Story:**
```
AS A user applying to different types of companies
I WANT TO choose from multiple CV templates
SO THAT I can match the company culture (formal vs startup)
```

**Acceptance Criteria:**
- [ ] User can select template before generation
- [ ] At least 3 templates available:
  - Harvard (Classic) - Current formal template
  - Modern - Two-column layout with color accents
  - Minimalist - Ultra-clean, no borders, lots of whitespace
- [ ] Template preview thumbnails shown
- [ ] All templates respect one-page constraint
- [ ] Same data model works for all templates

**Template Specifications:**

**1. Harvard (Classic)** - Already implemented
- Single column
- Times New Roman
- Black & white
- Formal sections with underlines

**2. Modern Template**
```html
<!-- Two-column layout -->
<div class="container">
  <div class="left-column">
    <!-- Skills, Education, Contact -->
  </div>
  <div class="right-column">
    <!-- Summary, Experience, Projects -->
  </div>
</div>

<!-- Color scheme -->
:root {
  --primary-color: #2563eb; /* Blue */
  --accent-color: #dbeafe;
  --text-color: #1f2937;
}
```

**3. Minimalist Template**
```html
<!-- Ultra-clean, lots of spacing -->
body {
  font-family: 'Helvetica Neue', Arial, sans-serif;
  font-size: 11pt;
  line-height: 1.6;
  color: #333;
}

.section-title {
  border-bottom: none; /* No underlines */
  margin-bottom: 12pt;
  font-weight: 300;
  letter-spacing: 2pt;
}
```

**Technical Implementation:**
```csharp
public enum CVTemplate
{
    Harvard,
    Modern,
    Minimalist
}

public class GenerateRequest
{
    public string JobDescription { get; set; }
    public List<string> Formats { get; set; }
    public CVTemplate Template { get; set; } = CVTemplate.Harvard;
}

// DocumentRenderer
public async Task<byte[]> RenderPdfAsync(GeneratedCV cv, CVTemplate template)
{
    var templatePath = template switch
    {
        CVTemplate.Harvard => _harvardTemplatePath,
        CVTemplate.Modern => _modernTemplatePath,
        CVTemplate.Minimalist => _minimalistTemplatePath,
        _ => _harvardTemplatePath
    };
    
    var html = RenderHtml(cv, templatePath);
    // ... rest of PDF generation
}
```

**Frontend:**
```javascript
<div className="template-selector">
  <div className={`template-option ${template === 'harvard' ? 'selected' : ''}`}>
    <img src="/previews/harvard.png" alt="Harvard Template" />
    <span>Harvard</span>
  </div>
  <div className={`template-option ${template === 'modern' ? 'selected' : ''}`}>
    <img src="/previews/modern.png" alt="Modern Template" />
    <span>Modern</span>
  </div>
  <div className={`template-option ${template === 'minimalist' ? 'selected' : ''}`}>
    <img src="/previews/minimalist.png" alt="Minimalist Template" />
    <span>Minimalist</span>
  </div>
</div>
```

**Dependencies:** None

**Design Notes:**
- Create template preview images (screenshots)
- Ensure all templates pass ATS checks
- Modern template should still be ATS-parseable (no complex tables)

---

#### FEATURE-007: Two-Page Resume Option

**Epic:** Flexibility  
**Story Points:** 5  
**Priority:** P2 - Medium  
**Labels:** `enhancement`, `backend`

**User Story:**
```
AS A senior professional with extensive experience
I WANT THE option to generate a two-page resume
SO THAT I can include more relevant experiences
```

**Acceptance Criteria:**
- [ ] Toggle in UI: "Allow 2 pages if needed"
- [ ] Default is still 1 page (unchecked)
- [ ] If checked and content fits in 1 page, still 1 page
- [ ] If checked and content needs 2 pages:
  - Include top 4 experiences instead of 3
  - Include 4 bullets per experience instead of 3
  - Include up to 2 projects
  - Max 2 pages (never 2.5 or 3)
- [ ] Both PDF and DOCX respect page limit

**Technical Implementation:**
```csharp
public class GenerateRequest
{
    public string JobDescription { get; set; }
    public List<string> Formats { get; set; }
    public CVTemplate Template { get; set; } = CVTemplate.Harvard;
    public bool AllowTwoPages { get; set; } = false; // NEW
}

// PageConstraintHelper
public GeneratedCV EnforcePageConstraint(GeneratedCV cv, bool allowTwoPages)
{
    var maxLines = allowTwoPages ? 96 : 48; // ~48 lines per page
    var currentLines = CalculateTotalLines(cv);
    
    if (currentLines <= maxLines)
    {
        return cv; // Fits within limit
    }
    
    // Reduction logic with new max
    while (currentLines > maxLines && /* reduction possible */)
    {
        // ... existing reduction strategy
    }
    
    return cv;
}

// CVSelector
public GeneratedCV SelectAndAdaptCV(CVMaster cvMaster, JobRequirements requirements, bool allowTwoPages)
{
    var maxExperiences = allowTwoPages ? 4 : 3;
    var maxBulletsPerExp = allowTwoPages ? 4 : 3;
    var maxProjects = allowTwoPages ? 2 : 1;
    
    // ... rest of selection logic
}
```

**UI:**
```javascript
<Form.Check 
  type="checkbox"
  label="Allow 2 pages if needed (recommended for 8+ years experience)"
  checked={allowTwoPages}
  onChange={(e) => setAllowTwoPages(e.target.checked)}
/>
```

**Dependencies:** None

---

#### FEATURE-008: Advanced Keyword Analysis

**Epic:** Analytics & Insights  
**Story Points:** 8  
**Priority:** P2 - Medium  
**Labels:** `enhancement`, `analytics`, `backend`, `nlp`

**User Story:**
```
AS A user wanting to optimize my CV
I WANT TO see detailed keyword analysis
SO THAT I understand how to improve my match score
```

**Acceptance Criteria:**
- [ ] Analyze job description for keyword frequency
- [ ] Categorize keywords into:
  - Technical skills
  - Soft skills
  - Industry terms
  - Action verbs
- [ ] Show which keywords are present/missing in CV
- [ ] Highlight keyword density in generated CV
- [ ] Suggest where to add missing keywords

**Technical Implementation:**
```csharp
public class KeywordAnalysis
{
    public Dictionary<string, int> TechnicalKeywords { get; set; }
    public Dictionary<string, int> SoftSkillKeywords { get; set; }
    public Dictionary<string, int> IndustryKeywords { get; set; }
    public Dictionary<string, int> ActionVerbs { get; set; }
    public List<string> PresentInCV { get; set; }
    public List<string> MissingFromCV { get; set; }
    public double KeywordDensity { get; set; }
}

public KeywordAnalysis AnalyzeKeywords(string jobDescription, CVMaster cvMaster)
{
    var jdWords = ExtractWords(jobDescription);
    var cvWords = ExtractWords(SerializeCVToText(cvMaster));
    
    var technicalKeywords = jdWords
        .Where(w => IsTechnicalTerm(w))
        .GroupBy(w => w.ToLower())
        .ToDictionary(g => g.Key, g => g.Count());
    
    var softSkillKeywords = jdWords
        .Where(w => IsSoftSkill(w))
        .GroupBy(w => w.ToLower())
        .ToDictionary(g => g.Key, g => g.Count());
    
    var presentInCV = technicalKeywords.Keys
        .Where(k => cvWords.Contains(k, StringComparer.OrdinalIgnoreCase))
        .ToList();
    
    var missingFromCV = technicalKeywords.Keys
        .Except(presentInCV, StringComparer.OrdinalIgnoreCase)
        .ToList();
    
    return new KeywordAnalysis
    {
        TechnicalKeywords = technicalKeywords,
        SoftSkillKeywords = softSkillKeywords,
        PresentInCV = presentInCV,
        MissingFromCV = missingFromCV,
        KeywordDensity = (double)presentInCV.Count / technicalKeywords.Count
    };
}
```

**UI Display:**
```
📊 Keyword Analysis

Technical Keywords (Frequency in JD):
  ✅ React (5) - Present in your CV
  ✅ PostgreSQL (3) - Present
  ✅ Docker (2) - Present
  ❌ Kubernetes (4) - MISSING
  ❌ GraphQL (2) - MISSING

Soft Skills:
  ✅ Agile (3)
  ✅ Team collaboration (2)
  ❌ Leadership (2) - Consider highlighting in bullets

Keyword Density: 75% (12/16 keywords present)

💡 Suggestions:
  • Add "Kubernetes" to skills if you have experience
  • Mention "GraphQL" in relevant project bullets
  • Highlight "leadership" in your mentoring experience
```

**Dependencies:** FEATURE-002 (Match Score)

---

### Priority 3: Advanced Features (High Impact, High Effort)

---

#### FEATURE-009: LLM Integration (Claude/GPT API)

**Epic:** AI-Powered Matching  
**Story Points:** 21  
**Priority:** P3 - Low (Future)  
**Labels:** `enhancement`, `ai`, `backend`, `api-integration`

**User Story:**
```
AS A user wanting the best possible CV
I WANT AI to understand semantic matches
SO THAT my CV is optimally tailored even with synonym/related skills
```

**Acceptance Criteria:**
- [ ] Integrate Claude or GPT API for intelligent matching
- [ ] API understands semantic relationships:
  - "leadership" → selects "trained 8 team members" bullet
  - "scalability" → selects "handling 10K+ requests" bullet
  - "React developer" → includes Next.js experience
- [ ] Fallback to existing regex-based system if API fails/unavailable
- [ ] API key configurable via environment variable
- [ ] Cost estimation shown to user (optional)

**Technical Implementation:**

**Prompt Engineering:**
```csharp
public async Task<GeneratedCV> GenerateWithLLM(CVMaster cvMaster, JobRequirements requirements)
{
    var prompt = $@"
You are an expert resume writer. Given a CV Master and Job Description, 
select the 3 most relevant experiences and 3 best bullets for each.

Job Description:
{requirements.OriginalText}

CV Master:
{JsonSerializer.Serialize(cvMaster)}

Return ONLY a JSON object with this structure:
{{
  ""selectedExperiences"": [
    {{
      ""role"": ""Junior Software Developer"",
      ""company"": ""Primero Systems Inc."",
      ""selectedBullets"": [0, 2, 5] // indices of bullets to include
    }}
  ],
  ""topSkills"": [""React"", ""PostgreSQL"", ""Docker"", ...],
  ""reasoning"": ""Primero Systems is most relevant because...""
}}

Rules:
- NEVER invent experiences, dates, or technologies
- ONLY select from provided CV Master
- Prioritize quantifiable achievements
- Match technical skills to job requirements
- Consider recency and seniority level
";

    var response = await _claudeClient.Messages.CreateAsync(new MessageRequest
    {
        Model = "claude-sonnet-4-20250514",
        MaxTokens = 2000,
        Messages = new[] 
        { 
            new Message { Role = "user", Content = prompt } 
        }
    });
    
    var llmResponse = JsonSerializer.Deserialize<LLMSelectionResponse>(response.Content[0].Text);
    
    return BuildGeneratedCV(cvMaster, llmResponse);
}
```

**API Configuration:**
```json
// appsettings.json
{
  "LLM": {
    "Provider": "Claude", // or "OpenAI"
    "ApiKey": "${ANTHROPIC_API_KEY}",
    "Model": "claude-sonnet-4-20250514",
    "Enabled": true,
    "FallbackToClassic": true
  }
}
```

**Cost Management:**
```csharp
public class LLMCostEstimator
{
    private const double COST_PER_1K_INPUT_TOKENS = 0.003;
    private const double COST_PER_1K_OUTPUT_TOKENS = 0.015;
    
    public double EstimateCost(string jobDescription, CVMaster cvMaster)
    {
        var inputTokens = EstimateTokens(jobDescription + JsonSerializer.Serialize(cvMaster));
        var outputTokens = 500; // Estimated response size
        
        var cost = (inputTokens / 1000.0 * COST_PER_1K_INPUT_TOKENS) +
                   (outputTokens / 1000.0 * COST_PER_1K_OUTPUT_TOKENS);
        
        return cost; // In USD
    }
}
```

**UI:**
```javascript
<Form.Check 
  type="checkbox"
  label="Use AI-powered matching (costs ~$0.02 per CV)"
  checked={useAI}
  onChange={(e) => setUseAI(e.target.checked)}
/>
```

**Dependencies:**
- Anthropic or OpenAI SDK
- API key management
- Error handling & fallback

**Risks:**
- API costs can add up
- API rate limits
- Response latency (2-5 seconds)
- Need fallback for API failures

---

#### FEATURE-010: ATS Score & Recommendations

**Epic:** Quality Assurance  
**Story Points:** 13  
**Priority:** P3 - Low (Future)  
**Labels:** `enhancement`, `analytics`, `ats`

**User Story:**
```
AS A user submitting to ATS systems
I WANT TO know if my CV will pass ATS parsing
SO THAT I can fix issues before applying
```

**Acceptance Criteria:**
- [ ] Analyze generated CV for ATS compatibility
- [ ] Check multiple criteria:
  - Font is ATS-friendly (Times, Arial, Calibri)
  - No images or graphics
  - No tables or text boxes
  - No headers/footers
  - Contact info in standard format
  - Keywords from JD present
  - Section headings are standard
  - File format is .pdf or .docx (not .pages)
- [ ] Provide score (0-100) with breakdown
- [ ] List specific issues and how to fix

**ATS Checks:**
```csharp
public class ATSCompatibilityChecker
{
    public ATSScore CheckCompatibility(GeneratedCV cv, byte[] pdfBytes, JobRequirements requirements)
    {
        var score = 100;
        var issues = new List<ATSIssue>();
        var recommendations = new List<string>();
        
        // 1. Font check
        if (!IsATSFriendlyFont(cv))
        {
            score -= 10;
            issues.Add(new ATSIssue 
            { 
                Severity = "High",
                Issue = "Non-standard font detected",
                Fix = "Use Times New Roman, Arial, or Calibri"
            });
        }
        
        // 2. Image check
        if (ContainsImages(pdfBytes))
        {
            score -= 15;
            issues.Add(new ATSIssue
            {
                Severity = "Critical",
                Issue = "Images detected in PDF",
                Fix = "Remove all images - ATS cannot parse them"
            });
        }
        
        // 3. Keyword density
        var keywordDensity = CalculateKeywordDensity(cv, requirements);
        if (keywordDensity < 0.5)
        {
            score -= 20;
            issues.Add(new ATSIssue
            {
                Severity = "High",
                Issue = $"Low keyword density ({keywordDensity:P0})",
                Fix = "Include more keywords from job description"
            });
        }
        
        // 4. Contact info format
        if (!IsValidContactFormat(cv.Header))
        {
            score -= 5;
            issues.Add(new ATSIssue
            {
                Severity = "Medium",
                Issue = "Contact info may not parse correctly",
                Fix = "Use format: email@domain.com | (123) 456-7890"
            });
        }
        
        // 5. Section headings
        if (!HasStandardSections(cv))
        {
            score -= 10;
            issues.Add(new ATSIssue
            {
                Severity = "Medium",
                Issue = "Non-standard section headings",
                Fix = "Use: Education, Experience, Skills, Projects"
            });
        }
        
        return new ATSScore
        {
            Overall = Math.Max(0, score),
            Issues = issues,
            Recommendations = GenerateRecommendations(issues, cv, requirements)
        };
    }
    
    private List<string> GenerateRecommendations(List<ATSIssue> issues, GeneratedCV cv, JobRequirements req)
    {
        var recommendations = new List<string>();
        
        if (!issues.Any())
        {
            recommendations.Add("✅ Your CV is fully ATS-optimized!");
            return recommendations;
        }
        
        recommendations.Add($"⚠️ {issues.Count} issue(s) detected:");
        
        foreach (var issue in issues.OrderByDescending(i => i.Severity))
        {
            recommendations.Add($"  • {issue.Issue} → {issue.Fix}");
        }
        
        return recommendations;
    }
}

public class ATSScore
{
    public int Overall { get; set; }
    public List<ATSIssue> Issues { get; set; }
    public List<string> Recommendations { get; set; }
}

public class ATSIssue
{
    public string Severity { get; set; } // Critical, High, Medium, Low
    public string Issue { get; set; }
    public string Fix { get; set; }
}
```

**UI Display:**
```
┌─────────────────────────────────────────┐
│  ATS Compatibility Score                │
│                                         │
│  92/100  🟢 Excellent                   │
│  ████████████████████░░                 │
│                                         │
│  ✅ Font: ATS-friendly                  │
│  ✅ Format: Clean structure             │
│  ✅ Keywords: 85% density               │
│  ⚠️ Contact: Phone format non-standard  │
│                                         │
│  Recommendations:                       │
│  • Use (123) 456-7890 format for phone │
│  • Consider adding 2 more JD keywords  │
│                                         │
│  [Download Anyway] [Fix & Regenerate]  │
└─────────────────────────────────────────┘
```

**Dependencies:** FEATURE-002 (Match Score)

---

#### FEATURE-011: Cover Letter Generator

**Epic:** Application Suite  
**Story Points:** 13  
**Priority:** P3 - Low (Future)  
**Labels:** `enhancement`, `new-feature`, `ai`

**User Story:**
```
AS A user applying to jobs
I WANT TO generate a matching cover letter
SO THAT I save time and ensure consistency with my CV
```

**Acceptance Criteria:**
- [ ] User can optionally generate cover letter with CV
- [ ] Cover letter uses same job description input
- [ ] Template structure:
  - Opening: Express interest in role
  - Body (2 paragraphs): Highlight relevant experience
  - Closing: Express enthusiasm, call to action
- [ ] Personalization:
  - Company name (extracted from JD)
  - Specific role title
  - Relevant experiences from CV
  - Matching keywords
- [ ] Export as PDF or DOCX
- [ ] Preview before download

**Template Structure:**
```
[Your Name]
[Your Address]
[Your Email] | [Your Phone]
[Date]

[Hiring Manager Name / Hiring Team]
[Company Name]
[Company Address]

Dear [Hiring Manager / Hiring Team],

I am writing to express my strong interest in the [Job Title] position at [Company Name]. 
With [X] years of experience in [Top Skills], I am confident I can contribute immediately 
to your team's success.

In my current role at [Most Recent Company], I have [Achievement 1 from CV] and 
[Achievement 2 from CV]. My expertise in [Skill 1], [Skill 2], and [Skill 3] directly 
aligns with your requirements for [JD Requirement 1] and [JD Requirement 2].

[Optional: Additional paragraph about specific project or achievement relevant to JD]

I am particularly excited about [Something specific from JD or company mission] and would 
welcome the opportunity to discuss how my background in [Top Skills] can help [Company] 
achieve [Goal mentioned in JD or inferred].

Thank you for considering my application. I look forward to the opportunity to discuss 
this position further.

Sincerely,
[Your Name]
```

**Technical Implementation:**
```csharp
public class CoverLetterGenerator
{
    public string GenerateCoverLetter(
        CVMaster cvMaster, 
        JobRequirements requirements,
        GeneratedCV selectedContent)
    {
        var template = LoadTemplate("cover-letter-template.txt");
        
        var data = new
        {
            YourName = cvMaster.Header.FullName,
            YourEmail = cvMaster.Header.Email,
            YourPhone = cvMaster.Header.Phone,
            YourLocation = cvMaster.Header.Location,
            Date = DateTime.Now.ToString("MMMM dd, yyyy"),
            
            CompanyName = ExtractCompanyName(requirements.OriginalText),
            JobTitle = ExtractJobTitle(requirements.OriginalText),
            
            YearsOfExperience = CalculateYears(cvMaster.Experience),
            TopSkills = string.Join(", ", selectedContent.Skills.Take(3)),
            
            MostRecentCompany = selectedContent.Experience.First().Company,
            Achievement1 = ExtractAchievement(selectedContent.Experience.First(), 0),
            Achievement2 = ExtractAchievement(selectedContent.Experience.First(), 1),
            
            Skill1 = selectedContent.Skills[0],
            Skill2 = selectedContent.Skills[1],
            Skill3 = selectedContent.Skills[2],
            
            JDRequirement1 = requirements.RequiredExperience.FirstOrDefault(),
            CompanyMission = ExtractMission(requirements.OriginalText)
        };
        
        return RenderTemplate(template, data);
    }
}
```

**UI:**
```javascript
<Form.Check 
  type="checkbox"
  label="Also generate cover letter"
  checked={generateCoverLetter}
  onChange={(e) => setGenerateCoverLetter(e.target.checked)}
/>

// If checked, additional downloads
{
  cvPdf: "CV_20250216.pdf",
  cvDocx: "CV_20250216.docx",
  coverLetterPdf: "CoverLetter_20250216.pdf",
  coverLetterDocx: "CoverLetter_20250216.docx"
}
```

**Dependencies:**
- Similar templating as CV
- Company/role extraction logic
- Additional template file

---

#### FEATURE-012: Multi-Language Support

**Epic:** Internationalization  
**Story Points:** 21  
**Priority:** P4 - Lowest (Future)  
**Labels:** `enhancement`, `i18n`, `backend`, `frontend`

**User Story:**
```
AS A user applying to jobs in different countries
I WANT TO generate CVs in multiple languages
SO THAT I can apply to international positions
```

**Acceptance Criteria:**
- [ ] Support English, Spanish, French, German (minimum)
- [ ] CV Master contains translations for all fields
- [ ] User selects language before generation
- [ ] All template text translated (section headings, labels)
- [ ] Date formats localized
- [ ] Phone number formats localized
- [ ] Generated CV is 100% in selected language

**CV Master Structure:**
```json
{
  "header": {
    "fullName": "Ezequiel Rindello",
    "email": "ezequiel@example.com",
    "phone": "+54 9 341 279-3916",
    "location_en": "Rosario, Argentina",
    "location_es": "Rosario, Argentina",
    "linkedin": "linkedin.com/in/ezequielrindello",
    "github": "github.com/ezequielrindello"
  },
  "experience": [
    {
      "role_en": "Junior Software Developer",
      "role_es": "Desarrollador de Software Junior",
      "role_fr": "Développeur de Logiciels Junior",
      "company": "Primero Systems Inc.",
      "location": "Remote · San Diego, CA",
      "startDate_en": "Jun 2025",
      "startDate_es": "Jun 2025",
      "endDate_en": "Present",
      "endDate_es": "Actualidad",
      "bullets_en": [
        "Developed and maintained a large-scale internal SaaS application using React 19..."
      ],
      "bullets_es": [
        "Desarrollé y mantuve una aplicación SaaS interna a gran escala usando React 19..."
      ],
      "skills": ["React", "PostgreSQL", "Docker"]
    }
  ]
}
```

**Implementation:**
```csharp
public class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    
    public LocalizationService()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["education"] = "Education",
                ["experience"] = "Experience",
                ["skills"] = "Skills",
                ["projects"] = "Projects",
                ["present"] = "Present"
            },
            ["es"] = new Dictionary<string, string>
            {
                ["education"] = "Educación",
                ["experience"] = "Experiencia",
                ["skills"] = "Habilidades",
                ["projects"] = "Proyectos",
                ["present"] = "Actualidad"
            }
        };
    }
    
    public string Translate(string key, string language)
    {
        return _translations[language][key];
    }
}

public GeneratedCV SelectAndAdaptCV(
    CVMaster cvMaster, 
    JobRequirements requirements,
    string language = "en")
{
    // Extract language-specific fields
    var localizedExperiences = cvMaster.Experience.Select(e => new Experience
    {
        Role = GetLocalizedField(e, "role", language),
        Company = e.Company,
        Location = e.Location,
        StartDate = GetLocalizedField(e, "startDate", language),
        EndDate = GetLocalizedField(e, "endDate", language),
        Bullets = GetLocalizedField(e, "bullets", language) as List<string>,
        Skills = e.Skills
    }).ToList();
    
    // ... rest of selection logic
}
```

**Dependencies:**
- Translation management
- Locale-specific formatting
- Potentially LLM for translation quality

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/FEATURE-XXX`)
3. **Follow coding standards:**
   - C#: Follow Microsoft C# Coding Conventions
   - React: Use ESLint rules provided
   - Comments: Document complex logic
4. **Write tests** (when test infrastructure is added)
5. **Update documentation** if adding features
6. **Submit pull request** with clear description

### Development Workflow

```bash
# 1. Create feature branch
git checkout -b feature/preview-implementation

# 2. Make changes
# ... code, test, commit ...

# 3. Push to your fork
git push origin feature/preview-implementation

# 4. Create PR on GitHub
# Describe changes, link to feature ticket (FEATURE-XXX)
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **PuppeteerSharp** - For excellent PDF generation
- **DocumentFormat.OpenXml** - For reliable DOCX creation
- **Stubble** - For clean Mustache templating
- **React Bootstrap** - For rapid UI development

---

## 📞 Support

For issues, questions, or feature requests:
- Open an issue on GitHub
- Check existing issues for solutions
- Review documentation and QUICKSTART.md

---

**Built with ❤️ for making job applications easier**

Last Updated: February 16, 2026  
Version: 1.0.0
