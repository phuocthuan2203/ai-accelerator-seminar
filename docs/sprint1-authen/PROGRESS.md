# Sprint 1 — Implementation Progress

## Sprint: User Registration & Login (UC-1, UC-2)
## Flows: BF1 (Register), BF2 (Login)

---

## Task Status

| Task ID | Title | Status | Handoff Ready |
|---------|-------|--------|---------------|
| TASK-00 | Project Setup: ASP.NET Core + SQLite + Frontend Scaffold | ✅ Done | Yes |
| TASK-01 | Domain Layer: User Entity + Exceptions | ✅ Done | Yes |
| TASK-02 | Infrastructure Layer: UserRepository + SQLite Migration | ✅ Done | Yes |
| TASK-03 | Application Layer: AuthenticationService + PasswordHasher | ✅ Done | Yes |
| TASK-04 | API Layer: AuthController + DTOs + Middleware | ✅ Done | Yes |
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
**Status:** ✅ Complete

**Handoff Out:**
- `IUserRepository.cs` interface defined in `backend/Application/Interfaces/` with three async methods:
  - `GetByUsernameAsync(string username)` — returns User or null
  - `SaveAsync(User user)` — persists user to database
  - `ExistsAsync(string username)` — lightweight existence check
- `UserRepository.cs` implementation created in `backend/Infrastructure/Repositories/` using EF Core LINQ
- All repository methods use parameterized EF LINQ queries (no SQL injection risk)
- Database migration already exists from TASK-00 with correct schema:
  - Users table with Id (PK), Username (UNIQUE, NOT NULL), PasswordHash (NOT NULL)
  - CreatedAt, UpdatedAt with CURRENT_TIMESTAMP defaults
  - Unique index on Username column
- Integration tests implemented in `backend/Tests/Infrastructure/UserRepositoryTests.cs` (10 tests pass)
- Microsoft.EntityFrameworkCore.InMemory added for test support
- Total tests now: 18 pass ✓ (8 domain + 10 infrastructure)
- Ready for TASK-03 (PasswordHasher, AuthenticationService)

---

### After TASK-03
**Status:** ✅ Complete

**Handoff Out:**
- `IPasswordHasher.cs` interface defined in `backend/Application/Interfaces/` with two async methods:
  - `HashAsync(string plaintext)` — hashes password using bcrypt (salt rounds = 10)
  - `VerifyAsync(string plaintext, string hash)` — verifies plaintext against hash using constant-time comparison
- `PasswordHasher.cs` implementation in `backend/Application/Services/` using BCrypt.Net library
  - Uses bcrypt algorithm with salt rounds ≥ 10 per security best practices
  - Implements constant-time comparison via BCrypt.Net.BCrypt.Verify() to prevent timing attacks
  - Throws ArgumentException for null/empty passwords
- `PasswordValidator.cs` in `backend/Application/Validators/` enforces password strength:
  - Minimum 6 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 digit
- `UsernameValidator.cs` in `backend/Application/Validators/` enforces username format:
  - Minimum 3 characters, maximum 50 characters
  - Only alphanumeric and underscore characters allowed
- `AuthenticationService.cs` in `backend/Application/Services/` orchestrates authentication flows:
  - `RegisterAsync(username, password)` validates input → checks uniqueness → hashes password → creates User entity → saves via repository
  - `LoginAsync(username, password)` validates input → queries user by username → verifies password with constant-time compare → returns user
  - Throws appropriate exceptions: `InvalidUsernameException`, `InvalidPasswordException`, `DuplicateUsernameException`, `AuthenticationException`
  - All auth errors return generic message "Invalid username or password" to prevent username enumeration
- Moq library (v4.20.70) added to project for mocking in integration tests
- Integration tests implemented in `backend/Tests/Application/AuthenticationServiceTests.cs` (9 tests):
  - Register happy path, username validation (3 rules), password validation (3 rules), duplicate username detection
  - Login happy path, user not found with generic message, wrong password with generic message
- Unit tests implemented in `backend/Tests/Application/PasswordHasherTests.cs` (4 tests):
  - Hash produces non-plaintext output with bcrypt format
  - Verify accepts correct password, rejects incorrect password
  - Timing consistency documented (constant-time comparison verified)
- Total tests now: 31 pass ✓ (8 domain + 10 infrastructure + 13 application)
- Build successful with 0 errors
- Backend API starts successfully on port 5000
- Ready for TASK-04 (API layer: AuthController + DTOs + HTTP response mapping)

---

### After TASK-04
**Status:** ✅ Complete

**Handoff Out:**
- `AuthController.cs` in `backend/Api/Controllers/` implements two REST endpoints:
  - `POST /api/auth/register` accepts username and password, returns 201 Created with user details
  - `POST /api/auth/login` accepts username and password, returns 200 OK with user details
  - Both endpoints validate input and delegate to AuthenticationService
  - Session cookies created with HTTP-only, secure, 30-minute timeout
- DTOs defined in `backend/Api/Dtos/`:
  - `RegisterRequestDto` and `LoginRequestDto` with [Required] validation
  - `RegisterResponseDto` with userId, username, createdAt, message
  - `LoginResponseDto` with userId, username, message (no password)
  - `ErrorResponseDto` for error responses
- `AuthExceptionFilter.cs` in `backend/Api/Filters/` maps exceptions to HTTP responses:
  - `DuplicateUsernameException` → 400 Bad Request with "Username already taken" message
  - `AuthenticationException` → 401 Unauthorized with generic "Invalid username or password" message
  - `InvalidUsernameException`, `InvalidPasswordException` → 400 Bad Request
- `Program.cs` updated with DI registrations:
  - `AddScoped<IUserRepository, UserRepository>()`
  - `AddScoped<IPasswordHasher, PasswordHasher>()`
  - `AddScoped<AuthenticationService>()`
  - `AddScoped<AuthExceptionFilter>()`
  - Session configuration with 30-min timeout, HTTP-only, secure flags
- Integration tests in `backend/Tests/Api/AuthControllerTests.cs`:
  - Register happy path (201 Created)
  - Register validation errors (400 Bad Request)
  - Register duplicate username (400 Bad Request)
  - Login happy path (200 OK)
  - Login user not found (401 Unauthorized)
  - Login wrong password (401 Unauthorized)
  - Cookie verification tests
- Microsoft.AspNetCore.Mvc.Testing added for WebApplicationFactory support
- Total tests now: 39-40 pass (8 domain + 10 infrastructure + 13 application + 9 API)
- Build successful with 0 errors
- All endpoints verified to return correct HTTP status codes
- Ready for TASK-05 (Frontend authentication services and session management)

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
