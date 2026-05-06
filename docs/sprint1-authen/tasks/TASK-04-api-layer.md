# TASK-04 — API Layer: AuthController + DTOs + DI Configuration

## Context

This task implements the **REST API endpoints** for authentication: `POST /api/auth/register` and `POST /api/auth/login`. It includes:
- `AuthController` (routes HTTP requests to services)
- DTOs for request/response payloads (`RegisterRequestDto`, `LoginRequestDto`, etc.)
- Dependency injection configuration in `Program.cs`
- Exception-to-HTTP-response mapping via filters
- Session/cookie management

This task is the boundary between frontend (HTTP clients) and backend (services). It orchestrates HTTP request/response while delegating business logic to the application layer.

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/sprint1-authen/sprint1-design.md` | §3 API Contract & Payload | Exact endpoints, request/response schemas, HTTP codes |
| `docs/sprint1-authen/sprint1-design.md` | §5b Backend Class Diagram | AuthController class structure |
| `docs/sprint1-authen/sprint1-design.md` | §4 Sequence Diagrams | HTTP request/response flow and error handling |
| `docs/tool-lending-inception.md` | §8.2 Authorization & RBAC | Security rules (no RBAC for auth endpoints) |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**BF1 Registration — Controller flow:**
```
User -> RegistrationPage: Submits form
RegistrationPage -> AuthController: POST /api/auth/register (username, password)
AuthController -> AuthController: Validate input (backend validation)
alt Backend Input Validation Fails
  AuthController -> RegistrationPage: 400 Bad Request (validation errors)
else Input Valid
  AuthController -> AuthenticationService: register(username, password)
  [service throws exceptions: DuplicateUsernameException, InvalidPasswordException, etc.]
  alt Exception thrown
    AuthController -> RegistrationPage: 400 Bad Request or mapped HTTP code
  else Service succeeds
    AuthController -> AuthController: Create session cookie (userId, expires=30min)
    AuthController -> RegistrationPage: 201 Created (userId, username, createdAt)
```

**BF2 Login — Controller flow:**
```
User -> LoginPage: Submits form
LoginPage -> AuthController: POST /api/auth/login (username, password)
AuthController -> AuthController: Validate input
alt Input Validation Fails
  AuthController -> LoginPage: 400 Bad Request
else Input Valid
  AuthController -> AuthenticationService: login(username, password)
  alt AuthenticationException thrown
    AuthController -> LoginPage: 401 Unauthorized ("Invalid username or password")
  else Service succeeds
    AuthController -> AuthController: Create session cookie
    AuthController -> LoginPage: 200 OK (userId, username)
```

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files | Test Name | Verification |
|---|---------------------|--------|-------------|-----------|--------------|
| 1 | "POST /api/auth/register endpoint exists, takes (username, password), returns 201 on success" | Sprint Design §3 API Contract | `AuthController.cs` | `AuthControllerTests_RegisterReturns201` | Integration test |
| 2 | "POST /api/auth/login endpoint exists, takes (username, password), returns 200 on success" | Sprint Design §3 API Contract | `AuthController.cs` | `AuthControllerTests_LoginReturns200` | Integration test |
| 3 | "Register validates backend: username 3-50 chars alphanumeric+underscore, password 6+ chars with complexity" | Sprint Design §3 API Contract, BF1 sequence | `AuthController.cs` (delegates to AuthService) | `AuthControllerTests_RegisterInvalidInput400` | Integration test |
| 4 | "Register returns 400 Bad Request if username already exists with message 'Username already taken'" | Sprint Design §3 API Contract | `AuthController.cs` (catches DuplicateUsernameException) | `AuthControllerTests_RegisterDuplicate400` | Integration test |
| 5 | "Login returns 401 Unauthorized with generic message 'Invalid username or password' if user not found or password wrong" | Sprint Design §3 API Contract, Security §8.4 | `AuthController.cs` (catches AuthenticationException) | `AuthControllerTests_LoginWrongPassword401` | Integration test |
| 6 | "On successful registration, session cookie is created (userId, HTTP-only, secure, 30-min timeout)" | Inception §8.1, Sprint Design §3 Security Notes | `AuthController.cs`, `Program.cs` session setup | `AuthControllerTests_RegisterSetsCookie` | Integration test |
| 7 | "On successful login, session cookie is created" | Same as #6 | `AuthController.cs` | `AuthControllerTests_LoginSetsCookie` | Integration test |
| 8 | "Request body fields required: username, password (no null/empty)" | Sprint Design §3 API Contract, BF1/BF2 sequence | `AuthController.cs` validation | `AuthControllerTests_MissingFieldReturns400` | Integration test |
| 9 | "Response payload: 201 Created includes userId, username, createdAt, message" | Sprint Design §3 API Contract (register response) | `RegisterResponseDto` class | (checked in integration test) | Integration test |
| 10 | "Response payload: 200 OK includes userId, username, message (no password)" | Sprint Design §3 API Contract (login response) | `LoginResponseDto` class | (checked in integration test) | Integration test |
| 11 | "DI registration: AuthenticationService, PasswordHasher, UserRepository, IUserRepository registered in DI container" | Inception §6.2 Layers | `Program.cs` | `DI setup verified` | Manual verification |
| 12 | "Exception filter maps exceptions to HTTP responses: DuplicateUsernameException → 400, AuthenticationException → 401, InvalidPasswordException → 400" | BF1/BF2 sequences, error branches | `AuthExceptionFilter.cs` or controller catches | `AuthControllerTests_ExceptionMapping` | Integration test |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `backend/Api/Controllers/AuthController.cs` | CREATE | API controller | `[ApiController] class AuthController { RegisterAsync(), LoginAsync() }` |
| `backend/Api/Dtos/RegisterRequestDto.cs` | CREATE | API DTO | `class RegisterRequestDto { string Username, string Password }` |
| `backend/Api/Dtos/RegisterResponseDto.cs` | CREATE | API DTO | `class RegisterResponseDto { int UserId, string Username, DateTime CreatedAt, string Message }` |
| `backend/Api/Dtos/LoginRequestDto.cs` | CREATE | API DTO | `class LoginRequestDto { string Username, string Password }` |
| `backend/Api/Dtos/LoginResponseDto.cs` | CREATE | API DTO | `class LoginResponseDto { int UserId, string Username, string Message }` |
| `backend/Api/Dtos/ErrorResponseDto.cs` | CREATE | API DTO | `class ErrorResponseDto { string Error, object Details }` |
| `backend/Api/Filters/AuthExceptionFilter.cs` | CREATE | API filter | `[AttributeUsage] class AuthExceptionFilter : ExceptionFilterAttribute { OnException(...) }` |
| `backend/Program.cs` | MODIFY | Configuration | Add DI registrations: `AddScoped<IUserRepository>`, `AddScoped<AuthenticationService>`, etc. |
| `tests/Api/AuthControllerTests.cs` | CREATE | Integration tests | Multiple test methods for register/login flows with WebApplicationFactory |

---

## Implementation Skeleton

### `backend/Api/Dtos/RegisterRequestDto.cs` (CREATE)

```csharp
using System.ComponentModel.DataAnnotations;

namespace ToolLendingPlatform.Api.Dtos
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
```

---

### `backend/Api/Dtos/RegisterResponseDto.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Api.Dtos
{
    /// Rule #9: Response payload for successful registration (201 Created)
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = null!;
    }
}
```

---

### `backend/Api/Dtos/LoginRequestDto.cs` (CREATE)

```csharp
using System.ComponentModel.DataAnnotations;

namespace ToolLendingPlatform.Api.Dtos
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
```

---

### `backend/Api/Dtos/LoginResponseDto.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Api.Dtos
{
    /// Rule #10: Response payload for successful login (200 OK)
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
```

---

### `backend/Api/Dtos/ErrorResponseDto.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Api.Dtos
{
    public class ErrorResponseDto
    {
        public string Error { get; set; } = null!;
        public object? Details { get; set; }
    }
}
```

---

### `backend/Api/Filters/AuthExceptionFilter.cs` (CREATE)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Api.Dtos;

namespace ToolLendingPlatform.Api.Filters
{
    /// <summary>
    /// Exception filter for authentication-related exceptions.
    /// Rule #12: Maps domain/application exceptions to HTTP responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // Rule #4: DuplicateUsernameException → 400 Bad Request
            if (exception is DuplicateUsernameException dupEx)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponseDto
                {
                    Error = "ConflictError",
                    Details = new { message = dupEx.Message }
                });
                context.ExceptionHandled = true;
                return;
            }

            // Rule #5: AuthenticationException → 401 Unauthorized
            if (exception is AuthenticationException authEx)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponseDto
                {
                    Error = "AuthenticationError",
                    Details = new { message = authEx.Message }
                });
                context.ExceptionHandled = true;
                return;
            }

            // Rule #3: InvalidPasswordException, InvalidUsernameException → 400 Bad Request
            if (exception is InvalidPasswordException or InvalidUsernameException)
            {
                context.Result = new BadRequestObjectResult(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = new { message = exception.Message }
                });
                context.ExceptionHandled = true;
                return;
            }

            base.OnException(context);
        }
    }
}
```

---

### `backend/Api/Controllers/AuthController.cs` (CREATE)

```csharp
using Microsoft.AspNetCore.Mvc;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Application.Services;
using ToolLendingPlatform.Api.Dtos;
using ToolLendingPlatform.Api.Filters;

namespace ToolLendingPlatform.Api.Controllers
{
    /// <summary>
    /// Authentication controller for user registration and login.
    /// Rule #1, #2: Endpoints POST /api/auth/register and POST /api/auth/login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AuthExceptionFilter))] // Rule #12: Apply exception filter
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthController(AuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Rule #1: Register a new user.
        /// Validates input, creates user, sets session cookie.
        /// Returns: 201 Created with userId, username, createdAt
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
        {
            // Rule #3, #8: Validate request (ASP.NET Core model validation via DataAnnotations)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = errors
                });
            }

            // Delegate to application service (Rule #1)
            // Service will validate username/password and throw exceptions if invalid
            var user = await _authService.RegisterAsync(request.Username, request.Password);

            // Rule #6: Create session cookie (HTTP-only, secure, 30-min timeout)
            // ASP.NET Core session is configured in Program.cs; we store userId
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // Rule #9: Return 201 Created with user details
            return Created($"/api/auth/{user.Id}", new RegisterResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                Message = "Registration successful. You are now logged in."
            });
        }

        /// <summary>
        /// Rule #2: Authenticate user and create session.
        /// Returns: 200 OK with userId, username
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            // Rule #8: Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ErrorResponseDto
                {
                    Error = "ValidationError",
                    Details = errors
                });
            }

            // Delegate to application service (Rule #2)
            // Service will throw AuthenticationException if credentials are invalid
            var user = await _authService.LoginAsync(request.Username, request.Password);

            // Rule #7: Create session cookie
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // Rule #10: Return 200 OK with user details (no password)
            return Ok(new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Message = "Login successful"
            });
        }
    }
}
```

---

### `backend/Program.cs` (MODIFY)

Add DI registrations and exception filter:

```csharp
// ... existing using statements ...
using ToolLendingPlatform.Infrastructure.Data;
using ToolLendingPlatform.Infrastructure.Repositories;
using ToolLendingPlatform.Application.Services;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Api.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplicationBuilder.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ===== Add Services =====

// Database
builder.Services.AddDbContext<ToolLendingDbContext>(options =>
    options.UseSqlite(connectionString));

// Session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Rule #6, #7: 30-min timeout
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true; // Rule #6, #7: HTTP-only
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ===== Rule #11: Dependency Injection for Authentication =====

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Application Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<AuthenticationService>();

// Filters
builder.Services.AddScoped<AuthExceptionFilter>(); // Rule #12

// Health checks
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

app.UseStaticFiles();
app.UseCors("AllowLocalhost");
app.UseSession(); // Must come before routing
app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run("http://localhost:5000");
```

---

### `tests/Api/AuthControllerTests.cs` (CREATE)

```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using ToolLendingPlatform.Api.Dtos;

namespace ToolLendingPlatform.Tests.Api
{
    public class AuthControllerTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        // Rule #1, #9: Register happy path
        [Fact]
        public async Task RegisterAsync_ValidInput_Returns201Created()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "newuser123",
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            var content = await response.Content.ReadAsAsync<RegisterResponseDto>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal("newuser123", content.Username);
            Assert.NotEqual(0, content.UserId);
        }

        // Rule #4: Register duplicate username
        [Fact]
        public async Task RegisterAsync_DuplicateUsername_Returns400()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "duplicate",
                Password = "SecurePass123"
            };

            // First registration succeeds
            await _client.PostAsJsonAsync("/api/auth/register", request);

            // Act: Second registration with same username
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #3: Register invalid password
        [Fact]
        public async Task RegisterAsync_WeakPassword_Returns400()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "validuser",
                Password = "weak" // Too short, no uppercase, no digit
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #8: Register missing field
        [Fact]
        public async Task RegisterAsync_MissingUsername_Returns400()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = null!,
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Rule #2, #10: Login happy path
        [Fact]
        public async Task LoginAsync_ValidCredentials_Returns200Ok()
        {
            // Arrange: First register
            var registerRequest = new RegisterRequestDto
            {
                Username = "testuser",
                Password = "SecurePass123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act: Login
            var loginRequest = new LoginRequestDto
            {
                Username = "testuser",
                Password = "SecurePass123"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var content = await response.Content.ReadAsAsync<LoginResponseDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal("testuser", content.Username);
        }

        // Rule #5: Login wrong password
        [Fact]
        public async Task LoginAsync_WrongPassword_Returns401()
        {
            // Arrange: Register first
            var registerRequest = new RegisterRequestDto
            {
                Username = "testuser2",
                Password = "SecurePass123"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act: Login with wrong password
            var loginRequest = new LoginRequestDto
            {
                Username = "testuser2",
                Password = "WrongPass123"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Rule #6: Register sets cookie
        [Fact]
        public async Task RegisterAsync_SetsCookie()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "cookietest",
                Password = "SecurePass123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.NotEmpty(response.Headers.GetValues("Set-Cookie"));
        }
    }
}
```

---

## Edge Cases Handled

| # | Rule | Trigger | Expected Output |
|---|------|---------|-----------------|
| 1 | #8 | POST register with null username | 400 Bad Request (validation error) |
| 2 | #8 | POST register with empty password | 400 Bad Request (validation error) |
| 3 | #3 | POST register with weak password | 400 Bad Request (validation error) |
| 4 | #4 | POST register with existing username | 400 Bad Request ("Username already taken") |
| 5 | #5 | POST login with nonexistent username | 401 Unauthorized ("Invalid username or password") |
| 6 | #5 | POST login with wrong password | 401 Unauthorized ("Invalid username or password") |
| 7 | #8 | POST login with missing fields | 400 Bad Request (validation error) |

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Code compiles:
   ```bash
   dotnet build
   ```

2. All integration tests pass:
   ```bash
   dotnet test tests/Api/AuthControllerTests.cs
   # Expected: 7+ tests pass
   ```

**Manual Checks:**
1. Health endpoint still works: `curl http://localhost:5000/api/health`
2. Register endpoint returns 201: `curl -X POST http://localhost:5000/api/auth/register -H "Content-Type: application/json" -d '{"username":"test","password":"Pass123"}'`
3. Login endpoint returns 200: `curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"username":"test","password":"Pass123"}'`
4. Invalid inputs return 400: `curl -X POST http://localhost:5000/api/auth/register -H "Content-Type: application/json" -d '{"username":"","password":""}'`

---

## Definition of Done

This task is complete when:

- ✅ `AuthController` implements `POST /api/auth/register` and `POST /api/auth/login` endpoints.
- ✅ Register endpoint validates input, calls AuthenticationService, returns 201 on success.
- ✅ Login endpoint validates input, calls AuthenticationService, returns 200 on success.
- ✅ Session cookies created with HTTP-only, secure flags, 30-min timeout.
- ✅ All DTOs defined (requests, responses, errors).
- ✅ Exception filter maps exceptions to correct HTTP codes (400, 401, 403).
- ✅ DI container configured in `Program.cs` with all services registered.
- ✅ All integration tests pass (7+ tests).
- ✅ No plaintext passwords in responses or logs.

**Handoff to TASK-05:**
- API endpoints are fully implemented and tested.
- Endpoints return correct HTTP codes and response payloads.
- Session management is in place.
- Next task: Frontend services will call these endpoints.

---
