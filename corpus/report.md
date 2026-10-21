# Job-post corpus: relevance report

Generated 2026-10-21 from 194 posts published between 2026-07-22 and 2026-10-20 (freshness window 90 days). Skills are what `JobDescriptionParser` extracts, so this report measures the parser as much as the market.

Regenerate: `dotnet run --project ErCv.Collector -- report`. Source posts: `corpus/postings/`.

## Corpus

| Source | Posts |
|---|---|
| Greenhouse | 114 |
| Ashby | 48 |
| Lever | 28 |
| Workable | 4 |

| Role family | Posts |
|---|---|
| software | 41 |
| ml | 32 |
| devops | 29 |
| data | 25 |
| security | 20 |
| backend | 13 |
| mobile | 11 |
| frontend | 10 |
| fullstack | 9 |
| qa | 4 |

| Seniority (from the title) | Posts |
|---|---|
| intern | 13 |
| junior | 10 |
| mid | 81 |
| senior | 42 |
| staff | 36 |
| lead | 3 |
| principal | 9 |

| Month posted | Posts |
|---|---|
| 2026-07 | 6 |
| 2026-08 | 16 |
| 2026-09 | 56 |
| 2026-10 | 116 |

Companies: 35.

## Skill frequency over time

Share of the posts published each month that the parser finds the skill in (top 30 overall). Months with few posts swing a lot; read the column counts first.

| Skill | All posts | 2026-07 (n=6) | 2026-08 (n=16) | 2026-09 (n=56) | 2026-10 (n=116) |
|---|---|---|---|---|---|
| python | 89 (46%) | 83% | 38% | 39% | 48% |
| observability | 68 (35%) | 33% | 31% | 34% | 36% |
| ai agents | 62 (32%) | 50% | 38% | 36% | 28% |
| machine learning | 54 (28%) | 33% | 25% | 27% | 28% |
| distributed systems | 49 (25%) | 0% | 31% | 30% | 23% |
| go | 48 (25%) | 17% | 19% | 16% | 30% |
| typescript | 48 (25%) | 33% | 25% | 27% | 23% |
| aws | 45 (23%) | 17% | 31% | 20% | 24% |
| llm | 40 (21%) | 50% | 31% | 20% | 18% |
| gcp | 36 (19%) | 17% | 13% | 14% | 22% |
| java | 34 (18%) | 0% | 25% | 14% | 19% |
| kubernetes | 34 (18%) | 0% | 19% | 18% | 18% |
| on-call | 33 (17%) | 0% | 25% | 13% | 19% |
| javascript | 31 (16%) | 0% | 19% | 20% | 15% |
| react | 31 (16%) | 17% | 13% | 27% | 11% |
| sql | 31 (16%) | 17% | 31% | 14% | 15% |
| ci/cd | 28 (14%) | 33% | 19% | 14% | 13% |
| incident response | 27 (14%) | 0% | 6% | 16% | 15% |
| azure | 26 (13%) | 0% | 6% | 11% | 16% |
| networking | 24 (12%) | 0% | 13% | 16% | 11% |
| system design | 24 (12%) | 50% | 13% | 11% | 11% |
| github | 19 (10%) | 0% | 13% | 13% | 9% |
| infrastructure as code | 19 (10%) | 17% | 13% | 5% | 11% |
| terraform | 19 (10%) | 0% | 19% | 5% | 11% |
| data modeling | 18 (9%) | 17% | 6% | 7% | 10% |
| openai | 18 (9%) | 0% | 6% | 18% | 6% |
| rust | 18 (9%) | 17% | 13% | 13% | 7% |
| c++ | 17 (9%) | 17% | 0% | 11% | 9% |
| code review | 17 (9%) | 17% | 13% | 9% | 8% |
| algorithms | 16 (8%) | 17% | 0% | 5% | 10% |

## Skills the parser misses

Terms that recent posts use but the skill dictionary does not have, so the parser never extracts them, match scores ignore them and the gaps tab cannot mention them. Candidates for `Data/skills.json`; some are concepts rather than skills and a person should decide.

### Known technologies and practices not in the dictionary

From a reviewer watch list (`RelevanceReport.WatchList`), in at least 2 posts.

| Term | Posts | Share | Families |
|---|---|---|---|
| agents | 73 | 38% | ml 20, software 16, security 9 |
| scalability | 30 | 15% | devops 8, software 6, data 5 |
| experimentation | 27 | 14% | ml 7, data 5, fullstack 4 |
| Claude | 13 | 7% | software 4, ml 3, devops 2 |
| Salesforce | 9 | 5% | fullstack 2, software 2, backend 1 |
| Anthropic | 8 | 4% | fullstack 2, software 2, backend 1 |
| blockchain | 6 | 3% | backend 1, data 1, frontend 1 |
| Argo | 5 | 3% | devops 3, data 1, fullstack 1 |
| Copilot | 5 | 3% | software 3, devops 1, mobile 1 |
| web3 | 5 | 3% | backend 1, frontend 1, fullstack 1 |
| Okta | 3 | 2% | ml 2, devops 1 |
| buildkite | 3 | 2% | qa 2, security 1 |
| performance tuning | 3 | 2% | backend 2, fullstack 1 |
| transformers | 3 | 2% | ml 3 |
| Combine | 2 | 1% | fullstack 1, mobile 1 |
| Gemini | 2 | 1% | ml 2 |
| Ray | 2 | 1% | ml 2 |
| gdpr | 2 | 1% | security 2 |
| iso 27001 | 2 | 1% | security 2 |
| load balancing | 2 | 1% | devops 2 |
| multithreading | 2 | 1% | fullstack 1, software 1 |
| pci | 2 | 1% | security 2 |
| soc 2 | 2 | 1% | security 2 |
| tcp/ip | 2 | 1% | devops 1, security 1 |

### Other product-looking terms (review by hand)

CamelCase or `*.js` words in at least 3 posts that are not in the dictionary, the watch list or a company name in the corpus. Expect some noise (other companies' products, brand names).

SaaS (21), CapitalG (6), DatadogLife (6), ElevenAPI (6), ElevenAgents (6), ElevenCreative (6), PinFlex (6), SpaceX (6), SpaceXSI (6), WeWork (6), ChatGPT (5), SpaceXAI (4), DataFusion (3), LiveKit (3), NetSuite (3), RemoteUS (3)

## Skills the parser gets wrong

### Dictionary words that match ordinary English

Skills found in at least 3 posts where, in most of those posts, every match is written in lowercase (products are usually capitalized: "Next.js", "Go"). Lowercase is a hint, not proof: concepts such as microservices are normally lowercase too, so read the example. The real false positives inflate match scores and highlight the wrong words.

| Skill | Posts | Only lowercase prose | Example |
|---|---|---|---|
| observability | 68 | 57 | "...th dbt tests, CI/CD, and data observability - Experience evangelizing and..." |
| distributed systems | 49 | 45 | "...tical cloud-native enterprise distributed systems. - Strong Proficiency in Java..." |
| on-call | 33 | 31 | "...thin team services. Fully own on-call responsibilities, driving inc..." |
| incident response | 27 | 23 | "...anges, backups, restores, and incident response - Reducing operational load b..." |
| networking | 24 | 23 | "...Karpenter autoscaling, Cilium networking, and ArgoCD-driven GitOps dep..." |
| system design | 24 | 21 | "...xperience contributing to the system design or architecture (architecture..." |
| data modeling | 18 | 18 | "...forcing best practices around data modeling, testing, documentation, and..." |
| algorithms | 16 | 16 | "...principles (data structures, algorithms, version control) - Familiari..." |
| code review | 17 | 15 | "..., testing, documentation, and code review Qualifications Twilio value..." |
| infrastructure as code | 19 | 12 | "...P, with a cloud warehouse and infrastructure as code. You provision, deploy, monit..." |
| data structures | 10 | 10 | "...tware engineering principles (data structures, algorithms, version control)..." |
| dbt | 10 | 10 | "...rks, including expertise with dbt tests, CI/CD, and data observ..." |
| event-driven | 10 | 10 | "...rs use to build long-running, event-driven, and scheduled applications...." |
| evals | 8 | 8 | "...lly moves loss and downstream evals. - Treat data quality as a sy..." |
| concurrency | 7 | 7 | "...e: realtime inference on GPU, concurrency, autoscaling, observability,..." |
| design systems | 10 | 7 | "...tain and enhance Ramp's brand design system and component library, ensuri..." |
| accessibility | 8 | 6 | "...visual and interaction craft, accessibility, performance, resilience, ins..." |
| microservices | 6 | 6 | "...specs, and build robust APIs, microservices, and tooling relied on by cro..." |
| threat modeling | 6 | 6 | "...orm infrastructure. - Conduct threat modeling, architecture reviews, and te..." |
| data warehouse | 10 | 5 | "...atastores (relational, NoSQL, data warehouse solutions) - Strong product s..." |
| prompt engineering | 6 | 5 | "...Haves - Experience with LLMs, prompt engineering, or building tools on top of..." |
| deep learning | 4 | 4 | "...- Strong expertise in applied deep learning and mainstream RecSys model a..." |
| serverless | 4 | 4 | "...as of application deployment, serverless computing, or container orche..." |
| agile | 3 | 3 | "...expect - To work in a small, agile, data-driven, customer-obsess..." |

### Words taken as skills from "N years of ..."

Skills the parser reports that are not in the dictionary. The word after "N years of" only counts when the dictionary knows it, so anything listed here is a parser bug.

None.

## Dictionary skills nobody asks for

193 of the 417 distinct dictionary skills (after alias normalization) appear in none of the 194 posts, and 60 in only one. Not a reason to delete them on its own (the corpus is software roles at product companies), but they cost nothing to find and a profile can still list them. Review the ones that look dated when the dictionary is next tuned.

**In no post:** .net, .net maui, aci, activemq, actix, ai, algolia, alpine.js, amplify, android studio, ant design, apache, api gateway, arangodb, asp.net core, assembly, astro, axum, azure app service, azure container, azure devops, azure event hub, azure functions, azure kubernetes, azure logic apps, azure service bus, azure sql, backbone, bdd, bicep, bitbucket, bootstrap, bullmq, bulma, celery, chakra ui, chef, circleci, clojure, cloud functions, cloudwatch, cobol, cockroachdb, codeigniter, confluence, containerd, cosmos db, couchbase, couchdb, cqrs, dapper, dart, domain-driven design, drizzle, dynatrace, echo, elastic beanstalk, elixir, ember, emotion, entity framework, enzyme, erlang, esbuild, etcd, event sourcing, expo, express, f#, fargate, fastify, fiber, firestore, flask, flutter, flyway, fortran, foundation, gatsby, gin, groovy, hapi, haskell, hazelcast, heroku, hexagonal architecture, hibernate, htmx, https, iis, insomnia, integration testing, ionic, jaeger, jasmine, jest, jquery, junit, kanban, keras, knex, koa, ktor, laravel, layered architecture, lerna, less, liquibase, lit, logstash, lua, mariadb, material ui, matlab, meilisearch, memcached, micronaut, mocha, mongoose, monorepo, mstest, mvc, mvp, nats, neo4j, nestjs, netlify, nginx, nunit, nuxt, nx, openapi, openshift, pair programming, perl, phoenix, php, pl/sql, podman, powershell, preact, pull request, pulsar, puppet, puppeteer, quarkus, qwik, radix ui, railway, rancher, rds, redshift, remix, rethinkdb, rollup, sagemaker, scrum, sequelize, shadcn, sidekiq, sns, soa, solidjs, solr, sqlite, ssl, stencil, step functions, styled-components, sveltekit, svn, swagger, symfony, t-sql, tdd, teamcity, testing library, testng, tidb, travis ci, trunk-based development, turborepo, typeorm, typesense, unit testing, vb.net, visual basic, vite, vitest, xamarin, xunit, zipkin, zsh

**In one post:** abac, aks, angular, aurora, babel, bamboo, c#, cassandra, clean architecture, cloud run, cloud storage, cloudformation, cloudfront, computer vision, cypress, django, e2e testing, ecs, encryption, eslint, firebase, flux, gitlab ci, hadoop, hive, jupyter, jwt, kibana, mvvm, nomad, numpy, objective-c, oracle, playwright, postman, power bi, prettier, prisma, pubsub, pytest, route53, scikit-learn, scss, selenium, shell, sinatra, soap, spanner, spinnaker, splunk, spring boot, sql server, sqs, storybook, swiftui, tailwind, tls, unix, vpc, webpack

## Seniority wording

### Words in titles

| Title word | Posts | Parser keyword |
|---|---|---|
| intern | 12 | intern |
| internship | 2 | internship |
| new grad | 5 | new grad |
| graduate | 1 | graduate |
| early career | 1 | early career |
| associate | 2 | associate |
| ii | 2 | ii |
| senior | 45 | senior |
| sr | 2 | sr |
| staff | 36 | staff |
| lead | 3 | lead |
| principal | 9 | principal |
| architect | 1 | **none** |

### Title level against the level the parser reads from the post

Rows: level from the title (the collector's classifier). Columns: `JobDescriptionParser` seniority on the full post (the title, then the body). The parser reads the level from the title when the title names one, so disagreement comes from titles without a level word, where it falls back to the body and the years asked for.

| Title \ Parser | intern | junior | mid | senior | staff | lead | principal |
|---|---|---|---|---|---|---|---|
| intern | 13 | 0 | 0 | 0 | 0 | 0 | 0 |
| junior | 0 | 10 | 0 | 0 | 0 | 0 | 0 |
| mid | 0 | 8 | 63 | 9 | 1 | 0 | 0 |
| senior | 0 | 0 | 0 | 42 | 0 | 0 | 0 |
| staff | 0 | 0 | 0 | 0 | 36 | 0 | 0 |
| lead | 0 | 0 | 0 | 0 | 0 | 3 | 0 |
| principal | 0 | 0 | 0 | 0 | 0 | 0 | 9 |

Agreement: 176 of 194 (91%).

### Years of experience asked for

First "N years" or "N+ years" in each post.

| Years | Posts |
|---|---|
| not stated | 91 |
| 0 to 1 | 3 |
| 2 to 3 | 20 |
| 4 to 5 | 34 |
| 6 to 7 | 11 |
| 8 or more | 35 |

