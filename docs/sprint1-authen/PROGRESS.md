# Sprint 1 — Implementation Progress

## Sprint: User Registration & Login (UC-1, UC-2)
## Flows: BF1 (Register), BF2 (Login)

---

## Task Status

| Task ID | Title | Status | Handoff Ready |
|---------|-------|--------|---------------|
| TASK-00 | Project Setup: ASP.NET Core + SQLite + Frontend Scaffold | ✅ Done | Yes |
| TASK-01 | Domain Layer: User Entity + Exceptions | ✅ Done | Yes |
| TASK-02 | Infrastructure Layer: UserRepository + SQLite Migration | ⬜ Not Started | No |
| TASK-03 | Application Layer: AuthenticationService + PasswordHasher | ⬜ Not Started | No |
| TASK-04 | API Layer: AuthController + DTOs + Middleware | ⬜ Not Started | No |
| TASK-05 | Frontend Services: AuthService + SessionManagement | ⬜ Not Started | No |
| TASK-06 | Frontend UI: Registration & Login Pages + Forms | ⬜ Not Started | No |

**Status legend:** ⬜ Not Started · 🔄 In Progress · ✅ Done · 🚫 Blocked

---

## Rule Coverage Watchlist

Critical business rules that must not drift:

| # | Exact Rule | Owner Task(s) | Notes |
|---|-----------|---------------|-------|
| 1 | Passwords stored using hashing (bcrypt) | TASK-03, TASK-01 | Non-negotiable for security |
| 2 | Username must be unique; reject duplicates with 400 | TASK-02, TASK-03, TASK-04 | Prevent account collision |
| 3 | Password minimum 6 characters, with complexity rules | TASK-03, TASK-05, TASK-06 | Frontend + backend validation |
| 4 | Session-based auth with HTTP-only cookies | TASK-04, TASK-00 | Session middleware in ASP.NET Core |
| 5 | Generic error "Invalid username or password" on login failure | TASK-03, TASK-04 | Prevent username enumeration |
| 6 | Successful registration auto-logs in user (201 Created) | TASK-03, TASK-04 | Design § 3 API Contract |
| 7 | Frontend input validation before submit | TASK-06 | Immediate user feedback |

---

## Handoff Status

(Updated after each task completion)

### After TASK-00
**Status:** ✅ Complete

**Handoff Out:**
- Backend project structure established: `backend/Domain/`, `backend/Application/Interfaces/`, `backend/Infrastructure/Data/`, `backend/Api/Controllers/`, `backend/Api/Middleware/`
- `ToolLendingDbContext` registered in DI with SQLite connection
- EF Core initial migration created (`Migrations/20260506133547_InitialCreate.cs`)
- Database file created at `backend/tool_lending.db`
- `HealthController` endpoint available at `/api/health` (returns 200 OK)
- Session middleware configured with HTTP-only, secure cookies (30-min timeout)
- CORS enabled for localhost development
- Frontend static files served from `wwwroot/` directory
- Ready for TASK-01 (domain entities and business logic)

---

### After TASK-01
**Status:** ✅ Complete

**Handoff Out:**
- `User.cs` domain entity created in `backend/Domain/User.cs` with immutable properties (private setters)
- User constructor validates username and password hash, throwing appropriate exceptions
- Four custom exceptions defined in `backend/Domain/Exceptions/`:
  - `InvalidUsernameException` — thrown when username is null/empty
  - `InvalidPasswordException` — thrown when password hash is null/empty
  - `DuplicateUsernameException` — thrown when username already exists (used by TASK-02/TASK-03)
  - `AuthenticationException` — thrown on login failure with generic message (used by TASK-03)
- Unit tests implemented in `backend/Tests/Domain/UserTests.cs` (8 tests pass)
- xUnit test framework added to project
- Ready for TASK-02 (infrastructure layer: UserRepository + migrations)

---

### After TASK-02
**Status:** ⬜ Not Started

---

### After TASK-03
**Status:** ⬜ Not Started

---

### After TASK-04
**Status:** ⬜ Not Started

---

### After TASK-05
**Status:** ⬜ Not Started

---

### After TASK-06
**Status:** ⬜ Not Started

---

## Issues Log

| # | Task ID | Issue Description | Tried | Status |
|---|---------|-------------------|-------|--------|
| (none yet) | — | — | — | — |

---

## Solutions Log

| # | Issue # | Solution Applied | Verified By |
|---|---------|------------------|-------------|
| (none yet) | — | — | — |

---
