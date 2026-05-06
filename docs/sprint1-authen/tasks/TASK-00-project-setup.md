# TASK-00 — Project Setup: ASP.NET Core + SQLite + Frontend Scaffold

## Context

This is the **foundational setup task** for the entire Tool Lending Platform. It establishes the project structure, initializes ASP.NET Core with dependency injection and configuration, sets up SQLite, and scaffolds the frontend with basic HTML/CSS/JS structure. Without this setup, no subsequent tasks can run.

This task enables all future flows by providing:
- A running ASP.NET Core API server (localhost:5000 or configurable)
- SQLite database file and connection string
- Frontend static file serving
- Dependency injection container
- Session middleware configuration
- CORS configuration for local development

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/tool-lending-inception.md` | §6 Architecture Design | Layered architecture, technology stack (ASP.NET Core, HTML/CSS/JS, SQLite) |
| `docs/tool-lending-srs.md` | §3.4 Technology & Architecture Preferences | HTML/CSS/JS frontend, ASP.NET Core backend, SQLite database |
| `docs/sprint1-authen/sprint1-design.md` | §2.3 Backend API Layer, §6.3 Technology & Integration View | API endpoints, project organization, middleware requirements |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Not applicable for setup task** — this is infrastructure-only. No user flows run on this task alone.

---

## Design Rule Checklist

Setup-specific rules extracted from the Inception and Sprint Design documents:

| # | Exact Rule | Source | Owner Files / Layer | Verification Hook |
|---|-----------|--------|---------------------|--------------------|
| 1 | "ASP.NET Core (C#)" backend technology | Inception §6.3, SRS §3.4 | `backend/Program.cs`, `.csproj` files | Project builds without errors |
| 2 | "HTML, CSS, JavaScript (vanilla or lightweight framework)" frontend | Inception §6.3, SRS §3.4 | `frontend/index.html`, CSS, JS | Files exist and static serving works |
| 3 | "SQLite" database, "lightweight engine" | Inception §6.3, SRS §3.4 | `appsettings.json`, `ToolLendingDbContext.cs` | Connection string configured, migrations can run |
| 4 | "Session-based (username/password) authentication" | Inception §8.1, Sprint Design §3 | `Program.cs` middleware setup | Session middleware registered and middleware pipeline configured |
| 5 | "Layered architecture": Presentation → API → Application → Domain → Infrastructure | Inception §6.2 | Folder structure: `/backend/Api`, `/backend/Application`, `/backend/Domain`, `/backend/Infrastructure` | Folders exist and DI respects layer boundaries |
| 6 | No external API integrations required for prototype | Inception §3.5, SRS §3.5 | No 3rd-party NuGet packages for OAuth, payment, etc. | `Program.cs` only registers core services |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `backend/ToolLendingPlatform.csproj` | CREATE | Project file | `<TargetFramework>net8.0</TargetFramework>`, NuGet packages for EF Core, ASP.NET Core |
| `backend/Program.cs` | CREATE | Configuration | `WebApplicationBuilder`, `services.AddScoped`, `app.UseSession()`, `app.UseRouting()`, `app.MapControllers()` |
| `backend/appsettings.json` | CREATE | Configuration | `"ConnectionStrings": { "DefaultConnection": "..." }`, `"Logging": { ... }`, `"AllowedHosts": "*"` |
| `backend/appsettings.Development.json` | CREATE | Configuration | `"Logging": { "LogLevel": { "Default": "Debug" } }` |
| `backend/.gitignore` | CREATE | VCS | `bin/`, `obj/`, `*.db`, `appsettings.local.json` |
| `backend/Domain/User.cs` | CREATE (placeholder) | Domain | Empty class `namespace ToolLendingPlatform.Domain { public class User { } }` |
| `backend/Application/Interfaces/IUserRepository.cs` | CREATE (placeholder) | Application | Empty interface for TASK-02 |
| `backend/Infrastructure/Data/ToolLendingDbContext.cs` | CREATE | Infrastructure | `DbContext`, `DbSet<User>`, `OnConfiguring` with SQLite |
| `backend/Api/Controllers/HealthController.cs` | CREATE | API | `[ApiController]`, `[Route("api/[controller]")]`, `[HttpGet("health")]` → `{ "status": "ok" }` |
| `backend/Api/Middleware/SessionMiddleware.cs` | CREATE | API | Session cookie setup, HTTP-only, secure flags |
| `frontend/index.html` | CREATE | Presentation | `<!DOCTYPE html>`, `<head>`, `<body>`, `<script src="app.js"></script>` |
| `frontend/css/styles.css` | CREATE | Presentation | Basic CSS reset, layout grid/flexbox scaffolding |
| `frontend/js/app.js` | CREATE | Presentation | `console.log("App loaded")`, basic initialization |
| `frontend/pages/register.html` | CREATE | Presentation | Placeholder page (content filled in TASK-06) |
| `frontend/pages/login.html` | CREATE | Presentation | Placeholder page (content filled in TASK-06) |
| `frontend/js/auth.js` | CREATE (placeholder) | Presentation | Empty module for TASK-05 |

---

## Implementation Skeleton

### `backend/ToolLendingPlatform.csproj` (CREATE)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- EF Core for SQLite -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    
    <!-- ASP.NET Core dependencies (auto-included by SDK) -->
    <!-- Password hashing -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    
    <!-- Logging (optional, for debugging) -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
  </ItemGroup>
</Project>
```

**Responsibilities:**
- Defines project as ASP.NET Core web project targeting .NET 8.
- Includes EF Core with SQLite support.
- Includes BCrypt for password hashing.

---

### `backend/Program.cs` (CREATE)

```csharp
using ToolLendingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;

var builder = WebApplicationBuilder.CreateBuilder(args);

// ===== Configuration =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ===== Add Services =====

// Database
builder.Services.AddDbContext<ToolLendingDbContext>(options =>
    options.UseSqlite(connectionString));

// Session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS in production
});

// Controllers
builder.Services.AddControllers();

// CORS (for local dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Health checks (optional, for monitoring)
builder.Services.AddHealthChecks();

// ===== Build App =====
var app = builder.Build();

// ===== Database Migration =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToolLendingDbContext>();
    db.Database.Migrate();
}

// ===== Configure Middleware Pipeline =====

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Static files (frontend)
app.UseStaticFiles();

// CORS
app.UseCors("AllowLocalhost");

// Session middleware MUST come before routing
app.UseSession();

// Routing
app.UseRouting();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Fallback: serve index.html for SPA (optional, for TASK-06+)
app.MapFallbackToFile("index.html");

app.Run("http://localhost:5000");
```

**Responsibilities:**
- Registers DbContext with SQLite.
- Configures session middleware (30-min timeout, HTTP-only cookies).
- Adds controllers and CORS.
- Runs database migrations on startup.
- Serves static frontend files.

**Dependency Contracts:**
- `ToolLendingDbContext` must be defined in `backend/Infrastructure/Data/ToolLendingDbContext.cs`.
- `appsettings.json` must have `"ConnectionStrings": { "DefaultConnection": "..." }`.

---

### `backend/appsettings.json` (CREATE)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tool_lending.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Responsibilities:**
- Defines SQLite database file location (`tool_lending.db` in project root).
- Logging configuration.

---

### `backend/appsettings.Development.json` (CREATE)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Responsibilities:**
- Override logging levels for development environment.

---

### `backend/Infrastructure/Data/ToolLendingDbContext.cs` (CREATE)

```csharp
using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;

namespace ToolLendingPlatform.Infrastructure.Data
{
    public class ToolLendingDbContext : DbContext
    {
        public ToolLendingDbContext(DbContextOptions<ToolLendingDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        // Future: DbSet<Tool>, DbSet<BorrowRequest>, etc.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
```

**Responsibilities:**
- Defines DbContext with Users DbSet.
- Configures entity mappings: unique username index, required fields, defaults.

---

### `backend/Domain/User.cs` (CREATE, placeholder for TASK-01)

```csharp
namespace ToolLendingPlatform.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

**Responsibilities (placeholder):**
- Placeholder entity; will be expanded in TASK-01 with business logic.

---

### `backend/Application/Interfaces/IUserRepository.cs` (CREATE, placeholder for TASK-02)

```csharp
using ToolLendingPlatform.Domain;

namespace ToolLendingPlatform.Application.Interfaces
{
    public interface IUserRepository
    {
        // Methods to be defined in TASK-02
    }
}
```

---

### `backend/Api/Controllers/HealthController.cs` (CREATE)

```csharp
using Microsoft.AspNetCore.Mvc;

namespace ToolLendingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
        }
    }
}
```

**Responsibilities:**
- Simple health check endpoint for deployment verification.

---

### `backend/Api/Middleware/SessionMiddleware.cs` (CREATE, or use ASP.NET Core built-in)

```csharp
using Microsoft.AspNetCore.Http;

namespace ToolLendingPlatform.Api.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ASP.NET Core session middleware is already configured in Program.cs
            // This placeholder can be extended for custom session logic in future tasks
            await _next(context);
        }
    }
}
```

**Note:** ASP.NET Core session middleware is registered in `Program.cs` via `app.UseSession()`.

---

### `frontend/index.html` (CREATE)

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Tool Lending Platform</title>
    <link rel="stylesheet" href="/css/styles.css">
</head>
<body>
    <div id="app">
        <header>
            <nav class="navbar">
                <h1>Tool Lending Platform</h1>
                <div id="nav-menu"></div>
            </nav>
        </header>
        <main id="main-content">
            <!-- Page content will be loaded here -->
        </main>
    </div>

    <script src="/js/app.js"></script>
</body>
</html>
```

**Responsibilities:**
- Single-page app container.
- Loads styles and main app script.

---

### `frontend/css/styles.css` (CREATE)

```css
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

html, body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    line-height: 1.6;
    color: #333;
}

body {
    background-color: #f8f9fa;
}

#app {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

header {
    background: white;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    padding: 1rem;
}

.navbar {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.navbar h1 {
    font-size: 1.5rem;
    color: #2c3e50;
}

main {
    flex: 1;
    max-width: 1200px;
    margin: 0 auto;
    width: 100%;
    padding: 2rem 1rem;
}

/* Form styles (used in TASK-06) */
form {
    background: white;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    max-width: 500px;
    margin: 0 auto;
}

form label {
    display: block;
    margin: 1rem 0 0.5rem;
    font-weight: 500;
}

form input {
    width: 100%;
    padding: 0.75rem;
    margin-bottom: 1rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-size: 1rem;
}

form button {
    width: 100%;
    padding: 0.75rem;
    background: #3498db;
    color: white;
    border: none;
    border-radius: 4px;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.2s;
}

form button:hover {
    background: #2980b9;
}

.error {
    color: #e74c3c;
    font-size: 0.875rem;
    margin-top: -0.5rem;
    margin-bottom: 1rem;
}

.success {
    color: #27ae60;
    font-size: 0.875rem;
}
```

**Responsibilities:**
- Basic styling for layout and forms.
- Responsive design scaffolding.

---

### `frontend/js/app.js` (CREATE)

```javascript
document.addEventListener('DOMContentLoaded', () => {
    console.log('Tool Lending Platform initialized');

    // Health check on app startup
    fetch('/api/health')
        .then(res => res.json())
        .then(data => console.log('Backend health:', data))
        .catch(err => console.error('Backend unreachable:', err));
});
```

**Responsibilities:**
- App initialization.
- Health check verification.

---

### `frontend/pages/register.html` (CREATE, placeholder)

```html
<div id="register-page">
    <h2>Register</h2>
    <p>(Content filled in TASK-06)</p>
</div>
```

---

### `frontend/pages/login.html` (CREATE, placeholder)

```html
<div id="login-page">
    <h2>Login</h2>
    <p>(Content filled in TASK-06)</p>
</div>
```

---

### `frontend/js/auth.js` (CREATE, placeholder for TASK-05)

```javascript
// AuthService and related functions will be implemented in TASK-05
class AuthService {
    // Placeholder
}

export default AuthService;
```

---

## Edge Cases Handled

No runtime edge cases for this setup task; it is purely infrastructure.

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Project builds without errors:
   ```bash
   cd backend
   dotnet build
   ```

2. Database migration runs successfully:
   ```bash
   cd backend
   dotnet ef database update
   # Verify tool_lending.db file created
   ```

3. Health endpoint responds:
   ```bash
   dotnet run --project backend
   # In another terminal:
   curl http://localhost:5000/api/health
   # Expected: { "status": "ok", "timestamp": "..." }
   ```

4. Frontend static files are served:
   ```bash
   # Visit http://localhost:5000/index.html
   # Expected: HTML page loads, console shows "Tool Lending Platform initialized"
   ```

5. Browser console shows no errors (check F12 → Console).

**Manual Checks:**
1. Verify project structure:
   ```
   backend/
   ├── ToolLendingPlatform.csproj
   ├── Program.cs
   ├── appsettings.json
   ├── appsettings.Development.json
   ├── Domain/
   │   └── User.cs
   ├── Application/
   │   └── Interfaces/
   │       └── IUserRepository.cs
   ├── Infrastructure/
   │   └── Data/
   │       └── ToolLendingDbContext.cs
   └── Api/
       ├── Controllers/
       │   └── HealthController.cs
       └── Middleware/
           └── SessionMiddleware.cs
   
   frontend/
   ├── index.html
   ├── css/
   │   └── styles.css
   ├── js/
   │   ├── app.js
   │   └── auth.js
   └── pages/
       ├── register.html
       └── login.html
   ```

2. Verify database file created: `backend/tool_lending.db` exists after migration.

3. Verify session middleware logging in console (if debug enabled).

---

## Definition of Done

This task is complete when:

- ✅ ASP.NET Core project builds successfully (`dotnet build` exits with 0).
- ✅ SQLite database is created and migrations run (`dotnet ef database update` succeeds).
- ✅ Health endpoint responds with 200 OK at `http://localhost:5000/api/health`.
- ✅ Frontend static files are served correctly (`index.html` loads and app.js runs).
- ✅ No build errors or warnings in the project.
- ✅ All folder structure from "Affected Files" is in place.
- ✅ `Program.cs` registers session middleware and controllers.
- ✅ Database connection string in `appsettings.json` points to valid SQLite file.

**Handoff to TASK-01:**
- Project builds and runs cleanly.
- DbContext is configured with EF Core.
- Folder structure respects layered architecture.
- Ready to implement domain entities (User, exceptions, etc.).

---
