# Job-post corpus: relevance report

Generated 2026-10-20 from 194 posts published between 2026-07-22 and 2026-10-20 (freshness window 90 days). Skills are what `JobDescriptionParser` extracts, so this report measures the parser as much as the market.

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
| next.js | 77 (40%) | 33% | 56% | 46% | 34% |
| go | 69 (36%) | 17% | 50% | 30% | 37% |
| machine learning | 54 (28%) | 33% | 25% | 27% | 28% |
| end-to-end | 49 (25%) | 0% | 44% | 32% | 21% |
| typescript | 49 (25%) | 33% | 25% | 29% | 23% |
| aws | 45 (23%) | 17% | 31% | 20% | 24% |
| gcp | 36 (19%) | 17% | 13% | 14% | 22% |
| javascript | 35 (18%) | 0% | 25% | 23% | 16% |
| java | 34 (18%) | 0% | 25% | 14% | 19% |
| kubernetes | 34 (18%) | 0% | 19% | 18% | 18% |
| foundation | 31 (16%) | 0% | 6% | 18% | 17% |
| react | 31 (16%) | 17% | 13% | 27% | 11% |
| solid | 31 (16%) | 17% | 19% | 14% | 16% |
| sql | 31 (16%) | 17% | 31% | 14% | 15% |
| llm | 29 (15%) | 33% | 6% | 16% | 15% |
| ci/cd | 28 (14%) | 33% | 19% | 14% | 13% |
| azure | 26 (13%) | 0% | 6% | 11% | 16% |
| spark | 21 (11%) | 17% | 6% | 4% | 15% |
| less | 20 (10%) | 0% | 13% | 14% | 9% |
| github | 19 (10%) | 0% | 13% | 13% | 9% |
| terraform | 19 (10%) | 0% | 19% | 5% | 11% |
| openai | 18 (9%) | 0% | 6% | 18% | 6% |
| rust | 18 (9%) | 17% | 13% | 13% | 7% |
| c++ | 17 (9%) | 17% | 0% | 11% | 9% |
| code review | 17 (9%) | 17% | 13% | 9% | 8% |
| postgresql | 15 (8%) | 17% | 6% | 9% | 7% |
| professional | 15 (8%) | 33% | 6% | 5% | 8% |
| node.js | 14 (7%) | 0% | 19% | 2% | 9% |
| c | 13 (7%) | 17% | 13% | 7% | 5% |

## Skills the parser misses

Terms that recent posts use but the skill dictionary does not have, so the parser never extracts them, match scores ignore them and the gaps tab cannot mention them. Candidates for the MATCH-07 dictionary; some are concepts rather than skills and a person should decide.

### Known technologies and practices not in the dictionary

From a reviewer watch list (`RelevanceReport.WatchList`), in at least 2 posts.

| Term | Posts | Share | Families |
|---|---|---|---|
| agents | 73 | 38% | ml 20, software 16, security 9 |
| observability | 68 | 35% | devops 14, ml 11, software 9 |
| distributed systems | 49 | 25% | software 16, devops 13, backend 7 |
| agentic | 37 | 19% | ml 10, security 6, software 6 |
| scalability | 30 | 15% | devops 8, software 6, data 5 |
| networking | 29 | 15% | devops 12, security 5, software 5 |
| on-call | 29 | 15% | devops 13, security 4, software 4 |
| experimentation | 27 | 14% | ml 7, data 5, fullstack 4 |
| incident response | 27 | 14% | devops 10, security 9, data 5 |
| system design | 20 | 10% | backend 5, ml 4, devops 2 |
| accessibility | 19 | 10% | frontend 6, software 3, devops 2 |
| llms | 18 | 9% | ml 10, software 3, data 1 |
| data modeling | 17 | 9% | data 9, software 4, backend 3 |
| MCP | 16 | 8% | ml 9, backend 2, devops 2 |
| algorithms | 16 | 8% | software 7, ml 4, security 3 |
| Claude | 13 | 7% | software 4, ml 3, devops 2 |
| data structures | 10 | 5% | software 5, data 2, security 2 |
| lakehouse | 10 | 5% | data 3, software 2, backend 1 |
| Salesforce | 9 | 5% | fullstack 2, software 2, backend 1 |
| data warehouse | 9 | 5% | data 3, devops 2, backend 1 |
| etl | 9 | 5% | data 9 |
| generative ai | 9 | 5% | frontend 2, ml 2, backend 1 |
| Anthropic | 8 | 4% | fullstack 2, software 2, backend 1 |
| evals | 8 | 4% | ml 4, data 1, devops 1 |
| Iceberg | 7 | 4% | software 3, data 2, backend 1 |
| concurrency | 7 | 4% | software 4, backend 1, ml 1 |
| a/b testing | 6 | 3% | frontend 2, data 1, devops 1 |
| blockchain | 6 | 3% | backend 1, data 1, frontend 1 |
| design systems | 6 | 3% | frontend 3, fullstack 2, mobile 1 |
| dns | 6 | 3% | software 2, backend 1, devops 1 |
| prompt engineering | 6 | 3% | ml 4, data 2 |
| slos | 6 | 3% | devops 5, ml 1 |
| threat modeling | 6 | 3% | security 6 |
| Argo | 5 | 3% | devops 3, data 1, fullstack 1 |
| Copilot | 5 | 3% | software 3, devops 1, mobile 1 |
| api design | 5 | 3% | backend 2, software 2, devops 1 |
| langgraph | 5 | 3% | ml 4, frontend 1 |
| web3 | 5 | 3% | backend 1, frontend 1, fullstack 1 |
| large language models | 4 | 2% | ml 3, software 1 |
| nosql | 4 | 2% | software 2, devops 1, fullstack 1 |
| parquet | 4 | 2% | software 3, backend 1 |
| siem | 4 | 2% | security 4 |
| Okta | 3 | 2% | ml 2, devops 1 |
| RAG | 3 | 2% | ml 2, frontend 1 |
| buildkite | 3 | 2% | qa 2, security 1 |
| clickhouse | 3 | 2% | data 2, devops 1 |
| genai | 3 | 2% | frontend 1, fullstack 1, ml 1 |
| gradle | 3 | 2% | software 2, mobile 1 |
| jetpack compose | 3 | 2% | mobile 3 |
| performance tuning | 3 | 2% | backend 2, fullstack 1 |
| slo | 3 | 2% | devops 2, backend 1 |
| transformers | 3 | 2% | ml 3 |
| CUDA | 2 | 1% | devops 1, ml 1 |
| Combine | 2 | 1% | fullstack 1, mobile 1 |
| Envoy | 2 | 1% | devops 1, security 1 |
| Gemini | 2 | 1% | ml 2 |
| JAX | 2 | 1% | ml 2 |
| Ray | 2 | 1% | ml 2 |
| cdn | 2 | 1% | devops 1, software 1 |
| elt | 2 | 1% | data 2 |
| embeddings | 2 | 1% | data 1, software 1 |
| fine-tuning | 2 | 1% | ml 2 |
| gdpr | 2 | 1% | security 2 |
| iso 27001 | 2 | 1% | security 2 |
| istio | 2 | 1% | devops 1, security 1 |
| jvm | 2 | 1% | backend 1, security 1 |
| load balancing | 2 | 1% | devops 2 |
| mlflow | 2 | 1% | ml 2 |
| multithreading | 2 | 1% | fullstack 1, software 1 |
| pci | 2 | 1% | security 2 |
| penetration testing | 2 | 1% | security 2 |
| soc 2 | 2 | 1% | security 2 |
| tcp/ip | 2 | 1% | devops 1, security 1 |

### Other product-looking terms (review by hand)

CamelCase or `*.js` words in at least 3 posts that are not in the dictionary, the watch list or a company name in the corpus. Expect some noise (other companies' products, brand names).

SaaS (21), CapitalG (6), DatadogLife (6), ElevenAPI (6), ElevenAgents (6), ElevenCreative (6), PinFlex (6), SpaceX (6), SpaceXSI (6), WeWork (6), ChatGPT (5), GitOps (5), IaC (4), SpaceXAI (4), DataFusion (3), LiveKit (3), NetSuite (3), RemoteUS (3), WebRTC (3)

## Skills the parser gets wrong

### Dictionary words that match ordinary English

Skills found in at least 3 posts where, in most of those posts, every match is written in lowercase (products are usually capitalized: "Next.js", "Go"). Lowercase is a hint, not proof: concepts such as microservices are normally lowercase too, so read the example. The real false positives inflate match scores and highlight the wrong words.

| Skill | Posts | Only lowercase prose | Example |
|---|---|---|---|
| next.js | 77 | 74 | "...ge AI systems that enable the next generation of intelligent app..." |
| end-to-end | 49 | 46 | "...nsure alignment. You will own end-to-end feature delivery, author tech..." |
| foundation | 31 | 29 | "...trusted account and commerce foundation that lets anyone, anywhere, b..." |
| less | 20 | 20 | "...work is faster, clearer, and less fragmented. Millions of indiv..." |
| code review | 17 | 15 | "..., testing, documentation, and code review Qualifications Twilio value..." |
| dbt | 10 | 10 | "...rks, including expertise with dbt tests, CI/CD, and data observ..." |
| event-driven | 10 | 10 | "...rs use to build long-running, event-driven, and scheduled applications...." |
| https | 7 | 7 | ".... To learn more, please visit https://www.redditinc.com/careers/..." |
| microservices | 6 | 6 | "...specs, and build robust APIs, microservices, and tooling relied on by cro..." |
| deep learning | 4 | 4 | "...- Strong expertise in applied deep learning and mainstream RecSys model a..." |
| serverless | 4 | 4 | "...as of application deployment, serverless computing, or container orche..." |
| agile | 3 | 3 | "...expect - To work in a small, agile, data-driven, customer-obsess..." |
| rails | 5 | 3 | "...ns with international banking rails - giving every other team a r..." |

### Words taken as skills from "N years of ..."

Not in the dictionary; the parser keeps the word that follows "N years of". Most are not skills.

professional (15), building (12), industry (6), hands-on (4), working (4), designing (3), applied (2), thereafter (2), across (1), cloud (1), contributing (1), delivering (1), demonstrated (1), deploying (1), full-time (1), fully (1), leading (1), progressive (1), qa (1), securing (1), security (1), shipping (1), total (1), why (1), writing (1)

## Dictionary skills nobody asks for

205 of the 397 distinct dictionary skills (after alias normalization) appear in none of the 194 posts, and 66 in only one. Not a reason to delete them on its own (the corpus is software roles at product companies), but they cost nothing to find and the ones that look dated are candidates to drop or merge in MATCH-07.

**In no post:** .net, .net maui, aad, aci, activemq, actix, algolia, alpine.js, amazon sqs, amplify, android studio, ant design, antd, api gateway, arangodb, asp.net core, astro, axum, azure active directory, azure app service, azure container, azure devops, azure event hub, azure functions, azure kubernetes, azure logic apps, azure service bus, azure sql, backbone, bdd, bicep, bitbucket, bull, bullmq, bulma, celery, chakra ui, chef, circleci, clojure, cloud functions, cloudwatch, cobol, cockroachdb, codeigniter, confluence, containerd, cosmos db, cosmosdb, couchbase, couchdb, cqrs, css3, dapper, dart, ddd, domain-driven design, drizzle, dynatrace, e2e testing, elastic beanstalk, elixir, ember, ember.js, emotion, entity framework, enzyme, erlang, esbuild, etcd, event sourcing, expo, express, express.js, f#, fargate, fastify, faunadb, fiber, firestore, flask, flutter, flyway, fortran, gatsby, gin, gin-gonic, groovy, hapi, hashicorp vault, haskell, hazelcast, heroku, hexagonal architecture, hibernate, htmx, iis, insomnia, integration testing, ionic, jaeger, jasmine, jest, jquery, junit, kanban, keras, knex, koa, ktor, laravel, layered architecture, lerna, liquibase, lit, logstash, lua, mariadb, material ui, matlab, maui, meilisearch, memcached, micronaut, mocha, mongoose, monorepo, mstest, mui, mvc, nats, neo4j, nestjs, netlify, newrelic, nginx, nunit, nuxt, nx, openapi, openshift, pair programming, perl, phoenix, php, pl/sql, plsql, podman, power bi, powershell, preact, pubsub, pull request, pulsar, puppet, puppeteer, quarkus, qwik, radix ui, railway, rancher, rds, redshift, rethinkdb, rollup, sagemaker, scrum, sequelize, shadcn, sidekiq, sns, soa, solidjs, solr, sqlite, ssl, stencil, step functions, styled-components, subversion, sveltekit, svn, swagger, symfony, t-sql, tdd, teamcity, testing library, testng, tidb, travis ci, trunk-based development, turborepo, typeorm, typesense, unit testing, vb.net, visual basic, vite, vitest, xamarin, xunit, xunit.net, zipkin, zsh

**In one post:** abac, aks, angular, apache flink, assembly, aurora, babel, bamboo, c#, cassandra, clean architecture, cloud run, cloud storage, cloudformation, cloudfront, computer vision, cpp, cypress, django, echo, ecs, encryption, eslint, firebase, flux, gitlab ci, hadoop, hive, jupyter, jwt, kibana, mvp, mvvm, nomad, numpy, oauth2, objective-c, openid connect, oracle, playwright, postman, prettier, prisma, pytest, remix, route53, ruby on rails, scikit-learn, scss, selenium, shell, sinatra, soap, spanner, spinnaker, splunk, spring boot, sql server, sqs, storybook, tailwind, tls, unix, vpc, webpack, websocket

## Seniority wording

### Words in titles

| Title word | Posts | Parser keyword |
|---|---|---|
| intern | 12 | intern |
| internship | 2 | **none** |
| new grad | 5 | new grad |
| graduate | 1 | graduate |
| early career | 1 | **none** |
| associate | 2 | associate |
| ii | 2 | **none** |
| senior | 45 | senior |
| sr | 2 | sr. |
| staff | 36 | only inside "staff engineer" |
| lead | 3 | lead |
| principal | 9 | principal |
| architect | 1 | architect |

### Title level against the level the parser reads from the post

Rows: level from the title (the collector's classifier). Columns: `JobDescriptionParser` seniority on the full post. The parser takes the first seniority word it meets, which in long posts is often a team mention rather than the role.

| Title \ Parser | intern | junior | mid | senior | staff | lead | principal |
|---|---|---|---|---|---|---|---|
| intern | 8 | 0 | 1 | 2 | 0 | 0 | 2 |
| junior | 0 | 6 | 1 | 2 | 0 | 0 | 1 |
| mid | 0 | 11 | 35 | 10 | 0 | 16 | 9 |
| senior | 0 | 0 | 0 | 24 | 0 | 10 | 8 |
| staff | 0 | 0 | 4 | 3 | 5 | 14 | 10 |
| lead | 0 | 0 | 0 | 0 | 0 | 2 | 1 |
| principal | 0 | 0 | 0 | 0 | 0 | 0 | 9 |

Agreement: 89 of 194 (46%).

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

