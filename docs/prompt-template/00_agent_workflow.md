# Agent Workflow — Global Execution Standard (.NET + Angular)

> This file governs every task in this project. Follow each section strictly before, during, and after implementation.

---

## Local Development Environment

**Minimum Tech Spec:**

- OS: Linux (Ubuntu 25.10)
- Docker: Docker Engine (with `docker compose` plugin, not legacy `docker-compose`)
- .NET SDK: 9.0.x (LTS)
- Node.js: v22.20.0 via NVM 0.40.3
- npm: 10.9.3
- Angular CLI: 20.3.5
- Database: PostgreSQL 16 with `pgvector` extension (runs in Docker on a non-default port, e.g., 5434)

**Before starting any task, read the Project Structure Reference section in `README.md`.** All layer names, folder paths, and build/run commands in this file refer to the definitions there. If the structure differs from what is shown below, your README takes precedence.

---

## 1. Task Execution Workflow

For every task, follow this order without deviation:

0. Always say **"HI BOSS THUAN"** before executing.

1. **Read the task file**
   - Read the task `.md` file completely before writing any code.
   - Do not skim. Ensure all Design Rules, Edge Cases, and Verification Steps are understood.

1.5 **Read and internalize the Sequence Diagram Reference**
   - Read the Sequence Diagram Reference section in the task file completely.
   - Understand the exact layer each step happens in (layers defined in README.md Project Structure Reference).
   - Map each diagram step to the files and methods you will write.
   - If any step is ambiguous, missing layer labels, or conflicts with other task sections, **STOP immediately** and log it in PROGRESS.md Issues Log with specific evidence before writing any code.
   - **Critical rule:** You are not allowed to add logic that is not shown in the diagram, nor skip logic that is shown. The diagram is the contract.

2. **Read PROGRESS.md**
   - Read the latest `docs/sprint-N/PROGRESS.md`:
     - **Handoff Out** notes from previous tasks.
     - **Issues Log** and **Solutions Log**.
   - This is mandatory to avoid duplicating work or re-introducing past bugs.

3. **Create a task branch**
   - From `develop` (or sprint base branch), create a new branch:
     ```bash
     git checkout develop
     git pull
     git checkout -b feature/task-{N}-{short-slug}
     ```

4. **Load environment & start services**

   Use the commands defined in the root README.md → **Project Structure Reference → Commands table**:
   
   4.1 **Backend**: Run the "Build backend" and "Run backend" commands
   
   4.2 **Frontend**: Run the "Run frontend dev" command
   
   (For first-time frontend setup, install dependencies first: use your package manager per the Commands table)

5. **When backend structure changes, ensure clean build before running**
   - If the task modifies files in any of the Domain, Application, or Infrastructure layers (see root README.md Project Structure Reference), run the "Clean backend" command from the root README.md before running the API.
   - Then run the "Run backend" command to start the API.

6. **Implement with tests (never implementation without tests)**

   - Follow the conventions in sections 2–3.
   - For every task that touches backend or frontend logic, write the corresponding **test file in the same task** — not after.
   - See Section 6 (Test Execution Standard) for:
     - which test type to write per layer,
     - where to place the file,
     - what rules apply.
   - Do **not** proceed to the verification step until both the implementation and its tests exist.

7. **Build & verify locally**

   - Follow Section 4 (Verification Checklist) and Section 6.3 (Three-Gate Verification).
   - All of the following must pass before you consider the task complete:
     - Layer-specific tests (Gate 1).
     - Coverage expectations (Gate 2) where applicable.
     - Edge-case traceability (Gate 3).
     - Manual build/run checks in Section 4.

8. **Update PROGRESS.md**

   In `docs/sprint-N/PROGRESS.md`:

   - Update the task’s status:
     - At the start: ⬜ → 🔄 In Progress
     - After completion: 🔄 → ✅ Done
   - Append to **Handoff Out**:
     - File paths, class names, DI registrations, DB changes, new RabbitMQ queues/exchanges, SignalR hubs/groups, etc.
   - Append to **Errors Log** for any non-trivial issues:
     - Format:  
       `**[T{N}] {title}:** {cause} → {fix applied}`
   - Append to **Solutions Log** to record final fixes:
     - Format:  
       `**[T{N}] {issue title}:** {what was done to resolve}`

9. **Commit according to Git conventions**

   - See Section 5 (Git Conventions).
   - A commit must include both implementation AND test files. Never commit one without the other.
   - Use commit type `test` only when the task is pure test retrofit.

10. **Open PR and wait for human review/merge**

   - Push your branch and open a Pull Request into `develop`.
   - You are not allowed to merge; suggest that Boss Thuận or another maintainer does the merge after review and local verification.

---

## 2. .NET / ASP.NET Core Conventions

### Structure

> Layer folder paths are defined in the root README.md → **Project Structure Reference → Layers table.** Use that table as the authoritative source for where each layer's code lives. The responsibilities of each layer are:

- **Domain Layer**  
  - Domain entities and value objects (e.g., User, Course, Request).
  - Business invariants and core logic.
  - No HTTP or ORM implementation details.

- **Application Layer**  
  - Use-case services and application orchestration.
  - Interfaces for repositories and external adapters (e.g., IUserRepository, IPasswordHasher, INotificationService).
  - Enforces business rules and coordinates domain/persistence boundaries.

- **Infrastructure Layer**  
  - ORM DbContext, migrations, and repository implementations.
  - Persistence and external integration implementations.
  - Concrete implementations of application layer interfaces.

- **API Layer**  
  - Controllers, request/response DTOs, and API wiring.
  - Dependency injection configuration and setup.
  - Middleware for validation, error handling, authentication/authorization, and rate limiting.

### Naming

| Element      | Convention                         | Example                        |
|-------------|-------------------------------------|--------------------------------|
| Namespaces  | `LocalCommunityToolLending.{Layer}.{Area}` | `LocalCommunityToolLending.Domain.Users` |
| Classes     | PascalCase                          | `Course`, `CourseService`      |
| Methods     | PascalCase                          | `PublishCourseAsync`           |
| Properties  | PascalCase                          | `InstructorId`, `CreatedAt`    |
| Private fields | `_camelCase`                     | `_courseRepository`            |
| DTOs        | Suffix with `Dto`                  | `CreateCourseRequestDto`       |
| Controllers | Suffix with `Controller`           | `CoursesController`            |
| Interfaces  | Prefix with `I`                    | `ICourseRepository`            |

### Code Rules

- Controllers:
  - Use attribute routing (`[Route("api/[controller]")]`) and method-level attributes (`[HttpGet]`, `[HttpPost]`, etc.).
  - Return `ActionResult<T>` or `IActionResult`, not raw types.
  - Never expose domain entities directly over HTTP; always map to DTOs.

- Repositories / EF Core:
  - Use `DbContext` injected via DI.
  - Use async methods (`ToListAsync`, `FirstOrDefaultAsync`, etc.).
  - Enforce ownership (ABAC) and tenant boundaries at the application or repository layer where appropriate.

- Validation:
  - Apply `[Required]`, `[StringLength]`, etc. on DTOs and view models.
  - Use `ModelState` checks in controllers; invalid models must return 400 with structured error payload.

- Error handling:
  - No unhandled thrown exceptions reach the client.
  - Use exception-handling middleware or filters to map domain/application exceptions to HTTP status codes (400, 403, 404, 409, 500).

- Security:
  - `UserId` / `InstructorId` / `StudentId` are derived from the authenticated principal (JWT claims) via `HttpContext.User`, never from the request body or query.
  - Ownership checks must be enforced before any mutation (e.g., only the course owner or admin can modify a given course).

- Async:
  - Use async/await for I/O-bound operations.
  - Do not use `.Result` or `.Wait()` on tasks in ASP.NET Core code.

---

## 3. Angular Conventions

### Structure

```text
frontend/src/app/
├── core/services/          # HttpClient services (one per API aggregate)
├── core/guards/            # Route guards, auth checks
├── features/
│   ├── dashboard/          # Dashboard screens
│   └── course-builder/     # Course builder, lesson editor, etc.
└── shared/                 # Shared components, pipes, validators
```

### Rules

- Use **standalone components** (`standalone: true`). No NgModules.
- Components must not call `HttpClient` directly — always go through a service in `core/services/`.
- Prefer **Reactive Forms** for all forms; no template-driven forms.
- API base URL lives only in `environment*.ts`; never hardcode URLs.
- Handle HTTP errors in services, expose clear observables/promises to components.
- Forms:
  - Show validation messages only after control is `touched`.
  - Use meaningful error messages (no generic “Error” alerts).
- Navigation:
  - After creating entities (e.g., course), navigate to appropriate details page within the success callback.
  - After 204 No Content actions (delete, etc.), update local state without full page reload where possible.

---

## 4. Verification Checklist (Local Only)

Complete every check before moving to the next task. Do not skip.

### Backend

> Use the commands from the root README.md → **Project Structure Reference → Commands table**.

- [ ] Run the "Build backend" command → **0 errors**
- [ ] For schema changes:
  - [ ] New migrations created via the "Add DB migration" command
  - [ ] "Apply DB migration" command runs without error
- [ ] API starts cleanly:
  - [ ] Run the "Run backend" command
- [ ] Swagger / OpenAPI UI reachable (e.g., `http://localhost:{port}/swagger`).
- [ ] For each new or modified endpoint:
  - [ ] Happy path tested manually (via Swagger/Postman).
  - [ ] All documented error paths (401, 403, 404, 400, 429, 500) verified where applicable.

### Frontend

> Use the commands from the root README.md → **Project Structure Reference → Commands table**.

- [ ] Run the "Lint frontend" command → **0 lint errors** (if lint configured).
- [ ] Run the "Build frontend" command → **0 build errors**.
- [ ] Run the "Run frontend dev" command — launches without console errors.
- [ ] New or updated routes render without blank screens or JS errors.
- [ ] Forms show validation messages and disable/enable buttons according to design.

### Database

- [ ] New tables/columns exist in PostgreSQL with expected types and constraints.
- [ ] Foreign keys and indexes defined per design document.
- [ ] No ad-hoc schema changes were made outside EF Core migrations.

### Feature Flags

- [ ] Any flags defined in the task's **Feature Flags** section are registered in configuration (e.g., `appsettings.json`, `environment.ts`).
- [ ] No feature-flagged code path is active by default unless the task explicitly specifies it.
- [ ] If a flag was introduced in a previous task, verify this task does not inadvertently toggle it off or change its default.
- [ ] Flags are retrievable at runtime (hardcoded or from config) — no broken references.

---

## 5. Git Conventions

### Branch Naming

```text
feature/task-{N}-{short-slug}
```

Examples:

- `feature/task-1-domain-layer`
- `feature/task-3-expiration-worker`
- `feature/task-7-student-onboarding-ui`

### Commit Message Format (Conventional Commits)

```text
<type>(<scope>): <short description>
```

| Type      | When to use                         |
|-----------|-------------------------------------|
| `feat`    | New functionality                   |
| `fix`     | Bug or logic correction             |
| `chore`   | Config, tooling, wiring (no logic)  |
| `refactor`| Code restructure without behavior change |
| `test`    | Adding or fixing tests              |

Examples:

```text
feat(domain): add Course and Instructor aggregates
chore(api): configure Swagger and problem-details responses
feat(worker): implement DraftExpirationWorker
test(application): cover cooldown rule for assessment retry
```

### Merge Rules

- Squash commits for a single task branch are acceptable.
- Never commit directly to `develop` or `main`.
- After review and local verification, a maintainer merges the PR.
- Delete the task branch after merge.

---

## 6. Test Execution Standard

> This section governs test file creation and execution for every task.
> It is not optional. Follow it for every task that touches backend or frontend code.

### 6.1 Test Type Per Layer

Every layer has a corresponding test type. Write the correct type — do not substitute.

| Layer                          | Project folder (see root README) | Test location | Test type                         |
|--------------------------------|--|--|-----------------------------------|
| Domain (pure entities/value objects) | Domain layer folder | `{Domain layer folder}/Tests/...` | xUnit tests for non-trivial logic |
| Application (services/use cases)| Application layer folder | `{Application layer folder}/Tests/...` | xUnit with mocks (e.g., Moq) |
| Infrastructure (repositories, integrations) | Infrastructure layer folder | `{Infrastructure layer folder}/Tests/...` | xUnit + EF Core InMemory / test DB |
| API (controllers, filters, middleware) | API layer folder | `{API layer folder}/Tests/...` | xUnit + WebApplicationFactory / TestServer |
| Frontend service | Frontend layer folder | Same folder: `<name>.service.spec.ts` | Jasmine + HttpClientTestingModule |
| Frontend component | Frontend layer folder | Same folder: `<name>.component.spec.ts` | Jasmine + Angular TestBed |

The test folder structure should mirror the backend project folders and namespaces.

### 6.2 Security Rules for Tests

- Never use real JWTs, OAuth tokens, or real API keys in any test.
- Backend tests:
  - Use test principals / fake claims for authenticated scenarios.
  - Do not call real external services; mock all HTTP clients and message bus clients.
- Frontend tests:
  - Use `HttpClientTestingModule` to mock HTTP.
  - Never call real API endpoints.
  - Fake token strings are allowed for testing parsing/branching logic but must not be real tokens.

### 6.3 Three-Gate Verification (Required Before Task Handoff)

Run all three gates in order after writing the test file(s). A task cannot be handed off if any gate fails.

#### Gate 1 — Correctness: tests pass, nothing skipped

**Backend:**

Run the "Run backend tests" command from the root README.md → **Project Structure Reference → Commands table** for each touched layer.

Expected: all relevant test projects build and run, with 0 failed tests.

**Frontend:**

Run the "Run frontend tests" command from README.md → **Project Structure Reference → Commands table** (or the CI variant for headless runs).

Expected: specs run with 0 failures.

Additional checks:

- No test method is empty.
- No tests are skipped/disabled (`[Fact(Skip=...)]`, `xit`, `xdescribe`, etc.).

#### Gate 2 — Coverage: meaningful lines are covered

If coverage tooling is configured:

- Backend: use the "Run backend tests" command with coverage tool (e.g., Coverlet) as per your project setup.
- Frontend: use the "Frontend coverage" command from README.md → **Project Structure Reference → Commands table**.

Minimum expectations (guideline):

- Backend service and controller classes: ≥ 70–80% line coverage.
- Frontend service and component classes: ≥ 70% statement coverage.
- Complex domain or business logic: high coverage on branches and edge cases.

If coverage is below threshold for critical logic, add tests until it is acceptable.

#### Gate 3 — Behavior: every edge case has a named test

Every edge case listed in the task file’s **Edge Cases Handled** section must map to at least one test method.

In the task file, fill a traceability table:

```markdown
| Edge case                                     | Test method name                          | Asserts                          |
|-----------------------------------------------|-------------------------------------------|----------------------------------|
| User submits empty title                      | `CreateCourse_ReturnsBadRequest_WhenTitleMissing` | HTTP 400 + validation payload  |
| Course owner id does not match current user   | `UpdateCourse_ReturnsForbidden_WhenNotOwner`      | HTTP 403                        |
| …                                             | …                                         | …                                |
```

If any edge case is missing a test, add it before marking the task done.

---

## 7. Shared Invariants

These rules apply across the project and override task-specific instructions if they conflict:

- Expiration dates or timeouts are always calculated on the **backend** (e.g., `DateTime.UtcNow.AddDays(7)`); the frontend never decides expiration logic.
- User identity (`UserId`, `InstructorId`, etc.) is always derived from the authentication context (JWT claims), not from client-supplied fields.
- All user-generated rich text is sanitized before being stored in the database.
- EF Core migrations are the only allowed way to change the schema; never apply ad-hoc SQL outside migrations.
- Docker infrastructure services must be running before backend API startup and migration commands.

---

## 8. UI Screen Implementation Standard (Angular)

> This section applies to every task that creates or modifies an Angular component with a visual template.

### 8.1 Design System Authority

- Follow the project’s design system document (e.g., `DESIGN-SYSTEM-v2.md`) as the **single source of truth**:
  - Colors, typography, spacing.
  - Shared components and layout shells.
  - Animation and interaction patterns.

Before implementing any screen:

1. Read the relevant parts of the design system.
2. Use existing shared components where possible (buttons, cards, form fields, etc.).
3. Use only the provided CSS utility classes / tokens; do not hardcode arbitrary colors, spacing, or radii.

### 8.2 Angular Structural Rules

- Components are standalone; compose them via `loadComponent` routes.
- Keep components “thin”; push logic into services as much as possible.
- Use typed observables and strict TypeScript (`"strict": true`).
- Fetch data in `ngOnInit` / appropriate lifecycle hooks, not in the constructor.
- Route parameters and query parameters must come from `ActivatedRoute`.

### 8.3 File Placement & Output Order

New screen components go in:

```text
src/app/features/{feature-name}/
├── {name}.component.ts
├── {name}.component.html
└── {name}.component.scss
```

When implementing a new screen, output in this order:

1. `{name}.component.ts` — full class with state, form, and service calls.
2. `{name}.component.html` — full template, no TODO placeholders.
3. `{name}.component.scss` — only what’s necessary beyond shared styles.
4. Route entry diff for `app.routes.ts` (using `loadComponent` + lazy loading).
5. Service changes in `core/services/` (if new endpoints are used).
6. Corresponding test: `{name}.component.spec.ts`.
