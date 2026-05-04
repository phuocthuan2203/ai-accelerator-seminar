# Prompt Template Bundle — AI-Assisted SDLC Workflow

This directory contains a complete set of system prompts that guide AI agents through a rigorous, SDLC-aligned workflow: from raw requirements to working, tested code.

## Overview

The workflow progresses through five prompts, each producing artifacts that feed into the next:

```
01 Requirement Engineering
    ↓ (SRS with user stories & acceptance criteria)
02 Inception Sprint (Analysis & Design)
    ↓ (Global architecture, state machines, ERD, security model)
03 Sprint Design Document
    ↓ (Per-sprint flows, API contracts, sequence diagrams, class designs)
04 Sprint Task Breakdown
    ↓ (Atomic implementation tasks, PROGRESS.md, VERIFICATION.md)
00 Agent Workflow
    ↓ (Execution standards, verification gates, testing discipline)
    ✅ Working, tested code
```

## What Changed (vs. Previous Version)

This version addresses three critical issues discovered in real projects:

### Issue 1: Design Misalignment (Fixed by embedding sequence diagrams)
**Problem:** Agents silently skip rules or contradict the design.
**Root cause:** No hard link between the design document and the task file.
**Solution:** Task files now include a mandatory **Sequence Diagram Reference** section that embeds the exact diagram from the Sprint Design Document. Agents must follow it step-by-step or raise issues immediately.

### Issue 2: Task Files Too Broad (Fixed by precise specifications)
**Problem:** Implementation Skeleton, Rule Checklist, Edge Cases, and Affected Files all allowed inference, causing different agents to make different assumptions.
**Solution:** 
- **Rule Checklist** now requires: exact rule (verbatim), source citation (§X.Y or diagram step), AND proposed test name.
- **Implementation Skemust update leton** now shows: full class names, exact method signatures (parameter types + return types), and explicit "must NOT do" guards.
- **Edge Cases** now specify: trigger input (exact value), expected output (HTTP code + JSON shape), and traceability to a rule row.
- **Affected Files** now include: "Must-contain" checklist of class/method names as verification checkpoints.

### Issue 3: Poor Handoff Quality (Fixed by structured output)
**Problem:** Agents wrote generic summaries ("service layer implemented") and missed forward-looking dependencies (new interfaces, DI registrations, migrations).
**Solution:** PROGRESS.md handoff is now a **structured checklist**:
- **What now exists:** Exact file paths, class names, DI registrations, migrations, env vars, flags, message queues.
- **What NEXT task must know:** Explicit forward-looking dependencies (unimplemented interfaces, schema assumptions, config expectations).
- **Deviations:** Any deviation from design with reason.

## New Features

### Sequence Diagrams as Guardrails
- Sprint Design Document (03) now enforces a **Sequence Diagram Completeness Rule**: all alt/else branches, layer labels, state transitions, and side effects must be shown.
- Task files (04) embed the diagram directly in a new **Sequence Diagram Reference** section.
- Agent Workflow (00) adds step **1.5**: read and internalize the diagram before coding. **Agents cannot skip or contradict the diagram without raising issues.**

### Feature Flags
- New **Feature Flags** section in task files tracks which flows are gated behind toggles.
- Prevents accidental cross-sprint logic leakage (e.g., implementing sprint N+1 code in sprint N).
- Agent Workflow (00) adds verification check: flags are registered and not inadvertently toggled.

## Key Principles

### 1. Verbatim Traceability
Every design rule in a task file is quoted verbatim from the design document, with source citation (§X.Y or diagram step). No paraphrasing.

### 2. Precise Specifications
- Method signatures are complete (class name, method name, parameter types, return type).
- Edge cases specify exact triggering input and exact expected output (HTTP code + JSON shape, or UI state label).
- "Must-contain" checklists in Affected Files serve as completion checkpoints.

### 3. Diagram-Driven Implementation
Sequence diagrams are not optional reading—they are embedded in every task file and agents must follow them exactly or raise issues immediately.

### 4. Structured Handoff
Handoff notes are not free-form. Agents answer specific questions (file paths, class names, DI registrations, forward-looking dependencies) to ensure the next session starts with full context.

### 5. Feature Flag Awareness
Every task declares which flows it gates or gates on. This prevents silent cross-sprint logic corruption.

## How the Pieces Fit Together

### The Planning Chain
1. **04 (Task Breakdown)** extracts rules from the design using a strict **Rule Ledger** that quotes verbatim sources.
2. Each rule gets a **proposed test name** at planning time, not implementation time.
3. Each file gets a **Must-contain** checklist so implementers know exactly what to verify before closing.

### The Implementation Chain
1. **00 (Agent Workflow)** step 1.5 forces the agent to read the **Sequence Diagram Reference** first.
2. The agent maps each diagram step to the code it will write—no steps skipped, no steps added.
3. Edge cases are tested by name (the proposed test from the task file), and each test traces back to a rule row.
4. The **Verification Checklist** in 00 forces checks on feature flags and sequence diagram adherence.

### The Handoff Chain
1. After each task, **PROGRESS.md** captures exact system state: files, classes, DI registrations, migrations, flags.
2. It also captures forward-looking dependencies: "IUserRepository is defined but not implemented yet—next task must add the concrete class."
3. When a new session starts, the agent reads PROGRESS.md first and understands exactly where the previous task left off.

## Example: One Task's Journey

**In 04 (Task Breakdown):**
```
Design Rule Checklist, Row #2:
| # | Exact rule | Source | Owner files | Proposed test name |
| 2 | User must wait 3 hours after 3 failed attempts | Sprint Design Doc §3.2, table "Login Rules" | Application/Services | LoginService_ReturnsTooManyAttemptsError_WhenCooldownActive |
```

**In the task file (created by 04):**
```markdown
## Sequence Diagram Reference
[Diagram shows: 3. LoginService checks cooldown state; 3a. If cooldown active, return 429 Too Many Requests]

## Design Rule Checklist
| # | Exact rule | Source | ... | Proposed test name |
| 2 | "After 3 failed attempts, user must wait 3 hours" | Sprint Design §3.2 | LoginService | LoginService_ReturnsTooManyAttemptsError_WhenCooldownActive |

## Edge Cases
| # | Rule | Trigger | Expected output |
| 2 | 2 | POST /api/login at minute 5 of 180-minute cooldown | 429 Too Many Requests; body: { "error": "try_again_in_seconds": 10500 } |
```

**During implementation (00 Agent Workflow):**
1. Step 1.5: Agent reads diagram, sees step 3a about cooldown check.
2. Implements `LoginService.IsInCooldown()` method.
3. Writes test `LoginService_ReturnsTooManyAttemptsError_WhenCooldownActive` (exact name from task file).
4. Verifies the 429 response matches the exact JSON shape in the edge case.

**After task completion (PROGRESS.md):**
```markdown
**What now exists:**
- Files: `backend/application/Services/LoginService.cs` (CREATE)
- Classes: `LoginService` (Application), `LoginAttempt` domain entity
- DI: `Program.cs: services.AddScoped<ILoginService, LoginService>()`
- DB migrations: `Migration_0002_AddLoginAttempts`: login_attempts(id, user_id FK, failed_count, last_failed_at)

**What NEXT task must know:**
- `LoginService.IsInCooldown()` is implemented; next task that handles "login completed" flow must reset `failed_count` in this table
- `login_attempts` table is created; next task querying it must filter by `user_id = current_user_id`
```

## Files in This Bundle

| File | Purpose |
|------|---------|
| `00_agent_workflow.md` | Execution standards, verification gates, testing discipline, DI/naming conventions, git workflow |
| `01_requirement_engineering.md` | Transform raw requirement into SRS with user stories and acceptance criteria |
| `02_analysis_and_design_inception_sprint.md` | Produce global architecture, domain model, BCE classes, state machines, ERD, security design |
| `03_sprint_design_doc.md` | Design per-sprint flows, API contracts, sequence diagrams, class designs; enforces sequence diagram completeness |
| `04_sprint_task_breakdown.md` | Break design into atomic tasks; produce PROGRESS.md and VERIFICATION.md; enforces verbatim rule traceability and structured handoff |
| `README.md` (this file) | Overview and rationale |

---

## Using a Different Tech Stack?

These templates were developed for a specific project and contain hardcoded references to ASP.NET Core, Angular, PostgreSQL, and other technologies. **If you use a different stack, the structure and principles are universal — only specific references need updating.** Below is a map of where to find and substitute them.

### Runtime & Backend Framework

| What's hardcoded | Where | What to change |
|---|---|---|
| `dotnet` CLI commands (`dotnet build`, `dotnet test`, `dotnet ef`) | `00_agent_workflow.md` Sections 1, 4, 6, 7 | Your runtime's CLI (e.g., `java`, `python`, `npm run`, `cargo`) |
| ASP.NET Core patterns (`[HttpPost]`, `ActionResult<T>`, middleware, filters) | `00_agent_workflow.md` Section 2 | Your backend framework's controller pattern |
| C# syntax in skeletons (`namespace`, `async Task<T>`, `_camelCase` fields) | `04_sprint_task_breakdown.md` Task file format | Your language's syntax and idioms |
| EF Core migrations (`DbContext`, `ToListAsync`, `dotnet ef migrations`) | `00_agent_workflow.md` Section 7, `04_sprint_task_breakdown.md` PROGRESS.md | Your ORM or migration tool |

### Frontend Framework

| What's hardcoded | Where | What to change |
|---|---|---|
| Angular CLI (`ng serve`, `ng test`, `ng build`) | `00_agent_workflow.md` Sections 1, 3, 6 | Your frontend framework's CLI |
| Standalone components, ReactiveFormsModule, `ActivatedRoute` | `00_agent_workflow.md` Section 3 | Your component/form/routing pattern |
| `environment*.ts` config files | `00_agent_workflow.md` Section 3, `04_sprint_task_breakdown.md` PROGRESS.md | Your frontend config file names |

### Database & Messaging

| What's hardcoded | Where | What to change |
|---|---|---|
| PostgreSQL 16 + pgvector, port 5434 in Docker | `00_agent_workflow.md` Local Development Environment | Your database engine, extensions, and port |
| RabbitMQ (queues/exchanges mentioned in PROGRESS.md format) | `04_sprint_task_breakdown.md` PROGRESS.md format | Your message broker (Kafka, AWS SQS, Azure Service Bus, etc.) |
| SignalR hubs/groups | `04_sprint_task_breakdown.md` PROGRESS.md format | Your real-time communication layer (WebSockets, SSE, Pusher, etc.) |

### Authentication & Security

| What's hardcoded | Where | What to change |
|---|---|---|
| JWT via `HttpContext.User` / claims | `00_agent_workflow.md` Section 2 and Section 7 | Your auth mechanism (session cookies, OAuth, OIDC, API keys, etc.) |
| BCrypt password hashing (referenced in domain examples) | `04_sprint_task_breakdown.md` PROGRESS.md example | Your hashing library |

### Testing & Tooling

| What's hardcoded | Where | What to change |
|---|---|---|
| xUnit + Moq (backend test framework) | `00_agent_workflow.md` Section 6 | Your backend test framework (JUnit, pytest, Go test, etc.) |
| Jasmine + Angular TestBed + ChromeHeadless | `00_agent_workflow.md` Section 6 | Your frontend test runner (Jest, Vitest, Playwright, etc.) |
| Coverlet (code coverage tool) | `00_agent_workflow.md` Section 6.3 Gates 1 & 2 | Your coverage tool (Istanbul/c8, JaCoCo, go cover, etc.) |
| Swagger/Swashbuckle (API documentation) | `00_agent_workflow.md` Section 4 Verification Checklist | Your API explorer (Redoc, Stoplight, Postman, etc.) |

### Naming Conventions & Structure

| What's hardcoded | Where | What to change |
|---|---|---|
| Root namespace `LocalCommunityToolLending.{Layer}` | `00_agent_workflow.md` Section 2, `04_sprint_task_breakdown.md` skeletons | Your project's root namespace |
| 5-layer DDD folders: `domain/`, `application/`, `infrastructure/`, `api/` | `00_agent_workflow.md` Section 2, `04_sprint_task_breakdown.md` paths | Your backend folder structure |
| Angular folder structure: `core/`, `features/`, `shared/` | `00_agent_workflow.md` Section 3 | Your frontend folder structure |
| Git branch pattern: `feature/task-{N}-{short-slug}` | `00_agent_workflow.md` Section 5 | Your branch naming convention |
| Config files: `appsettings.json` (.NET), `environment.ts` (Angular) | `00_agent_workflow.md` Section 4, `04_sprint_task_breakdown.md` PROGRESS.md | Your config file names |
| Personalization: "HI BOSS THUAN" (step 0), "Boss Thuận does the merge" (step 10) | `00_agent_workflow.md` Sections 1 and 10 | Remove or replace with your own team conventions |

---

## Project Structure Reference

> **Fill this section in once when you start a new project.** The agent workflow and task files reference these definitions — you never need to edit them for structure changes. Only update this section.

### Layers

Define your project's layer structure here. The agent workflow will refer to these folder paths and responsibilities.

| Layer | Folder path | Responsibilities |
|-------|-------------|-----------------|
| Domain | `backend/domain/` | Entities, value objects, domain logic (no HTTP or DB details) |
| Application | `backend/application/` | Use-case services, interfaces for repositories and external adapters |
| Infrastructure | `backend/infrastructure/` | ORM, migrations, repository implementations, external integrations |
| API | `backend/api/` | Controllers, request/response DTOs, middleware, DI root |
| Frontend | `frontend/` | UI layer (Angular app root) |

**How to adapt this table:**
- For a **monolith**: folder paths might be `src/Domain/`, `src/Application/`, etc.
- For a **pure backend**: remove the Frontend row.
- For **microservices**: repeat this table per service, with service-specific folder names.
- For a **different language/framework**: keep the layer names (Domain, Application, etc.) but update the folder paths to match your project.

### Commands

Define your project's build, run, and test commands here.

| Action | Command |
|--------|---------|
| Build backend | `cd backend && dotnet build` |
| Run backend | `dotnet run --project backend/api/YourApiProject.csproj` |
| Clean backend | `cd backend && dotnet clean && dotnet build` |
| Run backend tests | `dotnet test ./backend/{layer}/Tests/{layer}.Tests.csproj` |
| Build frontend | `cd frontend && npm run build` |
| Run frontend dev | `cd frontend && npm start` |
| Lint frontend | `cd frontend && npm run lint` |
| Run frontend tests | `cd frontend && npm test` |
| Run frontend tests (CI) | `cd frontend && ng test --watch=false --browsers=ChromeHeadless` |
| Frontend coverage | `cd frontend && ng test --watch=false --code-coverage` |
| Add DB migration | `dotnet ef migrations add {Name} -p backend/infrastructure/{InfraProject}.csproj -s backend/api/{ApiProject}.csproj` |
| Apply DB migration | `dotnet ef database update -p backend/infrastructure/{InfraProject}.csproj -s backend/api/{ApiProject}.csproj` |

**How to adapt this table:**
- Replace `dotnet` commands with your runtime (e.g., `go build`, `cargo build`, `python -m pytest`)
- Replace Angular/npm commands with your frontend tool (e.g., `npm`, `yarn`, `pnpm`)
- Replace `backend/`, `frontend/` paths with your folder structure
- Replace `.csproj` project names with your actual project file names
- Add or remove rows to match your project's commands

---

## Getting Started with the Prompts

### Quick Overview (Five Steps)

1. **01 — Requirement Engineering:** Collect raw idea → Ask clarifying questions → Generate SRS
2. **02 — Inception Sprint:** Take SRS → Ask architecture questions → Generate global design (no code)
3. **03 — Sprint Design:** Take SRS + inception → Design per-sprint flows → Generate sequence diagrams
4. **04 — Task Breakdown:** Take all designs → Extract verbatim rules → Create atomic task files
5. **00 — Agent Execution:** Take one task → Implement code following diagrams exactly → Update PROGRESS.md

### Detailed Workflow

#### Step 1: Requirement Engineering (01_requirement_engineering.md)

**Input:** Raw requirement from audience  
**Output:** Software Requirements Specification (SRS)

```
Collect a raw idea ("I want to build a tool-lending platform")
→ Ask clarifying questions
→ Generate complete SRS with user stories & acceptance criteria
```

**What to look for in output:**
- User roles clearly defined
- Features described as user stories (As a..., I want to..., so that...)
- Acceptance criteria are testable (Given/When/Then format)

---

#### Step 2: Inception Sprint (02_analysis_and_design_inception_sprint.md)

**Input:** SRS from Step 1  
**Output:** Global architecture, domain model, state machines, ERD, security design

```
Take the SRS
→ Ask clarifying questions about architecture, scale, security
→ Generate Inception Sprint artifacts (no code yet)
```

**What to look for in output:**
- Actor identification (users, systems, stakeholders)
- Domain model (entities and relationships)
- BCE classes (Boundary, Control, Entity per use case)
- State machines for key entities
- Full ERD with tables, columns, relationships
- Security design (authentication, authorization, data protection)

**Key principle:** This is the "map" for all future sprints. Get this right.

---

#### Step 3: Sprint Design Document (03_sprint_design_doc.md)

**Input:** SRS + Inception Sprint document  
**Output:** Per-sprint flows, API contracts, sequence diagrams, class designs

```
For the first sprint:
→ Describe which use cases / flows are in scope (e.g., "UC-1 BF2, BF3, BF4")
→ Ask clarifying questions about behavior, edge cases, UI expectations
→ Generate detailed sequence diagrams for each flow
```

**What to look for in output:**
- Flows listed with clear scope (BF/AF IDs)
- API contracts for all endpoints (request/response, HTTP codes)
- **Sequence diagrams that show:**
  - All actors and layers
  - Both happy path AND alt/error branches
  - Layer labels (Presentation / API / Application / Domain / Infrastructure)
  - State transitions (reference Inception state machines)
  - Side effects (DB writes, message broker publishes)

**Critical check:** If diagrams are missing alt branches or layer labels, ask the agent to revise them. Incomplete diagrams cause misalignment later.

---

#### Step 4: Sprint Task Breakdown (04_sprint_task_breakdown.md)

**Input:** SRS + Inception Sprint document + Sprint Design Document  
**Output:** Atomic implementation tasks, PROGRESS.md, VERIFICATION.md

```
Break the sprint design into executable tasks
→ Agent extracts rules with verbatim quotes and source citations
→ Agent creates TASK-01, TASK-02, etc. with:
   - Sequence Diagram Reference (diagram embedded)
   - Design Rule Checklist (exact rules, sources, proposed test names)
   - Implementation Skeleton (full class names, method signatures)
   - Edge Cases (exact triggers, exact outputs)
   - Affected Files (must-contain checklists)
   - Feature Flags (if applicable)
```

**What to look for in output:**
- Each task has an embedded sequence diagram from Step 3
- Rules are quoted verbatim, not paraphrased
- Test names are proposed at planning time (e.g., `LoginService_ReturnsTooManyAttemptsError_WhenCooldownActive`)
- Edge cases specify exact HTTP codes, JSON shapes, or UI states
- Files have "must-contain" checklists (class/method names to verify)
- PROGRESS.md template is ready for handoff notes

**Critical check:** Read a few task files. Can you tell exactly what code needs to be written? If you're still inferring, the task is too vague.

---

#### Step 5: Agent Execution (00_agent_workflow.md)

**Input:** One task file from Step 4  
**Output:** Implemented, tested code

```
For each task:
→ Agent reads task file completely
→ Agent reads Sequence Diagram Reference (step 1.5 — MANDATORY)
→ Agent implements code following the diagram exactly
→ Agent writes tests named exactly as proposed in the Design Rule Checklist
→ Agent verifies Feature Flags are registered
→ Agent updates PROGRESS.md with structured handoff
```

**What to watch during implementation:**
- Step 1.5 is mandatory: agent must read sequence diagram before coding
- If the agent finds a conflict between the diagram and another section, it STOPS and logs it in PROGRESS.md
- Tests are named exactly as proposed (no renaming)
- Handoff is a structured checklist, not free-form notes

**After task completes:**
- PROGRESS.md is updated with:
  - Exact file paths created/modified
  - Class and method names
  - DI registrations
  - DB migrations
  - Env vars, feature flags
  - **Forward-looking dependencies** (what the next task must know)

---

### Multi-Session Workflow

When handing off between sessions (agent → AI → human → next agent):

#### At Session Start
```
Agent reads PROGRESS.md from previous session
→ Understands exactly what was implemented
→ Knows what was NOT implemented yet
→ Knows what dependencies exist
→ Starts the next task with full context
```

#### During Session
```
Agent reads the Sequence Diagram Reference
→ Maps each diagram step to code
→ Writes tests with proposed names
→ Doesn't add steps not shown
→ Doesn't skip steps shown
```

#### At Session End
```
Agent updates PROGRESS.md:
- Files, classes, DI, migrations, env vars, flags
- What NEXT task must know
- Any deviations from design
```

---

### Troubleshooting

#### Problem: "The task file is still too vague"

**Solution:** The planning agent (04) didn't extract rules tightly enough.
- Check the Design Rule Checklist — are rules quoted verbatim or paraphrased?
- Check the Implementation Skeleton — do method signatures show parameter types?
- Check Edge Cases — do they specify exact HTTP codes and JSON shapes?
- Re-run 04 and ask it to be more prescriptive.

#### Problem: "The agent implemented something not in the design"

**Solution:** The agent either:
1. Didn't read the Sequence Diagram Reference (step 1.5 was skipped)
2. Read it but didn't stop when it found a conflict

- Check PROGRESS.md Issues Log — was a conflict logged?
- If not, the agent violated step 1.5. Re-run with explicit instruction to stop if confused.
- If logged, review the conflict with the design team and update the design or task.

#### Problem: "The handoff notes don't tell me what to do next"

**Solution:** The handoff is too generic.
- Check that "What the NEXT task must know" section is specific and forward-looking.
- Examples of good handoffs: "IUserRepository interface is defined but NOT implemented yet—next task must add concrete class in Infrastructure and register in DI"
- Bad handoff: "Service layer implemented"
- Re-run 04 and ask the agent to be specific about unfinished business.

#### Problem: "Agent flagged a conflict in PROGRESS.md Issues Log"

**Solution:**
1. Read the conflict description.
2. Check if it's a real design problem (e.g., diagram is incomplete) or an agent misunderstanding.
3. If design problem: update Sprint Design Document (03), then update the task file.
4. If agent misunderstanding: clarify in the task file and re-run the agent.

---

### For Your Seminar Demo

#### Flow (~70 minutes total)

1. **Live audience:** Collect raw requirement (5 min)
2. **Show 01:** Run requirement engineering (10 min) → produce SRS
3. **Show 02:** Run inception sprint (10 min) → produce architecture
4. **Show 03:** Run sprint design (15 min) → produce flows + diagrams
5. **Show 04:** Run task breakdown (10 min) → produce task files
6. **Show 00:** Run agent on first task (15 min) → produce working code
7. **Reflection:** Explain how documentation guided the AI (5 min)

#### Key Points to Highlight

- **Before:** Vague task → agent inference → misalignment
- **After:** Precise task + diagram → agent follows exactly → correct code
- **Handoff:** Structured notes → next agent inherits context → no blind spots
- **Feature flags:** Prevent logic corruption across sprints

---

### Common Questions

**Q: Do I need to run all five steps every time?**  
A: No. Steps 1–2 are one-time for a new product. Step 3 happens once per sprint. Steps 4–5 repeat for each task in the sprint.

**Q: Can I skip the Sequence Diagram Reference?**  
A: No. It's the guardrail that keeps agents on track. If it's missing or incomplete, stop and revise.

**Q: What if the design is wrong and the agent finds it?**  
A: Good! The agent should log it in PROGRESS.md Issues Log and stop. Fix the design, update the task, and re-run the agent.

**Q: How do I know a task is complete?**  
A: Check the Definition of Done in the task file:
- All design rules implemented
- All edge cases handled
- All files in Affected Files created/modified
- All tests pass
- PROGRESS.md updated with structured handoff

**Q: What if an agent keeps making the same mistake?**  
A: It probably means the task file is still vague. Tighten the Implementation Skeleton or Edge Cases to remove inference, then re-run.

## Testing & Verification

Each prompt is designed to enable three levels of verification:

1. **Per-task (in 04 task files):** Edge cases are testable, design rules have proposed test names, "must-contain" checklists are verifiable.
2. **Integration (in 04 VERIFICATION.md):** Agent-executed checks for cross-task consistency, human-executed checks for full workflows.
3. **Post-completion (in 00 Verification Checklist):** Feature flags, sequence diagram adherence, layer dependencies.

---

**Last updated:** 2026-05-01  
**Status:** Stable, tested on real projects
