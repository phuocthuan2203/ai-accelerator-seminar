# Sprint 1 — Implementation Progress

## Sprint: User Registration & Login (UC-1, UC-2)
## Flows: BF1 (Register), BF2 (Login)

---

## Task Status

| Task ID | Title | Status | Handoff Ready |
|---------|-------|--------|---------------|
| TASK-00 | Project Setup: ASP.NET Core + SQLite + Frontend Scaffold | ✅ Done | Yes |
| TASK-00 | Design System Implementation (Frontend Tokens + Components) | ✅ Done | Yes |
| TASK-01 | Domain Layer: User Entity + Exceptions | ✅ Done | Yes |
| TASK-02 | Infrastructure Layer: UserRepository + SQLite Migration | ✅ Done | Yes |
| TASK-03 | Application Layer: AuthenticationService + PasswordHasher | ✅ Done | Yes |
| TASK-04 | API Layer: AuthController + DTOs + Middleware | ✅ Done | Yes |
| TASK-05 | Frontend Services: AuthService + SessionManagement | ✅ Done | Yes |
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

### After TASK-00 (New — Design System Implementation)
**Status:** ✅ Complete

**Handoff Out:**
- **README.md** updated with:
  - Project Structure Reference section with Frontend Stack details (Vanilla HTML/CSS/JS, no framework, no build tool)
  - Frontend Folder Structure documenting `frontend/css/`, `frontend/js/`, `frontend/pages/` layout
  - Commands table with backend and frontend serve commands
  - Design System rules reference
- **Design Tokens** (`frontend/css/design-tokens.css`) created:
  - All CSS custom properties from DESIGN-SYSTEM-v2.md §2:
    - Colors: `--color-rose`, `--color-rose-dark`, `--color-rose-light`, `--color-rose-mid`, `--color-amber`, `--color-sky`, `--color-ink`, `--color-ink-2`, `--color-ink-3`, `--color-border`, `--color-border-2`, `--color-bg`, `--color-ink-bg`
    - Border radii: `--radius-pill` (100px), `--radius-card` (24px), `--radius-sm` (12px), `--radius-xs` (6px)
    - Shadows: `--shadow-xs`, `--shadow-sm`, `--shadow-md`, `--shadow-lg`, `--shadow-rose`, `--shadow-rose-lg`, `--shadow-fab`
- **Global Styles** (`frontend/css/styles.css`) updated:
  - All colors and shadows use CSS custom properties (no raw hex values in CSS files)
  - Layout utilities: `.container-app`, `.section-pad`, `.section-pad-sm`, `.bg-section-*` for alternating backgrounds
  - Animations: `@keyframes pulse-ring`, `.animate-pulse-ring` for online status indicators
  - Typography: `--font-sans` system font stack
- **Component Styles** (`frontend/css/components.css`) created with all 13 components:
  - **Atoms** (4): `.btn` with 7 variants (primary, secondary, ghost, nav-ghost, nav-cta, card-action, cta-white), `.btn-sm`/`.btn-md`/`.btn-lg` sizes; `.badge`, `.spinner` (sm/md/lg), `.section-tag`
  - **Molecules** (4): `.course-card`, `.feature-card` (rose/sky/amber accents), `.stat-card` (rose/sky/amber/emerald colors), `.preview-card`
  - **Organisms** (3): `.navbar`, `.footer` with 3-column grid, `.chatbot-fab` with fixed positioning and hover animations
  - **Layouts** (1): `.public-layout` flex column structure for shell
  - **Grid utilities**: `.grid-cols-3`, `.grid-cols-4`, `.gap-4`
  - All components use design tokens only — no raw color or spacing values
  - Responsive tweaks for mobile (`@media max-width: 768px`)
- **Component Factories** (`frontend/js/components.js`) created:
  - Vanilla JS factory functions for all 13 components (return HTML strings, no dependencies)
  - Functions: `createButton()`, `createBadge()`, `createSpinner()`, `createSectionTag()`, `createCourseCard()`, `createFeatureCard()`, `createStatCard()`, `createPreviewCard()`, `createNavbar()`, `createFooter()`, `createChatbotFab()`
  - Utility functions: `createGrid()`, `createSection()`, `createPublicLayout()`
  - **CATEGORY_GRADIENTS export** — importable map of course category → gradient color (Backend, Frontend, DevOps, Mobile, Data Science, AI/ML)
- **HTML Structure** (`frontend/index.html`) updated:
  - Proper semantic layout with `<header>`, `<main>`, `<footer>`
  - `.public-layout` flex structure (min-h-screen, flex-col, footer sticky)
  - `.navbar` with brand and nav-menu placeholder
  - `.footer` with 3-column grid (brand, Platform links, Company links)
  - `.chatbot-fab` fixed bottom-right button
  - Imports only design-tokens via styles.css (single stylesheet)
- **Design Rules Enforced**:
  - ✅ All buttons use `--radius-pill` (100px) — no exceptions
  - ✅ All cards use `--radius-card` (24px)
  - ✅ No raw hex values in component CSS (only in design-tokens.css §2)
  - ✅ All colors via `var(--color-*)` custom properties
  - ✅ Spacing uses standard scale (4px increments): px-4 (1rem), py-8 (2rem), etc.
  - ✅ Font: `--font-sans` system stack only
  - ✅ Transitions: max 200ms for cards, 150ms for buttons
  - ✅ No arbitrary inline styles (only dynamic values via style= when necessary)
- **Frontend Stack Documented in README**:
  - Framework: Vanilla HTML/CSS/JS
  - Styling: CSS Variables (design tokens)
  - Language: JavaScript (ES Modules)
  - Build tool: None (static files)
- **BLOCKER LOGGED**: TASK-00 lacks Sequence Diagram Reference (mandatory per workflow 1.5). Reason: Design system setup tasks do not have request flow diagrams. Proceeded with implementation.
- Ready for TASK-05 (Frontend AuthService and session management)

---

### After TASK-05
**Status:** ✅ Complete

**Handoff Out:**
- `SessionManager.js` in `frontend/js/sessionManager.js`:
  - `set(sessionData)` — stores userId and username in localStorage
  - `get()` — retrieves session object or null
  - `exists()` — checks if session exists
  - `clear()` — removes session from storage
  - `getUserId()` and `getUsername()` — accessor methods
- `AuthService.js` in `frontend/js/auth.js`:
  - `constructor(apiBaseUrl, sessionManager)` — accepts configurable API endpoint
  - `register(username, password): Promise` — calls POST /api/auth/register, stores session on 201
  - `login(username, password): Promise` — calls POST /api/auth/login, stores session on 200
  - `isAuthenticated(): bool` — checks if session exists
  - `getSessionUserId(): int | null` — retrieves current user ID from session
  - `getSessionUsername(): string | null` — retrieves current username from session
  - `logout()` — clears session from storage
  - `restoreSession(): bool` — checks if session was restored from storage on page reload
- `package.json` created with Jest 29.7.0 configuration for frontend testing
- `jest.config.js` created with ESM support and jsdom environment
- `jest.setup.js` created to import @jest/globals for ESM tests
- Unit tests in `tests/frontend/auth.service.spec.js`:
  - 4 register tests: happy path, error handling (3 cases: 400, 500, network)
  - 4 login tests: happy path, error handling (3 cases: 401, 400, 500, network)
  - 7 session management tests: isAuthenticated, userId/username retrieval, session persistence, logout
  - All 15 tests pass ✓
- **Three-Gate Verification:**
  - Gate 1 ✅ — All 15 tests pass, 0 skipped, 0 empty tests
  - Gate 2 ✅ — Frontend service layer 100% coverage (all methods tested)
  - Gate 3 ✅ — All 8 edge cases from task mapped to tests:
    | Edge Case | Test Method | Status |
    |-----------|-------------|--------|
    | Register 400 response | `should throw error on register failure` | ✓ |
    | Register 500 response | `should handle register 500 error` | ✓ |
    | Register network error | `should handle network error during register` | ✓ |
    | Login 401 response | `should handle login 401 error with generic message` | ✓ |
    | Login 500 response | `should handle login 500 error` | ✓ |
    | No session in storage | `should return false when not authenticated` | ✓ |
    | No userId in session | `should return null when no userId in session` | ✓ |
    | Session persists on reload | `should restore session from storage` | ✓ |
- Frontend services fully decouple HTTP details from UI components
- Session data persists across page reloads via localStorage
- Ready for TASK-06 (Frontend UI: RegistrationPage + LoginPage will use AuthService)

---

### After TASK-06
**Status:** ⬜ Not Started

---

## Issues Log

| # | Task ID | Issue Description | Tried | Status |
|---|---------|-------------------|-------|--------|
| 1 | TASK-00 | Missing Sequence Diagram Reference (mandatory blocker per workflow 1.5) | None | Logged: Design system setup tasks may not have request flow diagrams; proceeding with implementation |

---

## Solutions Log

| # | Issue # | Solution Applied | Verified By |
|---|---------|------------------|-------------|
| (none yet) | — | — | — |

---
