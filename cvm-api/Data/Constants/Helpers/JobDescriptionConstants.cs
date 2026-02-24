namespace ErCvApi.Constants
{
    public static class JobDescriptionConstants
    {
        // Default Values
        public const string DefaultSeniority = "mid-level";

        // Regex Patterns
        public const string YearsOfExperiencePattern = @"(\d+)\+?\s*(?:years?|yrs?)";
        public const string YearsWithTechPattern = @"(\d+)\+?\s*(?:years?|yrs?)\s*(?:of\s*)?(?:experience\s*)?(?:with\s*)?([a-z0-9\.\#\+\-\/]+)";
        public const string WordSplitPattern = @"\W+";
        public const string BulletLinePattern = @"^[\-\*\•\d\.]+\s*";

        // Requirement Keywords
        public const string ExperienceKeyword = "experience";
        public const string RequiredKeyword = "required";
        public const string MustHaveKeyword = "must have";

        // Seniority Keywords — order matters: more specific first
        public static readonly string[] SeniorityKeywords =
        {
            "principal", "staff engineer", "distinguished", "fellow",
            "architect", "lead", "tech lead", "team lead",
            "senior", "sr.",
            "mid-level", "mid level", "intermediate",
            "junior", "jr.", "entry-level", "entry level",
            "associate", "intern", "graduate", "new grad"
        };

        // Negation words — if these precede a seniority keyword, ignore it
        public static readonly string[] NegationWords =
        {
            "no", "not", "without", "non", "don't", "doesn't", "require",
            "required", "necessary", "needed", "prefer", "need"
        };

        // ─────────────────────────────────────────────────────────────────────
        // TECH KEYWORDS — comprehensive list for skill extraction
        // Grouped by category for maintainability
        // ─────────────────────────────────────────────────────────────────────

        public static readonly string[] CommonTechKeywords =
        {
            // ── Languages ───────────────────────────────────────────────────
            "c#", "csharp", "c sharp",
            ".net", "dotnet", "dot net",
            "java",
            "javascript", "js",
            "typescript", "ts",
            "python",
            "ruby",
            "go", "golang",
            "rust",
            "c++", "cpp",
            "c",
            "php",
            "scala",
            "kotlin",
            "swift",
            "objective-c",
            "r",
            "matlab",
            "perl",
            "lua",
            "dart",
            "haskell",
            "elixir",
            "erlang",
            "clojure",
            "f#",
            "vb.net", "visual basic",
            "cobol",
            "fortran",
            "assembly",
            "groovy",
            "shell", "bash", "zsh", "powershell",
            "sql",
            "plsql", "pl/sql",
            "t-sql",

            // ── Frontend Frameworks & Libraries ─────────────────────────────
            "react", "react.js", "reactjs",
            "angular", "angularjs",
            "vue", "vue.js", "vuejs",
            "svelte", "sveltekit",
            "next.js", "nextjs", "next",
            "nuxt", "nuxt.js",
            "gatsby",
            "remix",
            "astro",
            "solid", "solidjs",
            "ember", "ember.js",
            "backbone",
            "jquery",
            "alpine.js",
            "htmx",
            "lit",
            "preact",
            "stencil",
            "qwik",

            // ── CSS / Styling ────────────────────────────────────────────────
            "css", "css3",
            "scss", "sass",
            "less",
            "tailwind", "tailwindcss",
            "bootstrap",
            "material ui", "mui",
            "chakra ui",
            "ant design", "antd",
            "styled-components",
            "emotion",
            "radix ui",
            "shadcn",
            "bulma",
            "foundation",
            "storybook",

            // ── Backend Frameworks ───────────────────────────────────────────
            "asp.net", "asp.net core",
            "node.js", "nodejs", "node",
            "express", "express.js",
            "nest.js", "nestjs",
            "fastify",
            "hapi",
            "koa",
            "django",
            "flask",
            "fastapi",
            "spring", "spring boot",
            "quarkus",
            "micronaut",
            "rails", "ruby on rails",
            "sinatra",
            "laravel",
            "symfony",
            "codeigniter",
            "gin", "gin-gonic",
            "echo",
            "fiber",
            "actix",
            "axum",
            "phoenix",
            "ktor",

            // ── Databases — SQL ──────────────────────────────────────────────
            "postgresql", "postgres",
            "mysql",
            "sql server", "mssql",
            "sqlite",
            "oracle",
            "mariadb",
            "cockroachdb",
            "tidb",
            "aurora",

            // ── Databases — NoSQL ────────────────────────────────────────────
            "mongodb", "mongo",
            "dynamodb",
            "cosmosdb",
            "cassandra",
            "couchdb",
            "couchbase",
            "firestore", "firebase",
            "neo4j",
            "arangodb",
            "rethinkdb",
            "faunadb",

            // ── Cache / KV ───────────────────────────────────────────────────
            "redis",
            "memcached",
            "hazelcast",
            "etcd",

            // ── Search ───────────────────────────────────────────────────────
            "elasticsearch", "elastic",
            "opensearch",
            "solr",
            "typesense",
            "algolia",
            "meilisearch",

            // ── Message Brokers / Queues ─────────────────────────────────────
            "kafka",
            "rabbitmq",
            "activemq",
            "sqs", "amazon sqs",
            "sns",
            "nats",
            "pulsar",
            "celery",
            "bull", "bullmq",
            "sidekiq",

            // ── Cloud — AWS ──────────────────────────────────────────────────
            "aws", "amazon web services",
            "ec2",
            "s3",
            "lambda",
            "rds",
            "ecs",
            "eks",
            "cloudfront",
            "cloudwatch",
            "iam",
            "vpc",
            "route53",
            "api gateway",
            "sagemaker",
            "bedrock",
            "amplify",
            "elastic beanstalk",
            "fargate",
            "step functions",

            // ── Cloud — Azure ────────────────────────────────────────────────
            "azure",
            "azure devops",
            "azure functions",
            "azure blob",
            "azure sql",
            "azure kubernetes", "aks",
            "azure container", "aci",
            "cosmos db",
            "azure active directory", "aad",
            "azure service bus",
            "azure event hub",
            "azure logic apps",
            "azure app service",

            // ── Cloud — GCP ──────────────────────────────────────────────────
            "gcp", "google cloud",
            "bigquery",
            "cloud run",
            "cloud functions",
            "gke",
            "pubsub",
            "cloud storage",
            "spanner",
            "vertex ai",

            // ── Containers & Orchestration ───────────────────────────────────
            "docker",
            "kubernetes", "k8s",
            "helm",
            "podman",
            "containerd",
            "openshift",
            "rancher",
            "nomad",

            // ── CI/CD ────────────────────────────────────────────────────────
            "ci/cd", "cicd",
            "github actions",
            "gitlab ci", "gitlab",
            "jenkins",
            "circleci",
            "travis ci",
            "teamcity",
            "bamboo",
            "argo cd", "argocd",
            "flux",
            "spinnaker",

            // ── IaC & Config ─────────────────────────────────────────────────
            "terraform",
            "ansible",
            "chef",
            "puppet",
            "pulumi",
            "cloudformation",
            "cdk",
            "bicep",

            // ── Monitoring & Observability ───────────────────────────────────
            "prometheus",
            "grafana",
            "datadog",
            "newrelic", "new relic",
            "splunk",
            "sentry",
            "jaeger",
            "zipkin",
            "opentelemetry",
            "dynatrace",
            "kibana",
            "logstash",

            // ── API & Integration ────────────────────────────────────────────
            "rest", "restful", "rest api",
            "graphql",
            "grpc",
            "soap",
            "websocket", "websockets",
            "openapi", "swagger",
            "postman",
            "insomnia",
            "oauth", "oauth2",
            "jwt",
            "saml",
            "webhooks",

            // ── Architecture Patterns ────────────────────────────────────────
            "microservices",
            "monorepo",
            "event-driven",
            "event sourcing",
            "cqrs",
            "domain-driven design", "ddd",
            "hexagonal architecture",
            "clean architecture",
            "layered architecture",
            "serverless",
            "soa",
            "mvc", "mvvm", "mvp",

            // ── Testing ──────────────────────────────────────────────────────
            "unit testing",
            "integration testing",
            "e2e testing", "end-to-end",
            "tdd", "test-driven development",
            "bdd",
            "jest",
            "vitest",
            "mocha",
            "jasmine",
            "cypress",
            "playwright",
            "selenium",
            "puppeteer",
            "xunit", "xunit.net",
            "nunit",
            "mstest",
            "pytest",
            "junit",
            "testng",
            "enzyme",
            "testing library",

            // ── Version Control ──────────────────────────────────────────────
            "git",
            "github",
            "gitlab",
            "bitbucket",
            "svn", "subversion",

            // ── AI / ML ──────────────────────────────────────────────────────
            "machine learning", "ml",
            "deep learning",
            "tensorflow",
            "pytorch",
            "scikit-learn", "sklearn",
            "keras",
            "hugging face",
            "langchain",
            "llm",
            "openai",
            "nlp",
            "computer vision",
            "pandas",
            "numpy",
            "jupyter",
            "spark", "apache spark",

            // ── Data Engineering ─────────────────────────────────────────────
            "dbt",
            "airflow", "apache airflow",
            "hadoop",
            "hive",
            "flink", "apache flink",
            "databricks",
            "snowflake",
            "redshift",
            "bigquery",
            "looker",
            "tableau",
            "power bi",

            // ── Tools & Misc ─────────────────────────────────────────────────
            "linux",
            "unix",
            "nginx",
            "apache",
            "iis",
            "vite",
            "webpack",
            "rollup",
            "esbuild",
            "babel",
            "eslint",
            "prettier",
            "nx",
            "turborepo",
            "lerna",
            "vercel",
            "netlify",
            "heroku",
            "railway",
            "supabase",
            "prisma",
            "drizzle",
            "typeorm", "type orm",
            "entity framework", "ef core",
            "dapper",
            "hibernate",
            "sequelize",
            "mongoose",
            "knex",
            "liquibase",
            "flyway",
            "figma",
            "jira",
            "confluence",
            "notion",
            "linear",

            // ── Security ────────────────────────────────────────────────────
            "oauth", "oauth2",
            "openid connect", "oidc",
            "sso",
            "rbac",
            "abac",
            "zero trust",
            "ssl", "tls",
            "https",
            "encryption",
            "vault", "hashicorp vault",
            "owasp",

            // ── Mobile ──────────────────────────────────────────────────────
            "react native",
            "flutter",
            "xamarin",
            "maui", ".net maui",
            "ionic",
            "expo",
            "android",
            "ios",
            "xcode",
            "android studio",

            // ── Practices (detectable from JD text) ─────────────────────────
            "agile",
            "scrum",
            "kanban",
            "devops",
            "devsecops",
            "sre", "site reliability",
            "pair programming",
            "code review",
            "pull request",
            "trunk-based development",
            "continuous integration",
            "continuous deployment",
            "continuous delivery",
        };

        // Common Words (Stopwords) — expanded
        public static readonly string[] CommonWords =
        {
            "the", "and", "for", "with", "you", "that", "this", "will", "from",
            "have", "has", "are", "our", "your", "their", "been", "more", "than",
            "such", "about", "into", "through", "should", "would", "could", "must",
            "what", "when", "where", "which", "who", "how", "they", "them", "then",
            "also", "both", "each", "few", "most", "other", "some", "very", "just",
            "like", "well", "good", "great", "new", "high", "best", "able", "need",
            "make", "work", "role", "team", "build", "using", "help", "join", "want",
            "look", "plus", "take", "provide", "ensure", "maintain", "develop",
            "candidate", "position", "company", "opportunity", "environment",
            "responsibilities", "requirements", "qualifications", "preferred",
            "required", "bonus", "ideal", "apply", "including", "following"
        };
    }
}
