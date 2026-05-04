# Prompt Template Audit Report

**Auditor:** Claude Code  
**Date:** 2026-05-04  
**Branch:** feature/audit-prompt-templates  
**Scope:** 5 SDLC workflow templates + README  
**Target:** Tool-lending app demo (45 min), ASP.NET Core + Angular + PostgreSQL  
**Audience:** 1-3rd year students  

---

## Executive Summary

**Overall Assessment:** ✅ **Sound architecture, but 3 critical issues + 7 minor issues need fixing before live demo**

The templates form a logically coherent SDLC workflow (Req → Design → Sprint Design → Task Breakdown → Implementation). However:

1. **Phase 2 (Inception) is over-scoped for a 45-min demo** — asks 8–15 questions + generates 6+ artifacts. Recommend time-boxing.
2. **Handoff clarity gaps** — PROGRESS.md format is detailed but examples lack concrete detail about *what* to pass forward.
3. **Sequence diagram requirement** is mentioned in README but enforcement is weak in task templates (should be mandatory, not advisory).

Below: detailed findings organized by category.

---

## Critical Issues (Must Fix Before Demo)

### Issue 1: Phase 2 (Inception Sprint) Time Budget vs Demo Window

**Problem:**  
The Inception prompt (02_analysis_and_design_inception_sprint.md) asks **8–15 clarifying questions** and produces **6 major artifacts** (actors, domain model, BCE, state machines, architecture, ERD, security). This easily takes **4–6 hours** in real projects.

For a **45-minute demo**, this phase becomes a bottleneck:
- If you skip it, task breakdown (Phase 4) lacks foundational design.
- If you run it, you only have ~10 minutes left for later phases.

**Recommendation:**
Create a **"Demo Mode" variant** of Phase 2 that:
- Reduces clarifying questions to **3–5 essentials** (skip non-critical design decisions).
- Pre-answers assumptions for the tool-lending app (e.g., "layered + event-driven", "single-tenant", "PostgreSQL ERD").
- Produces a **condensed Inception Report** (~2–3 pages instead of 10+) with only critical artifacts (domain model, ERD excerpt, security model).

**Impact:** Cuts Inception from 4–6 hours to **30–45 minutes**, making a 45-min demo feasible.

**Fix Type:** Create a new document or section in README.

---

### Issue 2: Sequence Diagram Enforcement is Weak in Task Templates

**Problem:**  
The README emphasizes: *"Sequence diagrams are embedded in every task file. Agents must follow them exactly or raise issues immediately."* (README §5, Issue 1 fix).

However, in `00_agent_workflow.md` (Step 1.5), the enforcement is **advisory, not mandatory**:

> "If any step is ambiguous, missing layer labels, or conflicts with other task sections, **STOP immediately** and log it..."

And in `04_sprint_task_breakdown.md`, the **Sequence Diagram Reference** section is described but **not enforced in the Format section** — it's just one of many task file sections.

**Result:** Agents can treat the diagram as "nice to have" rather than "contract law." This defeats the guardrail purpose.

**Recommendation:**
1. In `00_agent_workflow.md` Step 1.5: Change "If ambiguous, STOP" to **"STOP if diagram is missing or incomplete"** before reading rest of task.
2. In `04_sprint_task_breakdown.md` Task File Format: Move **Sequence Diagram Reference section to the TOP**, before Design Rule Checklist, and add a critical note:
   > **CRITICAL RULE:** This diagram is your contract. You must implement exactly what is shown—no more, no fewer steps. If you find a conflict with the Design Rule Checklist, log it in PROGRESS.md Issues Log and STOP.

**Fix Type:** Reorder and strengthen language in existing templates.

---

### Issue 3: PROGRESS.md Handoff Examples Lack Concrete Specificity

**Problem:**  
In `04_sprint_task_breakdown.md` (PROGRESS.md Format), the **"What now exists"** handoff section uses examples like:

> Files: `backend/application/Services/UserService.cs` (CREATE)  
> Classes: `UserService` (Application)  
> DI: `Program.cs: services.AddScoped<IUserService, UserService>()`

While this is good, it **doesn't specify what the NEXT task must know** concretely. The "What NEXT task must know" section says:

> `IPasswordHasher` interface is defined in `backend/application/Interfaces/` but NOT implemented yet — next task must add `BcryptPasswordHasher`...

**But it doesn't say:**
- What method signature does `IPasswordHasher` have? (needed so next task doesn't guess)
- What constructor signature does `UserService` expect? (needed so next task injects correctly)
- Does `UserService` already call `IPasswordHasher.Hash()`? (needed so next task knows what's already wired)

**Result:** The next session's agent still has to reverse-engineer the code or ask clarifying questions, negating the handoff benefit.

**Recommendation:**
Expand the "What NEXT task must know" example to include:
- **Undefined contracts** (interface signatures waiting for implementation)
- **Partial integrations** (which files call which, and what's still missing)
- **Schema assumptions** (e.g., "users table has `status` column; DO NOT add another status column in next task")
- **Feature flag state** (what's enabled/disabled and why)

**Fix Type:** Update PROGRESS.md example section with richer detail.

---

## Minor Issues (Should Fix for Clarity)

### Issue 4: Inception Doc "Do NOT re-ask" Section is Incomplete

**File:** `02_analysis_and_design_inception_sprint.md`, **Phase 1 handoff section** (lines 38–43)

**Problem:**  
The prompt says "Do NOT re-ask about topics the SRS already covers" and lists 5 examples (tech stack, performance, auth method, scope, user roles). But it doesn't list all obvious topics, so agents might still ask redundant questions about:
- Data retention policies
- Soft-delete vs hard-delete strategy
- Time zone handling
- Scaling assumptions from SRS Section 5

**Recommendation:**  
Expand the "Do NOT re-ask" list to 10–12 items to cover common redundancy traps.

**Fix Type:** Update Phase 1 handoff section.

---

### Issue 5: README "What Changed" Section is Outdated

**File:** `README.md`, **Section "What Changed"** (lines 34–56)

**Problem:**  
This section describes improvements in "this version" but doesn't specify version numbers or dates. It's unclear if these were already implemented or are aspirational.

Also, the three issues described (Design Misalignment, Task Files Too Broad, Poor Handoff Quality) read like they're **already fixed**, but when you read the actual templates, some fixes are incomplete (see Issue 3 above).

**Recommendation:**
1. Add version header: `## What Changed (v2.1 → v2.2, 2026-05-04)`
2. Mark each issue as `[IMPLEMENTED]`, `[PARTIAL]`, or `[PLANNED]`
3. If PARTIAL, note what's still needed.

**Fix Type:** Clarify README versioning.

---

### Issue 6: Task File Format Doesn't Specify "Test Inclusion Granularity"

**File:** `04_sprint_task_breakdown.md`, **Test Inclusion Rule** (line 180)

**Problem:**  
The prompt says: "Every task that touches non-trivial logic must include its **tests in the same task**."

But it doesn't clarify:
- Does "non-trivial" include a simple DTO validation method? (Probably not, but agents might interpret differently.)
- If a task touches 5 files, should all 5 have tests, or just the "main" file? (E.g., if a task creates UserEntity + UserRepository + UserService, do we test just UserService or all three?)
- Should frontend components always have tests, or only "complex" components? (Agents will debate what "complex" means.)

**Recommendation:**  
Add a **test granularity table** in the "Test Inclusion Rule" section:

| Layer | Test Required? | Granularity | Example |
|-------|---|---|---|
| Domain entities | Only if non-trivial logic | Per aggregate root | `User` entity with validation logic → test. Simple enum → skip. |
| Application services | Always | One test file per service class | `UserService` → `UserServiceTests` |
| Infrastructure repositories | Integration test | One per repository | `UserRepository` + in-memory DB test |
| API controllers | Integration test | Per endpoint/controller | `AuthController` → `AuthControllerTests` |
| Frontend services | Always | Per service | `AuthService` → `auth.service.spec.ts` |
| Frontend components | Always for interactive, consider for display-only | Per component | Form component → test. Static card → optional. |

**Fix Type:** Add clarity table to `04_sprint_task_breakdown.md`.

---

### Issue 7: Sequence Diagram Example in Sprint Design Doc Lacks Alt/Error Completeness Guidance

**File:** `03_sprint_design_doc.md`, **Section 4 — Detailed Sequence Diagrams** (lines 201–235)

**Problem:**  
The prompt says diagrams must show "all `alt` / `else` / `error` branches" but doesn't give an example of what "completeness" looks like. An agent might create a diagram with happy path + 1 error branch and think it's complete, missing other important branches.

**Recommendation:**  
Add an **example checklist** of common branches to include:

```
For each endpoint / use case in the diagram, include:
- ✅ Happy path (success)
- ✅ Input validation failure (400 Bad Request)
- ✅ Authentication failure (401 Unauthorized)
- ✅ Authorization failure (403 Forbidden)
- ✅ Resource not found (404 Not Found)
- ✅ Business rule violation (409 Conflict, e.g., cooldown, duplicate)
- ✅ External service timeout (503 Service Unavailable)
- ✅ State machine guards (cannot transition from State X to State Y)
- ⚠️  Rate limiting (429 Too Many Requests) — if applicable
```

**Fix Type:** Add guidance to Section 4 of `03_sprint_design_doc.md`.

---

### Issue 8: Task Breakdown "Ordering Rule" Doesn't Account for Parallel Dependencies

**File:** `04_sprint_task_breakdown.md`, **Ordering Rule** (lines 153–176)

**Problem:**  
The "Ordering Rule" says: "Order tasks from **least dependent to most dependent**" and provides a linear sequence (Domain → Infrastructure → Application → API → Frontend Service → Frontend Presentation → Frontend Routing).

However, some tasks **can run in parallel**:
- Domain + Database migrations can run in parallel (with a note that migrations don't block domain tests).
- Frontend service layer + backend API layer can run in parallel (if API contracts are clear).

The current guidance implies strict sequencing, which might cause unnecessary blocking in a multi-agent scenario.

**Recommendation:**  
Add a **parallel execution note**:

> **Parallel execution:** Some tasks can run concurrently if dependencies are clear:
> - Domain entities + migrations can be split (domain tests don't need DB; migrations can start once schema is defined).
> - Backend API contracts can be finalized in advance so frontend service layer starts in parallel.
> - Document these split points in PROGRESS.md under "What NEXT task must know."

**Fix Type:** Add parallel execution guidance to Ordering Rule.

---

### Issue 9: README "Adapting These Templates" Section Needs Tech-Stack Mapping Table

**File:** `README.md`, **Section "Adapting These Templates to Your Project"** (lines 96–119)

**Problem:**  
The section says: "If you use a different tech stack, update the examples in the templates" and provides a table with columns like "Backend runtime & framework", "Frontend framework", etc.

However, it doesn't give **concrete examples** for common stacks. A student using Python + React would need to search the templates for ".NET" and "Angular" and manually translate. This is error-prone.

**Recommendation:**  
Add a **tech-stack translation quick-reference** in README as a subsection:

```markdown
### Tech Stack Translation Examples

#### Stack: Node.js + React + MongoDB

| Component | Update from | Update to | Example |
|-----------|---|---|---|
| Backend async methods | `.NET Task<T>` | `async function` / `Promise<T>` | `public async Task<UserDto> CreateUserAsync(...)` → `async function createUser(...) { return {...} }` |
| Frontend framework | `Angular standalone component` | `React functional component` | `export class UserListComponent { ... }` → `export function UserList() { ... }` |
| Database | `EF Core migrations` | `Mongoose schema` | `add-migration AddUsers` → `mongoose.model('User', userSchema)` |
| Testing | `xUnit` | `Jest` | `dotnet test` → `npm test -- --coverage` |

```

**Fix Type:** Add quick-reference table to README.

---

## Redundancy Analysis

### Where Templates Overlap (Expected, But Worth Noting)

| Overlap | Files | Why OK | How to Handle in Demo |
|---------|-------|-------|---|
| **Architecture description** | Inception (§6) + Sprint Design (§2) | Inception is global; Sprint Design refines for specific flows | Show that Sprint Design builds on, doesn't repeat, Inception |
| **State machines** | Inception (§5) + Sprint Design (implied) | Inception defines entity lifecycles; Sprint Design shows state transitions per flow | Use Inception as "reference" in Sprint Design; only show affected states |
| **API contracts** | Sprint Design (§3) + Task Breakdown (implied) | Sprint Design is "what"; Task Breakdown is "how to implement" | In demo, emphasize Sprint Design is design decision; Task Breakdown is specification |
| **Sequence diagrams** | Sprint Design (§4) + Task file (embedded) | Sprint Design is end-to-end flow; Task file is layer-specific detail | Task files cite Sprint Design diagram; agents verify alignment |

**Conclusion:** Overlap is **intentional and healthy**—not redundancy, but "zoom in" at each phase.

---

## Connection Strength Assessment

### Handoff Quality: Req → Design → Sprint Design → Tasks → Implementation

| Handoff | Strength | Issue | Severity |
|---------|----------|-------|----------|
| SRS → Inception | ✅ Strong | Inception explicitly says "read SRS end-to-end first" and lists what NOT to re-ask | Low |
| Inception → Sprint Design | ✅ Strong | Sprint Design says "treat Inception as authoritative" and refines within that | Low |
| Sprint Design → Task Breakdown | ⚠️ Moderate | Task Breakdown extracts "Rule Ledger" but example is vague | **Medium** |
| Task Breakdown → Task Implementation | ⚠️ Moderate | PROGRESS.md handoff is detailed but lacks concrete code/config specifics | **Medium** |
| Within Tasks (Diagram Alignment) | ⚠️ Weak | Sequence Diagram Reference is "advisory" not enforced; conflicts logged but not enforced as blockers | **High** |

---

## Recommendations Summary

### For 45-Minute Demo

| Priority | Issue | Action | Time Saving |
|----------|-------|--------|---|
| **P0** | Inception over-scoped | Create "Demo Mode" variant OR pre-answer assumptions | 3+ hours |
| **P0** | Sequence diagram enforcement weak | Strengthen language + enforce as blocker | Clarity only |
| **P0** | PROGRESS.md handoff vague | Expand example with concrete detail | Clarity only |
| **P1** | Task test granularity unclear | Add test matrix | Clarity only |
| **P1** | Sequence diagram completeness | Add alt/error checklist | Clarity only |
| **P1** | README versioning unclear | Clarify what's implemented vs planned | Clarity only |
| **P2** | Inception "Do NOT re-ask" incomplete | Expand to 10+ items | Clarity only |
| **P2** | Parallel execution not mentioned | Add note to Ordering Rule | Helpful for multi-agent |
| **P2** | Tech-stack translation missing | Add quick-reference table | Helpful for students |

---

## Detailed Fix Plan (For User Approval)

### Phase A: Critical Fixes (Must do before demo)

**1. Create `docs/prompt-template/02_DEMO-MODE-inception-sprint.md`**
- Condensed version of Inception prompt for 30–45 min execution
- Reduces questions from 8–15 to 3–5 essentials
- Pre-answers or skips non-critical design decisions
- Produces 2–3 page Inception Report instead of 10+

**2. Strengthen Sequence Diagram Enforcement**
- In `00_agent_workflow.md` Step 1.5: Reword to make diagram reading **mandatory blocker**
- In `04_sprint_task_breakdown.md`: Move Sequence Diagram Reference to **top of task file format**, before Design Rule Checklist

**3. Expand PROGRESS.md Handoff Example**
- Add concrete detail: interface signatures, partial integrations, schema assumptions, feature flag state
- Show what "next task must know" really means with working example

### Phase B: Clarity Fixes (Should do before or after demo)

**4. Add Test Granularity Table** to `04_sprint_task_breakdown.md`
**5. Add Alt/Error Branch Checklist** to `03_sprint_design_doc.md` Section 4
**6. Clarify README Versioning** + mark what's IMPLEMENTED vs PARTIAL vs PLANNED
**7. Expand "Do NOT re-ask" List** in `02_analysis_and_design_inception_sprint.md`
**8. Add Parallel Execution Note** to `04_sprint_task_breakdown.md` Ordering Rule
**9. Add Tech-Stack Translation Table** to `README.md`

---

## Risk Assessment for 45-Minute Live Demo

| Phase | Duration | Risk | Mitigation |
|-------|----------|------|---|
| **Phase 1: Requirement Engineering** | 5 min | User has to answer 6–12 questions quickly | Pre-load example raw idea; have answers ready |
| **Phase 2: Inception Sprint** | 15 min (DEMO MODE) | Condensed design might miss edge cases | Use pre-simplified tool-lending app; skip non-critical design decisions |
| **Phase 3: Sprint Design** | 10 min | Sequence diagrams take time to create | Pre-draft outline; have students follow along, not create from scratch |
| **Phase 4: Task Breakdown** | 10 min | Rule Ledger extraction is tedious | Show how to extract rules from design (walkthrough, not live-coding) |
| **Phase 5: Task Implementation** | 5 min | Can't fully implement in 5 min | Show pre-written skeleton or partial implementation; live update 1–2 methods |

**Verdict:** 45 min is tight but feasible with **Phase 2 demo-mode variant**. Without it, you'll run out of time after Phase 3.

---

## Next Steps

1. **Review this audit** and approve the fix plan.
2. **Prioritize which fixes to implement** (I recommend P0 items before demo, P1–P2 after).
3. **I'll implement approved fixes** and create a demo-ready version.
4. **End-to-end test:** I'll execute the 5 phases with a tool-lending app example to verify the workflow works in practice.
