#!/usr/bin/env node
/**
 * Security/Hardening smoke checks (2.0.3)
 *
 * Focus:
 * - SEC-401: rate limiting (expects at least one 429 after burst)
 * - Endpoint availability posture
 *
 * Usage:
 *   node testing/security-hardening-2.0.3.js
 *
 * Env:
 *   API_URL=http://localhost:5000/api
 */

const API_BASE = process.env.API_URL || "http://localhost:5000/api";

async function runBurst() {
  const requests = [];
  for (let i = 0; i < 70; i++) {
    requests.push(fetch(`${API_BASE}/cv/health`).then(r => r.status).catch(() => 0));
  }
  return Promise.all(requests);
}

function assert(name, ok, details = "") {
  console.log(`${ok ? "✅" : "❌"} ${name}`);
  if (!ok && details) console.log(`   ${details}`);
  return ok;
}

(async () => {
  console.log("\nSecurity/Hardening Smoke 2.0.3");
  console.log(`API: ${API_BASE}\n`);

  const statuses = await runBurst();
  const count200 = statuses.filter(s => s === 200).length;
  const count429 = statuses.filter(s => s === 429).length;
  const countOther = statuses.filter(s => s !== 200 && s !== 429).length;

  let failed = 0;
  if (!assert("Health endpoint responds in burst", count200 > 0, `No 200 responses. Statuses: ${JSON.stringify(statuses)}`)) failed++;
  if (!assert("Rate limiter returns 429 under high burst", count429 > 0, `No 429 responses detected. 200=${count200}, other=${countOther}`)) failed++;
  if (!assert("No unexpected status flood", countOther < 5, `Unexpected statuses count: ${countOther}`)) failed++;

  console.log("\nSummary");
  console.log(`200: ${count200}`);
  console.log(`429: ${count429}`);
  console.log(`Other: ${countOther}`);

  process.exit(failed ? 1 : 0);
})().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
