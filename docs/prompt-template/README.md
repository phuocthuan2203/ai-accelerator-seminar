# Prompt Template Bundle — AI-Assisted SDLC Workflow

This directory contains a complete set of system prompts that guide AI agents through a rigorous, SDLC-aligned workflow: from raw requirements to working, tested code. **These templates are portable—they work for any project; only the root README.md project structure needs configuration.**

## Files in This Directory

| File | Purpose |
|------|---------|
| `01_requirement_engineering.md` | Transform raw requirement into SRS (Software Requirements Specification) with user stories and acceptance criteria |
| `02_analysis_and_design_inception_sprint.md` | Produce global architecture, domain model, BCE classes, state machines, ERD, security design (happens once per project) |
| `03_sprint_design_doc.md` | Design per-sprint flows, API contracts, sequence diagrams, detailed class designs (once per sprint) |
| `04_sprint_task_breakdown.md` | Break sprint design into atomic, testable tasks; produce PROGRESS.md and VERIFICATION.md (once per sprint) |
| `00_agent_workflow.md` | Execution standards for implementing one task: verification gates, testing discipline, handoff structure (one task at a time) |
| `README.md` (this file) | Guide to using these templates, key principles, adaptation map |

## Workflow Sequence

```
Raw Idea
  ↓ [01_requirement_engineering.md]
SRS Document (15–20 pages)
  ↓ [02_analysis_and_design_inception_sprint.md]
Inception Report (domain model, ERD, architecture)
  ↓ [03_sprint_design_doc.md]
Sprint Design Document (API contracts, sequences, rules)
  ↓ [04_sprint_task_breakdown.md]
PROGRESS.md + 8 atomic task files
  ↓ [00_agent_workflow.md] (per task)
Implemented, tested code
  ↓
(Repeat steps 3–5 for next sprint)
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

---

## New Features

### Sequence Diagrams as Guardrails
- Every task file embeds the exact sequence diagram from the sprint design. Agents cannot skip steps or add steps not shown.
- If a conflict is found, the agent must stop and log it in PROGRESS.md Issues Log before proceeding.

### Feature Flags
- New **Feature Flags** section in task files tracks which flows are gated behind toggles.
- Prevents accidental cross-sprint logic leakage.

### Structured Handoff
- PROGRESS.md is not free-form notes. It's a **structured checklist** that captures:
  - Exact file paths, class names, DI registrations, migrations, env vars, feature flags
  - **What the NEXT task must know** (forward-looking dependencies, unimplemented interfaces, schema assumptions)
  - Deviations from design with reason

---

## Key Principles

### 1. Verbatim Traceability
Every design rule in a task file is quoted verbatim from the design document, with source citation (§X.Y or diagram step). No paraphrasing.

### 2. Precise Specifications
- Method signatures are complete (class name, method name, parameter types, return type).
- Edge cases specify exact triggering input and exact expected output (HTTP code + JSON shape, UI state label).
- "Must-contain" checklists in Affected Files serve as completion checkpoints.

### 3. Diagram-Driven Implementation
Sequence diagrams are embedded in every task file. Agents must follow them exactly or raise issues immediately.

### 4. Structured Handoff
Handoff notes answer specific questions (file paths, class names, DI registrations, forward-looking dependencies) to ensure the next session starts with full context.

---

## Adapting These Templates to Your Project

These templates are portable—they work for any tech stack. However, they do reference specific technologies (ASP.NET Core, Angular, PostgreSQL, etc.) in examples and skeletons.

**When you start a new project:**

1. **Configure the project structure:** See `../README.md` → **Project Structure Reference** section.
   - Define your layers and folder paths (Layers table).
   - Define your build, run, and test commands (Commands table).
   - Only update these two tables; the agent workflow and task templates will reference them.

2. **Adapt tech-specific examples:** If you use a different tech stack, update the examples in the templates:

   | What to update | Files | Examples |
   |---|---|---|
   | **Backend runtime & framework** | `00_agent_workflow.md` Section 2 (Conventions) | Change `.NET` examples to your language (Java, Python, Go, etc.) |
   | **Frontend framework** | `00_agent_workflow.md` Section 3 (Conventions) | Change `Angular` to React, Vue, Svelte, etc. |
   | **Database & ORM** | `04_sprint_task_breakdown.md` PROGRESS.md example | Change `EF Core` to your ORM (Hibernate, SQLAlchemy, etc.) |
   | **Testing frameworks** | `00_agent_workflow.md` Section 6 (Test Execution) | Change `xUnit`, `Jasmine` to your test runners |
   | **Authentication** | `00_agent_workflow.md` Section 2 (Code Rules, Security) | Change `JWT + HttpContext` to your auth mechanism |
   | **Naming conventions** | All files | Change namespace patterns, class/method naming to your language's idioms |
   | **Personalization** | `00_agent_workflow.md` Steps 0 and 10 | Update "HI BOSS THUAN" and merge instructions to your team |

3. **All structural definitions live in `../README.md` only.** Don't replicate them in task files or prompts. If the project structure changes, update the root README once and all templates automatically follow it.

---

---

## End-to-End Workflow: Concrete Example

> This section shows how to apply all five templates sequentially, using a concrete example: **a simple course marketplace** where instructors post courses and students enroll.

### Phase 1: Requirement Engineering (01_requirement_engineering.md)

**Input:** Raw idea ("We want to build a platform where instructors can post courses and students can enroll, like mini Udemy. Instructors get paid.")

**Process:**
1. Copy the system prompt from `01_requirement_engineering.md`
2. Provide the raw requirement
3. AI asks ~12 clarifying questions (Product & goals, Users & roles, Core features, Payments, Data, Compliance, Non-functional, Technology, Timeline, Success)
4. You answer the questions
5. AI generates complete SRS (15–20 pages)

**Output:** `docs/SRS.md` with sections: Introduction, Overall Description, System Features (as user stories), External Interface Requirements, Data Requirements, Quality Attributes, Appendices

---

### Phase 2: Inception Sprint (02_analysis_and_design_inception_sprint.md)

**Input:** SRS from Phase 1

**Process:**
1. Copy the system prompt from `02_analysis_and_design_inception_sprint.md`
2. Provide the SRS
3. AI conducts a design sprint to produce:
   - Domain model (entities: User, Instructor, Student, Course, Lesson, Quiz, Enrollment, Payment, Payout)
   - State machines for high-risk entities (Course: Draft → Published → Archived; Enrollment: Pending → Active → Completed)
   - Sequence diagrams mapping use cases to layer interactions
   - BCE (Boundary-Control-Entity) classes per use case
   - ERD with tables, columns, constraints
   - Architecture (layered: Domain/Application/Infrastructure/API; tech stack decisions)

**Output:** `docs/Inception-Sprint-Report.md` (the "map" for all future sprints)

---

### Phase 3: Sprint Design Document (03_sprint_design_doc.md)

**Input:** SRS + Inception Report

**Process:**
1. Copy the system prompt from `03_sprint_design_doc.md`
2. Provide SRS + Inception Report
3. Specify which use cases are in scope for Sprint 1 (e.g., UC-1 Register, UC-2 Create Course, UC-3 Publish Course)
4. AI designs the sprint in detail:
   - Overview (scope in/out)
   - API contracts (endpoints, request/response, HTTP codes)
   - Data model (entities, DTOs)
   - Sequence diagrams (all happy path + alt/error branches, layer labels, state transitions, side effects)
   - Business rules
   - Validation rules
   - Error handling

**Output:** `docs/sprint-1/Sprint-1-Design-Document.md` (API contracts, sequences, rules)

---

### Phase 4: Sprint Task Breakdown (04_sprint_task_breakdown.md)

**Input:** SRS + Inception Report + Sprint Design Document

**Process:**
1. Copy the system prompt from `04_sprint_task_breakdown.md`
2. Provide all three design documents
3. AI breaks the sprint into ~8 atomic tasks:
   - TASK-01: Domain layer (entities, exceptions)
   - TASK-02: Database migrations
   - TASK-03: Repositories
   - TASK-04: Services (Application layer)
   - TASK-05: API controllers
   - TASK-06: Frontend services & auth guard
   - TASK-07: Frontend forms
   - TASK-08: Frontend pages
4. For each task, AI produces:
   - **Design Rule Checklist:** exact rules (verbatim), source citations, proposed test names
   - **Sequence Diagram Reference:** the exact diagram for this task
   - **Implementation Skeleton:** full class names, method signatures (parameter types, return types)
   - **Edge Cases Handled:** trigger input, expected output (HTTP codes, JSON shape, UI state)
   - **Affected Files:** "must-contain" checklists (class/method names to verify)
   - **Feature Flags:** if any gates this task
5. AI also produces:
   - `docs/sprint-1/PROGRESS.md` (progress tracker template, rule coverage watchlist, handoff checklist, issues log)
   - `docs/sprint-1/VERIFICATION.md` (sprint-level verification plan)

**Output:** `docs/sprint-1/PROGRESS.md`, `docs/sprint-1/VERIFICATION.md`, `docs/sprint-1/tasks/TASK-01.md` through `TASK-08.md`

---

### Phase 5: Implementation per Task (00_agent_workflow.md)

**For each task (repeat 8 times for Sprint 1):**

**Input:** One task file (e.g., `TASK-01-domain-entities.md`)

**Process:**
1. Copy the system prompt from `00_agent_workflow.md` in a new session
2. Provide the task file
3. Agent executes Step 0: Say "HI BOSS THUAN"
4. Agent executes Steps 1–7:
   - Read task file completely
   - Read PROGRESS.md to check for prior issues
   - Read Sequence Diagram Reference (Step 1.5 — mandatory)
   - Create feature branch: `git checkout -b feature/task-01-domain-entities`
   - Load environment (backend, frontend dev servers)
   - Implement with tests (never without tests):
     - Write domain entities (User, Instructor, Course)
     - Write enums (CourseStatus)
     - Write exceptions
     - Write unit tests with proposed names from Design Rule Checklist
   - Run tests locally → 0 failures
   - Verify all rules from Design Rule Checklist are implemented and tested
   - Verify "must-contain" checklist items exist
5. Agent executes Steps 8–10:
   - Update PROGRESS.md with structured handoff:
     - Files created/modified (exact paths)
     - Classes/methods introduced
     - DI registrations
     - DB migrations
     - Env vars, feature flags
     - **What the NEXT task must know** (forward-looking dependencies)
   - Commit with conventional commit: `feat(domain): add User, Instructor, Course entities`
   - Push branch and open PR
   - Wait for review/merge

**Output:** Implemented, tested code

**After merge:** Next session reads PROGRESS.md handoff and starts TASK-02 with full context.

---

## Summary: The Full Loop

```
Raw Idea
  ↓ [01] Ask & Answer
SRS (15–20 pages)
  ↓ [02] Design Sprint
Inception Report (domain model, ERD, arch)
  ↓ [03] Design per-sprint
Sprint 1 Design Doc (API, sequences, rules)
  ↓ [04] Break into tasks
PROGRESS.md + TASK-01 through TASK-08
  ↓ [00] Implement Task 1
Code + tests + PR + handoff
  ↓ [Review & merge]
  ↓ [00] Implement Task 2
Code + tests + PR + handoff
  ↓ [Repeat for TASK-03–08]
Sprint 1 running (auth + course CRUD)
  ↓ [Repeat 03–00 for Sprint 2, 3, ...]
Complete product
```

**Time estimates:**
- Phase 1 (SRS): 2–3 hours
- Phase 2 (Inception): 4–6 hours
- Phase 3 (Sprint Design): 3–4 hours
- Phase 4 (Task Breakdown): 2–3 hours
- Phase 5 (Implementation, per task): 1–3 hours per task
- **Total per sprint: ~25–35 hours (design + code)**

---

## Before You Start

1. **Ensure your project structure is defined** in the root `../README.md` → **Project Structure Reference**
   - Layers table: folder paths for Domain, Application, Infrastructure, API, Frontend
   - Commands table: build, run, test, migration commands
2. **Copy the five prompt files** as-is into your project (they're portable)
3. **Start with Phase 1** when you have a raw idea from a stakeholder
4. **Never skip** the Sequence Diagram Reference (Phase 5 Step 1.5) — it's your guardrail

---

## FAQ

**Q: Do I need to run all five phases every time?**  
A: No. Phases 1–2 happen once per project. Phase 3 happens once per sprint. Phases 4–5 repeat for each task in the sprint.

**Q: Can I skip the Sequence Diagram Reference?**  
A: No. It's the guardrail that prevents agents from drifting. If it's missing or incomplete, stop and revise.

**Q: What if the agent finds a design problem?**  
A: Good! The agent logs it in PROGRESS.md Issues Log and stops. Fix the design document, update the task file, and re-run.

**Q: How do I know a task is complete?**  
A: Check the Definition of Done in the task file: all rules implemented, all edge cases handled, all tests pass, PROGRESS.md updated.

**Q: What if an agent keeps making the same mistake?**  
A: The task file is probably still vague. Tighten the Implementation Skeleton or Edge Cases to remove inference, then re-run.

---

> **For your specific project structure, build commands, and tech stack configuration, see the root `../README.md`.**
