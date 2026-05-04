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

## Getting Started with the Prompts

### For a new seminar demo:
1. Collect a raw requirement from the audience.
2. Use `01_requirement_engineering.md` to clarify and generate the SRS.
3. Use `02_analysis_and_design_inception_sprint.md` to design the global architecture.
4. Use `03_sprint_design_doc.md` to design the first sprint (include sequence diagrams with alt branches and layer labels).
5. Use `04_sprint_task_breakdown.md` to break the sprint into atomic tasks, each with embedded sequence diagrams.
6. Use `00_agent_workflow.md` as the execution standard for each task.
7. After each task, update PROGRESS.md with the structured handoff format.
8. In the next session, the agent reads PROGRESS.md first for full context.

### For multi-session work:
- Always start each session by reading PROGRESS.md from the previous task.
- Always reference the Sequence Diagram Reference in your current task before writing code.
- Always update the handoff section with exact file paths, class names, and forward-looking dependencies.

## Testing & Verification

Each prompt is designed to enable three levels of verification:

1. **Per-task (in 04 task files):** Edge cases are testable, design rules have proposed test names, "must-contain" checklists are verifiable.
2. **Integration (in 04 VERIFICATION.md):** Agent-executed checks for cross-task consistency, human-executed checks for full workflows.
3. **Post-completion (in 00 Verification Checklist):** Feature flags, sequence diagram adherence, layer dependencies.

---

**Last updated:** 2026-05-01  
**Status:** Stable, tested on real projects
