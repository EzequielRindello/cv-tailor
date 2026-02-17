# ER CV Generator — Future Features Backlog

> Tickets de desarrollo para features futuras. Formato: contexto, criterios de aceptación, implementación técnica.

---

## Priority 1 — Quick Wins

---

### FEATURE-101: Fuzzy Skill Matching

**Epic:** Matching Quality  
**Story Points:** 3  
**Labels:** `backend`, `matching`

**Problema actual:**
El matcher es case-sensitive y literal. "Node.js" no matchea "Node", "TypeScript" no matchea "TS", ".NET Core" no matchea ".NET". Esto reduce artificialmente el match score y puede excluir experiencias relevantes.

**Acceptance Criteria:**
- `.NET` matchea `.NET Core`, `ASP.NET`, `dotnet`
- `Node` matchea `Node.js`, `NodeJS`
- `JS` matchea `JavaScript`
- `TS` matchea `TypeScript`
- `Postgres` matchea `PostgreSQL`
- `k8s` matchea `Kubernetes`
- Matching sigue siendo case-insensitive

**Implementación:**

Crear un diccionario de aliases en `JobDescriptionParser.cs` o un helper nuevo `SkillNormalizer.cs`:

```csharp
public class SkillNormalizer
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "node.js", "node" }, { "nodejs", "node" },
        { "typescript", "ts" }, { "ts", "typescript" },
        { "javascript", "js" }, { "js", "javascript" },
        { "postgresql", "postgres" }, { "postgres", "postgresql" },
        { ".net core", ".net" }, { "asp.net", ".net" }, { "dotnet", ".net" },
        { "kubernetes", "k8s" }, { "k8s", "kubernetes" },
        { "amazon web services", "aws" },
        { "google cloud platform", "gcp" },
        { "c sharp", "c#" }, { "csharp", "c#" },
        { "react.js", "react" }, { "reactjs", "react" },
        { "vue.js", "vue" }, { "angular.js", "angular" },
    };

    public static string Normalize(string skill)
    {
        var lower = skill.Trim().ToLower();
        return Aliases.TryGetValue(lower, out var normalized) ? normalized : lower;
    }

    public static bool FuzzyMatch(string a, string b)
    {
        var na = Normalize(a);
        var nb = Normalize(b);
        return na == nb || na.Contains(nb) || nb.Contains(na);
    }
}
```

Reemplazar todos los `.Equals(..., OrdinalIgnoreCase)` y `.Contains(..., OrdinalIgnoreCase)` en `RelevanceScorer.cs`, `MatchScoreCalculator.cs`, y `MissingSkillsAnalyzer.cs` por `SkillNormalizer.FuzzyMatch(a, b)`.

**Archivos a modificar:** `RelevanceScorer.cs`, `MatchScoreCalculator.cs`, `MissingSkillsAnalyzer.cs`  
**Archivo nuevo:** `Helpers/SkillNormalizer.cs`

---

### FEATURE-102: CV Preview (HTML inline)

**Epic:** UX  
**Story Points:** 5  
**Labels:** `frontend`, `backend`

**Problema actual:**
El usuario genera el CV, lo descarga, abre el PDF, ve que algo no le gusta, vuelve a la app. Demasiados pasos para iterar.

**Acceptance Criteria:**
- Después de generar, aparece un preview del CV en la misma página
- El preview usa el HTML del template renderizado (no el PDF)
- El usuario puede hacer scroll si tiene más de una página
- Los botones de descarga siguen disponibles junto al preview
- El preview se actualiza si el usuario usa el Bullet Editor y regenera

**Implementación:**

Backend — agregar `html` al response de `/generate`:

```csharp
// CVGeneratorService.cs — en GenerateCVAsync()
var html = renderer.RenderHtmlPublic(generatedCV); // hacer RenderHtml público

return (documents, analysis, html); // agregar html al return
```

```csharp
// CVController.cs
return Ok(new
{
    success = true,
    files = ...,
    matchScore = ...,
    // ... resto del análisis
    previewHtml = html   // ← nuevo
});
```

Frontend — mostrar el HTML en un iframe después de generación:

```jsx
// En App.js, al estado de analysisResult agregar:
const [previewHtml, setPreviewHtml] = useState(null);

// En handleGenerate, después de setAnalysisResult:
setPreviewHtml(response.data.previewHtml);

// Componente CVPreview:
export default function CVPreview({ html }) {
  if (!html) return null;
  return (
    <div className="cv-preview-wrapper">
      <h6>Preview</h6>
      <div className="cv-preview-frame">
        <iframe
          srcDoc={html}
          title="CV Preview"
          style={{ width: '100%', height: '1100px', border: 'none' }}
          sandbox="allow-same-origin"
        />
      </div>
    </div>
  );
}
```

**Nota:** El preview en iframe puede verse levemente distinto al PDF por diferencias entre Chromium headless y el browser del usuario. Es aceptable para uso personal.

**Archivos a modificar:** `DocumentRenderer.cs`, `CVGeneratorService.cs`, `CVController.cs`, `App.js`  
**Archivo nuevo:** `components/CVPreview/CVPreview.jsx`

---

### FEATURE-103: Keyboard Shortcut & Auto-paste from Clipboard

**Epic:** UX  
**Story Points:** 2  
**Labels:** `frontend`

**Problema actual:**
El workflow manual es: copiar JD → click en textarea → Ctrl+V → click en Generate. Se puede reducir a: copiar JD → un shortcut.

**Acceptance Criteria:**
- `Ctrl+Shift+V` (o `Cmd+Shift+V` en Mac) pega el clipboard en el textarea automáticamente
- `Ctrl+Enter` ejecuta Generate si el textarea tiene contenido
- Un tooltip en el textarea muestra los shortcuts disponibles

**Implementación:**

```jsx
// En JobDescriptionInput.jsx o en App.js
useEffect(() => {
  const handleKeydown = async (e) => {
    // Ctrl/Cmd + Shift + V → paste clipboard
    if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'V') {
      e.preventDefault();
      try {
        const text = await navigator.clipboard.readText();
        if (text) onChange({ target: { value: text } });
      } catch (err) {
        // Clipboard API no disponible — ignorar silenciosamente
      }
    }

    // Ctrl/Cmd + Enter → generate
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

## Priority 2 — Game Changers

---

### FEATURE-201: Generation History (localStorage)

**Epic:** Persistencia  
**Story Points:** 5  
**Labels:** `frontend`

**Problema actual:**
Cada vez que cerrás el browser perdés todo. Si aplicaste a 10 trabajos no tenés registro de qué CV generaste para cada uno.

**Acceptance Criteria:**
- Últimas 10 generaciones guardadas en `localStorage`
- Cada entrada muestra: título del rol extraído del JD, fecha, match score, template usado
- Botón "History" en el header abre un panel lateral o modal
- Cada entrada tiene botón "Regenerate" que pre-carga el JD en el textarea
- Botón "Clear history"
- El historial sobrevive refresh pero no transferencia entre dispositivos (es localStorage)

**Implementación:**

```javascript
// utils/history.js
const STORAGE_KEY = 'cv-generator-history';
const MAX_ENTRIES = 10;

export function saveToHistory(entry) {
  const history = getHistory();
  const newEntry = {
    id: Date.now(),
    jobTitle: extractJobTitle(entry.jobDescription),  // regex básico
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
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]');
  } catch {
    return [];
  }
}

export function clearHistory() {
  localStorage.removeItem(STORAGE_KEY);
}

function extractJobTitle(jd) {
  // Tomar la primera línea no vacía, truncar a 60 chars
  const firstLine = jd.split('\n').find(l => l.trim().length > 3) || 'Unknown Role';
  return firstLine.trim().slice(0, 60);
}
```

```jsx
// components/HistoryPanel/HistoryPanel.jsx
export default function HistoryPanel({ onRestore, onClose }) {
  const [history, setHistory] = useState(getHistory());

  const handleClear = () => {
    clearHistory();
    setHistory([]);
  };

  return (
    <div className="history-panel">
      <div className="history-header">
        <h6>Generation History</h6>
        <button onClick={onClose}>✕</button>
      </div>
      {history.length === 0 ? (
        <p className="history-empty">No history yet.</p>
      ) : (
        <>
          <ul className="history-list">
            {history.map(entry => (
              <li key={entry.id} className="history-entry">
                <div className="history-title">{entry.jobTitle}</div>
                <div className="history-meta">
                  {new Date(entry.generatedAt).toLocaleDateString()} ·{' '}
                  {entry.matchScore ? `${entry.matchScore}% match` : ''} ·{' '}
                  {entry.template}
                </div>
                <button onClick={() => onRestore(entry.jobDescription)}>
                  ↩ Regenerate
                </button>
              </li>
            ))}
          </ul>
          <button className="history-clear" onClick={handleClear}>Clear history</button>
        </>
      )}
    </div>
  );
}
```

**Archivos nuevos:** `utils/history.js`, `components/HistoryPanel/HistoryPanel.jsx`  
**Archivos a modificar:** `App.js`, `Header.jsx`

---

### FEATURE-202: Multi CV Master / Perfiles

**Epic:** Flexibilidad  
**Story Points:** 8  
**Labels:** `backend`, `frontend`

**Problema actual:**
Solo existe un `cv-master.json`. Si querés mantener un perfil tech y uno más generalista, tenés que editar el JSON a mano antes de generar.

**Acceptance Criteria:**
- Soporte para múltiples archivos en `Data/` (ej: `cv-master-tech.json`, `cv-master-ops.json`)
- Backend expone endpoint `GET /api/cv/profiles` que devuelve los nombres disponibles
- Frontend muestra un selector de perfil antes del textarea
- El perfil seleccionado se envía en el request de generate
- Default: el primer perfil disponible (o el que se llame `cv-master.json`)

**Implementación:**

```csharp
// CVController.cs — nuevo endpoint
[HttpGet("profiles")]
public IActionResult GetProfiles()
{
    var dataPath = Path.Combine(_env.ContentRootPath, "Data");
    var profiles = Directory.GetFiles(dataPath, "cv-master*.json")
        .Select(f => new {
            id = Path.GetFileNameWithoutExtension(f),
            name = Path.GetFileNameWithoutExtension(f)
                       .Replace("cv-master-", "")
                       .Replace("cv-master", "Default")
        })
        .ToList();

    return Ok(profiles);
}
```

```csharp
// GenerateRequest.cs — agregar campo
public string ProfileId { get; set; } = "cv-master";

// CVGeneratorService.cs — usar ProfileId en LoadCVMasterAsync
private async Task<CVMaster> LoadCVMasterAsync(string profileId = "cv-master")
{
    var path = Path.Combine(_env.ContentRootPath, "Data", $"{profileId}.json");
    if (!File.Exists(path))
        throw new FileNotFoundException($"Profile '{profileId}' not found");
    // ... resto igual
}
```

```jsx
// Frontend — ProfileSelector.jsx (componente simple similar a TemplateSelector)
useEffect(() => {
  axios.get(`${API_BASE_URL}/cv/profiles`)
    .then(r => setProfiles(r.data))
    .catch(() => {}); // silencioso si falla
}, []);
```

**Archivos a modificar:** `CVController.cs`, `CVGeneratorService.cs`, `GenerateRequest`  
**Archivo nuevo:** `components/ProfileSelector/ProfileSelector.jsx`

---

### FEATURE-203: LLM Integration (Claude API)

**Epic:** Matching Quality  
**Story Points:** 13  
**Labels:** `backend`, `ai`

**Problema actual:**
El matching es puramente léxico. Claude (o cualquier LLM) puede entender que "lideré un equipo de 5" es evidencia de "leadership", que "Next.js" es relevante para un JD que pide "React", o que "mejoré la performance de queries" matchea "database optimization".

**Acceptance Criteria:**
- Toggle opcional en UI: "Use AI matching (slower, smarter)"
- Si está activado, el LLM selecciona experiencias y bullets en lugar del scorer clásico
- Si la API falla o el toggle está off, usa el sistema clásico (fallback automático)
- La API key se configura en `appsettings.json` o variable de entorno, nunca en el frontend
- El tiempo de respuesta adicional se muestra en UI ("AI matching enabled — may take 10s")

**Implementación:**

```csharp
// appsettings.json
{
  "LLM": {
    "Enabled": false,
    "ApiKey": "",  // o usar env var ANTHROPIC_API_KEY
    "Model": "claude-haiku-4-5-20251001",  // haiku es más rápido y barato para esto
    "FallbackToClassic": true
  }
}
```

```csharp
// Helpers/LLMSelector.cs
public class LLMSelector
{
    private readonly IConfiguration _config;

    public async Task<GeneratedCV?> TrySelectWithLLM(CVMaster cv, JobRequirements req)
    {
        try
        {
            var prompt = $@"
You are a resume expert. Given the CV Master and job requirements below,
select the 3 most relevant experiences and the 3 best bullets for each.

Job Requirements (already parsed):
{JsonSerializer.Serialize(req)}

CV Master Experiences:
{JsonSerializer.Serialize(cv.Experience.Select((e, i) => new { index = i, e.Role, e.Company, e.Bullets, e.Skills }))}

Respond ONLY with valid JSON in this exact format:
{{
  ""selectedExperiences"": [
    {{ ""index"": 0, ""selectedBullets"": [0, 2, 4] }},
    {{ ""index"": 2, ""selectedBullets"": [1, 3, 5] }},
    {{ ""index"": 4, ""selectedBullets"": [0, 1, 2] }}
  ]
}}

Rules:
- Select exactly 3 experiences (or fewer if CV has fewer than 3)
- Select exactly 3 bullets per experience (or all if fewer than 3)
- Use indices, never modify content
- Prioritize quantifiable achievements and skill matches
";
            var response = await CallAnthropicAPI(prompt);
            var selection = JsonSerializer.Deserialize<LLMSelection>(response);
            return BuildCVFromSelection(cv, selection);
        }
        catch
        {
            return null; // fallback al scorer clásico
        }
    }
}
```

```csharp
// CVSelector.cs — modificar SelectAndAdaptCV
public async Task<GeneratedCV> SelectAndAdaptCV(CVMaster cv, JobRequirements req, bool useLLM = false, bool allowTwoPages = false)
{
    if (useLLM)
    {
        var llmResult = await _llmSelector.TrySelectWithLLM(cv, req);
        if (llmResult != null) return llmResult;
        // Si falla, cae al clásico
    }
    // ... lógica clásica existente
}
```

**Costo estimado:** ~$0.001–0.003 por generación con Haiku. Para uso personal es despreciable.

**Archivos nuevos:** `Helpers/LLMSelector.cs`  
**Archivos a modificar:** `CVSelector.cs`, `CVGeneratorService.cs`, `GenerateRequest`, `Program.cs`, `App.js`

---

## Priority 3 — Nice to Have

---

### FEATURE-301: Cover Letter Generator

**Epic:** Application Suite  
**Story Points:** 8  
**Labels:** `backend`, `frontend`

**Problema actual:**
El CV está cubierto pero la cover letter sigue siendo manual. Usan exactamente la misma información.

**Acceptance Criteria:**
- Checkbox "Also generate cover letter" junto al formato selector
- Cover letter usa el mismo JD y mismo CV Master seleccionado
- Exporta como PDF y/o DOCX (mismos formatos que el CV)
- Estructura fija: apertura → 2 párrafos de experiencia → cierre
- No usa LLM — construye el texto con templates y datos del CV (igual que el summary)

**Implementación:**

Template `cover-letter-template.html` siguiendo el mismo approach de Mustache que los CV templates.

```csharp
// Helpers/CoverLetterGenerator.cs
public string Generate(CVMaster cv, JobRequirements req, GeneratedCV selectedCV)
{
    var topExp = selectedCV.Experience.First();
    var achievement = ExtractFirstAchievement(topExp.Bullets);
    var topSkills = selectedCV.Skills.Take(3).ToList();
    var jobTitle = ExtractJobTitle(req.RequiredExperience.FirstOrDefault() ?? "");
    var yearsExp = CalculateYears(cv.Experience);

    return Render("cover-letter-template.html", new {
        FullName = cv.Header.FullName,
        Email = cv.Header.Email,
        Phone = cv.Header.Phone,
        Date = DateTime.Now.ToString("MMMM dd, yyyy"),
        JobTitle = jobTitle,
        YearsExp = yearsExp,
        TopSkills = FormatList(topSkills),
        MostRecentCompany = topExp.Company,
        Achievement = achievement,
        Skill1 = topSkills.ElementAtOrDefault(0) ?? "",
        Skill2 = topSkills.ElementAtOrDefault(1) ?? "",
        SeniorityLevel = req.SeniorityLevel
    });
}
```

**Archivos nuevos:** `Helpers/CoverLetterGenerator.cs`, `Templates/cover-letter-template.html`  
**Archivos a modificar:** `CVGeneratorService.cs`, `CVController.cs`, `App.js`, `FormatSelector.jsx`

---

### FEATURE-302: PageConstraint Fix — Real Height Measurement

**Epic:** Quality / Bug Fix  
**Story Points:** 5  
**Labels:** `backend`, `bug`

**Problema actual:**
`PageConstraintHelper` cuenta "líneas" con constantes fijas (`BulletLines = 1`, etc.) pero una bullet de 200 caracteres ocupa 2 líneas en el PDF real. El resultado es que CVs con bullets largas pueden desbordarse a segunda página silenciosamente.

**Acceptance Criteria:**
- El cálculo de altura considera el largo aproximado de cada bullet
- Una bullet que supere ~90 caracteres cuenta como 2 líneas
- El cálculo sigue siendo una heurística (no renderizado real) pero más precisa
- No introduce dependencias nuevas

**Implementación:**

```csharp
// PageConstraintHelper.cs — reemplazar BulletLines constante
private int CalculateBulletLines(string bullet, int charsPerLine = 90)
{
    if (string.IsNullOrEmpty(bullet)) return 1;
    return (int)Math.Ceiling((double)bullet.Length / charsPerLine);
}

// En CalculateTotalLines:
foreach (var exp in cv.Experience)
{
    lines += ExperienceTitleLines;
    lines += exp.Bullets.Sum(b => CalculateBulletLines(b));
}
```

**Archivos a modificar:** `PageConstraintHelper.cs`

---

### FEATURE-303: Export Settings Persistence

**Epic:** UX  
**Story Points:** 2  
**Labels:** `frontend`

**Problema actual:**
Cada vez que abrís la app volvés al default: Harvard template, PDF only, 1 page. Si siempre usás Minimalist + PDF + 1 page, tenés que setear eso en cada sesión.

**Acceptance Criteria:**
- Template, formato(s) y page option se guardan en `localStorage`
- Se restauran automáticamente al cargar la app
- Se actualizan cada vez que el usuario cambia alguno

**Implementación:**

```javascript
// utils/settings.js
const KEY = 'cv-generator-settings';

export const defaultSettings = {
  template: 'harvard',
  formats: { pdf: true, docx: false },
  allowTwoPages: false,
};

export function loadSettings() {
  try {
    return { ...defaultSettings, ...JSON.parse(localStorage.getItem(KEY) || '{}') };
  } catch {
    return defaultSettings;
  }
}

export function saveSettings(settings) {
  localStorage.setItem(KEY, JSON.stringify(settings));
}
```

```javascript
// App.js
const saved = loadSettings();
const [template, setTemplate] = useState(saved.template);
const [selectedFormats, setSelectedFormats] = useState(saved.formats);
const [allowTwoPages, setAllowTwoPages] = useState(saved.allowTwoPages);

// En cada setter, también persistir:
const handleTemplateChange = (t) => {
  setTemplate(t);
  saveSettings({ template: t, formats: selectedFormats, allowTwoPages });
};
```

**Archivos nuevos:** `utils/settings.js`  
**Archivos a modificar:** `App.js`

---

## Resumen

| Feature | Impacto | Esfuerzo | Prioridad |
|---|---|---|---|
| FEATURE-101: Fuzzy Skill Matching | Alto | Bajo | P1 |
| FEATURE-102: CV Preview | Alto | Medio | P1 |
| FEATURE-103: Keyboard Shortcuts | Bajo | Bajo | P1 |
| FEATURE-201: Generation History | Medio | Medio | P2 |
| FEATURE-202: Multi CV Master | Alto | Medio | P2 |
| FEATURE-203: LLM Integration | Alto | Alto | P2 |
| FEATURE-301: Cover Letter | Medio | Medio | P3 |
| FEATURE-302: PageConstraint Fix | Medio | Bajo | P3 |
| FEATURE-303: Settings Persistence | Bajo | Bajo | P3 |

**Recomendación de orden si arrancás de nuevo:** 101 → 302 → 103 → 303 → 102 → 201 → 202 → 203
