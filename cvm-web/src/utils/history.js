const STORAGE_KEY = 'cv-generator-history';
const MAX_ENTRIES = 10;

function extractJobTitle(jobDescription) {
  const firstLine = (jobDescription || '')
    .split('\n')
    .map((line) => line.trim())
    .find((line) => line.length > 0);

  return (firstLine || 'Unknown role').slice(0, 80);
}

export function getHistory() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const parsed = raw ? JSON.parse(raw) : [];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function saveToHistory(entry) {
  const current = getHistory();
  const nextItem = {
    id: Date.now(),
    jobTitle: extractJobTitle(entry.jobDescription),
    generatedAt: new Date().toISOString(),
    matchScore: entry.matchScore ?? null,
    template: entry.template,
    jobDescription: entry.jobDescription,
  };

  const updated = [nextItem, ...current].slice(0, MAX_ENTRIES);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
  return updated;
}

export function clearHistory() {
  localStorage.removeItem(STORAGE_KEY);
}
