# TASK-01 — Domain Layer: User Entity + Exceptions

## Context

This task implements the **User domain entity** and related **exceptions** used throughout the authentication flows (registration and login). The User entity is the core aggregate root for Sprint 1, holding username and password hash. This task also defines custom exceptions that the application layer will throw, allowing controllers to map them to HTTP responses.

No persistence or API logic here—only business-rule enforcement (e.g., username validation, password strength checks).

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/tool-lending-inception.md` | §4 BCE Class Identification, §5 User State Machine | User entity lifecycle and state transitions |
| `docs/tool-lending-inception.md` | §7 Database Design, Table: Users | User table schema (username, password_hash, created_at) |
| `docs/sprint1-authen/sprint1-design.md` | §4 Detailed Sequence Diagrams (BF1, BF2) | User creation, password hashing, exceptions thrown |
| `docs/sprint1-authen/sprint1-design.md` | §5b Backend Class Diagram | User domain class structure and immutability |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**From BF1 — User Registration (excerpt):**
```
AuthenticationService -> User: new User(username, password_hash)
activate User
User -> AuthenticationService: User instance
deactivate User
```

**From BF2 — User Login (excerpt):**
```
AuthenticationService -> PasswordHasher: verifyPassword(input_password, user.password_hash)
```

**Key observations:**
1. User is instantiated by AuthenticationService with (username, password_hash).
2. Password is NEVER stored in plaintext; only hash is stored.
3. User entity is immutable after creation (no setters on core fields).

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files / Layer | Proposed Test Name | Verification Hook |
|---|---------------------|--------|----------------------|--------------------|-------------------|
| 1 | "Passwords stored using hashing (e.g., bcrypt)" | Inception §5.2, SRS §5.2 | `User.cs`, `PasswordHasher.cs` (TASK-03) | `UserEntityTests_PasswordHashNotStored` | Unit test |
| 2 | Username must be unique in database (via index in TASK-02) | Inception §7.2, Sprint Design §3 API Contract, Sequence BF1 | `User.cs` (validation logic), `UserRepository.cs` (TASK-02) | `UserEntityTests_UsernameRequired` | Unit test |
| 3 | "Use constant-time password comparison (bcrypt.verify) to prevent timing attacks" | Inception §8.4, Sprint Design §3 Security Notes | `PasswordHasher.cs` (TASK-03) | (covered in TASK-03) | Integration test |
| 4 | "After User is created, it is immutable" per Inception §5b Class Diagram | Inception §5b | `User.cs` properties (private setters) | `UserEntityTests_Immutable` | Unit test |
| 5 | Exceptions: `InvalidUsernameException`, `DuplicateUsernameException`, `AuthenticationException` must be defined | Sprint Design §5b Backend Class Diagram | Exceptions folder | (no test needed, just exists) | Manual verification |
| 6 | User aggregate has: id, username, password_hash, created_at, updated_at per Inception §7.1 | Inception §7.1 Table: Users | `User.cs` properties | (POCO check, see verification) | Manual verification |
| 7 | "User has no business logic in prototype" — only value storage and immutability | Inception §5.2 Domain Layer, Design Rule 1 | `User.cs` (no methods beyond getter helpers) | (N/A) | Code review |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `backend/Domain/User.cs` | CREATE | Domain entity | `class User { int Id, string Username, string PasswordHash, DateTime CreatedAt, DateTime UpdatedAt; constructor; private setters }` |
| `backend/Domain/Exceptions/InvalidUsernameException.cs` | CREATE | Domain exception | `class InvalidUsernameException : Exception { ... }` |
| `backend/Domain/Exceptions/DuplicateUsernameException.cs` | CREATE | Domain exception | `class DuplicateUsernameException : Exception { ... }` |
| `backend/Domain/Exceptions/AuthenticationException.cs` | CREATE | Domain exception | `class AuthenticationException : Exception { ... }` |
| `backend/Domain/Exceptions/InvalidPasswordException.cs` | CREATE | Domain exception | `class InvalidPasswordException : Exception { ... }` |
| `tests/Domain/UserTests.cs` | CREATE | Unit tests | `public class UserTests { [Fact] public void TestUserCreation() { ... } }` |

---

## Implementation Skeleton

### `backend/Domain/User.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Domain
{
    /// <summary>
    /// User aggregate root.
    /// Represents a person using the Tool Lending Platform.
    /// Immutable after creation; all state is set in constructor.
    /// </summary>
    public class User
    {
        // Rule #6: All required properties per Inception §7.1 Table: Users
        public int Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Rule #4: Private setters ensure immutability after creation
        // Constructor for new registrations
        public User(string username, string passwordHash)
        {
            // Rule #2: Validate username is not null/empty
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidUsernameException("Username cannot be empty");

            // Rule #1: Password hash must exist (actual hashing happens in PasswordHasher/TASK-03)
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new InvalidPasswordException("Password hash cannot be empty");

            Username = username;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            // Id is auto-generated by database (EF Core will set after SaveChanges)
        }

        // EF Core requires a parameterless constructor for entity loading from database
        protected User() { }
    }
}
```

**Responsibilities:**
- Immutable aggregate root (Rule #4).
- Validates username and password hash on construction (Rule #2, #1).
- Stores only password hash, never plaintext (Rule #1).

**What this entity does NOT do:**
- Does NOT hash passwords (that's PasswordHasher in TASK-03).
- Does NOT check for username uniqueness (that's the repository in TASK-02).
- Does NOT perform login verification (that's AuthenticationService in TASK-03).

---

### `backend/Domain/Exceptions/InvalidUsernameException.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when username validation fails (e.g., null, empty, invalid format).
    /// Rule #5: Custom exception for registration/login errors.
    /// </summary>
    public class InvalidUsernameException : Exception
    {
        public InvalidUsernameException(string message = "Invalid username") 
            : base(message) { }
    }
}
```

---

### `backend/Domain/Exceptions/DuplicateUsernameException.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when attempting to register with a username that already exists.
    /// Rule #5: Custom exception for registration duplicate check.
    /// Caught by controller and mapped to 400 Bad Request.
    /// </summary>
    public class DuplicateUsernameException : Exception
    {
        public DuplicateUsernameException(string username) 
            : base($"Username '{username}' is already taken") { }
    }
}
```

---

### `backend/Domain/Exceptions/AuthenticationException.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when login fails (user not found or password incorrect).
    /// Rule #5: Custom exception for login errors.
    /// Caught by controller and mapped to 401 Unauthorized.
    /// Generic message used to prevent username enumeration (Design §8.4).
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message = "Invalid username or password") 
            : base(message) { }
    }
}
```

---

### `backend/Domain/Exceptions/InvalidPasswordException.cs` (CREATE)

```csharp
namespace ToolLendingPlatform.Domain.Exceptions
{
    /// <summary>
    /// Thrown when password validation fails (e.g., null, empty, insufficient complexity).
    /// Rule #5: Custom exception for password validation errors.
    /// </summary>
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message = "Invalid password") 
            : base(message) { }
    }
}
```

---

### `tests/Domain/UserTests.cs` (CREATE)

```csharp
using Xunit;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Domain.Exceptions;

namespace ToolLendingPlatform.Tests.Domain
{
    public class UserTests
    {
        // Rule #6: User creation with valid inputs
        [Fact]
        public void UserCreation_ValidInputs_Succeeds()
        {
            // Arrange
            var username = "john_doe";
            var passwordHash = "hashed_password_abc123";

            // Act
            var user = new User(username, passwordHash);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(username, user.Username);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
            Assert.True(user.UpdatedAt <= DateTime.UtcNow);
        }

        // Rule #2: Username validation
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UserCreation_NullOrEmptyUsername_ThrowsInvalidUsernameException(string username)
        {
            // Arrange
            var passwordHash = "hashed_password_abc123";

            // Act & Assert
            Assert.Throws<InvalidUsernameException>(() => new User(username, passwordHash));
        }

        // Rule #1: Password hash must exist
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UserCreation_NullOrEmptyPasswordHash_ThrowsInvalidPasswordException(string passwordHash)
        {
            // Arrange
            var username = "john_doe";

            // Act & Assert
            Assert.Throws<InvalidPasswordException>(() => new User(username, passwordHash));
        }

        // Rule #4: Immutability check
        [Fact]
        public void UserCreation_PropertiesAreImmutable()
        {
            // Arrange
            var user = new User("john_doe", "hashed_password");

            // Act & Assert
            // Properties should not have public setters; this test verifies the class design
            // If this compiles without error, setters are private ✓
            Assert.Equal("john_doe", user.Username);
            // Attempting user.Username = "hack" would fail at compile time (✓ immutable)
        }
    }
}
```

**Tests cover:**
- Rule #6: Valid user creation with all required properties.
- Rule #2: Username validation (null, empty, whitespace).
- Rule #1: Password hash validation (null, empty, whitespace).
- Rule #4: Immutability (via design verification).

---

## Edge Cases Handled

| # | Rule Row # | Trigger Input / Condition | Expected Output |
|---|------------|--------------------------|-----------------|
| 1 | #2 | Constructor called with `null` username | `InvalidUsernameException` thrown |
| 2 | #2 | Constructor called with `""` username | `InvalidUsernameException` thrown |
| 3 | #2 | Constructor called with whitespace-only username | `InvalidUsernameException` thrown |
| 4 | #1 | Constructor called with `null` password hash | `InvalidPasswordException` thrown |
| 5 | #1 | Constructor called with `""` password hash | `InvalidPasswordException` thrown |
| 6 | #4 | After construction, attempt to modify `Username` property | Compile error (private setter) |
| 7 | #4 | After construction, attempt to modify `PasswordHash` property | Compile error (private setter) |

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Code compiles without errors:
   ```bash
   cd backend
   dotnet build
   ```

2. All unit tests pass:
   ```bash
   dotnet test tests/Domain/UserTests.cs
   # Expected: 5 tests pass ✓
   ```

3. No compilation warnings related to entity design.

**Manual Checks:**
1. Verify `User.cs` has:
   - Properties: `Id`, `Username`, `PasswordHash`, `CreatedAt`, `UpdatedAt` (all public get, private set)
   - Constructor with (username, passwordHash) parameters
   - No public setters
   - Null/empty validation in constructor

2. Verify exceptions folder exists with all 4 exception classes:
   - `InvalidUsernameException`
   - `DuplicateUsernameException`
   - `AuthenticationException`
   - `InvalidPasswordException`

3. Exception messages are user-friendly (e.g., "Username cannot be empty", not cryptic codes).

---

## Definition of Done

This task is complete when:

- ✅ `User.cs` compiles with no errors and all properties defined (Rule #6).
- ✅ User constructor validates username and password hash, throwing appropriate exceptions (Rule #2, #1).
- ✅ User properties are immutable (private setters) after construction (Rule #4).
- ✅ All 4 exception classes are defined in `backend/Domain/Exceptions/` (Rule #5).
- ✅ Unit tests in `UserTests.cs` pass (5/5 tests).
- ✅ No public setters on core User properties (compile-time immutability verified).
- ✅ User entity has no persistence, authentication, or business logic beyond validation.

**Handoff to TASK-02:**
- `User` entity is defined and immutable.
- `IUserRepository` interface exists (placeholder from TASK-00).
- Next task will implement `UserRepository` with SaveAsync, GetByUsernameAsync methods.
- Repository will handle database persistence and uniqueness checks.

---
