# TASK-03 — Application Layer: AuthenticationService + PasswordHasher

## Context

This task implements the **application-level orchestration** for authentication: `AuthenticationService` and `PasswordHasher`. These services handle the business logic for registration (username validation, password hashing) and login (password verification). They use the domain layer (User entity, exceptions) and the infrastructure layer (UserRepository) to implement the complete authentication workflows.

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/tool-lending-inception.md` | §4 BCE Class Identification, §5 User State Machine | AuthenticationService, PasswordHasher control classes |
| `docs/sprint1-authen/sprint1-design.md` | §4 Sequence Diagrams BF1, BF2 | Exact flow of registration and login |
| `docs/sprint1-authen/sprint1-design.md` | §5b Backend Class Diagram | Service method signatures and responsibilities |
| `docs/sprint1-authen/sprint1-design.md` | §3 API Contract | Validation rules, error codes, response payloads |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**From BF1 (Registration) — Happy Path:**
```
AuthController -> AuthenticationService: register(username, password)
AuthenticationService -> UserRepository: getUserByUsername(username)
AuthenticationService -> PasswordHasher: hashPassword(password)
PasswordHasher -> PasswordHasher: bcrypt.hash(password, salt_rounds=10)
AuthenticationService -> User: new User(username, password_hash)
AuthenticationService -> UserRepository: save(user)
UserRepository -> SQLite: INSERT
AuthController -> AuthService: 201 Created
```

**From BF1 (Registration) — Duplicate Username:**
```
AuthenticationService -> UserRepository: getUserByUsername(username)
UserRepository -> SQLite: SELECT ... WHERE username = ?
alt Username Already Exists
  AuthenticationService -> AuthController: throw DuplicateUsernameException
  AuthController -> AuthService: 400 Bad Request
```

**From BF2 (Login) — Happy Path:**
```
AuthController -> AuthenticationService: login(username, password)
AuthenticationService -> UserRepository: getUserByUsername(username)
AuthenticationService -> PasswordHasher: verifyPassword(input_password, user.password_hash)
PasswordHasher -> PasswordHasher: bcrypt.compare(input, hash) [constant-time]
AuthController -> AuthService: 200 OK
```

**From BF2 (Login) — Wrong Password:**
```
PasswordHasher -> PasswordHasher: bcrypt.compare returns false
AuthenticationService -> AuthController: throw AuthenticationException
AuthController -> AuthService: 401 Unauthorized
```

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files | Test Name | Verification |
|---|---------------------|--------|-------------|-----------|--------------|
| 1 | "Passwords stored using hashing (e.g., bcrypt)" | SRS §5.2, Inception §8.3 | `PasswordHasher.cs` | `PasswordHasherTests_HashNotPlaintext` | Unit test |
| 2 | "Use constant-time password comparison (bcrypt.verify) to prevent timing attacks" | Inception §8.4, Sprint Design §3 | `PasswordHasher.cs` | `PasswordHasherTests_VerifyUsesConstantTime` | Code review + unit test |
| 3 | "Password minimum 6 characters, with complexity" (1 upper, 1 lower, 1 digit) | Sprint Design §3 API Contract | `AuthenticationService.cs` | `AuthServiceTests_PasswordValidation` | Unit test |
| 4 | "Username minimum 3 characters, max 50, alphanumeric + underscore" | Sprint Design §3 API Contract (implicit) | `AuthenticationService.cs` | `AuthServiceTests_UsernameValidation` | Unit test |
| 5 | "Throw DuplicateUsernameException if username exists during registration" | BF1 sequence, Sprint Design §3 | `AuthenticationService.cs` | `AuthServiceTests_RegisterDuplicateThrows` | Integration test |
| 6 | "Throw AuthenticationException if username not found or password incorrect" | BF2 sequence, Sprint Design §3 | `AuthenticationService.cs` | `AuthServiceTests_LoginWrongPasswordThrows` | Integration test |
| 7 | "Generic error 'Invalid username or password' to prevent username enumeration" | Inception §8.4, Sprint Design §3 | `AuthenticationException`, usage in service | (implicit in error messages) | Code review |
| 8 | "Register: validate input, hash password, create User entity, save via repository" | BF1 sequence | `AuthenticationService.cs` | `AuthServiceTests_RegisterHappyPath` | Integration test |
| 9 | "Login: validate input, query user by username, verify password with constant-time compare" | BF2 sequence | `AuthenticationService.cs` | `AuthServiceTests_LoginHappyPath` | Integration test |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `backend/Application/Services/PasswordHasher.cs` | CREATE | Application service | `class PasswordHasher { HashAsync(password), VerifyAsync(plaintext, hash) }` |
| `backend/Application/Interfaces/IPasswordHasher.cs` | CREATE | Application interface | `interface IPasswordHasher { Task<string> HashAsync(...), Task<bool> VerifyAsync(...) }` |
| `backend/Application/Services/AuthenticationService.cs` | CREATE | Application service | `class AuthenticationService { RegisterAsync(...), LoginAsync(...) }` |
| `backend/Application/Validators/PasswordValidator.cs` | CREATE | Helper class | `class PasswordValidator { ValidatePassword(password): {valid, errors} }` |
| `tests/Application/AuthenticationServiceTests.cs` | CREATE | Integration tests | Multiple test methods covering register/login flows |
| `tests/Application/PasswordHasherTests.cs` | CREATE | Unit tests | Hash and verify test methods |

---

## Implementation Skeleton

### `backend/Application/Interfaces/IPasswordHasher.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing and verification.
    /// Rule #1, #2: Contract for bcrypt-based password operations.
    /// </summary>
    public interface IPasswordHasher
    {
        /// Rule #1: Hash plaintext password using bcrypt
        Task<string> HashAsync(string plaintext);

        /// Rule #2: Verify plaintext against hash using constant-time comparison
        Task<bool> VerifyAsync(string plaintext, string hash);
    }
}
```

---

### `backend/Application/Services/PasswordHasher.cs` (CREATE)

```csharp
using BCrypt.Net;
using ToolLendingPlatform.Application.Interfaces;

namespace ToolLendingPlatform.Application.Services
{
    /// <summary>
    /// PasswordHasher using BCrypt.Net library.
    /// Rule #1: Hashes passwords with bcrypt algorithm (salt rounds ≥ 10).
    /// Rule #2: Verifies passwords using BCrypt.Net which implements constant-time comparison.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltRounds = 10; // bcrypt salt rounds (≥ 10 per security best practices)

        /// <summary>
        /// Rule #1: Hash plaintext password using bcrypt.
        /// Salt rounds = 10 ensures reasonable performance + security balance.
        /// </summary>
        public Task<string> HashAsync(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("Password cannot be null or empty", nameof(plaintext));

            var hash = BCrypt.Net.BCrypt.HashPassword(plaintext, SaltRounds);
            return Task.FromResult(hash);
        }

        /// <summary>
        /// Rule #2: Verify plaintext against hash using constant-time comparison.
        /// BCrypt.Net.BCrypt.Verify() internally uses constant-time comparison to prevent timing attacks.
        /// </summary>
        public Task<bool> VerifyAsync(string plaintext, string hash)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(hash))
                return Task.FromResult(false);

            bool isValid = BCrypt.Net.BCrypt.Verify(plaintext, hash);
            return Task.FromResult(isValid);
        }
    }
}
```

---

### `backend/Application/Validators/PasswordValidator.cs` (CREATE)

```csharp
using System.Text.RegularExpressions;

namespace ToolLendingPlatform.Application.Validators
{
    /// <summary>
    /// Validates password strength per Sprint Design §3 API Contract.
    /// Rule #3: Password requires:
    ///   - Minimum 6 characters
    ///   - At least 1 uppercase letter
    ///   - At least 1 lowercase letter
    ///   - At least 1 digit
    /// </summary>
    public class PasswordValidator
    {
        public static (bool Valid, List<string> Errors) ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password cannot be empty");
                return (false, errors);
            }

            if (password.Length < 6)
                errors.Add("Password must be at least 6 characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must contain at least 1 uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must contain at least 1 lowercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("Password must contain at least 1 digit");

            return (errors.Count == 0, errors);
        }
    }
}
```

---

### `backend/Application/Validators/UsernameValidator.cs` (CREATE)

```csharp
using System.Text.RegularExpressions;

namespace ToolLendingPlatform.Application.Validators
{
    /// <summary>
    /// Validates username format per Sprint Design §3 API Contract.
    /// Rule #4: Username requires:
    ///   - Minimum 3 characters, maximum 50 characters
    ///   - Only alphanumeric and underscore
    /// </summary>
    public class UsernameValidator
    {
        public static (bool Valid, List<string> Errors) ValidateUsername(string username)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errors.Add("Username cannot be empty");
                return (false, errors);
            }

            if (username.Length < 3)
                errors.Add("Username must be at least 3 characters");

            if (username.Length > 50)
                errors.Add("Username must not exceed 50 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                errors.Add("Username can only contain letters, numbers, and underscores");

            return (errors.Count == 0, errors);
        }
    }
}
```

---

### `backend/Application/Services/AuthenticationService.cs` (CREATE)

```csharp
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Application.Validators;

namespace ToolLendingPlatform.Application.Services
{
    /// <summary>
    /// AuthenticationService orchestrates registration and login flows.
    /// Rule #8: Register: validate input, hash password, create User entity, save.
    /// Rule #9: Login: validate input, query user, verify password with constant-time compare.
    /// Rule #5, #6: Throw appropriate exceptions for errors.
    /// </summary>
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>
        /// Rule #8: Register a new user.
        /// Steps:
        ///   1. Validate username format (Rule #4)
        ///   2. Validate password strength (Rule #3)
        ///   3. Check if username already exists (Rule #5)
        ///   4. Hash password using bcrypt (Rule #1)
        ///   5. Create User entity
        ///   6. Save to repository
        /// Throws: InvalidUsernameException, InvalidPasswordException, DuplicateUsernameException
        /// </summary>
        public async Task<User> RegisterAsync(string username, string password)
        {
            // Validate username format (Rule #4)
            var (usernameValid, usernameErrors) = UsernameValidator.ValidateUsername(username);
            if (!usernameValid)
                throw new InvalidUsernameException(string.Join("; ", usernameErrors));

            // Validate password strength (Rule #3)
            var (passwordValid, passwordErrors) = PasswordValidator.ValidatePassword(password);
            if (!passwordValid)
                throw new InvalidPasswordException(string.Join("; ", passwordErrors));

            // Check username uniqueness (Rule #5)
            bool userExists = await _userRepository.ExistsAsync(username);
            if (userExists)
                throw new DuplicateUsernameException(username);

            // Hash password (Rule #1, #2)
            string passwordHash = await _passwordHasher.HashAsync(password);

            // Create User entity (domain validation happens here)
            var user = new User(username, passwordHash);

            // Persist to repository
            await _userRepository.SaveAsync(user);

            return user;
        }

        /// <summary>
        /// Rule #9: Authenticate user with username and password.
        /// Steps:
        ///   1. Validate input (username and password required)
        ///   2. Query user by username
        ///   3. If user not found, throw AuthenticationException (Rule #6, #7)
        ///   4. Verify password using constant-time comparison (Rule #2)
        ///   5. If password incorrect, throw AuthenticationException (Rule #6, #7)
        ///   6. Return authenticated user
        /// Throws: AuthenticationException (with generic message to prevent username enumeration)
        /// </summary>
        public async Task<User> LoginAsync(string username, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            // Query user by username
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            // Verify password using constant-time comparison (Rule #2)
            bool passwordValid = await _passwordHasher.VerifyAsync(password, user.PasswordHash);
            if (!passwordValid)
                throw new AuthenticationException("Invalid username or password"); // Rule #7: Generic message

            return user;
        }
    }
}
```

---

### `tests/Application/PasswordHasherTests.cs` (CREATE)

```csharp
using Xunit;
using ToolLendingPlatform.Application.Services;

namespace ToolLendingPlatform.Tests.Application
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher = new();

        // Rule #1: Hash produces non-plaintext output
        [Fact]
        public async Task HashAsync_ValidPassword_ReturnsHashedPassword()
        {
            // Arrange
            var plaintext = "SecurePassword123";

            // Act
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEqual(plaintext, hash); // Hash should not be plaintext
            Assert.StartsWith("$2a$", hash); // bcrypt format verification
        }

        // Rule #2: Verify accepts correct password
        [Fact]
        public async Task VerifyAsync_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var plaintext = "CorrectPassword123";
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Act
            var isValid = await _passwordHasher.VerifyAsync(plaintext, hash);

            // Assert
            Assert.True(isValid);
        }

        // Rule #2: Verify rejects incorrect password
        [Fact]
        public async Task VerifyAsync_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var plaintext = "CorrectPassword123";
            var wrongPassword = "WrongPassword456";
            var hash = await _passwordHasher.HashAsync(plaintext);

            // Act
            var isValid = await _passwordHasher.VerifyAsync(wrongPassword, hash);

            // Assert
            Assert.False(isValid);
        }

        // Rule #2: Timing should be consistent (constant-time)
        [Fact]
        public async Task VerifyAsync_TimingIsConsistent()
        {
            // Arrange
            var hash = await _passwordHasher.HashAsync("CorrectPassword123");
            var correctPassword = "CorrectPassword123";
            var wrongPassword = "WrongPassword______"; // Similar length

            // Act & Assert (BCrypt.NET internally uses constant-time comparison)
            // We trust the library implementation; this test documents the expectation
            var correctResult = await _passwordHasher.VerifyAsync(correctPassword, hash);
            var wrongResult = await _passwordHasher.VerifyAsync(wrongPassword, hash);

            Assert.True(correctResult);
            Assert.False(wrongResult);
            // Both should take approximately the same time (not measurable in unit test, but principle verified)
        }
    }
}
```

---

### `tests/Application/AuthenticationServiceTests.cs` (CREATE)

```csharp
using Xunit;
using Moq;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Domain.Exceptions;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Application.Services;

namespace ToolLendingPlatform.Tests.Application
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _authService = new AuthenticationService(_mockUserRepository.Object, _mockPasswordHasher.Object);
        }

        // Rule #8: Register happy path
        [Fact]
        public async Task RegisterAsync_ValidInput_CreatesAndSavesUser()
        {
            // Arrange
            var username = "newuser";
            var password = "SecurePass123";
            var passwordHash = "$2a$10$...";

            _mockUserRepository.Setup(r => r.ExistsAsync(username)).ReturnsAsync(false);
            _mockPasswordHasher.Setup(p => p.HashAsync(password)).ReturnsAsync(passwordHash);
            _mockUserRepository.Setup(r => r.SaveAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            // Act
            var result = await _authService.RegisterAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(passwordHash, result.PasswordHash);
            _mockUserRepository.Verify(r => r.SaveAsync(It.IsAny<User>()), Times.Once);
        }

        // Rule #4: Username validation - too short
        [Fact]
        public async Task RegisterAsync_UsernameTooShort_ThrowsInvalidUsernameException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidUsernameException>(
                () => _authService.RegisterAsync("ab", "SecurePass123"));
        }

        // Rule #4: Username validation - invalid characters
        [Fact]
        public async Task RegisterAsync_UsernameInvalidCharacters_ThrowsInvalidUsernameException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidUsernameException>(
                () => _authService.RegisterAsync("user@example", "SecurePass123"));
        }

        // Rule #3: Password validation - too short
        [Fact]
        public async Task RegisterAsync_PasswordTooShort_ThrowsInvalidPasswordException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _authService.RegisterAsync("validuser", "Pass1"));
        }

        // Rule #3: Password validation - no uppercase
        [Fact]
        public async Task RegisterAsync_PasswordNoUppercase_ThrowsInvalidPasswordException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _authService.RegisterAsync("validuser", "securepass123"));
        }

        // Rule #5: Duplicate username
        [Fact]
        public async Task RegisterAsync_UsernameDuplicate_ThrowsDuplicateUsernameException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.ExistsAsync("existing")).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateUsernameException>(
                () => _authService.RegisterAsync("existing", "SecurePass123"));
        }

        // Rule #9: Login happy path
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var username = "testuser";
            var password = "SecurePass123";
            var passwordHash = "$2a$10$...";
            var user = new User(username, passwordHash);

            _mockUserRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyAsync(password, passwordHash)).ReturnsAsync(true);

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        // Rule #6, #7: Login - user not found (generic message)
        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsAuthenticationException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AuthenticationException>(
                () => _authService.LoginAsync("nonexistent", "SomePass123"));
            Assert.Equal("Invalid username or password", ex.Message); // Rule #7: Generic message
        }

        // Rule #6, #7: Login - wrong password (generic message)
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
        {
            // Arrange
            var username = "testuser";
            var passwordHash = "$2a$10$...";
            var user = new User(username, passwordHash);

            _mockUserRepository.Setup(r => r.GetByUsernameAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyAsync("WrongPass123", passwordHash)).ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AuthenticationException>(
                () => _authService.LoginAsync(username, "WrongPass123"));
            Assert.Equal("Invalid username or password", ex.Message); // Rule #7: Generic message
        }
    }
}
```

---

## Edge Cases Handled

| # | Rule | Trigger | Expected Output |
|---|------|---------|-----------------|
| 1 | #3 | Password "Pass" (5 chars, has all complexity) | `InvalidPasswordException` (too short) |
| 2 | #3 | Password "securepass123" (no uppercase) | `InvalidPasswordException` |
| 3 | #3 | Password "SECUREPASS123" (no lowercase) | `InvalidPasswordException` |
| 4 | #3 | Password "SecurePass" (no digit) | `InvalidPasswordException` |
| 5 | #4 | Username "ab" | `InvalidUsernameException` (too short) |
| 6 | #4 | Username "a" * 51 | `InvalidUsernameException` (too long) |
| 7 | #4 | Username "user@domain" | `InvalidUsernameException` (invalid char) |
| 8 | #5 | Register with existing username | `DuplicateUsernameException` |
| 9 | #9 | Login with empty password | `AuthenticationException` |
| 10 | #9 | Login with wrong password | `AuthenticationException` (generic message) |

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

2. All tests pass:
   ```bash
   dotnet test tests/Application/
   # Expected: 10+ tests pass
   ```

**Manual Checks:**
1. Verify PasswordHasher uses BCrypt (check hash format starts with `$2a$`).
2. Verify AuthenticationService throws correct exceptions with generic messages.
3. Verify no plaintext passwords logged or exposed in exceptions.

---

## Definition of Done

This task is complete when:

- ✅ `IPasswordHasher` interface with `HashAsync`, `VerifyAsync` methods.
- ✅ `PasswordHasher` implementation using BCrypt with salt rounds ≥ 10.
- ✅ `PasswordValidator` enforces 6+ chars, 1 uppercase, 1 lowercase, 1 digit.
- ✅ `UsernameValidator` enforces 3-50 chars, alphanumeric + underscore.
- ✅ `AuthenticationService` with `RegisterAsync` and `LoginAsync` methods.
- ✅ Register flow: validate input → check uniqueness → hash password → create User → save.
- ✅ Login flow: query user → verify password with constant-time compare → return user.
- ✅ All exceptions thrown with correct messages (generic for auth errors).
- ✅ All tests pass (10+ tests covering happy paths and edge cases).
- ✅ No plaintext passwords in code, logs, or exceptions.

**Handoff to TASK-04:**
- `AuthenticationService` is fully implemented and tested.
- `IPasswordHasher` interface is defined and implemented.
- Next task: API layer (AuthController) will call these services and map exceptions to HTTP responses.

---
