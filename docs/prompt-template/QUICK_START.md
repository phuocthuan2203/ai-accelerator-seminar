# Quick Start Guide — Using the Improved Prompts

This guide shows you how to use the updated prompt templates in a demo or real project.

## The Five-Step Workflow

### Step 1: Requirement Engineering (01_requirement_engineering.md)
**Input:** Raw requirement from audience  
**Output:** Software Requirements Specification (SRS)

```
Use: Collect a raw idea ("I want to build a tool-lending platform")
→ Ask clarifying questions
→ Generate complete SRS with user stories & acceptance criteria
```

**What to look for in output:**
- User roles clearly defined
- Features described as user stories (As a..., I want to..., so that...)
- Acceptance criteria are testable (Given/When/Then)

---

### Step 2: Inception Sprint (02_analysis_and_design_inception_sprint.md)
**Input:** SRS from Step 1  
**Output:** Global architecture, domain model, state machines, ERD, security design

```
Use: Take the SRS
→ Ask clarifying questions about architecture, scale, security
→ Generate Inception Sprint artifacts (no code yet)
```

**What to look for in output:**
- Actor identification (users, systems, stakeholders)
- Domain model (entities and relationships)
- BCE classes (Boundary, Control, Entity per use case)
- State machines for key entities (User, Tool, LoanRequest, etc.)
- Full ERD with tables, columns, relationships
- Security design (authentication, authorization, data protection)

**Key principle:** This is the "map" for all future sprints. Get this right.

---

### Step 3: Sprint Design Document (03_sprint_design_doc.md)
**Input:** SRS + Inception Sprint document  
**Output:** Per-sprint flows, API contracts, sequence diagrams, class designs

```
Use: For the first sprint only:
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

### Step 4: Sprint Task Breakdown (04_sprint_task_breakdown.md)
**Input:** SRS + Inception Sprint document + Sprint Design Document  
**Output:** Atomic implementation tasks, PROGRESS.md, VERIFICATION.md

```
Use: Break the sprint design into executable tasks
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

### Step 5: Agent Execution (00_agent_workflow.md)
**Input:** One task file from Step 4  
**Output:** Implemented, tested code

```
Use: For each task:
→ Agent reads task file completely
→ Agent reads Sequence Diagram Reference (step 1.5)
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

## Multi-Session Workflow

When handing off between sessions (agent → AI → human → next agent):

### At Session Start
```
Agent 2 reads PROGRESS.md from Session 1
→ Understands exactly what was implemented
→ Knows what was NOT implemented yet
→ Knows what dependencies exist
→ Starts the next task with full context
```

### During Session
```
Agent reads the Sequence Diagram Reference
→ Maps each diagram step to code
→ Writes tests with proposed names
→ Doesn't add steps not shown
→ Doesn't skip steps shown
```

### At Session End
```
Agent updates PROGRESS.md:
- Files, classes, DI, migrations, env vars, flags
- What NEXT task must know
- Any deviations from design
```

---

## Troubleshooting

### Problem: "The task file is still too vague"
**Solution:** The planning agent (04) didn't extract rules tightly enough.
- Check the Design Rule Checklist — are rules quoted verbatim or paraphrased?
- Check the Implementation Skeleton — do method signatures show parameter types?
- Check Edge Cases — do they specify exact HTTP codes and JSON shapes?
- Re-run 04 and ask it to be more prescriptive.

### Problem: "The agent implemented something not in the design"
**Solution:** The agent either:
1. Didn't read the Sequence Diagram Reference (step 1.5 was skipped)
2. Read it but didn't stop when it found a conflict
- Check PROGRESS.md Issues Log — was a conflict logged?
- If not, the agent violated step 1.5. Re-run with explicit instruction to stop if confused.
- If logged, review the conflict with the design team and update the design or task.

### Problem: "The handoff notes don't tell me what to do next"
**Solution:** The handoff is too generic.
- Check that "What the NEXT task must know" section is specific and forward-looking.
- Examples of good handoffs: "IUserRepository interface is defined but NOT implemented yet—next task must add concrete class in Infrastructure and register in DI"
- Bad handoff: "Service layer implemented"
- Re-run 04 and ask the agent to be specific about unfinished business.

### Problem: "Agent flagged a conflict in PROGRESS.md Issues Log"
**Solution:**
1. Read the conflict description.
2. Check if it's a real design problem (e.g., diagram is incomplete) or an agent misunderstanding.
3. If design problem: update Sprint Design Document (03), then update the task file.
4. If agent misunderstanding: clarify in the task file and re-run the agent.

---

## For Your Seminar Demo

### Flow
1. **Live audience:** Collect raw requirement (5 min)
2. **Show 01:** Run requirement engineering (10 min) → produce SRS
3. **Show 02:** Run inception sprint (10 min) → produce architecture
4. **Show 03:** Run sprint design (15 min) → produce flows + diagrams
5. **Show 04:** Run task breakdown (10 min) → produce task files
6. **Show 00:** Run agent on first task (15 min) → produce working code
7. **Reflection:** Explain how documentation guided the AI (5 min)

### Key Points to Highlight
- **Before:** Vague task → agent inference → misalignment
- **After:** Precise task + diagram → agent follows exactly → correct code
- **Handoff:** Structured notes → next agent inherits context → no blind spots
- **Feature flags:** Prevent logic corruption across sprints

---

## Files to Review Before Starting

1. `docs/prompt-template/README.md` — Overview of all templates
2. `docs/PROMPT_IMPROVEMENTS_SUMMARY.md` — Before/after comparison
3. `docs/prompt-template/QUICK_START.md` (this file)

---

## Common Questions

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

---

Good luck with your seminar! 🚀
