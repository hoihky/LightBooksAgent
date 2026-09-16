# LightBooksAgent

Local-first multi-agent platform for publishing technical articles about programming, game development, and game design.

## Stack

- **.NET 10** + **Blazor Server**
- **Microsoft Agent Framework** packages (workflow evolution in progress)
- **Local LLM** via llama.cpp at `http://localhost:9931/v1`
- **SQLite** for metadata and agent memory
- **MDWeb** for optional Markdown → HTML export

## Documentation

Markdown sources live in `docs-src/`. HTML is generated into `docs/` via MDWeb:

```bash
./scripts/generate-docs.sh
```

- [docs/index.html](docs/index.html) — documentation overview (portfolio-style landing page)
- [docs/plan.html](docs/plan.html) — workflow, agents, memory system
- [docs/system-design.html](docs/system-design.html) — architecture and data model
- [docs/roadmap.html](docs/roadmap.html) — development phases

## Prerequisites

1. .NET 10 SDK
2. llama.cpp server running at `http://localhost:9931/v1`
3. MDWeb project (for optional HTML export; set `MdWeb.ProjectPath` in appsettings)

## Run

```bash
dotnet build
dotnet run --project src/LightBooksAgent.Web
```

Open the URL shown in the console (typically `https://localhost:7xxx`).

## Publishing workflow (summary)

1. Human creates article project
2. Human proposes and confirms topic
3. Research Agent researches the confirmed topic
4. Human approves research
5. Outline → human approval → writing → auto review → human draft review
6. Final approval → publish Markdown → optional MDWeb HTML export
7. Reflection Agent stores lessons in agent memory

## Project structure

```
src/
  LightBooksAgent.Web/           Blazor UI + SignalR
  LightBooksAgent.Core/          Domain models and interfaces
  LightBooksAgent.Application/   Services + EF Core DbContext
  LightBooksAgent.Agents/        Agent prompts and runner
  LightBooksAgent.Workflows/     Publishing workflow orchestration
  LightBooksAgent.Infrastructure/ LLM, memory, MDWeb, URL fetch
```

## Configuration

Edit `src/LightBooksAgent.Web/appsettings.json`:

- `LocalLlm.BaseUrl` — llama.cpp OpenAI-compatible endpoint
- `MdWeb.ProjectPath` — path to your MDWeb installation
- `Publishing.ArticlesPath` — where published Markdown is stored
