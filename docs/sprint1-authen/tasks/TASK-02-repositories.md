# TASK-02 — Infrastructure Layer: UserRepository + SQLite Migration

## Context

This task implements the **UserRepository** (persistence layer) for the User aggregate. It defines the interface `IUserRepository` with concrete implementation using EF Core + SQLite. Additionally, it creates the database migration that creates the `users` table with the proper schema (username uniqueness index, constraints, etc.).

This task bridges the domain layer (TASK-01) and the application layer (TASK-03), providing methods to query and persist users.

---

## Design References

| Document | Section / Anchor | Purpose |
|----------|------------------|---------|
| `docs/tool-lending-inception.md` | §7 Database Design (Full ERD), §7.1 Tables & Columns | Users table schema |
| `docs/tool-lending-inception.md` | §7.2 Relationships, §7.3 Indexing Strategy | Uniqueness constraint on username |
| `docs/sprint1-authen/sprint1-design.md` | §5b Backend Class Diagram | UserRepository interface and implementation |
| `docs/sprint1-authen/sprint1-design.md` | §7 Database Design (Sprint Scope) | Users table excerpt, operations (INSERT, SELECT) |

---

## Sequence Diagram Reference (MANDATORY — Read Before Any Code)

**Sprint Design Doc section:** §4 — Detailed Sequence Diagrams  
**Relevant flows:** BF1 (Registration), BF2 (Login)

**From BF1 — User Registration (excerpt):**
```
AuthenticationService -> UserRepository: getUserByUsername(username)
activate UserRepository
UserRepository -> SQLite: SELECT * FROM Users WHERE username = ?
SQLite -> UserRepository: null or User
deactivate UserRepository

alt Username Already Exists
  UserRepository -> AuthenticationService: User found
  AuthenticationService -> AuthController: throw DuplicateUsernameException

else Username Is Unique
  AuthenticationService -> UserRepository: save(user)
  activate UserRepository
  UserRepository -> SQLite: INSERT INTO Users (username, password_hash, created_at)
  SQLite -> UserRepository: INSERT OK, userId = 1
  deactivate UserRepository
```

**From BF2 — User Login (excerpt):**
```
AuthenticationService -> UserRepository: getUserByUsername(username)
activate UserRepository
UserRepository -> SQLite: SELECT * FROM Users WHERE username = ?
SQLite -> UserRepository: User or null
deactivate UserRepository

alt User Not Found
  UserRepository -> AuthenticationService: null
  AuthenticationService -> AuthController: throw AuthenticationException

else User Found
  [continue with password verification]
```

**Key observations:**
1. Repository queries users by username.
2. Repository saves new users (INSERT).
3. Repository returns `User` or `null` (no exceptions thrown from repo).

---

## Design Rule Checklist

| # | Exact Rule (Verbatim) | Source | Owner Files / Layer | Test Name | Verification |
|---|---------------------|--------|----------------------|-----------|--------------|
| 1 | "username: VARCHAR(255), unique, not null" | Inception §7.1 Users table | `Migration_*.cs`, `ToolLendingDbContext.cs` (TASK-00) | `UserRepositoryTests_UsernameIsUnique` | Integration test |
| 2 | "password_hash: VARCHAR(255), not null, [bcrypt]" | Inception §7.1 Users table | `Migration_*.cs`, `User.cs` (TASK-01) | (implicit in save tests) | Integration test |
| 3 | "created_at, updated_at: DATETIME, default CURRENT_TIMESTAMP" | Inception §7.1 Users table | `Migration_*.cs` | `UserRepositoryTests_TimestampsSetOnCreate` | Integration test |
| 4 | "IUserRepository.getUserByUsername(username): User \| null" | Inception §5b class diagram, Sprint Design §5b | `IUserRepository.cs`, `UserRepository.cs` | `UserRepositoryTests_GetByUsernameReturnsUser` | Integration test |
| 5 | "IUserRepository.save(user): User (persisted)" | Inception §5b class diagram, BF1 sequence | `IUserRepository.cs`, `UserRepository.cs` | `UserRepositoryTests_SavePersistsUser` | Integration test |
| 6 | "IUserRepository.exists(username): bool" | Inception §5b class diagram, BF1 validation | `IUserRepository.cs`, `UserRepository.cs` | `UserRepositoryTests_ExistsReturnsTrue` | Integration test |
| 7 | "Uniqueness constraint enforced: duplicate username INSERT fails" | Inception §7.3 Indexing Strategy | `Migration_*.cs` (UNIQUE index) | `UserRepositoryTests_DuplicateUsernameThrowsException` | Integration test |
| 8 | "Database queries use parameterized queries (EF Core)" | Inception §8.4 Security Design (SQL Injection) | `UserRepository.cs` (EF LINQ only, no string concat) | (code review) | Manual verification |

---

## Affected Files

| File Path | Action | Layer / Role | Must-Contain |
|-----------|--------|--------------|-------------|
| `backend/Application/Interfaces/IUserRepository.cs` | CREATE/MODIFY | Application interface | `interface IUserRepository { Task<User> GetByUsernameAsync(string); Task<User> SaveAsync(User); Task<bool> ExistsAsync(string); }` |
| `backend/Infrastructure/Repositories/UserRepository.cs` | CREATE | Infrastructure implementation | `class UserRepository : IUserRepository { ... }` with EF Core DbContext |
| `backend/Infrastructure/Migrations/Migration_0001_CreateUsersTable.cs` | CREATE | Database migration | `CreateTable("Users", ...)` with columns and unique index |
| `tests/Infrastructure/UserRepositoryTests.cs` | CREATE | Integration tests | Multiple `[Fact]` test methods covering all repository methods |

---

## Implementation Skeleton

### `backend/Application/Interfaces/IUserRepository.cs` (CREATE/MODIFY)

```csharp
using ToolLendingPlatform.Domain;

namespace ToolLendingPlatform.Application.Interfaces
{
    /// <summary>
    /// Repository interface for User aggregate persistence.
    /// Implements the Repository pattern to abstract data access.
    /// Rule #4, #5, #6: Method contracts for user queries and saves.
    /// </summary>
    public interface IUserRepository
    {
        /// Rule #4: Query user by username; return User or null if not found
        Task<User?> GetByUsernameAsync(string username);

        /// Rule #5: Persist a User entity to the database; return the saved user with ID
        Task<User> SaveAsync(User user);

        /// Rule #6: Check if username exists without loading full User
        Task<bool> ExistsAsync(string username);
    }
}
```

**Responsibilities:**
- Define contract for user repository methods.
- Rule #4, #5, #6: Three core methods for registration and login flows.

---

### `backend/Infrastructure/Repositories/UserRepository.cs` (CREATE)

```csharp
using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Application.Interfaces;
using ToolLendingPlatform.Infrastructure.Data;

namespace ToolLendingPlatform.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IUserRepository.
    /// Rule #8: Uses EF LINQ (parameterized queries) to prevent SQL injection.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ToolLendingDbContext _dbContext;

        public UserRepository(ToolLendingDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Rule #4: Query user by username using parameterized EF LINQ.
        /// Returns null if not found (no exception thrown).
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            // Rule #8: EF Core translates LINQ to parameterized SQL automatically
            return await _dbContext.Users
                .AsNoTracking() // Read-only query for performance
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Rule #5: Persist User entity to database.
        /// Rule #7: If username already exists, EF Core will throw DbUpdateException (unique constraint violation).
        /// Returns the saved user (with ID populated by database).
        /// </summary>
        public async Task<User> SaveAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dbContext.Users.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Rule #7: Unique constraint violation on username
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") ?? false)
                {
                    throw new InvalidOperationException($"Username '{user.Username}' already exists", ex);
                }
                throw;
            }

            return user;
        }

        /// <summary>
        /// Rule #6: Check if username exists without loading full User object.
        /// Lightweight query for registration validation.
        /// </summary>
        public async Task<bool> ExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Username == username);
        }
    }
}
```

**Responsibilities:**
- Rule #4: Query by username (returns null if not found).
- Rule #5: Save user (throws on unique constraint violation).
- Rule #6: Lightweight exists check.
- Rule #8: Uses EF LINQ parameterized queries (no SQL injection risk).

---

### `backend/Infrastructure/Migrations/Migration_0001_CreateUsersTable.cs` (CREATE)

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace ToolLendingPlatform.Infrastructure.Migrations
{
    /// <summary>
    /// Migration to create the Users table with schema per Inception §7.1.
    /// Rule #1: username VARCHAR(255), UNIQUE, NOT NULL
    /// Rule #2: password_hash VARCHAR(255), NOT NULL
    /// Rule #3: created_at, updated_at DATETIME with defaults
    /// Rule #7: UNIQUE constraint on username
    /// </summary>
    public partial class CreateUsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    
                    // Rule #1: Username - unique, not null
                    Username = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
                    
                    // Rule #2: Password hash - not null
                    PasswordHash = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false),
                    
                    // Rule #3: Timestamps with defaults
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // Rule #7: Unique index on username for fast lookups and constraint enforcement
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
```

**Responsibilities:**
- Rule #1, #2, #3: Create Users table with correct schema.
- Rule #7: Create UNIQUE constraint on username.

---

### `tests/Infrastructure/UserRepositoryTests.cs` (CREATE)

```csharp
using Xunit;
using Microsoft.EntityFrameworkCore;
using ToolLendingPlatform.Domain;
using ToolLendingPlatform.Infrastructure.Data;
using ToolLendingPlatform.Infrastructure.Repositories;

namespace ToolLendingPlatform.Tests.Infrastructure
{
    public class UserRepositoryTests
    {
        private ToolLendingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ToolLendingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique per test
                .Options;
            return new ToolLendingDbContext(options);
        }

        // Rule #4: GetByUsername returns user when exists
        [Fact]
        public async Task GetByUsernameAsync_UserExists_ReturnsUser()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("john_doe", "hashed_password_123");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.GetByUsernameAsync("john_doe");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john_doe", result.Username);
            Assert.Equal("hashed_password_123", result.PasswordHash);
        }

        // Rule #4: GetByUsername returns null when user does not exist
        [Fact]
        public async Task GetByUsernameAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.GetByUsernameAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        // Rule #5: Save persists user to database with ID
        [Fact]
        public async Task SaveAsync_ValidUser_PersistsAndReturnsUserWithId()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("jane_doe", "hashed_password_456");
            var repository = new UserRepository(dbContext);

            // Act
            var savedUser = await repository.SaveAsync(user);

            // Assert
            Assert.NotEqual(0, savedUser.Id); // ID should be auto-generated
            Assert.Equal("jane_doe", savedUser.Username);

            // Verify in database
            var retrievedUser = await dbContext.Users.FirstAsync(u => u.Username == "jane_doe");
            Assert.Equal(savedUser.Id, retrievedUser.Id);
        }

        // Rule #6: Exists returns true when user exists
        [Fact]
        public async Task ExistsAsync_UsernameExists_ReturnsTrue()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user = new User("alice", "hashed_password_789");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.ExistsAsync("alice");

            // Assert
            Assert.True(result);
        }

        // Rule #6: Exists returns false when user does not exist
        [Fact]
        public async Task ExistsAsync_UsernameDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var repository = new UserRepository(dbContext);

            // Act
            var result = await repository.ExistsAsync("nonexistent");

            // Assert
            Assert.False(result);
        }

        // Rule #7: Duplicate username throws exception
        [Fact]
        public async Task SaveAsync_DuplicateUsername_ThrowsException()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var user1 = new User("duplicate", "hash1");
            var user2 = new User("duplicate", "hash2");
            
            dbContext.Users.Add(user1);
            await dbContext.SaveChangesAsync();

            var repository = new UserRepository(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await repository.SaveAsync(user2));
        }

        // Rule #3: CreatedAt and UpdatedAt are set
        [Fact]
        public async Task SaveAsync_ValidUser_SetsTimestamps()
        {
            // Arrange
            using var dbContext = CreateInMemoryDbContext();
            var beforeSave = DateTime.UtcNow;
            var user = new User("timestamp_test", "hash");
            var repository = new UserRepository(dbContext);

            // Act
            await repository.SaveAsync(user);
            var afterSave = DateTime.UtcNow;

            // Assert
            Assert.True(user.CreatedAt >= beforeSave && user.CreatedAt <= afterSave);
            Assert.True(user.UpdatedAt >= beforeSave && user.UpdatedAt <= afterSave);
        }
    }
}
```

**Tests cover:**
- Rule #4: GetByUsername (exists, not exists).
- Rule #5: Save persists user with ID.
- Rule #6: Exists check (true, false).
- Rule #7: Duplicate username raises exception.
- Rule #3: Timestamps set on creation.

---

## Edge Cases Handled

| # | Rule Row # | Trigger Input / Condition | Expected Output |
|---|------------|--------------------------|-----------------|
| 1 | #4 | `GetByUsernameAsync(null)` | Returns `null` (no exception) |
| 2 | #4 | `GetByUsernameAsync("")` | Returns `null` (whitespace ignored) |
| 3 | #4 | User not in database | Returns `null` |
| 4 | #5 | `SaveAsync(null)` | `ArgumentNullException` thrown |
| 5 | #5 | Two users with same username | Second save throws `InvalidOperationException` |
| 6 | #6 | `ExistsAsync(null)` | Returns `false` |
| 7 | #6 | `ExistsAsync("")` | Returns `false` |
| 8 | #7 | INSERT with duplicate username | Database constraint violation caught, mapped to `InvalidOperationException` |

---

## Feature Flags

| Flag Name | Default Value | Controlled Behavior | When to Remove |
|-----------|---------------|---------------------|----------------|
| (none) | — | — | — |

---

## Verification Steps (for this task)

**Automated Checks:**
1. Project builds:
   ```bash
   cd backend
   dotnet build
   ```

2. All repository integration tests pass:
   ```bash
   dotnet test tests/Infrastructure/UserRepositoryTests.cs
   # Expected: 7 tests pass ✓
   ```

3. Database migration applies successfully:
   ```bash
   cd backend
   dotnet ef database update --verbose
   # Expected: Migration applies, tool_lending.db created/updated
   ```

4. SQL schema verification (optional, but recommended):
   ```bash
   sqlite3 backend/tool_lending.db ".schema Users"
   # Expected output:
   # CREATE TABLE "Users" (
   #   "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
   #   "Username" VARCHAR(255) NOT NULL UNIQUE,
   #   "PasswordHash" VARCHAR(255) NOT NULL,
   #   "CreatedAt" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
   #   "UpdatedAt" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
   # );
   # CREATE UNIQUE INDEX "IX_Users_Username" ON "Users"("Username");
   ```

**Manual Checks:**
1. Verify `IUserRepository.cs` has three async methods:
   - `Task<User?> GetByUsernameAsync(string username)`
   - `Task<User> SaveAsync(User user)`
   - `Task<bool> ExistsAsync(string username)`

2. Verify `UserRepository.cs` implements IUserRepository.

3. Verify migration file includes UNIQUE constraint on username (Rule #7).

4. Verify no raw SQL strings in UserRepository (only LINQ).

---

## Definition of Done

This task is complete when:

- ✅ `IUserRepository.cs` defines three methods (Rule #4, #5, #6).
- ✅ `UserRepository.cs` implements all three methods using EF Core LINQ (Rule #8).
- ✅ Migration file creates Users table with correct schema (Rule #1, #2, #3, #7).
- ✅ Migration applies successfully (`dotnet ef database update` exits 0).
- ✅ All 7 integration tests pass.
- ✅ Database schema verified (UNIQUE constraint on username exists).
- ✅ No SQL injection risks (all queries use EF LINQ, no string concatenation).

**Handoff to TASK-03:**
- `IUserRepository` interface is fully defined and implemented.
- `UserRepository` is registered in DI container (will happen in TASK-04).
- Next task: Application layer (PasswordHasher, AuthenticationService) will call these repository methods.

---
