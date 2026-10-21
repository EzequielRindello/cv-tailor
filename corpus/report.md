# Job-post corpus: relevance report

Generated 2026-10-21 from 194 posts published between 2026-07-24 and 2026-10-21 (freshness window 90 days). Skills are what `JobDescriptionParser` extracts, so this report measures the parser as much as the market.

Regenerate: `dotnet run --project ErCv.Collector -- report`. Source posts: `corpus/postings/`.

## Corpus

| Source | Posts |
|---|---|
| Greenhouse | 114 |
| Ashby | 47 |
| Lever | 28 |
| Workable | 5 |

| Role family | Posts |
|---|---|
| software | 41 |
| ml | 34 |
| data | 28 |
| devops | 26 |
| security | 20 |
| backend | 14 |
| mobile | 10 |
| fullstack | 9 |
| frontend | 8 |
| qa | 4 |

| Seniority (from the title) | Posts |
|---|---|
| intern | 12 |
| junior | 11 |
| mid | 78 |
| senior | 43 |
| staff | 36 |
| lead | 3 |
| principal | 11 |

| Month posted | Posts |
|---|---|
| 2026-07 | 5 |
| 2026-08 | 18 |
| 2026-09 | 52 |
| 2026-10 | 119 |

Companies: 36.

## Skill frequency over time

Share of the posts published each month that the parser finds the skill in (top 30 overall). Months with few posts swing a lot; read the column counts first.

| Skill | All posts | 2026-07 (n=5) | 2026-08 (n=18) | 2026-09 (n=52) | 2026-10 (n=119) |
|---|---|---|---|---|---|
| python | 98 (51%) | 80% | 39% | 44% | 54% |
| observability | 72 (37%) | 40% | 33% | 31% | 40% |
| ai agents | 61 (31%) | 40% | 39% | 37% | 28% |
| machine learning | 54 (28%) | 40% | 17% | 33% | 27% |
| distributed systems | 51 (26%) | 0% | 28% | 29% | 26% |
| go | 50 (26%) | 20% | 22% | 10% | 34% |
| aws | 46 (24%) | 20% | 33% | 19% | 24% |
| typescript | 44 (23%) | 40% | 22% | 25% | 21% |
| llm | 40 (21%) | 40% | 33% | 19% | 18% |
| sql | 39 (20%) | 20% | 28% | 17% | 20% |
| kubernetes | 38 (20%) | 0% | 22% | 15% | 22% |
| gcp | 36 (19%) | 20% | 17% | 12% | 22% |
| java | 35 (18%) | 0% | 33% | 15% | 18% |
| on-call | 33 (17%) | 0% | 33% | 10% | 18% |
| javascript | 28 (14%) | 0% | 22% | 19% | 12% |
| react | 28 (14%) | 20% | 6% | 25% | 11% |
| incident response | 27 (14%) | 0% | 11% | 15% | 14% |
| azure | 26 (13%) | 0% | 11% | 8% | 17% |
| ci/cd | 25 (13%) | 40% | 17% | 12% | 12% |
| system design | 24 (12%) | 40% | 11% | 12% | 12% |
| algorithms | 21 (11%) | 20% | 6% | 8% | 13% |
| networking | 20 (10%) | 0% | 11% | 15% | 8% |
| code review | 19 (10%) | 20% | 11% | 6% | 11% |
| github | 19 (10%) | 0% | 11% | 13% | 8% |
| rust | 19 (10%) | 0% | 11% | 8% | 11% |
| terraform | 19 (10%) | 0% | 17% | 6% | 11% |
| c++ | 18 (9%) | 0% | 6% | 8% | 11% |
| infrastructure as code | 18 (9%) | 20% | 11% | 4% | 11% |
| openai | 18 (9%) | 0% | 6% | 15% | 8% |
| data modeling | 16 (8%) | 20% | 6% | 10% | 8% |

## Skills the parser misses

Terms that recent posts use but the skill dictionary does not have, so the parser never extracts them, match scores ignore them and the gaps tab cannot mention them. Candidates for `Data/skills.json`; some are concepts rather than skills and a person should decide.

### Known technologies and practices not in the dictionary

From a reviewer watch list (`RelevanceReport.WatchList`), in at least 2 posts.

| Term | Posts | Share | Families |
|---|---|---|---|
| agents | 73 | 38% | ml 20, software 16, security 9 |
| experimentation | 31 | 16% | ml 7, data 6, software 5 |
| scalability | 31 | 16% | devops 9, software 6, data 5 |
| Claude | 13 | 7% | software 4, ml 3, devops 2 |
| Salesforce | 9 | 5% | software 3, fullstack 2, backend 1 |
| Anthropic | 8 | 4% | fullstack 2, software 2, backend 1 |
| blockchain | 7 | 4% | ml 2, backend 1, data 1 |
| Copilot | 5 | 3% | software 3, devops 1, mobile 1 |
| web3 | 5 | 3% | backend 1, frontend 1, fullstack 1 |
| Argo | 4 | 2% | devops 2, data 1, fullstack 1 |
| Combine | 4 | 2% | data 1, fullstack 1, mobile 1 |
| load balancing | 4 | 2% | devops 4 |
| Okta | 3 | 2% | ml 2, devops 1 |
| cilium | 3 | 2% | devops 3 |
| gdpr | 3 | 2% | security 2, software 1 |
| performance tuning | 3 | 2% | backend 1, fullstack 1, ml 1 |
| Ray | 2 | 1% | ml 2 |
| bazel | 2 | 1% | mobile 1, security 1 |
| buildkite | 2 | 1% | qa 1, security 1 |
| crossplane | 2 | 1% | devops 2 |
| iso 27001 | 2 | 1% | security 2 |
| multithreading | 2 | 1% | fullstack 1, software 1 |
| pci | 2 | 1% | security 2 |
| soc 2 | 2 | 1% | security 2 |
| tcp/ip | 2 | 1% | devops 1, security 1 |
| transformers | 2 | 1% | ml 2 |

### Other product-looking terms (review by hand)

CamelCase or `*.js` words in at least 3 posts that are not in the dictionary, the watch list or a company name in the corpus. Expect some noise (other companies' products, brand names).

SaaS (20), CapitalG (6), DatadogLife (6), ElevenAPI (6), ElevenAgents (6), ElevenCreative (6), PinFlex (6), SpaceX (6), SpaceXSI (6), WeWork (6), ChatGPT (5), SpaceXAI (4), DataFusion (3), LiveKit (3), NetSuite (3), RemoteUS (3)

## Skills the parser gets wrong

### Dictionary words that match ordinary English

Skills found in at least 3 posts where, in most of those posts, every match is written in lowercase (products are usually capitalized: "Next.js", "Go"). Lowercase is a hint, not proof: concepts such as microservices are normally lowercase too, so read the example. The real false positives inflate match scores and highlight the wrong words.

| Skill | Posts | Only lowercase prose | Example |
|---|---|---|---|
| observability | 72 | 64 | "...th dbt tests, CI/CD, and data observability - Experience evangelizing and..." |
| distributed systems | 51 | 49 | "...tical cloud-native enterprise distributed systems. - Strong Proficiency in Java..." |
| on-call | 33 | 31 | "...thin team services. Fully own on-call responsibilities, driving inc..." |
| incident response | 27 | 23 | "...anges, backups, restores, and incident response - Reducing operational load b..." |
| system design | 24 | 21 | "...sely with senior engineers on system design, multi-agent orchestration, a..." |
| algorithms | 21 | 20 | "...principles (data structures, algorithms, version control) - Familiari..." |
| networking | 20 | 18 | "...Karpenter autoscaling, Cilium networking, and ArgoCD-driven GitOps dep..." |
| code review | 19 | 17 | "..., testing, documentation, and code review Qualifications Twilio value..." |
| data modeling | 16 | 16 | "...forcing best practices around data modeling, testing, documentation, and..." |
| data structures | 12 | 12 | "...tware engineering principles (data structures, algorithms, version control)..." |
| infrastructure as code | 18 | 12 | "...P, with a cloud warehouse and infrastructure as code. You provision, deploy, monit..." |
| dbt | 11 | 11 | "...rks, including expertise with dbt tests, CI/CD, and data observ..." |
| concurrency | 9 | 9 | "...e: realtime inference on GPU, concurrency, autoscaling, observability,..." |
| evals | 9 | 9 | "...lly moves loss and downstream evals. - Treat data quality as a sy..." |
| event-driven | 8 | 8 | "...rs use to build long-running, event-driven, and scheduled applications...." |
| data warehouse | 11 | 6 | "...atastores (relational, NoSQL, data warehouse solutions) - Strong product s..." |
| design systems | 7 | 6 | "...tain and enhance Ramp's brand design system and component library, ensuri..." |
| microservices | 6 | 6 | "...specs, and build robust APIs, microservices, and tooling relied on by cro..." |
| threat modeling | 6 | 6 | "...orm infrastructure. - Conduct threat modeling, architecture reviews, and te..." |
| deep learning | 5 | 5 | "...research in machine learning, deep learning, or a closely related field -..." |
| prompt engineering | 6 | 5 | "...Haves - Experience with LLMs, prompt engineering, or building tools on top of..." |
| accessibility | 5 | 4 | "...visual and interaction craft, accessibility, performance, resilience, ins..." |
| agile | 4 | 4 | "...expect - To work in a small, agile, data-driven, customer-obsess..." |
| serverless | 4 | 4 | "...e Developer Platform or other serverless edge-computing platforms. - E..." |

### Words taken as skills from "N years of ..."

Skills the parser reports that are not in the dictionary. The word after "N years of" only counts when the dictionary knows it, so anything listed here is a parser bug.

None.

## Dictionary skills nobody asks for

202 of the 417 distinct dictionary skills (after alias normalization) appear in none of the 194 posts, and 57 in only one. Not a reason to delete them on its own (the corpus is software roles at product companies), but they cost nothing to find and a profile can still list them. Review the ones that look dated when the dictionary is next tuned.

**In no post:** .net, .net maui, aci, activemq, actix, ai, algolia, alpine.js, amplify, android studio, ant design, apache, api gateway, arangodb, asp.net core, assembly, astro, aurora, axum, azure app service, azure container, azure devops, azure event hub, azure functions, azure kubernetes, azure logic apps, azure service bus, azure sql, backbone, bamboo, bdd, bicep, bitbucket, bootstrap, bullmq, bulma, c#, celery, chakra ui, chef, circleci, clojure, cloud functions, cloud run, cloud storage, cloudwatch, cobol, cockroachdb, codeigniter, confluence, containerd, cosmos db, couchbase, couchdb, cqrs, dapper, dart, domain-driven design, drizzle, dynatrace, echo, ecs, elastic beanstalk, elixir, ember, emotion, entity framework, enzyme, erlang, esbuild, etcd, event sourcing, expo, express, f#, fargate, fastify, fiber, firestore, flask, flutter, flyway, fortran, foundation, gatsby, gin, groovy, hapi, haskell, hazelcast, heroku, hexagonal architecture, hibernate, htmx, https, iis, insomnia, integration testing, ionic, jaeger, jasmine, jest, jquery, junit, kanban, keras, knex, koa, ktor, laravel, layered architecture, lerna, less, liquibase, lit, logstash, lua, mariadb, material ui, matlab, meilisearch, memcached, micronaut, mocha, mongoose, monorepo, mstest, mvc, mvp, nats, neo4j, nestjs, netlify, nginx, nunit, nuxt, nx, openapi, openshift, pair programming, perl, phoenix, php, pl/sql, podman, powershell, preact, pubsub, pull request, pulsar, puppet, puppeteer, pytest, quarkus, qwik, radix ui, railway, rancher, redshift, remix, rethinkdb, rollup, sagemaker, scrum, scss, sequelize, shadcn, sidekiq, sns, soa, solidjs, solr, spanner, sqlite, ssl, stencil, step functions, storybook, styled-components, sveltekit, svn, swagger, symfony, t-sql, teamcity, testing library, testng, tidb, travis ci, trunk-based development, turborepo, typeorm, typesense, unit testing, vb.net, visual basic, vite, vitest, xamarin, xunit, zipkin, zsh

**In one post:** angular, ansible, azure blob, babel, bedrock, cassandra, cdn, clean architecture, cloudformation, cloudfront, computer vision, cypress, django, e2e testing, ec2, elt, embeddings, encryption, eslint, fine-tuning, firebase, hadoop, hive, jwt, mvvm, new relic, nlp, nomad, numpy, objective-c, playwright, postman, power bi, prettier, prisma, rds, route53, scikit-learn, selenium, sentry, shell, sinatra, soap, spinnaker, splunk, spring boot, sql server, sqs, tailwind, tdd, tls, unix, vertex ai, vpc, webhooks, webpack, xcode

## Seniority wording

### Words in titles

| Title word | Posts | Parser keyword |
|---|---|---|
| intern | 11 | intern |
| internship | 2 | internship |
| new grad | 6 | new grad |
| graduate | 1 | graduate |
| early career | 1 | early career |
| associate | 2 | associate |
| ii | 3 | ii |
| senior | 48 | senior |
| sr | 2 | sr |
| staff | 37 | staff |
| lead | 3 | lead |
| principal | 11 | principal |
| architect | 1 | **none** |

### Title level against the level the parser reads from the post

Rows: level from the title (the collector's classifier). Columns: `JobDescriptionParser` seniority on the full post (the title, then the body). The parser reads the level from the title when the title names one, so disagreement comes from titles without a level word, where it falls back to the body and the years asked for.

| Title \ Parser | intern | junior | mid | senior | staff | lead | principal |
|---|---|---|---|---|---|---|---|
| intern | 12 | 0 | 0 | 0 | 0 | 0 | 0 |
| junior | 0 | 11 | 0 | 0 | 0 | 0 | 0 |
| mid | 0 | 9 | 61 | 6 | 2 | 0 | 0 |
| senior | 0 | 0 | 0 | 43 | 0 | 0 | 0 |
| staff | 0 | 0 | 0 | 0 | 36 | 0 | 0 |
| lead | 0 | 0 | 0 | 0 | 0 | 3 | 0 |
| principal | 0 | 0 | 0 | 0 | 0 | 0 | 11 |

Agreement: 177 of 194 (91%).

### Years of experience asked for

First "N years" or "N+ years" in each post.

| Years | Posts |
|---|---|
| not stated | 92 |
| 0 to 1 | 3 |
| 2 to 3 | 20 |
| 4 to 5 | 35 |
| 6 to 7 | 13 |
| 8 or more | 31 |

