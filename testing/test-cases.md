# ER CV Generator — Manual Test Cases

> Instrucciones: para cada test case, pegá el JD en la app, generá el CV, y reportame:
> 1. Screenshot o texto del **Match Score** (overall % + breakdown)
> 2. Screenshot o texto del **ATS Score**
> 3. Lista de **Missing Skills**
> 4. El **Summary** generado (el texto)
> 5. Qué **3 experiencias** aparecieron y en qué orden
> 6. Los **bullets seleccionados** de la primera experiencia
> 7. Si algo se ve roto, mal formateado, o inesperado

---

## TC-01 — Perfect Match (Senior Backend, Go + Kafka)

**Objetivo:** Verificar que la app detecta un match casi perfecto y selecciona las experiencias/bullets correctos.

**Configuración:** Template: Harvard · Formato: PDF · 1 página

**JD a pegar:**
```
Senior Backend Engineer — Payments Infrastructure

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

We offer a collaborative remote-first environment with 2-week agile sprints.
```

**Lo que esperamos ver:**
- Match Score: 85%+
- Stripe aparece primero, Airbnb segundo, Twilio tercero
- Bullets seleccionados de Stripe deben incluir el de Kafka/2M transactions y el de Redis rate limiting
- Summary debe mencionar Go, Kafka, PostgreSQL o Redis
- Missing Skills: pocos o ninguno requerido
- ATS Score: 80%+
- `LowMatchWarning`: null (no debe aparecer)

**Red flags a reportar:**
- Si el Match Score es menor a 70%
- Si Twilio aparece antes que Airbnb
- Si el summary dice "Software Developer" en lugar de algo como "Senior Backend Engineer"
- Si Missing Skills lista Go, Kafka, o PostgreSQL (que sí están en el CV)

---

## TC-02 — Good Match (Frontend React, Mid-level)

**Objetivo:** Verificar selección correcta para un rol frontend puro, y que el seniority se detecta como mid-level.

**Configuración:** Template: Minimalist · Formato: PDF · 1 página

**JD a pegar:**
```
Frontend Engineer — React / TypeScript

About the role:
We're building the next generation of developer tooling and need a frontend engineer to own our web application.

What you'll do:
- Build and maintain React components and pages
- Work with TypeScript in a modern Next.js codebase
- Write unit and integration tests with Jest and Testing Library
- Collaborate with designers using Figma specs
- Participate in code reviews and agile ceremonies

Requirements:
- 3+ years experience with React and TypeScript
- Solid understanding of modern JavaScript (ES2020+)
- Experience with Next.js or similar SSR framework
- Familiarity with REST APIs and GraphQL
- Experience writing tests (Jest, Cypress, or Playwright)
- Understanding of CSS, responsive design
- Git and GitHub workflow

Nice to have:
- Experience with Tailwind CSS
- Familiarity with CI/CD pipelines
- Open source contributions
```

**Lo que esperamos ver:**
- Match Score: 70-85%
- Airbnb aparece primero (tiene React, TypeScript, GraphQL, Jest, Cypress)
- Stripe segundo (TypeScript, GitHub Actions, pero menos frontend)
- Twilio tercero o no aparece
- Summary menciona "Frontend Developer" o "Full Stack Developer", no "Backend"
- Bullets seleccionados de Airbnb incluyen el de React + host dashboard y el de GraphQL
- Missing Skills: posiblemente "Tailwind CSS", "Testing Library", "Playwright"
- ATS Score: 75%+

**Red flags:**
- Si Twilio aparece primero (tiene muy poco frontend)
- Si el summary dice "Go" o menciona Kafka (no relevantes para este JD)
- Si "React" o "TypeScript" aparecen en Missing Skills

---

## TC-03 — Partial Match (DevOps / Platform Engineer)

**Objetivo:** Verificar que la app maneja bien un JD donde el perfil matchea parcialmente — hay skills relevantes pero no es el core del CV.

**Configuración:** Template: Harvard · Formato: PDF + DOCX · 1 página

**JD a pegar:**
```
Platform Engineer — DevOps / Infrastructure

We're looking for a Platform Engineer to own our cloud infrastructure and internal developer platform.

Responsibilities:
- Design and maintain Kubernetes clusters on AWS EKS
- Build and improve CI/CD pipelines (GitHub Actions, ArgoCD)
- Write infrastructure as code using Terraform
- Monitor system health with Prometheus and Grafana
- Collaborate with development teams to improve deployment workflows
- Manage container lifecycle with Docker
- Implement security best practices and access controls

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
- Experience with service mesh (Istio, Linkerd)
- Cost optimization experience
```

**Lo que esperamos ver:**
- Match Score: 50-70% (el perfil tiene las tools pero no es un DevOps puro)
- Stripe primero (Kubernetes, ArgoCD, GitHub Actions, AWS, Grafana, Prometheus)
- Twilio segundo o tercero
- Summary: algo como "Senior Software Engineer" con mención de Kubernetes/AWS — no debería decir "DevOps Engineer" ya que el rol más reciente es SWE
- Missing Skills: "Terraform" (está en skills del CV master pero verificar), "Helm", "Istio", "Linux"
- ATS Score: 65%+
- `LowMatchWarning`: null (no debería triggerear con 50-70%)

**Red flags:**
- Si Match Score es mayor a 80% (sería inflado — este CV no es un DevOps puro)
- Si Airbnb aparece primero (tiene menos infra que Stripe)
- Si Terraform aparece en Missing Skills (está en el cv-master.json en cloud skills)

---

## TC-04 — Poor Match (iOS Developer) — Mismatch Warning Test

**Objetivo:** Verificar que el sistema detecta un mismatch grave y muestra `LowMatchWarning`.

**Configuración:** Template: Harvard · Formato: PDF · 1 página

**JD a pegar:**
```
iOS Software Engineer

We are looking for an iOS engineer to build native mobile experiences.

Requirements:
- 3+ years iOS development experience
- Proficiency in Swift and Objective-C
- Experience with UIKit and SwiftUI
- Xcode and Instruments proficiency
- Knowledge of iOS SDK, Core Data, and push notifications
- Experience with CocoaPods or Swift Package Manager
- Understanding of MVC and MVVM patterns for iOS
- App Store submission experience

Nice to have:
- Experience with React Native
- Backend API integration experience
- Firebase or similar BaaS
```

**Lo que esperamos ver:**
- Match Score: <30%
- `LowMatchWarning` presente y visible en el response (el frontend debería mostrarlo)
- Missing Skills: Swift, Objective-C, SwiftUI, UIKit, Xcode, CocoaPods, Core Data
- El CV igual se genera (no debe romper) pero con las experiencias más "cercanas"
- ATS Score: bajo por keyword density
- Summary: genérico, sin skills de iOS (porque no las tiene)

**Red flags:**
- Si `LowMatchWarning` es null (el sistema debería haberlo detectado)
- Si Match Score es mayor a 30%
- Si la app crashea o devuelve error en lugar de generar un CV con warning

---

## TC-05 — Alias / Fuzzy Matching Test

**Objetivo:** Verificar que `SkillNormalizer` resuelve aliases correctamente. El JD usa nombres alternativos de skills que sí están en el CV.

**Configuración:** Template: Harvard · Formato: PDF · 1 página

**JD a pegar:**
```
Full Stack Developer

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
We use AWS and containerized deployments with Docker.
```

**Lo que esperamos ver:**
- "NodeJS" debe matchear "Node.js" en el CV → aparece en matched, no en missing
- "ReactJS" debe matchear "React"
- "PostgresQL" / "Postgres" debe matchear "PostgreSQL"
- "k8s" debe matchear "Kubernetes"
- "TS" debe matchear "TypeScript"
- Match Score: 75%+
- Missing Skills: vacío o solo skills realmente ausentes
- Skills section en el CV generado debe incluir Node.js, React, PostgreSQL, TypeScript

**Red flags — estos serían bugs del SkillNormalizer:**
- Si "NodeJS", "ReactJS", "Postgres", "k8s", o "TS" aparecen en Missing Skills
- Si Match Score es menor a 60% (indicaría que los aliases no resolvieron)
- Si el Skills section del CV generado no incluye los skills matcheados

---

## TC-06 — Two-Page Mode Test

**Objetivo:** Verificar que el modo 2 páginas incluye más experiencias, más bullets, y proyectos.

**Configuración:** Template: Harvard · Formato: PDF · **2 páginas** · misma JD que TC-01

**JD a pegar:** (misma que TC-01 — copiar el mismo texto)

**Lo que esperamos ver:**
- 4 experiencias en lugar de 3 (Box Internship debe aparecer)
- 4 bullets por experiencia en lugar de 3
- Al menos 1 proyecto al final (OpenMQ o Querywise, el más relevante primero)
- El PDF tiene 2 páginas (o al menos se extiende más que el de TC-01)
- Match Score igual que TC-01 (el score se calcula sobre el master, no el output)

**Comparar con TC-01:**
- ¿Cuántos bullets más tiene cada experiencia?
- ¿Qué proyecto apareció? ¿Es el correcto para el JD de backend/Go?
- ¿La Box internship aportó algo relevante o es ruido?

---

## TC-07 — Seniority Detection Test

**Objetivo:** Verificar que el JD "Junior" no triggerear seniority mismatch y que el summary ajusta el título.

**Configuración:** Template: Minimalist · Formato: PDF · 1 página

**JD a pegar:**
```
Junior Software Engineer — Full Stack

We're a startup looking for a junior engineer eager to learn and grow.

You will:
- Build features across our React frontend and Node.js backend
- Write and maintain PostgreSQL queries and migrations
- Participate in code reviews
- Contribute to agile ceremonies and sprint planning

Requirements:
- 0-2 years experience
- Basic knowledge of React or similar frontend framework
- Understanding of REST APIs
- Familiarity with SQL and databases
- Git basics

No senior experience required. We value curiosity and willingness to learn over years of experience.
```

**Puntos clave a verificar:**
- "No senior experience required" — el parser NO debe detectar "senior" de esa frase (negation detection)
- Seniority detectado debe ser "junior" o "entry-level", no "senior"
- `SeniorityMatch` en el response: probablemente `false` (el CV es Senior, el JD pide Junior)
- Summary no debe decir "Senior Software Engineer" si el JD pide junior

**Red flags:**
- Si el seniority detectado es "senior" (falla de negation detection)
- Si Match Score tiene penalización enorme por seniority mismatch (sería injusto — estar over-qualified no debería destruir el score)

---

## Cómo reportarme los resultados

Para cada test case, pasame:

```
TC-0X Results:
- Match Score: XX% (Skills: X/X · Keywords: X/X · Seniority: true/false)
- LowMatchWarning: [null | "texto del warning"]
- ATS Score: XX
- Summary generado: "[texto completo]"
- Experiencias seleccionadas: 1. Company A · 2. Company B · 3. Company C
- Bullets exp 1 (primeros 2): "[bullet 1]" / "[bullet 2]"
- Missing Skills (required): [lista]
- Algo roto o inesperado: [descripción o "nada"]
```

Si algo falla, el contexto exacto me ayuda a identificar si es un bug de parsing, scoring, o rendering.
