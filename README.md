# 👑 DagoniteEmpire
![image](https://github.com/KrystianKempski/DagoniteEmpire/assets/19647553/36e5a213-fd3d-4849-bae7-826be70e2f0f)

<div align="center">
<img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/KrystianKempski/DagoniteEmpire/docker-image.yml?style=for-the-badge&logo=github-actions&logoColor=white">
<img alt="GitHub Release" src="https://img.shields.io/github/v/release/KrystianKempski/DagoniteEmpire?style=for-the-badge&logo=github&logoColor=white">
<img alt="Website" src="https://img.shields.io/website?url=https%3A%2F%2Fdagonite-empire.drik.it&style=for-the-badge&logo=kubernetes&logoColor=white&label=demo%20website">
</div>

## ⚔️ About
A comprehensive tool for starting an online, text-based RPG campaign, enabling multiple players to start long-term games in an original, well-defined world. Each player creates their own character with special abilities and skill sets, who then become a part of an adventure team with other players in a campaign led by a Game Master. 
Application database is made on PostGreSQL.

## 🎲 Try it yourself
You can access release and development version of the application here:
  - Release: [dagonite-empire.drik.it](https://dagonite-empire.drik.it)
  - Dev: [dagonite-empire-dev.drik.it](https://dagonite-empire-dev.drik.it)

You can use default test users or create your own account.

Default player account:
  - Login: `player@example.com`
  - Password: `Guest123*`

Default Game Master account:
  - Login: `gm@example.com`
  - Password: `Guest123*`

🚧 Work in progress.

## 🛠️ Used tools  
Whole application was developed with .NET 9.0 Blazor Server.

Multiple frameworks and tools were used for best apperience and user-friendly interface:
  - Syncfusion solutions components
  - Mudblazor
  - EntityFrameworkCore PostgreSQL (+ pgvector for SCRIBE)
  - Magick.NET
  - AutoMapper
  - jquery
  - toastr
  - nlog
  - bootstrap
  - Microsoft.SemanticKernel (+ Ollama connector) – agent layer for SCRIBE
  - OpenTelemetry (opt-in via `OTEL_EXPORTER_OTLP_ENDPOINT`)

## 🧠 SCRIBE – AI memory for campaigns

SCRIBE (Semantic Campaign Retrieval and Intelligence Based Explorer) is an
optional in-app RAG / agent layer that lets players and GMs ask Polish-language
questions about their campaign history. It runs against a remote Ollama host
(GPU) and stores embeddings in PostgreSQL via pgvector.

Docs:
  - [docs/SCRIBE_ARCHITECTURE.md](docs/SCRIBE_ARCHITECTURE.md) – architecture, data model, RAG flow
  - [docs/SCRIBE_DEPLOYMENT.md](docs/SCRIBE_DEPLOYMENT.md) – deploy app + GPU host
  - [docs/SCRIBE_QUICKREF.md](docs/SCRIBE_QUICKREF.md) – endpoints, request shapes, config keys
  - [docs/GPU_SERVER_SETUP.md](docs/GPU_SERVER_SETUP.md) – Ollama + ROCm/CUDA setup

## 🏗️ Deploy
Application is built automatically when new release is published using Github Actions as a Docker image and pushed to DockerHub. FluxCD residing on private Kubernetes cluster is monitoring DockerHub registry and when a new version is ready it recreates the app container with new image wihitn 5 minutes. Deployment configuration is set using custom helm chart available in [drik-homelab-helm-charts](https://github.com/drikqlis/drik-homelab-helm-charts) repository and managed by FluxCD.

To build application from imported code, local PostreSQL server is required, with authorisation specified in appsetting.jsnon.
 
