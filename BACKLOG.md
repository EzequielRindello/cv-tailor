# ER CV Generator — Feature Backlog

> Tickets de desarrollo. Formato: contexto, criterios de aceptación, implementación técnica.

---

## ✅ Implemented

| Feature | Descripción |
|---|---|
| FEATURE-101 | Fuzzy Skill Matching — `SkillNormalizer.cs` centraliza todo el matching |
| FEATURE-102 | CV Preview HTML — `RenderHtmlPublic()` expuesto, `previewHtml` en response |
| FEATURE-102-FE | CV Preview inline en frontend (`CVPreview` + `iframe srcDoc`) |
| FEATURE-103 | Keyboard shortcuts: `Ctrl/Cmd+Enter` (generate) y `Ctrl/Cmd+Shift+V` (paste) |
| FEATURE-201 | Generation history (localStorage, top 10, recarga de JD, clear history) |
| FEATURE-202 | Multi CV Master — `GET /api/cv/profiles`, `ProfileId` en `GenerateRequest` |
| FEATURE-202-FE | Selector de perfil en frontend con envío de `profileId` |
| FEATURE-302 | PageConstraint Fix — estimación por caracteres reemplaza líneas fijas |
| FEATURE-303 | Persistencia de settings de export/template/page mode en localStorage |
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
| PERF-001 | PDF rendering optimizado: browser compartido (lazy-init) y cierre por página |
| TEST-001 | Test runner configurable por perfil (`--profile` o `PROFILE_ID`) |
| ENH-001 | Preview refresh en `/regenerate` con `previewHtml` actualizado |

---

## Priority 1 — Pending

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

## Priority 2 — Pending

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

## Summary

| Feature | Impacto | Esfuerzo | Estado |
|---|---|---|---|
| FEATURE-101: Fuzzy Skill Matching | Alto | Bajo | ✅ Done |
| FEATURE-102: CV Preview backend | Alto | Bajo | ✅ Done |
| FEATURE-102-FE: CV Preview frontend | Alto | Bajo | ✅ Done |
| FEATURE-103: Keyboard Shortcuts | Bajo | Bajo | ✅ Done |
| FEATURE-201: Generation History | Medio | Medio | ✅ Done |
| FEATURE-202: Multi CV Master backend | Alto | Bajo | ✅ Done |
| FEATURE-202-FE: Profile Selector | Medio | Bajo | ✅ Done |
| FEATURE-203: LLM Integration | Alto | Alto | 🔒 Needs API key |
| FEATURE-301: Cover Letter | Medio | Medio | ⏳ Pending |
| FEATURE-302: PageConstraint Fix | Medio | Bajo | ✅ Done |
| FEATURE-303: Settings Persistence | Bajo | Bajo | ✅ Done |
| PERF-001: Shared Puppeteer browser | Alto | Medio | ✅ Done |
| TEST-001: Runner profile configurable | Medio | Bajo | ✅ Done |
| ENH-001: Preview refresh on regenerate | Medio | Bajo | ✅ Done |
| FIX-001: KeywordAnalyzer → GeneratedCV | Alto | Bajo | ✅ Done |
| FIX-002: Role mismatch warning | Medio | Bajo | ✅ Done |
| FIX-003: Bullet STAR scoring | Medio | Bajo | ✅ Done |
| FIX-004: SkillNormalizer centralizado | Alto | Bajo | ✅ Done |

**Orden recomendado de implementacion (actualizado):**
1) FEATURE-301
2) FEATURE-203 (cuando haya API key y budget)
