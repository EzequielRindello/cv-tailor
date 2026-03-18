#!/usr/bin/env node
/**
 * Bug regression checks (2.0.2)
 *
 * Focus:
 * - BUG-402: strict format validation on generate/regenerate/download
 * - Smoke coverage for regenerate endpoint flow
 *
 * Usage:
 *   node testing/bug-regression-2.0.2.js
 *
 * Env:
 *   API_URL=http://localhost:5000/api
 */

const API_BASE = process.env.API_URL || "http://localhost:5000/api";

async function post(path, body) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  const text = await res.text();
  let json = null;
  try {
    json = JSON.parse(text);
  } catch {
    json = { raw: text };
  }

  return { status: res.status, ok: res.ok, json };
}

async function get(path) {
  const res = await fetch(`${API_BASE}${path}`);
  const text = await res.text();
  let json = null;
  try {
    json = JSON.parse(text);
  } catch {
    json = { raw: text };
  }
  return { status: res.status, ok: res.ok, json };
}

function check(name, condition, details = "") {
  const status = condition ? "PASS" : "FAIL";
  console.log(`${condition ? "✅" : "❌"} ${status} - ${name}`);
  if (!condition && details) console.log(`   ${details}`);
  return condition;
}

async function main() {
  console.log("\nER CV Generator - Bug Regression 2.0.2");
  console.log(`API: ${API_BASE}\n`);

  let passed = 0;
  let failed = 0;

  // 1) generate with invalid format must return 400
  const invalidGenerate = await post("/cv/generate", {
    jobDescription: "Backend role with C# and PostgreSQL",
    formats: ["exe"],
    template: "Harvard",
    allowTwoPages: false,
  });
  if (check(
    "Generate rejects invalid formats",
    invalidGenerate.status === 400,
    `Expected 400, got ${invalidGenerate.status}. Payload: ${JSON.stringify(invalidGenerate.json)}`
  )) passed++; else failed++;

  // 2) generate valid request (needed for regenerate smoke)
  const validGenerate = await post("/cv/generate", {
    jobDescription: "Senior backend engineer with .NET, PostgreSQL, REST APIs",
    formats: ["pdf"],
    template: "Harvard",
    allowTwoPages: false,
  });
  if (check(
    "Generate accepts valid format",
    validGenerate.status === 200 && validGenerate.json?.success === true,
    `Expected 200/success=true, got ${validGenerate.status}. Payload: ${JSON.stringify(validGenerate.json)}`
  )) passed++; else failed++;

  // 3) regenerate with invalid format must return 400
  const invalidRegenerate = await post("/cv/regenerate", {
    generatedCV: validGenerate.json?.generatedCV,
    bulletSelections: {},
    formats: ["zip"],
    template: "Harvard",
    allowTwoPages: false,
  });
  if (check(
    "Regenerate rejects invalid formats",
    invalidRegenerate.status === 400,
    `Expected 400, got ${invalidRegenerate.status}. Payload: ${JSON.stringify(invalidRegenerate.json)}`
  )) passed++; else failed++;

  // 4) download with invalid format must return 400
  const invalidDownload = await get("/cv/download/zip");
  if (check(
    "Download rejects invalid format path",
    invalidDownload.status === 400,
    `Expected 400, got ${invalidDownload.status}. Payload: ${JSON.stringify(invalidDownload.json)}`
  )) passed++; else failed++;

  // 5) regenerate smoke with valid payload (reorder-compatible path)
  const validRegenerate = await post("/cv/regenerate", {
    generatedCV: validGenerate.json?.generatedCV,
    bulletSelections: { exp0: [0, 1, 2] },
    formats: ["pdf"],
    template: "Harvard",
    allowTwoPages: false,
  });
  if (check(
    "Regenerate accepts valid payload",
    validRegenerate.status === 200 && validRegenerate.json?.success === true,
    `Expected 200/success=true, got ${validRegenerate.status}. Payload: ${JSON.stringify(validRegenerate.json)}`
  )) passed++; else failed++;

  console.log("\n----------------------------------------");
  console.log(`Passed: ${passed}`);
  console.log(`Failed: ${failed}`);
  console.log("----------------------------------------\n");

  process.exit(failed > 0 ? 1 : 0);
}

main().catch((err) => {
  console.error("Fatal test runner error:", err);
  process.exit(1);
});
