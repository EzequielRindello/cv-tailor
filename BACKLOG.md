# ER CV Generator — Feature Backlog

> Tickets de desarrollo. Formato: contexto, criterios de aceptación, implementación técnica.

---

## ✅ Implemented (Backend)

| Feature | Descripción |
|---|---|
| FEATURE-101 | Fuzzy Skill Matching — `SkillNormalizer.cs` centraliza todo el matching |
| FEATURE-102 | CV Preview HTML — `RenderHtmlPublic()` expuesto, `previewHtml` en response |
| FEATURE-202 | Multi CV Master — `GET /api/cv/profiles`, `ProfileId` en `GenerateRequest` |
| FEATURE-302 | PageConstraint Fix — estimación por caracteres reemplaza líneas fijas |
| FIX-001 | `KeywordAnalyzer` ahora usa `GeneratedCV` en lugar de `CVMaster` |
| FIX-002 | Role mismatch warning: `LowMatchWarning` en `MatchScore` cuando Overall < 30% |
| FIX-003 | Bullet scoring mejorado: bonus por verbos de acción, penalización bullets cortos |
| FIX-004 | `SkillNormalizer` reemplaza todas las comparaciones string dispersas |
| FIX-005 | Reorder de bullets persiste correctamente en `/regenerate` |
| FIX-006 | Validación estricta de formatos (`pdf`, `docx`) con `400 BadRequest` |
| FIX-007 | Parsing robusto de años en scoring y summary con warning logs |
| FIX-008 | Frontend usa fallback de `API_BASE_URL` + mensaje claro de conectividad |
| FIX-009 | Matching con límites de palabra para evitar falsos positivos |
| FIX-010 | Límites de tamaño para archivos en session + respuesta 413 controlada |
| FIX-011 | Modelos inicializados para eliminar warnings CS8618 (nullable clean build) |
| FIX-012 | Security baseline: rate limiting, HTTPS redirection no-dev y postura de acceso explícita |

---

## Priority 1 — Pending Frontend

---

### FEATURE-102-FE: CV Preview (HTML inline)

**Epic:** UX
**Labels:** `frontend`
**Backend:** ✅ Listo — `previewHtml` ya viene en el response de `/generate`

**Acceptance Criteria:**
- Después de generar, aparece un preview del CV en la misma página
- El preview usa `response.data.previewHtml` en un `<iframe srcDoc>`
- El usuario puede hacer scroll si el CV tiene más de una página
- Los botones de descarga siguen disponibles junto al preview
- El preview se actualiza cuando el usuario usa el Bullet Editor y regenera

**Implementación:**

```jsx
// App.js
const [previewHtml, setPreviewHtml] = useState(null);
// En handleGenerate, después de setAnalysisResult:
setPreviewHtml(response.data.previewHtml);

// components/CVPreview/CVPreview.jsx
export default function CVPreview({ html }) {
  if (!html) return null;
  return (
    <div className="cv-preview-wrapper">
      <h6>Preview</h6>
      <iframe
        srcDoc={html}
        title="CV Preview"
        style={{ width: '100%', height: '1100px', border: 'none' }}
        sandbox="allow-same-origin"
      />
    </div>
  );
}
```

**Nota:** El preview puede verse levemente distinto al PDF (diferencias entre Chromium headless y el browser). Aceptable para uso personal.

**Archivos a modificar:** `App.js`
**Archivo nuevo:** `components/CVPreview/CVPreview.jsx`

---

### FEATURE-103: Keyboard Shortcuts & Auto-paste

**Epic:** UX
**Labels:** `frontend`

**Acceptance Criteria:**
- `Ctrl+Shift+V` / `Cmd+Shift+V` pega el clipboard en el textarea
- `Ctrl+Enter` ejecuta Generate si el textarea tiene contenido
- Tooltip en el textarea muestra los shortcuts disponibles

**Implementación:**

```jsx
useEffect(() => {
  const handleKeydown = async (e) => {
    if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'V') {
      e.preventDefault();
      try {
        const text = await navigator.clipboard.readText();
        if (text) onChange({ target: { value: text } });
      } catch {}
    }
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      e.preventDefault();
      if (value.trim()) onGenerate();
    }
  };
  window.addEventListener('keydown', handleKeydown);
  return () => window.removeEventListener('keydown', handleKeydown);
}, [value, onChange, onGenerate]);
```

**Archivos a modificar:** `App.js`, `JobDescriptionInput.jsx`

---

## Priority 2 — Pending

---

### FEATURE-201: Generation History (localStorage)

**Epic:** Persistencia
**Labels:** `frontend`

**Acceptance Criteria:**
- Últimas 10 generaciones guardadas en `localStorage`
- Cada entrada: título del rol extraído del JD, fecha, match score, template
- Botón "History" abre un panel lateral
- Botón "↩ Regenerate" en cada entrada pre-carga el JD en el textarea
- Botón "Clear history"

**Implementación:**

```javascript
// utils/history.js
const STORAGE_KEY = 'cv-generator-history';
const MAX_ENTRIES = 10;

export function saveToHistory(entry) {
  const history = getHistory();
  const newEntry = {
    id: Date.now(),
    jobTitle: extractJobTitle(entry.jobDescription),
    generatedAt: new Date().toISOString(),
    matchScore: entry.matchScore?.overall ?? null,
    template: entry.template,
    jobDescription: entry.jobDescription,
  };
  const updated = [newEntry, ...history].slice(0, MAX_ENTRIES);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
  return updated;
}

export function getHistory() {
  try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); }
  catch { return []; }
}

export function clearHistory() { localStorage.removeItem(STORAGE_KEY); }

function extractJobTitle(jd) {
  const firstLine = jd.split('\n').find(l => l.trim().length > 3) || 'Unknown Role';
  return firstLine.trim().slice(0, 60);
}
```

**Archivos nuevos:** `utils/history.js`, `components/HistoryPanel/HistoryPanel.jsx`
**Archivos a modificar:** `App.js`

---

### FEATURE-202-FE: Profile Selector (Frontend)

**Epic:** Flexibilidad
**Labels:** `frontend`
**Backend:** ✅ Listo — `GET /api/cv/profiles` disponible, `profileId` en request

**Acceptance Criteria:**
- Al cargar la app, llama `GET /api/cv/profiles`
- Muestra un selector de perfil antes del textarea
- Si solo hay un perfil, el selector se oculta (no ruido innecesario)
- El `profileId` seleccionado se incluye en el request de generate

**Implementación:**

```jsx
// components/ProfileSelector/ProfileSelector.jsx
export default function ProfileSelector({ value, onChange }) {
  const [profiles, setProfiles] = useState([]);

  useEffect(() => {
    axios.get(`${API_BASE_URL}/cv/profiles`)
      .then(r => setProfiles(r.data))
      .catch(() => {});
  }, []);

  if (profiles.length <= 1) return null;

  return (
    <div className="profile-selector">
      <label>CV Profile</label>
      <select value={value} onChange={e => onChange(e.target.value)}>
        {profiles.map(p => (
          <option key={p.id} value={p.id}>{p.name}</option>
        ))}
      </select>
    </div>
  );
}
```

**Archivos nuevos:** `components/ProfileSelector/ProfileSelector.jsx`
**Archivos a modificar:** `App.js`

---

### FEATURE-203: LLM Integration (Claude API)

**Epic:** Matching Quality
**Labels:** `backend`, `ai`
**Status:** 🔒 Bloqueado por costo — implementar cuando haya API key disponible

**Problema:** El matching es puramente léxico. Un LLM puede entender que "lideré un equipo" es evidencia de "leadership", que "Next.js" es relevante para un JD que pide "React", etc.

**Acceptance Criteria:**
- Toggle opcional en UI: "Use AI matching (slower, smarter)"
- Si activado, LLM selecciona experiencias y bullets en lugar del scorer clásico
- Fallback automático al scorer clásico si la API falla o el toggle está off
- API key en `appsettings.json` / env var, nunca en el frontend
- Costo estimado: ~$0.001–0.003 por generación con claude-haiku-4-5-20251001

**Implementación:**

```csharp
// appsettings.json
{ "LLM": { "Enabled": false, "ApiKey": "", "Model": "claude-haiku-4-5-20251001", "FallbackToClassic": true } }
```

```csharp
// Helpers/LLMSelector.cs — prompt structured output:
// Pass CVMaster experiences + parsed JobRequirements
// Ask for JSON: { selectedExperiences: [{ index, selectedBullets: [] }] }
// Use indices only — never modify content
// Fallback: return null → CVSelector falls back to classic scorer
```

**Archivos nuevos:** `Helpers/LLMSelector.cs`
**Archivos a modificar:** `CVSelector.cs`, `CVGeneratorService.cs`, `GenerateRequest`, `Program.cs`, `App.js`

---

## Priority 3 — Nice to Have

---

### FEATURE-301: Cover Letter Generator

**Epic:** Application Suite
**Labels:** `backend`, `frontend`

**Acceptance Criteria:**
- Checkbox "Also generate cover letter" junto al selector de formato
- Usa el mismo JD y mismo perfil seleccionado
- Exporta como PDF y/o DOCX
- Estructura: apertura → 2 párrafos de experiencia → cierre
- Sin LLM — usa template Mustache + datos del CV generado

**Implementación:**

```csharp
// Helpers/CoverLetterGenerator.cs
public string Generate(CVMaster cv, JobRequirements req, GeneratedCV selectedCV)
{
    var topExp = selectedCV.Experience.First();
    var topSkills = selectedCV.Skills.Take(3).ToList();

    return Render("cover-letter-template.html", new {
        FullName = cv.Header.FullName,
        Email = cv.Header.Email,
        Date = DateTime.Now.ToString("MMMM dd, yyyy"),
        JobTitle = DetectJobTitle(req),
        YearsExp = CalculateTechYears(cv.Experience),
        TopSkills = string.Join(", ", topSkills),
        MostRecentCompany = topExp.Company,
        Achievement = ExtractTopAchievement(topExp.Bullets),
    });
}
```

**Archivos nuevos:** `Helpers/CoverLetterGenerator.cs`, `Templates/cover-letter-template.html`
**Archivos a modificar:** `CVGeneratorService.cs`, `CVController.cs`, `App.js`, `FormatSelector.jsx`

---

### FEATURE-303: Export Settings Persistence

**Epic:** UX
**Labels:** `frontend`

**Acceptance Criteria:**
- Template, formato(s) y page option se guardan en `localStorage`
- Se restauran automáticamente al cargar la app

**Implementación:**

```javascript
// utils/settings.js
const KEY = 'cv-generator-settings';
export const defaultSettings = { template: 'harvard', formats: { pdf: true, docx: false }, allowTwoPages: false };

export function loadSettings() {
  try { return { ...defaultSettings, ...JSON.parse(localStorage.getItem(KEY) || '{}') }; }
  catch { return defaultSettings; }
}

export function saveSettings(s) { localStorage.setItem(KEY, JSON.stringify(s)); }
```

**Archivos nuevos:** `utils/settings.js`
**Archivos a modificar:** `App.js`

---

## Summary

| Feature | Impacto | Esfuerzo | Estado |
|---|---|---|---|
| FEATURE-101: Fuzzy Skill Matching | Alto | Bajo | ✅ Done |
| FEATURE-102: CV Preview backend | Alto | Bajo | ✅ Done |
| FEATURE-102-FE: CV Preview frontend | Alto | Bajo | ⏳ Frontend pending |
| FEATURE-103: Keyboard Shortcuts | Bajo | Bajo | ⏳ Frontend pending |
| FEATURE-201: Generation History | Medio | Medio | ⏳ Frontend pending |
| FEATURE-202: Multi CV Master backend | Alto | Bajo | ✅ Done |
| FEATURE-202-FE: Profile Selector | Medio | Bajo | ⏳ Frontend pending |
| FEATURE-203: LLM Integration | Alto | Alto | 🔒 Needs API key |
| FEATURE-301: Cover Letter | Medio | Medio | ⏳ Not started |
| FEATURE-302: PageConstraint Fix | Medio | Bajo | ✅ Done |
| FEATURE-303: Settings Persistence | Bajo | Bajo | ⏳ Frontend pending |
| FIX-001: KeywordAnalyzer → GeneratedCV | Alto | Bajo | ✅ Done |
| FIX-002: Role mismatch warning | Medio | Bajo | ✅ Done |
| FIX-003: Bullet STAR scoring | Medio | Bajo | ✅ Done |
| FIX-004: SkillNormalizer centralizado | Alto | Bajo | ✅ Done |

**Orden recomendado de implementacion (actualizado):**
1) FEATURE-102-FE → FEATURE-202-FE → FEATURE-103 → FEATURE-303 → FEATURE-201
2) FEATURE-301 → FEATURE-203 (cuando haya API key y budget)
