---
title: Development Roadmap
order: 3
---

# LightBooksAgent — Development Roadmap

## Phase 0 — Foundation (Week 1–2)

**Goal**: Solution skeleton, local LLM connectivity, basic Blazor shell.

- [x] Solution structure with layered projects
- [x] Documentation (plan, system design, roadmap)
- [ ] Domain entities and interfaces in Core
- [ ] EF Core + SQLite setup with initial migration
- [ ] Local LLM client wired to `http://localhost:9931/v1` with health check
- [ ] Basic Blazor layout, navigation, placeholder pages
- [ ] Single "hello agent" proof-of-concept calling local LLM

**Deliverable**: App starts, calls local LLM, shows response in a test page.

---

## Phase 1 — Workflow Core (Week 3–4)

**Goal**: MAF workflow shell with HITL gates and checkpoints.

- [x] MAF sequential publishing workflow definition (`PublishingWorkflowFactory`)
- [x] HITL RequestPort executors for each gate type (`HitlGateFactory` prepare/port/apply pattern)
- [x] Checkpoint manager (JSON file store + SQLite run metadata via `WorkflowCheckpointService`)
- [x] Workflow host with event pump, resume, and HITL response submission (`PublishingWorkflowHost`)
- [x] Strategy-pattern step handlers with dispatcher (`IPublishingStepHandler` / `PublishingStepDispatcher`)
- [x] Comprehensive workflow unit tests (79 tests in `LightBooksAgent.Workflows.Tests`)
- [ ] `HitlService` — create/resolve review requests (wired in event processor; UI integration pending)
- [ ] Article project CRUD
- [ ] Workflow timeline UI (static stepper component)
- [ ] Human topic proposal UI (steps 2–3)

**Deliverable**: Create article → propose topic → confirm → workflow pauses at research gate.

---

## Phase 2 — Research Agent + Activity (Week 5–6)

**Goal**: Research pipeline with full observability.

- [ ] Research Agent with URL fetch tool
- [ ] Research brief generation and storage
- [ ] Research approval HITL gate
- [ ] Activity logging middleware on all agents
- [ ] SignalR `ActivityHub` for real-time feed
- [ ] Agent Monitor page (status cards, URL table)
- [ ] Research materials UI in Article Workspace

**Deliverable**: End-to-end: confirm topic → research → human approves research brief.

---

## Phase 3 — Memory System (Week 7)

**Goal**: Agents learn from past experience.

- [ ] `MemoryService` with episodic, semantic, procedural layers
- [ ] Local embedding generation via llama.cpp `/v1/embeddings`
- [ ] `QueryMemory` and `SaveObservation` agent tools
- [ ] Memory context injection before agent runs
- [ ] Post-feedback memory distillation
- [ ] Memory Explorer UI page

**Deliverable**: Second article run retrieves lessons from first article's feedback.

---

## Phase 4 — Writing Pipeline (Week 8–10)

**Goal**: Full draft generation with auto-review and human gates.

- [ ] Outline Architect Agent
- [ ] Writer Agent (section-by-section, category prompts)
- [ ] Technical Reviewer Agent
- [ ] Editor Agent
- [ ] Draft version history
- [ ] Outline and draft review UI with diff view
- [ ] Revision loop with configurable max iterations

**Deliverable**: Full draft pipeline from outline through human draft review.

---

## Phase 5 — Publish + MDWeb (Week 11)

**Goal**: Export artifacts and optional HTML generation.

- [ ] Publisher — Markdown export with YAML frontmatter
- [ ] MDWeb integration via `ISiteGenerator`
- [ ] Optional HTML export (default + WeChat themes)
- [ ] Publish confirmation HITL gate
- [ ] Reflection Agent for post-publish memory update

**Deliverable**: Publish Markdown article and optionally generate HTML site.

---

## Phase 6 — Control & Polish (Week 12–13)

**Goal**: Production-ready local publishing platform.

- [ ] Agent start/stop/pause controls
- [ ] Error recovery and retry from checkpoint
- [ ] Run history and archived articles
- [ ] Settings page (LLM, MDWeb, revision limits, URL allowlist)
- [ ] Dashboard with summary metrics
- [ ] Integration tests for workflow state transitions

**Deliverable**: Stable, observable, resumable publishing platform.

---

## Phase 7 — Learning Extensions (Optional)

- [ ] Category-specific fine-tuned prompt libraries
- [ ] Concurrent URL research (MAF concurrent workflow)
- [ ] Handoff workflow for complex revision routing
- [ ] Local knowledge base (upload reference docs, embed, search)
- [ ] Article series / multi-part publishing
- [ ] Export activity logs and memory reports

---

## Learning Milestones

| Phase | MAF concepts learned |
|-------|---------------------|
| 0 | Single agent, OpenAI-compatible local client |
| 1 | Sequential workflows, HITL RequestPort, checkpoints |
| 2 | Agent tools, middleware, structured output |
| 3 | Context providers, memory patterns |
| 4 | Multi-agent pipeline, agent specialization |
| 5 | External tool integration (MDWeb), post-workflow executors |
| 6 | Operational control, error handling, persistence |

---

## Current Status

**Phase 1 in progress** — MAF sequential workflow with RequestPort HITL gates, conditional edge routing, JSON checkpointing, and 81 passing unit tests. Next: HITL UI integration, workflow timeline, SignalR activity streaming.
