namespace ErCv.Web.Data
{
    public record SampleJobDescription(string Id, string Label, string Text);

    /// <summary>Curated job descriptions a visitor can load with one click (APP-03).</summary>
    public static class SampleJobDescriptions
    {
        public static readonly IReadOnlyList<SampleJobDescription> All = new[]
        {
            new SampleJobDescription("backend", "Backend", """
                Senior Backend Engineer (Go)

                We are looking for a Senior Backend Engineer with 6+ years of experience building high-throughput services in Go. You will design event-driven systems on Kafka, own services running on Kubernetes, and work with PostgreSQL and Redis.

                Requirements:
                - Strong Go experience and distributed systems fundamentals
                - Hands-on Kafka, gRPC and REST API design
                - PostgreSQL, Redis, AWS
                - CI/CD, observability, on-call ownership
                """),
            new SampleJobDescription("frontend", "Frontend", """
                Frontend Engineer (React)

                Join our product team to build a fast, accessible web app. You will work with React, TypeScript, Redux and GraphQL, write component and end-to-end tests, and collaborate closely with designers.

                Requirements:
                - 3+ years with React and TypeScript
                - HTML, CSS, responsive design, accessibility
                - Jest, Cypress, Git
                - Nice to have: Next.js, Node.js
                """),
            new SampleJobDescription("fullstack", "Full stack", """
                Full Stack Engineer

                Help us build a SaaS product end to end. You will ship features across a Next.js and TypeScript frontend and a Node.js API backed by PostgreSQL, deploy with Docker on AWS, and review code with the rest of a small team.

                Requirements:
                - 4+ years of full stack experience
                - TypeScript, React, Next.js, Node.js
                - PostgreSQL, REST API and GraphQL design
                - Docker, Git, CI/CD
                - Nice to have: Redis, Terraform
                """),
            new SampleJobDescription("data", "Data", """
                Data Engineer

                Build and maintain batch and streaming pipelines. You will use Python, SQL, Spark and Airflow on AWS, model data in Snowflake and keep pipelines reliable and tested.

                Requirements:
                - 4+ years of data engineering experience
                - Python, SQL, Spark, Airflow
                - AWS (S3, Glue, Redshift), Snowflake
                - Kafka is a plus
                """),
            new SampleJobDescription("devops", "DevOps", """
                DevOps Engineer

                Own our cloud platform. You will manage infrastructure as code with Terraform, run workloads on Kubernetes and Docker, build CI/CD pipelines with GitHub Actions and improve monitoring with Prometheus and Grafana.

                Requirements:
                - 5+ years in DevOps or SRE
                - AWS, Terraform, Kubernetes, Docker
                - Linux, Bash, Python scripting
                - Incident response and observability
                """),
            new SampleJobDescription("junior", "Junior", """
                Junior Software Developer

                Great first or second role. No senior experience needed. You will work with JavaScript, React and Node.js alongside mentors, fix bugs and ship small features.

                Requirements:
                - 0-2 years of experience or relevant internships
                - JavaScript, HTML, CSS
                - Basic Git and SQL
                - Eagerness to learn
                """)
        };
    }
}
