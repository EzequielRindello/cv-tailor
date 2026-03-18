#!/usr/bin/env node
/**
 * ER CV Generator — Automated Test Runner
 *
 * Usage:
 *   node test-runner.js                  # runs all test cases
 *   node test-runner.js --tc TC-01       # runs a single test case
 *   node test-runner.js --profile cv-master  # override profile id
 *   node test-runner.js --report html    # output format: html | json | text (default)
 *
 * Requirements:
 *   - Node.js 18+
 *   - API running at http://localhost:5000
 *   - npm install node-fetch (or Node 18 native fetch works too)
 *
 * Add new test cases at the bottom of TEST_CASES array.
 */

const API_BASE = process.env.API_URL || "http://localhost:5000/api";
const args = process.argv.slice(2);
const filterTC = args.includes("--tc") ? args[args.indexOf("--tc") + 1] : null;
const reportFormat = args.includes("--report") ? args[args.indexOf("--report") + 1] : "text";
const profileArg = args.includes("--profile") ? args[args.indexOf("--profile") + 1] : null;
const PROFILE_ID = profileArg || process.env.PROFILE_ID || "cv-master";

// ─── Assertions ─────────────────────────────────────────────────────────────

function expect(label, value, check, hint = "") {
  const pass = check(value);
  return {
    label,
    pass,
    value: JSON.stringify(value),
    hint: pass ? "" : hint,
  };
}

function gte(n) { return v => typeof v === "number" && v >= n; }
function lte(n) { return v => typeof v === "number" && v <= n; }
function isNull() { return v => v === null || v === undefined; }
function isNotNull() { return v => v !== null && v !== undefined; }
function includes(str) { return v => typeof v === "string" && v.toLowerCase().includes(str.toLowerCase()); }
function notIncludes(str) { return v => typeof v === "string" && !v.toLowerCase().includes(str.toLowerCase()); }
function notInArray(str) { return v => Array.isArray(v) && !v.some(x => x.toLowerCase().includes(str.toLowerCase())); }
function firstIs(str) { return v => Array.isArray(v) && v[0]?.toLowerCase().includes(str.toLowerCase()); }
function hasLength(n) { return v => Array.isArray(v) && v.length >= n; }

// ─── Test Cases ──────────────────────────────────────────────────────────────

const TEST_CASES = [
  {
    id: "TC-01",
    name: "Perfect Match — Senior Backend Go/Kafka",
    config: { template: "Harvard", formats: ["pdf"], allowTwoPages: false },
    jd: `Senior Backend Engineer — Payments Infrastructure

We are looking for a Senior Backend Engineer to join our Payments Infrastructure team.

Requirements:
- 5+ years of backend engineering experience
- Strong proficiency in Go or similar systems language
- Experience with distributed systems and message queues (Kafka, RabbitMQ)
- Deep knowledge of PostgreSQL and Redis
- Experience with Kubernetes and container orchestration
- Familiarity with observability tools (Prometheus, Grafana, Datadog)
- Experience designing and building high-throughput REST APIs
- Strong understanding of microservices architecture

Nice to have:
- Experience with ArgoCD or GitOps workflows
- Payment processing domain knowledge
- Experience mentoring junior engineers

We offer a collaborative remote-first environment with 2-week agile sprints.`,
    assertions: (r) => [
      expect("Match Score >= 75%", r.matchScore.overall, gte(75), "Parser likely still pulling garbage tokens or skills not matching"),
      expect("LowMatchWarning is null", r.matchScore.lowMatchWarning, isNull(), "Score too low — check SkillNormalizer or parser"),
      expect("Seniority: Match", r.matchScore.seniorityMatch, v => v === true, "Seniority detection wrong"),
      expect("Stripe is first experience", r.generatedCV.experience.map(e => e.company), firstIs("Stripe"), "Scorer not ranking Stripe first"),
      expect("Summary is Senior Backend role", r.generatedCV.summary, includes("Senior"), "DeriveRoleFromCV not reading candidate seniority"),
      expect("Summary not Junior", r.generatedCV.summary, notIncludes("Junior"), "JD seniority leaking into summary"),
      expect("Go not in Missing Skills", r.missingSkills.requiredMissing, notInArray("go"), "SkillNormalizer or whitelist miss"),
      expect("Kafka not in Missing Skills", r.missingSkills.requiredMissing, notInArray("kafka"), "Parser not finding Kafka"),
      expect("'Experience' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("experience"), "FIX-A neverSkills filter broken"),
      expect("'Strong' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("strong"), "FIX-A neverSkills filter broken"),
      expect("ATS Score >= 70", r.atsScore.overall, gte(70), "ATS check too aggressive"),
    ]
  },

  {
    id: "TC-02",
    name: "Good Match — Frontend React/TypeScript (Modern template)",
    config: { template: "Modern", formats: ["pdf"], allowTwoPages: false },
    jd: `Frontend Engineer — React / TypeScript

We're building the next generation of developer tooling and need a frontend engineer to own our web application.

What you'll do:
- Build and maintain React components and pages
- Work with TypeScript in a modern Next.js codebase
- Write unit and integration tests with Jest and Testing Library
- Collaborate with designers using Figma specs

Requirements:
- 3+ years experience with React and TypeScript
- Solid understanding of modern JavaScript (ES2020+)
- Experience with Next.js or similar SSR framework
- Familiarity with REST APIs and GraphQL
- Experience writing tests (Jest, Cypress, or Playwright)
- Git and GitHub workflow

Nice to have:
- Experience with Tailwind CSS
- Familiarity with CI/CD pipelines`,
    assertions: (r) => [
      expect("Match Score >= 58%", r.matchScore.overall, gte(58), "Frontend skills not matching — CV master lacks Figma, Tailwind, Playwright which is expected"),
      expect("Airbnb is first experience", r.generatedCV.experience.map(e => e.company), firstIs("Airbnb"), "Airbnb has most React/TS/GraphQL — should rank first"),
      expect("Summary mentions Frontend or Full Stack", r.generatedCV.summary, v => includes("frontend")(v) || includes("full stack")(v), "DetectRoleTypeFromJD not finding frontend signals"),
      expect("Summary not Backend", r.generatedCV.summary, notIncludes("backend"), "Wrong role type detected"),
      expect("Summary not Junior", r.generatedCV.summary, notIncludes("Junior"), "JD seniority leaking"),
      expect("React not in Missing Skills", r.missingSkills.requiredMissing, notInArray("react"), "React missing — check SkillNormalizer"),
      expect("TypeScript not in Missing Skills", r.missingSkills.requiredMissing, notInArray("typescript"), "TypeScript missing — alias issue?"),
      expect("'Familiarity' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("familiarity"), "FIX-A neverSkills filter broken"),
      expect("'Nice' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("nice"), "FIX-A neverSkills filter broken"),
    ]
  },

  {
    id: "TC-03",
    name: "Partial Match — DevOps/Platform Engineer",
    config: { template: "Harvard", formats: ["pdf", "docx"], allowTwoPages: false },
    jd: `Platform Engineer — DevOps / Infrastructure

We're looking for a Platform Engineer to own our cloud infrastructure.

Responsibilities:
- Design and maintain Kubernetes clusters on AWS EKS
- Build and improve CI/CD pipelines (GitHub Actions, ArgoCD)
- Write infrastructure as code using Terraform
- Monitor system health with Prometheus and Grafana
- Manage container lifecycle with Docker

Requirements:
- 3+ years in DevOps, SRE, or Platform Engineering
- Strong Kubernetes and Docker experience
- Proficiency with Terraform or Pulumi
- Experience with GitHub Actions or similar CI/CD tools
- AWS or GCP cloud experience
- Linux systems administration
- Scripting in Python or Bash

Nice to have:
- Helm chart authoring
- Experience with service mesh (Istio, Linkerd)`,
    assertions: (r) => [
      expect("Match Score 45–75%", r.matchScore.overall, v => v >= 45 && v <= 75, "Score inflated or too low for partial DevOps match"),
      expect("LowMatchWarning is null", r.matchScore.lowMatchWarning, isNull(), "45%+ should not trigger mismatch warning"),
      expect("Stripe is first experience", r.generatedCV.experience.map(e => e.company), firstIs("Stripe"), "Stripe has most infra skills"),
      expect("'Cost' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("cost"), "FIX-A neverSkills filter broken"),
      expect("'Monitor' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("monitor"), "FIX-A neverSkills filter broken"),
      expect("'Manage' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("manage"), "FIX-A neverSkills filter broken"),
    ]
  },

  {
    id: "TC-04",
    name: "Poor Match — iOS Developer (Mismatch Warning Test)",
    config: { template: "Harvard", formats: ["pdf"], allowTwoPages: false },
    jd: `iOS Software Engineer

We are looking for an iOS engineer to build native mobile experiences.

Requirements:
- 3+ years iOS development experience
- Proficiency in Swift and Objective-C
- Experience with UIKit and SwiftUI
- Xcode and Instruments proficiency
- Knowledge of iOS SDK, Core Data, and push notifications
- Experience with CocoaPods or Swift Package Manager

Nice to have:
- Experience with React Native
- Backend API integration experience`,
    assertions: (r) => [
      expect("Match Score <= 40%", r.matchScore.overall, lte(40), "iOS profile shouldn't score high on a backend CV — React Native nice-to-have inflating score"),
      expect("LowMatchWarning is NOT null", r.matchScore.lowMatchWarning, isNotNull(), "Raise LowMatchWarning threshold to 40% in MatchScoreCalculator — currently set to 30"),
      expect("'BaaS' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("baas"), "FIX-A neverSkills filter broken"),
      expect("'Backend' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("backend"), "FIX-A neverSkills filter broken"),
      expect("Response has generatedCV", r.generatedCV, isNotNull(), "App crashed or returned null on mismatch — should degrade gracefully"),
    ]
  },

  {
    id: "TC-05",
    name: "Fuzzy Matching — Aliases (NodeJS, k8s, TS, Postgres)",
    config: { template: "Harvard", formats: ["pdf"], allowTwoPages: false },
    jd: `Full Stack Developer

We need a full stack developer comfortable across the entire stack.

Technical requirements:
- NodeJS backend development experience
- ReactJS or React frontend skills
- PostgresQL or Postgres database experience
- Experience with k8s for container orchestration
- TypeScript (TS) proficiency
- Familiarity with CI/CD and devops practices
- RESTful API design
- Git and version control

The ideal candidate has experience with microservices and distributed systems.
We use AWS and containerized deployments with Docker.`,
    assertions: (r) => [
      expect("Match Score >= 70%", r.matchScore.overall, gte(70), "Aliases not resolving — SkillNormalizer issue"),
      expect("NodeJS not in Missing Skills", r.missingSkills.requiredMissing, notInArray("nodejs"), "NodeJS→Node.js alias broken"),
      expect("ReactJS not in Missing Skills", r.missingSkills.requiredMissing, notInArray("reactjs"), "ReactJS→React alias broken"),
      expect("Postgres not in Missing Skills", r.missingSkills.requiredMissing, notInArray("postgres"), "Postgres→PostgreSQL alias broken"),
      expect("k8s not in Missing Skills", r.missingSkills.requiredMissing, notInArray("k8s"), "k8s→Kubernetes alias broken"),
      expect("NodeJS not in Keyword suggestions", r.keywordAnalysis?.suggestions ?? [], notInArray("nodejs"), "KeywordAnalyzer not using FuzzyMatch for present check"),
      expect("ReactJS not in Keyword suggestions", r.keywordAnalysis?.suggestions ?? [], notInArray("reactjs"), "KeywordAnalyzer not using FuzzyMatch for present check"),
      expect("'Familiarity' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("familiarity"), "FIX-A neverSkills filter broken"),
    ]
  },

  {
    id: "TC-06",
    name: "Two-Page Mode — Same JD as TC-01",
    config: { template: "Harvard", formats: ["pdf"], allowTwoPages: true },
    jd: `Senior Backend Engineer — Payments Infrastructure

We are looking for a Senior Backend Engineer to join our Payments Infrastructure team.

Requirements:
- 5+ years of backend engineering experience
- Strong proficiency in Go or similar systems language
- Experience with distributed systems and message queues (Kafka, RabbitMQ)
- Deep knowledge of PostgreSQL and Redis
- Experience with Kubernetes and container orchestration
- Familiarity with observability tools (Prometheus, Grafana, Datadog)
- Experience designing and building high-throughput REST APIs

Nice to have:
- Experience with ArgoCD or GitOps workflows
- Payment processing domain knowledge`,
    assertions: (r) => [
      expect("4 experiences in 2-page mode", r.generatedCV.experience, hasLength(4), "allowTwoPages=true should include Box internship"),
      expect("At least 1 project", r.generatedCV.projects, hasLength(1), "2-page mode should include projects"),
      expect("Match Score >= 75%", r.matchScore.overall, gte(75), "Same JD as TC-01 — should score similarly"),
    ]
  },

  {
    id: "TC-07",
    name: "Negation Detection — 'No senior experience required'",
    config: { template: "Minimalist", formats: ["pdf"], allowTwoPages: false },
    jd: `Junior Software Engineer — Full Stack

We're a startup looking for a junior engineer eager to learn and grow.

You will:
- Build features across our React frontend and Node.js backend
- Write and maintain PostgreSQL queries and migrations
- Participate in code reviews

Requirements:
- 0-2 years experience
- Basic knowledge of React or similar frontend framework
- Understanding of REST APIs
- Familiarity with SQL and databases
- Git basics

No senior experience required. We value curiosity over years of experience.`,
    assertions: (r) => [
      expect("Seniority detected as junior", r.matchScore, v => !v.seniorityMatch, "Seniority mismatch expected — CV is Senior, JD is Junior"),
      expect("Summary does NOT say Junior", r.generatedCV.summary, notIncludes("Junior Developer"), "JD seniority leaking into candidate title — FIX-B broken"),
      expect("Summary says Senior", r.generatedCV.summary, includes("Senior"), "Candidate's own seniority should come from their CV"),
      expect("'Basic' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("basic"), "FIX-A neverSkills filter broken"),
      expect("'Understanding' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("understanding"), "FIX-A neverSkills filter broken"),
      expect("'Write' not in Missing Skills", r.missingSkills.requiredMissing, notInArray("write"), "FIX-A neverSkills filter broken"),
    ]
  },
];

// ─── Runner ──────────────────────────────────────────────────────────────────

async function runTest(tc) {
  const start = Date.now();
  let result;

  try {
    const body = {
      jobDescription: tc.jd,
      formats: tc.config.formats,
      template: tc.config.template,
      allowTwoPages: tc.config.allowTwoPages,
      profileId: PROFILE_ID,
    };

    const res = await fetch(`${API_BASE}/cv/generate`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(`HTTP ${res.status}: ${text.slice(0, 200)}`);
    }

    const data = await res.json();
    const elapsed = Date.now() - start;

    const assertions = tc.assertions(data);
    const passed = assertions.filter(a => a.pass).length;
    const total = assertions.length;

    return {
      id: tc.id,
      name: tc.name,
      status: passed === total ? "PASS" : "FAIL",
      passed,
      total,
      elapsed,
      assertions,
      data: {
        matchScore: data.matchScore?.overall,
        seniorityMatch: data.matchScore?.seniorityMatch,
        lowMatchWarning: data.matchScore?.lowMatchWarning ?? null,
        atsScore: data.atsScore?.overall,
        summary: data.generatedCV?.summary,
        experiences: data.generatedCV?.experience?.map(e => `${e.company} (${e.role})`),
        requiredMissing: data.missingSkills?.requiredMissing,
        niceToHaveMissing: data.missingSkills?.niceToHaveMissing,
        projectCount: data.generatedCV?.projects?.length ?? 0,
        experienceCount: data.generatedCV?.experience?.length ?? 0,
      },
      error: null,
    };
  } catch (err) {
    return {
      id: tc.id,
      name: tc.name,
      status: "ERROR",
      passed: 0,
      total: tc.assertions.length,
      elapsed: Date.now() - start,
      assertions: [],
      data: null,
      error: err.message,
    };
  }
}

// ─── Report formatters ────────────────────────────────────────────────────────

function printText(results) {
  const totalPass = results.filter(r => r.status === "PASS").length;
  const totalFail = results.filter(r => r.status === "FAIL").length;
  const totalError = results.filter(r => r.status === "ERROR").length;

  console.log("\n╔══════════════════════════════════════════════════════╗");
  console.log(`║  ER CV Generator — Test Report                       ║`);
  console.log(`║  ${new Date().toLocaleString().padEnd(51)}║`);
  console.log("╚══════════════════════════════════════════════════════╝\n");

  for (const r of results) {
    const icon = r.status === "PASS" ? "✅" : r.status === "ERROR" ? "💥" : "❌";
    console.log(`${icon} ${r.id} — ${r.name}`);
    console.log(`   Score: ${r.data?.matchScore ?? "N/A"}% · ATS: ${r.data?.atsScore ?? "N/A"} · Time: ${r.elapsed}ms`);
    console.log(`   Assertions: ${r.passed}/${r.total} passed`);

    if (r.error) {
      console.log(`   ERROR: ${r.error}`);
    } else {
      if (r.data?.summary) console.log(`   Summary: "${r.data.summary.slice(0, 100)}..."`);
      if (r.data?.experiences) console.log(`   Experiences: ${r.data.experiences.join(" → ")}`);
      if (r.data?.requiredMissing?.length)
        console.log(`   Missing (required): ${r.data.requiredMissing.join(", ")}`);

      const failed = r.assertions.filter(a => !a.pass);
      if (failed.length) {
        console.log(`   Failed assertions:`);
        for (const a of failed)
          console.log(`     ✗ ${a.label} — got: ${a.value} — ${a.hint}`);
      }
    }
    console.log();
  }

  console.log("─────────────────────────────────────────────────────");
  console.log(`Results: ${totalPass} passed · ${totalFail} failed · ${totalError} errors`);
  console.log(`Total assertions: ${results.reduce((s, r) => s + r.passed, 0)}/${results.reduce((s, r) => s + r.total, 0)} passed\n`);
}

function printHtml(results) {
  const totalPass = results.filter(r => r.status === "PASS").length;

  const rows = results.map(r => {
    const statusColor = r.status === "PASS" ? "#16a34a" : r.status === "ERROR" ? "#7c3aed" : "#dc2626";
    const failedAssertions = r.assertions.filter(a => !a.pass);

    const assertionRows = failedAssertions.map(a =>
      `<tr style="background:#fef2f2"><td colspan="2" style="padding:4px 12px;font-size:12px;color:#dc2626">
        ✗ <b>${a.label}</b> — got: <code>${a.value}</code> — ${a.hint}
      </td></tr>`
    ).join("");

    return `
      <tr>
        <td style="padding:8px 12px;font-weight:bold;color:${statusColor}">${r.status}</td>
        <td style="padding:8px 12px;font-weight:bold">${r.id}</td>
        <td style="padding:8px 12px">${r.name}</td>
        <td style="padding:8px 12px;text-align:center">${r.data?.matchScore ?? "N/A"}%</td>
        <td style="padding:8px 12px;text-align:center">${r.data?.atsScore ?? "N/A"}</td>
        <td style="padding:8px 12px;text-align:center">${r.passed}/${r.total}</td>
        <td style="padding:8px 12px;font-size:12px;color:#6b7280">${r.elapsed}ms</td>
      </tr>
      ${assertionRows}
      ${r.error ? `<tr><td colspan="7" style="padding:4px 12px;color:#7c3aed;font-size:12px">💥 ${r.error}</td></tr>` : ""}
      ${r.data?.summary ? `<tr><td colspan="7" style="padding:2px 12px;font-size:11px;color:#374151;background:#f9fafb"><i>${r.data.summary.slice(0, 120)}...</i></td></tr>` : ""}
      ${r.data?.experiences ? `<tr><td colspan="7" style="padding:2px 12px 8px;font-size:11px;color:#374151;background:#f9fafb">${r.data.experiences.join(" → ")}</td></tr>` : ""}
    `;
  }).join("");

  const html = `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>CV Generator Test Report</title>
  <style>
    body { font-family: system-ui, sans-serif; margin: 32px; background: #f9fafb; color: #111; }
    h1 { font-size: 22px; margin-bottom: 4px; }
    .meta { color: #6b7280; font-size: 13px; margin-bottom: 24px; }
    table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
    th { background: #1e293b; color: white; padding: 10px 12px; text-align: left; font-size: 13px; }
    tr:hover td { background: #f1f5f9 !important; }
    .summary { margin-top: 20px; padding: 16px; background: white; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
  </style>
</head>
<body>
  <h1>ER CV Generator — Test Report</h1>
  <p class="meta">Generated: ${new Date().toLocaleString()} · API: ${API_BASE} · Profile: ${PROFILE_ID}</p>
  <table>
    <thead>
      <tr>
        <th>Status</th><th>ID</th><th>Name</th>
        <th>Match%</th><th>ATS</th><th>Assertions</th><th>Time</th>
      </tr>
    </thead>
    <tbody>${rows}</tbody>
  </table>
  <div class="summary">
    <b>Summary:</b> ${totalPass}/${results.length} test cases passed ·
    ${results.reduce((s,r) => s + r.passed, 0)}/${results.reduce((s,r) => s + r.total, 0)} assertions passed
  </div>
</body>
</html>`;

  const filename = `test-report-${Date.now()}.html`;
  require("fs").writeFileSync(filename, html);
  console.log(`\nHTML report written to: ${filename}`);
}

// ─── Main ─────────────────────────────────────────────────────────────────────

(async () => {
  const cases = filterTC
    ? TEST_CASES.filter(tc => tc.id === filterTC)
    : TEST_CASES;

  if (!cases.length) {
    console.error(`No test case found with id: ${filterTC}`);
    process.exit(1);
  }

  console.log(`\nRunning ${cases.length} test case(s) against ${API_BASE} (profile: ${PROFILE_ID})...\n`);

  const results = [];
  for (const tc of cases) {
    process.stdout.write(`  Running ${tc.id}...`);
    const result = await runTest(tc);
    results.push(result);
    console.log(` ${result.status} (${result.passed}/${result.total} assertions, ${result.elapsed}ms)`);
  }

  if (reportFormat === "html") {
    printHtml(results);
  } else if (reportFormat === "json") {
    console.log(JSON.stringify(results, null, 2));
  } else {
    printText(results);
  }

  const anyFailed = results.some(r => r.status !== "PASS");
  process.exit(anyFailed ? 1 : 0);
})();
