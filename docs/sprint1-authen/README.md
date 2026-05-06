# Sprint 1 — Task Breakdown & Implementation Guide

## Overview

This directory contains the **complete task breakdown** for **Sprint 1: User Registration & Login (UC-1, UC-2)**.

All tasks are ready for implementation. Each task is **self-contained**, **design-driven**, and includes:
- Design rule checklist (what to build and why)
- Implementation skeleton (code structure)
- Test specifications
- Verification steps
- Definition of done

---

## Files in This Directory

| File | Purpose |
|------|---------|
| `sprint1-design.md` | **Design document** from which all tasks are derived. Read this first to understand the architecture, API contracts, wireframes, and sequence diagrams. |
| `PROGRESS.md` | **Live progress tracker**. Updated after each task completion with handoff notes, issues, and solutions. |
| `VERIFICATION.md` | **Verification plan** for agent (automated) and human (manual) testing. Run these checks after all tasks complete. |
| `tasks/TASK-00-project-setup.md` | **Infrastructure setup**: ASP.NET Core, SQLite, frontend scaffold. *Start here.* |
| `tasks/TASK-01-domain-entities.md` | **Domain layer**: User entity, exceptions. |
| `tasks/TASK-02-repositories.md` | **Persistence layer**: UserRepository, database migration. |
| `tasks/TASK-03-application-services.md` | **Application layer**: AuthenticationService, PasswordHasher. |
| `tasks/TASK-04-api-layer.md` | **API layer**: AuthController, DTOs, DI configuration. |
| `tasks/TASK-05-frontend-services.md` | **Frontend services**: AuthService, SessionManager. |
| `tasks/TASK-06-frontend-ui.md` | **Frontend UI**: Registration & Login pages, form handling. |

---

## Quick Start: Task Execution Order

**Follow this sequence** to implement Sprint 1:

```
TASK-00 (Setup)
    ↓
TASK-01 (Domain) ← Can run parallel with TASK-02 domain layer
    ↓
TASK-02 (Repositories)
    ↓
TASK-03 (Services) ← Backend logic
    ↓
TASK-04 (API) ← Can run parallel with TASK-05/06 if API contract published
    ↓
TASK-05 (Frontend Services) ← Frontend logic
    ↓
TASK-06 (Frontend UI) ← User-facing pages
    ↓
VERIFICATION (Full E2E testing)
```

---

## Key Design Rules to Remember

These are the **critical rules** that must not be violated:

| Rule | Impact | Where to Check |
|------|--------|-----------------|
| **Passwords hashed with bcrypt** (not plaintext) | Security: data breach risk | TASK-03, test `PasswordHasherTests` |
| **Constant-time password verification** (prevents timing attacks) | Security: timing attack risk | TASK-03, verify `BCrypt.Verify()` |
| **Username must be unique** (database constraint + application validation) | Data integrity | TASK-02, test `UserRepositoryTests` |
| **Generic error "Invalid username or password"** (prevents username enumeration) | Security: account enumeration risk | TASK-03, TASK-04 error messages |
| **Session cookies HTTP-only, secure, 30-min timeout** | Session security | TASK-04, `Program.cs` configuration |
| **Frontend input validation real-time** (immediate feedback to user) | UX quality | TASK-06, registration & login pages |
| **All exceptions mapped to correct HTTP codes** (400, 401, etc.) | API contract | TASK-04, `AuthExceptionFilter` |

---

## Layered Architecture Recap

The system follows **layered architecture**:

```
┌─────────────────────────────────────┐
│ TASK-06: Frontend UI (Pages & Forms)│ ← User interaction
├─────────────────────────────────────┤
│ TASK-05: Frontend Services (JS API) │ ← Client-side logic
├─────────────────────────────────────┤
│ TASK-04: API Layer (Controllers)    │ ← HTTP boundary
├─────────────────────────────────────┤
│ TASK-03: Application Services       │ ← Use-case orchestration
├─────────────────────────────────────┤
│ TASK-01: Domain Layer (Entities)    │ ← Core business logic
├─────────────────────────────────────┤
│ TASK-02: Infrastructure (Repos)     │ ← Data persistence
├─────────────────────────────────────┤
│ TASK-00: Configuration & Setup      │ ← Bootstrapping
└─────────────────────────────────────┘
```

**Data flow direction:**
- **Down (implementation):** Frontend calls API calls Services calls Domain/Repos
- **Up (dependencies):** Domain/Repos inject into Services inject into API inject into Frontend

---

## Task Dependencies & Handoffs

### TASK-00 → TASK-01
- **What TASK-00 provides:**
  - Project builds: `dotnet build` works
  - Database configured: connection string, migrations runnable
  - DI container initialized
- **What TASK-01 needs:**
  - Project structure in place
  - Can focus on domain entities without infrastructure complexity

---

### TASK-01 → TASK-02
- **What TASK-01 provides:**
  - `User` entity defined and immutable
  - Exceptions (`DuplicateUsernameException`, etc.) defined
- **What TASK-02 needs:**
  - User schema clear for migration
  - Repository can persist User entities

---

### TASK-02 → TASK-03
- **What TASK-02 provides:**
  - `IUserRepository` interface defined
  - `UserRepository` implementation (SaveAsync, GetByUsernameAsync, etc.)
  - Database migrations applied
  - Users table exists in SQLite
- **What TASK-03 needs:**
  - Repository to query/persist users
  - Database is "ready"

---

### TASK-03 → TASK-04
- **What TASK-03 provides:**
  - `AuthenticationService` with register() and login() methods
  - `IPasswordHasher` interface and implementation
  - Exception throwing behavior documented
- **What TASK-04 needs:**
  - Services to call from controllers
  - Exception types known (for error mapping)

---

### TASK-04 → TASK-05 (optional parallel)
- **What TASK-04 provides:**
  - API endpoints: POST /api/auth/register, POST /api/auth/login
  - Request/response DTOs and schemas
  - HTTP codes (201, 200, 400, 401) documented
- **What TASK-05 needs:**
  - API contract (URL, method, payload shape)
  - Can start writing AuthService to match API contract
  - Tests can mock API

---

### TASK-05 → TASK-06
- **What TASK-05 provides:**
  - `AuthService` with register(), login(), isAuthenticated(), etc.
  - SessionManager for localStorage
  - Promise-based API for form to use
- **What TASK-06 needs:**
  - Service methods to call on form submit
  - Session management for persistence
  - Can build UI that calls services

---

## How to Use Each Task File

Each task file has this structure:

1. **Context** — 2-3 sentences on what this task does and why it's ordered here
2. **Design References** — Links to sections in sprint-design.md (read these first)
3. **Sequence Diagram Reference** — The exact flow(s) this task implements (follow it exactly)
4. **Design Rule Checklist** — Every rule this task must enforce (with sources and tests)
5. **Affected Files** — Exact files to create/modify
6. **Implementation Skeleton** — Code structure (not full impl, but detailed outline)
7. **Edge Cases Handled** — What happens when things go wrong
8. **Verification Steps** — How to test this task in isolation and as part of the whole
9. **Definition of Done** — Checklist to mark task complete

---

## Common Implementation Patterns

### Backend

**Dependency Injection Flow:**
```csharp
// Program.cs registers services
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<AuthenticationService>();

// Controller injects via constructor
public AuthController(AuthenticationService authService) { ... }

// Service injects via constructor
public AuthenticationService(IUserRepository repo) { ... }
```

**Exception Flow:**
```csharp
// Domain throws exception
throw new DuplicateUsernameException(username);

// Service propagates
// (service validates, calls repo, catches domain exceptions, re-throws if needed)

// Controller catches and maps to HTTP
// AuthExceptionFilter intercepts and returns BadRequest(400)
```

**Database Access:**
```csharp
// Repository uses EF Core LINQ (parameterized, no SQL injection)
await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

// Never: string concat SQL
// ❌ "SELECT * FROM Users WHERE username = '" + username + "'"
```

### Frontend

**Service Pattern:**
```javascript
// Service wraps HTTP calls
async register(username, password) {
  const response = await fetch('/api/auth/register', {...});
  return response.json();
}

// Component/Page uses service
const result = await authService.register(username, password);
```

**Form Validation:**
```javascript
// Real-time feedback as user types
input.addEventListener('input', () => {
  const errors = validate(input.value);
  errorDiv.textContent = errors.length > 0 ? errors[0] : '';
  submitBtn.disabled = errors.length > 0;
});
```

---

## Testing Strategy

### Backend Tests
- **Unit tests** on services/entities (no database)
- **Integration tests** on repositories (test database)
- **API tests** on controllers (test HTTP contract)

Example:
```bash
# Run all tests
dotnet test

# Run one test file
dotnet test tests/Application/PasswordHasherTests.cs

# Run one test
dotnet test -k "TestMethodName"
```

### Frontend Tests
- **Unit tests** on services (mock fetch)
- **Manual tests** on UI (browser)

Example:
```bash
# Run JS tests (if using Jest/Jasmine)
npm test

# Manual test in browser
http://localhost:5000/pages/register.html
```

---

## Troubleshooting Common Issues

### "Unknown command or flag" in dotnet
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Ensure you're in `backend/` directory before running dotnet commands

### "Database is locked" or migration fails
- Close any other `dotnet run` instances
- Delete `backend/tool_lending.db` and re-run migration: `dotnet ef database update`

### Frontend can't reach API (CORS errors)
- Ensure backend is running: `dotnet run` in backend/
- Ensure CORS is configured in `Program.cs` (TASK-04 handles this)
- Check browser console (F12) for exact CORS error

### Session cookie not persisting
- Ensure session middleware is registered: `app.UseSession()` in Program.cs
- Ensure cookies are enabled in browser
- For HTTPS/production, ensure `SecurePolicy = Always` is set (not for localhost)

### Password hash doesn't match expected format
- Bcrypt hashes should start with `$2a$` or `$2b$`
- Verify salt rounds ≥ 10: slower but more secure
- Test: `echo "SecurePass123" | htpasswd -BC 10 /dev/stdout` (on Linux)

---

## Sprint Completion Criteria

Sprint 1 is complete and ready to ship when:

✅ All 7 tasks (TASK-00 through TASK-06) are marked **Done** in PROGRESS.md

✅ All **agent-executed verification** checks pass (see VERIFICATION.md Part 1)
- Backend builds without errors
- All tests pass (35+)
- Database schema correct
- No security issues

✅ All **human-executed verification** checks pass (see VERIFICATION.md Part 2)
- Registration works end-to-end
- Login works end-to-end
- Validation messages appear real-time
- Session persists across reloads
- Generic error messages (no username enumeration)
- No plaintext passwords in responses

✅ No open items in PROGRESS.md **Issues Log**

✅ All design rules from Design Rule Checklists implemented and verified

---

## Next Steps (After Sprint 1)

Once Sprint 1 ships:

1. **Celebrate! 🎉** Authentication is complete.
2. **Sprint 2:** Tool upload and management (UC-3, UC-4)
3. **Sprint 3:** Tool browsing and borrowing requests (UC-5, UC-6)
4. **Sprint 4+:** Additional features (ratings, notifications, etc.)

For Sprint 2, use the same process:
- Sprint Design Document (from template-03)
- Task breakdown (from template-04)
- Implement tasks in dependency order

---

## Questions or Issues?

If you hit a **blocker**:
1. Check this README's **Troubleshooting** section
2. Check the specific task file's **Edge Cases Handled** section
3. Review PROGRESS.md **Issues Log** to see if someone else solved it
4. Log the issue in PROGRESS.md Issues Log with exact error message
5. Request clarification from the team or product owner

---

## Files Reference Quick Link

- **Architecture Overview:** `docs/tool-lending-inception.md`
- **User Stories & Scope:** `docs/tool-lending-srs.md`
- **This Sprint's Design:** `sprint1-design.md`
- **This Sprint's Tasks:** `tasks/TASK-*.md`
- **Verification:** `VERIFICATION.md`
- **Progress Tracker:** `PROGRESS.md`

---

Good luck with Sprint 1! 🚀
